using System;

namespace ExtraLandscapingTools.Systems
{
    [Flags]
    internal enum GroundWaterFlags
    {
        None = 0,
        Amount = 1 << 0,
        Pollution = 1 << 1,
    }
}
