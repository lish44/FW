using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FW
{

    public class UnityLoggerUtility : LoggerUtility
    {
        private string filterString;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="filterStr">只显示包含此字段的输出</param>
        public UnityLoggerUtility(string filterStr = "")
        {
            filterString = filterStr;
        }

        /// <summary>
        /// 白字输出
        /// </summary>
        /// <param name="message">输出内容</param>
        public void Debug(string message)
        {
            if (message.Contains(filterString))
                UnityEngine.Debug.Log(message);
        }

        /// <summary>
        /// 白字输出
        /// </summary>
        /// <param name="message">输出内容</param>
        public void Info(string message)
        {
            if (message.Contains(filterString))
                UnityEngine.Debug.Log(message);
        }
        /// <summary>
        /// 黄字输出
        /// </summary>
        /// <param name="message">输出内容</param>
        public void Warning(string message)
        {
            if (message.Contains(filterString))
                UnityEngine.Debug.LogWarning(message);
        }
        /// <summary>
        /// 红字输出
        /// </summary>
        /// <param name="message">输出内容</param>
        public void Error(string message)
        {
            UnityEngine.Debug.LogError(message);
        }
    }
}