﻿//using System.IO;
//using System.Collections.Generic;
//using System.Text.RegularExpressions;
//using UnityEngine;
//using DotLiquid;
//using ible.Editor.Utility;

//namespace ible.Foundation.UI.AutoGen.Editor
//{
//    public class GenerateListScrollRect : GenerateUICodeBase
//    {
//        private static readonly string s_uiCodeTemplate = @"// <auto-generated>
//{% for namespace in Namespaces %}
//using {{namespace}};{% endfor %}

//namespace ible.GameModule.UI.{{Namespace}}
//{
//#if UNITY_EDITOR
//    [Foundation.UI.AutoGen.UINameAttribute(TypeName = {{TypeName}})]
//#endif
//    public class {{UIName}} : ListScrollRectBase<{{ContentType}}>
//    {
//    }
//}
//";

//        public static void GenerateUICode(AutoUIListScrollRectGenerator target)
//        {
//            AutoUIReference childReference = target.GetChildReference();
//            if (childReference != null)
//            {
//                if (!childReference.IsCorrect())
//                {
//                    return;
//                }

//                Hash hash = new Hash();
//                hash["Namespace"] = target.Namespace;
//                hash["UIName"] = target.UIName;
//                hash["TypeName"] = "\"" + target.UITypeName + "\"";
//                hash["ContentType"] = childReference.TypeName;

//                List<string> usingNamespace = new List<string>();
//                usingNamespace.Add("UnityEngine");
//                usingNamespace.Add("ibel.Foundation.UI");
//                if (!string.IsNullOrEmpty(childReference.Namespace))
//                {
//                    if (!usingNamespace.Contains(childReference.Namespace))
//                        usingNamespace.Add(childReference.Namespace);
//                }

//                hash["Namespaces"] = usingNamespace;

//                string folder = string.Format("Assets/CoreScripts/Public/GameModule/UI/{0}/", target.Namespace);
//                if (!Directory.Exists(folder))
//                {
//                    Directory.CreateDirectory(folder);
//                }

//                string uiFile = target.UIName + ".cs";
//                GenerateCode.Generate(folder + uiFile, s_uiCodeTemplate, hash);
//            }
//            else
//            {
//                Debug.LogError(string.Format("Can't Find ListScrollRect [{0}] Content Child Reference", target.gameObject.name), target.gameObject);
//                return;
//            }
//        }

//        public static void DeleteUICode(string nameSpace, string uiName)
//        {
//            string folder = string.Format("Assets/CoreScripts/Public/GameModule/UI/{0}/", nameSpace);
//            string uiFile = uiName + ".cs";
//            if (File.Exists(folder + uiFile))
//                File.Delete(folder + uiFile);
//        }
//    }
//}