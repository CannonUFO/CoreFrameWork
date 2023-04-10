using UnityEngine;

namespace ible.Foundation.Utility
{
    public static class GameObjectExtensionMethods
    {
        public static void SetActiveSafe(this GameObject go, bool active)
        {
            if (go == null)
            {
                return;
            }
            if (go.activeSelf != active)
                go.SetActive(active);
        }

        public static GameObject Instantiate(this GameObject self)
        {
            GameObject go = GameObject.Instantiate<GameObject>(self);
            go.transform.SetParent(self.transform.parent);
            go.transform.localPosition = self.transform.localPosition;
            go.transform.localRotation = self.transform.localRotation;
            go.transform.localScale = self.transform.localScale;
            return go;
        }

        public static string FullPath(this GameObject obj)
        {
            string path = "/" + obj.name;
            while (obj.transform.parent != null)
            {
                obj = obj.transform.parent.gameObject;
                path = "/" + obj.name + path;
            }
            return path;
        }
    }
}