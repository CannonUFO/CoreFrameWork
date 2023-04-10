using UnityEngine;
using UnityEditor;
using ible.Foundation.UI;

namespace ible.Foundation.UI.Editor
{
    [CustomEditor(typeof(BaseUIComponent), true)]
    public class BaseUIComponentEditor : UnityEditor.Editor
    {
        private bool _isFoldout = false;

        public override void OnInspectorGUI()
        {
            BaseUIComponent targetUI = (BaseUIComponent)target;

            _isFoldout = EditorGUILayout.Foldout(_isFoldout, "Link Tool");
            if (_isFoldout)
            {
                bool isCombineArray = targetUI.IsCombineArray;
                targetUI.IsCombineArray = EditorGUILayout.Toggle("IsCombineArray", targetUI.IsCombineArray);

                if (GUILayout.Button("Link"))
                {
                    ible.Foundation.UI.AutoGen.AutoUIGenerator.LinkReference(targetUI.gameObject, targetUI.GetType(), targetUI.IsCombineArray);
                }

                if (isCombineArray != targetUI.IsCombineArray)
                    EditorUtility.SetDirty(targetUI);

                EditorGUILayout.Space(20);
            }

            DrawDefaultInspector();
        }
    }
}
