//using System;
//using UnityEditor;
//using UnityEngine;
//using UnityEngine.SceneManagement;
//using ible.GameModule.UI.Config;

//using UnityEditor.Experimental.SceneManagement;

//namespace ible.Foundation.UI.AutoGen.Editor
//{
//    public class VariantWindow : EditorWindow
//    {
//        public BaseUIConfig Target;

//        private string _curSceneName;
//        private Vector2 _scrollPosition;

//        [MenuItem("Window/UI/Variant Window")]
//        public static void ShowWindow()
//        {
//            EditorWindow.GetWindow<VariantWindow>("Variant Window");
//        }

//        private bool IsPrefabStage(ref string sceneName)
//        {
//            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
//            if (prefabStage != null)
//            {
//                Type type = UINameAttribute.GetTargetType("Editor.Config." + prefabStage.scene.name);
//                if (type != null)
//                    return true;
//            }

//            return false;
//        }

//        private void OnGUI()
//        {
//            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
//            if (prefabStage != null && Target == null)
//            {
//                string sceneName = prefabStage.scene.name;
//                if (sceneName.StartsWith("Canvas - "))
//                {
//                    sceneName = sceneName.Replace("Canvas - ", "");
//                }

//                if (_curSceneName != sceneName)
//                {
//                    _curSceneName = sceneName;

//                    Type type = UINameAttribute.GetTargetType("Editor.Config." + sceneName);
//                    if (type != null)
//                    {
//                        var config = prefabStage.prefabContentsRoot.GetComponent(type);
//                        if (config == null)
//                        {
//                            prefabStage.prefabContentsRoot.AddComponent(type);
//                        }
//                    }
//                }

//                Target = prefabStage.prefabContentsRoot.GetComponent<BaseUIConfig>();
//            }

//            if (Target != null && prefabStage == null)
//            {
//                Target = null;
//            }

//            if (Target != null)
//            {
//                if (GUILayout.Button("Refresh"))
//                {
//                    Target.RefreshProperty();
//                }

//                EditorGUILayout.Space(10);

//                Target.OnInspectorPropertyGUI(true, ref _scrollPosition);
//            }
//        }
//    }
//}