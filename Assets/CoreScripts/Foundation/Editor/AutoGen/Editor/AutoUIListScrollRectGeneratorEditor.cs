//using System;
//using System.Linq;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEditor;


//namespace ible.Foundation.UI.AutoGen.Editor
//{
//    [CustomEditor(typeof(AutoUIListScrollRectGenerator))]
//    public class AutoUIListScrollRectGeneratorEditor : AutoUIGeneratorEditor
//    {
//        private AutoUIListScrollRectGenerator _targetGenerator;

//        public override void OnInspectorGUI()
//        {
//            _targetGenerator = (AutoUIListScrollRectGenerator)target;

//            base.OnInspectorGUI();
//        }

//        protected override void OnInspectorGUIButton()
//        {
//            if (GUILayout.Button("Generate Code"))
//            {
//                GenerateListScrollRect.GenerateUICode(_targetGenerator);

//                AssetDatabase.Refresh();
//            }

//            Type type = UINameAttribute.GetTargetType(_targetGenerator.UITypeName);
//            if (type != null)
//            {
//                if (_targetGenerator.gameObject.GetComponent(type) == null)
//                {
//                    if (GUILayout.Button("Add UI Component"))
//                    {
//                        _targetGenerator.gameObject.AddComponent(type);
//                        SetContentFiller(_targetGenerator);
//                        LinkContentReference(_targetGenerator);
//                    }
//                }
//                else
//                {
//                    if (GUILayout.Button("Link Reference"))
//                    {
//                        SetContentFiller(_targetGenerator);
//                        LinkContentReference(_targetGenerator);
//                    }
//                }

//                GUILayout.Space(20);

//                if (GUILayout.Button("Delete Code"))
//                {
//                    if (EditorUtility.DisplayDialog("Alert!!!", "Confirm Delete Code?", "Confirm", "Cancel"))
//                    {
//                        var component = _targetGenerator.gameObject.GetComponent(type);
//                        if (component != null)
//                        {
//                            DestroyImmediate(component, true);
//                        }

//                        GenerateListScrollRect.DeleteUICode(_targetGenerator.Namespace, _targetGenerator.UIName);
//                        AssetDatabase.Refresh();
//                    }
//                }
//            }
//        }

//        protected static void SetContentFiller(AutoUIListScrollRectGenerator generator)
//        {
//            ListScrollRect listScrollRect = generator.GetComponent<ListScrollRect>();
//            if (listScrollRect != null)
//            {
//                SerializedObject so = new SerializedObject(listScrollRect);
//                string variableName = "contentFillerInterface";
//                var prop = so.FindProperty(variableName);
//                prop.objectReferenceValue = generator.gameObject;
//                so.ApplyModifiedProperties();
//            }
//            else
//            {
//                Debug.LogError(string.Format("Can't Find ListScrollRect [{0}]", generator.gameObject.name), generator.gameObject);
//            }
//        }

//        protected static void LinkContentReference(AutoUIListScrollRectGenerator generator)
//        {
//            AutoUIReference refenrence = generator.GetChildReference();
//            Type type = UINameAttribute.GetTargetType(generator.UITypeName);
//            SerializedObject so = new SerializedObject(generator.GetComponent(type));

//            SetProperty(so, "content", refenrence.GetComponent(refenrence.TargetType));
//            SetProperty(so, "listScrollRect", generator.GetComponent<ListScrollRect>());

//            so.ApplyModifiedProperties();
//        }

//        private static void SetProperty(SerializedObject so, string variableName, UnityEngine.Object obj)
//        {
//            var prop = so.FindProperty(variableName);
//            if (prop != null && obj != null)
//            {
//                prop.objectReferenceValue = obj;
//            }
//            else
//            {
//                Debug.LogError(string.Format("Can't Find Variable [{0}]", variableName));
//            }
//        }
//    }
//}