//------------------------------------------------------------------------
// |                                                                   |
// | Autor:Adam                                                           |
// |                                       |
// |                                                                   |
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FW
{
    public class FUIManager : MonoBehaviour, IDispose
    {

        /// <summary>
        /// The _inst.
        /// </summary>
        private static FUIManager _inst = null;
        public static FUIManager Inst
        {
            get
            {
                if (null == _inst)
                {
                    ManagerGO = new GameObject(typeof(FUIManager).Name);
                    _inst = ManagerGO.AddComponent<FUIManager>();
                    _inst.Init();
                }
                return _inst;
            }
        }
        private static GameObject ManagerGO;

 
 
        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
           
        }

        public void SetParent(Transform parent)
        {
            transform.SetParent(parent);
        }
        public void Dispose()
        {

        }
    }

}