using System.Collections;
using UnityEngine;
using System.Globalization;
using ible.Foundation.Utility;
using System.Collections.Generic;
using Debug = ible.Foundation.Utility.Debug;

namespace ible.Foundation
{
    /// <summary>
    /// GameApp Core Module，提供使用與抽象型別
    /// </summary>
    public abstract class GameApp<T> : Singleton<T> where T : MonoBehaviour
    {
        public const string DefaultSystemManager = "DefaultSystemManager";
        private List<SystemManager> _systemManagers = new List<SystemManager>();
        public EventSystem.EventSystem EventSystem { get { return _eventSystem; } }
        protected readonly EventSystem.EventSystem _eventSystem = new EventSystem.EventSystem();
        /// <summary>
        /// 是否正在初始化
        /// </summary>
        public bool IsInitializing { get; protected set; }
        private bool _initOnce;
        public bool IsDirectExitGame { get; set; } = false;

        public SystemManager FirstSystemManager
        {
            get 
            {
                if(_systemManagers.Count > 0)
                    return _systemManagers[0]; 
                return null;
            }
        }

        /// <summary>
        /// 初始化 System Manager
        /// </summary>
        public override void Initialize()
        {
            Debug.Log("2.GameApp.Initialize");
            if (!IsInitializing)
            {
                initialized = false;
                IsInitializing = true;

                CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
                Screen.sleepTimeout = SleepTimeout.NeverSleep;
            }
            if (!_initOnce)
            {
                _initOnce = true;
                InitOnce();
            }
            StartCoroutine(DoInitialize());
        }

        /// <summary>
        /// 單次初始化
        /// </summary>
        protected abstract void InitOnce();

        /// <summary>
        /// 初始化coroutine流程
        /// </summary>
        /// <returns></returns>
        protected abstract IEnumerator DoInitialize();


        public abstract void RestartGame();

        public SystemManager AddSystemManager(string name = null)
        {

            var ret = new SystemManager(name);
            _systemManagers.Add(ret);
            return ret;
        }
    }
}