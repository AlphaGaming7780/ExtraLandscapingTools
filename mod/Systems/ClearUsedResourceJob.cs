using Game.Simulation;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace ExtraLandscapingTools.Systems
{
#if RELEASE
    [BurstCompile]
#endif
    internal struct ClearUsedResourceJob : IJobParallelFor
    {
        public NativeArray<NaturalResourceCell> m_Buffer;
        [ReadOnly] public ResourceFlags m_Flags;

        public void Execute(int index)
        {
            NaturalResourceCell cell = m_Buffer[index];
            if ((m_Flags & ResourceFlags.Fertility) != 0) cell.m_Fertility.m_Used = 0;
            if ((m_Flags & ResourceFlags.Oil) != 0) cell.m_Oil.m_Used = 0;
            if ((m_Flags & ResourceFlags.Ore) != 0) cell.m_Ore.m_Used = 0;
            if ((m_Flags & ResourceFlags.Fish) != 0) cell.m_Fish.m_Used = 0;
            m_Buffer[index] = cell;
        }
    }
}
