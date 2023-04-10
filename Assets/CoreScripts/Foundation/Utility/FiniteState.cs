using System.Diagnostics;

namespace ible.Foundation.Utility
{
    /// <summary>
    ///     有限狀態機
    /// </summary>
    /// <typeparam name="T">狀態型別</typeparam>
    public class FiniteState<T>
    {
        private readonly object _lock = new object();

        /// <summary>
        ///     當前狀態
        /// </summary>
        private T _currentState;

        /// <summary>
        ///     初始狀態
        /// </summary>
        private T _initialState;

        /// <summary>
        ///     是否剛進入目前狀態
        /// </summary>
        private bool _isEntering;

        /// <summary>
        ///     下一個狀態
        /// </summary>
        private T _nextState;

        private T _prevState;

        /// <summary>
        ///     是否準備轉移
        /// </summary>
        private bool _toTransit;

        /// <summary>
        ///     轉移時間
        /// </summary>
        private Stopwatch _transitTime = Stopwatch.StartNew();

        /// <summary>
        ///     當前狀態
        /// </summary>
        public T Current
        {
            get
            {
                lock (_lock)
                {
                    return _currentState;
                }
            }
        }

        public T Previous
        {
            get
            {
                lock (_lock)
                {
                    return _prevState;
                }
            }
        }

        /// <summary>
        ///     在這次 Tick() 是否為剛剛進入目前的狀態，可用來引發進入事件。
        /// </summary>
        public bool Entering
        {
            get
            {
                lock (_lock)
                {
                    return _isEntering;
                }
            }
        }

        /// <summary>
        ///     從進入這個狀態開始到現在過了多少秒。
        /// </summary>
        public double Elapsed
        {
            get
            {
                lock (_lock)
                {
                    return _transitTime.Elapsed.TotalSeconds;
                }
            }
        }

        /// <summary>
        ///     建構式
        /// </summary>
        /// <param name="initialState">初始狀態</param>
        public FiniteState(T initialState)
        {
            _initialState = initialState;
            Init();
        }

        /// <summary>
        ///     要求轉移到新狀態，實際轉移會在下一次呼叫 Tick() 時執行
        /// </summary>
        /// <param name="newState">要轉移過去的新狀態</param>
        public void Transit(T newState)
        {
            lock (_lock)
            {
                _nextState = newState;
                _toTransit = true;
            }
        }

        /// <summary>
        ///     通常被 timer 或 main loop 所呼叫, 表示進入下一個狀態
        /// </summary>
        /// <example>
        ///     <code><![CDATA[
        ///      switch ( _state.Tick() )
        ///      {
        ///      case STATE_INITIAL:
        ///          ....
        ///
        ///  ]]></code>
        /// </example>
        /// <returns>目前狀態</returns>
        public T Tick()
        {
            lock (_lock)
            {
                if (_toTransit)
                {
                    _transitTime.Reset();
                    _transitTime.Start();
                    _toTransit = false;
                    _prevState = _currentState;
                    _currentState = _nextState;
                    _isEntering = true;
                }
                else
                {
                    _isEntering = false;
                }

                return _currentState;
            }
        }

        /// <summary>
        ///     初始有限狀態機
        /// </summary>
        public void Init()
        {
            _nextState = _initialState;
            _prevState = _initialState;
            _transitTime.Reset();
            _transitTime.Start();
            _toTransit = true;
            _currentState = _initialState;
            _isEntering = false;
        }
    }
}