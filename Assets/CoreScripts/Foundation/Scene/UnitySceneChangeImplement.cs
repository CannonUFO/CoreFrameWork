using System.Collections.Generic;
using UnityEngine;

//using UnityEngine.AddressableAssets;
//using UnityEngine.ResourceManagement.AsyncOperations;
//using UnityEngine.ResourceManagement.ResourceProviders;

namespace ible.Foundation.Scene
{
    /// <summary>
    /// 使用UnityEngine.Scene去作場景轉換
    /// </summary>
    public class UnitySceneChangeImplement : IChangeSceneImplement
    {
        private AsyncOperation _asyncOperation;
        private SceneManager _manager;

        public UnitySceneChangeImplement(SceneManager manager)
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
            if (_asyncOperation == null)
            {
                _asyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(
                    _manager.SceneConfig.emptySceneFileName);
            }

            if (_asyncOperation.isDone)
            {
                _asyncOperation = null;
                return true;
            }

            return false;
        }

        public bool DoTargetScene(string from, string to, Dictionary<string, object> bundle)
        {
            if (_asyncOperation == null)
            {
                var sceneInfo = _manager.SceneConfig.GetSceneInfoByName(to);
                _asyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(
                    sceneInfo.sceneFileName);
            }

            if (_asyncOperation.isDone)
            {
                _asyncOperation = null;
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