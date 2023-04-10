using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ible.Foundation.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ible.Foundation.UI.AutoGen
{
    public class AutoUIGenerator : MonoBehaviour
    {
#if UNITY_EDITOR
        public static readonly string s_commonNamespace = "Common";

        public string UIName;
        public string Namespace;
        public bool IsCombineArray = false;

        [SerializeField]
        private List<AutoUIReference> _references = new List<AutoUIReference>();

        [ContextMenu("Collect AutoUIReferences")]
        public void CollectUIReference()
        {
            _references.Clear();
            GetAutoUIReferences(transform, ref _references);
        }

        public string UITypeName
        {
            get
            {
                if (string.IsNullOrEmpty(Namespace))
                {
                    return UIName;
                }

                return string.Format("{0}.{1}", Namespace, UIName);
            }
        }

        public static void AddUIComponentLinkReference(AutoUIGenerator[] generators)
        {
            foreach (AutoUIGenerator generator in generators)
            {
                Type type = UINameAttribute.GetTargetType(generator.UITypeName);
                if (type != null)
                {
                    if (generator.gameObject.GetComponent(type) == null)
                    {
                        generator.gameObject.AddComponent(type);
                    }

                    LinkReference(generator);
                }
                else
                {
                    Debug.LogError(string.Format("GameObject [{0}] Not Generate Code !!!", generator.gameObject.name), generator.gameObject);
                }
            }
        }

        public static void LinkReference(AutoUIGenerator generator)
        {
            Type type = UINameAttribute.GetTargetType(generator.UITypeName);
            LinkReference(generator.gameObject, type, generator.IsCombineArray);
        }

        public static void LinkReference(GameObject go, Type type, bool isCombineArray)
        {
            SerializedObject so = new SerializedObject(go.GetComponent(type));

            List<AutoUIReference> referenceList = new List<AutoUIReference>();
            GetAutoUIReferences(go.transform, ref referenceList);

            LinkReference(so, referenceList, isCombineArray);
        }

        public static void LinkReference(SerializedObject so, List<AutoUIReference> referenceList, bool isCombineArray, bool apply = true)
        {
            if (isCombineArray)
            {
                List<AutoUIReferenceArrayElement> elementList = new List<AutoUIReferenceArrayElement>();
                GetAutoUIReferenceArray(ref referenceList, ref elementList);

                foreach (var element in elementList)
                {
                    var prop = so.FindProperty(element.LowerVariableName);
                    if (prop.isArray)
                    {
                        prop.arraySize = element.ReferenceDict.Count;
                        foreach (var iter in element.ReferenceDict)
                        {
                            var p = prop.GetArrayElementAtIndex(iter.Key);
                            if (p != null && p.propertyType == SerializedPropertyType.ObjectReference)
                            {
                                var reference = iter.Value;
                                p.objectReferenceValue = reference.GetComponent(reference.TargetType);
                            }
                        }
                    }
                }
            }

            foreach (AutoUIReference reference in referenceList)
            {
                var prop = so.FindProperty(reference.LowerVariableName);
                if (prop != null && reference.TargetType != null)
                {
                    prop.objectReferenceValue = reference.GetComponent(reference.TargetType);
                }
                else
                {
                    Debug.LogError(string.Format("Can't Find Variable [{0}]", reference.LowerVariableName), reference.gameObject);
                }
            }

            if (apply)
                so.ApplyModifiedProperties();
        }

        public static void GetAutoUIReferenceArray(ref List<AutoUIReference> referenceList, ref List<AutoUIReferenceArrayElement> elementList)
        {
            elementList.Clear();

            List<AutoUIReferenceArrayElement> tempList = new List<AutoUIReferenceArrayElement>();
            foreach (AutoUIReference reference in referenceList)
            {
                if (!reference.IsCorrect())
                {
                    continue;
                }

                string name;
                int number;
                if (reference.GetVariableNumber(out name, out number))
                {
                    AutoUIReferenceArrayElement element = tempList.Find(x => x.TypeFullName == reference.TypeFullName && x.VariableName == name);
                    if (element == null)
                    {
                        element = new AutoUIReferenceArrayElement();
                        element.TypeFullName = reference.TypeFullName;
                        element.VariableName = name;

                        tempList.Add(element);
                    }
                    element.ReferenceDict.Add(number, reference);
                }
            }

            foreach (var element in tempList)
            {
                AutoUIReference reference = referenceList.Find(x => x.TypeFullName == element.TypeFullName && x.VariableName == element.VariableName);
                if (reference != null && element.ReferenceDict.ContainsKey(1))
                {
                    referenceList.Remove(reference);

                    Dictionary<int, AutoUIReference> dict = new Dictionary<int, AutoUIReference>();
                    dict.Add(0, reference);
                    for (int i = 1; i <= element.ReferenceDict.Count; i++)
                    {
                        if (element.ReferenceDict.ContainsKey(i))
                        {
                            referenceList.Remove(element.ReferenceDict[i]);

                            dict.Add(i, element.ReferenceDict[i]);
                        }
                        else
                            break;
                    }
                    element.ReferenceDict = dict;

                    elementList.Add(element);
                }
            }
        }

        public static void GetAutoUIReferences(Transform targetTransform, ref List<AutoUIReference> componentList)
        {
            AutoUIReference[] components = targetTransform.GetComponents<AutoUIReference>();
            AutoUIGenerator generator = targetTransform.GetComponent<AutoUIGenerator>();
            if (generator != null)
            {
                Type generateType = UINameAttribute.GetTargetType(generator.UITypeName);
                for (int i = 0; i < components.Length; i++)
                {
                    AutoUIReference reference = components[i];
                    if (reference.TargetType == generateType)
                    {
                        continue;
                    }

                    componentList.Add(reference);
                }
            }

            List<AutoUIReference> skipList = new List<AutoUIReference>();
            foreach (Transform tf in targetTransform)
            {
                GetComponentsInChildSkipGenerator(tf, ref componentList, ref skipList);
            }
        }

        private static void GetComponentsInChildSkipGenerator(Transform targetTransform, ref List<AutoUIReference> componentList, ref List<AutoUIReference> skipList)
        {
            AutoUIReference[] components = targetTransform.GetComponents<AutoUIReference>();
            AutoUIGenerator generator = targetTransform.GetComponent<AutoUIGenerator>();
            if (generator != null)
            {
                if (!generator.IsContinueCollect(ref skipList))
                {
                    Type generateType = UINameAttribute.GetTargetType(generator.UITypeName);
                    for (int i = 0; i < components.Length; i++)
                    {
                        if (skipList.Contains(components[i]))
                            continue;

                        AutoUIReference reference = components[i];
                        if (reference.TargetType == generateType)
                        {
                            componentList.Add(reference);
                        }
                    }

                    return;
                }
            }

            for (int i = 0; i < components.Length; i++)
            {
                if (skipList.Contains(components[i]))
                    continue;

                componentList.Add(components[i]);
            }

            foreach (Transform tf in targetTransform)
            {
                GetComponentsInChildSkipGenerator(tf, ref componentList, ref skipList);
            }
        }

        public bool IsRootUI()
        {
            return transform.parent == null && GetComponent<UICanvas>() != null;
        }

        public virtual bool IsContinueCollect(ref List<AutoUIReference> skipList)
        {
            return false;
        }

        protected virtual void Reset()
        {
            List<AutoUIGenerator> componentList = new List<AutoUIGenerator>();
            GetComponents<AutoUIGenerator>(componentList);

            if (componentList.Find(x => x != this))
            {
                Debug.LogError(string.Format("GameObject [{0}] too much AutoUIGenerator !!!", gameObject.name), gameObject);

                DestroyImmediate(this);
                return;
            }

            UICanvas uiCanvas = GetComponent<UICanvas>();
            if (uiCanvas != null)
            {
                UIName = uiCanvas.CanvasName;
                Namespace = uiCanvas.CanvasName;
            }
            else
            {
                UIName = gameObject.name;

                UICanvas rootCanvas = transform.root.GetComponent<UICanvas>();
                if (rootCanvas != null)
                {
                    Namespace = rootCanvas.CanvasName;
                }
                else
                {
                    Namespace = s_commonNamespace;
                }
            }

            UIName = Regex.Replace(UIName, @"[^\w]", string.Empty);
            Namespace = Regex.Replace(Namespace, @"[^\w]", string.Empty);
        }

#endif
    }
}