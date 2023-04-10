using ible.Foundation.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ible.Foundation.UI
{
    /// <summary>
    /// UI載入器
    /// </summary>
    public class UILoader : IUILoader
    {
        private Dictionary<AsyncOperationHandle<GameObject>, KeyValuePair<UIData, Action<IBaseUIView, UIData>>> _callbackDataDict 
            = new Dictionary<AsyncOperationHandle<GameObject>, KeyValuePair<UIData, Action<IBaseUIView, UIData>>>();
        

        public void Load(UIData data, Action<IBaseUIView, UIData> OnLoadComplete)
        {
            var path = PathUtility.GetUIAddressablePath(data.Name.GetEnumName());
            var obj = Addressables.InstantiateAsync(path, Vector3.zero, Quaternion.identity);
            obj.Completed += OnLoadInstantiateComplete;
            if(_callbackDataDict.ContainsKey(obj))
                _callbackDataDict.Remove(obj);
            _callbackDataDict.Add(obj, new KeyValuePair<UIData, Action<IBaseUIView, UIData>>(data, OnLoadComplete));
        }

        private void OnLoadInstantiateComplete(AsyncOperationHandle<GameObject> obj)
        {
            if(obj.Result != null)
            {
                var uiRoot = obj.Result.GetComponent<IBaseUIView>();

                if (_callbackDataDict.TryGetValue(obj, out var pairData))
                {
                    pairData.Value.Invoke(uiRoot, pairData.Key);
                    _callbackDataDict.Remove(obj);
                }
            }
        }
    }
}