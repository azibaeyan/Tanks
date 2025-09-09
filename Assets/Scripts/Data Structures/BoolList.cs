
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Runtime.CompilerServices;

public struct BoolList 
{
    private ushort _data;
    private ushort _count;

    public readonly uint Count => _count;

    
    public Action<ulong> OnDataChanged;


    public BoolList(ushort count)
    {
        _data = 0;
        _count = 0;
        OnDataChanged = null;
    }


    public BoolList(ushort count, ushort data)
    {
        _data = data;
        _count = count;
        OnDataChanged = null;
    }


    public bool this[int index]
    {
        readonly get 
        {
            IndexAssert(index);
            return (_data >> index & 0b1) == 1;
        }
        set
        {
            IndexAssert(index);
            value = (_data >> index & 0b1) == 1;
            OnDataChanged?.Invoke((ulong)this);
        }
    }


    private readonly void IndexAssert(int index)
    {
        if (index >= _count) throw new IndexOutOfRangeException();
    }


    public void Add(bool value) 
    {
        _data = (ushort)((_data & ((0b1 << _count) - 0b1)) | ((value ? 0b0 : 0b1) << _count));
        _count++;
        OnDataChanged?.Invoke((ulong)this);
    }


    public void Pop() 
    {
        _count--;
        OnDataChanged?.Invoke((ulong)this);
    }


    public readonly int TrueValuesCount
    {
        get 
        {
            int trueValuesCount = 0;
            for (int i = 0; i < _count; i++) if (((_data >> i) & 0b1) == 1) trueValuesCount++;
            return trueValuesCount;
        }
    }


    public static explicit operator ushort(BoolList boolList) => boolList._data;
}
