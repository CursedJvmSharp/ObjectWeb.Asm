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

namespace ObjectWeb.Asm.Commons
{

	/// <summary>
	/// A <seealso cref="MethodVisitor"/> providing a more detailed API to generate and transform instructions.
	/// 
	/// @author Eric Bruneton
	/// </summary>
	public class InstructionAdapter : MethodVisitor
	{

	  /// <summary>
	  /// The type of the java.lang.Object class. </summary>
	  public static readonly JType ObjectType = JType.GetType("Ljava/lang/Object;");

	  /// <summary>
	  /// Constructs a new <seealso cref="InstructionAdapter"/>. <i>Subclasses must not use this constructor</i>.
	  /// Instead, they must use the <seealso cref="InstructionAdapter(int, MethodVisitor)"/> version.
	  /// </summary>
	  /// <param name="methodVisitor"> the method visitor to which this adapter delegates calls. </param>
	  /// <exception cref="IllegalStateException"> If a subclass calls this constructor. </exception>
	  public InstructionAdapter(MethodVisitor methodVisitor) : this(IOpcodes.Asm9, methodVisitor)
	  {
		if (this.GetType() != typeof(InstructionAdapter))
		{
		  throw new System.InvalidOperationException();
		}
	  }

	  /// <summary>
	  /// Constructs a new <seealso cref="InstructionAdapter"/>.
	  /// </summary>
	  /// <param name="api"> the ASM API version implemented by this visitor. Must be one of the {@code
	  ///     ASM}<i>x</i> values in <seealso cref="IOpcodes"/>. </param>
	  /// <param name="methodVisitor"> the method visitor to which this adapter delegates calls. </param>
	  public InstructionAdapter(int api, MethodVisitor methodVisitor) : base(api, methodVisitor)
	  {
	  }

	  public override void VisitInsn(int opcode)
	  {
		switch (opcode)
		{
		  case IOpcodes.Nop:
			Nop();
			break;
		  case IOpcodes.Aconst_Null:
			Aconst(null);
			break;
		  case IOpcodes.Iconst_M1:
		  case IOpcodes.Iconst_0:
		  case IOpcodes.Iconst_1:
		  case IOpcodes.Iconst_2:
		  case IOpcodes.Iconst_3:
		  case IOpcodes.Iconst_4:
		  case IOpcodes.Iconst_5:
			Iconst(opcode - IOpcodes.Iconst_0);
			break;
		  case IOpcodes.Lconst_0:
		  case IOpcodes.Lconst_1:
			Lconst((long)(opcode - IOpcodes.Lconst_0));
			break;
		  case IOpcodes.Fconst_0:
		  case IOpcodes.Fconst_1:
		  case IOpcodes.Fconst_2:
			Fconst((float)(opcode - IOpcodes.Fconst_0));
			break;
		  case IOpcodes.Dconst_0:
		  case IOpcodes.Dconst_1:
			Dconst((double)(opcode - IOpcodes.Dconst_0));
			break;
		  case IOpcodes.Iaload:
			Aload(JType.IntType);
			break;
		  case IOpcodes.Laload:
			Aload(JType.LongType);
			break;
		  case IOpcodes.Faload:
			Aload(JType.FloatType);
			break;
		  case IOpcodes.Daload:
			Aload(JType.DoubleType);
			break;
		  case IOpcodes.Aaload:
			Aload(ObjectType);
			break;
		  case IOpcodes.Baload:
			Aload(JType.ByteType);
			break;
		  case IOpcodes.Caload:
			Aload(JType.CharType);
			break;
		  case IOpcodes.Saload:
			Aload(JType.ShortType);
			break;
		  case IOpcodes.Iastore:
			Astore(JType.IntType);
			break;
		  case IOpcodes.Lastore:
			Astore(JType.LongType);
			break;
		  case IOpcodes.Fastore:
			Astore(JType.FloatType);
			break;
		  case IOpcodes.Dastore:
			Astore(JType.DoubleType);
			break;
		  case IOpcodes.Aastore:
			Astore(ObjectType);
			break;
		  case IOpcodes.Bastore:
			Astore(JType.ByteType);
			break;
		  case IOpcodes.Castore:
			Astore(JType.CharType);
			break;
		  case IOpcodes.Sastore:
			Astore(JType.ShortType);
			break;
		  case IOpcodes.Pop:
			Pop();
			break;
		  case IOpcodes.Pop2:
			Pop2();
			break;
		  case IOpcodes.Dup:
			Dup();
			break;
		  case IOpcodes.Dup_X1:
			DupX1();
			break;
		  case IOpcodes.Dup_X2:
			DupX2();
			break;
		  case IOpcodes.Dup2:
			Dup2();
			break;
		  case IOpcodes.Dup2_X1:
			Dup2X1();
			break;
		  case IOpcodes.Dup2_X2:
			Dup2X2();
			break;
		  case IOpcodes.Swap:
			Swap();
			break;
		  case IOpcodes.Iadd:
			Add(JType.IntType);
			break;
		  case IOpcodes.Ladd:
			Add(JType.LongType);
			break;
		  case IOpcodes.Fadd:
			Add(JType.FloatType);
			break;
		  case IOpcodes.Dadd:
			Add(JType.DoubleType);
			break;
		  case IOpcodes.Isub:
			Sub(JType.IntType);
			break;
		  case IOpcodes.Lsub:
			Sub(JType.LongType);
			break;
		  case IOpcodes.Fsub:
			Sub(JType.FloatType);
			break;
		  case IOpcodes.Dsub:
			Sub(JType.DoubleType);
			break;
		  case IOpcodes.Imul:
			Mul(JType.IntType);
			break;
		  case IOpcodes.Lmul:
			Mul(JType.LongType);
			break;
		  case IOpcodes.Fmul:
			Mul(JType.FloatType);
			break;
		  case IOpcodes.Dmul:
			Mul(JType.DoubleType);
			break;
		  case IOpcodes.Idiv:
			Div(JType.IntType);
			break;
		  case IOpcodes.Ldiv:
			Div(JType.LongType);
			break;
		  case IOpcodes.Fdiv:
			Div(JType.FloatType);
			break;
		  case IOpcodes.Ddiv:
			Div(JType.DoubleType);
			break;
		  case IOpcodes.Irem:
			Rem(JType.IntType);
			break;
		  case IOpcodes.Lrem:
			Rem(JType.LongType);
			break;
		  case IOpcodes.Frem:
			Rem(JType.FloatType);
			break;
		  case IOpcodes.Drem:
			Rem(JType.DoubleType);
			break;
		  case IOpcodes.Ineg:
			Neg(JType.IntType);
			break;
		  case IOpcodes.Lneg:
			Neg(JType.LongType);
			break;
		  case IOpcodes.Fneg:
			Neg(JType.FloatType);
			break;
		  case IOpcodes.Dneg:
			Neg(JType.DoubleType);
			break;
		  case IOpcodes.Ishl:
			Shl(JType.IntType);
			break;
		  case IOpcodes.Lshl:
			Shl(JType.LongType);
			break;
		  case IOpcodes.Ishr:
			Shr(JType.IntType);
			break;
		  case IOpcodes.Lshr:
			Shr(JType.LongType);
			break;
		  case IOpcodes.Iushr:
			Ushr(JType.IntType);
			break;
		  case IOpcodes.Lushr:
			Ushr(JType.LongType);
			break;
		  case IOpcodes.Iand:
			And(JType.IntType);
			break;
		  case IOpcodes.Land:
			And(JType.LongType);
			break;
		  case IOpcodes.Ior:
			Or(JType.IntType);
			break;
		  case IOpcodes.Lor:
			Or(JType.LongType);
			break;
		  case IOpcodes.Ixor:
			Xor(JType.IntType);
			break;
		  case IOpcodes.Lxor:
			Xor(JType.LongType);
			break;
		  case IOpcodes.I2L:
			Cast(JType.IntType, JType.LongType);
			break;
		  case IOpcodes.I2F:
			Cast(JType.IntType, JType.FloatType);
			break;
		  case IOpcodes.I2D:
			Cast(JType.IntType, JType.DoubleType);
			break;
		  case IOpcodes.L2I:
			Cast(JType.LongType, JType.IntType);
			break;
		  case IOpcodes.L2F:
			Cast(JType.LongType, JType.FloatType);
			break;
		  case IOpcodes.L2D:
			Cast(JType.LongType, JType.DoubleType);
			break;
		  case IOpcodes.F2I:
			Cast(JType.FloatType, JType.IntType);
			break;
		  case IOpcodes.F2L:
			Cast(JType.FloatType, JType.LongType);
			break;
		  case IOpcodes.F2D:
			Cast(JType.FloatType, JType.DoubleType);
			break;
		  case IOpcodes.D2I:
			Cast(JType.DoubleType, JType.IntType);
			break;
		  case IOpcodes.D2L:
			Cast(JType.DoubleType, JType.LongType);
			break;
		  case IOpcodes.D2F:
			Cast(JType.DoubleType, JType.FloatType);
			break;
		  case IOpcodes.I2B:
			Cast(JType.IntType, JType.ByteType);
			break;
		  case IOpcodes.I2C:
			Cast(JType.IntType, JType.CharType);
			break;
		  case IOpcodes.I2S:
			Cast(JType.IntType, JType.ShortType);
			break;
		  case IOpcodes.Lcmp:
			Lcmp();
			break;
		  case IOpcodes.Fcmpl:
			Cmpl(JType.FloatType);
			break;
		  case IOpcodes.Fcmpg:
			Cmpg(JType.FloatType);
			break;
		  case IOpcodes.Dcmpl:
			Cmpl(JType.DoubleType);
			break;
		  case IOpcodes.Dcmpg:
			Cmpg(JType.DoubleType);
			break;
		  case IOpcodes.Ireturn:
			Areturn(JType.IntType);
			break;
		  case IOpcodes.Lreturn:
			Areturn(JType.LongType);
			break;
		  case IOpcodes.Freturn:
			Areturn(JType.FloatType);
			break;
		  case IOpcodes.Dreturn:
			Areturn(JType.DoubleType);
			break;
		  case IOpcodes.Areturn:
			Areturn(ObjectType);
			break;
		  case IOpcodes.Return:
			Areturn(JType.VoidType);
			break;
		  case IOpcodes.Arraylength:
			Arraylength();
			break;
		  case IOpcodes.Athrow:
			Athrow();
			break;
		  case IOpcodes.Monitorenter:
			Monitorenter();
			break;
		  case IOpcodes.Monitorexit:
			Monitorexit();
			break;
		  default:
			throw new System.ArgumentException();
		}
	  }

	  public override void VisitIntInsn(int opcode, int operand)
	  {
		switch (opcode)
		{
		  case IOpcodes.Bipush:
			Iconst(operand);
			break;
		  case IOpcodes.Sipush:
			Iconst(operand);
			break;
		  case IOpcodes.Newarray:
			switch (operand)
			{
			  case IOpcodes.Boolean:
				Newarray(JType.BooleanType);
				break;
			  case IOpcodes.Char:
				Newarray(JType.CharType);
				break;
			  case IOpcodes.Byte:
				Newarray(JType.ByteType);
				break;
			  case IOpcodes.Short:
				Newarray(JType.ShortType);
				break;
			  case IOpcodes.Int:
				Newarray(JType.IntType);
				break;
			  case IOpcodes.Float:
				Newarray(JType.FloatType);
				break;
			  case IOpcodes.Long:
				Newarray(JType.LongType);
				break;
			  case IOpcodes.Double:
				Newarray(JType.DoubleType);
				break;
			  default:
				throw new System.ArgumentException();
			}
			break;
		  default:
			throw new System.ArgumentException();
		}
	  }

	  public override void VisitVarInsn(int opcode, int var)
	  {
		switch (opcode)
		{
		  case IOpcodes.Iload:
			Load(var, JType.IntType);
			break;
		  case IOpcodes.Lload:
			Load(var, JType.LongType);
			break;
		  case IOpcodes.Fload:
			Load(var, JType.FloatType);
			break;
		  case IOpcodes.Dload:
			Load(var, JType.DoubleType);
			break;
		  case IOpcodes.Aload:
			Load(var, ObjectType);
			break;
		  case IOpcodes.Istore:
			Store(var, JType.IntType);
			break;
		  case IOpcodes.Lstore:
			Store(var, JType.LongType);
			break;
		  case IOpcodes.Fstore:
			Store(var, JType.FloatType);
			break;
		  case IOpcodes.Dstore:
			Store(var, JType.DoubleType);
			break;
		  case IOpcodes.Astore:
			Store(var, ObjectType);
			break;
		  case IOpcodes.Ret:
			Ret(var);
			break;
		  default:
			throw new System.ArgumentException();
		}
	  }

	  public override void VisitTypeInsn(int opcode, string type)
	  {
		JType objectType = JType.GetObjectType(type);
		switch (opcode)
		{
		  case IOpcodes.New:
			Anew(objectType);
			break;
		  case IOpcodes.Anewarray:
			Newarray(objectType);
			break;
		  case IOpcodes.Checkcast:
			Checkcast(objectType);
			break;
		  case IOpcodes.Instanceof:
			InstanceOf(objectType);
			break;
		  default:
			throw new System.ArgumentException();
		}
	  }

	  public override void VisitFieldInsn(int opcode, string owner, string name, string descriptor)
	  {
		switch (opcode)
		{
		  case IOpcodes.Getstatic:
			Getstatic(owner, name, descriptor);
			break;
		  case IOpcodes.Putstatic:
			Putstatic(owner, name, descriptor);
			break;
		  case IOpcodes.Getfield:
			Getfield(owner, name, descriptor);
			break;
		  case IOpcodes.Putfield:
			Putfield(owner, name, descriptor);
			break;
		  default:
			throw new System.ArgumentException();
		}
	  }

	  public override void VisitMethodInsn(int opcodeAndSource, string owner, string name, string descriptor, bool isInterface)
	  {
		if (api < IOpcodes.Asm5 && (opcodeAndSource & IOpcodes.Source_Deprecated) == 0)
		{
		  // Redirect the call to the deprecated version of this method.
		  base.VisitMethodInsn(opcodeAndSource, owner, name, descriptor, isInterface);
		  return;
		}
		int opcode = opcodeAndSource & ~IOpcodes.Source_Mask;

		switch (opcode)
		{
		  case IOpcodes.Invokespecial:
			Invokespecial(owner, name, descriptor, isInterface);
			break;
		  case IOpcodes.Invokevirtual:
			Invokevirtual(owner, name, descriptor, isInterface);
			break;
		  case IOpcodes.Invokestatic:
			Invokestatic(owner, name, descriptor, isInterface);
			break;
		  case IOpcodes.Invokeinterface:
			Invokeinterface(owner, name, descriptor);
			break;
		  default:
			throw new System.ArgumentException();
		}
	  }

	  public override void VisitInvokeDynamicInsn(string name, string descriptor, Handle bootstrapMethodHandle, params object[] bootstrapMethodArguments)
	  {
		Invokedynamic(name, descriptor, bootstrapMethodHandle, bootstrapMethodArguments);
	  }

	  public override void VisitJumpInsn(int opcode, Label label)
	  {
		switch (opcode)
		{
		  case IOpcodes.Ifeq:
			Ifeq(label);
			break;
		  case IOpcodes.Ifne:
			Ifne(label);
			break;
		  case IOpcodes.Iflt:
			Iflt(label);
			break;
		  case IOpcodes.Ifge:
			Ifge(label);
			break;
		  case IOpcodes.Ifgt:
			Ifgt(label);
			break;
		  case IOpcodes.Ifle:
			Ifle(label);
			break;
		  case IOpcodes.If_Icmpeq:
			Ificmpeq(label);
			break;
		  case IOpcodes.If_Icmpne:
			Ificmpne(label);
			break;
		  case IOpcodes.If_Icmplt:
			Ificmplt(label);
			break;
		  case IOpcodes.If_Icmpge:
			Ificmpge(label);
			break;
		  case IOpcodes.If_Icmpgt:
			Ificmpgt(label);
			break;
		  case IOpcodes.If_Icmple:
			Ificmple(label);
			break;
		  case IOpcodes.If_Acmpeq:
			Ifacmpeq(label);
			break;
		  case IOpcodes.If_Acmpne:
			Ifacmpne(label);
			break;
		  case IOpcodes.Goto:
			GoTo(label);
			break;
		  case IOpcodes.Jsr:
			Jsr(label);
			break;
		  case IOpcodes.Ifnull:
			Ifnull(label);
			break;
		  case IOpcodes.Ifnonnull:
			Ifnonnull(label);
			break;
		  default:
			throw new System.ArgumentException();
		}
	  }

	  public override void VisitLabel(Label label)
	  {
		Mark(label);
	  }

	  public override void VisitLdcInsn(object value)
	  {
		if (api < IOpcodes.Asm5 && (value is Handle || (value is JType && ((JType) value).Sort == JType.Method)))
		{
		  throw new System.NotSupportedException("This feature requires ASM5");
		}
		if (api < IOpcodes.Asm7 && value is ConstantDynamic)
		{
		  throw new System.NotSupportedException("This feature requires ASM7");
		}
		if (value is int)
		{
		  Iconst(((int?) value).Value);
		}
		else if (value is Byte)
		{
		  Iconst(((sbyte?) value).Value);
		}
		else if (value is char)
		{
		  Iconst(((char?) value).Value);
		}
		else if (value is short)
		{
		  Iconst(((short?) value).Value);
		}
		else if (value is Boolean)
		{
		  Iconst(((bool?) value).Value ? 1 : 0);
		}
		else if (value is float)
		{
		  Fconst(((float?) value).Value);
		}
		else if (value is long)
		{
		  Lconst(((long?) value).Value);
		}
		else if (value is Double)
		{
		  Dconst(((double?) value).Value);
		}
		else if (value is string)
		{
		  Aconst(value);
		}
		else if (value is Type)
		{
		  Tconst((JType) value);
		}
		else if (value is Handle)
		{
		  Hconst((Handle) value);
		}
		else if (value is ConstantDynamic)
		{
		  Cconst((ConstantDynamic) value);
		}
		else
		{
		  throw new System.ArgumentException();
		}
	  }

	  public override void VisitIincInsn(int var, int increment)
	  {
		Iinc(var, increment);
	  }

	  public override void VisitTableSwitchInsn(int min, int max, Label dflt, params Label[] labels)
	  {
		Tableswitch(min, max, dflt, labels);
	  }

	  public override void VisitLookupSwitchInsn(Label dflt, int[] keys, Label[] labels)
	  {
		Lookupswitch(dflt, keys, labels);
	  }

	  public override void VisitMultiANewArrayInsn(string descriptor, int numDimensions)
	  {
		Multianewarray(descriptor, numDimensions);
	  }

	  // -----------------------------------------------------------------------------------------------

	  /// <summary>
	  /// Generates a nop instruction. </summary>
	  public virtual void Nop()
	  {
		mv.VisitInsn(IOpcodes.Nop);
	  }

	  /// <summary>
	  /// Generates the instruction to push the given value on the stack.
	  /// </summary>
	  /// <param name="value"> the constant to be pushed on the stack. This parameter must be an <seealso cref="Integer"/>,
	  ///     a <seealso cref="Float"/>, a <seealso cref="Long"/>, a <seealso cref="Double"/>, a <seealso cref="string"/>, a <seealso cref="Type"/> of
	  ///     OBJECT or ARRAY sort for {@code .class} constants, for classes whose version is 49, a
	  ///     <seealso cref="Type"/> of METHOD sort for MethodType, a <seealso cref="Handle"/> for MethodHandle constants,
	  ///     for classes whose version is 51 or a <seealso cref="ConstantDynamic"/> for a constant dynamic for
	  ///     classes whose version is 55. </param>
	  public virtual void Aconst(object value)
	  {
		if (value == null)
		{
		  mv.VisitInsn(IOpcodes.Aconst_Null);
		}
		else
		{
		  mv.VisitLdcInsn(value);
		}
	  }

	  /// <summary>
	  /// Generates the instruction to push the given value on the stack.
	  /// </summary>
	  /// <param name="intValue"> the constant to be pushed on the stack. </param>
	  public virtual void Iconst(int intValue)
	  {
		if (intValue >= -1 && intValue <= 5)
		{
		  mv.VisitInsn(IOpcodes.Iconst_0 + intValue);
		}
		else if (intValue >= sbyte.MinValue && intValue <= sbyte.MaxValue)
		{
		  mv.VisitIntInsn(IOpcodes.Bipush, intValue);
		}
		else if (intValue >= short.MinValue && intValue <= short.MaxValue)
		{
		  mv.VisitIntInsn(IOpcodes.Sipush, intValue);
		}
		else
		{
		  mv.VisitLdcInsn(intValue);
		}
	  }

	  /// <summary>
	  /// Generates the instruction to push the given value on the stack.
	  /// </summary>
	  /// <param name="longValue"> the constant to be pushed on the stack. </param>
	  public virtual void Lconst(long longValue)
	  {
		if (longValue == 0L || longValue == 1L)
		{
		  mv.VisitInsn(IOpcodes.Lconst_0 + (int) longValue);
		}
		else
		{
		  mv.VisitLdcInsn(longValue);
		}
	  }

	  /// <summary>
	  /// Generates the instruction to push the given value on the stack.
	  /// </summary>
	  /// <param name="floatValue"> the constant to be pushed on the stack. </param>
	  public virtual void Fconst(float floatValue)
	  {
		int bits = BitConverter.SingleToInt32Bits(floatValue);
		if (bits == 0L || bits == 0x3F800000 || bits == 0x40000000)
		{ // 0..2
		  mv.VisitInsn(IOpcodes.Fconst_0 + (int) floatValue);
		}
		else
		{
		  mv.VisitLdcInsn(floatValue);
		}
	  }

	  /// <summary>
	  /// Generates the instruction to push the given value on the stack.
	  /// </summary>
	  /// <param name="doubleValue"> the constant to be pushed on the stack. </param>
	  public virtual void Dconst(double doubleValue)
	  {
		long bits = System.BitConverter.DoubleToInt64Bits(doubleValue);
		if (bits == 0L || bits == 0x3FF0000000000000L)
		{ // +0.0d and 1.0d
		  mv.VisitInsn(IOpcodes.Dconst_0 + (int) doubleValue);
		}
		else
		{
		  mv.VisitLdcInsn(doubleValue);
		}
	  }

	  /// <summary>
	  /// Generates the instruction to push the given type on the stack.
	  /// </summary>
	  /// <param name="type"> the type to be pushed on the stack. </param>
	  public virtual void Tconst(JType type)
	  {
		mv.VisitLdcInsn(type);
	  }

	  /// <summary>
	  /// Generates the instruction to push the given handle on the stack.
	  /// </summary>
	  /// <param name="handle"> the handle to be pushed on the stack. </param>
	  public virtual void Hconst(Handle handle)
	  {
		mv.VisitLdcInsn(handle);
	  }

	  /// <summary>
	  /// Generates the instruction to push the given constant dynamic on the stack.
	  /// </summary>
	  /// <param name="constantDynamic"> the constant dynamic to be pushed on the stack. </param>
	  public virtual void Cconst(ConstantDynamic constantDynamic)
	  {
		mv.VisitLdcInsn(constantDynamic);
	  }

	  public virtual void Load(int var, JType type)
	  {
		mv.VisitVarInsn(type.GetOpcode(IOpcodes.Iload), var);
	  }

	  public virtual void Aload(JType type)
	  {
		mv.VisitInsn(type.GetOpcode(IOpcodes.Iaload));
	  }

	  public virtual void Store(int var, JType type)
	  {
		mv.VisitVarInsn(type.GetOpcode(IOpcodes.Istore), var);
	  }

	  public virtual void Astore(JType type)
	  {
		mv.VisitInsn(type.GetOpcode(IOpcodes.Iastore));
	  }

	  public virtual void Pop()
	  {
		mv.VisitInsn(IOpcodes.Pop);
	  }

	  public virtual void Pop2()
	  {
		mv.VisitInsn(IOpcodes.Pop2);
	  }

	  public virtual void Dup()
	  {
		mv.VisitInsn(IOpcodes.Dup);
	  }

	  public virtual void Dup2()
	  {
		mv.VisitInsn(IOpcodes.Dup2);
	  }

	  public virtual void DupX1()
	  {
		mv.VisitInsn(IOpcodes.Dup_X1);
	  }

	  public virtual void DupX2()
	  {
		mv.VisitInsn(IOpcodes.Dup_X2);
	  }

	  public virtual void Dup2X1()
	  {
		mv.VisitInsn(IOpcodes.Dup2_X1);
	  }

	  public virtual void Dup2X2()
	  {
		mv.VisitInsn(IOpcodes.Dup2_X2);
	  }

	  public virtual void Swap()
	  {
		mv.VisitInsn(IOpcodes.Swap);
	  }

	  public virtual void Add(JType type)
	  {
		mv.VisitInsn(type.GetOpcode(IOpcodes.Iadd));
	  }

	  public virtual void Sub(JType type)
	  {
		mv.VisitInsn(type.GetOpcode(IOpcodes.Isub));
	  }

	  public virtual void Mul(JType type)
	  {
		mv.VisitInsn(type.GetOpcode(IOpcodes.Imul));
	  }

	  public virtual void Div(JType type)
	  {
		mv.VisitInsn(type.GetOpcode(IOpcodes.Idiv));
	  }

	  public virtual void Rem(JType type)
	  {
		mv.VisitInsn(type.GetOpcode(IOpcodes.Irem));
	  }

	  public virtual void Neg(JType type)
	  {
		mv.VisitInsn(type.GetOpcode(IOpcodes.Ineg));
	  }

	  public virtual void Shl(JType type)
	  {
		mv.VisitInsn(type.GetOpcode(IOpcodes.Ishl));
	  }

	  public virtual void Shr(JType type)
	  {
		mv.VisitInsn(type.GetOpcode(IOpcodes.Ishr));
	  }

	  public virtual void Ushr(JType type)
	  {
		mv.VisitInsn(type.GetOpcode(IOpcodes.Iushr));
	  }

	  public virtual void And(JType type)
	  {
		mv.VisitInsn(type.GetOpcode(IOpcodes.Iand));
	  }

	  public virtual void Or(JType type)
	  {
		mv.VisitInsn(type.GetOpcode(IOpcodes.Ior));
	  }

	  public virtual void Xor(JType type)
	  {
		mv.VisitInsn(type.GetOpcode(IOpcodes.Ixor));
	  }

	  public virtual void Iinc(int var, int increment)
	  {
		mv.VisitIincInsn(var, increment);
	  }

	  /// <summary>
	  /// Generates the instruction to cast from the first given type to the other.
	  /// </summary>
	  /// <param name="from"> a Type. </param>
	  /// <param name="to"> a Type. </param>
	  public virtual void Cast(JType from, JType to)
	  {
		Cast(mv, from, to);
	  }

	  /// <summary>
	  /// Generates the instruction to cast from the first given type to the other.
	  /// </summary>
	  /// <param name="methodVisitor"> the method visitor to use to generate the instruction. </param>
	  /// <param name="from"> a Type. </param>
	  /// <param name="to"> a Type. </param>
	  internal static void Cast(MethodVisitor methodVisitor, JType from, JType to)
	  {
		if (from != to)
		{
		  if (from == JType.DoubleType)
		  {
			if (to == JType.FloatType)
			{
			  methodVisitor.VisitInsn(IOpcodes.D2F);
			}
			else if (to == JType.LongType)
			{
			  methodVisitor.VisitInsn(IOpcodes.D2L);
			}
			else
			{
			  methodVisitor.VisitInsn(IOpcodes.D2I);
			  Cast(methodVisitor, JType.IntType, to);
			}
		  }
		  else if (from == JType.FloatType)
		  {
			if (to == JType.DoubleType)
			{
			  methodVisitor.VisitInsn(IOpcodes.F2D);
			}
			else if (to == JType.LongType)
			{
			  methodVisitor.VisitInsn(IOpcodes.F2L);
			}
			else
			{
			  methodVisitor.VisitInsn(IOpcodes.F2I);
			  Cast(methodVisitor, JType.IntType, to);
			}
		  }
		  else if (from == JType.LongType)
		  {
			if (to == JType.DoubleType)
			{
			  methodVisitor.VisitInsn(IOpcodes.L2D);
			}
			else if (to == JType.FloatType)
			{
			  methodVisitor.VisitInsn(IOpcodes.L2F);
			}
			else
			{
			  methodVisitor.VisitInsn(IOpcodes.L2I);
			  Cast(methodVisitor, JType.IntType, to);
			}
		  }
		  else
		  {
			if (to == JType.ByteType)
			{
			  methodVisitor.VisitInsn(IOpcodes.I2B);
			}
			else if (to == JType.CharType)
			{
			  methodVisitor.VisitInsn(IOpcodes.I2C);
			}
			else if (to == JType.DoubleType)
			{
			  methodVisitor.VisitInsn(IOpcodes.I2D);
			}
			else if (to == JType.FloatType)
			{
			  methodVisitor.VisitInsn(IOpcodes.I2F);
			}
			else if (to == JType.LongType)
			{
			  methodVisitor.VisitInsn(IOpcodes.I2L);
			}
			else if (to == JType.ShortType)
			{
			  methodVisitor.VisitInsn(IOpcodes.I2S);
			}
		  }
		}
	  }

	  public virtual void Lcmp()
	  {
		mv.VisitInsn(IOpcodes.Lcmp);
	  }

	  public virtual void Cmpl(JType type)
	  {
		mv.VisitInsn(type == JType.FloatType ? IOpcodes.Fcmpl : IOpcodes.Dcmpl);
	  }

	  public virtual void Cmpg(JType type)
	  {
		mv.VisitInsn(type == JType.FloatType ? IOpcodes.Fcmpg : IOpcodes.Dcmpg);
	  }

	  public virtual void Ifeq(Label label)
	  {
		mv.VisitJumpInsn(IOpcodes.Ifeq, label);
	  }

	  public virtual void Ifne(Label label)
	  {
		mv.VisitJumpInsn(IOpcodes.Ifne, label);
	  }

	  public virtual void Iflt(Label label)
	  {
		mv.VisitJumpInsn(IOpcodes.Iflt, label);
	  }

	  public virtual void Ifge(Label label)
	  {
		mv.VisitJumpInsn(IOpcodes.Ifge, label);
	  }

	  public virtual void Ifgt(Label label)
	  {
		mv.VisitJumpInsn(IOpcodes.Ifgt, label);
	  }

	  public virtual void Ifle(Label label)
	  {
		mv.VisitJumpInsn(IOpcodes.Ifle, label);
	  }

	  public virtual void Ificmpeq(Label label)
	  {
		mv.VisitJumpInsn(IOpcodes.If_Icmpeq, label);
	  }

	  public virtual void Ificmpne(Label label)
	  {
		mv.VisitJumpInsn(IOpcodes.If_Icmpne, label);
	  }

	  public virtual void Ificmplt(Label label)
	  {
		mv.VisitJumpInsn(IOpcodes.If_Icmplt, label);
	  }

	  public virtual void Ificmpge(Label label)
	  {
		mv.VisitJumpInsn(IOpcodes.If_Icmpge, label);
	  }

	  public virtual void Ificmpgt(Label label)
	  {
		mv.VisitJumpInsn(IOpcodes.If_Icmpgt, label);
	  }

	  public virtual void Ificmple(Label label)
	  {
		mv.VisitJumpInsn(IOpcodes.If_Icmple, label);
	  }

	  public virtual void Ifacmpeq(Label label)
	  {
		mv.VisitJumpInsn(IOpcodes.If_Acmpeq, label);
	  }

	  public virtual void Ifacmpne(Label label)
	  {
		mv.VisitJumpInsn(IOpcodes.If_Acmpne, label);
	  }

	  public virtual void GoTo(Label label)
	  {
		mv.VisitJumpInsn(IOpcodes.Goto, label);
	  }

	  public virtual void Jsr(Label label)
	  {
		mv.VisitJumpInsn(IOpcodes.Jsr, label);
	  }

	  public virtual void Ret(int var)
	  {
		mv.VisitVarInsn(IOpcodes.Ret, var);
	  }

	  public virtual void Tableswitch(int min, int max, Label dflt, params Label[] labels)
	  {
		mv.VisitTableSwitchInsn(min, max, dflt, labels);
	  }

	  public virtual void Lookupswitch(Label dflt, int[] keys, Label[] labels)
	  {
		mv.VisitLookupSwitchInsn(dflt, keys, labels);
	  }

	  public virtual void Areturn(JType type)
	  {
		mv.VisitInsn(type.GetOpcode(IOpcodes.Ireturn));
	  }

	  public virtual void Getstatic(string owner, string name, string descriptor)
	  {
		mv.VisitFieldInsn(IOpcodes.Getstatic, owner, name, descriptor);
	  }

	  public virtual void Putstatic(string owner, string name, string descriptor)
	  {
		mv.VisitFieldInsn(IOpcodes.Putstatic, owner, name, descriptor);
	  }

	  public virtual void Getfield(string owner, string name, string descriptor)
	  {
		mv.VisitFieldInsn(IOpcodes.Getfield, owner, name, descriptor);
	  }

	  public virtual void Putfield(string owner, string name, string descriptor)
	  {
		mv.VisitFieldInsn(IOpcodes.Putfield, owner, name, descriptor);
	  }

	  /// <summary>
	  /// Deprecated.
	  /// </summary>
	  /// <param name="owner"> the internal name of the method's owner class. </param>
	  /// <param name="name"> the method's name. </param>
	  /// <param name="descriptor"> the method's descriptor (see <seealso cref="Type"/>). </param>
	  /// @deprecated use <seealso cref="Invokevirtual(string,string,string,bool)"/> instead. 
	  [Obsolete("use <seealso cref=\"invokevirtual(String, String, String, bool)\"/> instead.")]
	  public virtual void Invokevirtual(string owner, string name, string descriptor)
	  {
		if (api >= IOpcodes.Asm5)
		{
		  Invokevirtual(owner, name, descriptor, false);
		  return;
		}
		mv.VisitMethodInsn(IOpcodes.Invokevirtual, owner, name, descriptor);
	  }

	  /// <summary>
	  /// Generates the instruction to call the given virtual method.
	  /// </summary>
	  /// <param name="owner"> the internal name of the method's owner class (see {@link
	  ///     Type#getInternalName()}). </param>
	  /// <param name="name"> the method's name. </param>
	  /// <param name="descriptor"> the method's descriptor (see <seealso cref="Type"/>). </param>
	  /// <param name="isInterface"> if the method's owner class is an interface. </param>
	  public virtual void Invokevirtual(string owner, string name, string descriptor, bool isInterface)
	  {
		if (api < IOpcodes.Asm5)
		{
		  if (isInterface)
		  {
			throw new System.NotSupportedException("INVOKEVIRTUAL on interfaces require ASM 5");
		  }
		  Invokevirtual(owner, name, descriptor);
		  return;
		}
		mv.VisitMethodInsn(IOpcodes.Invokevirtual, owner, name, descriptor, isInterface);
	  }

	  /// <summary>
	  /// Deprecated.
	  /// </summary>
	  /// <param name="owner"> the internal name of the method's owner class. </param>
	  /// <param name="name"> the method's name. </param>
	  /// <param name="descriptor"> the method's descriptor (see <seealso cref="Type"/>). </param>
	  /// @deprecated use <seealso cref="Invokespecial(string,string,string,bool)"/> instead. 
	  [Obsolete("use <seealso cref=\"invokespecial(String, String, String, bool)\"/> instead.")]
	  public virtual void Invokespecial(string owner, string name, string descriptor)
	  {
		if (api >= IOpcodes.Asm5)
		{
		  Invokespecial(owner, name, descriptor, false);
		  return;
		}
		mv.VisitMethodInsn(IOpcodes.Invokespecial, owner, name, descriptor, false);
	  }

	  /// <summary>
	  /// Generates the instruction to call the given special method.
	  /// </summary>
	  /// <param name="owner"> the internal name of the method's owner class (see {@link
	  ///     Type#getInternalName()}). </param>
	  /// <param name="name"> the method's name. </param>
	  /// <param name="descriptor"> the method's descriptor (see <seealso cref="Type"/>). </param>
	  /// <param name="isInterface"> if the method's owner class is an interface. </param>
	  public virtual void Invokespecial(string owner, string name, string descriptor, bool isInterface)
	  {
		if (api < IOpcodes.Asm5)
		{
		  if (isInterface)
		  {
			throw new System.NotSupportedException("INVOKESPECIAL on interfaces require ASM 5");
		  }
		  Invokespecial(owner, name, descriptor);
		  return;
		}
		mv.VisitMethodInsn(IOpcodes.Invokespecial, owner, name, descriptor, isInterface);
	  }

	  /// <summary>
	  /// Deprecated.
	  /// </summary>
	  /// <param name="owner"> the internal name of the method's owner class. </param>
	  /// <param name="name"> the method's name. </param>
	  /// <param name="descriptor"> the method's descriptor (see <seealso cref="Type"/>). </param>
	  /// @deprecated use <seealso cref="Invokestatic(string,string,string,bool)"/> instead. 
	  [Obsolete("use <seealso cref=\"invokestatic(String, String, String, bool)\"/> instead.")]
	  public virtual void Invokestatic(string owner, string name, string descriptor)
	  {
		if (api >= IOpcodes.Asm5)
		{
		  Invokestatic(owner, name, descriptor, false);
		  return;
		}
		mv.VisitMethodInsn(IOpcodes.Invokestatic, owner, name, descriptor, false);
	  }

	  /// <summary>
	  /// Generates the instruction to call the given static method.
	  /// </summary>
	  /// <param name="owner"> the internal name of the method's owner class (see {@link
	  ///     Type#getInternalName()}). </param>
	  /// <param name="name"> the method's name. </param>
	  /// <param name="descriptor"> the method's descriptor (see <seealso cref="Type"/>). </param>
	  /// <param name="isInterface"> if the method's owner class is an interface. </param>
	  public virtual void Invokestatic(string owner, string name, string descriptor, bool isInterface)
	  {
		if (api < IOpcodes.Asm5)
		{
		  if (isInterface)
		  {
			throw new System.NotSupportedException("INVOKESTATIC on interfaces require ASM 5");
		  }
		  Invokestatic(owner, name, descriptor);
		  return;
		}
		mv.VisitMethodInsn(IOpcodes.Invokestatic, owner, name, descriptor, isInterface);
	  }

	  /// <summary>
	  /// Generates the instruction to call the given interface method.
	  /// </summary>
	  /// <param name="owner"> the internal name of the method's owner class (see {@link
	  ///     Type#getInternalName()}). </param>
	  /// <param name="name"> the method's name. </param>
	  /// <param name="descriptor"> the method's descriptor (see <seealso cref="Type"/>). </param>
	  public virtual void Invokeinterface(string owner, string name, string descriptor)
	  {
		mv.VisitMethodInsn(IOpcodes.Invokeinterface, owner, name, descriptor, true);
	  }

	  /// <summary>
	  /// Generates the instruction to call the given dynamic method.
	  /// </summary>
	  /// <param name="name"> the method's name. </param>
	  /// <param name="descriptor"> the method's descriptor (see <seealso cref="Type"/>). </param>
	  /// <param name="bootstrapMethodHandle"> the bootstrap method. </param>
	  /// <param name="bootstrapMethodArguments"> the bootstrap method constant arguments. Each argument must be
	  ///     an <seealso cref="Integer"/>, <seealso cref="Float"/>, <seealso cref="Long"/>, <seealso cref="Double"/>, <seealso cref="string"/>, {@link
	  ///     Type}, <seealso cref="Handle"/> or <seealso cref="ConstantDynamic"/> value. This method is allowed to modify
	  ///     the content of the array so a caller should expect that this array may change. </param>
	  public virtual void Invokedynamic(string name, string descriptor, Handle bootstrapMethodHandle, object[] bootstrapMethodArguments)
	  {
		mv.VisitInvokeDynamicInsn(name, descriptor, bootstrapMethodHandle, bootstrapMethodArguments);
	  }

	  public virtual void Anew(JType type)
	  {
		mv.VisitTypeInsn(IOpcodes.New, type.InternalName);
	  }

	  /// <summary>
	  /// Generates the instruction to create and push on the stack an array of the given type.
	  /// </summary>
	  /// <param name="type"> an array Type. </param>
	  public virtual void Newarray(JType type)
	  {
		Newarray(mv, type);
	  }

	  /// <summary>
	  /// Generates the instruction to create and push on the stack an array of the given type.
	  /// </summary>
	  /// <param name="methodVisitor"> the method visitor to use to generate the instruction. </param>
	  /// <param name="type"> an array Type. </param>
	  internal static void Newarray(MethodVisitor methodVisitor, JType type)
	  {
		int arrayType;
		switch (type.Sort)
		{
		  case JType.Boolean:
			arrayType = IOpcodes.Boolean;
			break;
		  case JType.Char:
			arrayType = IOpcodes.Char;
			break;
		  case JType.Byte:
			arrayType = IOpcodes.Byte;
			break;
		  case JType.Short:
			arrayType = IOpcodes.Short;
			break;
		  case JType.Int:
			arrayType = IOpcodes.Int;
			break;
		  case JType.Float:
			arrayType = IOpcodes.Float;
			break;
		  case JType.Long:
			arrayType = IOpcodes.Long;
			break;
		  case JType.Double:
			arrayType = IOpcodes.Double;
			break;
		  default:
			methodVisitor.VisitTypeInsn(IOpcodes.Anewarray, type.InternalName);
			return;
		}
		methodVisitor.VisitIntInsn(IOpcodes.Newarray, arrayType);
	  }

	  public virtual void Arraylength()
	  {
		mv.VisitInsn(IOpcodes.Arraylength);
	  }

	  public virtual void Athrow()
	  {
		mv.VisitInsn(IOpcodes.Athrow);
	  }

	  public virtual void Checkcast(JType type)
	  {
		mv.VisitTypeInsn(IOpcodes.Checkcast, type.InternalName);
	  }

	  public virtual void InstanceOf(JType type)
	  {
		mv.VisitTypeInsn(IOpcodes.Instanceof, type.InternalName);
	  }

	  public virtual void Monitorenter()
	  {
		mv.VisitInsn(IOpcodes.Monitorenter);
	  }

	  public virtual void Monitorexit()
	  {
		mv.VisitInsn(IOpcodes.Monitorexit);
	  }

	  public virtual void Multianewarray(string descriptor, int numDimensions)
	  {
		mv.VisitMultiANewArrayInsn(descriptor, numDimensions);
	  }

	  public virtual void Ifnull(Label label)
	  {
		mv.VisitJumpInsn(IOpcodes.Ifnull, label);
	  }

	  public virtual void Ifnonnull(Label label)
	  {
		mv.VisitJumpInsn(IOpcodes.Ifnonnull, label);
	  }

	  public virtual void Mark(Label label)
	  {
		mv.VisitLabel(label);
	  }
	}

}