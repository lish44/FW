using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FW
{

    public class HandleBase
    {
        internal bool UsedAssetBundle;
        public FAssetInfo Info;

        internal FrameDef.TaskPriority Priority = FrameDef.TaskPriority.Normal;


        /// <summary>
        /// 加载路径
        /// </summary>
        public string LoadPath
        {
            get
            {
                return FResourceCommon.GetLoadPath(UsedAssetBundle, Info.AssetPath, Info.BundlePath);
            }
        }
    }

}
