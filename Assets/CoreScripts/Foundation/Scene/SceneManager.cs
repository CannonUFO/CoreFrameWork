//#define SCENE_CHANGE_LOG
using ible.Foundation.Utility;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ible.Foundation.Scene
{
    /// <summary>
    /// 場景轉換管理器
    /// </summary>
    [SingletonInit("Prefab/SceneManager")]
    public class SceneManager : Singleton<SceneManager>
    {
        public enum Status
        {
            None,
            Prepare,
            LoadingEmptyScene,
            EmptySceneProcess,
            LoadingTargetScene,
            InitTargetScene,
            Complete
        }

        public delegate void SceneChangeEvent(string from, string to, Dictionary<string, object> bundle);

        public class ChangeSceneListener
        {
            /// <summary>
            ///     開始轉換Scene
            /// </summary>
            public SceneChangeEvent OnStart { get; set; }

            /// <summary>
            ///     離開目前場景
            /// </summary>
            public SceneChangeEvent OnLeavingCurrentScene { get; set; }

            /// <summary>
            ///     進入轉場中間Scene
            /// </summary>
            public SceneChangeEvent OnEnterEmptyScene { get; set; }

            /// <summary>
            ///     進入目標Scene
            /// </summary>
            public SceneChangeEvent OnEnterTargetScene { get; set; }

            /// <summary>
            ///     轉換Scene結束
            /// </summary>
            public SceneChangeEvent OnFinish { get; set; }
        }

        private class ProcessorStateRecord
        {
            private IChangeSceneProcessor _processor;
            private int _prepareProgress;
            private int _loadingProgress;
            private int _initProgress;

            public IChangeSceneProcessor Processor => _processor;

            public ProcessorStateRecord(IChangeSceneProcessor processor)
            {
                _processor = processor;
                ResetState();
            }

            private void ResetState()
            {
                _prepareProgress = 0;
                _initProgress = 0;
                _loadingProgress = 0;
            }

            public int DoSceneChangePrepareProcess(bool isEntering, string from, string to, Dictionary<string, object> bundle)
            {
                if (_prepareProgress == 100)
                {
                    return _prepareProgress;
                }
                _prepareProgress = _processor?.PrepareProcess(isEntering, from, to, bundle) ?? 100;
                return _prepareProgress;
            }

            public int DoLoadingProcess(bool isEntering, string from, string to, Dictionary<string, object> bundle)
            {
                if (_loadingProgress == 100)
                {
                    return _loadingProgress;
                }
                _loadingProgress = _processor?.LoadingProcess(isEntering, from, to, bundle) ?? 100;
                return _loadingProgress;
            }

            public int DoTargetSceneInitProcess(bool isEntering, string from, string to, Dictionary<string, object> bundle)
            {
                if (_initProgress == 100)
                {
                    return _initProgress;
                }
                _initProgress = _processor?.InitProcess(isEntering, from, to, bundle) ?? 100;
                return _initProgress;
            }
        }

        //public bool needChangeToEmpty = true;

        private List<ChangeSceneListener> _listeners = new List<ChangeSceneListener>();
        private List<ProcessorStateRecord> _records = new List<ProcessorStateRecord>();
        private HashSet<IChangeSceneProcessor> _processors = new HashSet<IChangeSceneProcessor>();

        private string _lastSceneName;
        private string _currentSceneName;
        private string _targetSceneName;
        private Dictionary<string, object> _bundle;
        private FiniteState<Status> _status = new FiniteState<Status>(Status.InitTargetScene);

        private IChangeSceneImplement _changeSceneImplement;

        public Status SceneLoadingStatus => _status.Current;
        public SceneConfig SceneConfig
        {
            get
            {
                return _sceneConfig;
            }
        }

        private SceneConfig _sceneConfig;

        private List<ProcessorStateRecord> _tempList = new List<ProcessorStateRecord>();
        private List<ChangeSceneListener> _tempListeners = new List<ChangeSceneListener>();

        public string LastSceneName => _lastSceneName;
        public string CurrentSceneName => _currentSceneName;
        public string TargetSceneName => _targetSceneName;

        public bool IsChangingScene => _status.Current != Status.Complete && _status.Current != Status.None;

        public float PrepareProgress { get; private set; }
        public float LoadingProgress { get; private set; }
        public float InitProgress { get; private set; }

        public override void Initialize()
        {
            if (initialized)
                return;

            base.Initialize();
            //if (sceneSetting.loadFromBundle)
            //    _changeSceneImplement = new BundleSceneChangeImplement(this);
            //else
            _changeSceneImplement = new UnitySceneChangeImplement(this);

            
        }

        public IEnumerator StartFirstScene()
        {
            yield return LoadConfig();

            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (!_changeSceneImplement.HasScene(scene))
            {
                var setting = _sceneConfig.GetSceneInfoByFileName(scene.name);
                _targetSceneName = setting.sceneName;
            }
            else
            {
                //Debug.LogError("start from a unknown scene ???????");
            }
        }

        private IEnumerator LoadConfig()
        {
            if (_sceneConfig == null)
            {
                yield return SceneConfig.Initialize();
                _sceneConfig = SceneConfig.Instance;
            }
        }

        /// <summary>
        ///     要求轉換Scene
        /// </summary>
        /// <param name="sceneType">目標Scene名稱</param>
        /// <param name="bundle">轉換過程中遇傳給Processor或Listener的參數選項</param>
        /// <param name="force">強制轉場, 無視目前是否正在轉場過程</param>
        public bool LoadScene(string sceneType, Dictionary<string, object> bundle = null, bool force = false)
        {
            if (string.IsNullOrEmpty(sceneType))
            {
#if UNITY_EDITOR
                //Debug.LogError("if (string.IsNullOrEmpty(sceneType))");
#endif
                return false;
            }

            if (!force && _status.Current != Status.None && _status.Current != Status.Complete)
            {
#if DEBUG
                //Debug.LogErrorFormat("[SceneManager.ChangeScene]Fail -> status {0}", _status.Current);
#endif
                return false;
            }

            if (!_changeSceneImplement.HasScene(sceneType))
            {
#if DEBUG
                //Debug.LogError($"[SceneManager.ChangeScene]Fail {sceneType} not found");
#endif
                return false;
            }

#if DEBUG
            //Debug.Log($"[SceneManager.ChangeScene]Start Load Scene({sceneType})");
#endif
            _targetSceneName = sceneType;
            _bundle = bundle;
            _status.Transit(Status.Prepare);
            return true;
        }

        public void AddSceneChangeListener(ChangeSceneListener listener)
        {
            _listeners.Add(listener);
        }

        public bool RemoveSceneChangeListener(ChangeSceneListener listener)
        {
            return _listeners.Remove(listener);
        }

        internal void AddChangeSceneProcessor(IChangeSceneProcessor processor)
        {
            if (_processors.Add(processor))
            {
                _records.Add(new ProcessorStateRecord(processor));
            }
        }

        //internal bool RemoveChangeSceneProcessor(IChangeSceneProcessor processor)
        //{
        //    _processors.Remove(processor);
        //    for (var i = 0; i < _records.Count; i++)
        //    {
        //        if (_records[i].Processor == processor)
        //        {
        //            _records.RemoveAt(i);
        //            return true;
        //        }
        //    }
        //    return false;
        //}
        

        private void Update()
        {
            switch (_status.Tick())
            {
                case Status.None:
                    break;

                case Status.Prepare:
                {
                    if (_status.Entering)
                    {
                        PrepareProgress = 0f;

                        NotifyStartChangingScene();
                    }

                    if (SceneChangePrepareProcess(_status.Entering))
                    {
                        if (_sceneConfig.EnterEmptyScene)
                            _status.Transit(Status.LoadingEmptyScene);
                        else
                        {
                            NotifyLeavingCurrentScene();
                            _status.Transit(Status.LoadingTargetScene);
                        }
                    }
                    break;
                }
                case Status.LoadingEmptyScene:
                {
                    if (_status.Entering)
                    {
                        NotifyLeavingCurrentScene();
                    }

                    if (_changeSceneImplement.DoEmptyScene(_currentSceneName, _targetSceneName, _bundle))
                    {
                        
                        _status.Transit(Status.EmptySceneProcess);
                    }
                    break;
                }
                case Status.EmptySceneProcess:
                {
                    if (_status.Entering)
                    {
                        LoadingProgress = 0f;
                        _changeSceneImplement.AddChangeSceneProcessor();
                        NotifyEnterEmptyScene();
                    }

                    if (EmptrySceneProcess(_status.Entering))
                    {
                        _status.Transit(Status.LoadingTargetScene);
                    }
                    break;
                }
                case Status.LoadingTargetScene:
                {
                    if (_changeSceneImplement.DoTargetScene(_currentSceneName, _targetSceneName, _bundle))
                    {
                        
                        _status.Transit(Status.InitTargetScene);
                    }
                    break;
                }
                case Status.InitTargetScene:
                {
                    if (_status.Entering)
                    {
                        InitProgress = 0f;
                        _changeSceneImplement.AddChangeSceneProcessor();
                        NotifyEnterTargetScene();
                    }

                    if (TargetSceneInitProcess(_status.Entering))
                    {
                        _status.Transit(Status.Complete);
                    }
                    break;
                }
                case Status.Complete:
                {
                    if (_status.Entering)
                    {
                        NotifyChangeSceneFinished();
                        _lastSceneName = _currentSceneName;
                        _currentSceneName = _targetSceneName;
                    }
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool TargetSceneInitProcess(bool isEntering)
        {
            int isReadyCount = 0;
            _tempList.Clear();
            _tempList.AddRange(_records);

            int allProgress = 0;
            foreach (ProcessorStateRecord record in _tempList)
            {
                int progress = record.DoTargetSceneInitProcess(isEntering, _currentSceneName, _targetSceneName, _bundle);
                allProgress += progress;
                if (progress == 100)
                {
                    isReadyCount++;
                }
            }

            InitProgress = (float)allProgress / (float)(_tempList.Count * 100);

            return isReadyCount == _tempList.Count;
        }

        private bool SceneChangePrepareProcess(bool isEntering)
        {
            int isSuccessCount = 0;
            _tempList.Clear();
            _tempList.AddRange(_records);

            int allProgress = 0;
            foreach (ProcessorStateRecord record in _tempList)
            {
                int process = record.DoSceneChangePrepareProcess(isEntering, _currentSceneName, _targetSceneName, _bundle);
                allProgress += process;
                if (process == 100)
                {
                    isSuccessCount++;
                }
            }

            PrepareProgress = (float)allProgress / (float)(_tempList.Count * 100);

            return isSuccessCount == _tempList.Count;
        }

        private bool EmptrySceneProcess(bool isEntering)
        {
            int isReadyCount = 0;
            _tempList.Clear();
            _tempList.AddRange(_records);

            int allProgress = 0;
            foreach (ProcessorStateRecord record in _tempList)
            {
                int progress = record.DoLoadingProcess(isEntering, _currentSceneName, _targetSceneName, _bundle);
                allProgress += progress;
                if (progress == 100)
                {
                    isReadyCount++;
                }
            }

            LoadingProgress = (float)allProgress / (float)(_tempList.Count * 100);

            return isReadyCount == _tempList.Count;
        }

        private void NotifyLeavingCurrentScene()
        {
            _tempListeners.Clear();
            _tempListeners.AddRange(_listeners);

#if UNITY_EDITOR && SCENE_CHANGE_LOG
            Debug.Log($"Notify Leave Current Scene: {_currentSceneName} => {_targetSceneName}");
#endif

            foreach (ChangeSceneListener changeSceneListener in _tempListeners)
            {
                if (changeSceneListener.OnLeavingCurrentScene == null)
                    continue;
                changeSceneListener.OnLeavingCurrentScene(_currentSceneName, _targetSceneName, _bundle);
            }
        }

        private void NotifyEnterEmptyScene()
        {
            _tempListeners.Clear();
            _tempListeners.AddRange(_listeners);

#if UNITY_EDITOR && SCENE_CHANGE_LOG
            Debug.Log($"Notify Enter Empty Scene: {_currentSceneName} => {_targetSceneName}");
#endif
            foreach (ChangeSceneListener changeSceneListener in _tempListeners)
            {
                if (changeSceneListener.OnEnterEmptyScene == null)
                    continue;
                changeSceneListener.OnEnterEmptyScene(_currentSceneName, _targetSceneName, _bundle);
            }
        }

        private void NotifyEnterTargetScene()
        {
            _tempListeners.Clear();
            _tempListeners.AddRange(_listeners);

#if UNITY_EDITOR && SCENE_CHANGE_LOG
            Debug.Log($"Notify Enter Target Scene: {_currentSceneName} => {_targetSceneName}");
#endif

            foreach (ChangeSceneListener changeSceneListener in _tempListeners)
            {
                if (changeSceneListener.OnEnterTargetScene == null)
                    continue;
                changeSceneListener.OnEnterTargetScene(_currentSceneName, _targetSceneName, _bundle);
            }
        }

        private void NotifyChangeSceneFinished()
        {
            _tempListeners.Clear();
            _tempListeners.AddRange(_listeners);

#if UNITY_EDITOR && SCENE_CHANGE_LOG
            Debug.Log($"Notify Change Scene Finish: {_currentSceneName} => {_targetSceneName}");
#endif

            foreach (ChangeSceneListener changeSceneListener in _tempListeners)
            {
                if (changeSceneListener.OnFinish == null)
                    continue;
                changeSceneListener.OnFinish(_currentSceneName, _targetSceneName, _bundle);
            }
        }

        private void NotifyStartChangingScene()
        {
            _tempListeners.Clear();
            _tempListeners.AddRange(_listeners);

#if UNITY_EDITOR && SCENE_CHANGE_LOG
            Debug.Log($"Notify Start Change Scene: {_currentSceneName} => {_targetSceneName}");
#endif

            foreach (ChangeSceneListener listener in _tempListeners)
            {
                if (listener.OnStart == null)
                {
                    continue;
                }
                listener.OnStart(_currentSceneName, _targetSceneName, _bundle);
            }
        }

        protected override void OnSingletonDestroy()
        {
            _status.Init();
            _status.Transit(Status.InitTargetScene);
            _sceneConfig.Destroy();
            _changeSceneImplement = null;
        }
    }
}