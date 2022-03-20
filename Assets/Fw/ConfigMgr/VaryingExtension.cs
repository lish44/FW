using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public abstract class VaryingExtension
{
    private static VaryingExtension _defaultEx;

    public static VaryingExtension DefaultEx
    {
        get
        {
            if (_defaultEx == null)
            {
                _defaultEx = new DummyVaryingExtension();
            }
            return _defaultEx;
        }
    }

    //注意:默认方法只能处理子类字段全为值类型,对于包含引用类型的子类必须手动重写
    public override int GetHashCode()
    {
        FieldInfo[] fieldInfos = GetType().GetFields();

        int hash = 17;
        for (int i = 0; i < fieldInfos.Length; i++)
        {
            FieldInfo field = fieldInfos[i];
            hash = hash * 3 + field.GetValue(this).GetHashCode();
        }
        return hash;
    }

    //注意:默认方法只能处理子类字段全为值类型,对于包含引用类型的子类必须手动重写
    public override bool Equals(object obj)
    {
        if (obj == null || obj.GetType() != GetType())
        {
            return false;
        }
        FieldInfo[] fieldInfos = GetType().GetFields();

        for (int i = 0; i < fieldInfos.Length; i++)
        {
            FieldInfo field = fieldInfos[i];
            if (!field.GetValue(this).Equals(field.GetValue(obj)))
            {
                return false;
            }
        }
        return true;
    }

    private class DummyVaryingExtension : VaryingExtension
    {

        public override int GetHashCode()
        {

            return 9527;
        }

        //注意:默认方法只能处理子类字段全为值类型,对于包含引用类型的子类必须手动重写
        public override bool Equals(object obj)
        {

            return true;
        }

    }
}
