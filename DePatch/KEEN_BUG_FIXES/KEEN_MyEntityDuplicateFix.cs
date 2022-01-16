using System;
using System.Threading;
using HarmonyLib;
using NLog;
using Sandbox.Game.Entities;
using VRage;
using VRage.Game.Entity;
using VRageMath;

namespace DePatch.KEEN_BUG_FIXES
{
    [HarmonyPatch(typeof(MyEntity), "EntityId", MethodType.Setter)]
    internal class KEEN_MyEntityDuplicateFix
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static readonly Action<MyEntity, long> EntityIdSetter = (e, value) => AccessTools.Field(typeof(MyEntity), "m_entityId").SetValue(e, value);

        private static bool Prefix(MyEntity __instance, ref long value)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.MyEntityDuplicateFix)
                return true;

            var id = __instance.EntityId;

            if (id == 0L)
            {
                if (value == 0L)
                    return false;

                long NewValue = value;

                // here we check for duplicate ID and if found marking as used and adding new one. to avoid crash.
                if (MyEntityIdentifier.ExistsById(value))
                {
                    // if it's voxels remove the value id
                    if (__instance.GetType().ToString() == "Sandbox.Game.Entities.MyVoxelPhysics")
                    {
                        MyEntityIdentifier.RemoveEntity(value);
                        NewValue = MyEntityIdentifier.GetIdUniqueNumber(value);
                    }
                    else
                    {
                        MyEntityIdentifier.MarkIdUsed(value);

                        NewValue = MyEntityIdentifier.GetIdUniqueNumber(value);

                        if (MyEntityIdentifier.ExistsById(NewValue))
                        {
                            bool CheckNewValueID = true;
                            int loopCount = 0;

                            // make sure that new ID is not used.
                            while (CheckNewValueID)
                            {
                                if (MyEntityIdentifier.ExistsById(NewValue))
                                    NewValue = MyEntityIdentifier.GetIdUniqueNumber(NewValue);
                                else
                                    CheckNewValueID = false;

                                Thread.Sleep(2);
                                loopCount++;
                                if (loopCount > 120)
                                {
                                    loopCount = 0;
                                    Log.Warn($"ALERT!!! CheckNewValueID stuck in loop, cant allocate not existing ID.");
                                }
                            }
                        }

                        if (__instance.Name == value.ToString())
                            __instance.Name = NewValue.ToString();

                        SendSomeLogs(__instance, value, NewValue);
                    }
                }

                EntityIdSetter(__instance, NewValue);
                MyEntityIdentifier.AddEntityWithId(__instance);

                return false;
            }

            if (value == 0L)
            {
                EntityIdSetter(__instance, 0L);
                MyEntityIdentifier.RemoveEntity(id);

                return false;
            }

            EntityIdSetter(__instance, value);
            MyEntityIdentifier.SwapRegisteredEntityId(__instance, id, __instance.EntityId);

            return false;
        }

        private static void SendSomeLogs(MyEntity __instance, long value, long NewValue)
        {
            try
            {
                var ObjectLocation = __instance.PositionComp?.GetPosition();
                MyCubeGrid InstanceGrid = null;

                if (ObjectLocation == Vector3D.Zero && __instance is MyCubeBlock CubeBlock)
                {
                    InstanceGrid = CubeBlock.CubeGrid;
                    var Position = InstanceGrid.PositionComp?.GetPosition();

                    if (InstanceGrid != null && Position != null && Position != Vector3D.Zero)
                        ObjectLocation = InstanceGrid.PositionComp?.GetPosition();
                }

                // Create some LOG so we can try to find the problem if will not be fixed by giving new ID.
                if (ObjectLocation != null && ObjectLocation != Vector3D.Zero && InstanceGrid == null)
                    Log.Warn($"Found Duplicate EntityId {value} for Entity {__instance.GetType()} Block Located at GPS: {ObjectLocation} replaced with {NewValue}. Crash Avoided.");
                else if (ObjectLocation != null && ObjectLocation != Vector3D.Zero && InstanceGrid != null)
                    Log.Warn($"Found Duplicate EntityId {value} for Entity {__instance.GetType()} Grid Located at GPS: {ObjectLocation} with Grid name {InstanceGrid.DisplayName} replaced with {NewValue}. Crash Avoided.");
                else
                    Log.Warn($"Found Duplicate EntityId {value} for Entity {__instance.GetType()} replaced with {NewValue}. Crash Avoided.");
            }
            catch (Exception e)
            {
                Log.Error(e, $"Crash in send some logs code");
            }
        }
    }
}