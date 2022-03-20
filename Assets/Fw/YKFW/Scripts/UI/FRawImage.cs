using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace FW
{

    public class FRawImage : RawImage
    {
        private FResourceRef m_ResourceRef;

        /// <summary>
        /// 加载贴图
        /// </summary>
        /// <param name="rawImage"></param>
        /// <param name="path"></param>
        /// <param name="callBack"></param>
        /// <param name="controlAlpha">true 控制贴图alpha值，加载完成后设置为1</param>

        public void LoadRawImage(string path, CallBack<FRawImage> callBack, bool controlAlpha = false)
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("The path can not be null.");
                if (null != callBack)
                {
                    callBack(null);
                }

                return;
            }

            if (controlAlpha)
            {
                SetAlpha(0);
            }
            FResourcesManager.Inst.LoadObject(path, typeof(Texture2D), (obj) =>
            {
                if (null != obj)
                {
                    if (null != m_ResourceRef)
                    {
                        m_ResourceRef.ReleaseImmediate();
                    }

                    FResourceRef _Ref = obj as FResourceRef;

                    m_ResourceRef = _Ref;
                    Texture tex = _Ref.Asset as Texture;
                    this.texture = tex;

                    if (controlAlpha)
                    {
                        SetAlpha(1);
                    }
                    if (null != callBack)
                    {
                        callBack(this);
                        callBack = null;
                    }
                }

            }, false, false, FrameDef.TaskPriority.Highest);
        }


        private void OnDestroy()
        {
            this.texture = null;
            if (null != m_ResourceRef)
            {
                m_ResourceRef.ReleaseImmediate();
            }
            m_ResourceRef = null;
  
        }

        public void SetAlpha(float alpha)
        {

            MaskableGraphic _graphic = this as MaskableGraphic;
            Color n = _graphic.color;
            n.a = Mathf.Clamp(alpha, 0, 1);
            _graphic.color = n;

        }
        public float GetAlpha()
        {
            return (this as MaskableGraphic).color.a;
        }
    }
}
