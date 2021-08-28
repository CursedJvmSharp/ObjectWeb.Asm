

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
	/// Information about an exception handler. Corresponds to an element of the exception_table array of
	/// a Code attribute, as defined in the Java Virtual Machine Specification (JVMS). Handler instances
	/// can be chained together, with their <seealso cref="nextHandler"/> field, to describe a full JVMS
	/// exception_table array.
	/// </summary>
	/// <seealso cref= <a href="https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.7.3">JVMS
	///     4.7.3</a>
	/// @author Eric Bruneton </seealso>
	internal sealed class Handler
	{

	  /// <summary>
	  /// The start_pc field of this JVMS exception_table entry. Corresponds to the beginning of the
	  /// exception handler's scope (inclusive).
	  /// </summary>
	  internal readonly Label startPc;

	  /// <summary>
	  /// The end_pc field of this JVMS exception_table entry. Corresponds to the end of the exception
	  /// handler's scope (exclusive).
	  /// </summary>
	  internal readonly Label endPc;

	  /// <summary>
	  /// The handler_pc field of this JVMS exception_table entry. Corresponding to the beginning of the
	  /// exception handler's code.
	  /// </summary>
	  internal readonly Label handlerPc;

	  /// <summary>
	  /// The catch_type field of this JVMS exception_table entry. This is the constant pool index of the
	  /// internal name of the type of exceptions handled by this handler, or 0 to catch any exceptions.
	  /// </summary>
	  internal readonly int catchType;

	  /// <summary>
	  /// The internal name of the type of exceptions handled by this handler, or {@literal null} to
	  /// catch any exceptions.
	  /// </summary>
	  internal readonly string catchTypeDescriptor;

	  /// <summary>
	  /// The next exception handler. </summary>
	  internal Handler nextHandler;

	  /// <summary>
	  /// Constructs a new Handler.
	  /// </summary>
	  /// <param name="startPc"> the start_pc field of this JVMS exception_table entry. </param>
	  /// <param name="endPc"> the end_pc field of this JVMS exception_table entry. </param>
	  /// <param name="handlerPc"> the handler_pc field of this JVMS exception_table entry. </param>
	  /// <param name="catchType"> The catch_type field of this JVMS exception_table entry. </param>
	  /// <param name="catchTypeDescriptor"> The internal name of the type of exceptions handled by this handler,
	  ///     or {@literal null} to catch any exceptions. </param>
	  public Handler(Label startPc, Label endPc, Label handlerPc, int catchType, string catchTypeDescriptor)
	  {
		this.startPc = startPc;
		this.endPc = endPc;
		this.handlerPc = handlerPc;
		this.catchType = catchType;
		this.catchTypeDescriptor = catchTypeDescriptor;
	  }

	  /// <summary>
	  /// Constructs a new Handler from the given one, with a different scope.
	  /// </summary>
	  /// <param name="handler"> an existing Handler. </param>
	  /// <param name="startPc"> the start_pc field of this JVMS exception_table entry. </param>
	  /// <param name="endPc"> the end_pc field of this JVMS exception_table entry. </param>
	  public Handler(Handler handler, Label startPc, Label endPc) : this(startPc, endPc, handler.handlerPc, handler.catchType, handler.catchTypeDescriptor)
	  {
		this.nextHandler = handler.nextHandler;
	  }

	  /// <summary>
	  /// Removes the range between start and end from the Handler list that begins with the given
	  /// element.
	  /// </summary>
	  /// <param name="firstHandler"> the beginning of a Handler list. May be {@literal null}. </param>
	  /// <param name="start"> the start of the range to be removed. </param>
	  /// <param name="end"> the end of the range to be removed. Maybe {@literal null}. </param>
	  /// <returns> the exception handler list with the start-end range removed. </returns>
	  internal static Handler RemoveRange(Handler firstHandler, Label start, Label end)
	  {
		if (firstHandler == null)
		{
		  return null;
		}
		else
		{
		  firstHandler.nextHandler = RemoveRange(firstHandler.nextHandler, start, end);
		}
		int handlerStart = firstHandler.startPc.bytecodeOffset;
		int handlerEnd = firstHandler.endPc.bytecodeOffset;
		int rangeStart = start.bytecodeOffset;
		int rangeEnd = end == null ? int.MaxValue : end.bytecodeOffset;
		// Return early if [handlerStart,handlerEnd[ and [rangeStart,rangeEnd[ don't intersect.
		if (rangeStart >= handlerEnd || rangeEnd <= handlerStart)
		{
		  return firstHandler;
		}
		if (rangeStart <= handlerStart)
		{
		  if (rangeEnd >= handlerEnd)
		  {
			// If [handlerStart,handlerEnd[ is included in [rangeStart,rangeEnd[, remove firstHandler.
			return firstHandler.nextHandler;
		  }
		  else
		  {
			// [handlerStart,handlerEnd[ - [rangeStart,rangeEnd[ = [rangeEnd,handlerEnd[
			return new Handler(firstHandler, end, firstHandler.endPc);
		  }
		}
		else if (rangeEnd >= handlerEnd)
		{
		  // [handlerStart,handlerEnd[ - [rangeStart,rangeEnd[ = [handlerStart,rangeStart[
		  return new Handler(firstHandler, firstHandler.startPc, start);
		}
		else
		{
		  // [handlerStart,handlerEnd[ - [rangeStart,rangeEnd[ =
		  //     [handlerStart,rangeStart[ + [rangeEnd,handerEnd[
		  firstHandler.nextHandler = new Handler(firstHandler, end, firstHandler.endPc);
		  return new Handler(firstHandler, firstHandler.startPc, start);
		}
	  }

	  /// <summary>
	  /// Returns the number of elements of the Handler list that begins with the given element.
	  /// </summary>
	  /// <param name="firstHandler"> the beginning of a Handler list. May be {@literal null}. </param>
	  /// <returns> the number of elements of the Handler list that begins with 'handler'. </returns>
	  internal static int GetExceptionTableLength(Handler firstHandler)
	  {
		int length = 0;
		Handler handler = firstHandler;
		while (handler != null)
		{
		  length++;
		  handler = handler.nextHandler;
		}
		return length;
	  }

	  /// <summary>
	  /// Returns the size in bytes of the JVMS exception_table corresponding to the Handler list that
	  /// begins with the given element. <i>This includes the exception_table_length field.</i>
	  /// </summary>
	  /// <param name="firstHandler"> the beginning of a Handler list. May be {@literal null}. </param>
	  /// <returns> the size in bytes of the exception_table_length and exception_table structures. </returns>
	  internal static int GetExceptionTableSize(Handler firstHandler)
	  {
		return 2 + 8 * GetExceptionTableLength(firstHandler);
	  }

	  /// <summary>
	  /// Puts the JVMS exception_table corresponding to the Handler list that begins with the given
	  /// element. <i>This includes the exception_table_length field.</i>
	  /// </summary>
	  /// <param name="firstHandler"> the beginning of a Handler list. May be {@literal null}. </param>
	  /// <param name="output"> where the exception_table_length and exception_table structures must be put. </param>
	  internal static void PutExceptionTable(Handler firstHandler, ByteVector output)
	  {
		output.PutShort(GetExceptionTableLength(firstHandler));
		Handler handler = firstHandler;
		while (handler != null)
		{
		  output.PutShort(handler.startPc.bytecodeOffset).PutShort(handler.endPc.bytecodeOffset).PutShort(handler.handlerPc.bytecodeOffset).PutShort(handler.catchType);
		  handler = handler.nextHandler;
		}
	  }
	}

}