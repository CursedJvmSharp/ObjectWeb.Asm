/*
* Copyright (c) 1995, 2013, Oracle and/or its affiliates. All rights reserved.
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

using System.IO;

namespace Java.IO
{
    /// <summary>
    ///     The
    ///     <c>DataInput</c>
    ///     interface provides
    ///     for reading bytes from a binary stream and
    ///     reconstructing from them data in any of
    ///     the Java primitive types. There is also
    ///     a
    ///     facility for reconstructing a
    ///     <c>String</c>
    ///     from data in
    ///     <a href="#modified-utf-8">modified UTF-8</a>
    ///     format.
    ///     <p>
    ///         It is generally true of all the reading
    ///         routines in this interface that if end of
    ///         file is reached before the desired number
    ///         of bytes has been read, an
    ///         <c>EOFException</c>
    ///         (which is a kind of
    ///         <c>IOException</c>
    ///         )
    ///         is thrown. If any byte cannot be read for
    ///         any reason other than end of file, an
    ///         <c>IOException</c>
    ///         other than
    ///         <c>EOFException</c>
    ///         is
    ///         thrown. In particular, an
    ///         <c>IOException</c>
    ///         may be thrown if the input stream has been
    ///         closed.
    ///         <h3>
    ///             <a name="modified-utf-8">Modified UTF-8</a>
    ///         </h3>
    ///         <p>
    ///             Implementations of the DataInput and DataOutput interfaces represent
    ///             Unicode strings in a format that is a slight modification of UTF-8.
    ///             (For information regarding the standard UTF-8 format, see section
    ///             <i>3.9 Unicode Encoding Forms</i> of
    ///             <i>
    ///                 The Unicode Standard, Version
    ///                 4.0
    ///             </i>
    ///             ).
    ///             Note that in the following table, the most significant bit appears in the
    ///             far left-hand column.
    ///             <blockquote>
    ///                 &lt;table border="1" cellspacing="0" cellpadding="8"
    ///                 summary="Bit Values and bytes"&gt;
    ///                 <tr>
    ///                     <th colspan="9">
    ///                         <span style="font-weight:normal">
    ///                             All characters in the range
    ///                             <c>'\u005Cu0001'</c>
    ///                             to
    ///                             <c>'\u005Cu007F'</c>
    ///                             are represented by a single byte:
    ///                         </span>
    ///                     </th>
    ///                 </tr>
    ///                 <tr>
    ///                     <td></td>
    ///                     <th colspan="8" id="bit_a">Bit Values</th>
    ///                 </tr>
    ///                 <tr>
    ///                     <th id="byte1_a">Byte 1</th>
    ///                     <td>
    ///                         <center>0</center>
    ///                         <td colspan="7">
    ///                             <center>bits 6-0</center>
    ///                 </tr>
    ///                 <tr>
    ///                     <th colspan="9">
    ///                         <span style="font-weight:normal">
    ///                             The null character
    ///                             <c>'\u005Cu0000'</c>
    ///                             and characters
    ///                             in the range
    ///                             <c>'\u005Cu0080'</c>
    ///                             to
    ///                             <c>'\u005Cu07FF'</c>
    ///                             are
    ///                             represented by a pair of bytes:
    ///                         </span>
    ///                     </th>
    ///                 </tr>
    ///                 <tr>
    ///                     <td></td>
    ///                     <th colspan="8" id="bit_b">Bit Values</th>
    ///                 </tr>
    ///                 <tr>
    ///                     <th id="byte1_b">Byte 1</th>
    ///                     <td>
    ///                         <center>1</center>
    ///                         <td>
    ///                             <center>1</center>
    ///                             <td>
    ///                                 <center>0</center>
    ///                                 <td colspan="5">
    ///                                     <center>bits 10-6</center>
    ///                 </tr>
    ///                 <tr>
    ///                     <th id="byte2_a">Byte 2</th>
    ///                     <td>
    ///                         <center>1</center>
    ///                         <td>
    ///                             <center>0</center>
    ///                             <td colspan="6">
    ///                                 <center>bits 5-0</center>
    ///                 </tr>
    ///                 <tr>
    ///                     <th colspan="9">
    ///                         <span style="font-weight:normal">
    ///                             <c>char</c>
    ///                             Values in the range
    ///                             <c>'\u005Cu0800'</c>
    ///                             to
    ///                             <c>'\u005CuFFFF'</c>
    ///                             are represented by three bytes:
    ///                         </span>
    ///                     </th>
    ///                 </tr>
    ///                 <tr>
    ///                     <td></td>
    ///                     <th colspan="8" id="bit_c">Bit Values</th>
    ///                 </tr>
    ///                 <tr>
    ///                     <th id="byte1_c">Byte 1</th>
    ///                     <td>
    ///                         <center>1</center>
    ///                         <td>
    ///                             <center>1</center>
    ///                             <td>
    ///                                 <center>1</center>
    ///                                 <td>
    ///                                     <center>0</center>
    ///                                     <td colspan="4">
    ///                                         <center>bits 15-12</center>
    ///                 </tr>
    ///                 <tr>
    ///                     <th id="byte2_b">Byte 2</th>
    ///                     <td>
    ///                         <center>1</center>
    ///                         <td>
    ///                             <center>0</center>
    ///                             <td colspan="6">
    ///                                 <center>bits 11-6</center>
    ///                 </tr>
    ///                 <tr>
    ///                     <th id="byte3">Byte 3</th>
    ///                     <td>
    ///                         <center>1</center>
    ///                         <td>
    ///                             <center>0</center>
    ///                             <td colspan="6">
    ///                                 <center>bits 5-0</center>
    ///                 </tr>
    ///                 </table>
    ///             </blockquote>
    ///             <p>
    ///                 The differences between this format and the
    ///                 standard UTF-8 format are the following:
    ///                 <ul>
    ///                     <li>
    ///                         The null byte
    ///                         <c>'\u005Cu0000'</c>
    ///                         is encoded in 2-byte format
    ///                         rather than 1-byte, so that the encoded strings never have
    ///                         embedded nulls.
    ///                         <li>
    ///                             Only the 1-byte, 2-byte, and 3-byte formats are used.
    ///                             <li>
    ///                                 <a href="../lang/Character.html#unicode">Supplementary characters</a>
    ///                                 are represented in the form of surrogate pairs.
    ///                 </ul>
    /// </summary>
    /// <author>Frank Yellin</author>
    /// <seealso cref="DataInputStream" />
    /// <seealso cref="IDataOutput" />
    /// <since>JDK1.0</since>
    public interface IDataInput
    {
        /// <summary>
        ///     Reads some bytes from an input
        ///     stream and stores them into the buffer
        ///     array
        ///     <paramref name="b" />
        ///     . The number of bytes
        ///     read is equal
        ///     to the length of
        ///     <paramref name="b" />
        ///     .
        ///     <p>
        ///         This method blocks until one of the
        ///         following conditions occurs:
        ///         <ul>
        ///             <li>
        ///                 <c>b.length</c>
        ///                 bytes of input data are available, in which
        ///                 case a normal return is made.
        ///                 <li>
        ///                     End of
        ///                     file is detected, in which case an
        ///                     <c>EOFException</c>
        ///                     is thrown.
        ///                     <li>
        ///                         An I/O error occurs, in
        ///                         which case an
        ///                         <c>IOException</c>
        ///                         other
        ///                         than
        ///                         <c>EOFException</c>
        ///                         is thrown.
        ///         </ul>
        ///         <p>
        ///             If
        ///             <paramref name="b" />
        ///             is
        ///             <see langword="null" />
        ///             ,
        ///             a
        ///             <c>NullPointerException</c>
        ///             is thrown.
        ///             If
        ///             <c>b.length</c>
        ///             is zero, then
        ///             no bytes are read. Otherwise, the first
        ///             byte read is stored into element
        ///             <c>b[0]</c>
        ///             ,
        ///             the next one into
        ///             <c>b[1]</c>
        ///             , and
        ///             so on.
        ///             If an exception is thrown from
        ///             this method, then it may be that some but
        ///             not all bytes of
        ///             <paramref name="b" />
        ///             have been
        ///             updated with data from the input stream.
        /// </summary>
        /// <param name="b">the buffer into which the data is read.</param>
        /// <exception>
        ///     EOFException
        ///     if this stream reaches the end before reading
        ///     all the bytes.
        /// </exception>
        /// <exception>
        ///     IOException
        ///     if an I/O error occurs.
        /// </exception>
        /// <exception cref="IOException" />
        void ReadFully(byte[] b);

        /// <summary>
        ///     Reads
        ///     <paramref name="len" />
        ///     bytes from
        ///     an input stream.
        ///     <p>
        ///         This method
        ///         blocks until one of the following conditions
        ///         occurs:
        ///         <ul>
        ///             <li>
        ///                 <paramref name="len" />
        ///                 bytes
        ///                 of input data are available, in which case
        ///                 a normal return is made.
        ///                 <li>
        ///                     End of file
        ///                     is detected, in which case an
        ///                     <c>EOFException</c>
        ///                     is thrown.
        ///                     <li>
        ///                         An I/O error occurs, in
        ///                         which case an
        ///                         <c>IOException</c>
        ///                         other
        ///                         than
        ///                         <c>EOFException</c>
        ///                         is thrown.
        ///         </ul>
        ///         <p>
        ///             If
        ///             <paramref name="b" />
        ///             is
        ///             <see langword="null" />
        ///             ,
        ///             a
        ///             <c>NullPointerException</c>
        ///             is thrown.
        ///             If
        ///             <paramref name="off" />
        ///             is negative, or
        ///             <paramref name="len" />
        ///             is negative, or
        ///             <c>off+len</c>
        ///             is
        ///             greater than the length of the array
        ///             <paramref name="b" />
        ///             ,
        ///             then an
        ///             <c>IndexOutOfBoundsException</c>
        ///             is thrown.
        ///             If
        ///             <paramref name="len" />
        ///             is zero,
        ///             then no bytes are read. Otherwise, the first
        ///             byte read is stored into element
        ///             <c>b[off]</c>
        ///             ,
        ///             the next one into
        ///             <c>b[off+1]</c>
        ///             ,
        ///             and so on. The number of bytes read is,
        ///             at most, equal to
        ///             <paramref name="len" />
        ///             .
        /// </summary>
        /// <param name="b">the buffer into which the data is read.</param>
        /// <param name="off">an int specifying the offset into the data.</param>
        /// <param name="len">an int specifying the number of bytes to read.</param>
        /// <exception>
        ///     EOFException
        ///     if this stream reaches the end before reading
        ///     all the bytes.
        /// </exception>
        /// <exception>
        ///     IOException
        ///     if an I/O error occurs.
        /// </exception>
        /// <exception cref="IOException" />
        void ReadFully(byte[] b, int off, int len);

        /// <summary>
        ///     Makes an attempt to skip over
        ///     <paramref name="n" />
        ///     bytes
        ///     of data from the input
        ///     stream, discarding the skipped bytes. However,
        ///     it may skip
        ///     over some smaller number of
        ///     bytes, possibly zero. This may result from
        ///     any of a
        ///     number of conditions; reaching
        ///     end of file before
        ///     <paramref name="n" />
        ///     bytes
        ///     have been skipped is
        ///     only one possibility.
        ///     This method never throws an
        ///     <c>EOFException</c>
        ///     .
        ///     The actual
        ///     number of bytes skipped is returned.
        /// </summary>
        /// <param name="n">the number of bytes to be skipped.</param>
        /// <returns>the number of bytes actually skipped.</returns>
        /// <exception>
        ///     IOException
        ///     if an I/O error occurs.
        /// </exception>
        /// <exception cref="IOException" />
        int SkipBytes(int n);

        /// <summary>
        ///     Reads one input byte and returns
        ///     <see langword="true" />
        ///     if that byte is nonzero,
        ///     <see langword="false" />
        ///     if that byte is zero.
        ///     This method is suitable for reading
        ///     the byte written by the
        ///     <c>writeBoolean</c>
        ///     method of interface
        ///     <c>DataOutput</c>
        ///     .
        /// </summary>
        /// <returns>
        ///     the
        ///     <c>boolean</c>
        ///     value read.
        /// </returns>
        /// <exception>
        ///     EOFException
        ///     if this stream reaches the end before reading
        ///     all the bytes.
        /// </exception>
        /// <exception>
        ///     IOException
        ///     if an I/O error occurs.
        /// </exception>
        /// <exception cref="IOException" />
        bool ReadBoolean();

        /// <summary>Reads and returns one input byte.</summary>
        /// <remarks>
        ///     Reads and returns one input byte.
        ///     The byte is treated as a signed value in
        ///     the range
        ///     <c>-128</c>
        ///     through
        ///     <c>127</c>
        ///     ,
        ///     inclusive.
        ///     This method is suitable for
        ///     reading the byte written by the
        ///     <c>writeByte</c>
        ///     method of interface
        ///     <c>DataOutput</c>
        ///     .
        /// </remarks>
        /// <returns>the 8-bit value read.</returns>
        /// <exception>
        ///     EOFException
        ///     if this stream reaches the end before reading
        ///     all the bytes.
        /// </exception>
        /// <exception>
        ///     IOException
        ///     if an I/O error occurs.
        /// </exception>
        /// <exception cref="IOException" />
        byte ReadByte();

        /// <summary>
        ///     Reads one input byte, zero-extends
        ///     it to type
        ///     <c>int</c>
        ///     , and returns
        ///     the result, which is therefore in the range
        ///     <c>0</c>
        ///     through
        ///     <c>255</c>
        ///     .
        ///     This method is suitable for reading
        ///     the byte written by the
        ///     <c>writeByte</c>
        ///     method of interface
        ///     <c>DataOutput</c>
        ///     if the argument to
        ///     <c>writeByte</c>
        ///     was intended to be a value in the range
        ///     <c>0</c>
        ///     through
        ///     <c>255</c>
        ///     .
        /// </summary>
        /// <returns>the unsigned 8-bit value read.</returns>
        /// <exception>
        ///     EOFException
        ///     if this stream reaches the end before reading
        ///     all the bytes.
        /// </exception>
        /// <exception>
        ///     IOException
        ///     if an I/O error occurs.
        /// </exception>
        /// <exception cref="IOException" />
        int ReadUnsignedByte();

        /// <summary>
        ///     Reads two input bytes and returns
        ///     a
        ///     <c>short</c>
        ///     value. Let
        ///     <c>a</c>
        ///     be the first byte read and
        ///     <c>b</c>
        ///     be the second byte. The value
        ///     returned
        ///     is:
        ///     <pre>
        ///         <c>(short)((a &lt;&lt; 8) | (b & 0xff))</c>
        ///     </pre>
        ///     This method
        ///     is suitable for reading the bytes written
        ///     by the
        ///     <c>writeShort</c>
        ///     method of
        ///     interface
        ///     <c>DataOutput</c>
        ///     .
        /// </summary>
        /// <returns>the 16-bit value read.</returns>
        /// <exception>
        ///     EOFException
        ///     if this stream reaches the end before reading
        ///     all the bytes.
        /// </exception>
        /// <exception>
        ///     IOException
        ///     if an I/O error occurs.
        /// </exception>
        /// <exception cref="IOException" />
        short ReadShort();

        /// <summary>
        ///     Reads two input bytes and returns
        ///     an
        ///     <c>int</c>
        ///     value in the range
        ///     <c>0</c>
        ///     through
        ///     <c>65535</c>
        ///     . Let
        ///     <c>a</c>
        ///     be the first byte read and
        ///     <c>b</c>
        ///     be the second byte. The value returned is:
        ///     <pre>
        ///         <c>(((a & 0xff) &lt;&lt; 8) | (b & 0xff))</c>
        ///     </pre>
        ///     This method is suitable for reading the bytes
        ///     written by the
        ///     <c>writeShort</c>
        ///     method
        ///     of interface
        ///     <c>DataOutput</c>
        ///     if
        ///     the argument to
        ///     <c>writeShort</c>
        ///     was intended to be a value in the range
        ///     <c>0</c>
        ///     through
        ///     <c>65535</c>
        ///     .
        /// </summary>
        /// <returns>the unsigned 16-bit value read.</returns>
        /// <exception>
        ///     EOFException
        ///     if this stream reaches the end before reading
        ///     all the bytes.
        /// </exception>
        /// <exception>
        ///     IOException
        ///     if an I/O error occurs.
        /// </exception>
        /// <exception cref="IOException" />
        int ReadUnsignedShort();

        /// <summary>
        ///     Reads two input bytes and returns a
        ///     <c>char</c>
        ///     value.
        ///     Let
        ///     <c>a</c>
        ///     be the first byte read and
        ///     <c>b</c>
        ///     be the second byte. The value
        ///     returned is:
        ///     <pre>
        ///         <c>(char)((a &lt;&lt; 8) | (b & 0xff))</c>
        ///     </pre>
        ///     This method
        ///     is suitable for reading bytes written by
        ///     the
        ///     <c>writeChar</c>
        ///     method of interface
        ///     <c>DataOutput</c>
        ///     .
        /// </summary>
        /// <returns>
        ///     the
        ///     <c>char</c>
        ///     value read.
        /// </returns>
        /// <exception>
        ///     EOFException
        ///     if this stream reaches the end before reading
        ///     all the bytes.
        /// </exception>
        /// <exception>
        ///     IOException
        ///     if an I/O error occurs.
        /// </exception>
        /// <exception cref="IOException" />
        char ReadChar();

        /// <summary>
        ///     Reads four input bytes and returns an
        ///     <c>int</c>
        ///     value. Let
        ///     <c>a-d</c>
        ///     be the first through fourth bytes read. The value returned is:
        ///     <pre>
        ///         <c>
        ///             (((a & 0xff) &lt;&lt; 24) | ((b & 0xff) &lt;&lt; 16) |
        ///             ((c & 0xff) &lt;&lt;  8) | (d & 0xff))
        ///         </c>
        ///     </pre>
        ///     This method is suitable
        ///     for reading bytes written by the
        ///     <c>writeInt</c>
        ///     method of interface
        ///     <c>DataOutput</c>
        ///     .
        /// </summary>
        /// <returns>
        ///     the
        ///     <c>int</c>
        ///     value read.
        /// </returns>
        /// <exception>
        ///     EOFException
        ///     if this stream reaches the end before reading
        ///     all the bytes.
        /// </exception>
        /// <exception>
        ///     IOException
        ///     if an I/O error occurs.
        /// </exception>
        /// <exception cref="IOException" />
        int ReadInt();

        /// <summary>
        ///     Reads eight input bytes and returns
        ///     a
        ///     <c>long</c>
        ///     value. Let
        ///     <c>a-h</c>
        ///     be the first through eighth bytes read.
        ///     The value returned is:
        ///     <pre>
        ///         <c>
        ///             (((long)(a & 0xff) &lt;&lt; 56) |
        ///             ((long)(b & 0xff) &lt;&lt; 48) |
        ///             ((long)(c & 0xff) &lt;&lt; 40) |
        ///             ((long)(d & 0xff) &lt;&lt; 32) |
        ///             ((long)(e & 0xff) &lt;&lt; 24) |
        ///             ((long)(f & 0xff) &lt;&lt; 16) |
        ///             ((long)(g & 0xff) &lt;&lt;  8) |
        ///             ((long)(h & 0xff)))
        ///         </c>
        ///     </pre>
        ///     <p>
        ///         This method is suitable
        ///         for reading bytes written by the
        ///         <c>writeLong</c>
        ///         method of interface
        ///         <c>DataOutput</c>
        ///         .
        /// </summary>
        /// <returns>
        ///     the
        ///     <c>long</c>
        ///     value read.
        /// </returns>
        /// <exception>
        ///     EOFException
        ///     if this stream reaches the end before reading
        ///     all the bytes.
        /// </exception>
        /// <exception>
        ///     IOException
        ///     if an I/O error occurs.
        /// </exception>
        /// <exception cref="IOException" />
        long ReadLong();

        /// <summary>
        ///     Reads four input bytes and returns
        ///     a
        ///     <c>float</c>
        ///     value. It does this
        ///     by first constructing an
        ///     <c>int</c>
        ///     value in exactly the manner
        ///     of the
        ///     <c>readInt</c>
        ///     method, then converting this
        ///     <c>int</c>
        ///     value to a
        ///     <c>float</c>
        ///     in
        ///     exactly the manner of the method
        ///     <c>BitConverter.Int32BitsToSingle</c>
        ///     .
        ///     This method is suitable for reading
        ///     bytes written by the
        ///     <c>writeFloat</c>
        ///     method of interface
        ///     <c>DataOutput</c>
        ///     .
        /// </summary>
        /// <returns>
        ///     the
        ///     <c>float</c>
        ///     value read.
        /// </returns>
        /// <exception>
        ///     EOFException
        ///     if this stream reaches the end before reading
        ///     all the bytes.
        /// </exception>
        /// <exception>
        ///     IOException
        ///     if an I/O error occurs.
        /// </exception>
        /// <exception cref="IOException" />
        float ReadFloat();

        /// <summary>
        ///     Reads eight input bytes and returns
        ///     a
        ///     <c>double</c>
        ///     value. It does this
        ///     by first constructing a
        ///     <c>long</c>
        ///     value in exactly the manner
        ///     of the
        ///     <c>readLong</c>
        ///     method, then converting this
        ///     <c>long</c>
        ///     value to a
        ///     <c>double</c>
        ///     in exactly
        ///     the manner of the method
        ///     <c>BitConverter.Int64BitsToDouble</c>
        ///     .
        ///     This method is suitable for reading
        ///     bytes written by the
        ///     <c>writeDouble</c>
        ///     method of interface
        ///     <c>DataOutput</c>
        ///     .
        /// </summary>
        /// <returns>
        ///     the
        ///     <c>double</c>
        ///     value read.
        /// </returns>
        /// <exception>
        ///     EOFException
        ///     if this stream reaches the end before reading
        ///     all the bytes.
        /// </exception>
        /// <exception>
        ///     IOException
        ///     if an I/O error occurs.
        /// </exception>
        /// <exception cref="IOException" />
        double ReadDouble();

        /// <summary>
        ///     Reads in a string that has been encoded using a
        ///     <a href="#modified-utf-8">modified UTF-8</a>
        ///     format.
        /// </summary>
        /// <remarks>
        ///     Reads in a string that has been encoded using a
        ///     <a href="#modified-utf-8">modified UTF-8</a>
        ///     format.
        ///     The general contract of
        ///     <c>readUTF</c>
        ///     is that it reads a representation of a Unicode
        ///     character string encoded in modified
        ///     UTF-8 format; this string of characters
        ///     is then returned as a
        ///     <c>String</c>
        ///     .
        ///     <p>
        ///         First, two bytes are read and used to
        ///         construct an unsigned 16-bit integer in
        ///         exactly the manner of the
        ///         <c>readUnsignedShort</c>
        ///         method . This integer value is called the
        ///         <i>UTF length</i> and specifies the number
        ///         of additional bytes to be read. These bytes
        ///         are then converted to characters by considering
        ///         them in groups. The length of each group
        ///         is computed from the value of the first
        ///         byte of the group. The byte following a
        ///         group, if any, is the first byte of the
        ///         next group.
        ///         <p>
        ///             If the first byte of a group
        ///             matches the bit pattern
        ///             <c>0xxxxxxx</c>
        ///             (where
        ///             <c>x</c>
        ///             means "may be
        ///             <c>0</c>
        ///             or
        ///             <c>1</c>
        ///             "), then the group consists
        ///             of just that byte. The byte is zero-extended
        ///             to form a character.
        ///             <p>
        ///                 If the first byte
        ///                 of a group matches the bit pattern
        ///                 <c>110xxxxx</c>
        ///                 ,
        ///                 then the group consists of that byte
        ///                 <c>a</c>
        ///                 and a second byte
        ///                 <c>b</c>
        ///                 . If there
        ///                 is no byte
        ///                 <c>b</c>
        ///                 (because byte
        ///                 <c>a</c>
        ///                 was the last of the bytes
        ///                 to be read), or if byte
        ///                 <c>b</c>
        ///                 does
        ///                 not match the bit pattern
        ///                 <c>10xxxxxx</c>
        ///                 ,
        ///                 then a
        ///                 <c>UTFDataFormatException</c>
        ///                 is thrown. Otherwise, the group is converted
        ///                 to the character:
        ///                 <pre>
        ///                     <c>(char)(((a & 0x1F) &lt;&lt; 6) | (b & 0x3F))</c>
        ///                 </pre>
        ///                 If the first byte of a group
        ///                 matches the bit pattern
        ///                 <c>1110xxxx</c>
        ///                 ,
        ///                 then the group consists of that byte
        ///                 <c>a</c>
        ///                 and two more bytes
        ///                 <c>b</c>
        ///                 and
        ///                 <c>c</c>
        ///                 .
        ///                 If there is no byte
        ///                 <c>c</c>
        ///                 (because
        ///                 byte
        ///                 <c>a</c>
        ///                 was one of the last
        ///                 two of the bytes to be read), or either
        ///                 byte
        ///                 <c>b</c>
        ///                 or byte
        ///                 <c>c</c>
        ///                 does not match the bit pattern
        ///                 <c>10xxxxxx</c>
        ///                 ,
        ///                 then a
        ///                 <c>UTFDataFormatException</c>
        ///                 is thrown. Otherwise, the group is converted
        ///                 to the character:
        ///                 <pre>
        ///                     <c>(char)(((a & 0x0F) &lt;&lt; 12) | ((b & 0x3F) &lt;&lt; 6) | (c & 0x3F))</c>
        ///                 </pre>
        ///                 If the first byte of a group matches the
        ///                 pattern
        ///                 <c>1111xxxx</c>
        ///                 or the pattern
        ///                 <c>10xxxxxx</c>
        ///                 , then a
        ///                 <c>UTFDataFormatException</c>
        ///                 is thrown.
        ///                 <p>
        ///                     If end of file is encountered
        ///                     at any time during this entire process,
        ///                     then an
        ///                     <c>EOFException</c>
        ///                     is thrown.
        ///                     <p>
        ///                         After every group has been converted to
        ///                         a character by this process, the characters
        ///                         are gathered, in the same order in which
        ///                         their corresponding groups were read from
        ///                         the input stream, to form a
        ///                         <c>String</c>
        ///                         ,
        ///                         which is returned.
        ///                         <p>
        ///                             The
        ///                             <c>writeUTF</c>
        ///                             method of interface
        ///                             <c>DataOutput</c>
        ///                             may be used to write data that is suitable
        ///                             for reading by this method.
        /// </remarks>
        /// <returns>a Unicode string.</returns>
        /// <exception>
        ///     EOFException
        ///     if this stream reaches the end
        ///     before reading all the bytes.
        /// </exception>
        /// <exception>
        ///     IOException
        ///     if an I/O error occurs.
        /// </exception>
        /// <exception>
        ///     UTFDataFormatException
        ///     if the bytes do not represent a
        ///     valid modified UTF-8 encoding of a string.
        /// </exception>
        /// <exception cref="IOException" />
        string ReadUtf();
    }
}