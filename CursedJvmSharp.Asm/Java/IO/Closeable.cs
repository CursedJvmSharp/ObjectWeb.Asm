/*
* Copyright (c) 2003, 2013, Oracle and/or its affiliates. All rights reserved.
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

using Java.Lang;

namespace Java.IO
{
    /// <summary>
    ///     A
    ///     <c>Closeable</c>
    ///     is a source or destination of data that can be closed.
    ///     The close method is invoked to release resources that the object is
    ///     holding (such as open files).
    /// </summary>
    /// <since>1.5</since>
    public interface Closeable : AutoCloseable
    {
        /// <summary>
        ///     Closes this stream and releases any system resources associated
        ///     with it.
        /// </summary>
        /// <remarks>
        ///     Closes this stream and releases any system resources associated
        ///     with it. If the stream is already closed then invoking this
        ///     method has no effect.
        ///     <p>
        ///         As noted in
        ///         <see cref="Java.Lang.AutoCloseable.Close()" />
        ///         , cases where the
        ///         close may fail require careful attention. It is strongly advised
        ///         to relinquish the underlying resources and to internally
        ///         <em>mark</em> the
        ///         <c>Closeable</c>
        ///         as closed, prior to throwing
        ///         the
        ///         <c>IOException</c>
        ///         .
        /// </remarks>
        /// <exception cref="IOException">if an I/O error occurs</exception>
        /// <exception cref="System.IO.IOException" />
        new void Close();
    }
}