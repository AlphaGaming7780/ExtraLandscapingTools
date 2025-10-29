using Colossal.IO.AssetDatabase;
using ExtraLandscapingTools.Systems;
using Game.Modding;
using Game.Settings;

namespace ExtraLandscapingTools
{
    //[FileLocation($"ModsSettings\\{nameof(ExtraLandscapingTools)}\\settings")]
    [FileLocation("ExtraLandscapingTools")]
    internal class ELTSettings : ModSetting
    {
        public ELTSettings(IMod mod) : base(mod) { }

        public const string kMainSection = "Main";
        public const string kDepletedResourceGroup = "DepletedResource";

        [SettingsUIButton]
        [SettingsUISection(kMainSection, kDepletedResourceGroup)]
        public bool ClearDepletedFertilityResource { set { MainSystem.s_clearUsedFertilityResource = true; } }

        [SettingsUIButton]
        [SettingsUISection(kMainSection, kDepletedResourceGroup)]
        public bool ClearDepletedOreResource { set { MainSystem.s_clearUsedOreResource = true; } }

        [SettingsUIButton]
        [SettingsUISection(kMainSection, kDepletedResourceGroup)]
        public bool ClearDepletedOilResource { set { MainSystem.s_clearUsedOilResource = true; } }

        [SettingsUIButton]
        [SettingsUISection(kMainSection, kDepletedResourceGroup)]
        public bool ClearDepletedFishResource { set { MainSystem.s_clearUsedFishResource = true; } }

        public override void SetDefaults()
        {
            
        }
    }
}
