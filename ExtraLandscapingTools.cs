using Colossal.Logging;
using Game;
using Game.Modding;
using Game.SceneFlow;
using Unity.Entities;
using Unity.Collections;
using Extra.Lib;
using Extra.Lib.Debugger;
using Extra.Lib.Helper;
using System.Linq;
using System.IO;
using System.Reflection;
using Colossal.PSI.Environment;
using UnityEngine;
using Extra.Lib.Localization;
using Game.Prefabs;
using Logger = Extra.Lib.Debugger.Logger;

namespace ExtraLandscapingTools
{
	public class ELT : IMod
	{
		private readonly GameObject ExtraLandscapingToolsGameObject = new();
		internal static ILog log = LogManager.GetLogger($"{nameof(ExtraLandscapingTools)}").SetShowsErrorsInUI(false);
#if DEBUG
        internal static Logger Logger = new(log, true);
#else
		internal static Logger Logger = new(log, false);
#endif
        //private Harmony harmony;
        public void OnLoad(UpdateSystem updateSystem)
		{
            Logger.Info(nameof(OnLoad));

			if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
			{
				Logger.Info($"Current mod asset at {asset.path}");
				FileInfo fileInfo = new(asset.path);
				CustomBrushes.folderToLoadCustomBrushes.Add($"{fileInfo.Directory.FullName}\\Brushes");

				string pathToDataBrushes = Path.Combine(EnvPath.kUserDataPath, "ModsData", nameof(ExtraLandscapingTools), "Brushes");
				if (Directory.Exists(pathToDataBrushes)) CustomBrushes.folderToLoadCustomBrushes.Add(pathToDataBrushes);

			}
			else Logger.Error("Failed to get the ExecutableAsset.");

			EntityQueryDesc entityQueryDesc = new()
			{
				All = [ComponentType.ReadOnly<TerraformingData>()]
			};
			ExtraLocalization.LoadLocalization(Logger, Assembly.GetExecutingAssembly(), false);
			ExtraLib.AddOnEditEnities(new(OnEditEntities, entityQueryDesc));
			ExtraLib.AddOnMainMenu(OnMainMenu);

			//harmony = new($"{nameof(ExtraLandscapingTools)}.{nameof(ELT)}");
			//harmony.PatchAll(typeof(ELT).Assembly);
			//var patchedMethods = harmony.GetPatchedMethods().ToArray();
			//Logger.Info($"Plugin ExtraLandscapingTools made patches! Patched methods: " + patchedMethods.Length);
			//foreach (var patchedMethod in patchedMethods)
			//{
			//	Logger.Info($"Patched method: {patchedMethod.Module.Name}:{patchedMethod.Name}");
			//}
		}

		public void OnDispose()
		{
            Logger.Info(nameof(OnDispose));
			//harmony.UnpatchAll($"{nameof(ExtraLandscapingTools)}.{nameof(ELT)}");
		}

		internal static Stream GetEmbedded(string embeddedPath) {
			return Assembly.GetExecutingAssembly().GetManifestResourceStream("ExtraLandscapingTools.embedded."+embeddedPath);
		}

		private void OnMainMenu() {
			ExtraLib.extraLibMonoScript.StartCoroutine(CustomBrushes.LoadCustomBrushes());
		}

		private void OnEditEntities(NativeArray<Entity> entities)
		{   
			string[] removeTools = ["Material 1", "Material 2"];
			
			foreach(Entity entity in entities) {
				if(ExtraLib.m_PrefabSystem.TryGetPrefab(entity, out TerraformingPrefab prefab)) {

					if(removeTools.Contains(prefab.name)) continue;

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
					
					ExtraLib.m_EntityManager.AddOrSetComponentData(entity, TerraformingUI.ToComponentData());
				}
			}
		}
    }
}
