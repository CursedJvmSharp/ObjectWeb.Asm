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
    ///     A position in the bytecode of a method. Labels are used for jump, goto, and switch instructions,
    ///     and for try catch blocks. A label designates the <i>instruction</i> that is just after. Note
    ///     however that there can be other elements between a label and the instruction it designates (such
    ///     as other labels, stack map frames, line numbers, etc.).
    ///     @author Eric Bruneton
    /// </summary>
    public class Label
    {
        /// <summary>
        ///     A flag indicating that a label is only used for debug attributes. Such a label is not the start
        ///     of a basic block, the target of a jump instruction, or an exception handler. It can be safely
        ///     ignored in control flow graph analysis algorithms (for optimization purposes).
        /// </summary>
        internal const int Flag_Debug_Only = 1;

        /// <summary>
        ///     A flag indicating that a label is the target of a jump instruction, or the start of an
        ///     exception handler.
        /// </summary>
        internal const int Flag_Jump_Target = 2;

        /// <summary>
        ///     A flag indicating that the bytecode offset of a label is known.
        /// </summary>
        internal const int Flag_Resolved = 4;

        /// <summary>
        ///     A flag indicating that a label corresponds to a reachable basic block.
        /// </summary>
        internal const int Flag_Reachable = 8;

        /// <summary>
        ///     A flag indicating that the basic block corresponding to a label ends with a subroutine call. By
        ///     construction in <seealso cref = "MethodWriter.VisitJumpInsn"/>, labels with this flag set have at least two
        ///     outgoing edges:
        ///     <ul>
        ///         <li>
        ///             the first one corresponds to the instruction that follows the jsr instruction in the
        ///             bytecode, i.e. where execution continues when it returns from the jsr call. This is a
        ///             virtual control flow edge, since execution never goes directly from the jsr to the next
        ///             instruction. Instead, it goes to the subroutine and eventually returns to the instruction
        ///             following the jsr. This virtual edge is used to compute the real outgoing edges of the
        ///             basic blocks ending with a ret instruction, in <seealso cref = "AddSubroutineRetSuccessors"/>.
        ///             <li>the second one corresponds to the target of the jsr instruction,
        ///     </ul>
        /// </summary>
        internal const int Flag_Subroutine_Caller = 16;

        /// <summary>
        ///     A flag indicating that the basic block corresponding to a label is the start of a subroutine.
        /// </summary>
        internal const int Flag_Subroutine_Start = 32;

        /// <summary>
        ///     A flag indicating that the basic block corresponding to a label is the end of a subroutine.
        /// </summary>
        internal const int Flag_Subroutine_End = 64;

        /// <summary>
        ///     The number of elements to add to the <seealso cref = "_otherLineNumbers"/> array when it needs to be
        ///     resized to store a new source line number.
        /// </summary>
        internal const int Line_Numbers_Capacity_Increment = 4;

        /// <summary>
        ///     The number of elements to add to the <seealso cref = "_forwardReferences"/> array when it needs to be
        ///     resized to store a new forward reference.
        /// </summary>
        internal const int Forward_References_Capacity_Increment = 6;

        /// <summary>
        ///     The bit mask to extract the type of a forward reference to this label. The extracted type is
        ///     either <seealso cref = "Forward_Reference_Type_Short"/> or <seealso cref = "Forward_Reference_Type_Wide"/>.
        /// </summary>
        /// <seealso cref =  # forwardReferences
        /// </seealso>
        internal const int Forward_Reference_Type_Mask = unchecked((int)0xF0000000);

        /// <summary>
        ///     The type of forward references stored with two bytes in the bytecode. This is the case, for
        ///     instance, of a forward reference from an ifnull instruction.
        /// </summary>
        internal const int Forward_Reference_Type_Short = 0x10000000;

        /// <summary>
        ///     The type of forward references stored in four bytes in the bytecode. This is the case, for
        ///     instance, of a forward reference from a lookupswitch instruction.
        /// </summary>
        internal const int Forward_Reference_Type_Wide = 0x20000000;

        /// <summary>
        ///     The bit mask to extract the 'handle' of a forward reference to this label. The extracted handle
        ///     is the bytecode offset where the forward reference value is stored (using either 2 or 4 bytes,
        ///     as indicated by the <seealso cref = "Forward_Reference_Type_Mask"/>).
        /// </summary>
        /// <seealso cref =  # forwardReferences
        /// </seealso>
        internal const int Forward_Reference_Handle_Mask = 0x0FFFFFFF;

        /// <summary>
        ///     A sentinel element used to indicate the end of a list of labels.
        /// </summary>
        /// <seealso cref =  # nextListElement
        /// </seealso>
        internal static readonly Label EmptyList = new();

        /// <summary>
        ///     The offset of this label in the bytecode of its method, in bytes. This value is set if and only
        ///     if the <seealso cref = "Flag_Resolved"/> flag is set.
        /// </summary>
        internal int bytecodeOffset;

        /// <summary>
        ///     The type and status of this label or its corresponding basic block. Must be zero or more of
        ///     <seealso cref = "Flag_Debug_Only"/>, <seealso cref = "Flag_Jump_Target"/>, <seealso cref = "Flag_Resolved"/>, {@link
        ///     #FLAG_REACHABLE}, <seealso cref = "Flag_Subroutine_Caller"/>, <seealso cref = "Flag_Subroutine_Start"/>, {@link
        ///     #FLAG_SUBROUTINE_END}.
        /// </summary>
        internal short flags;

        /// <summary>
        ///     The forward references to this label. The first element is the number of forward references,
        ///     times 2 (this corresponds to the index of the last element actually used in this array). Then,
        ///     each forward reference is described with two consecutive integers noted
        ///     'sourceInsnBytecodeOffset' and 'reference':
        ///     <ul>
        ///         <li>
        ///             'sourceInsnBytecodeOffset' is the bytecode offset of the instruction that contains the
        ///             forward reference,
        ///             <li>
        ///                 'reference' contains the type and the offset in the bytecode where the forward reference
        ///                 value must be stored, which can be extracted with <seealso cref = "Forward_Reference_Type_Mask"/>
        ///                 and <seealso cref = "Forward_Reference_Handle_Mask"/>.
        ///     </ul>
        ///     <para>
        ///         For instance, for an ifnull instruction at bytecode offset x, 'sourceInsnBytecodeOffset' is
        ///         equal to x, and 'reference' is of type <seealso cref = "Forward_Reference_Type_Short"/> with value x + 1
        ///         (because the ifnull instruction uses a 2 bytes bytecode offset operand stored one byte after
        ///         the start of the instruction itself). For the default case of a lookupswitch instruction at
        ///         bytecode offset x, 'sourceInsnBytecodeOffset' is equal to x, and 'reference' is of type {@link
        ///         #FORWARD_REFERENCE_TYPE_WIDE} with value between x + 1 and x + 4 (because the lookupswitch
        ///         instruction uses a 4 bytes bytecode offset operand stored one to four bytes after the start of
        ///         the instruction itself).
        ///     </para>
        /// </summary>
        private int[] _forwardReferences;

        /// <summary>
        ///     The input and output stack map frames of the basic block corresponding to this label. This
        ///     field is only used when the <seealso cref = "MethodWriter.Compute_All_Frames"/> or {@link
        ///     MethodWriter#COMPUTE_INSERTED_FRAMES} option is used.
        /// </summary>
        internal Frame frame;

        /// <summary>
        ///     A user managed state associated with this label. Warning: this field is used by the ASM tree
        ///     package. In order to use it with the ASM tree package you must override the getLabelNode method
        ///     in MethodNode.
        /// </summary>
        public object Info { get; set; }

        // -----------------------------------------------------------------------------------------------
        // Fields for the control flow and data flow graph analysis algorithms (used to compute the
        // maximum stack size or the stack map frames). A control flow graph contains one node per "basic
        // block", and one edge per "jump" from one basic block to another. Each node (i.e., each basic
        // block) is represented with the Label object that corresponds to the first instruction of this
        // basic block. Each node also stores the list of its successors in the graph, as a linked list of
        // Edge objects.
        //
        // The control flow analysis algorithms used to compute the maximum stack size or the stack map
        // frames are similar and use two steps. The first step, during the visit of each instruction,
        // builds information about the state of the local variables and the operand stack at the end of
        // each basic block, called the "output frame", <i>relatively</i> to the frame state at the
        // beginning of the basic block, which is called the "input frame", and which is <i>unknown</i>
        // during this step. The second step, in {@link MethodWriter#computeAllFrames} and {@link
        // MethodWriter#computeMaxStackAndLocal}, is a fix point algorithm
        // that computes information about the input frame of each basic block, from the input state of
        // the first basic block (known from the method signature), and by the using the previously
        // computed relative output frames.
        //
        // The algorithm used to compute the maximum stack size only computes the relative output and
        // absolute input stack heights, while the algorithm used to compute stack map frames computes
        // relative output frames and absolute input frames.
        /// <summary>
        ///     The number of elements in the input stack of the basic block corresponding to this label. This
        ///     field is computed in <seealso cref = "MethodWriter.ComputeMaxStackAndLocal"/>.
        /// </summary>
        internal short inputStackSize;

        /// <summary>
        ///     The source line number corresponding to this label, or 0. If there are several source line
        ///     numbers corresponding to this label, the first one is stored in this field, and the remaining
        ///     ones are stored in <seealso cref = "_otherLineNumbers"/>.
        /// </summary>
        private short _lineNumber;

        /// <summary>
        ///     The successor of this label, in the order they are visited in <seealso cref = "MethodVisitor.VisitLabel"/>.
        ///     This linked list does not include labels used for debug info only. If the {@link
        ///     MethodWriter#COMPUTE_ALL_FRAMES} or <seealso cref = "MethodWriter.Compute_Inserted_Frames"/> option is used
        ///     then it does not contain either successive labels that denote the same bytecode offset (in this
        ///     case only the first label appears in this list).
        /// </summary>
        internal Label nextBasicBlock;

        /// <summary>
        ///     The next element in the list of labels to which this label belongs, or {@literal null} if it
        ///     does not belong to any list. All lists of labels must end with the <seealso cref = "EmptyList"/>
        ///     sentinel, in order to ensure that this field is null if and only if this label does not belong
        ///     to a list of labels. Note that there can be several lists of labels at the same time, but that
        ///     a label can belong to at most one list at a time (unless some lists share a common tail, but
        ///     this is not used in practice).
        ///     <para>
        ///         List of labels are used in <seealso cref = "MethodWriter.ComputeAllFrames"/> and {@link
        ///         MethodWriter#computeMaxStackAndLocal} to compute stack map frames and the maximum stack size,
        ///         respectively, as well as in <seealso cref = "MarkSubroutine"/> and <seealso cref = "AddSubroutineRetSuccessors"/>
        ///         to
        ///         compute the basic blocks belonging to subroutines and their outgoing edges. Outside of these
        ///         methods, this field should be null (this property is a precondition and a postcondition of
        ///         these methods).
        ///     </para>
        /// </summary>
        internal Label nextListElement;

        /// <summary>
        ///     The source line numbers corresponding to this label, in addition to <seealso cref = "_lineNumber"/>, or
        ///     null. The first element of this array is the number n of source line numbers it contains, which
        ///     are stored between indices 1 and n (inclusive).
        /// </summary>
        private int[] _otherLineNumbers;

        /// <summary>
        ///     The outgoing edges of the basic block corresponding to this label, in the control flow graph of
        ///     its method. These edges are stored in a linked list of <seealso cref = "Edge"/> objects, linked to each
        ///     other by their <seealso cref = "Edge.nextEdge"/> field.
        /// </summary>
        internal Edge outgoingEdges;

        /// <summary>
        ///     The maximum height reached by the output stack, relatively to the top of the input stack, in
        ///     the basic block corresponding to this label. This maximum is always positive or {@literal
        ///     null}.
        /// </summary>
        internal short outputStackMax;

        /// <summary>
        ///     The number of elements in the output stack, at the end of the basic block corresponding to this
        ///     label. This field is only computed for basic blocks that end with a RET instruction.
        /// </summary>
        internal short outputStackSize;

        /// <summary>
        ///     The id of the subroutine to which this basic block belongs, or 0. If the basic block belongs to
        ///     several subroutines, this is the id of the "oldest" subroutine that contains it (with the
        ///     convention that a subroutine calling another one is "older" than the callee). This field is
        ///     computed in <seealso cref = "MethodWriter.ComputeMaxStackAndLocal"/>, if the method contains JSR
        ///     instructions.
        /// </summary>
        internal short subroutineId;

        // -----------------------------------------------------------------------------------------------
        // Constructor and accessors
        // -----------------------------------------------------------------------------------------------
        /// <summary>
        ///     Returns the bytecode offset corresponding to this label. This offset is computed from the start
        ///     of the method's bytecode.
        ///     <i>
        ///         This method is intended for <seealso cref = "Attribute"/> sub classes, and is
        ///         normally not needed by class generators or adapters.
        ///     </i>
        /// </summary>
        /// <returns> the bytecode offset corresponding to this label. </returns>
        /// <exception cref = "IllegalStateException"> if this label is not resolved yet. </exception>
        public virtual int Offset
        {
            get
            {
                if ((flags & Flag_Resolved) == 0)
                    throw new InvalidOperationException("Label offset position has not been resolved yet");
                return bytecodeOffset;
            }
        }

        /// <summary>
        ///     Returns the "canonical" <seealso cref = "Label"/> instance corresponding to this label's bytecode offset,
        ///     if known, otherwise the label itself. The canonical instance is the first label (in the order
        ///     of their visit by <seealso cref = "MethodVisitor.VisitLabel"/>) corresponding to this bytecode offset. It
        ///     cannot be known for labels which have not been visited yet.
        ///     <para>
        ///         <i>
        ///             This method should only be used when the <seealso cref = "MethodWriter.Compute_All_Frames"/> option
        ///             is used.
        ///         </i>
        ///     </para>
        /// </summary>
        /// <returns>
        ///     the label itself if <seealso cref = "frame"/> is null, otherwise the Label's frame owner. This
        ///     corresponds to the "canonical" label instance described above thanks to the way the label
        ///     frame is set in <seealso cref = "MethodWriter.VisitLabel"/>.
        /// </returns>
        public Label CanonicalInstance => frame == null ? this : frame.owner;

        // -----------------------------------------------------------------------------------------------
        // Methods to manage line numbers
        // -----------------------------------------------------------------------------------------------
        /// <summary>
        ///     Adds a source line number corresponding to this label.
        /// </summary>
        /// <param name = "lineNumber"> a source line number (which should be strictly positive). </param>
        public void AddLineNumber(int lineNumber)
        {
            if (this._lineNumber == 0)
            {
                this._lineNumber = (short)lineNumber;
            }
            else
            {
                if (_otherLineNumbers == null)
                    _otherLineNumbers = new int[Line_Numbers_Capacity_Increment];
                var otherLineNumberIndex = ++_otherLineNumbers[0];
                if (otherLineNumberIndex >= _otherLineNumbers.Length)
                {
                    var newLineNumbers = new int[_otherLineNumbers.Length + Line_Numbers_Capacity_Increment];
                    Array.Copy(_otherLineNumbers, 0, newLineNumbers, 0, _otherLineNumbers.Length);
                    _otherLineNumbers = newLineNumbers;
                }

                _otherLineNumbers[otherLineNumberIndex] = lineNumber;
            }
        }

        /// <summary>
        ///     Makes the given visitor visit this label and its source line numbers, if applicable.
        /// </summary>
        /// <param name = "methodVisitor"> a method visitor. </param>
        /// <param name = "visitLineNumbers"> whether to visit of the label's source line numbers, if any. </param>
        public void Accept(MethodVisitor methodVisitor, bool visitLineNumbers)
        {
            methodVisitor.VisitLabel(this);
            if (visitLineNumbers && _lineNumber != 0)
            {
                methodVisitor.VisitLineNumber(_lineNumber & 0xFFFF, this);
                if (_otherLineNumbers != null)
                    for (var i = 1; i <= _otherLineNumbers[0]; ++i)
                        methodVisitor.VisitLineNumber(_otherLineNumbers[i], this);
            }
        }

        // -----------------------------------------------------------------------------------------------
        // Methods to compute offsets and to manage forward references
        // -----------------------------------------------------------------------------------------------
        /// <summary>
        ///     Puts a reference to this label in the bytecode of a method. If the bytecode offset of the label
        ///     is known, the relative bytecode offset between the label and the instruction referencing it is
        ///     computed and written directly. Otherwise, a null relative offset is written and a new forward
        ///     reference is declared for this label.
        /// </summary>
        /// <param name = "code"> the bytecode of the method. This is where the reference is appended. </param>
        /// <param name = "sourceInsnBytecodeOffset">
        ///     the bytecode offset of the instruction that contains the
        ///     reference to be appended.
        /// </param>
        /// <param name = "wideReference"> whether the reference must be stored in 4 bytes (instead of 2 bytes). </param>
        public void Put(ByteVector code, int sourceInsnBytecodeOffset, bool wideReference)
        {
            if ((flags & Flag_Resolved) == 0)
            {
                if (wideReference)
                {
                    AddForwardReference(sourceInsnBytecodeOffset, Forward_Reference_Type_Wide, code.length);
                    code.PutInt(-1);
                }
                else
                {
                    AddForwardReference(sourceInsnBytecodeOffset, Forward_Reference_Type_Short, code.length);
                    code.PutShort(-1);
                }
            }
            else
            {
                if (wideReference)
                    code.PutInt(bytecodeOffset - sourceInsnBytecodeOffset);
                else
                    code.PutShort(bytecodeOffset - sourceInsnBytecodeOffset);
            }
        }

        /// <summary>
        ///     Adds a forward reference to this label. This method must be called only for a true forward
        ///     reference, i.e. only if this label is not resolved yet. For backward references, the relative
        ///     bytecode offset of the reference can be, and must be, computed and stored directly.
        /// </summary>
        /// <param name = "sourceInsnBytecodeOffset">
        ///     the bytecode offset of the instruction that contains the
        ///     reference stored at referenceHandle.
        /// </param>
        /// <param name = "referenceType">
        ///     either <seealso cref = "Forward_Reference_Type_Short"/> or {@link
        ///     #FORWARD_REFERENCE_TYPE_WIDE}.
        /// </param>
        /// <param name = "referenceHandle">
        ///     the offset in the bytecode where the forward reference value must be
        ///     stored.
        /// </param>
        private void AddForwardReference(int sourceInsnBytecodeOffset, int referenceType, int referenceHandle)
        {
            if (_forwardReferences == null)
                _forwardReferences = new int[Forward_References_Capacity_Increment];
            var lastElementIndex = _forwardReferences[0];
            if (lastElementIndex + 2 >= _forwardReferences.Length)
            {
                var newValues = new int[_forwardReferences.Length + Forward_References_Capacity_Increment];
                Array.Copy(_forwardReferences, 0, newValues, 0, _forwardReferences.Length);
                _forwardReferences = newValues;
            }

            _forwardReferences[++lastElementIndex] = sourceInsnBytecodeOffset;
            _forwardReferences[++lastElementIndex] = referenceType | referenceHandle;
            _forwardReferences[0] = lastElementIndex;
        }

        /// <summary>
        ///     Sets the bytecode offset of this label to the given value and resolves the forward references
        ///     to this label, if any. This method must be called when this label is added to the bytecode of
        ///     the method, i.e. when its bytecode offset becomes known. This method fills in the blanks that
        ///     where left in the bytecode by each forward reference previously added to this label.
        /// </summary>
        /// <param name = "code"> the bytecode of the method. </param>
        /// <param name = "bytecodeOffset"> the bytecode offset of this label. </param>
        /// <returns>
        ///     {@literal true} if a blank that was left for this label was too small to store the
        ///     offset. In such a case the corresponding jump instruction is replaced with an equivalent
        ///     ASM specific instruction using an unsigned two bytes offset. These ASM specific
        ///     instructions are later replaced with standard bytecode instructions with wider offsets (4
        ///     bytes instead of 2), in ClassReader.
        /// </returns>
        public bool Resolve(byte[] code, int bytecodeOffset)
        {
            flags |= Flag_Resolved;
            this.bytecodeOffset = bytecodeOffset;
            if (_forwardReferences == null)
                return false;
            var hasAsmInstructions = false;
            for (var i = _forwardReferences[0]; i > 0; i -= 2)
            {
                var sourceInsnBytecodeOffset = _forwardReferences[i - 1];
                var reference = _forwardReferences[i];
                var relativeOffset = bytecodeOffset - sourceInsnBytecodeOffset;
                var handle = reference & Forward_Reference_Handle_Mask;
                if ((reference & Forward_Reference_Type_Mask) == Forward_Reference_Type_Short)
                {
                    if (relativeOffset < short.MinValue || relativeOffset > short.MaxValue)
                    {
                        // Change the opcode of the jump instruction, in order to be able to find it later in
                        // ClassReader. These ASM specific opcodes are similar to jump instruction opcodes, except
                        // that the 2 bytes offset is unsigned (and can therefore represent values from 0 to
                        // 65535, which is sufficient since the size of a method is limited to 65535 bytes).
                        var opcode = code[sourceInsnBytecodeOffset] & 0xFF;
                        if (opcode < IOpcodes.Ifnull)
                            // Change IFEQ ... JSR to ASM_IFEQ ... ASM_JSR.
                            code[sourceInsnBytecodeOffset] = (byte)(opcode + Constants.Asm_Opcode_Delta);
                        else
                            // Change IFNULL and IFNONNULL to ASM_IFNULL and ASM_IFNONNULL.
                            code[sourceInsnBytecodeOffset] = (byte)(opcode + Constants.Asm_Ifnull_Opcode_Delta);
                        hasAsmInstructions = true;
                    }

                    code[handle++] = (byte)(int)((uint)relativeOffset >> 8);
                    code[handle] = (byte)relativeOffset;
                }
                else
                {
                    code[handle++] = (byte)(int)((uint)relativeOffset >> 24);
                    code[handle++] = (byte)(int)((uint)relativeOffset >> 16);
                    code[handle++] = (byte)(int)((uint)relativeOffset >> 8);
                    code[handle] = (byte)relativeOffset;
                }
            }

            return hasAsmInstructions;
        }

        // -----------------------------------------------------------------------------------------------
        // Methods related to subroutines
        // -----------------------------------------------------------------------------------------------
        /// <summary>
        ///     Finds the basic blocks that belong to the subroutine starting with the basic block
        ///     corresponding to this label, and marks these blocks as belonging to this subroutine. This
        ///     method follows the control flow graph to find all the blocks that are reachable from the
        ///     current basic block WITHOUT following any jsr target.
        ///     <para>
        ///         Note: a precondition and postcondition of this method is that all labels must have a null
        ///         <seealso cref = "nextListElement"/>.
        ///     </para>
        /// </summary>
        /// <param name = "subroutineId">
        ///     the id of the subroutine starting with the basic block corresponding to
        ///     this label.
        /// </param>
        public void MarkSubroutine(short subroutineId)
        {
            // Data flow algorithm: put this basic block in a list of blocks to process (which are blocks
            // belonging to subroutine subroutineId) and, while there are blocks to process, remove one from
            // the list, mark it as belonging to the subroutine, and add its successor basic blocks in the
            // control flow graph to the list of blocks to process (if not already done).
            var listOfBlocksToProcess = this;
            listOfBlocksToProcess.nextListElement = EmptyList;
            while (listOfBlocksToProcess != EmptyList)
            {
                // Remove a basic block from the list of blocks to process.
                var basicBlock = listOfBlocksToProcess;
                listOfBlocksToProcess = listOfBlocksToProcess.nextListElement;
                basicBlock.nextListElement = null;
                // If it is not already marked as belonging to a subroutine, mark it as belonging to
                // subroutineId and add its successors to the list of blocks to process (unless already done).
                if (basicBlock.subroutineId == 0)
                {
                    basicBlock.subroutineId = subroutineId;
                    listOfBlocksToProcess = basicBlock.PushSuccessors(listOfBlocksToProcess);
                }
            }
        }

        /// <summary>
        ///     Finds the basic blocks that end a subroutine starting with the basic block corresponding to
        ///     this label and, for each one of them, adds an outgoing edge to the basic block following the
        ///     given subroutine call. In other words, completes the control flow graph by adding the edges
        ///     corresponding to the return from this subroutine, when called from the given caller basic
        ///     block.
        ///     <para>
        ///         Note: a precondition and postcondition of this method is that all labels must have a null
        ///         <seealso cref = "nextListElement"/>.
        ///     </para>
        /// </summary>
        /// <param name = "subroutineCaller">
        ///     a basic block that ends with a jsr to the basic block corresponding to
        ///     this label. This label is supposed to correspond to the start of a subroutine.
        /// </param>
        public void AddSubroutineRetSuccessors(Label subroutineCaller)
        {
            // Data flow algorithm: put this basic block in a list blocks to process (which are blocks
            // belonging to a subroutine starting with this label) and, while there are blocks to process,
            // remove one from the list, put it in a list of blocks that have been processed, add a return
            // edge to the successor of subroutineCaller if applicable, and add its successor basic blocks
            // in the control flow graph to the list of blocks to process (if not already done).
            var listOfProcessedBlocks = EmptyList;
            var listOfBlocksToProcess = this;
            listOfBlocksToProcess.nextListElement = EmptyList;
            while (listOfBlocksToProcess != EmptyList)
            {
                // Move a basic block from the list of blocks to process to the list of processed blocks.
                var basicBlock = listOfBlocksToProcess;
                listOfBlocksToProcess = basicBlock.nextListElement;
                basicBlock.nextListElement = listOfProcessedBlocks;
                listOfProcessedBlocks = basicBlock;
                // Add an edge from this block to the successor of the caller basic block, if this block is
                // the end of a subroutine and if this block and subroutineCaller do not belong to the same
                // subroutine.
                if ((basicBlock.flags & Flag_Subroutine_End) != 0 &&
                    basicBlock.subroutineId != subroutineCaller.subroutineId)
                    basicBlock.outgoingEdges = new Edge(basicBlock.outputStackSize,
                        subroutineCaller.outgoingEdges.successor, basicBlock.outgoingEdges);
                // Add its successors to the list of blocks to process. Note that {@link #pushSuccessors} does
                // not push basic blocks which are already in a list. Here this means either in the list of
                // blocks to process, or in the list of already processed blocks. This second list is
                // important to make sure we don't reprocess an already processed block.
                listOfBlocksToProcess = basicBlock.PushSuccessors(listOfBlocksToProcess);
            }

            // Reset the {@link #nextListElement} of all the basic blocks that have been processed to null,
            // so that this method can be called again with a different subroutine or subroutine caller.
            while (listOfProcessedBlocks != EmptyList)
            {
                var newListOfProcessedBlocks = listOfProcessedBlocks.nextListElement;
                listOfProcessedBlocks.nextListElement = null;
                listOfProcessedBlocks = newListOfProcessedBlocks;
            }
        }

        /// <summary>
        ///     Adds the successors of this label in the method's control flow graph (except those
        ///     corresponding to a jsr target, and those already in a list of labels) to the given list of
        ///     blocks to process, and returns the new list.
        /// </summary>
        /// <param name = "listOfLabelsToProcess">
        ///     a list of basic blocks to process, linked together with their
        ///     <seealso cref = "nextListElement"/> field.
        /// </param>
        /// <returns> the new list of blocks to process. </returns>
        private Label PushSuccessors(Label listOfLabelsToProcess)
        {
            var newListOfLabelsToProcess = listOfLabelsToProcess;
            var outgoingEdge = outgoingEdges;
            while (outgoingEdge != null)
            {
                // By construction, the second outgoing edge of a basic block that ends with a jsr instruction
                // leads to the jsr target (see {@link #FLAG_SUBROUTINE_CALLER}).
                var isJsrTarget = (flags & Flag_Subroutine_Caller) != 0 && outgoingEdge == outgoingEdges.nextEdge;
                if (!isJsrTarget && outgoingEdge.successor.nextListElement == null)
                {
                    // Add this successor to the list of blocks to process, if it does not already belong to a
                    // list of labels.
                    outgoingEdge.successor.nextListElement = newListOfLabelsToProcess;
                    newListOfLabelsToProcess = outgoingEdge.successor;
                }

                outgoingEdge = outgoingEdge.nextEdge;
            }

            return newListOfLabelsToProcess;
        }

        // -----------------------------------------------------------------------------------------------
        // Overridden Object methods
        // -----------------------------------------------------------------------------------------------
        /// <summary>
        ///     Returns a string representation of this label.
        /// </summary>
        /// <returns> a string representation of this label. </returns>
        public override string ToString()
        {
            return "L" + this.GetHashCode();
        }
    }
}