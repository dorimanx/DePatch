using ProtoBuf;

namespace DePatch.OptiDamage
{
    [ProtoContract]
    public struct SyncGridDamageContract
    {
        [ProtoMember(1)]
        public long CubeGridId;

        [ProtoMember(2)]
        public DamageContract[] DamageContracts;

        public SyncGridDamageContract(long cubeGridId, DamageContract[] damageContracts)
        {
            CubeGridId = cubeGridId;
            DamageContracts = damageContracts;
        }
    }
}
