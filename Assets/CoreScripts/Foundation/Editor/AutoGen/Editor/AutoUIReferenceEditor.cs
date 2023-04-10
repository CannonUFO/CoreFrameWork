using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace ible.Foundation.UI.AutoGen.Editor
{
    [CustomEditor(typeof(AutoUIReference))]
    public class AutoUIReferenceEditor : UnityEditor.Editor
    {
        private AutoUIReference _target;

        private AutoUIGenerator _relatedGenerator;
        private List<AutoUIReference> _tempList = new List<AutoUIReference>();

        public override void OnInspectorGUI()
        {
            _target = (AutoUIReference)target;

            EditorGUI.BeginChangeCheck();

            _target.VariableName = EditorGUILayout.TextField("Variable Name", _target.VariableName);

            bool enable = GUI.enabled;
            GUI.enabled = false;
            EditorGUILayout.TextField("Namespace", _target.Namespace);
            EditorGUILayout.TextField("Type Name", _target.TypeName);
            GUI.enabled = enable;

            _target.CollectTypeFullNameList();
            if (_target.TypeFullNameList.Count > 0)
            {
                int indexTemp = _target.SelectIndex;
                _target.SelectIndex = EditorGUILayout.Popup("Target Type", _target.SelectIndex, _target.TypeFullNameList.ToArray());
                if (_target.SelectIndex < _target.TypeFullNameList.Count)
                {
                    string typeName = _target.TypeFullNameList[_target.SelectIndex];
                    List<AutoUIReference> componentList = new List<AutoUIReference>();
                    _target.GetComponents(componentList);
                    if (componentList.Find(x => x != _target && x.TypeFullName == typeName))
                    {
                        _target.SelectIndex = indexTemp;

                        Debug.LogError(string.Format("Same Type [{0}] in GameObject [{1}]", typeName, _target.name), _target.gameObject);
                    }
                    else
                    {
                        _target.TypeFullName = typeName;
                    }
                }
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Find Generator"))
            {
                _relatedGenerator = GetAutoUIGenerator(_target);
            }
            enable = GUI.enabled;
            GUI.enabled = false;
            EditorGUILayout.ObjectField(_relatedGenerator, typeof(AutoUIGenerator), true);
            GUI.enabled = enable;
            EditorGUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(_target);
            }
        }

        private AutoUIGenerator GetAutoUIGenerator(AutoUIReference reference)
        {
            _tempList.Clear();

            AutoUIGenerator generator = null;
            generator = reference.transform.GetComponent<AutoUIGenerator>();
            if (generator != null)
            {
                Type generateType = UINameAttribute.GetTargetType(generator.UITypeName);
                if (reference.TargetType != generateType)
                {
                    return generator;
                }
            }

            if (reference.transform.parent == null)
            {
                return null;
            }
            generator = reference.transform.parent.GetComponentInParent<AutoUIGenerator>();
            while (generator != null)
            {
                if (generator.IsContinueCollect(ref _tempList))
                {
                    if (_tempList.Contains(reference))
                    {
                        return generator;
                    }
                    else
                    {
                        if (reference.transform.parent == null)
                        {
                            return null;
                        }
                        generator = generator.transform.parent.GetComponentInParent<AutoUIGenerator>();
                    }
                }
                else
                {
                    return generator;
                }
            }
            return null;
        }
    }
}
