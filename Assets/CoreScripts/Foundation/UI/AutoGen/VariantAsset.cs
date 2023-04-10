using System;
using System.Linq;
using UnityEngine;

namespace ible.Foundation.UI.AutoGen
{
    [CreateAssetMenu(menuName = "Config/Create VariantAsset")]
    public class VariantAsset : ScriptableObject
    {
        public string[] IgnoreNamespaceList;
        public string[] CustomNamespaceList;
        public string[] IgnoreGenerateNamespaceList;

        public string[] CustomFullNameList;

        public bool IsNeedGenerate(Type type)
        {
            if (type.Namespace == null)
                return false;
            return !IgnoreNamespaceList.Any(x => type.Namespace.StartsWith(x)) && !IgnoreGenerateNamespaceList.Any(x => type.Namespace.StartsWith(x));
        }

        public bool IsIgnoreGenerateNamespaceList(Type type)
        {
            if (type.Namespace == null)
                return true;
            return IgnoreGenerateNamespaceList.Any(x => type.Namespace.StartsWith(x));
        }

        public bool IsCustomNamespace(Type type)
        {
            bool isFullName = CustomFullNameList.Any(x => x == type.FullName);
            if (isFullName)
                return true;

            if (type.Namespace == null)
                return false;
            return CustomNamespaceList.Any(x => type.Namespace.StartsWith(x));
        }
    }
}