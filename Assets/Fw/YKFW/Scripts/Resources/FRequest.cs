using System;
using System.Collections;
using System.Collections.Generic;

using System.Linq;

using Object = UnityEngine.Object;

namespace FW
{

    /// <summary>
    /// 资源请求类
    /// </summary>
    internal class FRequest : IDisposable
    {
        internal string LoadPath
        {
            get
            {
                return AssetBundleData.Path;
            }
        }
        internal KeyInfo m_keyInfo;
        internal bool UsedAssetBundle;

        internal FrameDef.TaskPriority Priority = FrameDef.TaskPriority.Normal;


        internal FAssetBundleData AssetBundleData;
        /// <summary>
        /// 关键字为资源路径+资源类型
        /// </summary>
        internal Dictionary<KeyInfo, List<CallBack<object>>> CallBackDic;

        //是否用异步
        internal bool LoadAsync;
        internal bool isCache;
        internal bool isMainRequest;
        internal bool StayMemory = false;
        //请求是否完成
        internal bool isDone;

        internal FRequest(KeyInfo keyInfo, bool usedAssetBundle, FAssetBundleData assetBundleData, bool Async, bool isCache, bool stayMemory, FrameDef.TaskPriority priority = FrameDef.TaskPriority.Normal)
        {

            m_keyInfo = keyInfo;
            UsedAssetBundle = usedAssetBundle;
            AssetBundleData = assetBundleData;
            LoadAsync = Async;
            this.isCache = isCache;
            StayMemory = stayMemory;
            Priority = priority;

        }

        public void Dispose()
        {

        }

        public void AddCallBack(string assetPath, Type type, List<CallBack<object>> list)
        {
            if (null == list)
            {
                return;
            }

            if (null == CallBackDic)
            {
                CallBackDic = new Dictionary<KeyInfo, List<CallBack<object>>>();
            }

            KeyInfo _key = CallBackDic.Keys.ToList().Find(v => v.path == assetPath && v.type == type);
            List<CallBack<object>> _value = null;
            if (null == _key)
            {
                _key = new KeyInfo(assetPath, type);
                CallBackDic.Add(_key, new List<CallBack<object>>());
            }
            _value = CallBackDic[_key];

            for (int i = 0; i < list.Count; ++i)
            {
                if (!_value.Contains(list[i]))
                {
                    if (null != list[i])
                    {
                        _value.Add(list[i]);
                    }
                }
            }


        }

    }

}
