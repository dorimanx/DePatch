using Sandbox.Game;
using Sandbox.Game.Entities.Character;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace DePatch
{
    public static class ReflectionUtils
    {
        public static MethodInfo GetMethod<T>(string name, bool isPrivate = false)
        {
            return typeof(T).GetMethod(name, BindingFlags.Instance | (isPrivate ? BindingFlags.NonPublic : BindingFlags.Public));
        }

        public static FieldInfo GetField<T>(string name, bool isPrivate = false)
        {
            return typeof(T).GetField(name, BindingFlags.Instance | (isPrivate ? BindingFlags.NonPublic : BindingFlags.Public));
        }

        public static bool PlayersNarby(IMyCubeBlock block, int radius)
        {
            if (block != null)
            {
                BoundingSphereD sphere = new BoundingSphereD(block.GetPosition(), radius);
                List<IMyEntity> AllentitiesInsphere = MyAPIGateway.Entities.GetEntitiesInSphere(ref sphere);

                try
                {
                    IEnumerable<MyCharacter> enumerable() => AllentitiesInsphere.OfType<IMyCharacter>().Where(Player => !Player.IsDead && Player.IsPlayer).Select(Player => Player as MyCharacter);

                    if (enumerable().Any() && enumerable().Any((MyCharacter c) => MyVisualScriptLogicProvider.GetOnlinePlayers().Contains(c.GetPlayerIdentityId())))
                        return true;
                }
                catch
                {
                }
            }
            return false;
        }

        internal static object InvokeInstanceMethod(Type type, object instance, string methodName, Object[] args)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                                     | BindingFlags.Static;
            var method = type.GetMethod(methodName, bindFlags);
            return method.Invoke(instance, args);
        }
    }
}
