using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

public static class YKUtils
{
    static public void BubbleSort<T>(this T[] array, Comparison<T> comparison)
    {
        T temp;//存储临时变量
        for (int i = 0; i < array.Length; i++)
            for (int j = i - 1; j >= 0; j--)
                //if (intArray[j + 1] < intArray[j])
                if (comparison(array[j + 1], array[j]) < 0)
                {
                    temp = array[j + 1];
                    array[j + 1] = array[j];
                    array[j] = temp;
                }
    }

}

[Serializable]
public class Config
{
    [ConfigComment("ID")]
    public readonly long id;
    [ConfigComment("描述")]
    public readonly string describe;

    [NonSerialized]
    private string _uniformMD5;


    public Config(long id)
    {
        this.id = id;
    }

    public Config()
    {
    }

    [NonSerialized]
    private HashSet<string> _assets = new HashSet<string>();

    public HashSet<string> Assets
    {
        get { return _assets; }
    }

    public bool UniformCheck()
    {
        string md5 = _getMD5();

        if (string.IsNullOrEmpty(_uniformMD5))
        {
            _uniformMD5 = md5;
            return true;
        }

        bool b = _uniformMD5 == md5;
        //            _uniformMD5 = md5;

        return b;
    }


    private string _getMD5()
    {
        FieldInfo[] fieldInfos = GetType().GetFields();
        fieldInfos.BubbleSort((f1, f2) =>
        {
            return f1.Name.GetHashCode().CompareTo(f2.Name.GetHashCode());
        });
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < fieldInfos.Length; i++)
        {
            FieldInfo field = fieldInfos[i];
            Attribute[] attributes = Attribute.GetCustomAttributes(field, typeof(NonSerializedAttribute));
            if (attributes.Length != 0)
            {
                continue;
            }
            object value = field.GetValue(this);
            sb.Append(_getConfigFieldMD5(value));
        }

        return MD5Utils.GetMD5(sb.ToString());
    }

    private static string _getConfigFieldMD5(object obj)
    {
        if (obj == null)
        {
            return MD5Utils.GetMD5("null");
        }
        Type t = obj.GetType();
        string md5 = "";
        if (t.IsArray || (t.IsGenericType && typeof(IEnumerable).IsAssignableFrom(t)))
        {
            IEnumerable arr = (IEnumerable)obj;
            StringBuilder sb = new StringBuilder();
            foreach (object o in arr)
            {
                sb.Append(_getConfigFieldMD5(o));
            }
            md5 = MD5Utils.GetMD5(sb.ToString());
        }
        else if (t.IsSubclassOf(typeof(Config)))
        {
            md5 = ((Config)obj)._getMD5();
        }
        else
        {
            md5 = MD5Utils.GetMD5(obj.ToString());
        }

        return md5;
    }
}

public class ConfigCommentAttribute : Attribute
{
    public readonly string comment;
    public ConfigCommentAttribute(string comment)
    {
        this.comment = comment;
    }
}

public class ConfigAssetAttribute : Attribute { }
public class ConfigPathAttribute : Attribute { }

