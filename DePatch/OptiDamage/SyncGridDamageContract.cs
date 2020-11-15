using System;
using System.Runtime.CompilerServices;
using ProtoBuf;

namespace DePatch
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
			this.CubeGridId = cubeGridId;
			this.DamageContracts = damageContracts;
		}
	}
}
