using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
public class FImage : Image
{
    public static Action<string, FImage, Action<FImage>> LoadImage;
    public static Action<string, FImage, Action<FImage>> LoadImageMat;

    protected override void Awake()
    {
        base.Awake();
        if (sprite == null) this.Alpha = 0;
    }
    public float Alpha
    {
        get
        {
            return color.a;
        }
        set
        {
            Color n = color;
            n.a = Mathf.Clamp(value, 0, 1);
            color = n;
        }
    }
    private bool mIsGray;
    public bool IsGray
    {
        get
        {
            return mIsGray;
        }
    }

    private Color mOldColor;
    public void SetGray(bool _isGray)
    {
        if (mIsGray == _isGray) return;
        mIsGray = _isGray;
        if (_isGray)
        {
            mOldColor = color;
            color = new Color(0, 0, 0, color.a);
        }
        else
        {
            color = new Color(mOldColor.r, mOldColor.g, mOldColor.b, Alpha);
        }
    }

    public void Load(string _name, Action<FImage> _callBack = null, Material _Mat = null)
    {
        Alpha = 0;
        if (string.IsNullOrEmpty(_name))
        {
            _callBack?.Invoke(null);
            return;
        }

        LoadImage(_name, this, _callBack);


    }
}
