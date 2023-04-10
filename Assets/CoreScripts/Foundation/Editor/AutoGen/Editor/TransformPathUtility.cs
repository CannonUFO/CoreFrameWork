using UnityEngine;

namespace ible.Foundation.UI.AutoGen.Editor
{
    public class TransformPathUtility
    {
        public static string GetTransformPath(Transform tf, bool includeRoot = true)
        {
            return GetTransformRelatedPath(tf.root, tf, includeRoot);
        }

        public static string GetTransformRelatedPath(Transform parent, Transform tf, bool includeRoot = true)
        {
            if (tf.parent == null || tf == parent)
                return includeRoot ? tf.name : string.Empty;

            string path = "/" + tf.name;
            while (tf.parent != null && tf != parent)
            {
                tf = tf.parent;
                if (!includeRoot && (tf.parent == null || tf == parent))
                    break;
                path = "/" + tf.name + path;
            }
            return path.Substring(1, path.Length - 1);
        }

        public static bool IsAnyTransformPathConflict(Transform tf)
        {
            bool conflict = false;
            while (tf.parent != null)
            {
                foreach (Transform child in tf.parent)
                {
                    if (child != tf && child.name == tf.name)
                    {
                        conflict = true;
                        break;
                    }
                }

                if (conflict)
                {
                    break;
                }

                tf = tf.parent;
            }
            return conflict;
        }

        public static string GetPathDepth(Transform tf)
        {
            string depth = string.Empty;
            while (tf.parent != null)
            {
                int childCount = tf.parent.childCount;
                int number = childCount.ToString().Length;
                for (int i = 0; i < childCount; i++)
                {
                    if (tf.parent.GetChild(i) == tf)
                    {
                        depth = depth.Insert(0, i.ToString("D" + number.ToString()));
                        break;
                    }
                }

                tf = tf.parent;
            }
            return depth;
        }
    }
}
