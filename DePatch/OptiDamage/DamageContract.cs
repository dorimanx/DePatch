using System;
using ProtoBuf;
using VRage.Game.ModAPI;
using VRage.Utils;

namespace DePatch
{
	[ProtoContract]
	public struct DamageContract
	{
		public DamageContract(long blockId, float damage, MyStringHash damageType, MyHitInfo? hitInfo, long attackerId)
		{
			this.BlockId = blockId;
			this.Damage = damage;
			this.DamageType = damageType;
			this.HitInfo = hitInfo;
			this.AttackerId = attackerId;
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
}
