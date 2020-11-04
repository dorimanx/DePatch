using System;
using ProtoBuf;
using VRage.Game.ModAPI;
using VRage.Utils;

namespace DePatch
{
	// Token: 0x02000010 RID: 16
	[ProtoContract]
	public struct DamageContract
	{
		// Token: 0x06000023 RID: 35 RVA: 0x00002E1C File Offset: 0x0000101C
		public DamageContract(long blockId, float damage, MyStringHash damageType, MyHitInfo? hitInfo, long attackerId)
		{
			this.BlockId = blockId;
			this.Damage = damage;
			this.DamageType = damageType;
			this.HitInfo = hitInfo;
			this.AttackerId = attackerId;
		}

		// Token: 0x0400000F RID: 15
		[ProtoMember(1)]
		public long BlockId;

		// Token: 0x04000010 RID: 16
		[ProtoMember(2)]
		public float Damage;

		// Token: 0x04000011 RID: 17
		[ProtoMember(3)]
		public MyStringHash DamageType;

		// Token: 0x04000012 RID: 18
		[ProtoMember(4)]
		public MyHitInfo? HitInfo;

		// Token: 0x04000013 RID: 19
		[ProtoMember(5)]
		public long AttackerId;
	}
}
