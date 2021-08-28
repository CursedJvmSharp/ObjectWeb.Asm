/*
* Copyright (c) 1994, 2006, Oracle and/or its affiliates. All rights reserved.
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

namespace Java.IO
{
    /// <summary>
    ///     A data input stream lets an application read primitive Java data
    ///     types from an underlying input stream in a machine-independent
    ///     way.
    /// </summary>
    /// <remarks>
    ///     A data input stream lets an application read primitive Java data
    ///     types from an underlying input stream in a machine-independent
    ///     way. An application uses a data output stream to write data that
    ///     can later be read by a data input stream.
    ///     <p>
    ///         DataInputStream is not necessarily safe for multithreaded access.
    ///         Thread safety is optional and is the responsibility of users of
    ///         methods in this class.
    /// </remarks>
    /// <author>Arthur van Hoff</author>
    /// <seealso cref="DataOutputStream" />
    /// <since>JDK1.0</since>
    [System.Runtime.InteropServices.Guid("61B42867-2D34-4F4D-8853-2DA855000EED")]
    public class DataInputStream : DataInput, IDisposable
    {
        private readonly MemoryStream _in;
        private readonly byte[] readBuffer = new byte[8];

        /// <summary>working arrays initialized on demand by readUTF</summary>
        private byte[] bytearr = new byte[80];

        private char[] chararr = new char[80];

        private char[] lineBuffer;

        /// <summary>
        ///     Creates a DataInputStream that uses the specified
        ///     underlying InputStream.
        /// </summary>
        /// <param name="in">the specified input stream</param>
        public DataInputStream(MemoryStream @in)
        {
            _in = @in;
        }

        /// <summary>
        ///     See the general contract of the <code>readFully</code>
        ///     method of <code>DataInput</code>.
        /// </summary>
        /// <remarks>
        ///     See the general contract of the <code>readFully</code>
        ///     method of <code>DataInput</code>.
        ///     <p>
        ///         Bytes
        ///         for this operation are read from the contained
        ///         input stream.
        /// </remarks>
        /// <param name="b">the buffer into which the data is read.</param>
        /// <exception>
        ///     EOFException
        ///     if this input stream reaches the end before
        ///     reading all the bytes.
        /// </exception>
        /// <exception>
        ///     IOException
        ///     the stream has been closed and the contained
        ///     input stream does not support reading after close, or
        ///     another I/O error occurs.
        /// </exception>
        /// <seealso cref="FilterInputStream.@in" />
        /// <exception cref="IOException" />
        public void ReadFully(byte[] b)
        {
            ReadFully(b, 0, b.Length);
        }

        /// <summary>
        ///     See the general contract of the <code>readFully</code>
        ///     method of <code>DataInput</code>.
        /// </summary>
        /// <remarks>
        ///     See the general contract of the <code>readFully</code>
        ///     method of <code>DataInput</code>.
        ///     <p>
        ///         Bytes
        ///         for this operation are read from the contained
        ///         input stream.
        /// </remarks>
        /// <param name="b">the buffer into which the data is read.</param>
        /// <param name="off">the start offset of the data.</param>
        /// <param name="len">the number of bytes to read.</param>
        /// <exception>
        ///     EOFException
        ///     if this input stream reaches the end before
        ///     reading all the bytes.
        /// </exception>
        /// <exception>
        ///     IOException
        ///     the stream has been closed and the contained
        ///     input stream does not support reading after close, or
        ///     another I/O error occurs.
        /// </exception>
        /// <seealso cref="FilterInputStream.@in" />
        /// <exception cref="IOException" />
        public void ReadFully(byte[] b, int off, int len)
        {
            if (len < 0) throw new IndexOutOfRangeException();
            var n = 0;
            while (n < len)
            {
                var count = _in.Read(b, off + n, len - n);
                if (count < 0) throw new EOFException();
                n += count;
            }
        }

        /// <summary>
        ///     See the general contract of the <code>skipBytes</code>
        ///     method of <code>DataInput</code>.
        /// </summary>
        /// <remarks>
        ///     See the general contract of the <code>skipBytes</code>
        ///     method of <code>DataInput</code>.
        ///     <p>
        ///         Bytes for this operation are read from the contained
        ///         input stream.
        /// </remarks>
        /// <param name="n">the number of bytes to be skipped.</param>
        /// <returns>the actual number of bytes skipped.</returns>
        /// <exception>
        ///     IOException
        ///     if the contained input stream does not support
        ///     seek, or the stream has been closed and
        ///     the contained input stream does not support
        ///     reading after close, or another I/O error occurs.
        /// </exception>
        /// <exception cref="IOException" />
        public int SkipBytes(int n)
        {
            var total = 0;
            var cur = 0;
            while (total < n && (cur = (int)_in.Seek(n - total, SeekOrigin.Current)) > 0) total += cur;
            return total;
        }

        /// <summary>
        ///     See the general contract of the <code>readBoolean</code>
        ///     method of <code>DataInput</code>.
        /// </summary>
        /// <remarks>
        ///     See the general contract of the <code>readBoolean</code>
        ///     method of <code>DataInput</code>.
        ///     <p>
        ///         Bytes for this operation are read from the contained
        ///         input stream.
        /// </remarks>
        /// <returns>the <code>boolean</code> value read.</returns>
        /// <exception>
        ///     EOFException
        ///     if this input stream has reached the end.
        /// </exception>
        /// <exception>
        ///     IOException
        ///     the stream has been closed and the contained
        ///     input stream does not support reading after close, or
        ///     another I/O error occurs.
        /// </exception>
        /// <seealso cref="FilterInputStream.@in" />
        /// <exception cref="IOException" />
        public bool ReadBoolean()
        {
            var ch = _in.ReadByte();
            if (ch < 0) throw new EOFException();
            return ch != 0;
        }

        /// <summary>
        ///     See the general contract of the <code>readByte</code>
        ///     method of <code>DataInput</code>.
        /// </summary>
        /// <remarks>
        ///     See the general contract of the <code>readByte</code>
        ///     method of <code>DataInput</code>.
        ///     <p>
        ///         Bytes
        ///         for this operation are read from the contained
        ///         input stream.
        /// </remarks>
        /// <returns>
        ///     the next byte of this input stream as a signed 8-bit
        ///     <code>byte</code>.
        /// </returns>
        /// <exception>
        ///     EOFException
        ///     if this input stream has reached the end.
        /// </exception>
        /// <exception>
        ///     IOException
        ///     the stream has been closed and the contained
        ///     input stream does not support reading after close, or
        ///     another I/O error occurs.
        /// </exception>
        /// <seealso cref="FilterInputStream.@in" />
        /// <exception cref="IOException" />
        public byte ReadByte()
        {
            var ch = _in.ReadByte();
            if (ch < 0) throw new EOFException();
            return unchecked((byte)ch);
        }

        /// <summary>
        ///     See the general contract of the <code>readUnsignedByte</code>
        ///     method of <code>DataInput</code>.
        /// </summary>
        /// <remarks>
        ///     See the general contract of the <code>readUnsignedByte</code>
        ///     method of <code>DataInput</code>.
        ///     <p>
        ///         Bytes
        ///         for this operation are read from the contained
        ///         input stream.
        /// </remarks>
        /// <returns>
        ///     the next byte of this input stream, interpreted as an
        ///     unsigned 8-bit number.
        /// </returns>
        /// <exception>
        ///     EOFException
        ///     if this input stream has reached the end.
        /// </exception>
        /// <exception>
        ///     IOException
        ///     the stream has been closed and the contained
        ///     input stream does not support reading after close, or
        ///     another I/O error occurs.
        /// </exception>
        /// <seealso cref="FilterInputStream.@in" />
        /// <exception cref="IOException" />
        public int ReadUnsignedByte()
        {
            var ch = _in.ReadByte();
            if (ch < 0) throw new EOFException();
            return ch;
        }

        /// <summary>
        ///     See the general contract of the <code>readShort</code>
        ///     method of <code>DataInput</code>.
        /// </summary>
        /// <remarks>
        ///     See the general contract of the <code>readShort</code>
        ///     method of <code>DataInput</code>.
        ///     <p>
        ///         Bytes
        ///         for this operation are read from the contained
        ///         input stream.
        /// </remarks>
        /// <returns>
        ///     the next two bytes of this input stream, interpreted as a
        ///     signed 16-bit number.
        /// </returns>
        /// <exception>
        ///     EOFException
        ///     if this input stream reaches the end before
        ///     reading two bytes.
        /// </exception>
        /// <exception>
        ///     IOException
        ///     the stream has been closed and the contained
        ///     input stream does not support reading after close, or
        ///     another I/O error occurs.
        /// </exception>
        /// <seealso cref="FilterInputStream.@in" />
        /// <exception cref="IOException" />
        public short ReadShort()
        {
            var ch1 = _in.ReadByte();
            var ch2 = _in.ReadByte();
            if ((ch1 | ch2) < 0) throw new EOFException();
            return (short)((ch1 << 8) + (ch2 << 0));
        }

        /// <summary>
        ///     See the general contract of the <code>readUnsignedShort</code>
        ///     method of <code>DataInput</code>.
        /// </summary>
        /// <remarks>
        ///     See the general contract of the <code>readUnsignedShort</code>
        ///     method of <code>DataInput</code>.
        ///     <p>
        ///         Bytes
        ///         for this operation are read from the contained
        ///         input stream.
        /// </remarks>
        /// <returns>
        ///     the next two bytes of this input stream, interpreted as an
        ///     unsigned 16-bit integer.
        /// </returns>
        /// <exception>
        ///     EOFException
        ///     if this input stream reaches the end before
        ///     reading two bytes.
        /// </exception>
        /// <exception>
        ///     IOException
        ///     the stream has been closed and the contained
        ///     input stream does not support reading after close, or
        ///     another I/O error occurs.
        /// </exception>
        /// <seealso cref="FilterInputStream.@in" />
        /// <exception cref="IOException" />
        public int ReadUnsignedShort()
        {
            var ch1 = _in.ReadByte();
            var ch2 = _in.ReadByte();
            if ((ch1 | ch2) < 0) throw new EOFException();
            return (ch1 << 8) + (ch2 << 0);
        }

        /// <summary>
        ///     See the general contract of the <code>readChar</code>
        ///     method of <code>DataInput</code>.
        /// </summary>
        /// <remarks>
        ///     See the general contract of the <code>readChar</code>
        ///     method of <code>DataInput</code>.
        ///     <p>
        ///         Bytes
        ///         for this operation are read from the contained
        ///         input stream.
        /// </remarks>
        /// <returns>
        ///     the next two bytes of this input stream, interpreted as a
        ///     <code>char</code>.
        /// </returns>
        /// <exception>
        ///     EOFException
        ///     if this input stream reaches the end before
        ///     reading two bytes.
        /// </exception>
        /// <exception>
        ///     IOException
        ///     the stream has been closed and the contained
        ///     input stream does not support reading after close, or
        ///     another I/O error occurs.
        /// </exception>
        /// <seealso cref="FilterInputStream.@in" />
        /// <exception cref="IOException" />
        public char ReadChar()
        {
            var ch1 = _in.ReadByte();
            var ch2 = _in.ReadByte();
            if ((ch1 | ch2) < 0) throw new EOFException();
            return (char)((ch1 << 8) + (ch2 << 0));
        }

        /// <summary>
        ///     See the general contract of the <code>readInt</code>
        ///     method of <code>DataInput</code>.
        /// </summary>
        /// <remarks>
        ///     See the general contract of the <code>readInt</code>
        ///     method of <code>DataInput</code>.
        ///     <p>
        ///         Bytes
        ///         for this operation are read from the contained
        ///         input stream.
        /// </remarks>
        /// <returns>
        ///     the next four bytes of this input stream, interpreted as an
        ///     <code>int</code>.
        /// </returns>
        /// <exception>
        ///     EOFException
        ///     if this input stream reaches the end before
        ///     reading four bytes.
        /// </exception>
        /// <exception>
        ///     IOException
        ///     the stream has been closed and the contained
        ///     input stream does not support reading after close, or
        ///     another I/O error occurs.
        /// </exception>
        /// <seealso cref="FilterInputStream.@in" />
        /// <exception cref="IOException" />
        public int ReadInt()
        {
            var ch1 = _in.ReadByte();
            var ch2 = _in.ReadByte();
            var ch3 = _in.ReadByte();
            var ch4 = _in.ReadByte();
            if ((ch1 | ch2 | ch3 | ch4) < 0) throw new EOFException();
            return (ch1 << 24) + (ch2 << 16) + (ch3 << 8) + (ch4 << 0);
        }

        /// <summary>
        ///     See the general contract of the <code>readLong</code>
        ///     method of <code>DataInput</code>.
        /// </summary>
        /// <remarks>
        ///     See the general contract of the <code>readLong</code>
        ///     method of <code>DataInput</code>.
        ///     <p>
        ///         Bytes
        ///         for this operation are read from the contained
        ///         input stream.
        /// </remarks>
        /// <returns>
        ///     the next eight bytes of this input stream, interpreted as a
        ///     <code>long</code>.
        /// </returns>
        /// <exception>
        ///     EOFException
        ///     if this input stream reaches the end before
        ///     reading eight bytes.
        /// </exception>
        /// <exception>
        ///     IOException
        ///     the stream has been closed and the contained
        ///     input stream does not support reading after close, or
        ///     another I/O error occurs.
        /// </exception>
        /// <seealso cref="FilterInputStream.@in" />
        /// <exception cref="IOException" />
        public long ReadLong()
        {
            ReadFully(readBuffer, 0, 8);
            return ((long)readBuffer[0] << 56) + ((long)(readBuffer[1] & 255) << 48) + ((long
                       )(readBuffer[2] & 255) <<
                       40) + ((long)(readBuffer[3] &
                                     255) << 32) +
                   ((long)(readBuffer
                       [4] & 255) << 24) + ((readBuffer[5] & 255) << 16) + ((readBuffer[6] & 255) << 8)
                   + ((readBuffer[7] & 255) << 0);
        }

        /// <summary>
        ///     See the general contract of the <code>readFloat</code>
        ///     method of <code>DataInput</code>.
        /// </summary>
        /// <remarks>
        ///     See the general contract of the <code>readFloat</code>
        ///     method of <code>DataInput</code>.
        ///     <p>
        ///         Bytes
        ///         for this operation are read from the contained
        ///         input stream.
        /// </remarks>
        /// <returns>
        ///     the next four bytes of this input stream, interpreted as a
        ///     <code>float</code>.
        /// </returns>
        /// <exception>
        ///     EOFException
        ///     if this input stream reaches the end before
        ///     reading four bytes.
        /// </exception>
        /// <exception>
        ///     IOException
        ///     the stream has been closed and the contained
        ///     input stream does not support reading after close, or
        ///     another I/O error occurs.
        /// </exception>
        /// <seealso cref="ReadInt()" />
        /// <seealso cref="Runtime.IntBitsToFloat" />
        /// <exception cref="IOException" />
        public float ReadFloat()
        {
            return BitConverter.Int32BitsToSingle(ReadInt());
        }

        /// <summary>
        ///     See the general contract of the <code>readDouble</code>
        ///     method of <code>DataInput</code>.
        /// </summary>
        /// <remarks>
        ///     See the general contract of the <code>readDouble</code>
        ///     method of <code>DataInput</code>.
        ///     <p>
        ///         Bytes
        ///         for this operation are read from the contained
        ///         input stream.
        /// </remarks>
        /// <returns>
        ///     the next eight bytes of this input stream, interpreted as a
        ///     <code>double</code>.
        /// </returns>
        /// <exception>
        ///     EOFException
        ///     if this input stream reaches the end before
        ///     reading eight bytes.
        /// </exception>
        /// <exception>
        ///     IOException
        ///     the stream has been closed and the contained
        ///     input stream does not support reading after close, or
        ///     another I/O error occurs.
        /// </exception>
        /// <seealso cref="ReadLong()" />
        /// <seealso cref="BitConverter.Int64BitsToDouble(long)" />
        /// <exception cref="System.IO.IOException" />
        public double ReadDouble()
        {
            return BitConverter.Int64BitsToDouble(ReadLong());
        }


        /// <summary>
        ///     See the general contract of the <code>readUTF</code>
        ///     method of <code>DataInput</code>.
        /// </summary>
        /// <remarks>
        ///     See the general contract of the <code>readUTF</code>
        ///     method of <code>DataInput</code>.
        ///     <p>
        ///         Bytes
        ///         for this operation are read from the contained
        ///         input stream.
        /// </remarks>
        /// <returns>a Unicode string.</returns>
        /// <exception>
        ///     EOFException
        ///     if this input stream reaches the end before
        ///     reading all the bytes.
        /// </exception>
        /// <exception>
        ///     IOException
        ///     the stream has been closed and the contained
        ///     input stream does not support reading after close, or
        ///     another I/O error occurs.
        /// </exception>
        /// <exception>
        ///     UTFDataFormatException
        ///     if the bytes do not represent a valid
        ///     modified UTF-8 encoding of a string.
        /// </exception>
        /// <seealso cref="ReadUTF(DataInput)" />
        /// <exception cref="IOException" />
        public string ReadUTF()
        {
            return ReadUTF(this);
        }

        /// <summary>
        ///     Reads some number of bytes from the contained input stream and
        ///     stores them into the buffer array <code>b</code>.
        /// </summary>
        /// <remarks>
        ///     Reads some number of bytes from the contained input stream and
        ///     stores them into the buffer array <code>b</code>. The number of
        ///     bytes actually read is returned as an integer. This method blocks
        ///     until input data is available, end of file is detected, or an
        ///     exception is thrown.
        ///     <p>
        ///         If <code>b</code> is null, a <code>NullPointerException</code> is
        ///         thrown. If the length of <code>b</code> is zero, then no bytes are
        ///         read and <code>0</code> is returned; otherwise, there is an attempt
        ///         to read at least one byte. If no byte is available because the
        ///         stream is at end of file, the value <code>-1</code> is returned;
        ///         otherwise, at least one byte is read and stored into <code>b</code>.
        ///         <p>
        ///             The first byte read is stored into element <code>b[0]</code>, the
        ///             next one into <code>b[1]</code>, and so on. The number of bytes read
        ///             is, at most, equal to the length of <code>b</code>. Let <code>k</code>
        ///             be the number of bytes actually read; these bytes will be stored in
        ///             elements <code>b[0]</code> through <code>b[k-1]</code>, leaving
        ///             elements <code>b[k]</code> through <code>b[b.length-1]</code>
        ///             unaffected.
        ///             <p>
        ///                 The <code>read(b)</code> method has the same effect as:
        ///                 <blockquote>
        ///                     <pre>
        ///                         read(b, 0, b.length)
        ///                     </pre>
        ///                 </blockquote>
        /// </remarks>
        /// <param name="b">the buffer into which the data is read.</param>
        /// <returns>
        ///     the total number of bytes read into the buffer, or
        ///     <code>-1</code> if there is no more data because the end
        ///     of the stream has been reached.
        /// </returns>
        /// <exception>
        ///     IOException
        ///     if the first byte cannot be read for any reason
        ///     other than end of file, the stream has been closed and the underlying
        ///     input stream does not support reading after close, or another I/O
        ///     error occurs.
        /// </exception>
        /// <seealso cref="FilterInputStream.@in" />
        /// <seealso cref="InputStream.Read(byte[], int, int)" />
        /// <exception cref="IOException" />
        public int Read(byte[] b)
        {
            return _in.Read(b, 0, b.Length);
        }

        /// <summary>
        ///     Reads up to <code>len</code> bytes of data from the contained
        ///     input stream into an array of bytes.
        /// </summary>
        /// <remarks>
        ///     Reads up to <code>len</code> bytes of data from the contained
        ///     input stream into an array of bytes.  An attempt is made to read
        ///     as many as <code>len</code> bytes, but a smaller number may be read,
        ///     possibly zero. The number of bytes actually read is returned as an
        ///     integer.
        ///     <p>
        ///         This method blocks until input data is available, end of file is
        ///         detected, or an exception is thrown.
        ///         <p>
        ///             If <code>len</code> is zero, then no bytes are read and
        ///             <code>0</code> is returned; otherwise, there is an attempt to read at
        ///             least one byte. If no byte is available because the stream is at end of
        ///             file, the value <code>-1</code> is returned; otherwise, at least one
        ///             byte is read and stored into <code>b</code>.
        ///             <p>
        ///                 The first byte read is stored into element <code>b[off]</code>, the
        ///                 next one into <code>b[off+1]</code>, and so on. The number of bytes read
        ///                 is, at most, equal to <code>len</code>. Let <i>k</i> be the number of
        ///                 bytes actually read; these bytes will be stored in elements
        ///                 <code>b[off]</code> through <code>b[off+</code><i>k</i><code>-1]</code>,
        ///                 leaving elements <code>b[off+</code><i>k</i><code>]</code> through
        ///                 <code>b[off+len-1]</code> unaffected.
        ///                 <p>
        ///                     In every case, elements <code>b[0]</code> through
        ///                     <code>b[off]</code> and elements <code>b[off+len]</code> through
        ///                     <code>b[b.length-1]</code> are unaffected.
        /// </remarks>
        /// <param name="b">the buffer into which the data is read.</param>
        /// <param name="off">the start offset in the destination array <code>b</code></param>
        /// <param name="len">the maximum number of bytes read.</param>
        /// <returns>
        ///     the total number of bytes read into the buffer, or
        ///     <code>-1</code> if there is no more data because the end
        ///     of the stream has been reached.
        /// </returns>
        /// <exception>
        ///     NullPointerException
        ///     If <code>b</code> is <code>null</code>.
        /// </exception>
        /// <exception>
        ///     IndexOutOfBoundsException
        ///     If <code>off</code> is negative,
        ///     <code>len</code> is negative, or <code>len</code> is greater than
        ///     <code>b.length - off</code>
        /// </exception>
        /// <exception>
        ///     IOException
        ///     if the first byte cannot be read for any reason
        ///     other than end of file, the stream has been closed and the underlying
        ///     input stream does not support reading after close, or another I/O
        ///     error occurs.
        /// </exception>
        /// <seealso cref="FilterInputStream.@in" />
        /// <seealso cref="InputStream.Read(byte[], int, int)" />
        /// <exception cref="IOException" />
        public  int Read(byte[] b, int off, int len)
        {
            return _in.Read(b, off, len);
        }

        /// <summary>
        ///     Reads from the
        ///     stream <code>in</code> a representation
        ///     of a Unicode  character string encoded in
        ///     <a href="DataInput.html#modified-utf-8">modified UTF-8</a> format;
        ///     this string of characters is then returned as a <code>String</code>.
        /// </summary>
        /// <remarks>
        ///     Reads from the
        ///     stream <code>in</code> a representation
        ///     of a Unicode  character string encoded in
        ///     <a href="DataInput.html#modified-utf-8">modified UTF-8</a> format;
        ///     this string of characters is then returned as a <code>String</code>.
        ///     The details of the modified UTF-8 representation
        ///     are  exactly the same as for the <code>readUTF</code>
        ///     method of <code>DataInput</code>.
        /// </remarks>
        /// <param name="in">a data input stream.</param>
        /// <returns>a Unicode string.</returns>
        /// <exception>
        ///     EOFException
        ///     if the input stream reaches the end
        ///     before all the bytes.
        /// </exception>
        /// <exception>
        ///     IOException
        ///     the stream has been closed and the contained
        ///     input stream does not support reading after close, or
        ///     another I/O error occurs.
        /// </exception>
        /// <exception>
        ///     UTFDataFormatException
        ///     if the bytes do not represent a
        ///     valid modified UTF-8 encoding of a Unicode string.
        /// </exception>
        /// <seealso cref="ReadUnsignedShort()" />
        /// <exception cref="IOException" />
        public static string ReadUTF(DataInput @in)
        {
            var utflen = @in.ReadUnsignedShort();
            byte[] bytearr = null;
            char[] chararr = null;
            if (@in is DataInputStream)
            {
                var dis = (DataInputStream)@in;
                if (dis.bytearr.Length < utflen)
                {
                    dis.bytearr = new byte[utflen * 2];
                    dis.chararr = new char[utflen * 2];
                }

                chararr = dis.chararr;
                bytearr = dis.bytearr;
            }
            else
            {
                bytearr = new byte[utflen];
                chararr = new char[utflen];
            }

            int c;
            int char2;
            int char3;
            var count = 0;
            var chararr_count = 0;
            @in.ReadFully(bytearr, 0, utflen);
            while (count < utflen)
            {
                c = bytearr[count] & 0xff;
                if (c > 127) break;
                count++;
                chararr[chararr_count++] = (char)c;
            }

            while (count < utflen)
            {
                c = bytearr[count] & 0xff;
                switch (c >> 4)
                {
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                    {
                        /* 0xxxxxxx*/
                        count++;
                        chararr[chararr_count++] = (char)c;
                        break;
                    }

                    case 12:
                    case 13:
                    {
                        /* 110x xxxx   10xx xxxx*/
                        count += 2;
                        if (count > utflen)
                            throw new Exception("UTFDataFormat: malformed input: partial character at end");
                        char2 = bytearr[count - 1];
                        if ((char2 & 0xC0) != 0x80)
                            throw new Exception("UTFDataFormat: malformed input around byte " + count);
                        chararr[chararr_count++] = (char)(((c & 0x1F) << 6) | (char2 &
                                                                               0x3F));
                        break;
                    }

                    case 14:
                    {
                        /* 1110 xxxx  10xx xxxx  10xx xxxx */
                        count += 3;
                        if (count > utflen)
                            throw new Exception("UTFDataFormat: malformed input: partial character at end");
                        char2 = bytearr[count - 2];
                        char3 = bytearr[count - 1];
                        if ((char2 & 0xC0) != 0x80 || (char3 & 0xC0) != 0x80)
                            throw new Exception("UTFDataFormat: malformed input around byte " + (count - 1));
                        chararr[chararr_count++] = (char)(((c & 0x0F) << 12) | ((char2
                                                              & 0x3F) << 6) |
                                                          ((char3 & 0x3F) << 0));
                        break;
                    }

                    default:
                    {
                        /* 10xx xxxx,  1111 xxxx */
                        throw new Exception("UTFDataFormat: malformed input around byte " + count);
                    }
                }
            }

            // The number of chars produced may be less than utflen
            return new string(chararr, 0, chararr_count);
        }

        public void Dispose()
        {
            _in?.Dispose();
        }
    }
}