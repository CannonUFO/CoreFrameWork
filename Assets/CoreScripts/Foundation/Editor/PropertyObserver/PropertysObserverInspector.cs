using ible.Foundation.EventSystem.Property.PropertyObserve;
using ible.Foundation.Utility;
using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Debug = ible.Foundation.Utility.Debug;

namespace ible.Editor.PropertyObserver
{
    using PropertyObserve = Foundation.EventSystem.Property.PropertyObserve;

    [CustomEditor(typeof(PropertysObserver), true)]
    internal class PropertysObserverInspector : UnityEditor.Editor
    {
        private const int HeightGap = 5;
        private const int ReservedGap = 10;

        private const string PropertyType = "propertyType";
        private const string PropertyKey = "propertyKey";
        private const string HandleTypeValue = "handleType";

        private const string ImageProperty = "image";
        private const string ToggleProperty = "toggle";
        private const string TextProperty = "text";
        private const string BindingInfosProperty = "bindingInfos";
        private const string GameObjectProperty = "gameObject";
        private const string SelectableProperty = "selectable";
        private const string SliderProperty = "slider";
        private const string TransformProperty = "transform";
        private const string RectTransformProperty = "rectTransform";
        private const string ToStringFormat = "toStringForamt";
        private const string IsUniversal = "isUniversal";
        private const string AudioPlayerProperty = "audioPlayerWwise";

        private const string StrTarget = "Target";
        private const string StrPropertyKey = "PropertyKey";
        private const string StrHandleType = "HandleType";

        private static readonly PropertyNameInfo PropertyNameMapping = PropertyNameInfo.Instance;

        private ReorderableList _list;

        private GUIContent[] _propertyNameOptions;

        private PropertySetting _setting;

        private float LineHeight
        {
            get { return EditorGUIUtility.singleLineHeight + HeightGap; }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();
            _list.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            _setting =
                AssetDatabase.LoadAssetAtPath<PropertySetting>(PathUtility.GetPropertyConfigPath());

            _propertyNameOptions = new GUIContent[_setting.propertyInfos.Count];
            for (int i = 0; i < _propertyNameOptions.Length; i++)
            {
                PropertyEditorInfo info = _setting.propertyInfos[i];
                if (!string.IsNullOrEmpty(info.groupName))
                {
                    _propertyNameOptions[i] = new GUIContent(info.groupName + "/" + info.propertyKey, info.comment);
                }
                else
                {
                    _propertyNameOptions[i] = new GUIContent(info.propertyKey, info.comment);
                }
            }

            _list = new ReorderableList(serializedObject,
                serializedObject.FindProperty(BindingInfosProperty),
                true, true, true, true);

            _list.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    rect.height = EditorGUIUtility.singleLineHeight;
                    rect.y += 2;

                    SerializedProperty bindingInfo = _list.serializedProperty.GetArrayElementAtIndex(index);

                    SerializedProperty propertyType = bindingInfo.FindPropertyRelative(PropertyType);
                    SerializedProperty propertyName = bindingInfo.FindPropertyRelative(PropertyKey);
                    SerializedProperty handleTypeValue = bindingInfo.FindPropertyRelative(HandleTypeValue);

                    // 找出當前屬性名稱字串對應到的Index
                    int propertyNameIndex = 0;
                    for (int i = 0; i < _propertyNameOptions.Length; i++)
                    {
                        int lastIndexOfSlash = _propertyNameOptions[i].text.LastIndexOf("/", StringComparison.Ordinal);
                        if (lastIndexOfSlash >= 0
                           && _propertyNameOptions[i].text.Substring(lastIndexOfSlash + 1) == propertyName.stringValue)
                        {
                            propertyNameIndex = i;
                            break;
                        }
                        if (lastIndexOfSlash < 0
                           && _propertyNameOptions[i].text == propertyName.stringValue)
                        {
                            propertyNameIndex = i;
                            break;
                        }
                    }

                    // 顯示屬性名稱選項
                    propertyNameIndex = EditorGUI.Popup(rect,
                            new GUIContent(StrPropertyKey), propertyNameIndex, _propertyNameOptions);

                    string propertyNameStr;
                    int lastIndexOfSlashResult = _propertyNameOptions[propertyNameIndex].text.LastIndexOf("/",
                        StringComparison.Ordinal);
                    if (lastIndexOfSlashResult >= 0)
                    {
                        propertyNameStr = _propertyNameOptions[propertyNameIndex].text.Substring(lastIndexOfSlashResult + 1);
                    }
                    else
                    {
                        propertyNameStr = _propertyNameOptions[propertyNameIndex].text;
                    }

                    PropertyKey propertyKeyValue;
                    GameUtility.GetCurrentEnumValueFromString(propertyNameStr, out propertyKeyValue);
                    propertyName.stringValue = propertyKeyValue.ToString();

                    // 從mapping中找出屬性名稱對應到的屬性類型
                    PropertyType tempType;
                    if (!PropertyNameMapping.NameToTypeDic.TryGetValue(propertyKeyValue, out tempType))
                    {
                        Debug.LogErrorFormat(
                            "propertyKey( {0} ) doesn't set property type!! please check setting again!!!",
                            propertyKeyValue.ToString());
                        EditorGUILayout.HelpBox(
                            string.Format("propertyKey( {0} ) doesn't set property type!! please check setting again!!!",
                                propertyKeyValue),
                            MessageType.Warning);
                        return; // 屬性沒設定類別  壞掉囉
                    }
                    propertyType.stringValue = tempType.ToString();

                    // 找出對應類型的Info
                    PropertyTypeEditorInfo propertyTypeEditorInfo =
                            _setting.propertyTypeEditorInfos.Find(
                                tempInfo => tempInfo.typeName == tempType.ToString());

                    // 產生HandleType的選項
                    var handleTypeContents = new GUIContent[propertyTypeEditorInfo.handleTypeEditorInfos.Count];
                    for (int i = 0; i < propertyTypeEditorInfo.handleTypeEditorInfos.Count; i++)
                    {
                        PropertyHandleTypeEditorInfo info = propertyTypeEditorInfo.handleTypeEditorInfos[i];
                        handleTypeContents[i] = new GUIContent(info.typeName, info.comment);
                    }

                    // 顯示處理方式選項
                    ShowHandleTypePopup(ref rect, handleTypeValue, handleTypeContents);

                    // NOTE: 如果新增類型必須修改這邊的switch
                    switch (tempType)
                    {
                        case PropertyObserve.PropertyType.Int:
                        {
                            IntHandleType intHandleType;
                            GameUtility.GetCurrentEnumValueFromString(handleTypeValue.stringValue, out intHandleType);

                            // 根據當前選擇的處理方式, 顯示需要的Component欄位
                            switch (intHandleType)
                            {
                                case IntHandleType.ToText:
                                {
                                    ShowText(bindingInfo, ref rect);

                                    rect.y += LineHeight;
                                    SerializedProperty tostringFormat = bindingInfo.FindPropertyRelative(ToStringFormat);
                                    EditorGUI.PropertyField(rect, tostringFormat);
                                    break;
                                }
                                case IntHandleType.AddCommas:
                                {
                                    ShowText(bindingInfo, ref rect);
                                    break;
                                }
                                case IntHandleType.BFormatOrAddCommans:
                                case IntHandleType.KmbFormat:
                                {
                                    ShowText(bindingInfo, ref rect);
                                    break;
                                }
                                case IntHandleType.RedHint:
                                {
                                    ShowText(bindingInfo, ref rect);
                                    ShowGameObject(bindingInfo, ref rect);
                                    break;
                                }
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                            break;
                        }
                        case PropertyObserve.PropertyType.Long:
                        {
                            LongHandleType intHandleType;
                            GameUtility.GetCurrentEnumValueFromString(handleTypeValue.stringValue, out intHandleType);

                            // 根據當前選擇的處理方式, 顯示需要的Component欄位
                            switch (intHandleType)
                            {
                                case LongHandleType.ToText:
                                {
                                    ShowText(bindingInfo, ref rect);

                                    rect.y += LineHeight;
                                    SerializedProperty tostringFormat = bindingInfo.FindPropertyRelative(ToStringFormat);
                                    EditorGUI.PropertyField(rect, tostringFormat);
                                    break;
                                }
                                case LongHandleType.AddCommas:
                                {
                                    ShowText(bindingInfo, ref rect);
                                    break;
                                }
                                case LongHandleType.BFormatOrAddCommans:
                                case LongHandleType.KmbFormat:
                                {
                                    ShowText(bindingInfo, ref rect);
                                    break;
                                }
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                            break;
                        }
                        case PropertyObserve.PropertyType.Float:
                        {
                            FloatHandleType floatHandleType;
                            GameUtility.GetCurrentEnumValueFromString(handleTypeValue.stringValue, out floatHandleType);

                            // 根據當前選擇的處理方式, 顯示需要的Component欄位
                            switch (floatHandleType)
                            {
                                case FloatHandleType.ToText:
                                {
                                    ShowText(bindingInfo, ref rect);
                                    break;
                                }
                                case FloatHandleType.Floor:
                                {
                                    ShowText(bindingInfo, ref rect);
                                    break;
                                }
                                case FloatHandleType.Ceiling:
                                {
                                    ShowText(bindingInfo, ref rect);
                                    break;
                                }
                                case FloatHandleType.Round:
                                {
                                    ShowText(bindingInfo, ref rect);
                                    break;
                                }
                                case FloatHandleType.SliderPercentage:
                                {
                                    ShowSlider(bindingInfo, ref rect);
                                    break;
                                }
                                case FloatHandleType.ImageFillAmount:
                                {
                                    ShowImage(bindingInfo, ref rect);
                                    break;
                                }
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            break;
                        }
                        case PropertyObserve.PropertyType.String:
                        {
                            StringHandleType stringHandleType;
                            GameUtility.GetCurrentEnumValueFromString(handleTypeValue.stringValue, out stringHandleType);

                            // 根據當前選擇的處理方式, 顯示需要的Component欄位
                            switch (stringHandleType)
                            {
                                case StringHandleType.ToText:
                                {
                                    ShowText(bindingInfo, ref rect);
                                    break;
                                }
                                case StringHandleType.AsImageSprite:
                                {
                                    ShowImage(bindingInfo, ref rect);
                                    break;
                                }
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                            break;
                        }
                        case PropertyObserve.PropertyType.Bool:
                        {
                            BoolHandleType boolHandleType;
                            GameUtility.GetCurrentEnumValueFromString(handleTypeValue.stringValue, out boolHandleType);

                            // 根據當前選擇的處理方式, 顯示需要的Component欄位
                            switch (boolHandleType)
                            {
                                case BoolHandleType.ToText:
                                {
                                    ShowText(bindingInfo, ref rect);
                                    break;
                                }
                                case BoolHandleType.ToToggle:
                                {
                                    ShowToggle(bindingInfo, ref rect);
                                    break;
                                }
                                case BoolHandleType.Visible:
                                {
                                    ShowGameObject(bindingInfo, ref rect);
                                    break;
                                }
                                case BoolHandleType.IsInteractable:
                                {
                                    ShowSelectable(bindingInfo, ref rect);
                                    break;
                                }
                                case BoolHandleType.IsPlayAudio:
                                {
                                    ShowAudioPlayer(bindingInfo, ref rect);
                                    break;
                                }
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                            break;
                        }
                        case PropertyObserve.PropertyType.TimeSpan:
                        {
                            TimeSpanHandleType timeSpanHandleType;
                            GameUtility.GetCurrentEnumValueFromString(handleTypeValue.stringValue, out timeSpanHandleType);

                            // 根據當前選擇的處理方式, 顯示需要的Component欄位
                            switch (timeSpanHandleType)
                            {
                                case TimeSpanHandleType.ToText:
                                {
                                    ShowText(bindingInfo, ref rect);
                                    break;
                                }
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            break;
                        }
                        case PropertyObserve.PropertyType.DateTime:
                        {
                            DateTimeHandleType dateTimeHandleType;
                            GameUtility.GetCurrentEnumValueFromString(handleTypeValue.stringValue, out dateTimeHandleType);

                            // 根據當前選擇的處理方式, 顯示需要的Component欄位
                            switch (dateTimeHandleType)
                            {
                                case DateTimeHandleType.ToText:
                                {
                                    ShowText(bindingInfo, ref rect);

                                    rect.y += LineHeight;
                                    SerializedProperty isUniversal = bindingInfo.FindPropertyRelative(IsUniversal);
                                    EditorGUI.PropertyField(rect, isUniversal);
                                    break;
                                }
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                            break;
                        }
                        case PropertyObserve.PropertyType.Vector3:
                        {
                            Vector3HandleType vector3HandleType;
                            GameUtility.GetCurrentEnumValueFromString(handleTypeValue.stringValue, out vector3HandleType);

                            // 根據當前選擇的處理方式, 顯示需要的Component欄位
                            switch (vector3HandleType)
                            {
                                case Vector3HandleType.ToLocalPosition:
                                {
                                    ShowTransfrom(bindingInfo, ref rect);
                                    break;
                                }
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                            break;
                        }
                        case PropertyObserve.PropertyType.Vector2:
                        {
                            Vector2HandleType vector2HandleType;
                            GameUtility.GetCurrentEnumValueFromString(handleTypeValue.stringValue, out vector2HandleType);

                            // 根據當前選擇的處理方式, 顯示需要的Component欄位
                            switch (vector2HandleType)
                            {
                                case Vector2HandleType.ToAnchorPosition:
                                {
                                    ShowRectTransfrom(bindingInfo, ref rect);
                                    break;
                                }
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                            break;
                        }
                        case PropertyObserve.PropertyType.Color:
                        {
                            ColorHandleType colorHandleType;
                            GameUtility.GetCurrentEnumValueFromString(handleTypeValue.stringValue, out colorHandleType);

                            // 根據當前選擇的處理方式, 顯示需要的Component欄位
                            switch (colorHandleType)
                            {
                                case ColorHandleType.ToImageColor:
                                {
                                    ShowImage(bindingInfo, ref rect);
                                    break;
                                }
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                            break;
                        }
                        case PropertyObserve.PropertyType.Quaternion:
                        {
                            QuaternionHandleType quaternionHandleType;
                            GameUtility.GetCurrentEnumValueFromString(handleTypeValue.stringValue, out quaternionHandleType);

                            // 根據當前選擇的處理方式, 顯示需要的Component欄位
                            switch (quaternionHandleType)
                            {
                                case QuaternionHandleType.ToLocalRotation:
                                {
                                    ShowTransfrom(bindingInfo, ref rect);
                                    break;
                                }
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                            break;
                        }
                        case PropertyObserve.PropertyType.ULong:
                        {
                            ULongHandleType intHandleType;
                            GameUtility.GetCurrentEnumValueFromString(handleTypeValue.stringValue, out intHandleType);

                            // 根據當前選擇的處理方式, 顯示需要的Component欄位
                            switch (intHandleType)
                            {
                                case ULongHandleType.ToText:
                                {
                                    ShowText(bindingInfo, ref rect);

                                    rect.y += LineHeight;
                                    SerializedProperty tostringFormat = bindingInfo.FindPropertyRelative(ToStringFormat);
                                    EditorGUI.PropertyField(rect, tostringFormat);
                                    break;
                                }
                                case ULongHandleType.AddCommas:
                                {
                                    ShowText(bindingInfo, ref rect);
                                    break;
                                }
                                case ULongHandleType.BFormatOrAddCommas:
                                case ULongHandleType.KmbFormat:
                                {
                                    ShowText(bindingInfo, ref rect);
                                    break;
                                }
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                            break;
                        }
                        case PropertyObserve.PropertyType.WorldMapPosition:
                        {
                            WorldMapPositionHandleType positionHandleType;
                            GameUtility.GetCurrentEnumValueFromString(handleTypeValue.stringValue, out positionHandleType);

                            switch (positionHandleType)
                            {
                                case WorldMapPositionHandleType.ToText:
                                {
                                    ShowText(bindingInfo, ref rect);
                                    break;
                                }
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                            break;
                        }
                        case PropertyObserve.PropertyType.None:
                        {
                            break;
                        }
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                };

            _list.onAddCallback = list =>
            {
                int index = list.serializedProperty.arraySize;
                list.serializedProperty.arraySize++;
                list.index = index;
            };

            _list.elementHeightCallback = index =>
            {
                SerializedProperty bindingInfo = _list.serializedProperty.GetArrayElementAtIndex(index);
                SerializedProperty propertyType = bindingInfo.FindPropertyRelative(PropertyType);
                SerializedProperty handleTypeValue = bindingInfo.FindPropertyRelative(HandleTypeValue);

                PropertyType type;
                GameUtility.GetCurrentEnumValueFromString(propertyType.stringValue, out type);

                // NOTE: 如果新增類型必須修改這邊的switch
                switch (type)
                {
                    case PropertyObserve.PropertyType.None:
                    {
                        return ReservedGap;
                    }
                    case PropertyObserve.PropertyType.Int:
                    {
                        IntHandleType handleType;
                        GameUtility.GetCurrentEnumValueFromString(handleTypeValue.stringValue, out handleType);
                        switch (handleType)
                        {
                            case IntHandleType.ToText:
                            {
                                return LineHeight * 4 + ReservedGap;
                            }
                            case IntHandleType.AddCommas:
                            {
                                return LineHeight * 3 + ReservedGap;
                            }
                            case IntHandleType.BFormatOrAddCommans:
                            case IntHandleType.KmbFormat:
                            {
                                return LineHeight * 3 + ReservedGap;
                            }
                            case IntHandleType.RedHint:
                            {
                                return LineHeight * 4 + ReservedGap;
                            }
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                    case PropertyObserve.PropertyType.Long:
                    {
                        LongHandleType handleType;
                        GameUtility.GetCurrentEnumValueFromString(handleTypeValue.stringValue, out handleType);
                        switch (handleType)
                        {
                            case LongHandleType.ToText:
                            {
                                return LineHeight * 4 + ReservedGap;
                            }
                            case LongHandleType.AddCommas:
                            {
                                return LineHeight * 3 + ReservedGap;
                            }
                            case LongHandleType.BFormatOrAddCommans:
                            case LongHandleType.KmbFormat:
                            {
                                return LineHeight * 3 + ReservedGap;
                            }
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                    case PropertyObserve.PropertyType.Float:
                    {
                        FloatHandleType handleType;
                        GameUtility.GetCurrentEnumValueFromString(handleTypeValue.stringValue, out handleType);
                        switch (handleType)
                        {
                            case FloatHandleType.ToText:
                            {
                                return LineHeight * 3 + ReservedGap;
                            }
                            case FloatHandleType.Floor:
                            {
                                return LineHeight * 3 + ReservedGap;
                            }
                            case FloatHandleType.Ceiling:
                            {
                                return LineHeight * 3 + ReservedGap;
                            }
                            case FloatHandleType.Round:
                            {
                                return LineHeight * 3 + ReservedGap;
                            }
                            case FloatHandleType.SliderPercentage:
                            {
                                return LineHeight * 3 + ReservedGap;
                            }
                            case FloatHandleType.ImageFillAmount:
                            {
                                return LineHeight * 3 + ReservedGap;
                            }
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                    case PropertyObserve.PropertyType.String:
                    {
                        StringHandleType handleType;
                        GameUtility.GetCurrentEnumValueFromString(handleTypeValue.stringValue, out handleType);
                        switch (handleType)
                        {
                            case StringHandleType.ToText:
                            {
                                return LineHeight * 3 + ReservedGap;
                            }
                            case StringHandleType.AsImageSprite:
                            {
                                return LineHeight * 3 + ReservedGap;
                            }
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                    case PropertyObserve.PropertyType.Bool:
                    {
                        BoolHandleType handleType;
                        GameUtility.GetCurrentEnumValueFromString(handleTypeValue.stringValue, out handleType);
                        switch (handleType)
                        {
                            case BoolHandleType.ToText:
                            {
                                return LineHeight * 3 + ReservedGap;
                            }
                            case BoolHandleType.ToToggle:
                            {
                                return LineHeight * 3 + ReservedGap;
                            }
                            case BoolHandleType.Visible:
                            {
                                return LineHeight * 3 + ReservedGap;
                            }
                            case BoolHandleType.IsInteractable:
                            {
                                return LineHeight * 3 + ReservedGap;
                            }
                            case BoolHandleType.IsPlayAudio:
                            {
                                return LineHeight * 3 + ReservedGap;
                            }
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                    case PropertyObserve.PropertyType.TimeSpan:
                    {
                        TimeSpanHandleType handleType;
                        GameUtility.GetCurrentEnumValueFromString(handleTypeValue.stringValue, out handleType);
                        switch (handleType)
                        {
                            case TimeSpanHandleType.ToText:
                            {
                                return LineHeight * 3 + ReservedGap;
                            }
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                    case PropertyObserve.PropertyType.DateTime:
                    {
                        DateTimeHandleType handleType;
                        GameUtility.GetCurrentEnumValueFromString(handleTypeValue.stringValue, out handleType);
                        switch (handleType)
                        {
                            case DateTimeHandleType.ToText:
                            {
                                return LineHeight * 4 + ReservedGap;
                            }
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                    case PropertyObserve.PropertyType.Vector3:
                    {
                        Vector3HandleType handleType;
                        GameUtility.GetCurrentEnumValueFromString(handleTypeValue.stringValue, out handleType);
                        switch (handleType)
                        {
                            case Vector3HandleType.ToLocalPosition:
                            {
                                return LineHeight * 3 + ReservedGap;
                            }
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                    case PropertyObserve.PropertyType.Vector2:
                    {
                        Vector2HandleType handleType;
                        GameUtility.GetCurrentEnumValueFromString(handleTypeValue.stringValue, out handleType);
                        switch (handleType)
                        {
                            case Vector2HandleType.ToAnchorPosition:
                            {
                                return LineHeight * 3 + ReservedGap;
                            }
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                    case PropertyObserve.PropertyType.Color:
                    {
                        ColorHandleType handleType;
                        GameUtility.GetCurrentEnumValueFromString(handleTypeValue.stringValue, out handleType);
                        switch (handleType)
                        {
                            case ColorHandleType.ToImageColor:
                            {
                                return LineHeight * 3 + ReservedGap;
                            }
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                    case PropertyObserve.PropertyType.Quaternion:
                    {
                        QuaternionHandleType handleType;
                        GameUtility.GetCurrentEnumValueFromString(handleTypeValue.stringValue, out handleType);
                        switch (handleType)
                        {
                            case QuaternionHandleType.ToLocalRotation:
                            {
                                return LineHeight * 3 + ReservedGap;
                            }
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                    case PropertyObserve.PropertyType.ULong:
                    {
                        ULongHandleType handleType;
                        GameUtility.GetCurrentEnumValueFromString(handleTypeValue.stringValue, out handleType);

                        switch (handleType)
                        {
                            case ULongHandleType.ToText:
                            {
                                return LineHeight * 4 + ReservedGap;
                            }
                            case ULongHandleType.AddCommas:
                            {
                                return LineHeight * 3 + ReservedGap;
                            }
                            case ULongHandleType.BFormatOrAddCommas:
                            case ULongHandleType.KmbFormat:
                            {
                                return LineHeight * 3 + ReservedGap;
                            }
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                    case PropertyObserve.PropertyType.WorldMapPosition:
                    {
                        WorldMapPositionHandleType positionHandleType;
                        GameUtility.GetCurrentEnumValueFromString(handleTypeValue.stringValue, out positionHandleType);

                        switch (positionHandleType)
                        {
                            case WorldMapPositionHandleType.ToText:
                            {
                                return LineHeight * 3 + ReservedGap;
                            }
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            };
        }

        private void ShowHandleTypePopup(ref Rect rect, SerializedProperty handleTypeValue, GUIContent[] handleTypeContents)
        {
            rect.y += LineHeight;
            int currentHandleTypeIndex = 0;
            for (int i = 0; i < handleTypeContents.Length; i++)
            {
                if (handleTypeContents[i].text == handleTypeValue.stringValue)
                {
                    currentHandleTypeIndex = i;
                }
            }
            currentHandleTypeIndex = EditorGUI.Popup(rect, new GUIContent { text = StrHandleType }, currentHandleTypeIndex,
                handleTypeContents);
            handleTypeValue.stringValue = handleTypeContents[currentHandleTypeIndex].text;
        }

        private void ShowRectTransfrom(SerializedProperty bindingInfo, ref Rect rect)
        {
            rect.y += LineHeight;

            SerializedProperty rectTransformProperty = bindingInfo.FindPropertyRelative(RectTransformProperty);
            EditorGUI.ObjectField(rect, rectTransformProperty, new GUIContent { text = StrTarget });
        }

        private void ShowTransfrom(SerializedProperty bindingInfo, ref Rect rect)
        {
            rect.y += LineHeight;

            SerializedProperty transformProperty = bindingInfo.FindPropertyRelative(TransformProperty);
            EditorGUI.ObjectField(rect, transformProperty, new GUIContent { text = StrTarget });
        }

        private void ShowGameObject(SerializedProperty bindingInfo, ref Rect rect)
        {
            rect.y += LineHeight;

            SerializedProperty gameObjectProperty = bindingInfo.FindPropertyRelative(GameObjectProperty);
            EditorGUI.ObjectField(rect, gameObjectProperty, new GUIContent { text = StrTarget });
        }

        private void ShowSelectable(SerializedProperty bindingInfo, ref Rect rect)
        {
            rect.y += LineHeight;

            SerializedProperty selectableProperty = bindingInfo.FindPropertyRelative(SelectableProperty);
            EditorGUI.ObjectField(rect, selectableProperty, new GUIContent { text = StrTarget });
        }

        private void ShowAudioPlayer(SerializedProperty bindingInfo, ref Rect rect)
        {
            rect.y += LineHeight;

            SerializedProperty selectableProperty = bindingInfo.FindPropertyRelative(AudioPlayerProperty);
            EditorGUI.ObjectField(rect, selectableProperty, new GUIContent { text = StrTarget });
        }

        private void ShowToggle(SerializedProperty bindingInfo, ref Rect rect)
        {
            rect.y += LineHeight;

            SerializedProperty toggleProperty = bindingInfo.FindPropertyRelative(ToggleProperty);
            EditorGUI.ObjectField(rect, toggleProperty, new GUIContent { text = StrTarget });
        }

        private void ShowImage(SerializedProperty bindingInfo, ref Rect rect)
        {
            rect.y += LineHeight;

            SerializedProperty imageProperty = bindingInfo.FindPropertyRelative(ImageProperty);
            EditorGUI.ObjectField(rect, imageProperty, new GUIContent { text = StrTarget });
        }

        private void ShowSlider(SerializedProperty bindingInfo, ref Rect rect)
        {
            rect.y += LineHeight;

            SerializedProperty sliderProperty = bindingInfo.FindPropertyRelative(SliderProperty);
            EditorGUI.ObjectField(rect, sliderProperty, new GUIContent { text = StrTarget });
        }

        private void ShowText(SerializedProperty bindingInfo, ref Rect rect)
        {
            rect.y += LineHeight;

            SerializedProperty textProperty = bindingInfo.FindPropertyRelative(TextProperty);
            EditorGUI.ObjectField(rect, textProperty, new GUIContent { text = StrTarget });
        }
    }
}