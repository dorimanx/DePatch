using DePatch.CoolDown;
using NLog;
using Sandbox.Engine.Multiplayer;
using Sandbox.Game.World;
using System;
using System.Reflection;
using Torch.Managers.PatchManager;
using VRage.Utils;

namespace DePatch.GamePatches
{
	[PatchShim]

	public static class ValidationFailedClass
	{
		public static readonly Logger Log = LogManager.GetCurrentClassLogger();

		private static void Patch(PatchContext ctx)
		{
			ctx.GetPattern(typeof(MyMultiplayerServerBase).GetMethod("ValidationFailed", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)).
				Prefixes.Add(typeof(ValidationFailedClass).GetMethod(nameof(ValidationFailedLOG), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static));
			Log.Info("Patched ValidationFailed Spam Log");
		}

		private static bool ValidationFailedLOG(ulong clientId, bool kick = true, string additionalInfo = null, bool stackTrace = true)
		{
			if (DePatchPlugin.Instance.Config.ValidationFailedSuspend)
			{
				int LoopCooldown = 120 * 1000; // 120 sec
				SteamIdCooldownKey ValidationFailedRequestID = new SteamIdCooldownKey(clientId);

				_ = CooldownManager.CheckCooldown(ValidationFailedRequestID, null, out var remainingSecondsToNextLog);

				if (remainingSecondsToNextLog < 1)
				{
					CooldownManager.StartCooldown(ValidationFailedRequestID, null, LoopCooldown);

					string msg = MySession.Static.Players.TryGetIdentityNameFromSteamId(clientId) + (kick ? " was trying to cheat!" : "'s action was blocked.");
					MyLog.Default.WriteLine(msg);
					if (additionalInfo != null)
					{
						MyLog.Default.WriteLine(additionalInfo);
					}
					if (stackTrace)
					{
						MyLog.Default.WriteLine(Environment.StackTrace);
					}
				}
				return false;
			}
			return true;
		}
	}
}
