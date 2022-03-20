//-----------------------------------------------------------------------
//| Autor:Adam                                                             |
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Reflection;

namespace FW
{
    public class FConfigData
    {
        public Dictionary<int, string> ConfigDataDic;



        public FConfigData()
        {
            ConfigDataDic = new Dictionary<int, string>();
        }

        protected object converFieldValue(string fieldType, string v)
        {
            object o = null;
            switch (fieldType)
            {
                case "INT":
                    o = FUtils.StrToInt(v.Replace('_', '-'));
                    break;
                case "FLOAT":
                    o = FUtils.StrToFloat(v.Replace('_', '-'));
                    break;
                case "DOUBLE":
                    o = FUtils.StrToDouble(v.Replace('_', '-'));
                    break;
                case "BOOL":
                    o = v == "1";
                    break;
                case "STR":
                    o = v;
                    break;
                case "STRARR":
                    if (string.IsNullOrEmpty(v))
                        o = new string[0];
                    else
                        o = v.Split('+');
                    break;
                case "STRARR~":
                    if (string.IsNullOrEmpty(v))
                        o = new string[0];
                    else
                        o = v.Split('~');
                    break;
                case "INTARR":
                    if (string.IsNullOrEmpty(v))
                        o = new int[0];
                    else
                    {
                        v = v.Replace('_', '-');
                        o = Array.ConvertAll<string, int>(v.Split('+'), (a) =>
                        {
                            return FUtils.StrToInt(a);
                        });
                    }
                    break;
                case "FLOATARR":
                    if (string.IsNullOrEmpty(v))
                        o = new float[0];
                    else
                    {
                        v = v.Replace('_', '-');
                        o = Array.ConvertAll<string, float>(v.Split('+'), (a) =>
                        {
                            return FUtils.StrToFloat(a);
                        });
                    }
                    break;
                case "DOUBLEARR":
                    if (string.IsNullOrEmpty(v))
                        o = new double[0];
                    else
                    {
                        v = v.Replace('_', '-');
                        o = Array.ConvertAll<string, double>(v.Split('+'), (a) =>
                        {
                            return FUtils.StrToFloat(a);
                        });
                    }
                    break;
                case "INTARR2":
                    {
                        if (string.IsNullOrEmpty(v))
                            o = new int[0][];
                        else
                        {
                            v = v.Replace('_', '-');
                            string[] arr = v.Split('|');
                            int[][] arrs = new int[arr.Length][];
                            for (int i = 0; i < arr.Length; i++)
                                arrs[i] = (int[])converFieldValue("INTARR", arr[i]);
                            o = arrs;
                        }
                    }
                    break;
                case "INTARR3":
                    {
                        if (string.IsNullOrEmpty(v))
                            o = new int[0][][];
                        else
                        {
                            v = v.Replace('_', '-');
                            string[] arr = v.Split('`');
                            int[][][] arrs = new int[arr.Length][][];
                            for (int i = 0; i < arr.Length; i++)
                                arrs[i] = (int[][])converFieldValue("INTARR2", arr[i]);
                            o = arrs;
                        }
                    }
                    break;
                case "FLOATARR2":
                    {
                        if (string.IsNullOrEmpty(v))
                            o = new float[0][];
                        else
                        {
                            v = v.Replace('_', '-');
                            string[] arr = v.Split('|');
                            float[][] arrs = new float[arr.Length][];
                            for (int i = 0; i < arr.Length; i++)
                                arrs[i] = (float[])converFieldValue("FLOATARR", arr[i]);
                            o = arrs;
                        }
                    }
                    break;
                case "DOUBLEARR2":
                    {
                        if (string.IsNullOrEmpty(v))
                            o = new double[0][];
                        else
                        {
                            v = v.Replace('_', '-');
                            string[] arr = v.Split('|');
                            double[][] arrs = new double[arr.Length][];
                            for (int i = 0; i < arr.Length; i++)
                                arrs[i] = (double[])converFieldValue("DOUBLEARR", arr[i]);
                            o = arrs;
                        }
                    }
                    break;
                case "FLOATARR3":
                    {
                        if (string.IsNullOrEmpty(v))
                            o = new float[0][][];
                        else
                        {
                            v = v.Replace('_', '-');
                            string[] arr = v.Split('`');
                            float[][][] arrs = new float[arr.Length][][];
                            for (int i = 0; i < arr.Length; i++)
                                arrs[i] = (float[][])converFieldValue("FLOATARR2", arr[i]);
                            o = arrs;
                        }
                    }
                    break;
                case "DOUBLEARR3":
                    {
                        if (string.IsNullOrEmpty(v))
                            o = new double[0][][];
                        else
                        {
                            v = v.Replace('_', '-');
                            string[] arr = v.Split('`');
                            double[][][] arrs = new double[arr.Length][][];
                            for (int i = 0; i < arr.Length; i++)
                                arrs[i] = (double[][])converFieldValue("DOUBLEARR2", arr[i]);
                            o = arrs;
                        }
                    }
                    break;
                case "STRARR2":
                    {
                        if (string.IsNullOrEmpty(v))
                            o = new string[0][];
                        else
                        {
                            string[] arr = v.Split('|');
                            string[][] arrs = new string[arr.Length][];
                            for (int i = 0; i < arr.Length; i++)
                                arrs[i] = (string[])converFieldValue("STRARR", arr[i]);
                            o = arrs;
                        }
                    }
                    break;
                case "STRARR3":
                    {
                        if (string.IsNullOrEmpty(v))
                            o = new string[0][][];
                        else
                        {
                            string[] arr = v.Split('`');
                            string[][][] arrs = new string[arr.Length][][];
                            for (int i = 0; i < arr.Length; i++)
                                arrs[i] = (string[][])converFieldValue("STRARR2", arr[i]);
                            o = arrs;
                        }
                    }
                    break;
                case "BITOR":
                    {
                        string[] arr = v.Split('|');
                        int tempO = 0;
                        int len = arr.Length;
                        for (int i = 0; i < len; i++)
                        {
                            int tempI = FUtils.StrToInt(arr[i]) << (len - i - 1);
                            tempO = tempO | tempI;
                        }
                        o = tempO;
                    }
                    break;
                case "DIC":
                    {
                        Dictionary<string, string> dic = new Dictionary<string, string>();
                        if (!string.IsNullOrEmpty(v))
                        {
                            string[] arr = v.Split('|');
                            for (int i = 0; i < arr.Length; i++)
                            {
                                string[] value = arr[i].Split(':');
                                dic.Add(value[0], value[1]);
                            }
                        }
                        o = dic;
                    }
                    break;
                case "DICARR":
                    {
                        Dictionary<string, string[]> dic = new Dictionary<string, string[]>();
                        if (!string.IsNullOrEmpty(v))
                        {
                            string[] arr = v.Split('|');
                            for (int i = 0; i < arr.Length; i++)
                            {
                                string[] value = arr[i].Split(':');
                                dic.Add(value[0], value[1].Split('+'));
                            }
                        }
                        o = dic;
                    }
                    break;
                case "DICARR~":
                    {
                        Dictionary<string, string[]> dic = new Dictionary<string, string[]>();
                        if (!string.IsNullOrEmpty(v))
                        {
                            string[] arr = v.Split('|');
                            for (int i = 0; i < arr.Length; i++)
                            {
                                string[] value = arr[i].Split(':');
                                dic.Add(value[0], value[1].Split('~'));
                            }
                        }
                        o = dic;
                    }
                    break;
                default:
                    if (fieldType.Contains("ENUMS:"))
                    {
                        string[] arr = fieldType.Split(':');
                        o = FUtils.EnumsParse(Runtime.GetType(arr[1], true), v);
                    }
                    else if (fieldType.Contains("ENUM:"))
                    {
                        if (string.IsNullOrEmpty(v))
                            v = "None";
                        string[] arr = fieldType.Split(':');
                        o = Enum.Parse(Runtime.GetType(arr[1], true), v);
                    }
                    else
                        exConverFieldValue(fieldType, v, out o);
                    break;
            }
            
            return o;
        }


        protected virtual void exConverFieldValue(string fieldType, string v, out object o)
        {
            o = null;
        }

        /// <summary>
        /// 解析配置
        /// </summary>
        /// <param name="name"></param>
        /// <param name="fieldType"></param>
        /// <param name="fieldName"></param>
        /// <param name="data"></param>
        public virtual void Parse(string name, string[] fieldType, List<string> fieldName, string[] data)
        {
            if (fieldType[0].Length > 0)
            {
                Type type = GetType();
                for (int i = 0; i < fieldType.Length; i++)
                {
                    try
                    {
                        if (fieldName[i].Length <= 0)
                            break;
                        FieldInfo fieldInfo = type.GetField(fieldName[i]);
                        fieldInfo.SetValue(this, converFieldValue(fieldType[i].ToUpper(), data[i]));
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Config Parse Error[" + i + "]\n" + e);
                    }
                }
            }
            else
            {
                for (int i = 0; i < fieldType.Length; i++)
                {
                    ConfigDataDic.Add(FUtils.StrToInt(fieldName[i]), data[i]);
                }
            }
        }

        public virtual void ParseToDic(string name, string[] fieldType, List<string> fieldName, string[] data)
        {
            for (int i = 0; i < fieldType.Length; i++)
            {
                ConfigDataDic.Add(FUtils.StrToInt(fieldName[i]), data[i]);
            }
        }

        public virtual bool Contains(int key)
        {
            return ConfigDataDic.ContainsKey(key);
        }

        public virtual string Get(int key)
        {
            string rt = null;
            ConfigDataDic.TryGetValue(key, out rt);
            return rt;
        }

        public virtual T Get<T>(int key)
        {
            object v = null;
            if (typeof(T) == typeof(int))
            {
                v = FUtils.StrToInt(Get(key));
            }
            else if (typeof(T) == typeof(float))
            {
                v = FUtils.StrToFloat(Get(key));
            }
            return (T)v;
        }


        //public virtual void Get(int key, out string value)
        //{
        //    ConfigDataDic.TryGetValue (key, out value);
        //}


        public virtual string this[int key]
        {
            get
            {
                return Get(key);
            }
        }


    }
}