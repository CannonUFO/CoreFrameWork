using System;
using System.Collections.Generic;
using UnityEngine;

namespace ible.Foundation.Utility
{
    public class EnumCache<T> where T : struct, IConvertible, IFormattable
    {
        protected string[] _names;
        protected int[] _values;
        protected List<string> _nameMods;
        protected Dictionary<int, string> _enumMap = new Dictionary<int, string>();

        protected delegate string NameFinder(int value);

        protected NameFinder nameFinder;

        public const string UNDEFINED = "UNDEFINED";

        public struct EnumIndex
        {
            public string name;
            public int value;
        }

        private static EnumCache<T> s_instance;

        public static EnumCache<T> Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = new EnumCache<T>();
                }
                return s_instance;
            }
        }

        public void Merge(Type type, string[] replaceNames = null)
        {
#if UNITY_EDITOR
            Debug.Log($"Merge type {type}");
#endif
            init(type, true, replaceNames);
        }

        protected void init(string[] replaceNames = null)
        {
            init(typeof(T), false, replaceNames);
        }

        protected void init(Type type, bool merge = false, string[] replaceNames = null)
        {
            _names = replaceNames == null ? Enum.GetNames(type) : replaceNames;
            var values = Enum.GetValues(type);

            if (!merge)
            {
                _values = new int[values.Length];
                values.CopyTo(_values, 0);
                _enumMap.Clear();

                if (values.Length != _names.Length)
                {
#if DEBUG
                    Debug.LogWarningFormat("Enum Cache Name/Count {0}/{1} Count Mismatch! It will lead unpaired cache result!", _names.Length, values.Length);
#endif
                }
            }

            int matchIDs = 0;
            for (int i = 0; i < values.Length; ++i)
            {
                int value = (int)values.GetValue(i);

                if (i >= _values.Length) break;
                if (i >= _names.Length) break;

                if (!merge || !_enumMap.ContainsKey(value))
                    _enumMap[value] = _names[i];

                if (value == i && value == _values[i])
                {
                    matchIDs++;
                }
            }

            if (matchIDs == values.Length)
            {
                nameFinder = GetEnumNameByIndex;
#if UNITY_EDITOR
                Debug.Log("Use index string cache for Enum " + typeof(T).Name);
#endif
            }
            else
            {
                nameFinder = GetEnumNameByValue;
#if UNITY_EDITOR
                Debug.Log("Use map string cache for Enum " + typeof(T).Name);
#endif
            }
        }

        public EnumCache()
        {
            init(null);
        }

        public EnumCache(bool initDefault)
        {
            if (initDefault)
            {
                init(null);
            }
        }

        public EnumCache(string[] replaceNames)
        {
            init(replaceNames);
        }

        public int Count { get { return _values.Length; } }
        public int this[int index] { get { return _values[index]; } }

        public string GetEnumNameByIndex(int index)
        {
            // fast access, value = index
            try
            {
                return _names[index];
            }
            catch
            {
#if UNITY_EDITOR
                Debug.LogWarning("enum index value dosen't exist: " + index.ToString());
#endif
                return UNDEFINED;
            }
        }

        public string GetEnumNameByValue(int value)
        {
            string name;
            if (_enumMap.TryGetValue(value, out name))
            {
                return name;
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogWarning("enum value dosen't exist: " + value.ToString());
#endif
                return UNDEFINED;
            }
        }

        public bool TryGetEnumNameByValue(int value, out string name)
        {
            if (_enumMap.TryGetValue(value, out name))
            {
                return true;
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogWarning("enum value dosen't exist: " + value.ToString());
#endif
                name = UNDEFINED;
                return false;
            }
        }

        public string GetEnumName(int enumValue)
        {
            return nameFinder(enumValue);
        }

        public void ApplyEnumNameMods(System.Func<string, string> func)
        {
            if (_nameMods == null) _nameMods = new List<string>();

            _nameMods.Clear();
            for (int i = 0; i < _names.Length; ++i)
            {
                _nameMods.Add(func(_names[i]));
            }
        }

        public string GetEnumNameMods(int index)
        {
            return _nameMods[index];
        }
    }
}