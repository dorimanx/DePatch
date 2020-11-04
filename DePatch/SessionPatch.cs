using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
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
		internal static void Prefix()
		{
			if (!DePatchPlugin.Instance.Config.DamageThreading)
			{
				return;
			}
			ICollection<MyPlayer> onlinePlayers = MySession.Static.Players.GetOnlinePlayers();
			if (DamageNetwork.DamageQueue.Count == 0 || onlinePlayers.Count == 0 || SessionPatch.Timer.ElapsedMilliseconds < 500L)
			{
				return;
			}
			using (IEnumerator<KeyValuePair<MyCubeGrid, List<DamageContract>>> enumerator = (from b in DamageNetwork.DamageQueue.ToList<KeyValuePair<MyCubeGrid, List<DamageContract>>>()
			where b.Key != null && b.Value.Count > 0
			select b).GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					KeyValuePair<MyCubeGrid, List<DamageContract>> element = enumerator.Current;
					IEnumerable<MyPlayer> source = from b in onlinePlayers
					where b != null && b.Controller != null && b.Controller.ControlledEntity != null && b.Controller.ControlledEntity.Entity != null
					where Vector3D.DistanceSquared(b.Controller.ControlledEntity.Entity.PositionComp.GetPosition(), element.Key.PositionComp.GetPosition()) < Math.Pow((double)MySession.Static.Settings.SyncDistance, 2.0)
					select b;
					byte[] contract = MyAPIGateway.Utilities.SerializeToBinary<SyncGridDamageContract>(new SyncGridDamageContract(element.Key.EntityId, element.Value.ToArray()));
					Task[] tasks = (from b in source
					select Task.Factory.StartNew<bool>(() => MyAPIGateway.Multiplayer.SendMessageTo(64467, contract, b.Id.SteamId, true))).ToArray<Task<bool>>();
					Task.WaitAll(tasks);
				}
			}
			SessionPatch.Timer.Restart();
		}
		public static Stopwatch Timer = new Stopwatch();
	}
}
