using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Sandbox.Definitions;
using VRage.Game;

namespace DePatch
{
	internal class DrillSettings
	{
		// Token: 0x17000025 RID: 37
		// (get) Token: 0x06000075 RID: 117 RVA: 0x00003700 File Offset: 0x00001900
		// (set) Token: 0x06000076 RID: 118 RVA: 0x00003708 File Offset: 0x00001908
		public string Subtype { get; set; }

		// Token: 0x17000026 RID: 38
		// (get) Token: 0x06000077 RID: 119 RVA: 0x00003711 File Offset: 0x00001911
		// (set) Token: 0x06000078 RID: 120 RVA: 0x00003719 File Offset: 0x00001919
		public DrillingMode Mode { get; set; }

		// Token: 0x17000027 RID: 39
		// (get) Token: 0x06000079 RID: 121 RVA: 0x00003722 File Offset: 0x00001922
		// (set) Token: 0x0600007A RID: 122 RVA: 0x0000372A File Offset: 0x0000192A
		public bool RightClick { get; set; }

		// Token: 0x17000028 RID: 40
		// (get) Token: 0x0600007B RID: 123 RVA: 0x00003733 File Offset: 0x00001933
		// (set) Token: 0x0600007C RID: 124 RVA: 0x0000373B File Offset: 0x0000193B
		public float TickRate { get; set; } = 90f;

		// Token: 0x0600007D RID: 125 RVA: 0x00003744 File Offset: 0x00001944
		public static string Serialize(DrillSettings settings)
		{
			return string.Format("{0}:{1}:{2}:{3}", new object[]
			{
				settings.Subtype,
				settings.RightClick,
				Enum.Format(typeof(DrillingMode), settings.Mode, "g"),
				settings.TickRate
			});
		}

		// Token: 0x0600007E RID: 126 RVA: 0x000037A8 File Offset: 0x000019A8
		public static DrillSettings Deserialize(string raw)
		{
			string[] array = raw.Split(new char[]
			{
				':'
			});
			return new DrillSettings
			{
				Subtype = array[0],
				RightClick = bool.Parse(array[1]),
				Mode = (DrillingMode)Enum.Parse(typeof(DrillingMode), array[2]),
				TickRate = float.Parse(array[3])
			};
		}

		// Token: 0x0600007F RID: 127 RVA: 0x00003810 File Offset: 0x00001A10
		public static void InitDefinitions()
		{
			(from b in MyDefinitionManager.Static.GetAllDefinitions()
			where b is MyShipDrillDefinition
			select b into d
			where DrillSettings.drills.ToList<DrillSettings>().FindAll((DrillSettings b) => b.Subtype == d.Id.SubtypeName).Count == 0
			select d).ForEach(delegate(MyDefinitionBase d)
			{
				DrillSettings.drills.Add(new DrillSettings
				{
					Subtype = d.Id.SubtypeName
				});
			});
		}

		// Token: 0x04000041 RID: 65
		public static ObservableCollection<DrillSettings> drills = new ObservableCollection<DrillSettings>();
	}
}
