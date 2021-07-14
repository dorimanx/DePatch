using System.Collections.Generic;
using System.Reflection;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using Torch.Managers.PatchManager;
using VRage.Game.ModAPI;

namespace DePatch.GamePatches
{
    [PatchShim]

    internal static class MyBeaconAlertPatch
    {
        public static readonly List<string> BadNames = new List<string> { "Small Grid", "Static Grid", "Large Grid" };

        private static void Patch(PatchContext ctx)
        {
            ctx.GetPattern(typeof(MyEntityController).GetMethod("RaiseControlledEntityChanged", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)).
                Prefixes.Add(typeof(MyBeaconAlertPatch).GetMethod(nameof(RaiseControlledEntityChanged), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static));
        }

        private static bool IsBadName(string name)
        {
            foreach (string badName in BadNames)
            {
                if (name.Contains(badName))
                    return true;
            }
            return false;
        }

        private static void RaiseControlledEntityChanged(MyEntityController __instance)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.BeaconAlert)
                return;

            string text = "";
            if (__instance.ControlledEntity is MyCockpit)
            {
                new List<IMyBeacon>();
                List<IMySlimBlock> list = new List<IMySlimBlock>();
                IMyTerminalBlock myTerminalBlock = __instance.ControlledEntity as IMyTerminalBlock;
                if (myTerminalBlock != null)
                {
                    myTerminalBlock.CubeGrid.GetBlocks(list, null);
                    if (!list.Exists((IMySlimBlock x) => x.FatBlock != null && DePatchPlugin.Instance.Config.BeaconSubTypes.Contains(x.BlockDefinition.Id.SubtypeName)))
                        text = text + "\n" + DePatchPlugin.Instance.Config.WithOutBeaconText;

                    if (IsBadName(myTerminalBlock.CubeGrid.DisplayName))
                        text = text + "\n" + DePatchPlugin.Instance.Config.WithDefaultNameText;
                }
                if (text.Length > 0)
                {
                    MyVisualScriptLogicProvider.ShowNotification(DePatchPlugin.Instance.Config.RedAlertText, 10000, "Red", __instance.Player.Identity.IdentityId);
                    MyVisualScriptLogicProvider.ShowNotification(text, 10000, "Green", __instance.Player.Identity.IdentityId);
                }
            }
        }
    }
}
