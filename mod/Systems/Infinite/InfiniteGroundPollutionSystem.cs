using ExtraLandscapingTools.Systems.Jobs;
using Game;
using Game.Simulation;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace ExtraLandscapingTools.Systems.Infinite
{
    // Keeps ground (soil) pollution at 0, every update. Disabled by default and turned on only while
    // RegenMode is Infinite and PreventGroundPollution is checked. Distinct from groundwater
    // pollution (GroundWater.m_Polluted, handled by InfiniteGroundWaterSystem): this is
    // Game.Simulation.GroundPollution, the soil contamination produced by buildings and already
    // faded natively over time by GroundPollutionSystem's own PollutionFadeJob - unlike groundwater
    // pollution, which never decreases on its own.
    internal partial class InfiniteGroundPollutionSystem : GameSystemBase
    {
        // Same cadence as Game.Simulation.GroundPollutionSystem itself, the system that owns this
        // buffer (native fade job).
        private const int kUpdateInterval = 2048;

        private GroundPollutionSystem m_GroundPollutionSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            m_GroundPollutionSystem = World.GetOrCreateSystemManaged<GroundPollutionSystem>();
            Enabled = IsSettingEnabled(ELT.s_setting);
        }

        public override int GetUpdateInterval(SystemUpdatePhase phase)
        {
            return kUpdateInterval;
        }

        protected override void OnUpdate()
        {
            if (!IsSettingEnabled(ELT.s_setting)) return;

            NativeArray<GroundPollution> groundPollutionCells = m_GroundPollutionSystem.GetData(false, out JobHandle dependencies).m_Buffer;
            JobHandle jobHandle = JobHandle.CombineDependencies(Dependency, dependencies);

            JobHandle clearJobHandle = ClearGroundPollutionJob.Schedule(groundPollutionCells, 64, jobHandle);
            m_GroundPollutionSystem.AddWriter(clearJobHandle);
            Dependency = clearJobHandle;
        }

        private static bool IsSettingEnabled(ELTSettings settings) => settings != null && settings.RegenMode == RegenMode.Infinite && settings.PreventGroundPollution;

        // Called by the relevant ELTSettings setters (RegenMode, PreventGroundPollution) to re-sync
        // this system's enabled state live.
        internal static void Refresh()
        {
            InfiniteGroundPollutionSystem system = World.DefaultGameObjectInjectionWorld?.GetExistingSystemManaged<InfiniteGroundPollutionSystem>();
            if (system != null) system.Enabled = IsSettingEnabled(ELT.s_setting);
        }
    }
}
