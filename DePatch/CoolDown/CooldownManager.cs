using System.Collections.Generic;

namespace DePatch.CoolDown
{
    public interface ICooldownKey
    {
        /* Marker Interface */
    }

    public class CooldownManager
    {
        private static readonly Dictionary<ICooldownKey, CurrentCooldown> cooldownMap = new Dictionary<ICooldownKey, CurrentCooldown>();

        public static bool CheckCooldown(ICooldownKey key, string command, out long remainingSeconds)
        {
            remainingSeconds = 0;

            if (cooldownMap.TryGetValue(key, out CurrentCooldown currentCooldown))
            {
                remainingSeconds = currentCooldown.GetRemainingSeconds(command);

                if (remainingSeconds > 0)
                    return false;
            }
            return true;
        }

        public static void StartCooldown(ICooldownKey key, string command, long cooldown)
        {
            var currentCooldown = new CurrentCooldown(cooldown);

            if (cooldownMap.ContainsKey(key))
                cooldownMap[key] = currentCooldown;
            else
                cooldownMap.Add(key, currentCooldown);

            currentCooldown.StartCooldown(command);
        }

        public static void StopCooldown(ICooldownKey key)
        {
            if (cooldownMap.ContainsKey(key))
                cooldownMap.Remove(key);
        }
    }
}