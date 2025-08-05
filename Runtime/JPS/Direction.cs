using System;

namespace UniformGridPathfinder.JPS
{
    [Flags]
    public enum Direction {
        None = 0b_0000_0000,
        North = 0b_0000_0001,
        East = 0b_0000_0010,
        South = 0b_0000_0100,
        West = 0b_0000_1000,
        NorthEast = North | East,
        SouthEast = South | East,
        SouthWest = South | West,
        NorthWest = North | West,
    }
}
