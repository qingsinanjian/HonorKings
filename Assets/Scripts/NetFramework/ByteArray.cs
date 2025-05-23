using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ByteArray
{
    const int DEFAULT_SIZE = 1024;
    public byte[] bytes;

    public int readIndex;
    public int writeIndex;
    public int initSize;
    /// <summary>
    /// ��������
    /// </summary>
    public int capacity;
    /// <summary>
    /// ��д֮��ĳ���
    /// </summary>
    public int Length
    {
        get { return writeIndex - readIndex; }
    }
    /// <summary>
    /// ����
    /// </summary>
    public int Remain
    {
        get { return capacity - writeIndex; }
    }

    public ByteArray(int size = DEFAULT_SIZE)
    {
        bytes = new byte[size];
        capacity = size;
        initSize = size;
        readIndex = 0;
        writeIndex = 0;
    }

    public ByteArray(byte[] defaultBytes)
    {
        bytes = defaultBytes;
        capacity = defaultBytes.Length;
        initSize = defaultBytes.Length;
        readIndex = 0;
        writeIndex = defaultBytes.Length;
    }

    public void MoveBytes()
    {
        if(Length > 0)
        {
            Array.Copy(bytes, readIndex, bytes, 0, Length);
        }
        writeIndex = Length;
        readIndex = 0;
    }

    /// <summary>
    /// ����
    /// </summary>
    /// <param name="size"></param>
    /// <exception cref="Exception"></exception>
    public void ReSize(int size)
    {
        if (size < Length || size < initSize)
        {
            return;
        }
        byte[] newBytes = new byte[size];
        Array.Copy(bytes, readIndex, newBytes, 0, Length);
        bytes = newBytes;
        capacity = size;
        writeIndex = Length;
        readIndex = 0;
    }
}
