using ible.Foundation.UI.Editor;
using UnityEngine;

namespace ible.Foundation.UI.AutoGen.Editor
{
    public class UITemplateEditor : BaseUIComponentEditor
    {
        protected AutoUIGenerator RestoreAutoUIGenerator(Transform tf, string uiName, string uiNamespace, bool isCombineArray)
        {
            AutoUIGenerator generator = tf.GetComponent<AutoUIGenerator>();
            if (generator == null)
            {
                generator = tf.gameObject.AddComponent<AutoUIGenerator>();
            }

            generator.UIName = uiName;
            generator.Namespace = uiNamespace;
            generator.IsCombineArray = isCombineArray;

            return generator;
        }

        protected void RestoreAutoUIReference(Transform tf, string path, string variableName, string typeFullName)
        {
            Transform targetTransform;
            if (string.IsNullOrEmpty(path))
            {
                targetTransform = tf;
            }
            else
            {
                targetTransform = tf.Find(path);
            }

            bool find = false;
            var references = targetTransform.GetComponents<AutoUIReference>();
            foreach (var reference in references)
            {
                if (reference.TypeFullName == typeFullName)
                {
                    reference.VariableName = variableName;
                    find = true;
                }

                if (reference.VariableName == variableName && reference.TypeFullName != typeFullName)
                {
                    DestroyImmediate(reference);
                }
            }

            if (!find)
            {
                var reference = targetTransform.gameObject.AddComponent<AutoUIReference>();
                reference.VariableName = variableName;
                reference.TypeFullName = typeFullName;
            }
        }
    }
}
