using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

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
            base.Init();
            var go = FW.ResMgr.Ins.Load<GameObject>("MainCanvas");
            m_mainCanvas = go.transform as RectTransform;
            GameObject.DontDestroyOnLoad(go);

            //找层级
            m_bot = m_mainCanvas.Find("Bot");
            m_mid = m_mainCanvas.Find("Mid");
            m_top = m_mainCanvas.Find("Top");
            m_system = m_mainCanvas.Find("System");

            go = FW.ResMgr.Ins.Load<GameObject>("EventSystem");
            GameObject.DontDestroyOnLoad(go);
        }

        public UIMgr()
        {
        }

        public void OpenPanel<T>(string _panelName = "", E_UI_layer _layer = E_UI_layer.Mid, UnityAction<T> _callback = null) where T : PanelBase
        {
            if (_panelName == "") _panelName = typeof(T).Name;
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
                Transform father = m_bot;
                switch (_layer)
                {
                    case E_UI_layer.Mid:
                        father = m_mid;
                        break;
                    case E_UI_layer.Top:
                        father = m_top;
                        break;
                    case E_UI_layer.System:
                        father = m_system;
                        break;
                }

                FW.Utility.TransformOperation.SetParent(go.transform, father);

                // 得到面板身上的脚本
                T panel = go.GetComponent<T>();
                // 处理面板创建完成后的逻辑 因为异步加载至少要等一帧
                if (_callback != null)
                    _callback(panel);

                panel.Show();
                if (!m_allPanelDic.ContainsKey(_panelName))
                    m_allPanelDic.Add(_panelName, panel);

            });
        }

        /// <summary>
        /// 删面板
        /// </summary>
        public void ClosePanel(string _panelName)
        {
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

        public void HiedPanel(string _panelName)
        {
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
        public T GetPanel<T>(string _panelName) where T : PanelBase
        {
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