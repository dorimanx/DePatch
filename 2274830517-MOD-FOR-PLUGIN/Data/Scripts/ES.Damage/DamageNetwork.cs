using ProtoBuf;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Utils;

namespace ES.Damage
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    class DamageNetwork : MySessionComponentBase
    {
        internal const ushort DAMAGE_CHANNEL = 64467;
        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            base.Init(sessionComponent);
            if (MyAPIGateway.Multiplayer.IsServer || MyAPIGateway.Utilities.IsDedicated) throw new Exception("Only on clients!!!");
            MyAPIGateway.Session.OnSessionReady += Session_OnSessionReady;
        }

        private void Session_OnSessionReady()
        {
            MyAPIGateway.Multiplayer.RegisterMessageHandler(DAMAGE_CHANNEL, OnMessage);
        }

        private void OnMessage(byte[] raw)
        {
            try
            {
                var message = MyAPIGateway.Utilities.SerializeFromBinary<SyncGridDamageContract>(raw);
                if (message.DamageContracts == null) return;

                foreach (var item in message.DamageContracts)
                {
                    var block = GetByIdOrDefault<MyCubeBlock>(item.BlockId);
                    if (block == null || block.MarkedForClose) continue;

                    block.SlimBlock.DoDamage(item.Damage, item.DamageType, true, item.HitInfo, item.AttackerId);
                }                
            }
            catch (Exception e)
            {
                MyLog.Default.Error(new StringBuilder(e.ToString()));
            }
        }

        private T GetByIdOrDefault<T>(long id) where T : class
        {
            IMyEntity e;
            MyAPIGateway.Entities.TryGetEntityById(id, out e);
            return e as T;
        }
    }


    [ProtoContract]
    public struct DamageContract
    {
        public DamageContract(long blockId, float damage, MyStringHash damageType, MyHitInfo? hitInfo, long attackerId)
        {
            BlockId = blockId;
            Damage = damage;
            DamageType = damageType;
            HitInfo = hitInfo;
            AttackerId = attackerId;
        }

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
    }

    [ProtoContract]
    public struct SyncGridDamageContract
    {
        public SyncGridDamageContract(long cubeGridId, DamageContract[] damageContracts)
        {
            CubeGridId = cubeGridId;
            DamageContracts = damageContracts;
        }

        [ProtoMember(1)]
        public long CubeGridId;

        [ProtoMember(2)]
        public DamageContract[] DamageContracts;
    }
}
