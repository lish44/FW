using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FW
{
    public class UIMgr : SingletonBase<UIMgr>
    {
        public Dictionary<string, PanelBase> m_allPanelDic = new Dictionary<string, PanelBase>();
        public Dictionary<string, PanelBase> m_hiedPanelDic = new Dictionary<string, PanelBase>();

        Transform m_bot;
        Transform m_mid;
        Transform m_top;
        Transform m_system;

        public RectTransform m_mainCanvas;
        public override void Init()
        {
        }
        public void Init(CallBack _callback = null)
        {
            GameObject g = GameObject.Find("MainCanvas");
            if (g != null) GameObject.Destroy(g);
            g = FW.ResMgr.Ins.Load<GameObject>("MainCanvas");
            var _cs = g.GetComponent<CanvasScaler>();
            if (_cs == null)
                _cs = g.AddComponent<CanvasScaler>();
            _cs.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            _cs.referenceResolution = new Vector2(1280, 720);
            g.GetComponent<Canvas>().worldCamera = Camera.main;
            m_mainCanvas = g.transform as RectTransform;
            GameObject.DontDestroyOnLoad(g);

            //找层级
            m_bot = m_mainCanvas.Find("Bot");
            m_mid = m_mainCanvas.Find("Mid");
            m_top = m_mainCanvas.Find("Top");
            m_system = m_mainCanvas.Find("System");

            g = FW.ResMgr.Ins.Load<GameObject>("EventSystem");
            GameObject.DontDestroyOnLoad(g);
            _callback?.Invoke();
        }


        public void OpenPanel<T>(E_UI_layer _layer = E_UI_layer.Mid, UnityAction<T> _callback = null) where T : PanelBase
        {
            string _panelName = typeof(T).Name;
            // 先去隐藏的dic里找
            if (m_hiedPanelDic.ContainsKey(_panelName) && !m_hiedPanelDic[_panelName].gameObject.activeInHierarchy)
            {
                var panel = m_hiedPanelDic[_panelName];
                panel.Refresh();
                panel.gameObject.SetActive(true);
                if (_callback != null) _callback(panel as T);
                m_hiedPanelDic.Remove(_panelName);
                return;
            }
            // 防止面板二次打开
            if (m_allPanelDic.ContainsKey(_panelName))
            {
                m_allPanelDic[_panelName].Refresh();
                if (_callback != null) _callback(m_allPanelDic[_panelName] as T);
                return;
            }

            FW.ResMgr.Ins.LoadAsync<GameObject>(_panelName, (go) =>
            {
                //作为canvas子对象
                Transform _father = m_bot;
                switch (_layer)
                {
                    case E_UI_layer.Mid:
                        _father = m_mid;
                        break;
                    case E_UI_layer.Top:
                        _father = m_top;
                        break;
                    case E_UI_layer.System:
                        _father = m_system;
                        break;
                }

                FW.Utility.TransformOperation.SetParent(go.transform, _father);

                // 得到面板身上的脚本
                T _panel = go.GetComponent<T>();
                if (_panel == null) _panel = go.AddComponent<T>();
                // 处理面板创建完成后的逻辑 因为异步加载至少要等一帧
                _callback?.Invoke(_panel);
                _panel.Initialize();
                _panel.Refresh();
                if (!m_allPanelDic.ContainsKey(_panelName))
                    m_allPanelDic.Add(_panelName, _panel);

            });
        }

        /// <summary>
        /// 删面板
        /// </summary>
        public void ClosePanel<T>() where T : PanelBase
        {

            string _panelName = typeof(T).Name;
            if (m_allPanelDic.ContainsKey(_panelName))
            {
                m_allPanelDic[_panelName].Hied(); // 面板删除前一些保存工作
                GameObject.Destroy(m_allPanelDic[_panelName].gameObject); // 会触发自身的OnDestroy -> 触 UnRegistPanel
                m_allPanelDic.Remove(_panelName);
            }
            else
            {
                Debug.Log(_panelName + " : 不存在 检查拼写");
            }
        }

        public void HiedPanel<T>() where T : PanelBase
        {
            string _panelName = typeof(T).Name;
            if (!m_hiedPanelDic.ContainsKey(_panelName) && m_allPanelDic.ContainsKey(_panelName))
            {
                m_hiedPanelDic.Add(_panelName, m_allPanelDic[_panelName]);
                m_allPanelDic[_panelName].Hied();
                m_hiedPanelDic[_panelName].gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 得到一个面板脚本
        /// </summary>
        public T GetPanel<T>() where T : PanelBase
        {
            string _panelName = typeof(T).Name;
            if (m_allPanelDic.ContainsKey(_panelName))
                return m_allPanelDic[_panelName] as T;
            return null;
        }

        /// <summary>
        /// 得到层级父对象
        /// </summary>
        public Transform GetLayerFather(E_UI_layer _layer)
        {
            switch (_layer)
            {
                case E_UI_layer.Bot:
                    return m_bot;
                case E_UI_layer.Mid:
                    return m_mid;
                case E_UI_layer.Top:
                    return m_top;
                case E_UI_layer.System:
                    return m_system;
            }
            return null;
        }

        /// <summary>
        /// 添加自定义事件
        /// </summary>
        /// <param name="_widgetObj">想要添加事件的组件</param>
        /// <param name="_type">事件类型</param>
        /// <param name="_callback">回调</param>
        public void AddCustomEventListner(MonoBehaviour _widgetObj, EventTriggerType _type, UnityAction<BaseEventData> _callback)
        {
            EventTrigger trigger = _widgetObj.GetComponent<EventTrigger>();
            if (trigger == null)
                trigger = _widgetObj.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = _type;
            entry.callback.AddListener(_callback);

            trigger.triggers.Add(entry);

        }

        // Ex:
        // FW.UIMgr.Ins.AddCustomEventListner (GetControl<UnityEngine.UI.InputField> ("InputField_M"), UnityEngine.EventSystems.EventTriggerType.PointerEnter, (data) => {
        //     print ("entered");
        // });

    }
}