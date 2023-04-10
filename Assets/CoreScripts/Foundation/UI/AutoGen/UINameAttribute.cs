#if UNITY_EDITOR

using System;
using System.Linq;
using UnityEngine;

namespace ible.Foundation.UI.AutoGen
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class UINameAttribute : Attribute
    {
        public string TypeName { get; set; }

        public static Type GetTargetType(string typeName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var currentType in assembly.GetTypes().Where(_ => typeof(MonoBehaviour).IsAssignableFrom(_)))
                {
                    var attributes = currentType.GetCustomAttributes(typeof(UINameAttribute), false);
                    if (attributes.Length > 0)
                    {
                        var targetAttribute = attributes.First() as UINameAttribute;

                        if (targetAttribute.TypeName == typeName)
                        {
                            return currentType;
                        }
                    }
                }
            }

            return null;
        }
    }
}

#endif