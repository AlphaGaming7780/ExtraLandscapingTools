using Colossal.Logging;
using Game;
using Game.Modding;
using Game.SceneFlow;
using ExtraLib;
using Unity.Entities;
using Unity.Collections;
using Game.Prefabs;
using System;
using ExtraLib.Helper;

namespace ExtraLandscapingTools
{
	public class ExtraLandscapingTools : IMod
	{
		public static ILog log = LogManager.GetLogger($"{nameof(ExtraLandscapingTools)}").SetShowsErrorsInUI(false);

		public void OnLoad(UpdateSystem updateSystem)
		{
			log.Info(nameof(OnLoad));

			if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
				log.Info($"Current mod asset at {asset.path}");

			EntityQueryDesc entityQueryDesc = new()
			{
				All = [ComponentType.ReadOnly<TerraformingData>()]
			};

			ExtraLib.ExtraLib.AddOnEditEnity(new(OnEditEntities, entityQueryDesc));

		}

		public void OnDispose()
		{
			log.Info(nameof(OnDispose));
		}

		public void OnEditEntities(NativeArray<Entity> entities)
		{   
			foreach(Entity entity in entities) {
				if(ExtraLib.ExtraLib.m_PrefabSystem.TryGetPrefab<TerraformingPrefab>(entity, out TerraformingPrefab prefab)) {
					var TerraformingUI = prefab.GetComponent<UIObject>();
					if (TerraformingUI == null)
					{
						TerraformingUI = prefab.AddComponent<UIObject>();
						TerraformingUI.active = true;
						TerraformingUI.m_IsDebugObject = false;
						TerraformingUI.m_Icon = "Media/Game/Icons/LotTool.svg";
						TerraformingUI.m_Priority = 1;
					}
					TerraformingUI.m_Group = PrefabsHelper.GetExistingToolCategory(prefab, "Terraforming") ?? TerraformingUI.m_Group;
				}
			}
		}

	}
}
