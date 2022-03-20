using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FW;


public class panel : PanelBase
{
    protected override void Awake()
    {
        base.Awake();
    }

    GameObject g;
    protected override void OnClick(string _widgetName)
    {
        switch (_widgetName)
        {
            case
                "1":
                // PoolMgr.Ins.Get<AudioSource>("huaah", "qwe", (g) => { this.g = g; });
                UIMgr.Ins.OpenPanel<panel3>();
                break;
            case
                "2":
                // PoolMgr.Ins.Put("huaah", g);
                AudioMgr.Ins.Play("Ensure");
                break;
        }
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
