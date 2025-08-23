
namespace DePatch.CoolDown
{
    public class SteamIdCooldownKey : ICooldownKey
    {
        public static readonly SteamIdCooldownKey LoopOnBootRequestID = new SteamIdCooldownKey(76000000000000001);
        public static readonly SteamIdCooldownKey LoopAliveLogRequestID = new SteamIdCooldownKey(76000000000000002);
        public static readonly SteamIdCooldownKey LoopPlayerIdsSaveRequestID = new SteamIdCooldownKey(76000000000000003);
        public static readonly SteamIdCooldownKey LoopSaveXML_ID = new SteamIdCooldownKey(76000000000000004);
        public static readonly SteamIdCooldownKey LoopCrashComponents = new SteamIdCooldownKey(76000000000000005);
        public static readonly SteamIdCooldownKey LoopRadarRequestID = new SteamIdCooldownKey(76000000000000006);

        private ulong SteamId { get; }

        public SteamIdCooldownKey(ulong SteamId)
        {
            this.SteamId = SteamId;
        }

        public override bool Equals(object obj)
        {
            return obj is SteamIdCooldownKey key && SteamId == key.SteamId;
        }

        public override int GetHashCode()
        {
            return -80009682 + SteamId.GetHashCode();
        }
    }

    public class EntityIdCooldownKey : ICooldownKey
    {
        public static readonly EntityIdCooldownKey OverSpeedGridKey = new EntityIdCooldownKey(125446691099827825);

        private long EntityId { get; }

        public EntityIdCooldownKey(long EntityId)
        {
            this.EntityId = EntityId;
        }

        public override bool Equals(object obj)
        {
            return obj is EntityIdCooldownKey key && EntityId == key.EntityId;
        }

        public override int GetHashCode()
        {
            return -1619204625 + EntityId.GetHashCode();
        }
    }
}
