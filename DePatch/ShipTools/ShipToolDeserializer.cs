using System;

namespace DePatch
{
    public static class ShipToolDeserializer
    {
        public static ShipTool Deserialize(string raw)
        {
            string[] array = raw.Split(':');
            return new ShipTool
            {
                Type = (ToolType)Enum.Parse(typeof(ToolType), array[2]),
                Speed = float.Parse(array[1]),
                Subtype = array[0]
            };
        }
    }
}
