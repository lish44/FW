using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace FW
{
    public class TimeMgr
    {
        #region 私有域
        /// <summary>
        /// 定时器列表
        /// </summary>
        private static YKList<Timer> _timerList = new YKList<Timer>();
        /// <summary>
        /// 时间获取类
        /// </summary>
        private static IGetTime _getTime;
        #endregion

        #region 公有方法
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="getTime">时间获取</param>
        public static void Init(IGetTime getTime)
        {
            _getTime = getTime;
            if (_getTime is SystemGetTime)
            {
                Thread t = new Thread(() =>
                {
                    while (true)
                    {
                        Thread.Sleep(1);
                        Update();
                    }
                });
                t.IsBackground = true;
                t.Start();
            }
        }

        /// <summary>
        /// 添加定时器
        /// </summary>
        /// <param name="call">回调函数</param>
        /// <param name="delay">延迟时间</param>
        /// <param name="once">是否只执行一次</param>
        public static void AddTimer(float delay, Action call, Func<bool> canUpdateChecker, bool once = true, bool ignoreScale = false, int customParam = 0)
        {
            Timer timer = new Timer(call, delay, once, ignoreScale, customParam, canUpdateChecker);
            timer.Reset(_getTime);
            _timerList.Add(timer);
        }

        public static void AddTimer(float delay, Action call, bool once = true, bool ignoreScale = false, int customParam = 0)
        {
            Timer timer = new Timer(call, delay, once, ignoreScale, customParam);
            timer.Reset(_getTime);
            _timerList.Add(timer);
        }

        /// <summary>
        /// 移出定时器
        /// </summary>
        /// <param name="call">回调函数</param>
        public static void RemoveTimer(Action call)
        {
            for (int i = _timerList.Size - 1; i >= 0; i--)
            {
                if (_timerList[i] != null && _timerList[i].Call == call)
                {
                    _timerList[i] = null;
                }
            }
        }


        public static bool ContainsTimer(int customParam)
        {
            for (int i = _timerList.Size - 1; i >= 0; i--)
            {
                if (_timerList[i] != null && _timerList[i].customParam == customParam)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 移出定时器
        /// </summary>
        /// <param name="call">回调函数</param>
        public static void RemoveAllTimer(int customParam)
        {
            for (int i = _timerList.Size - 1; i >= 0; i--)
            {
                if (_timerList[i] != null && _timerList[i].customParam == customParam)
                {
                    _timerList[i] = null;
                }
            }
        }

        /// <summary>
        /// 更新
        /// </summary>
        public static void Update()
        {
            for (int i = _timerList.Size - 1; i >= 0; i--)
            {
                if (_timerList[i] == null)
                {
                    _timerList.RemoveAt(i);
                    continue;
                }
                if (_timerList[i].IsReset(_getTime))
                {
                    if (_timerList[i].Call != null)
                        _timerList[i].Call();

                    if (_timerList[i] != null)
                    {
                        if (_timerList[i].Once)
                            _timerList[i] = null;
                        else
                            _timerList[i].Reset(_getTime);
                    }
                }
            }
        }

        /// <summary>
        /// 注销定时器;
        /// </summary>
        public static void Dispose()
        {
            for (int i = _timerList.Size - 1; i >= 0; i--)
            {
                _timerList.RemoveAt(i);
            }
        }
        #endregion

        #region 私有有方法

        #endregion

        #region 保护方法

        #endregion

        #region 重载方法

        #endregion

        #region 枚举

        #endregion
    }
}
