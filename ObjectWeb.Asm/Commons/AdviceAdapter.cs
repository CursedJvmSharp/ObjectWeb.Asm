using System;
using System.Collections.Generic;

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
namespace ObjectWeb.Asm.Commons
{
    /// <summary>
    ///     A <seealso cref="MethodVisitor" /> to insert before, after and around advices in methods and constructors.
    ///     For constructors, the code keeps track of the elements on the stack in order to detect when the
    ///     super class constructor is called (note that there can be multiple such calls in different
    ///     branches). {@code onMethodEnter} is called after each super class constructor call, because the
    ///     object cannot be used before it is properly initialized.
    ///     @author Eugene Kuleshov
    ///     @author Eric Bruneton
    /// </summary>
    public abstract class AdviceAdapter : GeneratorAdapter, IOpcodes
    {
        /// <summary>
        ///     Prefix of the error message when invalid opcodes are found.
        /// </summary>
        private const string InvalidOpcode = "Invalid opcode ";

        /// <summary>
        ///     The "uninitialized this" value.
        /// </summary>
        private static readonly object UNINITIALIZED_THIS = new();

        /// <summary>
        ///     Any value other than "uninitialized this".
        /// </summary>
        private static readonly object OTHER = new();

        /// <summary>
        ///     Whether the visited method is a constructor.
        /// </summary>
        private readonly bool _isConstructor;

        /// <summary>
        ///     The stack map frames corresponding to the labels of the forward jumps made *before* the super
        ///     class constructor has been called (note that the Java Virtual Machine forbids backward jumps
        ///     before the super class constructor is called). Note that by definition (cf. the 'before'), when
        ///     we reach a label from this map, <seealso cref="_superClassConstructorCalled" /> must be reset to false.
        ///     This field is only maintained for constructors.
        /// </summary>
        private IDictionary<Label, List<object>> _forwardJumpStackFrames;

        /// <summary>
        ///     The Values on the current execution stack frame (long and double are represented by two
        ///     elements). Each value is either <seealso cref="UNINITIALIZED_THIS" /> (for the uninitialized this value),
        ///     or <seealso cref="OTHER" /> (for any other value). This field is only maintained for constructors, in
        ///     branches where the super class constructor has not been called yet.
        /// </summary>
        private List<object> _stackFrame;

        /// <summary>
        ///     Whether the super class constructor has been called (if the visited method is a constructor),
        ///     at the current instruction. There can be multiple call sites to the super constructor (e.g. for
        ///     Java code such as {@code super(expr ? value1 : value2);}), in different branches. When scanning
        ///     the bytecode linearly, we can move from one branch where the super constructor has been called
        ///     to another where it has not been called yet. Therefore, this value can change from false to
        ///     true, and vice-versa.
        /// </summary>
        private bool _superClassConstructorCalled;

        /// <summary>
        ///     The access flags of the visited method.
        /// </summary>
        protected internal int methodAccess;

        /// <summary>
        ///     The descriptor of the visited method.
        /// </summary>
        protected internal string methodDesc;

        /// <summary>
        ///     Constructs a new <seealso cref="AdviceAdapter" />.
        /// </summary>
        /// <param name="api">
        ///     the ASM API version implemented by this visitor. Must be one of the {@code
        ///     ASM}<i>x</i> Values in <seealso cref="IOpcodes" />.
        /// </param>
        /// <param name="methodVisitor"> the method visitor to which this adapter delegates calls. </param>
        /// <param name="access"> the method's access flags (see <seealso cref="IOpcodes" />). </param>
        /// <param name="name"> the method's name. </param>
        /// <param name="descriptor"> the method's descriptor (see <seealso cref="Type" />). </param>
        public AdviceAdapter(int api, MethodVisitor methodVisitor, int access, string name, string descriptor) : base(
            api, methodVisitor, access, name, descriptor)
        {
            methodAccess = access;
            methodDesc = descriptor;
            _isConstructor = "<init>".Equals(name);
        }

        public override void VisitCode()
        {
            base.VisitCode();
            if (_isConstructor)
            {
                _stackFrame = new List<object>();
                _forwardJumpStackFrames = new Dictionary<Label, List<object>>();
            }
            else
            {
                OnMethodEnter();
            }
        }

        public override void VisitLabel(Label label)
        {
            base.VisitLabel(label);
            if (_isConstructor && _forwardJumpStackFrames != null)
            {
                if (_forwardJumpStackFrames.TryGetValue(label, out var labelStackFrame))
                {
                    _stackFrame = labelStackFrame;
                    _superClassConstructorCalled = false;
                    _forwardJumpStackFrames.Remove(label);
                }
            }
        }

        public override void VisitInsn(int opcode)
        {
            if (_isConstructor && !_superClassConstructorCalled)
            {
                int stackSize;
                switch (opcode)
                {
                    case IOpcodes.Ireturn:
                    case IOpcodes.Freturn:
                    case IOpcodes.Areturn:
                    case IOpcodes.Lreturn:
                    case IOpcodes.Dreturn:
                        throw new ArgumentException("Invalid return in constructor");
                    case IOpcodes.Return: // empty stack
                        OnMethodExit(opcode);
                        EndConstructorBasicBlockWithoutSuccessor();
                        break;
                    case IOpcodes.Athrow: // 1 before n/a after
                        PopValue();
                        OnMethodExit(opcode);
                        EndConstructorBasicBlockWithoutSuccessor();
                        break;
                    case IOpcodes.Nop:
                    case IOpcodes.Laload: // remove 2 add 2
                    case IOpcodes.Daload: // remove 2 add 2
                    case IOpcodes.Lneg:
                    case IOpcodes.Dneg:
                    case IOpcodes.Fneg:
                    case IOpcodes.Ineg:
                    case IOpcodes.L2D:
                    case IOpcodes.D2L:
                    case IOpcodes.F2I:
                    case IOpcodes.I2B:
                    case IOpcodes.I2C:
                    case IOpcodes.I2S:
                    case IOpcodes.I2F:
                    case IOpcodes.Arraylength:
                        break;
                    case IOpcodes.Aconst_Null:
                    case IOpcodes.Iconst_M1:
                    case IOpcodes.Iconst_0:
                    case IOpcodes.Iconst_1:
                    case IOpcodes.Iconst_2:
                    case IOpcodes.Iconst_3:
                    case IOpcodes.Iconst_4:
                    case IOpcodes.Iconst_5:
                    case IOpcodes.Fconst_0:
                    case IOpcodes.Fconst_1:
                    case IOpcodes.Fconst_2:
                    case IOpcodes.F2L: // 1 before 2 after
                    case IOpcodes.F2D:
                    case IOpcodes.I2L:
                    case IOpcodes.I2D:
                        PushValue(OTHER);
                        break;
                    case IOpcodes.Lconst_0:
                    case IOpcodes.Lconst_1:
                    case IOpcodes.Dconst_0:
                    case IOpcodes.Dconst_1:
                        PushValue(OTHER);
                        PushValue(OTHER);
                        break;
                    case IOpcodes.Iaload: // remove 2 add 1
                    case IOpcodes.Faload: // remove 2 add 1
                    case IOpcodes.Aaload: // remove 2 add 1
                    case IOpcodes.Baload: // remove 2 add 1
                    case IOpcodes.Caload: // remove 2 add 1
                    case IOpcodes.Saload: // remove 2 add 1
                    case IOpcodes.Pop:
                    case IOpcodes.Iadd:
                    case IOpcodes.Fadd:
                    case IOpcodes.Isub:
                    case IOpcodes.Lshl: // 3 before 2 after
                    case IOpcodes.Lshr: // 3 before 2 after
                    case IOpcodes.Lushr: // 3 before 2 after
                    case IOpcodes.L2I: // 2 before 1 after
                    case IOpcodes.L2F: // 2 before 1 after
                    case IOpcodes.D2I: // 2 before 1 after
                    case IOpcodes.D2F: // 2 before 1 after
                    case IOpcodes.Fsub:
                    case IOpcodes.Fmul:
                    case IOpcodes.Fdiv:
                    case IOpcodes.Frem:
                    case IOpcodes.Fcmpl: // 2 before 1 after
                    case IOpcodes.Fcmpg: // 2 before 1 after
                    case IOpcodes.Imul:
                    case IOpcodes.Idiv:
                    case IOpcodes.Irem:
                    case IOpcodes.Ishl:
                    case IOpcodes.Ishr:
                    case IOpcodes.Iushr:
                    case IOpcodes.Iand:
                    case IOpcodes.Ior:
                    case IOpcodes.Ixor:
                    case IOpcodes.Monitorenter:
                    case IOpcodes.Monitorexit:
                        PopValue();
                        break;
                    case IOpcodes.Pop2:
                    case IOpcodes.Lsub:
                    case IOpcodes.Lmul:
                    case IOpcodes.Ldiv:
                    case IOpcodes.Lrem:
                    case IOpcodes.Ladd:
                    case IOpcodes.Land:
                    case IOpcodes.Lor:
                    case IOpcodes.Lxor:
                    case IOpcodes.Dadd:
                    case IOpcodes.Dmul:
                    case IOpcodes.Dsub:
                    case IOpcodes.Ddiv:
                    case IOpcodes.Drem:
                        PopValue();
                        PopValue();
                        break;
                    case IOpcodes.Iastore:
                    case IOpcodes.Fastore:
                    case IOpcodes.Aastore:
                    case IOpcodes.Bastore:
                    case IOpcodes.Castore:
                    case IOpcodes.Sastore:
                    case IOpcodes.Lcmp: // 4 before 1 after
                    case IOpcodes.Dcmpl:
                    case IOpcodes.Dcmpg:
                        PopValue();
                        PopValue();
                        PopValue();
                        break;
                    case IOpcodes.Lastore:
                    case IOpcodes.Dastore:
                        PopValue();
                        PopValue();
                        PopValue();
                        PopValue();
                        break;
                    case IOpcodes.Dup:
                        PushValue(PeekValue());
                        break;
                    case IOpcodes.Dup_X1:
                        stackSize = _stackFrame.Count;
                        _stackFrame.Insert(stackSize - 2, _stackFrame[stackSize - 1]);
                        break;
                    case IOpcodes.Dup_X2:
                        stackSize = _stackFrame.Count;
                        _stackFrame.Insert(stackSize - 3, _stackFrame[stackSize - 1]);
                        break;
                    case IOpcodes.Dup2:
                        stackSize = _stackFrame.Count;
                        _stackFrame.Insert(stackSize - 2, _stackFrame[stackSize - 1]);
                        _stackFrame.Insert(stackSize - 2, _stackFrame[stackSize - 1]);
                        break;
                    case IOpcodes.Dup2_X1:
                        stackSize = _stackFrame.Count;
                        _stackFrame.Insert(stackSize - 3, _stackFrame[stackSize - 1]);
                        _stackFrame.Insert(stackSize - 3, _stackFrame[stackSize - 1]);
                        break;
                    case IOpcodes.Dup2_X2:
                        stackSize = _stackFrame.Count;
                        _stackFrame.Insert(stackSize - 4, _stackFrame[stackSize - 1]);
                        _stackFrame.Insert(stackSize - 4, _stackFrame[stackSize - 1]);
                        break;
                    case IOpcodes.Swap:
                        stackSize = _stackFrame.Count;
                        _stackFrame.Insert(stackSize - 2, _stackFrame[stackSize - 1]);
                        _stackFrame.RemoveAt(stackSize);
                        break;
                    default:
                        throw new ArgumentException(InvalidOpcode + opcode);
                }
            }
            else
            {
                switch (opcode)
                {
                    case IOpcodes.Return:
                    case IOpcodes.Ireturn:
                    case IOpcodes.Freturn:
                    case IOpcodes.Areturn:
                    case IOpcodes.Lreturn:
                    case IOpcodes.Dreturn:
                    case IOpcodes.Athrow:
                        OnMethodExit(opcode);
                        break;
                }
            }

            base.VisitInsn(opcode);
        }

        public override void VisitVarInsn(int opcode, int varIndex)
        {
            base.VisitVarInsn(opcode, varIndex);
            if (_isConstructor && !_superClassConstructorCalled)
                switch (opcode)
                {
                    case IOpcodes.Iload:
                    case IOpcodes.Fload:
                        PushValue(OTHER);
                        break;
                    case IOpcodes.Lload:
                    case IOpcodes.Dload:
                        PushValue(OTHER);
                        PushValue(OTHER);
                        break;
                    case IOpcodes.Aload:
                        PushValue(varIndex == 0 ? UNINITIALIZED_THIS : OTHER);
                        break;
                    case IOpcodes.Astore:
                    case IOpcodes.Istore:
                    case IOpcodes.Fstore:
                        PopValue();
                        break;
                    case IOpcodes.Lstore:
                    case IOpcodes.Dstore:
                        PopValue();
                        PopValue();
                        break;
                    case IOpcodes.Ret:
                        EndConstructorBasicBlockWithoutSuccessor();
                        break;
                    default:
                        throw new ArgumentException(InvalidOpcode + opcode);
                }
        }

        public override void VisitFieldInsn(int opcode, string owner, string name, string descriptor)
        {
            base.VisitFieldInsn(opcode, owner, name, descriptor);
            if (_isConstructor && !_superClassConstructorCalled)
            {
                var firstDescriptorChar = descriptor[0];
                var longOrDouble = firstDescriptorChar == 'J' || firstDescriptorChar == 'D';
                switch (opcode)
                {
                    case IOpcodes.Getstatic:
                        PushValue(OTHER);
                        if (longOrDouble) PushValue(OTHER);
                        break;
                    case IOpcodes.Putstatic:
                        PopValue();
                        if (longOrDouble) PopValue();
                        break;
                    case IOpcodes.Putfield:
                        PopValue();
                        PopValue();
                        if (longOrDouble) PopValue();
                        break;
                    case IOpcodes.Getfield:
                        if (longOrDouble) PushValue(OTHER);
                        break;
                    default:
                        throw new ArgumentException(InvalidOpcode + opcode);
                }
            }
        }

        public override void VisitIntInsn(int opcode, int operand)
        {
            base.VisitIntInsn(opcode, operand);
            if (_isConstructor && !_superClassConstructorCalled && opcode != IOpcodes.Newarray) PushValue(OTHER);
        }

        public override void VisitLdcInsn(object value)
        {
            base.VisitLdcInsn(value);
            if (_isConstructor && !_superClassConstructorCalled)
            {
                PushValue(OTHER);
                if (value is double? || value is long? ||
                    value is ConstantDynamic && ((ConstantDynamic)value).Size == 2) PushValue(OTHER);
            }
        }

        public override void VisitMultiANewArrayInsn(string descriptor, int numDimensions)
        {
            base.VisitMultiANewArrayInsn(descriptor, numDimensions);
            if (_isConstructor && !_superClassConstructorCalled)
            {
                for (var i = 0; i < numDimensions; i++) PopValue();
                PushValue(OTHER);
            }
        }

        public override void VisitTypeInsn(int opcode, string type)
        {
            base.VisitTypeInsn(opcode, type);
            // ANEWARRAY, CHECKCAST or INSTANCEOF don't change stack.
            if (_isConstructor && !_superClassConstructorCalled && opcode == IOpcodes.New) PushValue(OTHER);
        }

        public override void VisitMethodInsn(int opcodeAndSource, string owner, string name, string descriptor,
            bool isInterface)
        {
            if (api < IOpcodes.Asm5 && (opcodeAndSource & IOpcodes.Source_Deprecated) == 0)
            {
                // Redirect the call to the deprecated version of this method.
                base.VisitMethodInsn(opcodeAndSource, owner, name, descriptor, isInterface);
                return;
            }

            base.VisitMethodInsn(opcodeAndSource, owner, name, descriptor, isInterface);
            var opcode = opcodeAndSource & ~IOpcodes.Source_Mask;

            DoVisitMethodInsn(opcode, name, descriptor);
        }

        private void DoVisitMethodInsn(int opcode, string name, string descriptor)
        {
            if (_isConstructor && !_superClassConstructorCalled)
            {
                foreach (var argumentType in JType.GetArgumentTypes(descriptor))
                {
                    PopValue();
                    if (argumentType.Size == 2) PopValue();
                }

                switch (opcode)
                {
                    case IOpcodes.Invokeinterface:
                    case IOpcodes.Invokevirtual:
                        PopValue();
                        break;
                    case IOpcodes.Invokespecial:
                        var value = PopValue();
                        if (value == UNINITIALIZED_THIS && !_superClassConstructorCalled && name.Equals("<init>"))
                        {
                            _superClassConstructorCalled = true;
                            OnMethodEnter();
                        }

                        break;
                }

                var returnType = JType.GetReturnType(descriptor);
                if (returnType != JType.VoidType)
                {
                    PushValue(OTHER);
                    if (returnType.Size == 2) PushValue(OTHER);
                }
            }
        }

        public override void VisitInvokeDynamicInsn(string name, string descriptor, Handle bootstrapMethodHandle,
            params object[] bootstrapMethodArguments)
        {
            base.VisitInvokeDynamicInsn(name, descriptor, bootstrapMethodHandle, bootstrapMethodArguments);
            DoVisitMethodInsn(IOpcodes.Invokedynamic, name, descriptor);
        }

        public override void VisitJumpInsn(int opcode, Label label)
        {
            base.VisitJumpInsn(opcode, label);
            if (_isConstructor && !_superClassConstructorCalled)
            {
                switch (opcode)
                {
                    case IOpcodes.Ifeq:
                    case IOpcodes.Ifne:
                    case IOpcodes.Iflt:
                    case IOpcodes.Ifge:
                    case IOpcodes.Ifgt:
                    case IOpcodes.Ifle:
                    case IOpcodes.Ifnull:
                    case IOpcodes.Ifnonnull:
                        PopValue();
                        break;
                    case IOpcodes.If_Icmpeq:
                    case IOpcodes.If_Icmpne:
                    case IOpcodes.If_Icmplt:
                    case IOpcodes.If_Icmpge:
                    case IOpcodes.If_Icmpgt:
                    case IOpcodes.If_Icmple:
                    case IOpcodes.If_Acmpeq:
                    case IOpcodes.If_Acmpne:
                        PopValue();
                        PopValue();
                        break;
                    case IOpcodes.Jsr:
                        PushValue(OTHER);
                        break;
                    case IOpcodes.Goto:
                        EndConstructorBasicBlockWithoutSuccessor();
                        break;
                }

                AddForwardJump(label);
            }
        }

        public override void VisitLookupSwitchInsn(Label dflt, int[] keys, Label[] labels)
        {
            base.VisitLookupSwitchInsn(dflt, keys, labels);
            if (_isConstructor && !_superClassConstructorCalled)
            {
                PopValue();
                AddForwardJumps(dflt, labels);
                EndConstructorBasicBlockWithoutSuccessor();
            }
        }

        public override void VisitTableSwitchInsn(int min, int max, Label dflt, params Label[] labels)
        {
            base.VisitTableSwitchInsn(min, max, dflt, labels);
            if (_isConstructor && !_superClassConstructorCalled)
            {
                PopValue();
                AddForwardJumps(dflt, labels);
                EndConstructorBasicBlockWithoutSuccessor();
            }
        }

        public override void VisitTryCatchBlock(Label start, Label end, Label handler, string type)
        {
            base.VisitTryCatchBlock(start, end, handler, type);
            // By definition of 'forwardJumpStackFrames', 'handler' should be pushed only if there is an
            // instruction between 'start' and 'end' at which the super class constructor is not yet
            // called. Unfortunately, try catch blocks must be visited before their labels, so we have no
            // way to know this at this point. Instead, we suppose that the super class constructor has not
            // been called at the start of *any* exception handler. If this is wrong, normally there should
            // not be a second super class constructor call in the exception handler (an object can't be
            // initialized twice), so this is not issue (in the sense that there is no risk to emit a wrong
            // 'onMethodEnter').
            if (_isConstructor && !_forwardJumpStackFrames.ContainsKey(handler))
            {
                var handlerStackFrame = new List<object>();
                handlerStackFrame.Add(OTHER);
                _forwardJumpStackFrames[handler] = handlerStackFrame;
            }
        }

        private void AddForwardJumps(Label dflt, Label[] labels)
        {
            AddForwardJump(dflt);
            foreach (var label in labels) AddForwardJump(label);
        }

        private void AddForwardJump(Label label)
        {
            if (_forwardJumpStackFrames.ContainsKey(label)) return;
            _forwardJumpStackFrames[label] = new List<object>(_stackFrame);
        }

        private void EndConstructorBasicBlockWithoutSuccessor()
        {
            // The next instruction is not reachable from this instruction. If it is dead code, we
            // should not try to simulate stack operations, and there is no need to insert advices
            // here. If it is reachable with a backward jump, the only possible case Opcodes.is that the super
            // class constructor has already been called (backward jumps are forbidden before it is
            // called). If it is reachable with a forward jump, there are two sub-cases. Either the
            // super class constructor has already been called when reaching the next instruction, or
            // it has not been called. But in this case Opcodes.there must be a forwardJumpStackFrames entry
            // for a Label designating the next instruction, and superClassConstructorCalled will be
            // reset to false there. We can therefore always reset this field to true here.
            _superClassConstructorCalled = true;
        }

        private object PopValue()
        {
            var index = _stackFrame.Count - 1;
            var oldValue = _stackFrame[index];
            _stackFrame.RemoveAt(index);
            return oldValue;
        }

        private object PeekValue()
        {
            return _stackFrame[_stackFrame.Count - 1];
        }

        private void PushValue(object value)
        {
            _stackFrame.Add(value);
        }

        /// <summary>
        ///     Generates the "before" advice for the visited method. The default implementation of this method
        ///     does nothing. Subclasses can use or change all the local variables, but should not change state
        ///     of the stack. This method is called at the beginning of the method or after super class
        ///     constructor has been called (in constructors).
        /// </summary>
        public virtual void OnMethodEnter()
        {
        }

        /// <summary>
        ///     Generates the "after" advice for the visited method. The default implementation of this method
        ///     does nothing. Subclasses can use or change all the local variables, but should not change state
        ///     of the stack. This method is called at the end of the method, just before return and athrow
        ///     instructions. The top element on the stack contains the return value or the exception instance.
        ///     For example:
        ///     <pre>
        ///         public void onMethodExit(final int opcode) {
        ///         if (opcode == RETURN) {
        ///         visitInsn(ACONST_NULL);
        ///         } else if (opcode == ARETURN || opcode == ATHROW) {
        ///         dup();
        ///         } else {
        ///         if (opcode == LRETURN || opcode == DRETURN) {
        ///         dup2();
        ///         } else {
        ///         dup();
        ///         }
        ///         box(Type.getReturnType(this.methodDesc));
        ///         }
        ///         visitIntInsn(SIPUSH, opcode);
        ///         visitMethodInsn(INVOKESTATIC, owner, "onExit", "(Ljava/lang/Object;I)V");
        ///         }
        ///         // An actual call back method.
        ///         public static void onExit(final Object exitValue, final int opcode) {
        ///         ...
        ///         }
        ///     </pre>
        /// </summary>
        /// <param name="opcode">
        ///     one of <seealso cref="IIOpcodes.Return />, <seealso cref="IIOpcodes.Ireturn />, 
        ///     <seealso cref="IIOpcodes.Freturn />,
        ///     <seealso cref="IIOpcodes.Areturn />, 
        ///     <seealso cref="IIOpcodes.Lreturn />, <seealso cref="IIOpcodes.Dreturn /> or
        ///     {@link
        ///     Opcodes#ATHROW}.
        /// 
        /// </param>
        public virtual void OnMethodExit(int opcode)
        {
        }
    }
}