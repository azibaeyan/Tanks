
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Runtime.CompilerServices;

public struct BoolList 
{
    private readonly List<bool> _data;

    public readonly int Count => _data.Count;

    
    public Action<ulong> OnDataChanged;


    public BoolList(int count)
    {
        _data = new List<bool>(count);
        OnDataChanged = null;
        DataInit();
    }


    public BoolList(int count, ulong data)
    {
        _data = new List<bool>(count);
        OnDataChanged = null;
        
        for (int i = 0; i < count; i++)
        {
            _data.Add(data % 10 == 1);
            data /= 10;
        }
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void DataInit()
    {
        for (int i = 0; i < _data.Capacity; i++) _data.Add(false);
    }


    public bool this[int index]
    {
        readonly get 
        {
            IndexAssert(index);
            return _data[index];
        }
        set
        {
            IndexAssert(index);
            _data[index] = value;
            OnDataChanged?.Invoke((ulong)this);
        }
    }


    private readonly void IndexAssert(int index)
    {
        if (index >= _data.Count) throw new IndexOutOfRangeException();
    }


    public void Add(bool value) 
    {
        _data.Add(value);
        OnDataChanged?.Invoke((ulong)this);
    }


    public void Pop() 
    {
        _data.RemoveAt(_data.Count - 1);
        OnDataChanged?.Invoke((ulong)this);
    }


    public readonly int TrueValuesCount
    {
        get 
        {
            int trueValuesCount = 0;
            for (int i = 0; i < _data.Count; i++) 
                if (_data[i]) trueValuesCount++;
            return trueValuesCount;
        }
    }


    public static explicit operator ulong(BoolList boolList)
    {
        ulong data = 0;

        for (int i = 0; i < boolList._data.Count; i++)
            if (boolList._data[i]) data += (ulong)Math.Pow(10, i);

        return data;
    }

    public static implicit operator BoolList(ulong data) 
    {
        int count = 0;
        while (data > Math.Pow(10, count)) count++;
        count++;

        return new BoolList(count, data);
    }
}
