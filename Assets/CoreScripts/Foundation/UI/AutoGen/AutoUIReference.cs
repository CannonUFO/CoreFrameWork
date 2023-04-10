using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ible.Foundation.UI.AutoGen
{
    public class AutoUIReferenceArrayElement
    {
        public string TypeFullName;
        public string VariableName;

        public Dictionary<int, AutoUIReference> ReferenceDict = new Dictionary<int, AutoUIReference>();

        public string LowerVariableName
        {
            get
            {
                return VariableName.Substring(0, 1).ToLower() + VariableName.Substring(1, VariableName.Length - 1) + "s";
            }
        }

        public string UpperVariableName
        {
            get
            {
                return VariableName.Substring(0, 1).ToUpper() + VariableName.Substring(1, VariableName.Length - 1) + "s";
            }
        }
    }

    public class AutoUIReference : MonoBehaviour
    {
#if UNITY_EDITOR
        public string VariableName;
        public string TypeFullName;

        public List<string> TypeFullNameList = new List<string>(5);
        public int SelectIndex = 0;

        public string LowerVariableName
        {
            get
            {
                if (VariableName.Length > 1)
                {
                    return VariableName.Substring(0, 1).ToLower() + VariableName.Substring(1, VariableName.Length - 1);
                }
                return VariableName.ToLower();
            }
        }

        public string UpperVariableName
        {
            get
            {
                if (VariableName.Length > 1)
                {
                    return VariableName.Substring(0, 1).ToUpper() + VariableName.Substring(1, VariableName.Length - 1);
                }
                return VariableName.ToUpper();
            }
        }

        public string TypeName
        {
            get
            {
                if (TargetType != null)
                {
                    return TargetType.Name;
                }
                return TypeFullName;
            }
        }

        public string Namespace
        {
            get
            {
                if (TargetType != null)
                {
                    return TargetType.Namespace;
                }
                return string.Empty;
            }
        }

        public Type TargetType
        {
            get
            {
                var list = GetComponents(typeof(Behaviour));
                for (int i = 0; i < list.Length; i++)
                {
                    if (list[i] == null)
                        continue;

                    Type type = list[i].GetType();
                    if (type.FullName == TypeFullName)
                        return type;
                }
                return null;
            }
        }

        public static void GetComponentTypeList(Transform target, ref List<Type> typeList)
        {
            var list = target.GetComponents<Behaviour>();
            for (int i = 0; i < list.Length; i++)
            {
                if (list[i] == null)
                    continue;

                Type type = list[i].GetType();
                if (type == null)
                    continue;

                if (AutoUISetting.IgnoreTypes.Contains(type))
                    continue;

                typeList.Add(type);
            }
        }

        public bool GetVariableNumber(out string name, out int number)
        {
            int count = 0;
            number = 0;
            bool hasNumber = false;
            while (count < VariableName.Length)
            {
                count++;
                string checkString = VariableName.Substring(VariableName.Length - count, count);
                int outNumber = 0;
                if (!int.TryParse(checkString, out outNumber))
                {
                    break;
                }

                number = outNumber;
                hasNumber = true;
            }

            if (!hasNumber || VariableName.Length == count + 1)
            {
                name = string.Empty;

                return false;
            }

            name = VariableName.Substring(0, VariableName.Length - count + 1);
            return true;
        }

        public bool IsCorrect()
        {
            if (string.IsNullOrEmpty(TypeFullName))
            {
                Debug.LogError(string.Format("GameObject [{0}] Type is Empty !!!", gameObject.name), gameObject);

                return false;
            }

            if (string.IsNullOrEmpty(VariableName))
            {
                Debug.LogError(string.Format("GameObject [{0}] VariableName is Empty !!!", gameObject.name), gameObject);

                return false;
            }

            if (!Regex.IsMatch(VariableName, @"^\w+$"))
            {
                Debug.LogError(string.Format("GameObject [{0}] VariableName [{1}] is Error Format !!!", gameObject.name, VariableName), gameObject);

                return false;
            }
            else if (Regex.IsMatch(VariableName.Substring(0, 1), "[0-9]"))
            {
                Debug.LogError(string.Format("GameObject [{0}] VariableName [{1}] is Error Format !!!", gameObject.name, VariableName), gameObject);

                return false;
            }

            if (TargetType == null)
            {
                Debug.LogError(string.Format("GameObject [{0}] Type [{1}] is NULL !!!", gameObject.name, TypeFullName), gameObject);

                return false;
            }

            return true;
        }

        public void SetTypeFullName(string typeFullName)
        {
            CollectTypeFullNameList();
            SelectIndex = TypeFullNameList.IndexOf(typeFullName);
            TypeFullName = typeFullName;
        }

        public void CollectTypeFullNameList()
        {
            TypeFullNameList.Clear();

            List<Type> typeList = new List<Type>();
            GetComponentTypeList(transform, ref typeList);

            foreach (Type type in typeList)
            {
                TypeFullNameList.Add(type.FullName);
            }
        }

        private void Reset()
        {
            List<AutoUIReference> componentList = new List<AutoUIReference>();
            GetComponents<AutoUIReference>(componentList);

            CollectTypeFullNameList();

            if (componentList.Count > TypeFullNameList.Count)
            {
                Debug.LogError(string.Format("GameObject [{0}] too much AutoUIReference !!!", gameObject.name), gameObject);

                DestroyImmediate(this);

                return;
            }

            string tempName = gameObject.name;
            AutoUIGenerator creator = GetComponent<AutoUIGenerator>();
            if (creator != null)
                tempName = creator.UIName;
            tempName = Regex.Replace(tempName, @"[^\w]", string.Empty);

            if (componentList.Count > 1)
            {
                while (true)
                {
                    if (componentList.Find(x => x != this && x.VariableName == tempName) == null)
                    {
                        break;
                    }

                    tempName = tempName.Insert(0, "_");
                }
            }
            VariableName = tempName;

            AutoUIGenerator generator = GetComponent<AutoUIGenerator>();
            if (generator != null)
            {
                Type type = UINameAttribute.GetTargetType(generator.UITypeName);
                if (type != null && GetComponent(type) != null)
                {
                    if (componentList.Find(x => x != this && x.TypeFullName == type.FullName) == null)
                    {
                        SelectIndex = TypeFullNameList.IndexOf(type.FullName);
                        TypeFullName = type.FullName;
                        return;
                    }
                }
            }

            for (int i = 0, max = TypeFullNameList.Count; i < max; i++)
            {
                string typeName = TypeFullNameList[i];

                if (componentList.Find(x => x != this && x.TypeFullName == typeName))
                    continue;

                int curIndex = AutoUISetting.PriorityTypes.FindIndex(x => x.FullName == typeName);
                int bestIndex = AutoUISetting.PriorityTypes.FindIndex(x => x.FullName == TypeFullNameList[SelectIndex]);
                if (curIndex != -1)
                {
                    if (bestIndex != -1)
                    {
                        if (curIndex < bestIndex)
                        {
                            SelectIndex = i;
                        }
                    }
                    else
                    {
                        SelectIndex = i;
                    }
                }
            }

            if (SelectIndex < TypeFullNameList.Count)
                TypeFullName = TypeFullNameList[SelectIndex];
        }

#endif
    }
}