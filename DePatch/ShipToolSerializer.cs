using System;
using System.Runtime.CompilerServices;

namespace DePatch
{
	public static class ShipToolSerializer
	{
		public static string Serialize(ShipTool shipTool)
		{
			return string.Format("{0}:{1:0.00}:{2}", shipTool.Subtype, shipTool.Speed, Enum.Format(typeof(ToolType), shipTool.Type, "g"));
		}
	}
}
