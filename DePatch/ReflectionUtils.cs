using System.Reflection;

namespace DePatch
{
    public static class ReflectionUtils
    {
        public static MethodInfo GetMethod<T>(
            string name,
            bool isPrivate = false)
        {
            return typeof(T).GetMethod(name, BindingFlags.Instance | (isPrivate ? BindingFlags.NonPublic : BindingFlags.Public));
        }

        public static FieldInfo GetField<T>(string name, bool isPrivate = false)
        {
            return typeof(T).GetField(name, BindingFlags.Instance | (isPrivate ? BindingFlags.NonPublic : BindingFlags.Public));
        }
    }
}
