using Sandbox.Game;
using Sandbox.Game.Entities.Character;
using Sandbox.ModAPI;
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
                try
                {
                    BoundingSphereD sphere = new BoundingSphereD(block.GetPosition(), radius);
                    List<IMyEntity> AllentitiesInsphere = MyAPIGateway.Entities.GetEntitiesInSphere(ref sphere);
                    IEnumerable<MyCharacter> enumerable()
                    {
                        foreach (var Player in AllentitiesInsphere.OfType<IMyCharacter>())
                        {
                            if (!Player.IsDead && Player.IsPlayer)
                            {
                                yield return Player as MyCharacter;
                            }
                        }
                    }

                    IEnumerable<MyCharacter> characters = enumerable();
                    if (characters.Any())
                    {
                        List<long> onlinePlayers = MyVisualScriptLogicProvider.GetOnlinePlayers();
                        if (characters.Any((MyCharacter c) => onlinePlayers.Contains(c.GetPlayerIdentityId())))
                            return true;
                    }
                }
                catch
                {
                }
            return false;
        }
    }
}
