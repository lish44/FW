using UnityEngine.Events;

namespace FW {
    interface IEventInfo { }

    public class EventInfo<T> : IEventInfo {
        public UnityAction<T> m_action;
        public EventInfo (UnityAction<T> _action) {
            m_action += _action;
        }
    }
    public class EventInfo : IEventInfo {
        public UnityAction m_action;
        public EventInfo (UnityAction _action) {
            m_action += _action;
        }
    }
}