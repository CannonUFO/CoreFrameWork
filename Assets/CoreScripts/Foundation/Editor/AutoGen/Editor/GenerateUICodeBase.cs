using System;
using System.Collections.Generic;
using UnityEngine;
using DotLiquid;

namespace ible.Foundation.UI.AutoGen.Editor
{
    public class GenerateUICodeBase
    {
        protected static List<AutoUIUnitEvent> s_uiEventList = new List<AutoUIUnitEvent>();

        protected static bool GetAutoUIReferences(AutoUIGenerator target, ref List<AutoUIReference> referenceList)
        {
            AutoUIGenerator.GetAutoUIReferences(target.transform, ref referenceList);
            referenceList.Sort((x, y) =>
            {
                int typeCompare = string.Compare(x.TypeFullName, y.TypeFullName);
                if (typeCompare == 0)
                {
                    return string.Compare(x.VariableName, y.VariableName);
                }
                return typeCompare;
            });

            if (CheckVariableNameConflict(ref referenceList))
            {
                return false;
            }

            return true;
        }

        protected static bool GetReferenceHash(AutoUIGenerator target, List<AutoUIReference> referenceList,
                ref List<string> usingNamespace, ref List<string> registerList, ref List<Hash> functionList, ref List<Hash> components)
        {
            if (target.IsCombineArray)
            {
                List<AutoUIReferenceArrayElement> elementList = new List<AutoUIReferenceArrayElement>();
                AutoUIGenerator.GetAutoUIReferenceArray(ref referenceList, ref elementList);
                foreach (var element in elementList)
                {
                    Hash componentHash = new Hash();
                    componentHash["Type"] = element.TypeFullName + "[]";
                    componentHash["Name"] = element.LowerVariableName;
                    components.Add(componentHash);

                    foreach (var iter in element.ReferenceDict)
                    {
                        s_uiEventList.Clear();
                        if (GenerateEvents(iter.Value, ref s_uiEventList))
                        {
                            foreach (var uiEvent in s_uiEventList)
                            {
                                Hash functionHash = new Hash();
                                functionHash["Name"] = uiEvent.FunctionName;
                                functionHash["Params"] = uiEvent.FunctionParams;
                                functionHash["Return"] = uiEvent.FunctionReturn;
                                functionList.Add(functionHash);

                                string variableName = string.Format("{0}[{1}]", element.LowerVariableName, iter.Key);
                                string register = string.Format(uiEvent.RegisterEventFormat, variableName, uiEvent.FunctionName);
                                registerList.Add(register);
                            }
                        }
                    }
                }
            }

            bool skipGenerate = false;
            foreach (AutoUIReference reference in referenceList)
            {
                if (!reference.IsCorrect())
                {
                    skipGenerate = true;
                    continue;
                }

                if (skipGenerate)
                    continue;

                Hash componentHash = new Hash();
                componentHash["Type"] = reference.TypeName;
                componentHash["Name"] = reference.LowerVariableName;
                components.Add(componentHash);

                if (!string.IsNullOrEmpty(reference.Namespace))
                {
                    if (!usingNamespace.Contains(reference.Namespace))
                        usingNamespace.Add(reference.Namespace);
                }

                s_uiEventList.Clear();
                if (GenerateEvents(reference, ref s_uiEventList))
                {
                    foreach (var uiEvent in s_uiEventList)
                    {
                        Hash functionHash = new Hash();
                        functionHash["Name"] = uiEvent.FunctionName;
                        functionHash["Params"] = uiEvent.FunctionParams;
                        functionHash["Return"] = uiEvent.FunctionReturn;
                        functionList.Add(functionHash);

                        registerList.Add(uiEvent.RegisterEvent);
                    }
                }
            }

            if (skipGenerate)
            {
                return false;
            }

            return true;
        }

        protected static bool CheckVariableNameConflict(ref List<AutoUIReference> referenceList)
        {
            bool isConflict = false;
            for (int i = 0, max = referenceList.Count; i < max; i++)
            {
                AutoUIReference curRef = referenceList[i];
                AutoUIReference conflict = referenceList.Find(x => x != curRef && x.LowerVariableName == curRef.LowerVariableName);
                if (conflict)
                {
                    Debug.LogError(string.Format("VariableName [{0}] Conflict!!!", curRef.LowerVariableName), curRef.gameObject);

                    isConflict = true;
                    continue;
                }
            }
            return isConflict;
        }

        protected static bool GenerateEvents(AutoUIReference reference, ref List<AutoUIUnitEvent> eventList)
        {
            //if (typeof(UIButton).IsAssignableFrom(reference.TargetType))
            //{
            //    AutoUIUnitEvent evt = new AutoUIUnitEvent();
            //    evt.FunctionName = string.Format("OnClick{0}", reference.UpperVariableName);
            //    evt.FunctionParams = string.Empty;
            //    evt.FunctionReturn = "void";
            //    evt.RegisterEventFormat = "{0}.OnClick.OnTrigger.Event.AddListener({1});";
            //    evt.RegisterEvent = string.Format(evt.RegisterEventFormat, reference.LowerVariableName, evt.FunctionName);
            //    eventList.Add(evt);

            //    return true;
            //}

            if (typeof(UnityEngine.UI.Button).IsAssignableFrom(reference.TargetType))
            {
                AutoUIUnitEvent evt = new AutoUIUnitEvent();
                evt.FunctionName = string.Format("OnClick{0}", reference.UpperVariableName);
                evt.FunctionParams = string.Empty;
                evt.FunctionReturn = "void";
                evt.RegisterEventFormat = "{0}.onClick.AddListener({1});";
                evt.RegisterEvent = string.Format(evt.RegisterEventFormat, reference.LowerVariableName, evt.FunctionName);
                eventList.Add(evt);
                return true;
            }

            if (typeof(UnityEngine.UI.Toggle).IsAssignableFrom(reference.TargetType))
            {
                AutoUIUnitEvent evt = new AutoUIUnitEvent();
                evt.FunctionName = string.Format("On{0}Changed", reference.UpperVariableName);
                evt.FunctionParams = "bool isOn";
                evt.FunctionReturn = "void";
                evt.RegisterEventFormat = "{0}.onValueChanged.AddListener({1});";
                evt.RegisterEvent = string.Format(evt.RegisterEventFormat, reference.LowerVariableName, evt.FunctionName);
                eventList.Add(evt);
                return true;
            }

            if (typeof(UnityEngine.UI.Slider).IsAssignableFrom(reference.TargetType))
            {
                AutoUIUnitEvent evt = new AutoUIUnitEvent();
                evt.FunctionName = string.Format("On{0}Changed", reference.UpperVariableName);
                evt.FunctionParams = "float value";
                evt.FunctionReturn = "void";
                evt.RegisterEventFormat = "{0}.onValueChanged.AddListener({1});";
                evt.RegisterEvent = string.Format(evt.RegisterEventFormat, reference.LowerVariableName, evt.FunctionName);
                eventList.Add(evt);
                return true;
            }

            if (typeof(UnityEngine.UI.Scrollbar).IsAssignableFrom(reference.TargetType))
            {
                AutoUIUnitEvent evt = new AutoUIUnitEvent();
                evt.FunctionName = string.Format("On{0}Changed", reference.UpperVariableName);
                evt.FunctionParams = "float value";
                evt.FunctionReturn = "void";
                evt.RegisterEventFormat = "{0}.onValueChanged.AddListener({1});";
                evt.RegisterEvent = string.Format(evt.RegisterEventFormat, reference.LowerVariableName, evt.FunctionName);
                eventList.Add(evt);
                return true;
            }

            if (typeof(UnityEngine.UI.Dropdown).IsAssignableFrom(reference.TargetType))
            {
                AutoUIUnitEvent evt = new AutoUIUnitEvent();
                evt.FunctionName = string.Format("On{0}Changed", reference.UpperVariableName);
                evt.FunctionParams = "int index";
                evt.FunctionReturn = "void";
                evt.RegisterEventFormat = "{0}.onValueChanged.AddListener({1});";
                evt.RegisterEvent = string.Format(evt.RegisterEventFormat, reference.LowerVariableName, evt.FunctionName);
                eventList.Add(evt);
                return true;
            }

            if (typeof(UnityEngine.UI.ScrollRect).IsAssignableFrom(reference.TargetType))
            {
                AutoUIUnitEvent evt = new AutoUIUnitEvent();
                evt.FunctionName = string.Format("On{0}Changed", reference.UpperVariableName);
                evt.FunctionParams = "UnityEngine.Vector2 pos";
                evt.FunctionReturn = "void";
                evt.RegisterEventFormat = "{0}.onValueChanged.AddListener({1});";
                evt.RegisterEvent = string.Format(evt.RegisterEventFormat, reference.LowerVariableName, evt.FunctionName);
                eventList.Add(evt);
                return true;
            }

            if (typeof(UnityEngine.UI.InputField).IsAssignableFrom(reference.TargetType))
            {
                AutoUIUnitEvent evt = new AutoUIUnitEvent();
                evt.FunctionName = string.Format("On{0}Changed", reference.UpperVariableName);
                evt.FunctionParams = "string text";
                evt.FunctionReturn = "void";
                evt.RegisterEventFormat = "{0}.onValueChanged.AddListener({1});";
                evt.RegisterEvent = string.Format(evt.RegisterEventFormat, reference.LowerVariableName, evt.FunctionName);
                eventList.Add(evt);
                return true;
            }

            //if (typeof(TMPro.TMP_InputField).IsAssignableFrom(reference.TargetType))
            //{
            //    AutoUIUnitEvent evt = new AutoUIUnitEvent();
            //    evt.FunctionName = string.Format("On{0}Changed", reference.UpperVariableName);
            //    evt.FunctionParams = "string text";
            //    evt.FunctionReturn = "void";
            //    evt.RegisterEventFormat = "{0}.onValueChanged.AddListener({1});";
            //    evt.RegisterEvent = string.Format(evt.RegisterEventFormat, reference.LowerVariableName, evt.FunctionName);
            //    eventList.Add(evt);
            //    return true;
            //}

            //if (typeof(ibel.Foundation.UI.ListScrollRectBase).IsAssignableFrom(reference.TargetType))
            //{
            //    {
            //        AutoUIUnitEvent evt = new AutoUIUnitEvent();
            //        evt.FunctionName = string.Format("Get{0}ItemCount", reference.UpperVariableName);
            //        evt.FunctionParams = string.Empty;
            //        evt.FunctionReturn = "int";
            //        evt.RegisterEventFormat = "{0}.ItemCountEvent = {1};";
            //        evt.RegisterEvent = string.Format(evt.RegisterEventFormat, reference.LowerVariableName, evt.FunctionName);
            //        eventList.Add(evt);
            //    }

            //    {
            //        AutoUIUnitEvent evt = new AutoUIUnitEvent();
            //        evt.FunctionName = string.Format("Get{0}ItemType", reference.UpperVariableName);
            //        evt.FunctionParams = "int index";
            //        evt.FunctionReturn = "int";
            //        evt.RegisterEventFormat = "{0}.ItemTypeEvent = {1};";
            //        evt.RegisterEvent = string.Format(evt.RegisterEventFormat, reference.LowerVariableName, evt.FunctionName);
            //        eventList.Add(evt);
            //    }

            //    if (reference.TargetType.BaseType != null)
            //    {
            //        Type[] types = reference.TargetType.BaseType.GetGenericArguments();
            //        AutoUIUnitEvent evt = new AutoUIUnitEvent();
            //        evt.FunctionName = string.Format("Set{0}Item", reference.UpperVariableName);
            //        evt.FunctionParams = string.Format("int index, int itemType, {0} item", types[0].FullName);
            //        evt.FunctionReturn = "void";
            //        evt.RegisterEventFormat = "{0}.ItemShowEvent = {1};";
            //        evt.RegisterEvent = string.Format(evt.RegisterEventFormat, reference.LowerVariableName, evt.FunctionName);
            //        eventList.Add(evt);
            //    }

            //    return true;
            //}

            return false;
        }

        protected struct AutoUIUnitEvent
        {
            public string FunctionName;
            public string FunctionParams;
            public string FunctionReturn;

            public string RegisterEventFormat;
            public string RegisterEvent;
        }
    }
}
