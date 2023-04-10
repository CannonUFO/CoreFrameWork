using ible.Foundation.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ible.Foundation.Scene
{
    [Serializable]
    public class SceneInfo
    {
        public string sceneName;
        public string sceneFileName;
        public AssetReference sceneRef;
        public AssetReference prefabRef;
    }

    [CreateAssetMenu(fileName = "SceneConfig", menuName = "Config/Create Scene Config")]
    public class SceneConfig : ScriptableConfig<SceneConfig>
    {
        public string emptySceneFileName;
        public AssetReference emptySceneRef;

        public bool loadFromBundle = true;
        public bool EnterEmptyScene;

        public List<SceneInfo> sceneInfos;

        public SceneInfo GetSceneInfoByName(string sceneName)
        {
            foreach (SceneInfo sceneInfo in sceneInfos)
            {
                if (sceneInfo.sceneName == sceneName)
                    return sceneInfo;
            }
            return null;
        }

        public SceneInfo GetSceneInfoByFileName(string fileName)
        {
            foreach (SceneInfo sceneInfo in sceneInfos)
            {
                if (sceneInfo.sceneFileName == fileName)
                    return sceneInfo;
            }
            return null;
        }
    }
}