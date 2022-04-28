using System;
using System.Reflection;
using NLog;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.Screens.Helpers;
using SpaceEngineers.Game.Entities.Blocks;
using Torch;
using Torch.Managers.PatchManager;
using VRage.Game;
using VRage.Network;

namespace DePatch.GamePatches
{
    [PatchShim]
    public static class MyToolbarPatch
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static FieldInfo m_items;
        public static FieldInfo TerminalBlock { get; private set; }
        public static readonly Type MyToolbarItemTerminalBlockClass = Type.GetType("Sandbox.Game.Screens.Helpers.MyToolbarItemTerminalBlock, Sandbox.Game");

        public static void Patch(PatchContext ctx)
        {
            m_items = typeof(MyToolbar).EasyField("m_items");
            TerminalBlock = MyToolbarItemTerminalBlockClass.EasyField("m_block");

            ctx.Suffix(typeof(MyToolbar), "SetItemAtIndexInternal", typeof(MyToolbarPatch), nameof(SetItemAtIndexInternalPatch));
        }

#pragma warning disable IDE0060 // Remove unused parameter
        private static void SetItemAtIndexInternalPatch(MyToolbar __instance, ref int i, ref MyToolbarItem item, ref bool initialization, ref bool gamepad)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.FixExploits)
                return;

            if (__instance is null || __instance.Owner is null || item is null || __instance.ToolbarType == MyToolbarType.Character)
                return;

            // forbid using Timer Block, to Attach/Detach rotor/piston/hinges head, to stop Clang Machines.
            if (DePatchPlugin.Instance.Config.FixTimerDetachExploits && __instance.Owner is MyTimerBlock && item.GetObjectBuilder() != null)
            {
                var steamId = MyEventContext.Current.Sender.Value;
                var requesterPlayer = Sync.Players.TryGetPlayerBySteamId(steamId);

                if (item is MyToolbarItemActions Action && Action.ActionId == "Detach")
                {
                    __instance.SetItemAtIndex(i, null, false);
                    __instance.SetItemAtIndex(i, null, true);

                    if (__instance.GetControllerPlayerID() != 0L)
                        MyVisualScriptLogicProvider.ClearToolbarSlot(i, __instance.GetControllerPlayerID());
                    else
                    {
                        if (requesterPlayer != null)
                            MyVisualScriptLogicProvider.ClearToolbarSlot(i, requesterPlayer.Identity.IdentityId);
                    }

                    return;
                }
            }

            if (initialization)
                return;

            // deny adding toolbar item if ToolBar Block is owned by nobody = 0L but do allow if toolbar item block is also owned by 0L or shared with all.
            if (((MyCubeBlock)__instance.Owner).IDModule != null && ((MyCubeBlock)__instance.Owner).IDModule.Owner == 0L)
            {
                try
                {
                    // item can change from MyToolbarItemTerminalBlock to MyTooMyToolbarItemTerminalGroup
                    var IsItGroup = item.GetObjectBuilder();
                    var steamId = MyEventContext.Current.Sender.Value;
                    var requesterPlayer = Sync.Players.TryGetPlayerBySteamId(steamId);

                    if (IsItGroup != null && IsItGroup.TypeId.ToString() == "MyObjectBuilder_ToolbarItemEmote")
                        return;

                    if (IsItGroup != null && IsItGroup.TypeId.ToString() != "MyObjectBuilder_ToolbarItemTerminalGroup")
                    {
                        if (!(item is MyToolbarItemWeapon) && TerminalBlock.GetValue(item) is MyTerminalBlock TerminalItemBlock)
                        {
                            if (TerminalItemBlock != null && TerminalItemBlock.IDModule != null && (TerminalItemBlock.IDModule.Owner == 0 || TerminalItemBlock.IDModule.ShareMode == MyOwnershipShareModeEnum.All))
                                return;

                            if (TerminalItemBlock != null && requesterPlayer != null && TerminalItemBlock.OwnerId == requesterPlayer.Identity.IdentityId)
                                return;
                        }
                    }

                    if (((MyToolbarItem[])m_items.GetValue(__instance))[i] != null)
                    {
                        __instance.SetItemAtIndex(i, null, false);
                        __instance.SetItemAtIndex(i, null, true);

                        if (__instance.GetControllerPlayerID() != 0L)
                            MyVisualScriptLogicProvider.ClearToolbarSlot(i, __instance.GetControllerPlayerID());
                        else
                            if (requesterPlayer != null)
                            MyVisualScriptLogicProvider.ClearToolbarSlot(i, requesterPlayer.Identity.IdentityId);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error during SetItemAtIndexInternalPatch For ToolBars! Crash Avoided");
                }
            }
        }
    }
}