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
namespace ObjectWeb.Asm.Tree
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

	  internal static List<T> Add<T>(List<T> list, T element)
	  {
		var newList = list == null ? new List<T>(1) : list;
		newList.Add(element);
		return newList;
	  }

	  internal static List<T> AsArrayList<T>(int length)
	  {
		var list = new List<T>(length);
		for (var i = 0; i < length; ++i)
		{
		  list.Add(default(T));
		}
		return list;
	  }

	  internal static List<T> AsArrayList<T>(T[] array)
	  {
		if (array == null)
		{
		  return new List<T>();
		}
		var list = new List<T>(array.Length);
		foreach (var t in array)
		{
		  list.Add(t);
		}
		return list;
	  }

	  internal static List<sbyte> AsArrayList(byte[] byteArray)
	  {
		if (byteArray == null)
		{
		  return new List<sbyte>();
		}
		var byteList = new List<sbyte>(byteArray.Length);
		foreach (sbyte b in byteArray)
		{
		  byteList.Add(b);
		}
		return byteList;
	  }

	  internal static List<bool> AsArrayList(bool[] booleanArray)
	  {
		if (booleanArray == null)
		{
		  return new List<bool>();
		}
		var booleanList = new List<bool>(booleanArray.Length);
		foreach (var b in booleanArray)
		{
		  booleanList.Add(b);
		}
		return booleanList;
	  }

	  internal static List<short> AsArrayList(short[] shortArray)
	  {
		if (shortArray == null)
		{
		  return new List<short>();
		}
		var shortList = new List<short>(shortArray.Length);
		foreach (var s in shortArray)
		{
		  shortList.Add(s);
		}
		return shortList;
	  }

	  internal static List<char> AsArrayList(char[] charArray)
	  {
		if (charArray == null)
		{
		  return new List<char>();
		}
		var charList = new List<char>(charArray.Length);
		foreach (var c in charArray)
		{
		  charList.Add(c);
		}
		return charList;
	  }

	  internal static List<int> AsArrayList(int[] intArray)
	  {
		if (intArray == null)
		{
		  return new List<int>();
		}
		var intList = new List<int>(intArray.Length);
		foreach (var i in intArray)
		{
		  intList.Add(i);
		}
		return intList;
	  }

	  internal static List<float> AsArrayList(float[] floatArray)
	  {
		if (floatArray == null)
		{
		  return new List<float>();
		}
		var floatList = new List<float>(floatArray.Length);
		foreach (var f in floatArray)
		{
		  floatList.Add(f);
		}
		return floatList;
	  }

	  internal static List<long> AsArrayList(long[] longArray)
	  {
		if (longArray == null)
		{
		  return new List<long>();
		}
		var longList = new List<long>(longArray.Length);
		foreach (var l in longArray)
		{
		  longList.Add(l);
		}
		return longList;
	  }

	  internal static List<double> AsArrayList(double[] doubleArray)
	  {
		if (doubleArray == null)
		{
		  return new List<double>();
		}
		var doubleList = new List<double>(doubleArray.Length);
		foreach (var d in doubleArray)
		{
		  doubleList.Add(d);
		}
		return doubleList;
	  }

	  internal static List<T> AsArrayList<T>(int length, T[] array)
	  {
		var list = new List<T>(length);
		for (var i = 0; i < length; ++i)
		{
		  list.Add(array[i]); // NOPMD(UseArraysAsList): we convert a part of the array.
		}
		return list;
	  }
	}

}