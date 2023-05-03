using System;
using System.Reflection;
using Torch.Managers.PatchManager;
using NLog;
using Sandbox.Game.World;
using Sandbox.Game.Multiplayer;
using VRage.Network;
using Sandbox;
using VRage.Utils;
using System.IO;
using Sandbox.Engine.Networking;
using System.Collections.Generic;
using Torch;
using Torch.Managers;
using Sandbox.Game;
using VRage.GameServices;
using VRageMath;
using System.Threading;
using VRage.Game;
using VRage;
using VRage.FileSystem;
using System.IO.Compression;
using VRage.Game.Voxels;
using VRage.ObjectBuilders;
using System.Threading.Tasks;
using DePatch.CoolDown;

namespace DePatch.KEEN_BUG_FIXES
{
    public static class KEEN_SaveFix
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static FieldInfo m_isSaveInProgress;
        private static MethodInfo GatherVicinityInformation;
        private static MethodInfo OnServerSaving;

        private static PropertyInfo SavingSuccess;
        private static FieldInfo m_savingLock;
        private static PropertyInfo TooLongPath;
        private static PropertyInfo SavedSizeInBytes;
        private static PropertyInfo CloudResultSet;

        private static bool ServerStart = true;

        public static void Patch(PatchContext ctx)
        {
            m_isSaveInProgress = typeof(MySession).EasyField("m_isSaveInProgress");
            GatherVicinityInformation = typeof(MySession).EasyMethod("GatherVicinityInformation");
            OnServerSaving = typeof(MySession).EasyMethod("OnServerSaving");

            m_savingLock = typeof(MySessionSnapshot).EasyField("m_savingLock");
            SavingSuccess = typeof(MySessionSnapshot).EasyProp("SavingSuccess");
            TooLongPath = typeof(MySessionSnapshot).EasyProp("TooLongPath");
            SavedSizeInBytes = typeof(MySessionSnapshot).EasyProp("SavedSizeInBytes");
            CloudResultSet = typeof(MySessionSnapshot).EasyProp("CloudResult");

            ctx.Prefix(typeof(MySession), "Save", typeof(KEEN_SaveFix), nameof(MySession_Save), new[] { "snapshot", "customSaveName" });

            ctx.Prefix(typeof(MySessionSnapshot), "Save", typeof(KEEN_SaveFix), nameof(MySessionSnapshot_Save), new[] { "screenshotTaken", "thumbName" });

            ctx.Prefix(typeof(MyLocalCache), typeof(KEEN_SaveFix), nameof(SaveCheckpoint), new[] { "checkpoint", "sessionPath", "sizeInBytes", "fileList" });
            ctx.Prefix(typeof(MyLocalCache), typeof(KEEN_SaveFix), nameof(SaveWorldConfiguration), new[] { "configuration", "sessionPath", "sizeInBytes", "fileList" });
            ctx.Prefix(typeof(MyLocalCache), typeof(KEEN_SaveFix), nameof(SaveSector), new[] { "sector", "sessionPath", "sectorPosition", "sizeInBytes", "fileList" });
        }

        public static bool MySession_Save(MySession __instance, out MySessionSnapshot snapshot, ref bool __result, string customSaveName = null)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.GameSaveFix)
            {
                snapshot = new MySessionSnapshot();
                __result = false;
                return true;
            }

            // Prevent cheaters to grab world with request from cheat plugin.
            var steamId = MyEventContext.Current.Sender.Value;
            var requesterPlayer = Sync.Players.TryGetPlayerBySteamId(steamId);

            if (requesterPlayer != null && !MySession.Static.IsUserModerator(steamId))
            {
                snapshot = new MySessionSnapshot();
                Log.Error($"Detected User asking to save world, possible hacker ID: {steamId} Player name: {requesterPlayer.DisplayName}");

                __result = false;
                return false;
            }

            m_isSaveInProgress.SetValue(__instance, true);

            // use torch user notify system with try/catch
            SaveStartEnd(true);

            snapshot = new MySessionSnapshot();
            Log.Warn("Saving snapshot - START");

            using (MySandboxGame.Log.IndentUsing(LoggingOptions.NONE))
            {
                try
                {
                    string saveName = customSaveName ?? __instance.Name;
                    if (customSaveName != null)
                    {
                        if (!Path.IsPathRooted(customSaveName))
                        {
                            string directoryName = Path.GetDirectoryName(__instance.CurrentPath);
                            if (Directory.Exists(directoryName))
                                __instance.CurrentPath = Path.Combine(directoryName, customSaveName);
                            else
                                __instance.CurrentPath = MyLocalCache.GetSessionSavesPath(customSaveName, false, true, false);
                        }
                        else
                        {
                            __instance.CurrentPath = customSaveName;
                            saveName = Path.GetFileName(customSaveName);
                        }
                    }

                    snapshot.TargetDir = __instance.CurrentPath;
                    snapshot.SavingDir = Path.Combine(snapshot.TargetDir, ".new");

                    Log.Warn("Saving to .new directory");

                    snapshot.CheckpointSnapshot = __instance.GetCheckpoint(saveName, false);
                    snapshot.SectorSnapshot = __instance.GetSector(true);
                    snapshot.CompressedVoxelSnapshots = __instance.VoxelMaps.GetVoxelMapsData(true, true, null);
                    snapshot.VicinityGatherTask = (ParallelTasks.Task)GatherVicinityInformation.Invoke(__instance, new object[] { snapshot.CheckpointSnapshot });
                    Dictionary<string, IMyStorage> voxelStorageNameCache = new Dictionary<string, IMyStorage>();
                    snapshot.VoxelSnapshots = __instance.VoxelMaps.GetVoxelMapsData(true, false, voxelStorageNameCache);
                    snapshot.VoxelStorageNameCache = voxelStorageNameCache;

                    __instance.SaveDataComponents();

                    // use torch user notify system with try/catch
                    SaveStartEnd(false);
                }
                catch (Exception ex)
                {
                    m_isSaveInProgress.SetValue(__instance, false);
                    SaveStartEnd(false);
                    __result = false;
                    Log.Error(ex, "Error during Game Save Function! Crash Avoided");
                    return false;
                }
            }
            Log.Warn("Saving snapshot - END");

            m_isSaveInProgress.SetValue(__instance, false);

            __result = true;
            return false;
        }

        public static bool MySessionSnapshot_Save(MySessionSnapshot __instance, Func<bool> screenshotTaken, string thumbName, ref bool __result)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.GameSaveFix)
            {
                __result = false;
                return true;
            }

            // check if something went terribly wrong.
            if (__instance == null)
            {
                __result = false;
                return false;
            }

            // Prevent cheaters to grab world with request from cheat plugin.
            var steamId = MyEventContext.Current.Sender.Value;
            var requesterPlayer = Sync.Players.TryGetPlayerBySteamId(steamId);

            if (requesterPlayer != null && !MySession.Static.IsUserModerator(steamId))
            {
                Log.Error($"Detected User asking to save world, possible hacker ID: {steamId} Player name: {requesterPlayer.DisplayName}");

                __result = false;
                return false;
            }

            __instance.VicinityGatherTask.WaitOrExecute(false);
            bool game_SAVES_TO_CLOUD = MyPlatformGameSettings.GAME_SAVES_TO_CLOUD;
            bool flag = true;

            FastResourceLock m_savingLockInternal = (FastResourceLock)m_savingLock.GetValue(__instance);
            if (m_savingLockInternal == null)
            {
                __result = false;
                return false;
            }

            using (m_savingLockInternal.AcquireExclusiveUsing())
            {
                Log.Warn("Session snapshot save - START");

                using (MySandboxGame.Log.IndentUsing(LoggingOptions.NONE))
                {
                    Directory.CreateDirectory(__instance.TargetDir);

                    if (!CheckAccessToFiles(__instance))
                    {
                        SavingSuccess.SetValue(__instance, false);
                        Log.Warn("Failed to get file access for files in target dir. Exiting!");
                        __result = false;
                        return false;
                    }

                    string savingDir = __instance.SavingDir;
                    if (Directory.Exists(savingDir))
                        Directory.Delete(savingDir, true);

                    Directory.CreateDirectory(savingDir);
                    List<MyCloudFile> list = new List<MyCloudFile>();
                    if (thumbName != null)
                        list.Add(new MyCloudFile(thumbName, false));

                    try
                    {
                        ulong num3 = 0UL;
                        bool SaveCheckpointResult = false;
                        bool SaveSectorResult = false;
                        TooLongPath.SetValue(__instance, false);

                        _ = SaveCheckpoint(__instance.CheckpointSnapshot, __instance.SavingDir, out ulong num2, list, ref SaveCheckpointResult);
                        _ = SaveSector(__instance.SectorSnapshot, __instance.SavingDir, Vector3I.Zero, out ulong num, list, ref SaveSectorResult);

                        flag = SaveSectorResult && SaveCheckpointResult;

                        if (flag)
                        {
                            foreach (KeyValuePair<string, byte[]> keyValuePair in __instance.VoxelSnapshots)
                            {
                                if (Path.Combine(__instance.SavingDir, keyValuePair.Key).Length > 260)
                                {
                                    Log.Warn("VoxelSnapshots TooLongPath detected BREAK!");
                                    TooLongPath.SetValue(__instance, true);
                                    flag = false;
                                    break;
                                }

                                ulong num4 = 0UL;
                                flag = flag && SaveVoxelSnapshot(__instance, keyValuePair.Key, keyValuePair.Value, true, out num4, list);
                                if (flag)
                                    num3 += num4;
                            }

                            __instance.VoxelSnapshots.Clear();
                            __instance.VoxelStorageNameCache.Clear();

                            foreach (KeyValuePair<string, byte[]> keyValuePair2 in __instance.CompressedVoxelSnapshots)
                            {
                                if (Path.Combine(__instance.SavingDir, keyValuePair2.Key).Length > 260)
                                {
                                    Log.Warn("CompressedVoxelSnapshots TooLongPath detected BREAK!");
                                    TooLongPath.SetValue(__instance, true);
                                    flag = false;
                                    break;
                                }

                                ulong num5 = 0UL;
                                flag = flag && SaveVoxelSnapshot(__instance, keyValuePair2.Key, keyValuePair2.Value, false, out num5, list);
                                if (flag)
                                    num3 += num5;
                            }

                            __instance.CompressedVoxelSnapshots.Clear();
                        }

                        if (flag && Sync.IsServer)
                            flag = MyLocalCache.SaveLastSessionInfo(__instance.TargetDir, false, false, MySession.Static.Name, null, 0);

                        if (flag)
                        {
                            SavedSizeInBytes.SetValue(__instance, num + num2 + num3);

                            if (screenshotTaken != null)
                            {
                                while (!screenshotTaken())
                                {
                                    Thread.Sleep(5);
                                }
                            }

                            if (game_SAVES_TO_CLOUD)
                            {
                                string containerName = MyCloudHelper.LocalToCloudWorldPath(__instance.TargetDir);
                                CloudResultSet.SetValue(__instance, MyGameService.SaveToCloud(containerName, list));
                                flag = (CloudResult)CloudResultSet.GetValue(__instance) == CloudResult.Ok;
                            }
                        }

                        if (flag)
                        {
                            HashSet<string> hashSet = new HashSet<string>();
                            try
                            {
                                foreach (string text in Directory.GetFiles(savingDir))
                                {
                                    string fileName = Path.GetFileName(text);
                                    string destFileName = Path.Combine(__instance.TargetDir, fileName);
                                    File.Copy(text, destFileName, true);
                                    hashSet.Add(fileName);
                                }
                                Log.Info("Copy Save Files is done!");
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex, "Error during Game Save in Copy Files! Function! Crash Avoided");
                            }

                            try
                            {
                                foreach (string path in Directory.GetFiles(__instance.TargetDir))
                                {
                                    string fileName2 = Path.GetFileName(path);
                                    if (!hashSet.Contains(fileName2) && !(fileName2 == MyTextConstants.SESSION_THUMB_NAME_AND_EXTENSION))
                                        File.Delete(path);
                                }

                                // make sure .new is exist before trying to delete it!
                                if (Directory.Exists(savingDir))
                                    Directory.Delete(savingDir, true);
                                else
                                    Log.Warn(".new directory was not found! strange, Crash Avoided");
                            }
                            catch (Exception ex)
                            {
                                Log.Warn(ex, "There was an error while cleaning the snapshot.");
                            }

                            Backup(__instance.TargetDir, __instance.TargetDir);
                        }
                    }
                    catch (Exception ex2)
                    {
                        Log.Warn(ex2, "There was an error while saving snapshot.");
                        flag = false;
                        __result = flag;
                    }

                    if (!flag)
                    {
                        try
                        {
                            // make sure .new is exist before trying to delete it!
                            if (Directory.Exists(savingDir))
                            {
                                Directory.Delete(savingDir, true);
                                Log.Warn("Deleted .new directory, Flag was FALSE");
                            }
                        }
                        catch (Exception ex3)
                        {
                            Log.Warn(ex3, "There was an error while cleaning snapshot.");
                        }
                    }
                }
                Log.Warn("Saving world - END, All OK");
            }

            SavingSuccess.SetValue(__instance, flag);

            __result = flag;
            return false;
        }

        public static bool SaveCheckpoint(MyObjectBuilder_Checkpoint checkpoint, string sessionPath, out ulong sizeInBytes, List<MyCloudFile> fileList, ref bool __result)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.GameSaveFix)
            {
                sizeInBytes = 0UL;
                __result = false;
                return true;
            }

            // make sure we have data to work with.
            if (sessionPath == string.Empty || checkpoint == null)
            {
                sizeInBytes = 0UL;
                Log.Error("Error during SaveCheckpoint Function!, sessionPath or checkpoint where NULL, Crash Avoided");
                __result = false;
                return false;
            }

            string text = Path.Combine(sessionPath, "Sandbox.sbc");
            bool SerializeXMLResult = false;
            _ = SerializeXMLInternal(text, MyPlatformGameSettings.GAME_SAVES_COMPRESSED_BY_DEFAULT, checkpoint, out sizeInBytes, ref SerializeXMLResult, null);

            // make sure we dont have duplicate in this list!
            if (fileList != null && !fileList.Contains(new MyCloudFile(text, false)))
                fileList.Add(new MyCloudFile(text, false));

            MyObjectBuilder_WorldConfiguration myObjectBuilder_WorldConfiguration = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_WorldConfiguration>();
            myObjectBuilder_WorldConfiguration.Settings = checkpoint.Settings;
            myObjectBuilder_WorldConfiguration.Mods = checkpoint.Mods;
            myObjectBuilder_WorldConfiguration.SessionName = checkpoint.SessionName;
            myObjectBuilder_WorldConfiguration.LastSaveTime = new DateTime?(checkpoint.LastSaveTime);

            bool SaveWorldConfigurationResult = false;
            _ = SaveWorldConfiguration(myObjectBuilder_WorldConfiguration, sessionPath, out ulong num, fileList, ref SaveWorldConfigurationResult);

            sizeInBytes += num;

            __result = SerializeXMLResult & SaveWorldConfigurationResult;

            Log.Info($"SaveCheckpoint Function done! result was {__result}");

            return false;
        }

        private static bool SaveWorldConfiguration(MyObjectBuilder_WorldConfiguration configuration, string sessionPath, out ulong sizeInBytes, List<MyCloudFile> fileList, ref bool __result)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.GameSaveFix)
            {
                sizeInBytes = 0UL;
                __result = false;
                return true;
            }

            // make sure we have data to work with.
            if (sessionPath == string.Empty || configuration == null)
            {
                sizeInBytes = 0UL;
                __result = false;
                return false;
            }

            string text = Path.Combine(sessionPath, "Sandbox_config.sbc");
            //MyLog.Default.WriteLineAndConsole("Saving Sandbox world configuration file " + text);

            // make sure we dont have duplicate in this list!
            if (fileList != null && !fileList.Contains(new MyCloudFile(text, false)))
                fileList.Add(new MyCloudFile(text, false));

            bool SerializeXMLResult = false;
            _ = SerializeXMLInternal(text, false, configuration, out sizeInBytes, ref SerializeXMLResult, null);

            __result = SerializeXMLResult;
            return false;
        }

        public static bool SaveSector(MyObjectBuilder_Sector sector, string sessionPath, Vector3I sectorPosition, out ulong sizeInBytes, List<MyCloudFile> fileList, ref bool __result)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.GameSaveFix)
            {
                sizeInBytes = 0UL;
                __result = false;
                return true;
            }

            if (sector == null || fileList == null)
            {
                sizeInBytes = 0UL;
                Log.Warn($"SaveSector Function FAILED! Sector or fileList NULL");
                __result = false;
                return false;
            }

            try
            {
                string sectorPath = GetSectorPath(sessionPath, sectorPosition);
                bool SerializeXMLResult = false;

                _ = CooldownManager.CheckCooldown(SteamIdCooldownKey.LoopSaveXML_ID, null, out var remainingSecondsToNexXML_Save);

                if (remainingSecondsToNexXML_Save < 5)
                {
                    // arm new timer.
                    int LoopCooldown = 1500 * 1000;
                    CooldownManager.StartCooldown(SteamIdCooldownKey.LoopSaveXML_ID, null, LoopCooldown);

                    if (ServerStart)
                    {
                        ServerStart = false;
                        goto Skip;
                    }

                    // lock the path to prevent possible crash here!
                    lock (sectorPath)
                        _ = SerializeXMLInternal(sectorPath, MyPlatformGameSettings.GAME_SAVES_COMPRESSED_BY_DEFAULT, sector, out sizeInBytes, ref SerializeXMLResult, null);

                    // make sure we dont have duplicate in this list!
                    if (!fileList.Contains(new MyCloudFile(sectorPath, false)))
                        fileList.Add(new MyCloudFile(sectorPath, false));

                    Log.Warn($"SaveSector: SANDBOX_0_00.sbs Save DONE! result is {SerializeXMLResult}");

                Skip:;
                }
                else
                {
                    SerializeXMLResult = true;
                    Log.Warn($"SaveSector: SANDBOX_0_00.sbs will be saved in {remainingSecondsToNexXML_Save} seconds");
                    Log.Warn($"SaveSector: Now saving SANDBOX_0_00.sbsB5 only");
                }

                string text = sectorPath + "B5";
                // lock the path to prevent possible crash here!
                lock (text)
                    SerializeXMLResult = SerializePBInternal(text, MyPlatformGameSettings.GAME_SAVES_COMPRESSED_BY_DEFAULT, sector, out sizeInBytes);

                // make sure we dont have duplicate in this list!
                if (!fileList.Contains(new MyCloudFile(text, false)))
                    fileList.Add(new MyCloudFile(text, false));

                Log.Warn($"SaveSector: SANDBOX_0_00.sbsB5 Save DONE! result is {SerializeXMLResult}");

                __result = SerializeXMLResult;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error during SaveSector Function!, Crash Avoided");
                sizeInBytes = 0UL;
                __result = false;
                return false;
            }

            return false;
        }

        // local function
        public static bool SerializeXMLInternal(string path, bool compress, MyObjectBuilder_Base objectBuilder, out ulong sizeInBytes, ref bool __result, Type serializeAsType = null)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.GameSaveFix)
            {
                __result = false;
                sizeInBytes = 0UL;
                return true;
            }

            if (path == string.Empty || objectBuilder == null)
            {
                __result = false;
                sizeInBytes = 0UL;
                return false;
            }

            // delete file if already somehow there.
            if (File.Exists(path))
                File.Delete(path);

            try
            {
                if (path.Contains("Sandbox.sbc"))
                    Log.Warn($"Now Saving Sandbox.sbc");
                else if (path.Contains("Sandbox_config.sbc"))
                    Log.Warn($"Now Saving Sandbox_config.sbc");
                else if (path.Contains("SANDBOX_0_0_0_.sbs"))
                    Log.Warn($"Now Saving SANDBOX_0_0_0_.sbs");

                var sizeInBytesTmp = 0UL;

                var MyXmlSerializer = Task.Run(async () =>
                {
                    using (Stream stream1 = MyFileSystem.OpenWrite(path, FileMode.Create))
                    {
                        // check if stream is not null
                        if (stream1 == null)
                            return sizeInBytesTmp;

                        using (Stream stream2 = compress ? stream1.WrapGZip() : stream1)
                        {
                            long position = stream1.Position;
                            MyXmlSerializerManager.GetSerializer(serializeAsType ?? objectBuilder.GetType()).Serialize(stream2, objectBuilder);
                            sizeInBytesTmp = (ulong)(stream1.Position - position);

                            // add Flush here
                            await stream2.FlushAsync();
                        }
                    }
                    return sizeInBytesTmp;
                });

                Task.WaitAll(MyXmlSerializer);

                sizeInBytes = sizeInBytesTmp;
                __result = true;
                return false;
            }
            catch (Exception ex)
            {
                MyLog.Default.WriteLine("Error: " + path + " failed to serialize.");
                MyLog.Default.WriteLine(ex.ToString());

                __result = false;
                sizeInBytes = 0UL;
                return false;
            }
        }

        // local function
        public static bool SerializePBInternal(string path, bool compress, MyObjectBuilder_Base objectBuilder, out ulong sizeInBytes)
        {
            bool result = false;
            var sizeInBytesTmp = 0UL;

            try
            {
                var ObjectBuilderSerializer = Task.Run(() =>
                {
                    using (Stream stream = MyFileSystem.OpenWrite(path, FileMode.Create))
                    {
                        if (stream == null)
                            result = false;

                        result = MyObjectBuilderSerializer.SerializePB(stream, compress, objectBuilder, out sizeInBytesTmp);
                    }

                    return result;
                });

                Task.WaitAll(ObjectBuilderSerializer);

                sizeInBytes = sizeInBytesTmp;
                return result;
            }
            catch (Exception ex)
            {
                MyLog.Default.WriteLine("Error: " + path + " failed to serialize.");
                MyLog.Default.WriteLine(ex.ToString());
                sizeInBytes = 0UL;
                result = false;
            }
            sizeInBytes = sizeInBytesTmp;
            return result;
        }

        // local function
        private static void SaveStartEnd(bool Start)
        {
            try
            {
                if (Sync.IsServer)
                {
                    if (Start)
                        NetworkManager.RaiseStaticEvent(OnServerSaving, true, target: default, null);
                    else
                        NetworkManager.RaiseStaticEvent(OnServerSaving, false, target: default, null);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error during SaveStartEnd Function! bool was {Start}, Crash Avoided");
            }
        }

        // local function
        private static bool CheckAccessToFiles(MySessionSnapshot __instance)
        {
            try
            {
                foreach (string text in Directory.GetFiles(__instance.TargetDir, "*", SearchOption.TopDirectoryOnly))
                {
                    if (!(text == MySession.Static.ThumbPath) && !MyFileSystem.CheckFileWriteAccess(text))
                    {
                        Log.Error(string.Format("Couldn't access file '{0}'.", Path.GetFileName(text)));
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during Game Save in CheckAccessToFiles Function! Crash Avoided");
                return false;
            }

            return true;
        }

        // local function
        private static bool SaveVoxelSnapshot(MySessionSnapshot __instance, string storageName, byte[] snapshotData, bool compress, out ulong size, List<MyCloudFile> fileList)
        {
            string path = storageName + ".vx2";
            string text = Path.Combine(__instance.SavingDir, path);

            try
            {
                // make sure we dont have duplicate in this list!
                if (!fileList.Contains(new MyCloudFile(text, false)))
                    fileList.Add(new MyCloudFile(text, false));

                if (compress)
                {
                    // was 16384 here, boost the buffer for large servers!
                    using (MemoryStream memoryStream = new MemoryStream(32768))
                    {
                        using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Compress))
                        {
                            gzipStream.Write(snapshotData, 0, snapshotData.Length);
                        }

                        byte[] array = memoryStream.ToArray();
                        File.WriteAllBytes(text, array);
                        size = (ulong)array.Length;

                        if (__instance.VoxelStorageNameCache != null)
                        {
                            if (__instance.VoxelStorageNameCache.TryGetValue(storageName, out IMyStorage myStorage) && !myStorage.Closed)
                                myStorage.SetDataCache(array, true);
                        }
                        goto ExitNow;
                    }
                }

                File.WriteAllBytes(text, snapshotData);
                size = (ulong)snapshotData.Length;

            ExitNow:;
            }
            catch (Exception ex)
            {
                size = 0UL;
                Log.Error(string.Format("Failed to write voxel file '{0}'", text));
                Log.Error(ex, "Error during Game Save in SaveVoxelSnapshot Function! Crash Avoided");
                return false;
            }

            // since this is local function, and not patched, return true.
            return true;
        }

        // local function
        private static string GetSectorPath(string sessionPath, Vector3I sectorPosition)
        {
            if (!sessionPath.EndsWith("/"))
                sessionPath += "/";

            return sessionPath + GetSectorName(sectorPosition) + ".sbs";
        }

        // local function
        private static string GetSectorName(Vector3I sectorPosition)
        {
            return string.Format("{0}_{1}_{2}_{3}_", new object[]
            {
                "SANDBOX",
                sectorPosition.X,
                sectorPosition.Y,
                sectorPosition.Z
            });
        }

        // local function
        private static void Backup(string targetDir, string backupDir)
        {
            if (MySession.Static.MaxBackupSaves > 0)
            {
                string path = DateTime.Now.ToString("yyyy-MM-dd HHmmss");
                string text = Path.Combine(backupDir, MyTextConstants.SESSION_SAVE_BACKUP_FOLDER, path);
                Directory.CreateDirectory(text);

                foreach (string text2 in Directory.GetFiles(targetDir))
                {
                    string text3 = Path.Combine(text, Path.GetFileName(text2));
                    if (text3.Length < 260 && text2.Length < 260)
                        File.Copy(text2, text3, true);
                }

                string[] directories = Directory.GetDirectories(Path.Combine(backupDir, MyTextConstants.SESSION_SAVE_BACKUP_FOLDER));
                if (!MySessionSnapshot.IsSorted(directories))
                    Array.Sort(directories);

                if (directories.Length > MySession.Static.MaxBackupSaves)
                {
                    int num = directories.Length - MySession.Static.MaxBackupSaves;

                    for (int j = 0; j < num; j++)
                    {
                        Directory.Delete(directories[j], true);
                    }
                    return;
                }
            }
            else if (MySession.Static.MaxBackupSaves == 0 && Directory.Exists(Path.Combine(backupDir, MyTextConstants.SESSION_SAVE_BACKUP_FOLDER)))
                Directory.Delete(Path.Combine(backupDir, MyTextConstants.SESSION_SAVE_BACKUP_FOLDER), true);
        }
    }
}