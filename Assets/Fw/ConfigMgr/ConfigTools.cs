using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ConfigTools
{
    public static Dictionary<string, string> getDic(string scriptArgss)
    {
        if (scriptArgss == "" || scriptArgss == null) return null;
        string[] temp = scriptArgss.Split('|');
        Dictionary<string, string> ttt = new Dictionary<string, string>();
        for (int i = 0; i < temp.Length; i++)
        {
            string[] k = temp[i].Split(':');
            ttt.Add(k[0], k[1]);
        }
        return ttt;
    }

    public static Dictionary<string, string[]> getDicAr(string scriptArgss)
    {
        if (scriptArgss == "" || scriptArgss == null) return null;
        string[] temp = scriptArgss.Split('|');
        Dictionary<string, string[]> ttt = new Dictionary<string, string[]>();
        for (int i = 0; i < temp.Length; i++)
        {
            string[] k = temp[i].Split(':');
            ttt.Add(k[0], k[1].Split('+'));
        }
        return ttt;
    }

    public static Dictionary<string, string[]> getSDicAr(string scriptArgss)
    {
        if (scriptArgss == "" || scriptArgss == null) return null;
        string[] temp = scriptArgss.Split('|');
        Dictionary<string, string[]> ttt = new Dictionary<string, string[]>();
        for (int i = 0; i < temp.Length; i++)
        {
            string[] k = temp[i].Split(':');
            ttt.Add(k[0], k[1].Split('~'));
        }
        return ttt;
    }
    public static void WriteSimpleBuffer(Type t, object value, ByteBuffer buffer)
    {
        if (t.IsArray)
        {
            if (value == null)
            {
                buffer.writeShort(0);
            }
            else
            {
                IList list = (IList)value;
                Type subType = t.GetElementType();
                int len = list.Count;
                buffer.writeShort(len);
                for (int i = 0; i < len; i++)
                {
                    WriteSimpleBuffer(subType, list[i], buffer);
                }
            }
        }
        else if (t.IsEnum)
        {
            buffer.writeInt(Convert.ToInt32(Enum.ToObject(t, value)));
        }
        else
        {
            switch (t.Name)
            {
                case "String":
                    if (value == null)
                        buffer.writeUTF(null);
                    else
                        buffer.writeUTF(value.ToString());
                    break;
                case "Int32":
                case "UInt32":
                    if (value == null)
                        buffer.writeInt(0);
                    else
                        buffer.writeInt(int.Parse(value.ToString()));
                    break;
                case "Int64":
                case "UInt64":
                    if (value == null)
                        buffer.writeLong(0);
                    else
                        buffer.writeLong(long.Parse(value.ToString()));
                    break;
                case "Byte":
                case "SByte":
                    if (value == null)
                        buffer.writeByte(0);
                    else
                        buffer.writeByte(byte.Parse(value.ToString()));
                    break;
                case "Int16":
                case "UInt16":
                    if (value == null)
                        buffer.writeShort(0);
                    else
                        buffer.writeShort(short.Parse(value.ToString()));
                    break;
                case "Double":
                    if (value == null)
                        buffer.writeDouble(0);
                    else
                        buffer.writeDouble(double.Parse(value.ToString()));
                    break;
                case "Single":
                    if (value == null)
                        buffer.writeFloat(0);
                    else
                        buffer.writeFloat(float.Parse(value.ToString()));
                    break;
                case "Char":
                    if (value == null)
                        buffer.writeChar(0);
                    else
                        buffer.writeChar(char.Parse(value.ToString()));
                    break;
                case "Boolean":
                    if (value == null)
                        buffer.writeBoolean(false);
                    else
                        buffer.writeBoolean(bool.Parse(value.ToString()));
                    break;
            }
        }
    }
    public static object ReadSimpleBuffer(Type t, ByteBuffer buffer)
    {
        if (t.IsArray)
        {
            int len = buffer.readUnsignedShort();
            Type subType = t.GetElementType();
            Array ins = Array.CreateInstance(subType, len);
            for (int i = 0; i < len; i++)
            {
                ins.SetValue(ReadSimpleBuffer(subType, buffer), i);
            }
            return ins;
        }
        else if (t.IsEnum)
        {
            return Enum.ToObject(t, buffer.readInt());
        }
        else
        {
            switch (t.Name)
            {
                case "String":
                    return buffer.readUTF();
                case "Int32":
                case "UInt32":
                    return buffer.readInt();
                case "Int64":
                case "UInt64":
                    return buffer.readLong();
                case "Byte":
                    return buffer.readByte();
                case "SByte":
                    return buffer.readUnsignedByte();
                case "Int16":
                    return buffer.readShort();
                case "UInt16":
                    return buffer.readUnsignedShort();
                case "Double":
                    return buffer.readDouble();
                case "Single":
                    return buffer.readFloat();
                case "Char":
                    return buffer.readChar();
                case "Boolean":
                    return buffer.readBoolean();
            }
        }
        return null;
    }

    public static Type GetArrayElementType(Type t)
    {
        Type type = t;

        while (type.IsArray)
        {
            type = type.GetElementType();
        }

        return type;
    }

    public static Type GetGenericElementType(Type t)
    {
        Type type = t;

        while (type.IsGenericType && typeof(IEnumerable).IsAssignableFrom(t))
        {
            Type[] gas = type.GetGenericArguments();
            type = gas[0];
        }

        return type;
    }

    public static int GetGenericDimension(Type t)
    {
        int d = 0;
        if (t.IsGenericType && typeof(IEnumerable).IsAssignableFrom(t))
        {
            d = GetGenericDimension(t.GetGenericArguments()[0]) + 1;
        }

        return d;
    }

    public static int GetArrayDimension(Type t)
    {
        int d = 0;
        if (t.IsArray)
        {
            d = GetArrayDimension(t.GetElementType()) + 1;
        }

        return d;
    }

}

