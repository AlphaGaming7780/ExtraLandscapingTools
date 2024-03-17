using System.Collections;
using System.Collections.Generic;
using System.IO;
using Colossal.PSI.Common;
using Extra.Lib;
using Extra.Lib.Debugger;
using Game.Prefabs;
using UnityEngine;

namespace ExtraLandscapingTools;

public class CustomBrushes
{
	internal static readonly List<string> folderToLoadCustomBrushes = [];
	private static bool brushesLoaded = false;

	internal static IEnumerator LoadCustomBrushes() {

		if(brushesLoaded || folderToLoadCustomBrushes.Count <= 0) yield break;

		brushesLoaded = true;

		int numberOfBrushes = 0;
		int curentIndex = 0;

		foreach(string folder in folderToLoadCustomBrushes) {;
			numberOfBrushes += Directory.GetFiles(folder).Length;
		}

		var notificationInfo = ExtraLib.m_NotificationUISystem.AddOrUpdateNotification(
			$"{nameof(ExtraLandscapingTools)}.{nameof(ELT)}.{nameof(CustomBrushes)}", 
			title: "ExtraLandscapingTools, Loading the custom brushes.",
			progressState: ProgressState.Indeterminate, 
			progress: 0
		);

		yield return null;

		foreach(string folder in folderToLoadCustomBrushes) {;
			foreach(string filePath in Directory.GetFiles(folder)) 
			{
				string name = Path.GetFileNameWithoutExtension(filePath);
				notificationInfo.progressState = ProgressState.Progressing;
				notificationInfo.progress = (int)(curentIndex / (float)numberOfBrushes*100);
				notificationInfo.text = name;
				yield return null;

				byte[] fileData = File.ReadAllBytes(filePath);
				Texture2D texture2D = new(1, 1);
				if(!texture2D.LoadImage(fileData)) ELT.log.Warn($"Filded to load {name}.");

				BrushPrefab brushPrefab = (BrushPrefab)ScriptableObject.CreateInstance("BrushPrefab");
				brushPrefab.name = name;
				brushPrefab.m_Texture = texture2D;
				ExtraLib.m_PrefabSystem.AddPrefab(brushPrefab);
				curentIndex++;
			}
		}

		ExtraLib.m_NotificationUISystem.RemoveNotification(
			identifier: notificationInfo.id, 
			delay: 3f, 
			text: $"Done loaded {curentIndex}.",
			progressState: ProgressState.Complete, 
			progress: 100
		);
	}
}