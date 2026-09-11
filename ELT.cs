using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using Colossal.PSI.Environment;
using ExtraLandscapingTools.Systems;
using ExtraLib;
using ExtraLib.ClassExtension;
using ExtraLib.Helpers;
using Game;
using Game.Modding;
using Game.Prefabs;
using Game.SceneFlow;
using Game.Simulation;
using System.IO;
using System.Reflection;
using Unity.Collections;
using Unity.Entities;
using Logger = ExtraLib.Debugger.Logger;


namespace ExtraLandscapingTools
{
    public class ELT : IMod
	{
        internal static ELTSettings s_setting;
		internal static ILog log = LogManager.GetLogger($"{nameof(ExtraLandscapingTools)}").SetShowsErrorsInUI(false);
#if DEBUG
        internal static Logger Logger = new(log, true);
#else
		internal static Logger Logger = new(log, false);
#endif

        public void OnLoad(UpdateSystem updateSystem)
		{
            Logger.Info(nameof(OnLoad));

			if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
			{
				Logger.Info($"Current mod asset at {asset.path}");
				FileInfo fileInfo = new(asset.path);
				string path = Path.Combine(fileInfo.Directory.FullName, "CustomBrushes");
				if(Directory.Exists(path))
                    CustomBrushes.folderToLoadCustomBrushes.Add(path);

				string pathToDataBrushes = Path.Combine(EnvPath.kUserDataPath, "ModsData", nameof(ExtraLandscapingTools), "CustomBrushes");
				if (Directory.Exists(pathToDataBrushes)) CustomBrushes.folderToLoadCustomBrushes.Add(pathToDataBrushes);

			}
			else Logger.Error("Failed to get the ExecutableAsset.");

            s_setting = new ELTSettings(this);
            s_setting.RegisterKeyBindings();
            s_setting.RegisterInOptionsUI();
            AssetDatabase.global.LoadSettings("ELTSettings", s_setting, new ELTSettings(this));

            // AreaLotSimulationSystem is registered after GroundWaterPollutionSystem in
            // SystemOrder.cs, so depending on it alone still places ClearDepletedSystem after both. (I hope)
            updateSystem.UpdateAfter<ClearDepletedSystem, AreaLotSimulationSystem>(SystemUpdatePhase.GameSimulation);
            updateSystem.UpdateAfter<InfiniteResourceSystem, AreaLotSimulationSystem>(SystemUpdatePhase.GameSimulation);
            updateSystem.UpdateAfter<InfiniteGroundWaterSystem, GroundWaterPollutionSystem>(SystemUpdatePhase.GameSimulation);

            updateSystem.UpdateAfter<DailyRegenSystem, GameModeNaturalResourcesAdjustSystem>(SystemUpdatePhase.GameSimulation);

            EntityQueryDesc entityQueryDesc = new()
			{
				All = new[] { ComponentType.ReadOnly<TerraformingData>() }
			};

			ExtraLocalization.LoadLocalization(Logger, Assembly.GetExecutingAssembly(), false);
			EL.AddOnEditEnities(new(OnEditEntities, entityQueryDesc));

            EL.AddOnInitialize(Initialize);
		}

		public void OnDispose()
		{

		}

		internal static Stream GetEmbedded(string embeddedPath) {
			return Assembly.GetExecutingAssembly().GetManifestResourceStream("ExtraLandscapingTools.embedded."+embeddedPath);
		}

		private void Initialize()
		{
            GameManager.instance.StartCoroutine(CustomBrushes.LoadCustomBrushes());
        }

		private void OnEditEntities(NativeArray<Entity> entities)
		{   
			ELT.Logger.Info($"OnEditEntities called with {entities.Length} entities.");
            foreach (Entity entity in entities) 
			{
				if(EL.m_PrefabSystem.TryGetPrefab(entity, out TerraformingPrefab prefab)) {

					if(prefab.m_Target == TerraformingTarget.Material) continue;

					ELT.Logger.Info($"Entity {EL.m_PrefabSystem.GetPrefabName(entity)} ({entity}) is a TerraformingPrefab, adding TerraformingData.");

					var terraformingUI = prefab.GetComponent<UIObject>();

                    //They should already have a UIObject, but just in case, we add one if it doesn't exist.
                    if (terraformingUI == null)
					{
						terraformingUI = prefab.AddComponent<UIObject>();
						terraformingUI.active = true;
						terraformingUI.m_IsDebugObject = false;
						terraformingUI.m_Icon = "Media/Game/Icons/LotTool.svg";
						terraformingUI.m_Priority = 110;
					} 
					else if(prefab.m_Target != TerraformingTarget.Height && prefab.m_Target != TerraformingTarget.Material)
					{
						terraformingUI.m_Priority += 50;
                    }

                    terraformingUI.m_Group?.RemoveElement(entity);
					terraformingUI.m_Group = PrefabsHelper.GetUIAssetCategoryPrefab("Terraforming");
					terraformingUI.m_Group.AddElement(entity);

                    EL.m_EntityManager.AddOrSetComponentData(entity, terraformingUI.ToComponentData());
				} else
				{
					ELT.Logger.Warn($"Entity {EL.m_PrefabSystem.GetPrefabName(entity)} ({entity}) is not a TerraformingPrefab, but has TerraformingData.");
                }
			}
		}
    }
}
