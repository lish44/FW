using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FW;


public class NewBehaviourScript : MonoBehaviour
{
    void Start()
    {
        UIMgr.Ins.OpenPanel<panel>("panel");
        FResourcesManager.Inst.Init();
    }
}
