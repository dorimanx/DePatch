using System;
using System.Collections.Generic;
using Sandbox.Game.Entities;

namespace DePatch.BlocksDisable
{
    public class GridOverSpeed
    {
        private int _warningsCount;
        public MyCubeGrid Grid { get; set; }

        public int WarningsCount
        {
            get => _warningsCount;
            set
            {
                _warningsCount = LastChanged >= DateTime.Now.AddMinutes(-10) ? 0 : value;
                LastChanged = DateTime.Now;
            }
        }
        public DateTime LastChanged { get; set; }

        public GridOverSpeed(MyCubeGrid grid)
        {
            Grid = grid;
            _warningsCount = 0;
            LastChanged = DateTime.Now;
        }

        public class GridOverSpeedComparer : IEqualityComparer<GridOverSpeed>
        {
            public bool Equals(GridOverSpeed x, GridOverSpeed y)
            {
                return Equals(x.Grid, y.Grid);
            }

            public int GetHashCode(GridOverSpeed obj)
            {
                return obj.Grid != null ? obj.Grid.GetHashCode() : 0;
            }
        }
    }
}