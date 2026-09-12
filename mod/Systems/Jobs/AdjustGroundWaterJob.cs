using Game.Simulation;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
#if ELT_PROFILING
using Unity.Profiling;
#endif

namespace ExtraLandscapingTools.Systems.Jobs
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
#if ELT_PROFILING
            using var _ = s_ExecuteMarker.Auto();
#endif
            GroundWater cell = m_Buffer[index];
            if ((m_Flags & GroundWaterFlags.Amount) != 0) cell.m_Amount = cell.m_Max;
            if ((m_Flags & GroundWaterFlags.Pollution) != 0) cell.m_Polluted = 0;
            m_Buffer[index] = cell;
        }

        internal static JobHandle Schedule(NativeArray<GroundWater> buffer, GroundWaterFlags flags, int innerloopBatchCount, JobHandle dependsOn)
        {
#if ELT_PROFILING
            using var _ = s_ScheduleMarker.Auto();
#endif
            AdjustGroundWaterJob job = new() { m_Buffer = buffer, m_Flags = flags };
            return job.Schedule(buffer.Length, innerloopBatchCount, dependsOn);
        }

#if ELT_PROFILING
        private static readonly ProfilerMarker s_ExecuteMarker = new("ELT.AdjustGroundWaterJob.Execute");
        private static readonly ProfilerMarker s_ScheduleMarker = new("ELT.AdjustGroundWaterJob.Schedule");
#endif
    }
}
