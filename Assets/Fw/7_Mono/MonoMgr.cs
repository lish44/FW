﻿using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Events;

namespace FW
{
    public class MonoMgr : SingletonMono<MonoMgr>
    {
        private MonoController m_monoController;
        public void Init(CallBack _callback = null)
        {
            GameObject go = GameObject.Find("MgrRoots");
            if (go == null)
            {
                go = new GameObject("MonoController");
            }
            m_monoController = go.AddComponent<MonoController>();
            DontDestroyOnLoad(go);
            _callback?.Invoke();
        }

        public void AddUpdateListener(CallBack _action)
        {
            m_monoController.AddUpdateListener(_action);
        }

        public void RemoveUpdateListener(CallBack _action)
        {
            m_monoController.RemoveUpdateListener(_action);
        }

        public void AddStartListener(CallBack _action)
        {
            m_monoController.AddStartListener(_action);
        }

        public void RemoveStartListener(CallBack _action)
        {
            m_monoController.RemoveStartListener(_action);
        }

        public void AddAwakeListener(CallBack _action)
        {
            m_monoController.AddAwakeListener(_action);
        }

        public void RemoveAwakeListener(CallBack _action)
        {
            m_monoController.RemoveAwakeListener(_action);
        }

        //...
        public Coroutine StartCoroutine(IEnumerator _enumerator)
        {
            return m_monoController.StartCoroutine(_enumerator);
        }

        public void StopCoroutine(IEnumerator _routine)
        {
            m_monoController.StopCoroutine(_routine);
        }

        //...
        public Coroutine StartCoroutine(string _methodName, [DefaultValue("null")] object _value)
        {
            return m_monoController.StartCoroutine(_methodName, _value);
        }
        public void StopCoroutine(Coroutine _routine)
        {
            m_monoController.StopCoroutine(_routine);
        }

        //...
        public Coroutine StartCoroutine(string _methodName)
        {
            return m_monoController.StartCoroutine(_methodName);
        }

        public void StopCoroutine(string _methodName)
        {
            m_monoController.StopCoroutine(_methodName);
        }
        public void StopAllCoroutines()
        {
            m_monoController.StopAllCoroutines();
        }

    }
}