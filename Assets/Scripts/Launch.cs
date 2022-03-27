using System.Collections;
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
        });
    }
    private void Start()
    {

        FResourcesManager.Inst.Init();
        FW.Evencenter.Ins.AddEventListener<KeyCode>(EventName.KEY_DOWN, KeyDown);
        InputMgr.Ins.StartOrEndCheck(true);


        // ScenesMgr.Ins.LoadSceneAsyn("", () =>
        // {

        // });
    }

    void KeyDown(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.W:
                Log.Warning(1.ToString());
                break;
        }
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
}
