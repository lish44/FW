using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FW {
    public class SingleClip {
        AudioClip clip;

        public SingleClip (AudioClip _clip) {
            this.clip = _clip;
        }

        /// <summary>
        /// 播放一个音效片段
        /// </summary>
        /// <param name="_audioSource">传播放器</param>
        public void Play (AudioSource _audioSource) {
            _audioSource.clip = clip;
            _audioSource.Play ();
        }
    }
}