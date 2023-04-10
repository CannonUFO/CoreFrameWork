namespace ible.Foundation.EventSystem.Property.PropertyObserve
{
    // 填完名稱記得在PropertyNameInfo下面填入對應的類別

    public enum PropertyKey
    {
        None,

        // Test
        IntProperty,

        IntProperty2,
        FloatProperty,
        HpPercentage,
        StringProperty,
        BoolProperty,
        TimeSpanProperty,
        DateTimeProperty,
        IntProperty3,
        IntProperty4,
        IntProperty5,
        IntProperty6,
    }

    public static partial class EnumConv
    {
        /// <summary>
        /// Gets the name of the enum.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string GetEnumName(this PropertyKey value)
        {
            return Utility.EnumCache<PropertyKey>.Instance.GetEnumName((int)value);
        }
    }
}