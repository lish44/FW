using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FW
{
    ////包名 路径 包含的资源的路径 大小 包版本号 资源MD5码 
    public class FAssetBundleData
    {
        public string Path;

        /// <summary>
        /// 依赖的包路径,若没有依赖则为null
        /// </summary>
        public List<string> AssetBundlePathList;

        /// <summary>
        /// 字节
        /// </summary>
        public int Size;
        public int Version;
        public uint Crc;//默认为0
        public uint CompressCrc;//默认为0
        /// <summary>
        /// 是否为公共包
        /// </summary>
        public bool Common = false;
        /// <summary>
        /// 标记为不丢包
        /// </summary>
        public bool IsSolid = false;

        public FAssetBundleData()
        {

        }

        public void Init(string path, List<string> assetBundlePathList, int size, int version, uint crc, uint compressCrc)
        {
            Path = path;
            AssetBundlePathList = assetBundlePathList;

            Size = size;
            Version = version;
            Crc = crc;
            CompressCrc = compressCrc;

        }

        public void AddDependentAssetBundlePath(string assetBundlePath)
        {
            if(null == AssetBundlePathList)
            {
                AssetBundlePathList = new List<string>();
            }
            if(!AssetBundlePathList.Contains(assetBundlePath))
            {
                AssetBundlePathList.Add(assetBundlePath);
            }
        }

    }

}
