using ExtraLandscapingTools.Systems.Jobs;
using Game;
using Game.Prefabs.Modes;
using Game.Simulation;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace ExtraLandscapingTools.Systems.Infinite
{
    // Keeps the resources selected in the "Infinite Resources" settings group at 0 used, every
    // update. Disabled by default and turned on only while RegenMode is set to Infinite.
    internal partial class InfiniteResourceSystem : GameSystemBase
    {
        // Same cadence as Game.Simulation.AreaLotSimulationSystem, the system that actually writes
        // extraction back into NaturalResourceCell.m_Used (ExtractResourcesJob) with no per-entity
        // staggering: matching it keeps a resource from ever looking depleted between our updates.
        private const int kUpdateInterval = 512;

        private NaturalResourceSystem m_NaturalResourceSystem;
        private EntityQuery m_GameModeSettingQuery;

        protected override void OnCreate()
        {
            base.OnCreate();
            m_NaturalResourceSystem = World.GetOrCreateSystemManaged<NaturalResourceSystem>();
            m_GameModeSettingQuery = EntityManager.CreateEntityQuery(ComponentType.ReadOnly<ModeSettingData>());
            Enabled = ELT.s_setting?.RegenMode == RegenMode.Infinite;
        }

        public override int GetUpdateInterval(SystemUpdatePhase phase)
        {
            return kUpdateInterval;
        }

        protected override void OnUpdate()
        {
            ResourceFlags flags = GetPersistentInfiniteFlags(ELT.s_setting);
            if (flags == ResourceFlags.None) return;

            NativeArray<NaturalResourceCell> naturalResourceCells = m_NaturalResourceSystem.GetData(false, out JobHandle dependencies).m_Buffer;
            JobHandle jobHandle = JobHandle.CombineDependencies(Dependency, dependencies);

            JobHandle clearJobHandle = ClearUsedResourceJob.Schedule(naturalResourceCells, flags, 64, jobHandle);
            m_NaturalResourceSystem.AddWriter(clearJobHandle);
            Dependency = clearJobHandle;
        }

        private static ResourceFlags GetPersistentInfiniteFlags(ELTSettings settings)
        {
            if (settings == null || settings.RegenMode != RegenMode.Infinite) return ResourceFlags.None;

            ResourceFlags flags = ResourceFlags.None;
            if (settings.InfiniteOreResource) flags |= ResourceFlags.Ore;
            if (settings.InfiniteOilResource) flags |= ResourceFlags.Oil;
            if (settings.InfiniteFertilityResource) flags |= ResourceFlags.Fertility;
            if (settings.InfiniteFishResource) flags |= ResourceFlags.Fish;
            return flags;
        }

        private ResourceFlags GetGameModeInfiniteFlags()
        {
            if (m_GameModeSettingQuery.IsEmptyIgnoreFilter) return ResourceFlags.None;

            ModeSettingData modeSettingData = m_GameModeSettingQuery.GetSingleton<ModeSettingData>();
            if (!modeSettingData.m_Enable || !modeSettingData.m_EnableAdjustNaturalResources) return ResourceFlags.None;

            ResourceFlags flags = ResourceFlags.None;
            if (modeSettingData.m_PercentOreRefillAmountPerDay >= 100) flags |= ResourceFlags.Ore;
            if (modeSettingData.m_PercentOilRefillAmountPerDay >= 100) flags |= ResourceFlags.Oil;
            if (modeSettingData.m_PercentFertilityRefillAmountPerDay >= 100) flags |= ResourceFlags.Fertility;
            return flags;
        }

        internal bool IsGameModeInfinite(ResourceFlags flag) => (GetGameModeInfiniteFlags() & flag) != 0;

        // Called by ELTSettings.RegenMode's setter to turn this system on/off live.
        internal static void SetEnabled(bool enabled)
        {
            InfiniteResourceSystem system = World.DefaultGameObjectInjectionWorld?.GetExistingSystemManaged<InfiniteResourceSystem>();
            if (system != null) system.Enabled = enabled;
        }

        // Called by the settings UI to grey out a refill button already covered by the currently
        // active gamemode's own resource regen.
        internal static bool IsGameModeResourceInfinite(ResourceFlags flag)
        {
            InfiniteResourceSystem system = World.DefaultGameObjectInjectionWorld?.GetExistingSystemManaged<InfiniteResourceSystem>();
            return system != null && system.IsGameModeInfinite(flag);
        }
    }
}
