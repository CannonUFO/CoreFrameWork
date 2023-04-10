using System;
using System.Collections.Generic;
using ible.Foundation.EventSystem.Property.PropertyObserve;
using ible.Foundation.Utility;
using UnityEditor;
using UnityEngine;
using Debug = ible.Foundation.Utility.Debug;

namespace ible.Editor.PropertyObserver
{
    public class PropertyEditWindow : EditorWindow
    {
        public enum EditType
        {
            Property,
            HandleType
        }

        private int _currentEditingPropertyIndex = 0;

        private PropertyType _currentEditingPropertyType = PropertyType.None;
        private int _currentEdittingHandleTypeIndex = 0;

        private EditType _currentEditType = EditType.Property;

        private string[] _propertyKeysWithGroupName;

        private PropertySetting _setting;

        [MenuItem("Tools/PropertyObserver/Edit Property")]
        private static void ShowEditor()
        {
            PropertyEditWindow window = GetWindow<PropertyEditWindow>();
            window.titleContent = new GUIContent("Property Editor");
            window.Initialize();
        }

        private void Initialize()
        {
            _setting =
                AssetDatabase.LoadAssetAtPath<PropertySetting>(PathUtility.GetPropertyConfigPath());
            if (_setting == null)
            {
                PropertySetting.CreatePropertySettingAsset();
            }
            else
            {
                PropertySetting.UpdatePropertySettings();
            }

            _setting =
                AssetDatabase.LoadAssetAtPath<PropertySetting>(PathUtility.GetPropertyConfigPath());
            if (_setting == null)
            {
                Debug.Log("EventSettings.asset doesn't exist!!!");
                return;
            }

            _propertyKeysWithGroupName = new string[_setting.propertyInfos.Count];
            for (int i = 0; i < _setting.propertyInfos.Count; i++)
            {
                PropertyEditorInfo info = _setting.propertyInfos[i];
                if (string.IsNullOrEmpty(info.groupName))
                {
                    _propertyKeysWithGroupName[i] = info.propertyKey;
                }
                else
                {
                    _propertyKeysWithGroupName[i] = info.groupName + "/" + info.propertyKey;
                }
            }

            //_propertyTypes = new string[_setting._propertyTypeEditorInfos.Count];
            //for( int i = 0 ; i < _setting._propertyTypeEditorInfos.Count ; i++ )
            //{
            //    PropertyTypeEditorInfo info = _setting._propertyTypeEditorInfos[i];
            //    if( !string.IsNullOrEmpty( info._typeName ) )
            //    {
            //        _propertyTypes[i] = info._typeName;
            //    }
            //}
        }

        private void OnGUI()
        {
            if (_setting == null)
            {
                Debug.Log("PropertySettings.asset doesn't exist!!!");
                return;
            }

            _currentEditType = (EditType)EditorGUILayout.EnumPopup("Edit What", _currentEditType);

            switch (_currentEditType)
            {
                case EditType.Property:
                {
                    _currentEditingPropertyIndex =
                        EditorGUILayout.Popup("Property Key", _currentEditingPropertyIndex, _propertyKeysWithGroupName);

                    if (_currentEditingPropertyIndex < 0 ||
                        _currentEditingPropertyIndex >= _propertyKeysWithGroupName.Length)
                    {
                        return;
                    }

                    string propertyKeyFull = _propertyKeysWithGroupName[_currentEditingPropertyIndex];
                    string propertyKeyStr = propertyKeyFull;
                    if (propertyKeyFull.LastIndexOf('/') >= 0)
                    {
                        propertyKeyStr = propertyKeyFull.Substring(propertyKeyFull.LastIndexOf('/') + 1);
                    }
                    if (propertyKeyStr == PropertyKey.None.ToString())
                    {
                        return;
                    }

                    PropertyKey propertyKey;
                    GameUtility.GetCurrentEnumValueFromString(propertyKeyStr, out propertyKey);
                    if (propertyKey == PropertyKey.None)
                    {
                        return;
                    }

                    PropertyEditorInfo info = _setting.propertyInfos.Find(
                        infoTemp => infoTemp.propertyKey == propertyKey.ToString());
                    if (info == null)
                    {
                        return;
                    }

                    EditorGUILayout.LabelField("Type", info.type);
                    info.groupName = EditorGUILayout.TextField("Group Name", info.groupName);
                    info.comment = EditorGUILayout.TextField("Comment", info.comment);

                    if (string.IsNullOrEmpty(info.groupName))
                    {
                        _propertyKeysWithGroupName[_currentEditingPropertyIndex] = info.propertyKey;
                    }
                    else
                    {
                        _propertyKeysWithGroupName[_currentEditingPropertyIndex] = info.groupName + "/" +
                                                                                    info.propertyKey;
                    }

                    break;
                }
                case EditType.HandleType:
                {
                    PropertyType preType = _currentEditingPropertyType;
                    _currentEditingPropertyType = (PropertyType)EditorGUILayout.EnumPopup("Property Type",
                        _currentEditingPropertyType);
                    if (preType != _currentEditingPropertyType)
                    {
                        _currentEdittingHandleTypeIndex = 0;
                    }

                    if (_currentEditingPropertyType == PropertyType.None)
                    {
                        return;
                    }

                    string propertyTypeStr = _currentEditingPropertyType.ToString();
                    PropertyTypeEditorInfo info =
                        _setting.propertyTypeEditorInfos.Find(tempInfo => tempInfo.typeName == propertyTypeStr);

                    if (info == null)
                    {
                        return;
                    }

                    info.comment = EditorGUILayout.TextField("Comment", info.comment);

                    switch (_currentEditingPropertyType)
                    {
                        case PropertyType.None:
                        {
                            break;
                        }
                        case PropertyType.Int:
                        {
                            ShowHandleTypeEditInterface(info.handleTypeEditorInfos,
                                Enum.GetNames(typeof(IntHandleType)), IntHandleType.None);
                            break;
                        }
                        case PropertyType.Float:
                        {
                            ShowHandleTypeEditInterface(info.handleTypeEditorInfos,
                                Enum.GetNames(typeof(FloatHandleType)), FloatHandleType.None);
                            break;
                        }
                        case PropertyType.String:
                        {
                            ShowHandleTypeEditInterface(info.handleTypeEditorInfos,
                                Enum.GetNames(typeof(StringHandleType)), StringHandleType.None);
                            break;
                        }
                        case PropertyType.Bool:
                        {
                            ShowHandleTypeEditInterface(info.handleTypeEditorInfos,
                                Enum.GetNames(typeof(BoolHandleType)), BoolHandleType.None);
                            break;
                        }
                        case PropertyType.TimeSpan:
                        {
                            ShowHandleTypeEditInterface(info.handleTypeEditorInfos,
                                Enum.GetNames(typeof(TimeSpanHandleType)), TimeSpanHandleType.None);
                            break;
                        }
                        case PropertyType.DateTime:
                        {
                            ShowHandleTypeEditInterface(info.handleTypeEditorInfos,
                                Enum.GetNames(typeof(DateTimeHandleType)), DateTimeHandleType.None);
                            break;
                        }
                        case PropertyType.Vector3:
                        {
                            ShowHandleTypeEditInterface(info.handleTypeEditorInfos,
                                Enum.GetNames(typeof(Vector3HandleType)), Vector3HandleType.None);
                            break;
                        }
                        case PropertyType.Vector2:
                        {
                            ShowHandleTypeEditInterface(info.handleTypeEditorInfos,
                                Enum.GetNames(typeof(Vector2HandleType)), Vector2HandleType.None);
                            break;
                        }
                        case PropertyType.Color:
                        {
                            ShowHandleTypeEditInterface(info.handleTypeEditorInfos,
                                Enum.GetNames(typeof(ColorHandleType)), ColorHandleType.None);
                            break;
                        }
                        case PropertyType.Quaternion:
                        {
                            ShowHandleTypeEditInterface(info.handleTypeEditorInfos,
                                Enum.GetNames(typeof(QuaternionHandleType)), QuaternionHandleType.None);
                            break;
                        }
                        case PropertyType.Long:
                        {
                            ShowHandleTypeEditInterface(info.handleTypeEditorInfos,
                                Enum.GetNames(typeof(LongHandleType)), LongHandleType.None);
                            break;
                        }
                        case PropertyType.ULong:
                        {
                            ShowHandleTypeEditInterface(info.handleTypeEditorInfos,
                                Enum.GetNames(typeof(ULongHandleType)), ULongHandleType.None);
                            break;
                        }
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Save"))
            {
                Save();
            }
        }

        private void ShowHandleTypeEditInterface<THandleType>(
            List<PropertyHandleTypeEditorInfo> handleTypeEditorInfos,
            string[] handleTypes, THandleType defualHandleType)
        {
            _currentEdittingHandleTypeIndex = EditorGUILayout.Popup("Handle Type", _currentEdittingHandleTypeIndex, handleTypes);
            string handleTypeStr = handleTypes[_currentEdittingHandleTypeIndex];
            if (handleTypeStr == defualHandleType.ToString())
            {
                return;
            }

            PropertyHandleTypeEditorInfo info =
                handleTypeEditorInfos.Find(tempInfo => tempInfo.typeName == handleTypeStr);
            if (info == null)
            {
                return;
            }

            info.comment = EditorGUILayout.TextField("Comment", info.comment);
        }

        private void OnFocus()
        {
            Initialize();
        }

        private void OnLostFocus()
        {
            Save();
        }

        private void Save()
        {
            if (_setting != null)
            {
                EditorUtility.SetDirty(_setting);
                AssetDatabase.SaveAssets();
            }
        }
    }
}