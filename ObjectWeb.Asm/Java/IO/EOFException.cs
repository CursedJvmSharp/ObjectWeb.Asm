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

using System;
using System.IO;

namespace Java.IO
{
    /// <summary>
    ///     Signals that an end of file or end of stream has been reached
    ///     unexpectedly during input.
    /// </summary>
    /// <remarks>
    ///     Signals that an end of file or end of stream has been reached
    ///     unexpectedly during input.
    ///     <p>
    ///         This exception is mainly used by data input streams to signal end of
    ///         stream. Note that many other input operations return a special value on
    ///         end of stream rather than throwing an exception.
    /// </remarks>
    /// <author>Frank Yellin</author>
    /// <seealso cref="DataInputStream" />
    /// <seealso cref="IOException" />
    /// <since>JDK1.0</since>
    [Serializable]
    public class EofException : IOException
    {
        private const long SerialVersionUid = 6433858223774886977L;

        /// <summary>
        ///     Constructs an <code>EOFException</code> with <code>null</code>
        ///     as its error detail message.
        /// </summary>
        public EofException()
        {
        }

        /// <summary>
        ///     Constructs an <code>EOFException</code> with the specified detail
        ///     message.
        /// </summary>
        /// <remarks>
        ///     Constructs an <code>EOFException</code> with the specified detail
        ///     message. The string <code>s</code> may later be retrieved by the
        ///     <code>
        /// <see cref="Exception.Message" />
        /// </code>
        ///     method of class
        ///     <code>java.lang.Throwable</code>.
        /// </remarks>
        /// <param name="s">the detail message.</param>
        public EofException(string s)
            : base(s)
        {
        }
    }
}