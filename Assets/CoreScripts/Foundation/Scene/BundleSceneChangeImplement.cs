using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace ible.Foundation.Scene
{
    /// <summary>
    /// Addressables去作場景轉換
    /// </summary>
    public class BundleSceneChangeImplement : IChangeSceneImplement
    {
        private AsyncOperationHandle<SceneInstance> _handle;
        private SceneManager _manager;

        public BundleSceneChangeImplement(SceneManager manager)
        {
            _manager = manager;
        }

        public bool HasScene(string sceneName)
        {
            return _manager.SceneConfig.GetSceneInfoByName(sceneName) != null;
        }

        public bool HasScene(UnityEngine.SceneManagement.Scene scene)
        {
            return _manager.SceneConfig.GetSceneInfoByFileName(scene.name) != null;
        }

        public bool DoEmptyScene(string from, string to, Dictionary<string, object> bundle)
        {
            var sceneSetting = _manager.SceneConfig;

            if (!_handle.IsValid())
            {
                _handle = Addressables.LoadSceneAsync(sceneSetting.emptySceneRef.RuntimeKey); 
            }

            if (_handle.IsDone)
            {
                Addressables.Release(_handle);

                //var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                //var rootObjs = currentScene.GetRootGameObjects();
                //foreach (GameObject gameObject in rootObjs)
                //{
                //    var processors = gameObject.GetComponentsInChildren<IChangeSceneProcessor>();
                //    if (processors != null && processors.Length > 0)
                //    {
                //        foreach (IChangeSceneProcessor processor in processors)
                //        {
                //            _manager.AddChangeSceneProcessor(processor);
                //        }
                //    }
                //}
                return true;
            }

            return false;
        }

        public bool DoTargetScene(string from, string to, Dictionary<string, object> bundle)
        {
            if (!_handle.IsValid())
            {
                var sceneInfo = _manager.SceneConfig.GetSceneInfoByName(to);
                _handle = Addressables.LoadSceneAsync(
                    sceneInfo.sceneRef.RuntimeKey);
            }

            if (_handle.IsDone)
            {
                Addressables.Release(_handle);
                //var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                //var rootObjs = currentScene.GetRootGameObjects();
                //foreach (GameObject gameObject in rootObjs)
                //{
                //    var processors = gameObject.GetComponentsInChildren<IChangeSceneProcessor>();
                //    if (processors != null && processors.Length > 0)
                //    {
                //        foreach (IChangeSceneProcessor processor in processors)
                //        {
                //            _manager.AddChangeSceneProcessor(processor);
                //        }
                //    }
                //}
                return true;
            }

            return false;
        }

        public void AddChangeSceneProcessor()
        {
            var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var rootObjs = currentScene.GetRootGameObjects();
            foreach (GameObject gameObject in rootObjs)
            {
                var processors = gameObject.GetComponentsInChildren<IChangeSceneProcessor>();
                if (processors != null && processors.Length > 0)
                {
                    foreach (IChangeSceneProcessor processor in processors)
                    {
                        _manager.AddChangeSceneProcessor(processor);
                    }
                }
            }
        }
    }
}