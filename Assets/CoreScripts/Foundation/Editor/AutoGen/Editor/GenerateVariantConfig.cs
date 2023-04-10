using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Events;
using DotLiquid;
using ible.Editor.Utility;
using ible.Foundation.UI;
using ible.Foundation;
using ible.GameModule.UI;

namespace ible.Foundation.UI.AutoGen.Editor
{
    public class GenerateVariantConfig
    {
        private static readonly string s_folderPath = "Assets/CoreScripts/Public/GameModule/UI/Config";
        private static string s_refConfigTemplate
        {
            get
            {
                TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/MainAssets/DotLiquidData/VariantConfigTemplate.txt");
                if (textAsset != null)
                    return textAsset.text;
                return string.Empty;
            }
        }

        private static string s_refEditorTemplate
        {
            get
            {
                TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/MainAssets/DotLiquidData/VariantEditorTemplate.txt");
                if (textAsset != null)
                    return textAsset.text;
                return string.Empty;
            }
        }

        private enum ObjectType
        {
            Field = 0,
            GameObject,
            Transform,

            ParentGameObject,
        }

        private static VariantAsset s_variantAsset = null;
        private static Dictionary<int, GameObject> s_objDict = new Dictionary<int, GameObject>();

        [MenuItem("Tools/UI/Generate Select Object Config Code")]
        public static void GenerateSelectObjectConfigCode()
        {
            GameObject selectObject = Selection.activeGameObject;
            if (selectObject == null)
                return;

            GenerateConfigCode(selectObject);

            AssetDatabase.Refresh();
        }

        [MenuItem("Tools/UI/Generate All Config Code")]
        public static void GenerateAllConfigCode()
        {
            string folder = "Assets/MainAssets/ArtAssets/BundleAssets/UI";
            var info = new DirectoryInfo(folder);
            var fileInfo = info.GetFiles();
            foreach (var file in fileInfo)
            {
                if (file.Name.Contains(".meta"))
                    continue;

                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(folder + "/" + file.Name);
                if (go != null)
                    GenerateConfigCode(go);
            }

            AssetDatabase.Refresh();
        }

        [MenuItem("Tools/UI/Fix All UI Prefab")]
        public static void FixAllUIPrefab()
        {
            string folder = "Assets/MainAssets/ArtAssets/BundleAssets/UI";
            var info = new DirectoryInfo(folder);
            var fileInfo = info.GetFiles();
            foreach (var file in fileInfo)
            {
                if (file.Name.Contains(".meta"))
                    continue;

                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(folder + "/" + file.Name);
                if (go != null)
                {
                    EditorUtility.DisplayDialog("錯誤", "請確認內容", "OK");
                }
            }

            AssetDatabase.Refresh();
        }

        [MenuItem("Tools/UI/Add Select Object Config")]
        public static void AddSelectObjectConfig()
        {
            GameObject selectObject = Selection.activeGameObject;
            if (selectObject == null)
            {
                EditorUtility.DisplayDialog("錯誤", "沒有選擇任何物件", "OK");

                return;
            }

            UICanvas uiCanvas = selectObject.transform.root.GetComponent<UICanvas>();
            if (uiCanvas == null)
            {
                EditorUtility.DisplayDialog("錯誤", "選擇的物件不是UI", "OK");

                return;
            }

            if (PrefabUtility.IsPartOfVariantPrefab(selectObject.transform.root.gameObject))
            {
                EditorUtility.DisplayDialog("錯誤", "選擇的物件是Variant UI", "OK");

                return;
            }

            Type type = UINameAttribute.GetTargetType("Editor.Config." + uiCanvas.CanvasName);
            if (type == null)
            {
                EditorUtility.DisplayDialog("錯誤", "找不到這個UI的設定檔 請找程式處理", "OK");

                return;
            }

            if (selectObject.transform.root.GetComponent(type) != null)
            {
                EditorUtility.DisplayDialog("錯誤", "物件已經加上設定檔", "OK");

                return;
            }

            selectObject.transform.root.gameObject.AddComponent(type);
        }

        public static VariantAsset GetVariantAsset()
        {
            if (s_variantAsset == null)
            {
                s_variantAsset = AssetDatabase.LoadAssetAtPath<VariantAsset>("Assets/MainAssets/Config/VariantAsset.asset");
            }
            return s_variantAsset;
        }

        public static void GenerateConfigCode(GameObject selectObject)
        {
            if (selectObject == null)
            {
                return;
            }

            string objectName = null;

            BaseUIView uiRoot = selectObject.transform.root.GetComponent<BaseUIView>();
            if (uiRoot != null)
                objectName = uiRoot.GetType().Name;

            BaseUIComponent uiComponent = selectObject.transform.root.GetComponent<BaseUIComponent>();
            if (uiComponent != null)
                objectName = uiComponent.GetType().Name;

            if (string.IsNullOrEmpty(objectName))
            {
                return;
            }

            s_objDict.Clear();

            //CopyUnpackGameObject(selectObject, string.Format("{0}{1}.prefab", BaseUIConfig.BackUpPath, selectObject.transform.root.name));

            var variantAsset = GetVariantAsset();

            List<GameObject> parentList = new List<GameObject>();
            List<UnityEngine.Object> objectList = new List<UnityEngine.Object>();
            List<Reference> referenceList = new List<Reference>();
            var monos = selectObject.transform.root.gameObject.GetComponentsInChildren<MonoBehaviour>();
            foreach (var mono in monos)
            {
                if (mono == null)
                    continue;

                Type type = mono.GetType();
                if (type != null)
                {
                    if (!variantAsset.IsNeedGenerate(type))
                        continue;

                    bool isCustomType = variantAsset.IsCustomNamespace(type);
                    if (isCustomType)
                    {
                        if (!parentList.Contains(mono.gameObject))
                            parentList.Add(mono.gameObject);
                    }

                    ReferenceParent referenceParent = new ReferenceParent();
                    referenceParent.IsParentRoot = mono.transform == selectObject.transform.root;
                    referenceParent.ParentName = referenceParent.IsParentRoot ? "gameObject" : GetObjectFieldName(mono.gameObject);
                    referenceParent.ParentType = type.FullName;

                    SerializedObject serializedObject = new SerializedObject(mono);
                    var prop = serializedObject.GetIterator();
                    while (prop.NextVisible(true))
                    {
                        if (isCustomType)
                        {
                            if (prop.isArray)
                            {
                                for (int x = 0; x < prop.arraySize; x++)
                                {
                                    var p = prop.GetArrayElementAtIndex(x);
                                    if (p != null && p.propertyType == SerializedPropertyType.ObjectReference)
                                    {
                                        if (p.objectReferenceValue != null)
                                        {
                                            if (!objectList.Contains(p.objectReferenceValue))
                                                objectList.Add(p.objectReferenceValue);

                                            Reference reference;
                                            if (GetReference(selectObject.transform.root, referenceParent, p, out reference))
                                                referenceList.Add(reference);
                                        }
                                    }
                                }
                            }

                            if (prop.propertyType == SerializedPropertyType.ObjectReference && !prop.propertyPath.Contains("."))
                            {
                                if (prop.propertyPath != "m_Script")
                                {
                                    if (prop.objectReferenceValue != null)
                                    {
                                        if (!objectList.Contains(prop.objectReferenceValue))
                                            objectList.Add(prop.objectReferenceValue);

                                        Reference reference;
                                        if (GetReference(selectObject.transform.root, referenceParent, prop, out reference))
                                            referenceList.Add(reference);
                                    }
                                }
                            }
                        }

                        if (GetPersistentList(selectObject.transform.root, prop, referenceParent, ref objectList, ref referenceList))
                        {
                            if (!isCustomType)
                            {
                                if (!parentList.Contains(mono.gameObject))
                                    parentList.Add(mono.gameObject);
                            }
                        }
                    }
                }
            }

            Hash hash = new Hash();
            hash["UIName"] = objectName;

            List<string> namespaceList = new List<string>();
            namespaceList.Add("UnityEngine");
            namespaceList.Add("System.Collections.Generic");
            hash["Namespaces"] = namespaceList;

            List<ObjectData> objectDataList = new List<ObjectData>();
            foreach (var obj in objectList)
            {
                if (obj == null)
                    continue;

                GameObject go = GetGameObjectByObject(selectObject.transform.root, obj);
                if (go == null)
                    continue;

                string depth = TransformPathUtility.GetPathDepth(go.transform);

                Type type = obj.GetType();
                if (typeof(GameObject).IsAssignableFrom(type) || variantAsset.IsCustomNamespace(type))
                {
                    if (go != null && !parentList.Contains(go))
                    {
                        if (objectDataList.Find(x => x.Type == ObjectType.GameObject && x.Obj == go) == null)
                            objectDataList.Add(new ObjectData() { Obj = go, Type = ObjectType.GameObject, Depth = depth });
                    }
                }
                else if (typeof(Transform).IsAssignableFrom(type))
                {
                    if (objectDataList.Find(x => x.Type == ObjectType.Transform && x.Obj == obj) == null)
                        objectDataList.Add(new ObjectData() { Obj = obj, Type = ObjectType.Transform, Depth = depth });
                }
                else
                {
                    if (objectDataList.Find(x => x.Type == ObjectType.Field && x.Obj == obj) == null)
                        objectDataList.Add(new ObjectData() { Obj = obj, Type = ObjectType.Field, Depth = depth });
                }
            }

            foreach (var parent in parentList)
            {
                if (parent.transform == selectObject.transform.root)
                    continue;

                string depth = TransformPathUtility.GetPathDepth(parent.transform);

                if (objectDataList.Find(x => x.Type == ObjectType.ParentGameObject && x.Obj == parent) == null)
                    objectDataList.Add(new ObjectData() { Obj = parent, Type = ObjectType.ParentGameObject, Depth = depth });
            }

            objectDataList.Sort((x, y) =>
            {
                return string.Compare(x.Depth, y.Depth);
            });

            List<Hash> objectHashList = new List<Hash>();
            foreach (var objectData in objectDataList)
            {
                Hash objectHash = GetObjectHash(selectObject.transform.root, objectData.Obj);
                objectHash["References"] = GetObjectReferenceHashList(objectData.Obj, referenceList);
                objectHash["ObjectType"] = (int)objectData.Type;

                objectHashList.Add(objectHash);
            }
            hash["ObjectList"] = objectHashList;

            List<Hash> referenceHashList = new List<Hash>();
            foreach (var reference in referenceList)
            {
                if (!reference.IsGenerateCode)
                    continue;

                Hash referenceHash = new Hash();
                referenceHash["FunctionName"] = reference.FunctionName;
                referenceHash["IsParentRoot"] = reference.Parent.IsParentRoot;
                referenceHash["ParentType"] = reference.Parent.ParentType;
                referenceHash["ParentName"] = reference.Parent.ParentName;
                referenceHash["PropertyPath"] = reference.PropertyPath;
                referenceHash["Property"] = reference.Property;
                referenceHash["PropertyType"] = reference.PropertyType;
                referenceHash["IsCustomType"] = reference.IsCustomType;
                referenceHash["GameObject"] = reference.GameObject;
                referenceHash["IsRoot"] = reference.IsRoot;
                string methodString = string.Empty;
                if (!string.IsNullOrEmpty(reference.MethodName))
                {
                    methodString = string.Format(", \"{0}\"", reference.MethodName);
                    if (!string.IsNullOrEmpty(reference.MethodParam))
                    {
                        methodString += string.Format(", {0}", reference.MethodParam);
                        if (!string.IsNullOrEmpty(reference.MethodParamType))
                        {
                            methodString += string.Format(".GetComponent(typeof({0}))", reference.MethodParamType);
                        }
                    }
                }
                referenceHash["MethodString"] = methodString;
                referenceHashList.Add(referenceHash);
            }
            hash["ReferenceList"] = referenceHashList;

            string configPath = string.Format(s_folderPath + "/{0}Config.cs", objectName);
            GenerateCode.Generate(configPath, s_refConfigTemplate, hash);

            string editorPath = string.Format(s_folderPath + "/Editor/{0}ConfigEditor.cs", objectName);
            GenerateCode.Generate(editorPath, s_refEditorTemplate, hash);
        }

        public static void DeleteUICode(string uiName)
        {
            string configPath = string.Format(s_folderPath + "/{0}Config.cs", uiName);
            if (File.Exists(configPath))
                File.Delete(configPath);

            string editorPath = string.Format(s_folderPath + "/Editor/{0}ConfigEditor.cs", uiName);
            if (File.Exists(editorPath))
                File.Delete(editorPath);
        }

        private static void CopyUnpackGameObject(GameObject selectObject, string path)
        {
            GameObject instanceRoot;
            bool isInProject = string.IsNullOrEmpty(selectObject.scene.name);
            if (isInProject)
            {
                instanceRoot = (GameObject)PrefabUtility.InstantiatePrefab(selectObject);
                PrefabUtility.UnpackPrefabInstance(instanceRoot, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            }
            else
                instanceRoot = GameObject.Instantiate(selectObject);

            PrefabUtility.SaveAsPrefabAsset(instanceRoot, path);
            GameObject.DestroyImmediate(instanceRoot);
        }

        private static Hash GetObjectHash(Transform root, UnityEngine.Object obj)
        {
            Hash hash = new Hash();
            Type type = obj.GetType();
            GameObject go = GetGameObjectByObject(root, obj);
            bool isCustomType = GetVariantAsset().IsCustomNamespace(type);

            hash["Type"] = isCustomType ? typeof(GameObject).FullName : type.FullName;
            hash["Name"] = GetObjectFieldName(isCustomType ? go : obj);
            hash["DisplayName"] = "\"" + hash["Name"] + "\"";
            hash["Path"] = go != null ? TransformPathUtility.GetTransformPath(go.transform, false) : string.Empty;

            return hash;
        }

        private static List<Hash> GetObjectReferenceHashList(UnityEngine.Object obj, List<Reference> referenceList)
        {
            string name = GetObjectFieldName(obj);
            List<Hash> list = new List<Hash>();
            foreach (var reference in referenceList)
            {
                if (name == reference.Property)
                {
                    Hash hash = new Hash();
                    hash["IsParentRoot"] = reference.Parent.IsParentRoot;
                    hash["ParentType"] = reference.Parent.ParentType;
                    hash["ParentName"] = reference.Parent.ParentName;
                    hash["PropertyPath"] = reference.PropertyPath;

                    list.Add(hash);
                }
            }

            return list;
        }

        private static GameObject GetGameObjectByObject(Transform root, UnityEngine.Object obj)
        {
            GameObject go = null;

            int instanceId = obj.GetInstanceID();
            if (s_objDict.TryGetValue(instanceId, out go))
                return go;

            Type type = obj.GetType();
            if (typeof(GameObject).IsAssignableFrom(type))
            {
                go = GetChildGameObjectByInstanceID(root.gameObject, instanceId);
            }
            else if (typeof(Transform).IsAssignableFrom(type))
            {
                Transform tf = GetChildTransformByInstanceID(root, instanceId);
                if (tf != null)
                {
                    go = tf.gameObject;
                }
            }
            else if (typeof(MonoBehaviour).IsAssignableFrom(type))
            {
                var components = root.GetComponentsInChildren(type, true);
                foreach (var component in components)
                {
                    if (component.GetInstanceID() == instanceId)
                    {
                        go = component.gameObject;
                        break;
                    }
                }
            }
            else if (typeof(ScriptableObject).IsAssignableFrom(type))
            {
            }

            s_objDict.Add(instanceId, go);

            return go;
        }

        private static bool GetReference(Transform root, ReferenceParent parent, SerializedProperty prop, out Reference reference)
        {
            Type type = prop.objectReferenceValue.GetType();

            var variantAsset = GetVariantAsset();
            bool isCustomType = variantAsset.IsCustomNamespace(type);

            reference = new Reference();
            reference.IsGenerateCode = true;

            reference.Parent = parent;
            GameObject propertyObj = GetGameObjectByObject(root, prop.objectReferenceValue);
            if (propertyObj == null)
                return false;

            bool isRoot = propertyObj.transform == root;
            reference.Property = isRoot ? "gameObject" : GetObjectFieldName(isCustomType ? propertyObj : prop.objectReferenceValue);
            reference.PropertyPath = prop.propertyPath;

            reference.FunctionName = "FixReference";

            reference.PropertyType = type.FullName;
            reference.IsCustomType = isCustomType;
            if (isCustomType || typeof(GameObject).IsAssignableFrom(type))
                reference.GameObject = reference.Property;
            else if (typeof(Transform).IsAssignableFrom(type))
                reference.GameObject = isRoot ? "gameObject" : reference.Property + ".gameObject";
            else
                reference.GameObject = reference.Property + "GameObject";
            reference.IsRoot = isRoot;

            return true;
        }

        private static string GetObjectFieldName(UnityEngine.Object obj)
        {
            if (obj != null)
            {
                int instanceID = obj.GetInstanceID();
                return "obj" + (instanceID >= 0 ? "0" : "1") + Math.Abs(instanceID).ToString();
            }

            return null;
        }

        private static GameObject GetChildGameObjectByInstanceID(GameObject parent, int instanceID)
        {
            if (parent.GetInstanceID() == instanceID)
                return parent;

            foreach (Transform tf in parent.transform)
            {
                GameObject go = GetChildGameObjectByInstanceID(tf.gameObject, instanceID);
                if (go != null)
                    return go;
            }

            return null;
        }

        private static Transform GetChildTransformByInstanceID(Transform parent, int instanceID)
        {
            if (parent.GetInstanceID() == instanceID)
                return parent;

            foreach (Transform tf in parent.transform)
            {
                Transform result = GetChildTransformByInstanceID(tf, instanceID);
                if (result != null)
                    return result;
            }

            return null;
        }

        private static bool GetPersistentList(Transform root, SerializedProperty prop, ReferenceParent parent, ref List<UnityEngine.Object> objectList, ref List<Reference> refList)
        {
            bool anyPersistent = false;
            SerializedProperty persistentCalls = prop.FindPropertyRelative("m_PersistentCalls.m_Calls");
            if (persistentCalls != null && persistentCalls.isArray)
            {
                for (int i = 0; i < persistentCalls.arraySize; ++i)
                {
                    SerializedProperty call = persistentCalls.GetArrayElementAtIndex(i);

                    //var callState = call.FindPropertyRelative("m_CallState");

                    var target = call.FindPropertyRelative("m_Target");
                    if (target.objectReferenceValue != null)
                    {
                        if (!objectList.Contains(target.objectReferenceValue))
                            objectList.Add(target.objectReferenceValue);

                        var methodName = call.FindPropertyRelative("m_MethodName");

                        if (!string.IsNullOrEmpty(methodName.stringValue))
                        {
                            Reference reference;
                            if (GetReference(root, parent, target, out reference))
                            {
                                reference.PropertyPath = call.propertyPath;
                                reference.MethodName = methodName.stringValue;

                                var mode = call.FindPropertyRelative("m_Mode");
                                PersistentListenerMode modeEnum = (PersistentListenerMode)mode.enumValueIndex;

                                var arguments = call.FindPropertyRelative("m_Arguments");
                                SerializedProperty argument;
                                switch (modeEnum)
                                {
                                    case PersistentListenerMode.Void:
                                        reference.FunctionName = "FixVoidPersistent";
                                        break;
                                    case PersistentListenerMode.Float:
                                        argument = arguments.FindPropertyRelative("m_FloatArgument");
                                        reference.FunctionName = "FixFloatPersistent";
                                        reference.MethodParam = argument.floatValue.ToString();
                                        break;
                                    case PersistentListenerMode.Int:
                                        argument = arguments.FindPropertyRelative("m_IntArgument");
                                        reference.FunctionName = "FixIntPersistent";
                                        reference.MethodParam = argument.intValue.ToString();
                                        break;
                                    case PersistentListenerMode.String:
                                        argument = arguments.FindPropertyRelative("m_StringArgument");
                                        reference.FunctionName = "FixStringPersistent";
                                        reference.MethodParam = "\"" + argument.stringValue + "\"";
                                        break;
                                    case PersistentListenerMode.Bool:
                                        argument = arguments.FindPropertyRelative("m_BoolArgument");
                                        reference.FunctionName = "FixBoolPersistent";
                                        reference.MethodParam = argument.boolValue.ToString().ToLower();
                                        break;
                                    case PersistentListenerMode.Object:
                                        argument = arguments.FindPropertyRelative("m_ObjectArgument");
                                        reference.FunctionName = "FixObjectPersistent";
                                        if (argument.objectReferenceValue != null)
                                        {
                                            if (!objectList.Contains(argument.objectReferenceValue))
                                                objectList.Add(argument.objectReferenceValue);

                                            reference.MethodParam = GetObjectFieldName(argument.objectReferenceValue);

                                            Type paramType = argument.objectReferenceValue.GetType();
                                            if (GetVariantAsset().IsCustomNamespace(paramType))
                                                reference.MethodParamType = paramType.FullName;

                                            Reference paramReference;
                                            if (GetReference(root, parent, argument, out paramReference))
                                            {
                                                paramReference.IsGenerateCode = false;
                                                refList.Add(paramReference);
                                            }
                                        }
                                        break;
                                    case PersistentListenerMode.EventDefined:
                                        reference.FunctionName = "FixVoidPersistent";
                                        break;
                                }

                                anyPersistent = true;

                                refList.Add(reference);
                            }
                        }
                    }
                }
            }

            return anyPersistent;
        }

        private struct ReferenceParent
        {
            public string ParentName;
            public string ParentType;
            public bool IsParentRoot;
        }

        private struct Reference
        {
            public ReferenceParent Parent;

            public bool IsGenerateCode;

            public string Property;
            public string PropertyPath;
            public string PropertyType;
            public bool IsCustomType;
            public string GameObject;
            public bool IsRoot;

            public string FunctionName;
            public string MethodName;
            public string MethodParam;
            public string MethodParamType;
        }

        private class ObjectData
        {
            public UnityEngine.Object Obj;
            public ObjectType Type;
            public string Depth;
        }
    }
}