using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using FW;

public delegate void ConfigDataCallBack(long id, string fieldname, ref string data);

public class ConfigMgr
{
    public bool DebugMode = false;
    //定义配置管理表
    private Dictionary<Type, Dictionary<long, Config>> _configs = new Dictionary<Type, Dictionary<long, Config>>();

    //完整表数据存储类
    private Dictionary<Type, Config[]> _configParsesCatch = new Dictionary<Type, Config[]>();

    //定义配置文件数据表
    private readonly Dictionary<Type, Dictionary<long, FileLineInfo>> _sourceData = new Dictionary<Type, Dictionary<long, FileLineInfo>>();

    private readonly Dictionary<Type, Dictionary<long, byte[]>> _byteDataDic = new Dictionary<Type, Dictionary<long, byte[]>>();


    //定义配置文件字段数据表
    private readonly Dictionary<string, string[][]> _dicFileFieldInfo = new Dictionary<string, string[][]>();

    //缓冲字节数据
    private ByteBuffer _byteBuffer = new ByteBuffer();

    //某类型的配置是否包含Asset
    private readonly Dictionary<Type, bool> _configContainsAssets = new Dictionary<Type, bool>();
    //某类型的配置包含ConfigAsset标签的字段列表
    private readonly Dictionary<Type, List<string>> _configAssetFields = new Dictionary<Type, List<string>>();

    private static object _iLock = new object();

    struct FileLineInfo
    {
        public string[] LineData;
        public Type DerivedType;
    }
    private static ConfigMgr _instance;
    private static bool _init = false;
    public static ConfigMgr Ins
    {
        get
        {
            if (_instance == null)
            {
                lock (_iLock)
                {
                    if (_instance == null)
                    {
                        _instance = new ConfigMgr();
                    }
                }
            }
            if (!_init)
            {
                _init = true;
            }
            return _instance;
        }
    }
    public void Import()
    {
        string _path = Application.dataPath + "/Resources/Config";
        string[] _allPath = Directory.GetFiles(_path, "*.txt", SearchOption.AllDirectories);

        Assembly ass = Assembly.GetExecutingAssembly();

        //string _rpath = _allPath[i].Replace('\\', '/').Replace(".txt", "").Replace(Application.dataPath + "/Resources","");

        for (int i = 0; i < _allPath.Length; i++)
        {
            var _name = Path.GetFileNameWithoutExtension(_allPath[i]);
            var t = Resources.Load("Config/" + _name) as TextAsset;
            Type _configTypeClassName = ass.GetType(_name, false, false);
            ConfigMgr.Ins.ImportStrInfo(_configTypeClassName, t.text);
        }
    }
    public void Init(CallBack _callback = null)
    {
        Import();
        _callback?.Invoke();
    }


    /// <summary>
    /// 获取配置表集合(泛型)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T[] GetConfigDic<T>() where T : Config
    {
        Type t = typeof(T);
        Config[] result = null;
        _configParsesCatch.TryGetValue(t, out result);
        if (result == null)
        {
            long[] ids = GetAllId(t);
            T[] cfgs = new T[ids.Length];
            for (int i = 0; i < ids.Length; i++)
            {
                var config_data = GetConfigFromDic<T>(ids[i]);
                cfgs[i] = config_data;
            }
            _configParsesCatch.Add(t, cfgs);
            _configParsesCatch.TryGetValue(t, out result);
        }

        return (T[])result;
    }
    /// <summary>
    /// 获取某个配置表的配置(泛型)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="id"></param>
    /// <returns></returns>
    public T GetConfigFromDic<T>(long id) where T : Config
    {
        Type t = typeof(T);
        return (T)GetConfigFromDic(t, id);
    }
    /// <summary>
    /// 获取某个配置表的配置
    /// </summary>
    /// <param name="configName"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public Config GetConfigFromDic(Type type, long id, bool tryGet = false)
    {
        try
        {
            Config cfg = null;
            Dictionary<long, Config> typeDic;
            lock (_iLock)
            {
                if (_configs.TryGetValue(type, out typeDic))
                {
                    typeDic.TryGetValue(id, out cfg);
                }
                if (cfg == null)
                {
                    cfg = _loadConfigSource(type, id);
                }

                return cfg;
            }
        }
        catch (Exception e)
        {
            if (!tryGet)
            {
                Debug.Log(type.Name + " id: " + id + " parse Error! Because " + e);
            }
            return null;
        }
    }

    //解析strInfo,将解析好的配置表导入配置管理表 
    private Config _loadConfigSource(Type t, long id)
    {
        Config instance = null;
        if (_byteDataDic.ContainsKey(t))
        {
            instance = t.Assembly.CreateInstance(t.FullName) as Config;
            if (instance != null)
            {
                byte[] readBytes = _byteDataDic[t][id];
                _byteBuffer.clear();
                _byteBuffer.writeBytes(readBytes, 0, readBytes.Length);
                FieldInfo[] fields = instance.GetType().GetFields();
                FieldInfo field;
                for (int m = 0; m < fields.Length; m++)
                {
                    field = fields[m];
                    if (!field.IsInitOnly)
                        continue;
                    field.SetValue(instance, ConfigTools.ReadSimpleBuffer(field.FieldType, _byteBuffer));
                }
                _configs[t].Add(id, instance);
            }
            else
            {
                Debug.Log("解析byte配置类型: " + t.Name + " ID: " + id + " 发生错误.");
            }
            return instance;
        }
        if (_sourceData.ContainsKey(t))
        {
            Type baseType = null;
            //derivedType不为空的特殊处理

            FileLineInfo flInfo;
            if (!_sourceData[t].TryGetValue(id, out flInfo))
            {
                Debug.Log("配置表 " + t + " 中未能找到: " + id);
                return null;
            }
            if (flInfo.DerivedType != null)
            {
                baseType = t;
                t = _sourceData[t][id].DerivedType;
            }

            Type dicType = baseType ?? t;

            instance = _parseConfigSource(_dicFileFieldInfo[t.Name][0], _dicFileFieldInfo[t.Name][2], _sourceData[dicType][id].LineData, t);

            if (instance != null)
            {
                _configs[dicType].Add(id, instance);
            }
            else
            {
                Debug.Log("解析配置类型: " + t.Name + " ID: " + id + " 发生错误.");
            }
            return instance;
        }
        return null;
    }

    //解析一条配置
    private static Config _parseConfigSource(string[] typeNames, string[] fieldNames, string[] configInfo, Type configType)
    {
        string typeName = configType.FullName;
        Config instance = configType.Assembly.CreateInstance(typeName) as Config;
        if (instance != null)
        {
            FieldInfo fieldInfo;
            Type fieldType;
            object o;

            //第一次解析某个Type的配置时，把它的ConfigAsset字段保存下来，以避免每次都检查
            bool firstAnalysisThisType = !Ins._configContainsAssets.ContainsKey(configType);
            if (firstAnalysisThisType)
            {
                Ins._configContainsAssets.Add(configType, false);
            }
            bool containsAssets = false;

            for (int j = 0; j < configInfo.Length; j++)
            {
                fieldInfo = configType.GetField(fieldNames[j]);
                fieldType = fieldInfo.FieldType;

                //第一次解析此类型时，处理标记为ConfigAsset特性的string
                if (firstAnalysisThisType)
                {
                    if (_checkConfigAsset(fieldInfo))
                    {
                        containsAssets = true;
                        List<string> names;
                        if (!Ins._configAssetFields.TryGetValue(configType, out names))
                        {
                            names = new List<string>();
                            Ins._configAssetFields.Add(configType, names);
                        }
                        names.Add(fieldNames[j]);
                        if (fieldType.IsArray)
                        {
                            o = _parseArrayConfigField(typeNames[j], fieldNames[j], configInfo[j], fieldType);
                            if (o is Array)
                            {
                                Array array = o as Array;
                                foreach (object o1 in array)
                                {
                                    if (o1 is string)
                                    {
                                        string str = o1 as string;
                                        if (!string.IsNullOrEmpty(str))
                                            instance.Assets.Add(str);
                                    }
                                }
                            }
                        }
                        else
                            if (!string.IsNullOrEmpty(configInfo[j]))
                        {
                            instance.Assets.Add(configInfo[j]);
                        }
                    }
                }

                //处理字段被赋初值的数据(数组除外,空数组会初始化为一个0长度的实例)
                if (configInfo[j].TrimEnd().Equals(""))
                {
                    if (fieldType.IsArray && fieldInfo.GetValue(instance) == null)
                    {
                        o = Array.CreateInstance(fieldType.GetElementType(), 0);
                        fieldInfo.SetValue(instance, o);
                    }
                    continue;
                }

                if (fieldType.IsArray)
                {
                    o = _parseArrayConfigField(typeNames[j], fieldNames[j], configInfo[j], fieldType);
                    if (o is Array)
                    {
                        Array array = o as Array;
                        foreach (object o1 in array)
                        {
                            if (o1 is Config)
                            {
                                Config cig = o1 as Config;
                                foreach (string asset in cig.Assets)
                                {
                                    instance.Assets.Add(asset);
                                }
                            }
                        }
                    }
                }
                else if (fieldType.IsGenericType)
                {
                    o = _parseGenericArrayConfigField(typeNames[j], fieldNames[j], configInfo[j], fieldType);
                    if (o is IEnumerable)
                    {
                        IEnumerable array = o as IEnumerable;
                        foreach (object o1 in array)
                        {
                            if (o1 is Config)
                            {
                                Config cig = o1 as Config;
                                foreach (string asset in cig.Assets)
                                {
                                    instance.Assets.Add(asset);
                                }
                            }
                        }
                    }
                }
                else
                {
                    o = ParseConfigField(configInfo[j], fieldType);
                    if (o is Config)
                    {
                        Config cig = o as Config;
                        foreach (string asset in cig.Assets)
                        {
                            instance.Assets.Add(asset);
                        }
                    }
                }

                if (o == null)
                {
                    Debug.Log("Config Type " + configType + " ParseError: field: '" + fieldNames[j] + "' value: " + configInfo[j]);
                    return null;
                }

                fieldInfo.SetValue(instance, o);
            }

            //如果不是第一次解析，就直接把保存的字段加到Assets里面
            if (firstAnalysisThisType)
            {
                Ins._configContainsAssets[configType] = containsAssets;
            }
            else if (Ins._configContainsAssets[configType])
            {
                for (int i = 0; i < Ins._configAssetFields[configType].Count; i++)
                {
                    for (int j = 0; j < fieldNames.Length; j++)
                    {
                        if (fieldNames[j] == Ins._configAssetFields[configType][i])
                        {
                            fieldInfo = configType.GetField(fieldNames[j]);
                            fieldType = fieldInfo.FieldType;
                            if (fieldType.IsArray)
                            {
                                o = _parseArrayConfigField(typeNames[j], fieldNames[j], configInfo[j], fieldType);
                                if (o is Array)
                                {
                                    Array array = o as Array;
                                    foreach (object o1 in array)
                                    {
                                        if (o1 is string)
                                        {
                                            string str = o1 as string;
                                            if (!string.IsNullOrEmpty(str))
                                                instance.Assets.Add(str);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(configInfo[j]))
                                    instance.Assets.Add(configInfo[j]);
                            }
                        }
                    }
                }
            }
        }

        return instance;
    }
    public static object ParseConfigField(string data, Type fieldType)
    {
        object o;
        if (_checkCustomConfig(fieldType))
        {
            o = _parseCustonConfig(data, fieldType);
        }
        //处理非自定义类型数据
        else
        {
            o = _parseBaseValueType(data, fieldType);
        }
        return o;
    }
    //自定义类型数据转换成对应字段数据
    private static Config _parseCustonConfig(string data, Type t)
    {
        long refId;
        if (!long.TryParse(data, out refId))
        {
            Debug.Log("引用的自定义类型" + t.Name + "的数据（" + data +
                   "）格式有误，返回空值");
            return null;
        }

        Config refCfg = Ins.GetConfigFromDic(t, refId);
        return refCfg;
    }


    //判断自定义类型是否符合要求
    private static bool _checkCustomConfig(Type t)
    {
        if (t.IsArray)
        {
            if (t.Name.Contains("[][][]"))
                t = t.GetElementType().GetElementType().GetElementType();
            if (t.Name.Contains("[][]"))
                t = t.GetElementType().GetElementType();
            if (t.Name.Contains("[]"))
                t = t.GetElementType();
        }
        else if (t.IsGenericType)
        {
            if (t.Name.StartsWith("List")) t = t.GetGenericArguments()[0];
            else
            {
                return IsConfigClass(t.GetGenericArguments()[0]) && IsConfigClass(t.GetGenericArguments()[1]);
            }
        }
        return IsConfigClass(t);
    }

    //非自定义类型数据转换成对应字段数据
    private static object _parseBaseValueType(string data, Type t)
    {
        if (data.Equals(""))
        {
            return t.Name.Equals("String") ? "" : t.Assembly.CreateInstance(t.FullName);
        }
        try
        {
            if (t.IsEnum)
            {
                return Enum.Parse(t, data);
            }
            switch (t.Name)
            {
                case "String":
                    return data;
                case "Byte":
                    return byte.Parse(data);
                case "SByte":
                    return sbyte.Parse(data);
                case "Int16":
                    {
                        data = data.Replace('_', '-');
                        return short.Parse(data);
                    }
                case "UInt16":
                    {
                        data = data.Replace('_', '-');
                        return ushort.Parse(data);
                    }
                case "Int32":
                    {
                        data = data.Replace('_', '-');
                        //                            if (data.StartsWith("@"))
                        //                            {
                        //                                //return ((IConvertible)_parseTags(data, ex)).ToInt32();
                        //                                int value = Convert.ToInt32(_parseTags(data, ex));
                        //                                return value;
                        //                            }
                        return int.Parse(data);
                    }
                case "UInt32":
                    {
                        data = data.Replace('_', '-');
                        return uint.Parse(data);
                    }
                case "Int64":
                    {
                        data = data.Replace('_', '-');
                        return long.Parse(data);
                    }
                case "UInt64":
                    {
                        data = data.Replace('_', '-');
                        return ulong.Parse(data);
                    }
                case "Char":
                    return char.Parse(data);
                case "Boolean":
                    return data.ToLower() == "true" || data == "1";
                case "Single":
                    {
                        data = data.Replace('_', '-');
                        return float.Parse(data);
                    }
                case "Double":
                case "Float":
                    {
                        //TODO:简化此处
                        data = data.Replace('_', '-');
                        //                            if (data.StartsWith("@"))
                        //                            {
                        //                                return _parseTags(data, ex);
                        //                            }
                        if (data.Contains("E"))
                        {
                            data = data.Replace("E", "E+");
                        }
                        return double.Parse(data);
                    }
                case "IVaryingNumber":
                    {
                        data = data.Replace('_', '-');
                        if (data.StartsWith("@"))
                        {
                            return _parseTags(data);
                        }
                        if (data.Contains("E"))
                        {
                            data = data.Replace("E", "E+");
                        }
                        return new ConstantVarNumber(double.Parse(data));
                    }
                default:
                    Debug.Log("Unknown Config field type : " + t.Name + ".");
                    break;
            }
        }
        catch
        {
            return null;
        }
        return null;
    }

    private static object _parseTags(string data)
    {

        if (data.StartsWith("@level:"))
        {
            string idStr = data.Substring(7);
            long id;
            if (long.TryParse(idStr, out id))
            {
                return (IVaryingNumber)Ins.GetConfigFromDic(typeof(ConfigLevelVar), id);
            }
        }
        else if (data.StartsWith("@-level:"))
        {
            string idStr = data.Substring(8);
            long id;
            if (long.TryParse(idStr, out id))
            {
                ConfigLevelVar src = (ConfigLevelVar)Ins.GetConfigFromDic(typeof(ConfigLevelVar), id);
                return new ConfigLevelVar(true, src);
            }
        }

        return null;
    }



    private static object _parseGenericArrayConfigField(string typeName, string fieldName, string data, Type fieldType)
    {
        //重载分隔符
        string splitChar = "+";
        if (typeName.Contains("[]@"))
        {
            splitChar = typeName.Split('@')[1];
        }
        int dimension = ConfigTools.GetGenericDimension(fieldType);
        Type elementType = ConfigTools.GetGenericElementType(fieldType);
        Type d1T = elementType.MakeArrayType();

        if (dimension == 3)
        {

            Type wrapD2T = fieldType.GetGenericArguments()[0];
            Type wrapD1T = wrapD2T.GetGenericArguments()[0];

            Type d2T = wrapD1T.MakeArrayType();

            string[] line = data.Split('`');
            Array arrayD3 = Array.CreateInstance(d2T, line.Length);
            object wrapArrayD3 = Activator.CreateInstance(fieldType, arrayD3);
            for (int i = 0; i < line.Length; i++)
            {
                string[] p2 = line[i].Split('|');
                Array arrayD2 = Array.CreateInstance(d1T, p2.Length);
                object wrapArrayD2 = Activator.CreateInstance(wrapD2T, arrayD2);

                for (int j = 0; j < p2.Length; j++)
                {
                    object wrapArrayD1 = Activator.CreateInstance(wrapD1T, _parseArray(d1T, p2[j], splitChar));
                    arrayD2.SetValue(wrapArrayD1, j);
                }
                arrayD3.SetValue(wrapArrayD2, i);
            }
            return wrapArrayD3;
        }
        if (dimension == 2)
        {
            Type wrapD1T = fieldType.GetGenericArguments()[0];

            string[] line = data.Split('|');
            Array arrayD2 = Array.CreateInstance(wrapD1T, line.Length);
            object wrapArrayD2 = Activator.CreateInstance(fieldType, arrayD2);
            for (int i = 0; i < line.Length; i++)
            {
                object wrapArrayD1 = Activator.CreateInstance(wrapD1T, _parseArray(d1T, line[i], splitChar));
                arrayD2.SetValue(wrapArrayD1, i);
            }
            return wrapArrayD2;
        }
        if (dimension == 1)
        {
            object wrapArrayD1 = Activator.CreateInstance(fieldType, _parseArray(d1T, data, splitChar));
            return wrapArrayD1;
        }
        return null;
    }


    //判断字段是否标记了ConfigAsset特性
    private static bool _checkConfigAsset(FieldInfo fi)
    {
        if (fi.FieldType.Name != "String" && fi.FieldType.Name != "String[]")
        {
            return false;
        }
        Attribute[] a = Attribute.GetCustomAttributes(fi);
        for (int i = 0; i < a.Length; i++)
        {
            if (a[i].GetType() == typeof(ConfigAssetAttribute))
            {
                return true;
            }
        }
        return false;
    }

    private static object _parseArrayConfigField(string typeName, string fieldName, string data, Type fieldType)
    {
        //重载分隔符
        string splitChar = "+";
        if (typeName.Contains("[]@"))
        {
            splitChar = typeName.Split('@')[1];
        }
        int dimension = ConfigTools.GetArrayDimension(fieldType);

        if (dimension == 3)
        {
            string[] line = data.Split('`');
            Array arrConfig2 = Array.CreateInstance(fieldType.GetElementType(), line.Length);
            for (int i = 0; i < line.Length; i++)
            {
                string[] p2 = line[i].Split('|');
                Array tempElem = Array.CreateInstance(fieldType.GetElementType().GetElementType(), p2.Length);

                for (int j = 0; j < p2.Length; j++)
                {
                    tempElem.SetValue(
                        _parseArray(fieldType.GetElementType().GetElementType(), p2[j], splitChar), j);
                }
                arrConfig2.SetValue(tempElem, i);
            }
            return arrConfig2;
        }
        //字符串数据转换为指定类型数组数据
        if (dimension == 2)
        {
            string[] line = data.Split('|');
            Array arrConfig2 = Array.CreateInstance(fieldType.GetElementType(), line.Length);
            for (int i = 0; i < line.Length; i++)
            {
                arrConfig2.SetValue(_parseArray(fieldType.GetElementType(), line[i], splitChar), i);
            }
            return arrConfig2;
        }
        if (dimension == 1)
        {
            return _parseArray(fieldType, data, splitChar);
        }
        return null;
    }

    private static object _parseArray(Type fieldType, string data, string splitChar)
    {
        Type elementType = fieldType.GetElementType();
        string[] str1 = data.Split(splitChar[0]);
        int n = str1.Length;
        Array arr = Array.CreateInstance(elementType, n);
        if (elementType.IsEnum)
        {
            for (int i = 0; i < n; i++)
            {
                arr.SetValue(Enum.Parse(elementType, str1[i]), i);
            }
        }
        else
        {
            for (int i = 0; i < n; i++)
            {
                object element = ParseConfigField(str1[i], elementType);
                if (element == null)
                {
                    return null;
                }
                arr.SetValue(element, i);
            }
        }
        return arr;
    }



    public long[] GetAllId(Type type)
    {
        Dictionary<long, byte[]> bytesInfo;
        if (_byteDataDic.TryGetValue(type, out bytesInfo))
        {
            if (bytesInfo != null)
            {
                return bytesInfo.Keys.ToArray();
            }
        }
        Dictionary<long, FileLineInfo> infos;
        if (_sourceData.TryGetValue(type, out infos))
        {
            if (infos != null)
            {
                return infos.Keys.ToArray();
            }
        }
        return new long[0];
    }

    public static bool IsConfigClass(Type type)
    {
        if (type.IsInterface)
            return false;
        while (type != typeof(object))
        {
            if (type == typeof(Config))
                return true;
            type = type.BaseType;
        }
        return false;
    }

    /// <summary>
    /// 导入明文配置数据(泛型)
    /// </summary>
    /// <param name="strInfo"></param>
    public void ImportStrInfo<T>(string strInfo, ConfigDataCallBack callBack = null) where T : Config
    {
        try
        {

            ImportStrInfo(typeof(T), strInfo, null, callBack);
        }
        catch
        {
            Debug.Log("导入配置表出错:" + "类型:" + typeof(T));
            throw;
        }
    }
    public void ImportStrInfo(Type type, string strInfo, Type derivedType = null, ConfigDataCallBack callBack = null)
    {

        if (!IsConfigClass(type))
        {
            Debug.Log("导入的配置类型不符合要求，type name = " + type.Name);
            return;
        }
        lock (_iLock)
        {
            if (!_configs.ContainsKey(type))
            {
                _configs.Add(type, new Dictionary<long, Config>());
            }
            if (_sourceData.ContainsKey(type) && derivedType == null)
                return;
            string[] lineArr = strInfo.Split(new string[] { "\r\n" },
                StringSplitOptions.RemoveEmptyEntries);
            //导入文件字段数据
            string[][] tempArr2 = new string[3][];
            for (int i = 0; i < 3; i++)
            {
                tempArr2[i] = lineArr[i].Split('\t');
            }

            if (derivedType != null)
            {
                //检查数据格式
                if (!CheckConfig(derivedType, tempArr2[2], tempArr2[0]))
                    return;
                if (!_dicFileFieldInfo.ContainsKey(derivedType.Name))
                    _dicFileFieldInfo.Add(derivedType.Name, tempArr2);
            }
            else
            {
                //检查数据格式
                if (!CheckConfig(type, tempArr2[2], tempArr2[0]))
                    return;
                if (!_dicFileFieldInfo.ContainsKey(type.Name))
                    _dicFileFieldInfo.Add(type.Name, tempArr2);
            }

            //导入文件配置数据
            if (derivedType != null && _sourceData.ContainsKey(type))
            {
                for (int i = 3; i < lineArr.Length; i++)
                {
                    if (lineArr[i].Replace("\t", "") == "" || lineArr[i].Replace("\fieldType", "") == null)
                        continue;
                    string str = lineArr[i].Contains("\t")
                        ? lineArr[i].Substring(0, lineArr[i].IndexOf("\t"))
                        : lineArr[i];
                    long n;
                    if (long.TryParse(str, out n))
                    {
                        n = long.Parse(str);
                        FileLineInfo tempfi;
                        tempfi.LineData = lineArr[i].Split('\t');
                        for (int j = 0; j < tempfi.LineData.Length; j++)
                        {
                            if (callBack != null)
                            {
                                callBack(i, _dicFileFieldInfo[derivedType.Name][2][j], ref tempfi.LineData[j]);
                            }
                        }
                        tempfi.DerivedType = derivedType;
                        if (!_sourceData[type].ContainsKey(n))
                            _sourceData[type].Add(n, tempfi);
                        else
                        {
                            Debug.Log("配置表" + derivedType.Name + "导入配置数据id错误（第" + (i + 1) + "行 , 第1列）：数据id（" + str +
                                   "）已导入，导入此配置数据失败！" + "重复来源：" + _sourceData[type][n].DerivedType.Name);
                        }
                    }
                    else
                    {
                        Debug.Log("配置表" + type.Name + "导入配置数据id错误（第" + (i + 1) + "行 , 第1列） ： 数据id号（" + str +
                               "）的格式不符合要求，导入此配置数据失败!");
                    }
                }
            }
            else
            {
                Dictionary<long, FileLineInfo> tempDic = new Dictionary<long, FileLineInfo>();
                for (int i = 3; i < lineArr.Length; i++)
                {
                    if (lineArr[i].Replace("\t", "") == "" || lineArr[i].Replace("\fieldType", "") == null)
                        continue;
                    string str = lineArr[i].Contains("\t")
                        ? lineArr[i].Substring(0, lineArr[i].IndexOf("\t"))
                        : lineArr[i];
                    if (str.EndsWith("@"))
                        str = str.Remove(str.Length - 1);
                    long n;
                    if (str.StartsWith("#"))
                        continue;
                    if (str == null || str == "")
                    {
                        Debug.Log("配置表" + type.Name + "导入配置数据id错误（第" + (i + 1) + "行 , 第1列） ： 数据id号（" + str +
                                    "）的格式不符合要求，导入此配置数据失败!");
                        continue;
                    }
                    if (long.TryParse(str, out n))
                    {
                        n = long.Parse(str);
                        FileLineInfo tempfi;
                        tempfi.LineData = lineArr[i].Split('\t');
                        for (int j = 0; j < tempfi.LineData.Length; j++)
                        {
                            if (callBack != null)
                            {
                                callBack(i, _dicFileFieldInfo[type.Name][2][j], ref tempfi.LineData[j]);
                            }
                        }
                        tempfi.DerivedType = derivedType;
                        if (!tempDic.ContainsKey(n))
                            tempDic.Add(n, tempfi);
                        else
                        {
                            Debug.Log("配置表" + type.Name + "导入配置数据id错误（第" + (i + 1) + "行 , 第1列） ： 数据id（" + str +
                                   "）已导入，导入此配置数据失败!");
                        }
                    }
                    else
                    {
                        Debug.Log("配置表" + type.Name + "导入配置数据id错误（第" + (i + 1) + "行 , 第1列） ： 数据id号（" + str +
                               "）的格式不符合要求，导入此配置数据失败!");
                    }
                }
                _sourceData.Add(type, tempDic);
            }
        }
    }


    //检查传入的配置信息可能出现的各种错误，并打印对应错误信息
    private bool CheckConfig(Type t, string[] arrFieldName, string[] arrFieldType)
    {
#if DEBUG
        bool IsTrue = true;
        //检查字段名是否一一存在
        for (int i = 0; i < arrFieldName.Length; i++)
        {
            if (t.GetField(arrFieldName[i]) == null)
            {
                Debug.Log("配置表" + t.Name + "导入字段名(FieldName)错误（第3行，第" + (i + 1) + "列） ： 字段名\"" +
                       arrFieldName[i] + "\"在类\"" + t.Name + "\"中不存在，导入失败！");
                IsTrue = false;
            }
        }
        if (!IsTrue) return false;
        //检查字段类型与对应字段名是否一一匹配
        for (int i = 0; i < arrFieldType.Length; i++)
        {
            FieldInfo fi = t.GetField(arrFieldName[i]);
            string typeName = RuntimeTypeChangeToTypeName(fi.FieldType);
            if (!DebugMode)
            {
                if (!fi.IsInitOnly)
                {
                    Debug.Log("配置表" + t.Name + "写入权限错误（第1行，第" + (i + 1) + "列） ： 字段名\"" +
                           arrFieldName[i] + "\"该字段不是readonly，导入失败！");
                    IsTrue = false;
                }
            }
            string FieldType = arrFieldType[i];
            if (FieldType.Contains("[]@"))
            {
                FieldType = FieldType.Split('@')[0];
            }
            if (!FieldType.Equals(typeName))
            {
                Debug.Log("配置表" + t.Name + "导入字段" + arrFieldName[i] + " ： 类型\"" +
                      arrFieldType[i] + "\"与类型\"" + typeName + "\"不匹配，导入失败！");
                IsTrue = false;
                //if (!(typeName.StartsWith("IVaryingNumber") && (FieldType.StartsWith("double") || FieldType.StartsWith("float") || FieldType.StartsWith("int"))))
                //{

                //}
            }
        }
        return IsTrue;
#else
            return true;
#endif
    }

    //字段类型名转为配置表字段类型名
    private string RuntimeTypeChangeToTypeName(Type t)
    {
        if (t.IsArray || t.IsGenericType)
        {
            if ((t.IsGenericType && t.Name.StartsWith("ReadonlyArray")) || t.IsArray)
            {
                string suff = "";
                Type ementType = t;
                int d = t.IsArray ? ConfigTools.GetArrayDimension(t) : ConfigTools.GetGenericDimension(t);
                for (int i = 0; i < d; i++)
                {
                    suff += "[]";
                    ementType = t.IsArray ? ementType.GetElementType() : ementType.GetGenericArguments()[0];
                }

                return BaseTypeNameChange(ementType, null, suff);
            }
            return null;
        }
        return BaseTypeNameChange(t, null, "");
    }

    //基本数据类型别名转换为C#名
    private string BaseTypeNameChange(Type t1, Type t2, string strsuff)
    {
        string s = null;

        if (t2 != null)
        {
            s = getBaseType(t2.Name);
            string s1 = null;
            s1 = getBaseType(t1.Name);
            string k = "";
            if (s1 != null) k = s1;
            else
            {
                if (t1.IsArray)
                {
                    if (t1.Name.Contains("[][][]"))
                        k += BaseTypeNameChange(t1.GetElementType().GetElementType().GetElementType(), null,
                            "[][][]");
                    if (t1.Name.Contains("[][]"))
                        k += BaseTypeNameChange(t1.GetElementType().GetElementType(), null, "[][]");
                    if (t1.Name.Contains("[]"))
                        k += BaseTypeNameChange(t1.GetElementType(), null, "[]");
                }
                if (t1.IsEnum) k = "enum";
                if (IsConfigClass(t1)) k = t1.Name;
            }
            k += "|";
            if (s != null) k += s;
            else
            {
                if (t2.IsArray)
                {
                    if (t2.Name.Contains("[][][]"))
                        k += BaseTypeNameChange(t2.GetElementType().GetElementType().GetElementType(), null,
                            "[][][]");
                    if (t2.Name.Contains("[][]"))
                        k += BaseTypeNameChange(t2.GetElementType().GetElementType(), null, "[][]");
                    if (t2.Name.Contains("[]"))
                        k += BaseTypeNameChange(t2.GetElementType(), null, "[]");
                }
                if (t2.IsEnum) k += "enum";
                if (IsConfigClass(t2)) k += t2.Name;
            }
            k += strsuff;
            return k;
        }
        s = getBaseType(t1.Name);
        if (s != null) return s + strsuff;
        if (t1.IsEnum) return "enum" + strsuff;
        if (IsConfigClass(t1)) return t1.Name + strsuff;
        if (t1 == typeof(IVaryingNumber))
        {
            return "IVaryingNumber" + strsuff;
        }
        return null;
    }

    private string getBaseType(string t)
    {
        string s = null;
        switch (t)
        {
            case "String":
                s = "string";
                break;
            case "Int32":
                s = "int";
                break;
            case "UInt32":
                s = "uint";
                break;
            case "UInt64":
                s = "ulong";
                break;
            case "Byte":
                s = "byte";
                break;
            case "SByte":
                s = "sbyte";
                break;
            case "Int16":
                s = "short";
                break;
            case "Char":
                s = "char";
                break;
            case "UInt16":
                s = "ushort";
                break;
            case "Single":
                s = "float";
                break;
            case "Boolean":
                s = "bool";
                break;
            case "Double":
                s = "double";
                break;
            case "Int64":
                s = "long";
                break;
        }
        return s;
    }


}
