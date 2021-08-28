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
    ///     A <seealso cref="MethodVisitor" /> with convenient methods to generate code. For example, using this
    ///     adapter, the class below
    ///     <pre>
    ///         public class Example {
    ///         public static void main(String[] args) {
    ///         System.out.println(&quot;Hello world!&quot;);
    ///         }
    ///         }
    ///     </pre>
    ///     <para>
    ///         can be generated as follows:
    ///         <pre>
    ///             ClassWriter cw = new ClassWriter(0);
    ///             cw.visit(V1_1, ACC_PUBLIC, &quot;Example&quot;, null, &quot;java/lang/Object&quot;, null);
    ///             Method m = Method.getMethod(&quot;void &lt;init&gt; ()&quot;);
    ///             GeneratorAdapter mg = new GeneratorAdapter(ACC_PUBLIC, m, null, null, cw);
    ///             mg.loadThis();
    ///             mg.invokeConstructor(Type.getType(Object.class), m);
    ///             mg.returnValue();
    ///             mg.endMethod();
    ///             m = Method.getMethod(&quot;void main (String[])&quot;);
    ///             mg = new GeneratorAdapter(ACC_PUBLIC + ACC_STATIC, m, null, null, cw);
    ///             mg.getStatic(Type.getType(System.class), &quot;out&quot;, Type.getType(PrintStream.class));
    ///             mg.push(&quot;Hello world!&quot;);
    ///             mg.invokeVirtual(Type.getType(PrintStream.class),
    ///             Method.getMethod(&quot;void println (String)&quot;));
    ///             mg.returnValue();
    ///             mg.endMethod();
    ///             cw.visitEnd();
    ///         </pre>
    ///         @author Juozas Baliuka
    ///         @author Chris Nokleberg
    ///         @author Eric Bruneton
    ///         @author Prashant Deva
    ///     </para>
    /// </summary>
    public class GeneratorAdapter : LocalVariablesSorter
    {
        private const string ClassDescriptor = "Ljava/lang/Class;";

        /// <summary>
        ///     Constant for the <seealso cref="Math" /> method.
        /// </summary>
        public const int Add = IOpcodes.Iadd;

        /// <summary>
        ///     Constant for the <seealso cref="Math" /> method.
        /// </summary>
        public const int Sub = IOpcodes.Isub;

        /// <summary>
        ///     Constant for the <seealso cref="Math" /> method.
        /// </summary>
        public const int Mul = IOpcodes.Imul;

        /// <summary>
        ///     Constant for the <seealso cref="Math" /> method.
        /// </summary>
        public const int Div = IOpcodes.Idiv;

        /// <summary>
        ///     Constant for the <seealso cref="Math" /> method.
        /// </summary>
        public const int Rem = IOpcodes.Irem;

        /// <summary>
        ///     Constant for the <seealso cref="Math" /> method.
        /// </summary>
        public const int Neg = IOpcodes.Ineg;

        /// <summary>
        ///     Constant for the <seealso cref="Math" /> method.
        /// </summary>
        public const int Shl = IOpcodes.Ishl;

        /// <summary>
        ///     Constant for the <seealso cref="Math" /> method.
        /// </summary>
        public const int Shr = IOpcodes.Ishr;

        /// <summary>
        ///     Constant for the <seealso cref="Math" /> method.
        /// </summary>
        public const int Ushr = IOpcodes.Iushr;

        /// <summary>
        ///     Constant for the <seealso cref="Math" /> method.
        /// </summary>
        public const int And = IOpcodes.Iand;

        /// <summary>
        ///     Constant for the <seealso cref="Math" /> method.
        /// </summary>
        public const int Or = IOpcodes.Ior;

        /// <summary>
        ///     Constant for the <seealso cref="Math" /> method.
        /// </summary>
        public const int Xor = IOpcodes.Ixor;

        /// <summary>
        ///     Constant for the <seealso cref="IfCmp" /> method.
        /// </summary>
        public const int Eq = IOpcodes.Ifeq;

        /// <summary>
        ///     Constant for the <seealso cref="IfCmp" /> method.
        /// </summary>
        public const int Ne = IOpcodes.Ifne;

        /// <summary>
        ///     Constant for the <seealso cref="IfCmp" /> method.
        /// </summary>
        public const int Lt = IOpcodes.Iflt;

        /// <summary>
        ///     Constant for the <seealso cref="IfCmp" /> method.
        /// </summary>
        public const int Ge = IOpcodes.Ifge;

        /// <summary>
        ///     Constant for the <seealso cref="IfCmp" /> method.
        /// </summary>
        public const int Gt = IOpcodes.Ifgt;

        /// <summary>
        ///     Constant for the <seealso cref="IfCmp" /> method.
        /// </summary>
        public const int Le = IOpcodes.Ifle;

        private static readonly JType BYTE_TYPE = JType.GetObjectType("java/lang/Byte");

        private static readonly JType BOOLEAN_TYPE = JType.GetObjectType("java/lang/Boolean");

        private static readonly JType SHORT_TYPE = JType.GetObjectType("java/lang/Short");

        private static readonly JType CHARACTER_TYPE = JType.GetObjectType("java/lang/Character");

        private static readonly JType INTEGER_TYPE = JType.GetObjectType("java/lang/Integer");

        private static readonly JType FLOAT_TYPE = JType.GetObjectType("java/lang/Float");

        private static readonly JType LONG_TYPE = JType.GetObjectType("java/lang/Long");

        private static readonly JType DOUBLE_TYPE = JType.GetObjectType("java/lang/Double");

        private static readonly JType NUMBER_TYPE = JType.GetObjectType("java/lang/Number");

        private static readonly JType OBJECT_TYPE = JType.GetObjectType("java/lang/Object");

        private static readonly Method BOOLEAN_VALUE = Method.GetMethod("boolean booleanValue()");

        private static readonly Method CHAR_VALUE = Method.GetMethod("char charValue()");

        private static readonly Method INT_VALUE = Method.GetMethod("int intValue()");

        private static readonly Method FLOAT_VALUE = Method.GetMethod("float floatValue()");

        private static readonly Method LONG_VALUE = Method.GetMethod("long longValue()");

        private static readonly Method DOUBLE_VALUE = Method.GetMethod("double doubleValue()");

        /// <summary>
        ///     The access flags of the visited method.
        /// </summary>
        private readonly int _access;

        /// <summary>
        ///     The argument types of the visited method.
        /// </summary>
        private readonly JType[] _argumentTypes;

        /// <summary>
        ///     The types of the local variables of the visited method.
        /// </summary>
        private readonly List<JType> _localTypes = new();

        /// <summary>
        ///     The return type of the visited method.
        /// </summary>
        private readonly JType _returnType;

        /// <summary>
        ///     Constructs a new <seealso cref="GeneratorAdapter" />. <i>Subclasses must not use this constructor</i>.
        ///     Instead, they must use the <seealso cref="GeneratorAdapter(int, MethodVisitor, int, string, string)" />
        ///     version.
        /// </summary>
        /// <param name="methodVisitor"> the method visitor to which this adapter delegates calls. </param>
        /// <param name="access"> the method's access flags (see <seealso cref="IOpcodes" />). </param>
        /// <param name="name"> the method's name. </param>
        /// <param name="descriptor"> the method's descriptor (see <seealso cref="Type" />). </param>
        /// <exception cref="IllegalStateException"> if a subclass calls this constructor. </exception>
        public GeneratorAdapter(MethodVisitor methodVisitor, int access, string name, string descriptor) : this(
            IOpcodes.Asm9, methodVisitor, access, name, descriptor)
        {
            if (GetType() != typeof(GeneratorAdapter)) throw new InvalidOperationException();
        }

        /// <summary>
        ///     Constructs a new <seealso cref="GeneratorAdapter" />.
        /// </summary>
        /// <param name="api">
        ///     the ASM API version implemented by this visitor. Must be one of the {@code
        ///     ASM}<i>x</i> values in <seealso cref="IOpcodes" />.
        /// </param>
        /// <param name="methodVisitor"> the method visitor to which this adapter delegates calls. </param>
        /// <param name="access"> the method's access flags (see <seealso cref="IOpcodes" />). </param>
        /// <param name="name"> the method's name. </param>
        /// <param name="descriptor"> the method's descriptor (see <seealso cref="Type" />). </param>
        public GeneratorAdapter(int api, MethodVisitor methodVisitor, int access, string name, string descriptor) :
            base(api, access, descriptor, methodVisitor)
        {
            this._access = access;
            this.Name = name;
            _returnType = JType.GetReturnType(descriptor);
            _argumentTypes = JType.GetArgumentTypes(descriptor);
        }

        /// <summary>
        ///     Constructs a new <seealso cref="GeneratorAdapter" />. <i>Subclasses must not use this constructor</i>.
        ///     Instead, they must use the <seealso cref="GeneratorAdapter(int, MethodVisitor, int, String, String)" />
        ///     version.
        /// </summary>
        /// <param name="access"> access flags of the adapted method. </param>
        /// <param name="method"> the adapted method. </param>
        /// <param name="methodVisitor"> the method visitor to which this adapter delegates calls. </param>
        public GeneratorAdapter(int access, Method method, MethodVisitor methodVisitor) : this(methodVisitor, access,
            method.Name, method.Descriptor)
        {
        }

        /// <summary>
        ///     Constructs a new <seealso cref="GeneratorAdapter" />. <i>Subclasses must not use this constructor</i>.
        ///     Instead, they must use the <seealso cref="GeneratorAdapter(int, MethodVisitor, int, String, String)" />
        ///     version.
        /// </summary>
        /// <param name="access"> access flags of the adapted method. </param>
        /// <param name="method"> the adapted method. </param>
        /// <param name="signature"> the signature of the adapted method (may be {@literal null}). </param>
        /// <param name="exceptions"> the exceptions thrown by the adapted method (may be {@literal null}). </param>
        /// <param name="classVisitor"> the class visitor to which this adapter delegates calls. </param>
        public GeneratorAdapter(int access, Method method, string signature, JType[] exceptions,
            ClassVisitor classVisitor) : this(access, method,
            classVisitor.VisitMethod(access, method.Name, method.Descriptor, signature,
                exceptions == null ? null : GetInternalNames(exceptions)))
        {
        }

        public virtual int Access => _access;

        /// <summary>
        ///     The name of the visited method.
        /// </summary>
        public virtual string Name { get; }

        public virtual JType ReturnType => _returnType;

        public virtual JType[] ArgumentTypes => (JType[])_argumentTypes.Clone();

        /// <summary>
        ///     Returns the internal names of the given types.
        /// </summary>
        /// <param name="types"> a set of types. </param>
        /// <returns> the internal names of the given types. </returns>
        private static string[] GetInternalNames(JType[] types)
        {
            var names = new string[types.Length];
            for (var i = 0; i < names.Length; ++i) names[i] = types[i].InternalName;
            return names;
        }

        // -----------------------------------------------------------------------------------------------
        // Instructions to push constants on the stack
        // -----------------------------------------------------------------------------------------------

        /// <summary>
        ///     Generates the instruction to push the given value on the stack.
        /// </summary>
        /// <param name="value"> the value to be pushed on the stack. </param>
        public virtual void Push(bool value)
        {
            Push(value ? 1 : 0);
        }

        /// <summary>
        ///     Generates the instruction to push the given value on the stack.
        /// </summary>
        /// <param name="value"> the value to be pushed on the stack. </param>
        public virtual void Push(int value)
        {
            if (value >= -1 && value <= 5)
                mv.VisitInsn(IOpcodes.Iconst_0 + value);
            else if (value >= sbyte.MinValue && value <= sbyte.MaxValue)
                mv.VisitIntInsn(IOpcodes.Bipush, value);
            else if (value >= short.MinValue && value <= short.MaxValue)
                mv.VisitIntInsn(IOpcodes.Sipush, value);
            else
                mv.VisitLdcInsn(value);
        }

        /// <summary>
        ///     Generates the instruction to push the given value on the stack.
        /// </summary>
        /// <param name="value"> the value to be pushed on the stack. </param>
        public virtual void Push(long value)
        {
            if (value == 0L || value == 1L)
                mv.VisitInsn(IOpcodes.Lconst_0 + (int)value);
            else
                mv.VisitLdcInsn(value);
        }

        /// <summary>
        ///     Generates the instruction to push the given value on the stack.
        /// </summary>
        /// <param name="value"> the value to be pushed on the stack. </param>
        public virtual void Push(float value)
        {
            var bits = BitConverter.SingleToInt32Bits(value);
            if (bits == 0L || bits == 0x3F800000 || bits == 0x40000000)
                // 0..2
                mv.VisitInsn(IOpcodes.Fconst_0 + (int)value);
            else
                mv.VisitLdcInsn(value);
        }

        /// <summary>
        ///     Generates the instruction to push the given value on the stack.
        /// </summary>
        /// <param name="value"> the value to be pushed on the stack. </param>
        public virtual void Push(double value)
        {
            var bits = BitConverter.DoubleToInt64Bits(value);
            if (bits == 0L || bits == 0x3FF0000000000000L)
                // +0.0d and 1.0d
                mv.VisitInsn(IOpcodes.Dconst_0 + (int)value);
            else
                mv.VisitLdcInsn(value);
        }

        /// <summary>
        ///     Generates the instruction to push the given value on the stack.
        /// </summary>
        /// <param name="value"> the value to be pushed on the stack. May be {@literal null}. </param>
        public virtual void Push(string value)
        {
            if (ReferenceEquals(value, null))
                mv.VisitInsn(IOpcodes.Aconst_Null);
            else
                mv.VisitLdcInsn(value);
        }

        /// <summary>
        ///     Generates the instruction to push the given value on the stack.
        /// </summary>
        /// <param name="value"> the value to be pushed on the stack. </param>
        public virtual void Push(JType value)
        {
            if (value == null)
                mv.VisitInsn(IOpcodes.Aconst_Null);
            else
                switch (value.Sort)
                {
                    case JType.Boolean:
                        mv.VisitFieldInsn(IOpcodes.Getstatic, "java/lang/Boolean", "TYPE", ClassDescriptor);
                        break;
                    case JType.Char:
                        mv.VisitFieldInsn(IOpcodes.Getstatic, "java/lang/Character", "TYPE", ClassDescriptor);
                        break;
                    case JType.Byte:
                        mv.VisitFieldInsn(IOpcodes.Getstatic, "java/lang/Byte", "TYPE", ClassDescriptor);
                        break;
                    case JType.Short:
                        mv.VisitFieldInsn(IOpcodes.Getstatic, "java/lang/Short", "TYPE", ClassDescriptor);
                        break;
                    case JType.Int:
                        mv.VisitFieldInsn(IOpcodes.Getstatic, "java/lang/Integer", "TYPE", ClassDescriptor);
                        break;
                    case JType.Float:
                        mv.VisitFieldInsn(IOpcodes.Getstatic, "java/lang/Float", "TYPE", ClassDescriptor);
                        break;
                    case JType.Long:
                        mv.VisitFieldInsn(IOpcodes.Getstatic, "java/lang/Long", "TYPE", ClassDescriptor);
                        break;
                    case JType.Double:
                        mv.VisitFieldInsn(IOpcodes.Getstatic, "java/lang/Double", "TYPE", ClassDescriptor);
                        break;
                    default:
                        mv.VisitLdcInsn(value);
                        break;
                }
        }

        /// <summary>
        ///     Generates the instruction to push a handle on the stack.
        /// </summary>
        /// <param name="handle"> the handle to be pushed on the stack. </param>
        public virtual void Push(Handle handle)
        {
            if (handle == null)
                mv.VisitInsn(IOpcodes.Aconst_Null);
            else
                mv.VisitLdcInsn(handle);
        }

        /// <summary>
        ///     Generates the instruction to push a constant dynamic on the stack.
        /// </summary>
        /// <param name="constantDynamic"> the constant dynamic to be pushed on the stack. </param>
        public virtual void Push(ConstantDynamic constantDynamic)
        {
            if (constantDynamic == null)
                mv.VisitInsn(IOpcodes.Aconst_Null);
            else
                mv.VisitLdcInsn(constantDynamic);
        }

        // -----------------------------------------------------------------------------------------------
        // Instructions to load and store method arguments
        // -----------------------------------------------------------------------------------------------

        /// <summary>
        ///     Returns the index of the given method argument in the frame's local variables array.
        /// </summary>
        /// <param name="arg"> the index of a method argument. </param>
        /// <returns> the index of the given method argument in the frame's local variables array. </returns>
        private int GetArgIndex(int arg)
        {
            var index = (_access & IOpcodes.Acc_Static) == 0 ? 1 : 0;
            for (var i = 0; i < arg; i++) index += _argumentTypes[i].Size;
            return index;
        }

        /// <summary>
        ///     Generates the instruction to push a local variable on the stack.
        /// </summary>
        /// <param name="type"> the type of the local variable to be loaded. </param>
        /// <param name="index"> an index in the frame's local variables array. </param>
        private void LoadInsn(JType type, int index)
        {
            mv.VisitVarInsn(type.GetOpcode(IOpcodes.Iload), index);
        }

        /// <summary>
        ///     Generates the instruction to store the top stack value in a local variable.
        /// </summary>
        /// <param name="type"> the type of the local variable to be stored. </param>
        /// <param name="index"> an index in the frame's local variables array. </param>
        private void StoreInsn(JType type, int index)
        {
            mv.VisitVarInsn(type.GetOpcode(IOpcodes.Istore), index);
        }

        /// <summary>
        ///     Generates the instruction to load 'this' on the stack.
        /// </summary>
        public virtual void LoadThis()
        {
            if ((_access & IOpcodes.Acc_Static) != 0)
                throw new InvalidOperationException("no 'this' pointer within static method");
            mv.VisitVarInsn(IOpcodes.Aload, 0);
        }

        /// <summary>
        ///     Generates the instruction to load the given method argument on the stack.
        /// </summary>
        /// <param name="arg"> the index of a method argument. </param>
        public virtual void LoadArg(int arg)
        {
            LoadInsn(_argumentTypes[arg], GetArgIndex(arg));
        }

        /// <summary>
        ///     Generates the instructions to load the given method arguments on the stack.
        /// </summary>
        /// <param name="arg"> the index of the first method argument to be loaded. </param>
        /// <param name="count"> the number of method arguments to be loaded. </param>
        public virtual void LoadArgs(int arg, int count)
        {
            var index = GetArgIndex(arg);
            for (var i = 0; i < count; ++i)
            {
                var argumentType = _argumentTypes[arg + i];
                LoadInsn(argumentType, index);
                index += argumentType.Size;
            }
        }

        /// <summary>
        ///     Generates the instructions to load all the method arguments on the stack.
        /// </summary>
        public virtual void LoadArgs()
        {
            LoadArgs(0, _argumentTypes.Length);
        }

        /// <summary>
        ///     Generates the instructions to load all the method arguments on the stack, as a single object
        ///     array.
        /// </summary>
        public virtual void LoadArgArray()
        {
            Push(_argumentTypes.Length);
            NewArray(OBJECT_TYPE);
            for (var i = 0; i < _argumentTypes.Length; i++)
            {
                Dup();
                Push(i);
                LoadArg(i);
                Box(_argumentTypes[i]);
                ArrayStore(OBJECT_TYPE);
            }
        }

        /// <summary>
        ///     Generates the instruction to store the top stack value in the given method argument.
        /// </summary>
        /// <param name="arg"> the index of a method argument. </param>
        public virtual void StoreArg(int arg)
        {
            StoreInsn(_argumentTypes[arg], GetArgIndex(arg));
        }

        // -----------------------------------------------------------------------------------------------
        // Instructions to load and store local variables
        // -----------------------------------------------------------------------------------------------

        /// <summary>
        ///     Returns the type of the given local variable.
        /// </summary>
        /// <param name="local">
        ///     a local variable identifier, as returned by {@link
        ///     LocalVariablesSorter#newLocal(Type)}.
        /// </param>
        /// <returns> the type of the given local variable. </returns>
        public virtual JType GetLocalType(int local)
        {
            return _localTypes[local - firstLocal];
        }

        public override void SetLocalType(int local, JType type)
        {
            var index = local - firstLocal;
            while (_localTypes.Count < index + 1) _localTypes.Add(null);
            _localTypes[index] = type;
        }

        /// <summary>
        ///     Generates the instruction to load the given local variable on the stack.
        /// </summary>
        /// <param name="local">
        ///     a local variable identifier, as returned by {@link
        ///     LocalVariablesSorter#newLocal(Type)}.
        /// </param>
        public virtual void LoadLocal(int local)
        {
            LoadInsn(GetLocalType(local), local);
        }

        /// <summary>
        ///     Generates the instruction to load the given local variable on the stack.
        /// </summary>
        /// <param name="local">
        ///     a local variable identifier, as returned by {@link
        ///     LocalVariablesSorter#newLocal(Type)}.
        /// </param>
        /// <param name="type"> the type of this local variable. </param>
        public virtual void LoadLocal(int local, JType type)
        {
            SetLocalType(local, type);
            LoadInsn(type, local);
        }

        /// <summary>
        ///     Generates the instruction to store the top stack value in the given local variable.
        /// </summary>
        /// <param name="local">
        ///     a local variable identifier, as returned by {@link
        ///     LocalVariablesSorter#newLocal(Type)}.
        /// </param>
        public virtual void StoreLocal(int local)
        {
            StoreInsn(GetLocalType(local), local);
        }

        /// <summary>
        ///     Generates the instruction to store the top stack value in the given local variable.
        /// </summary>
        /// <param name="local">
        ///     a local variable identifier, as returned by {@link
        ///     LocalVariablesSorter#newLocal(Type)}.
        /// </param>
        /// <param name="type"> the type of this local variable. </param>
        public virtual void StoreLocal(int local, JType type)
        {
            SetLocalType(local, type);
            StoreInsn(type, local);
        }

        /// <summary>
        ///     Generates the instruction to load an element from an array.
        /// </summary>
        /// <param name="type"> the type of the array element to be loaded. </param>
        public virtual void ArrayLoad(JType type)
        {
            mv.VisitInsn(type.GetOpcode(IOpcodes.Iaload));
        }

        /// <summary>
        ///     Generates the instruction to store an element in an array.
        /// </summary>
        /// <param name="type"> the type of the array element to be stored. </param>
        public virtual void ArrayStore(JType type)
        {
            mv.VisitInsn(type.GetOpcode(IOpcodes.Iastore));
        }

        // -----------------------------------------------------------------------------------------------
        // Instructions to manage the stack
        // -----------------------------------------------------------------------------------------------

        /// <summary>
        ///     Generates a POP instruction.
        /// </summary>
        public virtual void Pop()
        {
            mv.VisitInsn(IOpcodes.Pop);
        }

        /// <summary>
        ///     Generates a POP2 instruction.
        /// </summary>
        public virtual void Pop2()
        {
            mv.VisitInsn(IOpcodes.Pop2);
        }

        /// <summary>
        ///     Generates a DUP instruction.
        /// </summary>
        public virtual void Dup()
        {
            mv.VisitInsn(IOpcodes.Dup);
        }

        /// <summary>
        ///     Generates a DUP2 instruction.
        /// </summary>
        public virtual void Dup2()
        {
            mv.VisitInsn(IOpcodes.Dup2);
        }

        /// <summary>
        ///     Generates a DUP_X1 instruction.
        /// </summary>
        public virtual void DupX1()
        {
            mv.VisitInsn(IOpcodes.Dup_X1);
        }

        /// <summary>
        ///     Generates a DUP_X2 instruction.
        /// </summary>
        public virtual void DupX2()
        {
            mv.VisitInsn(IOpcodes.Dup_X2);
        }

        /// <summary>
        ///     Generates a DUP2_X1 instruction.
        /// </summary>
        public virtual void Dup2X1()
        {
            mv.VisitInsn(IOpcodes.Dup2_X1);
        }

        /// <summary>
        ///     Generates a DUP2_X2 instruction.
        /// </summary>
        public virtual void Dup2X2()
        {
            mv.VisitInsn(IOpcodes.Dup2_X2);
        }

        /// <summary>
        ///     Generates a SWAP instruction.
        /// </summary>
        public virtual void Swap()
        {
            mv.VisitInsn(IOpcodes.Swap);
        }

        /// <summary>
        ///     Generates the instructions to swap the top two stack values.
        /// </summary>
        /// <param name="prev"> type of the top - 1 stack value. </param>
        /// <param name="type"> type of the top stack value. </param>
        public virtual void Swap(JType prev, JType type)
        {
            if (type.Size == 1)
            {
                if (prev.Size == 1)
                {
                    Swap(); // Same as dupX1 pop.
                }
                else
                {
                    DupX2();
                    Pop();
                }
            }
            else
            {
                if (prev.Size == 1)
                {
                    Dup2X1();
                    Pop2();
                }
                else
                {
                    Dup2X2();
                    Pop2();
                }
            }
        }

        // -----------------------------------------------------------------------------------------------
        // Instructions to do mathematical and logical operations
        // -----------------------------------------------------------------------------------------------

        /// <summary>
        ///     Generates the instruction to do the specified mathematical or logical operation.
        /// </summary>
        /// <param name="op">
        ///     a mathematical or logical operation. Must be one of ADD, SUB, MUL, DIV, REM, NEG,
        ///     SHL, SHR, USHR, AND, OR, XOR.
        /// </param>
        /// <param name="type"> the type of the operand(s) for this operation. </param>
        public virtual void Math(int op, JType type)
        {
            mv.VisitInsn(type.GetOpcode(op));
        }

        /// <summary>
        ///     Generates the instructions to compute the bitwise negation of the top stack value.
        /// </summary>
        public virtual void Not()
        {
            mv.VisitInsn(IOpcodes.Iconst_1);
            mv.VisitInsn(IOpcodes.Ixor);
        }

        /// <summary>
        ///     Generates the instruction to increment the given local variable.
        /// </summary>
        /// <param name="local"> the local variable to be incremented. </param>
        /// <param name="amount"> the amount by which the local variable must be incremented. </param>
        public virtual void Iinc(int local, int amount)
        {
            mv.VisitIincInsn(local, amount);
        }

        /// <summary>
        ///     Generates the instructions to cast a numerical value from one type to another.
        /// </summary>
        /// <param name="from"> the type of the top stack value </param>
        /// <param name="to"> the type into which this value must be cast. </param>
        public virtual void Cast(JType from, JType to)
        {
            if (from != to)
            {
                if (from.Sort < JType.Boolean || from.Sort > JType.Double || to.Sort < JType.Boolean ||
                    to.Sort > JType.Double) throw new ArgumentException("Cannot cast from " + @from + " to " + to);
                InstructionAdapter.Cast(mv, from, to);
            }
        }

        // -----------------------------------------------------------------------------------------------
        // Instructions to do boxing and unboxing operations
        // -----------------------------------------------------------------------------------------------

        private static JType GetBoxedType(JType type)
        {
            switch (type.Sort)
            {
                case JType.Byte:
                    return BYTE_TYPE;
                case JType.Boolean:
                    return BOOLEAN_TYPE;
                case JType.Short:
                    return SHORT_TYPE;
                case JType.Char:
                    return CHARACTER_TYPE;
                case JType.Int:
                    return INTEGER_TYPE;
                case JType.Float:
                    return FLOAT_TYPE;
                case JType.Long:
                    return LONG_TYPE;
                case JType.Double:
                    return DOUBLE_TYPE;
                default:
                    return type;
            }
        }

        /// <summary>
        ///     Generates the instructions to box the top stack value. This value is replaced by its boxed
        ///     equivalent on top of the stack.
        /// </summary>
        /// <param name="type"> the type of the top stack value. </param>
        public virtual void Box(JType type)
        {
            if (type.Sort == JType.Object || type.Sort == JType.Array) return;
            if (type == JType.VoidType)
            {
                Push((string)null);
            }
            else
            {
                var boxedType = GetBoxedType(type);
                NewInstance(boxedType);
                if (type.Size == 2)
                {
                    // Pp -> Ppo -> oPpo -> ooPpo -> ooPp -> o
                    DupX2();
                    DupX2();
                    Pop();
                }
                else
                {
                    // p -> po -> opo -> oop -> o
                    DupX1();
                    Swap();
                }

                InvokeConstructor(boxedType, new Method("<init>", JType.VoidType, new[] { type }));
            }
        }

        /// <summary>
        ///     Generates the instructions to box the top stack value using Java 5's valueOf() method. This
        ///     value is replaced by its boxed equivalent on top of the stack.
        /// </summary>
        /// <param name="type"> the type of the top stack value. </param>
        public virtual void ValueOf(JType type)
        {
            if (type.Sort == JType.Object || type.Sort == JType.Array) return;
            if (type == JType.VoidType)
            {
                Push((string)null);
            }
            else
            {
                var boxedType = GetBoxedType(type);
                InvokeStatic(boxedType, new Method("valueOf", boxedType, new[] { type }));
            }
        }

        /// <summary>
        ///     Generates the instructions to unbox the top stack value. This value is replaced by its unboxed
        ///     equivalent on top of the stack.
        /// </summary>
        /// <param name="type"> the type of the top stack value. </param>
        public virtual void Unbox(JType type)
        {
            var boxedType = NUMBER_TYPE;
            Method unboxMethod;
            switch (type.Sort)
            {
                case JType.Void:
                    return;
                case JType.Char:
                    boxedType = CHARACTER_TYPE;
                    unboxMethod = CHAR_VALUE;
                    break;
                case JType.Boolean:
                    boxedType = BOOLEAN_TYPE;
                    unboxMethod = BOOLEAN_VALUE;
                    break;
                case JType.Double:
                    unboxMethod = DOUBLE_VALUE;
                    break;
                case JType.Float:
                    unboxMethod = FLOAT_VALUE;
                    break;
                case JType.Long:
                    unboxMethod = LONG_VALUE;
                    break;
                case JType.Int:
                case JType.Short:
                case JType.Byte:
                    unboxMethod = INT_VALUE;
                    break;
                default:
                    unboxMethod = null;
                    break;
            }

            if (unboxMethod == null)
            {
                CheckCast(type);
            }
            else
            {
                CheckCast(boxedType);
                InvokeVirtual(boxedType, unboxMethod);
            }
        }

        // -----------------------------------------------------------------------------------------------
        // Instructions to jump to other instructions
        // -----------------------------------------------------------------------------------------------

        /// <summary>
        ///     Constructs a new <seealso cref="Label" />.
        /// </summary>
        /// <returns> a new <seealso cref="Label" />. </returns>
        public virtual Label NewLabel()
        {
            return new Label();
        }

        /// <summary>
        ///     Marks the current code position with the given label.
        /// </summary>
        /// <param name="label"> a label. </param>
        public virtual void Mark(Label label)
        {
            mv.VisitLabel(label);
        }

        /// <summary>
        ///     Marks the current code position with a new label.
        /// </summary>
        /// <returns> the label that was created to mark the current code position. </returns>
        public virtual Label Mark()
        {
            var label = new Label();
            mv.VisitLabel(label);
            return label;
        }

        /// <summary>
        ///     Generates the instructions to jump to a label based on the comparison of the top two stack
        ///     values.
        /// </summary>
        /// <param name="type"> the type of the top two stack values. </param>
        /// <param name="mode"> how these values must be compared. One of EQ, NE, LT, GE, GT, LE. </param>
        /// <param name="label"> where to jump if the comparison result is {@literal true}. </param>
        public virtual void IfCmp(JType type, int mode, Label label)
        {
            switch (type.Sort)
            {
                case JType.Long:
                    mv.VisitInsn(IOpcodes.Lcmp);
                    break;
                case JType.Double:
                    mv.VisitInsn(mode == Ge || mode == Gt ? IOpcodes.Dcmpl : IOpcodes.Dcmpg);
                    break;
                case JType.Float:
                    mv.VisitInsn(mode == Ge || mode == Gt ? IOpcodes.Fcmpl : IOpcodes.Fcmpg);
                    break;
                case JType.Array:
                case JType.Object:
                    if (mode == Eq)
                    {
                        mv.VisitJumpInsn(IOpcodes.If_Acmpeq, label);
                        return;
                    }
                    else if (mode == Ne)
                    {
                        mv.VisitJumpInsn(IOpcodes.If_Acmpne, label);
                        return;
                    }
                    else
                    {
                        throw new ArgumentException("Bad comparison for type " + type);
                    }
                default:
                    var intOp = -1;
                    switch (mode)
                    {
                        case Eq:
                            intOp = IOpcodes.If_Icmpeq;
                            break;
                        case Ne:
                            intOp = IOpcodes.If_Icmpne;
                            break;
                        case Ge:
                            intOp = IOpcodes.If_Icmpge;
                            break;
                        case Lt:
                            intOp = IOpcodes.If_Icmplt;
                            break;
                        case Le:
                            intOp = IOpcodes.If_Icmple;
                            break;
                        case Gt:
                            intOp = IOpcodes.If_Icmpgt;
                            break;
                        default:
                            throw new ArgumentException("Bad comparison mode " + mode);
                    }

                    mv.VisitJumpInsn(intOp, label);
                    return;
            }

            mv.VisitJumpInsn(mode, label);
        }

        /// <summary>
        ///     Generates the instructions to jump to a label based on the comparison of the top two integer
        ///     stack values.
        /// </summary>
        /// <param name="mode"> how these values must be compared. One of EQ, NE, LT, GE, GT, LE. </param>
        /// <param name="label"> where to jump if the comparison result is {@literal true}. </param>
        public virtual void IfICmp(int mode, Label label)
        {
            IfCmp(JType.IntType, mode, label);
        }

        /// <summary>
        ///     Generates the instructions to jump to a label based on the comparison of the top integer stack
        ///     value with zero.
        /// </summary>
        /// <param name="mode"> how these values must be compared. One of EQ, NE, LT, GE, GT, LE. </param>
        /// <param name="label"> where to jump if the comparison result is {@literal true}. </param>
        public virtual void IfZCmp(int mode, Label label)
        {
            mv.VisitJumpInsn(mode, label);
        }

        /// <summary>
        ///     Generates the instruction to jump to the given label if the top stack value is null.
        /// </summary>
        /// <param name="label"> where to jump if the condition is {@literal true}. </param>
        public virtual void IfNull(Label label)
        {
            mv.VisitJumpInsn(IOpcodes.Ifnull, label);
        }

        /// <summary>
        ///     Generates the instruction to jump to the given label if the top stack value is not null.
        /// </summary>
        /// <param name="label"> where to jump if the condition is {@literal true}. </param>
        public virtual void IfNonNull(Label label)
        {
            mv.VisitJumpInsn(IOpcodes.Ifnonnull, label);
        }

        /// <summary>
        ///     Generates the instruction to jump to the given label.
        /// </summary>
        /// <param name="label"> where to jump if the condition is {@literal true}. </param>
        public virtual void GoTo(Label label)
        {
            mv.VisitJumpInsn(IOpcodes.Goto, label);
        }

        /// <summary>
        ///     Generates a RET instruction.
        /// </summary>
        /// <param name="local">
        ///     a local variable identifier, as returned by {@link
        ///     LocalVariablesSorter#newLocal(Type)}.
        /// </param>
        public virtual void Ret(int local)
        {
            mv.VisitVarInsn(IOpcodes.Ret, local);
        }

        /// <summary>
        ///     Generates the instructions for a switch statement.
        /// </summary>
        /// <param name="keys"> the switch case keys. </param>
        /// <param name="generator"> a generator to generate the code for the switch cases. </param>
        public virtual void TableSwitch(int[] keys, ITableSwitchGenerator generator)
        {
            float density;
            if (keys.Length == 0)
                density = 0;
            else
                density = (float)keys.Length / (keys[keys.Length - 1] - keys[0] + 1);
            TableSwitch(keys, generator, density >= 0.5f);
        }

        /// <summary>
        ///     Generates the instructions for a switch statement.
        /// </summary>
        /// <param name="keys"> the switch case keys. </param>
        /// <param name="generator"> a generator to generate the code for the switch cases. </param>
        /// <param name="useTable">
        ///     {@literal true} to use a TABLESWITCH instruction, or {@literal false} to use a
        ///     LOOKUPSWITCH instruction.
        /// </param>
        public virtual void TableSwitch(int[] keys, ITableSwitchGenerator generator, bool useTable)
        {
            for (var i = 1; i < keys.Length; ++i)
                if (keys[i] < keys[i - 1])
                    throw new ArgumentException("keys must be sorted in ascending order");
            var defaultLabel = NewLabel();
            var endLabel = NewLabel();
            if (keys.Length > 0)
            {
                var numKeys = keys.Length;
                if (useTable)
                {
                    var min = keys[0];
                    var max = keys[numKeys - 1];
                    var range = max - min + 1;
                    var labels = new Label[range];
                    Arrays.Fill(labels, defaultLabel);
                    for (var i = 0; i < numKeys; ++i) labels[keys[i] - min] = NewLabel();
                    mv.VisitTableSwitchInsn(min, max, defaultLabel, labels);
                    for (var i = 0; i < range; ++i)
                    {
                        var label = labels[i];
                        if (label != defaultLabel)
                        {
                            Mark(label);
                            generator.GenerateCase(i + min, endLabel);
                        }
                    }
                }
                else
                {
                    var labels = new Label[numKeys];
                    for (var i = 0; i < numKeys; ++i) labels[i] = NewLabel();
                    mv.VisitLookupSwitchInsn(defaultLabel, keys, labels);
                    for (var i = 0; i < numKeys; ++i)
                    {
                        Mark(labels[i]);
                        generator.GenerateCase(keys[i], endLabel);
                    }
                }
            }

            Mark(defaultLabel);
            generator.GenerateDefault();
            Mark(endLabel);
        }

        /// <summary>
        ///     Generates the instruction to return the top stack value to the caller.
        /// </summary>
        public virtual void ReturnValue()
        {
            mv.VisitInsn(_returnType.GetOpcode(IOpcodes.Ireturn));
        }

        // -----------------------------------------------------------------------------------------------
        // Instructions to load and store fields
        // -----------------------------------------------------------------------------------------------

        /// <summary>
        ///     Generates a get field or set field instruction.
        /// </summary>
        /// <param name="opcode"> the instruction's opcode. </param>
        /// <param name="ownerType"> the class in which the field is defined. </param>
        /// <param name="name"> the name of the field. </param>
        /// <param name="fieldType"> the type of the field. </param>
        private void FieldInsn(int opcode, JType ownerType, string name, JType fieldType)
        {
            mv.VisitFieldInsn(opcode, ownerType.InternalName, name, fieldType.Descriptor);
        }

        /// <summary>
        ///     Generates the instruction to push the value of a static field on the stack.
        /// </summary>
        /// <param name="owner"> the class in which the field is defined. </param>
        /// <param name="name"> the name of the field. </param>
        /// <param name="type"> the type of the field. </param>
        public virtual void GetStatic(JType owner, string name, JType type)
        {
            FieldInsn(IOpcodes.Getstatic, owner, name, type);
        }

        /// <summary>
        ///     Generates the instruction to store the top stack value in a static field.
        /// </summary>
        /// <param name="owner"> the class in which the field is defined. </param>
        /// <param name="name"> the name of the field. </param>
        /// <param name="type"> the type of the field. </param>
        public virtual void PutStatic(JType owner, string name, JType type)
        {
            FieldInsn(IOpcodes.Putstatic, owner, name, type);
        }

        /// <summary>
        ///     Generates the instruction to push the value of a non static field on the stack.
        /// </summary>
        /// <param name="owner"> the class in which the field is defined. </param>
        /// <param name="name"> the name of the field. </param>
        /// <param name="type"> the type of the field. </param>
        public virtual void GetField(JType owner, string name, JType type)
        {
            FieldInsn(IOpcodes.Getfield, owner, name, type);
        }

        /// <summary>
        ///     Generates the instruction to store the top stack value in a non static field.
        /// </summary>
        /// <param name="owner"> the class in which the field is defined. </param>
        /// <param name="name"> the name of the field. </param>
        /// <param name="type"> the type of the field. </param>
        public virtual void PutField(JType owner, string name, JType type)
        {
            FieldInsn(IOpcodes.Putfield, owner, name, type);
        }

        // -----------------------------------------------------------------------------------------------
        // Instructions to invoke methods
        // -----------------------------------------------------------------------------------------------

        /// <summary>
        ///     Generates an invoke method instruction.
        /// </summary>
        /// <param name="opcode"> the instruction's opcode. </param>
        /// <param name="type"> the class in which the method is defined. </param>
        /// <param name="method"> the method to be invoked. </param>
        /// <param name="isInterface"> whether the 'type' class is an interface or not. </param>
        private void InvokeInsn(int opcode, JType type, Method method, bool isInterface)
        {
            var owner = type.Sort == JType.Array ? type.Descriptor : type.InternalName;
            mv.VisitMethodInsn(opcode, owner, method.Name, method.Descriptor, isInterface);
        }

        /// <summary>
        ///     Generates the instruction to invoke a normal method.
        /// </summary>
        /// <param name="owner"> the class in which the method is defined. </param>
        /// <param name="method"> the method to be invoked. </param>
        public virtual void InvokeVirtual(JType owner, Method method)
        {
            InvokeInsn(IOpcodes.Invokevirtual, owner, method, false);
        }

        /// <summary>
        ///     Generates the instruction to invoke a constructor.
        /// </summary>
        /// <param name="type"> the class in which the constructor is defined. </param>
        /// <param name="method"> the constructor to be invoked. </param>
        public virtual void InvokeConstructor(JType type, Method method)
        {
            InvokeInsn(IOpcodes.Invokespecial, type, method, false);
        }

        /// <summary>
        ///     Generates the instruction to invoke a static method.
        /// </summary>
        /// <param name="owner"> the class in which the method is defined. </param>
        /// <param name="method"> the method to be invoked. </param>
        public virtual void InvokeStatic(JType owner, Method method)
        {
            InvokeInsn(IOpcodes.Invokestatic, owner, method, false);
        }

        /// <summary>
        ///     Generates the instruction to invoke an interface method.
        /// </summary>
        /// <param name="owner"> the class in which the method is defined. </param>
        /// <param name="method"> the method to be invoked. </param>
        public virtual void InvokeInterface(JType owner, Method method)
        {
            InvokeInsn(IOpcodes.Invokeinterface, owner, method, true);
        }

        /// <summary>
        ///     Generates an invokedynamic instruction.
        /// </summary>
        /// <param name="name"> the method's name. </param>
        /// <param name="descriptor"> the method's descriptor (see <seealso cref="Type" />). </param>
        /// <param name="bootstrapMethodHandle"> the bootstrap method. </param>
        /// <param name="bootstrapMethodArguments">
        ///     the bootstrap method constant arguments. Each argument must be
        ///     an <seealso cref="Integer" />, <seealso cref="Float" />, <seealso cref="Long" />, <seealso cref="Double" />,
        ///     <seealso cref="string" />, {@link
        ///     Type} or <seealso cref="Handle" /> value. This method is allowed to modify the content of the array so
        ///     a caller should expect that this array may change.
        /// </param>
        public virtual void InvokeDynamic(string name, string descriptor, Handle bootstrapMethodHandle,
            params object[] bootstrapMethodArguments)
        {
            mv.VisitInvokeDynamicInsn(name, descriptor, bootstrapMethodHandle, bootstrapMethodArguments);
        }

        // -----------------------------------------------------------------------------------------------
        // Instructions to create objects and arrays
        // -----------------------------------------------------------------------------------------------

        /// <summary>
        ///     Generates a type dependent instruction.
        /// </summary>
        /// <param name="opcode"> the instruction's opcode. </param>
        /// <param name="type"> the instruction's operand. </param>
        private void TypeInsn(int opcode, JType type)
        {
            mv.VisitTypeInsn(opcode, type.InternalName);
        }

        /// <summary>
        ///     Generates the instruction to create a new object.
        /// </summary>
        /// <param name="type"> the class of the object to be created. </param>
        public virtual void NewInstance(JType type)
        {
            TypeInsn(IOpcodes.New, type);
        }

        /// <summary>
        ///     Generates the instruction to create a new array.
        /// </summary>
        /// <param name="type"> the type of the array elements. </param>
        public virtual void NewArray(JType type)
        {
            InstructionAdapter.Newarray(mv, type);
        }

        // -----------------------------------------------------------------------------------------------
        // Miscellaneous instructions
        // -----------------------------------------------------------------------------------------------

        /// <summary>
        ///     Generates the instruction to compute the length of an array.
        /// </summary>
        public virtual void ArrayLength()
        {
            mv.VisitInsn(IOpcodes.Arraylength);
        }

        /// <summary>
        ///     Generates the instruction to throw an exception.
        /// </summary>
        public virtual void ThrowException()
        {
            mv.VisitInsn(IOpcodes.Athrow);
        }

        /// <summary>
        ///     Generates the instructions to create and throw an exception. The exception class must have a
        ///     constructor with a single String argument.
        /// </summary>
        /// <param name="type"> the class of the exception to be thrown. </param>
        /// <param name="message"> the detailed message of the exception. </param>
        public virtual void ThrowException(JType type, string message)
        {
            NewInstance(type);
            Dup();
            Push(message);
            InvokeConstructor(type, Method.GetMethod("void <init> (String)"));
            ThrowException();
        }

        /// <summary>
        ///     Generates the instruction to check that the top stack value is of the given type.
        /// </summary>
        /// <param name="type"> a class or interface type. </param>
        public virtual void CheckCast(JType type)
        {
            if (!type.Equals(OBJECT_TYPE)) TypeInsn(IOpcodes.Checkcast, type);
        }

        /// <summary>
        ///     Generates the instruction to test if the top stack value is of the given type.
        /// </summary>
        /// <param name="type"> a class or interface type. </param>
        public virtual void InstanceOf(JType type)
        {
            TypeInsn(IOpcodes.Instanceof, type);
        }

        /// <summary>
        ///     Generates the instruction to get the monitor of the top stack value.
        /// </summary>
        public virtual void MonitorEnter()
        {
            mv.VisitInsn(IOpcodes.Monitorenter);
        }

        /// <summary>
        ///     Generates the instruction to release the monitor of the top stack value.
        /// </summary>
        public virtual void MonitorExit()
        {
            mv.VisitInsn(IOpcodes.Monitorexit);
        }

        // -----------------------------------------------------------------------------------------------
        // Non instructions
        // -----------------------------------------------------------------------------------------------

        /// <summary>
        ///     Marks the end of the visited method.
        /// </summary>
        public virtual void EndMethod()
        {
            if ((_access & IOpcodes.Acc_Abstract) == 0) mv.VisitMaxs(0, 0);
            mv.VisitEnd();
        }

        /// <summary>
        ///     Marks the start of an exception handler.
        /// </summary>
        /// <param name="start"> beginning of the exception handler's scope (inclusive). </param>
        /// <param name="end"> end of the exception handler's scope (exclusive). </param>
        /// <param name="exception"> internal name of the type of exceptions handled by the handler. </param>
        public virtual void CatchException(Label start, Label end, JType exception)
        {
            var catchLabel = new Label();
            if (exception == null)
                mv.VisitTryCatchBlock(start, end, catchLabel, null);
            else
                mv.VisitTryCatchBlock(start, end, catchLabel, exception.InternalName);
            Mark(catchLabel);
        }
    }
}