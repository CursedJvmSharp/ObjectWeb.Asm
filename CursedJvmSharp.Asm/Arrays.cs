﻿//---------------------------------------------------------------------------------------------------------
//	Copyright © 2007 - 2021 Tangible Software Solutions, Inc.
//	This class can be used by anyone provided that the copyright notice remains intact.
//
//	This class is used to replace some calls to java.util.Arrays methods with the C# equivalent.
//---------------------------------------------------------------------------------------------------------

using System;

internal static class Arrays
{
    public static T[] CopyOf<T>(T[] original, int newLength)
    {
        var dest = new T[newLength];
        Array.Copy(original, dest, newLength);
        return dest;
    }

    public static T[] CopyOfRange<T>(T[] original, int fromIndex, int toIndex)
    {
        var length = toIndex - fromIndex;
        var dest = new T[length];
        Array.Copy(original, fromIndex, dest, 0, length);
        return dest;
    }

    public static void Fill<T>(T[] array, T value)
    {
        for (var i = 0; i < array.Length; i++) array[i] = value;
    }

    public static void Fill<T>(T[] array, int fromIndex, int toIndex, T value)
    {
        for (var i = fromIndex; i < toIndex; i++) array[i] = value;
    }
}