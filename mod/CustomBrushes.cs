using System.Collections;
using System.Collections.Generic;
using System.IO;
using Colossal.PSI.Common;
using ExtraLib;
using ExtraLib.Helpers;
using Game.Prefabs;
using UnityEngine;

namespace ExtraLandscapingTools
{
    public class CustomBrushes
    {
        internal static readonly List<string> folderToLoadCustomBrushes = new();
        private static bool brushesLoaded = false;

        internal static IEnumerator LoadCustomBrushes()
        {

            if (brushesLoaded || folderToLoadCustomBrushes.Count <= 0) yield break;

            brushesLoaded = true;

            int numberOfBrushes = 0;
            int currentIndex = 0;
            int loadedCount = 0;

            foreach (string folder in folderToLoadCustomBrushes)
            {
                if (!Directory.Exists(folder)) continue;
                numberOfBrushes += Directory.GetFiles(folder).Length;
            }

            var notificationInfo = EL.m_NotificationUISystem.AddOrUpdateNotification(
                $"{nameof(ExtraLandscapingTools)}.{nameof(ELT)}.{nameof(CustomBrushes)}",
                title: "ELT, Loading the custom brushes.",
                progressState: ProgressState.Indeterminate,
                progress: 0
            );

            yield return null;

            foreach (string folder in folderToLoadCustomBrushes)
            {
                foreach (string filePath in Directory.GetFiles(folder))
                {
                    string name = Path.GetFileNameWithoutExtension(filePath);
                    notificationInfo.progressState = ProgressState.Progressing;
                    notificationInfo.progress = (int)(currentIndex / (float)numberOfBrushes * 100);
                    notificationInfo.text = name;
                    yield return null;

                    currentIndex++;

                    byte[] fileData = File.ReadAllBytes(filePath);
                    Texture2D texture2D = new(1, 1);
                    if (!texture2D.LoadImage(fileData))
                    {
                        ELT.Logger.Warn($"Failed to load {name}.");
                        UnityEngine.Object.Destroy(texture2D);
                        continue;
                    }

                    TextureHelper.Format(ref texture2D, TextureFormat.Alpha8);
                    texture2D.wrapMode = TextureWrapMode.Clamp;

                    BrushPrefab brushPrefab = ScriptableObject.CreateInstance<BrushPrefab>();
                    brushPrefab.name = name;
                    brushPrefab.m_Texture = texture2D;
                    EL.m_PrefabSystem.AddPrefab(brushPrefab);
                    loadedCount++;
                }
            }

            EL.m_NotificationUISystem.RemoveNotification(
                identifier: notificationInfo.id,
                delay: 3f,
                text: $"Done loaded {loadedCount}.",
                progressState: ProgressState.Complete,
                progress: 100
            );
        }
    }
}