using System;

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
namespace ObjectWeb.Asm
{
	/// <summary>
	/// A dynamically extensible vector of bytes. This class is roughly equivalent to a DataOutputStream
	/// on top of a ByteArrayOutputStream, but is more efficient.
	/// 
	/// @author Eric Bruneton
	/// </summary>
	public class ByteVector
	{

	  /// <summary>
	  /// The content of this vector. Only the first <seealso cref="length"/> bytes contain real data. </summary>
	  internal byte[] data;

	  /// <summary>
	  /// The actual number of bytes in this vector. </summary>
	  internal int length;

	  /// <summary>
	  /// Constructs a new <seealso cref="ByteVector"/> with a default initial capacity. </summary>
	  public ByteVector()
	  {
		data = new byte[64];
	  }

	  /// <summary>
	  /// Constructs a new <seealso cref="ByteVector"/> with the given initial capacity.
	  /// </summary>
	  /// <param name="initialCapacity"> the initial capacity of the byte vector to be constructed. </param>
	  public ByteVector(int initialCapacity)
	  {
		data = new byte[initialCapacity];
	  }

	  /// <summary>
	  /// Constructs a new <seealso cref="ByteVector"/> from the given initial data.
	  /// </summary>
	  /// <param name="data"> the initial data of the new byte vector. </param>
	  public ByteVector(byte[] data)
	  {
		this.data = data;
		this.length = data.Length;
	  }

	  /// <summary>
	  /// Puts a byte into this byte vector. The byte vector is automatically enlarged if necessary.
	  /// </summary>
	  /// <param name="byteValue"> a byte. </param>
	  /// <returns> this byte vector. </returns>
	  public virtual ByteVector PutByte(int byteValue)
	  {
		int currentLength = length;
		if (currentLength + 1 > data.Length)
		{
		  Enlarge(1);
		}
		data[currentLength++] = (byte) byteValue;
		length = currentLength;
		return this;
	  }

	  /// <summary>
	  /// Puts two bytes into this byte vector. The byte vector is automatically enlarged if necessary.
	  /// </summary>
	  /// <param name="byteValue1"> a byte. </param>
	  /// <param name="byteValue2"> another byte. </param>
	  /// <returns> this byte vector. </returns>
	  public ByteVector Put11(int byteValue1, int byteValue2)
	  {
		int currentLength = length;
		if (currentLength + 2 > data.Length)
		{
		  Enlarge(2);
		}
		byte[] currentData = data;
		currentData[currentLength++] = (byte) byteValue1;
		currentData[currentLength++] = (byte) byteValue2;
		length = currentLength;
		return this;
	  }

	  /// <summary>
	  /// Puts a short into this byte vector. The byte vector is automatically enlarged if necessary.
	  /// </summary>
	  /// <param name="shortValue"> a short. </param>
	  /// <returns> this byte vector. </returns>
	  public virtual ByteVector PutShort(int shortValue)
	  {
		int currentLength = length;
		if (currentLength + 2 > data.Length)
		{
		  Enlarge(2);
		}
		byte[] currentData = data;
		currentData[currentLength++] = (byte)((int)((uint)shortValue >> 8));
		currentData[currentLength++] = (byte) shortValue;
		length = currentLength;
		return this;
	  }

	  /// <summary>
	  /// Puts a byte and a short into this byte vector. The byte vector is automatically enlarged if
	  /// necessary.
	  /// </summary>
	  /// <param name="byteValue"> a byte. </param>
	  /// <param name="shortValue"> a short. </param>
	  /// <returns> this byte vector. </returns>
	  public ByteVector Put12(int byteValue, int shortValue)
	  {
		int currentLength = length;
		if (currentLength + 3 > data.Length)
		{
		  Enlarge(3);
		}
		byte[] currentData = data;
		currentData[currentLength++] = (byte) byteValue;
		currentData[currentLength++] = (byte)((int)((uint)shortValue >> 8));
		currentData[currentLength++] = (byte) shortValue;
		length = currentLength;
		return this;
	  }

	  /// <summary>
	  /// Puts two bytes and a short into this byte vector. The byte vector is automatically enlarged if
	  /// necessary.
	  /// </summary>
	  /// <param name="byteValue1"> a byte. </param>
	  /// <param name="byteValue2"> another byte. </param>
	  /// <param name="shortValue"> a short. </param>
	  /// <returns> this byte vector. </returns>
	  public ByteVector Put112(int byteValue1, int byteValue2, int shortValue)
	  {
		int currentLength = length;
		if (currentLength + 4 > data.Length)
		{
		  Enlarge(4);
		}
		byte[] currentData = data;
		currentData[currentLength++] = (byte) byteValue1;
		currentData[currentLength++] = (byte) byteValue2;
		currentData[currentLength++] = (byte)((int)((uint)shortValue >> 8));
		currentData[currentLength++] = (byte) shortValue;
		length = currentLength;
		return this;
	  }

	  /// <summary>
	  /// Puts an int into this byte vector. The byte vector is automatically enlarged if necessary.
	  /// </summary>
	  /// <param name="intValue"> an int. </param>
	  /// <returns> this byte vector. </returns>
	  public virtual ByteVector PutInt(int intValue)
	  {
		int currentLength = length;
		if (currentLength + 4 > data.Length)
		{
		  Enlarge(4);
		}
		byte[] currentData = data;
		currentData[currentLength++] = (byte)((int)((uint)intValue >> 24));
		currentData[currentLength++] = (byte)((int)((uint)intValue >> 16));
		currentData[currentLength++] = (byte)((int)((uint)intValue >> 8));
		currentData[currentLength++] = (byte) intValue;
		length = currentLength;
		return this;
	  }

	  /// <summary>
	  /// Puts one byte and two shorts into this byte vector. The byte vector is automatically enlarged
	  /// if necessary.
	  /// </summary>
	  /// <param name="byteValue"> a byte. </param>
	  /// <param name="shortValue1"> a short. </param>
	  /// <param name="shortValue2"> another short. </param>
	  /// <returns> this byte vector. </returns>
	  public ByteVector Put122(int byteValue, int shortValue1, int shortValue2)
	  {
		int currentLength = length;
		if (currentLength + 5 > data.Length)
		{
		  Enlarge(5);
		}
		byte[] currentData = data;
		currentData[currentLength++] = (byte) byteValue;
		currentData[currentLength++] = (byte)((int)((uint)shortValue1 >> 8));
		currentData[currentLength++] = (byte) shortValue1;
		currentData[currentLength++] = (byte)((int)((uint)shortValue2 >> 8));
		currentData[currentLength++] = (byte) shortValue2;
		length = currentLength;
		return this;
	  }

	  /// <summary>
	  /// Puts a long into this byte vector. The byte vector is automatically enlarged if necessary.
	  /// </summary>
	  /// <param name="longValue"> a long. </param>
	  /// <returns> this byte vector. </returns>
	  public virtual ByteVector PutLong(long longValue)
	  {
		int currentLength = length;
		if (currentLength + 8 > data.Length)
		{
		  Enlarge(8);
		}
		byte[] currentData = data;
		int intValue = (int)((long)((ulong)longValue >> 32));
		currentData[currentLength++] = (byte)((int)((uint)intValue >> 24));
		currentData[currentLength++] = (byte)((int)((uint)intValue >> 16));
		currentData[currentLength++] = (byte)((int)((uint)intValue >> 8));
		currentData[currentLength++] = (byte) intValue;
		intValue = (int) longValue;
		currentData[currentLength++] = (byte)((int)((uint)intValue >> 24));
		currentData[currentLength++] = (byte)((int)((uint)intValue >> 16));
		currentData[currentLength++] = (byte)((int)((uint)intValue >> 8));
		currentData[currentLength++] = (byte) intValue;
		length = currentLength;
		return this;
	  }

	  /// <summary>
	  /// Puts an UTF8 string into this byte vector. The byte vector is automatically enlarged if
	  /// necessary.
	  /// </summary>
	  /// <param name="stringValue"> a String whose UTF8 encoded length must be less than 65536. </param>
	  /// <returns> this byte vector. </returns>
	  // DontCheck(AbbreviationAsWordInName): can't be renamed (for backward binary compatibility).
	  public virtual ByteVector PutUtf8(string stringValue)
	  {
		int charLength = stringValue.Length;
		if (charLength > 65535)
		{
		  throw new System.ArgumentException("UTF8 string too large");
		}
		int currentLength = length;
		if (currentLength + 2 + charLength > data.Length)
		{
		  Enlarge(2 + charLength);
		}
		byte[] currentData = data;
		// Optimistic algorithm: instead of computing the byte length and then serializing the string
		// (which requires two loops), we assume the byte length is equal to char length (which is the
		// most frequent case), and we start serializing the string right away. During the
		// serialization, if we find that this assumption is wrong, we continue with the general method.
		currentData[currentLength++] = (byte)((int)((uint)charLength >> 8));
		currentData[currentLength++] = (byte) charLength;
		for (int i = 0; i < charLength; ++i)
		{
		  char charValue = stringValue[i];
		  if (charValue >= '\u0001' && charValue <= '\u007F')
		  {
			currentData[currentLength++] = (byte) charValue;
		  }
		  else
		  {
			length = currentLength;
			return EncodeUtf8(stringValue, i, 65535);
		  }
		}
		length = currentLength;
		return this;
	  }

	  /// <summary>
	  /// Puts an UTF8 string into this byte vector. The byte vector is automatically enlarged if
	  /// necessary. The string length is encoded in two bytes before the encoded characters, if there is
	  /// space for that (i.e. if this.length - offset - 2 &gt;= 0).
	  /// </summary>
	  /// <param name="stringValue"> the String to encode. </param>
	  /// <param name="offset"> the index of the first character to encode. The previous characters are supposed
	  ///     to have already been encoded, using only one byte per character. </param>
	  /// <param name="maxByteLength"> the maximum byte length of the encoded string, including the already
	  ///     encoded characters. </param>
	  /// <returns> this byte vector. </returns>
	  public ByteVector EncodeUtf8(string stringValue, int offset, int maxByteLength)
	  {
		int charLength = stringValue.Length;
		int byteLength = offset;
		for (int i = offset; i < charLength; ++i)
		{
		  char charValue = stringValue[i];
		  if (charValue >= (char)0x0001 && charValue <= (char)0x007F)
		  {
			byteLength++;
		  }
		  else if (charValue <= (char)0x07FF)
		  {
			byteLength += 2;
		  }
		  else
		  {
			byteLength += 3;
		  }
		}
		if (byteLength > maxByteLength)
		{
		  throw new System.ArgumentException("UTF8 string too large");
		}
		// Compute where 'byteLength' must be stored in 'data', and store it at this location.
		int byteLengthOffset = length - offset - 2;
		if (byteLengthOffset >= 0)
		{
		  data[byteLengthOffset] = (byte)((int)((uint)byteLength >> 8));
		  data[byteLengthOffset + 1] = (byte) byteLength;
		}
		if (length + byteLength - offset > data.Length)
		{
		  Enlarge(byteLength - offset);
		}
		int currentLength = length;
		for (int i = offset; i < charLength; ++i)
		{
		  char charValue = stringValue[i];
		  if (charValue >= (char)0x0001 && charValue <= (char)0x007F)
		  {
			data[currentLength++] = (byte) charValue;
		  }
		  else if (charValue <= (char)0x07FF)
		  {
			data[currentLength++] = unchecked((byte)(0xC0 | charValue >> 6 & 0x1F));
			data[currentLength++] = unchecked((byte)(0x80 | charValue & 0x3F));
		  }
		  else
		  {
			data[currentLength++] = unchecked((byte)(0xE0 | charValue >> 12 & 0xF));
			data[currentLength++] = unchecked((byte)(0x80 | charValue >> 6 & 0x3F));
			data[currentLength++] = unchecked((byte)(0x80 | charValue & 0x3F));
		  }
		}
		length = currentLength;
		return this;
	  }

	  /// <summary>
	  /// Puts an array of bytes into this byte vector. The byte vector is automatically enlarged if
	  /// necessary.
	  /// </summary>
	  /// <param name="byteArrayValue"> an array of bytes. May be {@literal null} to put {@code byteLength} null
	  ///     bytes into this byte vector. </param>
	  /// <param name="byteOffset"> index of the first byte of byteArrayValue that must be copied. </param>
	  /// <param name="byteLength"> number of bytes of byteArrayValue that must be copied. </param>
	  /// <returns> this byte vector. </returns>
	  public virtual ByteVector PutByteArray(byte[] byteArrayValue, int byteOffset, int byteLength)
	  {
		if (length + byteLength > data.Length)
		{
		  Enlarge(byteLength);
		}
		if (byteArrayValue != null)
		{
		  Array.Copy(byteArrayValue, byteOffset, data, length, byteLength);
		}
		length += byteLength;
		return this;
	  }

	  /// <summary>
	  /// Enlarges this byte vector so that it can receive 'size' more bytes.
	  /// </summary>
	  /// <param name="size"> number of additional bytes that this byte vector should be able to receive. </param>
	  private void Enlarge(int size)
	  {
		int doubleCapacity = 2 * data.Length;
		int minimalCapacity = length + size;
		byte[] newData = new byte[doubleCapacity > minimalCapacity ? doubleCapacity : minimalCapacity];
		Array.Copy(data, 0, newData, 0, length);
		data = newData;
	  }
	}

}