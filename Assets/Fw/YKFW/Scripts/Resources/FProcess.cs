using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using System.Linq;
using Object = UnityEngine.Object;
namespace FW
{
    /// <summary>
    /// 资源加载处理
    /// </summary>
    internal class FProcess : IDisposable
    {

        internal FRequest m_FRequest;

        /// <summary>
        /// 关键字为资源路径+资源类型
        /// </summary>
        internal Dictionary<KeyInfo, List<CallBack<object>>> CallBackDic
        {
            get
            {
                return m_FRequest.CallBackDic;
            }
            set
            {
                m_FRequest.CallBackDic = value;
            }
        }


        /// <summary>
        /// 进程所依赖的资源
        /// </summary>
        internal List<string> AllChildrenDependenceList;

        internal FResourceUnit ResUnit;

        /// <summary>
        /// 进程状态
        /// </summary>
        internal FrameDef.AsyncState State;
        internal bool LoadAsync;
        internal bool isCache;
        internal bool isMainProcess;
        internal FResourcesManager m_ResourcesManager;

        internal string LoadPath
        {
            get
            {
                return m_FRequest.LoadPath;
            }
        }

        internal bool UsedAssetBundle
        {
            get
            {
                return m_FRequest.UsedAssetBundle;
            }
        }

        public FProcess(FRequest request, FResourcesManager manager)
        {
            m_FRequest = request;

            State = FrameDef.AsyncState.Hanging;

            this.m_ResourcesManager = manager;
            LoadAsync = request.LoadAsync;
            this.isCache = request.isCache;
            this.isMainProcess = request.isMainRequest;
            ResUnit = new FResourceUnit(this, manager, request.StayMemory);

            AllChildrenDependenceList = new List<string>();

            FEventManager.Inst.AddEvent(FrameDef.ResourcesProcessType.AssetLoaded, AssetLoaded);

        }

        public void Dispose()
        {

        }


        internal void TryLoad()
        {
            if (State == FrameDef.AsyncState.Ready)
            {

                m_ResourcesManager.StartCoroutine(_load(LoadPath));


            }

        }

        private IEnumerator _load(string loadPath)
        {

            FResourcesManager.Inst.AddCoroutinCount();

            State = FrameDef.AsyncState.Loading;

            if (!UsedAssetBundle)
            {
                //非打包模式
                if (!LoadAsync)
                {
                    this.ResUnit.InputResource(null, Resources.LoadAll(loadPath));
                }
                else
                {
                    yield return null;
                    var res = Resources.LoadAll(loadPath);
                    this.ResUnit.InputResource(null, res);
                }
                //非打包模式算数量
                this.ResUnit.Size = 1;
                ProcessComplete();
                yield break;
            }
            else
            {

                string _finalPath = FResourceCommon.GetURLPath("StreamingResources/" + LoadPath, string.Empty, string.Empty);

                if (!File.Exists(_finalPath))
                {
                    _finalPath = FResourceCommon.GetStreamingAssetsPath("StreamingResources/" + LoadPath);
                }

                AssetBundle _assetBundle = null;

                if (!LoadAsync)
                {
                    _assetBundle = AssetBundle.LoadFromFile(_finalPath);
                    if (null == _assetBundle)
                    {

                        ResUnit.InputResource(null, null);

                        ProcessComplete();
                        yield break;
                    }
                }
                else
                {

                    AssetBundleCreateRequest _request = AssetBundle.LoadFromFileAsync(_finalPath);//避免卡顿,用异步     

                    yield return _request;

                    //出错处理
                    if (null == _request)
                    {

                        ResUnit.InputResource(null, null);

                        ProcessComplete();
                        yield break;
                    }
                    else
                    {
                        _assetBundle = _request.assetBundle;

                        if (null == _assetBundle)
                        {

                            ResUnit.InputResource(null, null);

                            ProcessComplete();
                            yield break;
                        }
                    }
                }

                AssetBundleRequest _assetRequest = _assetBundle.LoadAllAssetsAsync();
                yield return _assetRequest;
                ResUnit.InputResource(_assetBundle, _assetRequest.allAssets);
                //ResUnit.InputResource(_assetBundle, _assetBundle.LoadAllAssets());
                ProcessComplete();

            }

        }

        private IEnumerator _unLoadAudio(AudioClip clip)
        {
            while (clip.loadState == AudioDataLoadState.Loading)
            {
                yield return null;

            }
            ResUnit.TryToUnloadAssetBundle();
        }


        /// <summary>
        /// 加载完成
        /// </summary>
        public void ProcessComplete()
        {

            FResourcesManager.Inst.ReduceCoroutinCount();

            m_ResourcesManager.ProcessDic.Remove(LoadPath);
            State = FrameDef.AsyncState.LoadEnd;

            if (!m_ResourcesManager.LoadedResourceUnitDic.ContainsKey(ResUnit.LoadPath))
            {
                m_ResourcesManager.LoadMemorySize += ResUnit.Size;
                m_ResourcesManager.LoadedResourceUnitDic.Add(ResUnit.LoadPath, ResUnit);
            }

            List<string> _assetPathList = m_FRequest.AssetBundleData.AssetBundlePathList;

            for (int i = 0; i < _assetPathList.Count; ++i)
            {
                string _assetPath = _assetPathList[i];
                m_ResourcesManager.AssetLoad(_assetPath);
            }

            for (int i = 0; i < _assetPathList.Count; ++i)
            {
                string _assetPath = _assetPathList[i];

                FEventManager.Inst.DispatchEvent(FrameDef.ResourcesProcessType.AssetCached, _assetPath, ResUnit);
                FEventManager.Inst.DispatchEvent(FrameDef.ResourcesProcessType.AssetLoaded, _assetPath, ResUnit);
            }

            FEventManager.Inst.DispatchEvent(FrameDef.ResourcesProcessType.AssetCached, LoadPath, ResUnit);
            FEventManager.Inst.DispatchEvent(FrameDef.ResourcesProcessType.AssetLoaded, LoadPath, ResUnit);
            m_ResourcesManager.AssetLoad(LoadPath);
        }

        private void AssetLoaded(params object[] args)
        {
            string _loadPath = args[0] as string;

            FResourceUnit _unit = args[1] as FResourceUnit;

            if (AllChildrenDependenceList.Count > 0)
            {
                for (int i = 0; i < AllChildrenDependenceList.Count; ++i)
                {
                    if (AllChildrenDependenceList[i] == _loadPath)
                    {
                        ResUnit.AddResourceRefToDic(_loadPath, _unit.BeUsed());

                        AllChildrenDependenceList.RemoveAt(i--);

                    }
                }

                if (0 == AllChildrenDependenceList.Count)
                {
                    State = FrameDef.AsyncState.Ready;
                }
            }


            if (State == FrameDef.AsyncState.LoadEnd && 0 == AllChildrenDependenceList.Count)
            {
                FEventManager.Inst.RemoveEvent(FrameDef.ResourcesProcessType.AssetLoaded, AssetLoaded);

                allDone();
            }

        }


        /// <summary>
        /// 整个加载任务完成，（其依赖的资源也加载完成了）
        /// </summary>
        private void allDone()
        {

            KeyInfo[] _keyArray = FUtils.DicKeysToArray(CallBackDic);
            if (null != _keyArray && _keyArray.Length > 0)
            {
                for (int i = 0; i < _keyArray.Length; ++i)
                {
                    KeyInfo _key = _keyArray[i];
                    List<CallBack<object>> _value = CallBackDic[_key];
                    for (int j = 0; j < _value.Count; ++j)
                    {
                        if (null != _value[j])
                        {
                            var o = ResUnit.GetInstance(_key, isCache);
                            if(_value[j] != null) _value[j](o);

                            _value[j] = null;
                        }

                    }
                }
            }
            else
            {
                if (isCache)
                {

                    object _obj = ResUnit.GetInstance(m_FRequest.m_keyInfo, isCache);
                    m_ResourcesManager.AddToCacheList(_obj as FResourceRef);

                }

            }



            if (isMainProcess)
            {
                if (null != ResUnit.MainAsset && ResUnit.MainAsset is AudioClip)
                {
                    m_ResourcesManager.StartCoroutine(_unLoadAudio(ResUnit.MainAsset as AudioClip));
                }
                else
                {
                    ResUnit.TryToUnloadAssetBundle();
                }
            }

            CallBackDic = null;

            AllChildrenDependenceList = null;

            State = FrameDef.AsyncState.Done;

        }


        public void AddCallBack(Dictionary<KeyInfo, List<CallBack<object>>> dic)
        {
            if (null == dic)
            {
                return;
            }
            KeyInfo[] _keyArray = FUtils.DicKeysToArray(dic);
            if (null != _keyArray)
            {
                for (int i = 0; i < _keyArray.Length; ++i)
                {
                    KeyInfo _key = _keyArray[i];
                    List<CallBack<object>> _value = dic[_key];

                    AddCallBack(_key.path, _key.type, _value);
                }
            }
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
