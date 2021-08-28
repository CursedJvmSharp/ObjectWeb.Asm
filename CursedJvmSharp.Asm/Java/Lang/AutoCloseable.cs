/*
* Copyright (c) 2009, 2013, Oracle and/or its affiliates. All rights reserved.
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
using Java.IO;

namespace Java.Lang
{
	/// <summary>
	///     An object that may hold resources (such as file or socket handles)
	///     until it is closed.
	/// </summary>
	/// <remarks>
	///     An object that may hold resources (such as file or socket handles)
	///     until it is closed. The
	///     <see cref="Close()" />
	///     method of an
	///     <c>AutoCloseable</c>
	///     object is called automatically when exiting a
	///     <c>try</c>
	///     -with-resources block for which the object has been declared in
	///     the resource specification header. This construction ensures prompt
	///     release, avoiding resource exhaustion exceptions and errors that
	///     may otherwise occur.
	/// </remarks>
	/// <apiNote>
	///     <p>
	///         It is possible, and in fact common, for a base class to
	///         implement AutoCloseable even though not all of its subclasses or
	///         instances will hold releasable resources.  For code that must operate
	///         in complete generality, or when it is known that the
	///         <c>AutoCloseable</c>
	///         instance requires resource release, it is recommended to use
	///         <c>try</c>
	///         -with-resources constructions. However, when using facilities such as
	///         <see cref="Java.Util.Stream.Stream{T}" />
	///         that support both I/O-based and
	///         non-I/O-based forms,
	///         <c>try</c>
	///         -with-resources blocks are in
	///         general unnecessary when using non-I/O-based forms.
	/// </apiNote>
	/// <author>Josh Bloch</author>
	/// <since>1.7</since>
	public interface AutoCloseable : IDisposable
    {
	    /// <summary>Closes this resource, relinquishing any underlying resources.</summary>
	    /// <remarks>
	    ///     Closes this resource, relinquishing any underlying resources.
	    ///     This method is invoked automatically on objects managed by the
	    ///     <c>try</c>
	    ///     -with-resources statement.
	    ///     <p>
	    ///         While this interface method is declared to throw
	    ///         <c>Exception</c>
	    ///         , implementers are <em>strongly</em> encouraged to
	    ///         declare concrete implementations of the
	    ///         <c>close</c>
	    ///         method to
	    ///         throw more specific exceptions, or to throw no exception at all
	    ///         if the close operation cannot fail.
	    ///         <p>
	    ///             Cases where the close operation may fail require careful
	    ///             attention by implementers. It is strongly advised to relinquish
	    ///             the underlying resources and to internally <em>mark</em> the
	    ///             resource as closed, prior to throwing the exception. The
	    ///             <c>close</c>
	    ///             method is unlikely to be invoked more than once and so
	    ///             this ensures that the resources are released in a timely manner.
	    ///             Furthermore it reduces problems that could arise when the resource
	    ///             wraps, or is wrapped, by another resource.
	    ///             <p>
	    ///                 <em>
	    ///                     Implementers of this interface are also strongly advised
	    ///                     to not have the
	    ///                     <c>close</c>
	    ///                     method throw
	    ///                     <see cref="Exception" />
	    ///                     .
	    ///                 </em>
	    ///                 This exception interacts with a thread's interrupted status,
	    ///                 and runtime misbehavior is likely to occur if an
	    ///                 <c>InterruptedException</c>
	    ///                 is
	    ///                 <linkplain>
	    ///                     Throwable#addSuppressed
	    ///                     suppressed
	    ///                 </linkplain>
	    ///                 .
	    ///                 More generally, if it would cause problems for an
	    ///                 exception to be suppressed, the
	    ///                 <c>AutoCloseable.close</c>
	    ///                 method should not throw it.
	    ///                 <p>
	    ///                     Note that unlike the
	    ///                     <see cref="Closeable.Close">close</see>
	    ///                     method of
	    ///                     <see cref="Closeable" />
	    ///                     , this
	    ///                     <c>close</c>
	    ///                     method
	    ///                     is <em>not</em> required to be idempotent.  In other words,
	    ///                     calling this
	    ///                     <c>close</c>
	    ///                     method more than once may have some
	    ///                     visible side effect, unlike
	    ///                     <c>Closeable.close</c>
	    ///                     which is
	    ///                     required to have no effect if called more than once.
	    ///                     However, implementers of this interface are strongly encouraged
	    ///                     to make their
	    ///                     <c>close</c>
	    ///                     methods idempotent.
	    /// </remarks>
	    /// <exception cref="Exception">if this resource cannot be closed</exception>
	    /// <exception cref="Exception" />
	    void Close();
    }
}