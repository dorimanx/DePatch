using System;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace DePatch
{
	[ProtoContract]
	public struct SyncGridDamageContract
	{
		// Token: 0x06000024 RID: 36 RVA: 0x00002E43 File Offset: 0x00001043
		public SyncGridDamageContract(long cubeGridId, DamageContract[] damageContracts)
		{
			this.CubeGridId = cubeGridId;
			this.DamageContracts = damageContracts;
		}

		// Token: 0x04000014 RID: 20
		[ProtoMember(1)]
		public long CubeGridId;

		// Token: 0x04000015 RID: 21
		[ProtoMember(2)]
		public DamageContract[] DamageContracts;
	}
}
