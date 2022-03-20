using System;
using System.Collections.Generic;
using System.Text;

namespace FW
{
    public class Timer
    {
        /// <summary>
        /// 回调函数
        /// </summary>
        private Action _call;
        /// <summary>
        /// 检查是否暂停(战斗内暂停不走timescale)
        /// </summary>
        private Func<bool> _canUpdateChecker;
        /// <summary>
        /// 记录上一次的时间
        /// </summary>
        private float _preTime;
        /// <summary>
        /// 延迟时间：秒
        /// </summary>
        private float _delay;
        /// <summary>
        /// 结束时间
        /// </summary>
        private float _endTime;
        /// <summary>
        /// 是否执行一次
        /// </summary>
        private bool _once;
        /// <summary>
        /// 忽略缩放
        /// </summary>
        private bool _ignoreScale;
        /// <summary>
        /// 自定义类型参数
        /// </summary>
        private int _customParam;

        /// <summary>
        /// 回调函数
        /// </summary>
        public Action Call
        {
            get
            {
                return _call;
            }
        }

        /// <summary>
        /// 延迟时间：秒
        /// </summary>
        public float Delay
        {
            get
            {
                return _delay;
            }
        }

        /// <summary>
        /// 是否执行一次
        /// </summary>
        public bool Once
        {
            get
            {
                return _once;
            }
        }

        /// <summary>
        /// 忽略缩放
        /// </summary>
        public bool IgnoreScale
        {
            get
            {
                return _ignoreScale;
            }
        }
        /// <summary>
        /// 自定义类型参数
        /// </summary>
        public int customParam
        {
            get
            {
                return _customParam;
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public Timer(Action call, float delay, bool once = false, bool ignoreScale = true, int _customParam = 0, Func<bool> _canUpdateChecker = null)
        {
            this._call = call;
            this._delay = delay;
            this._once = once;
            this._ignoreScale = ignoreScale;
            this._customParam = _customParam;
            this._canUpdateChecker = _canUpdateChecker;
        }

        public Timer(Action call, float delay, bool once = false, bool ignoreScale = true, int _customParam = 0)
        {
            this._call = call;
            this._delay = delay;
            this._once = once;
            this._ignoreScale = ignoreScale;
            this._customParam = _customParam;
        }

        /// <summary>
        /// 重置，重新计算时长
        /// </summary>
        public void Reset(IGetTime getTime)
        {
            float time = (_ignoreScale ? getTime.GetUnscaledTime() : getTime.GetTime());
            _endTime = time + _delay;
            if (_canUpdateChecker != null)
                _preTime = time;
        }

        /// <summary>
        /// 是否可以重置
        /// </summary>
        /// <returns></returns>
        public bool IsReset(IGetTime getTime)
        {
            float time = (_ignoreScale ? getTime.GetUnscaledTime() : getTime.GetTime());
            if (_canUpdateChecker != null)
            {
                if (!_canUpdateChecker())
                    _endTime += time - _preTime;
                _preTime = time;
            }
            return _endTime <= time;
        }
    }
}
