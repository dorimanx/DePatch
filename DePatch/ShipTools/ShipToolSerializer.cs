using System;
using System.Runtime.CompilerServices;

namespace DePatch
{
	public static class ShipToolSerializer
	{
		public static string Serialize(ShipTool shipTool)
		{
			return string.Format("{0}:{1:0.00}:{2}", (object) shipTool.Subtype, (object) shipTool.Speed, (object) Enum.Format(typeof(ToolType), (object) shipTool.Type, "g"));
		}
	}
}
