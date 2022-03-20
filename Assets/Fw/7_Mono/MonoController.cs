using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FW {
    public class MonoController : MonoBehaviour {

        private event UnityAction m_AwakeEvent;
        private event UnityAction m_startEvent;
        private event UnityAction m_updateEvent;

        private void Awake () {
            DontDestroyOnLoad (this.gameObject);
            if (m_AwakeEvent != null)
                m_AwakeEvent ();
        }

        private void Start () {
            // DontDestroyOnLoad (this.gameObject);
            if (m_startEvent != null)
                m_startEvent ();
        }

        private void Update () {
            if (m_updateEvent != null)
                m_updateEvent ();
        }

        public void AddUpdateListener (UnityAction _action) {
            m_updateEvent += _action;
        }

        public void RemoveUpdateListener (UnityAction _action) {
            m_updateEvent -= _action;
        }

        public void AddStartListener (UnityAction _action) {
            m_startEvent += _action;
        }

        public void RemoveStartListener (UnityAction _action) {
            m_startEvent -= _action;
        }

        public void AddAwakeListener (UnityAction _action) {
            m_AwakeEvent += _action;
        }

        public void RemoveAwakeListener (UnityAction _action) {
            m_AwakeEvent -= _action;
        }
    }
}