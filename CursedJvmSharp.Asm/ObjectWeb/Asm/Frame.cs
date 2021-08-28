using System;
using System.Text;

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
	/// The input and output stack map frames of a basic block.
	/// 
	/// <para>Stack map frames are computed in two steps:
	/// 
	/// <ul>
	///   <li>During the visit of each instruction in MethodWriter, the state of the frame at the end of
	///       the current basic block is updated by simulating the action of the instruction on the
	///       previous state of this so called "output frame".
	///   <li>After all instructions have been visited, a fix point algorithm is used in MethodWriter to
	///       compute the "input frame" of each basic block (i.e. the stack map frame at the beginning of
	///       the basic block). See <seealso cref="MethodWriter.computeAllFrames"/>.
	/// </ul>
	/// 
	/// </para>
	/// <para>Output stack map frames are computed relatively to the input frame of the basic block, which
	/// is not yet known when output frames are computed. It is therefore necessary to be able to
	/// represent abstract types such as "the type at position x in the input frame locals" or "the type
	/// at position x from the top of the input frame stack" or even "the type at position x in the input
	/// frame, with y more (or less) array dimensions". This explains the rather complicated type format
	/// used in this class, explained below.
	/// 
	/// </para>
	/// <para>The local variables and the operand stack of input and output frames contain values called
	/// "abstract types" hereafter. An abstract type is represented with 4 fields named DIM, KIND, FLAGS
	/// and VALUE, packed in a single int value for better performance and memory efficiency:
	/// 
	/// <pre>
	///   =====================================
	///   |...DIM|KIND|.F|...............VALUE|
	///   =====================================
	/// </pre>
	/// 
	/// <ul>
	///   <li>the DIM field, stored in the 6 most significant bits, is a signed number of array
	///       dimensions (from -32 to 31, included). It can be retrieved with <seealso cref="DIM_MASK"/> and a
	///       right shift of <seealso cref="DIM_SHIFT"/>.
	///   <li>the KIND field, stored in 4 bits, indicates the kind of VALUE used. These 4 bits can be
	///       retrieved with <seealso cref="KIND_MASK"/> and, without any shift, must be equal to {@link
	///       #CONSTANT_KIND}, <seealso cref="REFERENCE_KIND"/>, <seealso cref="UNINITIALIZED_KIND"/>, <seealso cref="LOCAL_KIND"/>
	///       or <seealso cref="STACK_KIND"/>.
	///   <li>the FLAGS field, stored in 2 bits, contains up to 2 boolean flags. Currently only one flag
	///       is defined, namely <seealso cref="TOP_IF_LONG_OR_DOUBLE_FLAG"/>.
	///   <li>the VALUE field, stored in the remaining 20 bits, contains either
	///       <ul>
	///         <li>one of the constants <seealso cref="ITEM_TOP"/>, <seealso cref="ITEM_ASM_BOOLEAN"/>, {@link
	///             #ITEM_ASM_BYTE}, <seealso cref="ITEM_ASM_CHAR"/> or <seealso cref="ITEM_ASM_SHORT"/>, {@link
	///             #ITEM_INTEGER}, <seealso cref="ITEM_FLOAT"/>, <seealso cref="ITEM_LONG"/>, <seealso cref="ITEM_DOUBLE"/>, {@link
	///             #ITEM_NULL} or <seealso cref="ITEM_UNINITIALIZED_THIS"/>, if KIND is equal to {@link
	///             #CONSTANT_KIND}.
	///         <li>the index of a <seealso cref="Symbol.TYPE_TAG"/> <seealso cref="Symbol"/> in the type table of a {@link
	///             SymbolTable}, if KIND is equal to <seealso cref="REFERENCE_KIND"/>.
	///         <li>the index of an <seealso cref="Symbol.UNINITIALIZED_TYPE_TAG"/> <seealso cref="Symbol"/> in the type
	///             table of a SymbolTable, if KIND is equal to <seealso cref="UNINITIALIZED_KIND"/>.
	///         <li>the index of a local variable in the input stack frame, if KIND is equal to {@link
	///             #LOCAL_KIND}.
	///         <li>a position relatively to the top of the stack of the input stack frame, if KIND is
	///             equal to <seealso cref="STACK_KIND"/>,
	///       </ul>
	/// </ul>
	/// 
	/// </para>
	/// <para>Output frames can contain abstract types of any kind and with a positive or negative array
	/// dimension (and even unassigned types, represented by 0 - which does not correspond to any valid
	/// abstract type value). Input frames can only contain CONSTANT_KIND, REFERENCE_KIND or
	/// UNINITIALIZED_KIND abstract types of positive or {@literal null} array dimension. In all cases
	/// the type table contains only internal type names (array type descriptors are forbidden - array
	/// dimensions must be represented through the DIM field).
	/// 
	/// </para>
	/// <para>The LONG and DOUBLE types are always represented by using two slots (LONG + TOP or DOUBLE +
	/// TOP), for local variables as well as in the operand stack. This is necessary to be able to
	/// simulate DUPx_y instructions, whose effect would be dependent on the concrete types represented
	/// by the abstract types in the stack (which are not always known).
	/// 
	/// @author Eric Bruneton
	/// </para>
	/// </summary>
	internal class Frame
	{

	  // Constants used in the StackMapTable attribute.
	  // See https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.7.4.

	  internal const int SAME_FRAME = 0;
	  internal const int SAME_LOCALS_1_STACK_ITEM_FRAME = 64;
	  internal const int RESERVED = 128;
	  internal const int SAME_LOCALS_1_STACK_ITEM_FRAME_EXTENDED = 247;
	  internal const int CHOP_FRAME = 248;
	  internal const int SAME_FRAME_EXTENDED = 251;
	  internal const int APPEND_FRAME = 252;
	  internal const int FULL_FRAME = 255;

	  internal const int ITEM_TOP = 0;
	  internal const int ITEM_INTEGER = 1;
	  internal const int ITEM_FLOAT = 2;
	  internal const int ITEM_DOUBLE = 3;
	  internal const int ITEM_LONG = 4;
	  internal const int ITEM_NULL = 5;
	  internal const int ITEM_UNINITIALIZED_THIS = 6;
	  internal const int ITEM_OBJECT = 7;
	  internal const int ITEM_UNINITIALIZED = 8;
	  // Additional, ASM specific constants used in abstract types below.
	  private const int ITEM_ASM_BOOLEAN = 9;
	  private const int ITEM_ASM_BYTE = 10;
	  private const int ITEM_ASM_CHAR = 11;
	  private const int ITEM_ASM_SHORT = 12;

	  // The size and offset in bits of each field of an abstract type.

	  private const int DIM_SIZE = 6;
	  private const int KIND_SIZE = 4;
	  private const int FLAGS_SIZE = 2;
	  private const int VALUE_SIZE = 32 - DIM_SIZE - KIND_SIZE - FLAGS_SIZE;

	  private const int DIM_SHIFT = KIND_SIZE + FLAGS_SIZE + VALUE_SIZE;
	  private const int KIND_SHIFT = FLAGS_SIZE + VALUE_SIZE;
	  private const int FLAGS_SHIFT = VALUE_SIZE;

	  // Bitmasks to get each field of an abstract type.

	  private const int DIM_MASK = ((1 << DIM_SIZE) - 1) << DIM_SHIFT;
	  private const int KIND_MASK = ((1 << KIND_SIZE) - 1) << KIND_SHIFT;
	  private const int VALUE_MASK = (1 << VALUE_SIZE) - 1;

	  // Constants to manipulate the DIM field of an abstract type.

	  /// <summary>
	  /// The constant to be added to an abstract type to get one with one more array dimension. </summary>
	  private const int ARRAY_OF = +1 << DIM_SHIFT;

	  /// <summary>
	  /// The constant to be added to an abstract type to get one with one less array dimension. </summary>
	  private const int ELEMENT_OF = -1 << DIM_SHIFT;

	  // Possible values for the KIND field of an abstract type.

	  private const int CONSTANT_KIND = 1 << KIND_SHIFT;
	  private const int REFERENCE_KIND = 2 << KIND_SHIFT;
	  private const int UNINITIALIZED_KIND = 3 << KIND_SHIFT;
	  private const int LOCAL_KIND = 4 << KIND_SHIFT;
	  private const int STACK_KIND = 5 << KIND_SHIFT;

	  // Possible flags for the FLAGS field of an abstract type.

	  /// <summary>
	  /// A flag used for LOCAL_KIND and STACK_KIND abstract types, indicating that if the resolved,
	  /// concrete type is LONG or DOUBLE, TOP should be used instead (because the value has been
	  /// partially overridden with an xSTORE instruction).
	  /// </summary>
	  private const int TOP_IF_LONG_OR_DOUBLE_FLAG = 1 << FLAGS_SHIFT;

	  // Useful predefined abstract types (all the possible CONSTANT_KIND types).

	  private const int TOP = CONSTANT_KIND | ITEM_TOP;
	  private const int BOOLEAN = CONSTANT_KIND | ITEM_ASM_BOOLEAN;
	  private const int BYTE = CONSTANT_KIND | ITEM_ASM_BYTE;
	  private const int CHAR = CONSTANT_KIND | ITEM_ASM_CHAR;
	  private const int SHORT = CONSTANT_KIND | ITEM_ASM_SHORT;
	  private const int INTEGER = CONSTANT_KIND | ITEM_INTEGER;
	  private const int FLOAT = CONSTANT_KIND | ITEM_FLOAT;
	  private const int LONG = CONSTANT_KIND | ITEM_LONG;
	  private const int DOUBLE = CONSTANT_KIND | ITEM_DOUBLE;
	  private const int NULL = CONSTANT_KIND | ITEM_NULL;
	  private const int UNINITIALIZED_THIS = CONSTANT_KIND | ITEM_UNINITIALIZED_THIS;

	  // -----------------------------------------------------------------------------------------------
	  // Instance fields
	  // -----------------------------------------------------------------------------------------------

	  /// <summary>
	  /// The basic block to which these input and output stack map frames correspond. </summary>
	  internal Label owner;

	  /// <summary>
	  /// The input stack map frame locals. This is an array of abstract types. </summary>
	  private int[] inputLocals;

	  /// <summary>
	  /// The input stack map frame stack. This is an array of abstract types. </summary>
	  private int[] inputStack;

	  /// <summary>
	  /// The output stack map frame locals. This is an array of abstract types. </summary>
	  private int[] outputLocals;

	  /// <summary>
	  /// The output stack map frame stack. This is an array of abstract types. </summary>
	  private int[] outputStack;

	  /// <summary>
	  /// The start of the output stack, relatively to the input stack. This offset is always negative or
	  /// null. A null offset means that the output stack must be appended to the input stack. A -n
	  /// offset means that the first n output stack elements must replace the top n input stack
	  /// elements, and that the other elements must be appended to the input stack.
	  /// </summary>
	  private short outputStackStart;

	  /// <summary>
	  /// The index of the top stack element in <seealso cref="outputStack"/>. </summary>
	  private short outputStackTop;

	  /// <summary>
	  /// The number of types that are initialized in the basic block. See <seealso cref="initializations"/>. </summary>
	  private int initializationCount;

	  /// <summary>
	  /// The abstract types that are initialized in the basic block. A constructor invocation on an
	  /// UNINITIALIZED or UNINITIALIZED_THIS abstract type must replace <i>every occurrence</i> of this
	  /// type in the local variables and in the operand stack. This cannot be done during the first step
	  /// of the algorithm since, during this step, the local variables and the operand stack types are
	  /// still abstract. It is therefore necessary to store the abstract types of the constructors which
	  /// are invoked in the basic block, in order to do this replacement during the second step of the
	  /// algorithm, where the frames are fully computed. Note that this array can contain abstract types
	  /// that are relative to the input locals or to the input stack.
	  /// </summary>
	  private int[] initializations;

	  // -----------------------------------------------------------------------------------------------
	  // Constructor
	  // -----------------------------------------------------------------------------------------------

	  /// <summary>
	  /// Constructs a new Frame.
	  /// </summary>
	  /// <param name="owner"> the basic block to which these input and output stack map frames correspond. </param>
	  public Frame(Label owner)
	  {
		this.owner = owner;
	  }

	  /// <summary>
	  /// Sets this frame to the value of the given frame.
	  /// 
	  /// <para>WARNING: after this method is called the two frames share the same data structures. It is
	  /// recommended to discard the given frame to avoid unexpected side effects.
	  /// 
	  /// </para>
	  /// </summary>
	  /// <param name="frame"> The new frame value. </param>
	  public void copyFrom(Frame frame)
	  {
		inputLocals = frame.inputLocals;
		inputStack = frame.inputStack;
		outputStackStart = 0;
		outputLocals = frame.outputLocals;
		outputStack = frame.outputStack;
		outputStackTop = frame.outputStackTop;
		initializationCount = frame.initializationCount;
		initializations = frame.initializations;
	  }

	  // -----------------------------------------------------------------------------------------------
	  // Static methods to get abstract types from other type formats
	  // -----------------------------------------------------------------------------------------------

	  /// <summary>
	  /// Returns the abstract type corresponding to the given public API frame element type.
	  /// </summary>
	  /// <param name="symbolTable"> the type table to use to lookup and store type <seealso cref="Symbol"/>. </param>
	  /// <param name="type"> a frame element type described using the same format as in {@link
	  ///     MethodVisitor#visitFrame}, i.e. either <seealso cref="Opcodes.TOP"/>, <seealso cref="Opcodes.INTEGER"/>, {@link
	  ///     Opcodes#FLOAT}, <seealso cref="Opcodes.LONG"/>, <seealso cref="Opcodes.DOUBLE"/>, <seealso cref="Opcodes.NULL"/>, or
	  ///     <seealso cref="Opcodes.UNINITIALIZED_THIS"/>, or the internal name of a class, or a Label designating
	  ///     a NEW instruction (for uninitialized types). </param>
	  /// <returns> the abstract type corresponding to the given frame element type. </returns>
	  internal static int getAbstractTypeFromApiFormat(SymbolTable symbolTable, object type)
	  {
		if (type is int)
		{
		  return CONSTANT_KIND | ((int?) type).Value;
		}
		else if (type is string)
		{
		  string descriptor = JType.getObjectType((string) type).Descriptor;
		  return getAbstractTypeFromDescriptor(symbolTable, descriptor, 0);
		}
		else
		{
		  return UNINITIALIZED_KIND | symbolTable.addUninitializedType("", ((Label) type).bytecodeOffset);
		}
	  }

	  /// <summary>
	  /// Returns the abstract type corresponding to the internal name of a class.
	  /// </summary>
	  /// <param name="symbolTable"> the type table to use to lookup and store type <seealso cref="Symbol"/>. </param>
	  /// <param name="internalName"> the internal name of a class. This must <i>not</i> be an array type
	  ///     descriptor. </param>
	  /// <returns> the abstract type value corresponding to the given internal name. </returns>
	  internal static int getAbstractTypeFromInternalName(SymbolTable symbolTable, string internalName)
	  {
		return REFERENCE_KIND | symbolTable.addType(internalName);
	  }

	  /// <summary>
	  /// Returns the abstract type corresponding to the given type descriptor.
	  /// </summary>
	  /// <param name="symbolTable"> the type table to use to lookup and store type <seealso cref="Symbol"/>. </param>
	  /// <param name="buffer"> a string ending with a type descriptor. </param>
	  /// <param name="offset"> the start offset of the type descriptor in buffer. </param>
	  /// <returns> the abstract type corresponding to the given type descriptor. </returns>
	  private static int getAbstractTypeFromDescriptor(SymbolTable symbolTable, string buffer, int offset)
	  {
		string internalName;
		switch (buffer[offset])
		{
		  case 'V':
			return 0;
		  case 'Z':
		  case 'C':
		  case 'B':
		  case 'S':
		  case 'I':
			return INTEGER;
		  case 'F':
			return FLOAT;
		  case 'J':
			return LONG;
		  case 'D':
			return DOUBLE;
		  case 'L':
			internalName = buffer.Substring(offset + 1, (buffer.Length - 1) - (offset + 1));
			return REFERENCE_KIND | symbolTable.addType(internalName);
		  case '[':
			int elementDescriptorOffset = offset + 1;
			while (buffer[elementDescriptorOffset] == '[')
			{
			  ++elementDescriptorOffset;
			}
			int typeValue;
			switch (buffer[elementDescriptorOffset])
			{
			  case 'Z':
				typeValue = BOOLEAN;
				break;
			  case 'C':
				typeValue = CHAR;
				break;
			  case 'B':
				typeValue = BYTE;
				break;
			  case 'S':
				typeValue = SHORT;
				break;
			  case 'I':
				typeValue = INTEGER;
				break;
			  case 'F':
				typeValue = FLOAT;
				break;
			  case 'J':
				typeValue = LONG;
				break;
			  case 'D':
				typeValue = DOUBLE;
				break;
			  case 'L':
				internalName = buffer.Substring(elementDescriptorOffset + 1, (buffer.Length - 1) - (elementDescriptorOffset + 1));
				typeValue = REFERENCE_KIND | symbolTable.addType(internalName);
				break;
			  default:
				throw new System.ArgumentException();
			}
			return ((elementDescriptorOffset - offset) << DIM_SHIFT) | typeValue;
		  default:
			throw new System.ArgumentException();
		}
	  }

	  // -----------------------------------------------------------------------------------------------
	  // Methods related to the input frame
	  // -----------------------------------------------------------------------------------------------

	  /// <summary>
	  /// Sets the input frame from the given method description. This method is used to initialize the
	  /// first frame of a method, which is implicit (i.e. not stored explicitly in the StackMapTable
	  /// attribute).
	  /// </summary>
	  /// <param name="symbolTable"> the type table to use to lookup and store type <seealso cref="Symbol"/>. </param>
	  /// <param name="access"> the method's access flags. </param>
	  /// <param name="descriptor"> the method descriptor. </param>
	  /// <param name="maxLocals"> the maximum number of local variables of the method. </param>
	  public void setInputFrameFromDescriptor(SymbolTable symbolTable, int access, string descriptor, int maxLocals)
	  {
		inputLocals = new int[maxLocals];
		inputStack = new int[0];
		int inputLocalIndex = 0;
		if ((access & Opcodes.ACC_STATIC) == 0)
		{
		  if ((access & Constants.ACC_CONSTRUCTOR) == 0)
		  {
			inputLocals[inputLocalIndex++] = REFERENCE_KIND | symbolTable.addType(symbolTable.ClassName);
		  }
		  else
		  {
			inputLocals[inputLocalIndex++] = UNINITIALIZED_THIS;
		  }
		}
		foreach (JType argumentType in JType.getArgumentTypes(descriptor))
		{
		  int abstractType = getAbstractTypeFromDescriptor(symbolTable, argumentType.Descriptor, 0);
		  inputLocals[inputLocalIndex++] = abstractType;
		  if (abstractType == LONG || abstractType == DOUBLE)
		  {
			inputLocals[inputLocalIndex++] = TOP;
		  }
		}
		while (inputLocalIndex < maxLocals)
		{
		  inputLocals[inputLocalIndex++] = TOP;
		}
	  }

	  /// <summary>
	  /// Sets the input frame from the given public API frame description.
	  /// </summary>
	  /// <param name="symbolTable"> the type table to use to lookup and store type <seealso cref="Symbol"/>. </param>
	  /// <param name="numLocal"> the number of local variables. </param>
	  /// <param name="local"> the local variable types, described using the same format as in {@link
	  ///     MethodVisitor#visitFrame}. </param>
	  /// <param name="numStack"> the number of operand stack elements. </param>
	  /// <param name="stack"> the operand stack types, described using the same format as in {@link
	  ///     MethodVisitor#visitFrame}. </param>
	  public void setInputFrameFromApiFormat(SymbolTable symbolTable, int numLocal, object[] local, int numStack, object[] stack)
	  {
		int inputLocalIndex = 0;
		for (int i = 0; i < numLocal; ++i)
		{
		  inputLocals[inputLocalIndex++] = getAbstractTypeFromApiFormat(symbolTable, local[i]);
		  if (Equals(local[i], Opcodes.LONG) || Equals(local[i], Opcodes.DOUBLE))
		  {
			inputLocals[inputLocalIndex++] = TOP;
		  }
		}
		while (inputLocalIndex < inputLocals.Length)
		{
		  inputLocals[inputLocalIndex++] = TOP;
		}
		int numStackTop = 0;
		for (int i = 0; i < numStack; ++i)
		{
		  if (Equals(stack[i], Opcodes.LONG) || Equals(stack[i], Opcodes.DOUBLE))
		  {
			++numStackTop;
		  }
		}
		inputStack = new int[numStack + numStackTop];
		int inputStackIndex = 0;
		for (int i = 0; i < numStack; ++i)
		{
		  inputStack[inputStackIndex++] = getAbstractTypeFromApiFormat(symbolTable, stack[i]);
		  if (Equals(stack[i], Opcodes.LONG) || Equals(stack[i], Opcodes.DOUBLE))
		  {
			inputStack[inputStackIndex++] = TOP;
		  }
		}
		outputStackTop = 0;
		initializationCount = 0;
	  }

	  public int InputStackSize
	  {
		  get
		  {
			return inputStack.Length;
		  }
	  }

	  // -----------------------------------------------------------------------------------------------
	  // Methods related to the output frame
	  // -----------------------------------------------------------------------------------------------

	  /// <summary>
	  /// Returns the abstract type stored at the given local variable index in the output frame.
	  /// </summary>
	  /// <param name="localIndex"> the index of the local variable whose value must be returned. </param>
	  /// <returns> the abstract type stored at the given local variable index in the output frame. </returns>
	  private int getLocal(int localIndex)
	  {
		if (outputLocals == null || localIndex >= outputLocals.Length)
		{
		  // If this local has never been assigned in this basic block, it is still equal to its value
		  // in the input frame.
		  return LOCAL_KIND | localIndex;
		}
		else
		{
		  int abstractType = outputLocals[localIndex];
		  if (abstractType == 0)
		  {
			// If this local has never been assigned in this basic block, so it is still equal to its
			// value in the input frame.
			abstractType = outputLocals[localIndex] = LOCAL_KIND | localIndex;
		  }
		  return abstractType;
		}
	  }

	  /// <summary>
	  /// Replaces the abstract type stored at the given local variable index in the output frame.
	  /// </summary>
	  /// <param name="localIndex"> the index of the output frame local variable that must be set. </param>
	  /// <param name="abstractType"> the value that must be set. </param>
	  private void setLocal(int localIndex, int abstractType)
	  {
		// Create and/or resize the output local variables array if necessary.
		if (outputLocals == null)
		{
		  outputLocals = new int[10];
		}
		int outputLocalsLength = outputLocals.Length;
		if (localIndex >= outputLocalsLength)
		{
		  int[] newOutputLocals = new int[Math.Max(localIndex + 1, 2 * outputLocalsLength)];
		  Array.Copy(outputLocals, 0, newOutputLocals, 0, outputLocalsLength);
		  outputLocals = newOutputLocals;
		}
		// Set the local variable.
		outputLocals[localIndex] = abstractType;
	  }

	  /// <summary>
	  /// Pushes the given abstract type on the output frame stack.
	  /// </summary>
	  /// <param name="abstractType"> an abstract type. </param>
	  private void push(int abstractType)
	  {
		// Create and/or resize the output stack array if necessary.
		if (outputStack == null)
		{
		  outputStack = new int[10];
		}
		int outputStackLength = outputStack.Length;
		if (outputStackTop >= outputStackLength)
		{
		  int[] newOutputStack = new int[Math.Max(outputStackTop + 1, 2 * outputStackLength)];
		  Array.Copy(outputStack, 0, newOutputStack, 0, outputStackLength);
		  outputStack = newOutputStack;
		}
		// Pushes the abstract type on the output stack.
		outputStack[outputStackTop++] = abstractType;
		// Updates the maximum size reached by the output stack, if needed (note that this size is
		// relative to the input stack size, which is not known yet).
		short outputStackSize = (short)(outputStackStart + outputStackTop);
		if (outputStackSize > owner.outputStackMax)
		{
		  owner.outputStackMax = outputStackSize;
		}
	  }

	  /// <summary>
	  /// Pushes the abstract type corresponding to the given descriptor on the output frame stack.
	  /// </summary>
	  /// <param name="symbolTable"> the type table to use to lookup and store type <seealso cref="Symbol"/>. </param>
	  /// <param name="descriptor"> a type or method descriptor (in which case its return type is pushed). </param>
	  private void push(SymbolTable symbolTable, string descriptor)
	  {
		int typeDescriptorOffset = descriptor[0] == '(' ? JType.getReturnTypeOffset(descriptor) : 0;
		int abstractType = getAbstractTypeFromDescriptor(symbolTable, descriptor, typeDescriptorOffset);
		if (abstractType != 0)
		{
		  push(abstractType);
		  if (abstractType == LONG || abstractType == DOUBLE)
		  {
			push(TOP);
		  }
		}
	  }

	  /// <summary>
	  /// Pops an abstract type from the output frame stack and returns its value.
	  /// </summary>
	  /// <returns> the abstract type that has been popped from the output frame stack. </returns>
	  private int pop()
	  {
		if (outputStackTop > 0)
		{
		  return outputStack[--outputStackTop];
		}
		else
		{
		  // If the output frame stack is empty, pop from the input stack.
		  return STACK_KIND | -(--outputStackStart);
		}
	  }

	  /// <summary>
	  /// Pops the given number of abstract types from the output frame stack.
	  /// </summary>
	  /// <param name="elements"> the number of abstract types that must be popped. </param>
	  private void pop(int elements)
	  {
		if (outputStackTop >= elements)
		{
		  outputStackTop -= (short)elements;
		}
		else
		{
		  // If the number of elements to be popped is greater than the number of elements in the output
		  // stack, clear it, and pop the remaining elements from the input stack.
		  outputStackStart -= (short)(elements - outputStackTop);
		  outputStackTop = 0;
		}
	  }

	  /// <summary>
	  /// Pops as many abstract types from the output frame stack as described by the given descriptor.
	  /// </summary>
	  /// <param name="descriptor"> a type or method descriptor (in which case its argument types are popped). </param>
	  private void pop(string descriptor)
	  {
		char firstDescriptorChar = descriptor[0];
		if (firstDescriptorChar == '(')
		{
		  pop((JType.getArgumentsAndReturnSizes(descriptor) >> 2) - 1);
		}
		else if (firstDescriptorChar == 'J' || firstDescriptorChar == 'D')
		{
		  pop(2);
		}
		else
		{
		  pop(1);
		}
	  }

	  // -----------------------------------------------------------------------------------------------
	  // Methods to handle uninitialized types
	  // -----------------------------------------------------------------------------------------------

	  /// <summary>
	  /// Adds an abstract type to the list of types on which a constructor is invoked in the basic
	  /// block.
	  /// </summary>
	  /// <param name="abstractType"> an abstract type on a which a constructor is invoked. </param>
	  private void addInitializedType(int abstractType)
	  {
		// Create and/or resize the initializations array if necessary.
		if (initializations == null)
		{
		  initializations = new int[2];
		}
		int initializationsLength = initializations.Length;
		if (initializationCount >= initializationsLength)
		{
		  int[] newInitializations = new int[Math.Max(initializationCount + 1, 2 * initializationsLength)];
		  Array.Copy(initializations, 0, newInitializations, 0, initializationsLength);
		  initializations = newInitializations;
		}
		// Store the abstract type.
		initializations[initializationCount++] = abstractType;
	  }

	  /// <summary>
	  /// Returns the "initialized" abstract type corresponding to the given abstract type.
	  /// </summary>
	  /// <param name="symbolTable"> the type table to use to lookup and store type <seealso cref="Symbol"/>. </param>
	  /// <param name="abstractType"> an abstract type. </param>
	  /// <returns> the REFERENCE_KIND abstract type corresponding to abstractType if it is
	  ///     UNINITIALIZED_THIS or an UNINITIALIZED_KIND abstract type for one of the types on which a
	  ///     constructor is invoked in the basic block. Otherwise returns abstractType. </returns>
	  private int getInitializedType(SymbolTable symbolTable, int abstractType)
	  {
		if (abstractType == UNINITIALIZED_THIS || (abstractType & (DIM_MASK | KIND_MASK)) == UNINITIALIZED_KIND)
		{
		  for (int i = 0; i < initializationCount; ++i)
		  {
			int initializedType = initializations[i];
			int dim = initializedType & DIM_MASK;
			int kind = initializedType & KIND_MASK;
			int value = initializedType & VALUE_MASK;
			if (kind == LOCAL_KIND)
			{
			  initializedType = dim + inputLocals[value];
			}
			else if (kind == STACK_KIND)
			{
			  initializedType = dim + inputStack[inputStack.Length - value];
			}
			if (abstractType == initializedType)
			{
			  if (abstractType == UNINITIALIZED_THIS)
			  {
				return REFERENCE_KIND | symbolTable.addType(symbolTable.ClassName);
			  }
			  else
			  {
				return REFERENCE_KIND | symbolTable.addType(symbolTable.getType(abstractType & VALUE_MASK).value);
			  }
			}
		  }
		}
		return abstractType;
	  }

	  // -----------------------------------------------------------------------------------------------
	  // Main method, to simulate the execution of each instruction on the output frame
	  // -----------------------------------------------------------------------------------------------

	  /// <summary>
	  /// Simulates the action of the given instruction on the output stack frame.
	  /// </summary>
	  /// <param name="opcode"> the opcode of the instruction. </param>
	  /// <param name="arg"> the numeric operand of the instruction, if any. </param>
	  /// <param name="argSymbol"> the Symbol operand of the instruction, if any. </param>
	  /// <param name="symbolTable"> the type table to use to lookup and store type <seealso cref="Symbol"/>. </param>
	  public virtual void execute(int opcode, int arg, Symbol argSymbol, SymbolTable symbolTable)
	  {
		// Abstract types popped from the stack or read from local variables.
		int abstractType1;
		int abstractType2;
		int abstractType3;
		int abstractType4;
		switch (opcode)
		{
		  case Opcodes.NOP:
		  case Opcodes.INEG:
		  case Opcodes.LNEG:
		  case Opcodes.FNEG:
		  case Opcodes.DNEG:
		  case Opcodes.I2B:
		  case Opcodes.I2C:
		  case Opcodes.I2S:
		  case Opcodes.GOTO:
		  case Opcodes.RETURN:
			break;
		  case Opcodes.ACONST_NULL:
			push(NULL);
			break;
		  case Opcodes.ICONST_M1:
		  case Opcodes.ICONST_0:
		  case Opcodes.ICONST_1:
		  case Opcodes.ICONST_2:
		  case Opcodes.ICONST_3:
		  case Opcodes.ICONST_4:
		  case Opcodes.ICONST_5:
		  case Opcodes.BIPUSH:
		  case Opcodes.SIPUSH:
		  case Opcodes.ILOAD:
			push(INTEGER);
			break;
		  case Opcodes.LCONST_0:
		  case Opcodes.LCONST_1:
		  case Opcodes.LLOAD:
			push(LONG);
			push(TOP);
			break;
		  case Opcodes.FCONST_0:
		  case Opcodes.FCONST_1:
		  case Opcodes.FCONST_2:
		  case Opcodes.FLOAD:
			push(FLOAT);
			break;
		  case Opcodes.DCONST_0:
		  case Opcodes.DCONST_1:
		  case Opcodes.DLOAD:
			push(DOUBLE);
			push(TOP);
			break;
		  case Opcodes.LDC:
			switch (argSymbol.tag)
			{
			  case Symbol.CONSTANT_INTEGER_TAG:
				push(INTEGER);
				break;
			  case Symbol.CONSTANT_LONG_TAG:
				push(LONG);
				push(TOP);
				break;
			  case Symbol.CONSTANT_FLOAT_TAG:
				push(FLOAT);
				break;
			  case Symbol.CONSTANT_DOUBLE_TAG:
				push(DOUBLE);
				push(TOP);
				break;
			  case Symbol.CONSTANT_CLASS_TAG:
				push(REFERENCE_KIND | symbolTable.addType("java/lang/Class"));
				break;
			  case Symbol.CONSTANT_STRING_TAG:
				push(REFERENCE_KIND | symbolTable.addType("java/lang/String"));
				break;
			  case Symbol.CONSTANT_METHOD_TYPE_TAG:
				push(REFERENCE_KIND | symbolTable.addType("java/lang/invoke/MethodType"));
				break;
			  case Symbol.CONSTANT_METHOD_HANDLE_TAG:
				push(REFERENCE_KIND | symbolTable.addType("java/lang/invoke/MethodHandle"));
				break;
			  case Symbol.CONSTANT_DYNAMIC_TAG:
				push(symbolTable, argSymbol.value);
				break;
			  default:
				throw new ("AssertionError");
			}
			break;
		  case Opcodes.ALOAD:
			push(getLocal(arg));
			break;
		  case Opcodes.LALOAD:
		  case Opcodes.D2L:
			pop(2);
			push(LONG);
			push(TOP);
			break;
		  case Opcodes.DALOAD:
		  case Opcodes.L2D:
			pop(2);
			push(DOUBLE);
			push(TOP);
			break;
		  case Opcodes.AALOAD:
			pop(1);
			abstractType1 = pop();
			push(abstractType1 == NULL ? abstractType1 : ELEMENT_OF + abstractType1);
			break;
		  case Opcodes.ISTORE:
		  case Opcodes.FSTORE:
		  case Opcodes.ASTORE:
			abstractType1 = pop();
			setLocal(arg, abstractType1);
			if (arg > 0)
			{
			  int previousLocalType = getLocal(arg - 1);
			  if (previousLocalType == LONG || previousLocalType == DOUBLE)
			  {
				setLocal(arg - 1, TOP);
			  }
			  else if ((previousLocalType & KIND_MASK) == LOCAL_KIND || (previousLocalType & KIND_MASK) == STACK_KIND)
			  {
				// The type of the previous local variable is not known yet, but if it later appears
				// to be LONG or DOUBLE, we should then use TOP instead.
				setLocal(arg - 1, previousLocalType | TOP_IF_LONG_OR_DOUBLE_FLAG);
			  }
			}
			break;
		  case Opcodes.LSTORE:
		  case Opcodes.DSTORE:
			pop(1);
			abstractType1 = pop();
			setLocal(arg, abstractType1);
			setLocal(arg + 1, TOP);
			if (arg > 0)
			{
			  int previousLocalType = getLocal(arg - 1);
			  if (previousLocalType == LONG || previousLocalType == DOUBLE)
			  {
				setLocal(arg - 1, TOP);
			  }
			  else if ((previousLocalType & KIND_MASK) == LOCAL_KIND || (previousLocalType & KIND_MASK) == STACK_KIND)
			  {
				// The type of the previous local variable is not known yet, but if it later appears
				// to be LONG or DOUBLE, we should then use TOP instead.
				setLocal(arg - 1, previousLocalType | TOP_IF_LONG_OR_DOUBLE_FLAG);
			  }
			}
			break;
		  case Opcodes.IASTORE:
		  case Opcodes.BASTORE:
		  case Opcodes.CASTORE:
		  case Opcodes.SASTORE:
		  case Opcodes.FASTORE:
		  case Opcodes.AASTORE:
			pop(3);
			break;
		  case Opcodes.LASTORE:
		  case Opcodes.DASTORE:
			pop(4);
			break;
		  case Opcodes.POP:
		  case Opcodes.IFEQ:
		  case Opcodes.IFNE:
		  case Opcodes.IFLT:
		  case Opcodes.IFGE:
		  case Opcodes.IFGT:
		  case Opcodes.IFLE:
		  case Opcodes.IRETURN:
		  case Opcodes.FRETURN:
		  case Opcodes.ARETURN:
		  case Opcodes.TABLESWITCH:
		  case Opcodes.LOOKUPSWITCH:
		  case Opcodes.ATHROW:
		  case Opcodes.MONITORENTER:
		  case Opcodes.MONITOREXIT:
		  case Opcodes.IFNULL:
		  case Opcodes.IFNONNULL:
			pop(1);
			break;
		  case Opcodes.POP2:
		  case Opcodes.IF_ICMPEQ:
		  case Opcodes.IF_ICMPNE:
		  case Opcodes.IF_ICMPLT:
		  case Opcodes.IF_ICMPGE:
		  case Opcodes.IF_ICMPGT:
		  case Opcodes.IF_ICMPLE:
		  case Opcodes.IF_ACMPEQ:
		  case Opcodes.IF_ACMPNE:
		  case Opcodes.LRETURN:
		  case Opcodes.DRETURN:
			pop(2);
			break;
		  case Opcodes.DUP:
			abstractType1 = pop();
			push(abstractType1);
			push(abstractType1);
			break;
		  case Opcodes.DUP_X1:
			abstractType1 = pop();
			abstractType2 = pop();
			push(abstractType1);
			push(abstractType2);
			push(abstractType1);
			break;
		  case Opcodes.DUP_X2:
			abstractType1 = pop();
			abstractType2 = pop();
			abstractType3 = pop();
			push(abstractType1);
			push(abstractType3);
			push(abstractType2);
			push(abstractType1);
			break;
		  case Opcodes.DUP2:
			abstractType1 = pop();
			abstractType2 = pop();
			push(abstractType2);
			push(abstractType1);
			push(abstractType2);
			push(abstractType1);
			break;
		  case Opcodes.DUP2_X1:
			abstractType1 = pop();
			abstractType2 = pop();
			abstractType3 = pop();
			push(abstractType2);
			push(abstractType1);
			push(abstractType3);
			push(abstractType2);
			push(abstractType1);
			break;
		  case Opcodes.DUP2_X2:
			abstractType1 = pop();
			abstractType2 = pop();
			abstractType3 = pop();
			abstractType4 = pop();
			push(abstractType2);
			push(abstractType1);
			push(abstractType4);
			push(abstractType3);
			push(abstractType2);
			push(abstractType1);
			break;
		  case Opcodes.SWAP:
			abstractType1 = pop();
			abstractType2 = pop();
			push(abstractType1);
			push(abstractType2);
			break;
		  case Opcodes.IALOAD:
		  case Opcodes.BALOAD:
		  case Opcodes.CALOAD:
		  case Opcodes.SALOAD:
		  case Opcodes.IADD:
		  case Opcodes.ISUB:
		  case Opcodes.IMUL:
		  case Opcodes.IDIV:
		  case Opcodes.IREM:
		  case Opcodes.IAND:
		  case Opcodes.IOR:
		  case Opcodes.IXOR:
		  case Opcodes.ISHL:
		  case Opcodes.ISHR:
		  case Opcodes.IUSHR:
		  case Opcodes.L2I:
		  case Opcodes.D2I:
		  case Opcodes.FCMPL:
		  case Opcodes.FCMPG:
			pop(2);
			push(INTEGER);
			break;
		  case Opcodes.LADD:
		  case Opcodes.LSUB:
		  case Opcodes.LMUL:
		  case Opcodes.LDIV:
		  case Opcodes.LREM:
		  case Opcodes.LAND:
		  case Opcodes.LOR:
		  case Opcodes.LXOR:
			pop(4);
			push(LONG);
			push(TOP);
			break;
		  case Opcodes.FALOAD:
		  case Opcodes.FADD:
		  case Opcodes.FSUB:
		  case Opcodes.FMUL:
		  case Opcodes.FDIV:
		  case Opcodes.FREM:
		  case Opcodes.L2F:
		  case Opcodes.D2F:
			pop(2);
			push(FLOAT);
			break;
		  case Opcodes.DADD:
		  case Opcodes.DSUB:
		  case Opcodes.DMUL:
		  case Opcodes.DDIV:
		  case Opcodes.DREM:
			pop(4);
			push(DOUBLE);
			push(TOP);
			break;
		  case Opcodes.LSHL:
		  case Opcodes.LSHR:
		  case Opcodes.LUSHR:
			pop(3);
			push(LONG);
			push(TOP);
			break;
		  case Opcodes.IINC:
			setLocal(arg, INTEGER);
			break;
		  case Opcodes.I2L:
		  case Opcodes.F2L:
			pop(1);
			push(LONG);
			push(TOP);
			break;
		  case Opcodes.I2F:
			pop(1);
			push(FLOAT);
			break;
		  case Opcodes.I2D:
		  case Opcodes.F2D:
			pop(1);
			push(DOUBLE);
			push(TOP);
			break;
		  case Opcodes.F2I:
		  case Opcodes.ARRAYLENGTH:
		  case Opcodes.INSTANCEOF:
			pop(1);
			push(INTEGER);
			break;
		  case Opcodes.LCMP:
		  case Opcodes.DCMPL:
		  case Opcodes.DCMPG:
			pop(4);
			push(INTEGER);
			break;
		  case Opcodes.JSR:
		  case Opcodes.RET:
			throw new System.ArgumentException("JSR/RET are not supported with computeFrames option");
		  case Opcodes.GETSTATIC:
			push(symbolTable, argSymbol.value);
			break;
		  case Opcodes.PUTSTATIC:
			pop(argSymbol.value);
			break;
		  case Opcodes.GETFIELD:
			pop(1);
			push(symbolTable, argSymbol.value);
			break;
		  case Opcodes.PUTFIELD:
			pop(argSymbol.value);
			pop();
			break;
		  case Opcodes.INVOKEVIRTUAL:
		  case Opcodes.INVOKESPECIAL:
		  case Opcodes.INVOKESTATIC:
		  case Opcodes.INVOKEINTERFACE:
			pop(argSymbol.value);
			if (opcode != Opcodes.INVOKESTATIC)
			{
			  abstractType1 = pop();
			  if (opcode == Opcodes.INVOKESPECIAL && argSymbol.name[0] == '<')
			  {
				addInitializedType(abstractType1);
			  }
			}
			push(symbolTable, argSymbol.value);
			break;
		  case Opcodes.INVOKEDYNAMIC:
			pop(argSymbol.value);
			push(symbolTable, argSymbol.value);
			break;
		  case Opcodes.NEW:
			push(UNINITIALIZED_KIND | symbolTable.addUninitializedType(argSymbol.value, arg));
			break;
		  case Opcodes.NEWARRAY:
			pop();
			switch (arg)
			{
			  case Opcodes.T_BOOLEAN:
				push(ARRAY_OF | BOOLEAN);
				break;
			  case Opcodes.T_CHAR:
				push(ARRAY_OF | CHAR);
				break;
			  case Opcodes.T_BYTE:
				push(ARRAY_OF | BYTE);
				break;
			  case Opcodes.T_SHORT:
				push(ARRAY_OF | SHORT);
				break;
			  case Opcodes.T_INT:
				push(ARRAY_OF | INTEGER);
				break;
			  case Opcodes.T_FLOAT:
				push(ARRAY_OF | FLOAT);
				break;
			  case Opcodes.T_DOUBLE:
				push(ARRAY_OF | DOUBLE);
				break;
			  case Opcodes.T_LONG:
				push(ARRAY_OF | LONG);
				break;
			  default:
				throw new System.ArgumentException();
			}
			break;
		  case Opcodes.ANEWARRAY:
			string arrayElementType = argSymbol.value;
			pop();
			if (arrayElementType[0] == '[')
			{
			  push(symbolTable, '[' + arrayElementType);
			}
			else
			{
			  push(ARRAY_OF | REFERENCE_KIND | symbolTable.addType(arrayElementType));
			}
			break;
		  case Opcodes.CHECKCAST:
			string castType = argSymbol.value;
			pop();
			if (castType[0] == '[')
			{
			  push(symbolTable, castType);
			}
			else
			{
			  push(REFERENCE_KIND | symbolTable.addType(castType));
			}
			break;
		  case Opcodes.MULTIANEWARRAY:
			pop(arg);
			push(symbolTable, argSymbol.value);
			break;
		  default:
			throw new System.ArgumentException();
		}
	  }

	  // -----------------------------------------------------------------------------------------------
	  // Frame merging methods, used in the second step of the stack map frame computation algorithm
	  // -----------------------------------------------------------------------------------------------

	  /// <summary>
	  /// Computes the concrete output type corresponding to a given abstract output type.
	  /// </summary>
	  /// <param name="abstractOutputType"> an abstract output type. </param>
	  /// <param name="numStack"> the size of the input stack, used to resolve abstract output types of
	  ///     STACK_KIND kind. </param>
	  /// <returns> the concrete output type corresponding to 'abstractOutputType'. </returns>
	  private int getConcreteOutputType(int abstractOutputType, int numStack)
	  {
		int dim = abstractOutputType & DIM_MASK;
		int kind = abstractOutputType & KIND_MASK;
		if (kind == LOCAL_KIND)
		{
		  // By definition, a LOCAL_KIND type designates the concrete type of a local variable at
		  // the beginning of the basic block corresponding to this frame (which is known when
		  // this method is called, but was not when the abstract type was computed).
		  int concreteOutputType = dim + inputLocals[abstractOutputType & VALUE_MASK];
		  if ((abstractOutputType & TOP_IF_LONG_OR_DOUBLE_FLAG) != 0 && (concreteOutputType == LONG || concreteOutputType == DOUBLE))
		  {
			concreteOutputType = TOP;
		  }
		  return concreteOutputType;
		}
		else if (kind == STACK_KIND)
		{
		  // By definition, a STACK_KIND type designates the concrete type of a local variable at
		  // the beginning of the basic block corresponding to this frame (which is known when
		  // this method is called, but was not when the abstract type was computed).
		  int concreteOutputType = dim + inputStack[numStack - (abstractOutputType & VALUE_MASK)];
		  if ((abstractOutputType & TOP_IF_LONG_OR_DOUBLE_FLAG) != 0 && (concreteOutputType == LONG || concreteOutputType == DOUBLE))
		  {
			concreteOutputType = TOP;
		  }
		  return concreteOutputType;
		}
		else
		{
		  return abstractOutputType;
		}
	  }

	  /// <summary>
	  /// Merges the input frame of the given <seealso cref="Frame"/> with the input and output frames of this
	  /// <seealso cref="Frame"/>. Returns {@literal true} if the given frame has been changed by this operation
	  /// (the input and output frames of this <seealso cref="Frame"/> are never changed).
	  /// </summary>
	  /// <param name="symbolTable"> the type table to use to lookup and store type <seealso cref="Symbol"/>. </param>
	  /// <param name="dstFrame"> the <seealso cref="Frame"/> whose input frame must be updated. This should be the frame
	  ///     of a successor, in the control flow graph, of the basic block corresponding to this frame. </param>
	  /// <param name="catchTypeIndex"> if 'frame' corresponds to an exception handler basic block, the type
	  ///     table index of the caught exception type, otherwise 0. </param>
	  /// <returns> {@literal true} if the input frame of 'frame' has been changed by this operation. </returns>
	  public bool merge(SymbolTable symbolTable, Frame dstFrame, int catchTypeIndex)
	  {
		bool frameChanged = false;

		// Compute the concrete types of the local variables at the end of the basic block corresponding
		// to this frame, by resolving its abstract output types, and merge these concrete types with
		// those of the local variables in the input frame of dstFrame.
		int numLocal = inputLocals.Length;
		int numStack = inputStack.Length;
		if (dstFrame.inputLocals == null)
		{
		  dstFrame.inputLocals = new int[numLocal];
		  frameChanged = true;
		}
		for (int i = 0; i < numLocal; ++i)
		{
		  int concreteOutputType;
		  if (outputLocals != null && i < outputLocals.Length)
		  {
			int abstractOutputType = outputLocals[i];
			if (abstractOutputType == 0)
			{
			  // If the local variable has never been assigned in this basic block, it is equal to its
			  // value at the beginning of the block.
			  concreteOutputType = inputLocals[i];
			}
			else
			{
			  concreteOutputType = getConcreteOutputType(abstractOutputType, numStack);
			}
		  }
		  else
		  {
			// If the local variable has never been assigned in this basic block, it is equal to its
			// value at the beginning of the block.
			concreteOutputType = inputLocals[i];
		  }
		  // concreteOutputType might be an uninitialized type from the input locals or from the input
		  // stack. However, if a constructor has been called for this class type in the basic block,
		  // then this type is no longer uninitialized at the end of basic block.
		  if (initializations != null)
		  {
			concreteOutputType = getInitializedType(symbolTable, concreteOutputType);
		  }
		  frameChanged |= merge(symbolTable, concreteOutputType, dstFrame.inputLocals, i);
		}

		// If dstFrame is an exception handler block, it can be reached from any instruction of the
		// basic block corresponding to this frame, in particular from the first one. Therefore, the
		// input locals of dstFrame should be compatible (i.e. merged) with the input locals of this
		// frame (and the input stack of dstFrame should be compatible, i.e. merged, with a one
		// element stack containing the caught exception type).
		if (catchTypeIndex > 0)
		{
		  for (int i = 0; i < numLocal; ++i)
		  {
			frameChanged |= merge(symbolTable, inputLocals[i], dstFrame.inputLocals, i);
		  }
		  if (dstFrame.inputStack == null)
		  {
			dstFrame.inputStack = new int[1];
			frameChanged = true;
		  }
		  frameChanged |= merge(symbolTable, catchTypeIndex, dstFrame.inputStack, 0);
		  return frameChanged;
		}

		// Compute the concrete types of the stack operands at the end of the basic block corresponding
		// to this frame, by resolving its abstract output types, and merge these concrete types with
		// those of the stack operands in the input frame of dstFrame.
		int numInputStack = inputStack.Length + outputStackStart;
		if (dstFrame.inputStack == null)
		{
		  dstFrame.inputStack = new int[numInputStack + outputStackTop];
		  frameChanged = true;
		}
		// First, do this for the stack operands that have not been popped in the basic block
		// corresponding to this frame, and which are therefore equal to their value in the input
		// frame (except for uninitialized types, which may have been initialized).
		for (int i = 0; i < numInputStack; ++i)
		{
		  int concreteOutputType = inputStack[i];
		  if (initializations != null)
		  {
			concreteOutputType = getInitializedType(symbolTable, concreteOutputType);
		  }
		  frameChanged |= merge(symbolTable, concreteOutputType, dstFrame.inputStack, i);
		}
		// Then, do this for the stack operands that have pushed in the basic block (this code is the
		// same as the one above for local variables).
		for (int i = 0; i < outputStackTop; ++i)
		{
		  int abstractOutputType = outputStack[i];
		  int concreteOutputType = getConcreteOutputType(abstractOutputType, numStack);
		  if (initializations != null)
		  {
			concreteOutputType = getInitializedType(symbolTable, concreteOutputType);
		  }
		  frameChanged |= merge(symbolTable, concreteOutputType, dstFrame.inputStack, numInputStack + i);
		}
		return frameChanged;
	  }

	  /// <summary>
	  /// Merges the type at the given index in the given abstract type array with the given type.
	  /// Returns {@literal true} if the type array has been modified by this operation.
	  /// </summary>
	  /// <param name="symbolTable"> the type table to use to lookup and store type <seealso cref="Symbol"/>. </param>
	  /// <param name="sourceType"> the abstract type with which the abstract type array element must be merged.
	  ///     This type should be of <seealso cref="CONSTANT_KIND"/>, <seealso cref="REFERENCE_KIND"/> or {@link
	  ///     #UNINITIALIZED_KIND} kind, with positive or {@literal null} array dimensions. </param>
	  /// <param name="dstTypes"> an array of abstract types. These types should be of <seealso cref="CONSTANT_KIND"/>,
	  ///     <seealso cref="REFERENCE_KIND"/> or <seealso cref="UNINITIALIZED_KIND"/> kind, with positive or {@literal
	  ///     null} array dimensions. </param>
	  /// <param name="dstIndex"> the index of the type that must be merged in dstTypes. </param>
	  /// <returns> {@literal true} if the type array has been modified by this operation. </returns>
	  private static bool merge(SymbolTable symbolTable, int sourceType, int[] dstTypes, int dstIndex)
	  {
		int dstType = dstTypes[dstIndex];
		if (dstType == sourceType)
		{
		  // If the types are equal, merge(sourceType, dstType) = dstType, so there is no change.
		  return false;
		}
		int srcType = sourceType;
		if ((sourceType & ~DIM_MASK) == NULL)
		{
		  if (dstType == NULL)
		  {
			return false;
		  }
		  srcType = NULL;
		}
		if (dstType == 0)
		{
		  // If dstTypes[dstIndex] has never been assigned, merge(srcType, dstType) = srcType.
		  dstTypes[dstIndex] = srcType;
		  return true;
		}
		int mergedType;
		if ((dstType & DIM_MASK) != 0 || (dstType & KIND_MASK) == REFERENCE_KIND)
		{
		  // If dstType is a reference type of any array dimension.
		  if (srcType == NULL)
		  {
			// If srcType is the NULL type, merge(srcType, dstType) = dstType, so there is no change.
			return false;
		  }
		  else if ((srcType & (DIM_MASK | KIND_MASK)) == (dstType & (DIM_MASK | KIND_MASK)))
		  {
			// If srcType has the same array dimension and the same kind as dstType.
			if ((dstType & KIND_MASK) == REFERENCE_KIND)
			{
			  // If srcType and dstType are reference types with the same array dimension,
			  // merge(srcType, dstType) = dim(srcType) | common super class of srcType and dstType.
			  mergedType = (srcType & DIM_MASK) | REFERENCE_KIND | symbolTable.addMergedType(srcType & VALUE_MASK, dstType & VALUE_MASK);
			}
			else
			{
			  // If srcType and dstType are array types of equal dimension but different element types,
			  // merge(srcType, dstType) = dim(srcType) - 1 | java/lang/Object.
			  int mergedDim = ELEMENT_OF + (srcType & DIM_MASK);
			  mergedType = mergedDim | REFERENCE_KIND | symbolTable.addType("java/lang/Object");
			}
		  }
		  else if ((srcType & DIM_MASK) != 0 || (srcType & KIND_MASK) == REFERENCE_KIND)
		  {
			// If srcType is any other reference or array type,
			// merge(srcType, dstType) = min(srcDdim, dstDim) | java/lang/Object
			// where srcDim is the array dimension of srcType, minus 1 if srcType is an array type
			// with a non reference element type (and similarly for dstDim).
			int srcDim = srcType & DIM_MASK;
			if (srcDim != 0 && (srcType & KIND_MASK) != REFERENCE_KIND)
			{
			  srcDim = ELEMENT_OF + srcDim;
			}
			int dstDim = dstType & DIM_MASK;
			if (dstDim != 0 && (dstType & KIND_MASK) != REFERENCE_KIND)
			{
			  dstDim = ELEMENT_OF + dstDim;
			}
			mergedType = Math.Min(srcDim, dstDim) | REFERENCE_KIND | symbolTable.addType("java/lang/Object");
		  }
		  else
		  {
			// If srcType is any other type, merge(srcType, dstType) = TOP.
			mergedType = TOP;
		  }
		}
		else if (dstType == NULL)
		{
		  // If dstType is the NULL type, merge(srcType, dstType) = srcType, or TOP if srcType is not a
		  // an array type or a reference type.
		  mergedType = (srcType & DIM_MASK) != 0 || (srcType & KIND_MASK) == REFERENCE_KIND ? srcType : TOP;
		}
		else
		{
		  // If dstType is any other type, merge(srcType, dstType) = TOP whatever srcType.
		  mergedType = TOP;
		}
		if (mergedType != dstType)
		{
		  dstTypes[dstIndex] = mergedType;
		  return true;
		}
		return false;
	  }

	  // -----------------------------------------------------------------------------------------------
	  // Frame output methods, to generate StackMapFrame attributes
	  // -----------------------------------------------------------------------------------------------

	  /// <summary>
	  /// Makes the given <seealso cref="MethodWriter"/> visit the input frame of this <seealso cref="Frame"/>. The visit is
	  /// done with the <seealso cref="MethodWriter.visitFrameStart"/>, <seealso cref="MethodWriter.visitAbstractType"/> and
	  /// <seealso cref="MethodWriter.visitFrameEnd"/> methods.
	  /// </summary>
	  /// <param name="methodWriter"> the <seealso cref="MethodWriter"/> that should visit the input frame of this {@link
	  ///     Frame}. </param>
	  public void accept(MethodWriter methodWriter)
	  {
		// Compute the number of locals, ignoring TOP types that are just after a LONG or a DOUBLE, and
		// all trailing TOP types.
		int[] localTypes = inputLocals;
		int numLocal = 0;
		int numTrailingTop = 0;
		int i = 0;
		while (i < localTypes.Length)
		{
		  int localType = localTypes[i];
		  i += (localType == LONG || localType == DOUBLE) ? 2 : 1;
		  if (localType == TOP)
		  {
			numTrailingTop++;
		  }
		  else
		  {
			numLocal += numTrailingTop + 1;
			numTrailingTop = 0;
		  }
		}
		// Compute the stack size, ignoring TOP types that are just after a LONG or a DOUBLE.
		int[] stackTypes = inputStack;
		int numStack = 0;
		i = 0;
		while (i < stackTypes.Length)
		{
		  int stackType = stackTypes[i];
		  i += (stackType == LONG || stackType == DOUBLE) ? 2 : 1;
		  numStack++;
		}
		// Visit the frame and its content.
		int frameIndex = methodWriter.visitFrameStart(owner.bytecodeOffset, numLocal, numStack);
		i = 0;
		while (numLocal-- > 0)
		{
		  int localType = localTypes[i];
		  i += (localType == LONG || localType == DOUBLE) ? 2 : 1;
		  methodWriter.visitAbstractType(frameIndex++, localType);
		}
		i = 0;
		while (numStack-- > 0)
		{
		  int stackType = stackTypes[i];
		  i += (stackType == LONG || stackType == DOUBLE) ? 2 : 1;
		  methodWriter.visitAbstractType(frameIndex++, stackType);
		}
		methodWriter.visitFrameEnd();
	  }

	  /// <summary>
	  /// Put the given abstract type in the given ByteVector, using the JVMS verification_type_info
	  /// format used in StackMapTable attributes.
	  /// </summary>
	  /// <param name="symbolTable"> the type table to use to lookup and store type <seealso cref="Symbol"/>. </param>
	  /// <param name="abstractType"> an abstract type, restricted to <seealso cref="Frame.CONSTANT_KIND"/>, {@link
	  ///     Frame#REFERENCE_KIND} or <seealso cref="Frame.UNINITIALIZED_KIND"/> types. </param>
	  /// <param name="output"> where the abstract type must be put. </param>
	  /// <seealso cref= <a href="https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.7.4">JVMS
	  ///     4.7.4</a> </seealso>
	  internal static void putAbstractType(SymbolTable symbolTable, int abstractType, ByteVector output)
	  {
		int arrayDimensions = (abstractType & Frame.DIM_MASK) >> DIM_SHIFT;
		if (arrayDimensions == 0)
		{
		  int typeValue = abstractType & VALUE_MASK;
		  switch (abstractType & KIND_MASK)
		  {
			case CONSTANT_KIND:
			  output.putByte(typeValue);
			  break;
			case REFERENCE_KIND:
			  output.putByte(ITEM_OBJECT).putShort(symbolTable.addConstantClass(symbolTable.getType(typeValue).value).index);
			  break;
			case UNINITIALIZED_KIND:
			  output.putByte(ITEM_UNINITIALIZED).putShort((int) symbolTable.getType(typeValue).data);
			  break;
			default:
			  throw new ("AssertionError");
		  }
		}
		else
		{
		  // Case of an array type, we need to build its descriptor first.
		  StringBuilder typeDescriptor = new StringBuilder();
		  while (arrayDimensions-- > 0)
		  {
			typeDescriptor.Append('[');
		  }
		  if ((abstractType & KIND_MASK) == REFERENCE_KIND)
		  {
			typeDescriptor.Append('L').Append(symbolTable.getType(abstractType & VALUE_MASK).value).Append(';');
		  }
		  else
		  {
			switch (abstractType & VALUE_MASK)
			{
			  case Frame.ITEM_ASM_BOOLEAN:
				typeDescriptor.Append('Z');
				break;
			  case Frame.ITEM_ASM_BYTE:
				typeDescriptor.Append('B');
				break;
			  case Frame.ITEM_ASM_CHAR:
				typeDescriptor.Append('C');
				break;
			  case Frame.ITEM_ASM_SHORT:
				typeDescriptor.Append('S');
				break;
			  case Frame.ITEM_INTEGER:
				typeDescriptor.Append('I');
				break;
			  case Frame.ITEM_FLOAT:
				typeDescriptor.Append('F');
				break;
			  case Frame.ITEM_LONG:
				typeDescriptor.Append('J');
				break;
			  case Frame.ITEM_DOUBLE:
				typeDescriptor.Append('D');
				break;
			  default:
				throw new ("AssertionError");
			}
		  }
		  output.putByte(ITEM_OBJECT).putShort(symbolTable.addConstantClass(typeDescriptor.ToString()).index);
		}
	  }
	}

}