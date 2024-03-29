﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;


public static class MD5Utils
{
    public static string GetMD5(FileInfo info)
    {
        if (info.Name.Substring(0, 1) == ".")
        {
            return null;
        }
        byte[] byData = new byte[info.Length];
        FileStream stream = File.OpenRead(info.FullName);
        stream.Read(byData, 0, (int)info.Length);
        stream.Flush();
        stream.Close();
        return GetMD5(byData);
    }
    public static string GetMD5(byte[] bytes)
    {
        // encrypt bytes
        System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
        byte[] hashBytes = md5.ComputeHash(bytes);
        // Convert the encrypted bytes back to a string (base 16)
        string hashString = "";
        for (int i = 0; i < hashBytes.Length; i++)
        {
            hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
        }
        return hashString.PadLeft(32, '0');

    }
    public static string GetMD5(string str)
    {
        UTF8Encoding ue = new UTF8Encoding();
        byte[] bytes = ue.GetBytes(str);
        return GetMD5(bytes);
    }
}

