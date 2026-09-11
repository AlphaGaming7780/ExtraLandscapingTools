using Game;
using Game.Simulation;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace ExtraLandscapingTools.Systems
{
    // Replicates Game.Simulation.GameModeNaturalResourcesAdjustSystem's percent-per-day refill for
    // natural resources, plus the equivalent for groundwater amount/pollution, driven by our own
    // settings instead of the gamemode prefab. Disabled by default and turned on only while RegenMode
    // is set to DailyRegen.
    //
    // Same update interval as GameModeNaturalResourcesAdjustSystem (128 updates/day), the system this
    // one is registered UpdateAfter in ELT.cs: UpdateSystem only inherits an anchor's exact tick
    // offset when the interval matches, so this has to line up with its anchor.
    internal partial class DailyRegenSystem : GameSystemBase
    {
        private const int kUpdatesPerDay = 128;
        private const int kUpdateInterval = TimeSystem.kTicksPerDay / kUpdatesPerDay;

        private NaturalResourceSystem m_NaturalResourceSystem;
        private GroundWaterSystem m_GroundWaterSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            m_NaturalResourceSystem = World.GetOrCreateSystemManaged<NaturalResourceSystem>();
            m_GroundWaterSystem = World.GetOrCreateSystemManaged<GroundWaterSystem>();
            Enabled = ELT.s_setting?.RegenMode == RegenMode.DailyRegen;
        }

        public override int GetUpdateInterval(SystemUpdatePhase phase)
        {
            return kUpdateInterval;
        }

        protected override void OnUpdate()
        {
            ELTSettings settings = ELT.s_setting;
            if (settings == null) return;

            RegenNaturalResources(settings);
            RegenGroundWater(settings);
        }

        private void RegenNaturalResources(ELTSettings settings)
        {
            bool regenOre = settings.OreRegenPercent > 0;
            bool regenOil = settings.OilRegenPercent > 0;
            bool regenFertility = settings.FertilityRegenPercent > 0;
            bool regenFish = settings.FishRegenPercent > 0;
            if (!regenOre && !regenOil && !regenFertility && !regenFish) return;

            NativeArray<NaturalResourceCell> naturalResourceCells = m_NaturalResourceSystem.GetData(false, out JobHandle dependencies).m_Buffer;
            JobHandle jobHandle = JobHandle.CombineDependencies(Dependency, dependencies);

            RegenNaturalResourceJob regenJob = new()
            {
                m_Buffer = naturalResourceCells,
                m_RegenOre = regenOre,
                m_RegenOil = regenOil,
                m_RegenFertility = regenFertility,
                m_RegenFish = regenFish,
                m_OreRegenPercent = settings.OreRegenPercent,
                m_OilRegenPercent = settings.OilRegenPercent,
                m_FertilityRegenPercent = settings.FertilityRegenPercent,
                m_FishRegenPercent = settings.FishRegenPercent,
            };
            JobHandle regenJobHandle = regenJob.Schedule(naturalResourceCells.Length, 64, jobHandle);
            m_NaturalResourceSystem.AddWriter(regenJobHandle);
            Dependency = regenJobHandle;
        }

        private void RegenGroundWater(ELTSettings settings)
        {
            bool regenAmount = settings.GroundWaterRegenPercent > 0;
            bool regenPollution = settings.GroundWaterPollutionReductionPercent > 0;
            if (!regenAmount && !regenPollution) return;

            NativeArray<GroundWater> groundWaterCells = m_GroundWaterSystem.GetData(false, out JobHandle dependencies).m_Buffer;
            JobHandle jobHandle = JobHandle.CombineDependencies(Dependency, dependencies);

            RegenGroundWaterJob regenJob = new()
            {
                m_Buffer = groundWaterCells,
                m_RegenAmount = regenAmount,
                m_RegenPollution = regenPollution,
                m_AmountRegenPercent = settings.GroundWaterRegenPercent,
                m_PollutionReductionPercent = settings.GroundWaterPollutionReductionPercent,
            };
            JobHandle regenJobHandle = regenJob.Schedule(groundWaterCells.Length, 64, jobHandle);
            m_GroundWaterSystem.AddWriter(regenJobHandle);
            Dependency = regenJobHandle;
        }

        // Called by ELTSettings.RegenMode's setter to turn this system on/off live.
        internal static void SetEnabled(bool enabled)
        {
            DailyRegenSystem system = World.DefaultGameObjectInjectionWorld?.GetExistingSystemManaged<DailyRegenSystem>();
            if (system != null) system.Enabled = enabled;
        }

#if RELEASE
        [BurstCompile]
#endif
        private struct RegenNaturalResourceJob : IJobParallelFor
        {
            public NativeArray<NaturalResourceCell> m_Buffer;
            [ReadOnly] public bool m_RegenOre;
            [ReadOnly] public bool m_RegenOil;
            [ReadOnly] public bool m_RegenFertility;
            [ReadOnly] public bool m_RegenFish;
            [ReadOnly] public int m_OreRegenPercent;
            [ReadOnly] public int m_OilRegenPercent;
            [ReadOnly] public int m_FertilityRegenPercent;
            [ReadOnly] public int m_FishRegenPercent;

            public void Execute(int index)
            {
                NaturalResourceCell cell = m_Buffer[index];

                if (m_RegenFertility) cell.m_Fertility.m_Used = Regen(cell.m_Fertility.m_Used, cell.m_Fertility.m_Base, m_FertilityRegenPercent);
                if (m_RegenOil) cell.m_Oil.m_Used = Regen(cell.m_Oil.m_Used, cell.m_Oil.m_Base, m_OilRegenPercent);
                if (m_RegenOre) cell.m_Ore.m_Used = Regen(cell.m_Ore.m_Used, cell.m_Ore.m_Base, m_OreRegenPercent);
                if (m_RegenFish) cell.m_Fish.m_Used = Regen(cell.m_Fish.m_Used, cell.m_Fish.m_Base, m_FishRegenPercent);

                m_Buffer[index] = cell;
            }

            private static ushort Regen(ushort used, ushort baseAmount, int percentPerDay)
            {
                return (ushort)math.max(0f, (float)(int)used - (float)(int)baseAmount * ((float)percentPerDay / 100f) / kUpdatesPerDay);
            }
        }

#if RELEASE
        [BurstCompile]
#endif
        private struct RegenGroundWaterJob : IJobParallelFor
        {
            public NativeArray<GroundWater> m_Buffer;
            [ReadOnly] public bool m_RegenAmount;
            [ReadOnly] public bool m_RegenPollution;
            [ReadOnly] public int m_AmountRegenPercent;
            [ReadOnly] public int m_PollutionReductionPercent;

            public void Execute(int index)
            {
                GroundWater cell = m_Buffer[index];

                if (m_RegenAmount)
                {
                    cell.m_Amount = (short)math.min((int)cell.m_Max, (int)cell.m_Amount + Step(cell.m_Max, m_AmountRegenPercent));
                }
                if (m_RegenPollution)
                {
                    cell.m_Polluted = (short)math.max(0, (int)cell.m_Polluted - Step(cell.m_Amount, m_PollutionReductionPercent));
                }

                m_Buffer[index] = cell;
            }

            private static int Step(int total, int percentPerDay)
            {
                return (int)((float)total * ((float)percentPerDay / 100f) / kUpdatesPerDay);
            }
        }
    }
}
