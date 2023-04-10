using System.Collections;
using System.Collections.Generic;

namespace ible.Foundation.Scene
{
    /// <summary>
    /// 實作轉場的介面
    /// </summary>
    public interface IChangeSceneImplement
    {
        bool HasScene(string sceneName);

        bool HasScene(UnityEngine.SceneManagement.Scene sceneName);

        bool DoEmptyScene(string from, string to, Dictionary<string, object> bundle);

        bool DoTargetScene(string from, string to, Dictionary<string, object> bundle);

        void AddChangeSceneProcessor();
    }
}