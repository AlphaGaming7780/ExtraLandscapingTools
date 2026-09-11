using Game.Simulation;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace ExtraLandscapingTools.Systems
{
#if RELEASE
    [BurstCompile]
#endif
    internal struct AdjustGroundWaterJob : IJobParallelFor
    {
        public NativeArray<GroundWater> m_Buffer;
        [ReadOnly] public GroundWaterFlags m_Flags;

        public void Execute(int index)
        {
            GroundWater cell = m_Buffer[index];
            if ((m_Flags & GroundWaterFlags.Amount) != 0) cell.m_Amount = cell.m_Max;
            if ((m_Flags & GroundWaterFlags.Pollution) != 0) cell.m_Polluted = 0;
            m_Buffer[index] = cell;
        }
    }
}
