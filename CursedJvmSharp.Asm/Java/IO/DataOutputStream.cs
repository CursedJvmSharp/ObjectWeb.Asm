/*
* Copyright (c) 1994, 2004, Oracle and/or its affiliates. All rights reserved.
* ORACLE PROPRIETARY/CONFIDENTIAL. Use is subject to license terms.
*
*
*
*
*
*
*
*
*
*
*
*
*
*
*
*
*
*
*
*
*/

using System;
using System.IO;

namespace CursedJvmSharp.Asm.Java.IO
{
    /// <summary>
    ///     A data output stream lets an application write primitive Java data
    ///     types to an output stream in a portable way.
    /// </summary>
    /// <remarks>
    ///     A data output stream lets an application write primitive Java data
    ///     types to an output stream in a portable way. An application can
    ///     then use a data input stream to read the data back in.
    /// </remarks>
    /// <author>unascribed</author>
    /// <seealso cref="DataInputStream" />
    /// <since>JDK1.0</since>
    public class DataOutputStream : DataOutput
    {
        private readonly MemoryStream _out;
        private readonly byte[] writeBuffer = new byte[8];

        /// <summary>bytearr is initialized on demand by writeUTF</summary>
        private byte[] bytearr;

        /// <summary>The number of bytes written to the data output stream so far.</summary>
        /// <remarks>
        ///     The number of bytes written to the data output stream so far.
        ///     If this counter overflows, it will be wrapped to Integer.MAX_VALUE.
        /// </remarks>
        protected internal int written;

        /// <summary>
        ///     Creates a new data output stream to write data to the specified
        ///     underlying output stream.
        /// </summary>
        /// <remarks>
        ///     Creates a new data output stream to write data to the specified
        ///     underlying output stream. The counter <code>written</code> is
        ///     set to zero.
        /// </remarks>
        /// <param name="out">
        ///     the underlying output stream, to be saved for later
        ///     use.
        /// </param>
        /// <seealso cref="FilterOutputStream.@out" />
        public DataOutputStream(MemoryStream @out)
        {
            _out = @out;
        }

        /// <summary>
        ///     Writes the specified byte (the low eight bits of the argument
        ///     <code>b</code>) to the underlying output stream.
        /// </summary>
        /// <remarks>
        ///     Writes the specified byte (the low eight bits of the argument
        ///     <code>b</code>) to the underlying output stream. If no exception
        ///     is thrown, the counter <code>written</code> is incremented by
        ///     <code>1</code>.
        ///     <p>
        ///         Implements the <code>write</code> method of <code>OutputStream</code>.
        /// </remarks>
        /// <param name="b">the <code>byte</code> to be written.</param>
        /// <exception>
        ///     IOException
        ///     if an I/O error occurs.
        /// </exception>
        /// <seealso cref="FilterOutputStream.@out" />
        /// <exception cref="IOException" />
        public void Write(byte b)
        {
            lock (this)
            {
                _out.WriteByte(b);
                IncCount(1);
            }
        }

        public void Write(byte[] b)
        {
            _out.Write(b);
            IncCount(b.Length);
        }

        /// <summary>
        ///     Writes <code>len</code> bytes from the specified byte array
        ///     starting at offset <code>off</code> to the underlying output stream.
        /// </summary>
        /// <remarks>
        ///     Writes <code>len</code> bytes from the specified byte array
        ///     starting at offset <code>off</code> to the underlying output stream.
        ///     If no exception is thrown, the counter <code>written</code> is
        ///     incremented by <code>len</code>.
        /// </remarks>
        /// <param name="b">the data.</param>
        /// <param name="off">the start offset in the data.</param>
        /// <param name="len">the number of bytes to write.</param>
        /// <exception>
        ///     IOException
        ///     if an I/O error occurs.
        /// </exception>
        /// <seealso cref="FilterOutputStream.@out" />
        /// <exception cref="IOException" />
        public void Write(byte[] b, int off, int len)
        {
            lock (this)
            {
                _out.Write(b, off, len);
                IncCount(len);
            }
        }

        /// <summary>
        ///     Writes a <code>boolean</code> to the underlying output stream as
        ///     a 1-byte value.
        /// </summary>
        /// <remarks>
        ///     Writes a <code>boolean</code> to the underlying output stream as
        ///     a 1-byte value. The value <code>true</code> is written out as the
        ///     value <code>(byte)1</code>; the value <code>false</code> is
        ///     written out as the value <code>(byte)0</code>. If no exception is
        ///     thrown, the counter <code>written</code> is incremented by
        ///     <code>1</code>.
        /// </remarks>
        /// <param name="v">a <code>boolean</code> value to be written.</param>
        /// <exception>
        ///     IOException
        ///     if an I/O error occurs.
        /// </exception>
        /// <seealso cref="FilterOutputStream.@out" />
        /// <exception cref="IOException" />
        public void WriteBoolean(bool v)
        {
            Write((byte)(v ? 1 : 0));
            IncCount(1);
        }

        /// <summary>
        ///     Writes out a <code>byte</code> to the underlying output stream as
        ///     a 1-byte value.
        /// </summary>
        /// <remarks>
        ///     Writes out a <code>byte</code> to the underlying output stream as
        ///     a 1-byte value. If no exception is thrown, the counter
        ///     <code>written</code> is incremented by <code>1</code>.
        /// </remarks>
        /// <param name="v">a <code>byte</code> value to be written.</param>
        /// <exception>
        ///     IOException
        ///     if an I/O error occurs.
        /// </exception>
        /// <seealso cref="FilterOutputStream.@out" />
        /// <exception cref="IOException" />
        public void WriteByte(int v)
        {
            Write((byte)v);
            IncCount(1);
        }

        /// <summary>
        ///     Writes a <code>short</code> to the underlying output stream as two
        ///     bytes, high byte first.
        /// </summary>
        /// <remarks>
        ///     Writes a <code>short</code> to the underlying output stream as two
        ///     bytes, high byte first. If no exception is thrown, the counter
        ///     <code>written</code> is incremented by <code>2</code>.
        /// </remarks>
        /// <param name="v">a <code>short</code> to be written.</param>
        /// <exception>
        ///     IOException
        ///     if an I/O error occurs.
        /// </exception>
        /// <seealso cref="FilterOutputStream.@out" />
        /// <exception cref="IOException" />
        public void WriteShort(int v)
        {
            Write((byte)((byte) ((uint) v >> 8) & 0xFF));
            Write((byte)((byte) ((uint) v >> 0) & 0xFF));
            IncCount(2);
        }

        /// <summary>
        ///     Writes a <code>char</code> to the underlying output stream as a
        ///     2-byte value, high byte first.
        /// </summary>
        /// <remarks>
        ///     Writes a <code>char</code> to the underlying output stream as a
        ///     2-byte value, high byte first. If no exception is thrown, the
        ///     counter <code>written</code> is incremented by <code>2</code>.
        /// </remarks>
        /// <param name="v">a <code>char</code> value to be written.</param>
        /// <exception>
        ///     IOException
        ///     if an I/O error occurs.
        /// </exception>
        /// <seealso cref="FilterOutputStream.@out" />
        /// <exception cref="IOException" />
        public void WriteChar(int v)
        {
            Write((byte)((int) ((uint) v >> 8) & 0xFF));
            Write((byte)((int) ((uint) v >> 0) & 0xFF));
            IncCount(2);
        }

        /// <summary>
        ///     Writes an <code>int</code> to the underlying output stream as four
        ///     bytes, high byte first.
        /// </summary>
        /// <remarks>
        ///     Writes an <code>int</code> to the underlying output stream as four
        ///     bytes, high byte first. If no exception is thrown, the counter
        ///     <code>written</code> is incremented by <code>4</code>.
        /// </remarks>
        /// <param name="v">an <code>int</code> to be written.</param>
        /// <exception>
        ///     IOException
        ///     if an I/O error occurs.
        /// </exception>
        /// <seealso cref="FilterOutputStream.@out" />
        /// <exception cref="IOException" />
        public void WriteInt(int v)
        {
            Write((byte)((int) ((uint) v >> 24) & 0xFF));
            Write((byte)((int) ((uint) v >> 16) & 0xFF));
            Write((byte)((int) ((uint) v >> 8) & 0xFF));
            Write((byte)((int) ((uint) v >> 0) & 0xFF));
            IncCount(4);
        }

        /// <summary>
        ///     Writes a <code>long</code> to the underlying output stream as eight
        ///     bytes, high byte first.
        /// </summary>
        /// <remarks>
        ///     Writes a <code>long</code> to the underlying output stream as eight
        ///     bytes, high byte first. In no exception is thrown, the counter
        ///     <code>written</code> is incremented by <code>8</code>.
        /// </remarks>
        /// <param name="v">a <code>long</code> to be written.</param>
        /// <exception>
        ///     IOException
        ///     if an I/O error occurs.
        /// </exception>
        /// <seealso cref="FilterOutputStream.@out" />
        /// <exception cref="IOException" />
        public void WriteLong(long v)
        {
            writeBuffer[0] = unchecked((byte) (long) ((ulong) v >> 56));
            writeBuffer[1] = unchecked((byte) (long) ((ulong) v >> 48));
            writeBuffer[2] = unchecked((byte) (long) ((ulong) v >> 40));
            writeBuffer[3] = unchecked((byte) (long) ((ulong) v >> 32));
            writeBuffer[4] = unchecked((byte) (long) ((ulong) v >> 24));
            writeBuffer[5] = unchecked((byte) (long) ((ulong) v >> 16));
            writeBuffer[6] = unchecked((byte) (long) ((ulong) v >> 8));
            writeBuffer[7] = unchecked((byte) (long) ((ulong) v >> 0));
            _out.Write(writeBuffer, 0, 8);
            IncCount(8);
        }

        /// <summary>
        ///     Converts the float argument to an <code>int</code> using the
        ///     <code>floatToIntBits</code> method in class <code>Float</code>,
        ///     and then writes that <code>int</code> value to the underlying
        ///     output stream as a 4-byte quantity, high byte first.
        /// </summary>
        /// <remarks>
        ///     Converts the float argument to an <code>int</code> using the
        ///     <code>floatToIntBits</code> method in class <code>Float</code>,
        ///     and then writes that <code>int</code> value to the underlying
        ///     output stream as a 4-byte quantity, high byte first. If no
        ///     exception is thrown, the counter <code>written</code> is
        ///     incremented by <code>4</code>.
        /// </remarks>
        /// <param name="v">a <code>float</code> value to be written.</param>
        /// <exception>
        ///     IOException
        ///     if an I/O error occurs.
        /// </exception>
        /// <seealso cref="FilterOutputStream.@out" />
        /// <seealso cref="Sharpen.Runtime.FloatToIntBits(float)" />
        /// <exception cref="System.IO.IOException" />
        public void WriteFloat(float v)
        {
            WriteInt(BitConverter.SingleToInt32Bits(v));
        }

        /// <summary>
        ///     Converts the double argument to a <code>long</code> using the
        ///     <code>doubleToLongBits</code> method in class <code>Double</code>,
        ///     and then writes that <code>long</code> value to the underlying
        ///     output stream as an 8-byte quantity, high byte first.
        /// </summary>
        /// <remarks>
        ///     Converts the double argument to a <code>long</code> using the
        ///     <code>doubleToLongBits</code> method in class <code>Double</code>,
        ///     and then writes that <code>long</code> value to the underlying
        ///     output stream as an 8-byte quantity, high byte first. If no
        ///     exception is thrown, the counter <code>written</code> is
        ///     incremented by <code>8</code>.
        /// </remarks>
        /// <param name="v">a <code>double</code> value to be written.</param>
        /// <exception>
        ///     IOException
        ///     if an I/O error occurs.
        /// </exception>
        /// <seealso cref="FilterOutputStream.@out" />
        /// <seealso cref="BitConverter.DoubleToInt64Bits" />
        /// <exception cref="IOException" />
        public void WriteDouble(double v)
        {
            WriteLong(BitConverter.DoubleToInt64Bits(v));
        }

        /// <summary>
        ///     Writes out the string to the underlying output stream as a
        ///     sequence of bytes.
        /// </summary>
        /// <remarks>
        ///     Writes out the string to the underlying output stream as a
        ///     sequence of bytes. Each character in the string is written out, in
        ///     sequence, by discarding its high eight bits. If no exception is
        ///     thrown, the counter <code>written</code> is incremented by the
        ///     length of <code>s</code>.
        /// </remarks>
        /// <param name="s">a string of bytes to be written.</param>
        /// <exception>
        ///     IOException
        ///     if an I/O error occurs.
        /// </exception>
        /// <seealso cref="FilterOutputStream.@out" />
        /// <exception cref="IOException" />
        public void WriteBytes(string s)
        {
            var len = s.Length;
            for (var i = 0; i < len; i++) Write(unchecked((byte) s[i]));
            IncCount(len);
        }

        /// <summary>
        ///     Writes a string to the underlying output stream as a sequence of
        ///     characters.
        /// </summary>
        /// <remarks>
        ///     Writes a string to the underlying output stream as a sequence of
        ///     characters. Each character is written to the data output stream as
        ///     if by the <code>writeChar</code> method. If no exception is
        ///     thrown, the counter <code>written</code> is incremented by twice
        ///     the length of <code>s</code>.
        /// </remarks>
        /// <param name="s">a <code>String</code> value to be written.</param>
        /// <exception>
        ///     IOException
        ///     if an I/O error occurs.
        /// </exception>
        /// <seealso cref="WriteChar(int)" />
        /// <seealso cref="FilterOutputStream.@out" />
        /// <exception cref="IOException" />
        public void WriteChars(string s)
        {
            var len = s.Length;
            for (var i = 0; i < len; i++)
            {
                int v = s[i];
                Write((byte)((int) ((uint) v >> 8) & 0xFF));
                Write((byte)((int) ((uint) v >> 0) & 0xFF));
            }

            IncCount(len * 2);
        }

        /// <summary>
        ///     Writes a string to the underlying output stream using
        ///     <a href="DataInput.html#modified-utf-8">modified UTF-8</a>
        ///     encoding in a machine-independent manner.
        /// </summary>
        /// <remarks>
        ///     Writes a string to the underlying output stream using
        ///     <a href="DataInput.html#modified-utf-8">modified UTF-8</a>
        ///     encoding in a machine-independent manner.
        ///     <p>
        ///         First, two bytes are written to the output stream as if by the
        ///         <code>writeShort</code> method giving the number of bytes to
        ///         follow. This value is the number of bytes actually written out,
        ///         not the length of the string. Following the length, each character
        ///         of the string is output, in sequence, using the modified UTF-8 encoding
        ///         for the character. If no exception is thrown, the counter
        ///         <code>written</code> is incremented by the total number of
        ///         bytes written to the output stream. This will be at least two
        ///         plus the length of <code>str</code>, and at most two plus
        ///         thrice the length of <code>str</code>.
        /// </remarks>
        /// <param name="str">a string to be written.</param>
        /// <exception>
        ///     IOException
        ///     if an I/O error occurs.
        /// </exception>
        /// <exception cref="IOException" />
        public void WriteUTF(string str)
        {
            WriteUTF(str, this);
        }

        /// <summary>
        ///     Increases the written counter by the specified value
        ///     until it reaches Integer.MAX_VALUE.
        /// </summary>
        private void IncCount(int value)
        {
            var temp = written + value;
            if (temp < 0) temp = int.MaxValue;
            written = temp;
        }

        /// <summary>Flushes this data output stream.</summary>
        /// <remarks>
        ///     Flushes this data output stream. This forces any buffered output
        ///     bytes to be written out to the stream.
        ///     <p>
        ///         The <code>flush</code> method of <code>DataOutputStream</code>
        ///         calls the <code>flush</code> method of its underlying output stream.
        /// </remarks>
        /// <exception>
        ///     IOException
        ///     if an I/O error occurs.
        /// </exception>
        /// <seealso cref="FilterOutputStream.@out" />
        /// <seealso cref="OutputStream.Flush()" />
        /// <exception cref="IOException" />
        public void Flush()
        {
            _out.Flush();
        }

        /// <summary>
        ///     Writes a string to the specified DataOutput using
        ///     <a href="DataInput.html#modified-utf-8">modified UTF-8</a>
        ///     encoding in a machine-independent manner.
        /// </summary>
        /// <remarks>
        ///     Writes a string to the specified DataOutput using
        ///     <a href="DataInput.html#modified-utf-8">modified UTF-8</a>
        ///     encoding in a machine-independent manner.
        ///     <p>
        ///         First, two bytes are written to out as if by the <code>writeShort</code>
        ///         method giving the number of bytes to follow. This value is the number of
        ///         bytes actually written out, not the length of the string. Following the
        ///         length, each character of the string is output, in sequence, using the
        ///         modified UTF-8 encoding for the character. If no exception is thrown, the
        ///         counter <code>written</code> is incremented by the total number of
        ///         bytes written to the output stream. This will be at least two
        ///         plus the length of <code>str</code>, and at most two plus
        ///         thrice the length of <code>str</code>.
        /// </remarks>
        /// <param name="str">a string to be written.</param>
        /// <param name="out">destination to write to</param>
        /// <returns>The number of bytes written out.</returns>
        /// <exception>
        ///     IOException
        ///     if an I/O error occurs.
        /// </exception>
        /// <exception cref="IOException" />
        internal static int WriteUTF(string str, DataOutput @out)
        {
            var strlen = str.Length;
            var utflen = 0;
            int c;
            var count = 0;
            /* use charAt instead of copying String to char array */
            for (var i = 0; i < strlen; i++)
            {
                c = str[i];
                if (c >= 0x0001 && c <= 0x007F)
                    utflen++;
                else if (c > 0x07FF)
                    utflen += 3;
                else
                    utflen += 2;
            }

            if (utflen > 65535) throw new Exception("UTFDataFormat: encoded string too long: " + utflen + " bytes");
            byte[] bytearr = null;
            if (@out is DataOutputStream)
            {
                var dos = (DataOutputStream) @out;
                if (dos.bytearr == null || dos.bytearr.Length < utflen + 2) dos.bytearr = new byte[utflen * 2 + 2];
                bytearr = dos.bytearr;
            }
            else
            {
                bytearr = new byte[utflen + 2];
            }

            bytearr[count++] = unchecked((byte) ((int) ((uint) utflen >> 8) & 0xFF));
            bytearr[count++] = unchecked((byte) ((int) ((uint) utflen >> 0) & 0xFF));
            var i_1 = 0;
            for (i_1 = 0; i_1 < strlen; i_1++)
            {
                c = str[i_1];
                if (!(c >= 0x0001 && c <= 0x007F)) break;
                bytearr[count++] = unchecked((byte) c);
            }

            for (; i_1 < strlen; i_1++)
            {
                c = str[i_1];
                if (c >= 0x0001 && c <= 0x007F)
                {
                    bytearr[count++] = unchecked((byte) c);
                }
                else if (c > 0x07FF)
                {
                    bytearr[count++] = unchecked((byte) (0xE0 | ((c >> 12) & 0x0F)));
                    bytearr[count++] = unchecked((byte) (0x80 | ((c >> 6) & 0x3F)));
                    bytearr[count++] = unchecked((byte) (0x80 | ((c >> 0) & 0x3F)));
                }
                else
                {
                    bytearr[count++] = unchecked((byte) (0xC0 | ((c >> 6) & 0x1F)));
                    bytearr[count++] = unchecked((byte) (0x80 | ((c >> 0) & 0x3F)));
                }
            }

            @out.Write(bytearr, 0, utflen + 2);
            return utflen + 2;
        }

        /// <summary>
        ///     Returns the current value of the counter <code>written</code>,
        ///     the number of bytes written to this data output stream so far.
        /// </summary>
        /// <remarks>
        ///     Returns the current value of the counter <code>written</code>,
        ///     the number of bytes written to this data output stream so far.
        ///     If the counter overflows, it will be wrapped to Integer.MAX_VALUE.
        /// </remarks>
        /// <returns>the value of the <code>written</code> field.</returns>
        /// <seealso cref="written" />
        public int Size()
        {
            return written;
        }
    }
}