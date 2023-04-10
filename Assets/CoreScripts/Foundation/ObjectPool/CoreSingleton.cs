
namespace ible.Foundation.ObjectPool
{
    public abstract class CoreSingleton<T> where T : CoreSingleton<T>, new()
    {
        private static volatile T s_instance;
        private static object s_lock = new object();

        public static ObjectPool<T>.Factory Factory;

        protected bool initialized;
        public bool Initialized { get { return initialized; } }

        public static bool HasInstance
        {
            get
            {
                return s_instance != null;
            }
        }

        public static T Instance
        {
            get
            {
                if (s_instance == null)
                {
                    lock (s_lock)
                    {
                        if (Factory != null)
                        {
                            s_instance = Factory();
                        }
                        else
                        {
                            s_instance = new T();
                        }
                    }
                }
                return s_instance;
            }
            set
            {
                lock (s_lock)
                {
                    s_instance = value;
                }
            }
        }
    }
}
