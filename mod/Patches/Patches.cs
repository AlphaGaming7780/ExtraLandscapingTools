// using Extra.Lib;
// using Game.Tools;
// using Game.UI.InGame;
// using HarmonyLib;

// namespace ExtraLandscapingTools.Patches;

// [HarmonyPatch( typeof( ToolUISystem ), "OnToolChanged", typeof(ToolBaseSystem) )]
// class ToolUISystem_OnToolChanged
// {
//     internal static bool showMarker = false;
//     private static void Postfix( ToolBaseSystem tool ) {

//         if(tool is TerrainToolSystem) {
//             ExtraLibUI.RunUIScript(ELT_UI.GetStringFromEmbbededJSFile("UI.js"));
//         } else {
//             ExtraLibUI.RunUIScript(ELT_UI.GetStringFromEmbbededJSFile("REMOVE_UI.js"));
//         }
//     }
// }