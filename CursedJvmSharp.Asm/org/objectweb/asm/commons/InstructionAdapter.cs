using org.objectweb.asm;
using CursedJvmSharp.Asm.Java.IO;
using System;
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
	/// A <seealso cref="MethodVisitor"/> providing a more detailed API to generate and transform instructions.
	/// 
	/// @author Eric Bruneton
	/// </summary>
	public class InstructionAdapter : MethodVisitor
	{

	  /// <summary>
	  /// The type of the java.lang.Object class. </summary>
	  public static readonly org.objectweb.asm.JType OBJECT_TYPE = org.objectweb.asm.JType.getType("Ljava/lang/Object;");

	  /// <summary>
	  /// Constructs a new <seealso cref="InstructionAdapter"/>. <i>Subclasses must not use this constructor</i>.
	  /// Instead, they must use the <seealso cref="InstructionAdapter(int, MethodVisitor)"/> version.
	  /// </summary>
	  /// <param name="methodVisitor"> the method visitor to which this adapter delegates calls. </param>
	  /// <exception cref="IllegalStateException"> If a subclass calls this constructor. </exception>
	  public InstructionAdapter(MethodVisitor methodVisitor) : this(Opcodes.ASM9, methodVisitor)
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
	  ///     ASM}<i>x</i> values in <seealso cref="Opcodes"/>. </param>
	  /// <param name="methodVisitor"> the method visitor to which this adapter delegates calls. </param>
	  public InstructionAdapter(int api, MethodVisitor methodVisitor) : base(api, methodVisitor)
	  {
	  }

	  public override void visitInsn(int opcode)
	  {
		switch (opcode)
		{
		  case Opcodes.NOP:
			nop();
			break;
		  case Opcodes.ACONST_NULL:
			aconst(null);
			break;
		  case Opcodes.ICONST_M1:
		  case Opcodes.ICONST_0:
		  case Opcodes.ICONST_1:
		  case Opcodes.ICONST_2:
		  case Opcodes.ICONST_3:
		  case Opcodes.ICONST_4:
		  case Opcodes.ICONST_5:
			iconst(opcode - Opcodes.ICONST_0);
			break;
		  case Opcodes.LCONST_0:
		  case Opcodes.LCONST_1:
			lconst((long)(opcode - Opcodes.LCONST_0));
			break;
		  case Opcodes.FCONST_0:
		  case Opcodes.FCONST_1:
		  case Opcodes.FCONST_2:
			fconst((float)(opcode - Opcodes.FCONST_0));
			break;
		  case Opcodes.DCONST_0:
		  case Opcodes.DCONST_1:
			dconst((double)(opcode - Opcodes.DCONST_0));
			break;
		  case Opcodes.IALOAD:
			aload(org.objectweb.asm.JType.INT_TYPE);
			break;
		  case Opcodes.LALOAD:
			aload(org.objectweb.asm.JType.LONG_TYPE);
			break;
		  case Opcodes.FALOAD:
			aload(org.objectweb.asm.JType.FLOAT_TYPE);
			break;
		  case Opcodes.DALOAD:
			aload(org.objectweb.asm.JType.DOUBLE_TYPE);
			break;
		  case Opcodes.AALOAD:
			aload(OBJECT_TYPE);
			break;
		  case Opcodes.BALOAD:
			aload(org.objectweb.asm.JType.BYTE_TYPE);
			break;
		  case Opcodes.CALOAD:
			aload(org.objectweb.asm.JType.CHAR_TYPE);
			break;
		  case Opcodes.SALOAD:
			aload(org.objectweb.asm.JType.SHORT_TYPE);
			break;
		  case Opcodes.IASTORE:
			astore(org.objectweb.asm.JType.INT_TYPE);
			break;
		  case Opcodes.LASTORE:
			astore(org.objectweb.asm.JType.LONG_TYPE);
			break;
		  case Opcodes.FASTORE:
			astore(org.objectweb.asm.JType.FLOAT_TYPE);
			break;
		  case Opcodes.DASTORE:
			astore(org.objectweb.asm.JType.DOUBLE_TYPE);
			break;
		  case Opcodes.AASTORE:
			astore(OBJECT_TYPE);
			break;
		  case Opcodes.BASTORE:
			astore(org.objectweb.asm.JType.BYTE_TYPE);
			break;
		  case Opcodes.CASTORE:
			astore(org.objectweb.asm.JType.CHAR_TYPE);
			break;
		  case Opcodes.SASTORE:
			astore(org.objectweb.asm.JType.SHORT_TYPE);
			break;
		  case Opcodes.POP:
			pop();
			break;
		  case Opcodes.POP2:
			pop2();
			break;
		  case Opcodes.DUP:
			dup();
			break;
		  case Opcodes.DUP_X1:
			dupX1();
			break;
		  case Opcodes.DUP_X2:
			dupX2();
			break;
		  case Opcodes.DUP2:
			dup2();
			break;
		  case Opcodes.DUP2_X1:
			dup2X1();
			break;
		  case Opcodes.DUP2_X2:
			dup2X2();
			break;
		  case Opcodes.SWAP:
			swap();
			break;
		  case Opcodes.IADD:
			add(org.objectweb.asm.JType.INT_TYPE);
			break;
		  case Opcodes.LADD:
			add(org.objectweb.asm.JType.LONG_TYPE);
			break;
		  case Opcodes.FADD:
			add(org.objectweb.asm.JType.FLOAT_TYPE);
			break;
		  case Opcodes.DADD:
			add(org.objectweb.asm.JType.DOUBLE_TYPE);
			break;
		  case Opcodes.ISUB:
			sub(org.objectweb.asm.JType.INT_TYPE);
			break;
		  case Opcodes.LSUB:
			sub(org.objectweb.asm.JType.LONG_TYPE);
			break;
		  case Opcodes.FSUB:
			sub(org.objectweb.asm.JType.FLOAT_TYPE);
			break;
		  case Opcodes.DSUB:
			sub(org.objectweb.asm.JType.DOUBLE_TYPE);
			break;
		  case Opcodes.IMUL:
			mul(org.objectweb.asm.JType.INT_TYPE);
			break;
		  case Opcodes.LMUL:
			mul(org.objectweb.asm.JType.LONG_TYPE);
			break;
		  case Opcodes.FMUL:
			mul(org.objectweb.asm.JType.FLOAT_TYPE);
			break;
		  case Opcodes.DMUL:
			mul(org.objectweb.asm.JType.DOUBLE_TYPE);
			break;
		  case Opcodes.IDIV:
			div(org.objectweb.asm.JType.INT_TYPE);
			break;
		  case Opcodes.LDIV:
			div(org.objectweb.asm.JType.LONG_TYPE);
			break;
		  case Opcodes.FDIV:
			div(org.objectweb.asm.JType.FLOAT_TYPE);
			break;
		  case Opcodes.DDIV:
			div(org.objectweb.asm.JType.DOUBLE_TYPE);
			break;
		  case Opcodes.IREM:
			rem(org.objectweb.asm.JType.INT_TYPE);
			break;
		  case Opcodes.LREM:
			rem(org.objectweb.asm.JType.LONG_TYPE);
			break;
		  case Opcodes.FREM:
			rem(org.objectweb.asm.JType.FLOAT_TYPE);
			break;
		  case Opcodes.DREM:
			rem(org.objectweb.asm.JType.DOUBLE_TYPE);
			break;
		  case Opcodes.INEG:
			neg(org.objectweb.asm.JType.INT_TYPE);
			break;
		  case Opcodes.LNEG:
			neg(org.objectweb.asm.JType.LONG_TYPE);
			break;
		  case Opcodes.FNEG:
			neg(org.objectweb.asm.JType.FLOAT_TYPE);
			break;
		  case Opcodes.DNEG:
			neg(org.objectweb.asm.JType.DOUBLE_TYPE);
			break;
		  case Opcodes.ISHL:
			shl(org.objectweb.asm.JType.INT_TYPE);
			break;
		  case Opcodes.LSHL:
			shl(org.objectweb.asm.JType.LONG_TYPE);
			break;
		  case Opcodes.ISHR:
			shr(org.objectweb.asm.JType.INT_TYPE);
			break;
		  case Opcodes.LSHR:
			shr(org.objectweb.asm.JType.LONG_TYPE);
			break;
		  case Opcodes.IUSHR:
			ushr(org.objectweb.asm.JType.INT_TYPE);
			break;
		  case Opcodes.LUSHR:
			ushr(org.objectweb.asm.JType.LONG_TYPE);
			break;
		  case Opcodes.IAND:
			and(org.objectweb.asm.JType.INT_TYPE);
			break;
		  case Opcodes.LAND:
			and(org.objectweb.asm.JType.LONG_TYPE);
			break;
		  case Opcodes.IOR:
			or(org.objectweb.asm.JType.INT_TYPE);
			break;
		  case Opcodes.LOR:
			or(org.objectweb.asm.JType.LONG_TYPE);
			break;
		  case Opcodes.IXOR:
			xor(org.objectweb.asm.JType.INT_TYPE);
			break;
		  case Opcodes.LXOR:
			xor(org.objectweb.asm.JType.LONG_TYPE);
			break;
		  case Opcodes.I2L:
			cast(org.objectweb.asm.JType.INT_TYPE, org.objectweb.asm.JType.LONG_TYPE);
			break;
		  case Opcodes.I2F:
			cast(org.objectweb.asm.JType.INT_TYPE, org.objectweb.asm.JType.FLOAT_TYPE);
			break;
		  case Opcodes.I2D:
			cast(org.objectweb.asm.JType.INT_TYPE, org.objectweb.asm.JType.DOUBLE_TYPE);
			break;
		  case Opcodes.L2I:
			cast(org.objectweb.asm.JType.LONG_TYPE, org.objectweb.asm.JType.INT_TYPE);
			break;
		  case Opcodes.L2F:
			cast(org.objectweb.asm.JType.LONG_TYPE, org.objectweb.asm.JType.FLOAT_TYPE);
			break;
		  case Opcodes.L2D:
			cast(org.objectweb.asm.JType.LONG_TYPE, org.objectweb.asm.JType.DOUBLE_TYPE);
			break;
		  case Opcodes.F2I:
			cast(org.objectweb.asm.JType.FLOAT_TYPE, org.objectweb.asm.JType.INT_TYPE);
			break;
		  case Opcodes.F2L:
			cast(org.objectweb.asm.JType.FLOAT_TYPE, org.objectweb.asm.JType.LONG_TYPE);
			break;
		  case Opcodes.F2D:
			cast(org.objectweb.asm.JType.FLOAT_TYPE, org.objectweb.asm.JType.DOUBLE_TYPE);
			break;
		  case Opcodes.D2I:
			cast(org.objectweb.asm.JType.DOUBLE_TYPE, org.objectweb.asm.JType.INT_TYPE);
			break;
		  case Opcodes.D2L:
			cast(org.objectweb.asm.JType.DOUBLE_TYPE, org.objectweb.asm.JType.LONG_TYPE);
			break;
		  case Opcodes.D2F:
			cast(org.objectweb.asm.JType.DOUBLE_TYPE, org.objectweb.asm.JType.FLOAT_TYPE);
			break;
		  case Opcodes.I2B:
			cast(org.objectweb.asm.JType.INT_TYPE, org.objectweb.asm.JType.BYTE_TYPE);
			break;
		  case Opcodes.I2C:
			cast(org.objectweb.asm.JType.INT_TYPE, org.objectweb.asm.JType.CHAR_TYPE);
			break;
		  case Opcodes.I2S:
			cast(org.objectweb.asm.JType.INT_TYPE, org.objectweb.asm.JType.SHORT_TYPE);
			break;
		  case Opcodes.LCMP:
			lcmp();
			break;
		  case Opcodes.FCMPL:
			cmpl(org.objectweb.asm.JType.FLOAT_TYPE);
			break;
		  case Opcodes.FCMPG:
			cmpg(org.objectweb.asm.JType.FLOAT_TYPE);
			break;
		  case Opcodes.DCMPL:
			cmpl(org.objectweb.asm.JType.DOUBLE_TYPE);
			break;
		  case Opcodes.DCMPG:
			cmpg(org.objectweb.asm.JType.DOUBLE_TYPE);
			break;
		  case Opcodes.IRETURN:
			areturn(org.objectweb.asm.JType.INT_TYPE);
			break;
		  case Opcodes.LRETURN:
			areturn(org.objectweb.asm.JType.LONG_TYPE);
			break;
		  case Opcodes.FRETURN:
			areturn(org.objectweb.asm.JType.FLOAT_TYPE);
			break;
		  case Opcodes.DRETURN:
			areturn(org.objectweb.asm.JType.DOUBLE_TYPE);
			break;
		  case Opcodes.ARETURN:
			areturn(OBJECT_TYPE);
			break;
		  case Opcodes.RETURN:
			areturn(org.objectweb.asm.JType.VOID_TYPE);
			break;
		  case Opcodes.ARRAYLENGTH:
			arraylength();
			break;
		  case Opcodes.ATHROW:
			athrow();
			break;
		  case Opcodes.MONITORENTER:
			monitorenter();
			break;
		  case Opcodes.MONITOREXIT:
			monitorexit();
			break;
		  default:
			throw new System.ArgumentException();
		}
	  }

	  public override void visitIntInsn(int opcode, int operand)
	  {
		switch (opcode)
		{
		  case Opcodes.BIPUSH:
			iconst(operand);
			break;
		  case Opcodes.SIPUSH:
			iconst(operand);
			break;
		  case Opcodes.NEWARRAY:
			switch (operand)
			{
			  case Opcodes.T_BOOLEAN:
				newarray(org.objectweb.asm.JType.BOOLEAN_TYPE);
				break;
			  case Opcodes.T_CHAR:
				newarray(org.objectweb.asm.JType.CHAR_TYPE);
				break;
			  case Opcodes.T_BYTE:
				newarray(org.objectweb.asm.JType.BYTE_TYPE);
				break;
			  case Opcodes.T_SHORT:
				newarray(org.objectweb.asm.JType.SHORT_TYPE);
				break;
			  case Opcodes.T_INT:
				newarray(org.objectweb.asm.JType.INT_TYPE);
				break;
			  case Opcodes.T_FLOAT:
				newarray(org.objectweb.asm.JType.FLOAT_TYPE);
				break;
			  case Opcodes.T_LONG:
				newarray(org.objectweb.asm.JType.LONG_TYPE);
				break;
			  case Opcodes.T_DOUBLE:
				newarray(org.objectweb.asm.JType.DOUBLE_TYPE);
				break;
			  default:
				throw new System.ArgumentException();
			}
			break;
		  default:
			throw new System.ArgumentException();
		}
	  }

	  public override void visitVarInsn(int opcode, int var)
	  {
		switch (opcode)
		{
		  case Opcodes.ILOAD:
			load(var, org.objectweb.asm.JType.INT_TYPE);
			break;
		  case Opcodes.LLOAD:
			load(var, org.objectweb.asm.JType.LONG_TYPE);
			break;
		  case Opcodes.FLOAD:
			load(var, org.objectweb.asm.JType.FLOAT_TYPE);
			break;
		  case Opcodes.DLOAD:
			load(var, org.objectweb.asm.JType.DOUBLE_TYPE);
			break;
		  case Opcodes.ALOAD:
			load(var, OBJECT_TYPE);
			break;
		  case Opcodes.ISTORE:
			store(var, org.objectweb.asm.JType.INT_TYPE);
			break;
		  case Opcodes.LSTORE:
			store(var, org.objectweb.asm.JType.LONG_TYPE);
			break;
		  case Opcodes.FSTORE:
			store(var, org.objectweb.asm.JType.FLOAT_TYPE);
			break;
		  case Opcodes.DSTORE:
			store(var, org.objectweb.asm.JType.DOUBLE_TYPE);
			break;
		  case Opcodes.ASTORE:
			store(var, OBJECT_TYPE);
			break;
		  case Opcodes.RET:
			ret(var);
			break;
		  default:
			throw new System.ArgumentException();
		}
	  }

	  public override void visitTypeInsn(int opcode, string type)
	  {
		org.objectweb.asm.JType objectType = org.objectweb.asm.JType.getObjectType(type);
		switch (opcode)
		{
		  case Opcodes.NEW:
			anew(objectType);
			break;
		  case Opcodes.ANEWARRAY:
			newarray(objectType);
			break;
		  case Opcodes.CHECKCAST:
			checkcast(objectType);
			break;
		  case Opcodes.INSTANCEOF:
			instanceOf(objectType);
			break;
		  default:
			throw new System.ArgumentException();
		}
	  }

	  public override void visitFieldInsn(int opcode, string owner, string name, string descriptor)
	  {
		switch (opcode)
		{
		  case Opcodes.GETSTATIC:
			getstatic(owner, name, descriptor);
			break;
		  case Opcodes.PUTSTATIC:
			putstatic(owner, name, descriptor);
			break;
		  case Opcodes.GETFIELD:
			getfield(owner, name, descriptor);
			break;
		  case Opcodes.PUTFIELD:
			putfield(owner, name, descriptor);
			break;
		  default:
			throw new System.ArgumentException();
		}
	  }

	  public override void visitMethodInsn(int opcodeAndSource, string owner, string name, string descriptor, bool isInterface)
	  {
		if (api < Opcodes.ASM5 && (opcodeAndSource & Opcodes.SOURCE_DEPRECATED) == 0)
		{
		  // Redirect the call to the deprecated version of this method.
		  base.visitMethodInsn(opcodeAndSource, owner, name, descriptor, isInterface);
		  return;
		}
		int opcode = opcodeAndSource & ~Opcodes.SOURCE_MASK;

		switch (opcode)
		{
		  case Opcodes.INVOKESPECIAL:
			invokespecial(owner, name, descriptor, isInterface);
			break;
		  case Opcodes.INVOKEVIRTUAL:
			invokevirtual(owner, name, descriptor, isInterface);
			break;
		  case Opcodes.INVOKESTATIC:
			invokestatic(owner, name, descriptor, isInterface);
			break;
		  case Opcodes.INVOKEINTERFACE:
			invokeinterface(owner, name, descriptor);
			break;
		  default:
			throw new System.ArgumentException();
		}
	  }

	  public override void visitInvokeDynamicInsn(string name, string descriptor, Handle bootstrapMethodHandle, params object[] bootstrapMethodArguments)
	  {
		invokedynamic(name, descriptor, bootstrapMethodHandle, bootstrapMethodArguments);
	  }

	  public override void visitJumpInsn(int opcode, Label label)
	  {
		switch (opcode)
		{
		  case Opcodes.IFEQ:
			ifeq(label);
			break;
		  case Opcodes.IFNE:
			ifne(label);
			break;
		  case Opcodes.IFLT:
			iflt(label);
			break;
		  case Opcodes.IFGE:
			ifge(label);
			break;
		  case Opcodes.IFGT:
			ifgt(label);
			break;
		  case Opcodes.IFLE:
			ifle(label);
			break;
		  case Opcodes.IF_ICMPEQ:
			ificmpeq(label);
			break;
		  case Opcodes.IF_ICMPNE:
			ificmpne(label);
			break;
		  case Opcodes.IF_ICMPLT:
			ificmplt(label);
			break;
		  case Opcodes.IF_ICMPGE:
			ificmpge(label);
			break;
		  case Opcodes.IF_ICMPGT:
			ificmpgt(label);
			break;
		  case Opcodes.IF_ICMPLE:
			ificmple(label);
			break;
		  case Opcodes.IF_ACMPEQ:
			ifacmpeq(label);
			break;
		  case Opcodes.IF_ACMPNE:
			ifacmpne(label);
			break;
		  case Opcodes.GOTO:
			goTo(label);
			break;
		  case Opcodes.JSR:
			jsr(label);
			break;
		  case Opcodes.IFNULL:
			ifnull(label);
			break;
		  case Opcodes.IFNONNULL:
			ifnonnull(label);
			break;
		  default:
			throw new System.ArgumentException();
		}
	  }

	  public override void visitLabel(Label label)
	  {
		mark(label);
	  }

	  public override void visitLdcInsn(object value)
	  {
		if (api < Opcodes.ASM5 && (value is Handle || (value is org.objectweb.asm.JType && ((org.objectweb.asm.JType) value).Sort == org.objectweb.asm.JType.METHOD)))
		{
		  throw new System.NotSupportedException("This feature requires ASM5");
		}
		if (api < Opcodes.ASM7 && value is ConstantDynamic)
		{
		  throw new System.NotSupportedException("This feature requires ASM7");
		}
		if (value is int)
		{
		  iconst(((int?) value).Value);
		}
		else if (value is Byte)
		{
		  iconst(((sbyte?) value).Value);
		}
		else if (value is Character)
		{
		  iconst(((char?) value).Value);
		}
		else if (value is Short)
		{
		  iconst(((short?) value).Value);
		}
		else if (value is Boolean)
		{
		  iconst(((bool?) value).Value ? 1 : 0);
		}
		else if (value is Float)
		{
		  fconst(((float?) value).Value);
		}
		else if (value is Long)
		{
		  lconst(((long?) value).Value);
		}
		else if (value is Double)
		{
		  dconst(((double?) value).Value);
		}
		else if (value is string)
		{
		  aconst(value);
		}
		else if (value is Type)
		{
		  tconst((org.objectweb.asm.JType) value);
		}
		else if (value is Handle)
		{
		  hconst((Handle) value);
		}
		else if (value is ConstantDynamic)
		{
		  cconst((ConstantDynamic) value);
		}
		else
		{
		  throw new System.ArgumentException();
		}
	  }

	  public override void visitIincInsn(int var, int increment)
	  {
		iinc(var, increment);
	  }

	  public override void visitTableSwitchInsn(int min, int max, Label dflt, params Label[] labels)
	  {
		tableswitch(min, max, dflt, labels);
	  }

	  public override void visitLookupSwitchInsn(Label dflt, int[] keys, Label[] labels)
	  {
		lookupswitch(dflt, keys, labels);
	  }

	  public override void visitMultiANewArrayInsn(string descriptor, int numDimensions)
	  {
		multianewarray(descriptor, numDimensions);
	  }

	  // -----------------------------------------------------------------------------------------------

	  /// <summary>
	  /// Generates a nop instruction. </summary>
	  public virtual void nop()
	  {
		mv.visitInsn(Opcodes.NOP);
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
	  public virtual void aconst(object value)
	  {
		if (value == null)
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
	  /// <param name="intValue"> the constant to be pushed on the stack. </param>
	  public virtual void iconst(int intValue)
	  {
		if (intValue >= -1 && intValue <= 5)
		{
		  mv.visitInsn(Opcodes.ICONST_0 + intValue);
		}
		else if (intValue >= sbyte.MinValue && intValue <= sbyte.MaxValue)
		{
		  mv.visitIntInsn(Opcodes.BIPUSH, intValue);
		}
		else if (intValue >= short.MinValue && intValue <= short.MaxValue)
		{
		  mv.visitIntInsn(Opcodes.SIPUSH, intValue);
		}
		else
		{
		  mv.visitLdcInsn(intValue);
		}
	  }

	  /// <summary>
	  /// Generates the instruction to push the given value on the stack.
	  /// </summary>
	  /// <param name="longValue"> the constant to be pushed on the stack. </param>
	  public virtual void lconst(long longValue)
	  {
		if (longValue == 0L || longValue == 1L)
		{
		  mv.visitInsn(Opcodes.LCONST_0 + (int) longValue);
		}
		else
		{
		  mv.visitLdcInsn(longValue);
		}
	  }

	  /// <summary>
	  /// Generates the instruction to push the given value on the stack.
	  /// </summary>
	  /// <param name="floatValue"> the constant to be pushed on the stack. </param>
	  public virtual void fconst(float floatValue)
	  {
		int bits = BitConverter.SingleToInt32Bits(floatValue);
		if (bits == 0L || bits == 0x3F800000 || bits == 0x40000000)
		{ // 0..2
		  mv.visitInsn(Opcodes.FCONST_0 + (int) floatValue);
		}
		else
		{
		  mv.visitLdcInsn(floatValue);
		}
	  }

	  /// <summary>
	  /// Generates the instruction to push the given value on the stack.
	  /// </summary>
	  /// <param name="doubleValue"> the constant to be pushed on the stack. </param>
	  public virtual void dconst(double doubleValue)
	  {
		long bits = System.BitConverter.DoubleToInt64Bits(doubleValue);
		if (bits == 0L || bits == 0x3FF0000000000000L)
		{ // +0.0d and 1.0d
		  mv.visitInsn(Opcodes.DCONST_0 + (int) doubleValue);
		}
		else
		{
		  mv.visitLdcInsn(doubleValue);
		}
	  }

	  /// <summary>
	  /// Generates the instruction to push the given type on the stack.
	  /// </summary>
	  /// <param name="type"> the type to be pushed on the stack. </param>
	  public virtual void tconst(org.objectweb.asm.JType type)
	  {
		mv.visitLdcInsn(type);
	  }

	  /// <summary>
	  /// Generates the instruction to push the given handle on the stack.
	  /// </summary>
	  /// <param name="handle"> the handle to be pushed on the stack. </param>
	  public virtual void hconst(Handle handle)
	  {
		mv.visitLdcInsn(handle);
	  }

	  /// <summary>
	  /// Generates the instruction to push the given constant dynamic on the stack.
	  /// </summary>
	  /// <param name="constantDynamic"> the constant dynamic to be pushed on the stack. </param>
	  public virtual void cconst(ConstantDynamic constantDynamic)
	  {
		mv.visitLdcInsn(constantDynamic);
	  }

	  public virtual void load(int var, org.objectweb.asm.JType type)
	  {
		mv.visitVarInsn(type.getOpcode(Opcodes.ILOAD), var);
	  }

	  public virtual void aload(org.objectweb.asm.JType type)
	  {
		mv.visitInsn(type.getOpcode(Opcodes.IALOAD));
	  }

	  public virtual void store(int var, org.objectweb.asm.JType type)
	  {
		mv.visitVarInsn(type.getOpcode(Opcodes.ISTORE), var);
	  }

	  public virtual void astore(org.objectweb.asm.JType type)
	  {
		mv.visitInsn(type.getOpcode(Opcodes.IASTORE));
	  }

	  public virtual void pop()
	  {
		mv.visitInsn(Opcodes.POP);
	  }

	  public virtual void pop2()
	  {
		mv.visitInsn(Opcodes.POP2);
	  }

	  public virtual void dup()
	  {
		mv.visitInsn(Opcodes.DUP);
	  }

	  public virtual void dup2()
	  {
		mv.visitInsn(Opcodes.DUP2);
	  }

	  public virtual void dupX1()
	  {
		mv.visitInsn(Opcodes.DUP_X1);
	  }

	  public virtual void dupX2()
	  {
		mv.visitInsn(Opcodes.DUP_X2);
	  }

	  public virtual void dup2X1()
	  {
		mv.visitInsn(Opcodes.DUP2_X1);
	  }

	  public virtual void dup2X2()
	  {
		mv.visitInsn(Opcodes.DUP2_X2);
	  }

	  public virtual void swap()
	  {
		mv.visitInsn(Opcodes.SWAP);
	  }

	  public virtual void add(org.objectweb.asm.JType type)
	  {
		mv.visitInsn(type.getOpcode(Opcodes.IADD));
	  }

	  public virtual void sub(org.objectweb.asm.JType type)
	  {
		mv.visitInsn(type.getOpcode(Opcodes.ISUB));
	  }

	  public virtual void mul(org.objectweb.asm.JType type)
	  {
		mv.visitInsn(type.getOpcode(Opcodes.IMUL));
	  }

	  public virtual void div(org.objectweb.asm.JType type)
	  {
		mv.visitInsn(type.getOpcode(Opcodes.IDIV));
	  }

	  public virtual void rem(org.objectweb.asm.JType type)
	  {
		mv.visitInsn(type.getOpcode(Opcodes.IREM));
	  }

	  public virtual void neg(org.objectweb.asm.JType type)
	  {
		mv.visitInsn(type.getOpcode(Opcodes.INEG));
	  }

	  public virtual void shl(org.objectweb.asm.JType type)
	  {
		mv.visitInsn(type.getOpcode(Opcodes.ISHL));
	  }

	  public virtual void shr(org.objectweb.asm.JType type)
	  {
		mv.visitInsn(type.getOpcode(Opcodes.ISHR));
	  }

	  public virtual void ushr(org.objectweb.asm.JType type)
	  {
		mv.visitInsn(type.getOpcode(Opcodes.IUSHR));
	  }

	  public virtual void and(org.objectweb.asm.JType type)
	  {
		mv.visitInsn(type.getOpcode(Opcodes.IAND));
	  }

	  public virtual void or(org.objectweb.asm.JType type)
	  {
		mv.visitInsn(type.getOpcode(Opcodes.IOR));
	  }

	  public virtual void xor(org.objectweb.asm.JType type)
	  {
		mv.visitInsn(type.getOpcode(Opcodes.IXOR));
	  }

	  public virtual void iinc(int var, int increment)
	  {
		mv.visitIincInsn(var, increment);
	  }

	  /// <summary>
	  /// Generates the instruction to cast from the first given type to the other.
	  /// </summary>
	  /// <param name="from"> a Type. </param>
	  /// <param name="to"> a Type. </param>
	  public virtual void cast(org.objectweb.asm.JType from, org.objectweb.asm.JType to)
	  {
		cast(mv, from, to);
	  }

	  /// <summary>
	  /// Generates the instruction to cast from the first given type to the other.
	  /// </summary>
	  /// <param name="methodVisitor"> the method visitor to use to generate the instruction. </param>
	  /// <param name="from"> a Type. </param>
	  /// <param name="to"> a Type. </param>
	  internal static void cast(MethodVisitor methodVisitor, org.objectweb.asm.JType from, org.objectweb.asm.JType to)
	  {
		if (from != to)
		{
		  if (from == org.objectweb.asm.JType.DOUBLE_TYPE)
		  {
			if (to == org.objectweb.asm.JType.FLOAT_TYPE)
			{
			  methodVisitor.visitInsn(Opcodes.D2F);
			}
			else if (to == org.objectweb.asm.JType.LONG_TYPE)
			{
			  methodVisitor.visitInsn(Opcodes.D2L);
			}
			else
			{
			  methodVisitor.visitInsn(Opcodes.D2I);
			  cast(methodVisitor, org.objectweb.asm.JType.INT_TYPE, to);
			}
		  }
		  else if (from == org.objectweb.asm.JType.FLOAT_TYPE)
		  {
			if (to == org.objectweb.asm.JType.DOUBLE_TYPE)
			{
			  methodVisitor.visitInsn(Opcodes.F2D);
			}
			else if (to == org.objectweb.asm.JType.LONG_TYPE)
			{
			  methodVisitor.visitInsn(Opcodes.F2L);
			}
			else
			{
			  methodVisitor.visitInsn(Opcodes.F2I);
			  cast(methodVisitor, org.objectweb.asm.JType.INT_TYPE, to);
			}
		  }
		  else if (from == org.objectweb.asm.JType.LONG_TYPE)
		  {
			if (to == org.objectweb.asm.JType.DOUBLE_TYPE)
			{
			  methodVisitor.visitInsn(Opcodes.L2D);
			}
			else if (to == org.objectweb.asm.JType.FLOAT_TYPE)
			{
			  methodVisitor.visitInsn(Opcodes.L2F);
			}
			else
			{
			  methodVisitor.visitInsn(Opcodes.L2I);
			  cast(methodVisitor, org.objectweb.asm.JType.INT_TYPE, to);
			}
		  }
		  else
		  {
			if (to == org.objectweb.asm.JType.BYTE_TYPE)
			{
			  methodVisitor.visitInsn(Opcodes.I2B);
			}
			else if (to == org.objectweb.asm.JType.CHAR_TYPE)
			{
			  methodVisitor.visitInsn(Opcodes.I2C);
			}
			else if (to == org.objectweb.asm.JType.DOUBLE_TYPE)
			{
			  methodVisitor.visitInsn(Opcodes.I2D);
			}
			else if (to == org.objectweb.asm.JType.FLOAT_TYPE)
			{
			  methodVisitor.visitInsn(Opcodes.I2F);
			}
			else if (to == org.objectweb.asm.JType.LONG_TYPE)
			{
			  methodVisitor.visitInsn(Opcodes.I2L);
			}
			else if (to == org.objectweb.asm.JType.SHORT_TYPE)
			{
			  methodVisitor.visitInsn(Opcodes.I2S);
			}
		  }
		}
	  }

	  public virtual void lcmp()
	  {
		mv.visitInsn(Opcodes.LCMP);
	  }

	  public virtual void cmpl(org.objectweb.asm.JType type)
	  {
		mv.visitInsn(type == org.objectweb.asm.JType.FLOAT_TYPE ? Opcodes.FCMPL : Opcodes.DCMPL);
	  }

	  public virtual void cmpg(org.objectweb.asm.JType type)
	  {
		mv.visitInsn(type == org.objectweb.asm.JType.FLOAT_TYPE ? Opcodes.FCMPG : Opcodes.DCMPG);
	  }

	  public virtual void ifeq(Label label)
	  {
		mv.visitJumpInsn(Opcodes.IFEQ, label);
	  }

	  public virtual void ifne(Label label)
	  {
		mv.visitJumpInsn(Opcodes.IFNE, label);
	  }

	  public virtual void iflt(Label label)
	  {
		mv.visitJumpInsn(Opcodes.IFLT, label);
	  }

	  public virtual void ifge(Label label)
	  {
		mv.visitJumpInsn(Opcodes.IFGE, label);
	  }

	  public virtual void ifgt(Label label)
	  {
		mv.visitJumpInsn(Opcodes.IFGT, label);
	  }

	  public virtual void ifle(Label label)
	  {
		mv.visitJumpInsn(Opcodes.IFLE, label);
	  }

	  public virtual void ificmpeq(Label label)
	  {
		mv.visitJumpInsn(Opcodes.IF_ICMPEQ, label);
	  }

	  public virtual void ificmpne(Label label)
	  {
		mv.visitJumpInsn(Opcodes.IF_ICMPNE, label);
	  }

	  public virtual void ificmplt(Label label)
	  {
		mv.visitJumpInsn(Opcodes.IF_ICMPLT, label);
	  }

	  public virtual void ificmpge(Label label)
	  {
		mv.visitJumpInsn(Opcodes.IF_ICMPGE, label);
	  }

	  public virtual void ificmpgt(Label label)
	  {
		mv.visitJumpInsn(Opcodes.IF_ICMPGT, label);
	  }

	  public virtual void ificmple(Label label)
	  {
		mv.visitJumpInsn(Opcodes.IF_ICMPLE, label);
	  }

	  public virtual void ifacmpeq(Label label)
	  {
		mv.visitJumpInsn(Opcodes.IF_ACMPEQ, label);
	  }

	  public virtual void ifacmpne(Label label)
	  {
		mv.visitJumpInsn(Opcodes.IF_ACMPNE, label);
	  }

	  public virtual void goTo(Label label)
	  {
		mv.visitJumpInsn(Opcodes.GOTO, label);
	  }

	  public virtual void jsr(Label label)
	  {
		mv.visitJumpInsn(Opcodes.JSR, label);
	  }

	  public virtual void ret(int var)
	  {
		mv.visitVarInsn(Opcodes.RET, var);
	  }

	  public virtual void tableswitch(int min, int max, Label dflt, params Label[] labels)
	  {
		mv.visitTableSwitchInsn(min, max, dflt, labels);
	  }

	  public virtual void lookupswitch(Label dflt, int[] keys, Label[] labels)
	  {
		mv.visitLookupSwitchInsn(dflt, keys, labels);
	  }

	  public virtual void areturn(org.objectweb.asm.JType type)
	  {
		mv.visitInsn(type.getOpcode(Opcodes.IRETURN));
	  }

	  public virtual void getstatic(string owner, string name, string descriptor)
	  {
		mv.visitFieldInsn(Opcodes.GETSTATIC, owner, name, descriptor);
	  }

	  public virtual void putstatic(string owner, string name, string descriptor)
	  {
		mv.visitFieldInsn(Opcodes.PUTSTATIC, owner, name, descriptor);
	  }

	  public virtual void getfield(string owner, string name, string descriptor)
	  {
		mv.visitFieldInsn(Opcodes.GETFIELD, owner, name, descriptor);
	  }

	  public virtual void putfield(string owner, string name, string descriptor)
	  {
		mv.visitFieldInsn(Opcodes.PUTFIELD, owner, name, descriptor);
	  }

	  /// <summary>
	  /// Deprecated.
	  /// </summary>
	  /// <param name="owner"> the internal name of the method's owner class. </param>
	  /// <param name="name"> the method's name. </param>
	  /// <param name="descriptor"> the method's descriptor (see <seealso cref="Type"/>). </param>
	  /// @deprecated use <seealso cref="invokevirtual(String, String, String, bool)"/> instead. 
	  [Obsolete("use <seealso cref=\"invokevirtual(String, String, String, bool)\"/> instead.")]
	  public virtual void invokevirtual(string owner, string name, string descriptor)
	  {
		if (api >= Opcodes.ASM5)
		{
		  invokevirtual(owner, name, descriptor, false);
		  return;
		}
		mv.visitMethodInsn(Opcodes.INVOKEVIRTUAL, owner, name, descriptor);
	  }

	  /// <summary>
	  /// Generates the instruction to call the given virtual method.
	  /// </summary>
	  /// <param name="owner"> the internal name of the method's owner class (see {@link
	  ///     Type#getInternalName()}). </param>
	  /// <param name="name"> the method's name. </param>
	  /// <param name="descriptor"> the method's descriptor (see <seealso cref="Type"/>). </param>
	  /// <param name="isInterface"> if the method's owner class is an interface. </param>
	  public virtual void invokevirtual(string owner, string name, string descriptor, bool isInterface)
	  {
		if (api < Opcodes.ASM5)
		{
		  if (isInterface)
		  {
			throw new System.NotSupportedException("INVOKEVIRTUAL on interfaces require ASM 5");
		  }
		  invokevirtual(owner, name, descriptor);
		  return;
		}
		mv.visitMethodInsn(Opcodes.INVOKEVIRTUAL, owner, name, descriptor, isInterface);
	  }

	  /// <summary>
	  /// Deprecated.
	  /// </summary>
	  /// <param name="owner"> the internal name of the method's owner class. </param>
	  /// <param name="name"> the method's name. </param>
	  /// <param name="descriptor"> the method's descriptor (see <seealso cref="Type"/>). </param>
	  /// @deprecated use <seealso cref="invokespecial(String, String, String, bool)"/> instead. 
	  [Obsolete("use <seealso cref=\"invokespecial(String, String, String, bool)\"/> instead.")]
	  public virtual void invokespecial(string owner, string name, string descriptor)
	  {
		if (api >= Opcodes.ASM5)
		{
		  invokespecial(owner, name, descriptor, false);
		  return;
		}
		mv.visitMethodInsn(Opcodes.INVOKESPECIAL, owner, name, descriptor, false);
	  }

	  /// <summary>
	  /// Generates the instruction to call the given special method.
	  /// </summary>
	  /// <param name="owner"> the internal name of the method's owner class (see {@link
	  ///     Type#getInternalName()}). </param>
	  /// <param name="name"> the method's name. </param>
	  /// <param name="descriptor"> the method's descriptor (see <seealso cref="Type"/>). </param>
	  /// <param name="isInterface"> if the method's owner class is an interface. </param>
	  public virtual void invokespecial(string owner, string name, string descriptor, bool isInterface)
	  {
		if (api < Opcodes.ASM5)
		{
		  if (isInterface)
		  {
			throw new System.NotSupportedException("INVOKESPECIAL on interfaces require ASM 5");
		  }
		  invokespecial(owner, name, descriptor);
		  return;
		}
		mv.visitMethodInsn(Opcodes.INVOKESPECIAL, owner, name, descriptor, isInterface);
	  }

	  /// <summary>
	  /// Deprecated.
	  /// </summary>
	  /// <param name="owner"> the internal name of the method's owner class. </param>
	  /// <param name="name"> the method's name. </param>
	  /// <param name="descriptor"> the method's descriptor (see <seealso cref="Type"/>). </param>
	  /// @deprecated use <seealso cref="invokestatic(String, String, String, bool)"/> instead. 
	  [Obsolete("use <seealso cref=\"invokestatic(String, String, String, bool)\"/> instead.")]
	  public virtual void invokestatic(string owner, string name, string descriptor)
	  {
		if (api >= Opcodes.ASM5)
		{
		  invokestatic(owner, name, descriptor, false);
		  return;
		}
		mv.visitMethodInsn(Opcodes.INVOKESTATIC, owner, name, descriptor, false);
	  }

	  /// <summary>
	  /// Generates the instruction to call the given static method.
	  /// </summary>
	  /// <param name="owner"> the internal name of the method's owner class (see {@link
	  ///     Type#getInternalName()}). </param>
	  /// <param name="name"> the method's name. </param>
	  /// <param name="descriptor"> the method's descriptor (see <seealso cref="Type"/>). </param>
	  /// <param name="isInterface"> if the method's owner class is an interface. </param>
	  public virtual void invokestatic(string owner, string name, string descriptor, bool isInterface)
	  {
		if (api < Opcodes.ASM5)
		{
		  if (isInterface)
		  {
			throw new System.NotSupportedException("INVOKESTATIC on interfaces require ASM 5");
		  }
		  invokestatic(owner, name, descriptor);
		  return;
		}
		mv.visitMethodInsn(Opcodes.INVOKESTATIC, owner, name, descriptor, isInterface);
	  }

	  /// <summary>
	  /// Generates the instruction to call the given interface method.
	  /// </summary>
	  /// <param name="owner"> the internal name of the method's owner class (see {@link
	  ///     Type#getInternalName()}). </param>
	  /// <param name="name"> the method's name. </param>
	  /// <param name="descriptor"> the method's descriptor (see <seealso cref="Type"/>). </param>
	  public virtual void invokeinterface(string owner, string name, string descriptor)
	  {
		mv.visitMethodInsn(Opcodes.INVOKEINTERFACE, owner, name, descriptor, true);
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
	  public virtual void invokedynamic(string name, string descriptor, Handle bootstrapMethodHandle, object[] bootstrapMethodArguments)
	  {
		mv.visitInvokeDynamicInsn(name, descriptor, bootstrapMethodHandle, bootstrapMethodArguments);
	  }

	  public virtual void anew(org.objectweb.asm.JType type)
	  {
		mv.visitTypeInsn(Opcodes.NEW, type.InternalName);
	  }

	  /// <summary>
	  /// Generates the instruction to create and push on the stack an array of the given type.
	  /// </summary>
	  /// <param name="type"> an array Type. </param>
	  public virtual void newarray(org.objectweb.asm.JType type)
	  {
		newarray(mv, type);
	  }

	  /// <summary>
	  /// Generates the instruction to create and push on the stack an array of the given type.
	  /// </summary>
	  /// <param name="methodVisitor"> the method visitor to use to generate the instruction. </param>
	  /// <param name="type"> an array Type. </param>
	  internal static void newarray(MethodVisitor methodVisitor, org.objectweb.asm.JType type)
	  {
		int arrayType;
		switch (type.Sort)
		{
		  case org.objectweb.asm.JType.BOOLEAN:
			arrayType = Opcodes.T_BOOLEAN;
			break;
		  case org.objectweb.asm.JType.CHAR:
			arrayType = Opcodes.T_CHAR;
			break;
		  case org.objectweb.asm.JType.BYTE:
			arrayType = Opcodes.T_BYTE;
			break;
		  case org.objectweb.asm.JType.SHORT:
			arrayType = Opcodes.T_SHORT;
			break;
		  case org.objectweb.asm.JType.INT:
			arrayType = Opcodes.T_INT;
			break;
		  case org.objectweb.asm.JType.FLOAT:
			arrayType = Opcodes.T_FLOAT;
			break;
		  case org.objectweb.asm.JType.LONG:
			arrayType = Opcodes.T_LONG;
			break;
		  case org.objectweb.asm.JType.DOUBLE:
			arrayType = Opcodes.T_DOUBLE;
			break;
		  default:
			methodVisitor.visitTypeInsn(Opcodes.ANEWARRAY, type.InternalName);
			return;
		}
		methodVisitor.visitIntInsn(Opcodes.NEWARRAY, arrayType);
	  }

	  public virtual void arraylength()
	  {
		mv.visitInsn(Opcodes.ARRAYLENGTH);
	  }

	  public virtual void athrow()
	  {
		mv.visitInsn(Opcodes.ATHROW);
	  }

	  public virtual void checkcast(org.objectweb.asm.JType type)
	  {
		mv.visitTypeInsn(Opcodes.CHECKCAST, type.InternalName);
	  }

	  public virtual void instanceOf(org.objectweb.asm.JType type)
	  {
		mv.visitTypeInsn(Opcodes.INSTANCEOF, type.InternalName);
	  }

	  public virtual void monitorenter()
	  {
		mv.visitInsn(Opcodes.MONITORENTER);
	  }

	  public virtual void monitorexit()
	  {
		mv.visitInsn(Opcodes.MONITOREXIT);
	  }

	  public virtual void multianewarray(string descriptor, int numDimensions)
	  {
		mv.visitMultiANewArrayInsn(descriptor, numDimensions);
	  }

	  public virtual void ifnull(Label label)
	  {
		mv.visitJumpInsn(Opcodes.IFNULL, label);
	  }

	  public virtual void ifnonnull(Label label)
	  {
		mv.visitJumpInsn(Opcodes.IFNONNULL, label);
	  }

	  public virtual void mark(Label label)
	  {
		mv.visitLabel(label);
	  }
	}

}