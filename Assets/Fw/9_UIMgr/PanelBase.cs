using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FW
{
    public class PanelBase : MonoBehaviour
    {

        private Dictionary<string, List<UIBehaviour>> m_controlDic = new Dictionary<string, List<UIBehaviour>>();

        protected virtual void Awake()
        {
            FindChildrenControl<Button>();
            FindChildrenControl<Image>();
            FindChildrenControl<Text>();
            FindChildrenControl<Toggle>();
            FindChildrenControl<Slider>();
            FindChildrenControl<ScrollRect>();
            FindChildrenControl<InputField>();
            FindChildrenControl<ToggleGroup>();
        }

        public T GetControl<T>(string _widgetName) where T : UIBehaviour
        {
            if (m_controlDic.ContainsKey(_widgetName))
            {
                for (int i = 0, j = m_controlDic[_widgetName].Count; i < j; ++i)
                {
                    if (m_controlDic[_widgetName][i] is T)
                        return m_controlDic[_widgetName][i] as T;
                }
            }
            return null;
        }

        // 找子控件添加进m_controlDic
        private void FindChildrenControl<T>() where T : UIBehaviour
        {
            T[] controls = GetComponentsInChildren<T>();
            for (int i = 0, j = controls.Length; i < j; ++i)
            {
                string goName = controls[i].gameObject.name;
                // if (!goName.EndsWith ("M")) continue; // 如果没打M标记就不进行添加

                if (m_controlDic.ContainsKey(goName))
                    m_controlDic[goName].Add(controls[i]);
                else
                    m_controlDic.Add(goName, new List<UIBehaviour>() { controls[i] });

                if (controls[i] is Button)
                    (controls[i] as Button).onClick.AddListener(() => { OnClick(goName); });
                else if (controls[i] is Slider)
                    (controls[i] as Slider).onValueChanged.AddListener((f) => { OnSliderValueChanged(goName, f); });
                else if (controls[i] is InputField)
                {
                    (controls[i] as InputField).onEndEdit.AddListener((str) => { onEndEdit(goName, str); });
                }
                else if (controls[i] is Toggle)
                {
                    (controls[i] as Toggle).onValueChanged.AddListener((b) => { OnToggleChanged(goName, b); });
                }
            }
        }

        public virtual void ChangeTextContent(string _widgetName, string _content)
        {
            Text t = GetControl<Text>(_widgetName);
            try
            {
                t.text = _content;
            }
            catch
            {
                Debug.Log(_widgetName + "不存在莫法改");
                throw;
            }
        }

        protected virtual void OnClick(string _widgetName) { }
        protected virtual void OnSliderValueChanged(string _widgetName, float _value) { }
        protected virtual void onEndEdit(string _widgetName, string _content) { }
        protected virtual void OnValueChanged(string _widgetName) { }
        protected virtual void OnToggleChanged(string _widgetName, bool isSel) { }

        public virtual void Show() { }
        public virtual void Refresh() { }
        public virtual void Hied() { }
    }
}