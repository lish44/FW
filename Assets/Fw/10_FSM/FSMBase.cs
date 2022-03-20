using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSMBase {
    /// <summary>
    /// 状态的ID 用来标识每一个状态
    /// </summary>
    public E_FSM_State_Type state { get; }
    //初始化
    public FSMBase (E_FSM_State_Type _state) {
        this.state = _state;
    }

    /// <summary>
    /// 状态进入
    /// </summary>
    /// <param name="args">任意可变参数</param>
    public virtual void OnEnter (params object[] args) { }

    /// <summary>
    /// 状态保持
    /// </summary>
    public virtual void OnStay (params object[] args) { }

    /// <summary>
    /// 状态退出
    /// </summary>
    public virtual void OnExit (params object[] args) { }
}

public class StateTemplates<T> : FSMBase where T : class {
    public T owner; //状态的拥有者方便后期调用
    public StateTemplates (E_FSM_State_Type _state, T t) : base (_state) {
        this.owner = t;
    }
}