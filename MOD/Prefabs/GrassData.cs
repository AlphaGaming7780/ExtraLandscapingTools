using ExtraLandscapingTools.Systems.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace ExtraLandscapingTools.Prefabs
{
    public struct GrassData : IComponentData
    {
        public GrassToolSystem.State m_State;
    }
}
