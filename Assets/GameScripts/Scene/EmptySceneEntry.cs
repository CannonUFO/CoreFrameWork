using ible.Foundation.Scene;
using System.Collections.Generic;
using UnityEngine;

namespace ible.GameScripts.Scene
{
    public class EmptySceneEntry : MonoBehaviour, IChangeSceneProcessor
    {
        public int InitProcess(bool isEntering, string from, string to, Dictionary<string, object> bundle)
        {
            return 100;
        }

        public int LoadingProcess(bool isEntering, string from, string to, Dictionary<string, object> bundle)
        {
            return 100;
        }

        public int PrepareProcess(bool isEntering, string from, string to, Dictionary<string, object> bundle)
        {
            return 100;
        }
    }
}