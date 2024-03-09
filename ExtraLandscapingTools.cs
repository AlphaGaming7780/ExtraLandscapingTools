using Colossal.Logging;
using Game;
using Game.Modding;
using Game.SceneFlow;
using ExtraLib;
using Unity.Entities;
using Unity.Collections;
using Game.Prefabs;

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

            EntityQueryDesc entityQueryDesc = new();
            entityQueryDesc.All = [ComponentType.ReadOnly<TerraformingData>()];

            ExtraLib.ExtraLib.AddOnEditEnity(new(OnEditEntities))

        }

        public void OnDispose()
        {
            log.Info(nameof(OnDispose));
        }

        private void OnEditEntities(Unity.Collections.NativeArray<Entity> entities)
        {

        }

    }
}
