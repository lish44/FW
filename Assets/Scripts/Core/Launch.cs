﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FW;
using System;
using System.Reflection;
using System.IO;
public class Launch : SingletonMono<Launch>, IGetTime
{
    [SerializeField]
    private bool isEditor;

    private Dictionary<Type, object> mManagersDic = new Dictionary<Type, object>();

    private void Awake()
    {

        if (isEditor)
            Log.Init(new UnityLoggerUtility());

        Log.Level = Log.LogLevel.All;
        DontDestroyOnLoad(this.gameObject);
        ResMgr.Ins.Init(() =>
        {
            TimeMgr.Init(this);
            Evencenter.Ins.Init();
            PoolMgr.Ins.Init();
            MonoMgr.Ins.Init();
            InputMgr.Ins.Init();
            AudioMgr.Ins.Init();
            UIMgr.Ins.Init();
            ConfigMgr.Ins.Init();
            AddFunc();
        });
    }
    private void Start()
    {
        InputMgr.Ins.StartOrEndCheck(true);
        UIMgr.Ins.OpenPanel<Panel>();
    }
    public void AddManager(object obj)
    {
        Type _type = obj.GetType();
        if (mManagersDic.ContainsKey(_type))
        {
            Log.Debug("Add SameType:" + _type.ToString());
            mManagersDic[_type] = obj;
            return;
        }
        mManagersDic.Add(_type, obj);
    }
    public T GetManager<T>() where T : class
    {
        if (mManagersDic.ContainsKey(typeof(T)))
        {
            object obj = mManagersDic[typeof(T)];
            if (obj != null)
                return obj as T;
        }
        return null;
    }
    private void Update()
    {
        TimeMgr.Update();
    }

    public float GetTime()
    {
        return Time.time;
    }
    public float GetUnscaledTime()
    {
        return Time.realtimeSinceStartup;
    }
    private void AddFunc()
    {
        FImage.LoadImage = (_name, _img, _callBack) =>
        {
            ResMgr.Ins.LoadAsync<Sprite>(_name, (_sp) =>
            {
                if (_img == null || _sp == null) return;
                _img.sprite = _sp;
                _img.Alpha = 1;
                _img.SetGray(false);
                _callBack?.Invoke(_img);
            });
        };
    }
}
