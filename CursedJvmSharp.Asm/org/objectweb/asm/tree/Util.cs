using org.objectweb.asm;
using CursedJvmSharp.Asm.Java.IO;
using System.Collections.Generic;

// ASM: a very small and fast Java bytecode manipulation framework
// Copyright (c) 2000-2011 INRIA, France Telecom
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
// 1. Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
// 3. Neither the name of the copyright holders nor the names of its
//    contributors may be used to endorse or promote products derived from
//    this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
namespace org.objectweb.asm.tree
{

	/// <summary>
	/// Utility methods to convert an array of primitive or object values to a mutable ArrayList, not
	/// baked by the array (unlike <seealso cref="java.util.Arrays.asList"/>).
	/// 
	/// @author Eric Bruneton
	/// </summary>
	internal sealed class Util
	{

	  private Util()
	  {
	  }

	  internal static List<T> add<T>(List<T> list, T element)
	  {
		List<T> newList = list == null ? new List<T>(1) : list;
		newList.Add(element);
		return newList;
	  }

	  internal static List<T> asArrayList<T>(int length)
	  {
		List<T> list = new List<T>(length);
		for (int i = 0; i < length; ++i)
		{
		  list.Add(default(T));
		}
		return list;
	  }

	  internal static List<T> asArrayList<T>(T[] array)
	  {
		if (array == null)
		{
		  return new List<T>();
		}
		List<T> list = new List<T>(array.Length);
		foreach (T t in array)
		{
		  list.Add(t);
		}
		return list;
	  }

	  internal static List<sbyte> asArrayList(sbyte[] byteArray)
	  {
		if (byteArray == null)
		{
		  return new List<sbyte>();
		}
		List<sbyte> byteList = new List<sbyte>(byteArray.Length);
		foreach (sbyte b in byteArray)
		{
		  byteList.Add(b);
		}
		return byteList;
	  }

	  internal static List<bool> asArrayList(bool[] booleanArray)
	  {
		if (booleanArray == null)
		{
		  return new List<bool>();
		}
		List<bool> booleanList = new List<bool>(booleanArray.Length);
		foreach (bool b in booleanArray)
		{
		  booleanList.Add(b);
		}
		return booleanList;
	  }

	  internal static List<short> asArrayList(short[] shortArray)
	  {
		if (shortArray == null)
		{
		  return new List<short>();
		}
		List<short> shortList = new List<short>(shortArray.Length);
		foreach (short s in shortArray)
		{
		  shortList.Add(s);
		}
		return shortList;
	  }

	  internal static List<char> asArrayList(char[] charArray)
	  {
		if (charArray == null)
		{
		  return new List<char>();
		}
		List<char> charList = new List<char>(charArray.Length);
		foreach (char c in charArray)
		{
		  charList.Add(c);
		}
		return charList;
	  }

	  internal static List<int> asArrayList(int[] intArray)
	  {
		if (intArray == null)
		{
		  return new List<int>();
		}
		List<int> intList = new List<int>(intArray.Length);
		foreach (int i in intArray)
		{
		  intList.Add(i);
		}
		return intList;
	  }

	  internal static List<float> asArrayList(float[] floatArray)
	  {
		if (floatArray == null)
		{
		  return new List<float>();
		}
		List<float> floatList = new List<float>(floatArray.Length);
		foreach (float f in floatArray)
		{
		  floatList.Add(f);
		}
		return floatList;
	  }

	  internal static List<long> asArrayList(long[] longArray)
	  {
		if (longArray == null)
		{
		  return new List<long>();
		}
		List<long> longList = new List<long>(longArray.Length);
		foreach (long l in longArray)
		{
		  longList.Add(l);
		}
		return longList;
	  }

	  internal static List<double> asArrayList(double[] doubleArray)
	  {
		if (doubleArray == null)
		{
		  return new List<double>();
		}
		List<double> doubleList = new List<double>(doubleArray.Length);
		foreach (double d in doubleArray)
		{
		  doubleList.Add(d);
		}
		return doubleList;
	  }

	  internal static List<T> asArrayList<T>(int length, T[] array)
	  {
		List<T> list = new List<T>(length);
		for (int i = 0; i < length; ++i)
		{
		  list.Add(array[i]); // NOPMD(UseArraysAsList): we convert a part of the array.
		}
		return list;
	  }
	}

}