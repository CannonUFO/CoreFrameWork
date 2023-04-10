using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using ible.Foundation.UI;
using ible.Foundation;

namespace ible.Foundation.UI.AutoGen.Editor
{
    [CustomEditor(typeof(AutoUIGenerator))]
    public class AutoUIGeneratorEditor : UnityEditor.Editor
    {
        protected const string s_menuRoot = "Tools/UI/AutoUI/";

        private AutoUIGenerator _target = null;
        private Type _targetType = null;
        protected bool isRoot = false;

        private SerializedProperty _uiReferences;

        public void OnEnable()
        {
            _uiReferences = serializedObject.FindProperty("_references");
        }

        private static List<string> s_namespaceList = new List<string>();

        static AutoUIGeneratorEditor()
        {
            s_namespaceList = Enum.GetNames(typeof(UIName)).ToList();
            s_namespaceList.Sort();
        }

        [MenuItem(s_menuRoot + "Generate UI Code (Current Scene)")]
        public static void GenerateCodeCurrentScene()
        {
            AutoUIGenerator[] generators = GameObject.FindObjectsOfType<AutoUIGenerator>();
            foreach (AutoUIGenerator generator in generators)
            {
                GenerateUINode.GenerateUICode(generator, generator.transform.parent == null);
            }

            AssetDatabase.Refresh();
        }

        [MenuItem(s_menuRoot + "Add Component Link Reference (Current Scene)")]
        public static void AddUIComponentLinkReferenceCurrentScene()
        {
            AutoUIGenerator[] generators = GameObject.FindObjectsOfType<AutoUIGenerator>();
            AutoUIGenerator.AddUIComponentLinkReference(generators);
        }

        [MenuItem(s_menuRoot + "Generate UI Code (Select Object)")]
        public static void GenerateCodeSelectObject()
        {
            AutoUIGenerator[] generators = Selection.activeGameObject.GetComponentsInChildren<AutoUIGenerator>(true);
            foreach (AutoUIGenerator generator in generators)
            {
                GenerateUINode.GenerateUICode(generator, generator.transform.parent == null);
            }

            AssetDatabase.Refresh();
        }

        [MenuItem(s_menuRoot + "Add Component Link Reference (Select Object)")]
        public static void AddUIComponentLinkReferenceSelectObject()
        {
            AutoUIGenerator[] generators = Selection.activeGameObject.GetComponentsInChildren<AutoUIGenerator>(true);
            AutoUIGenerator.AddUIComponentLinkReference(generators);
        }

        private static T EditorCopyComponent<T>(T original, T copyTarget) where T : Component
        {
            ComponentUtility.CopyComponent(original);
            ComponentUtility.PasteComponentValues(copyTarget);
            return copyTarget;
        }

        private static string GetTransformPath(Transform root, Transform tf, bool includeRoot = true)
        {
            if (tf.parent == null || tf == root)
                return includeRoot ? tf.name : string.Empty;

            string path = "/" + tf.name;
            while (tf.parent != null)
            {
                tf = tf.parent;
                if (!includeRoot && tf.parent == null)
                    break;
                if (tf == root)
                    break;
                path = "/" + tf.name + path;
            }
            return path.Substring(1, path.Length - 1);
        }

        public override void OnInspectorGUI()
        {
            _target = (AutoUIGenerator)target;
            _targetType = UINameAttribute.GetTargetType(_target.UITypeName);

            EditorGUI.BeginChangeCheck();

            string canvasName = _target.UIName;
            UICanvas uiCanvas = _target.transform.root.GetComponent<UICanvas>();
            if (uiCanvas != null)
            {
                canvasName = uiCanvas.CanvasName;
            }

            isRoot = _target.IsRootUI();

            bool enable = GUI.enabled;
            GUI.enabled = !isRoot;
            _target.UIName = EditorGUILayout.TextField("UI Name", _target.UIName);
            GUI.enabled = enable;

            if (_targetType != null)
            {
                enable = GUI.enabled;
                GUI.enabled = false;
                EditorGUILayout.TextField("Namespace", _target.Namespace);
                GUI.enabled = enable;
            }
            else
            {
                if (isRoot)
                {
                    _target.Namespace = canvasName;
                }
                else
                {
                    string[] namespaceOptions;
                    string[] namespaceValues;
                    if (uiCanvas == null)
                    {
                        string[] array = s_namespaceList.ToArray();
                        namespaceOptions = new string[array.Length + 1];
                        namespaceValues = new string[array.Length + 1];
                        namespaceOptions[0] = AutoUIGenerator.s_commonNamespace;
                        namespaceValues[0] = AutoUIGenerator.s_commonNamespace;
                        for (int i = 0; i < array.Length; i++)
                        {
                            namespaceValues[i + 1] = array[i];
                            var token = array[i];
                            for(int j=1; j < token.Length; j++)
                            {
                                if(Char.IsUpper(token, j))
                                {
                                    token = token.Substring(0, j);
                                    if (string.IsNullOrEmpty(token))
                                    {
                                        token = array[i].Substring(0, 1).ToUpper();
                                    }
                                }
                            }
                            namespaceOptions[i + 1] = $"{token[0]}/{token}/{array[i]}";
                        }
                    }
                    else
                    {
                        namespaceOptions = new string[] { canvasName, AutoUIGenerator.s_commonNamespace };
                        namespaceValues = namespaceOptions;
                    }

                    int curIndex = Array.IndexOf(namespaceValues, _target.Namespace);
                    if (curIndex == -1)
                    {
                        curIndex = 0;
                        _target.Namespace = namespaceValues[0];
                    }

                    int selectIndex = EditorGUILayout.Popup("Namespace", curIndex, namespaceOptions);
                    if (selectIndex != curIndex)
                    {
                        _target.Namespace = namespaceValues[selectIndex];
                    }
                }
            }

            _target.IsCombineArray = EditorGUILayout.Toggle("IsCombineArray", _target.IsCombineArray);

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(_target);
            }

            if (_uiReferences != null)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PropertyField(_uiReferences);
                EditorGUI.EndDisabledGroup();
            }

            OnInspectorGUIButton();
        }

        protected virtual void OnInspectorGUIButton()
        {
            if (GUILayout.Button("Generate Code"))
            {
                GenerateUINode.GenerateUICode(_target, isRoot);

                //if(_target.GetComponent<UICanvas>() != null)
                //{
                //    GenerateVariantConfig.GenerateConfigCode(_target.gameObject);
                //}

                AssetDatabase.Refresh();
            }

            if (GUILayout.Button("Collect References"))
            {
                _target?.CollectUIReference();
                EditorUtility.SetDirty(_target);
            }

            if (_targetType != null)
            {
                EditorGUI.BeginChangeCheck();

                if (_target.gameObject.GetComponent(_targetType) == null)
                {
                    if (GUILayout.Button("Add UI Component"))
                    {
                        _target.gameObject.AddComponent(_targetType);
                        AutoUIGenerator.LinkReference(_target);
                    }
                }
                else
                {
                    if (GUILayout.Button("Link Reference"))
                    {
                        AutoUIGenerator.LinkReference(_target);
                    }
                }

                GUILayout.Space(20);

                if (GUILayout.Button("Delete Code"))
                {
                    if (EditorUtility.DisplayDialog("Alert!!!", "Confirm Delete Code?", "Confirm", "Cancel"))
                    {
                        var component = _target.gameObject.GetComponent(_targetType);
                        if (component != null)
                        {
                            DestroyImmediate(component, true);
                        }

                        GenerateUINode.DeleteUICode(_target.Namespace, _target.UIName);

                        GenerateVariantConfig.DeleteUICode(_target.UIName);

                        AssetDatabase.Refresh();
                    }
                }

                GUILayout.Space(20);

                if (GUILayout.Button("Copy To Duplicate"))
                {
                    CopyToDuplicate();
                }

                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(_target);
                }
            }
        }

        private void CopyToDuplicate()
        {
            List<AutoUIReference> referenceList = new List<AutoUIReference>();
            AutoUIGenerator.GetAutoUIReferences(_target.transform, ref referenceList);

            var targetComponet = _target.gameObject.GetComponent(_targetType);

            AutoUIReference[] roots = _target.gameObject.GetComponents<AutoUIReference>();
            AutoUIReference rootReference = roots.FirstOrDefault(x => x.TypeFullName == _targetType.FullName);

            int correctCount = 0;
            int errorCount = 0;

            Transform parent = _target.transform.parent;
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform tf = parent.GetChild(i);
                if (tf != _target.transform)
                {
                    bool isCorrect = true;
                    Dictionary<Transform, List<AutoUIReference>> tempDict = new Dictionary<Transform, List<AutoUIReference>>();
                    foreach (var reference in referenceList)
                    {
                        string path = GetTransformPath(_target.transform, reference.transform, false);

                        Transform editTarget = string.IsNullOrEmpty(path) ? tf : tf.Find(path);
                        if (editTarget == null)
                        {
                            isCorrect = false;
                            break;
                        }

                        if (!tempDict.ContainsKey(editTarget))
                            tempDict.Add(editTarget, new List<AutoUIReference>());
                        tempDict[editTarget].Add(reference);
                    }

                    if (isCorrect)
                    {
                        foreach (var iter in tempDict)
                        {
                            foreach (var reference in iter.Value)
                            {
                                CheckAndCopyAutoUIReference(iter.Key, reference);
                            }
                        }

                        if (targetComponet != null)
                        {
                            var uiComponet = tf.gameObject.GetComponent(_targetType);
                            if (uiComponet == null)
                            {
                                uiComponet = tf.gameObject.AddComponent(_targetType);
                                EditorCopyComponent(targetComponet, uiComponet);
                            }
                        }

                        AutoUIGenerator generator = tf.gameObject.GetComponent<AutoUIGenerator>();
                        if (generator == null)
                        {
                            generator = tf.gameObject.AddComponent<AutoUIGenerator>();
                        }
                        EditorCopyComponent(_target, generator);
                        AutoUIGenerator.LinkReference(generator);

                        if (rootReference != null)
                        {
                            AutoUIReference targetReference = CheckAndCopyAutoUIReference(tf, rootReference);
                            targetReference.VariableName += i.ToString();
                        }

                        correctCount++;
                    }
                    else
                        errorCount++;
                }
            }

            if (correctCount > 0)
            {
                Debug.Log(string.Format("Success Copy To {0} Object", correctCount));
            }

            if (errorCount > 0)
            {
                Debug.LogError(string.Format("Fail Copy To {0} Object", errorCount));
            }
        }

        private AutoUIReference CheckAndCopyAutoUIReference(Transform tf, AutoUIReference reference)
        {
            AutoUIReference[] locals = tf.GetComponents<AutoUIReference>();
            AutoUIReference targetReference = locals.FirstOrDefault(x => x.TypeFullName == reference.TypeFullName);
            if (targetReference == null)
            {
                targetReference = tf.gameObject.AddComponent<AutoUIReference>();
            }
            EditorCopyComponent(reference, targetReference);

            return targetReference;
        }
    }
}
