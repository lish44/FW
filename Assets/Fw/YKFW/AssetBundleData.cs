// Decompiled with JetBrains decompiler
// Type: ScriptableObjectClassLibrary.AssetBundleData
// Assembly: ScriptableObjectClassLibrary, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C77A4B0F-4620-4B3E-B0CC-96C1D946C334
// Assembly location: C:\Workspace\C_int\Unity\Assets\Plugins\GameDll\ScriptableObjectClassLibrary.dll

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ScriptableObjectClassLibrary
{
    [Serializable]
    public class ListData
    {
        public List<string> Data;
    }
    [Serializable]
  public class AssetBundleData : ScriptableObject
  {
    public List<string> Path;
    public List<ListData> AssetBundlePathList;
    public List<bool> Common;
    public List<string> AssetPath;
    public List<string> FullName;
    public List<bool> IsCommonRes;
    public List<bool> IsSolid;
    public List<string> AssetBundlePath;

    public void InitAssetBundleData(string txt)
    {
      this.Path = new List<string>();
      this.Common = new List<bool>();
      this.AssetBundlePathList = new List<ListData>();
      string[] strArray1 = txt.Split(new char[2]
      {
        '\r',
        '\n'
      }, StringSplitOptions.RemoveEmptyEntries);
      int length = strArray1.Length;
      if (length <= 1)
        return;
      for (int index = 1; index < length; ++index)
      {
        string[] strArray2 = strArray1[index].Split('\t');
        this.Path.Add(strArray2[0]);
        this.Common.Add("1" == strArray2[1]);
        this.AssetBundlePathList.Add(new ListData()
        {
          Data = ((IEnumerable<string>) strArray2[2].Split(new char[1]
          {
            '+'
          }, StringSplitOptions.RemoveEmptyEntries)).ToList<string>()
        });
      }
    }

    public void InitMainFestData(string txt)
    {
      this.AssetPath = new List<string>();
      this.FullName = new List<string>();
      this.IsCommonRes = new List<bool>();
      this.IsSolid = new List<bool>();
      this.AssetBundlePath = new List<string>();
      string[] strArray1 = txt.Split(new char[2]
      {
        '\r',
        '\n'
      }, StringSplitOptions.RemoveEmptyEntries);
      int length = strArray1.Length;
      if (length <= 1)
        return;
      for (int index = 1; index < length; ++index)
      {
        string[] strArray2 = strArray1[index].Split('\t');
        this.AssetPath.Add(strArray2[0]);
        this.FullName.Add(strArray2[1]);
        this.IsCommonRes.Add("1" == strArray2[2]);
        this.IsSolid.Add("1" == strArray2[3]);
        this.AssetBundlePath.Add(strArray2[4]);
      }
    }
  }
}
