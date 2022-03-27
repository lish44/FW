using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace FW
{
    public class ScenesMgr : SingletonBase<ScenesMgr>
    {

        public void LoadScene(string _sceneName, CallBack _callback)
        {
            SceneManager.LoadScene(_sceneName);
            _callback();
        }

        public void LoadSceneAsyn(string _sceneName, CallBack _callback)
        {
            MonoMgr.Ins.StartCoroutine(ReallyLoadSceneAsyn(_sceneName, _callback));
        }

        private IEnumerator ReallyLoadSceneAsyn(string _sceneName, CallBack _callback)
        {
            AsyncOperation ao = SceneManager.LoadSceneAsync(_sceneName);
            //更新进度条
            float ProgressValue;
            ao.allowSceneActivation = false;
            FW.Evencenter.Ins.AddEventListener<bool>(EventName.LOADINGFINISH, (b) =>
            {
                ao.allowSceneActivation = b;
            });

            while (!ao.isDone)
            {
                if (ao.progress < 0.9f)
                {
                    ProgressValue = ao.progress;
                }
                else
                {
                    ProgressValue = 1.0f;
                }
                FW.Evencenter.Ins.EventTrigger<float>(EventName.LOADING, ProgressValue);
                yield return null;
            }
            ProgressValue = 0;
            // GC清理一次;
            System.GC.Collect();
            FW.AudioMgr.Ins.ReleaseFreeAudio();
            FW.PoolMgr.Ins.CleanAll();
            _callback();
        }
    }
}