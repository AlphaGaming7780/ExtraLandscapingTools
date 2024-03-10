using Colossal.Logging;
using Game;
using Game.Modding;
using Game.SceneFlow;
using Unity.Entities;
using Unity.Collections;
using Game.Prefabs;
using Extra.Lib;
using Extra.Lib.Debugger;
using Extra.Lib.Helper;
using System.Linq;
using System.IO;
using System.Reflection;
using HarmonyLib;
using System.Collections;

namespace ExtraLandscapingTools
{
	public class ELT : IMod
	{
		public static ILog log = LogManager.GetLogger($"{nameof(ExtraLandscapingTools)}").SetShowsErrorsInUI(false);
		private Harmony harmony;
		public void OnLoad(UpdateSystem updateSystem)
		{
			log.Info(nameof(OnLoad));

			if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
				log.Info($"Current mod asset at {asset.path}");

			EntityQueryDesc entityQueryDesc = new()
			{
				All = [ComponentType.ReadOnly<TerraformingData>()]
			};

			ExtraLib.AddOnEditEnities(new(OnEditEntities, entityQueryDesc));

			harmony = new($"{nameof(ExtraLandscapingTools)}.{nameof(ELT)}");
			harmony.PatchAll(typeof(ELT).Assembly);
			var patchedMethods = harmony.GetPatchedMethods().ToArray();
			Print.Info($"Plugin ExtraDetailingTools made patches! Patched methods: " + patchedMethods.Length);
			foreach (var patchedMethod in patchedMethods)
			{
				Print.Info($"Patched method: {patchedMethod.Module.Name}:{patchedMethod.Name}");
			}			

		}

		public void OnDispose()
		{
			log.Info(nameof(OnDispose));
			harmony.UnpatchAll($"{nameof(ExtraLandscapingTools)}.{nameof(ELT)}");
		}

		internal static Stream GetEmbedded(string embeddedPath) {
			return Assembly.GetExecutingAssembly().GetManifestResourceStream("ExtraLandscapingTools.embedded."+embeddedPath);
		}

		public void OnEditEntities(NativeArray<Entity> entities)
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
