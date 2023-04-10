using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ible.Foundation.Utility
{
    public abstract class ScriptableConfig<T> : ScriptableObject where T : ScriptableObject
    {
        private static bool s_isLoaded = false;

        private static T s_instance;
        public static T Instance
        { 
            get
            {
                return s_instance;
            }
        }

        public static IEnumerator Initialize()
        {
            yield return LoadInstance();
        }

        public static void ReInitialize()
        {
            s_isLoaded = false;
            LoadInstance();
        }

        public void Destroy() 
        {
            s_isLoaded = false;
            s_instance = null;
        }

        private static IEnumerator LoadInstance()
        {
            if (s_isLoaded)
                yield break;
            s_isLoaded = true;
            var path = PathUtility.GetConfigPath(typeof(T).Name);
            var handle = Addressables.LoadAssetAsync<ScriptableObject>(path);
            handle.Completed += OnLoadComplete;
            yield return handle;
        }

        private static void OnLoadComplete(AsyncOperationHandle<ScriptableObject> obj)
        {
            s_instance = obj.Result as T;
            Addressables.Release(obj);
        }
    }
}
