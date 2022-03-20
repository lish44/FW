//------------------------------------------------------------------------
// |                                                                   |
// | Autor:Adam                                                           |
// |                                       |
// |                                                                   |
//-----------------------------------------------------------------------

using UnityEngine;
using System.Collections.Generic;
using System.Text;


namespace FW
{
    public class FConfigManager : IDispose
    {

        private static Dictionary<string, T> converDic<T>(Dictionary<string, FConfigData> sourceDic) where T : FConfigData
        {
            Dictionary<string, T> dic = new Dictionary<string, T>();
            foreach (KeyValuePair<string, FConfigData> item in sourceDic)
            {
                dic.Add(item.Key, (T)item.Value);
            }
            return dic;
        }
        private static FConfigManager inst = null;
        public static FConfigManager Inst
        {
            get
            {
                if (null == inst)
                {
                    inst = new FConfigManager();
                }
                return inst;
            }
        }
        /// <summary>
        /// format  : 
        /// data1=data content;
        /// //data comment
        /// data2=data content;
        /// </summary>
        /// <param name="container"></param>
        /// <param name="data"></param>
        public static T parseFConfig<T>(string data)
        {
            return (T)FConfig.parseFConfig<T>(data);
        }

        public static Dictionary<string, string> parseFConfig(string data)
        {
            return FConfig.parseFConfig(data);
        }

        public static void parseExcelAndCache<T>(string root, string path, CallBack callBack = null) where T : FConfigData, new()
        {
            string name = typeof(T).Name;

            if (path != null)
                path = root + "/" + name;
            else
                path = "Config/Game/" + name;

            FResourcesManager.Inst.LoadObject(path, (obj) =>
            {
                FResourceRef _Ref = obj as FResourceRef;
                TextAsset ta = _Ref.Asset as TextAsset;
                if (ta == null)
                {
                    Log.Error("not find target in path ", path);
                    return;
                }
                FConfig.parseExcelAndCache<T>(name, ta.text);
                if (null != callBack)
                {
                    callBack();
                }
            });


        }



        public static void parseExcelAndCache<T>(string path = null) where T : FConfigData, new()
        {
            parseExcelAndCache<T>("Config/Game", path);
        }
        public static void parseExcelAndCache<T>(string path, CallBack callBack) where T : FConfigData, new()
        {
            parseExcelAndCache<T>("Config/Game", path, callBack);
        }


        public static Dictionary<int, FConfigData> getConfig(string name)
        {
            return FConfig.getConfig(name);
        }


        public static Dictionary<int, FConfigData> getConfig<T>() where T : FConfigData
        {
            return FConfig.getConfig<T>();
        }

        public static Dictionary<int, T> GetConfig<T>() where T : FConfigData
        {
            Dictionary<int, FConfigData> _dic = FConfig.getConfig<T>();
            Dictionary<int, T> _targetDic = new Dictionary<int, T>();
            foreach (var data in _dic)
            {
                _targetDic.Add(data.Key, data.Value as T);
            }
            _dic = null;
            return _targetDic;
        }



        public static bool contains(string name, int id)
        {
            Dictionary<int, FConfigData> dic = getConfig(name);
            if (dic == null)
                return false;
            return dic.ContainsKey(id);
        }
        public static FConfigData get(string name, int id)
        {
            FConfigData v = null;
            getConfig(name).TryGetValue(id, out v);
            if (v == null)
            {
                Log.Error(name, "not find config from id ", id.ToString());
            }
            return v;
        }

        public static T get<T>(int id) where T : FConfigData
        {
            return get(typeof(T).Name, id) as T;
        }
        public static bool contains<T>(int id) where T : FConfigData
        {
            return contains(typeof(T).Name, id);
        }




        public static string get(string name, int id, int key)
        {
            return get(name, id).Get(key);
        }

        public static T get<T>(string name, int id, int key)
        {
            return get(name, id).Get<T>(key);
        }




        public void SetParent(Transform parent)
        {

        }

        public void Dispose()
        {
            inst = new FConfigManager();
        }
    }
}
