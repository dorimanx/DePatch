using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using Sandbox.Game.Entities;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using VRageMath;

namespace DePatch
{
    [HarmonyPatch(typeof(MySession), "UpdateComponents")]
    public class SessionPatch
    {
        public static Stopwatch Timer = new Stopwatch();

        internal static void Prefix()
        {
            if (!DePatchPlugin.Instance.Config.DamageThreading)
                return;

            ICollection<MyPlayer> onlinePlayers = MySession.Static.Players.GetOnlinePlayers();
            if (DamageNetwork.GetDamageQueue().Count == 0 || onlinePlayers.Count == 0 || SessionPatch.Timer.ElapsedMilliseconds < 500L)
                return;

            foreach (KeyValuePair<MyCubeGrid, List<DamageContract>> keyValuePair in DamageNetwork.GetDamageQueue().ToList<KeyValuePair<MyCubeGrid, List<DamageContract>>>().Where<KeyValuePair<MyCubeGrid, List<DamageContract>>>((Func<KeyValuePair<MyCubeGrid, List<DamageContract>>, bool>)(b => b.Key != null && b.Value.Count > 0)))
            {
                KeyValuePair<MyCubeGrid, List<DamageContract>> element = keyValuePair;
                IEnumerable<MyPlayer> source = onlinePlayers.Where<MyPlayer>((Func<MyPlayer, bool>)(b => b != null && b.Controller != null && b.Controller.ControlledEntity != null && b.Controller.ControlledEntity.Entity != null)).Where<MyPlayer>((Func<MyPlayer, bool>)(b => Vector3D.DistanceSquared(b.Controller.ControlledEntity.Entity.PositionComp.GetPosition(), element.Key.PositionComp.GetPosition()) < Math.Pow((double)MySession.Static.Settings.SyncDistance, 2.0)));
                byte[] contract = MyAPIGateway.Utilities.SerializeToBinary<SyncGridDamageContract>(new SyncGridDamageContract(element.Key.EntityId, element.Value.ToArray()));
                Func<MyPlayer, Task<bool>> selector = (Func<MyPlayer, Task<bool>>)(b => Task.Factory.StartNew<bool>((Func<bool>)(() => MyAPIGateway.Multiplayer.SendMessageTo((ushort)64467, contract, b.Id.SteamId, true))));
                Task.WaitAll((Task[])source.Select<MyPlayer, Task<bool>>(selector).ToArray<Task<bool>>());
            }
            SessionPatch.Timer.Restart();
        }
    }
}
