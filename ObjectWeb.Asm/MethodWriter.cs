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
    ///     A <seealso cref="MethodVisitor" /> that generates a corresponding 'method_info' structure, as defined in the
    ///     Java Virtual Machine Specification (JVMS).
    /// </summary>
    /// <seealso cref=
    /// <a href="https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.6">
    ///     JVMS
    ///     4.6
    /// </a>
    /// @author Eric Bruneton
    /// @author Eugene Kuleshov
    /// </seealso>
    internal sealed class MethodWriter : MethodVisitor
    {
        /// <summary>
        ///     Indicates that nothing must be computed.
        /// </summary>
        internal const int Compute_Nothing = 0;

        /// <summary>
        ///     Indicates that the maximum stack size and the maximum number of local variables must be
        ///     computed, from scratch.
        /// </summary>
        internal const int Compute_Max_Stack_And_Local = 1;

        /// <summary>
        ///     Indicates that the maximum stack size and the maximum number of local variables must be
        ///     computed, from the existing stack map frames. This can be done more efficiently than with the
        ///     control flow graph algorithm used for <seealso cref="Compute_Max_Stack_And_Local" />, by using a linear
        ///     scan of the bytecode instructions.
        /// </summary>
        internal const int Compute_Max_Stack_And_Local_From_Frames = 2;

        /// <summary>
        ///     Indicates that the stack map frames of type F_INSERT must be computed. The other frames are not
        ///     computed. They should all be of type F_NEW and should be sufficient to compute the content of
        ///     the F_INSERT frames, together with the bytecode instructions between a F_NEW and a F_INSERT
        ///     frame - and without any knowledge of the type hierarchy (by definition of F_INSERT).
        /// </summary>
        internal const int Compute_Inserted_Frames = 3;

        /// <summary>
        ///     Indicates that all the stack map frames must be computed. In this case the maximum stack size
        ///     and the maximum number of local variables is also computed.
        /// </summary>
        internal const int Compute_All_Frames = 4;

        /// <summary>
        ///     Indicates that <seealso cref="STACK_SIZE_DELTA" /> is not applicable (not constant or never used).
        /// </summary>
        private const int Na = 0;

        /// <summary>
        ///     The stack size variation corresponding to each JVM opcode. The stack size variation for opcode
        ///     'o' is given by the array element at index 'o'.
        /// </summary>
        /// <seealso cref=
        /// <a href="https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-6.html">JVMS 6</a>
        /// </seealso>
        private static readonly int[] STACK_SIZE_DELTA =
        {
            0, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 1, 1, 1, 2, 2, 1, 1, 1, Na, Na, 1, 2, 1, 2, 1, Na, Na, Na, Na, Na, Na, Na,
            Na, Na, Na, Na, Na, Na, Na, Na, Na, Na, Na, Na, Na, -1, 0, -1, 0, -1, -1, -1, -1, -1, -2, -1, -2, -1, Na,
            Na, Na, Na, Na, Na, Na, Na, Na, Na, Na, Na, Na, Na, Na, Na, Na, Na, Na, Na, -3, -4, -3, -4, -3, -3, -3, -3,
            -1, -2, 1, 1, 1, 2, 2, 2, 0, -1, -2, -1, -2, -1, -2, -1, -2, -1, -2, -1, -2, -1, -2, -1, -2, -1, -2, -1, -2,
            0, 0, 0, 0, -1, -1, -1, -1, -1, -1, -1, -2, -1, -2, -1, -2, 0, 1, 0, 1, -1, -1, 0, 0, 1, 1, -1, 0, -1, 0, 0,
            0, -3, -1, -1, -3, -3, -1, -1, -1, -1, -1, -1, -2, -2, -2, -2, -2, -2, -2, -2, 0, 1, 0, -1, -1, -1, -2, -1,
            -2, -1, 0, Na, Na, Na, Na, Na, Na, Na, Na, Na, 1, 0, 0, 0, Na, 0, 0, -1, -1, Na, Na, -1, -1, Na, Na
        };

        // Note: fields are ordered as in the method_info structure, and those related to attributes are
        // ordered as in Section 4.7 of the JVMS.

        /// <summary>
        ///     The access_flags field of the method_info JVMS structure. This field can contain ASM specific
        ///     access flags, such as <seealso cref="IIOpcodes.Acc_Deprecated />, which are removed when generating the
        ///     ClassFile structure.
        /// </summary>
        private readonly int _accessFlags;

        /// <summary>
        ///     The 'code' field of the Code attribute.
        /// </summary>
        private readonly ByteVector _code = new();

        // -----------------------------------------------------------------------------------------------
        // Fields used to compute the maximum stack size and number of locals, and the stack map frames
        // -----------------------------------------------------------------------------------------------

        /// <summary>
        ///     Indicates what must be computed. Must be one of <seealso cref="Compute_All_Frames" />, {@link
        ///     #COMPUTE_INSERTED_FRAMES}, <seealso cref="Compute_Max_Stack_And_Local" /> or <seealso cref="Compute_Nothing" />.
        /// </summary>
        private readonly int _compute;

        /// <summary>
        ///     The descriptor of this method.
        /// </summary>
        private readonly string _descriptor;

        /// <summary>
        ///     The descriptor_index field of the method_info JVMS structure.
        /// </summary>
        private readonly int _descriptorIndex;

        /// <summary>
        ///     The exception_index_table array of the Exceptions attribute, or {@literal null}.
        /// </summary>
        private readonly int[] _exceptionIndexTable;

        /// <summary>
        ///     The name of this method.
        /// </summary>
        private readonly string _name;

        /// <summary>
        ///     The name_index field of the method_info JVMS structure.
        /// </summary>
        private readonly int _nameIndex;

        // Other method_info attributes:

        /// <summary>
        ///     The number_of_exceptions field of the Exceptions attribute.
        /// </summary>
        private readonly int _numberOfExceptions;

        /// <summary>
        ///     The signature_index field of the Signature attribute.
        /// </summary>
        private readonly int _signatureIndex;

        /// <summary>
        ///     Where the constants used in this MethodWriter must be stored.
        /// </summary>
        private readonly SymbolTable _symbolTable;

        /// <summary>
        ///     The current basic block, i.e. the basic block of the last visited instruction. When {@link
        ///     #compute} is equal to <seealso cref="Compute_Max_Stack_And_Local" /> or <seealso cref="Compute_All_Frames" />, this
        ///     field is {@literal null} for unreachable code. When <seealso cref="_compute" /> is equal to {@link
        ///     #COMPUTE_MAX_STACK_AND_LOCAL_FROM_FRAMES} or <seealso cref="Compute_Inserted_Frames" />, this field stays
        ///     unchanged throughout the whole method (i.e. the whole code is seen as a single basic block;
        ///     indeed, the existing frames are sufficient by hypothesis to compute any intermediate frame -
        ///     and the maximum stack size as well - without using any control flow graph).
        /// </summary>
        private Label _currentBasicBlock;

        /// <summary>
        ///     The current stack map frame. The first element contains the bytecode offset of the instruction
        ///     to which the frame corresponds, the second element is the number of locals and the third one is
        ///     the number of stack elements. The local variables start at index 3 and are followed by the
        ///     operand stack elements. In summary frame[0] = offset, frame[1] = numLocal, frame[2] = numStack.
        ///     Local variables and operand stack entries contain abstract types, as defined in <seealso cref="Frame" />,
        ///     but restricted to <seealso cref="Frame.ConstantKind" />, <seealso cref="Frame.ReferenceKind" /> or {@link
        ///     Frame#UNINITIALIZED_KIND} abstract types. Long and double types use only one array entry.
        /// </summary>
        private int[] _currentFrame;

        /// <summary>
        ///     The number of local variables in the last visited stack map frame.
        /// </summary>
        private int _currentLocals;

        /// <summary>
        ///     The default_value field of the AnnotationDefault attribute, or {@literal null}.
        /// </summary>
        private ByteVector _defaultValue;

        /// <summary>
        ///     The first non standard attribute of this method. The next ones can be accessed with the {@link
        ///     Attribute#nextAttribute} field. May be {@literal null}.
        ///     <para>
        ///         <b>WARNING</b>: this list stores the attributes in the <i>reverse</i> order of their visit.
        ///         firstAttribute is actually the last attribute visited in <seealso cref="VisitAttribute" />. The {@link
        ///         #putMethodInfo} method writes the attributes in the order defined by this list, i.e. in the
        ///         reverse order specified by the user.
        ///     </para>
        /// </summary>
        private Attribute _firstAttribute;

        /// <summary>
        ///     The first basic block of the method. The next ones (in bytecode offset order) can be accessed
        ///     with the <seealso cref="Label.nextBasicBlock" /> field.
        /// </summary>
        private readonly Label _firstBasicBlock;

        /// <summary>
        ///     The first non standard attribute of the Code attribute. The next ones can be accessed with the
        ///     <seealso cref="Attribute.nextAttribute" /> field. May be {@literal null}.
        ///     <para>
        ///         <b>WARNING</b>: this list stores the attributes in the <i>reverse</i> order of their visit.
        ///         firstAttribute is actually the last attribute visited in <seealso cref="VisitAttribute" />. The {@link
        ///         #putMethodInfo} method writes the attributes in the order defined by this list, i.e. in the
        ///         reverse order specified by the user.
        ///     </para>
        /// </summary>
        private Attribute _firstCodeAttribute;

        /// <summary>
        ///     The first element in the exception handler list (used to generate the exception_table of the
        ///     Code attribute). The next ones can be accessed with the <seealso cref="Handler.nextHandler" /> field. May
        ///     be {@literal null}.
        /// </summary>
        private Handler _firstHandler;

        // -----------------------------------------------------------------------------------------------
        // Other miscellaneous status fields
        // -----------------------------------------------------------------------------------------------

        /// <summary>
        ///     Whether the bytecode of this method contains ASM specific instructions.
        /// </summary>
        private bool _hasAsmInstructionsConflict;

        /// <summary>
        ///     Whether this method contains subroutines.
        /// </summary>
        private bool _hasSubroutines;

        /// <summary>
        ///     The number of method parameters that can have runtime visible annotations, or 0.
        /// </summary>
        private int _invisibleAnnotableParameterCount;

        /// <summary>
        ///     The last basic block of the method (in bytecode offset order). This field is updated each time
        ///     a basic block is encountered, and is used to append it at the end of the basic block list.
        /// </summary>
        private Label _lastBasicBlock;

        /// <summary>
        ///     The start offset of the last visited instruction. Used to set the offset field of type
        ///     annotations of type 'offset_target' (see
        ///     <a
        ///         href="https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.7.20.1">
        ///         JVMS
        ///         4.7.20.1
        ///     </a>
        ///     ).
        /// </summary>
        private int _lastBytecodeOffset;

        /// <summary>
        ///     The last runtime invisible type annotation of the Code attribute. The previous ones can be
        ///     accessed with the <seealso cref="AnnotationWriter._previousAnnotation" /> field. May be {@literal null}.
        /// </summary>
        private AnnotationWriter _lastCodeRuntimeInvisibleTypeAnnotation;

        /// <summary>
        ///     The last runtime visible type annotation of the Code attribute. The previous ones can be
        ///     accessed with the <seealso cref="AnnotationWriter._previousAnnotation" /> field. May be {@literal null}.
        /// </summary>
        private AnnotationWriter _lastCodeRuntimeVisibleTypeAnnotation;

        /// <summary>
        ///     The last element in the exception handler list (used to generate the exception_table of the
        ///     Code attribute). The next ones can be accessed with the <seealso cref="Handler.nextHandler" /> field. May
        ///     be {@literal null}.
        /// </summary>
        private Handler _lastHandler;

        /// <summary>
        ///     The last runtime invisible annotation of this method. The previous ones can be accessed with
        ///     the <seealso cref="AnnotationWriter._previousAnnotation" /> field. May be {@literal null}.
        /// </summary>
        private AnnotationWriter _lastRuntimeInvisibleAnnotation;

        /// <summary>
        ///     The runtime invisible parameter annotations of this method. Each array element contains the
        ///     last annotation of a parameter (which can be {@literal null} - the previous ones can be
        ///     accessed with the <seealso cref="AnnotationWriter._previousAnnotation" /> field). May be {@literal null}.
        /// </summary>
        private AnnotationWriter[] _lastRuntimeInvisibleParameterAnnotations;

        /// <summary>
        ///     The last runtime invisible type annotation of this method. The previous ones can be accessed
        ///     with the <seealso cref="AnnotationWriter._previousAnnotation" /> field. May be {@literal null}.
        /// </summary>
        private AnnotationWriter _lastRuntimeInvisibleTypeAnnotation;

        /// <summary>
        ///     The last runtime visible annotation of this method. The previous ones can be accessed with the
        ///     <seealso cref="AnnotationWriter._previousAnnotation" /> field. May be {@literal null}.
        /// </summary>
        private AnnotationWriter _lastRuntimeVisibleAnnotation;

        /// <summary>
        ///     The runtime visible parameter annotations of this method. Each array element contains the last
        ///     annotation of a parameter (which can be {@literal null} - the previous ones can be accessed
        ///     with the <seealso cref="AnnotationWriter._previousAnnotation" /> field). May be {@literal null}.
        /// </summary>
        private AnnotationWriter[] _lastRuntimeVisibleParameterAnnotations;

        /// <summary>
        ///     The last runtime visible type annotation of this method. The previous ones can be accessed with
        ///     the <seealso cref="AnnotationWriter._previousAnnotation" /> field. May be {@literal null}.
        /// </summary>
        private AnnotationWriter _lastRuntimeVisibleTypeAnnotation;

        /// <summary>
        ///     The line_number_table array of the LineNumberTable code attribute, or {@literal null}.
        /// </summary>
        private ByteVector _lineNumberTable;

        /// <summary>
        ///     The line_number_table_length field of the LineNumberTable code attribute.
        /// </summary>
        private int _lineNumberTableLength;

        /// <summary>
        ///     The local_variable_table array of the LocalVariableTable code attribute, or {@literal null}.
        /// </summary>
        private ByteVector _localVariableTable;

        /// <summary>
        ///     The local_variable_table_length field of the LocalVariableTable code attribute.
        /// </summary>
        private int _localVariableTableLength;

        /// <summary>
        ///     The local_variable_type_table array of the LocalVariableTypeTable code attribute, or {@literal
        ///     null}.
        /// </summary>
        private ByteVector _localVariableTypeTable;

        /// <summary>
        ///     The local_variable_type_table_length field of the LocalVariableTypeTable code attribute.
        /// </summary>
        private int _localVariableTypeTableLength;

        /// <summary>
        ///     The max_locals field of the Code attribute.
        /// </summary>
        private int _maxLocals;

        /// <summary>
        ///     The maximum relative stack size after the last visited instruction. This size is relative to
        ///     the beginning of <seealso cref="_currentBasicBlock" />, i.e. the true maximum stack size after the last
        ///     visited instruction is equal to the <seealso cref="Label.inputStackSize" /> of the current basic block
        ///     plus <seealso cref="_maxRelativeStackSize" />.When <seealso cref="_compute" /> is equal to {@link
        ///     #COMPUTE_MAX_STACK_AND_LOCAL_FROM_FRAMES}, <seealso cref="_currentBasicBlock" /> is always the start of
        ///     the method, so this relative size is also equal to the absolute maximum stack size after the
        ///     last visited instruction.
        /// </summary>
        private int _maxRelativeStackSize;

        // Code attribute fields and sub attributes:

        /// <summary>
        ///     The max_stack field of the Code attribute.
        /// </summary>
        private int _maxStack;

        /// <summary>
        ///     The 'parameters' array of the MethodParameters attribute, or {@literal null}.
        /// </summary>
        private ByteVector _parameters;

        /// <summary>
        ///     The parameters_count field of the MethodParameters attribute.
        /// </summary>
        private int _parametersCount;

        /// <summary>
        ///     The last frame that was written in <seealso cref="_stackMapTableEntries" />. This field has the same
        ///     format as <seealso cref="_currentFrame" />.
        /// </summary>
        private int[] _previousFrame;

        /// <summary>
        ///     The bytecode offset of the last frame that was written in <seealso cref="_stackMapTableEntries" />.
        /// </summary>
        private int _previousFrameOffset;

        /// <summary>
        ///     The relative stack size after the last visited instruction. This size is relative to the
        ///     beginning of <seealso cref="_currentBasicBlock" />, i.e. the true stack size after the last visited
        ///     instruction is equal to the <seealso cref="Label.inputStackSize" /> of the current basic block plus {@link
        ///     #relativeStackSize}. When <seealso cref="_compute" /> is equal to {@link
        ///     #COMPUTE_MAX_STACK_AND_LOCAL_FROM_FRAMES}, <seealso cref="_currentBasicBlock" /> is always the start of
        ///     the method, so this relative size is also equal to the absolute stack size after the last
        ///     visited instruction.
        /// </summary>
        private int _relativeStackSize;

        /// <summary>
        ///     The length in bytes in <seealso cref="SymbolTable.getSource" /> which must be copied to get the
        ///     method_info for this method (excluding its first 6 bytes for access_flags, name_index and
        ///     descriptor_index).
        /// </summary>
        private int _sourceLength;

        /// <summary>
        ///     The offset in bytes in <seealso cref="SymbolTable.getSource" /> from which the method_info for this method
        ///     (excluding its first 6 bytes) must be copied, or 0.
        /// </summary>
        private int _sourceOffset;

        /// <summary>
        ///     The 'entries' array of the StackMapTable code attribute.
        /// </summary>
        private ByteVector _stackMapTableEntries;

        /// <summary>
        ///     The number_of_entries field of the StackMapTable code attribute.
        /// </summary>
        private int _stackMapTableNumberOfEntries;

        /// <summary>
        ///     The number of method parameters that can have runtime visible annotations, or 0.
        /// </summary>
        private int _visibleAnnotableParameterCount;

        // -----------------------------------------------------------------------------------------------
        // Constructor and accessors
        // -----------------------------------------------------------------------------------------------

        /// <summary>
        ///     Constructs a new <seealso cref="MethodWriter" />.
        /// </summary>
        /// <param name="symbolTable"> where the constants used in this AnnotationWriter must be stored. </param>
        /// <param name="access"> the method's access flags (see <seealso cref="IOpcodes" />). </param>
        /// <param name="name"> the method's name. </param>
        /// <param name="descriptor"> the method's descriptor (see <seealso cref="Type" />). </param>
        /// <param name="signature"> the method's signature. May be {@literal null}. </param>
        /// <param name="exceptions"> the internal names of the method's exceptions. May be {@literal null}. </param>
        /// <param name="compute"> indicates what must be computed (see #compute). </param>
        public MethodWriter(SymbolTable symbolTable, int access, string name, string descriptor, string signature,
            string[] exceptions, int compute) : base(IOpcodes.Asm9)
        {
            this._symbolTable = symbolTable;
            _accessFlags = "<init>".Equals(name) ? access | Constants.Acc_Constructor : access;
            _nameIndex = symbolTable.AddConstantUtf8(name);
            this._name = name;
            _descriptorIndex = symbolTable.AddConstantUtf8(descriptor);
            this._descriptor = descriptor;
            _signatureIndex = ReferenceEquals(signature, null) ? 0 : symbolTable.AddConstantUtf8(signature);
            if (exceptions != null && exceptions.Length > 0)
            {
                _numberOfExceptions = exceptions.Length;
                _exceptionIndexTable = new int[_numberOfExceptions];
                for (var i = 0; i < _numberOfExceptions; ++i)
                    _exceptionIndexTable[i] = symbolTable.AddConstantClass(exceptions[i]).index;
            }
            else
            {
                _numberOfExceptions = 0;
                _exceptionIndexTable = null;
            }

            this._compute = compute;
            if (compute != Compute_Nothing)
            {
                // Update maxLocals and currentLocals.
                var argumentsSize = JType.GetArgumentsAndReturnSizes(descriptor) >> 2;
                if ((access & IOpcodes.Acc_Static) != 0) --argumentsSize;
                _maxLocals = argumentsSize;
                _currentLocals = argumentsSize;
                // Create and visit the label for the first basic block.
                _firstBasicBlock = new Label();
                VisitLabel(_firstBasicBlock);
            }
        }

        public bool HasFrames()
        {
            return _stackMapTableNumberOfEntries > 0;
        }

        public bool HasAsmInstructions()
        {
            return _hasAsmInstructionsConflict;
        }

        // -----------------------------------------------------------------------------------------------
        // Implementation of the MethodVisitor abstract class
        // -----------------------------------------------------------------------------------------------

        public override void VisitParameter(string name, int access)
        {
            if (_parameters == null) _parameters = new ByteVector();
            ++_parametersCount;
            _parameters.PutShort(ReferenceEquals(name, null) ? 0 : _symbolTable.AddConstantUtf8(name)).PutShort(access);
        }

        public override AnnotationVisitor VisitAnnotationDefault()
        {
            _defaultValue = new ByteVector();
            return new AnnotationWriter(_symbolTable, false, _defaultValue, null);
        }

        public override AnnotationVisitor VisitAnnotation(string descriptor, bool visible)
        {
            if (visible)
                return _lastRuntimeVisibleAnnotation =
                    AnnotationWriter.Create(_symbolTable, descriptor, _lastRuntimeVisibleAnnotation);
            return _lastRuntimeInvisibleAnnotation =
                AnnotationWriter.Create(_symbolTable, descriptor, _lastRuntimeInvisibleAnnotation);
        }

        public override AnnotationVisitor VisitTypeAnnotation(int typeRef, TypePath typePath, string descriptor,
            bool visible)
        {
            if (visible)
                return _lastRuntimeVisibleTypeAnnotation = AnnotationWriter.Create(_symbolTable, typeRef, typePath,
                    descriptor, _lastRuntimeVisibleTypeAnnotation);
            return _lastRuntimeInvisibleTypeAnnotation = AnnotationWriter.Create(_symbolTable, typeRef, typePath,
                descriptor, _lastRuntimeInvisibleTypeAnnotation);
        }

        public override void VisitAnnotableParameterCount(int parameterCount, bool visible)
        {
            if (visible)
                _visibleAnnotableParameterCount = parameterCount;
            else
                _invisibleAnnotableParameterCount = parameterCount;
        }

        public override AnnotationVisitor VisitParameterAnnotation(int parameter, string annotationDescriptor,
            bool visible)
        {
            if (visible)
            {
                if (_lastRuntimeVisibleParameterAnnotations == null)
                    _lastRuntimeVisibleParameterAnnotations =
                        new AnnotationWriter[JType.GetArgumentTypes(_descriptor).Length];
                return _lastRuntimeVisibleParameterAnnotations[parameter] = AnnotationWriter.Create(_symbolTable,
                    annotationDescriptor, _lastRuntimeVisibleParameterAnnotations[parameter]);
            }

            if (_lastRuntimeInvisibleParameterAnnotations == null)
                _lastRuntimeInvisibleParameterAnnotations =
                    new AnnotationWriter[JType.GetArgumentTypes(_descriptor).Length];
            return _lastRuntimeInvisibleParameterAnnotations[parameter] = AnnotationWriter.Create(_symbolTable,
                annotationDescriptor, _lastRuntimeInvisibleParameterAnnotations[parameter]);
        }

        public override void VisitAttribute(Attribute attribute)
        {
            // Store the attributes in the <i>reverse</i> order of their visit by this method.
            if (attribute.CodeAttribute)
            {
                attribute.nextAttribute = _firstCodeAttribute;
                _firstCodeAttribute = attribute;
            }
            else
            {
                attribute.nextAttribute = _firstAttribute;
                _firstAttribute = attribute;
            }
        }

        public override void VisitCode()
        {
            // Nothing to do.
        }

        public override void VisitFrame(int type, int numLocal, object[] local, int numStack, object[] stack)
        {
            if (_compute == Compute_All_Frames) return;

            if (_compute == Compute_Inserted_Frames)
            {
                if (_currentBasicBlock.frame == null)
                {
                    // This should happen only once, for the implicit first frame (which is explicitly visited
                    // in ClassReader if the EXPAND_ASM_INSNS option is used - and COMPUTE_INSERTED_FRAMES
                    // can't be set if EXPAND_ASM_INSNS is not used).
                    _currentBasicBlock.frame = new CurrentFrame(_currentBasicBlock);
                    _currentBasicBlock.frame.SetInputFrameFromDescriptor(_symbolTable, _accessFlags, _descriptor,
                        numLocal);
                    _currentBasicBlock.frame.Accept(this);
                }
                else
                {
                    if (type == IOpcodes.F_New)
                        _currentBasicBlock.frame.SetInputFrameFromApiFormat(_symbolTable, numLocal, local, numStack,
                            stack);
                    // If type is not F_NEW then it is F_INSERT by hypothesis, and currentBlock.frame contains
                    // the stack map frame at the current instruction, computed from the last F_NEW frame and
                    // the bytecode instructions in between (via calls to CurrentFrame#execute).
                    _currentBasicBlock.frame.Accept(this);
                }
            }
            else if (type == IOpcodes.F_New)
            {
                if (_previousFrame == null)
                {
                    var argumentsSize = JType.GetArgumentsAndReturnSizes(_descriptor) >> 2;
                    var implicitFirstFrame = new Frame(new Label());
                    implicitFirstFrame.SetInputFrameFromDescriptor(_symbolTable, _accessFlags, _descriptor,
                        argumentsSize);
                    implicitFirstFrame.Accept(this);
                }

                _currentLocals = numLocal;
                var frameIndex = VisitFrameStart(_code.length, numLocal, numStack);
                for (var i = 0; i < numLocal; ++i)
                    _currentFrame[frameIndex++] = Frame.GetAbstractTypeFromApiFormat(_symbolTable, local[i]);
                for (var i = 0; i < numStack; ++i)
                    _currentFrame[frameIndex++] = Frame.GetAbstractTypeFromApiFormat(_symbolTable, stack[i]);
                VisitFrameEnd();
            }
            else
            {
                if (_symbolTable.MajorVersion < IOpcodes.V1_6)
                    throw new ArgumentException("Class versions V1_5 or less must use F_NEW frames.");
                int offsetDelta;
                if (_stackMapTableEntries == null)
                {
                    _stackMapTableEntries = new ByteVector();
                    offsetDelta = _code.length;
                }
                else
                {
                    offsetDelta = _code.length - _previousFrameOffset - 1;
                    if (offsetDelta < 0)
                    {
                        if (type == IOpcodes.F_Same)
                            return;
                        throw new InvalidOperationException();
                    }
                }

                switch (type)
                {
                    case IOpcodes.F_Full:
                        _currentLocals = numLocal;
                        _stackMapTableEntries.PutByte(Frame.Full_Frame).PutShort(offsetDelta).PutShort(numLocal);
                        for (var i = 0; i < numLocal; ++i) PutFrameType(local[i]);
                        _stackMapTableEntries.PutShort(numStack);
                        for (var i = 0; i < numStack; ++i) PutFrameType(stack[i]);
                        break;
                    case IOpcodes.F_Append:
                        _currentLocals += numLocal;
                        _stackMapTableEntries.PutByte(Frame.Same_Frame_Extended + numLocal).PutShort(offsetDelta);
                        for (var i = 0; i < numLocal; ++i) PutFrameType(local[i]);
                        break;
                    case IOpcodes.F_Chop:
                        _currentLocals -= numLocal;
                        _stackMapTableEntries.PutByte(Frame.Same_Frame_Extended - numLocal).PutShort(offsetDelta);
                        break;
                    case IOpcodes.F_Same:
                        if (offsetDelta < 64)
                            _stackMapTableEntries.PutByte(offsetDelta);
                        else
                            _stackMapTableEntries.PutByte(Frame.Same_Frame_Extended).PutShort(offsetDelta);
                        break;
                    case IOpcodes.F_Same1:
                        if (offsetDelta < 64)
                            _stackMapTableEntries.PutByte(Frame.Same_Locals_1_Stack_Item_Frame + offsetDelta);
                        else
                            _stackMapTableEntries.PutByte(Frame.Same_Locals_1_Stack_Item_Frame_Extended)
                                .PutShort(offsetDelta);
                        PutFrameType(stack[0]);
                        break;
                    default:
                        throw new ArgumentException();
                }

                _previousFrameOffset = _code.length;
                ++_stackMapTableNumberOfEntries;
            }

            if (_compute == Compute_Max_Stack_And_Local_From_Frames)
            {
                _relativeStackSize = numStack;
                for (var i = 0; i < numStack; ++i)
                    if (Equals(stack[i], IOpcodes.@long) || Equals(stack[i], IOpcodes.@double))
                        _relativeStackSize++;
                if (_relativeStackSize > _maxRelativeStackSize) _maxRelativeStackSize = _relativeStackSize;
            }

            _maxStack = Math.Max(_maxStack, numStack);
            _maxLocals = Math.Max(_maxLocals, _currentLocals);
        }

        public override void VisitInsn(int opcode)
        {
            _lastBytecodeOffset = _code.length;
            // Add the instruction to the bytecode of the method.
            _code.PutByte(opcode);
            // If needed, update the maximum stack size and number of locals, and stack map frames.
            if (_currentBasicBlock != null)
            {
                if (_compute == Compute_All_Frames || _compute == Compute_Inserted_Frames)
                {
                    _currentBasicBlock.frame.Execute(opcode, 0, null, null);
                }
                else
                {
                    var size = _relativeStackSize + STACK_SIZE_DELTA[opcode];
                    if (size > _maxRelativeStackSize) _maxRelativeStackSize = size;
                    _relativeStackSize = size;
                }

                if (opcode >= IOpcodes.Ireturn && opcode <= IOpcodes.Return || opcode == IOpcodes.Athrow)
                    EndCurrentBasicBlockWithNoSuccessor();
            }
        }

        public override void VisitIntInsn(int opcode, int operand)
        {
            _lastBytecodeOffset = _code.length;
            // Add the instruction to the bytecode of the method.
            if (opcode == IOpcodes.Sipush)
                _code.Put12(opcode, operand);
            else
                // BIPUSH or NEWARRAY
                _code.Put11(opcode, operand);
            // If needed, update the maximum stack size and number of locals, and stack map frames.
            if (_currentBasicBlock != null)
            {
                if (_compute == Compute_All_Frames || _compute == Compute_Inserted_Frames)
                {
                    _currentBasicBlock.frame.Execute(opcode, operand, null, null);
                }
                else if (opcode != IOpcodes.Newarray)
                {
                    // The stack size delta is 1 for BIPUSH or SIPUSH, and 0 for NEWARRAY.
                    var size = _relativeStackSize + 1;
                    if (size > _maxRelativeStackSize) _maxRelativeStackSize = size;
                    _relativeStackSize = size;
                }
            }
        }

        public override void VisitVarInsn(int opcode, int varIndex)
        {
            _lastBytecodeOffset = _code.length;
            // Add the instruction to the bytecode of the method.
            if (varIndex < 4 && opcode != IOpcodes.Ret)
            {
                int optimizedOpcode;
                if (opcode < IOpcodes.Istore)
                    optimizedOpcode = Constants.Iload_0 + ((opcode - IOpcodes.Iload) << 2) + varIndex;
                else
                    optimizedOpcode = Constants.Istore_0 + ((opcode - IOpcodes.Istore) << 2) + varIndex;
                _code.PutByte(optimizedOpcode);
            }
            else if (varIndex >= 256)
            {
                _code.PutByte(Constants.Wide).Put12(opcode, varIndex);
            }
            else
            {
                _code.Put11(opcode, varIndex);
            }

            // If needed, update the maximum stack size and number of locals, and stack map frames.
            if (_currentBasicBlock != null)
            {
                if (_compute == Compute_All_Frames || _compute == Compute_Inserted_Frames)
                {
                    _currentBasicBlock.frame.Execute(opcode, varIndex, null, null);
                }
                else
                {
                    if (opcode == IOpcodes.Ret)
                    {
                        // No stack size delta.
                        _currentBasicBlock.flags |= Label.Flag_Subroutine_End;
                        _currentBasicBlock.outputStackSize = (short)_relativeStackSize;
                        EndCurrentBasicBlockWithNoSuccessor();
                    }
                    else
                    {
                        // xLOAD or xSTORE
                        var size = _relativeStackSize + STACK_SIZE_DELTA[opcode];
                        if (size > _maxRelativeStackSize) _maxRelativeStackSize = size;
                        _relativeStackSize = size;
                    }
                }
            }

            if (_compute != Compute_Nothing)
            {
                int currentMaxLocals;
                if (opcode == IOpcodes.Lload || opcode == IOpcodes.Dload || opcode == IOpcodes.Lstore ||
                    opcode == IOpcodes.Dstore)
                    currentMaxLocals = varIndex + 2;
                else
                    currentMaxLocals = varIndex + 1;
                if (currentMaxLocals > _maxLocals) _maxLocals = currentMaxLocals;
            }

            if (opcode >= IOpcodes.Istore && _compute == Compute_All_Frames && _firstHandler != null)
                // If there are exception handler blocks, each instruction within a handler range is, in
                // theory, a basic block (since execution can jump from this instruction to the exception
                // handler). As a consequence, the local variable types at the beginning of the handler
                // block should be the merge of the local variable types at all the instructions within the
                // handler range. However, instead of creating a basic block for each instruction, we can
                // get the same result in a more efficient way. Namely, by starting a new basic block after
                // each xSTORE instruction, which is what we do here.
                VisitLabel(new Label());
        }

        public override void VisitTypeInsn(int opcode, string type)
        {
            _lastBytecodeOffset = _code.length;
            // Add the instruction to the bytecode of the method.
            var typeSymbol = _symbolTable.AddConstantClass(type);
            _code.Put12(opcode, typeSymbol.index);
            // If needed, update the maximum stack size and number of locals, and stack map frames.
            if (_currentBasicBlock != null)
            {
                if (_compute == Compute_All_Frames || _compute == Compute_Inserted_Frames)
                {
                    _currentBasicBlock.frame.Execute(opcode, _lastBytecodeOffset, typeSymbol, _symbolTable);
                }
                else if (opcode == IOpcodes.New)
                {
                    // The stack size delta is 1 for NEW, and 0 for ANEWARRAY, CHECKCAST, or INSTANCEOF.
                    var size = _relativeStackSize + 1;
                    if (size > _maxRelativeStackSize) _maxRelativeStackSize = size;
                    _relativeStackSize = size;
                }
            }
        }

        public override void VisitFieldInsn(int opcode, string owner, string name, string descriptor)
        {
            _lastBytecodeOffset = _code.length;
            // Add the instruction to the bytecode of the method.
            var fieldrefSymbol = _symbolTable.AddConstantFieldref(owner, name, descriptor);
            _code.Put12(opcode, fieldrefSymbol.index);
            // If needed, update the maximum stack size and number of locals, and stack map frames.
            if (_currentBasicBlock != null)
            {
                if (_compute == Compute_All_Frames || _compute == Compute_Inserted_Frames)
                {
                    _currentBasicBlock.frame.Execute(opcode, 0, fieldrefSymbol, _symbolTable);
                }
                else
                {
                    int size;
                    var firstDescChar = descriptor[0];
                    switch (opcode)
                    {
                        case IOpcodes.Getstatic:
                            size = _relativeStackSize + (firstDescChar == 'D' || firstDescChar == 'J' ? 2 : 1);
                            break;
                        case IOpcodes.Putstatic:
                            size = _relativeStackSize + (firstDescChar == 'D' || firstDescChar == 'J' ? -2 : -1);
                            break;
                        case IOpcodes.Getfield:
                            size = _relativeStackSize + (firstDescChar == 'D' || firstDescChar == 'J' ? 1 : 0);
                            break;
                        case IOpcodes.Putfield:
                        default:
                            size = _relativeStackSize + (firstDescChar == 'D' || firstDescChar == 'J' ? -3 : -2);
                            break;
                    }

                    if (size > _maxRelativeStackSize) _maxRelativeStackSize = size;
                    _relativeStackSize = size;
                }
            }
        }

        public override void VisitMethodInsn(int opcode, string owner, string name, string descriptor, bool isInterface)
        {
            _lastBytecodeOffset = _code.length;
            // Add the instruction to the bytecode of the method.
            var methodrefSymbol = _symbolTable.AddConstantMethodref(owner, name, descriptor, isInterface);
            if (opcode == IOpcodes.Invokeinterface)
                _code.Put12(IOpcodes.Invokeinterface, methodrefSymbol.index)
                    .Put11(methodrefSymbol.ArgumentsAndReturnSizes >> 2, 0);
            else
                _code.Put12(opcode, methodrefSymbol.index);
            // If needed, update the maximum stack size and number of locals, and stack map frames.
            if (_currentBasicBlock != null)
            {
                if (_compute == Compute_All_Frames || _compute == Compute_Inserted_Frames)
                {
                    _currentBasicBlock.frame.Execute(opcode, 0, methodrefSymbol, _symbolTable);
                }
                else
                {
                    var argumentsAndReturnSize = methodrefSymbol.ArgumentsAndReturnSizes;
                    var stackSizeDelta = (argumentsAndReturnSize & 3) - (argumentsAndReturnSize >> 2);
                    int size;
                    if (opcode == IOpcodes.Invokestatic)
                        size = _relativeStackSize + stackSizeDelta + 1;
                    else
                        size = _relativeStackSize + stackSizeDelta;
                    if (size > _maxRelativeStackSize) _maxRelativeStackSize = size;
                    _relativeStackSize = size;
                }
            }
        }

        public override void VisitInvokeDynamicInsn(string name, string descriptor, Handle bootstrapMethodHandle,
            params object[] bootstrapMethodArguments)
        {
            _lastBytecodeOffset = _code.length;
            // Add the instruction to the bytecode of the method.
            var invokeDynamicSymbol =
                _symbolTable.AddConstantInvokeDynamic(name, descriptor, bootstrapMethodHandle,
                    bootstrapMethodArguments);
            _code.Put12(IOpcodes.Invokedynamic, invokeDynamicSymbol.index);
            _code.PutShort(0);
            // If needed, update the maximum stack size and number of locals, and stack map frames.
            if (_currentBasicBlock != null)
            {
                if (_compute == Compute_All_Frames || _compute == Compute_Inserted_Frames)
                {
                    _currentBasicBlock.frame.Execute(IOpcodes.Invokedynamic, 0, invokeDynamicSymbol, _symbolTable);
                }
                else
                {
                    var argumentsAndReturnSize = invokeDynamicSymbol.ArgumentsAndReturnSizes;
                    var stackSizeDelta = (argumentsAndReturnSize & 3) - (argumentsAndReturnSize >> 2) + 1;
                    var size = _relativeStackSize + stackSizeDelta;
                    if (size > _maxRelativeStackSize) _maxRelativeStackSize = size;
                    _relativeStackSize = size;
                }
            }
        }

        public override void VisitJumpInsn(int opcode, Label label)
        {
            _lastBytecodeOffset = _code.length;
            // Add the instruction to the bytecode of the method.
            // Compute the 'base' opcode, i.e. GOTO or JSR if opcode is GOTO_W or JSR_W, otherwise opcode.
            var baseOpcode = opcode >= Constants.Goto_W ? opcode - Constants.WideJumpOpcodeDelta : opcode;
            var nextInsnIsJumpTarget = false;
            if ((label.flags & Label.Flag_Resolved) != 0 && label.bytecodeOffset - _code.length < short.MinValue)
            {
                // Case of a backward jump with an offset < -32768. In this case we automatically replace GOTO
                // with GOTO_W, JSR with JSR_W and IFxxx <l> with IFNOTxxx <L> GOTO_W <l> L:..., where
                // IFNOTxxx is the "opposite" opcode of IFxxx (e.g. IFNE for IFEQ) and where <L> designates
                // the instruction just after the GOTO_W.
                if (baseOpcode == IOpcodes.Goto)
                {
                    _code.PutByte(Constants.Goto_W);
                }
                else if (baseOpcode == IOpcodes.Jsr)
                {
                    _code.PutByte(Constants.Jsr_W);
                }
                else
                {
                    // Put the "opposite" opcode of baseOpcode. This can be done by flipping the least
                    // significant bit for IFNULL and IFNONNULL, and similarly for IFEQ ... IF_ACMPEQ (with a
                    // pre and post offset by 1). The jump offset is 8 bytes (3 for IFNOTxxx, 5 for GOTO_W).
                    _code.PutByte(baseOpcode >= IOpcodes.Ifnull ? baseOpcode ^ 1 : ((baseOpcode + 1) ^ 1) - 1);
                    _code.PutShort(8);
                    // Here we could put a GOTO_W in theory, but if ASM specific instructions are used in this
                    // method or another one, and if the class has frames, we will need to insert a frame after
                    // this GOTO_W during the additional ClassReader -> ClassWriter round trip to remove the ASM
                    // specific instructions. To not miss this additional frame, we need to use an ASM_GOTO_W
                    // here, which has the unfortunate effect of forcing this additional round trip (which in
                    // some case would not have been really necessary, but we can't know this at this point).
                    _code.PutByte(Constants.Asm_Goto_W);
                    _hasAsmInstructionsConflict = true;
                    // The instruction after the GOTO_W becomes the target of the IFNOT instruction.
                    nextInsnIsJumpTarget = true;
                }

                label.Put(_code, _code.length - 1, true);
            }
            else if (baseOpcode != opcode)
            {
                // Case of a GOTO_W or JSR_W specified by the user (normally ClassReader when used to remove
                // ASM specific instructions). In this case we keep the original instruction.
                _code.PutByte(opcode);
                label.Put(_code, _code.length - 1, true);
            }
            else
            {
                // Case of a jump with an offset >= -32768, or of a jump with an unknown offset. In these
                // cases we store the offset in 2 bytes (which will be increased via a ClassReader ->
                // ClassWriter round trip if it turns out that 2 bytes are not sufficient).
                _code.PutByte(baseOpcode);
                label.Put(_code, _code.length - 1, false);
            }

            // If needed, update the maximum stack size and number of locals, and stack map frames.
            if (_currentBasicBlock != null)
            {
                Label nextBasicBlock = null;
                if (_compute == Compute_All_Frames)
                {
                    _currentBasicBlock.frame.Execute(baseOpcode, 0, null, null);
                    // Record the fact that 'label' is the target of a jump instruction.
                    label.CanonicalInstance.flags |= Label.Flag_Jump_Target;
                    // Add 'label' as a successor of the current basic block.
                    AddSuccessorToCurrentBasicBlock(Edge.Jump, label);
                    if (baseOpcode != IOpcodes.Goto)
                        // The next instruction starts a new basic block (except for GOTO: by default the code
                        // following a goto is unreachable - unless there is an explicit label for it - and we
                        // should not compute stack frame types for its instructions).
                        nextBasicBlock = new Label();
                }
                else if (_compute == Compute_Inserted_Frames)
                {
                    _currentBasicBlock.frame.Execute(baseOpcode, 0, null, null);
                }
                else if (_compute == Compute_Max_Stack_And_Local_From_Frames)
                {
                    // No need to update maxRelativeStackSize (the stack size delta is always negative).
                    _relativeStackSize += STACK_SIZE_DELTA[baseOpcode];
                }
                else
                {
                    if (baseOpcode == IOpcodes.Jsr)
                    {
                        // Record the fact that 'label' designates a subroutine, if not already done.
                        if ((label.flags & Label.Flag_Subroutine_Start) == 0)
                        {
                            label.flags |= Label.Flag_Subroutine_Start;
                            _hasSubroutines = true;
                        }

                        _currentBasicBlock.flags |= Label.Flag_Subroutine_Caller;
                        // Note that, by construction in this method, a block which calls a subroutine has at
                        // least two successors in the control flow graph: the first one (added below) leads to
                        // the instruction after the JSR, while the second one (added here) leads to the JSR
                        // target. Note that the first successor is virtual (it does not correspond to a possible
                        // execution path): it is only used to compute the successors of the basic blocks ending
                        // with a ret, in {@link Label#addSubroutineRetSuccessors}.
                        AddSuccessorToCurrentBasicBlock(_relativeStackSize + 1, label);
                        // The instruction after the JSR starts a new basic block.
                        nextBasicBlock = new Label();
                    }
                    else
                    {
                        // No need to update maxRelativeStackSize (the stack size delta is always negative).
                        _relativeStackSize += STACK_SIZE_DELTA[baseOpcode];
                        AddSuccessorToCurrentBasicBlock(_relativeStackSize, label);
                    }
                }

                // If the next instruction starts a new basic block, call visitLabel to add the label of this
                // instruction as a successor of the current block, and to start a new basic block.
                if (nextBasicBlock != null)
                {
                    if (nextInsnIsJumpTarget) nextBasicBlock.flags |= Label.Flag_Jump_Target;
                    VisitLabel(nextBasicBlock);
                }

                if (baseOpcode == IOpcodes.Goto) EndCurrentBasicBlockWithNoSuccessor();
            }
        }

        public override void VisitLabel(Label label)
        {
            // Resolve the forward references to this label, if any.
            _hasAsmInstructionsConflict |= label.Resolve(_code.data, _code.length);
            // visitLabel starts a new basic block (except for debug only labels), so we need to update the
            // previous and current block references and list of successors.
            if ((label.flags & Label.Flag_Debug_Only) != 0) return;
            if (_compute == Compute_All_Frames)
            {
                if (_currentBasicBlock != null)
                {
                    if (label.bytecodeOffset == _currentBasicBlock.bytecodeOffset)
                    {
                        // We use {@link Label#getCanonicalInstance} to store the state of a basic block in only
                        // one place, but this does not work for labels which have not been visited yet.
                        // Therefore, when we detect here two labels having the same bytecode offset, we need to
                        // - consolidate the state scattered in these two instances into the canonical instance:
                        _currentBasicBlock.flags |= (short)(label.flags & Label.Flag_Jump_Target);
                        // - make sure the two instances share the same Frame instance (the implementation of
                        // {@link Label#getCanonicalInstance} relies on this property; here label.frame should be
                        // null):
                        label.frame = _currentBasicBlock.frame;
                        // - and make sure to NOT assign 'label' into 'currentBasicBlock' or 'lastBasicBlock', so
                        // that they still refer to the canonical instance for this bytecode offset.
                        return;
                    }

                    // End the current basic block (with one new successor).
                    AddSuccessorToCurrentBasicBlock(Edge.Jump, label);
                }

                // Append 'label' at the end of the basic block list.
                if (_lastBasicBlock != null)
                {
                    if (label.bytecodeOffset == _lastBasicBlock.bytecodeOffset)
                    {
                        // Same comment as above.
                        _lastBasicBlock.flags |= (short)(label.flags & Label.Flag_Jump_Target);
                        // Here label.frame should be null.
                        label.frame = _lastBasicBlock.frame;
                        _currentBasicBlock = _lastBasicBlock;
                        return;
                    }

                    _lastBasicBlock.nextBasicBlock = label;
                }

                _lastBasicBlock = label;
                // Make it the new current basic block.
                _currentBasicBlock = label;
                // Here label.frame should be null.
                label.frame = new Frame(label);
            }
            else if (_compute == Compute_Inserted_Frames)
            {
                if (_currentBasicBlock == null)
                    // This case should happen only once, for the visitLabel call in the constructor. Indeed, if
                    // compute is equal to COMPUTE_INSERTED_FRAMES, currentBasicBlock stays unchanged.
                    _currentBasicBlock = label;
                else
                    // Update the frame owner so that a correct frame offset is computed in Frame.accept().
                    _currentBasicBlock.frame.owner = label;
            }
            else if (_compute == Compute_Max_Stack_And_Local)
            {
                if (_currentBasicBlock != null)
                {
                    // End the current basic block (with one new successor).
                    _currentBasicBlock.outputStackMax = (short)_maxRelativeStackSize;
                    AddSuccessorToCurrentBasicBlock(_relativeStackSize, label);
                }

                // Start a new current basic block, and reset the current and maximum relative stack sizes.
                _currentBasicBlock = label;
                _relativeStackSize = 0;
                _maxRelativeStackSize = 0;
                // Append the new basic block at the end of the basic block list.
                if (_lastBasicBlock != null) _lastBasicBlock.nextBasicBlock = label;
                _lastBasicBlock = label;
            }
            else if (_compute == Compute_Max_Stack_And_Local_From_Frames && _currentBasicBlock == null)
            {
                // This case should happen only once, for the visitLabel call in the constructor. Indeed, if
                // compute is equal to COMPUTE_MAX_STACK_AND_LOCAL_FROM_FRAMES, currentBasicBlock stays
                // unchanged.
                _currentBasicBlock = label;
            }
        }

        public override void VisitLdcInsn(object value)
        {
            _lastBytecodeOffset = _code.length;
            // Add the instruction to the bytecode of the method.
            var constantSymbol = _symbolTable.AddConstant(value);
            var constantIndex = constantSymbol.index;
            char firstDescriptorChar;
            var isLongOrDouble = constantSymbol.tag == Symbol.Constant_Long_Tag ||
                                 constantSymbol.tag == Symbol.Constant_Double_Tag ||
                                 constantSymbol.tag == Symbol.Constant_Dynamic_Tag &&
                                 ((firstDescriptorChar = constantSymbol.value[0]) == 'J' || firstDescriptorChar == 'D');
            if (isLongOrDouble)
                _code.Put12(Constants.Ldc2_W, constantIndex);
            else if (constantIndex >= 256)
                _code.Put12(Constants.Ldc_W, constantIndex);
            else
                _code.Put11(IOpcodes.Ldc, constantIndex);
            // If needed, update the maximum stack size and number of locals, and stack map frames.
            if (_currentBasicBlock != null)
            {
                if (_compute == Compute_All_Frames || _compute == Compute_Inserted_Frames)
                {
                    _currentBasicBlock.frame.Execute(IOpcodes.Ldc, 0, constantSymbol, _symbolTable);
                }
                else
                {
                    var size = _relativeStackSize + (isLongOrDouble ? 2 : 1);
                    if (size > _maxRelativeStackSize) _maxRelativeStackSize = size;
                    _relativeStackSize = size;
                }
            }
        }

        public override void VisitIincInsn(int varIndex, int increment)
        {
            _lastBytecodeOffset = _code.length;
            // Add the instruction to the bytecode of the method.
            if (varIndex > 255 || increment > 127 || increment < -128)
                _code.PutByte(Constants.Wide).Put12(IOpcodes.Iinc, varIndex).PutShort(increment);
            else
                _code.PutByte(IOpcodes.Iinc).Put11(varIndex, increment);
            // If needed, update the maximum stack size and number of locals, and stack map frames.
            if (_currentBasicBlock != null && (_compute == Compute_All_Frames || _compute == Compute_Inserted_Frames))
                _currentBasicBlock.frame.Execute(IOpcodes.Iinc, varIndex, null, null);
            if (_compute != Compute_Nothing)
            {
                var currentMaxLocals = varIndex + 1;
                if (currentMaxLocals > _maxLocals) _maxLocals = currentMaxLocals;
            }
        }

        public override void VisitTableSwitchInsn(int min, int max, Label dflt, params Label[] labels)
        {
            _lastBytecodeOffset = _code.length;
            // Add the instruction to the bytecode of the method.
            _code.PutByte(IOpcodes.Tableswitch).PutByteArray(null, 0, (4 - _code.length % 4) % 4);
            dflt.Put(_code, _lastBytecodeOffset, true);
            _code.PutInt(min).PutInt(max);
            foreach (var label in labels) label.Put(_code, _lastBytecodeOffset, true);
            // If needed, update the maximum stack size and number of locals, and stack map frames.
            VisitSwitchInsn(dflt, labels);
        }

        public override void VisitLookupSwitchInsn(Label dflt, int[] keys, Label[] labels)
        {
            _lastBytecodeOffset = _code.length;
            // Add the instruction to the bytecode of the method.
            _code.PutByte(IOpcodes.Lookupswitch).PutByteArray(null, 0, (4 - _code.length % 4) % 4);
            dflt.Put(_code, _lastBytecodeOffset, true);
            _code.PutInt(labels.Length);
            for (var i = 0; i < labels.Length; ++i)
            {
                _code.PutInt(keys[i]);
                labels[i].Put(_code, _lastBytecodeOffset, true);
            }

            // If needed, update the maximum stack size and number of locals, and stack map frames.
            VisitSwitchInsn(dflt, labels);
        }

        private void VisitSwitchInsn(Label dflt, Label[] labels)
        {
            if (_currentBasicBlock != null)
            {
                if (_compute == Compute_All_Frames)
                {
                    _currentBasicBlock.frame.Execute(IOpcodes.Lookupswitch, 0, null, null);
                    // Add all the labels as successors of the current basic block.
                    AddSuccessorToCurrentBasicBlock(Edge.Jump, dflt);
                    dflt.CanonicalInstance.flags |= Label.Flag_Jump_Target;
                    foreach (var label in labels)
                    {
                        AddSuccessorToCurrentBasicBlock(Edge.Jump, label);
                        label.CanonicalInstance.flags |= Label.Flag_Jump_Target;
                    }
                }
                else if (_compute == Compute_Max_Stack_And_Local)
                {
                    // No need to update maxRelativeStackSize (the stack size delta is always negative).
                    --_relativeStackSize;
                    // Add all the labels as successors of the current basic block.
                    AddSuccessorToCurrentBasicBlock(_relativeStackSize, dflt);
                    foreach (var label in labels) AddSuccessorToCurrentBasicBlock(_relativeStackSize, label);
                }

                // End the current basic block.
                EndCurrentBasicBlockWithNoSuccessor();
            }
        }

        public override void VisitMultiANewArrayInsn(string descriptor, int numDimensions)
        {
            _lastBytecodeOffset = _code.length;
            // Add the instruction to the bytecode of the method.
            var descSymbol = _symbolTable.AddConstantClass(descriptor);
            _code.Put12(IOpcodes.Multianewarray, descSymbol.index).PutByte(numDimensions);
            // If needed, update the maximum stack size and number of locals, and stack map frames.
            if (_currentBasicBlock != null)
            {
                if (_compute == Compute_All_Frames || _compute == Compute_Inserted_Frames)
                    _currentBasicBlock.frame.Execute(IOpcodes.Multianewarray, numDimensions, descSymbol, _symbolTable);
                else
                    // No need to update maxRelativeStackSize (the stack size delta is always negative).
                    _relativeStackSize += 1 - numDimensions;
            }
        }

        public override AnnotationVisitor VisitInsnAnnotation(int typeRef, TypePath typePath, string descriptor,
            bool visible)
        {
            if (visible)
                return _lastCodeRuntimeVisibleTypeAnnotation = AnnotationWriter.Create(_symbolTable,
                    (typeRef & unchecked((int)0xFF0000FF)) | (_lastBytecodeOffset << 8), typePath, descriptor,
                    _lastCodeRuntimeVisibleTypeAnnotation);
            return _lastCodeRuntimeInvisibleTypeAnnotation = AnnotationWriter.Create(_symbolTable,
                (typeRef & unchecked((int)0xFF0000FF)) | (_lastBytecodeOffset << 8), typePath, descriptor,
                _lastCodeRuntimeInvisibleTypeAnnotation);
        }

        public override void VisitTryCatchBlock(Label start, Label end, Label handler, string type)
        {
            var newHandler = new Handler(start, end, handler,
                !ReferenceEquals(type, null) ? _symbolTable.AddConstantClass(type).index : 0, type);
            if (_firstHandler == null)
                _firstHandler = newHandler;
            else
                _lastHandler.nextHandler = newHandler;
            _lastHandler = newHandler;
        }

        public override AnnotationVisitor VisitTryCatchAnnotation(int typeRef, TypePath typePath, string descriptor,
            bool visible)
        {
            if (visible)
                return _lastCodeRuntimeVisibleTypeAnnotation = AnnotationWriter.Create(_symbolTable, typeRef, typePath,
                    descriptor, _lastCodeRuntimeVisibleTypeAnnotation);
            return _lastCodeRuntimeInvisibleTypeAnnotation = AnnotationWriter.Create(_symbolTable, typeRef, typePath,
                descriptor, _lastCodeRuntimeInvisibleTypeAnnotation);
        }

        public override void VisitLocalVariable(string name, string descriptor, string signature, Label start,
            Label end, int index)
        {
            if (!ReferenceEquals(signature, null))
            {
                if (_localVariableTypeTable == null) _localVariableTypeTable = new ByteVector();
                ++_localVariableTypeTableLength;
                _localVariableTypeTable.PutShort(start.bytecodeOffset)
                    .PutShort(end.bytecodeOffset - start.bytecodeOffset).PutShort(_symbolTable.AddConstantUtf8(name))
                    .PutShort(_symbolTable.AddConstantUtf8(signature)).PutShort(index);
            }

            if (_localVariableTable == null) _localVariableTable = new ByteVector();
            ++_localVariableTableLength;
            _localVariableTable.PutShort(start.bytecodeOffset).PutShort(end.bytecodeOffset - start.bytecodeOffset)
                .PutShort(_symbolTable.AddConstantUtf8(name)).PutShort(_symbolTable.AddConstantUtf8(descriptor))
                .PutShort(index);
            if (_compute != Compute_Nothing)
            {
                var firstDescChar = descriptor[0];
                var currentMaxLocals = index + (firstDescChar == 'J' || firstDescChar == 'D' ? 2 : 1);
                if (currentMaxLocals > _maxLocals) _maxLocals = currentMaxLocals;
            }
        }

        public override AnnotationVisitor VisitLocalVariableAnnotation(int typeRef, TypePath typePath, Label[] start,
            Label[] end, int[] index, string descriptor, bool visible)
        {
            // Create a ByteVector to hold a 'type_annotation' JVMS structure.
            // See https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.7.20.
            var typeAnnotation = new ByteVector();
            // Write target_type, target_info, and target_path.
            typeAnnotation.PutByte((int)((uint)typeRef >> 24)).PutShort(start.Length);
            for (var i = 0; i < start.Length; ++i)
                typeAnnotation.PutShort(start[i].bytecodeOffset)
                    .PutShort(end[i].bytecodeOffset - start[i].bytecodeOffset).PutShort(index[i]);
            TypePath.Put(typePath, typeAnnotation);
            // Write type_index and reserve space for num_element_value_pairs.
            typeAnnotation.PutShort(_symbolTable.AddConstantUtf8(descriptor)).PutShort(0);
            if (visible)
                return _lastCodeRuntimeVisibleTypeAnnotation = new AnnotationWriter(_symbolTable, true, typeAnnotation,
                    _lastCodeRuntimeVisibleTypeAnnotation);
            return _lastCodeRuntimeInvisibleTypeAnnotation = new AnnotationWriter(_symbolTable, true, typeAnnotation,
                _lastCodeRuntimeInvisibleTypeAnnotation);
        }

        public override void VisitLineNumber(int line, Label start)
        {
            if (_lineNumberTable == null) _lineNumberTable = new ByteVector();
            ++_lineNumberTableLength;
            _lineNumberTable.PutShort(start.bytecodeOffset);
            _lineNumberTable.PutShort(line);
        }

        public override void VisitMaxs(int maxStack, int maxLocals)
        {
            if (_compute == Compute_All_Frames)
            {
                ComputeAllFrames();
            }
            else if (_compute == Compute_Max_Stack_And_Local)
            {
                ComputeMaxStackAndLocal();
            }
            else if (_compute == Compute_Max_Stack_And_Local_From_Frames)
            {
                this._maxStack = _maxRelativeStackSize;
            }
            else
            {
                this._maxStack = maxStack;
                this._maxLocals = maxLocals;
            }
        }

        /// <summary>
        ///     Computes all the stack map frames of the method, from scratch.
        /// </summary>
        private void ComputeAllFrames()
        {
            // Complete the control flow graph with exception handler blocks.
            var handler = _firstHandler;
            while (handler != null)
            {
                var catchTypeDescriptor = ReferenceEquals(handler.catchTypeDescriptor, null)
                    ? "java/lang/Throwable"
                    : handler.catchTypeDescriptor;
                var catchType = Frame.GetAbstractTypeFromInternalName(_symbolTable, catchTypeDescriptor);
                // Mark handlerBlock as an exception handler.
                var handlerBlock = handler.handlerPc.CanonicalInstance;
                handlerBlock.flags |= Label.Flag_Jump_Target;
                // Add handlerBlock as a successor of all the basic blocks in the exception handler range.
                var handlerRangeBlock = handler.startPc.CanonicalInstance;
                var handlerRangeEnd = handler.endPc.CanonicalInstance;
                while (handlerRangeBlock != handlerRangeEnd)
                {
                    handlerRangeBlock.outgoingEdges =
                        new Edge(catchType, handlerBlock, handlerRangeBlock.outgoingEdges);
                    handlerRangeBlock = handlerRangeBlock.nextBasicBlock;
                }

                handler = handler.nextHandler;
            }

            // Create and visit the first (implicit) frame.
            var firstFrame = _firstBasicBlock.frame;
            firstFrame.SetInputFrameFromDescriptor(_symbolTable, _accessFlags, _descriptor, _maxLocals);
            firstFrame.Accept(this);

            // Fix point algorithm: add the first basic block to a list of blocks to process (i.e. blocks
            // whose stack map frame has changed) and, while there are blocks to process, remove one from
            // the list and update the stack map frames of its successor blocks in the control flow graph
            // (which might change them, in which case these blocks must be processed too, and are thus
            // added to the list of blocks to process). Also compute the maximum stack size of the method,
            // as a by-product.
            var listOfBlocksToProcess = _firstBasicBlock;
            listOfBlocksToProcess.nextListElement = Label.EmptyList;
            var maxStackSize = 0;
            Label basicBlock;
            while (listOfBlocksToProcess != Label.EmptyList)
            {
                // Remove a basic block from the list of blocks to process.
                basicBlock = listOfBlocksToProcess;
                listOfBlocksToProcess = listOfBlocksToProcess.nextListElement;
                basicBlock.nextListElement = null;
                // By definition, basicBlock is reachable.
                basicBlock.flags |= Label.Flag_Reachable;
                // Update the (absolute) maximum stack size.
                var maxBlockStackSize = basicBlock.frame.InputStackSize + basicBlock.outputStackMax;
                if (maxBlockStackSize > maxStackSize) maxStackSize = maxBlockStackSize;
                // Update the successor blocks of basicBlock in the control flow graph.
                var outgoingEdge = basicBlock.outgoingEdges;
                while (outgoingEdge != null)
                {
                    var successorBlock = outgoingEdge.successor.CanonicalInstance;
                    var successorBlockChanged =
                        basicBlock.frame.Merge(_symbolTable, successorBlock.frame, outgoingEdge.info);
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
            basicBlock = _firstBasicBlock;
            while (basicBlock != null)
            {
                if ((basicBlock.flags & (Label.Flag_Jump_Target | Label.Flag_Reachable)) ==
                    (Label.Flag_Jump_Target | Label.Flag_Reachable)) basicBlock.frame.Accept(this);
                if ((basicBlock.flags & Label.Flag_Reachable) == 0)
                {
                    // Find the start and end bytecode offsets of this unreachable block.
                    var nextBasicBlock = basicBlock.nextBasicBlock;
                    var startOffset = basicBlock.bytecodeOffset;
                    var endOffset = (nextBasicBlock == null ? _code.length : nextBasicBlock.bytecodeOffset) - 1;
                    if (endOffset >= startOffset)
                    {
                        // Replace its instructions with NOP ... NOP ATHROW.
                        for (var i = startOffset; i < endOffset; ++i) _code.data[i] = IOpcodes.Nop;
                        _code.data[endOffset] = unchecked((byte)IOpcodes.Athrow);
                        // Emit a frame for this unreachable block, with no local and a Throwable on the stack
                        // (so that the ATHROW could consume this Throwable if it were reachable).
                        var frameIndex = VisitFrameStart(startOffset, 0, 1);
                        _currentFrame[frameIndex] =
                            Frame.GetAbstractTypeFromInternalName(_symbolTable, "java/lang/Throwable");
                        VisitFrameEnd();
                        // Remove this unreachable basic block from the exception handler ranges.
                        _firstHandler = Handler.RemoveRange(_firstHandler, basicBlock, nextBasicBlock);
                        // The maximum stack size is now at least one, because of the Throwable declared above.
                        maxStackSize = Math.Max(maxStackSize, 1);
                    }
                }

                basicBlock = basicBlock.nextBasicBlock;
            }

            _maxStack = maxStackSize;
        }

        /// <summary>
        ///     Computes the maximum stack size of the method.
        /// </summary>
        private void ComputeMaxStackAndLocal()
        {
            // Complete the control flow graph with exception handler blocks.
            var handler = _firstHandler;
            while (handler != null)
            {
                var handlerBlock = handler.handlerPc;
                var handlerRangeBlock = handler.startPc;
                var handlerRangeEnd = handler.endPc;
                // Add handlerBlock as a successor of all the basic blocks in the exception handler range.
                while (handlerRangeBlock != handlerRangeEnd)
                {
                    if ((handlerRangeBlock.flags & Label.Flag_Subroutine_Caller) == 0)
                        handlerRangeBlock.outgoingEdges =
                            new Edge(Edge.Exception, handlerBlock, handlerRangeBlock.outgoingEdges);
                    else
                        // If handlerRangeBlock is a JSR block, add handlerBlock after the first two outgoing
                        // edges to preserve the hypothesis about JSR block successors order (see
                        // {@link #visitJumpInsn}).
                        handlerRangeBlock.outgoingEdges.nextEdge.nextEdge = new Edge(Edge.Exception, handlerBlock,
                            handlerRangeBlock.outgoingEdges.nextEdge.nextEdge);
                    handlerRangeBlock = handlerRangeBlock.nextBasicBlock;
                }

                handler = handler.nextHandler;
            }

            // Complete the control flow graph with the successor blocks of subroutines, if needed.
            if (_hasSubroutines)
            {
                // First step: find the subroutines. This step determines, for each basic block, to which
                // subroutine(s) it belongs. Start with the main "subroutine":
                short numSubroutines = 1;
                _firstBasicBlock.MarkSubroutine(numSubroutines);
                // Then, mark the subroutines called by the main subroutine, then the subroutines called by
                // those called by the main subroutine, etc.
                Label basicBlock;
                for (short currentSubroutine = 1; currentSubroutine <= numSubroutines; ++currentSubroutine)
                {
                    basicBlock = _firstBasicBlock;
                    while (basicBlock != null)
                    {
                        if ((basicBlock.flags & Label.Flag_Subroutine_Caller) != 0 &&
                            basicBlock.subroutineId == currentSubroutine)
                        {
                            var jsrTarget = basicBlock.outgoingEdges.nextEdge.successor;
                            if (jsrTarget.subroutineId == 0)
                                // If this subroutine has not been marked yet, find its basic blocks.
                                jsrTarget.MarkSubroutine(++numSubroutines);
                        }

                        basicBlock = basicBlock.nextBasicBlock;
                    }
                }

                // Second step: find the successors in the control flow graph of each subroutine basic block
                // 'r' ending with a RET instruction. These successors are the virtual successors of the basic
                // blocks ending with JSR instructions (see {@link #visitJumpInsn)} that can reach 'r'.
                basicBlock = _firstBasicBlock;
                while (basicBlock != null)
                {
                    if ((basicBlock.flags & Label.Flag_Subroutine_Caller) != 0)
                    {
                        // By construction, jsr targets are stored in the second outgoing edge of basic blocks
                        // that ends with a jsr instruction (see {@link #FLAG_SUBROUTINE_CALLER}).
                        var subroutine = basicBlock.outgoingEdges.nextEdge.successor;
                        subroutine.AddSubroutineRetSuccessors(basicBlock);
                    }

                    basicBlock = basicBlock.nextBasicBlock;
                }
            }

            // Data flow algorithm: put the first basic block in a list of blocks to process (i.e. blocks
            // whose input stack size has changed) and, while there are blocks to process, remove one
            // from the list, update the input stack size of its successor blocks in the control flow
            // graph, and add these blocks to the list of blocks to process (if not already done).
            var listOfBlocksToProcess = _firstBasicBlock;
            listOfBlocksToProcess.nextListElement = Label.EmptyList;
            var maxStackSize = _maxStack;
            while (listOfBlocksToProcess != Label.EmptyList)
            {
                // Remove a basic block from the list of blocks to process. Note that we don't reset
                // basicBlock.nextListElement to null on purpose, to make sure we don't reprocess already
                // processed basic blocks.
                var basicBlock = listOfBlocksToProcess;
                listOfBlocksToProcess = listOfBlocksToProcess.nextListElement;
                // Compute the (absolute) input stack size and maximum stack size of this block.
                int inputStackTop = basicBlock.inputStackSize;
                var maxBlockStackSize = inputStackTop + basicBlock.outputStackMax;
                // Update the absolute maximum stack size of the method.
                if (maxBlockStackSize > maxStackSize) maxStackSize = maxBlockStackSize;
                // Update the input stack size of the successor blocks of basicBlock in the control flow
                // graph, and add these blocks to the list of blocks to process, if not already done.
                var outgoingEdge = basicBlock.outgoingEdges;
                if ((basicBlock.flags & Label.Flag_Subroutine_Caller) != 0)
                    // Ignore the first outgoing edge of the basic blocks ending with a jsr: these are virtual
                    // edges which lead to the instruction just after the jsr, and do not correspond to a
                    // possible execution path (see {@link #visitJumpInsn} and
                    // {@link Label#FLAG_SUBROUTINE_CALLER}).
                    outgoingEdge = outgoingEdge.nextEdge;
                while (outgoingEdge != null)
                {
                    var successorBlock = outgoingEdge.successor;
                    if (successorBlock.nextListElement == null)
                    {
                        successorBlock.inputStackSize = (short)(outgoingEdge.info == Edge.Exception
                            ? 1
                            : inputStackTop + outgoingEdge.info);
                        successorBlock.nextListElement = listOfBlocksToProcess;
                        listOfBlocksToProcess = successorBlock;
                    }

                    outgoingEdge = outgoingEdge.nextEdge;
                }
            }

            _maxStack = maxStackSize;
        }

        public override void VisitEnd()
        {
            // Nothing to do.
        }

        // -----------------------------------------------------------------------------------------------
        // Utility methods: control flow analysis algorithm
        // -----------------------------------------------------------------------------------------------

        /// <summary>
        ///     Adds a successor to <seealso cref="_currentBasicBlock" /> in the control flow graph.
        /// </summary>
        /// <param name="info"> information about the control flow edge to be added. </param>
        /// <param name="successor"> the successor block to be added to the current basic block. </param>
        private void AddSuccessorToCurrentBasicBlock(int info, Label successor)
        {
            _currentBasicBlock.outgoingEdges = new Edge(info, successor, _currentBasicBlock.outgoingEdges);
        }

        /// <summary>
        ///     Ends the current basic block. This method must be used in the case where the current basic
        ///     block does not have any successor.
        ///     <para>
        ///         WARNING: this method must be called after the currently visited instruction has been put in
        ///         <seealso cref="_code" /> (if frames are computed, this method inserts a new Label to start a new basic
        ///         block after the current instruction).
        ///     </para>
        /// </summary>
        private void EndCurrentBasicBlockWithNoSuccessor()
        {
            if (_compute == Compute_All_Frames)
            {
                var nextBasicBlock = new Label();
                nextBasicBlock.frame = new Frame(nextBasicBlock);
                nextBasicBlock.Resolve(_code.data, _code.length);
                _lastBasicBlock.nextBasicBlock = nextBasicBlock;
                _lastBasicBlock = nextBasicBlock;
                _currentBasicBlock = null;
            }
            else if (_compute == Compute_Max_Stack_And_Local)
            {
                _currentBasicBlock.outputStackMax = (short)_maxRelativeStackSize;
                _currentBasicBlock = null;
            }
        }

        // -----------------------------------------------------------------------------------------------
        // Utility methods: stack map frames
        // -----------------------------------------------------------------------------------------------

        /// <summary>
        ///     Starts the visit of a new stack map frame, stored in <seealso cref="_currentFrame" />.
        /// </summary>
        /// <param name="offset"> the bytecode offset of the instruction to which the frame corresponds. </param>
        /// <param name="numLocal"> the number of local variables in the frame. </param>
        /// <param name="numStack"> the number of stack elements in the frame. </param>
        /// <returns> the index of the next element to be written in this frame. </returns>
        public int VisitFrameStart(int offset, int numLocal, int numStack)
        {
            var frameLength = 3 + numLocal + numStack;
            if (_currentFrame == null || _currentFrame.Length < frameLength) _currentFrame = new int[frameLength];
            _currentFrame[0] = offset;
            _currentFrame[1] = numLocal;
            _currentFrame[2] = numStack;
            return 3;
        }

        /// <summary>
        ///     Sets an abstract type in <seealso cref="_currentFrame" />.
        /// </summary>
        /// <param name="frameIndex"> the index of the element to be set in <seealso cref="_currentFrame" />. </param>
        /// <param name="abstractType"> an abstract type. </param>
        public void VisitAbstractType(int frameIndex, int abstractType)
        {
            _currentFrame[frameIndex] = abstractType;
        }

        /// <summary>
        ///     Ends the visit of <seealso cref="_currentFrame" /> by writing it in the StackMapTable entries and by
        ///     updating the StackMapTable number_of_entries (except if the current frame is the first one,
        ///     which is implicit in StackMapTable). Then resets <seealso cref="_currentFrame" /> to {@literal null}.
        /// </summary>
        public void VisitFrameEnd()
        {
            if (_previousFrame != null)
            {
                if (_stackMapTableEntries == null) _stackMapTableEntries = new ByteVector();
                PutFrame();
                ++_stackMapTableNumberOfEntries;
            }

            _previousFrame = _currentFrame;
            _currentFrame = null;
        }

        /// <summary>
        ///     Compresses and writes <seealso cref="_currentFrame" /> in a new StackMapTable entry.
        /// </summary>
        private void PutFrame()
        {
            var numLocal = _currentFrame[1];
            var numStack = _currentFrame[2];
            if (_symbolTable.MajorVersion < IOpcodes.V1_6)
            {
                // Generate a StackMap attribute entry, which are always uncompressed.
                _stackMapTableEntries.PutShort(_currentFrame[0]).PutShort(numLocal);
                PutAbstractTypes(3, 3 + numLocal);
                _stackMapTableEntries.PutShort(numStack);
                PutAbstractTypes(3 + numLocal, 3 + numLocal + numStack);
                return;
            }

            var offsetDelta = _stackMapTableNumberOfEntries == 0
                ? _currentFrame[0]
                : _currentFrame[0] - _previousFrame[0] - 1;
            var previousNumlocal = _previousFrame[1];
            var numLocalDelta = numLocal - previousNumlocal;
            var type = Frame.Full_Frame;
            if (numStack == 0)
                switch (numLocalDelta)
                {
                    case -3:
                    case -2:
                    case -1:
                        type = Frame.Chop_Frame;
                        break;
                    case 0:
                        type = offsetDelta < 64 ? Frame.Same_Frame : Frame.Same_Frame_Extended;
                        break;
                    case 1:
                    case 2:
                    case 3:
                        type = Frame.Append_Frame;
                        break;
                }
            else if (numLocalDelta == 0 && numStack == 1)
                type = offsetDelta < 63
                    ? Frame.Same_Locals_1_Stack_Item_Frame
                    : Frame.Same_Locals_1_Stack_Item_Frame_Extended;

            if (type != Frame.Full_Frame)
            {
                // Verify if locals are the same as in the previous frame.
                var frameIndex = 3;
                for (var i = 0; i < previousNumlocal && i < numLocal; i++)
                {
                    if (_currentFrame[frameIndex] != _previousFrame[frameIndex])
                    {
                        type = Frame.Full_Frame;
                        break;
                    }

                    frameIndex++;
                }
            }

            switch (type)
            {
                case Frame.Same_Frame:
                    _stackMapTableEntries.PutByte(offsetDelta);
                    break;
                case Frame.Same_Locals_1_Stack_Item_Frame:
                    _stackMapTableEntries.PutByte(Frame.Same_Locals_1_Stack_Item_Frame + offsetDelta);
                    PutAbstractTypes(3 + numLocal, 4 + numLocal);
                    break;
                case Frame.Same_Locals_1_Stack_Item_Frame_Extended:
                    _stackMapTableEntries.PutByte(Frame.Same_Locals_1_Stack_Item_Frame_Extended).PutShort(offsetDelta);
                    PutAbstractTypes(3 + numLocal, 4 + numLocal);
                    break;
                case Frame.Same_Frame_Extended:
                    _stackMapTableEntries.PutByte(Frame.Same_Frame_Extended).PutShort(offsetDelta);
                    break;
                case Frame.Chop_Frame:
                    _stackMapTableEntries.PutByte(Frame.Same_Frame_Extended + numLocalDelta).PutShort(offsetDelta);
                    break;
                case Frame.Append_Frame:
                    _stackMapTableEntries.PutByte(Frame.Same_Frame_Extended + numLocalDelta).PutShort(offsetDelta);
                    PutAbstractTypes(3 + previousNumlocal, 3 + numLocal);
                    break;
                case Frame.Full_Frame:
                default:
                    _stackMapTableEntries.PutByte(Frame.Full_Frame).PutShort(offsetDelta).PutShort(numLocal);
                    PutAbstractTypes(3, 3 + numLocal);
                    _stackMapTableEntries.PutShort(numStack);
                    PutAbstractTypes(3 + numLocal, 3 + numLocal + numStack);
                    break;
            }
        }

        /// <summary>
        ///     Puts some abstract types of <seealso cref="_currentFrame" /> in <seealso cref="_stackMapTableEntries" /> , using the
        ///     JVMS verification_type_info format used in StackMapTable attributes.
        /// </summary>
        /// <param name="start"> index of the first type in <seealso cref="_currentFrame" /> to write. </param>
        /// <param name="end"> index of last type in <seealso cref="_currentFrame" /> to write (exclusive). </param>
        private void PutAbstractTypes(int start, int end)
        {
            for (var i = start; i < end; ++i)
                Frame.PutAbstractType(_symbolTable, _currentFrame[i], _stackMapTableEntries);
        }

        /// <summary>
        ///     Puts the given public API frame element type in <seealso cref="_stackMapTableEntries" /> , using the JVMS
        ///     verification_type_info format used in StackMapTable attributes.
        /// </summary>
        /// <param name="type">
        ///     a frame element type described using the same format as in {@link
        ///     MethodVisitor#visitFrame}, i.e. either <seealso cref="IIOpcodes.top />, <seealso cref="IIOpcodes.integer />, {@link
        ///     Opcodes#FLOAT}, <seealso cref="IIOpcodes.long />, <seealso cref="IIOpcodes.double />, <seealso cref="IIOpcodes.null />
        ///     , or
        ///     <seealso cref="IIOpcodes.uninitializedThis />, or the internal name of a class, or a Label designating
        ///     a NEW instruction (for uninitialized types).
        /// </param>
        private void PutFrameType(object type)
        {
            if (type is int)
                _stackMapTableEntries.PutByte(((int?)type).Value);
            else if (type is string)
                _stackMapTableEntries.PutByte(Frame.Item_Object)
                    .PutShort(_symbolTable.AddConstantClass((string)type).index);
            else
                _stackMapTableEntries.PutByte(Frame.Item_Uninitialized).PutShort(((Label)type).bytecodeOffset);
        }

        // -----------------------------------------------------------------------------------------------
        // Utility methods
        // -----------------------------------------------------------------------------------------------

        /// <summary>
        ///     Returns whether the attributes of this method can be copied from the attributes of the given
        ///     method (assuming there is no method visitor between the given ClassReader and this
        ///     MethodWriter). This method should only be called just after this MethodWriter has been created,
        ///     and before any content is visited. It returns true if the attributes corresponding to the
        ///     constructor arguments (at most a Signature, an Exception, a Deprecated and a Synthetic
        ///     attribute) are the same as the corresponding attributes in the given method.
        /// </summary>
        /// <param name="source"> the source ClassReader from which the attributes of this method might be copied. </param>
        /// <param name="hasSyntheticAttribute">
        ///     whether the method_info JVMS structure from which the attributes
        ///     of this method might be copied contains a Synthetic attribute.
        /// </param>
        /// <param name="hasDeprecatedAttribute">
        ///     whether the method_info JVMS structure from which the attributes
        ///     of this method might be copied contains a Deprecated attribute.
        /// </param>
        /// <param name="descriptorIndex">
        ///     the descriptor_index field of the method_info JVMS structure from which
        ///     the attributes of this method might be copied.
        /// </param>
        /// <param name="signatureIndex">
        ///     the constant pool index contained in the Signature attribute of the
        ///     method_info JVMS structure from which the attributes of this method might be copied, or 0.
        /// </param>
        /// <param name="exceptionsOffset">
        ///     the offset in 'source.b' of the Exceptions attribute of the method_info
        ///     JVMS structure from which the attributes of this method might be copied, or 0.
        /// </param>
        /// <returns>
        ///     whether the attributes of this method can be copied from the attributes of the
        ///     method_info JVMS structure in 'source.b', between 'methodInfoOffset' and 'methodInfoOffset'
        ///     + 'methodInfoLength'.
        /// </returns>
        public bool CanCopyMethodAttributes(ClassReader source, bool hasSyntheticAttribute, bool hasDeprecatedAttribute,
            int descriptorIndex, int signatureIndex, int exceptionsOffset)
        {
            // If the method descriptor has changed, with more locals than the max_locals field of the
            // original Code attribute, if any, then the original method attributes can't be copied. A
            // conservative check on the descriptor changes alone ensures this (being more precise is not
            // worth the additional complexity, because these cases should be rare -- if a transform changes
            // a method descriptor, most of the time it needs to change the method's code too).
            if (source != _symbolTable.Source || descriptorIndex != this._descriptorIndex ||
                signatureIndex != this._signatureIndex ||
                hasDeprecatedAttribute != ((_accessFlags & IOpcodes.Acc_Deprecated) != 0)) return false;
            var needSyntheticAttribute =
                _symbolTable.MajorVersion < IOpcodes.V1_5 && (_accessFlags & IOpcodes.Acc_Synthetic) != 0;
            if (hasSyntheticAttribute != needSyntheticAttribute) return false;
            if (exceptionsOffset == 0)
            {
                if (_numberOfExceptions != 0) return false;
            }
            else if (source.ReadUnsignedShort(exceptionsOffset) == _numberOfExceptions)
            {
                var currentExceptionOffset = exceptionsOffset + 2;
                for (var i = 0; i < _numberOfExceptions; ++i)
                {
                    if (source.ReadUnsignedShort(currentExceptionOffset) != _exceptionIndexTable[i]) return false;
                    currentExceptionOffset += 2;
                }
            }

            return true;
        }

        /// <summary>
        ///     Sets the source from which the attributes of this method will be copied.
        /// </summary>
        /// <param name="methodInfoOffset">
        ///     the offset in 'symbolTable.getSource()' of the method_info JVMS
        ///     structure from which the attributes of this method will be copied.
        /// </param>
        /// <param name="methodInfoLength">
        ///     the length in 'symbolTable.getSource()' of the method_info JVMS
        ///     structure from which the attributes of this method will be copied.
        /// </param>
        public void SetMethodAttributesSource(int methodInfoOffset, int methodInfoLength)
        {
            // Don't copy the attributes yet, instead store their location in the source class reader so
            // they can be copied later, in {@link #putMethodInfo}. Note that we skip the 6 header bytes
            // of the method_info JVMS structure.
            _sourceOffset = methodInfoOffset + 6;
            _sourceLength = methodInfoLength - 6;
        }

        /// <summary>
        ///     Returns the size of the method_info JVMS structure generated by this MethodWriter. Also add the
        ///     names of the attributes of this method in the constant pool.
        /// </summary>
        /// <returns> the size in bytes of the method_info JVMS structure. </returns>
        public int ComputeMethodInfoSize()
        {
            // If this method_info must be copied from an existing one, the size computation is trivial.
            if (_sourceOffset != 0)
                // sourceLength excludes the first 6 bytes for access_flags, name_index and descriptor_index.
                return 6 + _sourceLength;
            // 2 bytes each for access_flags, name_index, descriptor_index and attributes_count.
            var size = 8;
            // For ease of reference, we use here the same attribute order as in Section 4.7 of the JVMS.
            if (_code.length > 0)
            {
                if (_code.length > 65535)
                    throw new MethodTooLargeException(_symbolTable.ClassName, _name, _descriptor, _code.length);
                _symbolTable.AddConstantUtf8(Constants.Code);
                // The Code attribute has 6 header bytes, plus 2, 2, 4 and 2 bytes respectively for max_stack,
                // max_locals, code_length and attributes_count, plus the bytecode and the exception table.
                size += 16 + _code.length + Handler.GetExceptionTableSize(_firstHandler);
                if (_stackMapTableEntries != null)
                {
                    var useStackMapTable = _symbolTable.MajorVersion >= IOpcodes.V1_6;
                    _symbolTable.AddConstantUtf8(useStackMapTable ? Constants.Stack_Map_Table : "StackMap");
                    // 6 header bytes and 2 bytes for number_of_entries.
                    size += 8 + _stackMapTableEntries.length;
                }

                if (_lineNumberTable != null)
                {
                    _symbolTable.AddConstantUtf8(Constants.Line_Number_Table);
                    // 6 header bytes and 2 bytes for line_number_table_length.
                    size += 8 + _lineNumberTable.length;
                }

                if (_localVariableTable != null)
                {
                    _symbolTable.AddConstantUtf8(Constants.Local_Variable_Table);
                    // 6 header bytes and 2 bytes for local_variable_table_length.
                    size += 8 + _localVariableTable.length;
                }

                if (_localVariableTypeTable != null)
                {
                    _symbolTable.AddConstantUtf8(Constants.Local_Variable_Type_Table);
                    // 6 header bytes and 2 bytes for local_variable_type_table_length.
                    size += 8 + _localVariableTypeTable.length;
                }

                if (_lastCodeRuntimeVisibleTypeAnnotation != null)
                    size += _lastCodeRuntimeVisibleTypeAnnotation.ComputeAnnotationsSize(Constants
                        .Runtime_Visible_Type_Annotations);
                if (_lastCodeRuntimeInvisibleTypeAnnotation != null)
                    size += _lastCodeRuntimeInvisibleTypeAnnotation.ComputeAnnotationsSize(Constants
                        .Runtime_Invisible_Type_Annotations);
                if (_firstCodeAttribute != null)
                    size += _firstCodeAttribute.ComputeAttributesSize(_symbolTable, _code.data, _code.length, _maxStack,
                        _maxLocals);
            }

            if (_numberOfExceptions > 0)
            {
                _symbolTable.AddConstantUtf8(Constants.Exceptions);
                size += 8 + 2 * _numberOfExceptions;
            }

            size += Attribute.ComputeAttributesSize(_symbolTable, _accessFlags, _signatureIndex);
            size += AnnotationWriter.ComputeAnnotationsSize(_lastRuntimeVisibleAnnotation,
                _lastRuntimeInvisibleAnnotation, _lastRuntimeVisibleTypeAnnotation,
                _lastRuntimeInvisibleTypeAnnotation);
            if (_lastRuntimeVisibleParameterAnnotations != null)
                size += AnnotationWriter.ComputeParameterAnnotationsSize(
                    Constants.Runtime_Visible_Parameter_Annotations, _lastRuntimeVisibleParameterAnnotations,
                    _visibleAnnotableParameterCount == 0
                        ? _lastRuntimeVisibleParameterAnnotations.Length
                        : _visibleAnnotableParameterCount);
            if (_lastRuntimeInvisibleParameterAnnotations != null)
                size += AnnotationWriter.ComputeParameterAnnotationsSize(
                    Constants.Runtime_Invisible_Parameter_Annotations, _lastRuntimeInvisibleParameterAnnotations,
                    _invisibleAnnotableParameterCount == 0
                        ? _lastRuntimeInvisibleParameterAnnotations.Length
                        : _invisibleAnnotableParameterCount);
            if (_defaultValue != null)
            {
                _symbolTable.AddConstantUtf8(Constants.Annotation_Default);
                size += 6 + _defaultValue.length;
            }

            if (_parameters != null)
            {
                _symbolTable.AddConstantUtf8(Constants.Method_Parameters);
                // 6 header bytes and 1 byte for parameters_count.
                size += 7 + _parameters.length;
            }

            if (_firstAttribute != null) size += _firstAttribute.ComputeAttributesSize(_symbolTable);
            return size;
        }

        /// <summary>
        ///     Puts the content of the method_info JVMS structure generated by this MethodWriter into the
        ///     given ByteVector.
        /// </summary>
        /// <param name="output"> where the method_info structure must be put. </param>
        public void PutMethodInfo(ByteVector output)
        {
            var useSyntheticAttribute = _symbolTable.MajorVersion < IOpcodes.V1_5;
            var mask = useSyntheticAttribute ? IOpcodes.Acc_Synthetic : 0;
            output.PutShort(_accessFlags & ~mask).PutShort(_nameIndex).PutShort(_descriptorIndex);
            // If this method_info must be copied from an existing one, copy it now and return early.
            if (_sourceOffset != 0)
            {
                output.PutByteArray(_symbolTable.Source.classFileBuffer, _sourceOffset, _sourceLength);
                return;
            }

            // For ease of reference, we use here the same attribute order as in Section 4.7 of the JVMS.
            var attributeCount = 0;
            if (_code.length > 0) ++attributeCount;
            if (_numberOfExceptions > 0) ++attributeCount;
            if ((_accessFlags & IOpcodes.Acc_Synthetic) != 0 && useSyntheticAttribute) ++attributeCount;
            if (_signatureIndex != 0) ++attributeCount;
            if ((_accessFlags & IOpcodes.Acc_Deprecated) != 0) ++attributeCount;
            if (_lastRuntimeVisibleAnnotation != null) ++attributeCount;
            if (_lastRuntimeInvisibleAnnotation != null) ++attributeCount;
            if (_lastRuntimeVisibleParameterAnnotations != null) ++attributeCount;
            if (_lastRuntimeInvisibleParameterAnnotations != null) ++attributeCount;
            if (_lastRuntimeVisibleTypeAnnotation != null) ++attributeCount;
            if (_lastRuntimeInvisibleTypeAnnotation != null) ++attributeCount;
            if (_defaultValue != null) ++attributeCount;
            if (_parameters != null) ++attributeCount;
            if (_firstAttribute != null) attributeCount += _firstAttribute.AttributeCount;
            // For ease of reference, we use here the same attribute order as in Section 4.7 of the JVMS.
            output.PutShort(attributeCount);
            if (_code.length > 0)
            {
                // 2, 2, 4 and 2 bytes respectively for max_stack, max_locals, code_length and
                // attributes_count, plus the bytecode and the exception table.
                var size = 10 + _code.length + Handler.GetExceptionTableSize(_firstHandler);
                var codeAttributeCount = 0;
                if (_stackMapTableEntries != null)
                {
                    // 6 header bytes and 2 bytes for number_of_entries.
                    size += 8 + _stackMapTableEntries.length;
                    ++codeAttributeCount;
                }

                if (_lineNumberTable != null)
                {
                    // 6 header bytes and 2 bytes for line_number_table_length.
                    size += 8 + _lineNumberTable.length;
                    ++codeAttributeCount;
                }

                if (_localVariableTable != null)
                {
                    // 6 header bytes and 2 bytes for local_variable_table_length.
                    size += 8 + _localVariableTable.length;
                    ++codeAttributeCount;
                }

                if (_localVariableTypeTable != null)
                {
                    // 6 header bytes and 2 bytes for local_variable_type_table_length.
                    size += 8 + _localVariableTypeTable.length;
                    ++codeAttributeCount;
                }

                if (_lastCodeRuntimeVisibleTypeAnnotation != null)
                {
                    size += _lastCodeRuntimeVisibleTypeAnnotation.ComputeAnnotationsSize(Constants
                        .Runtime_Visible_Type_Annotations);
                    ++codeAttributeCount;
                }

                if (_lastCodeRuntimeInvisibleTypeAnnotation != null)
                {
                    size += _lastCodeRuntimeInvisibleTypeAnnotation.ComputeAnnotationsSize(Constants
                        .Runtime_Invisible_Type_Annotations);
                    ++codeAttributeCount;
                }

                if (_firstCodeAttribute != null)
                {
                    size += _firstCodeAttribute.ComputeAttributesSize(_symbolTable, _code.data, _code.length, _maxStack,
                        _maxLocals);
                    codeAttributeCount += _firstCodeAttribute.AttributeCount;
                }

                output.PutShort(_symbolTable.AddConstantUtf8(Constants.Code)).PutInt(size).PutShort(_maxStack)
                    .PutShort(_maxLocals).PutInt(_code.length).PutByteArray(_code.data, 0, _code.length);
                Handler.PutExceptionTable(_firstHandler, output);
                output.PutShort(codeAttributeCount);
                if (_stackMapTableEntries != null)
                {
                    var useStackMapTable = _symbolTable.MajorVersion >= IOpcodes.V1_6;
                    output
                        .PutShort(
                            _symbolTable.AddConstantUtf8(useStackMapTable ? Constants.Stack_Map_Table : "StackMap"))
                        .PutInt(2 + _stackMapTableEntries.length).PutShort(_stackMapTableNumberOfEntries)
                        .PutByteArray(_stackMapTableEntries.data, 0, _stackMapTableEntries.length);
                }

                if (_lineNumberTable != null)
                    output.PutShort(_symbolTable.AddConstantUtf8(Constants.Line_Number_Table))
                        .PutInt(2 + _lineNumberTable.length).PutShort(_lineNumberTableLength)
                        .PutByteArray(_lineNumberTable.data, 0, _lineNumberTable.length);
                if (_localVariableTable != null)
                    output.PutShort(_symbolTable.AddConstantUtf8(Constants.Local_Variable_Table))
                        .PutInt(2 + _localVariableTable.length).PutShort(_localVariableTableLength)
                        .PutByteArray(_localVariableTable.data, 0, _localVariableTable.length);
                if (_localVariableTypeTable != null)
                    output.PutShort(_symbolTable.AddConstantUtf8(Constants.Local_Variable_Type_Table))
                        .PutInt(2 + _localVariableTypeTable.length).PutShort(_localVariableTypeTableLength)
                        .PutByteArray(_localVariableTypeTable.data, 0, _localVariableTypeTable.length);
                if (_lastCodeRuntimeVisibleTypeAnnotation != null)
                    _lastCodeRuntimeVisibleTypeAnnotation.PutAnnotations(
                        _symbolTable.AddConstantUtf8(Constants.Runtime_Visible_Type_Annotations), output);
                if (_lastCodeRuntimeInvisibleTypeAnnotation != null)
                    _lastCodeRuntimeInvisibleTypeAnnotation.PutAnnotations(
                        _symbolTable.AddConstantUtf8(Constants.Runtime_Invisible_Type_Annotations), output);
                if (_firstCodeAttribute != null)
                    _firstCodeAttribute.PutAttributes(_symbolTable, _code.data, _code.length, _maxStack, _maxLocals,
                        output);
            }

            if (_numberOfExceptions > 0)
            {
                output.PutShort(_symbolTable.AddConstantUtf8(Constants.Exceptions)).PutInt(2 + 2 * _numberOfExceptions)
                    .PutShort(_numberOfExceptions);
                foreach (var exceptionIndex in _exceptionIndexTable) output.PutShort(exceptionIndex);
            }

            Attribute.PutAttributes(_symbolTable, _accessFlags, _signatureIndex, output);
            AnnotationWriter.PutAnnotations(_symbolTable, _lastRuntimeVisibleAnnotation,
                _lastRuntimeInvisibleAnnotation,
                _lastRuntimeVisibleTypeAnnotation, _lastRuntimeInvisibleTypeAnnotation, output);
            if (_lastRuntimeVisibleParameterAnnotations != null)
                AnnotationWriter.PutParameterAnnotations(
                    _symbolTable.AddConstantUtf8(Constants.Runtime_Visible_Parameter_Annotations),
                    _lastRuntimeVisibleParameterAnnotations,
                    _visibleAnnotableParameterCount == 0
                        ? _lastRuntimeVisibleParameterAnnotations.Length
                        : _visibleAnnotableParameterCount, output);
            if (_lastRuntimeInvisibleParameterAnnotations != null)
                AnnotationWriter.PutParameterAnnotations(
                    _symbolTable.AddConstantUtf8(Constants.Runtime_Invisible_Parameter_Annotations),
                    _lastRuntimeInvisibleParameterAnnotations,
                    _invisibleAnnotableParameterCount == 0
                        ? _lastRuntimeInvisibleParameterAnnotations.Length
                        : _invisibleAnnotableParameterCount, output);
            if (_defaultValue != null)
                output.PutShort(_symbolTable.AddConstantUtf8(Constants.Annotation_Default)).PutInt(_defaultValue.length)
                    .PutByteArray(_defaultValue.data, 0, _defaultValue.length);
            if (_parameters != null)
                output.PutShort(_symbolTable.AddConstantUtf8(Constants.Method_Parameters))
                    .PutInt(1 + _parameters.length)
                    .PutByte(_parametersCount).PutByteArray(_parameters.data, 0, _parameters.length);
            if (_firstAttribute != null) _firstAttribute.PutAttributes(_symbolTable, output);
        }

        /// <summary>
        ///     Collects the attributes of this method into the given set of attribute prototypes.
        /// </summary>
        /// <param name="attributePrototypes"> a set of attribute prototypes. </param>
        public void CollectAttributePrototypes(Attribute.Set attributePrototypes)
        {
            attributePrototypes.AddAttributes(_firstAttribute);
            attributePrototypes.AddAttributes(_firstCodeAttribute);
        }
    }
}