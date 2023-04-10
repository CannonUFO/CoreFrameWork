using System;
using System.Reflection;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace ible.Foundation.Utility
{
    /// <summary>
    ///     注意 不行使用Awake和OnDestroy
    ///     改override OnAwake & OnSingletonDestroy
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static readonly string s_singletonUseKey = "Singleton Use";
        private static GameObject s_useRootGameObject;
        private static volatile T s_instance;

        private static object s_lock = new object();

        protected bool initialized;
        public bool Initialized { get { return initialized; } }

        public static bool HasInstance
        {
            get
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    return false;
                if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
                    return false; // 準備關閉play mode
#endif

                return s_instance != null && s_instance;
            }
        }

        public static T InitializeInstance()
        {
            if (s_instance == null)
            {
                s_instance = (T)FindObjectOfType(typeof(T));

                if (s_instance != null)
                {
                    return s_instance;
                }

                Stopwatch timer = Stopwatch.StartNew();
                var realType = typeof(T);
                string name = "(singleton) " + realType.Name;
                var initAttr = realType.GetCustomAttribute(typeof(SingletonInitAttribute)) as SingletonInitAttribute;
                if (initAttr != null)
                {
                    var prefab = Resources.Load<T>(initAttr.Path);
                    if (prefab == null)
                    {
                        CreateFromScript(name);
                    }
                    else
                    {
                        CreateFromPrefab(prefab, name);
                    }
                }
                else
                {
                    CreateFromScript(name);
                }

                (s_instance as Singleton<T>)?.Initialize(); // 確保return前已經Initialize, 不等到Awake
                if (timer.Elapsed.TotalSeconds > 0.1f)
                {
                    Debug.LogWarningFormat("{0} init spent too much time={1}",
                        name, timer.Elapsed.TotalSeconds);
                }
            }

            return s_instance;
        }

        public static T Instance
        {
            get
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    return null;
                if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
                    return null; // 準備關閉play mode
#endif
                if (s_instance == null)
                {
                    lock (s_lock)
                    {
                        return InitializeInstance();
                    }
                }
                return s_instance;
            }

            private set
            {
                lock (s_lock)
                {
                    s_instance = value;
                }
            }
        }

        private static GameObject UseRootGameObject
        {
            get
            {
                if (s_useRootGameObject == null)
                {
                    s_useRootGameObject = GameObject.Find(s_singletonUseKey);
                    if (s_useRootGameObject == null)
                    {
                        s_useRootGameObject = new GameObject(s_singletonUseKey);
                        DontDestroyOnLoad(s_useRootGameObject);
                    }
                }
                return s_useRootGameObject;
            }
        }

        private static void CreateFromPrefab(T prefab, string name)
        {
            s_instance = Instantiate(prefab, UseRootGameObject.transform);
            s_instance.gameObject.name = name;
        }

        private static void CreateFromScript(string name)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(UseRootGameObject.transform, false);
            s_instance = go.AddComponent<T>();
        }

        protected virtual void OnAwake()
        {
        }

        protected virtual void OnSingletonDestroy()
        {
        }

        protected virtual void OnDestroy()
        {
            lock (s_lock)
            {
                if (s_instance == this)
                {
                    OnSingletonDestroy();
                    s_instance = null;
                }
            }
        }

        private void Awake()
        {
            lock (s_lock)
            {
                if (s_instance == null || s_instance == this)
                {
                    s_instance = this as T;
                    //Initialize();
                    OnAwake();
                }
                else
                {
                    // s_instance != this
                    Destroy(gameObject);
                }
            }
        }

        public virtual void Initialize()
        {
            if (initialized)
                return;

            initialized = true;
            gameObject.name = "(singleton) " + typeof(T).Name;
            if (gameObject.transform.parent != UseRootGameObject.transform)
            {
                gameObject.transform.SetParent(UseRootGameObject.transform);
            }
        }
    }

    public class SingletonInitAttribute : Attribute
    {
        public string Path { get; private set; }

        public SingletonInitAttribute(string path)
        {
            Path = path;
        }
    }
}