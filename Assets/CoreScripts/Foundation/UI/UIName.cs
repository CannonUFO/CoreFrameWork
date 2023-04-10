namespace ible.Foundation.UI
{
    public enum UIName
    {
        Undefine,
        HeroUI,
        BackpackUI,
        ClanUI,
        MessageUI,
        ChangeSceneUI,
        TickerUI,
        SpriteAtlasUI,
    }

    public static partial class EnumConv
    {
        /// <summary>
        /// Gets the name of the enum.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string GetEnumName(this UIName value)
        {
            return Utility.EnumCache<UIName>.Instance.GetEnumName((int)value);
        }
    }
}