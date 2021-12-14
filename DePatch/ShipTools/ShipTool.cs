﻿using System.Collections.ObjectModel;

namespace DePatch.ShipTools
{
    public class ShipTool
    {
        public static readonly ObservableCollection<ShipTool> shipTools = new ObservableCollection<ShipTool>();

        public static readonly float DEFAULT_SPEED_W = 0.5f;
        public static readonly float DEFAULT_SPEED_G = 2f;

        public ToolType Type { get; set; }

        public string Subtype { get; set; }

        public float Speed { get; set; }
    }
}
