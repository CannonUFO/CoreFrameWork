
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ible.Foundation.Editor
{
    public class SceneAssetPostProcessor : AssetPostprocessor
    {
        private static readonly string ScenePath = "Assets/MainAssets/Scenes";
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            bool isRefresh = false;

            for(int i = 0; i < importedAssets.Length; i++)
            {
                if (importedAssets[i].StartsWith(ScenePath))
                {
                    isRefresh = true;
                    break;
                }
            }
            for (int i = 0; i < deletedAssets.Length; i++)
            {
                if (deletedAssets[i].StartsWith(ScenePath))
                {
                    isRefresh = true;
                    break;
                }
            }

            if(isRefresh) 
            {
                var files = Directory.GetFiles(ScenePath, "*.unity");
                var scenes = new EditorBuildSettingsScene[files.Length];
                int sceneIndex = 1;
                for (int i = 0; i < files.Length; i++)
                {

                    var filePath = files[i].Replace("\\", "/");
                    GUID guiId = new(AssetDatabase.AssetPathToGUID(filePath));
                    if(filePath.Contains("InitialScene"))
                        scenes[0] = new EditorBuildSettingsScene(guiId, true);
                    else
                        scenes[sceneIndex++] = new EditorBuildSettingsScene(guiId, true);
                }
                EditorBuildSettings.scenes = scenes;
            }
        }
    }
}
