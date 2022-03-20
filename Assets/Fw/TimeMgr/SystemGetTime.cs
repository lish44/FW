using System;
using System.Collections.Generic;
using System.Text;

namespace FW
{
    public class SystemGetTime : IGetTime
    {
        /// <summary>
        /// 开始时间：毫秒
        /// </summary>
        private long _startTime;

        /// <summary>
        /// 构造函数
        /// </summary>
        public SystemGetTime()
        {
            _startTime = TimeKit.CurrentTimeMillis();
        }
        /// <summary>
        /// 获取时间
        /// </summary>
        /// <returns>秒</returns>
        public float GetTime()
        {
            return (TimeKit.CurrentTimeMillis() - _startTime) / 1000f;
        }

        /// <summary>
        /// 获取未缩放时间
        /// </summary>
        /// <returns>秒</returns>
        public float GetUnscaledTime()
        {
            return (TimeKit.CurrentTimeMillis() - _startTime) / 1000f;
        }
    }
}
