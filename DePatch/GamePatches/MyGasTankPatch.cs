using NLog;
using ParallelTasks;
using Sandbox;
using Sandbox.Engine.Multiplayer;
using Sandbox.Game.Entities.Blocks;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using Torch.Managers.PatchManager;

namespace DePatch.GamePatches
{
    [PatchShim]

    internal static class MyGasTankPatch
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static readonly double ThresholdLow = 0.15;
        private static readonly uint FrameInterval = 60;
        private readonly static int Batches = 5;
        private static int Batch = 0;

        private static ulong LastFrameCounter = 0;

        internal readonly static MethodInfo OnFilledRatioCallback = typeof(MyGasTank).easyMethod("OnFilledRatioCallback");
        internal readonly static MethodInfo ChangeFilledRatio = typeof(MyGasTank).easyMethod("ChangeFilledRatio");

        private static Action<double> FilledCallback(MyGasTank x) => (Action<double>)OnFilledRatioCallback.CreateDelegate(typeof(Action<double>), x);
        public static ConcurrentDictionary<int, Tuple<MyGasTank, double>> TanksToUpdate = new ConcurrentDictionary<int, Tuple<MyGasTank, double>>();

        private static void Patch(PatchContext ctx)
        {
            ctx.GetPattern(OnFilledRatioCallback).
                Prefixes.Add(typeof(MyGasTankPatch).GetMethod(nameof(OnFilledRatioCallbackPatch), BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance));

            ctx.Prefix(typeof(MyGasTank), "ChangeFillRatioAmount", typeof(MyGasTankPatch), "ChangeFillRatioAmountPatch");
        }

        public static bool ChangeFillRatioAmountPatch(MyGasTank __instance, double newFilledRatio)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.GasTanksOptimization || __instance is null)
                return true;

            if (__instance.MarkedForClose || __instance.Closed)
            {
                _ = TanksToUpdate.TryRemove(__instance.GetHashCode(), out var tupleDispose);
                return true;
            }

            _ = ChangeFilledRatio.Invoke(__instance, new object[] { newFilledRatio, false });

            if (__instance.Capacity < 5000)
                return true;

            if (__instance.FilledRatio < ThresholdLow)
            {
                _ = TanksToUpdate.TryRemove(__instance.GetHashCode(), out var tupleDispose);
                return true;
            }

            var tuple = new Tuple<MyGasTank, double>(__instance, newFilledRatio);
            _ = TanksToUpdate.AddOrUpdate(__instance.GetHashCode(), tuple, (key, old) => tuple);

            return false;
        }

        public static bool OnFilledRatioCallbackPatch()
        {
            if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.GasTanksOptimization)
                return true;

            return false;
        }

        public static void UpdateClients(WorkData WorkBatch)
        {
            var WorkData = (UpdateWorkData)WorkBatch;
            if (WorkData is null)
                return;

            try
            {
                foreach (var TankID in TanksToUpdate.Keys)
                {

                    if ((TankID & int.MaxValue) % WorkData.TotalBatchsData != WorkData.BatchData)
                        continue;

                    TanksToUpdate.TryRemove(TankID, out var tuple);
                    if (tuple is null || tuple.Item1 is null)
                        continue;

                    MyMultiplayer.RaiseEvent(tuple.Item1, FilledCallback, tuple.Item2);
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "UpdateClients processing error!");
            }
        }

        public static void UpdateTanks()
        {
            if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.GasTanksOptimization)
                return;

            var FrameCounter = MySandboxGame.Static.SimulationFrameCounter;
            if (LastFrameCounter + FrameInterval > FrameCounter)
                return;

            LastFrameCounter = FrameCounter;

            Parallel.Start(UpdateClients, null, new UpdateWorkData(Batch, Batches));

            if (++Batch == Batches)
                Batch = 0;
        }

        public class UpdateWorkData : WorkData
        {
            public int BatchData;
            public int TotalBatchsData;

            public UpdateWorkData(int Batch, int TotalBatches)
            {
                BatchData = Batch;
                TotalBatchsData = TotalBatches;
            }
        }
    }
}