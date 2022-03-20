using ScriptableObjectClassLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace FW
{
    /// <summary>
    /// 1 放在工程目录Assets\Resources\下
    /// 2 挂在scene中名为ResourcesManager的gameObject上
    /// </summary>
    public class FResourcesManager : MonoBehaviour, IDispose
    {
        private static FResourcesManager _inst = null;
        public static FResourcesManager Inst
        {
            get
            {
                if (null == _inst)
                {
                    ManagerGO = new GameObject(typeof(FResourcesManager).Name);
                    _inst = ManagerGO.AddComponent<FResourcesManager>();

                }
                return _inst;
            }
        }

        private static GameObject ManagerGO;


        private static readonly object locker = new object();

        /// <summary>
        /// SDK提供的外部资源文件路径，热更用
        /// </summary>
        public static string RESROOT;

        /// <summary>
        /// 调试模式
        /// </summary>
        public bool DebugMode = false;


        /// <summary>
        /// 是否通过assetbundle加载资源
        /// </summary>
        public static bool UsedAssetBundle = false;
        /// <summary>
        /// 
        /// </summary>
        public static FrameDef.eCompressType CompressType = FrameDef.eCompressType.LZMA;
        /// <summary>
        /// 交叉引用的AB包释放规则，目前只有无引用图片会释放AB包
        /// </summary>
        public FrameDef.eCrossRefBundleReleaseType CrossRefBundleReleaseType = FrameDef.eCrossRefBundleReleaseType.ReleaseAssetBundleAndAsset;


        /// <summary>
        /// 最大协程数
        /// </summary>
        public int MaxCoroutineCount = 1024;

        /// <summary>
        /// 当前协程数
        /// </summary>
        private int CurrentCoroutineCount;

        /// <summary>
        /// 当前已缓存的资源内存大小
        /// </summary>
        public float LoadMemorySize = 0;


        /// <summary>
        /// 0引用的缓存区大小 单KB (垃圾桶容积 20M)
        /// </summary>
        public float MaxZeroRefCacheSize = 20480;

        /// <summary>
        /// 运行时0引用的缓存区大小
        /// </summary>
        public int CurrentZeroRefCacheSize = 0;

        public int m_ReadyRequestCount = 0;
        public int ProcessCount = 0;
        public int ProcessingCount = 0;

        public string Line0 = "---------------------------------------------";
        public string AssetBundlePath = string.Empty;
        public string AassetState;
        public string Line1 = "---------------------------------------------";
        public string[] AssetsDebug;


        private FrameDef.eResManagerState mInit = FrameDef.eResManagerState.NoStart;

        /// <summary>
        /// 就绪请求字典
        /// </summary>
        internal readonly Dictionary<FrameDef.TaskPriority, List<FRequest>> ReadyRequestDic = new Dictionary<FrameDef.TaskPriority, List<FRequest>>();

        /// <summary>
        /// 处理字典，关键字为加载路径
        /// </summary>
        internal readonly Dictionary<string, FProcess> ProcessDic = new Dictionary<string, FProcess>();

        /// <summary>
        /// 场景处理链表
        /// </summary>
        internal readonly List<FSceneProcess> SceneProcessList = new List<FSceneProcess>();

        /// <summary>
        /// 资源回收站
        /// </summary>
        internal readonly List<FResourceUnit> RecycleBin = new List<FResourceUnit>();

        /// <summary>
        /// 加载的资源信息 关键字为加载路径
        /// </summary>
        internal readonly Dictionary<string, FResourceUnit> LoadedResourceUnitDic = new Dictionary<string, FResourceUnit>();

        /// <summary>
        /// 缓存链表
        /// </summary>
        internal readonly List<FResourceRef> CacheList = new List<FResourceRef>();

        /// <summary>
        /// 关键字为资源类型
        /// </summary>
        internal Dictionary<Type, Dictionary<string, FAssetInfo>> AssetInfoDic;

        internal Dictionary<string, FAssetInfo> AllAssetInfoDic;

        /// <summary>
        /// 关键字为包路径
        /// </summary>
        internal Dictionary<string, FAssetBundleData> AssetBundleDataDic;

        private Dictionary<string, Shader> m_AllShaderDic = new Dictionary<string, Shader>();

        public List<string> WaitAssetList = new List<string>();


        public class ResUnitStateData
        {
            public FrameDef.ResUnitStateType type = FrameDef.ResUnitStateType.end;
            public string info;

        }

        void Awake()
        {
            DontDestroyOnLoad(gameObject);

        }


        private void Update()
        {
            if (mInit < FrameDef.eResManagerState.Prepare)
            {
                return;
            }

            m_ReadyRequestCount = ReadyRequestCount;
            if (DebugMode)
            {                
                ProcessCount = ProcessDic.Count;
                ProcessingCount = ProcessDic.Values.ToList().FindAll(v => v.State == FrameDef.AsyncState.Loading).Count;
                DebugLog(false);
            }

            if (m_ReadyRequestCount > 0)
            {
                if (CurrentCoroutineCount <= MaxCoroutineCount)
                {
                    int _leftCount = MaxCoroutineCount - CurrentCoroutineCount;
                    int _targetCount = m_ReadyRequestCount < _leftCount ? m_ReadyRequestCount : _leftCount;
                    for (int i = 0; i < _targetCount; i++)
                    {
                        handleRequest();
                    }
                }
            }

            if (ProcessDic.Count > 0)
            {
                lock (locker)
                {
                    int _count = ProcessDic.Count;
                    FProcess[] _array = new FProcess[_count];
                    ProcessDic.Values.CopyTo(_array, 0);
                    for (int i = 0; i < _array.Length; ++i)
                    {
                        _array[i].TryLoad();
                    }
      

                }
            }

            if (SceneProcessList.Count > 0)
            {
                int _count = SceneProcessList.Count;
                for (int i = 0; i < _count; ++i)
                {
                    SceneProcessList[i].TryLoad();

                }
            }



            //释放资源
            if (CurrentZeroRefCacheSize >= MaxZeroRefCacheSize ||
                CrossRefBundleReleaseType == FrameDef.eCrossRefBundleReleaseType.ReleaseAssetBundleAndAsset)
            {
                ClearRecycleBin();
            }


        }

        /// <summary>
        /// 初始化资源管理器
        /// </summary>
        /// <param name="callback">成功后的回调</param>

        public void Init(CallBack callback = null)
        {

            ClearAllAssets();
            ReadyRequestDic.Clear();
            ProcessDic.Clear();
            SceneProcessList.Clear();
            RecycleBin.Clear();
            LoadedResourceUnitDic.Clear();
            CacheList.Clear();

            for (int i = 0; i < 5; ++i)
            {
                ReadyRequestDic.Add(GetPriority(i), new List<FRequest>());
            }

            if (mInit > FrameDef.eResManagerState.NoStart)
                return;

            //使用assetbundle打包功能
            if (UsedAssetBundle)
            {
                //读取依赖关系网
                string _finalPath = FResourceCommon.GetURLPath("StreamingResources/" + FrameDef.AssetBundleData.ToLower() + FResourceCommon.assetbundleFileSuffix,
                    string.Empty, string.Empty);
                if (!System.IO.File.Exists(_finalPath))
                {
                    _finalPath = FResourceCommon.GetStreamingAssetsPath("StreamingResources/" + FrameDef.AssetBundleData.ToLower()) + FResourceCommon.assetbundleFileSuffix;
                }

                AssetBundle _assetBundle = AssetBundle.LoadFromFile(_finalPath);
                AssetBundleData _assetBundleTxt = _assetBundle.LoadAsset(FrameDef.AssetBundleData, typeof(AssetBundleData)) as AssetBundleData;
                parseAssetBundleData(_assetBundleTxt);
                parseManiFest(_assetBundleTxt);

                //TextAsset _assetBundleTxt = _assetBundle.LoadAsset(FrameDef.AssetBundleText, typeof(TextAsset)) as TextAsset;
                //parseAssetBundleData(_assetBundleTxt.text);
                //TextAsset _maniFestText = _assetBundle.LoadAsset(FrameDef.ManiFestText, typeof(TextAsset)) as TextAsset;
                //parseManiFest(_maniFestText.text);

                _assetBundle.Unload(true);

                LoadObject(FrameDef.AllShaders, typeof(GameObject), (matAll) =>
                {
                    FResourceUnit _allShaderUnit = LoadedResourceUnitDic[FrameDef.AllShaders.ToLower() + FResourceCommon.assetbundleFileSuffix];
                    _allShaderUnit.StayMemory = true;

                    Object[] _allShader = _allShaderUnit.AllAssets;
                    ShaderVariantCollection _collect = null;
                    for (int i = 0; i < _allShader.Length; ++i)
                    {
                        Shader _shader = _allShader[i] as Shader;
                        if (null != _shader)
                        {
                            if (!m_AllShaderDic.ContainsKey(_shader.name))
                            {
                                m_AllShaderDic.Add(_shader.name, _shader);
                            }
                        }
                        if( _allShader [i] is ShaderVariantCollection)
                        {
                            _collect = _allShader[i] as ShaderVariantCollection;
                        }
                    }

                    MonoBehaviour.Destroy((matAll as FResourceRefKeeper).gameObject);

                    if(null !=_collect)
                    {
                        _collect.WarmUp();
                        Debug.LogError("ShaderVariantCollection WarmUp.");
                    }

                    //Shader.WarmupAllShaders();

                    mInit = FrameDef.eResManagerState.Ready;

                    if (callback != null)
                    {
                        callback();
                    }

                }, true);


                mInit = FrameDef.eResManagerState.Prepare;
            }
            else
            {
                mInit = FrameDef.eResManagerState.Ready;
                if (callback != null)
                {
                    callback();
                }
            }



        }

        internal void AddCoroutinCount()
        {

            ++CurrentCoroutineCount;

        }
        internal void ReduceCoroutinCount()
        {

            --CurrentCoroutineCount;

        }

        #region Request


        internal bool ExistRequest(string loadPath)
        {
            if (string.IsNullOrEmpty(loadPath))
            {
                return false;
            }
            int _length = FrameDef.TaskPriority.end.GetHashCode();
            List<FRequest> _list = null;
            for (int i = 0; i < _length; ++i)
            {
                _list = ReadyRequestDic[GetPriority(i)];

                for (int j = 0; j < _list.Count; ++j)
                {
                    if (_list[j].LoadPath == loadPath)
                    {
                        return true;
                    }
                }

            }


            return false;
        }

        internal FRequest FindRequest(string loadPath)
        {
            if (string.IsNullOrEmpty(loadPath))
            {
                return null;
            }
            FRequest _request = null;
            int _length = FrameDef.TaskPriority.end.GetHashCode();
            List<FRequest> _list = null;
            for (int i = 0; i < _length; ++i)
            {
                _list = ReadyRequestDic[GetPriority(i)];

                for (int j = 0; j < _list.Count; ++j)
                {
                    if (_list[j].LoadPath == loadPath)
                    {
                        _request = _list[j];
                        break;
                    }
                }
            }

            return _request;
        }

        /// <summary>
        /// 生成请求
        /// </summary>
        /// <param name="keyInfo"></param>
        /// <param name="handle"></param>
        /// <param name="LoadAsync"></param>
        /// <param name="isCache"></param>
        /// <param name="StayMemory"></param>
        /// <param name="Priority"></param>
        /// <returns></returns>
        internal FRequest CreateRequest(string loadPath, KeyInfo keyInfo, CallBack<object> handle, bool LoadAsync, bool isCache = false, bool StayMemory = false, FrameDef.TaskPriority Priority = FrameDef.TaskPriority.Normal)
        {

            FRequest _request = FindRequest(loadPath);
            if (null != _request)
            {
                _request.Priority = _request.Priority.GetHashCode() > Priority.GetHashCode() ? Priority : _request.Priority;
                if (null != handle && null != keyInfo)
                {
                    List<CallBack<object>> _list = new List<CallBack<object>> { handle };
                    _request.AddCallBack(keyInfo.path, keyInfo.type, _list);
                }

                _request = null;
            }
            else
            {
                FProcess _targetProcess = FindProcess(loadPath);
                if (null != _targetProcess && _targetProcess.State != FrameDef.AsyncState.Done)
                {
                    if (null != handle && null != keyInfo)
                    {
                        List<CallBack<object>> _list = new List<CallBack<object>> { handle };
                        _targetProcess.AddCallBack(keyInfo.path, keyInfo.type, _list);
                    }
                    if (!isCache)
                    {
                        _targetProcess.isCache = isCache;
                    }

                }
                else
                {

                    _request = new FRequest(keyInfo, UsedAssetBundle, GetFAssetBundleData(loadPath), LoadAsync, isCache, StayMemory, Priority);

                    if (null != handle && null != keyInfo)
                    {
                        List<CallBack<object>> _list = new List<CallBack<object>> { handle };
                        _request.AddCallBack(keyInfo.path, keyInfo.type, _list);
                    }
                }
            }


            return _request;
        }
        #endregion

        #region Process
        internal bool ExistProcess(string loadPath)
        {
            if (string.IsNullOrEmpty(loadPath))
            {
                return false;
            }
            return ProcessDic.ContainsKey(loadPath);
        }

        internal FProcess FindProcess(string loadPath)
        {
            if (string.IsNullOrEmpty(loadPath))
            {
                return null;
            }
            FProcess _targetProcess = null;
            ProcessDic.TryGetValue(loadPath, out _targetProcess);

            return _targetProcess;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        internal FProcess CreateProcess(FRequest request)
        {
            if (null == request)
            {
                return null;
            }

            FProcess _targetProcess = FindProcess(request.LoadPath);

            bool LoadAsync = request.LoadAsync;
            bool isCache = request.isCache;
            FrameDef.TaskPriority Priority = request.Priority;
            bool StayMemory = request.StayMemory;

            if (null != _targetProcess && _targetProcess.State != FrameDef.AsyncState.Done)
            {
                _targetProcess.AddCallBack(request.CallBackDic);

                if (!request.isCache)
                {
                    _targetProcess.isCache = request.isCache;
                }

            }
            else
            {
                //新建进程
                _targetProcess = new FProcess(request, this);
                List<string> _AllDependencies = request.AssetBundleData.AssetBundlePathList;

                //处理依赖
                if (_AllDependencies.Count > 0)
                {
                    for (int i = 0; i < _AllDependencies.Count; i++)
                    {
                        _targetProcess.AllChildrenDependenceList.Add(_AllDependencies[i]);
                    }
                    for (int i = 0; i < _AllDependencies.Count; i++)
                    {
                        string _loadPath = _AllDependencies[i];

                        _loadObject(_loadPath, null, null, LoadAsync, false, FrameDef.TaskPriority.Highest, StayMemory);

                    }
                }
                else
                {
                    _targetProcess.State = FrameDef.AsyncState.Ready;
                }

            }

            return _targetProcess;
        }

        #endregion

        /// <summary>
        /// 调试输出,需要debugModel=true
        /// </summary>
        public string[] DebugLog(bool logOut)
        {
            List<ResUnitStateData> _ResUnitStateDataList = new List<FResourcesManager.ResUnitStateData>();

            List<FResourceUnit> _list = LoadedResourceUnitDic.Values.ToList();

            _list = _list.OrderBy(v => v.LoadPath).ToList();

            int _count = _list.Count;
            AssetsDebug = new string[_count];
            int index = 0;
            for (int i = 0; i < _count; ++i)
            {
                ResUnitStateData _data = new FResourcesManager.ResUnitStateData();
                index = i;
                FResourceUnit _unit = _list[i];
                string _loadPath = _unit.LoadPath;
                if (RecycleBin.Contains(_unit))
                {
                    AssetsDebug[index] = "[删除队列]  " + " 引用计数:" + _unit.ReferenceCount + " " + _unit.LoadPath;
                    _data.type = FrameDef.ResUnitStateType.Delet;
                }
                else if (_unit.isCache)
                {
                    AssetsDebug[index] = "[缓存资源]  " + " 引用计数:" + _unit.ReferenceCount + " " + _unit.LoadPath;

                }
                else if (_unit.StayMemory)
                {
                    AssetsDebug[index] = "[常驻内存]  " + " 引用计数:" + _unit.ReferenceCount + " " + _unit.LoadPath;

                }
                else if (_unit.m_AssetBundle != null && _unit.ReferenceCount != 0)
                {
                    AssetsDebug[index] = "[正常引用]  " + " 引用计数:" + _unit.ReferenceCount + " " + _unit.LoadPath;

                }
                else if (_unit.m_AssetBundle == null && _unit.ReferenceCount > 0)
                {
                    AssetsDebug[index] = "[包已卸载]  " + " 引用计数:" + _unit.ReferenceCount + " " + _unit.LoadPath;

                }
                else if (_unit.StayMemory)
                {
                    AssetsDebug[index] = "[常驻内存]  " + " 引用计数:" + _unit.ReferenceCount + " " + _unit.LoadPath;

                }

                else
                {
                    AssetsDebug[index] = "[未知状态]  " + " 引用计数:" + _unit.ReferenceCount + " " + _unit.LoadPath;
                }

                showState(_loadPath, AssetsDebug[index]);
            }

            if (!_list.Exists(v => v.LoadPath == AssetBundlePath))
            {
                AassetState = "不存在 " + AssetBundlePath;
            }

            WaitAssetList = WaitAssetList.OrderBy(v => v).ToList();

            return AssetsDebug;
        }

        public List<ResUnitStateData> DebugLog1()
        {
            List<ResUnitStateData> _ResUnitStateDataList = new List<FResourcesManager.ResUnitStateData>();

            List<FResourceUnit> _list = LoadedResourceUnitDic.Values.ToList();

            _list = _list.OrderBy(v => v.LoadPath).ToList();

            int _count = _list.Count;
            AssetsDebug = new string[_count];
            int index = 0;
            for (int i = 0; i < _count; ++i)
            {
                ResUnitStateData _data = new FResourcesManager.ResUnitStateData();
                index = i;
                FResourceUnit _unit = _list[i];
                string _loadPath = _unit.LoadPath;
                if (RecycleBin.Contains(_unit))
                {
                    AssetsDebug[index] = "[删除队列]  " + " 引用计数:" + _unit.ReferenceCount + " " + _unit.LoadPath;
                    _data.type = FrameDef.ResUnitStateType.Delet;
                }
                else if (_unit.isCache)
                {
                    AssetsDebug[index] = "[缓存资源]  " + " 引用计数:" + _unit.ReferenceCount + " " + _unit.LoadPath;
                    _data.type = FrameDef.ResUnitStateType.Cache;
                }
                else if (_unit.StayMemory)
                {
                    AssetsDebug[index] = "[常驻内存]  " + " 引用计数:" + _unit.ReferenceCount + " " + _unit.LoadPath;
                    _data.type = FrameDef.ResUnitStateType.Stay;
                }
                else if (_unit.m_AssetBundle != null && _unit.ReferenceCount != 0)
                {
                    AssetsDebug[index] = "[正常引用]  " + " 引用计数:" + _unit.ReferenceCount + " " + _unit.LoadPath;
                    _data.type = FrameDef.ResUnitStateType.Normal;
                }
                else if (_unit.m_AssetBundle == null && _unit.ReferenceCount > 0)
                {
                    AssetsDebug[index] = "[包已卸载]  " + " 引用计数:" + _unit.ReferenceCount + " " + _unit.LoadPath;
                    _data.type = FrameDef.ResUnitStateType.Unloaded;
                }
                else
                {
                    AssetsDebug[index] = "[未知状态]  " + " 引用计数:" + _unit.ReferenceCount + " " + _unit.LoadPath;
                    _data.type = FrameDef.ResUnitStateType.Unknown;
                }

                _data.info = AssetsDebug[index];
                _ResUnitStateDataList.Add(_data);
            }

            return _ResUnitStateDataList;
        }

        private void showState(string loadPath, string state)
        {
            if (AssetBundlePath == loadPath)
            {
                AassetState = state;
            }

        }

        internal void AddToRecycleBin(FResourceUnit unit)
        {
            lock (locker)
            {
                if (null == unit)
                {
                    return;
                }

                int _size = (int)unit.Size;
                if (!RecycleBin.Contains(unit))
                {

                    RecycleBin.Add(unit);

                    CurrentZeroRefCacheSize += _size;
                }
            }

        }

        internal void RemoveFromeRecycleBin(string loadPath)
        {
            lock (locker)
            {
                FResourceUnit _unit = null;
                if (LoadedResourceUnitDic.TryGetValue(loadPath, out _unit))
                {
                    RemoveFromeRecycleBin(_unit);
                }
            }
        }

        internal void RemoveFromeRecycleBin(FResourceUnit unit)
        {
            lock (locker)
            {
                if (null == unit)
                {
                    return;
                }

                if (RecycleBin.Contains(unit))
                {
                    RecycleBin.Remove(unit);

                    CurrentZeroRefCacheSize -= (int)unit.Size;
                }

            }
        }

        internal void AddToCacheList(FResourceRef keeper)
        {
            if (null != CacheList && !CacheList.Contains(keeper))
            {
                CacheList.Add(keeper);
            }

        }
        internal void RemoveFromCacheList(FResourceRef keeper)
        {
            if (null != CacheList)
            {
                CacheList.Remove(keeper);
            }
        }
        internal void RemoveFromCacheList(FResourceUnit unit)
        {
            if (null != unit && null != CacheList && CacheList.Count > 0)
            {
                FResourceRef _ref = CacheList.Find(v => v.resUnit == unit);
                RemoveFromCacheList(_ref);
            }

        }

        internal bool IsInCacheList(FResourceUnit unit)
        {
            return null != CacheList && null != unit && CacheList.Exists(v => v.resUnit == unit);
        }

        #region LegacyLoadWay



        #endregion

        #region 缓存资源


        /// <summary>
        /// 缓存目标资源，回调里的 object 为 null
        /// </summary>
        /// <param name="path"></param>
        /// <param name="handle"></param>
        public void CacheObject(string path, Type type, CallBack<object> handle, FrameDef.TaskPriority priority = FrameDef.TaskPriority.Low)
        {
            LoadObject(path, null == type || type == typeof(System.Object) ? typeof(UnityEngine.Object) : type, handle, true, true, priority, false);
        }
        #endregion

        #region 加载资源
        public void LoadObject(string assetPath, CallBack<object> handle)
        {
            LoadObject(assetPath, typeof(UnityEngine.Object), handle, false, false, FrameDef.TaskPriority.Normal, false);
        }

        public void LoadObject(string assetPath, FrameDef.TaskPriority priority, CallBack<object> handle)
        {
            LoadObject(assetPath, typeof(UnityEngine.Object), handle, false, false, priority, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="handle"></param>
        /// <param name="stayMemory">常驻内存，不会被清理</param>
        public void LoadObject(string assetPath, CallBack<object> handle, bool stayMemory)
        {
            LoadObject(assetPath, typeof(UnityEngine.Object), handle, false, false, FrameDef.TaskPriority.Normal, stayMemory);
        }

        public void LoadObjectAsync(string assetPath, CallBack<object> handle, FrameDef.TaskPriority priority = FrameDef.TaskPriority.Normal)
        {
            LoadObject(assetPath, typeof(UnityEngine.Object), handle, true, false, priority, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="resType"></param>
        /// <param name="handle"></param>
        /// <param name="async"></param>
        /// <param name="isCache">是否为缓存操作</param>
        /// <param name="priority"></param>
        /// <param name="stayMemory"></param>
        public void LoadObject(string assetPath, Type type, CallBack<object> handle, bool async, bool isCache = false, FrameDef.TaskPriority priority = FrameDef.TaskPriority.Normal, bool stayMemory = false)
        {

            loadObject(assetPath, type, handle, async, isCache, priority, stayMemory, true);
        }

        private void loadObject(string assetPath, Type type, CallBack<object> handle, bool async, bool isCache = false, FrameDef.TaskPriority priority = FrameDef.TaskPriority.Normal, bool stayMemory = false, bool isMainRequest = false)
        {
            if (string.IsNullOrEmpty(assetPath))
            {

                return;
            }

            FAssetInfo _assetInfo = GetInfo(assetPath, type);

            if (null == _assetInfo)
            {
                Debug.LogError("Can not find " + assetPath);
                if (null != handle)
                {
                    handle(null);
                }
                return;
            }
            if (type == typeof(UnityEngine.Object))
            {
                type = _assetInfo.AssetType;
            }

            _loadObject(_assetInfo.LoadPath, new KeyInfo(assetPath, type), handle, async, isCache, priority, stayMemory, isMainRequest);


        }

        private void _loadObject(string loadPath, KeyInfo keyInfo, CallBack<object> handle, bool async, bool isCache = false, FrameDef.TaskPriority priority = FrameDef.TaskPriority.Normal, bool stayMemory = false, bool isMainRequest = false)
        {
            if (string.IsNullOrEmpty(loadPath))
            {

                return;
            }


            AssetNeedLoad(loadPath);

            lock (locker)
            {
                FResourceUnit _unit = null;
                if (LoadedResourceUnitDic.TryGetValue(loadPath, out _unit))
                {

                    if (!isCache)
                    {
                        if (IsInCacheList(_unit))
                        {
                            _unit.isCache = false;
                            RemoveFromCacheList(_unit);
                        }
                    }

                    RemoveFromeRecycleBin(_unit);

                    for (int i = 0; i < _unit.AssetBundleData.AssetBundlePathList.Count; ++i)
                    {
                        string _key = _unit.AssetBundleData.AssetBundlePathList[i];
                        FResourceUnit _childUnit = null;
                        if (LoadedResourceUnitDic.TryGetValue(_key, out _childUnit))
                        {
                            RemoveFromeRecycleBin(_childUnit);
                            _unit.AddResourceRefToDic(_key, _childUnit.BeUsed());
                        }
                        else
                        {
                            UnityEngine.Debug.LogError("异常情况：依赖资源已丢失，" + "（调用者：" + _unit.LoadPath + "），" + "子资源：" + _key);
                        }
                    }

                    FEventManager.Inst.DispatchEvent(FrameDef.ResourcesProcessType.AssetCached, loadPath, _unit);
                    FEventManager.Inst.DispatchEvent(FrameDef.ResourcesProcessType.AssetLoaded, loadPath, _unit);
                    AssetLoad(loadPath);

                    if (null != handle && null != keyInfo)
                    {
                        handle(_unit.GetInstance(keyInfo, isCache));
                    }


                    return;
                }
            }



            //生成请求
            FRequest _req = CreateRequest(loadPath, keyInfo, handle, async, isCache, stayMemory, priority);

            if (null != _req)
            {
                _req.isMainRequest = isMainRequest;
                //装入请求队列等待读取
                ReadyRequestDic[_req.Priority].Add(_req);
            }


        }


        #endregion

        #region 卸载资源

        public void UnloadAsset(string path, bool flag, CallBack callBack)
        {
            FResourceUnit _unit = null;
            FAssetInfo _info = GetInfo(path, FResourceCommon.Object);
            if (LoadedResourceUnitDic.TryGetValue(_info.LoadPath, out _unit))
            {

                LoadedResourceUnitDic.Remove(_info.LoadPath);
                if (flag)
                {
                    RemoveFromeRecycleBin(_unit);
                    RemoveFromCacheList(_unit);
                    _unit.Dispose();
                }
                else
                {
                    _unit.UnloadAssetBundle(false);

                }
            }
        }

        public void UnloadAssets(List<string> pathList, bool flag, CallBack callBack)
        {
            if (null != pathList)
            {
                for (int i = 0; i < pathList.Count; ++i)
                {
                    UnloadAsset(pathList[i], flag, null);
                }
            }
            if (null != callBack)
            {
                callBack();
            }
        }

        /// <summary>
        /// 释放无用资源，效率低
        /// </summary>
        /// <param name="callBack"></param>
        public void UnloadUnusedAssets(CallBack callBack)
        {
            StartCoroutine(unloadUnusedAssets(callBack));

        }

        private IEnumerator unloadUnusedAssets(CallBack callBack)
        {
            AsyncOperation _as = Resources.UnloadUnusedAssets();//
            yield return _as;
            if (null != callBack)
            {
                callBack();
            }
        }

        #endregion

        #region 加载场景
        public void LoadScene(string name, CallBack handle, LoadSceneMode mode)
        {
            loadScene(name, handle, false, mode);

        }

        public void LoadSceneAsync(string name, CallBack handle, LoadSceneMode mode)
        {
            loadScene(name, handle, true, mode);
        }



        private void loadScene(string name, CallBack handle, bool async, LoadSceneMode mode)
        {

            List<CallBack> _callBackList = new List<CallBack>();
            _callBackList.Add(handle);
            FSceneProcess _process = new FSceneProcess(name, this, _callBackList, async, mode);

            SceneProcessList.Add(_process);
            _process.ProcessState = FrameDef.AsyncState.Ready;



        }

        #endregion

        #region 卸载场景


        public void UnloadScene(string name, CallBack handle)
        {
            StartCoroutine(unLoadScene(name, handle));
        }
        public void UnloadScene(Scene scene, CallBack handle)
        {
            StartCoroutine(unLoadScene(scene, handle));
        }
        public void UnloadScene(int sceneBuildIndex, CallBack handle)
        {
            StartCoroutine(unLoadScene(sceneBuildIndex, handle));
        }

        private IEnumerator unLoadScene(string name, CallBack handle)
        {

            AsyncOperation _asyncOperation = SceneManager.UnloadSceneAsync(name);
            yield return _asyncOperation;
            UnloadUnusedAssets(null);
            if (null != handle)
            {

                handle();
            }

        }
        private IEnumerator unLoadScene(Scene scene, CallBack handle)
        {

            AsyncOperation _asyncOperation = SceneManager.UnloadSceneAsync(scene);
            yield return _asyncOperation;
            UnloadUnusedAssets(null);
            if (null != handle)
            {
                handle();
            }

        }
        private IEnumerator unLoadScene(int sceneBuildIndex, CallBack handle)
        {

            AsyncOperation _asyncOperation = SceneManager.UnloadSceneAsync(sceneBuildIndex);
            yield return _asyncOperation;
            UnloadUnusedAssets(null);
            if (null != handle)
            {
                handle();
            }

        }
        #endregion


        public void ClearAllAssets()
        {

            using (var item = LoadedResourceUnitDic.GetEnumerator())
            {
                while (item.MoveNext())
                {
                    FResourceUnit value = item.Current.Value;
                    if (!value.StayMemory)
                    {
                        RemoveFromeRecycleBin(value);

                        LoadedResourceUnitDic.Remove(value.LoadPath);
                        value.Dispose();
                        LoadMemorySize -= value.Size;
                    }
                }
                item.Dispose();
            }



        }

        /// <summary>
        /// 清理缓存资源
        /// </summary>
        public void ClearCacheAssets()
        {

            for (int i = 0; i < CacheList.Count; ++i)
            {
                FResourceUnit _unit = CacheList[i].resUnit;
                RemoveFromCacheList(_unit);
                AddToRecycleBin(_unit);
                i--;

            }
        }

        /// <summary>
        /// 清理回收站
        /// </summary>
        public void ClearRecycleBin()
        {

            lock (locker)
            {
                if (0 == RecycleBin.Count)
                {
                    return;
                }

                for (int i = 0; i < RecycleBin.Count; ++i)
                {
                    FResourceUnit _unit = RecycleBin[i];

                    if (null != _unit)
                    {
                        if (0 != _unit.ParentList.Count)
                        {
                            continue;
                        }

                        RemoveFromeRecycleBin(_unit);
                        i--;

                        int _length = LoadedResourceUnitDic.Count;
                        FResourceUnit[] _array = new FResourceUnit[_length];
                        LoadedResourceUnitDic.Values.CopyTo(_array, 0);
                        for (int j = 0; j < _length; ++j)
                        {
                            _array[j].ResourceUnitDisposed(_unit);
                        }

                        //using (var item = LoadedResourceUnitDic.GetEnumerator())
                        //{
                        //    while (item.MoveNext())
                        //    {
                        //        FResourceUnit value = item.Current.Value;
                        //        value.ResourceUnitDisposed(_unit);
                        //    }
                        //    item.Dispose();
                        //}


                        if (0 == _unit.ReferenceCount)
                        {
                            LoadedResourceUnitDic.Remove(_unit.LoadPath);

                            LoadMemorySize -= _unit.Size;

                            _unit.Dispose();
                        }
                    }
                }
            }

        }

        /// <summary>
        /// 清理资源管理器
        /// </summary>
        public void ClearResouresManager()
        {
            ClearRecycleBin();
            ClearCacheAssets();
        }

        /// <summary>
        /// 总 就绪请求数量
        /// </summary>
        private int ReadyRequestCount
        {
            get
            {
                int _count = 0;
                int _length = 5;
                for (int i = 0; i < _length; ++i)
                {
                    _count += ReadyRequestDic[GetPriority(i)].Count;
                }
                return _count;
            }
        }

        private FrameDef.TaskPriority GetPriority(int value)
        {
            FrameDef.TaskPriority _priority = FrameDef.TaskPriority.Normal;
            switch (value)
            {
                case 0:
                    {
                        _priority = FrameDef.TaskPriority.Highest;
                    }
                    break;
                case 1:
                    {
                        _priority = FrameDef.TaskPriority.High;
                    }
                    break;
                case 2:
                    {
                        _priority = FrameDef.TaskPriority.Normal;
                    }
                    break;
                case 3:
                    {
                        _priority = FrameDef.TaskPriority.Low;
                    }
                    break;
                case 4:
                    {
                        _priority = FrameDef.TaskPriority.Lowest;
                    }
                    break;
            }

            return _priority;
        }

        private bool isBaseRes(string assetPath)
        {

            if (assetPath == FrameDef.AllScripts ||
                assetPath == FrameDef.AllShaders ||
                assetPath == FrameDef.ManiFest)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// 传入Type，更快查找
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public FAssetInfo GetInfo(string assetPath, Type type)
        {
            FAssetInfo _info = null;
            if (!UsedAssetBundle)
            {
                _info = new FAssetInfo(assetPath, UsedAssetBundle);
                _info.Size = 1;
                _info.AssetBundleData = new FAssetBundleData();
                _info.AssetBundleData.Path = assetPath;
                _info.AssetBundleData.AssetBundlePathList = null;

                //todo 同文件夹下同名不同后缀有风险
                _info.AssetType = FResourceCommon.Object;
                return _info;
            }

            if (isBaseRes(assetPath))
            {


                _info = new FAssetInfo(assetPath, UsedAssetBundle);
                _info.IsCommonRes = true;

                _info.AssetBundleData = new FAssetBundleData();
                _info.AssetBundleData.Common = true;
                _info.AssetBundleData.Path = assetPath.ToLower() + FResourceCommon.assetbundleFileSuffix;
                _info.AssetBundleData.AssetBundlePathList = null;

                _info.AssetType = typeof(UnityEngine.GameObject);
                return _info;
            }

            if (type == FResourceCommon.Object)
            {
                if (AllAssetInfoDic.TryGetValue(assetPath, out _info))
                {
                    return _info;
                }
            }
            else
            {
                Dictionary<string, FAssetInfo> _dicValue = null;
                if (AssetInfoDic.TryGetValue(type, out _dicValue))
                {
                    if (_dicValue.TryGetValue(assetPath, out _info))
                    {
                        return _info;
                    }
                }

                if (null == _info)
                {
                    if (DebugMode)
                    {
                        UnityEngine.Debug.LogError(assetPath + " 类型差异 " + type.ToString());
                    }
                    if (AllAssetInfoDic.TryGetValue(assetPath, out _info))
                    {
                        return _info;
                    }
                }
            }

            return _info;
        }

        /// <summary>
        /// 处理请求，生成处理任务，加入就绪任务列表
        /// </summary>
        private void handleRequest()
        {
            FRequest _Request = null;
            for (int i = 0; i < 5; ++i)
            {
                FrameDef.TaskPriority _type = GetPriority(i);

                if (ReadyRequestDic[_type].Count > 0)
                {
                    _Request = ReadyRequestDic[_type][0];

                    ReadyRequestDic[_type].RemoveAt(0);
                    if (null != _Request)
                    {
                        break;
                    }
                }
            }

            if (_Request == null)
            {
                return;
            }

            FProcess _Process = CreateProcess(_Request);
            if (null != _Process)
            {
                _Request.isDone = true;
                if (!ProcessDic.ContainsKey(_Process.LoadPath))
                {
                    ProcessDic.Add(_Process.LoadPath, _Process);
                }
            }
        }
        
        /// <summary>
        /// 需要被加载的资源
        /// </summary>
        /// <param name="assetPath"></param>
        public void AssetNeedLoad(string assetPath)
        {
            if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor)
            {
                lock (locker)
                {
                    if (!WaitAssetList.Exists(v => v == assetPath))
                    {
                        WaitAssetList.Add(assetPath);
                    }
                }
            }
        }

        /// <summary>
        /// 资源已被加载
        /// </summary>
        /// <param name="assetPath"></param>
        public void AssetLoad(string assetPath)
        {
            if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor)
            {
                lock (locker)
                {
                    if (WaitAssetList.Exists(v => v == assetPath))
                    {
                        WaitAssetList.Remove(assetPath);
                    }
                }
            }
        }

        private void parseAssetBundleData(string txt)
        {
            float _time = Time.realtimeSinceStartup;
            AssetBundleDataDic = new Dictionary<string, FAssetBundleData>();
            string[] _array = txt.Split(new char[2] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            int _length = _array.Length;
            if (_length > 1)
            {
                for (int i = 1; i < _length; ++i)
                {
                    FAssetBundleData _data = new FAssetBundleData();
                    string[] _bundleData = _array[i].Split('\t');
                    _data.Common = "1" == _bundleData[1];

                    //所依赖的包路径
                    string[] _assetArray = _bundleData[2].Split(new char[1] { '+' }, StringSplitOptions.RemoveEmptyEntries);
                    List<string> _assetList = _assetArray.ToList();
                    _data.Init(_bundleData[0],
                        _assetList,
                        FUtils.StrToInt(_bundleData[3]),
                        FUtils.StrToInt(_bundleData[4]),
                        FUtils.StrTouint(_bundleData[5]),
                        FUtils.StrTouint(_bundleData[6]));
                    string _key = _data.Path;
                    if (!AssetBundleDataDic.ContainsKey(_key))
                    {
                        AssetBundleDataDic.Add(_key, _data);
                    }
                    else
                    {
                        Debug.LogError("重复的AB包路径：" + _data.Path);
                    }
                }
            }
            if (DebugMode)
                UnityEngine.Debug.Log("解析AssetBundleManiFest耗时：" + (Time.realtimeSinceStartup - _time));
        }

        /// <summary>
        /// ManiFest内容转换为AssetInfo
        /// </summary>
        /// <param name="txt"></param>
        private void parseManiFest(string txt)
        {
            float _time = Time.realtimeSinceStartup;
            AssetInfoDic = new Dictionary<Type, Dictionary<string, FAssetInfo>>();
            AllAssetInfoDic = new Dictionary<string, FAssetInfo>();
            string[] _array = txt.Split(new char[2] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            int _length = _array.Length;
            if (_length > 1)
            {
                for (int i = 1; i < _length; ++i)
                {
                    string[] _childArray = _array[i].Split('\t');

                    string _assetPath = _childArray[0];
                    FAssetInfo _info = new FAssetInfo(FResourceCommon.DeletSuffix(_assetPath), UsedAssetBundle);
                    string _fullName = _childArray[1];
                    _info.AssetType = FResourceCommon.GetAssetType(_fullName);
                    _info.IsCommonRes = "1" == _childArray[2];
                    _info.IsSolid = "1" == _childArray[3];
                    string _assetBundlePath = _childArray[4];
                    _info.AssetBundleData = AssetBundleDataDic[_assetBundlePath];
                    _info.Size = _info.AssetBundleData.Size * 0.0009765625f;
                    _info.AssetBundleData.IsSolid = _info.IsSolid;
                    Dictionary<string, FAssetInfo> _dicValue = null;

                    if (!AssetInfoDic.TryGetValue(_info.AssetType, out _dicValue))
                    {
                        _dicValue = new Dictionary<string, FAssetInfo>();
                        AssetInfoDic.Add(_info.AssetType, _dicValue);
                    }

                    if (!_dicValue.ContainsKey(_info.AssetPath))
                    {
                        _dicValue.Add(_info.AssetPath, _info);
                    }

                    if (!AllAssetInfoDic.ContainsKey(_info.AssetPath))
                    {
                        AllAssetInfoDic.Add(_info.AssetPath, _info);
                    }
                    else
                    {
                        Log.Error("同名资源: " + _info.AssetPath);
                    }
                }
            }
            if (DebugMode)
                UnityEngine.Debug.Log("解析ManiFest耗时：" + (Time.realtimeSinceStartup - _time));
        }
        private void parseAssetBundleData(AssetBundleData txt)
        {
            float _time = Time.realtimeSinceStartup;
            AssetBundleDataDic = new Dictionary<string, FAssetBundleData>();
            int _length = txt.Path.Count;
            for (int i = 0; i < _length; i++)
            {
                FAssetBundleData _data = new FAssetBundleData();
                _data.Common = txt.Common[i];
                _data.Init(txt.Path[i], txt.AssetBundlePathList[i].Data, 0, 0, 0, 0);
                if (!AssetBundleDataDic.ContainsKey(_data.Path))
                {
                    AssetBundleDataDic.Add(_data.Path, _data);
                }
                else
                {
                    Debug.LogError("重复的AB包路径：" + _data.Path);
                }

            }
            if (DebugMode)
                UnityEngine.Debug.Log("解析AssetBundleManiFest耗时：" + (Time.realtimeSinceStartup - _time));
        }
        private void parseManiFest(AssetBundleData txt)
        {
            float _time = Time.realtimeSinceStartup;
            AssetInfoDic = new Dictionary<Type, Dictionary<string, FAssetInfo>>();
            AllAssetInfoDic = new Dictionary<string, FAssetInfo>();
            int _length = txt.AssetPath.Count;
            for (int i = 0; i < _length; i++)
            {
                FAssetInfo _info = new FAssetInfo(FResourceCommon.DeletSuffix(txt.AssetPath[i]), UsedAssetBundle);
                _info.AssetType = FResourceCommon.GetAssetType(txt.FullName[i]);
                _info.IsCommonRes = txt.IsCommonRes[i];
                _info.IsSolid = txt.IsSolid[i];
                _info.AssetBundleData = AssetBundleDataDic[txt.AssetBundlePath[i]];
                //_info.Size = _info.AssetBundleData.Size * 0.0009765625f;
                _info.AssetBundleData.IsSolid = _info.IsSolid;
                Dictionary<string, FAssetInfo> _dicValue = null;
                if (!AssetInfoDic.TryGetValue(_info.AssetType, out _dicValue))
                {
                    _dicValue = new Dictionary<string, FAssetInfo>();
                    AssetInfoDic.Add(_info.AssetType, _dicValue);
                }

                if (!_dicValue.ContainsKey(_info.AssetPath))
                {
                    _dicValue.Add(_info.AssetPath, _info);
                }

                if (!AllAssetInfoDic.ContainsKey(_info.AssetPath))
                {
                    AllAssetInfoDic.Add(_info.AssetPath, _info);
                }
                else
                {
                    Log.Error("同名资源: " + _info.AssetPath);
                }
            }
            if (DebugMode)
                UnityEngine.Debug.Log("解析ManiFest耗时：" + (Time.realtimeSinceStartup - _time));
        }

        public FAssetBundleData GetFAssetBundleData(string loadPath)
        {
            FAssetBundleData _data = null;
            if (!UsedAssetBundle)
            {
                _data = new FAssetBundleData();
                _data.Path = loadPath;
                _data.AssetBundlePathList = new List<string>();

                return _data;
            }

            AssetBundleDataDic.TryGetValue(loadPath, out _data);
            return _data;
        }

        public Shader GetShader(string name)
        {
            Shader _shader = null;

            if (FResourceCommon.IsEditor() || !UsedAssetBundle)
            {
                _shader = Shader.Find(name);

                if (_shader == null)
                {
                    Log.Debug("shader没找到,如果你是删除了res的打包最终版本,看效果请吧shader复制过来");
                }
            }
            else
            {
                if (!m_AllShaderDic.TryGetValue(name, out _shader))
                {
                    Log.Debug("无法从allshader包里找到名为 " + name + " 的shader");
                    _shader = Shader.Find(name);
                    if (null == _shader)
                    {
                        Log.Debug("无法从默认Shader里找到名为 " + name + " 的Shader");
                    }
                    else
                    {
                        Log.Debug("从默认Shader里找到名为 " + name + " 的Shader");
                    }
                }
            }

            return _shader;
        }

        public static T FindBundlePath<T>(string bundlePath, Dictionary<string, T> dic) where T : HandleBase
        {
            T _exsit = null;
            if (string.IsNullOrEmpty(bundlePath) || null == dic)
            { return _exsit; }
            List<T> _list = new List<T>(dic.Values);
            int _count = _list.Count;
            for (int i = 0; i < _count; ++i)
            {
                if (_list[i].Info.BundlePath == bundlePath)
                {
                    _exsit = _list[i];
                    break;
                }
            }

            return _exsit;
        }
        
        public void SetParent(Transform parent)
        {
            transform.SetParent(parent);
        }

        public void Dispose()
        {
            ClearAllAssets();
            ReadyRequestDic.Clear();
            ProcessDic.Clear();
            SceneProcessList.Clear();
            RecycleBin.Clear();
            LoadedResourceUnitDic.Clear();
            AssetInfoDic.Clear();
            AllAssetInfoDic.Clear();
            AssetBundleDataDic.Clear();
            CacheList.Clear();
        }

        [ContextMenu("清理缓存资源")]
        private void test1()
        {
            ClearCacheAssets();
        }

        [ContextMenu("清理删除队列里的资源")]
        private void test2()
        {
            ClearRecycleBin();
        }
        [ContextMenu("清理资源管理器")]
        private void test3()
        {
            ClearResouresManager();
        }

        [ContextMenu("强制GC")]
        private void test4()
        {
            UnloadUnusedAssets(null);
            GC.Collect();
        }
    }
}