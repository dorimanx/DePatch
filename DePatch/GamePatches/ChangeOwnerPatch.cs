﻿using System;
using System.Reflection;
using NLog;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Blocks;
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
    public static class ChangeOwnerPatch
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private static FieldInfo m_items;

        public static FieldInfo Block { get; private set; }
        public static readonly Type MyToolbarItemTerminalBlockClass = Type.GetType("Sandbox.Game.Screens.Helpers.MyToolbarItemTerminalBlock, Sandbox.Game");

        internal static readonly MethodInfo ChangeOwner = typeof(MyCubeBlock).GetMethod("ChangeOwner", BindingFlags.Instance | BindingFlags.Public) ?? throw new Exception("Failed to find patch method");
        internal static readonly MethodInfo ChangeOwnerPatchTarget = typeof(ChangeOwnerPatch).GetMethod(nameof(ChangeOwnerPatchSuffix), BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic) ?? throw new Exception("Failed to find patch method");

        public static void Patch(PatchContext ctx)
        {
            m_items = typeof(MyToolbar).GetField("m_items", BindingFlags.NonPublic | BindingFlags.Instance);
            Block = MyToolbarItemTerminalBlockClass.GetField("m_block", BindingFlags.Instance | BindingFlags.NonPublic);

            ctx.GetPattern(ChangeOwner).Suffixes.Add(ChangeOwnerPatchTarget);
        }

        private static void ChangeOwnerPatchSuffix(MyCubeBlock __instance, ref long owner, MyOwnershipShareModeEnum shareMode)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.FixExploits)
                return;

            if (__instance is null || __instance.MarkedForClose || __instance.Closed)
                return;

            if (owner == 0)
            {
                var steamId = MyEventContext.Current.Sender.Value;
                var requesterPlayer = Sync.Players.TryGetPlayerBySteamId(steamId);

                switch (__instance)
                {
                    case MyButtonPanel ButtonPanelBlock:
                        {
                            for (int j = ButtonPanelBlock.Toolbar.ItemCount - 1; j >= 0; j--)
                            {
                                if (((MyToolbarItem[])m_items.GetValue(ButtonPanelBlock.Toolbar))[j] != null)
                                {
                                    var ToolbarBlock = ((MyToolbarItem[])m_items.GetValue(ButtonPanelBlock.Toolbar))[j];
                                    var IsItGroup = ToolbarBlock.GetObjectBuilder();

                                    if (IsItGroup != null && IsItGroup.TypeId.ToString() != "MyObjectBuilder_ToolbarItemTerminalGroup")
                                    {
                                        var ItemBlock = (MyTerminalBlock)Block.GetValue(ToolbarBlock);
                                        if (ItemBlock != null && ItemBlock.IDModule != null && (ItemBlock.IDModule.Owner == 0 || ItemBlock.IDModule.ShareMode == MyOwnershipShareModeEnum.All))
                                            continue;

                                        if (ItemBlock != null && requesterPlayer != null && ItemBlock.OwnerId == requesterPlayer.Identity.IdentityId)
                                            continue;
                                    }

                                    ButtonPanelBlock.Toolbar.SetItemAtIndex(j, null, false);
                                    ButtonPanelBlock.Toolbar.SetItemAtIndex(j, null, true);

                                    if (requesterPlayer != null)
                                        MyVisualScriptLogicProvider.ClearToolbarSlot(j, requesterPlayer.Identity.IdentityId);
                                }
                            }
                            break;
                        }
                    case MyTimerBlock TimerBlock:
                        {
                            for (int j = TimerBlock.Toolbar.ItemCount - 1; j >= 0; j--)
                            {
                                if (((MyToolbarItem[])m_items.GetValue(TimerBlock.Toolbar))[j] != null)
                                {
                                    var ToolbarBlock = ((MyToolbarItem[])m_items.GetValue(TimerBlock.Toolbar))[j];
                                    var IsItGroup = ToolbarBlock.GetObjectBuilder();

                                    if (IsItGroup != null && IsItGroup.TypeId.ToString() != "MyObjectBuilder_ToolbarItemTerminalGroup")
                                    {
                                        var ItemBlock = (MyTerminalBlock)Block.GetValue(ToolbarBlock);
                                        if (ItemBlock != null && ItemBlock.IDModule != null && (ItemBlock.IDModule.Owner == 0 || ItemBlock.IDModule.ShareMode == MyOwnershipShareModeEnum.All))
                                            continue;

                                        if (ItemBlock != null && requesterPlayer != null && ItemBlock.OwnerId == requesterPlayer.Identity.IdentityId)
                                            continue;
                                    }

                                    TimerBlock.Toolbar.SetItemAtIndex(j, null, false);
                                    TimerBlock.Toolbar.SetItemAtIndex(j, null, true);

                                    if (requesterPlayer != null)
                                        MyVisualScriptLogicProvider.ClearToolbarSlot(j, requesterPlayer.Identity.IdentityId);
                                }
                            }
                            break;
                        }
                    case MySensorBlock SensorBlock:
                        {
                            for (int j = SensorBlock.Toolbar.ItemCount - 1; j >= 0; j--)
                            {
                                if (((MyToolbarItem[])m_items.GetValue(SensorBlock.Toolbar))[j] != null)
                                {
                                    var ToolbarBlock = ((MyToolbarItem[])m_items.GetValue(SensorBlock.Toolbar))[j];
                                    var IsItGroup = ToolbarBlock.GetObjectBuilder();

                                    if (IsItGroup != null && IsItGroup.TypeId.ToString() != "MyObjectBuilder_ToolbarItemTerminalGroup")
                                    {
                                        var ItemBlock = (MyTerminalBlock)Block.GetValue(ToolbarBlock);
                                        if (ItemBlock != null && ItemBlock.IDModule != null && (ItemBlock.IDModule.Owner == 0 || ItemBlock.IDModule.ShareMode == MyOwnershipShareModeEnum.All))
                                            continue;

                                        if (ItemBlock != null && requesterPlayer != null && ItemBlock.OwnerId == requesterPlayer.Identity.IdentityId)
                                            continue;
                                    }

                                    SensorBlock.Toolbar.SetItemAtIndex(j, null, false);
                                    SensorBlock.Toolbar.SetItemAtIndex(j, null, true);

                                    if (requesterPlayer != null)
                                        MyVisualScriptLogicProvider.ClearToolbarSlot(j, requesterPlayer.Identity.IdentityId);
                                }
                            }
                            break;
                        }
                    case MyTargetDummyBlock TargetDummyBlock:
                        {
                            for (int j = TargetDummyBlock.Toolbar.ItemCount - 1; j >= 0; j--)
                            {
                                if (((MyToolbarItem[])m_items.GetValue(TargetDummyBlock.Toolbar))[j] != null)
                                {
                                    var ToolbarBlock = ((MyToolbarItem[])m_items.GetValue(TargetDummyBlock.Toolbar))[j];
                                    var IsItGroup = ToolbarBlock.GetObjectBuilder();

                                    if (IsItGroup != null && IsItGroup.TypeId.ToString() != "MyObjectBuilder_ToolbarItemTerminalGroup")
                                    {
                                        var ItemBlock = (MyTerminalBlock)Block.GetValue(ToolbarBlock);
                                        if (ItemBlock != null && ItemBlock.IDModule != null && (ItemBlock.IDModule.Owner == 0 || ItemBlock.IDModule.ShareMode == MyOwnershipShareModeEnum.All))
                                            continue;

                                        if (ItemBlock != null && requesterPlayer != null && ItemBlock.OwnerId == requesterPlayer.Identity.IdentityId)
                                            continue;
                                    }

                                    TargetDummyBlock.Toolbar.SetItemAtIndex(j, null, false);
                                    TargetDummyBlock.Toolbar.SetItemAtIndex(j, null, true);

                                    if (requesterPlayer != null)
                                        MyVisualScriptLogicProvider.ClearToolbarSlot(j, requesterPlayer.Identity.IdentityId);
                                }
                            }
                            break;
                        }
                    case MyRemoteControl RemoteControl:
                        {
                            for (int j = RemoteControl.Toolbar.ItemCount - 1; j >= 0; j--)
                            {
                                if (((MyToolbarItem[])m_items.GetValue(RemoteControl.Toolbar))[j] != null)
                                {
                                    var ToolbarBlock = ((MyToolbarItem[])m_items.GetValue(RemoteControl.Toolbar))[j];
                                    var IsItGroup = ToolbarBlock.GetObjectBuilder();

                                    if (IsItGroup != null && IsItGroup.TypeId.ToString() != "MyObjectBuilder_ToolbarItemTerminalGroup")
                                    {
                                        var ItemBlock = (MyTerminalBlock)Block.GetValue(ToolbarBlock);
                                        if (ItemBlock != null && ItemBlock.IDModule != null && (ItemBlock.IDModule.Owner == 0 || ItemBlock.IDModule.ShareMode == MyOwnershipShareModeEnum.All))
                                            continue;

                                        if (ItemBlock != null && requesterPlayer != null && ItemBlock.OwnerId == requesterPlayer.Identity.IdentityId)
                                            continue;
                                    }

                                    RemoteControl.Toolbar.SetItemAtIndex(j, null, false);
                                    RemoteControl.Toolbar.SetItemAtIndex(j, null, true);

                                    if (RemoteControl.Toolbar.GetControllerPlayerID() != 0L)
                                        MyVisualScriptLogicProvider.ClearToolbarSlot(j, RemoteControl.Toolbar.GetControllerPlayerID());
                                    else
                                        if (requesterPlayer != null)
                                        MyVisualScriptLogicProvider.ClearToolbarSlot(j, requesterPlayer.Identity.IdentityId);
                                }
                            }
                            break;
                        }
                    case MyShipController ShipController:
                        {
                            for (int j = ShipController.Toolbar.ItemCount - 1; j >= 0; j--)
                            {
                                if (((MyToolbarItem[])m_items.GetValue(ShipController.Toolbar))[j] != null)
                                {
                                    var ToolbarBlock = ((MyToolbarItem[])m_items.GetValue(ShipController.Toolbar))[j];
                                    var IsItGroup = ToolbarBlock.GetObjectBuilder();

                                    if (IsItGroup != null && IsItGroup.TypeId.ToString() != "MyObjectBuilder_ToolbarItemTerminalGroup")
                                    {
                                        var ItemBlock = (MyTerminalBlock)Block.GetValue(ToolbarBlock);
                                        if (ItemBlock != null && ItemBlock.IDModule != null && (ItemBlock.IDModule.Owner == 0 || ItemBlock.IDModule.ShareMode == MyOwnershipShareModeEnum.All))
                                            continue;

                                        if (ItemBlock != null && requesterPlayer != null && ItemBlock.OwnerId == requesterPlayer.Identity.IdentityId)
                                            continue;
                                    }

                                    ShipController.Toolbar.SetItemAtIndex(j, null, false);
                                    ShipController.Toolbar.SetItemAtIndex(j, null, true);

                                    if (ShipController.Toolbar.GetControllerPlayerID() != 0L)
                                        MyVisualScriptLogicProvider.ClearToolbarSlot(j, ShipController.Toolbar.GetControllerPlayerID());
                                    else
                                        if (requesterPlayer != null)
                                            MyVisualScriptLogicProvider.ClearToolbarSlot(j, requesterPlayer.Identity.IdentityId);
                                }
                            }
                            break;
                        }
                    default:
                        break;
                }
            }
        }
    }
}