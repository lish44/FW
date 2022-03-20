using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FW;

public class panel3 : PanelBase
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
        UIMgr.Ins.HiedPanel(this.name);
    }

    public override void Refresh()
    {
        base.Refresh();
    }

    public override void Show()
    {
        base.Show();
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
        Hied();
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

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
