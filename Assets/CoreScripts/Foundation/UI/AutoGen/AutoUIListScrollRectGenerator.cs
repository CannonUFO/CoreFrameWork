//using System;
//using System.Collections.Generic;
//using UnityEngine;

//namespace ible.Foundation.UI.AutoGen
//{
//    [RequireComponent(typeof(ListScrollRect))]
//    public class AutoUIListScrollRectGenerator : AutoUIGenerator
//    {
//#if UNITY_EDITOR

//        public override bool IsContinueCollect(ref List<AutoUIReference> skipList)
//        {
//            skipList.Add(GetChildReference());
//            return true;
//        }

//        public AutoUIReference GetChildReference()
//        {
//            ListScrollRect listScrollRect = GetComponent<ListScrollRect>();
//            if (listScrollRect != null)
//            {
//                if (listScrollRect.content == null)
//                {
//                    Debug.LogError("ListScrollRect Content is None!!!");
//                }

//                if (listScrollRect.content.childCount != 1)
//                {
//                    Debug.LogError("ListScrollRect Content Child is Not Only One!!!");
//                }

//                if (listScrollRect.content != null && listScrollRect.content.childCount == 1)
//                {
//                    Transform item = listScrollRect.content.GetChild(0);
//                    AutoUIReference[] references = item.GetComponents<AutoUIReference>();
//                    if (references.Length == 1)
//                        return references[0];
//                    else
//                    {
//                        AutoUIGenerator generator = item.GetComponent<AutoUIGenerator>();
//                        if (generator != null)
//                        {
//                            Type generateType = UINameAttribute.GetTargetType(generator.UITypeName);
//                            foreach (var reference in references)
//                            {
//                                if (reference.TargetType == generateType)
//                                {
//                                    return reference;
//                                }
//                            }
//                        }
//                        else
//                            Debug.LogError("Can't Find Only One Reference");
//                    }
//                }
//            }

//            return null;
//        }

//        protected override void Reset()
//        {
//            base.Reset();

//            if (GetComponent<ListScrollRect>() == null)
//            {
//                Debug.LogError(string.Format("GameObject [{0}] Not Contain ListScrollRect !!!", gameObject.name), gameObject);

//                DestroyImmediate(this);
//                return;
//            }
//        }

//#endif
//    }
//}