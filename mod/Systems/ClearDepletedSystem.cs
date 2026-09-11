using Game;
using Game.Simulation;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace ExtraLandscapingTools.Systems
{
    // Runs the one-shot "Refill depleted resource" / "Refill groundwater" / "Clean groundwater
    // pollution" buttons. Disabled by default and only turned on for the next frame when a button is
    // clicked, so it costs nothing while idle.
    internal partial class ClearDepletedSystem : GameSystemBase
    {
        private static ResourceFlags s_pendingResourceFlags = ResourceFlags.None;
        private static GroundWaterFlags s_pendingGroundWaterFlags = GroundWaterFlags.None;

        private NaturalResourceSystem m_NaturalResourceSystem;
        private GroundWaterSystem m_GroundWaterSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            m_NaturalResourceSystem = World.GetOrCreateSystemManaged<NaturalResourceSystem>();
            m_GroundWaterSystem = World.GetOrCreateSystemManaged<GroundWaterSystem>();
            Enabled = false;
        }

        protected override void OnUpdate()
        {
            ResourceFlags resourceFlags = s_pendingResourceFlags;
            GroundWaterFlags groundWaterFlags = s_pendingGroundWaterFlags;
            s_pendingResourceFlags = ResourceFlags.None;
            s_pendingGroundWaterFlags = GroundWaterFlags.None;
            Enabled = false;

            if (resourceFlags != ResourceFlags.None)
            {
                NativeArray<NaturalResourceCell> naturalResourceCells = m_NaturalResourceSystem.GetData(false, out JobHandle resourceDependencies).m_Buffer;
                JobHandle resourceJobHandle = JobHandle.CombineDependencies(Dependency, resourceDependencies);

                ClearUsedResourceJob clearUsedResourceJob = new()
                {
                    m_Buffer = naturalResourceCells,
                    m_Flags = resourceFlags,
                };
                JobHandle clearJobHandle = clearUsedResourceJob.Schedule(naturalResourceCells.Length, 64, resourceJobHandle);
                m_NaturalResourceSystem.AddWriter(clearJobHandle);
                Dependency = clearJobHandle;
            }

            if (groundWaterFlags != GroundWaterFlags.None)
            {
                NativeArray<GroundWater> groundWaterCells = m_GroundWaterSystem.GetData(false, out JobHandle groundWaterDependencies).m_Buffer;
                JobHandle groundWaterJobHandle = JobHandle.CombineDependencies(Dependency, groundWaterDependencies);

                AdjustGroundWaterJob adjustJob = new()
                {
                    m_Buffer = groundWaterCells,
                    m_Flags = groundWaterFlags,
                };
                JobHandle adjustJobHandle = adjustJob.Schedule(groundWaterCells.Length, 64, groundWaterJobHandle);
                m_GroundWaterSystem.AddWriter(adjustJobHandle);
                Dependency = adjustJobHandle;
            }
        }

        internal static void RequestClearResource(ResourceFlags flags)
        {
            s_pendingResourceFlags |= flags;
            Enable();
        }

        internal static void RequestClearGroundWater(GroundWaterFlags flags)
        {
            s_pendingGroundWaterFlags |= flags;
            Enable();
        }

        private static void Enable()
        {
            ClearDepletedSystem system = World.DefaultGameObjectInjectionWorld?.GetExistingSystemManaged<ClearDepletedSystem>();
            if (system != null) system.Enabled = true;
        }
    }
}
