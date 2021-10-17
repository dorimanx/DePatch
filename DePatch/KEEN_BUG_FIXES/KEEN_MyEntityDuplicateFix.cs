using System;
using HarmonyLib;
using VRage;
using VRage.Game.Entity;

namespace DePatch.KEEN_BUG_FIXES
{
    [HarmonyPatch(typeof(MyEntity), "EntityId", MethodType.Setter)]
    internal class KEEN_MyEntityDuplicateFix
    {
        private static readonly Action<MyEntity, long> EntityIdSetter = (e, value) => AccessTools.Field(typeof(MyEntity), "m_entityId").SetValue(e, value);

        private static bool Prefix(MyEntity __instance, ref long value)
        {
            if (!DePatchPlugin.Instance.Config.Enabled)
                return true;

            var id = __instance.EntityId;
            if (id == 0L)
            {
                if (value == 0L) return false;
                EntityIdSetter(__instance, value);

                // here we check for duplicate ID and if found removing to avoid crash.
                if (MyEntityIdentifier.ExistsById(value))
                    MyEntityIdentifier.RemoveEntity(value);

                MyEntityIdentifier.AddEntityWithId(__instance);
                return false;
            }

            if (value == 0L)
            {
                EntityIdSetter(__instance, 0L);
                MyEntityIdentifier.RemoveEntity(id);
                return false;
            }

            EntityIdSetter(__instance, value);
            MyEntityIdentifier.SwapRegisteredEntityId(__instance, id, __instance.EntityId);
            return false;
        }
    }
}