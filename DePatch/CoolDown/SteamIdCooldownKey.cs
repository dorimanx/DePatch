
namespace DePatch.CoolDown
{
    public class SteamIdCooldownKey : ICooldownKey
    {
        private ulong SteamId { get; }

        public SteamIdCooldownKey(ulong SteamId)
        {
            this.SteamId = SteamId;
        }

        public override bool Equals(object obj)
        {
            return obj is SteamIdCooldownKey key &&
                   SteamId == key.SteamId;
        }

        public override int GetHashCode()
        {
            return -80009682 + SteamId.GetHashCode();
        }
    }
}
