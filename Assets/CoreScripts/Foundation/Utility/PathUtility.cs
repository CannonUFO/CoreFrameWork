
using System.Collections.Generic;
using System.Text;

namespace ible.Foundation.Utility
{
    public class PathUtility
    {
        private static StringBuilder s_stringBuilder = new StringBuilder();
        private static string s_uiRoot = "Assets/MainAssets/ArtSource/2D/UI/";
        private static string s_prefabExtension = ".prefab";
        private const string s_configRoot = "Assets/MainAssets/Configs/";
        private const string s_assetExtension = ".asset";

        public static readonly List<string> PreloadAtlasPath = new List<string>()
        {   "Assets/MainAssets/BundleAsset/SpriteAtlas/Dynamic_SpriteAtlas_Icon.spriteatlasv2"};
        public static string GetUIAddressablePath(string fileName)
        {
            return GetPath(s_uiRoot, fileName, s_prefabExtension);
        }

        public static string GetConfigPath(string fileName)
        {
            return GetPath(s_configRoot, fileName, s_assetExtension);
        }

        public const string PropertyFileName = "PropertySettings";
        public static string GetPropertyConfigPath()
        {
            return GetPath(s_configRoot, PropertyFileName, s_assetExtension);
        }

        public static string GetPath(string path, string fileName, string extension)
        {
            s_stringBuilder.Clear();
            s_stringBuilder.Append(path);
            s_stringBuilder.Append(fileName);
            s_stringBuilder.Append(extension);
            return s_stringBuilder.ToString();
        }
    }
}
