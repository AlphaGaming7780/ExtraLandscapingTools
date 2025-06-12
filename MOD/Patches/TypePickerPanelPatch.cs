
#if DEBUG
using Game.Prefabs;
using Game.UI.Editor;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace ExtraLandscapingTools.Patches
{
    class TypePickerPanelPatch
    {
        [HarmonyPatch()]
        class GetAllConcreteTypesDerivedFrom_PrefabBase
        {
            static System.Reflection.MethodBase TargetMethod()
            {
                return typeof(TypePickerPanel).GetMethod("GetAllConcreteTypesDerivedFrom").MakeGenericMethod(typeof(PrefabBase));
            }

            static void Postfix(ref IEnumerable<Type> __result)
            {
                var moreValues = from t in Assembly.GetExecutingAssembly().GetTypes()
                        where !t.IsAbstract && t.IsSubclassOf(typeof(PrefabBase))
                        select t;

                __result = __result.Concat(moreValues);

                return;
            }
        }

        [HarmonyPatch()]
        class GetAllConcreteTypesDerivedFrom_ComponentBase
        {
            static System.Reflection.MethodBase TargetMethod()
            {
                return typeof(TypePickerPanel).GetMethod("GetAllConcreteTypesDerivedFrom").MakeGenericMethod(typeof(ComponentBase));
            }

            static void Postfix(ref IEnumerable<Type> __result)
            {
                var moreValues = from t in Assembly.GetExecutingAssembly().GetTypes()
                                 where !t.IsAbstract && t.IsSubclassOf(typeof(ComponentBase))
                                 select t;

                __result = __result.Concat(moreValues);

                return;
            }
        }
    }
}
#endif
