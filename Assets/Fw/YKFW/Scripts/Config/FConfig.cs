using System.Collections;
//-----------------------------------------------------------------------
//| Autor:Adam                                                             |
//-----------------------------------------------------------------------
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace FW
{

    public static class FConfig
    {
        // 配置表缓存(解析好的)
        private static Dictionary<string, Dictionary<int, FConfigData>> m_configDic = new Dictionary<string, Dictionary<int, FConfigData>>();
        // 配置表缓存(未解析的)
        private static Dictionary<string, object[]> m_tempCacheConfigDataDic = new Dictionary<string, object[]>();


        static FConfig()
        {
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
            data = data.Replace("\r", "");

            System.Type type = typeof(T);
            object obj = System.Activator.CreateInstance(type);
            string[] list = data.Split(';');
            foreach (string item in list)
            {
                string str = item;
                int comment = item.Replace("\n", "").IndexOf("//");
                if (comment == 0)
                {
                    string[] temp = item.Split('\n');
                    str = temp[temp.Length - 1];
                }
                str = str.Replace("\n", "");
                if (str.Length == 0)
                    continue;

                string[] keyValue = str.Split('=');
                System.Reflection.FieldInfo variable = type.GetField(keyValue[0]);

                if (variable.FieldType == typeof(int))
                {
                    variable.SetValue(obj, int.Parse(keyValue[1]));
                }
                else if (variable.FieldType == typeof(float))
                {
                    variable.SetValue(obj, float.Parse(keyValue[1]));
                }
                else if (variable.FieldType == typeof(byte))
                {
                    variable.SetValue(obj, byte.Parse(keyValue[1]));
                }
                else
                {
                    variable.SetValue(obj, keyValue[1]);
                }
            }
            return (T)obj;
        }

        public static Dictionary<string, string> parseFConfig(string data)
        {
            data = data.Replace("\r", "");

            Dictionary<string, string> dic = new Dictionary<string, string>();
            string[] list = data.Split('\n');
            foreach (string item in list)
            {
                if (item.Length < 2 || item.Substring(0, 2) == "//")
                    continue;
                if (item.Length == 0)
                    continue;
                string[] keyValue = item.Split('=');
                dic.Add(keyValue[0].Replace("\t", ""), keyValue[1].Replace("\\n", "\n"));
            }
            return dic;
        }

        public static void removeExcelCache<T>()
        {
            string name = typeof(T).Name;
            m_configDic.Remove(name);
            m_tempCacheConfigDataDic.Remove(name);
        }

        public static void parseExcelAndCache<T>(string name, string excel) where T : FConfigData, new()
        {
            parseExcelAndCache(name, excel, typeof(T));
        }

        public static void parseExcelAndCache(string name, string excel, System.Type type)
        {
            if (excel.Length == 0)
                return;
            m_tempCacheConfigDataDic.Add(name, new object[] { excel, type });
        }

        public static void parseExcelAndAddCache(string name, string excel, System.Type type)
        {
            char splitStr = '\t';

            if (!m_configDic.ContainsKey(name))
            {
                m_configDic.Add(name, new Dictionary<int, FConfigData>());
            }

            string text = excel.Replace("\r", "");
            string[] line = text.Split('\n');

            List<string> fieldName = new List<string>(line[2].Split(splitStr));
            string[] fieldType = line[0].Split(splitStr);

            ConstructorInfo info = type.GetConstructor(new System.Type[0]);

            string[] temp;
            for (int i = 0; i < line.Length; i++)
            {
                try
                {
                    if (line[i] == "" || i < 3)
                        continue;
                    temp = line[i].Split(splitStr);
                    if (temp[0].Length <= 0)
                        continue;

                    int sid = FUtils.StrToInt(temp[0]);
                    m_configDic[name].Add(sid, (FConfigData)info.Invoke(null));
                    m_configDic[name][sid].Parse(name, fieldType, fieldName, temp);

                }
                catch (System.Exception e)
                {
                    throw new System.Exception("config Error : " + name + "\n" + line[i] + "\n\n" + e);
                }
            }
        }



        private static void checkCache(string name)
        {
            if (m_tempCacheConfigDataDic.ContainsKey(name))
            {
                object[] v = m_tempCacheConfigDataDic[name];
                parseExcelAndAddCache(name, (string)v[0], (System.Type)v[1]);
                m_tempCacheConfigDataDic.Remove(name);
            }
        }



        public static Dictionary<int, FConfigData> getConfig(string name)
        {
            checkCache(name);
            Dictionary<int, FConfigData> v = null;
            m_configDic.TryGetValue(name, out v);
            return v;
        }

        public static Dictionary<int, FConfigData> getConfig<T>() where T : FConfigData
        {
            return getConfig(typeof(T).Name);
        }



        public static bool contains(string name)
        {
            return getConfig(name) != null;
        }
        private static bool contains(string name, int id)
        {
            return getConfig(name).ContainsKey(id);
        }
        public static FConfigData get(string name, int id)
        {
            FConfigData v = null;
            getConfig(name).TryGetValue(id, out v);
            return v;
        }

        public static T get<T>(string name, int id) where T : FConfigData
        {
            return get(name, id) as T;
        }

        public static T get<T>(int id) where T : FConfigData
        {
            return get(typeof(T).Name, id) as T;
        }
        public static bool contains<T>(int id) where T : FConfigData
        {
            return contains(typeof(T).Name, id);
        }
        public static bool contains<T>() where T : FConfigData
        {
            return contains(typeof(T).Name);
        }


        public static T get<T>(string name, int id, int key)
        {
            return get(name, id).Get<T>(key);
        }


        public static void clear()
        {
            m_configDic.Clear();
            m_tempCacheConfigDataDic.Clear();
        }

    }

}