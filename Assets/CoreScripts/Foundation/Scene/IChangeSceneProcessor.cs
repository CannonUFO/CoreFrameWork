using System.Collections.Generic;

namespace ible.Foundation.Scene
{
    /// <summary>
    /// 介入場景轉換的過程控制
    /// </summary>
    public interface IChangeSceneProcessor
    {
        /// <summary>
        ///     轉換Scene前的準備流程, 會一直呼叫直到回傳100
        /// </summary>
        /// <param name="isEntering">是否是第一Frame</param>
        /// <param name="from">從哪個Scene發出轉換要求</param>
        /// <param name="to">目標Scene</param>
        /// <param name="bundle">參數</param>
        /// <returns></returns>
        int PrepareProcess(bool isEntering, string from, string to, Dictionary<string, object> bundle);

        /// <summary>
        ///     轉換Scene中間載入目標Scene的流程, 會一直呼叫直到回傳100
        /// </summary>
        /// <param name="isEntering">是否是第一Frame</param>
        /// <param name="from">從哪個Scene發出轉換要求</param>
        /// <param name="to">目標Scene</param>
        /// <param name="bundle">參數</param>
        /// <returns></returns>
        int LoadingProcess(bool isEntering, string from, string to, Dictionary<string, object> bundle);

        /// <summary>
        ///     下個Scene起來的初始化流程, 會一直呼叫直到回傳100
        /// </summary>
        /// /// <param name="isEntering">是否是第一Frame</param>
        /// <param name="from">從哪個Scene發出轉換要求</param>
        /// <param name="to">目標Scene</param>
        /// <param name="bundle">參數</param>
        /// <returns></returns>
        int InitProcess(bool isEntering, string from, string to, Dictionary<string, object> bundle);
    }
}