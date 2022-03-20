using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FW
{
    public class ClipMgr
    {
        string[] m_clipName; // 所有音效的名字
        List<SingleClip> m_allSingleClip = new List<SingleClip>();
        Dictionary<string, SingleClip> mAllSingleClip = new Dictionary<string, SingleClip>();

        public ClipMgr()
        {
            InitData();
        }
        //----------------------------配置文件读取 加载音效到内存-------------------------
        void InitData()
        {
            var _names = ResMgr.Ins.GetTypeByNames<AudioPath>();
            var _iter = _names.GetEnumerator();
            while (_iter.MoveNext())
            {
                var _clip = FW.ResMgr.Ins.Load<AudioClip>(_iter.Current);
                if (!mAllSingleClip.ContainsKey(_iter.Current))
                {
                    mAllSingleClip.Add(_iter.Current, new SingleClip(_clip));
                    continue;
                }
                FW.Log.Error("ClipMgr : 同名音效" + _names);
            }

        }
        //=============================================================================

        //---------------------------------根据名字找音源片段----------------------------
        /// <summary>
        /// 更具名字找到SingleClip
        /// </summary>
        /// <param name="_clipName">音效名字</param>
        /// <returns>返回SingleClip</returns>
        public SingleClip FindClipByName(string _clipName)
        {
            SingleClip _sc;
            mAllSingleClip.TryGetValue(_clipName, out _sc);
            return _sc;
        }
        //=============================================================================

    }
}