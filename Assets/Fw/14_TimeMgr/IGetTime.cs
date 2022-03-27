using System;
using System.Collections.Generic;
using System.Text;

namespace FW
{
    public interface IGetTime
    {
        /// <summary>
        /// 获取时间
        /// </summary>
        /// <returns>秒</returns>
        float GetTime();

        /// <summary>
        /// 获取未缩放时间
        /// </summary>
        /// <returns>秒</returns>
        float GetUnscaledTime();
    }
}
