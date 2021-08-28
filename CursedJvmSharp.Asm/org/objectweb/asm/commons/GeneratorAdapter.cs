using org.objectweb.asm;
using CursedJvmSharp.Asm.Java.IO;
using System.Collections.Generic;
using ClassVisitor = org.objectweb.asm.ClassVisitor;
using ConstantDynamic = org.objectweb.asm.ConstantDynamic;
using Handle = org.objectweb.asm.Handle;
using Label = org.objectweb.asm.Label;
using MethodVisitor = org.objectweb.asm.MethodVisitor;
using Opcodes = org.objectweb.asm.Opcodes;

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
namespace org.objectweb.asm.commons
{

	/// <summary>
	/// A <seealso cref="MethodVisitor"/> with convenient methods to generate code. For example, using this
	/// adapter, the class below
	/// 
	/// <pre>
	/// public class Example {
	///   public static void main(String[] args) {
	///     System.out.println(&quot;Hello world!&quot;);
	///   }
	/// }
	/// </pre>
	/// 
	/// <para>can be generated as follows:
	/// 
	/// <pre>
	/// ClassWriter cw = new ClassWriter(0);
	/// cw.visit(V1_1, ACC_PUBLIC, &quot;Example&quot;, null, &quot;java/lang/Object&quot;, null);
	/// 
	/// Method m = Method.getMethod(&quot;void &lt;init&gt; ()&quot;);
	/// GeneratorAdapter mg = new GeneratorAdapter(ACC_PUBLIC, m, null, null, cw);
	/// mg.loadThis();
	/// mg.invokeConstructor(Type.getType(Object.class), m);
	/// mg.returnValue();
	/// mg.endMethod();
	/// 
	/// m = Method.getMethod(&quot;void main (String[])&quot;);
	/// mg = new GeneratorAdapter(ACC_PUBLIC + ACC_STATIC, m, null, null, cw);
	/// mg.getStatic(Type.getType(System.class), &quot;out&quot;, Type.getType(PrintStream.class));
	/// mg.push(&quot;Hello world!&quot;);
	/// mg.invokeVirtual(Type.getType(PrintStream.class),
	///         Method.getMethod(&quot;void println (String)&quot;));
	/// mg.returnValue();
	/// mg.endMethod();
	/// 
	/// cw.visitEnd();
	/// </pre>
	/// 
	/// @author Juozas Baliuka
	/// @author Chris Nokleberg
	/// @author Eric Bruneton
	/// @author Prashant Deva
	/// </para>
	/// </summary>
	public class GeneratorAdapter : LocalVariablesSorter
	{

	  private const string CLASS_DESCRIPTOR = "Ljava/lang/Class;";

	  private static readonly org.objectweb.asm.JType BYTE_TYPE = org.objectweb.asm.JType.getObjectType("java/lang/Byte");

	  private static readonly org.objectweb.asm.JType BOOLEAN_TYPE = org.objectweb.asm.JType.getObjectType("java/lang/Boolean");

	  private static readonly org.objectweb.asm.JType SHORT_TYPE = org.objectweb.asm.JType.getObjectType("java/lang/Short");

	  private static readonly org.objectweb.asm.JType CHARACTER_TYPE = org.objectweb.asm.JType.getObjectType("java/lang/Character");

	  private static readonly org.objectweb.asm.JType INTEGER_TYPE = org.objectweb.asm.JType.getObjectType("java/lang/Integer");

	  private static readonly org.objectweb.asm.JType FLOAT_TYPE = org.objectweb.asm.JType.getObjectType("java/lang/Float");

	  private static readonly org.objectweb.asm.JType LONG_TYPE = org.objectweb.asm.JType.getObjectType("java/lang/Long");

	  private static readonly org.objectweb.asm.JType DOUBLE_TYPE = org.objectweb.asm.JType.getObjectType("java/lang/Double");

	  private static readonly org.objectweb.asm.JType NUMBER_TYPE = org.objectweb.asm.JType.getObjectType("java/lang/Number");

	  private static readonly org.objectweb.asm.JType OBJECT_TYPE = org.objectweb.asm.JType.getObjectType("java/lang/Object");

	  private static readonly Method BOOLEAN_VALUE = Method.getMethod("boolean booleanValue()");

	  private static readonly Method CHAR_VALUE = Method.getMethod("char charValue()");

	  private static readonly Method INT_VALUE = Method.getMethod("int intValue()");

	  private static readonly Method FLOAT_VALUE = Method.getMethod("float floatValue()");

	  private static readonly Method LONG_VALUE = Method.getMethod("long longValue()");

	  private static readonly Method DOUBLE_VALUE = Method.getMethod("double doubleValue()");

	  /// <summary>
	  /// Constant for the <seealso cref="math"/> method. </summary>
	  public static readonly int ADD = Opcodes.IADD;

	  /// <summary>
	  /// Constant for the <seealso cref="math"/> method. </summary>
	  public static readonly int SUB = Opcodes.ISUB;

	  /// <summary>
	  /// Constant for the <seealso cref="math"/> method. </summary>
	  public static readonly int MUL = Opcodes.IMUL;

	  /// <summary>
	  /// Constant for the <seealso cref="math"/> method. </summary>
	  public static readonly int DIV = Opcodes.IDIV;

	  /// <summary>
	  /// Constant for the <seealso cref="math"/> method. </summary>
	  public static readonly int REM = Opcodes.IREM;

	  /// <summary>
	  /// Constant for the <seealso cref="math"/> method. </summary>
	  public static readonly int NEG = Opcodes.INEG;

	  /// <summary>
	  /// Constant for the <seealso cref="math"/> method. </summary>
	  public static readonly int SHL = Opcodes.ISHL;

	  /// <summary>
	  /// Constant for the <seealso cref="math"/> method. </summary>
	  public static readonly int SHR = Opcodes.ISHR;

	  /// <summary>
	  /// Constant for the <seealso cref="math"/> method. </summary>
	  public static readonly int USHR = Opcodes.IUSHR;

	  /// <summary>
	  /// Constant for the <seealso cref="math"/> method. </summary>
	  public static readonly int AND = Opcodes.IAND;

	  /// <summary>
	  /// Constant for the <seealso cref="math"/> method. </summary>
	  public static readonly int OR = Opcodes.IOR;

	  /// <summary>
	  /// Constant for the <seealso cref="math"/> method. </summary>
	  public static readonly int XOR = Opcodes.IXOR;

	  /// <summary>
	  /// Constant for the <seealso cref="ifCmp"/> method. </summary>
	  public static readonly int EQ = Opcodes.IFEQ;

	  /// <summary>
	  /// Constant for the <seealso cref="ifCmp"/> method. </summary>
	  public static readonly int NE = Opcodes.IFNE;

	  /// <summary>
	  /// Constant for the <seealso cref="ifCmp"/> method. </summary>
	  public static readonly int LT = Opcodes.IFLT;

	  /// <summary>
	  /// Constant for the <seealso cref="ifCmp"/> method. </summary>
	  public static readonly int GE = Opcodes.IFGE;

	  /// <summary>
	  /// Constant for the <seealso cref="ifCmp"/> method. </summary>
	  public static readonly int GT = Opcodes.IFGT;

	  /// <summary>
	  /// Constant for the <seealso cref="ifCmp"/> method. </summary>
	  public static readonly int LE = Opcodes.IFLE;

	  /// <summary>
	  /// The access flags of the visited method. </summary>
	  private readonly int access;

	  /// <summary>
	  /// The name of the visited method. </summary>
	  private readonly string name;

	  /// <summary>
	  /// The return type of the visited method. </summary>
	  private readonly org.objectweb.asm.JType returnType;

	  /// <summary>
	  /// The argument types of the visited method. </summary>
	  private readonly org.objectweb.asm.JType[] argumentTypes;

	  /// <summary>
	  /// The types of the local variables of the visited method. </summary>
	  private readonly IList<org.objectweb.asm.JType> localTypes = new List<org.objectweb.asm.JType>();

	  /// <summary>
	  /// Constructs a new <seealso cref="GeneratorAdapter"/>. <i>Subclasses must not use this constructor</i>.
	  /// Instead, they must use the <seealso cref="GeneratorAdapter(int, MethodVisitor, int, String, String)"/>
	  /// version.
	  /// </summary>
	  /// <param name="methodVisitor"> the method visitor to which this adapter delegates calls. </param>
	  /// <param name="access"> the method's access flags (see <seealso cref="Opcodes"/>). </param>
	  /// <param name="name"> the method's name. </param>
	  /// <param name="descriptor"> the method's descriptor (see <seealso cref="Type"/>). </param>
	  /// <exception cref="IllegalStateException"> if a subclass calls this constructor. </exception>
	  public GeneratorAdapter(MethodVisitor methodVisitor, int access, string name, string descriptor) : this(Opcodes.ASM9, methodVisitor, access, name, descriptor)
	  {
		if (this.GetType() != typeof(GeneratorAdapter))
		{
		  throw new System.InvalidOperationException();
		}
	  }

	  /// <summary>
	  /// Constructs a new <seealso cref="GeneratorAdapter"/>.
	  /// </summary>
	  /// <param name="api"> the ASM API version implemented by this visitor. Must be one of the {@code
	  ///     ASM}<i>x</i> values in <seealso cref="Opcodes"/>. </param>
	  /// <param name="methodVisitor"> the method visitor to which this adapter delegates calls. </param>
	  /// <param name="access"> the method's access flags (see <seealso cref="Opcodes"/>). </param>
	  /// <param name="name"> the method's name. </param>
	  /// <param name="descriptor"> the method's descriptor (see <seealso cref="Type"/>). </param>
	  public GeneratorAdapter(int api, MethodVisitor methodVisitor, int access, string name, string descriptor) : base(api, access, descriptor, methodVisitor)
	  {
		this.access = access;
		this.name = name;
		this.returnType = org.objectweb.asm.JType.getReturnType(descriptor);
		this.argumentTypes = org.objectweb.asm.JType.getArgumentTypes(descriptor);
	  }

	  /// <summary>
	  /// Constructs a new <seealso cref="GeneratorAdapter"/>. <i>Subclasses must not use this constructor</i>.
	  /// Instead, they must use the <seealso cref="GeneratorAdapter(int, MethodVisitor, int, String, String)"/>
	  /// version.
	  /// </summary>
	  /// <param name="access"> access flags of the adapted method. </param>
	  /// <param name="method"> the adapted method. </param>
	  /// <param name="methodVisitor"> the method visitor to which this adapter delegates calls. </param>
	  public GeneratorAdapter(int access, Method method, MethodVisitor methodVisitor) : this(methodVisitor, access, method.Name, method.Descriptor)
	  {
	  }

	  /// <summary>
	  /// Constructs a new <seealso cref="GeneratorAdapter"/>. <i>Subclasses must not use this constructor</i>.
	  /// Instead, they must use the <seealso cref="GeneratorAdapter(int, MethodVisitor, int, String, String)"/>
	  /// version.
	  /// </summary>
	  /// <param name="access"> access flags of the adapted method. </param>
	  /// <param name="method"> the adapted method. </param>
	  /// <param name="signature"> the signature of the adapted method (may be {@literal null}). </param>
	  /// <param name="exceptions"> the exceptions thrown by the adapted method (may be {@literal null}). </param>
	  /// <param name="classVisitor"> the class visitor to which this adapter delegates calls. </param>
	  public GeneratorAdapter(int access, Method method, string signature, org.objectweb.asm.JType[] exceptions, ClassVisitor classVisitor) : this(access, method, classVisitor.visitMethod(access, method.Name, method.Descriptor, signature, exceptions == null ? null : getInternalNames(exceptions)))
	  {
	  }

	  /// <summary>
	  /// Returns the internal names of the given types.
	  /// </summary>
	  /// <param name="types"> a set of types. </param>
	  /// <returns> the internal names of the given types. </returns>
	  private static string[] getInternalNames(org.objectweb.asm.JType[] types)
	  {
		string[] names = new string[types.Length];
		for (int i = 0; i < names.Length; ++i)
		{
		  names[i] = types[i].getInternalName();
		}
		return names;
	  }

	  public virtual int Access
	  {
		  get
		  {
			return access;
		  }
	  }

	  public virtual string Name
	  {
		  get
		  {
			return name;
		  }
	  }

	  public virtual org.objectweb.asm.JType ReturnType
	  {
		  get
		  {
			return returnType;
		  }
	  }

	  public virtual org.objectweb.asm.JType[] ArgumentTypes
	  {
		  get
		  {
			return (org.objectweb.asm.JType[])argumentTypes.Clone();
		  }
	  }

	  // -----------------------------------------------------------------------------------------------
	  // Instructions to push constants on the stack
	  // -----------------------------------------------------------------------------------------------

	  /// <summary>
	  /// Generates the instruction to push the given value on the stack.
	  /// </summary>
	  /// <param name="value"> the value to be pushed on the stack. </param>
	  public virtual void push(bool value)
	  {
		push(value ? 1 : 0);
	  }

	  /// <summary>
	  /// Generates the instruction to push the given value on the stack.
	  /// </summary>
	  /// <param name="value"> the value to be pushed on the stack. </param>
	  public virtual void push(int value)
	  {
		if (value >= -1 && value <= 5)
		{
		  mv.visitInsn(Opcodes.ICONST_0 + value);
		}
		else if (value >= sbyte.MinValue && value <= sbyte.MaxValue)
		{
		  mv.visitIntInsn(Opcodes.BIPUSH, value);
		}
		else if (value >= short.MinValue && value <= short.MaxValue)
		{
		  mv.visitIntInsn(Opcodes.SIPUSH, value);
		}
		else
		{
		  mv.visitLdcInsn(value);
		}
	  }

	  /// <summary>
	  /// Generates the instruction to push the given value on the stack.
	  /// </summary>
	  /// <param name="value"> the value to be pushed on the stack. </param>
	  public virtual void push(long value)
	  {
		if (value == 0L || value == 1L)
		{
		  mv.visitInsn(Opcodes.LCONST_0 + (int) value);
		}
		else
		{
		  mv.visitLdcInsn(value);
		}
	  }

	  /// <summary>
	  /// Generates the instruction to push the given value on the stack.
	  /// </summary>
	  /// <param name="value"> the value to be pushed on the stack. </param>
	  public virtual void push(float value)
	  {
		int bits = Float.floatToIntBits(value);
		if (bits == 0L || bits == 0x3F800000 || bits == 0x40000000)
		{ // 0..2
		  mv.visitInsn(Opcodes.FCONST_0 + (int) value);
		}
		else
		{
		  mv.visitLdcInsn(value);
		}
	  }

	  /// <summary>
	  /// Generates the instruction to push the given value on the stack.
	  /// </summary>
	  /// <param name="value"> the value to be pushed on the stack. </param>
	  public virtual void push(double value)
	  {
		long bits = System.BitConverter.DoubleToInt64Bits(value);
		if (bits == 0L || bits == 0x3FF0000000000000L)
		{ // +0.0d and 1.0d
		  mv.visitInsn(Opcodes.DCONST_0 + (int) value);
		}
		else
		{
		  mv.visitLdcInsn(value);
		}
	  }

	  /// <summary>
	  /// Generates the instruction to push the given value on the stack.
	  /// </summary>
	  /// <param name="value"> the value to be pushed on the stack. May be {@literal null}. </param>
	  public virtual void push(string value)
	  {
		if (string.ReferenceEquals(value, null))
		{
		  mv.visitInsn(Opcodes.ACONST_NULL);
		}
		else
		{
		  mv.visitLdcInsn(value);
		}
	  }

	  /// <summary>
	  /// Generates the instruction to push the given value on the stack.
	  /// </summary>
	  /// <param name="value"> the value to be pushed on the stack. </param>
	  public virtual void push(org.objectweb.asm.JType value)
	  {
		if (value == null)
		{
		  mv.visitInsn(Opcodes.ACONST_NULL);
		}
		else
		{
		  switch (value.getSort())
		  {
			case org.objectweb.asm.JType.BOOLEAN:
			  mv.visitFieldInsn(Opcodes.GETSTATIC, "java/lang/Boolean", "TYPE", CLASS_DESCRIPTOR);
			  break;
			case org.objectweb.asm.JType.CHAR:
			  mv.visitFieldInsn(Opcodes.GETSTATIC, "java/lang/Character", "TYPE", CLASS_DESCRIPTOR);
			  break;
			case org.objectweb.asm.JType.BYTE:
			  mv.visitFieldInsn(Opcodes.GETSTATIC, "java/lang/Byte", "TYPE", CLASS_DESCRIPTOR);
			  break;
			case org.objectweb.asm.JType.SHORT:
			  mv.visitFieldInsn(Opcodes.GETSTATIC, "java/lang/Short", "TYPE", CLASS_DESCRIPTOR);
			  break;
			case org.objectweb.asm.JType.INT:
			  mv.visitFieldInsn(Opcodes.GETSTATIC, "java/lang/Integer", "TYPE", CLASS_DESCRIPTOR);
			  break;
			case org.objectweb.asm.JType.FLOAT:
			  mv.visitFieldInsn(Opcodes.GETSTATIC, "java/lang/Float", "TYPE", CLASS_DESCRIPTOR);
			  break;
			case org.objectweb.asm.JType.LONG:
			  mv.visitFieldInsn(Opcodes.GETSTATIC, "java/lang/Long", "TYPE", CLASS_DESCRIPTOR);
			  break;
			case org.objectweb.asm.JType.DOUBLE:
			  mv.visitFieldInsn(Opcodes.GETSTATIC, "java/lang/Double", "TYPE", CLASS_DESCRIPTOR);
			  break;
			default:
			  mv.visitLdcInsn(value);
			  break;
		  }
		}
	  }

	  /// <summary>
	  /// Generates the instruction to push a handle on the stack.
	  /// </summary>
	  /// <param name="handle"> the handle to be pushed on the stack. </param>
	  public virtual void push(Handle handle)
	  {
		if (handle == null)
		{
		  mv.visitInsn(Opcodes.ACONST_NULL);
		}
		else
		{
		  mv.visitLdcInsn(handle);
		}
	  }

	  /// <summary>
	  /// Generates the instruction to push a constant dynamic on the stack.
	  /// </summary>
	  /// <param name="constantDynamic"> the constant dynamic to be pushed on the stack. </param>
	  public virtual void push(ConstantDynamic constantDynamic)
	  {
		if (constantDynamic == null)
		{
		  mv.visitInsn(Opcodes.ACONST_NULL);
		}
		else
		{
		  mv.visitLdcInsn(constantDynamic);
		}
	  }

	  // -----------------------------------------------------------------------------------------------
	  // Instructions to load and store method arguments
	  // -----------------------------------------------------------------------------------------------

	  /// <summary>
	  /// Returns the index of the given method argument in the frame's local variables array.
	  /// </summary>
	  /// <param name="arg"> the index of a method argument. </param>
	  /// <returns> the index of the given method argument in the frame's local variables array. </returns>
	  private int getArgIndex(int arg)
	  {
		int index = (access & Opcodes.ACC_STATIC) == 0 ? 1 : 0;
		for (int i = 0; i < arg; i++)
		{
		  index += argumentTypes[i].getSize();
		}
		return index;
	  }

	  /// <summary>
	  /// Generates the instruction to push a local variable on the stack.
	  /// </summary>
	  /// <param name="type"> the type of the local variable to be loaded. </param>
	  /// <param name="index"> an index in the frame's local variables array. </param>
	  private void loadInsn(org.objectweb.asm.JType type, int index)
	  {
		mv.visitVarInsn(type.getOpcode(Opcodes.ILOAD), index);
	  }

	  /// <summary>
	  /// Generates the instruction to store the top stack value in a local variable.
	  /// </summary>
	  /// <param name="type"> the type of the local variable to be stored. </param>
	  /// <param name="index"> an index in the frame's local variables array. </param>
	  private void storeInsn(org.objectweb.asm.JType type, int index)
	  {
		mv.visitVarInsn(type.getOpcode(Opcodes.ISTORE), index);
	  }

	  /// <summary>
	  /// Generates the instruction to load 'this' on the stack. </summary>
	  public virtual void loadThis()
	  {
		if ((access & Opcodes.ACC_STATIC) != 0)
		{
		  throw new System.InvalidOperationException("no 'this' pointer within static method");
		}
		mv.visitVarInsn(Opcodes.ALOAD, 0);
	  }

	  /// <summary>
	  /// Generates the instruction to load the given method argument on the stack.
	  /// </summary>
	  /// <param name="arg"> the index of a method argument. </param>
	  public virtual void loadArg(int arg)
	  {
		loadInsn(argumentTypes[arg], getArgIndex(arg));
	  }

	  /// <summary>
	  /// Generates the instructions to load the given method arguments on the stack.
	  /// </summary>
	  /// <param name="arg"> the index of the first method argument to be loaded. </param>
	  /// <param name="count"> the number of method arguments to be loaded. </param>
	  public virtual void loadArgs(int arg, int count)
	  {
		int index = getArgIndex(arg);
		for (int i = 0; i < count; ++i)
		{
		  org.objectweb.asm.JType argumentType = argumentTypes[arg + i];
		  loadInsn(argumentType, index);
		  index += argumentType.getSize();
		}
	  }

	  /// <summary>
	  /// Generates the instructions to load all the method arguments on the stack. </summary>
	  public virtual void loadArgs()
	  {
		loadArgs(0, argumentTypes.Length);
	  }

	  /// <summary>
	  /// Generates the instructions to load all the method arguments on the stack, as a single object
	  /// array.
	  /// </summary>
	  public virtual void loadArgArray()
	  {
		push(argumentTypes.Length);
		newArray(OBJECT_TYPE);
		for (int i = 0; i < argumentTypes.Length; i++)
		{
		  dup();
		  push(i);
		  loadArg(i);
		  box(argumentTypes[i]);
		  arrayStore(OBJECT_TYPE);
		}
	  }

	  /// <summary>
	  /// Generates the instruction to store the top stack value in the given method argument.
	  /// </summary>
	  /// <param name="arg"> the index of a method argument. </param>
	  public virtual void storeArg(int arg)
	  {
		storeInsn(argumentTypes[arg], getArgIndex(arg));
	  }

	  // -----------------------------------------------------------------------------------------------
	  // Instructions to load and store local variables
	  // -----------------------------------------------------------------------------------------------

	  /// <summary>
	  /// Returns the type of the given local variable.
	  /// </summary>
	  /// <param name="local"> a local variable identifier, as returned by {@link
	  ///     LocalVariablesSorter#newLocal(Type)}. </param>
	  /// <returns> the type of the given local variable. </returns>
	  public virtual org.objectweb.asm.JType getLocalType(int local)
	  {
		return localTypes.get(local - firstLocal);
	  }

	  public override void setLocalType(int local, org.objectweb.asm.JType type)
	  {
		int index = local - firstLocal;
		while (localTypes.size() < index + 1)
		{
		  localTypes.add(null);
		}
		localTypes.set(index, type);
	  }

	  /// <summary>
	  /// Generates the instruction to load the given local variable on the stack.
	  /// </summary>
	  /// <param name="local"> a local variable identifier, as returned by {@link
	  ///     LocalVariablesSorter#newLocal(Type)}. </param>
	  public virtual void loadLocal(int local)
	  {
		loadInsn(getLocalType(local), local);
	  }

	  /// <summary>
	  /// Generates the instruction to load the given local variable on the stack.
	  /// </summary>
	  /// <param name="local"> a local variable identifier, as returned by {@link
	  ///     LocalVariablesSorter#newLocal(Type)}. </param>
	  /// <param name="type"> the type of this local variable. </param>
	  public virtual void loadLocal(int local, org.objectweb.asm.JType type)
	  {
		setLocalType(local, type);
		loadInsn(type, local);
	  }

	  /// <summary>
	  /// Generates the instruction to store the top stack value in the given local variable.
	  /// </summary>
	  /// <param name="local"> a local variable identifier, as returned by {@link
	  ///     LocalVariablesSorter#newLocal(Type)}. </param>
	  public virtual void storeLocal(int local)
	  {
		storeInsn(getLocalType(local), local);
	  }

	  /// <summary>
	  /// Generates the instruction to store the top stack value in the given local variable.
	  /// </summary>
	  /// <param name="local"> a local variable identifier, as returned by {@link
	  ///     LocalVariablesSorter#newLocal(Type)}. </param>
	  /// <param name="type"> the type of this local variable. </param>
	  public virtual void storeLocal(int local, org.objectweb.asm.JType type)
	  {
		setLocalType(local, type);
		storeInsn(type, local);
	  }

	  /// <summary>
	  /// Generates the instruction to load an element from an array.
	  /// </summary>
	  /// <param name="type"> the type of the array element to be loaded. </param>
	  public virtual void arrayLoad(org.objectweb.asm.JType type)
	  {
		mv.visitInsn(type.getOpcode(Opcodes.IALOAD));
	  }

	  /// <summary>
	  /// Generates the instruction to store an element in an array.
	  /// </summary>
	  /// <param name="type"> the type of the array element to be stored. </param>
	  public virtual void arrayStore(org.objectweb.asm.JType type)
	  {
		mv.visitInsn(type.getOpcode(Opcodes.IASTORE));
	  }

	  // -----------------------------------------------------------------------------------------------
	  // Instructions to manage the stack
	  // -----------------------------------------------------------------------------------------------

	  /// <summary>
	  /// Generates a POP instruction. </summary>
	  public virtual void pop()
	  {
		mv.visitInsn(Opcodes.POP);
	  }

	  /// <summary>
	  /// Generates a POP2 instruction. </summary>
	  public virtual void pop2()
	  {
		mv.visitInsn(Opcodes.POP2);
	  }

	  /// <summary>
	  /// Generates a DUP instruction. </summary>
	  public virtual void dup()
	  {
		mv.visitInsn(Opcodes.DUP);
	  }

	  /// <summary>
	  /// Generates a DUP2 instruction. </summary>
	  public virtual void dup2()
	  {
		mv.visitInsn(Opcodes.DUP2);
	  }

	  /// <summary>
	  /// Generates a DUP_X1 instruction. </summary>
	  public virtual void dupX1()
	  {
		mv.visitInsn(Opcodes.DUP_X1);
	  }

	  /// <summary>
	  /// Generates a DUP_X2 instruction. </summary>
	  public virtual void dupX2()
	  {
		mv.visitInsn(Opcodes.DUP_X2);
	  }

	  /// <summary>
	  /// Generates a DUP2_X1 instruction. </summary>
	  public virtual void dup2X1()
	  {
		mv.visitInsn(Opcodes.DUP2_X1);
	  }

	  /// <summary>
	  /// Generates a DUP2_X2 instruction. </summary>
	  public virtual void dup2X2()
	  {
		mv.visitInsn(Opcodes.DUP2_X2);
	  }

	  /// <summary>
	  /// Generates a SWAP instruction. </summary>
	  public virtual void swap()
	  {
		mv.visitInsn(Opcodes.SWAP);
	  }

	  /// <summary>
	  /// Generates the instructions to swap the top two stack values.
	  /// </summary>
	  /// <param name="prev"> type of the top - 1 stack value. </param>
	  /// <param name="type"> type of the top stack value. </param>
	  public virtual void swap(org.objectweb.asm.JType prev, org.objectweb.asm.JType type)
	  {
		if (type.getSize() == 1)
		{
		  if (prev.getSize() == 1)
		  {
			swap(); // Same as dupX1 pop.
		  }
		  else
		  {
			dupX2();
			pop();
		  }
		}
		else
		{
		  if (prev.getSize() == 1)
		  {
			dup2X1();
			pop2();
		  }
		  else
		  {
			dup2X2();
			pop2();
		  }
		}
	  }

	  // -----------------------------------------------------------------------------------------------
	  // Instructions to do mathematical and logical operations
	  // -----------------------------------------------------------------------------------------------

	  /// <summary>
	  /// Generates the instruction to do the specified mathematical or logical operation.
	  /// </summary>
	  /// <param name="op"> a mathematical or logical operation. Must be one of ADD, SUB, MUL, DIV, REM, NEG,
	  ///     SHL, SHR, USHR, AND, OR, XOR. </param>
	  /// <param name="type"> the type of the operand(s) for this operation. </param>
	  public virtual void math(int op, org.objectweb.asm.JType type)
	  {
		mv.visitInsn(type.getOpcode(op));
	  }

	  /// <summary>
	  /// Generates the instructions to compute the bitwise negation of the top stack value. </summary>
	  public virtual void not()
	  {
		mv.visitInsn(Opcodes.ICONST_1);
		mv.visitInsn(Opcodes.IXOR);
	  }

	  /// <summary>
	  /// Generates the instruction to increment the given local variable.
	  /// </summary>
	  /// <param name="local"> the local variable to be incremented. </param>
	  /// <param name="amount"> the amount by which the local variable must be incremented. </param>
	  public virtual void iinc(int local, int amount)
	  {
		mv.visitIincInsn(local, amount);
	  }

	  /// <summary>
	  /// Generates the instructions to cast a numerical value from one type to another.
	  /// </summary>
	  /// <param name="from"> the type of the top stack value </param>
	  /// <param name="to"> the type into which this value must be cast. </param>
	  public virtual void cast(org.objectweb.asm.JType from, org.objectweb.asm.JType to)
	  {
		if (from != to)
		{
		  if (from.getSort() < org.objectweb.asm.JType.BOOLEAN || from.getSort() > org.objectweb.asm.JType.DOUBLE || to.getSort() < org.objectweb.asm.JType.BOOLEAN || to.getSort() > org.objectweb.asm.JType.DOUBLE)
		  {
			throw new System.ArgumentException("Cannot cast from " + from + " to " + to);
		  }
		  InstructionAdapter.cast(mv, from, to);
		}
	  }

	  // -----------------------------------------------------------------------------------------------
	  // Instructions to do boxing and unboxing operations
	  // -----------------------------------------------------------------------------------------------

	  private static org.objectweb.asm.JType getBoxedType(org.objectweb.asm.JType type)
	  {
		switch (type.getSort())
		{
		  case org.objectweb.asm.JType.BYTE:
			return BYTE_TYPE;
		  case org.objectweb.asm.JType.BOOLEAN:
			return BOOLEAN_TYPE;
		  case org.objectweb.asm.JType.SHORT:
			return SHORT_TYPE;
		  case org.objectweb.asm.JType.CHAR:
			return CHARACTER_TYPE;
		  case org.objectweb.asm.JType.INT:
			return INTEGER_TYPE;
		  case org.objectweb.asm.JType.FLOAT:
			return FLOAT_TYPE;
		  case org.objectweb.asm.JType.LONG:
			return LONG_TYPE;
		  case org.objectweb.asm.JType.DOUBLE:
			return DOUBLE_TYPE;
		  default:
			return type;
		}
	  }

	  /// <summary>
	  /// Generates the instructions to box the top stack value. This value is replaced by its boxed
	  /// equivalent on top of the stack.
	  /// </summary>
	  /// <param name="type"> the type of the top stack value. </param>
	  public virtual void box(org.objectweb.asm.JType type)
	  {
		if (type.getSort() == org.objectweb.asm.JType.OBJECT || type.getSort() == org.objectweb.asm.JType.ARRAY)
		{
		  return;
		}
		if (type == org.objectweb.asm.JType.VOID_TYPE)
		{
		  push((string) null);
		}
		else
		{
		  org.objectweb.asm.JType boxedType = getBoxedType(type);
		  newInstance(boxedType);
		  if (type.getSize() == 2)
		  {
			// Pp -> Ppo -> oPpo -> ooPpo -> ooPp -> o
			dupX2();
			dupX2();
			pop();
		  }
		  else
		  {
			// p -> po -> opo -> oop -> o
			dupX1();
			swap();
		  }
		  invokeConstructor(boxedType, new Method("<init>", org.objectweb.asm.JType.VOID_TYPE, new org.objectweb.asm.JType[] {type}));
		}
	  }

	  /// <summary>
	  /// Generates the instructions to box the top stack value using Java 5's valueOf() method. This
	  /// value is replaced by its boxed equivalent on top of the stack.
	  /// </summary>
	  /// <param name="type"> the type of the top stack value. </param>
	  public virtual void valueOf(org.objectweb.asm.JType type)
	  {
		if (type.getSort() == org.objectweb.asm.JType.OBJECT || type.getSort() == org.objectweb.asm.JType.ARRAY)
		{
		  return;
		}
		if (type == org.objectweb.asm.JType.VOID_TYPE)
		{
		  push((string) null);
		}
		else
		{
		  org.objectweb.asm.JType boxedType = getBoxedType(type);
		  invokeStatic(boxedType, new Method("valueOf", boxedType, new org.objectweb.asm.JType[] {type}));
		}
	  }

	  /// <summary>
	  /// Generates the instructions to unbox the top stack value. This value is replaced by its unboxed
	  /// equivalent on top of the stack.
	  /// </summary>
	  /// <param name="type"> the type of the top stack value. </param>
	  public virtual void unbox(org.objectweb.asm.JType type)
	  {
		org.objectweb.asm.JType boxedType = NUMBER_TYPE;
		Method unboxMethod;
		switch (type.getSort())
		{
		  case org.objectweb.asm.JType.VOID:
			return;
		  case org.objectweb.asm.JType.CHAR:
			boxedType = CHARACTER_TYPE;
			unboxMethod = CHAR_VALUE;
			break;
		  case org.objectweb.asm.JType.BOOLEAN:
			boxedType = BOOLEAN_TYPE;
			unboxMethod = BOOLEAN_VALUE;
			break;
		  case org.objectweb.asm.JType.DOUBLE:
			unboxMethod = DOUBLE_VALUE;
			break;
		  case org.objectweb.asm.JType.FLOAT:
			unboxMethod = FLOAT_VALUE;
			break;
		  case org.objectweb.asm.JType.LONG:
			unboxMethod = LONG_VALUE;
			break;
		  case org.objectweb.asm.JType.INT:
		  case org.objectweb.asm.JType.SHORT:
		  case org.objectweb.asm.JType.BYTE:
			unboxMethod = INT_VALUE;
			break;
		  default:
			unboxMethod = null;
			break;
		}
		if (unboxMethod == null)
		{
		  checkCast(type);
		}
		else
		{
		  checkCast(boxedType);
		  invokeVirtual(boxedType, unboxMethod);
		}
	  }

	  // -----------------------------------------------------------------------------------------------
	  // Instructions to jump to other instructions
	  // -----------------------------------------------------------------------------------------------

	  /// <summary>
	  /// Constructs a new <seealso cref="Label"/>.
	  /// </summary>
	  /// <returns> a new <seealso cref="Label"/>. </returns>
	  public virtual Label newLabel()
	  {
		return new Label();
	  }

	  /// <summary>
	  /// Marks the current code position with the given label.
	  /// </summary>
	  /// <param name="label"> a label. </param>
	  public virtual void mark(Label label)
	  {
		mv.visitLabel(label);
	  }

	  /// <summary>
	  /// Marks the current code position with a new label.
	  /// </summary>
	  /// <returns> the label that was created to mark the current code position. </returns>
	  public virtual Label mark()
	  {
		Label label = new Label();
		mv.visitLabel(label);
		return label;
	  }

	  /// <summary>
	  /// Generates the instructions to jump to a label based on the comparison of the top two stack
	  /// values.
	  /// </summary>
	  /// <param name="type"> the type of the top two stack values. </param>
	  /// <param name="mode"> how these values must be compared. One of EQ, NE, LT, GE, GT, LE. </param>
	  /// <param name="label"> where to jump if the comparison result is {@literal true}. </param>
	  public virtual void ifCmp(org.objectweb.asm.JType type, int mode, Label label)
	  {
		switch (type.getSort())
		{
		  case org.objectweb.asm.JType.LONG:
			mv.visitInsn(Opcodes.LCMP);
			break;
		  case org.objectweb.asm.JType.DOUBLE:
			mv.visitInsn(mode == GE || mode == GT ? Opcodes.DCMPL : Opcodes.DCMPG);
			break;
		  case org.objectweb.asm.JType.FLOAT:
			mv.visitInsn(mode == GE || mode == GT ? Opcodes.FCMPL : Opcodes.FCMPG);
			break;
		  case org.objectweb.asm.JType.ARRAY:
		  case org.objectweb.asm.JType.OBJECT:
			if (mode == EQ)
			{
			  mv.visitJumpInsn(Opcodes.IF_ACMPEQ, label);
			  return;
			}
			else if (mode == NE)
			{
			  mv.visitJumpInsn(Opcodes.IF_ACMPNE, label);
			  return;
			}
			else
			{
			  throw new System.ArgumentException("Bad comparison for type " + type);
			}
		  default:
			int intOp = -1;
			switch (mode)
			{
			  case EQ:
				intOp = Opcodes.IF_ICMPEQ;
				break;
			  case NE:
				intOp = Opcodes.IF_ICMPNE;
				break;
			  case GE:
				intOp = Opcodes.IF_ICMPGE;
				break;
			  case LT:
				intOp = Opcodes.IF_ICMPLT;
				break;
			  case LE:
				intOp = Opcodes.IF_ICMPLE;
				break;
			  case GT:
				intOp = Opcodes.IF_ICMPGT;
				break;
			  default:
				throw new System.ArgumentException("Bad comparison mode " + mode);
			}
			mv.visitJumpInsn(intOp, label);
			return;
		}
		mv.visitJumpInsn(mode, label);
	  }

	  /// <summary>
	  /// Generates the instructions to jump to a label based on the comparison of the top two integer
	  /// stack values.
	  /// </summary>
	  /// <param name="mode"> how these values must be compared. One of EQ, NE, LT, GE, GT, LE. </param>
	  /// <param name="label"> where to jump if the comparison result is {@literal true}. </param>
	  public virtual void ifICmp(int mode, Label label)
	  {
		ifCmp(org.objectweb.asm.JType.INT_TYPE, mode, label);
	  }

	  /// <summary>
	  /// Generates the instructions to jump to a label based on the comparison of the top integer stack
	  /// value with zero.
	  /// </summary>
	  /// <param name="mode"> how these values must be compared. One of EQ, NE, LT, GE, GT, LE. </param>
	  /// <param name="label"> where to jump if the comparison result is {@literal true}. </param>
	  public virtual void ifZCmp(int mode, Label label)
	  {
		mv.visitJumpInsn(mode, label);
	  }

	  /// <summary>
	  /// Generates the instruction to jump to the given label if the top stack value is null.
	  /// </summary>
	  /// <param name="label"> where to jump if the condition is {@literal true}. </param>
	  public virtual void ifNull(Label label)
	  {
		mv.visitJumpInsn(Opcodes.IFNULL, label);
	  }

	  /// <summary>
	  /// Generates the instruction to jump to the given label if the top stack value is not null.
	  /// </summary>
	  /// <param name="label"> where to jump if the condition is {@literal true}. </param>
	  public virtual void ifNonNull(Label label)
	  {
		mv.visitJumpInsn(Opcodes.IFNONNULL, label);
	  }

	  /// <summary>
	  /// Generates the instruction to jump to the given label.
	  /// </summary>
	  /// <param name="label"> where to jump if the condition is {@literal true}. </param>
	  public virtual void goTo(Label label)
	  {
		mv.visitJumpInsn(Opcodes.GOTO, label);
	  }

	  /// <summary>
	  /// Generates a RET instruction.
	  /// </summary>
	  /// <param name="local"> a local variable identifier, as returned by {@link
	  ///     LocalVariablesSorter#newLocal(Type)}. </param>
	  public virtual void ret(int local)
	  {
		mv.visitVarInsn(Opcodes.RET, local);
	  }

	  /// <summary>
	  /// Generates the instructions for a switch statement.
	  /// </summary>
	  /// <param name="keys"> the switch case keys. </param>
	  /// <param name="generator"> a generator to generate the code for the switch cases. </param>
	  public virtual void tableSwitch(int[] keys, TableSwitchGenerator generator)
	  {
		float density;
		if (keys.Length == 0)
		{
		  density = 0;
		}
		else
		{
		  density = (float) keys.Length / (keys[keys.Length - 1] - keys[0] + 1);
		}
		tableSwitch(keys, generator, density >= 0.5f);
	  }

	  /// <summary>
	  /// Generates the instructions for a switch statement.
	  /// </summary>
	  /// <param name="keys"> the switch case keys. </param>
	  /// <param name="generator"> a generator to generate the code for the switch cases. </param>
	  /// <param name="useTable"> {@literal true} to use a TABLESWITCH instruction, or {@literal false} to use a
	  ///     LOOKUPSWITCH instruction. </param>
	  public virtual void tableSwitch(int[] keys, TableSwitchGenerator generator, bool useTable)
	  {
		for (int i = 1; i < keys.Length; ++i)
		{
		  if (keys[i] < keys[i - 1])
		  {
			throw new System.ArgumentException("keys must be sorted in ascending order");
		  }
		}
		Label defaultLabel = newLabel();
		Label endLabel = newLabel();
		if (keys.Length > 0)
		{
		  int numKeys = keys.Length;
		  if (useTable)
		  {
			int min = keys[0];
			int max = keys[numKeys - 1];
			int range = max - min + 1;
			Label[] labels = new Label[range];
			Arrays.Fill(labels, defaultLabel);
			for (int i = 0; i < numKeys; ++i)
			{
			  labels[keys[i] - min] = newLabel();
			}
			mv.visitTableSwitchInsn(min, max, defaultLabel, labels);
			for (int i = 0; i < range; ++i)
			{
			  Label label = labels[i];
			  if (label != defaultLabel)
			  {
				mark(label);
				generator.generateCase(i + min, endLabel);
			  }
			}
		  }
		  else
		  {
			Label[] labels = new Label[numKeys];
			for (int i = 0; i < numKeys; ++i)
			{
			  labels[i] = newLabel();
			}
			mv.visitLookupSwitchInsn(defaultLabel, keys, labels);
			for (int i = 0; i < numKeys; ++i)
			{
			  mark(labels[i]);
			  generator.generateCase(keys[i], endLabel);
			}
		  }
		}
		mark(defaultLabel);
		generator.generateDefault();
		mark(endLabel);
	  }

	  /// <summary>
	  /// Generates the instruction to return the top stack value to the caller. </summary>
	  public virtual void returnValue()
	  {
		mv.visitInsn(returnType.getOpcode(Opcodes.IRETURN));
	  }

	  // -----------------------------------------------------------------------------------------------
	  // Instructions to load and store fields
	  // -----------------------------------------------------------------------------------------------

	  /// <summary>
	  /// Generates a get field or set field instruction.
	  /// </summary>
	  /// <param name="opcode"> the instruction's opcode. </param>
	  /// <param name="ownerType"> the class in which the field is defined. </param>
	  /// <param name="name"> the name of the field. </param>
	  /// <param name="fieldType"> the type of the field. </param>
	  private void fieldInsn(int opcode, org.objectweb.asm.JType ownerType, string name, org.objectweb.asm.JType fieldType)
	  {
		mv.visitFieldInsn(opcode, ownerType.getInternalName(), name, fieldType.getDescriptor());
	  }

	  /// <summary>
	  /// Generates the instruction to push the value of a static field on the stack.
	  /// </summary>
	  /// <param name="owner"> the class in which the field is defined. </param>
	  /// <param name="name"> the name of the field. </param>
	  /// <param name="type"> the type of the field. </param>
	  public virtual void getStatic(org.objectweb.asm.JType owner, string name, org.objectweb.asm.JType type)
	  {
		fieldInsn(Opcodes.GETSTATIC, owner, name, type);
	  }

	  /// <summary>
	  /// Generates the instruction to store the top stack value in a static field.
	  /// </summary>
	  /// <param name="owner"> the class in which the field is defined. </param>
	  /// <param name="name"> the name of the field. </param>
	  /// <param name="type"> the type of the field. </param>
	  public virtual void putStatic(org.objectweb.asm.JType owner, string name, org.objectweb.asm.JType type)
	  {
		fieldInsn(Opcodes.PUTSTATIC, owner, name, type);
	  }

	  /// <summary>
	  /// Generates the instruction to push the value of a non static field on the stack.
	  /// </summary>
	  /// <param name="owner"> the class in which the field is defined. </param>
	  /// <param name="name"> the name of the field. </param>
	  /// <param name="type"> the type of the field. </param>
	  public virtual void getField(org.objectweb.asm.JType owner, string name, org.objectweb.asm.JType type)
	  {
		fieldInsn(Opcodes.GETFIELD, owner, name, type);
	  }

	  /// <summary>
	  /// Generates the instruction to store the top stack value in a non static field.
	  /// </summary>
	  /// <param name="owner"> the class in which the field is defined. </param>
	  /// <param name="name"> the name of the field. </param>
	  /// <param name="type"> the type of the field. </param>
	  public virtual void putField(org.objectweb.asm.JType owner, string name, org.objectweb.asm.JType type)
	  {
		fieldInsn(Opcodes.PUTFIELD, owner, name, type);
	  }

	  // -----------------------------------------------------------------------------------------------
	  // Instructions to invoke methods
	  // -----------------------------------------------------------------------------------------------

	  /// <summary>
	  /// Generates an invoke method instruction.
	  /// </summary>
	  /// <param name="opcode"> the instruction's opcode. </param>
	  /// <param name="type"> the class in which the method is defined. </param>
	  /// <param name="method"> the method to be invoked. </param>
	  /// <param name="isInterface"> whether the 'type' class is an interface or not. </param>
	  private void invokeInsn(int opcode, org.objectweb.asm.JType type, Method method, bool isInterface)
	  {
		string owner = type.getSort() == org.objectweb.asm.JType.ARRAY ? type.getDescriptor() : type.getInternalName();
		mv.visitMethodInsn(opcode, owner, method.Name, method.Descriptor, isInterface);
	  }

	  /// <summary>
	  /// Generates the instruction to invoke a normal method.
	  /// </summary>
	  /// <param name="owner"> the class in which the method is defined. </param>
	  /// <param name="method"> the method to be invoked. </param>
	  public virtual void invokeVirtual(org.objectweb.asm.JType owner, Method method)
	  {
		invokeInsn(Opcodes.INVOKEVIRTUAL, owner, method, false);
	  }

	  /// <summary>
	  /// Generates the instruction to invoke a constructor.
	  /// </summary>
	  /// <param name="type"> the class in which the constructor is defined. </param>
	  /// <param name="method"> the constructor to be invoked. </param>
	  public virtual void invokeConstructor(org.objectweb.asm.JType type, Method method)
	  {
		invokeInsn(Opcodes.INVOKESPECIAL, type, method, false);
	  }

	  /// <summary>
	  /// Generates the instruction to invoke a static method.
	  /// </summary>
	  /// <param name="owner"> the class in which the method is defined. </param>
	  /// <param name="method"> the method to be invoked. </param>
	  public virtual void invokeStatic(org.objectweb.asm.JType owner, Method method)
	  {
		invokeInsn(Opcodes.INVOKESTATIC, owner, method, false);
	  }

	  /// <summary>
	  /// Generates the instruction to invoke an interface method.
	  /// </summary>
	  /// <param name="owner"> the class in which the method is defined. </param>
	  /// <param name="method"> the method to be invoked. </param>
	  public virtual void invokeInterface(org.objectweb.asm.JType owner, Method method)
	  {
		invokeInsn(Opcodes.INVOKEINTERFACE, owner, method, true);
	  }

	  /// <summary>
	  /// Generates an invokedynamic instruction.
	  /// </summary>
	  /// <param name="name"> the method's name. </param>
	  /// <param name="descriptor"> the method's descriptor (see <seealso cref="Type"/>). </param>
	  /// <param name="bootstrapMethodHandle"> the bootstrap method. </param>
	  /// <param name="bootstrapMethodArguments"> the bootstrap method constant arguments. Each argument must be
	  ///     an <seealso cref="Integer"/>, <seealso cref="Float"/>, <seealso cref="Long"/>, <seealso cref="Double"/>, <seealso cref="string"/>, {@link
	  ///     Type} or <seealso cref="Handle"/> value. This method is allowed to modify the content of the array so
	  ///     a caller should expect that this array may change. </param>
	  public virtual void invokeDynamic(string name, string descriptor, Handle bootstrapMethodHandle, params object[] bootstrapMethodArguments)
	  {
		mv.visitInvokeDynamicInsn(name, descriptor, bootstrapMethodHandle, bootstrapMethodArguments);
	  }

	  // -----------------------------------------------------------------------------------------------
	  // Instructions to create objects and arrays
	  // -----------------------------------------------------------------------------------------------

	  /// <summary>
	  /// Generates a type dependent instruction.
	  /// </summary>
	  /// <param name="opcode"> the instruction's opcode. </param>
	  /// <param name="type"> the instruction's operand. </param>
	  private void typeInsn(int opcode, org.objectweb.asm.JType type)
	  {
		mv.visitTypeInsn(opcode, type.getInternalName());
	  }

	  /// <summary>
	  /// Generates the instruction to create a new object.
	  /// </summary>
	  /// <param name="type"> the class of the object to be created. </param>
	  public virtual void newInstance(org.objectweb.asm.JType type)
	  {
		typeInsn(Opcodes.NEW, type);
	  }

	  /// <summary>
	  /// Generates the instruction to create a new array.
	  /// </summary>
	  /// <param name="type"> the type of the array elements. </param>
	  public virtual void newArray(org.objectweb.asm.JType type)
	  {
		InstructionAdapter.newarray(mv, type);
	  }

	  // -----------------------------------------------------------------------------------------------
	  // Miscellaneous instructions
	  // -----------------------------------------------------------------------------------------------

	  /// <summary>
	  /// Generates the instruction to compute the length of an array. </summary>
	  public virtual void arrayLength()
	  {
		mv.visitInsn(Opcodes.ARRAYLENGTH);
	  }

	  /// <summary>
	  /// Generates the instruction to throw an exception. </summary>
	  public virtual void throwException()
	  {
		mv.visitInsn(Opcodes.ATHROW);
	  }

	  /// <summary>
	  /// Generates the instructions to create and throw an exception. The exception class must have a
	  /// constructor with a single String argument.
	  /// </summary>
	  /// <param name="type"> the class of the exception to be thrown. </param>
	  /// <param name="message"> the detailed message of the exception. </param>
	  public virtual void throwException(org.objectweb.asm.JType type, string message)
	  {
		newInstance(type);
		dup();
		push(message);
		invokeConstructor(type, Method.getMethod("void <init> (String)"));
		throwException();
	  }

	  /// <summary>
	  /// Generates the instruction to check that the top stack value is of the given type.
	  /// </summary>
	  /// <param name="type"> a class or interface type. </param>
	  public virtual void checkCast(org.objectweb.asm.JType type)
	  {
		if (!type.Equals(OBJECT_TYPE))
		{
		  typeInsn(Opcodes.CHECKCAST, type);
		}
	  }

	  /// <summary>
	  /// Generates the instruction to test if the top stack value is of the given type.
	  /// </summary>
	  /// <param name="type"> a class or interface type. </param>
	  public virtual void instanceOf(org.objectweb.asm.JType type)
	  {
		typeInsn(Opcodes.INSTANCEOF, type);
	  }

	  /// <summary>
	  /// Generates the instruction to get the monitor of the top stack value. </summary>
	  public virtual void monitorEnter()
	  {
		mv.visitInsn(Opcodes.MONITORENTER);
	  }

	  /// <summary>
	  /// Generates the instruction to release the monitor of the top stack value. </summary>
	  public virtual void monitorExit()
	  {
		mv.visitInsn(Opcodes.MONITOREXIT);
	  }

	  // -----------------------------------------------------------------------------------------------
	  // Non instructions
	  // -----------------------------------------------------------------------------------------------

	  /// <summary>
	  /// Marks the end of the visited method. </summary>
	  public virtual void endMethod()
	  {
		if ((access & Opcodes.ACC_ABSTRACT) == 0)
		{
		  mv.visitMaxs(0, 0);
		}
		mv.visitEnd();
	  }

	  /// <summary>
	  /// Marks the start of an exception handler.
	  /// </summary>
	  /// <param name="start"> beginning of the exception handler's scope (inclusive). </param>
	  /// <param name="end"> end of the exception handler's scope (exclusive). </param>
	  /// <param name="exception"> internal name of the type of exceptions handled by the handler. </param>
	  public virtual void catchException(Label start, Label end, org.objectweb.asm.JType exception)
	  {
		Label catchLabel = new Label();
		if (exception == null)
		{
		  mv.visitTryCatchBlock(start, end, catchLabel, null);
		}
		else
		{
		  mv.visitTryCatchBlock(start, end, catchLabel, exception.getInternalName());
		}
		mark(catchLabel);
	  }
	}

}