using NLog;
using Sandbox.Game;
using Sandbox.Game.Entities.Character;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Torch.Managers.PatchManager;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace DePatch
{
    public static class ReflectionUtils
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public const BindingFlags all = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        // Extensions by Dev Buddhist

        internal static object InvokeInstanceMethod(Type type, object instance, string methodName, object[] args)
        {
            var method = type.GetMethod(methodName, all);
            return method.Invoke(instance, args);
        }

        // Extensions by DEV Slimeradio

        public static FieldInfo EasyField(this Type type, string name, bool needThrow = true)
        {
            var ms = type.GetFields(all);
            foreach (var t in ms)
            {
                if (t.Name == name) { return t; }
            }

            Log.Error("Field not found: " + name);
            foreach (var t in ms)
            {
                Log.Error(type.Name + " -> " + t.Name);
                if (t.Name == name) { return t; }
            }

            if (needThrow) throw new Exception("Field " + name + " not found");
            return null;
        }

        public static PropertyInfo EasyProp(this Type type, String name, bool needThrow = true)
        {
            var ms = type.GetProperties(all);
            foreach (var t in ms)
            {
                if (t.Name == name) { return t; }
            }

            Log.Error("Property not found: " + name);
            foreach (var t in ms)
            {
                Log.Error(type.Name + " -> " + t.Name);
                if (t.Name == name) { return t; }
            }

            if (needThrow) throw new Exception("Property " + name + " not found");
            return null;
        }

        public static MethodInfo EasyMethod(this Type type, string name, bool needThrow = true, string[] names = null, Type[] types = null)
        {
            var ms = type.GetMethods(all);
            foreach (var t in ms)
            {
                if (t.Name == name)
                {
                    if (names == null) return t;

                    if (t.MatchMethod(names, types))
                        return t;
                }
            }

            if (needThrow) throw new Exception("Method " + name + " not found");
            return null;
        }

        /// <summary>
        /// Methods run before the original method is run. If they return false the original method is skipped.
        /// </summary>
        /// <param name="_ctx"></param>
        /// <param name="t"></param>
        /// <param name="t2"></param>
        /// <param name="name"></param>
        public static void Prefix(this PatchContext _ctx, Type t, Type t2, string name)
        {
            try
            {
                _ctx.GetPattern(t.EasyMethod(name)).Prefixes.Add(t2.EasyMethod(name));
            }
            catch (Exception e)
            {
                throw new Exception("Failed patch :" + name + " " + t, e);
            }
        }

        /// <summary>
        /// Methods run before the original method is run. If they return false the original method is skipped.
        /// </summary>
        /// <param name="_ctx"></param>
        /// <param name="t"></param>
        /// <param name="name"></param>
        /// <param name="t2"></param>
        /// <param name="name2"></param>
        public static void Prefix(this PatchContext _ctx, Type t, String name, Type t2, String name2)
        {
            try
            {
                _ctx.GetPattern(t.EasyMethod(name)).Prefixes.Add(t2.EasyMethod(name2));
            }
            catch
            {
                throw new Exception("Failed patch :" + name + " " + t);
            }
        }

        public static void Prefix(this PatchContext _ctx, Type t, Type t2, string name, string[] parametorNames = null)
        {
            try
            {
                _ctx.GetPattern(t.EasyMethod(name, names: parametorNames)).Prefixes.Add(t2.EasyMethod(name));
            }
            catch
            {
                throw new Exception("Failed patch :" + name + " " + t);
            }
        }

        public static void Prefix(this PatchContext _ctx, Type t, string name, Type t2, string name2, string[] parametorNames = null)
        {
            try
            {
                _ctx.GetPattern(t.EasyMethod(name, names: parametorNames)).Prefixes.Add(t2.EasyMethod(name2));
            }
            catch
            {
                throw new Exception("Failed patch :" + name + " " + t);
            }
        }

        public static void Suffix(this PatchContext _ctx, Type t, Type t2, string name)
        {
            try
            {
                _ctx.GetPattern(t.EasyMethod(name)).Suffixes.Add(t2.EasyMethod(name));
            }
            catch (Exception e)
            {
                throw new Exception("Failed patch :" + name + " " + t, e);
            }
        }

        public static void Suffix(this PatchContext _ctx, Type t, string name, Type t2, string name2)
        {
            try
            {
                _ctx.GetPattern(t.EasyMethod(name)).Suffixes.Add(t2.EasyMethod(name2));
            }
            catch (Exception e)
            {
                throw new Exception("Failed patch :" + name + " " + t, e);
            }
        }

        public static void Suffix(this PatchContext _ctx, Type t, string name, Type t2, string name2, string[] parametorNames = null)
        {
            try
            {
                _ctx.GetPattern(t.EasyMethod(name, names: parametorNames)).Suffixes.Add(t2.EasyMethod(name2));
            }
            catch
            {
                throw new Exception("Failed patch :" + name + " " + t);
            }
        }

        private static bool MatchMethod(this MethodInfo info, string[] names, Type[] types = null)
        {
            var pars = info.GetParameters();
            if (pars.Length != names.Length) return false;

            bool ok = true;
            for (var x = 0; x < pars.Length; x++)
            {
                if (types != null && pars[x].ParameterType != types[x])
                {
                    ok = false;
                    break;
                }

                if (pars[x].Name != names[x])
                {
                    ok = false;
                    break;
                }
            }

            return ok;
        }

        private static MethodInfo GetMethodPatch(this Type type, string name, BindingFlags flags)
        {
            return type.GetMethod(name, flags) ?? throw new Exception($"Couldn't find method {name} on {type}");
        }

        public static MethodInfo GetMethod_RYO(this Type t, string name)
        {
            return GetMethodPatch(t, name, all);
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
    }
}
