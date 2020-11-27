using ProtoBuf;
using VRage.Game.ModAPI;
using VRage.Utils;

namespace DePatch
{
    [ProtoContract]
    public struct DamageContract
    {
        [ProtoMember(1)]
        public long BlockId;

        [ProtoMember(2)]
        public float Damage;

        [ProtoMember(3)]
        public MyStringHash DamageType;

        [ProtoMember(4)]
        public MyHitInfo? HitInfo;

        [ProtoMember(5)]
        public long AttackerId;

        public DamageContract(long blockId, float damage, MyStringHash damageType, MyHitInfo? hitInfo, long attackerId)
        {
            BlockId = blockId;
            Damage = damage;
            DamageType = damageType;
            HitInfo = hitInfo;
            AttackerId = attackerId;
        }
    }
}
