using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace FW
{
    public class FImage : Image
    {

        private FResourceRef m_ResourceRef;

        /// <summary>
        /// 加载图集以及对应的精灵
        /// </summary>
        /// <param name="image"></param>
        /// <param name="path">图集路径</param>
        /// <param name="spriteName">精灵名字</param>
        /// <param name="callBack"></param>
        /// <param name="controlAlpha">true 控制贴图alpha值，加载完成后设置为1</param>
        public void LoadImage(string path, string spriteName, CallBack<FImage> callBack, bool controlAlpha = false)
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
            FResourcesManager.Inst.LoadObject(path, typeof(UnityEngine.Sprite), (obj) =>
            {
                if (null != obj)
                {
                    if (null != m_ResourceRef)
                    {
                        m_ResourceRef.ReleaseImmediate();
                    }
                    FResourceRef _Ref = obj as FResourceRef;
                    m_ResourceRef = _Ref;
                     Object [] sprites = _Ref.AllAsset;

                    int len = _Ref.AllAsset.Length;

                    for (int i = 0; i < len; i++)
                    {
                        if (sprites[i].name == spriteName)
                        {
                            this.sprite = sprites[i] as Sprite;

                            if (controlAlpha)
                            {
                                SetAlpha(1);
                            }
                            if (null != callBack)
                            {
                                callBack(this);
                                callBack = null;
                            }

                            return;
                        }

                    }


                }

            }, false, false, FrameDef.TaskPriority.Highest);
        }

        private void OnDestroy()
        {
            this.sprite = null;
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
