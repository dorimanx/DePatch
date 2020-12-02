using System.Collections.ObjectModel;

namespace DePatch
{
    public class ShipTool
    {
        public static readonly ObservableCollection<ShipTool> shipTools = new ObservableCollection<ShipTool>();

        public static readonly float DEFAULT_SPEED = 0.75f;

        public ToolType Type { get; set; }

        public string Subtype { get; set; }

        public float Speed { get; set; }
    }
}
