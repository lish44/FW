using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReadonlyArray<T> : IEnumerable
{
    private readonly T[] _array;

    public T this[int index]
    {
        get
        {
            return _array[index];
        }
    }

    public int Length { get { return _array.Length; } }

    public static ReadonlyArray<T> Empty { get { return new ReadonlyArray<T>(null); } }

    public ReadonlyArray(T[] source)
    {
        _array = source ?? new T[0];
    }

    public T[] ToArray()
    {
        T[] newArray = new T[_array.Length];
        for (int i = 0; i < newArray.Length; i++)
        {
            newArray[i] = _array[i];
        }
        return newArray;
    }

    public IEnumerator GetEnumerator()
    {
        return _array.GetEnumerator();
    }
}
