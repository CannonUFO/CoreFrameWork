using ible.Foundation;
using ible.Foundation.Scene;
using ible.Foundation.UI;
using ible.GameModule.Scene;
using System.Collections;
using Debug = ible.Foundation.Utility.Debug;
using ible.Foundation.Atlas;
using System.Collections.Generic;
using ible.Foundation.Utility;

namespace ible.GameModule
{
    public class GameAppImp : GameApp<GameAppImp>
    {
        protected override void InitOnce()
        {
            Debug.Log("3.GameAppImp.InitOnce");
            
#if DEBUG ||  UNITY_EDITOR
            InitDebugUI();
#endif
        }

        private void InitDebugUI()
        {
            
        }
        protected override IEnumerator DoInitialize()
        {
            Debug.Log("4.GameAppImp.DoInitialize");
            yield return GameConfig.Initialize();
            yield return SceneManager.Instance.StartFirstScene();
            AtlasManager.Instance.LoadAtlasAsync(PathUtility.PreloadAtlasPath);

            OnInitializeComplete();
        }

        private void OnInitializeComplete()
        {
            Debug.Init(GameConfig.Instance.ShowLog); 

            initialized = true;
            Debug.Log("5.GameAppImp.DoInitialize End");
        }

        public override void RestartGame()
        {
            if (UIManager.HasInstance)
                UIManager.Instance.HideAll();

            //to do re login
            if (SceneManager.HasInstance)
                SceneManager.Instance.LoadScene(SceneName.InitialScene);
        }

        protected override void OnSingletonDestroy()
        {
            GameConfig.Instance.Destroy();
        }

        private void Update()
        {
            _eventSystem?.ExecuteProcess();
        }
    }
}
