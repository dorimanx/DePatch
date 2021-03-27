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

namespace DePatch.OptiDamage
{
    // not used in game so disable this.
    //[HarmonyPatch(typeof(MySession), "UpdateComponents")]
    public class SessionPatch
    {
        public static Stopwatch Timer = new Stopwatch();

        internal static void Prefix()
        {
            if (!DePatchPlugin.Instance.Config.DamageThreading)
            {
                return;
            }
            var onlinePlayers = MySession.Static.Players.GetOnlinePlayers();
            if (DamageNetwork.DamageQueue.Count == 0 || onlinePlayers.Count == 0 || Timer.ElapsedMilliseconds < 500)
            {
                return;
            }
            foreach (var element in from b in DamageNetwork.DamageQueue.ToList()
                                                                               where b.Key != null && b.Value.Count > 0
                                                                               select b)
            {
                var source = from b in onlinePlayers
                                               where b != null && b.Controller != null && b.Controller.ControlledEntity != null && b.Controller.ControlledEntity.Entity != null
                                               where Vector3D.DistanceSquared(b.Controller.ControlledEntity.Entity.PositionComp.GetPosition(), element.Key.PositionComp.GetPosition()) < Math.Pow(MySession.Static.Settings.SyncDistance, 2.0)
                                               select b;
                var contract = MyAPIGateway.Utilities.SerializeToBinary(new SyncGridDamageContract(element.Key.EntityId, element.Value.ToArray()));
                Task[] tasks = source.Select((MyPlayer b) => Task.Factory.StartNew(() => MyAPIGateway.Multiplayer.SendMessageTo(64467, contract, b.Id.SteamId))).ToArray();
                Task.WaitAll(tasks);
            }
            Timer.Restart();
            return;
        }
    }
}
