using System.Collections.Generic;
using UnityEngine.Events;

namespace FW
{
    /// <summary>
    /// 事件中心
    /// 添加事件 ：Evencenter.Ins.AddEventListener< 返回类型 > (EventName, callback); 
    /// 删除事件 ：Evencenter.Ins.RemoveEventListener< 返回类型 > ("EventName", callback);
    /// </summary>
    public class Evencenter : SingletonBase<Evencenter>
    {

        private Dictionary<EventName, IEventInfo> m_eventDic;

        public override void Init()
        {
            m_eventDic = new Dictionary<EventName, IEventInfo>();
        }

        /// <summary>
        /// 注册监听函数
        /// </summary>
        /// <param name="_eventName">事件名</param>
        /// <param name="_action">回调函数</param>
        /// <typeparam name="T">参数类型</typeparam>
        public void AddEventListener<T>(EventName _eventName, UnityAction<T> _action)
        {
            if (!m_eventDic.ContainsKey(_eventName))
            {
                m_eventDic.Add(_eventName, new EventInfo<T>(_action));
                return;
            }
            (m_eventDic[_eventName] as EventInfo<T>).m_action += _action;

        }

        /// <summary>
        /// 事件的发布
        /// </summary>
        /// <param name="_eventName">事件名</param>
        /// <param name="_info">事件触发时需要传出去的参数</param>
        /// <typeparam name="T">参数类型</typeparam>
        public void EventTrigger<T>(EventName _eventName, T _info)
        {
            if (m_eventDic.ContainsKey(_eventName) && (m_eventDic[_eventName] as EventInfo<T>).m_action != null)
                (m_eventDic[_eventName] as EventInfo<T>).m_action(_info); //里氏转换 把父类转为子类
        }

        /// <summary>
        /// 取消函数监听 一般用于对象销毁
        /// </summary>
        /// <param name="_eventName">事件名</param>
        /// <param name="_action">注册的回调函数</param>
        /// <typeparam name="T">参数类型</typeparam>
        public void RemoveEventListener<T>(EventName _eventName, UnityAction<T> _action)
        {
            (m_eventDic[_eventName] as EventInfo<T>).m_action -= _action;
        }

        public void Clear()
        {
            m_eventDic.Clear();
        }

        //-----------------------------无参版----------------------------------

        public void AddEventListener(EventName _eventName, UnityAction _action)
        {
            if (!m_eventDic.ContainsKey(_eventName))
            {
                m_eventDic.Add(_eventName, new EventInfo(_action));
                return;
            }
            (m_eventDic[_eventName] as EventInfo).m_action += _action;

        }

        public void EventTrigger(EventName _eventName)
        {
            if (m_eventDic.ContainsKey(_eventName) && (m_eventDic[_eventName] as EventInfo).m_action != null)
                (m_eventDic[_eventName] as EventInfo).m_action();
        }

        public void RemoveEventListener(EventName _eventName, UnityAction _action)
        {
            (m_eventDic[_eventName] as EventInfo).m_action -= _action;
        }

    }
}