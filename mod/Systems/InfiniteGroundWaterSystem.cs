using Game;
using Game.Simulation;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace ExtraLandscapingTools.Systems
{
    // Keeps groundwater full and/or unpolluted, every update, depending on which of the two settings
    // is checked. Disabled by default and turned on only while RegenMode is Infinite and at least one
    // of the two groundwater toggles is on.
    internal partial class InfiniteGroundWaterSystem : GameSystemBase
    {
        // Same cadence as Game.Simulation.GroundWaterSystem itself, the system that owns this buffer
        // (flow between cells, pollution diffusion, its own native replenish rate).
        private const int kUpdateInterval = 128;

        private GroundWaterSystem m_GroundWaterSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            m_GroundWaterSystem = World.GetOrCreateSystemManaged<GroundWaterSystem>();
            Enabled = GetPersistentFlags(ELT.s_setting) != GroundWaterFlags.None;
        }

        public override int GetUpdateInterval(SystemUpdatePhase phase)
        {
            return kUpdateInterval;
        }

        protected override void OnUpdate()
        {
            GroundWaterFlags flags = GetPersistentFlags(ELT.s_setting);
            if (flags == GroundWaterFlags.None) return;

            NativeArray<GroundWater> groundWaterCells = m_GroundWaterSystem.GetData(false, out JobHandle dependencies).m_Buffer;
            JobHandle jobHandle = JobHandle.CombineDependencies(Dependency, dependencies);

            AdjustGroundWaterJob adjustJob = new()
            {
                m_Buffer = groundWaterCells,
                m_Flags = flags,
            };
            JobHandle adjustJobHandle = adjustJob.Schedule(groundWaterCells.Length, 64, jobHandle);
            m_GroundWaterSystem.AddWriter(adjustJobHandle);
            Dependency = adjustJobHandle;
        }

        private static GroundWaterFlags GetPersistentFlags(ELTSettings settings)
        {
            if (settings == null || settings.RegenMode != RegenMode.Infinite) return GroundWaterFlags.None;

            GroundWaterFlags flags = GroundWaterFlags.None;
            if (settings.InfiniteGroundWater) flags |= GroundWaterFlags.Amount;
            if (settings.PreventGroundWaterPollution) flags |= GroundWaterFlags.Pollution;
            return flags;
        }

        // Called by the relevant ELTSettings setters (RegenMode, InfiniteGroundWater,
        // PreventGroundWaterPollution) to re-sync this system's enabled state live.
        internal static void Refresh()
        {
            InfiniteGroundWaterSystem system = World.DefaultGameObjectInjectionWorld?.GetExistingSystemManaged<InfiniteGroundWaterSystem>();
            if (system != null) system.Enabled = GetPersistentFlags(ELT.s_setting) != GroundWaterFlags.None;
        }
    }
}
