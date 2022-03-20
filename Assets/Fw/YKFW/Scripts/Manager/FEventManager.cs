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
    public class FEventManager : FGameEvent, IDispose
    {

        /// <summary>
        /// The _inst.
        /// </summary>
        private static FEventManager _inst = null;
        public static FEventManager Inst
        {
            get
            {
                if (null == _inst)
                {

                    ManagerGO = new GameObject(typeof(FEventManager).Name);
                    _inst = ManagerGO.AddComponent<FEventManager>();
                }
                return _inst;
            }
        }
        private static GameObject ManagerGO;


        public void Init()
        { }

        protected override void EventHandlerError(Exception error)
        {
      
        }

        /// <summary>
        /// Removes All this type of event.
        /// </summary>
        public void RemoveEvent(Enum type)
        {
            base.RemoveEvent(type);
        }


        /// <summary>
        /// remove all event
        /// </summary>
        public void ClearEvent()
        {
            base.ClearEvent();
        }


        /// <summary>
        /// Dispatch the specified type, target and args. sync type. 
        /// </summary>
        public void DispatchEvent(Enum type, params object[] args)
        {
            base.DispatchEvent(type, args);
        }

        /// <summary>
        /// Dispatch the specified type, target and args. async type, in idle frame execute function
        /// </summary>
        public void DispatchAsyncEvent(Enum type, params object[] args)
        {
            base.DispatchAsyncEvent(type, args);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool HasEvent(Enum type)
        {
            return base.HasEvent(type);
        }

        /// <summary>
        /// 消息循环
        /// </summary>
        protected void Update()
        {
            base.UpdateEvent();
        }


        public void SetParent(Transform parent)
        {
            transform.SetParent(parent);
        }

        public void Dispose()
        {
            ClearEvent();

        }



    }
}
