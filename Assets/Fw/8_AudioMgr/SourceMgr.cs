using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FW
{
    public class SourceMgr
    {
        //-----------------------------字段，属性，引用---------------------------------
        private const int AudioSourceCount = 2;

        /// <summary>
        /// 所有播放器的集合
        /// </summary>
        List<AudioSource> m_allSources;

        /// <summary>
        /// 用于保存释放的临时list
        /// </summary>
        /// <typeparam name="AudioSource">实际类型是播放器</typeparam>
        List<AudioSource> tmpSources = new List<AudioSource>();

        GameObject go;
        //=============================================================================

        //---------------------------------构造函数-------------------------------------
        /// <summary>
        /// 构造函数 传那个对象就在那个对象上添加播放器
        /// </summary>
        /// /// <param name="tmpGo">游戏对象</param>
        public SourceMgr(GameObject _tmpGo)
        {
            this.go = _tmpGo;
            Initial();
        }
        //=============================================================================

        //---------------------------------初始化播放器----------------------------------
        /// <summary>
        /// 初始化allSources集合
        /// </summary>
        public void Initial()
        {
            m_allSources = new List<AudioSource>();
            for (int i = 0; i < AudioSourceCount; i++)
            {
                AudioSource tmpSource = go.AddComponent<AudioSource>(); //对象挂载音效组件
                m_allSources.Add(tmpSource); //默认存2个音效组件
            }
        }
        //=============================================================================

        //---------------------------------获取空闲播放器---------------------------------
        /// <summary>
        /// 获取空闲播放器
        /// </summary>
        /// <returns>返回 AudioSource</returns>
        public AudioSource GetFreeAudio()
        {
            //循环找到空闲的
            for (int i = 0; i < m_allSources.Count; i++)
            {
                if (!m_allSources[i].isPlaying)
                    return m_allSources[i];
            }
            //如果上面循环完没找到就动态创建一个
            AudioSource tmpSource = go.AddComponent<AudioSource>();
            m_allSources.Add(tmpSource);
            return tmpSource;
        }
        //=============================================================================

        //--------------------------------释放空闲的播放器--------------------------------
        /// <summary>
        /// 释放空闲的播放器
        /// </summary>
        public void ReleaseFreeAudio()
        {
            int tmpCount = 0;

            /*从allSources里找空闲放到tmp里准备删除
              这里必须要单独存出来 不能直接在循环里面删*/
            for (int i = 0; i < m_allSources.Count; i++)
            {
                if (!m_allSources[i].isPlaying)
                { //空闲一个就计数
                    tmpCount++;
                    if (tmpCount > AudioSourceCount)
                    {
                        tmpSources.Add(m_allSources[i]);
                    }
                }
            }
            //删all里面播放器 数据是从上面那个循环拿到的 
            for (int i = 0; i < tmpSources.Count; i++)
            {
                AudioSource tmpSource = tmpSources[i];
                //从list中移除
                m_allSources.Remove(tmpSource);
                //从场景中移除组件
                GameObject.Destroy(tmpSource);
            }
            //本质函数走完会清空所以数据 ，确保没有对象再引用 防止没被删完
            tmpSources.Clear();
        }
        //=============================================================================

        public void Stop(string _audioName)
        {
            for (int i = 0; i < m_allSources.Count; i++)
            {
                if (m_allSources[i].isPlaying && m_allSources[i].clip.name.Equals(_audioName))
                {
                    m_allSources[i].Stop();
                    // if (m_allSources[i].loop) m_allSources[i].loop = false;
                }
            }
        }

        public void Stop()
        {
            for (int i = 0; i < m_allSources.Count; i++)
            {
                if (m_allSources[i].isPlaying)
                {
                    m_allSources[i].Stop();
                }
            }
        }

        public bool IsPlaying()
        {
            for (int i = 0; i < m_allSources.Count; i++)
            {
                if (m_allSources[i].isPlaying) return true;
            }
            return false;
        }

        public bool IsPlaying(string _audioName)
        {
            for (int i = 0; i < m_allSources.Count; i++)
            {
                if (m_allSources[i].isPlaying && m_allSources[i].clip.name.Equals(_audioName))
                    return true;
            }
            return false;
        }

        public AudioSource GetSource(string _clipName)
        {
            for (int i = 0; i < m_allSources.Count; i++)
            {
                if (m_allSources[i].isPlaying && m_allSources[i].clip.name.Equals(_clipName))
                {
                    return m_allSources[i];
                }
            }
            return null;
        }
    }
}