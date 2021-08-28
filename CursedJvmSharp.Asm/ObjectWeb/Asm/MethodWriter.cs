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
	/// A <seealso cref="MethodVisitor"/> that generates a corresponding 'method_info' structure, as defined in the
	/// Java Virtual Machine Specification (JVMS).
	/// </summary>
	/// <seealso cref= <a href="https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.6">JVMS
	///     4.6</a>
	/// @author Eric Bruneton
	/// @author Eugene Kuleshov </seealso>
	internal sealed class MethodWriter : MethodVisitor
	{

	  /// <summary>
	  /// Indicates that nothing must be computed. </summary>
	  internal const int COMPUTE_NOTHING = 0;

	  /// <summary>
	  /// Indicates that the maximum stack size and the maximum number of local variables must be
	  /// computed, from scratch.
	  /// </summary>
	  internal const int COMPUTE_MAX_STACK_AND_LOCAL = 1;

	  /// <summary>
	  /// Indicates that the maximum stack size and the maximum number of local variables must be
	  /// computed, from the existing stack map frames. This can be done more efficiently than with the
	  /// control flow graph algorithm used for <seealso cref="COMPUTE_MAX_STACK_AND_LOCAL"/>, by using a linear
	  /// scan of the bytecode instructions.
	  /// </summary>
	  internal const int COMPUTE_MAX_STACK_AND_LOCAL_FROM_FRAMES = 2;

	  /// <summary>
	  /// Indicates that the stack map frames of type F_INSERT must be computed. The other frames are not
	  /// computed. They should all be of type F_NEW and should be sufficient to compute the content of
	  /// the F_INSERT frames, together with the bytecode instructions between a F_NEW and a F_INSERT
	  /// frame - and without any knowledge of the type hierarchy (by definition of F_INSERT).
	  /// </summary>
	  internal const int COMPUTE_INSERTED_FRAMES = 3;

	  /// <summary>
	  /// Indicates that all the stack map frames must be computed. In this case the maximum stack size
	  /// and the maximum number of local variables is also computed.
	  /// </summary>
	  internal const int COMPUTE_ALL_FRAMES = 4;

	  /// <summary>
	  /// Indicates that <seealso cref="STACK_SIZE_DELTA"/> is not applicable (not constant or never used). </summary>
	  private const int NA = 0;

	  /// <summary>
	  /// The stack size variation corresponding to each JVM opcode. The stack size variation for opcode
	  /// 'o' is given by the array element at index 'o'.
	  /// </summary>
	  /// <seealso cref= <a href="https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-6.html">JVMS 6</a> </seealso>
	  private static readonly int[] STACK_SIZE_DELTA = new int[] {0, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 1, 1, 1, 2, 2, 1, 1, 1, NA, NA, 1, 2, 1, 2, 1, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, -1, 0, -1, 0, -1, -1, -1, -1, -1, -2, -1, -2, -1, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, -3, -4, -3, -4, -3, -3, -3, -3, -1, -2, 1, 1, 1, 2, 2, 2, 0, -1, -2, -1, -2, -1, -2, -1, -2, -1, -2, -1, -2, -1, -2, -1, -2, -1, -2, -1, -2, 0, 0, 0, 0, -1, -1, -1, -1, -1, -1, -1, -2, -1, -2, -1, -2, 0, 1, 0, 1, -1, -1, 0, 0, 1, 1, -1, 0, -1, 0, 0, 0, -3, -1, -1, -3, -3, -1, -1, -1, -1, -1, -1, -2, -2, -2, -2, -2, -2, -2, -2, 0, 1, 0, -1, -1, -1, -2, -1, -2, -1, 0, NA, NA, NA, NA, NA, NA, NA, NA, NA, 1, 0, 0, 0, NA, 0, 0, -1, -1, NA, NA, -1, -1, NA, NA};

	  /// <summary>
	  /// Where the constants used in this MethodWriter must be stored. </summary>
	  private readonly SymbolTable symbolTable;

	  // Note: fields are ordered as in the method_info structure, and those related to attributes are
	  // ordered as in Section 4.7 of the JVMS.

	  /// <summary>
	  /// The access_flags field of the method_info JVMS structure. This field can contain ASM specific
	  /// access flags, such as <seealso cref="Opcodes.ACC_DEPRECATED"/>, which are removed when generating the
	  /// ClassFile structure.
	  /// </summary>
	  private readonly int accessFlags;

	  /// <summary>
	  /// The name_index field of the method_info JVMS structure. </summary>
	  private readonly int nameIndex;

	  /// <summary>
	  /// The name of this method. </summary>
	  private readonly string name;

	  /// <summary>
	  /// The descriptor_index field of the method_info JVMS structure. </summary>
	  private readonly int descriptorIndex;

	  /// <summary>
	  /// The descriptor of this method. </summary>
	  private readonly string descriptor;

	  // Code attribute fields and sub attributes:

	  /// <summary>
	  /// The max_stack field of the Code attribute. </summary>
	  private int maxStack;

	  /// <summary>
	  /// The max_locals field of the Code attribute. </summary>
	  private int maxLocals;

	  /// <summary>
	  /// The 'code' field of the Code attribute. </summary>
	  private readonly ByteVector code = new ByteVector();

	  /// <summary>
	  /// The first element in the exception handler list (used to generate the exception_table of the
	  /// Code attribute). The next ones can be accessed with the <seealso cref="Handler.nextHandler"/> field. May
	  /// be {@literal null}.
	  /// </summary>
	  private Handler firstHandler;

	  /// <summary>
	  /// The last element in the exception handler list (used to generate the exception_table of the
	  /// Code attribute). The next ones can be accessed with the <seealso cref="Handler.nextHandler"/> field. May
	  /// be {@literal null}.
	  /// </summary>
	  private Handler lastHandler;

	  /// <summary>
	  /// The line_number_table_length field of the LineNumberTable code attribute. </summary>
	  private int lineNumberTableLength;

	  /// <summary>
	  /// The line_number_table array of the LineNumberTable code attribute, or {@literal null}. </summary>
	  private ByteVector lineNumberTable;

	  /// <summary>
	  /// The local_variable_table_length field of the LocalVariableTable code attribute. </summary>
	  private int localVariableTableLength;

	  /// <summary>
	  /// The local_variable_table array of the LocalVariableTable code attribute, or {@literal null}.
	  /// </summary>
	  private ByteVector localVariableTable;

	  /// <summary>
	  /// The local_variable_type_table_length field of the LocalVariableTypeTable code attribute. </summary>
	  private int localVariableTypeTableLength;

	  /// <summary>
	  /// The local_variable_type_table array of the LocalVariableTypeTable code attribute, or {@literal
	  /// null}.
	  /// </summary>
	  private ByteVector localVariableTypeTable;

	  /// <summary>
	  /// The number_of_entries field of the StackMapTable code attribute. </summary>
	  private int stackMapTableNumberOfEntries;

	  /// <summary>
	  /// The 'entries' array of the StackMapTable code attribute. </summary>
	  private ByteVector stackMapTableEntries;

	  /// <summary>
	  /// The last runtime visible type annotation of the Code attribute. The previous ones can be
	  /// accessed with the <seealso cref="AnnotationWriter.previousAnnotation"/> field. May be {@literal null}.
	  /// </summary>
	  private AnnotationWriter lastCodeRuntimeVisibleTypeAnnotation;

	  /// <summary>
	  /// The last runtime invisible type annotation of the Code attribute. The previous ones can be
	  /// accessed with the <seealso cref="AnnotationWriter.previousAnnotation"/> field. May be {@literal null}.
	  /// </summary>
	  private AnnotationWriter lastCodeRuntimeInvisibleTypeAnnotation;

	  /// <summary>
	  /// The first non standard attribute of the Code attribute. The next ones can be accessed with the
	  /// <seealso cref="Attribute.nextAttribute"/> field. May be {@literal null}.
	  /// 
	  /// <para><b>WARNING</b>: this list stores the attributes in the <i>reverse</i> order of their visit.
	  /// firstAttribute is actually the last attribute visited in <seealso cref="visitAttribute"/>. The {@link
	  /// #putMethodInfo} method writes the attributes in the order defined by this list, i.e. in the
	  /// reverse order specified by the user.
	  /// </para>
	  /// </summary>
	  private Attribute firstCodeAttribute;

	  // Other method_info attributes:

	  /// <summary>
	  /// The number_of_exceptions field of the Exceptions attribute. </summary>
	  private readonly int numberOfExceptions;

	  /// <summary>
	  /// The exception_index_table array of the Exceptions attribute, or {@literal null}. </summary>
	  private readonly int[] exceptionIndexTable;

	  /// <summary>
	  /// The signature_index field of the Signature attribute. </summary>
	  private readonly int signatureIndex;

	  /// <summary>
	  /// The last runtime visible annotation of this method. The previous ones can be accessed with the
	  /// <seealso cref="AnnotationWriter.previousAnnotation"/> field. May be {@literal null}.
	  /// </summary>
	  private AnnotationWriter lastRuntimeVisibleAnnotation;

	  /// <summary>
	  /// The last runtime invisible annotation of this method. The previous ones can be accessed with
	  /// the <seealso cref="AnnotationWriter.previousAnnotation"/> field. May be {@literal null}.
	  /// </summary>
	  private AnnotationWriter lastRuntimeInvisibleAnnotation;

	  /// <summary>
	  /// The number of method parameters that can have runtime visible annotations, or 0. </summary>
	  private int visibleAnnotableParameterCount;

	  /// <summary>
	  /// The runtime visible parameter annotations of this method. Each array element contains the last
	  /// annotation of a parameter (which can be {@literal null} - the previous ones can be accessed
	  /// with the <seealso cref="AnnotationWriter.previousAnnotation"/> field). May be {@literal null}.
	  /// </summary>
	  private AnnotationWriter[] lastRuntimeVisibleParameterAnnotations;

	  /// <summary>
	  /// The number of method parameters that can have runtime visible annotations, or 0. </summary>
	  private int invisibleAnnotableParameterCount;

	  /// <summary>
	  /// The runtime invisible parameter annotations of this method. Each array element contains the
	  /// last annotation of a parameter (which can be {@literal null} - the previous ones can be
	  /// accessed with the <seealso cref="AnnotationWriter.previousAnnotation"/> field). May be {@literal null}.
	  /// </summary>
	  private AnnotationWriter[] lastRuntimeInvisibleParameterAnnotations;

	  /// <summary>
	  /// The last runtime visible type annotation of this method. The previous ones can be accessed with
	  /// the <seealso cref="AnnotationWriter.previousAnnotation"/> field. May be {@literal null}.
	  /// </summary>
	  private AnnotationWriter lastRuntimeVisibleTypeAnnotation;

	  /// <summary>
	  /// The last runtime invisible type annotation of this method. The previous ones can be accessed
	  /// with the <seealso cref="AnnotationWriter.previousAnnotation"/> field. May be {@literal null}.
	  /// </summary>
	  private AnnotationWriter lastRuntimeInvisibleTypeAnnotation;

	  /// <summary>
	  /// The default_value field of the AnnotationDefault attribute, or {@literal null}. </summary>
	  private ByteVector defaultValue;

	  /// <summary>
	  /// The parameters_count field of the MethodParameters attribute. </summary>
	  private int parametersCount;

	  /// <summary>
	  /// The 'parameters' array of the MethodParameters attribute, or {@literal null}. </summary>
	  private ByteVector parameters;

	  /// <summary>
	  /// The first non standard attribute of this method. The next ones can be accessed with the {@link
	  /// Attribute#nextAttribute} field. May be {@literal null}.
	  /// 
	  /// <para><b>WARNING</b>: this list stores the attributes in the <i>reverse</i> order of their visit.
	  /// firstAttribute is actually the last attribute visited in <seealso cref="visitAttribute"/>. The {@link
	  /// #putMethodInfo} method writes the attributes in the order defined by this list, i.e. in the
	  /// reverse order specified by the user.
	  /// </para>
	  /// </summary>
	  private Attribute firstAttribute;

	  // -----------------------------------------------------------------------------------------------
	  // Fields used to compute the maximum stack size and number of locals, and the stack map frames
	  // -----------------------------------------------------------------------------------------------

	  /// <summary>
	  /// Indicates what must be computed. Must be one of <seealso cref="COMPUTE_ALL_FRAMES"/>, {@link
	  /// #COMPUTE_INSERTED_FRAMES}, <seealso cref="COMPUTE_MAX_STACK_AND_LOCAL"/> or <seealso cref="COMPUTE_NOTHING"/>.
	  /// </summary>
	  private readonly int compute;

	  /// <summary>
	  /// The first basic block of the method. The next ones (in bytecode offset order) can be accessed
	  /// with the <seealso cref="Label.nextBasicBlock"/> field.
	  /// </summary>
	  private Label firstBasicBlock;

	  /// <summary>
	  /// The last basic block of the method (in bytecode offset order). This field is updated each time
	  /// a basic block is encountered, and is used to append it at the end of the basic block list.
	  /// </summary>
	  private Label lastBasicBlock;

	  /// <summary>
	  /// The current basic block, i.e. the basic block of the last visited instruction. When {@link
	  /// #compute} is equal to <seealso cref="COMPUTE_MAX_STACK_AND_LOCAL"/> or <seealso cref="COMPUTE_ALL_FRAMES"/>, this
	  /// field is {@literal null} for unreachable code. When <seealso cref="compute"/> is equal to {@link
	  /// #COMPUTE_MAX_STACK_AND_LOCAL_FROM_FRAMES} or <seealso cref="COMPUTE_INSERTED_FRAMES"/>, this field stays
	  /// unchanged throughout the whole method (i.e. the whole code is seen as a single basic block;
	  /// indeed, the existing frames are sufficient by hypothesis to compute any intermediate frame -
	  /// and the maximum stack size as well - without using any control flow graph).
	  /// </summary>
	  private Label currentBasicBlock;

	  /// <summary>
	  /// The relative stack size after the last visited instruction. This size is relative to the
	  /// beginning of <seealso cref="currentBasicBlock"/>, i.e. the true stack size after the last visited
	  /// instruction is equal to the <seealso cref="Label.inputStackSize"/> of the current basic block plus {@link
	  /// #relativeStackSize}. When <seealso cref="compute"/> is equal to {@link
	  /// #COMPUTE_MAX_STACK_AND_LOCAL_FROM_FRAMES}, <seealso cref="currentBasicBlock"/> is always the start of
	  /// the method, so this relative size is also equal to the absolute stack size after the last
	  /// visited instruction.
	  /// </summary>
	  private int relativeStackSize;

	  /// <summary>
	  /// The maximum relative stack size after the last visited instruction. This size is relative to
	  /// the beginning of <seealso cref="currentBasicBlock"/>, i.e. the true maximum stack size after the last
	  /// visited instruction is equal to the <seealso cref="Label.inputStackSize"/> of the current basic block
	  /// plus <seealso cref="maxRelativeStackSize"/>.When <seealso cref="compute"/> is equal to {@link
	  /// #COMPUTE_MAX_STACK_AND_LOCAL_FROM_FRAMES}, <seealso cref="currentBasicBlock"/> is always the start of
	  /// the method, so this relative size is also equal to the absolute maximum stack size after the
	  /// last visited instruction.
	  /// </summary>
	  private int maxRelativeStackSize;

	  /// <summary>
	  /// The number of local variables in the last visited stack map frame. </summary>
	  private int currentLocals;

	  /// <summary>
	  /// The bytecode offset of the last frame that was written in <seealso cref="stackMapTableEntries"/>. </summary>
	  private int previousFrameOffset;

	  /// <summary>
	  /// The last frame that was written in <seealso cref="stackMapTableEntries"/>. This field has the same
	  /// format as <seealso cref="currentFrame"/>.
	  /// </summary>
	  private int[] previousFrame;

	  /// <summary>
	  /// The current stack map frame. The first element contains the bytecode offset of the instruction
	  /// to which the frame corresponds, the second element is the number of locals and the third one is
	  /// the number of stack elements. The local variables start at index 3 and are followed by the
	  /// operand stack elements. In summary frame[0] = offset, frame[1] = numLocal, frame[2] = numStack.
	  /// Local variables and operand stack entries contain abstract types, as defined in <seealso cref="Frame"/>,
	  /// but restricted to <seealso cref="Frame.CONSTANT_KIND"/>, <seealso cref="Frame.REFERENCE_KIND"/> or {@link
	  /// Frame#UNINITIALIZED_KIND} abstract types. Long and double types use only one array entry.
	  /// </summary>
	  private int[] currentFrame;

	  /// <summary>
	  /// Whether this method contains subroutines. </summary>
	  private bool hasSubroutines;

	  // -----------------------------------------------------------------------------------------------
	  // Other miscellaneous status fields
	  // -----------------------------------------------------------------------------------------------

	  /// <summary>
	  /// Whether the bytecode of this method contains ASM specific instructions. </summary>
	  private bool hasAsmInstructions_Conflict;

	  /// <summary>
	  /// The start offset of the last visited instruction. Used to set the offset field of type
	  /// annotations of type 'offset_target' (see <a
	  /// href="https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.7.20.1">JVMS
	  /// 4.7.20.1</a>).
	  /// </summary>
	  private int lastBytecodeOffset;

	  /// <summary>
	  /// The offset in bytes in <seealso cref="SymbolTable.getSource"/> from which the method_info for this method
	  /// (excluding its first 6 bytes) must be copied, or 0.
	  /// </summary>
	  private int sourceOffset;

	  /// <summary>
	  /// The length in bytes in <seealso cref="SymbolTable.getSource"/> which must be copied to get the
	  /// method_info for this method (excluding its first 6 bytes for access_flags, name_index and
	  /// descriptor_index).
	  /// </summary>
	  private int sourceLength;

	  // -----------------------------------------------------------------------------------------------
	  // Constructor and accessors
	  // -----------------------------------------------------------------------------------------------

	  /// <summary>
	  /// Constructs a new <seealso cref="MethodWriter"/>.
	  /// </summary>
	  /// <param name="symbolTable"> where the constants used in this AnnotationWriter must be stored. </param>
	  /// <param name="access"> the method's access flags (see <seealso cref="Opcodes"/>). </param>
	  /// <param name="name"> the method's name. </param>
	  /// <param name="descriptor"> the method's descriptor (see <seealso cref="Type"/>). </param>
	  /// <param name="signature"> the method's signature. May be {@literal null}. </param>
	  /// <param name="exceptions"> the internal names of the method's exceptions. May be {@literal null}. </param>
	  /// <param name="compute"> indicates what must be computed (see #compute). </param>
	  public MethodWriter(SymbolTable symbolTable, int access, string name, string descriptor, string signature, string[] exceptions, int compute) : base(Opcodes.ASM9)
	  {
		this.symbolTable = symbolTable;
		this.accessFlags = "<init>".Equals(name) ? access | Constants.ACC_CONSTRUCTOR : access;
		this.nameIndex = symbolTable.addConstantUtf8(name);
		this.name = name;
		this.descriptorIndex = symbolTable.addConstantUtf8(descriptor);
		this.descriptor = descriptor;
		this.signatureIndex = string.ReferenceEquals(signature, null) ? 0 : symbolTable.addConstantUtf8(signature);
		if (exceptions != null && exceptions.Length > 0)
		{
		  numberOfExceptions = exceptions.Length;
		  this.exceptionIndexTable = new int[numberOfExceptions];
		  for (int i = 0; i < numberOfExceptions; ++i)
		  {
			this.exceptionIndexTable[i] = symbolTable.addConstantClass(exceptions[i]).index;
		  }
		}
		else
		{
		  numberOfExceptions = 0;
		  this.exceptionIndexTable = null;
		}
		this.compute = compute;
		if (compute != COMPUTE_NOTHING)
		{
		  // Update maxLocals and currentLocals.
		  int argumentsSize = JType.getArgumentsAndReturnSizes(descriptor) >> 2;
		  if ((access & Opcodes.ACC_STATIC) != 0)
		  {
			--argumentsSize;
		  }
		  maxLocals = argumentsSize;
		  currentLocals = argumentsSize;
		  // Create and visit the label for the first basic block.
		  firstBasicBlock = new Label();
		  visitLabel(firstBasicBlock);
		}
	  }

	  public bool hasFrames()
	  {
		return stackMapTableNumberOfEntries > 0;
	  }

	  public bool hasAsmInstructions()
	  {
		return hasAsmInstructions_Conflict;
	  }

	  // -----------------------------------------------------------------------------------------------
	  // Implementation of the MethodVisitor abstract class
	  // -----------------------------------------------------------------------------------------------

	  public override void visitParameter(string name, int access)
	  {
		if (parameters == null)
		{
		  parameters = new ByteVector();
		}
		++parametersCount;
		parameters.putShort((string.ReferenceEquals(name, null)) ? 0 : symbolTable.addConstantUtf8(name)).putShort(access);
	  }

	  public override AnnotationVisitor visitAnnotationDefault()
	  {
		defaultValue = new ByteVector();
		return new AnnotationWriter(symbolTable, false, defaultValue, null);
	  }

	  public override AnnotationVisitor visitAnnotation(string descriptor, bool visible)
	  {
		if (visible)
		{
		  return lastRuntimeVisibleAnnotation = AnnotationWriter.create(symbolTable, descriptor, lastRuntimeVisibleAnnotation);
		}
		else
		{
		  return lastRuntimeInvisibleAnnotation = AnnotationWriter.create(symbolTable, descriptor, lastRuntimeInvisibleAnnotation);
		}
	  }

	  public override AnnotationVisitor visitTypeAnnotation(int typeRef, TypePath typePath, string descriptor, bool visible)
	  {
		if (visible)
		{
		  return lastRuntimeVisibleTypeAnnotation = AnnotationWriter.create(symbolTable, typeRef, typePath, descriptor, lastRuntimeVisibleTypeAnnotation);
		}
		else
		{
		  return lastRuntimeInvisibleTypeAnnotation = AnnotationWriter.create(symbolTable, typeRef, typePath, descriptor, lastRuntimeInvisibleTypeAnnotation);
		}
	  }

	  public override void visitAnnotableParameterCount(int parameterCount, bool visible)
	  {
		if (visible)
		{
		  visibleAnnotableParameterCount = parameterCount;
		}
		else
		{
		  invisibleAnnotableParameterCount = parameterCount;
		}
	  }

	  public override AnnotationVisitor visitParameterAnnotation(int parameter, string annotationDescriptor, bool visible)
	  {
		if (visible)
		{
		  if (lastRuntimeVisibleParameterAnnotations == null)
		  {
			lastRuntimeVisibleParameterAnnotations = new AnnotationWriter[JType.getArgumentTypes(descriptor).Length];
		  }
		  return lastRuntimeVisibleParameterAnnotations[parameter] = AnnotationWriter.create(symbolTable, annotationDescriptor, lastRuntimeVisibleParameterAnnotations[parameter]);
		}
		else
		{
		  if (lastRuntimeInvisibleParameterAnnotations == null)
		  {
			lastRuntimeInvisibleParameterAnnotations = new AnnotationWriter[JType.getArgumentTypes(descriptor).Length];
		  }
		  return lastRuntimeInvisibleParameterAnnotations[parameter] = AnnotationWriter.create(symbolTable, annotationDescriptor, lastRuntimeInvisibleParameterAnnotations[parameter]);
		}
	  }

	  public override void visitAttribute(Attribute attribute)
	  {
		// Store the attributes in the <i>reverse</i> order of their visit by this method.
		if (attribute.CodeAttribute)
		{
		  attribute.nextAttribute = firstCodeAttribute;
		  firstCodeAttribute = attribute;
		}
		else
		{
		  attribute.nextAttribute = firstAttribute;
		  firstAttribute = attribute;
		}
	  }

	  public override void visitCode()
	  {
		// Nothing to do.
	  }

	  public override void visitFrame(int type, int numLocal, object[] local, int numStack, object[] stack)
	  {
		if (compute == COMPUTE_ALL_FRAMES)
		{
		  return;
		}

		if (compute == COMPUTE_INSERTED_FRAMES)
		{
		  if (currentBasicBlock.frame == null)
		  {
			// This should happen only once, for the implicit first frame (which is explicitly visited
			// in ClassReader if the EXPAND_ASM_INSNS option is used - and COMPUTE_INSERTED_FRAMES
			// can't be set if EXPAND_ASM_INSNS is not used).
			currentBasicBlock.frame = new CurrentFrame(currentBasicBlock);
			currentBasicBlock.frame.setInputFrameFromDescriptor(symbolTable, accessFlags, descriptor, numLocal);
			currentBasicBlock.frame.accept(this);
		  }
		  else
		  {
			if (type == Opcodes.F_NEW)
			{
			  currentBasicBlock.frame.setInputFrameFromApiFormat(symbolTable, numLocal, local, numStack, stack);
			}
			// If type is not F_NEW then it is F_INSERT by hypothesis, and currentBlock.frame contains
			// the stack map frame at the current instruction, computed from the last F_NEW frame and
			// the bytecode instructions in between (via calls to CurrentFrame#execute).
			currentBasicBlock.frame.accept(this);
		  }
		}
		else if (type == Opcodes.F_NEW)
		{
		  if (previousFrame == null)
		  {
			int argumentsSize = JType.getArgumentsAndReturnSizes(descriptor) >> 2;
			Frame implicitFirstFrame = new Frame(new Label());
			implicitFirstFrame.setInputFrameFromDescriptor(symbolTable, accessFlags, descriptor, argumentsSize);
			implicitFirstFrame.accept(this);
		  }
		  currentLocals = numLocal;
		  int frameIndex = visitFrameStart(code.length, numLocal, numStack);
		  for (int i = 0; i < numLocal; ++i)
		  {
			currentFrame[frameIndex++] = Frame.getAbstractTypeFromApiFormat(symbolTable, local[i]);
		  }
		  for (int i = 0; i < numStack; ++i)
		  {
			currentFrame[frameIndex++] = Frame.getAbstractTypeFromApiFormat(symbolTable, stack[i]);
		  }
		  visitFrameEnd();
		}
		else
		{
		  if (symbolTable.MajorVersion < Opcodes.V1_6)
		  {
			throw new System.ArgumentException("Class versions V1_5 or less must use F_NEW frames.");
		  }
		  int offsetDelta;
		  if (stackMapTableEntries == null)
		  {
			stackMapTableEntries = new ByteVector();
			offsetDelta = code.length;
		  }
		  else
		  {
			offsetDelta = code.length - previousFrameOffset - 1;
			if (offsetDelta < 0)
			{
			  if (type == Opcodes.F_SAME)
			  {
				return;
			  }
			  else
			  {
				throw new System.InvalidOperationException();
			  }
			}
		  }

		  switch (type)
		  {
			case Opcodes.F_FULL:
			  currentLocals = numLocal;
			  stackMapTableEntries.putByte(Frame.FULL_FRAME).putShort(offsetDelta).putShort(numLocal);
			  for (int i = 0; i < numLocal; ++i)
			  {
				putFrameType(local[i]);
			  }
			  stackMapTableEntries.putShort(numStack);
			  for (int i = 0; i < numStack; ++i)
			  {
				putFrameType(stack[i]);
			  }
			  break;
			case Opcodes.F_APPEND:
			  currentLocals += numLocal;
			  stackMapTableEntries.putByte(Frame.SAME_FRAME_EXTENDED + numLocal).putShort(offsetDelta);
			  for (int i = 0; i < numLocal; ++i)
			  {
				putFrameType(local[i]);
			  }
			  break;
			case Opcodes.F_CHOP:
			  currentLocals -= numLocal;
			  stackMapTableEntries.putByte(Frame.SAME_FRAME_EXTENDED - numLocal).putShort(offsetDelta);
			  break;
			case Opcodes.F_SAME:
			  if (offsetDelta < 64)
			  {
				stackMapTableEntries.putByte(offsetDelta);
			  }
			  else
			  {
				stackMapTableEntries.putByte(Frame.SAME_FRAME_EXTENDED).putShort(offsetDelta);
			  }
			  break;
			case Opcodes.F_SAME1:
			  if (offsetDelta < 64)
			  {
				stackMapTableEntries.putByte(Frame.SAME_LOCALS_1_STACK_ITEM_FRAME + offsetDelta);
			  }
			  else
			  {
				stackMapTableEntries.putByte(Frame.SAME_LOCALS_1_STACK_ITEM_FRAME_EXTENDED).putShort(offsetDelta);
			  }
			  putFrameType(stack[0]);
			  break;
			default:
			  throw new System.ArgumentException();
		  }

		  previousFrameOffset = code.length;
		  ++stackMapTableNumberOfEntries;
		}

		if (compute == COMPUTE_MAX_STACK_AND_LOCAL_FROM_FRAMES)
		{
		  relativeStackSize = numStack;
		  for (int i = 0; i < numStack; ++i)
		  {
			if (Equals(stack[i], Opcodes.LONG) || Equals(stack[i], Opcodes.DOUBLE))
			{
			  relativeStackSize++;
			}
		  }
		  if (relativeStackSize > maxRelativeStackSize)
		  {
			maxRelativeStackSize = relativeStackSize;
		  }
		}

		maxStack = Math.Max(maxStack, numStack);
		maxLocals = Math.Max(maxLocals, currentLocals);
	  }

	  public override void visitInsn(int opcode)
	  {
		lastBytecodeOffset = code.length;
		// Add the instruction to the bytecode of the method.
		code.putByte(opcode);
		// If needed, update the maximum stack size and number of locals, and stack map frames.
		if (currentBasicBlock != null)
		{
		  if (compute == COMPUTE_ALL_FRAMES || compute == COMPUTE_INSERTED_FRAMES)
		  {
			currentBasicBlock.frame.execute(opcode, 0, null, null);
		  }
		  else
		  {
			int size = relativeStackSize + STACK_SIZE_DELTA[opcode];
			if (size > maxRelativeStackSize)
			{
			  maxRelativeStackSize = size;
			}
			relativeStackSize = size;
		  }
		  if ((opcode >= Opcodes.IRETURN && opcode <= Opcodes.RETURN) || opcode == Opcodes.ATHROW)
		  {
			endCurrentBasicBlockWithNoSuccessor();
		  }
		}
	  }

	  public override void visitIntInsn(int opcode, int operand)
	  {
		lastBytecodeOffset = code.length;
		// Add the instruction to the bytecode of the method.
		if (opcode == Opcodes.SIPUSH)
		{
		  code.put12(opcode, operand);
		}
		else
		{ // BIPUSH or NEWARRAY
		  code.put11(opcode, operand);
		}
		// If needed, update the maximum stack size and number of locals, and stack map frames.
		if (currentBasicBlock != null)
		{
		  if (compute == COMPUTE_ALL_FRAMES || compute == COMPUTE_INSERTED_FRAMES)
		  {
			currentBasicBlock.frame.execute(opcode, operand, null, null);
		  }
		  else if (opcode != Opcodes.NEWARRAY)
		  {
			// The stack size delta is 1 for BIPUSH or SIPUSH, and 0 for NEWARRAY.
			int size = relativeStackSize + 1;
			if (size > maxRelativeStackSize)
			{
			  maxRelativeStackSize = size;
			}
			relativeStackSize = size;
		  }
		}
	  }

	  public override void visitVarInsn(int opcode, int var)
	  {
		lastBytecodeOffset = code.length;
		// Add the instruction to the bytecode of the method.
		if (var < 4 && opcode != Opcodes.RET)
		{
		  int optimizedOpcode;
		  if (opcode < Opcodes.ISTORE)
		  {
			optimizedOpcode = Constants.ILOAD_0 + ((opcode - Opcodes.ILOAD) << 2) + var;
		  }
		  else
		  {
			optimizedOpcode = Constants.ISTORE_0 + ((opcode - Opcodes.ISTORE) << 2) + var;
		  }
		  code.putByte(optimizedOpcode);
		}
		else if (var >= 256)
		{
		  code.putByte(Constants.WIDE).put12(opcode, var);
		}
		else
		{
		  code.put11(opcode, var);
		}
		// If needed, update the maximum stack size and number of locals, and stack map frames.
		if (currentBasicBlock != null)
		{
		  if (compute == COMPUTE_ALL_FRAMES || compute == COMPUTE_INSERTED_FRAMES)
		  {
			currentBasicBlock.frame.execute(opcode, var, null, null);
		  }
		  else
		  {
			if (opcode == Opcodes.RET)
			{
			  // No stack size delta.
			  currentBasicBlock.flags |= (short)Label.FLAG_SUBROUTINE_END;
			  currentBasicBlock.outputStackSize = (short) relativeStackSize;
			  endCurrentBasicBlockWithNoSuccessor();
			}
			else
			{ // xLOAD or xSTORE
			  int size = relativeStackSize + STACK_SIZE_DELTA[opcode];
			  if (size > maxRelativeStackSize)
			  {
				maxRelativeStackSize = size;
			  }
			  relativeStackSize = size;
			}
		  }
		}
		if (compute != COMPUTE_NOTHING)
		{
		  int currentMaxLocals;
		  if (opcode == Opcodes.LLOAD || opcode == Opcodes.DLOAD || opcode == Opcodes.LSTORE || opcode == Opcodes.DSTORE)
		  {
			currentMaxLocals = var + 2;
		  }
		  else
		  {
			currentMaxLocals = var + 1;
		  }
		  if (currentMaxLocals > maxLocals)
		  {
			maxLocals = currentMaxLocals;
		  }
		}
		if (opcode >= Opcodes.ISTORE && compute == COMPUTE_ALL_FRAMES && firstHandler != null)
		{
		  // If there are exception handler blocks, each instruction within a handler range is, in
		  // theory, a basic block (since execution can jump from this instruction to the exception
		  // handler). As a consequence, the local variable types at the beginning of the handler
		  // block should be the merge of the local variable types at all the instructions within the
		  // handler range. However, instead of creating a basic block for each instruction, we can
		  // get the same result in a more efficient way. Namely, by starting a new basic block after
		  // each xSTORE instruction, which is what we do here.
		  visitLabel(new Label());
		}
	  }

	  public override void visitTypeInsn(int opcode, string type)
	  {
		lastBytecodeOffset = code.length;
		// Add the instruction to the bytecode of the method.
		Symbol typeSymbol = symbolTable.addConstantClass(type);
		code.put12(opcode, typeSymbol.index);
		// If needed, update the maximum stack size and number of locals, and stack map frames.
		if (currentBasicBlock != null)
		{
		  if (compute == COMPUTE_ALL_FRAMES || compute == COMPUTE_INSERTED_FRAMES)
		  {
			currentBasicBlock.frame.execute(opcode, lastBytecodeOffset, typeSymbol, symbolTable);
		  }
		  else if (opcode == Opcodes.NEW)
		  {
			// The stack size delta is 1 for NEW, and 0 for ANEWARRAY, CHECKCAST, or INSTANCEOF.
			int size = relativeStackSize + 1;
			if (size > maxRelativeStackSize)
			{
			  maxRelativeStackSize = size;
			}
			relativeStackSize = size;
		  }
		}
	  }

	  public override void visitFieldInsn(int opcode, string owner, string name, string descriptor)
	  {
		lastBytecodeOffset = code.length;
		// Add the instruction to the bytecode of the method.
		Symbol fieldrefSymbol = symbolTable.addConstantFieldref(owner, name, descriptor);
		code.put12(opcode, fieldrefSymbol.index);
		// If needed, update the maximum stack size and number of locals, and stack map frames.
		if (currentBasicBlock != null)
		{
		  if (compute == COMPUTE_ALL_FRAMES || compute == COMPUTE_INSERTED_FRAMES)
		  {
			currentBasicBlock.frame.execute(opcode, 0, fieldrefSymbol, symbolTable);
		  }
		  else
		  {
			int size;
			char firstDescChar = descriptor[0];
			switch (opcode)
			{
			  case Opcodes.GETSTATIC:
				size = relativeStackSize + (firstDescChar == 'D' || firstDescChar == 'J' ? 2 : 1);
				break;
			  case Opcodes.PUTSTATIC:
				size = relativeStackSize + (firstDescChar == 'D' || firstDescChar == 'J' ? -2 : -1);
				break;
			  case Opcodes.GETFIELD:
				size = relativeStackSize + (firstDescChar == 'D' || firstDescChar == 'J' ? 1 : 0);
				break;
			  case Opcodes.PUTFIELD:
			  default:
				size = relativeStackSize + (firstDescChar == 'D' || firstDescChar == 'J' ? -3 : -2);
				break;
			}
			if (size > maxRelativeStackSize)
			{
			  maxRelativeStackSize = size;
			}
			relativeStackSize = size;
		  }
		}
	  }

	  public override void visitMethodInsn(int opcode, string owner, string name, string descriptor, bool isInterface)
	  {
		lastBytecodeOffset = code.length;
		// Add the instruction to the bytecode of the method.
		Symbol methodrefSymbol = symbolTable.addConstantMethodref(owner, name, descriptor, isInterface);
		if (opcode == Opcodes.INVOKEINTERFACE)
		{
		  code.put12(Opcodes.INVOKEINTERFACE, methodrefSymbol.index).put11(methodrefSymbol.ArgumentsAndReturnSizes >> 2, 0);
		}
		else
		{
		  code.put12(opcode, methodrefSymbol.index);
		}
		// If needed, update the maximum stack size and number of locals, and stack map frames.
		if (currentBasicBlock != null)
		{
		  if (compute == COMPUTE_ALL_FRAMES || compute == COMPUTE_INSERTED_FRAMES)
		  {
			currentBasicBlock.frame.execute(opcode, 0, methodrefSymbol, symbolTable);
		  }
		  else
		  {
			int argumentsAndReturnSize = methodrefSymbol.ArgumentsAndReturnSizes;
			int stackSizeDelta = (argumentsAndReturnSize & 3) - (argumentsAndReturnSize >> 2);
			int size;
			if (opcode == Opcodes.INVOKESTATIC)
			{
			  size = relativeStackSize + stackSizeDelta + 1;
			}
			else
			{
			  size = relativeStackSize + stackSizeDelta;
			}
			if (size > maxRelativeStackSize)
			{
			  maxRelativeStackSize = size;
			}
			relativeStackSize = size;
		  }
		}
	  }

	  public override void visitInvokeDynamicInsn(string name, string descriptor, Handle bootstrapMethodHandle, params object[] bootstrapMethodArguments)
	  {
		lastBytecodeOffset = code.length;
		// Add the instruction to the bytecode of the method.
		Symbol invokeDynamicSymbol = symbolTable.addConstantInvokeDynamic(name, descriptor, bootstrapMethodHandle, bootstrapMethodArguments);
		code.put12(Opcodes.INVOKEDYNAMIC, invokeDynamicSymbol.index);
		code.putShort(0);
		// If needed, update the maximum stack size and number of locals, and stack map frames.
		if (currentBasicBlock != null)
		{
		  if (compute == COMPUTE_ALL_FRAMES || compute == COMPUTE_INSERTED_FRAMES)
		  {
			currentBasicBlock.frame.execute(Opcodes.INVOKEDYNAMIC, 0, invokeDynamicSymbol, symbolTable);
		  }
		  else
		  {
			int argumentsAndReturnSize = invokeDynamicSymbol.ArgumentsAndReturnSizes;
			int stackSizeDelta = (argumentsAndReturnSize & 3) - (argumentsAndReturnSize >> 2) + 1;
			int size = relativeStackSize + stackSizeDelta;
			if (size > maxRelativeStackSize)
			{
			  maxRelativeStackSize = size;
			}
			relativeStackSize = size;
		  }
		}
	  }

	  public override void visitJumpInsn(int opcode, Label label)
	  {
		lastBytecodeOffset = code.length;
		// Add the instruction to the bytecode of the method.
		// Compute the 'base' opcode, i.e. GOTO or JSR if opcode is GOTO_W or JSR_W, otherwise opcode.
		int baseOpcode = opcode >= Constants.GOTO_W ? opcode - Constants.WIDE_JUMP_OPCODE_DELTA : opcode;
		bool nextInsnIsJumpTarget = false;
		if ((label.flags & Label.FLAG_RESOLVED) != 0 && label.bytecodeOffset - code.length < short.MinValue)
		{
		  // Case of a backward jump with an offset < -32768. In this case we automatically replace GOTO
		  // with GOTO_W, JSR with JSR_W and IFxxx <l> with IFNOTxxx <L> GOTO_W <l> L:..., where
		  // IFNOTxxx is the "opposite" opcode of IFxxx (e.g. IFNE for IFEQ) and where <L> designates
		  // the instruction just after the GOTO_W.
		  if (baseOpcode == Opcodes.GOTO)
		  {
			code.putByte(Constants.GOTO_W);
		  }
		  else if (baseOpcode == Opcodes.JSR)
		  {
			code.putByte(Constants.JSR_W);
		  }
		  else
		  {
			// Put the "opposite" opcode of baseOpcode. This can be done by flipping the least
			// significant bit for IFNULL and IFNONNULL, and similarly for IFEQ ... IF_ACMPEQ (with a
			// pre and post offset by 1). The jump offset is 8 bytes (3 for IFNOTxxx, 5 for GOTO_W).
			code.putByte(baseOpcode >= Opcodes.IFNULL ? baseOpcode ^ 1 : ((baseOpcode + 1) ^ 1) - 1);
			code.putShort(8);
			// Here we could put a GOTO_W in theory, but if ASM specific instructions are used in this
			// method or another one, and if the class has frames, we will need to insert a frame after
			// this GOTO_W during the additional ClassReader -> ClassWriter round trip to remove the ASM
			// specific instructions. To not miss this additional frame, we need to use an ASM_GOTO_W
			// here, which has the unfortunate effect of forcing this additional round trip (which in
			// some case would not have been really necessary, but we can't know this at this point).
			code.putByte(Constants.ASM_GOTO_W);
			hasAsmInstructions_Conflict = true;
			// The instruction after the GOTO_W becomes the target of the IFNOT instruction.
			nextInsnIsJumpTarget = true;
		  }
		  label.put(code, code.length - 1, true);
		}
		else if (baseOpcode != opcode)
		{
		  // Case of a GOTO_W or JSR_W specified by the user (normally ClassReader when used to remove
		  // ASM specific instructions). In this case we keep the original instruction.
		  code.putByte(opcode);
		  label.put(code, code.length - 1, true);
		}
		else
		{
		  // Case of a jump with an offset >= -32768, or of a jump with an unknown offset. In these
		  // cases we store the offset in 2 bytes (which will be increased via a ClassReader ->
		  // ClassWriter round trip if it turns out that 2 bytes are not sufficient).
		  code.putByte(baseOpcode);
		  label.put(code, code.length - 1, false);
		}

		// If needed, update the maximum stack size and number of locals, and stack map frames.
		if (currentBasicBlock != null)
		{
		  Label nextBasicBlock = null;
		  if (compute == COMPUTE_ALL_FRAMES)
		  {
			currentBasicBlock.frame.execute(baseOpcode, 0, null, null);
			// Record the fact that 'label' is the target of a jump instruction.
			label.CanonicalInstance.flags |= (short)Label.FLAG_JUMP_TARGET;
			// Add 'label' as a successor of the current basic block.
			addSuccessorToCurrentBasicBlock(Edge.JUMP, label);
			if (baseOpcode != Opcodes.GOTO)
			{
			  // The next instruction starts a new basic block (except for GOTO: by default the code
			  // following a goto is unreachable - unless there is an explicit label for it - and we
			  // should not compute stack frame types for its instructions).
			  nextBasicBlock = new Label();
			}
		  }
		  else if (compute == COMPUTE_INSERTED_FRAMES)
		  {
			currentBasicBlock.frame.execute(baseOpcode, 0, null, null);
		  }
		  else if (compute == COMPUTE_MAX_STACK_AND_LOCAL_FROM_FRAMES)
		  {
			// No need to update maxRelativeStackSize (the stack size delta is always negative).
			relativeStackSize += STACK_SIZE_DELTA[baseOpcode];
		  }
		  else
		  {
			if (baseOpcode == Opcodes.JSR)
			{
			  // Record the fact that 'label' designates a subroutine, if not already done.
			  if ((label.flags & Label.FLAG_SUBROUTINE_START) == 0)
			  {
				label.flags |= (short)Label.FLAG_SUBROUTINE_START;
				hasSubroutines = true;
			  }
			  currentBasicBlock.flags |= (short)Label.FLAG_SUBROUTINE_CALLER;
			  // Note that, by construction in this method, a block which calls a subroutine has at
			  // least two successors in the control flow graph: the first one (added below) leads to
			  // the instruction after the JSR, while the second one (added here) leads to the JSR
			  // target. Note that the first successor is virtual (it does not correspond to a possible
			  // execution path): it is only used to compute the successors of the basic blocks ending
			  // with a ret, in {@link Label#addSubroutineRetSuccessors}.
			  addSuccessorToCurrentBasicBlock(relativeStackSize + 1, label);
			  // The instruction after the JSR starts a new basic block.
			  nextBasicBlock = new Label();
			}
			else
			{
			  // No need to update maxRelativeStackSize (the stack size delta is always negative).
			  relativeStackSize += STACK_SIZE_DELTA[baseOpcode];
			  addSuccessorToCurrentBasicBlock(relativeStackSize, label);
			}
		  }
		  // If the next instruction starts a new basic block, call visitLabel to add the label of this
		  // instruction as a successor of the current block, and to start a new basic block.
		  if (nextBasicBlock != null)
		  {
			if (nextInsnIsJumpTarget)
			{
			  nextBasicBlock.flags |= (short)Label.FLAG_JUMP_TARGET;
			}
			visitLabel(nextBasicBlock);
		  }
		  if (baseOpcode == Opcodes.GOTO)
		  {
			endCurrentBasicBlockWithNoSuccessor();
		  }
		}
	  }

	  public override void visitLabel(Label label)
	  {
		// Resolve the forward references to this label, if any.
		hasAsmInstructions_Conflict |= label.resolve(code.data, code.length);
		// visitLabel starts a new basic block (except for debug only labels), so we need to update the
		// previous and current block references and list of successors.
		if ((label.flags & Label.FLAG_DEBUG_ONLY) != 0)
		{
		  return;
		}
		if (compute == COMPUTE_ALL_FRAMES)
		{
		  if (currentBasicBlock != null)
		  {
			if (label.bytecodeOffset == currentBasicBlock.bytecodeOffset)
			{
			  // We use {@link Label#getCanonicalInstance} to store the state of a basic block in only
			  // one place, but this does not work for labels which have not been visited yet.
			  // Therefore, when we detect here two labels having the same bytecode offset, we need to
			  // - consolidate the state scattered in these two instances into the canonical instance:
			  currentBasicBlock.flags |= (short)(label.flags & Label.FLAG_JUMP_TARGET);
			  // - make sure the two instances share the same Frame instance (the implementation of
			  // {@link Label#getCanonicalInstance} relies on this property; here label.frame should be
			  // null):
			  label.frame = currentBasicBlock.frame;
			  // - and make sure to NOT assign 'label' into 'currentBasicBlock' or 'lastBasicBlock', so
			  // that they still refer to the canonical instance for this bytecode offset.
			  return;
			}
			// End the current basic block (with one new successor).
			addSuccessorToCurrentBasicBlock(Edge.JUMP, label);
		  }
		  // Append 'label' at the end of the basic block list.
		  if (lastBasicBlock != null)
		  {
			if (label.bytecodeOffset == lastBasicBlock.bytecodeOffset)
			{
			  // Same comment as above.
			  lastBasicBlock.flags |= (short)(label.flags & Label.FLAG_JUMP_TARGET);
			  // Here label.frame should be null.
			  label.frame = lastBasicBlock.frame;
			  currentBasicBlock = lastBasicBlock;
			  return;
			}
			lastBasicBlock.nextBasicBlock = label;
		  }
		  lastBasicBlock = label;
		  // Make it the new current basic block.
		  currentBasicBlock = label;
		  // Here label.frame should be null.
		  label.frame = new Frame(label);
		}
		else if (compute == COMPUTE_INSERTED_FRAMES)
		{
		  if (currentBasicBlock == null)
		  {
			// This case should happen only once, for the visitLabel call in the constructor. Indeed, if
			// compute is equal to COMPUTE_INSERTED_FRAMES, currentBasicBlock stays unchanged.
			currentBasicBlock = label;
		  }
		  else
		  {
			// Update the frame owner so that a correct frame offset is computed in Frame.accept().
			currentBasicBlock.frame.owner = label;
		  }
		}
		else if (compute == COMPUTE_MAX_STACK_AND_LOCAL)
		{
		  if (currentBasicBlock != null)
		  {
			// End the current basic block (with one new successor).
			currentBasicBlock.outputStackMax = (short) maxRelativeStackSize;
			addSuccessorToCurrentBasicBlock(relativeStackSize, label);
		  }
		  // Start a new current basic block, and reset the current and maximum relative stack sizes.
		  currentBasicBlock = label;
		  relativeStackSize = 0;
		  maxRelativeStackSize = 0;
		  // Append the new basic block at the end of the basic block list.
		  if (lastBasicBlock != null)
		  {
			lastBasicBlock.nextBasicBlock = label;
		  }
		  lastBasicBlock = label;
		}
		else if (compute == COMPUTE_MAX_STACK_AND_LOCAL_FROM_FRAMES && currentBasicBlock == null)
		{
		  // This case should happen only once, for the visitLabel call in the constructor. Indeed, if
		  // compute is equal to COMPUTE_MAX_STACK_AND_LOCAL_FROM_FRAMES, currentBasicBlock stays
		  // unchanged.
		  currentBasicBlock = label;
		}
	  }

	  public override void visitLdcInsn(object value)
	  {
		lastBytecodeOffset = code.length;
		// Add the instruction to the bytecode of the method.
		Symbol constantSymbol = symbolTable.addConstant(value);
		int constantIndex = constantSymbol.index;
		char firstDescriptorChar;
		bool isLongOrDouble = constantSymbol.tag == Symbol.CONSTANT_LONG_TAG || constantSymbol.tag == Symbol.CONSTANT_DOUBLE_TAG || (constantSymbol.tag == Symbol.CONSTANT_DYNAMIC_TAG && ((firstDescriptorChar = constantSymbol.value[0]) == 'J' || firstDescriptorChar == 'D'));
		if (isLongOrDouble)
		{
		  code.put12(Constants.LDC2_W, constantIndex);
		}
		else if (constantIndex >= 256)
		{
		  code.put12(Constants.LDC_W, constantIndex);
		}
		else
		{
		  code.put11(Opcodes.LDC, constantIndex);
		}
		// If needed, update the maximum stack size and number of locals, and stack map frames.
		if (currentBasicBlock != null)
		{
		  if (compute == COMPUTE_ALL_FRAMES || compute == COMPUTE_INSERTED_FRAMES)
		  {
			currentBasicBlock.frame.execute(Opcodes.LDC, 0, constantSymbol, symbolTable);
		  }
		  else
		  {
			int size = relativeStackSize + (isLongOrDouble ? 2 : 1);
			if (size > maxRelativeStackSize)
			{
			  maxRelativeStackSize = size;
			}
			relativeStackSize = size;
		  }
		}
	  }

	  public override void visitIincInsn(int var, int increment)
	  {
		lastBytecodeOffset = code.length;
		// Add the instruction to the bytecode of the method.
		if ((var > 255) || (increment > 127) || (increment < -128))
		{
		  code.putByte(Constants.WIDE).put12(Opcodes.IINC, var).putShort(increment);
		}
		else
		{
		  code.putByte(Opcodes.IINC).put11(var, increment);
		}
		// If needed, update the maximum stack size and number of locals, and stack map frames.
		if (currentBasicBlock != null && (compute == COMPUTE_ALL_FRAMES || compute == COMPUTE_INSERTED_FRAMES))
		{
		  currentBasicBlock.frame.execute(Opcodes.IINC, var, null, null);
		}
		if (compute != COMPUTE_NOTHING)
		{
		  int currentMaxLocals = var + 1;
		  if (currentMaxLocals > maxLocals)
		  {
			maxLocals = currentMaxLocals;
		  }
		}
	  }

	  public override void visitTableSwitchInsn(int min, int max, Label dflt, params Label[] labels)
	  {
		lastBytecodeOffset = code.length;
		// Add the instruction to the bytecode of the method.
		code.putByte(Opcodes.TABLESWITCH).putByteArray(null, 0, (4 - code.length % 4) % 4);
		dflt.put(code, lastBytecodeOffset, true);
		code.putInt(min).putInt(max);
		foreach (Label label in labels)
		{
		  label.put(code, lastBytecodeOffset, true);
		}
		// If needed, update the maximum stack size and number of locals, and stack map frames.
		visitSwitchInsn(dflt, labels);
	  }

	  public override void visitLookupSwitchInsn(Label dflt, int[] keys, Label[] labels)
	  {
		lastBytecodeOffset = code.length;
		// Add the instruction to the bytecode of the method.
		code.putByte(Opcodes.LOOKUPSWITCH).putByteArray(null, 0, (4 - code.length % 4) % 4);
		dflt.put(code, lastBytecodeOffset, true);
		code.putInt(labels.Length);
		for (int i = 0; i < labels.Length; ++i)
		{
		  code.putInt(keys[i]);
		  labels[i].put(code, lastBytecodeOffset, true);
		}
		// If needed, update the maximum stack size and number of locals, and stack map frames.
		visitSwitchInsn(dflt, labels);
	  }

	  private void visitSwitchInsn(Label dflt, Label[] labels)
	  {
		if (currentBasicBlock != null)
		{
		  if (compute == COMPUTE_ALL_FRAMES)
		  {
			currentBasicBlock.frame.execute(Opcodes.LOOKUPSWITCH, 0, null, null);
			// Add all the labels as successors of the current basic block.
			addSuccessorToCurrentBasicBlock(Edge.JUMP, dflt);
			dflt.CanonicalInstance.flags |= (short)Label.FLAG_JUMP_TARGET;
			foreach (Label label in labels)
			{
			  addSuccessorToCurrentBasicBlock(Edge.JUMP, label);
			  label.CanonicalInstance.flags |= (short)Label.FLAG_JUMP_TARGET;
			}
		  }
		  else if (compute == COMPUTE_MAX_STACK_AND_LOCAL)
		  {
			// No need to update maxRelativeStackSize (the stack size delta is always negative).
			--relativeStackSize;
			// Add all the labels as successors of the current basic block.
			addSuccessorToCurrentBasicBlock(relativeStackSize, dflt);
			foreach (Label label in labels)
			{
			  addSuccessorToCurrentBasicBlock(relativeStackSize, label);
			}
		  }
		  // End the current basic block.
		  endCurrentBasicBlockWithNoSuccessor();
		}
	  }

	  public override void visitMultiANewArrayInsn(string descriptor, int numDimensions)
	  {
		lastBytecodeOffset = code.length;
		// Add the instruction to the bytecode of the method.
		Symbol descSymbol = symbolTable.addConstantClass(descriptor);
		code.put12(Opcodes.MULTIANEWARRAY, descSymbol.index).putByte(numDimensions);
		// If needed, update the maximum stack size and number of locals, and stack map frames.
		if (currentBasicBlock != null)
		{
		  if (compute == COMPUTE_ALL_FRAMES || compute == COMPUTE_INSERTED_FRAMES)
		  {
			currentBasicBlock.frame.execute(Opcodes.MULTIANEWARRAY, numDimensions, descSymbol, symbolTable);
		  }
		  else
		  {
			// No need to update maxRelativeStackSize (the stack size delta is always negative).
			relativeStackSize += 1 - numDimensions;
		  }
		}
	  }

	  public override AnnotationVisitor visitInsnAnnotation(int typeRef, TypePath typePath, string descriptor, bool visible)
	  {
		if (visible)
		{
		  return lastCodeRuntimeVisibleTypeAnnotation = AnnotationWriter.create(symbolTable, (typeRef & unchecked((int)0xFF0000FF)) | (lastBytecodeOffset << 8), typePath, descriptor, lastCodeRuntimeVisibleTypeAnnotation);
		}
		else
		{
		  return lastCodeRuntimeInvisibleTypeAnnotation = AnnotationWriter.create(symbolTable, (typeRef & unchecked((int)0xFF0000FF)) | (lastBytecodeOffset << 8), typePath, descriptor, lastCodeRuntimeInvisibleTypeAnnotation);
		}
	  }

	  public override void visitTryCatchBlock(Label start, Label end, Label handler, string type)
	  {
		Handler newHandler = new Handler(start, end, handler, !string.ReferenceEquals(type, null) ? symbolTable.addConstantClass(type).index : 0, type);
		if (firstHandler == null)
		{
		  firstHandler = newHandler;
		}
		else
		{
		  lastHandler.nextHandler = newHandler;
		}
		lastHandler = newHandler;
	  }

	  public override AnnotationVisitor visitTryCatchAnnotation(int typeRef, TypePath typePath, string descriptor, bool visible)
	  {
		if (visible)
		{
		  return lastCodeRuntimeVisibleTypeAnnotation = AnnotationWriter.create(symbolTable, typeRef, typePath, descriptor, lastCodeRuntimeVisibleTypeAnnotation);
		}
		else
		{
		  return lastCodeRuntimeInvisibleTypeAnnotation = AnnotationWriter.create(symbolTable, typeRef, typePath, descriptor, lastCodeRuntimeInvisibleTypeAnnotation);
		}
	  }

	  public override void visitLocalVariable(string name, string descriptor, string signature, Label start, Label end, int index)
	  {
		if (!string.ReferenceEquals(signature, null))
		{
		  if (localVariableTypeTable == null)
		  {
			localVariableTypeTable = new ByteVector();
		  }
		  ++localVariableTypeTableLength;
		  localVariableTypeTable.putShort(start.bytecodeOffset).putShort(end.bytecodeOffset - start.bytecodeOffset).putShort(symbolTable.addConstantUtf8(name)).putShort(symbolTable.addConstantUtf8(signature)).putShort(index);
		}
		if (localVariableTable == null)
		{
		  localVariableTable = new ByteVector();
		}
		++localVariableTableLength;
		localVariableTable.putShort(start.bytecodeOffset).putShort(end.bytecodeOffset - start.bytecodeOffset).putShort(symbolTable.addConstantUtf8(name)).putShort(symbolTable.addConstantUtf8(descriptor)).putShort(index);
		if (compute != COMPUTE_NOTHING)
		{
		  char firstDescChar = descriptor[0];
		  int currentMaxLocals = index + (firstDescChar == 'J' || firstDescChar == 'D' ? 2 : 1);
		  if (currentMaxLocals > maxLocals)
		  {
			maxLocals = currentMaxLocals;
		  }
		}
	  }

	  public override AnnotationVisitor visitLocalVariableAnnotation(int typeRef, TypePath typePath, Label[] start, Label[] end, int[] index, string descriptor, bool visible)
	  {
		// Create a ByteVector to hold a 'type_annotation' JVMS structure.
		// See https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.7.20.
		ByteVector typeAnnotation = new ByteVector();
		// Write target_type, target_info, and target_path.
		typeAnnotation.putByte((int)((uint)typeRef >> 24)).putShort(start.Length);
		for (int i = 0; i < start.Length; ++i)
		{
		  typeAnnotation.putShort(start[i].bytecodeOffset).putShort(end[i].bytecodeOffset - start[i].bytecodeOffset).putShort(index[i]);
		}
		TypePath.put(typePath, typeAnnotation);
		// Write type_index and reserve space for num_element_value_pairs.
		typeAnnotation.putShort(symbolTable.addConstantUtf8(descriptor)).putShort(0);
		if (visible)
		{
		  return lastCodeRuntimeVisibleTypeAnnotation = new AnnotationWriter(symbolTable, true, typeAnnotation, lastCodeRuntimeVisibleTypeAnnotation);
		}
		else
		{
		  return lastCodeRuntimeInvisibleTypeAnnotation = new AnnotationWriter(symbolTable, true, typeAnnotation, lastCodeRuntimeInvisibleTypeAnnotation);
		}
	  }

	  public override void visitLineNumber(int line, Label start)
	  {
		if (lineNumberTable == null)
		{
		  lineNumberTable = new ByteVector();
		}
		++lineNumberTableLength;
		lineNumberTable.putShort(start.bytecodeOffset);
		lineNumberTable.putShort(line);
	  }

	  public override void visitMaxs(int maxStack, int maxLocals)
	  {
		if (compute == COMPUTE_ALL_FRAMES)
		{
		  computeAllFrames();
		}
		else if (compute == COMPUTE_MAX_STACK_AND_LOCAL)
		{
		  computeMaxStackAndLocal();
		}
		else if (compute == COMPUTE_MAX_STACK_AND_LOCAL_FROM_FRAMES)
		{
		  this.maxStack = maxRelativeStackSize;
		}
		else
		{
		  this.maxStack = maxStack;
		  this.maxLocals = maxLocals;
		}
	  }

	  /// <summary>
	  /// Computes all the stack map frames of the method, from scratch. </summary>
	  private void computeAllFrames()
	  {
		// Complete the control flow graph with exception handler blocks.
		Handler handler = firstHandler;
		while (handler != null)
		{
		  string catchTypeDescriptor = string.ReferenceEquals(handler.catchTypeDescriptor, null) ? "java/lang/Throwable" : handler.catchTypeDescriptor;
		  int catchType = Frame.getAbstractTypeFromInternalName(symbolTable, catchTypeDescriptor);
		  // Mark handlerBlock as an exception handler.
		  Label handlerBlock = handler.handlerPc.CanonicalInstance;
		  handlerBlock.flags |= (short)Label.FLAG_JUMP_TARGET;
		  // Add handlerBlock as a successor of all the basic blocks in the exception handler range.
		  Label handlerRangeBlock = handler.startPc.CanonicalInstance;
		  Label handlerRangeEnd = handler.endPc.CanonicalInstance;
		  while (handlerRangeBlock != handlerRangeEnd)
		  {
			handlerRangeBlock.outgoingEdges = new Edge(catchType, handlerBlock, handlerRangeBlock.outgoingEdges);
			handlerRangeBlock = handlerRangeBlock.nextBasicBlock;
		  }
		  handler = handler.nextHandler;
		}

		// Create and visit the first (implicit) frame.
		Frame firstFrame = firstBasicBlock.frame;
		firstFrame.setInputFrameFromDescriptor(symbolTable, accessFlags, descriptor, this.maxLocals);
		firstFrame.accept(this);

		// Fix point algorithm: add the first basic block to a list of blocks to process (i.e. blocks
		// whose stack map frame has changed) and, while there are blocks to process, remove one from
		// the list and update the stack map frames of its successor blocks in the control flow graph
		// (which might change them, in which case these blocks must be processed too, and are thus
		// added to the list of blocks to process). Also compute the maximum stack size of the method,
		// as a by-product.
		Label listOfBlocksToProcess = firstBasicBlock;
		listOfBlocksToProcess.nextListElement = Label.EMPTY_LIST;
		int maxStackSize = 0;
        Label basicBlock;
        while (listOfBlocksToProcess != Label.EMPTY_LIST)
		{
		  // Remove a basic block from the list of blocks to process.
		  basicBlock = listOfBlocksToProcess;
		  listOfBlocksToProcess = listOfBlocksToProcess.nextListElement;
		  basicBlock.nextListElement = null;
		  // By definition, basicBlock is reachable.
		  basicBlock.flags |= (short)Label.FLAG_REACHABLE;
		  // Update the (absolute) maximum stack size.
		  int maxBlockStackSize = basicBlock.frame.InputStackSize + basicBlock.outputStackMax;
		  if (maxBlockStackSize > maxStackSize)
		  {
			maxStackSize = maxBlockStackSize;
		  }
		  // Update the successor blocks of basicBlock in the control flow graph.
		  Edge outgoingEdge = basicBlock.outgoingEdges;
		  while (outgoingEdge != null)
		  {
			Label successorBlock = outgoingEdge.successor.CanonicalInstance;
			bool successorBlockChanged = basicBlock.frame.merge(symbolTable, successorBlock.frame, outgoingEdge.info);
			if (successorBlockChanged && successorBlock.nextListElement == null)
			{
			  // If successorBlock has changed it must be processed. Thus, if it is not already in the
			  // list of blocks to process, add it to this list.
			  successorBlock.nextListElement = listOfBlocksToProcess;
			  listOfBlocksToProcess = successorBlock;
			}
			outgoingEdge = outgoingEdge.nextEdge;
		  }
		}

		// Loop over all the basic blocks and visit the stack map frames that must be stored in the
		// StackMapTable attribute. Also replace unreachable code with NOP* ATHROW, and remove it from
		// exception handler ranges.
		basicBlock = firstBasicBlock;
		while (basicBlock != null)
		{
		  if ((basicBlock.flags & (Label.FLAG_JUMP_TARGET | Label.FLAG_REACHABLE)) == (Label.FLAG_JUMP_TARGET | Label.FLAG_REACHABLE))
		  {
			basicBlock.frame.accept(this);
		  }
		  if ((basicBlock.flags & Label.FLAG_REACHABLE) == 0)
		  {
			// Find the start and end bytecode offsets of this unreachable block.
			Label nextBasicBlock = basicBlock.nextBasicBlock;
			int startOffset = basicBlock.bytecodeOffset;
			int endOffset = (nextBasicBlock == null ? code.length : nextBasicBlock.bytecodeOffset) - 1;
			if (endOffset >= startOffset)
			{
			  // Replace its instructions with NOP ... NOP ATHROW.
			  for (int i = startOffset; i < endOffset; ++i)
			  {
				code.data[i] = Opcodes.NOP;
			  }
			  code.data[endOffset] = unchecked((sbyte) Opcodes.ATHROW);
			  // Emit a frame for this unreachable block, with no local and a Throwable on the stack
			  // (so that the ATHROW could consume this Throwable if it were reachable).
			  int frameIndex = visitFrameStart(startOffset, 0, 1);
			  currentFrame[frameIndex] = Frame.getAbstractTypeFromInternalName(symbolTable, "java/lang/Throwable");
			  visitFrameEnd();
			  // Remove this unreachable basic block from the exception handler ranges.
			  firstHandler = Handler.removeRange(firstHandler, basicBlock, nextBasicBlock);
			  // The maximum stack size is now at least one, because of the Throwable declared above.
			  maxStackSize = Math.Max(maxStackSize, 1);
			}
		  }
		  basicBlock = basicBlock.nextBasicBlock;
		}

		this.maxStack = maxStackSize;
	  }

	  /// <summary>
	  /// Computes the maximum stack size of the method. </summary>
	  private void computeMaxStackAndLocal()
	  {
		// Complete the control flow graph with exception handler blocks.
		Handler handler = firstHandler;
		while (handler != null)
		{
		  Label handlerBlock = handler.handlerPc;
		  Label handlerRangeBlock = handler.startPc;
		  Label handlerRangeEnd = handler.endPc;
		  // Add handlerBlock as a successor of all the basic blocks in the exception handler range.
		  while (handlerRangeBlock != handlerRangeEnd)
		  {
			if ((handlerRangeBlock.flags & Label.FLAG_SUBROUTINE_CALLER) == 0)
			{
			  handlerRangeBlock.outgoingEdges = new Edge(Edge.EXCEPTION, handlerBlock, handlerRangeBlock.outgoingEdges);
			}
			else
			{
			  // If handlerRangeBlock is a JSR block, add handlerBlock after the first two outgoing
			  // edges to preserve the hypothesis about JSR block successors order (see
			  // {@link #visitJumpInsn}).
			  handlerRangeBlock.outgoingEdges.nextEdge.nextEdge = new Edge(Edge.EXCEPTION, handlerBlock, handlerRangeBlock.outgoingEdges.nextEdge.nextEdge);
			}
			handlerRangeBlock = handlerRangeBlock.nextBasicBlock;
		  }
		  handler = handler.nextHandler;
		}

		// Complete the control flow graph with the successor blocks of subroutines, if needed.
		if (hasSubroutines)
		{
		  // First step: find the subroutines. This step determines, for each basic block, to which
		  // subroutine(s) it belongs. Start with the main "subroutine":
		  short numSubroutines = 1;
		  firstBasicBlock.markSubroutine(numSubroutines);
		  // Then, mark the subroutines called by the main subroutine, then the subroutines called by
		  // those called by the main subroutine, etc.
          Label basicBlock;
          for (short currentSubroutine = 1; currentSubroutine <= numSubroutines; ++currentSubroutine)
          {
              basicBlock = firstBasicBlock;
              while (basicBlock != null)
			{
			  if ((basicBlock.flags & Label.FLAG_SUBROUTINE_CALLER) != 0 && basicBlock.subroutineId == currentSubroutine)
			  {
				Label jsrTarget = basicBlock.outgoingEdges.nextEdge.successor;
				if (jsrTarget.subroutineId == 0)
				{
				  // If this subroutine has not been marked yet, find its basic blocks.
				  jsrTarget.markSubroutine(++numSubroutines);
				}
			  }
			  basicBlock = basicBlock.nextBasicBlock;
			}
          }
		  // Second step: find the successors in the control flow graph of each subroutine basic block
		  // 'r' ending with a RET instruction. These successors are the virtual successors of the basic
		  // blocks ending with JSR instructions (see {@link #visitJumpInsn)} that can reach 'r'.
		  basicBlock = firstBasicBlock;
		  while (basicBlock != null)
		  {
			if ((basicBlock.flags & Label.FLAG_SUBROUTINE_CALLER) != 0)
			{
			  // By construction, jsr targets are stored in the second outgoing edge of basic blocks
			  // that ends with a jsr instruction (see {@link #FLAG_SUBROUTINE_CALLER}).
			  Label subroutine = basicBlock.outgoingEdges.nextEdge.successor;
			  subroutine.addSubroutineRetSuccessors(basicBlock);
			}
			basicBlock = basicBlock.nextBasicBlock;
		  }
		}

		// Data flow algorithm: put the first basic block in a list of blocks to process (i.e. blocks
		// whose input stack size has changed) and, while there are blocks to process, remove one
		// from the list, update the input stack size of its successor blocks in the control flow
		// graph, and add these blocks to the list of blocks to process (if not already done).
		Label listOfBlocksToProcess = firstBasicBlock;
		listOfBlocksToProcess.nextListElement = Label.EMPTY_LIST;
		int maxStackSize = maxStack;
		while (listOfBlocksToProcess != Label.EMPTY_LIST)
		{
		  // Remove a basic block from the list of blocks to process. Note that we don't reset
		  // basicBlock.nextListElement to null on purpose, to make sure we don't reprocess already
		  // processed basic blocks.
		  Label basicBlock = listOfBlocksToProcess;
		  listOfBlocksToProcess = listOfBlocksToProcess.nextListElement;
		  // Compute the (absolute) input stack size and maximum stack size of this block.
		  int inputStackTop = basicBlock.inputStackSize;
		  int maxBlockStackSize = inputStackTop + basicBlock.outputStackMax;
		  // Update the absolute maximum stack size of the method.
		  if (maxBlockStackSize > maxStackSize)
		  {
			maxStackSize = maxBlockStackSize;
		  }
		  // Update the input stack size of the successor blocks of basicBlock in the control flow
		  // graph, and add these blocks to the list of blocks to process, if not already done.
		  Edge outgoingEdge = basicBlock.outgoingEdges;
		  if ((basicBlock.flags & Label.FLAG_SUBROUTINE_CALLER) != 0)
		  {
			// Ignore the first outgoing edge of the basic blocks ending with a jsr: these are virtual
			// edges which lead to the instruction just after the jsr, and do not correspond to a
			// possible execution path (see {@link #visitJumpInsn} and
			// {@link Label#FLAG_SUBROUTINE_CALLER}).
			outgoingEdge = outgoingEdge.nextEdge;
		  }
		  while (outgoingEdge != null)
		  {
			Label successorBlock = outgoingEdge.successor;
			if (successorBlock.nextListElement == null)
			{
			  successorBlock.inputStackSize = (short)(outgoingEdge.info == Edge.EXCEPTION ? 1 : inputStackTop + outgoingEdge.info);
			  successorBlock.nextListElement = listOfBlocksToProcess;
			  listOfBlocksToProcess = successorBlock;
			}
			outgoingEdge = outgoingEdge.nextEdge;
		  }
		}
		this.maxStack = maxStackSize;
	  }

	  public override void visitEnd()
	  {
		// Nothing to do.
	  }

	  // -----------------------------------------------------------------------------------------------
	  // Utility methods: control flow analysis algorithm
	  // -----------------------------------------------------------------------------------------------

	  /// <summary>
	  /// Adds a successor to <seealso cref="currentBasicBlock"/> in the control flow graph.
	  /// </summary>
	  /// <param name="info"> information about the control flow edge to be added. </param>
	  /// <param name="successor"> the successor block to be added to the current basic block. </param>
	  private void addSuccessorToCurrentBasicBlock(int info, Label successor)
	  {
		currentBasicBlock.outgoingEdges = new Edge(info, successor, currentBasicBlock.outgoingEdges);
	  }

	  /// <summary>
	  /// Ends the current basic block. This method must be used in the case where the current basic
	  /// block does not have any successor.
	  /// 
	  /// <para>WARNING: this method must be called after the currently visited instruction has been put in
	  /// <seealso cref="code"/> (if frames are computed, this method inserts a new Label to start a new basic
	  /// block after the current instruction).
	  /// </para>
	  /// </summary>
	  private void endCurrentBasicBlockWithNoSuccessor()
	  {
		if (compute == COMPUTE_ALL_FRAMES)
		{
		  Label nextBasicBlock = new Label();
		  nextBasicBlock.frame = new Frame(nextBasicBlock);
		  nextBasicBlock.resolve(code.data, code.length);
		  lastBasicBlock.nextBasicBlock = nextBasicBlock;
		  lastBasicBlock = nextBasicBlock;
		  currentBasicBlock = null;
		}
		else if (compute == COMPUTE_MAX_STACK_AND_LOCAL)
		{
		  currentBasicBlock.outputStackMax = (short) maxRelativeStackSize;
		  currentBasicBlock = null;
		}
	  }

	  // -----------------------------------------------------------------------------------------------
	  // Utility methods: stack map frames
	  // -----------------------------------------------------------------------------------------------

	  /// <summary>
	  /// Starts the visit of a new stack map frame, stored in <seealso cref="currentFrame"/>.
	  /// </summary>
	  /// <param name="offset"> the bytecode offset of the instruction to which the frame corresponds. </param>
	  /// <param name="numLocal"> the number of local variables in the frame. </param>
	  /// <param name="numStack"> the number of stack elements in the frame. </param>
	  /// <returns> the index of the next element to be written in this frame. </returns>
	  public int visitFrameStart(int offset, int numLocal, int numStack)
	  {
		int frameLength = 3 + numLocal + numStack;
		if (currentFrame == null || currentFrame.Length < frameLength)
		{
		  currentFrame = new int[frameLength];
		}
		currentFrame[0] = offset;
		currentFrame[1] = numLocal;
		currentFrame[2] = numStack;
		return 3;
	  }

	  /// <summary>
	  /// Sets an abstract type in <seealso cref="currentFrame"/>.
	  /// </summary>
	  /// <param name="frameIndex"> the index of the element to be set in <seealso cref="currentFrame"/>. </param>
	  /// <param name="abstractType"> an abstract type. </param>
	  public void visitAbstractType(int frameIndex, int abstractType)
	  {
		currentFrame[frameIndex] = abstractType;
	  }

	  /// <summary>
	  /// Ends the visit of <seealso cref="currentFrame"/> by writing it in the StackMapTable entries and by
	  /// updating the StackMapTable number_of_entries (except if the current frame is the first one,
	  /// which is implicit in StackMapTable). Then resets <seealso cref="currentFrame"/> to {@literal null}.
	  /// </summary>
	  public void visitFrameEnd()
	  {
		if (previousFrame != null)
		{
		  if (stackMapTableEntries == null)
		  {
			stackMapTableEntries = new ByteVector();
		  }
		  putFrame();
		  ++stackMapTableNumberOfEntries;
		}
		previousFrame = currentFrame;
		currentFrame = null;
	  }

	  /// <summary>
	  /// Compresses and writes <seealso cref="currentFrame"/> in a new StackMapTable entry. </summary>
	  private void putFrame()
	  {
		int numLocal = currentFrame[1];
		int numStack = currentFrame[2];
		if (symbolTable.MajorVersion < Opcodes.V1_6)
		{
		  // Generate a StackMap attribute entry, which are always uncompressed.
		  stackMapTableEntries.putShort(currentFrame[0]).putShort(numLocal);
		  putAbstractTypes(3, 3 + numLocal);
		  stackMapTableEntries.putShort(numStack);
		  putAbstractTypes(3 + numLocal, 3 + numLocal + numStack);
		  return;
		}
		int offsetDelta = stackMapTableNumberOfEntries == 0 ? currentFrame[0] : currentFrame[0] - previousFrame[0] - 1;
		int previousNumlocal = previousFrame[1];
		int numLocalDelta = numLocal - previousNumlocal;
		int type = Frame.FULL_FRAME;
		if (numStack == 0)
		{
		  switch (numLocalDelta)
		  {
			case -3:
			case -2:
			case -1:
			  type = Frame.CHOP_FRAME;
			  break;
			case 0:
			  type = offsetDelta < 64 ? Frame.SAME_FRAME : Frame.SAME_FRAME_EXTENDED;
			  break;
			case 1:
			case 2:
			case 3:
			  type = Frame.APPEND_FRAME;
			  break;
			default:
			  // Keep the FULL_FRAME type.
			  break;
		  }
		}
		else if (numLocalDelta == 0 && numStack == 1)
		{
		  type = offsetDelta < 63 ? Frame.SAME_LOCALS_1_STACK_ITEM_FRAME : Frame.SAME_LOCALS_1_STACK_ITEM_FRAME_EXTENDED;
		}
		if (type != Frame.FULL_FRAME)
		{
		  // Verify if locals are the same as in the previous frame.
		  int frameIndex = 3;
		  for (int i = 0; i < previousNumlocal && i < numLocal; i++)
		  {
			if (currentFrame[frameIndex] != previousFrame[frameIndex])
			{
			  type = Frame.FULL_FRAME;
			  break;
			}
			frameIndex++;
		  }
		}
		switch (type)
		{
		  case Frame.SAME_FRAME:
			stackMapTableEntries.putByte(offsetDelta);
			break;
		  case Frame.SAME_LOCALS_1_STACK_ITEM_FRAME:
			stackMapTableEntries.putByte(Frame.SAME_LOCALS_1_STACK_ITEM_FRAME + offsetDelta);
			putAbstractTypes(3 + numLocal, 4 + numLocal);
			break;
		  case Frame.SAME_LOCALS_1_STACK_ITEM_FRAME_EXTENDED:
			stackMapTableEntries.putByte(Frame.SAME_LOCALS_1_STACK_ITEM_FRAME_EXTENDED).putShort(offsetDelta);
			putAbstractTypes(3 + numLocal, 4 + numLocal);
			break;
		  case Frame.SAME_FRAME_EXTENDED:
			stackMapTableEntries.putByte(Frame.SAME_FRAME_EXTENDED).putShort(offsetDelta);
			break;
		  case Frame.CHOP_FRAME:
			stackMapTableEntries.putByte(Frame.SAME_FRAME_EXTENDED + numLocalDelta).putShort(offsetDelta);
			break;
		  case Frame.APPEND_FRAME:
			stackMapTableEntries.putByte(Frame.SAME_FRAME_EXTENDED + numLocalDelta).putShort(offsetDelta);
			putAbstractTypes(3 + previousNumlocal, 3 + numLocal);
			break;
		  case Frame.FULL_FRAME:
		  default:
			stackMapTableEntries.putByte(Frame.FULL_FRAME).putShort(offsetDelta).putShort(numLocal);
			putAbstractTypes(3, 3 + numLocal);
			stackMapTableEntries.putShort(numStack);
			putAbstractTypes(3 + numLocal, 3 + numLocal + numStack);
			break;
		}
	  }

	  /// <summary>
	  /// Puts some abstract types of <seealso cref="currentFrame"/> in <seealso cref="stackMapTableEntries"/> , using the
	  /// JVMS verification_type_info format used in StackMapTable attributes.
	  /// </summary>
	  /// <param name="start"> index of the first type in <seealso cref="currentFrame"/> to write. </param>
	  /// <param name="end"> index of last type in <seealso cref="currentFrame"/> to write (exclusive). </param>
	  private void putAbstractTypes(int start, int end)
	  {
		for (int i = start; i < end; ++i)
		{
		  Frame.putAbstractType(symbolTable, currentFrame[i], stackMapTableEntries);
		}
	  }

	  /// <summary>
	  /// Puts the given public API frame element type in <seealso cref="stackMapTableEntries"/> , using the JVMS
	  /// verification_type_info format used in StackMapTable attributes.
	  /// </summary>
	  /// <param name="type"> a frame element type described using the same format as in {@link
	  ///     MethodVisitor#visitFrame}, i.e. either <seealso cref="Opcodes.TOP"/>, <seealso cref="Opcodes.INTEGER"/>, {@link
	  ///     Opcodes#FLOAT}, <seealso cref="Opcodes.LONG"/>, <seealso cref="Opcodes.DOUBLE"/>, <seealso cref="Opcodes.NULL"/>, or
	  ///     <seealso cref="Opcodes.UNINITIALIZED_THIS"/>, or the internal name of a class, or a Label designating
	  ///     a NEW instruction (for uninitialized types). </param>
	  private void putFrameType(object type)
	  {
		if (type is int)
		{
		  stackMapTableEntries.putByte(((int?) type).Value);
		}
		else if (type is string)
		{
		  stackMapTableEntries.putByte(Frame.ITEM_OBJECT).putShort(symbolTable.addConstantClass((string) type).index);
		}
		else
		{
		  stackMapTableEntries.putByte(Frame.ITEM_UNINITIALIZED).putShort(((Label) type).bytecodeOffset);
		}
	  }

	  // -----------------------------------------------------------------------------------------------
	  // Utility methods
	  // -----------------------------------------------------------------------------------------------

	  /// <summary>
	  /// Returns whether the attributes of this method can be copied from the attributes of the given
	  /// method (assuming there is no method visitor between the given ClassReader and this
	  /// MethodWriter). This method should only be called just after this MethodWriter has been created,
	  /// and before any content is visited. It returns true if the attributes corresponding to the
	  /// constructor arguments (at most a Signature, an Exception, a Deprecated and a Synthetic
	  /// attribute) are the same as the corresponding attributes in the given method.
	  /// </summary>
	  /// <param name="source"> the source ClassReader from which the attributes of this method might be copied. </param>
	  /// <param name="hasSyntheticAttribute"> whether the method_info JVMS structure from which the attributes
	  ///     of this method might be copied contains a Synthetic attribute. </param>
	  /// <param name="hasDeprecatedAttribute"> whether the method_info JVMS structure from which the attributes
	  ///     of this method might be copied contains a Deprecated attribute. </param>
	  /// <param name="descriptorIndex"> the descriptor_index field of the method_info JVMS structure from which
	  ///     the attributes of this method might be copied. </param>
	  /// <param name="signatureIndex"> the constant pool index contained in the Signature attribute of the
	  ///     method_info JVMS structure from which the attributes of this method might be copied, or 0. </param>
	  /// <param name="exceptionsOffset"> the offset in 'source.b' of the Exceptions attribute of the method_info
	  ///     JVMS structure from which the attributes of this method might be copied, or 0. </param>
	  /// <returns> whether the attributes of this method can be copied from the attributes of the
	  ///     method_info JVMS structure in 'source.b', between 'methodInfoOffset' and 'methodInfoOffset'
	  ///     + 'methodInfoLength'. </returns>
	  public bool canCopyMethodAttributes(ClassReader source, bool hasSyntheticAttribute, bool hasDeprecatedAttribute, int descriptorIndex, int signatureIndex, int exceptionsOffset)
	  {
		// If the method descriptor has changed, with more locals than the max_locals field of the
		// original Code attribute, if any, then the original method attributes can't be copied. A
		// conservative check on the descriptor changes alone ensures this (being more precise is not
		// worth the additional complexity, because these cases should be rare -- if a transform changes
		// a method descriptor, most of the time it needs to change the method's code too).
		if (source != symbolTable.Source || descriptorIndex != this.descriptorIndex || signatureIndex != this.signatureIndex || hasDeprecatedAttribute != ((accessFlags & Opcodes.ACC_DEPRECATED) != 0))
		{
		  return false;
		}
		bool needSyntheticAttribute = symbolTable.MajorVersion < Opcodes.V1_5 && (accessFlags & Opcodes.ACC_SYNTHETIC) != 0;
		if (hasSyntheticAttribute != needSyntheticAttribute)
		{
		  return false;
		}
		if (exceptionsOffset == 0)
		{
		  if (numberOfExceptions != 0)
		  {
			return false;
		  }
		}
		else if (source.readUnsignedShort(exceptionsOffset) == numberOfExceptions)
		{
		  int currentExceptionOffset = exceptionsOffset + 2;
		  for (int i = 0; i < numberOfExceptions; ++i)
		  {
			if (source.readUnsignedShort(currentExceptionOffset) != exceptionIndexTable[i])
			{
			  return false;
			}
			currentExceptionOffset += 2;
		  }
		}
		return true;
	  }

	  /// <summary>
	  /// Sets the source from which the attributes of this method will be copied.
	  /// </summary>
	  /// <param name="methodInfoOffset"> the offset in 'symbolTable.getSource()' of the method_info JVMS
	  ///     structure from which the attributes of this method will be copied. </param>
	  /// <param name="methodInfoLength"> the length in 'symbolTable.getSource()' of the method_info JVMS
	  ///     structure from which the attributes of this method will be copied. </param>
	  public void setMethodAttributesSource(int methodInfoOffset, int methodInfoLength)
	  {
		// Don't copy the attributes yet, instead store their location in the source class reader so
		// they can be copied later, in {@link #putMethodInfo}. Note that we skip the 6 header bytes
		// of the method_info JVMS structure.
		this.sourceOffset = methodInfoOffset + 6;
		this.sourceLength = methodInfoLength - 6;
	  }

	  /// <summary>
	  /// Returns the size of the method_info JVMS structure generated by this MethodWriter. Also add the
	  /// names of the attributes of this method in the constant pool.
	  /// </summary>
	  /// <returns> the size in bytes of the method_info JVMS structure. </returns>
	  public int computeMethodInfoSize()
	  {
		// If this method_info must be copied from an existing one, the size computation is trivial.
		if (sourceOffset != 0)
		{
		  // sourceLength excludes the first 6 bytes for access_flags, name_index and descriptor_index.
		  return 6 + sourceLength;
		}
		// 2 bytes each for access_flags, name_index, descriptor_index and attributes_count.
		int size = 8;
		// For ease of reference, we use here the same attribute order as in Section 4.7 of the JVMS.
		if (code.length > 0)
		{
		  if (code.length > 65535)
		  {
			throw new MethodTooLargeException(symbolTable.ClassName, name, descriptor, code.length);
		  }
		  symbolTable.addConstantUtf8(Constants.CODE);
		  // The Code attribute has 6 header bytes, plus 2, 2, 4 and 2 bytes respectively for max_stack,
		  // max_locals, code_length and attributes_count, plus the bytecode and the exception table.
		  size += 16 + code.length + Handler.getExceptionTableSize(firstHandler);
		  if (stackMapTableEntries != null)
		  {
			bool useStackMapTable = symbolTable.MajorVersion >= Opcodes.V1_6;
			symbolTable.addConstantUtf8(useStackMapTable ? Constants.STACK_MAP_TABLE : "StackMap");
			// 6 header bytes and 2 bytes for number_of_entries.
			size += 8 + stackMapTableEntries.length;
		  }
		  if (lineNumberTable != null)
		  {
			symbolTable.addConstantUtf8(Constants.LINE_NUMBER_TABLE);
			// 6 header bytes and 2 bytes for line_number_table_length.
			size += 8 + lineNumberTable.length;
		  }
		  if (localVariableTable != null)
		  {
			symbolTable.addConstantUtf8(Constants.LOCAL_VARIABLE_TABLE);
			// 6 header bytes and 2 bytes for local_variable_table_length.
			size += 8 + localVariableTable.length;
		  }
		  if (localVariableTypeTable != null)
		  {
			symbolTable.addConstantUtf8(Constants.LOCAL_VARIABLE_TYPE_TABLE);
			// 6 header bytes and 2 bytes for local_variable_type_table_length.
			size += 8 + localVariableTypeTable.length;
		  }
		  if (lastCodeRuntimeVisibleTypeAnnotation != null)
		  {
			size += lastCodeRuntimeVisibleTypeAnnotation.computeAnnotationsSize(Constants.RUNTIME_VISIBLE_TYPE_ANNOTATIONS);
		  }
		  if (lastCodeRuntimeInvisibleTypeAnnotation != null)
		  {
			size += lastCodeRuntimeInvisibleTypeAnnotation.computeAnnotationsSize(Constants.RUNTIME_INVISIBLE_TYPE_ANNOTATIONS);
		  }
		  if (firstCodeAttribute != null)
		  {
			size += firstCodeAttribute.computeAttributesSize(symbolTable, code.data, code.length, maxStack, maxLocals);
		  }
		}
		if (numberOfExceptions > 0)
		{
		  symbolTable.addConstantUtf8(Constants.EXCEPTIONS);
		  size += 8 + 2 * numberOfExceptions;
		}
		size += Attribute.computeAttributesSize(symbolTable, accessFlags, signatureIndex);
		size += AnnotationWriter.computeAnnotationsSize(lastRuntimeVisibleAnnotation, lastRuntimeInvisibleAnnotation, lastRuntimeVisibleTypeAnnotation, lastRuntimeInvisibleTypeAnnotation);
		if (lastRuntimeVisibleParameterAnnotations != null)
		{
		  size += AnnotationWriter.computeParameterAnnotationsSize(Constants.RUNTIME_VISIBLE_PARAMETER_ANNOTATIONS, lastRuntimeVisibleParameterAnnotations, visibleAnnotableParameterCount == 0 ? lastRuntimeVisibleParameterAnnotations.Length : visibleAnnotableParameterCount);
		}
		if (lastRuntimeInvisibleParameterAnnotations != null)
		{
		  size += AnnotationWriter.computeParameterAnnotationsSize(Constants.RUNTIME_INVISIBLE_PARAMETER_ANNOTATIONS, lastRuntimeInvisibleParameterAnnotations, invisibleAnnotableParameterCount == 0 ? lastRuntimeInvisibleParameterAnnotations.Length : invisibleAnnotableParameterCount);
		}
		if (defaultValue != null)
		{
		  symbolTable.addConstantUtf8(Constants.ANNOTATION_DEFAULT);
		  size += 6 + defaultValue.length;
		}
		if (parameters != null)
		{
		  symbolTable.addConstantUtf8(Constants.METHOD_PARAMETERS);
		  // 6 header bytes and 1 byte for parameters_count.
		  size += 7 + parameters.length;
		}
		if (firstAttribute != null)
		{
		  size += firstAttribute.computeAttributesSize(symbolTable);
		}
		return size;
	  }

	  /// <summary>
	  /// Puts the content of the method_info JVMS structure generated by this MethodWriter into the
	  /// given ByteVector.
	  /// </summary>
	  /// <param name="output"> where the method_info structure must be put. </param>
	  public void putMethodInfo(ByteVector output)
	  {
		bool useSyntheticAttribute = symbolTable.MajorVersion < Opcodes.V1_5;
		int mask = useSyntheticAttribute ? Opcodes.ACC_SYNTHETIC : 0;
		output.putShort(accessFlags & ~mask).putShort(nameIndex).putShort(descriptorIndex);
		// If this method_info must be copied from an existing one, copy it now and return early.
		if (sourceOffset != 0)
		{
		  output.putByteArray(symbolTable.Source.classFileBuffer, sourceOffset, sourceLength);
		  return;
		}
		// For ease of reference, we use here the same attribute order as in Section 4.7 of the JVMS.
		int attributeCount = 0;
		if (code.length > 0)
		{
		  ++attributeCount;
		}
		if (numberOfExceptions > 0)
		{
		  ++attributeCount;
		}
		if ((accessFlags & Opcodes.ACC_SYNTHETIC) != 0 && useSyntheticAttribute)
		{
		  ++attributeCount;
		}
		if (signatureIndex != 0)
		{
		  ++attributeCount;
		}
		if ((accessFlags & Opcodes.ACC_DEPRECATED) != 0)
		{
		  ++attributeCount;
		}
		if (lastRuntimeVisibleAnnotation != null)
		{
		  ++attributeCount;
		}
		if (lastRuntimeInvisibleAnnotation != null)
		{
		  ++attributeCount;
		}
		if (lastRuntimeVisibleParameterAnnotations != null)
		{
		  ++attributeCount;
		}
		if (lastRuntimeInvisibleParameterAnnotations != null)
		{
		  ++attributeCount;
		}
		if (lastRuntimeVisibleTypeAnnotation != null)
		{
		  ++attributeCount;
		}
		if (lastRuntimeInvisibleTypeAnnotation != null)
		{
		  ++attributeCount;
		}
		if (defaultValue != null)
		{
		  ++attributeCount;
		}
		if (parameters != null)
		{
		  ++attributeCount;
		}
		if (firstAttribute != null)
		{
		  attributeCount += firstAttribute.AttributeCount;
		}
		// For ease of reference, we use here the same attribute order as in Section 4.7 of the JVMS.
		output.putShort(attributeCount);
		if (code.length > 0)
		{
		  // 2, 2, 4 and 2 bytes respectively for max_stack, max_locals, code_length and
		  // attributes_count, plus the bytecode and the exception table.
		  int size = 10 + code.length + Handler.getExceptionTableSize(firstHandler);
		  int codeAttributeCount = 0;
		  if (stackMapTableEntries != null)
		  {
			// 6 header bytes and 2 bytes for number_of_entries.
			size += 8 + stackMapTableEntries.length;
			++codeAttributeCount;
		  }
		  if (lineNumberTable != null)
		  {
			// 6 header bytes and 2 bytes for line_number_table_length.
			size += 8 + lineNumberTable.length;
			++codeAttributeCount;
		  }
		  if (localVariableTable != null)
		  {
			// 6 header bytes and 2 bytes for local_variable_table_length.
			size += 8 + localVariableTable.length;
			++codeAttributeCount;
		  }
		  if (localVariableTypeTable != null)
		  {
			// 6 header bytes and 2 bytes for local_variable_type_table_length.
			size += 8 + localVariableTypeTable.length;
			++codeAttributeCount;
		  }
		  if (lastCodeRuntimeVisibleTypeAnnotation != null)
		  {
			size += lastCodeRuntimeVisibleTypeAnnotation.computeAnnotationsSize(Constants.RUNTIME_VISIBLE_TYPE_ANNOTATIONS);
			++codeAttributeCount;
		  }
		  if (lastCodeRuntimeInvisibleTypeAnnotation != null)
		  {
			size += lastCodeRuntimeInvisibleTypeAnnotation.computeAnnotationsSize(Constants.RUNTIME_INVISIBLE_TYPE_ANNOTATIONS);
			++codeAttributeCount;
		  }
		  if (firstCodeAttribute != null)
		  {
			size += firstCodeAttribute.computeAttributesSize(symbolTable, code.data, code.length, maxStack, maxLocals);
			codeAttributeCount += firstCodeAttribute.AttributeCount;
		  }
		  output.putShort(symbolTable.addConstantUtf8(Constants.CODE)).putInt(size).putShort(maxStack).putShort(maxLocals).putInt(code.length).putByteArray(code.data, 0, code.length);
		  Handler.putExceptionTable(firstHandler, output);
		  output.putShort(codeAttributeCount);
		  if (stackMapTableEntries != null)
		  {
			bool useStackMapTable = symbolTable.MajorVersion >= Opcodes.V1_6;
			output.putShort(symbolTable.addConstantUtf8(useStackMapTable ? Constants.STACK_MAP_TABLE : "StackMap")).putInt(2 + stackMapTableEntries.length).putShort(stackMapTableNumberOfEntries).putByteArray(stackMapTableEntries.data, 0, stackMapTableEntries.length);
		  }
		  if (lineNumberTable != null)
		  {
			output.putShort(symbolTable.addConstantUtf8(Constants.LINE_NUMBER_TABLE)).putInt(2 + lineNumberTable.length).putShort(lineNumberTableLength).putByteArray(lineNumberTable.data, 0, lineNumberTable.length);
		  }
		  if (localVariableTable != null)
		  {
			output.putShort(symbolTable.addConstantUtf8(Constants.LOCAL_VARIABLE_TABLE)).putInt(2 + localVariableTable.length).putShort(localVariableTableLength).putByteArray(localVariableTable.data, 0, localVariableTable.length);
		  }
		  if (localVariableTypeTable != null)
		  {
			output.putShort(symbolTable.addConstantUtf8(Constants.LOCAL_VARIABLE_TYPE_TABLE)).putInt(2 + localVariableTypeTable.length).putShort(localVariableTypeTableLength).putByteArray(localVariableTypeTable.data, 0, localVariableTypeTable.length);
		  }
		  if (lastCodeRuntimeVisibleTypeAnnotation != null)
		  {
			lastCodeRuntimeVisibleTypeAnnotation.putAnnotations(symbolTable.addConstantUtf8(Constants.RUNTIME_VISIBLE_TYPE_ANNOTATIONS), output);
		  }
		  if (lastCodeRuntimeInvisibleTypeAnnotation != null)
		  {
			lastCodeRuntimeInvisibleTypeAnnotation.putAnnotations(symbolTable.addConstantUtf8(Constants.RUNTIME_INVISIBLE_TYPE_ANNOTATIONS), output);
		  }
		  if (firstCodeAttribute != null)
		  {
			firstCodeAttribute.putAttributes(symbolTable, code.data, code.length, maxStack, maxLocals, output);
		  }
		}
		if (numberOfExceptions > 0)
		{
		  output.putShort(symbolTable.addConstantUtf8(Constants.EXCEPTIONS)).putInt(2 + 2 * numberOfExceptions).putShort(numberOfExceptions);
		  foreach (int exceptionIndex in exceptionIndexTable)
		  {
			output.putShort(exceptionIndex);
		  }
		}
		Attribute.putAttributes(symbolTable, accessFlags, signatureIndex, output);
		AnnotationWriter.putAnnotations(symbolTable, lastRuntimeVisibleAnnotation, lastRuntimeInvisibleAnnotation, lastRuntimeVisibleTypeAnnotation, lastRuntimeInvisibleTypeAnnotation, output);
		if (lastRuntimeVisibleParameterAnnotations != null)
		{
		  AnnotationWriter.putParameterAnnotations(symbolTable.addConstantUtf8(Constants.RUNTIME_VISIBLE_PARAMETER_ANNOTATIONS), lastRuntimeVisibleParameterAnnotations, visibleAnnotableParameterCount == 0 ? lastRuntimeVisibleParameterAnnotations.Length : visibleAnnotableParameterCount, output);
		}
		if (lastRuntimeInvisibleParameterAnnotations != null)
		{
		  AnnotationWriter.putParameterAnnotations(symbolTable.addConstantUtf8(Constants.RUNTIME_INVISIBLE_PARAMETER_ANNOTATIONS), lastRuntimeInvisibleParameterAnnotations, invisibleAnnotableParameterCount == 0 ? lastRuntimeInvisibleParameterAnnotations.Length : invisibleAnnotableParameterCount, output);
		}
		if (defaultValue != null)
		{
		  output.putShort(symbolTable.addConstantUtf8(Constants.ANNOTATION_DEFAULT)).putInt(defaultValue.length).putByteArray(defaultValue.data, 0, defaultValue.length);
		}
		if (parameters != null)
		{
		  output.putShort(symbolTable.addConstantUtf8(Constants.METHOD_PARAMETERS)).putInt(1 + parameters.length).putByte(parametersCount).putByteArray(parameters.data, 0, parameters.length);
		}
		if (firstAttribute != null)
		{
		  firstAttribute.putAttributes(symbolTable, output);
		}
	  }

	  /// <summary>
	  /// Collects the attributes of this method into the given set of attribute prototypes.
	  /// </summary>
	  /// <param name="attributePrototypes"> a set of attribute prototypes. </param>
	  public void collectAttributePrototypes(Attribute.Set attributePrototypes)
	  {
		attributePrototypes.addAttributes(firstAttribute);
		attributePrototypes.addAttributes(firstCodeAttribute);
	  }
	}

}