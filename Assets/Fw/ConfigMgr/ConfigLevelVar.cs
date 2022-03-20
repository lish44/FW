using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ConfigLevelVar : Config, IVaryingNumber
{
    [ConfigComment("等级初始值")] public readonly double firstItem;
    [ConfigComment("等级公差")] public readonly double tolerance;
    [ConfigComment("等级数组")] public readonly ReadonlyArray<double> extendArray = new ReadonlyArray<double>(new double[] { 0 });
    [ConfigComment("品级数组")] public readonly ReadonlyArray<double> gradeArray = new ReadonlyArray<double>(new double[] { 0 });
    //        [ConfigComment("被动等级关联")] public readonly int ExLvAttach = 0;

    private readonly bool _invert = false;

    public ConfigLevelVar(long id) : base(id)
    {
    }

    public ConfigLevelVar()
    {
    }

    public ConfigLevelVar(bool invert, ConfigLevelVar src) : base(src.id)
    {
        _invert = invert;
        firstItem = src.firstItem;
        tolerance = src.tolerance;
        extendArray = src.extendArray;
        gradeArray = src.gradeArray;
    }

    public double GetValue(VaryingExtension ex)
    {
        int index = 0;
        int grade = 0;

        if (ex is LevelVaryingExtension)
        {
            LevelVaryingExtension lvEx = (LevelVaryingExtension)ex;
            index = lvEx.Lv - 1;
            grade = lvEx.Grade - 1;
        }

        if (index < 0) return firstItem;

        double result = firstItem + tolerance * (index);

        result += extendArray[YKMath.Clamp(index, 0, extendArray.Length - 1)];
        result += gradeArray[YKMath.Clamp(grade, 0, gradeArray.Length - 1)];

        return _invert ? -result : result;
    }

    public double GetValue(int lv, int grade, params int[] passiveLv)
    {
        return GetValue(new LevelVaryingExtension(lv, 0, passiveLv));
    }
}

public class LevelVaryingExtension : VaryingExtension
{
    public static LevelVaryingExtension DEFALUT_LV_EX = new LevelVaryingExtension(1, 1);

    public readonly int Lv;
    public readonly int[] PassiveLv;
    public readonly int Grade;

    public LevelVaryingExtension(int lv, int grade, params int[] pLv)
    {
        Lv = lv;
        PassiveLv = new int[pLv.Length];
        for (int i = 0; i < pLv.Length; i++)
        {
            PassiveLv[i] = pLv[i];
        }
        Grade = grade;
    }

}
