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
		public static ObservableCollection<DrillSettings> drills = new ObservableCollection<DrillSettings>();

		public string Subtype { get; set; }

		public DrillingMode Mode { get; set; }

		public bool RightClick { get; set; }

		public float TickRate { get; set; } = 90f;

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
	}
}
