using Sandbox.Engine.Multiplayer;
using Sandbox.Game.World;
using Torch.Managers.PatchManager;
using VRage.Network;

namespace DePatch.GamePatches
{
    public static class PlayerEmojiCleanup
    {
        public static void Patch(PatchContext ctx) => ctx.Prefix(typeof(MyDedicatedServerBase), typeof(PlayerEmojiCleanup), nameof(OnConnectedClient));

        private static void OnConnectedClient(ref ConnectedClientDataMsg msg, ulong steamId)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.ClearPlayerNameEmoji)
                return;

            if (!char.IsLetter(msg.Name[0]) && !char.IsNumber(msg.Name[0]))
            {
                msg.Name = msg.Name.Substring(1);

                var Playeridentity = MySession.Static.Players.TryGetPlayerIdentity(steamId);
                Playeridentity?.SetDisplayName(msg.Name);
            }
        }
    }
}