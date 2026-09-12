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
    internal struct ClearUsedResourceJob : IJobParallelFor
    {
        public NativeArray<NaturalResourceCell> m_Buffer;
        [ReadOnly] public ResourceFlags m_Flags;

        public void Execute(int index)
        {
#if ELT_PROFILING
            using var _ = s_ExecuteMarker.Auto();
#endif
            NaturalResourceCell cell = m_Buffer[index];
            if ((m_Flags & ResourceFlags.Fertility) != 0) cell.m_Fertility.m_Used = 0;
            if ((m_Flags & ResourceFlags.Oil) != 0) cell.m_Oil.m_Used = 0;
            if ((m_Flags & ResourceFlags.Ore) != 0) cell.m_Ore.m_Used = 0;
            if ((m_Flags & ResourceFlags.Fish) != 0) cell.m_Fish.m_Used = 0;
            m_Buffer[index] = cell;
        }

        internal static JobHandle Schedule(NativeArray<NaturalResourceCell> buffer, ResourceFlags flags, int innerloopBatchCount, JobHandle dependsOn)
        {
#if ELT_PROFILING
            using var _ = s_ScheduleMarker.Auto();
#endif
            ClearUsedResourceJob job = new() { m_Buffer = buffer, m_Flags = flags };
            return job.Schedule(buffer.Length, innerloopBatchCount, dependsOn);
        }

#if ELT_PROFILING
        private static readonly ProfilerMarker s_ExecuteMarker = new("ELT.ClearUsedResourceJob.Execute");
        private static readonly ProfilerMarker s_ScheduleMarker = new("ELT.ClearUsedResourceJob.Schedule");
#endif
    }
}
