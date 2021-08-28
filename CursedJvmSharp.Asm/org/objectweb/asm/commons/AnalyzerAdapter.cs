using org.objectweb.asm;
using CursedJvmSharp.Asm.Java.IO;
using System;
using System.Collections.Generic;
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
	/// A <seealso cref="MethodVisitor"/> that keeps track of stack map frame changes between {@link
	/// #visitFrame(int, int, Object[], int, Object[])} calls. This adapter must be used with the {@link
	/// org.objectweb.asm.ClassReader#EXPAND_FRAMES} option. Each visit<i>X</i> instruction delegates to
	/// the next visitor in the chain, if any, and then simulates the effect of this instruction on the
	/// stack map frame, represented by <seealso cref="locals"/> and <seealso cref="stack"/>. The next visitor in the chain
	/// can get the state of the stack map frame <i>before</i> each instruction by reading the value of
	/// these fields in its visit<i>X</i> methods (this requires a reference to the AnalyzerAdapter that
	/// is before it in the chain). If this adapter is used with a class that does not contain stack map
	/// table attributes (i.e., pre Java 6 classes) then this adapter may not be able to compute the
	/// stack map frame for each instruction. In this case no exception is thrown but the <seealso cref="locals"/>
	/// and <seealso cref="stack"/> fields will be null for these instructions.
	/// 
	/// @author Eric Bruneton
	/// </summary>
	public class AnalyzerAdapter : MethodVisitor
	{

	  /// <summary>
	  /// The local variable slots for the current execution frame. Primitive types are represented by
	  /// <seealso cref="Opcodes.TOP"/>, <seealso cref="Opcodes.INTEGER"/>, <seealso cref="Opcodes.FLOAT"/>, <seealso cref="Opcodes.LONG"/>,
	  /// <seealso cref="Opcodes.DOUBLE"/>,<seealso cref="Opcodes.NULL"/> or <seealso cref="Opcodes.UNINITIALIZED_THIS"/> (long and
	  /// double are represented by two elements, the second one being TOP). Reference types are
	  /// represented by String objects (representing internal names), and uninitialized types by Label
	  /// objects (this label designates the NEW instruction that created this uninitialized value). This
	  /// field is {@literal null} for unreachable instructions.
	  /// </summary>
	  public List<object> locals;

	  /// <summary>
	  /// The operand stack slots for the current execution frame. Primitive types are represented by
	  /// <seealso cref="Opcodes.TOP"/>, <seealso cref="Opcodes.INTEGER"/>, <seealso cref="Opcodes.FLOAT"/>, <seealso cref="Opcodes.LONG"/>,
	  /// <seealso cref="Opcodes.DOUBLE"/>,<seealso cref="Opcodes.NULL"/> or <seealso cref="Opcodes.UNINITIALIZED_THIS"/> (long and
	  /// double are represented by two elements, the second one being TOP). Reference types are
	  /// represented by String objects (representing internal names), and uninitialized types by Label
	  /// objects (this label designates the NEW instruction that created this uninitialized value). This
	  /// field is {@literal null} for unreachable instructions.
	  /// </summary>
	  public List<object> stack;

	  /// <summary>
	  /// The labels that designate the next instruction to be visited. May be {@literal null}. </summary>
	  private List<Label> labels;

	  /// <summary>
	  /// The uninitialized types in the current execution frame. This map associates internal names to
	  /// Label objects. Each label designates a NEW instruction that created the currently uninitialized
	  /// types, and the associated internal name represents the NEW operand, i.e. the final, initialized
	  /// type value.
	  /// </summary>
	  public IDictionary<object, object> uninitializedTypes;

	  /// <summary>
	  /// The maximum stack size of this method. </summary>
	  private int maxStack;

	  /// <summary>
	  /// The maximum number of local variables of this method. </summary>
	  private int maxLocals;

	  /// <summary>
	  /// The owner's class name. </summary>
	  private string owner;

	  /// <summary>
	  /// Constructs a new <seealso cref="AnalyzerAdapter"/>. <i>Subclasses must not use this constructor</i>.
	  /// Instead, they must use the {@link #AnalyzerAdapter(int, String, int, String, String,
	  /// MethodVisitor)} version.
	  /// </summary>
	  /// <param name="owner"> the owner's class name. </param>
	  /// <param name="access"> the method's access flags (see <seealso cref="Opcodes"/>). </param>
	  /// <param name="name"> the method's name. </param>
	  /// <param name="descriptor"> the method's descriptor (see <seealso cref="Type"/>). </param>
	  /// <param name="methodVisitor"> the method visitor to which this adapter delegates calls. May be {@literal
	  ///     null}. </param>
	  /// <exception cref="IllegalStateException"> If a subclass calls this constructor. </exception>
	  public AnalyzerAdapter(string owner, int access, string name, string descriptor, MethodVisitor methodVisitor) : this(Opcodes.ASM9, owner, access, name, descriptor, methodVisitor)
	  {
		if (this.GetType() != typeof(AnalyzerAdapter))
		{
		  throw new System.InvalidOperationException();
		}
	  }

	  /// <summary>
	  /// Constructs a new <seealso cref="AnalyzerAdapter"/>.
	  /// </summary>
	  /// <param name="api"> the ASM API version implemented by this visitor. Must be one of the {@code
	  ///     ASM}<i>x</i> values in <seealso cref="Opcodes"/>. </param>
	  /// <param name="owner"> the owner's class name. </param>
	  /// <param name="access"> the method's access flags (see <seealso cref="Opcodes"/>). </param>
	  /// <param name="name"> the method's name. </param>
	  /// <param name="descriptor"> the method's descriptor (see <seealso cref="Type"/>). </param>
	  /// <param name="methodVisitor"> the method visitor to which this adapter delegates calls. May be {@literal
	  ///     null}. </param>
	  public AnalyzerAdapter(int api, string owner, int access, string name, string descriptor, MethodVisitor methodVisitor) : base(api, methodVisitor)
	  {
		this.owner = owner;
		locals = new List<object>();
		stack = new List<object>();
		uninitializedTypes = new Dictionary<object, object>();

		if ((access & Opcodes.ACC_STATIC) == 0)
		{
		  if ("<init>".Equals(name))
		  {
			locals.Add(Opcodes.UNINITIALIZED_THIS);
		  }
		  else
		  {
			locals.Add(owner);
		  }
		}
		foreach (org.objectweb.asm.JType argumentType in org.objectweb.asm.JType.getArgumentTypes(descriptor))
		{
		  switch (argumentType.Sort)
		  {
			case org.objectweb.asm.JType.BOOLEAN:
			case org.objectweb.asm.JType.CHAR:
			case org.objectweb.asm.JType.BYTE:
			case org.objectweb.asm.JType.SHORT:
			case org.objectweb.asm.JType.INT:
			  locals.Add(Opcodes.INTEGER);
			  break;
			case org.objectweb.asm.JType.FLOAT:
			  locals.Add(Opcodes.FLOAT);
			  break;
			case org.objectweb.asm.JType.LONG:
			  locals.Add(Opcodes.LONG);
			  locals.Add(Opcodes.TOP);
			  break;
			case org.objectweb.asm.JType.DOUBLE:
			  locals.Add(Opcodes.DOUBLE);
			  locals.Add(Opcodes.TOP);
			  break;
			case org.objectweb.asm.JType.ARRAY:
			  locals.Add(argumentType.Descriptor);
			  break;
			case org.objectweb.asm.JType.OBJECT:
			  locals.Add(argumentType.InternalName);
			  break;
			default:
			  throw new ();
		  }
		}
		maxLocals = locals.Count;
	  }

	  public override void visitFrame(int type, int numLocal, object[] local, int numStack, object[] stack)
	  {
		if (type != Opcodes.F_NEW)
		{ // Uncompressed frame.
		  throw new System.ArgumentException("AnalyzerAdapter only accepts expanded frames (see ClassReader.EXPAND_FRAMES)");
		}

		base.visitFrame(type, numLocal, local, numStack, stack);

		if (this.locals != null)
		{
		  this.locals.Clear();
		  this.stack.Clear();
		}
		else
		{
		  this.locals = new List<object>();
		  this.stack = new List<object>();
		}
		visitFrameTypes(numLocal, local, this.locals);
		visitFrameTypes(numStack, stack, this.stack);
		maxLocals = Math.Max(maxLocals, this.locals.Count);
		maxStack = Math.Max(maxStack, this.stack.Count);
	  }

	  private static void visitFrameTypes(int numTypes, object[] frameTypes, List<object> result)
	  {
		for (int i = 0; i < numTypes; ++i)
		{
		  object frameType = frameTypes[i];
		  result.Add(frameType);
		  if (Equals(frameType, Opcodes.LONG) || Equals(frameType, Opcodes.DOUBLE))
		  {
			result.Add(Opcodes.TOP);
		  }
		}
	  }

	  public override void visitInsn(int opcode)
	  {
		base.visitInsn(opcode);
		execute(opcode, 0, null);
		if ((opcode >= Opcodes.IRETURN && opcode <= Opcodes.RETURN) || opcode == Opcodes.ATHROW)
		{
		  this.locals = null;
		  this.stack = null;
		}
	  }

	  public override void visitIntInsn(int opcode, int operand)
	  {
		base.visitIntInsn(opcode, operand);
		execute(opcode, operand, null);
	  }

	  public override void visitVarInsn(int opcode, int var)
	  {
		base.visitVarInsn(opcode, var);
		bool isLongOrDouble = opcode == Opcodes.LLOAD || opcode == Opcodes.DLOAD || opcode == Opcodes.LSTORE || opcode == Opcodes.DSTORE;
		maxLocals = Math.Max(maxLocals, var + (isLongOrDouble ? 2 : 1));
		execute(opcode, var, null);
	  }

	  public override void visitTypeInsn(int opcode, string type)
	  {
		if (opcode == Opcodes.NEW)
		{
		  if (labels == null)
		  {
			Label label = new Label();
			labels = new List<Label>(3);
			labels.Add(label);
			if (mv != null)
			{
			  mv.visitLabel(label);
			}
		  }
		  foreach (Label label in labels)
		  {
			uninitializedTypes[label] = type;
		  }
		}
		base.visitTypeInsn(opcode, type);
		execute(opcode, 0, type);
	  }

	  public override void visitFieldInsn(int opcode, string owner, string name, string descriptor)
	  {
		base.visitFieldInsn(opcode, owner, name, descriptor);
		execute(opcode, 0, descriptor);
	  }

	  public override void visitMethodInsn(int opcodeAndSource, string owner, string name, string descriptor, bool isInterface)
	  {
		if (api < Opcodes.ASM5 && (opcodeAndSource & Opcodes.SOURCE_DEPRECATED) == 0)
		{
		  // Redirect the call to the deprecated version of this method.
		  base.visitMethodInsn(opcodeAndSource, owner, name, descriptor, isInterface);
		  return;
		}
		base.visitMethodInsn(opcodeAndSource, owner, name, descriptor, isInterface);
		int opcode = opcodeAndSource & ~Opcodes.SOURCE_MASK;

		if (this.locals == null)
		{
		  labels = null;
		  return;
		}
		pop(descriptor);
		if (opcode != Opcodes.INVOKESTATIC)
		{
		  object value = pop();
		  if (opcode == Opcodes.INVOKESPECIAL && name.Equals("<init>"))
		  {
			object initializedValue;
			if (Equals(value, Opcodes.UNINITIALIZED_THIS))
			{
			  initializedValue = this.owner;
			}
			else
			{
			  initializedValue = uninitializedTypes.GetValueOrNull(value);
			}
			for (int i = 0; i < locals.Count; ++i)
			{
			  if (locals[i] == value)
			  {
				locals[i] = initializedValue;
			  }
			}
			for (int i = 0; i < stack.Count; ++i)
			{
			  if (stack[i] == value)
			  {
				stack[i] = initializedValue;
			  }
			}
		  }
		}
		pushDescriptor(descriptor);
		labels = null;
	  }

	  public override void visitInvokeDynamicInsn(string name, string descriptor, Handle bootstrapMethodHandle, params object[] bootstrapMethodArguments)
	  {
		base.visitInvokeDynamicInsn(name, descriptor, bootstrapMethodHandle, bootstrapMethodArguments);
		if (this.locals == null)
		{
		  labels = null;
		  return;
		}
		pop(descriptor);
		pushDescriptor(descriptor);
		labels = null;
	  }

	  public override void visitJumpInsn(int opcode, Label label)
	  {
		base.visitJumpInsn(opcode, label);
		execute(opcode, 0, null);
		if (opcode == Opcodes.GOTO)
		{
		  this.locals = null;
		  this.stack = null;
		}
	  }

	  public override void visitLabel(Label label)
	  {
		base.visitLabel(label);
		if (labels == null)
		{
		  labels = new List<Label>(3);
		}
		labels.Add(label);
	  }

	  public override void visitLdcInsn(object value)
	  {
		base.visitLdcInsn(value);
		if (this.locals == null)
		{
		  labels = null;
		  return;
		}
		if (value is int)
		{
		  push(Opcodes.INTEGER);
		}
		else if (value is long)
		{
		  push(Opcodes.LONG);
		  push(Opcodes.TOP);
		}
		else if (value is float)
		{
		  push(Opcodes.FLOAT);
		}
		else if (value is Double)
		{
		  push(Opcodes.DOUBLE);
		  push(Opcodes.TOP);
		}
		else if (value is string)
		{
		  push("java/lang/String");
		}
		else if (value is JType)
		{
		  int sort = ((org.objectweb.asm.JType) value).Sort;
		  if (sort == org.objectweb.asm.JType.OBJECT || sort == org.objectweb.asm.JType.ARRAY)
		  {
			push("java/lang/Class");
		  }
		  else if (sort == org.objectweb.asm.JType.METHOD)
		  {
			push("java/lang/invoke/MethodType");
		  }
		  else
		  {
			throw new System.ArgumentException();
		  }
		}
		else if (value is Handle)
		{
		  push("java/lang/invoke/MethodHandle");
		}
		else if (value is ConstantDynamic)
		{
		  pushDescriptor(((ConstantDynamic) value).Descriptor);
		}
		else
		{
		  throw new System.ArgumentException();
		}
		labels = null;
	  }

	  public override void visitIincInsn(int var, int increment)
	  {
		base.visitIincInsn(var, increment);
		maxLocals = Math.Max(maxLocals, var + 1);
		execute(Opcodes.IINC, var, null);
	  }

	  public override void visitTableSwitchInsn(int min, int max, Label dflt, params Label[] labels)
	  {
		base.visitTableSwitchInsn(min, max, dflt, labels);
		execute(Opcodes.TABLESWITCH, 0, null);
		this.locals = null;
		this.stack = null;
	  }

	  public override void visitLookupSwitchInsn(Label dflt, int[] keys, Label[] labels)
	  {
		base.visitLookupSwitchInsn(dflt, keys, labels);
		execute(Opcodes.LOOKUPSWITCH, 0, null);
		this.locals = null;
		this.stack = null;
	  }

	  public override void visitMultiANewArrayInsn(string descriptor, int numDimensions)
	  {
		base.visitMultiANewArrayInsn(descriptor, numDimensions);
		execute(Opcodes.MULTIANEWARRAY, numDimensions, descriptor);
	  }

	  public override void visitLocalVariable(string name, string descriptor, string signature, Label start, Label end, int index)
	  {
		char firstDescriptorChar = descriptor[0];
		maxLocals = Math.Max(maxLocals, index + (firstDescriptorChar == 'J' || firstDescriptorChar == 'D' ? 2 : 1));
		base.visitLocalVariable(name, descriptor, signature, start, end, index);
	  }

	  public override void visitMaxs(int maxStack, int maxLocals)
	  {
		if (mv != null)
		{
		  this.maxStack = Math.Max(this.maxStack, maxStack);
		  this.maxLocals = Math.Max(this.maxLocals, maxLocals);
		  mv.visitMaxs(this.maxStack, this.maxLocals);
		}
	  }

	  // -----------------------------------------------------------------------------------------------

	  private object get(int local)
	  {
		maxLocals = Math.Max(maxLocals, local + 1);
		return local < locals.Count ? locals[local] : Opcodes.TOP;
	  }

	  private void set(int local, object type)
	  {
		maxLocals = Math.Max(maxLocals, local + 1);
		while (local >= locals.Count)
		{
		  locals.Add(Opcodes.TOP);
		}
		locals[local] = type;
	  }

	  private void push(object type)
	  {
		stack.Add(type);
		maxStack = Math.Max(maxStack, stack.Count);
	  }

	  private void pushDescriptor(string fieldOrMethodDescriptor)
	  {
		string descriptor = fieldOrMethodDescriptor[0] == '(' ? org.objectweb.asm.JType.getReturnType(fieldOrMethodDescriptor).Descriptor : fieldOrMethodDescriptor;
		switch (descriptor[0])
		{
		  case 'V':
			return;
		  case 'Z':
		  case 'C':
		  case 'B':
		  case 'S':
		  case 'I':
			push(Opcodes.INTEGER);
			return;
		  case 'F':
			push(Opcodes.FLOAT);
			return;
		  case 'J':
			push(Opcodes.LONG);
			push(Opcodes.TOP);
			return;
		  case 'D':
			push(Opcodes.DOUBLE);
			push(Opcodes.TOP);
			return;
		  case '[':
			push(descriptor);
			break;
		  case 'L':
			push(descriptor.Substring(1, (descriptor.Length - 1) - 1));
			break;
		  default:
			throw new ();
		}
	  }

	  private object pop()
      {
          var stackCount = stack.Count - 1;
          var current = stack[stackCount];stack.RemoveAt(stackCount);
          return current;
      }

	  private void pop(int numSlots)
	  {
		int size = stack.Count;
		int end = size - numSlots;
		for (int i = size - 1; i >= end; --i)
		{
		  stack.RemoveAt(i);
		}
	  }

	  private void pop(string descriptor)
	  {
		char firstDescriptorChar = descriptor[0];
		if (firstDescriptorChar == '(')
		{
		  int numSlots = 0;
		  org.objectweb.asm.JType[] types = org.objectweb.asm.JType.getArgumentTypes(descriptor);
		  foreach (org.objectweb.asm.JType type in types)
		  {
			numSlots += type.Size;
		  }
		  pop(numSlots);
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

	  private void execute(int opcode, int intArg, string stringArg)
	  {
		if (opcode == Opcodes.JSR || opcode == Opcodes.RET)
		{
		  throw new System.ArgumentException("JSR/RET are not supported");
		}
		if (this.locals == null)
		{
		  labels = null;
		  return;
		}
		object value1;
		object value2;
		object value3;
		object t4;
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
			push(Opcodes.NULL);
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
			push(Opcodes.INTEGER);
			break;
		  case Opcodes.LCONST_0:
		  case Opcodes.LCONST_1:
			push(Opcodes.LONG);
			push(Opcodes.TOP);
			break;
		  case Opcodes.FCONST_0:
		  case Opcodes.FCONST_1:
		  case Opcodes.FCONST_2:
			push(Opcodes.FLOAT);
			break;
		  case Opcodes.DCONST_0:
		  case Opcodes.DCONST_1:
			push(Opcodes.DOUBLE);
			push(Opcodes.TOP);
			break;
		  case Opcodes.ILOAD:
		  case Opcodes.FLOAD:
		  case Opcodes.ALOAD:
			push(get(intArg));
			break;
		  case Opcodes.LLOAD:
		  case Opcodes.DLOAD:
			push(get(intArg));
			push(Opcodes.TOP);
			break;
		  case Opcodes.LALOAD:
		  case Opcodes.D2L:
			pop(2);
			push(Opcodes.LONG);
			push(Opcodes.TOP);
			break;
		  case Opcodes.DALOAD:
		  case Opcodes.L2D:
			pop(2);
			push(Opcodes.DOUBLE);
			push(Opcodes.TOP);
			break;
		  case Opcodes.AALOAD:
			pop(1);
			value1 = pop();
			if (value1 is string)
			{
			  pushDescriptor(((string) value1).Substring(1));
			}
			else if (Equals(value1, Opcodes.NULL))
			{
			  push(value1);
			}
			else
			{
			  push("java/lang/Object");
			}
			break;
		  case Opcodes.ISTORE:
		  case Opcodes.FSTORE:
		  case Opcodes.ASTORE:
			value1 = pop();
			set(intArg, value1);
			if (intArg > 0)
			{
			  value2 = get(intArg - 1);
			  if (Equals(value2, Opcodes.LONG) || Equals(value2, Opcodes.DOUBLE))
			  {
				set(intArg - 1, Opcodes.TOP);
			  }
			}
			break;
		  case Opcodes.LSTORE:
		  case Opcodes.DSTORE:
			pop(1);
			value1 = pop();
			set(intArg, value1);
			set(intArg + 1, Opcodes.TOP);
			if (intArg > 0)
			{
			  value2 = get(intArg - 1);
			  if (Equals(value2, Opcodes.LONG) || Equals(value2, Opcodes.DOUBLE))
			  {
				set(intArg - 1, Opcodes.TOP);
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
			value1 = pop();
			push(value1);
			push(value1);
			break;
		  case Opcodes.DUP_X1:
			value1 = pop();
			value2 = pop();
			push(value1);
			push(value2);
			push(value1);
			break;
		  case Opcodes.DUP_X2:
			value1 = pop();
			value2 = pop();
			value3 = pop();
			push(value1);
			push(value3);
			push(value2);
			push(value1);
			break;
		  case Opcodes.DUP2:
			value1 = pop();
			value2 = pop();
			push(value2);
			push(value1);
			push(value2);
			push(value1);
			break;
		  case Opcodes.DUP2_X1:
			value1 = pop();
			value2 = pop();
			value3 = pop();
			push(value2);
			push(value1);
			push(value3);
			push(value2);
			push(value1);
			break;
		  case Opcodes.DUP2_X2:
			value1 = pop();
			value2 = pop();
			value3 = pop();
			t4 = pop();
			push(value2);
			push(value1);
			push(t4);
			push(value3);
			push(value2);
			push(value1);
			break;
		  case Opcodes.SWAP:
			value1 = pop();
			value2 = pop();
			push(value1);
			push(value2);
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
			push(Opcodes.INTEGER);
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
			push(Opcodes.LONG);
			push(Opcodes.TOP);
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
			push(Opcodes.FLOAT);
			break;
		  case Opcodes.DADD:
		  case Opcodes.DSUB:
		  case Opcodes.DMUL:
		  case Opcodes.DDIV:
		  case Opcodes.DREM:
			pop(4);
			push(Opcodes.DOUBLE);
			push(Opcodes.TOP);
			break;
		  case Opcodes.LSHL:
		  case Opcodes.LSHR:
		  case Opcodes.LUSHR:
			pop(3);
			push(Opcodes.LONG);
			push(Opcodes.TOP);
			break;
		  case Opcodes.IINC:
			set(intArg, Opcodes.INTEGER);
			break;
		  case Opcodes.I2L:
		  case Opcodes.F2L:
			pop(1);
			push(Opcodes.LONG);
			push(Opcodes.TOP);
			break;
		  case Opcodes.I2F:
			pop(1);
			push(Opcodes.FLOAT);
			break;
		  case Opcodes.I2D:
		  case Opcodes.F2D:
			pop(1);
			push(Opcodes.DOUBLE);
			push(Opcodes.TOP);
			break;
		  case Opcodes.F2I:
		  case Opcodes.ARRAYLENGTH:
		  case Opcodes.INSTANCEOF:
			pop(1);
			push(Opcodes.INTEGER);
			break;
		  case Opcodes.LCMP:
		  case Opcodes.DCMPL:
		  case Opcodes.DCMPG:
			pop(4);
			push(Opcodes.INTEGER);
			break;
		  case Opcodes.GETSTATIC:
			pushDescriptor(stringArg);
			break;
		  case Opcodes.PUTSTATIC:
			pop(stringArg);
			break;
		  case Opcodes.GETFIELD:
			pop(1);
			pushDescriptor(stringArg);
			break;
		  case Opcodes.PUTFIELD:
			pop(stringArg);
			pop();
			break;
		  case Opcodes.NEW:
			push(labels[0]);
			break;
		  case Opcodes.NEWARRAY:
			pop();
			switch (intArg)
			{
			  case Opcodes.T_BOOLEAN:
				pushDescriptor("[Z");
				break;
			  case Opcodes.T_CHAR:
				pushDescriptor("[C");
				break;
			  case Opcodes.T_BYTE:
				pushDescriptor("[B");
				break;
			  case Opcodes.T_SHORT:
				pushDescriptor("[S");
				break;
			  case Opcodes.T_INT:
				pushDescriptor("[I");
				break;
			  case Opcodes.T_FLOAT:
				pushDescriptor("[F");
				break;
			  case Opcodes.T_DOUBLE:
				pushDescriptor("[D");
				break;
			  case Opcodes.T_LONG:
				pushDescriptor("[J");
				break;
			  default:
				throw new System.ArgumentException("Invalid array type " + intArg);
			}
			break;
		  case Opcodes.ANEWARRAY:
			pop();
			pushDescriptor("[" + org.objectweb.asm.JType.getObjectType(stringArg));
			break;
		  case Opcodes.CHECKCAST:
			pop();
			pushDescriptor(org.objectweb.asm.JType.getObjectType(stringArg).Descriptor);
			break;
		  case Opcodes.MULTIANEWARRAY:
			pop(intArg);
			pushDescriptor(stringArg);
			break;
		  default:
			throw new System.ArgumentException("Invalid opcode " + opcode);
		}
		labels = null;
	  }
	}

}