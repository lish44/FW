using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using FW;
using LitJson;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace FW
{

    public class DataMgr : SingletonBase<DataMgr>
    {
        public T LoadJsonData<T>(string _configFileName) where T : class
        {
            string content = Utility.FileOperation.GetConfigFileAllContent(_configFileName);
            JsonReader jr = new JsonReader(content);
            T data = JsonMapper.ToObject<T>(jr);
            return data ?? data;
        }
        public void SaveJsonData(string _configFileName, object obj)
        {
            string path = Application.streamingAssetsPath + "/" + _configFileName + ".json";
            var json = JsonMapper.ToJson(obj);
            File.WriteAllText(path, json);
        }

    }
}