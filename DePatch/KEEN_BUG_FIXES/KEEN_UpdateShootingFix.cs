using System.Reflection;
using Torch.Managers.PatchManager;
using NLog;
using Sandbox.Game.Weapons;
using Sandbox.Game.Weapons.Guns.Barrels;

namespace DePatch.KEEN_BUG_FIXES
{
    [PatchShim]

    public static class KEEN_UpdateShootingFix
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static FieldInfo m_barrel;

        public static void Patch(PatchContext ctx)
        {
            m_barrel = typeof(MyLargeTurretBase).EasyField("m_barrel");

            ctx.Prefix(typeof(MyLargeTurretBase), typeof(KEEN_UpdateShootingFix), nameof(UpdateShooting));
        }

        private static bool UpdateShooting(MyLargeTurretBase __instance, bool shouldShoot)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.ShipToolsEnabled)
                return true;

            if (__instance == null)
                return false;

            var m_barrelEntity = (MyLargeBarrelBase)m_barrel.GetValue(__instance);
            if (m_barrelEntity == null)
                return false;

            return true;
        }
    }
}