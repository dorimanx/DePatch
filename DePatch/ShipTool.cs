using System;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace DePatch
{
	public class ShipTool
	{
		// Token: 0x17000029 RID: 41
		// (get) Token: 0x060000A7 RID: 167 RVA: 0x00004BAE File Offset: 0x00002DAE
		// (set) Token: 0x060000A8 RID: 168 RVA: 0x00004BB6 File Offset: 0x00002DB6
		public ToolType Type { get; set; }

		// Token: 0x1700002A RID: 42
		// (get) Token: 0x060000A9 RID: 169 RVA: 0x00004BBF File Offset: 0x00002DBF
		// (set) Token: 0x060000AA RID: 170 RVA: 0x00004BC7 File Offset: 0x00002DC7
		public string Subtype { get; set; }

		// Token: 0x1700002B RID: 43
		// (get) Token: 0x060000AB RID: 171 RVA: 0x00004BD0 File Offset: 0x00002DD0
		// (set) Token: 0x060000AC RID: 172 RVA: 0x00004BD8 File Offset: 0x00002DD8
		public float Speed { get; set; }

		// Token: 0x0400005E RID: 94
		public static readonly ObservableCollection<ShipTool> shipTools = new ObservableCollection<ShipTool>();

		// Token: 0x0400005F RID: 95
		public static readonly float DEFAULT_SPEED = 0.75f;
	}
}
