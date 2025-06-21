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
using HarmonyLib;
using System.IO;
using System.Linq;
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
        private Harmony harmony;
		public void OnLoad(UpdateSystem updateSystem)
		{
            Logger.Info(nameof(OnLoad));

			if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
			{
				Logger.Info($"Current mod asset at {asset.path}");
				FileInfo fileInfo = new(asset.path);
				CustomBrushes.folderToLoadCustomBrushes.Add($"{fileInfo.Directory.FullName}\\CustomBrushes");

				string pathToDataBrushes = Path.Combine(EnvPath.kUserDataPath, "ModsData", nameof(ExtraLandscapingTools), "CustomBrushes");
				if (Directory.Exists(pathToDataBrushes)) CustomBrushes.folderToLoadCustomBrushes.Add(pathToDataBrushes);

			}
			else Logger.Error("Failed to get the ExecutableAsset.");

            s_setting = new ELTSettings(this);
            s_setting.RegisterKeyBindings();
            s_setting.RegisterInOptionsUI();
            AssetDatabase.global.LoadSettings("ELTSettings", s_setting, new ELTSettings(this));

            updateSystem.UpdateAt<MainSystem>(SystemUpdatePhase.LateUpdate);

            EntityQueryDesc entityQueryDesc = new()
			{
				All = new[] { ComponentType.ReadOnly<TerraformingData>() }
			};

			ExtraLocalization.LoadLocalization(Logger, Assembly.GetExecutingAssembly(), false);
			EL.AddOnEditEnities(new(OnEditEntities, entityQueryDesc));

            EL.AddOnInitialize(Initialize);

			harmony = new($"{nameof(ExtraLandscapingTools)}.{nameof(ELT)}");
			harmony.PatchAll(typeof(ELT).Assembly);
			var patchedMethods = harmony.GetPatchedMethods().ToArray();
			Logger.Info($"Plugin ExtraLandscapingTools made patches! Patched methods: " + patchedMethods.Length);
			foreach (var patchedMethod in patchedMethods)
			{
				Logger.Info($"Patched method: {patchedMethod.Module.Name}:{patchedMethod.Name}");
			}
		}

		public void OnDispose()
		{
            Logger.Info(nameof(OnDispose));
			harmony.UnpatchAll($"{nameof(ExtraLandscapingTools)}.{nameof(ELT)}");
		}

		internal static Stream GetEmbedded(string embeddedPath) {
			return Assembly.GetExecutingAssembly().GetManifestResourceStream("ExtraLandscapingTools.embedded."+embeddedPath);
		}

		private void Initialize()
		{
            EL.extraLibMonoScript.StartCoroutine(CustomBrushes.LoadCustomBrushes());
        }

		private void OnEditEntities(NativeArray<Entity> entities)
		{   
			ELT.Logger.Info($"OnEditEntities called with {entities.Length} entities.");
            foreach (Entity entity in entities) {
				if(EL.m_PrefabSystem.TryGetPrefab(entity, out TerraformingPrefab prefab)) {

					if(prefab.m_Target == TerraformingTarget.Material) continue;

					ELT.Logger.Info($"Entity {EL.m_PrefabSystem.GetPrefabName(entity)} ({entity}) is a TerraformingPrefab, adding TerraformingData.");

                    var TerraformingUI = prefab.GetComponent<UIObject>();
					if (TerraformingUI == null)
					{
						TerraformingUI = prefab.AddComponent<UIObject>();
						TerraformingUI.active = true;
						TerraformingUI.m_IsDebugObject = false;
						TerraformingUI.m_Icon = "Media/Game/Icons/LotTool.svg";
						TerraformingUI.m_Priority = 1;
					}

					TerraformingUI.m_Group?.RemoveElement(entity);
					TerraformingUI.m_Group = PrefabsHelper.GetUIAssetCategoryPrefab("Terraforming");
					TerraformingUI.m_Group.AddElement(entity);

                    EL.m_EntityManager.AddOrSetComponentData(entity, TerraformingUI.ToComponentData());
				} else
				{
					ELT.Logger.Warn($"Entity {EL.m_PrefabSystem.GetPrefabName(entity)} ({entity}) is not a TerraformingPrefab, but has TerraformingData.");
                }
			}
		}
    }
}
