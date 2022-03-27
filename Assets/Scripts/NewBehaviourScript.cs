using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FW;


public class NewBehaviourScript : MonoBehaviour
{
    void Start()
    {
        TimeMgr.AddTimer(1, () => { Log.Debug(123.ToString()); }, false);
        // UIMgr.Ins.OpenPanel<panel>("panel");
        // FResourcesManager.Inst.Init();
        UIMgr.Ins.OpenPanel<panel>();
    }
}
