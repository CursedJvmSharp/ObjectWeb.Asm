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
    ///     A <seealso cref = "MethodVisitor"/> that keeps track of stack map frame changes between {@link
    ///     #visitFrame(int, int, Object[], int, Object[])} calls. This adapter must be used with the {@link
    ///     org.objectweb.asm.ClassReader#EXPAND_FRAMES} option. Each visit<i>X</i> instruction delegates to
    ///     the next visitor in the chain, if any, and then simulates the effect of this instruction on the
    ///     stack map frame, represented by <seealso cref = "locals"/> and <seealso cref = "stack"/>. The next visitor in the
    ///     chain
    ///     can get the state of the stack map frame <i>before</i> each instruction by reading the value of
    ///     these fields in its visit<i>X</i> methods (this requires a reference to the AnalyzerAdapter that
    ///     is before it in the chain). If this adapter is used with a class that does not contain stack map
    ///     table attributes (i.e., pre Java 6 classes) then this adapter may not be able to compute the
    ///     stack map frame for each instruction. In this case no exception is thrown but the <seealso cref = "locals"/>
    ///     and <seealso cref = "stack"/> fields will be null for these instructions.
    ///     @author Eric Bruneton
    /// </summary>
    public class AnalyzerAdapter : MethodVisitor
    {
        /// <summary>
        ///     The labels that designate the next instruction to be visited. May be {@literal null}.
        /// </summary>
        private List<Label> _labels;
        /// <summary>
        ///     The local variable slots for the current execution frame. Primitive types are represented by
        ///     <seealso cref = "IIOpcodes.top / > , <seealso cref = "IIOpcodes.integer / > , <seealso cref = "IIOpcodes. float  / > , 
        ///     <seealso cref = "IIOpcodes. long  / > , 
        ///     <seealso cref = "IIOpcodes. double  / > ,  <seealso cref = "IIOpcodes. null  / > or  <seealso cref = "IIOpcodes.uninitializedThis / > ///( long  and  ///double  are  represented  by  two  elements ,  the  second  one  being  TOP ) . Reference  types  are  ///represented  by  String  objects ( representing  internal  names ) ,  and  uninitialized  types  by  Label  ///objects ( this  label  designates  the  NEW  instruction  that  created  this  uninitialized  value ) . This  ///field  is  { @literal  null } for  unreachable  instructions .
        /// </summary>
        public List<object> Locals { get; set; }

        /// <summary>
        ///     The maximum number of local variables of this method.
        /// </summary>
        private int _maxLocals;
        /// <summary>
        ///     The maximum stack size of this method.
        /// </summary>
        private int _maxStack;
        /// <summary>
        ///     The owner's class name.
        /// </summary>
        private readonly string _owner;
        /// <summary>
        ///     The operand stack slots for the current execution frame. Primitive types are represented by
        ///     <seealso cref = "IIOpcodes.top / > , <seealso cref = "IIOpcodes.integer / > , <seealso cref = "IIOpcodes. float  / > , 
        ///     <seealso cref = "IIOpcodes. long  / > , 
        ///     <seealso cref = "IIOpcodes. double  / > ,  <seealso cref = "IIOpcodes. null  / > or  <seealso cref = "IIOpcodes.uninitializedThis / > ///( long  and  ///double  are  represented  by  two  elements ,  the  second  one  being  TOP ) . Reference  types  are  ///represented  by  String  objects ( representing  internal  names ) ,  and  uninitialized  types  by  Label  ///objects ( this  label  designates  the  NEW  instruction  that  created  this  uninitialized  value ) . This  ///field  is  { @literal  null } for  unreachable  instructions .
        /// </summary>
        public List<object> Stack { get; set; }

        /// <summary>
        ///     The uninitialized types in the current execution frame. This map associates internal names to
        ///     Label objects. Each label designates a NEW instruction that created the currently uninitialized
        ///     types, and the associated internal name represents the NEW operand, i.e. the final, initialized
        ///     type value.
        /// </summary>
        public IDictionary<object, object> UninitializedTypes { get; set; }

        /// <summary>
        ///     Constructs a new <seealso cref = "AnalyzerAdapter"/>. <i>Subclasses must not use this constructor</i>.
        ///     Instead, they must use the {@link #AnalyzerAdapter(int, String, int, String, String,
        ///     MethodVisitor)} version.
        /// </summary>
        /// <param name = "owner"> the owner's class name. </param>
        /// <param name = "access"> the method's access flags (see <seealso cref = "IOpcodes"/>). </param>
        /// <param name = "name"> the method's name. </param>
        /// <param name = "descriptor"> the method's descriptor (see <seealso cref = "Type"/>). </param>
        /// <param name = "methodVisitor">
        ///     the method visitor to which this adapter delegates calls. May be {@literal
        ///     null}.
        /// </param>
        /// <exception cref = "IllegalStateException"> If a subclass calls this constructor. </exception>
        public AnalyzerAdapter(string owner, int access, string name, string descriptor, MethodVisitor methodVisitor): this(IOpcodes.Asm9, owner, access, name, descriptor, methodVisitor)
        {
            if (GetType() != typeof(AnalyzerAdapter))
                throw new InvalidOperationException();
        }

        /// <summary>
        ///     Constructs a new <seealso cref = "AnalyzerAdapter"/>.
        /// </summary>
        /// <param name = "api">
        ///     the ASM API version implemented by this visitor. Must be one of the {@code
        ///     ASM}<i>x</i> Values in <seealso cref = "IOpcodes"/>.
        /// </param>
        /// <param name = "owner"> the owner's class name. </param>
        /// <param name = "access"> the method's access flags (see <seealso cref = "IOpcodes"/>). </param>
        /// <param name = "name"> the method's name. </param>
        /// <param name = "descriptor"> the method's descriptor (see <seealso cref = "Type"/>). </param>
        /// <param name = "methodVisitor">
        ///     the method visitor to which this adapter delegates calls. May be {@literal
        ///     null}.
        /// </param>
        public AnalyzerAdapter(int api, string owner, int access, string name, string descriptor, MethodVisitor methodVisitor): base(api, methodVisitor)
        {
            this._owner = owner;
            Locals = new List<object>();
            Stack = new List<object>();
            UninitializedTypes = new Dictionary<object, object>();
            if ((access & IOpcodes.Acc_Static) == 0)
            {
                if ("<init>".Equals(name))
                    Locals.Add(IOpcodes.uninitializedThis);
                else
                    Locals.Add(owner);
            }

            foreach (var argumentType in JType.GetArgumentTypes(descriptor))
                switch (argumentType.Sort)
                {
                    case JType.Boolean:
                    case JType.Char:
                    case JType.Byte:
                    case JType.Short:
                    case JType.Int:
                        Locals.Add(IOpcodes.integer);
                        break;
                    case JType.Float:
                        Locals.Add(IOpcodes.@float);
                        break;
                    case JType.Long:
                        Locals.Add(IOpcodes.@long);
                        Locals.Add(IOpcodes.top);
                        break;
                    case JType.Double:
                        Locals.Add(IOpcodes.@double);
                        Locals.Add(IOpcodes.top);
                        break;
                    case JType.Array:
                        Locals.Add(argumentType.Descriptor);
                        break;
                    case JType.Object:
                        Locals.Add(argumentType.InternalName);
                        break;
                    default:
                        throw new Exception();
                }

            _maxLocals = Locals.Count;
        }

        public override void VisitFrame(int type, int numLocal, object[] local, int numStack, object[] stack)
        {
            if (type != IOpcodes.F_New)
                // Uncompressed frame.
                throw new ArgumentException("AnalyzerAdapter only accepts expanded frames (see ClassReader.EXPAND_FRAMES)");
            base.VisitFrame(type, numLocal, local, numStack, stack);
            if (Locals != null)
            {
                Locals.Clear();
                this.Stack.Clear();
            }
            else
            {
                Locals = new List<object>();
                this.Stack = new List<object>();
            }

            VisitFrameTypes(numLocal, local, Locals);
            VisitFrameTypes(numStack, stack, this.Stack);
            _maxLocals = Math.Max(_maxLocals, Locals.Count);
            _maxStack = Math.Max(_maxStack, this.Stack.Count);
        }

        private static void VisitFrameTypes(int numTypes, object[] frameTypes, List<object> result)
        {
            for (var i = 0; i < numTypes; ++i)
            {
                var frameType = frameTypes[i];
                result.Add(frameType);
                if (Equals(frameType, IOpcodes.@long) || Equals(frameType, IOpcodes.@double))
                    result.Add(IOpcodes.top);
            }
        }

        public override void VisitInsn(int opcode)
        {
            base.VisitInsn(opcode);
            Execute(opcode, 0, null);
            if (opcode >= IOpcodes.Ireturn && opcode <= IOpcodes.Return || opcode == IOpcodes.Athrow)
            {
                Locals = null;
                Stack = null;
            }
        }

        public override void VisitIntInsn(int opcode, int operand)
        {
            base.VisitIntInsn(opcode, operand);
            Execute(opcode, operand, null);
        }

        public override void VisitVarInsn(int opcode, int varIndex)
        {
            base.VisitVarInsn(opcode, varIndex);
            var isLongOrDouble = opcode == IOpcodes.Lload || opcode == IOpcodes.Dload || opcode == IOpcodes.Lstore || opcode == IOpcodes.Dstore;
            _maxLocals = Math.Max(_maxLocals, varIndex + (isLongOrDouble ? 2 : 1));
            Execute(opcode, varIndex, null);
        }

        public override void VisitTypeInsn(int opcode, string type)
        {
            if (opcode == IOpcodes.New)
            {
                if (_labels == null)
                {
                    var label = new Label();
                    _labels = new List<Label>(3);
                    _labels.Add(label);
                    if (mv != null)
                        mv.VisitLabel(label);
                }

                foreach (var label in _labels)
                    UninitializedTypes[label] = type;
            }

            base.VisitTypeInsn(opcode, type);
            Execute(opcode, 0, type);
        }

        public override void VisitFieldInsn(int opcode, string owner, string name, string descriptor)
        {
            base.VisitFieldInsn(opcode, owner, name, descriptor);
            Execute(opcode, 0, descriptor);
        }

        public override void VisitMethodInsn(int opcodeAndSource, string owner, string name, string descriptor, bool isInterface)
        {
            if (api < IOpcodes.Asm5 && (opcodeAndSource & IOpcodes.Source_Deprecated) == 0)
            {
                // Redirect the call to the deprecated version of this method.
                base.VisitMethodInsn(opcodeAndSource, owner, name, descriptor, isInterface);
                return;
            }

            base.VisitMethodInsn(opcodeAndSource, owner, name, descriptor, isInterface);
            var opcode = opcodeAndSource & ~IOpcodes.Source_Mask;
            if (Locals == null)
            {
                _labels = null;
                return;
            }

            Pop(descriptor);
            if (opcode != IOpcodes.Invokestatic)
            {
                var value = Pop();
                if (opcode == IOpcodes.Invokespecial && name.Equals("<init>"))
                {
                    object initializedValue;
                    if (Equals(value, IOpcodes.uninitializedThis))
                        initializedValue = this._owner;
                    else
                    {
                        UninitializedTypes.TryGetValue(value, out initializedValue);
                    }

                    for (var i = 0; i < Locals.Count; ++i)
                        if (Locals[i] == value)
                            Locals[i] = initializedValue;
                    for (var i = 0; i < Stack.Count; ++i)
                        if (Stack[i] == value)
                            Stack[i] = initializedValue;
                }
            }

            PushDescriptor(descriptor);
            _labels = null;
        }

        public override void VisitInvokeDynamicInsn(string name, string descriptor, Handle bootstrapMethodHandle, params object[] bootstrapMethodArguments)
        {
            base.VisitInvokeDynamicInsn(name, descriptor, bootstrapMethodHandle, bootstrapMethodArguments);
            if (Locals == null)
            {
                _labels = null;
                return;
            }

            Pop(descriptor);
            PushDescriptor(descriptor);
            _labels = null;
        }

        public override void VisitJumpInsn(int opcode, Label label)
        {
            base.VisitJumpInsn(opcode, label);
            Execute(opcode, 0, null);
            if (opcode == IOpcodes.Goto)
            {
                Locals = null;
                Stack = null;
            }
        }

        public override void VisitLabel(Label label)
        {
            base.VisitLabel(label);
            if (_labels == null)
                _labels = new List<Label>(3);
            _labels.Add(label);
        }

        public override void VisitLdcInsn(object value)
        {
            base.VisitLdcInsn(value);
            if (Locals == null)
            {
                _labels = null;
                return;
            }

            if (value is int)
            {
                Push(IOpcodes.integer);
            }
            else if (value is long)
            {
                Push(IOpcodes.@long);
                Push(IOpcodes.top);
            }
            else if (value is float)
            {
                Push(IOpcodes.@float);
            }
            else if (value is double)
            {
                Push(IOpcodes.@double);
                Push(IOpcodes.top);
            }
            else if (value is string)
            {
                Push("java/lang/String");
            }
            else if (value is JType)
            {
                var sort = ((JType)value).Sort;
                if (sort == JType.Object || sort == JType.Array)
                    Push("java/lang/Class");
                else if (sort == JType.Method)
                    Push("java/lang/invoke/MethodType");
                else
                    throw new ArgumentException();
            }
            else if (value is Handle)
            {
                Push("java/lang/invoke/MethodHandle");
            }
            else if (value is ConstantDynamic)
            {
                PushDescriptor(((ConstantDynamic)value).Descriptor);
            }
            else
            {
                throw new ArgumentException();
            }

            _labels = null;
        }

        public override void VisitIincInsn(int varIndex, int increment)
        {
            base.VisitIincInsn(varIndex, increment);
            _maxLocals = Math.Max(_maxLocals, varIndex + 1);
            Execute(IOpcodes.Iinc, varIndex, null);
        }

        public override void VisitTableSwitchInsn(int min, int max, Label dflt, params Label[] labels)
        {
            base.VisitTableSwitchInsn(min, max, dflt, labels);
            Execute(IOpcodes.Tableswitch, 0, null);
            Locals = null;
            Stack = null;
        }

        public override void VisitLookupSwitchInsn(Label dflt, int[] keys, Label[] labels)
        {
            base.VisitLookupSwitchInsn(dflt, keys, labels);
            Execute(IOpcodes.Lookupswitch, 0, null);
            Locals = null;
            Stack = null;
        }

        public override void VisitMultiANewArrayInsn(string descriptor, int numDimensions)
        {
            base.VisitMultiANewArrayInsn(descriptor, numDimensions);
            Execute(IOpcodes.Multianewarray, numDimensions, descriptor);
        }

        public override void VisitLocalVariable(string name, string descriptor, string signature, Label start, Label end, int index)
        {
            var firstDescriptorChar = descriptor[0];
            _maxLocals = Math.Max(_maxLocals, index + (firstDescriptorChar == 'J' || firstDescriptorChar == 'D' ? 2 : 1));
            base.VisitLocalVariable(name, descriptor, signature, start, end, index);
        }

        public override void VisitMaxs(int maxStack, int maxLocals)
        {
            if (mv != null)
            {
                this._maxStack = Math.Max(this._maxStack, maxStack);
                this._maxLocals = Math.Max(this._maxLocals, maxLocals);
                mv.VisitMaxs(this._maxStack, this._maxLocals);
            }
        }

        // -----------------------------------------------------------------------------------------------
        private object Get(int local)
        {
            _maxLocals = Math.Max(_maxLocals, local + 1);
            return local < Locals.Count ? Locals[local] : IOpcodes.top;
        }

        private void Set(int local, object type)
        {
            _maxLocals = Math.Max(_maxLocals, local + 1);
            while (local >= Locals.Count)
                Locals.Add(IOpcodes.top);
            Locals[local] = type;
        }

        private void Push(object type)
        {
            Stack.Add(type);
            _maxStack = Math.Max(_maxStack, Stack.Count);
        }

        private void PushDescriptor(string fieldOrMethodDescriptor)
        {
            var descriptor = fieldOrMethodDescriptor[0] == '(' ? JType.GetReturnType(fieldOrMethodDescriptor).Descriptor : fieldOrMethodDescriptor;
            switch (descriptor[0])
            {
                case 'V':
                    return;
                case 'Z':
                case 'C':
                case 'B':
                case 'S':
                case 'I':
                    Push(IOpcodes.integer);
                    return;
                case 'F':
                    Push(IOpcodes.@float);
                    return;
                case 'J':
                    Push(IOpcodes.@long);
                    Push(IOpcodes.top);
                    return;
                case 'D':
                    Push(IOpcodes.@double);
                    Push(IOpcodes.top);
                    return;
                case '[':
                    Push(descriptor);
                    break;
                case 'L':
                    Push(descriptor.Substring(1, descriptor.Length - 1 - 1));
                    break;
                default:
                    throw new Exception();
            }
        }

        private object Pop()
        {
            var stackCount = Stack.Count - 1;
            var current = Stack[stackCount];
            Stack.RemoveAt(stackCount);
            return current;
        }

        private void Pop(int numSlots)
        {
            var size = Stack.Count;
            var end = size - numSlots;
            for (var i = size - 1; i >= end; --i)
                Stack.RemoveAt(i);
        }

        private void Pop(string descriptor)
        {
            var firstDescriptorChar = descriptor[0];
            if (firstDescriptorChar == '(')
            {
                var numSlots = 0;
                var types = JType.GetArgumentTypes(descriptor);
                foreach (var type in types)
                    numSlots += type.Size;
                Pop(numSlots);
            }
            else if (firstDescriptorChar == 'J' || firstDescriptorChar == 'D')
            {
                Pop(2);
            }
            else
            {
                Pop(1);
            }
        }

        private void Execute(int opcode, int intArg, string stringArg)
        {
            if (opcode == IOpcodes.Jsr || opcode == IOpcodes.Ret)
                throw new ArgumentException("JSR/RET are not supported");
            if (Locals == null)
            {
                _labels = null;
                return;
            }

            object value1;
            object value2;
            object value3;
            object t4;
            switch (opcode)
            {
                case IOpcodes.Nop:
                case IOpcodes.Ineg:
                case IOpcodes.Lneg:
                case IOpcodes.Fneg:
                case IOpcodes.Dneg:
                case IOpcodes.I2B:
                case IOpcodes.I2C:
                case IOpcodes.I2S:
                case IOpcodes.Goto:
                case IOpcodes.Return:
                    break;
                case IOpcodes.Aconst_Null:
                    Push(IOpcodes.@null);
                    break;
                case IOpcodes.Iconst_M1:
                case IOpcodes.Iconst_0:
                case IOpcodes.Iconst_1:
                case IOpcodes.Iconst_2:
                case IOpcodes.Iconst_3:
                case IOpcodes.Iconst_4:
                case IOpcodes.Iconst_5:
                case IOpcodes.Bipush:
                case IOpcodes.Sipush:
                    Push(IOpcodes.integer);
                    break;
                case IOpcodes.Lconst_0:
                case IOpcodes.Lconst_1:
                    Push(IOpcodes.@long);
                    Push(IOpcodes.top);
                    break;
                case IOpcodes.Fconst_0:
                case IOpcodes.Fconst_1:
                case IOpcodes.Fconst_2:
                    Push(IOpcodes.@float);
                    break;
                case IOpcodes.Dconst_0:
                case IOpcodes.Dconst_1:
                    Push(IOpcodes.@double);
                    Push(IOpcodes.top);
                    break;
                case IOpcodes.Iload:
                case IOpcodes.Fload:
                case IOpcodes.Aload:
                    Push(Get(intArg));
                    break;
                case IOpcodes.Lload:
                case IOpcodes.Dload:
                    Push(Get(intArg));
                    Push(IOpcodes.top);
                    break;
                case IOpcodes.Laload:
                case IOpcodes.D2L:
                    Pop(2);
                    Push(IOpcodes.@long);
                    Push(IOpcodes.top);
                    break;
                case IOpcodes.Daload:
                case IOpcodes.L2D:
                    Pop(2);
                    Push(IOpcodes.@double);
                    Push(IOpcodes.top);
                    break;
                case IOpcodes.Aaload:
                    Pop(1);
                    value1 = Pop();
                    if (value1 is string)
                        PushDescriptor(((string)value1).Substring(1));
                    else if (Equals(value1, IOpcodes.@null))
                        Push(value1);
                    else
                        Push("java/lang/Object");
                    break;
                case IOpcodes.Istore:
                case IOpcodes.Fstore:
                case IOpcodes.Astore:
                    value1 = Pop();
                    Set(intArg, value1);
                    if (intArg > 0)
                    {
                        value2 = Get(intArg - 1);
                        if (Equals(value2, IOpcodes.@long) || Equals(value2, IOpcodes.@double))
                            Set(intArg - 1, IOpcodes.top);
                    }

                    break;
                case IOpcodes.Lstore:
                case IOpcodes.Dstore:
                    Pop(1);
                    value1 = Pop();
                    Set(intArg, value1);
                    Set(intArg + 1, IOpcodes.top);
                    if (intArg > 0)
                    {
                        value2 = Get(intArg - 1);
                        if (Equals(value2, IOpcodes.@long) || Equals(value2, IOpcodes.@double))
                            Set(intArg - 1, IOpcodes.top);
                    }

                    break;
                case IOpcodes.Iastore:
                case IOpcodes.Bastore:
                case IOpcodes.Castore:
                case IOpcodes.Sastore:
                case IOpcodes.Fastore:
                case IOpcodes.Aastore:
                    Pop(3);
                    break;
                case IOpcodes.Lastore:
                case IOpcodes.Dastore:
                    Pop(4);
                    break;
                case IOpcodes.Pop:
                case IOpcodes.Ifeq:
                case IOpcodes.Ifne:
                case IOpcodes.Iflt:
                case IOpcodes.Ifge:
                case IOpcodes.Ifgt:
                case IOpcodes.Ifle:
                case IOpcodes.Ireturn:
                case IOpcodes.Freturn:
                case IOpcodes.Areturn:
                case IOpcodes.Tableswitch:
                case IOpcodes.Lookupswitch:
                case IOpcodes.Athrow:
                case IOpcodes.Monitorenter:
                case IOpcodes.Monitorexit:
                case IOpcodes.Ifnull:
                case IOpcodes.Ifnonnull:
                    Pop(1);
                    break;
                case IOpcodes.Pop2:
                case IOpcodes.If_Icmpeq:
                case IOpcodes.If_Icmpne:
                case IOpcodes.If_Icmplt:
                case IOpcodes.If_Icmpge:
                case IOpcodes.If_Icmpgt:
                case IOpcodes.If_Icmple:
                case IOpcodes.If_Acmpeq:
                case IOpcodes.If_Acmpne:
                case IOpcodes.Lreturn:
                case IOpcodes.Dreturn:
                    Pop(2);
                    break;
                case IOpcodes.Dup:
                    value1 = Pop();
                    Push(value1);
                    Push(value1);
                    break;
                case IOpcodes.Dup_X1:
                    value1 = Pop();
                    value2 = Pop();
                    Push(value1);
                    Push(value2);
                    Push(value1);
                    break;
                case IOpcodes.Dup_X2:
                    value1 = Pop();
                    value2 = Pop();
                    value3 = Pop();
                    Push(value1);
                    Push(value3);
                    Push(value2);
                    Push(value1);
                    break;
                case IOpcodes.Dup2:
                    value1 = Pop();
                    value2 = Pop();
                    Push(value2);
                    Push(value1);
                    Push(value2);
                    Push(value1);
                    break;
                case IOpcodes.Dup2_X1:
                    value1 = Pop();
                    value2 = Pop();
                    value3 = Pop();
                    Push(value2);
                    Push(value1);
                    Push(value3);
                    Push(value2);
                    Push(value1);
                    break;
                case IOpcodes.Dup2_X2:
                    value1 = Pop();
                    value2 = Pop();
                    value3 = Pop();
                    t4 = Pop();
                    Push(value2);
                    Push(value1);
                    Push(t4);
                    Push(value3);
                    Push(value2);
                    Push(value1);
                    break;
                case IOpcodes.Swap:
                    value1 = Pop();
                    value2 = Pop();
                    Push(value1);
                    Push(value2);
                    break;
                case IOpcodes.Iaload:
                case IOpcodes.Baload:
                case IOpcodes.Caload:
                case IOpcodes.Saload:
                case IOpcodes.Iadd:
                case IOpcodes.Isub:
                case IOpcodes.Imul:
                case IOpcodes.Idiv:
                case IOpcodes.Irem:
                case IOpcodes.Iand:
                case IOpcodes.Ior:
                case IOpcodes.Ixor:
                case IOpcodes.Ishl:
                case IOpcodes.Ishr:
                case IOpcodes.Iushr:
                case IOpcodes.L2I:
                case IOpcodes.D2I:
                case IOpcodes.Fcmpl:
                case IOpcodes.Fcmpg:
                    Pop(2);
                    Push(IOpcodes.integer);
                    break;
                case IOpcodes.Ladd:
                case IOpcodes.Lsub:
                case IOpcodes.Lmul:
                case IOpcodes.Ldiv:
                case IOpcodes.Lrem:
                case IOpcodes.Land:
                case IOpcodes.Lor:
                case IOpcodes.Lxor:
                    Pop(4);
                    Push(IOpcodes.@long);
                    Push(IOpcodes.top);
                    break;
                case IOpcodes.Faload:
                case IOpcodes.Fadd:
                case IOpcodes.Fsub:
                case IOpcodes.Fmul:
                case IOpcodes.Fdiv:
                case IOpcodes.Frem:
                case IOpcodes.L2F:
                case IOpcodes.D2F:
                    Pop(2);
                    Push(IOpcodes.@float);
                    break;
                case IOpcodes.Dadd:
                case IOpcodes.Dsub:
                case IOpcodes.Dmul:
                case IOpcodes.Ddiv:
                case IOpcodes.Drem:
                    Pop(4);
                    Push(IOpcodes.@double);
                    Push(IOpcodes.top);
                    break;
                case IOpcodes.Lshl:
                case IOpcodes.Lshr:
                case IOpcodes.Lushr:
                    Pop(3);
                    Push(IOpcodes.@long);
                    Push(IOpcodes.top);
                    break;
                case IOpcodes.Iinc:
                    Set(intArg, IOpcodes.integer);
                    break;
                case IOpcodes.I2L:
                case IOpcodes.F2L:
                    Pop(1);
                    Push(IOpcodes.@long);
                    Push(IOpcodes.top);
                    break;
                case IOpcodes.I2F:
                    Pop(1);
                    Push(IOpcodes.@float);
                    break;
                case IOpcodes.I2D:
                case IOpcodes.F2D:
                    Pop(1);
                    Push(IOpcodes.@double);
                    Push(IOpcodes.top);
                    break;
                case IOpcodes.F2I:
                case IOpcodes.Arraylength:
                case IOpcodes.Instanceof:
                    Pop(1);
                    Push(IOpcodes.integer);
                    break;
                case IOpcodes.Lcmp:
                case IOpcodes.Dcmpl:
                case IOpcodes.Dcmpg:
                    Pop(4);
                    Push(IOpcodes.integer);
                    break;
                case IOpcodes.Getstatic:
                    PushDescriptor(stringArg);
                    break;
                case IOpcodes.Putstatic:
                    Pop(stringArg);
                    break;
                case IOpcodes.Getfield:
                    Pop(1);
                    PushDescriptor(stringArg);
                    break;
                case IOpcodes.Putfield:
                    Pop(stringArg);
                    Pop();
                    break;
                case IOpcodes.New:
                    Push(_labels[0]);
                    break;
                case IOpcodes.Newarray:
                    Pop();
                    switch (intArg)
                    {
                        case IOpcodes.Boolean:
                            PushDescriptor("[Z");
                            break;
                        case IOpcodes.Char:
                            PushDescriptor("[C");
                            break;
                        case IOpcodes.Byte:
                            PushDescriptor("[B");
                            break;
                        case IOpcodes.Short:
                            PushDescriptor("[S");
                            break;
                        case IOpcodes.Int:
                            PushDescriptor("[I");
                            break;
                        case IOpcodes.Float:
                            PushDescriptor("[F");
                            break;
                        case IOpcodes.Double:
                            PushDescriptor("[D");
                            break;
                        case IOpcodes.Long:
                            PushDescriptor("[J");
                            break;
                        default:
                            throw new ArgumentException("Invalid array type " + intArg);
                    }

                    break;
                case IOpcodes.Anewarray:
                    Pop();
                    PushDescriptor("[" + JType.GetObjectType(stringArg));
                    break;
                case IOpcodes.Checkcast:
                    Pop();
                    PushDescriptor(JType.GetObjectType(stringArg).Descriptor);
                    break;
                case IOpcodes.Multianewarray:
                    Pop(intArg);
                    PushDescriptor(stringArg);
                    break;
                default:
                    throw new ArgumentException("Invalid opcode " + opcode);
            }

            _labels = null;
        }
    }
}
