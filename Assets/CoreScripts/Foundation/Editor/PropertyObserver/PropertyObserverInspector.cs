using System;
using ible.Foundation.EventSystem.Property.PropertyObserve;
using ible.Foundation.Utility;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Debug = ible.Foundation.Utility.Debug;

namespace ible.Editor.PropertyObserver
{
    using PropertyObserve = Foundation.EventSystem.Property.PropertyObserve;
    using PropertyObserver = Foundation.EventSystem.Property.PropertyObserve.PropertyObserver;

    [CustomEditor(typeof(PropertyObserver), true)]
    internal class PropertyObserverInspector : UnityEditor.Editor
    {
        private const string BindingInfoProperty = "bindingInfo";
        private const string PropertyType = "propertyType";
        private const string PropertyKey = "propertyKey";
        private const string HandleTypeValue = "handleType";

        private const string ImageProperty = "image";
        private const string ToggleProperty = "toggle";
        private const string SelectableProperty = "selectable";
        private const string TextProperty = "Text";
        private const string GameOBjectProperty = "gameObject";
        private const string SliderProperty = "slider";
        private const string TransformProperty = "transform";
        private const string RectTransformProperty = "rectTransform";
        private const string ToStringFormat = "toStringForamt";
        private const string IsUniversal = "isUniversal";
        private const string PostFix = "postFix";
        //private const string AudioPlayerProperty = "audioPlayerWwise";
        //private const string SetFloatValueProperty = "setFloat";

        private const string StrPropertyKey = "PropertyKey";
        private const string StrHandleType = "HandleType";

        private static readonly PropertyNameInfo PropertyKeyMapping = PropertyNameInfo.Instance;

        private GUIContent[] _propertyKeyOptions;

        private PropertySetting _setting;

        public override void OnInspectorGUI()
        {
            PropertyObserver observer = (PropertyObserver)target;
            serializedObject.Update();

            SerializedProperty bindingInfo = serializedObject.FindProperty(BindingInfoProperty);

            SerializedProperty propertyType = bindingInfo.FindPropertyRelative(PropertyType);
            SerializedProperty propertyKey = bindingInfo.FindPropertyRelative(PropertyKey);
            SerializedProperty handleTypeValue = bindingInfo.FindPropertyRelative(HandleTypeValue);

            SerializedProperty textProperty = bindingInfo.FindPropertyRelative(TextProperty);
            SerializedProperty imageProperty = bindingInfo.FindPropertyRelative(ImageProperty);
            SerializedProperty toggleProperty = bindingInfo.FindPropertyRelative(ToggleProperty);
            SerializedProperty gameObjectProperty = bindingInfo.FindPropertyRelative(GameOBjectProperty);
            SerializedProperty sliderProperty = bindingInfo.FindPropertyRelative(SliderProperty);
            SerializedProperty transformProperty = bindingInfo.FindPropertyRelative(TransformProperty);
            SerializedProperty rectTransformProperty = bindingInfo.FindPropertyRelative(RectTransformProperty);
            SerializedProperty tostringFormat = bindingInfo.FindPropertyRelative(ToStringFormat);
            SerializedProperty isUniversal = bindingInfo.FindPropertyRelative(IsUniversal);
            SerializedProperty postFix = bindingInfo.FindPropertyRelative(PostFix);
            SerializedProperty selectableProperty = bindingInfo.FindPropertyRelative(SelectableProperty);
            //SerializedProperty audioPlayerProperty = bindingInfo.FindPropertyRelative(AudioPlayerProperty);
            //SerializedProperty setFloatProperty = bindingInfo.FindPropertyRelative(SetFloatValueProperty);

            // 找出當前屬性名稱字串對應到的Index
            int propertyKeyIndex = 0;
            for (int i = 0; i < _propertyKeyOptions.Length; i++)
            {
                int lastIndexOfSlash = _propertyKeyOptions[i].text.LastIndexOf("/", StringComparison.Ordinal);
                if (lastIndexOfSlash >= 0
                   && _propertyKeyOptions[i].text.Substring(lastIndexOfSlash + 1) == propertyKey.stringValue)
                {
                    propertyKeyIndex = i;
                    break;
                }
                if (lastIndexOfSlash < 0
                   && _propertyKeyOptions[i].text == propertyKey.stringValue)
                {
                    propertyKeyIndex = i;
                    break;
                }
            }

            // 顯示屬性名稱選項
            propertyKeyIndex = EditorGUILayout.Popup(
                new GUIContent(StrPropertyKey), propertyKeyIndex, _propertyKeyOptions);

            string propertyKeyStr;
            int lastIndexOfSlashResult = _propertyKeyOptions[propertyKeyIndex].text.LastIndexOf("/",
                StringComparison.Ordinal);
            if (lastIndexOfSlashResult >= 0)
            {
                propertyKeyStr = _propertyKeyOptions[propertyKeyIndex].text.Substring(lastIndexOfSlashResult + 1);
            }
            else
            {
                propertyKeyStr = _propertyKeyOptions[propertyKeyIndex].text;
            }

            PropertyKey propertyKeyValue;
            GameUtility.GetCurrentEnumValueFromString(propertyKeyStr, out propertyKeyValue);
            propertyKey.stringValue = propertyKeyValue.ToString();

            // 從mapping中找出屬性名稱對應到的屬性類型
            PropertyType tempType;
            if (!PropertyKeyMapping.NameToTypeDic.TryGetValue(propertyKeyValue, out tempType))
            {
                Debug.LogErrorFormat("propertyKey( {0} ) doesn't set property type!! please check setting again!!!",
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
            ShowHandleTypePopup(handleTypeValue, handleTypeContents);

            // NOTE: 如果新增類型必須修改這邊的switch
            switch (tempType)
            {
                case PropertyObserve.PropertyType.None:
                {
                    break;
                }
                case PropertyObserve.PropertyType.Int:
                {
                    // 顯示處理方式選項
                    IntHandleType intHandleType;
                    GameUtility.GetCurrentEnumValueFromString(handleTypeValue.stringValue, out intHandleType);

                    // 根據當前選擇的處理方式, 檢查是否須存在需要的Component, 並填入對應的欄位
                    switch (intHandleType)
                    {
                        case IntHandleType.ToText:
                        {
                            CheckText(observer, textProperty);
                            EditorGUILayout.PropertyField(tostringFormat);
                            break;
                        }
                        case IntHandleType.AddCommas:
                        {
                            CheckText(observer, textProperty);
                            break;
                        }
                        case IntHandleType.BFormatOrAddCommans:
                        case IntHandleType.KmbFormat:
                        {
                            CheckText(observer, textProperty);
                            break;
                        }
                        case IntHandleType.RedHint:
                        {
                            EditorGUILayout.ObjectField(textProperty, new GUIContent("Text"));
                            EditorGUILayout.ObjectField(postFix, new GUIContent("Post Fix"));
                            EditorGUILayout.ObjectField(gameObjectProperty, new GUIContent("Root GameObject"));
                            break;
                        }
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    break;
                }
                case PropertyObserve.PropertyType.Long:
                {
                    // 顯示處理方式選項
                    LongHandleType intHandleType;
                    GameUtility.GetCurrentEnumValueFromString(handleTypeValue.stringValue, out intHandleType);

                    // 根據當前選擇的處理方式, 檢查是否須存在需要的Component, 並填入對應的欄位
                    switch (intHandleType)
                    {
                        case LongHandleType.ToText:
                        {
                            CheckText(observer, textProperty);
                            EditorGUILayout.PropertyField(tostringFormat);
                            break;
                        }
                        case LongHandleType.AddCommas:
                        {
                            CheckText(observer, textProperty);
                            break;
                        }
                        case LongHandleType.BFormatOrAddCommans:
                        case LongHandleType.KmbFormat:
                        {
                            CheckText(observer, textProperty);
                            break;
                        }
                        case LongHandleType.ToTimeTextRedHint:
                        {
                            EditorGUILayout.ObjectField(textProperty, new GUIContent("Text"));
                            EditorGUILayout.ObjectField(gameObjectProperty, new GUIContent("Root GameObject"));
                            break;
                        }
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
                }
                case PropertyObserve.PropertyType.Float:
                {
                    // 顯示處理方式選項
                    FloatHandleType floatHandleType;
                    GameUtility.GetCurrentEnumValueFromString(handleTypeValue.stringValue, out floatHandleType);

                    // 根據當前選擇的處理方式, 檢查是否須存在需要的Component, 並填入對應的欄位
                    switch (floatHandleType)
                    {
                        case FloatHandleType.ToText:
                        {
                            CheckText(observer, textProperty);
                            break;
                        }
                        case FloatHandleType.Floor:
                        {
                            CheckText(observer, textProperty);
                            break;
                        }
                        case FloatHandleType.Ceiling:
                        {
                            CheckText(observer, textProperty);
                            break;
                        }
                        case FloatHandleType.Round:
                        {
                            CheckText(observer, textProperty);
                            break;
                        }
                        case FloatHandleType.SliderPercentage:
                        {
                            CheckSlider(observer, sliderProperty);
                            break;
                        }
                        case FloatHandleType.ImageFillAmount:
                        {
                            CheckImage(observer, imageProperty);
                            break;
                        }
                        //case FloatHandleType.SetFloatValue:
                        //{
                        //    CheckSetFloatValueMono(observer, setFloatProperty);
                        //    break;
                        //}
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
                }
                case PropertyObserve.PropertyType.String:
                {
                    // 顯示處理方式選項
                    StringHandleType stringHandleType;
                    GameUtility.GetCurrentEnumValueFromString(handleTypeValue.stringValue, out stringHandleType);

                    // 根據當前選擇的處理方式, 檢查是否須存在需要的Component, 並填入對應的欄位
                    switch (stringHandleType)
                    {
                        case StringHandleType.ToText:
                        {
                            CheckText(observer, textProperty);
                            break;
                        }
                        case StringHandleType.AsImageSprite:
                        {
                            CheckImage(observer, imageProperty);
                            break;
                        }
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
                }
                case PropertyObserve.PropertyType.Bool:
                {
                    // 顯示處理方式選項
                    BoolHandleType boolHandleType;
                    GameUtility.GetCurrentEnumValueFromString(handleTypeValue.stringValue, out boolHandleType);

                    // 根據當前選擇的處理方式, 檢查是否須存在需要的Component, 並填入對應的欄位
                    switch (boolHandleType)
                    {
                        case BoolHandleType.ToText:
                        {
                            CheckText(observer, textProperty);
                            break;
                        }
                        case BoolHandleType.ToToggle:
                        {
                            CheckToggle(observer, toggleProperty);
                            break;
                        }
                        case BoolHandleType.Visible:
                        {
                            CheckGameObject(observer, gameObjectProperty);
                            break;
                        }
                        case BoolHandleType.IsInteractable:
                        {
                            CheckSelectable(observer, selectableProperty);
                            break;
                        }
                        //case BoolHandleType.IsPlayAudio:
                        //{
                        //    CheckAudio(observer, audioPlayerProperty);
                        //    break;
                        //}
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    break;
                }
                case PropertyObserve.PropertyType.TimeSpan:
                {
                    // 顯示處理方式選項
                    TimeSpanHandleType timeSpanHandleType;
                    GameUtility.GetCurrentEnumValueFromString(handleTypeValue.stringValue, out timeSpanHandleType);

                    // 根據當前選擇的處理方式, 檢查是否須存在需要的Component, 並填入對應的欄位
                    switch (timeSpanHandleType)
                    {
                        case TimeSpanHandleType.ToText:
                        {
                            CheckText(observer, textProperty);
                            break;
                        }
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    break;
                }
                case PropertyObserve.PropertyType.DateTime:
                {
                    // 顯示處理方式選項
                    DateTimeHandleType dateTimeHandleType;
                    GameUtility.GetCurrentEnumValueFromString(handleTypeValue.stringValue, out dateTimeHandleType);

                    // 根據當前選擇的處理方式, 檢查是否須存在需要的Component, 並填入對應的欄位
                    switch (dateTimeHandleType)
                    {
                        case DateTimeHandleType.ToText:
                        {
                            CheckText(observer, textProperty);
                            EditorGUILayout.PropertyField(isUniversal);
                            break;
                        }
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    break;
                }

                case PropertyObserve.PropertyType.Vector3:
                {
                    // 顯示處理方式選項
                    Vector3HandleType vector3HandleType;
                    GameUtility.GetCurrentEnumValueFromString(handleTypeValue.stringValue, out vector3HandleType);

                    // 根據當前選擇的處理方式, 檢查是否須存在需要的Component, 並填入對應的欄位
                    switch (vector3HandleType)
                    {
                        case Vector3HandleType.ToLocalPosition:
                        {
                            CheckTransform(observer, transformProperty);
                            break;
                        }
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
                }
                case PropertyObserve.PropertyType.Vector2:
                {
                    // 顯示處理方式選項
                    Vector2HandleType vector2HandleType;
                    GameUtility.GetCurrentEnumValueFromString(handleTypeValue.stringValue, out vector2HandleType);

                    // 根據當前選擇的處理方式, 檢查是否須存在需要的Component, 並填入對應的欄位
                    switch (vector2HandleType)
                    {
                        case Vector2HandleType.ToAnchorPosition:
                        {
                            CheckRectTransform(observer, rectTransformProperty);
                            break;
                        }
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
                }
                case PropertyObserve.PropertyType.Color:
                {
                    // 顯示處理方式選項
                    ColorHandleType colorHandleType;
                    GameUtility.GetCurrentEnumValueFromString(handleTypeValue.stringValue, out colorHandleType);

                    // 根據當前選擇的處理方式, 檢查是否須存在需要的Component, 並填入對應的欄位
                    switch (colorHandleType)
                    {
                        case ColorHandleType.ToImageColor:
                        {
                            CheckImage(observer, imageProperty);
                            break;
                        }
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
                }
                case PropertyObserve.PropertyType.Quaternion:
                {
                    // 顯示處理方式選項
                    QuaternionHandleType quaternionHandleType;
                    GameUtility.GetCurrentEnumValueFromString(handleTypeValue.stringValue, out quaternionHandleType);

                    // 根據當前選擇的處理方式, 檢查是否須存在需要的Component, 並填入對應的欄位
                    switch (quaternionHandleType)
                    {
                        case QuaternionHandleType.ToLocalRotation:
                        {
                            CheckTransform(observer, transformProperty);
                            break;
                        }
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
                }
                case PropertyObserve.PropertyType.ULong:
                {
                    // 顯示處理方式選項
                    ULongHandleType intHandleType;
                    GameUtility.GetCurrentEnumValueFromString(handleTypeValue.stringValue, out intHandleType);

                    // 根據當前選擇的處理方式, 檢查是否須存在需要的Component, 並填入對應的欄位
                    switch (intHandleType)
                    {
                        case ULongHandleType.ToText:
                        {
                            CheckText(observer, textProperty);
                            EditorGUILayout.PropertyField(tostringFormat);
                            break;
                        }
                        case ULongHandleType.AddCommas:
                        {
                            CheckText(observer, textProperty);
                            break;
                        }
                        case ULongHandleType.BFormatOrAddCommas:
                        case ULongHandleType.KmbFormat:
                        {
                            CheckText(observer, textProperty);
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
                            CheckText(observer, textProperty);
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

            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            _setting =
                AssetDatabase.LoadAssetAtPath<PropertySetting>(PathUtility.GetPropertyConfigPath());
            _propertyKeyOptions = new GUIContent[_setting.propertyInfos.Count];
            for (int i = 0; i < _propertyKeyOptions.Length; i++)
            {
                PropertyEditorInfo info = _setting.propertyInfos[i];
                if (!string.IsNullOrEmpty(info.groupName))
                {
                    _propertyKeyOptions[i] = new GUIContent(info.groupName + "/" + info.propertyKey, info.comment);
                }
                else
                {
                    _propertyKeyOptions[i] = new GUIContent(info.propertyKey, info.comment);
                }
            }
        }

        private void CheckRectTransform(PropertyObserver observer, SerializedProperty rectTransformProperty)
        {
            RectTransform rectTransform = observer.GetComponent<RectTransform>();

            if (rectTransform == null)
            {
                EditorGUILayout.HelpBox("You don't have RectTransform Component on the gameObject!",
                    MessageType.Warning);
                return;
            }

            rectTransformProperty.objectReferenceValue = rectTransform;
        }

        private void CheckTransform(PropertyObserver observer, SerializedProperty transformProperty)
        {
            Transform transform = observer.GetComponent<Transform>();

            if (transform == null)
            {
                EditorGUILayout.HelpBox("You don't have Transform Component on the gameObject!",
                    MessageType.Warning);
                return;
            }

            transformProperty.objectReferenceValue = transform;
        }

        private void CheckSlider(PropertyObserver observer, SerializedProperty sliderProperty)
        {
            Slider slider = observer.GetComponent<Slider>();

            if (slider == null)
            {
                EditorGUILayout.HelpBox("You don't have Slider Component on the gameObject!",
                    MessageType.Warning);
                return;
            }

            sliderProperty.objectReferenceValue = slider;
        }

        private void CheckGameObject(PropertyObserver observer, SerializedProperty gameObjectProperty)
        {
            GameObject gameObject = observer.gameObject;

            if (gameObject == null)
            {
                EditorGUILayout.HelpBox("You don't have  gameObject! wtf?????",
                    MessageType.Warning);
                return;
            }

            gameObjectProperty.objectReferenceValue = gameObject;
        }

        private void CheckSelectable(PropertyObserver observer, SerializedProperty selectableProperty)
        {
            Selectable selectable = observer.GetComponent<Selectable>();

            if (selectable == null)
            {
                EditorGUILayout.HelpBox("You don't have Selectable Component on the gameObject!",
                    MessageType.Warning);
                return;
            }

            selectableProperty.objectReferenceValue = selectable;
        }

        private void CheckAudio(PropertyObserver observer, SerializedProperty audioPlayerProperty)
        {
            //AudioPlayerWwise audioPlayer = observer.GetComponent<AudioPlayerWwise>();

            //if (audioPlayer == null)
            //{
            //    EditorGUILayout.HelpBox("You don't have AudioPlayerWwise Component on the gameObject!",
            //        MessageType.Warning);
            //    return;
            //}

            //audioPlayerProperty.objectReferenceValue = audioPlayer;
        }

        private void CheckToggle(PropertyObserver observer, SerializedProperty toggleProperty)
        {
            Toggle toggle = observer.GetComponent<Toggle>();

            if (toggle == null)
            {
                EditorGUILayout.HelpBox("You don't have Toggle Component on the gameObject!",
                    MessageType.Warning);
                return;
            }

            toggleProperty.objectReferenceValue = toggle;
        }

        private void CheckImage(PropertyObserver observer, SerializedProperty imageProperty)
        {
            Image image = observer.GetComponent<Image>();

            if (observer.GetComponent<Image>() == null)
            {
                EditorGUILayout.HelpBox("You don't have Image Component on the gameObject!",
                    MessageType.Warning);
                return;
            }

            imageProperty.objectReferenceValue = image;
        }

        //private void CheckSetFloatValueMono(PropertyObserver observer, SerializedProperty setFloatProperty)
        //{
        //    var setter = observer.GetComponent<SetFloatValue>();

        //    if (observer.GetComponent<SetFloatValue>() == null)
        //    {
        //        EditorGUILayout.HelpBox("You don't have SetFloatValue Component on the gameObject!",
        //            MessageType.Warning);
        //        return;
        //    }

        //    setFloatProperty.objectReferenceValue = setter;
        //}

        private void CheckText(PropertyObserver observer, SerializedProperty textProperty)
        {
            TextMeshProUGUI text = observer.GetComponent<TextMeshProUGUI>();

            if (text == null)
            {
                EditorGUILayout.HelpBox("You don't have Text Component on the gameObject!", MessageType.Warning);
                return;
            }

            if (text)
            {
                textProperty.objectReferenceValue = text;
            }

            GUI.enabled = false;
            EditorGUILayout.PropertyField(textProperty);
            GUI.enabled = true;
        }

        private void ShowHandleTypePopup(SerializedProperty handleTypeValue, GUIContent[] handleTypeContents)
        {
            int currentHandleTypeIndex = 0;
            for (int i = 0; i < handleTypeContents.Length; i++)
            {
                if (handleTypeContents[i].text == handleTypeValue.stringValue)
                {
                    currentHandleTypeIndex = i;
                }
            }
            currentHandleTypeIndex = EditorGUILayout.Popup(new GUIContent { text = StrHandleType }, currentHandleTypeIndex,
                handleTypeContents);
            handleTypeValue.stringValue = handleTypeContents[currentHandleTypeIndex].text;
        }
    }
}