using Game;
using Game.Simulation;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace ExtraLandscapingTools
{
    internal partial class MainSystem : GameSystemBase
    {
        internal static bool s_clearUsedOreResource = false;
        internal static bool s_clearUsedFertilityResource = false;
        internal static bool s_clearUsedOilResource = false;

        private NaturalResourceSystem _naturalResourceSystem;
        protected override void OnCreate()
        {
            base.OnCreate();
            _naturalResourceSystem = World.GetOrCreateSystemManaged<NaturalResourceSystem>();
        }

        protected override void OnUpdate()
        {

            if(s_clearUsedOreResource || s_clearUsedOilResource || s_clearUsedOreResource)
            {

                NativeArray<NaturalResourceCell> NaturalResourceCells = _naturalResourceSystem.GetData(false, out JobHandle dependencies).m_Buffer;
                JobHandle jobHandle = JobHandle.CombineDependencies(Dependency, dependencies);
                

                ClearUsedResourceJob clearUsedResourceJob = new()
                {
                    m_Buffer = NaturalResourceCells,
                    m_ClearUsedFertilityResource = s_clearUsedFertilityResource,
                    m_ClearUsedOilResource = s_clearUsedOilResource,
                    m_ClearUsedOreResource = s_clearUsedOreResource,
                };
                JobHandle ClearjobHandle = clearUsedResourceJob.Schedule(NaturalResourceCells.Length, 16, jobHandle);
                _naturalResourceSystem.AddWriter(ClearjobHandle);
                Dependency = ClearjobHandle;

                s_clearUsedOreResource = s_clearUsedOilResource = s_clearUsedFertilityResource = false;

            }

        }

#if RELEASE
        [BurstCompile]
#endif
        private struct ClearUsedResourceJob : IJobParallelFor
        {
            public NativeArray<NaturalResourceCell> m_Buffer;
            public bool m_ClearUsedOilResource;
            public bool m_ClearUsedOreResource;
            public bool m_ClearUsedFertilityResource;

            public void Execute(int index)
            {
                NaturalResourceCell cell = m_Buffer[index];
                if (m_ClearUsedFertilityResource) cell.m_Fertility.m_Used = 0; // cell.m_Fertility.m_Base;
                if (m_ClearUsedOilResource) cell.m_Oil.m_Used = 0; // cell.m_Oil.m_Base;
                if (m_ClearUsedOreResource) cell.m_Ore.m_Used = 0; // cell.m_Ore.m_Base;
                m_Buffer[index] = cell;
            }
        }

    }
}
