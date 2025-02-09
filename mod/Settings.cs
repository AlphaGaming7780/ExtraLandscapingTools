using Colossal.IO.AssetDatabase;
using ExtraLandscapingTools.Systems;
using Game.Modding;
using Game.Settings;

namespace ExtraLandscapingTools
{
    //[FileLocation($"ModsSettings\\{nameof(ExtraLandscapingTools)}\\settings")]
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

        public override void SetDefaults()
        {
            
        }
    }
}
