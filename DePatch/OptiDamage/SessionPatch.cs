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
            if (DePatchPlugin.Instance.Config.DamageThreading)
            {
                ICollection<MyPlayer> onlinePlayers = MySession.Static.Players.GetOnlinePlayers();
                if (DamageNetwork.DamageQueue.Count != 0 && onlinePlayers.Count != 0 && Timer.ElapsedMilliseconds >= 500L)
                {
                    foreach (KeyValuePair<MyCubeGrid, List<DamageContract>> keyValuePair in DamageNetwork.DamageQueue.ToList().Where(b => b.Key != null && b.Value.Count > 0))
                    {
                        KeyValuePair<MyCubeGrid, List<DamageContract>> element = keyValuePair;
                        IEnumerable<MyPlayer> source = onlinePlayers.Where(b => b != null &&
                            b.Controller != null && b.Controller.ControlledEntity != null &&
                            b.Controller.ControlledEntity.Entity != null).Where(
                            b => Vector3D.DistanceSquared(b.Controller.ControlledEntity.Entity.PositionComp.GetPosition(),
                            element.Key.PositionComp.GetPosition()) < Math.Pow(MySession.Static.Settings.SyncDistance, 2.0));

                        byte[] contract = MyAPIGateway.Utilities.SerializeToBinary(new SyncGridDamageContract(element.Key.EntityId, element.Value.ToArray()));
                        Task.WaitAll(source.Select(b =>
                        {
                            return Task.Factory.StartNew(() => MyAPIGateway.Multiplayer.SendMessageTo(64467, contract, b.Id.SteamId, true));
                        }).ToArray());
                    }
                }
                Timer.Restart();
            }
        }
    }
}
