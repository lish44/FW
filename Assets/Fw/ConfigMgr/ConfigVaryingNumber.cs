using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ConfigVaryingNumber : Config
{
    //注意:所有子类必须支持Ex不是自身对应的情况
    public abstract double GetValue(VaryingExtension ex);
}

public interface IVaryingExtensionRef
{
    VaryingExtension VaryingExtension { get; set; }
}

public interface IVaryingNumber
{
    //注意:所有子类必须支持Ex不是自身对应的情况
    double GetValue(VaryingExtension ex);
}

public class ConstantVarNumber : IVaryingNumber
{
    public static readonly ConstantVarNumber ZERO = new ConstantVarNumber(0);

    public readonly double Value;

    public ConstantVarNumber(double value)
    {
        Value = value;
    }

    public double GetValue(VaryingExtension ex)
    {
        return Value;
    }
}