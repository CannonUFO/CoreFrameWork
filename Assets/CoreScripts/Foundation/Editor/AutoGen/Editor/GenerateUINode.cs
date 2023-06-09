﻿using System.IO;
using System.Collections.Generic;
using DotLiquid;
using ible.Foundation;
using ible.Editor.Utility;
using ible.Foundation.UI;
using ible.GameModule.UI;

namespace ible.Foundation.UI.AutoGen.Editor
{
    public class GenerateUINode : GenerateUICodeBase
    {
        private static readonly string s_refCodeTemplate = @"// <auto-generated>
{% for namespace in Namespaces %}
using {{namespace}};{% endfor %}

namespace ible.GameModule.UI.{{Namespace}}
{
    public abstract class {{UIName}}Template : {{ParentClass}}
    {
        {% for component in Components %}[SerializeField]
        protected {{component.Type}} {{component.Name}};
        {% endfor %}
        protected {% if IsRoot %}override{% else %}virtual{% endif %} void Awake()
        {   {% if IsRoot %}
            base.Awake();{% endif %}{% for register in Registers %}
            {{register}}{% endfor %}
        }
        {% for function in Functions %}
        protected abstract {{function.Return}} {{function.Name}}({{function.Params}});{% endfor %}
    }
}
";

        private static readonly string s_uiCodeTemplate = @"{% for namespace in Namespaces %}
using {{namespace}};{% endfor %}

namespace ible.GameModule.UI.{{Namespace}}
{
    {% if IsRoot %}public class {{UIName}}Data : UIData<{{UIName}}Data>
    {
        //public {{UIName}}Data Data { get; private set; }

        public {{UIName}}Data() : base(UIName.{{UIName}})
        {

        }

        public new static {{UIName}}Data Allocate(/*{{UIName}}Data data*/)
        {
            var ret = UIData<{{UIName}}Data>.Allocate();
            //ret.Data = data;
            return ret;
        }

        public override void Reset()
        {
            base.Reset();
            //Data = null;
        }
    }
    {% endif %}
#if UNITY_EDITOR
    [ible.Foundation.UI.AutoGen.UINameAttribute(TypeName = {{TypeName}})]
#endif
    public class {{UIName}} : {{UIName}}Template
    {
        {% for component in Components %}//protected {{component.Type}} {{component.Name}};
        {% endfor %}
        {% if IsRoot %}protected override void OnShow()
        {
            base.OnShow();

            {{UIName}}Data customData = Data as {{UIName}}Data;
        }{% endif %}
        {% for function in Functions %}
        protected override {{function.Return}} {{function.Name}}({{function.Params}})
        {
            throw new System.NotImplementedException();
        }{% endfor %}
    }
}
";

        private static readonly string s_editorCodeTemplate = @"// <auto-generated>
using UnityEngine;
using UnityEditor;
using ible.Foundation.UI.AutoGen;
using ible.Foundation.UI.AutoGen.Editor;

namespace ible.GameModule.UI.{{Namespace}}
{
    [CustomEditor(typeof({{UIName}}Template), true)]
    public class {{UIName}}TemplateEditor : UITemplateEditor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button({{StringSignal}}Restore{{StringSignal}}))
            {
                {{UIName}}Template targetUITemplate = ({{UIName}}Template)target;
                {% for reference in References %}
                RestoreAutoUIReference(targetUITemplate.transform, {{reference.Path}}, {{reference.VariableName}}, {{reference.TypeFullName}});
                {% endfor %}
                AutoUIGenerator generator = RestoreAutoUIGenerator(targetUITemplate.transform, {{StringSignal}}{{UIName}}{{StringSignal}}, {{StringSignal}}{{Namespace}}{{StringSignal}}, {{IsCombineArray}});
                AutoUIGenerator.LinkReference(generator);
            }

            DrawDefaultInspector();
        }
    }
}
";

        public static void GenerateUICode(AutoUIGenerator target, bool isRoot)
        {
            Hash hash = new Hash();
            hash["Namespace"] = target.Namespace;
            hash["UIName"] = target.UIName;
            hash["StringSignal"] = "\"";
            hash["TypeName"] = "\"" + target.UITypeName + "\"";
            hash["IsRoot"] = isRoot;
            hash["ParentClass"] = isRoot ? typeof(BaseUIView).Name : typeof(BaseUIComponent).Name;

            List<string> usingNamespace = new List<string>();
            usingNamespace.Add("UnityEngine");
            usingNamespace.Add("ible.Foundation.UI");
            usingNamespace.Add("Debug = ible.Foundation.Utility.Debug");

            List<AutoUIReference> referenceList = new List<AutoUIReference>();
            if (!GetAutoUIReferences(target, ref referenceList))
            {
                return;
            }

            List<string> registerList = new List<string>();
            List<Hash> functionList = new List<Hash>();
            List<Hash> components = new List<Hash>();

            if (!GetReferenceHash(target, referenceList, ref usingNamespace, ref registerList, ref functionList, ref components))
            {
                return;
            }

            hash["Components"] = components;
            hash["Namespaces"] = usingNamespace;

            hash["Registers"] = registerList;
            hash["Functions"] = functionList;

            string folder = string.Format("Assets/GameScripts/UI/Implement/{0}/", target.Namespace);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            string templateFile = target.UIName + "Template.cs";
            GenerateCode.Generate(folder + templateFile, s_refCodeTemplate, hash);

            string uiFile = target.UIName + ".cs";
            GenerateCode.Generate(folder + uiFile, s_uiCodeTemplate, hash, true);

            if (!isRoot)
            {
                hash["IsCombineArray"] = target.IsCombineArray;

                List<Hash> refHashList = new List<Hash>(referenceList.Count);
                foreach (var reference in referenceList)
                {
                    Hash refHash = new Hash();
                    refHash["Path"] = "\"" + TransformPathUtility.GetTransformRelatedPath(target.transform, reference.transform, false) + "\"";
                    refHash["VariableName"] = "\"" + reference.VariableName + "\"";
                    refHash["TypeFullName"] = "\"" + reference.TypeFullName + "\"";

                    refHashList.Add(refHash);
                }
                hash["References"] = refHashList;

                string editorFolder = folder + "/Editor/";
                if (!Directory.Exists(editorFolder))
                {
                    Directory.CreateDirectory(editorFolder);
                }

                GenerateCode.Generate(editorFolder + target.UIName + "TemplateEditor.cs", s_editorCodeTemplate, hash);
            }
        }

        public static void DeleteUICode(string nameSpace, string uiName)
        {
            string folder = string.Format("Assets/GameScripts/UI/Implement/{0}/", nameSpace);
            string templateFile = uiName + "Template.cs";
            if (File.Exists(folder + templateFile))
                File.Delete(folder + templateFile);

            string uiFile = uiName + ".cs";
            if (File.Exists(folder + uiFile))
                File.Delete(folder + uiFile);

            string editorFolder = folder + "/Editor/";
            if (File.Exists(editorFolder + uiName + "TemplateEditor.cs"))
                File.Delete(editorFolder + uiName + "TemplateEditor.cs");
        }
    }
}
