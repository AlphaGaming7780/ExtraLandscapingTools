using System;

namespace ExtraLandscapingTools.Systems
{
    [Flags]
    internal enum ResourceFlags
    {
        None = 0,
        Ore = 1 << 0,
        Fertility = 1 << 1,
        Oil = 1 << 2,
        Fish = 1 << 3,
    }
}
