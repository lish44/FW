using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSM {
    //用个字典存所有状态
    public Dictionary<E_FSM_State_Type, FSMBase> m_allState;

    /// <summary>
    /// 上一个状态
    /// </summary>
    private FSMBase m_priousState;

    /// <summary>
    /// 当前状态
    /// </summary>
    private FSMBase m_curState;

    //构造函数
    public FSM () {
        m_priousState = null;
        m_curState = null;
        m_allState = new Dictionary<E_FSM_State_Type, FSMBase> ();
    }

    public void RegistState (FSMBase _fsmBase) {
        //没有这个状态就添加
        if (!m_allState.ContainsKey (_fsmBase.state))
            m_allState.Add (_fsmBase.state, _fsmBase);
    }

    public void UnRegistState (FSMBase _fsmBase) {
        if (m_allState.ContainsKey (_fsmBase.state))
            m_allState.Remove (_fsmBase.state);
    }

    public void ChangeState (E_FSM_State_Type _state, params object[] args) {
        //没有这个状态 或 就是当前状态 就退出
        if (!m_allState.ContainsKey (_state) || _state == m_curState.state) return;
        if (m_curState == null) return;
        //本次状态退出
        m_curState.OnExit (args);
        //退出后把本次状态设为前一个状态
        m_priousState = m_curState;
        //根据想要更改的状态 去字典找 并设为当前状态
        m_curState = m_allState[_state];

        m_curState.OnEnter (args);
    }

    public E_FSM_State_Type GetPriousState () {
        if (m_priousState != null)
            return m_priousState.state;

        return E_FSM_State_Type.None;
    }

    public E_FSM_State_Type GetCurState () {
        if (m_curState != null)
            return m_curState.state;

        return E_FSM_State_Type.None;
    }

    public void UpdateState (params object[] args) {
        if (m_curState != null)
            m_curState.OnStay (args);
    }

    /// <summary>
    /// 开始运作并进入一个状态
    /// </summary>
    /// <param name="_fsmBase">想要进入的状态</param>
    /// <param name="args">可选参数</param>
    public void Go (FSMBase _fsmBase, params object[] args) {
        if (m_curState != null) return;
        m_curState = _fsmBase;
        m_curState.OnEnter (args);
    }

}