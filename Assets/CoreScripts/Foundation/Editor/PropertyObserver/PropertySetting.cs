using System;
using System.Collections.Generic;
using System.IO;
using ible.Foundation.EventSystem.Property.PropertyObserve;
using ible.Foundation.Utility;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Debug = ible.Foundation.Utility.Debug;

namespace ible.Editor.PropertyObserver
{
    [Serializable]
    public class PropertyEditorInfo
    {
        [FormerlySerializedAs("_propertyName")]
        [FormerlySerializedAs("propertyName")]
        public string propertyKey;

        [FormerlySerializedAs("_groupName")] public string groupName;

        [FormerlySerializedAs("_type")] public string type;

        [FormerlySerializedAs("_comment")] public string comment;
    }

    [Serializable]
    public class PropertyHandleTypeEditorInfo
    {
        [FormerlySerializedAs("_typeName")] public string typeName;

        [FormerlySerializedAs("_comment")] public string comment;
    }

    [Serializable]
    public class PropertyTypeEditorInfo
    {
        [FormerlySerializedAs("_typeName")] public string typeName;

        [FormerlySerializedAs("_comment")] public string comment;

        [FormerlySerializedAs("_handleTypeEditorInfos")] public List<PropertyHandleTypeEditorInfo> handleTypeEditorInfos;
    }

    public class PropertySetting : ScriptableObject
    {
        [FormerlySerializedAs("_propertyInfos")] public List<PropertyEditorInfo> propertyInfos;

        [FormerlySerializedAs("_propertyTypeEditorInfos")] public List<PropertyTypeEditorInfo> propertyTypeEditorInfos;

        [MenuItem("Tools/PropertyObserver/Create PropertySettings")]
        public static void CreatePropertySettingAsset()
        {
            // 資料 Asset 路徑
            //if (!Directory.Exists(Path))
            //    Directory.CreateDirectory(Path);

            // 建立實體
            PropertySetting setting = CreateInstance<PropertySetting>();

            // 產生檔案
            AssetDatabase.CreateAsset(setting, PathUtility.GetPropertyConfigPath());

            UpdatePropertySettings();
        }

        [MenuItem("Tools/PropertyObserver/Update PropertySettings")]
        public static void UpdatePropertySettings()
        {
            PropertySetting setting =
                AssetDatabase.LoadAssetAtPath<PropertySetting>(PathUtility.GetPropertyConfigPath());

            if (setting == null)
            {
                Debug.Log("There isn't PropertySettings.asset !!!!");
                return;
            }

            if (setting.propertyInfos == null)
            {
                setting.propertyInfos = new List<PropertyEditorInfo>();
            }

            bool isDirty = false;

            // 移除已經刪掉的 PropertyKey
            for (int i = setting.propertyInfos.Count - 1; i >= 0; i--)
            {
                PropertyEditorInfo info = setting.propertyInfos[i];
                PropertyKey value;
                GameUtility.GetCurrentEnumValueFromString(info.propertyKey, out value);

                if (value == PropertyKey.None)
                {
                    setting.propertyInfos.RemoveAt(i);
                    isDirty = true;
                }
            }

            PropertyNameInfo nameInfo = PropertyNameInfo.Instance;

            // 新增新的 PropertyName
            string[] names = Enum.GetNames(typeof(PropertyKey));
            for (int i = 0; i < names.Length; i++)
            {
                string propertyKey = names.GetValue(i) as string;
                if (propertyKey == PropertyKey.None.ToString())
                {
                    continue;
                }

                PropertyEditorInfo info = setting.propertyInfos.Find(tmpInfo => tmpInfo.propertyKey == propertyKey);
                if (info != null)
                {
                    continue;
                }

                PropertyKey keyValue;
                GameUtility.GetCurrentEnumValueFromString(propertyKey, out keyValue);

                if (nameInfo.NameToTypeDic.ContainsKey(keyValue))
                {
                    // 新增
                    info = new PropertyEditorInfo
                    {
                        propertyKey = propertyKey,
                        type = nameInfo.NameToTypeDic[keyValue].ToString()
                    };
                    setting.propertyInfos.Add(info);
                    isDirty = true;
                }
                else
                {
                    Debug.LogErrorFormat("You don't set property type in PropertyKey.cs! PropertyKey: {0}", propertyKey);
                }
            }

            if (setting.propertyTypeEditorInfos == null)
            {
                setting.propertyTypeEditorInfos = new List<PropertyTypeEditorInfo>();
            }

            // 移除已經刪掉的 PropertyType
            for (int i = setting.propertyTypeEditorInfos.Count - 1; i >= 0; i--)
            {
                PropertyTypeEditorInfo info = setting.propertyTypeEditorInfos[i];
                PropertyType value;
                GameUtility.GetCurrentEnumValueFromString(info.typeName, out value);

                if (value == PropertyType.None)
                {
                    setting.propertyInfos.RemoveAt(i);
                    isDirty = true;
                }
            }

            // 新增新的 PropertyType
            names = Enum.GetNames(typeof(PropertyType));
            for (int i = 0; i < names.Length; i++)
            {
                string propertyTypeName = names.GetValue(i) as string;
                if (propertyTypeName == PropertyType.None.ToString())
                {
                    continue;
                }

                PropertyTypeEditorInfo info =
                    setting.propertyTypeEditorInfos.Find(tmpInfo => tmpInfo.typeName == propertyTypeName);
                if (info != null)
                {
                    continue;
                }

                // 新增
                info = new PropertyTypeEditorInfo { typeName = propertyTypeName };
                setting.propertyTypeEditorInfos.Add(info);
                isDirty = true;
            }

            foreach (PropertyTypeEditorInfo typeInfo in setting.propertyTypeEditorInfos)
            {
                if (typeInfo.handleTypeEditorInfos == null)
                {
                    typeInfo.handleTypeEditorInfos = new List<PropertyHandleTypeEditorInfo>();
                }
            }

            // 檢查 HandleType
            foreach (PropertyTypeEditorInfo typeInfo in setting.propertyTypeEditorInfos)
            {
                PropertyType propertyTypeValue;
                GameUtility.GetCurrentEnumValueFromString(typeInfo.typeName, out propertyTypeValue);

                switch (propertyTypeValue)
                {
                    case PropertyType.None:
                    {
                        break;
                    }
                    case PropertyType.Int:
                    {
                        CheckAndRemoveHandleInfo(typeInfo.handleTypeEditorInfos, IntHandleType.None, ref isDirty);
                        break;
                    }
                    case PropertyType.Long:
                    {
                        CheckAndRemoveHandleInfo(typeInfo.handleTypeEditorInfos, LongHandleType.None, ref isDirty);
                        break;
                    }
                    case PropertyType.Float:
                    {
                        CheckAndRemoveHandleInfo(typeInfo.handleTypeEditorInfos, FloatHandleType.None, ref isDirty);
                        break;
                    }
                    case PropertyType.String:
                    {
                        CheckAndRemoveHandleInfo(typeInfo.handleTypeEditorInfos, StringHandleType.None, ref isDirty);
                        break;
                    }
                    case PropertyType.Bool:
                    {
                        CheckAndRemoveHandleInfo(typeInfo.handleTypeEditorInfos, BoolHandleType.None, ref isDirty);
                        break;
                    }
                    case PropertyType.TimeSpan:
                    {
                        CheckAndRemoveHandleInfo(typeInfo.handleTypeEditorInfos, TimeSpanHandleType.None, ref isDirty);
                        break;
                    }
                    case PropertyType.DateTime:
                    {
                        CheckAndRemoveHandleInfo(typeInfo.handleTypeEditorInfos, DateTimeHandleType.None, ref isDirty);
                        break;
                    }
                    case PropertyType.Vector3:
                    {
                        CheckAndRemoveHandleInfo(typeInfo.handleTypeEditorInfos, Vector3HandleType.None, ref isDirty);
                        break;
                    }
                    case PropertyType.Vector2:
                    {
                        CheckAndRemoveHandleInfo(typeInfo.handleTypeEditorInfos, Vector2HandleType.None, ref isDirty);
                        break;
                    }
                    case PropertyType.Color:
                    {
                        CheckAndRemoveHandleInfo(typeInfo.handleTypeEditorInfos, ColorHandleType.None, ref isDirty);
                        break;
                    }
                    case PropertyType.Quaternion:
                    {
                        CheckAndRemoveHandleInfo(typeInfo.handleTypeEditorInfos, QuaternionHandleType.None, ref isDirty);
                        break;
                    }
                    case PropertyType.ULong:
                    {
                        CheckAndRemoveHandleInfo(typeInfo.handleTypeEditorInfos, ULongHandleType.None, ref isDirty);
                        break;
                    }
                    case PropertyType.WorldMapPosition:
                    {
                        CheckAndRemoveHandleInfo(typeInfo.handleTypeEditorInfos, WorldMapPositionHandleType.None, ref isDirty);
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (isDirty)
            {
                EditorUtility.SetDirty(setting);
                AssetDatabase.SaveAssets();
            }
        }

        private static void CheckAndRemoveHandleInfo<THandleType>(
            List<PropertyHandleTypeEditorInfo> handleTypeInfos,
            THandleType defualtHandleValue, ref bool isDirty)
        {
            string defaultHandleTypeStr = defualtHandleValue.ToString();

            // 移除已經刪掉的 PropertyHandleType
            for (int i = handleTypeInfos.Count - 1; i >= 0; i--)
            {
                PropertyHandleTypeEditorInfo info = handleTypeInfos[i];
                THandleType value;
                GameUtility.GetCurrentEnumValueFromString(info.typeName, out value);

                if (value.ToString() == defaultHandleTypeStr)
                {
                    handleTypeInfos.RemoveAt(i);
                    isDirty = true;
                }
            }

            // 新增新的 PropertyHandleType
            string[] names = Enum.GetNames(typeof(THandleType));
            for (int i = 0; i < names.Length; i++)
            {
                string propertyTypeName = names.GetValue(i) as string;
                if (propertyTypeName == defaultHandleTypeStr)
                {
                    continue;
                }

                PropertyHandleTypeEditorInfo info =
                    handleTypeInfos.Find(tmpInfo => tmpInfo.typeName == propertyTypeName);
                if (info != null)
                {
                    continue;
                }

                // 新增
                info = new PropertyHandleTypeEditorInfo
                {
                    typeName = propertyTypeName
                };
                handleTypeInfos.Add(info);
                isDirty = true;
            }
        }
    }
}