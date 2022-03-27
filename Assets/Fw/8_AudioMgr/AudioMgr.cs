using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FW
{
    public class AudioMgr : SingletonBase<AudioMgr>
    {
        SourceMgr sourceMgr;
        ClipMgr clipMgr;

        public override void Init()
        {
        }
        public void Init(CallBack _callback = null)
        {
            InitData();
            _callback?.Invoke();
        }
        public void InitData()
        {
            GameObject audioGo = new GameObject("AudioGameObject");
            clipMgr = new ClipMgr();
            sourceMgr = new SourceMgr(audioGo);
            GameObject.DontDestroyOnLoad(audioGo);
        }

        public void Play(string _audioName, bool _isLoop = false, float _volume = 1f)
        {
            //找对应片段 返回SingleClip
            var tempClip = clipMgr.FindClipByName(_audioName);
            if (tempClip == null)
            {
                Debug.Log(_audioName + " : 音频文件不存在 请检查拼写是否正确");
                return;
            }
            //找空闲播放器
            var tempSorce = sourceMgr.GetFreeAudio();
            tempSorce.loop = _isLoop;
            tempSorce.volume = _volume;
            tempClip.Play(tempSorce);
        }

        public void Stop(string _audioName)
        {
            sourceMgr.Stop(_audioName);
        }

        public void Stop()
        {
            sourceMgr.Stop();
        }

        public bool IsPlaying()
        {
            return sourceMgr.IsPlaying();
        }

        public bool IsPlaying(string _audioName)
        {
            return sourceMgr.IsPlaying(_audioName);
        }

        public void ReleaseFreeAudio()
        {
            this.Stop();
            sourceMgr.ReleaseFreeAudio();
        }

        public AudioSource GetAudioSorce(string _audioName)
        {
            return sourceMgr.GetSource(_audioName);
        }
    }
}