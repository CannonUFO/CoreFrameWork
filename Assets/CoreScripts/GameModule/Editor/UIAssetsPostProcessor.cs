//using ible.Foundation.Utility;
//using System;
//using UnityEditor;
//using UnityEditor.AddressableAssets;
//using UnityEditor.AddressableAssets.Settings;

//namespace ible.GameModule.Editor
//{
//    public class UIAssetsPostProcessor : AssetPostprocessor
//    {
//        private static readonly string s_uiAssetPath = "Assets/MainAssets/ArtSource/UI";
//        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
//        {
//            bool isRefresh = false;

//            for(int i = 0; i < importedAssets.Length; i++) 
//            {
//                if (importedAssets[i].StartsWith(s_uiAssetPath))
//                    isRefresh = true;


//                if (isRefresh)
//                    RefreshUIAsset();
//            }
//        }

//        private static void RefreshUIAsset()
//        {
//            if (!AddressableAssetSettingsDefaultObject.SettingsExists)
//            {
//                Debug.LogError("AddressableAssetSettingsDefaultObject is not exist");
//                return;
//            }

//            var defaultSetting = AddressableAssetSettingsDefaultObject.Settings;
//            if(defaultSetting == null || defaultSetting.groups == null)
//            {
//                defaultSetting = AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>(AddressableAssetSettingsDefaultObject.DefaultAssetPath);
//                if (defaultSetting == null || defaultSetting.groups == null)
//                {
//                    Debug.LogError("AddressableAssetSettings Groups is not exist");
//                    return;
//                }
//            }
//            RefreshUIPrefabs(defaultSetting, "BundleAssets_UI");

//            AssetDatabase.SaveAssets();
//        }

//        private static void RefreshUIPrefabs(AddressableAssetSettings defaultSetting, string bundleName)
//        {
            
//        }
//    }
//}

