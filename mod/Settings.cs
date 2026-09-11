using ExtraLandscapingTools.Systems;
using Game.Modding;
using Game.Settings;

namespace ExtraLandscapingTools
{
    [SettingsUIGroupOrder(kRegenModeGroup, kDailyRegenGroup, kInfiniteResourceGroup, kDepletedResourceGroup)]
    [SettingsUIShowGroupName(kRegenModeGroup, kInfiniteResourceGroup, kDailyRegenGroup, kDepletedResourceGroup)]
    internal class ELTSettings : ModSetting
    {
        public ELTSettings(IMod mod) : base(mod) { }

        public const string kMainSection = "Main";
        public const string kDepletedResourceGroup = "DepletedResource";
        public const string kInfiniteResourceGroup = "InfiniteResource";
        public const string kDailyRegenGroup = "DailyRegen";
        public const string kRegenModeGroup = "RegenMode";

        #region Regen Mode

        private RegenMode m_RegenMode = RegenMode.Disabled;

        // A dropdown makes "Infinite" and "DailyRegen" structurally mutually exclusive - there's no
        // way to select both at once, unlike two independent checkboxes.
        [SettingsUISection(kMainSection, kRegenModeGroup)]
        public RegenMode RegenMode
        {
            get => m_RegenMode;
            set
            {
                m_RegenMode = value;
                InfiniteResourceSystem.SetEnabled(value == RegenMode.Infinite);
                InfiniteGroundWaterSystem.Refresh();
                DailyRegenSystem.SetEnabled(value == RegenMode.DailyRegen);
            }
        }

        private bool IsNotInfiniteMode => RegenMode != RegenMode.Infinite;
        private bool IsNotDailyRegenMode => RegenMode != RegenMode.DailyRegen;

        #endregion

        #region Daily Regen

        [SettingsUISection(kMainSection, kDailyRegenGroup)]
        [SettingsUISlider(min = 0f, max = 100f, step = 1f, unit = "percentage")]
        [SettingsUIHideByCondition(typeof(ELTSettings), nameof(IsNotDailyRegenMode))]
        public int OreRegenPercent { get; set; } = 0;

        [SettingsUISection(kMainSection, kDailyRegenGroup)]
        [SettingsUISlider(min = 0f, max = 100f, step = 1f, unit = "percentage")]
        [SettingsUIHideByCondition(typeof(ELTSettings), nameof(IsNotDailyRegenMode))]
        public int OilRegenPercent { get; set; } = 0;

        [SettingsUISection(kMainSection, kDailyRegenGroup)]
        [SettingsUISlider(min = 0f, max = 100f, step = 1f, unit = "percentage")]
        [SettingsUIHideByCondition(typeof(ELTSettings), nameof(IsNotDailyRegenMode))]
        public int FertilityRegenPercent { get; set; } = 0;

        [SettingsUISection(kMainSection, kDailyRegenGroup)]
        [SettingsUISlider(min = 0f, max = 100f, step = 1f, unit = "percentage")]
        [SettingsUIHideByCondition(typeof(ELTSettings), nameof(IsNotDailyRegenMode))]
        public int FishRegenPercent { get; set; } = 0;

        [SettingsUISection(kMainSection, kDailyRegenGroup)]
        [SettingsUISlider(min = 0f, max = 100f, step = 1f, unit = "percentage")]
        [SettingsUIHideByCondition(typeof(ELTSettings), nameof(IsNotDailyRegenMode))]
        public int GroundWaterRegenPercent { get; set; } = 0;

        [SettingsUISection(kMainSection, kDailyRegenGroup)]
        [SettingsUISlider(min = 0f, max = 100f, step = 1f, unit = "percentage")]
        [SettingsUIHideByCondition(typeof(ELTSettings), nameof(IsNotDailyRegenMode))]
        public int GroundWaterPollutionReductionPercent { get; set; } = 0;

        #endregion

        #region Infinite Resource

        [SettingsUISection(kMainSection, kInfiniteResourceGroup)]
        [SettingsUIHideByCondition(typeof(ELTSettings), nameof(IsNotInfiniteMode))]
        public bool InfiniteFertilityResource { get; set; } = true;

        [SettingsUISection(kMainSection, kInfiniteResourceGroup)]
        [SettingsUIHideByCondition(typeof(ELTSettings), nameof(IsNotInfiniteMode))]
        public bool InfiniteOreResource { get; set; } = true;

        [SettingsUISection(kMainSection, kInfiniteResourceGroup)]
        [SettingsUIHideByCondition(typeof(ELTSettings), nameof(IsNotInfiniteMode))]
        public bool InfiniteOilResource { get; set; } = true;

        [SettingsUISection(kMainSection, kInfiniteResourceGroup)]
        [SettingsUIHideByCondition(typeof(ELTSettings), nameof(IsNotInfiniteMode))]
        public bool InfiniteFishResource { get; set; } = true;

        private bool m_InfiniteGroundWater = true;

        [SettingsUISection(kMainSection, kInfiniteResourceGroup)]
        [SettingsUIHideByCondition(typeof(ELTSettings), nameof(IsNotInfiniteMode))]
        public bool InfiniteGroundWater
        {
            get => m_InfiniteGroundWater;
            set
            {
                m_InfiniteGroundWater = value;
                InfiniteGroundWaterSystem.Refresh();
            }
        }

        private bool m_PreventGroundWaterPollution = true;

        [SettingsUISection(kMainSection, kInfiniteResourceGroup)]
        [SettingsUIHideByCondition(typeof(ELTSettings), nameof(IsNotInfiniteMode))]
        public bool PreventGroundWaterPollution
        {
            get => m_PreventGroundWaterPollution;
            set
            {
                m_PreventGroundWaterPollution = value;
                InfiniteGroundWaterSystem.Refresh();
            }
        }

        #endregion

        #region Clear Depleted Resource

        [SettingsUIButton]
        [SettingsUISection(kMainSection, kDepletedResourceGroup)]
        [SettingsUIDisableByCondition(typeof(ELTSettings), nameof(IsFertilityRefillDisabled))]
        public bool ClearDepletedFertilityResource { set { ClearDepletedSystem.RequestClearResource(ResourceFlags.Fertility); } }

        [SettingsUIButton]
        [SettingsUISection(kMainSection, kDepletedResourceGroup)]
        [SettingsUIDisableByCondition(typeof(ELTSettings), nameof(IsOreRefillDisabled))]
        public bool ClearDepletedOreResource { set { ClearDepletedSystem.RequestClearResource(ResourceFlags.Ore); } }

        [SettingsUIButton]
        [SettingsUISection(kMainSection, kDepletedResourceGroup)]
        [SettingsUIDisableByCondition(typeof(ELTSettings), nameof(IsOilRefillDisabled))]
        public bool ClearDepletedOilResource { set { ClearDepletedSystem.RequestClearResource(ResourceFlags.Oil); } }

        [SettingsUIButton]
        [SettingsUISection(kMainSection, kDepletedResourceGroup)]
        [SettingsUIDisableByCondition(typeof(ELTSettings), nameof(IsFishRefillDisabled))]
        public bool ClearDepletedFishResource { set { ClearDepletedSystem.RequestClearResource(ResourceFlags.Fish); } }

        [SettingsUIButton]
        [SettingsUISection(kMainSection, kDepletedResourceGroup)]
        [SettingsUIDisableByCondition(typeof(ELTSettings), nameof(IsGroundWaterRefillDisabled))]
        public bool ClearDepletedGroundWater { set { ClearDepletedSystem.RequestClearGroundWater(GroundWaterFlags.Amount); } }

        [SettingsUIButton]
        [SettingsUISection(kMainSection, kDepletedResourceGroup)]
        [SettingsUIDisableByCondition(typeof(ELTSettings), nameof(IsGroundWaterPollutionCleanDisabled))]
        public bool CleanGroundWaterPollution { set { ClearDepletedSystem.RequestClearGroundWater(GroundWaterFlags.Pollution); } }

        // Refilling is redundant while the resource is already kept infinite, either by this mod
        // (Infinite mode + per-resource toggle) or by the currently active gamemode's own resource
        // regen (no shipped gamemode reaches 100% today, but the check stays in case one does).
        public bool IsFertilityRefillDisabled() => (RegenMode == RegenMode.Infinite && InfiniteFertilityResource) || InfiniteResourceSystem.IsGameModeResourceInfinite(ResourceFlags.Fertility);
        public bool IsOreRefillDisabled() => (RegenMode == RegenMode.Infinite && InfiniteOreResource) || InfiniteResourceSystem.IsGameModeResourceInfinite(ResourceFlags.Ore);
        public bool IsOilRefillDisabled() => (RegenMode == RegenMode.Infinite && InfiniteOilResource) || InfiniteResourceSystem.IsGameModeResourceInfinite(ResourceFlags.Oil);
        // No gamemode equivalent exists for fish or groundwater.
        public bool IsFishRefillDisabled() => RegenMode == RegenMode.Infinite && InfiniteFishResource;
        public bool IsGroundWaterRefillDisabled() => RegenMode == RegenMode.Infinite && InfiniteGroundWater;
        public bool IsGroundWaterPollutionCleanDisabled() => RegenMode == RegenMode.Infinite && PreventGroundWaterPollution;

        #endregion

        public override void SetDefaults()
        {
            RegenMode = RegenMode.Disabled;
            InfiniteOreResource = true;
            InfiniteOilResource = true;
            InfiniteFertilityResource = true;
            InfiniteFishResource = true;
            InfiniteGroundWater = true;
            PreventGroundWaterPollution = true;
        }
    }
}
