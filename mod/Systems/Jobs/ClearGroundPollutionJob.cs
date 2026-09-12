using Game.Simulation;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
#if ELT_PROFILING
using Unity.Profiling;
#endif

namespace ExtraLandscapingTools.Systems.Jobs
{
    // Shared by ClearDepletedSystem (one-shot button) and InfiniteGroundPollutionSystem
    // (persistent), the only two places that need to zero out GroundPollution.m_Pollution.
#if RELEASE
    [BurstCompile]
#endif
    internal struct ClearGroundPollutionJob : IJobParallelFor
    {
        public NativeArray<GroundPollution> m_Buffer;

        public void Execute(int index)
        {
#if ELT_PROFILING
            using var _ = s_ExecuteMarker.Auto();
#endif
            GroundPollution cell = m_Buffer[index];
            cell.m_Pollution = 0;
            m_Buffer[index] = cell;
        }

        internal static JobHandle Schedule(NativeArray<GroundPollution> buffer, int innerloopBatchCount, JobHandle dependsOn)
        {
#if ELT_PROFILING
            using var _ = s_ScheduleMarker.Auto();
#endif
            ClearGroundPollutionJob job = new() { m_Buffer = buffer };
            return job.Schedule(buffer.Length, innerloopBatchCount, dependsOn);
        }

#if ELT_PROFILING
        private static readonly ProfilerMarker s_ExecuteMarker = new("ELT.ClearGroundPollutionJob.Execute");
        private static readonly ProfilerMarker s_ScheduleMarker = new("ELT.ClearGroundPollutionJob.Schedule");
#endif
    }
}
