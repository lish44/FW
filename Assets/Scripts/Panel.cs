using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FW;
using UnityEngine.UI;

public class Panel : PanelBase
{
    public override void ChangeTextContent(string _widgetName, string _content)
    {
        base.ChangeTextContent(_widgetName, _content);
    }

    public override bool Equals(object other)
    {
        return base.Equals(other);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override void Hied()
    {
        base.Hied();
    }

    public override void Initialize()
    {
        transform.Find("node").GetComponent<FImage>().Load("icon", (sp) =>
        {
            // sp.Alpha = 0.1f;
            sp.SetGray(true);

        });
        transform.Find("Tex").GetComponent<Text>().text = "123".orange<string>();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
    }

    public override void Refresh()
    {
        base.Refresh();
    }

    public override string ToString()
    {
        return base.ToString();
    }

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void OnClick(string _widgetName)
    {
        base.OnClick(_widgetName);
    }

    protected override void onEndEdit(string _widgetName, string _content)
    {
        base.onEndEdit(_widgetName, _content);
    }

    protected override void OnSliderValueChanged(string _widgetName, float _value)
    {
        base.OnSliderValueChanged(_widgetName, _value);
    }

    protected override void OnToggleChanged(string _widgetName, bool isSel)
    {
        base.OnToggleChanged(_widgetName, isSel);
    }

    protected override void OnValueChanged(string _widgetName)
    {
        base.OnValueChanged(_widgetName);
    }
}
