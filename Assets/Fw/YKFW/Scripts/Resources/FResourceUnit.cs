using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using Object = UnityEngine.Object;
using System.IO;

namespace FW
{

    /// <summary>
    /// 资源单位
    /// </summary>
    public class FResourceUnit : IDisposable
    {

        internal FProcess m_process;
        /// <summary>
        ///  AssetBundle包信息
        /// </summary>
        public FAssetBundleData AssetBundleData;

        internal List<FResourceUnit> ParentList;

        /// <summary>
        /// 依赖的下级资源，Dictionary  (资源路径 List(ResourceRef))
        /// </summary>
        internal Dictionary<string, List<FResourceRef>> DependencesAssetDic;

        internal int mReferenceCount;//上级引用计数
        internal int ReferenceCount
        {
            get
            {
                return mReferenceCount;
            }
            set
            {
                mReferenceCount = value;
                mReferenceCount = mReferenceCount < 0 ? 0 : mReferenceCount;
            }
        }

        internal float Size;

        /// <summary>
        /// 是否常驻内存
        /// </summary>
        internal bool StayMemory
        {
            get
            {
                return m_staryMemory;
            }
            set
            {
                m_staryMemory = value;
            }
        }
        private bool m_staryMemory;
        internal bool isCache;
        internal float StartTime;
        internal float LoadTime;
        internal float EndTime;

        /// <summary>
        /// AB包
        /// </summary>
        internal AssetBundle m_AssetBundle;

        /// <summary>
        /// 所有的资源对象
        /// </summary>
        internal Object[] AllAssets;

        /// <summary>
        /// 主资源
        /// </summary>
        internal Object MainAsset;

        /// <summary>
        /// 被加载出来了的资源
        /// </summary>
        internal List<Object> LoadedAssetList;

        /// <summary>
        /// 是否为公共资源
        /// </summary>
        internal bool IsCommonAsset
        {
            get
            {
                return AssetBundleData.Common;
            }
        }

        internal string LoadPath
        {
            get
            {
                return AssetBundleData.Path;
            }
        }
        /// <summary>
        /// 资源的实例化对象
        /// </summary>
        internal object GetInstance(KeyInfo keyInfo, bool cache)
        {
            string assetPath = keyInfo.path;
            string name = FResourceCommon.GetFileName(keyInfo.path, true);
            Type type = keyInfo.type;

            isCache = cache;
            if (!isCache)
            {
                m_ResourcesManager.RemoveFromCacheList(this);
            }
            Object _obj = GetAsset(name, type);
            MainAsset = _obj;
            object _target = null;
            if (null != _obj)
            {
                if (_obj is GameObject)
                {
                    if (cache)
                    {
                        FResourceRef reftmp = BeUsed(assetPath);
                        reftmp.SetAsset(_obj);
                        _target = reftmp;
                    }
                    else
                    {
                        GameObject _go = GameObject.Instantiate(_obj) as GameObject;
                        if (null == _go)
                        {
                            _go = new GameObject("NULL");
                        }
                        FResourceRefKeeper _keeper = _go.AddComponent<FResourceRefKeeper>();
                        _go.name = name;
                        _keeper.ResRef = BeUsed(assetPath);
                        _keeper.InstantiatedByResourceUnit = true;
                        _keeper.ResRef.SetAsset(_obj);
                        //_keeper.RefID = _keeper.ResRef.GetHashCode();

                        _target = _keeper;
                    }
                }
                else
                {
                    FResourceRef reftmp = BeUsed(assetPath);
                    reftmp.SetAsset(_obj);
                    _target = reftmp;


                }
            }
            else
            {
                FResourceRef reftmp = BeUsed(assetPath);
                _target = reftmp;
            }

            return _target;

        }

        internal FResourcesManager m_ResourcesManager;



        internal FResourceUnit(FProcess process, FResourcesManager manager, bool stayMemory = false)
        {
            m_process = process;
    
            AssetBundleData = process.m_FRequest.AssetBundleData;

            m_ResourcesManager = manager;


            StayMemory = stayMemory;

            ParentList = new List<FResourceUnit>();
            DependencesAssetDic = new Dictionary<string, List<FResourceRef>>();
        }

        internal void AddResourceRefToDic(string loadPath, FResourceRef resRef)
        {
            if (null == resRef)
            {
                return;
            }

            if (null == DependencesAssetDic)
            {
                DependencesAssetDic = new Dictionary<string, List<FResourceRef>>();
            }
            List<FResourceRef> _list = null;

            if (!DependencesAssetDic.TryGetValue(loadPath, out _list))
            {
                _list = new List<FResourceRef>();
                DependencesAssetDic.Add(loadPath, _list);
            }

            if (!_list.Contains(resRef))
            {
                if (_list.Count > 0)
                {
                    int _ = 0;
                }
                _list.Add(resRef);
            }

            if (!resRef.resUnit.ParentList.Contains(this))
            {
                resRef.resUnit.ParentList.Add(this);
            }


        }

        internal Object GetAsset(string name, Type type)
        {
            Object _asset = null;
            if (null == AllAssets)
            {
                return _asset;
            }


            int _length = AllAssets.Length;
            for (int i = 0; i < _length; ++i)
            {
                if (AllAssets[i].name == name)
                {
                    _asset = AllAssets[i];

                    if (type == typeof(Object))
                    {
                        _asset = AllAssets[i];
                        break;
                    }

                    if (AllAssets[i].GetType() == type)
                    {
                        _asset = AllAssets[i];
                        break;
                    }

                }
            }



            return _asset;

        }


        internal void TryToUnloadAssetBundle()
        {
            if (isCache || AssetBundleData.Common || AssetBundleData.IsSolid)
            {
                return;
            }

            UnloadAssetBundle(false);

            if (null != DependencesAssetDic && DependencesAssetDic.Count > 0)
            {

                string[] _keyArray = DependencesAssetDic.Keys.ToArray();
                for (int i = 0; i < _keyArray.Length; ++i)
                {
                    List<FResourceRef> _list = DependencesAssetDic[_keyArray[i]];
                    if (_list.Count > 0)
                    {
                        FResourceRef _ref = _list[0];
                        _ref.resUnit.isCache = false;
                        _ref.resUnit.TryToUnloadAssetBundle();
                    }
                }

            }
        }




        /// <summary>
        /// 丢AssetBundle包
        /// </summary>
        internal void UnloadAssetBundle(bool flag)
        {

            if (null != m_AssetBundle)
            {
                m_AssetBundle.Unload(flag);
            }

        }

        /// <summary>
        /// 释放指定资源，无法释放 GameObject 类型资源，GameObject 类型资源请使用Destroy(gameObject);
        /// </summary>
        /// <param name="asset"></param>
        internal void UnloadAsset(Object asset)
        {
            if (null != asset)
            {
                if (asset.GetType() == typeof(UnityEngine.GameObject))
                {
                    if (null == DependencesAssetDic || 0 == DependencesAssetDic.Count)
                    {
                        GameObject _go = asset as GameObject;
                        GameObject.DestroyImmediate(_go, true);
                    }
                }
                else
                {
                    Resources.UnloadAsset(asset);
                }
            }
        }


        internal void UnloadAllAsset()
        {

            for (int i = 0; i < AllAssets.Length; ++i)
            {
                UnloadAsset(AllAssets[i]);
            }
        }


        private void modifyMaterial(Object asset)
        {

            if (null != asset)
            {
                if (asset.GetType() == typeof(Material))
                {
                    Material mat = asset as Material;
                    Shader _target = null;

                    _target = Shader.Find(mat.shader.name);
                    if (null != _target)
                    {
                        mat.shader = _target;
                    }
                }
            }

        }

        private void ModifyGameObjectMaterial(GameObject go)
        {

            if (null != go)
            {
                Renderer[] _RendererArray = null;

                ParticleSystem[] _array = go.GetComponentsInChildren<ParticleSystem>();
                for (int i = 0; i < _array.Length; ++i)
                {
                    _RendererArray = _array[i].gameObject.GetComponents<Renderer>();
                    for (int j = 0; j < _RendererArray.Length; ++j)
                    {
                        if (null != _RendererArray[j].sharedMaterial)
                        {
                            Shader _shader = _RendererArray[j].sharedMaterial.shader;
                            if (null != _shader)
                            {
                                Shader _tmp = Shader.Find(_shader.name);
                                if (null != _tmp)
                                {
                                    _RendererArray[j].sharedMaterial.shader = _tmp;
                                }
                            }
                        }
                    }
                }

                _RendererArray = go.GetComponentsInChildren<Renderer>();
                for (int i = 0; i < _RendererArray.Length; ++i)
                {
                    if (null != _RendererArray[i].sharedMaterial)
                    {
                        Shader _shader = _RendererArray[i].sharedMaterial.shader;
                        if (null != _shader)
                        {
                            Shader _tmp = Shader.Find(_shader.name);
                            if (null != _tmp)
                            {
                                _RendererArray[i].sharedMaterial.shader = _tmp;
                            }
                        }
                    }

                }

            }

        }




        internal void InputResource(AssetBundle assetBundle, Object[] allAssets)
        {
            m_AssetBundle = assetBundle;
            //if (null == assetBundle && null == allAssets)
            //{

            //}
            //else if (null != m_AssetBundle)
            //{

            //    AllAssets = m_AssetBundle.LoadAllAssets();


            //}
            //else
            {
                AllAssets = allAssets;
            }


            if (AllAssets.Length > 0)
            {
                AllAssets = AllAssets.ToList().OrderByDescending(v => v.GetType() == typeof(GameObject)).ToArray();
            }

            if (FResourceCommon.IsEditor())
            {
                for (int i = 0; i < AllAssets.Length; ++i)
                {
                    modifyMaterial(AllAssets[i]);
                }

            }

        }


        /// <summary>
        /// 被引用
        /// </summary>
        /// <param name="assetPath">主请求资源的路径</param>
        /// <returns></returns>
        internal FResourceRef BeUsed(string assetPath = "")
        {
            if (StayMemory)
                return null;
            return new FResourceRef(this, assetPath);
        }



        internal void AddReferenceCount()
        {
            ++ReferenceCount;
        }

        internal void ReduceReferenceCount()
        {
            if (!StayMemory)
            {
                if (DependencesAssetDic.Count > 0)
                {

                    string[] _keyArray = DependencesAssetDic.Keys.ToArray();
                    for (int i = 0; i < _keyArray.Length; ++i)
                    {
                        List<FResourceRef> _list = DependencesAssetDic[_keyArray[i]];
                        if (_list.Count > 0)
                        {
                            FResourceRef _ref = _list[0];
                            _list.Remove(_ref);
                            _ref.ReleaseImmediate();
                        }

                    }

                }

                if (--ReferenceCount == 0)
                {
                    m_ResourcesManager.AddToRecycleBin(this);

                }
            }
        }

        internal void ResourceUnitDisposed(FResourceUnit _unit)
        {
            ParentList.Remove(_unit);
        }


        /// <summary>
        /// 释放整个资源单位
        /// </summary>
        public void Dispose()
        {

            if (!FResourcesManager.UsedAssetBundle)
            {
                return;
            }

            if (null == m_AssetBundle)
            {
                UnloadAllAsset();
            }
            else
            {
                UnloadAssetBundle(true);

            }

            m_process.m_FRequest.Dispose();
            m_process.Dispose();
            AssetBundleData = null;
            ParentList = null;
            DependencesAssetDic = null;


        }
    }


}
