using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace FW
{
    public class ResMgr : SingletonBase<ResMgr>
    {
        private Dictionary<Type, Dictionary<string, IGetPath>> map = new Dictionary<Type, Dictionary<string, IGetPath>>();
        CallBack<Type> mCallback;
        public string GetPath<T>(string _name) where T : UnityEngine.Object
        {
            string _res = string.Empty;

            if (typeof(GameObject) == typeof(T))
                _res = Get<PrefabPath>(_name);
            if (typeof(Sprite) == typeof(T))
                _res = Get<SpritePath>(_name);
            if (typeof(AudioClip) == typeof(T))
                _res = Get<AudioPath>(_name);

            return _res;
        }


        public void Init(CallBack _callback)
        {
            mCallback = (t) =>
            {
                if (t.Name.Equals("PrefabPath"))
                    LoadPath<PrefabPath>("Prefab", new string[] { "Assets/Resources/Prefab" });
                if (t.Name.Equals("SpritePath"))
                    LoadPath<SpritePath>("Sprite", new string[] { "Assets/Resources/Sprite" });
                if (t.Name.Equals("AudioPath"))
                    LoadPath<AudioPath>("AudioClip", new string[] { "Assets/Resources/Audio" });
            };
            _callback?.Invoke();

        }

        /// <summary>
        /// 得到类型的所有名字
        /// </summary>
        /// <typeparam name="T">路径类</typeparam>
        /// <returns></returns>
        public List<string> GetTypeByNames<T>() where T : IGetPath
        {
            Type _t = typeof(T);
            if (!map.ContainsKey(_t)) mCallback?.Invoke(_t);
            return map[typeof(T)].Keys.ToList();
        }

        private string Get<T>(string _name) where T : IGetPath
        {
            Type _t = typeof(T);
            if (!map.ContainsKey(_t)) mCallback?.Invoke(_t);
            return map[typeof(T)][_name].GetPath;
        }

        private void LoadPath<T>(string _typeName, string[] _searchScope) where T : IGetPath, new()
        {
            var _datas = LoadResourcesPathInfo<T>(_typeName, _searchScope);
            Type _t = typeof(T);
            if (!map.ContainsKey(_t))
            {
                map.Add(_t, new Dictionary<string, IGetPath>());
                for (int i = 0; i < _datas.Count; i++)
                {
                    T _data = _datas[i];
                    if (!map[_t].ContainsKey(_data.GetName)) map[_t].Add(_data.GetName, _data);
                    else Debug.Log(_t.Name + " 有同名->" + _data.GetName);
                }

            }
        }
        // 把资源名和路径加载出来
        private List<T> LoadResourcesPathInfo<T>(string _type, string[] _SearchScope) where T : IGetPath, new()
        {
            var GUID = AssetDatabase.FindAssets("t:" + _type, _SearchScope);
            int len = GUID.Length;
            if (len == 0)
            {
                Debug.Log("加载资源路径匹配错误!!");
                return null;
            }
            List<T> _datas = new List<T>();
            var res = new string[len];
            for (int i = 0; i < len; ++i)
            {
                res[i] = AssetDatabase.GUIDToAssetPath(GUID[i]);
                var _name = Path.GetFileNameWithoutExtension(res[i]);
                string _filePath = res[i].Replace("Assets/Resources/", string.Empty);// 去后缀名字
                int startIndex = _filePath.LastIndexOf(".");
                var _path = _filePath.Remove(startIndex, _filePath.Length - startIndex);
                T t = new T();
                t.SetInof(_name, _path);
                _datas.Add(t);
            }
            AssetDatabase.Refresh();
            return _datas;
        }

        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="_name">名字</param>
        /// <typeparam name="T">类型</typeparam>
        /// <returns></returns>
        public T Load<T>(string _name) where T : UnityEngine.Object
        {
            string path = GetPath<T>(_name);
            T res = Resources.Load<T>(path);
            if (res is GameObject)
            {
                var go = GameObject.Instantiate(res);
                go.name = go.name.Replace("(Clone)", "");
                return go;
            }
            return res;
        }
        // public T Load<T>(string _path) where T : UnityEngine.Object
        // {
        //     T res = Resources.Load<T>(_path);
        //     if (res is GameObject)
        //     {
        //         var go = GameObject.Instantiate(res);
        //         go.name = go.name.Replace("(Clone)", "");
        //         return go;
        //     }
        //     return res;
        // }

        public T[] LoadAll<T>(string _name) where T : UnityEngine.Object
        {
            string path = GetPath<T>(_name);
            T[] res = Resources.LoadAll<T>(path);
            if (res == null) return res;
            for (int i = 0; i < res.Length; ++i)
            {
                if (res[i] is GameObject)
                {
                    res[i] = GameObject.Instantiate(res[i]);
                    res[i].name = res[i].name.Replace("(Clone)", "");
                }
                else
                {
                    return res;
                }
            }
            return res;
        }

        //异步
        public void LoadAsync<T>(string _name, UnityAction<T> _callback) where T : UnityEngine.Object
        {
            FW.MonoMgr.Ins.StartCoroutine(ReallyLoadAsync(_name, _callback));
        }

        private IEnumerator ReallyLoadAsync<T>(string _name, UnityAction<T> _callback) where T : UnityEngine.Object
        {
            var path = GetPath<T>(_name);
            ResourceRequest r = Resources.LoadAsync<T>(path);
            yield return r;

            if (r.asset is GameObject)
            {
                var go = GameObject.Instantiate(r.asset) as T;
                go.name = go.name.Replace("(Clone)", "");
                _callback(go);
            }
            else
                _callback(r.asset as T);

        }
    }
}