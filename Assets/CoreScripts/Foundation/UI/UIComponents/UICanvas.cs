using UnityEngine;

namespace ible.Foundation.UI
{
    [RequireComponent(typeof(Canvas))]
    public class UICanvas : MonoBehaviour
    {
        public string CanvasName;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}