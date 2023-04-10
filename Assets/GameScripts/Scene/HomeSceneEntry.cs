using ible.Foundation.Scene;
using ible.Foundation.UI;
using System.Collections.Generic;
using UnityEngine;

namespace ible.GameScripts.Scene
{
    public class HomeSceneEntry : MonoBehaviour, IChangeSceneProcessor
    {
        public int PrepareProcess(bool isEntering, string from, string to, Dictionary<string, object> bundle)
        {
            return 100;
        }

        public int LoadingProcess(bool isEntering, string from, string to, Dictionary<string, object> bundle)
        {
            return 100;
        }

        public int InitProcess(bool isEntering, string from, string to, Dictionary<string, object> bundle)
        {
            return 100;
        }

        private void Start() 
        {
            UIManager.Instance.Show(UIName.BackpackUI);
        }
    }
}