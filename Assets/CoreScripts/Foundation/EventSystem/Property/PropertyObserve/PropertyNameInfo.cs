using System.Collections.Generic;
using UnityEngine;

namespace ible.Foundation.EventSystem.Property.PropertyObserve
{
    public class PropertyNameInfo
    {
        internal static readonly PropertyKeyComparer s_propertyKeyComparer = new PropertyKeyComparer();
        private static PropertyNameInfo s_instance;
        protected PropertyNameInfo()
        {
            NameToTypeDic = new Dictionary<PropertyKey, PropertyType>(s_propertyKeyComparer);

            // Test
            AddNameToTypeMapping(PropertyKey.IntProperty, PropertyType.Int);
            AddNameToTypeMapping(PropertyKey.IntProperty2, PropertyType.Int);
            AddNameToTypeMapping(PropertyKey.FloatProperty, PropertyType.Float);
            AddNameToTypeMapping(PropertyKey.HpPercentage, PropertyType.Float);
            AddNameToTypeMapping(PropertyKey.StringProperty, PropertyType.String);
            AddNameToTypeMapping(PropertyKey.BoolProperty, PropertyType.Bool);
            AddNameToTypeMapping(PropertyKey.TimeSpanProperty, PropertyType.TimeSpan);
            AddNameToTypeMapping(PropertyKey.DateTimeProperty, PropertyType.String);
            AddNameToTypeMapping(PropertyKey.IntProperty3, PropertyType.Quaternion);
            AddNameToTypeMapping(PropertyKey.IntProperty4, PropertyType.Vector2);
            AddNameToTypeMapping(PropertyKey.IntProperty5, PropertyType.Vector3);
            AddNameToTypeMapping(PropertyKey.IntProperty6, PropertyType.Color);
        }

        static public PropertyNameInfo Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = new PropertyNameInfo();
                }
                return s_instance;
            }
        }

        public Dictionary<PropertyKey, PropertyType> NameToTypeDic { get; private set; }
        private void AddNameToTypeMapping(PropertyKey key, PropertyType type)
        {
            if (NameToTypeDic.ContainsKey(key))
            {
                Debug.LogErrorFormat("propertyName( {0} ) existed in NameToType Map!!!! Please check setting!!!!", key);
                return;
            }

            NameToTypeDic.Add(key, type);
        }

        internal class PropertyKeyComparer : IEqualityComparer<PropertyKey>
        {
            public bool Equals(PropertyKey x, PropertyKey y)
            {
                return x == y;
            }

            public int GetHashCode(PropertyKey obj)
            {
                return (int)obj;
            }
        }
    }
}