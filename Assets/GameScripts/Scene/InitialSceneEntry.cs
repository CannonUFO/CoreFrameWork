using ible.Foundation.Scene;
using ible.Foundation.UI;
using ible.GameModule;
using ible.GameModule.UI.TickerUI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

namespace ible.GameScripts.Scene
{
    public class InitialSceneEntry : MonoBehaviour, IChangeSceneProcessor
    {
        private Dictionary<string, object> _bundle;

        public int PrepareProcess(bool isEntering, string from, string to, Dictionary<string, object> bundle)
        {
            if (isEntering) 
            {
                UIManager.Instance.HideAll();
                Debug.Log("InitialSceneEntry.PrepareProcess");
            }
            return 100;
        }

        public int LoadingProcess(bool isEntering, string from, string to, Dictionary<string, object> bundle)
        {
            if (isEntering)
            {
                Debug.Log("InitialSceneEntry.LoadingProcess");
            }
            return 100;
        }

        public int InitProcess(bool isEntering, string from, string to, Dictionary<string, object> bundle)
        {
            if(isEntering) 
            {
                Debug.Log("InitialSceneEntry.InitProcess");
            }
            
            return GameAppImp.Instance.Initialized?100:0;
        }

        
        private void Start()
        {
            SpriteAtlasManager.atlasRequested += OnAtlasRequest;
            Debug.Log("1.InitialSceneEntry.Start");
            var gameApp = GameAppImp.Instance;

            UIManager.Instance.HideAll();
            UIManager.Instance.Show(UIName.ChangeSceneUI);
            //UIManager.Instance.Show(UIName.TickerUI, TickerUIData.Allocate("test!"));
        }

        private void OnAtlasRequest(string arg1, Action<SpriteAtlas> arg2)
        {
            Debug.Log("InitialSceneEntry.OnAtlasRequest!");
        }
    }
}
