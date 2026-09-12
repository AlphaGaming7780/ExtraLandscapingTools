using ExtraLandscapingTools.Systems.Jobs;
using Game;
using Game.Simulation;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace ExtraLandscapingTools.Systems
{
    // Runs the one-shot "Refill depleted resource" / "Refill groundwater" / "Clean groundwater
    // pollution" / "Clean ground pollution" buttons. Disabled by default and only turned on for the
    // next frame when a button is clicked, so it costs nothing while idle.
    internal partial class ClearDepletedSystem : GameSystemBase
    {
        private static ResourceFlags s_pendingResourceFlags = ResourceFlags.None;
        private static GroundWaterFlags s_pendingGroundWaterFlags = GroundWaterFlags.None;
        private static bool s_pendingCleanGroundPollution = false;

        private NaturalResourceSystem m_NaturalResourceSystem;
        private GroundWaterSystem m_GroundWaterSystem;
        private GroundPollutionSystem m_GroundPollutionSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            m_NaturalResourceSystem = World.GetOrCreateSystemManaged<NaturalResourceSystem>();
            m_GroundWaterSystem = World.GetOrCreateSystemManaged<GroundWaterSystem>();
            m_GroundPollutionSystem = World.GetOrCreateSystemManaged<GroundPollutionSystem>();
            Enabled = false;
        }

        protected override void OnUpdate()
        {
            ResourceFlags resourceFlags = s_pendingResourceFlags;
            GroundWaterFlags groundWaterFlags = s_pendingGroundWaterFlags;
            bool cleanGroundPollution = s_pendingCleanGroundPollution;
            s_pendingResourceFlags = ResourceFlags.None;
            s_pendingGroundWaterFlags = GroundWaterFlags.None;
            s_pendingCleanGroundPollution = false;
            Enabled = false;

            if (resourceFlags != ResourceFlags.None)
            {
                NativeArray<NaturalResourceCell> naturalResourceCells = m_NaturalResourceSystem.GetData(false, out JobHandle resourceDependencies).m_Buffer;
                JobHandle resourceJobHandle = JobHandle.CombineDependencies(Dependency, resourceDependencies);

                JobHandle clearJobHandle = ClearUsedResourceJob.Schedule(naturalResourceCells, resourceFlags, 64, resourceJobHandle);
                m_NaturalResourceSystem.AddWriter(clearJobHandle);
                Dependency = clearJobHandle;
            }

            if (groundWaterFlags != GroundWaterFlags.None)
            {
                NativeArray<GroundWater> groundWaterCells = m_GroundWaterSystem.GetData(false, out JobHandle groundWaterDependencies).m_Buffer;
                JobHandle groundWaterJobHandle = JobHandle.CombineDependencies(Dependency, groundWaterDependencies);

                JobHandle adjustJobHandle = AdjustGroundWaterJob.Schedule(groundWaterCells, groundWaterFlags, 64, groundWaterJobHandle);
                m_GroundWaterSystem.AddWriter(adjustJobHandle);
                Dependency = adjustJobHandle;
            }

            if (cleanGroundPollution)
            {
                NativeArray<GroundPollution> groundPollutionCells = m_GroundPollutionSystem.GetData(false, out JobHandle groundPollutionDependencies).m_Buffer;
                JobHandle groundPollutionJobHandle = JobHandle.CombineDependencies(Dependency, groundPollutionDependencies);

                JobHandle clearGroundPollutionJobHandle = ClearGroundPollutionJob.Schedule(groundPollutionCells, 64, groundPollutionJobHandle);
                m_GroundPollutionSystem.AddWriter(clearGroundPollutionJobHandle);
                Dependency = clearGroundPollutionJobHandle;
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

        internal static void RequestCleanGroundPollution()
        {
            s_pendingCleanGroundPollution = true;
            Enable();
        }

        private static void Enable()
        {
            ClearDepletedSystem system = World.DefaultGameObjectInjectionWorld?.GetExistingSystemManaged<ClearDepletedSystem>();
            if (system != null) system.Enabled = true;
        }
    }
}
