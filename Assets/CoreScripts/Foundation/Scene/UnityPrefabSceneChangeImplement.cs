using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ible.Foundation.Scene
{
    public class UnityPrefabSceneChangeImplement : IChangeSceneImplement
    {
        //private GameObject _curEntry;
        private AsyncOperationHandle<GameObject>? _curHandler;

        private AsyncOperationHandle<GameObject>? _nextHandler;
        private SceneManager _manager;

        public UnityPrefabSceneChangeImplement(SceneManager manager)
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

        public bool DoEmptyScene(string @from, string to, Dictionary<string, object> bundle)
        {
            if (_nextHandler == null)
            {
                _nextHandler = Addressables.InstantiateAsync(_manager.SceneConfig.emptySceneFileName);
            }

            if (_nextHandler.Value.IsDone)
            {
                //_curEntry = _nextHandler.Value.Result;

                if (_curHandler.HasValue)
                    Addressables.ReleaseInstance(_curHandler.Value);
                _curHandler = _nextHandler;
                _nextHandler = null;
                return true;
            }

            return false;
        }

        public bool DoTargetScene(string @from, string to, Dictionary<string, object> bundle)
        {
            if (_nextHandler == null)
            {
                var sceneInfo = _manager.SceneConfig.GetSceneInfoByName(to);
                _nextHandler = Addressables.InstantiateAsync(sceneInfo.prefabRef);
            }

            if (_nextHandler.Value.IsDone)
            {
                //_curEntry = _nextHandler.Value.Result;

                

                if (_curHandler.HasValue)
                    Addressables.ReleaseInstance(_curHandler.Value);
                _curHandler = _nextHandler;
                _nextHandler = null;
                return true;
            }

            return false;
        }

        public void AddChangeSceneProcessor()
        {
            var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var rootObjs = currentScene.GetRootGameObjects();
            foreach (GameObject root in rootObjs)
            {
                var processors = root.GetComponentsInChildren<IChangeSceneProcessor>();
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