using org.objectweb.asm;
using CursedJvmSharp.Asm.Java.IO;
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
	/// A <seealso cref="MethodVisitor"/> to insert before, after and around advices in methods and constructors.
	/// For constructors, the code keeps track of the elements on the stack in order to detect when the
	/// super class constructor is called (note that there can be multiple such calls in different
	/// branches). {@code onMethodEnter} is called after each super class constructor call, because the
	/// object cannot be used before it is properly initialized.
	/// 
	/// @author Eugene Kuleshov
	/// @author Eric Bruneton
	/// </summary>
	public abstract class AdviceAdapter : GeneratorAdapter, Opcodes
	{

	  /// <summary>
	  /// The "uninitialized this" value. </summary>
	  private static readonly object UNINITIALIZED_THIS = new object();

	  /// <summary>
	  /// Any value other than "uninitialized this". </summary>
	  private static readonly object OTHER = new object();

	  /// <summary>
	  /// Prefix of the error message when invalid opcodes are found. </summary>
	  private const string INVALID_OPCODE = "Invalid opcode ";

	  /// <summary>
	  /// The access flags of the visited method. </summary>
	  protected internal int methodAccess;

	  /// <summary>
	  /// The descriptor of the visited method. </summary>
	  protected internal string methodDesc;

	  /// <summary>
	  /// Whether the visited method is a constructor. </summary>
	  private readonly bool isConstructor;

	  /// <summary>
	  /// Whether the super class constructor has been called (if the visited method is a constructor),
	  /// at the current instruction. There can be multiple call sites to the super constructor (e.g. for
	  /// Java code such as {@code super(expr ? value1 : value2);}), in different branches. When scanning
	  /// the bytecode linearly, we can move from one branch where the super constructor has been called
	  /// to another where it has not been called yet. Therefore, this value can change from false to
	  /// true, and vice-versa.
	  /// </summary>
	  private bool superClassConstructorCalled;

	  /// <summary>
	  /// The values on the current execution stack frame (long and double are represented by two
	  /// elements). Each value is either <seealso cref="UNINITIALIZED_THIS"/> (for the uninitialized this value),
	  /// or <seealso cref="OTHER"/> (for any other value). This field is only maintained for constructors, in
	  /// branches where the super class constructor has not been called yet.
	  /// </summary>
	  private IList<object> stackFrame;

	  /// <summary>
	  /// The stack map frames corresponding to the labels of the forward jumps made *before* the super
	  /// class constructor has been called (note that the Java Virtual Machine forbids backward jumps
	  /// before the super class constructor is called). Note that by definition (cf. the 'before'), when
	  /// we reach a label from this map, <seealso cref="superClassConstructorCalled"/> must be reset to false.
	  /// This field is only maintained for constructors.
	  /// </summary>
	  private IDictionary<Label, IList<object>> forwardJumpStackFrames;

	  /// <summary>
	  /// Constructs a new <seealso cref="AdviceAdapter"/>.
	  /// </summary>
	  /// <param name="api"> the ASM API version implemented by this visitor. Must be one of the {@code
	  ///     ASM}<i>x</i> values in <seealso cref="Opcodes"/>. </param>
	  /// <param name="methodVisitor"> the method visitor to which this adapter delegates calls. </param>
	  /// <param name="access"> the method's access flags (see <seealso cref="Opcodes"/>). </param>
	  /// <param name="name"> the method's name. </param>
	  /// <param name="descriptor"> the method's descriptor (see <seealso cref="Type Type"/>). </param>
	  public AdviceAdapter(int api, MethodVisitor methodVisitor, int access, string name, string descriptor) : base(api, methodVisitor, access, name, descriptor)
	  {
		methodAccess = access;
		methodDesc = descriptor;
		isConstructor = "<init>".Equals(name);
	  }

	  public override void visitCode()
	  {
		base.visitCode();
		if (isConstructor)
		{
		  stackFrame = new List<object>();
		  forwardJumpStackFrames = new Dictionary<Label, IList<object>>();
		}
		else
		{
		  onMethodEnter();
		}
	  }

	  public override void visitLabel(Label label)
	  {
		base.visitLabel(label);
		if (isConstructor && forwardJumpStackFrames != null)
		{
		  IList<object> labelStackFrame = forwardJumpStackFrames.GetValueOrNull(label);
		  if (labelStackFrame != null)
		  {
			stackFrame = labelStackFrame;
			superClassConstructorCalled = false;
			forwardJumpStackFrames.Remove(label);
		  }
		}
	  }

	  public override void visitInsn(int opcode)
	  {
		if (isConstructor && !superClassConstructorCalled)
		{
		  int stackSize;
		  switch (opcode)
		  {
			case Opcodes.IRETURN:
			case Opcodes.FRETURN:
			case Opcodes.ARETURN:
			case Opcodes.LRETURN:
			case Opcodes.DRETURN:
			  throw new System.ArgumentException("Invalid return in constructor");
			case Opcodes.RETURN: // empty stack
			  onMethodExit(opcode);
			  endConstructorBasicBlockWithoutSuccessor();
			  break;
			case Opcodes.ATHROW: // 1 before n/a after
			  popValue();
			  onMethodExit(opcode);
			  endConstructorBasicBlockWithoutSuccessor();
			  break;
			case Opcodes.NOP:
			case Opcodes.LALOAD: // remove 2 add 2
			case Opcodes.DALOAD: // remove 2 add 2
			case Opcodes.LNEG:
			case Opcodes.DNEG:
			case Opcodes.FNEG:
			case Opcodes.INEG:
			case Opcodes.L2D:
			case Opcodes.D2L:
			case Opcodes.F2I:
			case Opcodes.I2B:
			case Opcodes.I2C:
			case Opcodes.I2S:
			case Opcodes.I2F:
			case Opcodes.ARRAYLENGTH:
			  break;
			case Opcodes.ACONST_NULL:
			case Opcodes.ICONST_M1:
			case Opcodes.ICONST_0:
			case Opcodes.ICONST_1:
			case Opcodes.ICONST_2:
			case Opcodes.ICONST_3:
			case Opcodes.ICONST_4:
			case Opcodes.ICONST_5:
			case Opcodes.FCONST_0:
			case Opcodes.FCONST_1:
			case Opcodes.FCONST_2:
			case Opcodes.F2L: // 1 before 2 after
			case Opcodes.F2D:
			case Opcodes.I2L:
			case Opcodes.I2D:
			  pushValue(OTHER);
			  break;
			case Opcodes.LCONST_0:
			case Opcodes.LCONST_1:
			case Opcodes.DCONST_0:
			case Opcodes.DCONST_1:
			  pushValue(OTHER);
			  pushValue(OTHER);
			  break;
			case Opcodes.IALOAD: // remove 2 add 1
			case Opcodes.FALOAD: // remove 2 add 1
			case Opcodes.AALOAD: // remove 2 add 1
			case Opcodes.BALOAD: // remove 2 add 1
			case Opcodes.CALOAD: // remove 2 add 1
			case Opcodes.SALOAD: // remove 2 add 1
			case Opcodes.POP:
			case Opcodes.IADD:
			case Opcodes.FADD:
			case Opcodes.ISUB:
			case Opcodes.LSHL: // 3 before 2 after
			case Opcodes.LSHR: // 3 before 2 after
			case Opcodes.LUSHR: // 3 before 2 after
			case Opcodes.L2I: // 2 before 1 after
			case Opcodes.L2F: // 2 before 1 after
			case Opcodes.D2I: // 2 before 1 after
			case Opcodes.D2F: // 2 before 1 after
			case Opcodes.FSUB:
			case Opcodes.FMUL:
			case Opcodes.FDIV:
			case Opcodes.FREM:
			case Opcodes.FCMPL: // 2 before 1 after
			case Opcodes.FCMPG: // 2 before 1 after
			case Opcodes.IMUL:
			case Opcodes.IDIV:
			case Opcodes.IREM:
			case Opcodes.ISHL:
			case Opcodes.ISHR:
			case Opcodes.IUSHR:
			case Opcodes.IAND:
			case Opcodes.IOR:
			case Opcodes.IXOR:
			case Opcodes.MONITORENTER:
			case Opcodes.MONITOREXIT:
			  popValue();
			  break;
			case Opcodes.POP2:
			case Opcodes.LSUB:
			case Opcodes.LMUL:
			case Opcodes.LDIV:
			case Opcodes.LREM:
			case Opcodes.LADD:
			case Opcodes.LAND:
			case Opcodes.LOR:
			case Opcodes.LXOR:
			case Opcodes.DADD:
			case Opcodes.DMUL:
			case Opcodes.DSUB:
			case Opcodes.DDIV:
			case Opcodes.DREM:
			  popValue();
			  popValue();
			  break;
			case Opcodes.IASTORE:
			case Opcodes.FASTORE:
			case Opcodes.AASTORE:
			case Opcodes.BASTORE:
			case Opcodes.CASTORE:
			case Opcodes.SASTORE:
			case Opcodes.LCMP: // 4 before 1 after
			case Opcodes.DCMPL:
			case Opcodes.DCMPG:
			  popValue();
			  popValue();
			  popValue();
			  break;
			case Opcodes.LASTORE:
			case Opcodes.DASTORE:
			  popValue();
			  popValue();
			  popValue();
			  popValue();
			  break;
			case Opcodes.DUP:
			  pushValue(peekValue());
			  break;
			case Opcodes.DUP_X1:
			  stackSize = stackFrame.Count;
			  stackFrame.Insert(stackSize - 2, stackFrame[stackSize - 1]);
			  break;
			case Opcodes.DUP_X2:
			  stackSize = stackFrame.Count;
			  stackFrame.Insert(stackSize - 3, stackFrame[stackSize - 1]);
			  break;
			case Opcodes.DUP2:
			  stackSize = stackFrame.Count;
			  stackFrame.Insert(stackSize - 2, stackFrame[stackSize - 1]);
			  stackFrame.Insert(stackSize - 2, stackFrame[stackSize - 1]);
			  break;
			case Opcodes.DUP2_X1:
			  stackSize = stackFrame.Count;
			  stackFrame.Insert(stackSize - 3, stackFrame[stackSize - 1]);
			  stackFrame.Insert(stackSize - 3, stackFrame[stackSize - 1]);
			  break;
			case Opcodes.DUP2_X2:
			  stackSize = stackFrame.Count;
			  stackFrame.Insert(stackSize - 4, stackFrame[stackSize - 1]);
			  stackFrame.Insert(stackSize - 4, stackFrame[stackSize - 1]);
			  break;
			case Opcodes.SWAP:
			  stackSize = stackFrame.Count;
			  stackFrame.Insert(stackSize - 2, stackFrame[stackSize - 1]);
			  stackFrame.RemoveAt(stackSize);
			  break;
			default:
			  throw new System.ArgumentException(INVALID_OPCODE + opcode);
		  }
		}
		else
		{
		  switch (opcode)
		  {
			case Opcodes.RETURN:
			case Opcodes.IRETURN:
			case Opcodes.FRETURN:
			case Opcodes.ARETURN:
			case Opcodes.LRETURN:
			case Opcodes.DRETURN:
			case Opcodes.ATHROW:
			  onMethodExit(opcode);
			  break;
			default:
			  break;
		  }
		}
		base.visitInsn(opcode);
	  }

	  public override void visitVarInsn(int opcode, int var)
	  {
		base.visitVarInsn(opcode, var);
		if (isConstructor && !superClassConstructorCalled)
		{
		  switch (opcode)
		  {
			case Opcodes.ILOAD:
			case Opcodes.FLOAD:
			  pushValue(OTHER);
			  break;
			case Opcodes.LLOAD:
			case Opcodes.DLOAD:
			  pushValue(OTHER);
			  pushValue(OTHER);
			  break;
			case Opcodes.ALOAD:
			  pushValue(var == 0 ? UNINITIALIZED_THIS : OTHER);
			  break;
			case Opcodes.ASTORE:
			case Opcodes.ISTORE:
			case Opcodes.FSTORE:
			  popValue();
			  break;
			case Opcodes.LSTORE:
			case Opcodes.DSTORE:
			  popValue();
			  popValue();
			  break;
			case Opcodes.RET:
			  endConstructorBasicBlockWithoutSuccessor();
			  break;
			default:
			  throw new System.ArgumentException(INVALID_OPCODE + opcode);
		  }
		}
	  }

	  public override void visitFieldInsn(int opcode, string owner, string name, string descriptor)
	  {
		base.visitFieldInsn(opcode, owner, name, descriptor);
		if (isConstructor && !superClassConstructorCalled)
		{
		  char firstDescriptorChar = descriptor[0];
		  bool longOrDouble = firstDescriptorChar == 'J' || firstDescriptorChar == 'D';
		  switch (opcode)
		  {
			case Opcodes.GETSTATIC:
			  pushValue(OTHER);
			  if (longOrDouble)
			  {
				pushValue(OTHER);
			  }
			  break;
			case Opcodes.PUTSTATIC:
			  popValue();
			  if (longOrDouble)
			  {
				popValue();
			  }
			  break;
			case Opcodes.PUTFIELD:
			  popValue();
			  popValue();
			  if (longOrDouble)
			  {
				popValue();
			  }
			  break;
			case Opcodes.GETFIELD:
			  if (longOrDouble)
			  {
				pushValue(OTHER);
			  }
			  break;
			default:
			  throw new System.ArgumentException(INVALID_OPCODE + opcode);
		  }
		}
	  }

	  public override void visitIntInsn(int opcode, int operand)
	  {
		base.visitIntInsn(opcode, operand);
		if (isConstructor && !superClassConstructorCalled && opcode != Opcodes.NEWARRAY)
		{
		  pushValue(OTHER);
		}
	  }

	  public override void visitLdcInsn(object value)
	  {
		base.visitLdcInsn(value);
		if (isConstructor && !superClassConstructorCalled)
		{
		  pushValue(OTHER);
		  if (value is double? || value is long? || (value is ConstantDynamic && ((ConstantDynamic) value).Size == 2))
		  {
			pushValue(OTHER);
		  }
		}
	  }

	  public override void visitMultiANewArrayInsn(string descriptor, int numDimensions)
	  {
		base.visitMultiANewArrayInsn(descriptor, numDimensions);
		if (isConstructor && !superClassConstructorCalled)
		{
		  for (int i = 0; i < numDimensions; i++)
		  {
			popValue();
		  }
		  pushValue(OTHER);
		}
	  }

	  public override void visitTypeInsn(int opcode, string type)
	  {
		base.visitTypeInsn(opcode, type);
		// ANEWARRAY, CHECKCAST or INSTANCEOF don't change stack.
		if (isConstructor && !superClassConstructorCalled && opcode == Opcodes.NEW)
		{
		  pushValue(OTHER);
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
		base.visitMethodInsn(opcodeAndSource, owner, name, descriptor, isInterface);
		int opcode = opcodeAndSource & ~Opcodes.SOURCE_MASK;

		doVisitMethodInsn(opcode, name, descriptor);
	  }

	  private void doVisitMethodInsn(int opcode, string name, string descriptor)
	  {
		if (isConstructor && !superClassConstructorCalled)
		{
		  foreach (org.objectweb.asm.JType argumentType in org.objectweb.asm.JType.getArgumentTypes(descriptor))
		  {
			popValue();
			if (argumentType.Size == 2)
			{
			  popValue();
			}
		  }
		  switch (opcode)
		  {
			case Opcodes.INVOKEINTERFACE:
			case Opcodes.INVOKEVIRTUAL:
			  popValue();
			  break;
			case Opcodes.INVOKESPECIAL:
			  object value = popValue();
			  if (value == UNINITIALIZED_THIS && !superClassConstructorCalled && name.Equals("<init>"))
			  {
				superClassConstructorCalled = true;
				onMethodEnter();
			  }
			  break;
			default:
			  break;
		  }

		  org.objectweb.asm.JType returnType = org.objectweb.asm.JType.getReturnType(descriptor);
		  if (returnType != org.objectweb.asm.JType.VOID_TYPE)
		  {
			pushValue(OTHER);
			if (returnType.Size == 2)
			{
			  pushValue(OTHER);
			}
		  }
		}
	  }

	  public override void visitInvokeDynamicInsn(string name, string descriptor, Handle bootstrapMethodHandle, params object[] bootstrapMethodArguments)
	  {
		base.visitInvokeDynamicInsn(name, descriptor, bootstrapMethodHandle, bootstrapMethodArguments);
		doVisitMethodInsn(Opcodes.INVOKEDYNAMIC, name, descriptor);
	  }

	  public override void visitJumpInsn(int opcode, Label label)
	  {
		base.visitJumpInsn(opcode, label);
		if (isConstructor && !superClassConstructorCalled)
		{
		  switch (opcode)
		  {
			case Opcodes.IFEQ:
			case Opcodes.IFNE:
			case Opcodes.IFLT:
			case Opcodes.IFGE:
			case Opcodes.IFGT:
			case Opcodes.IFLE:
			case Opcodes.IFNULL:
			case Opcodes.IFNONNULL:
			  popValue();
			  break;
			case Opcodes.IF_ICMPEQ:
			case Opcodes.IF_ICMPNE:
			case Opcodes.IF_ICMPLT:
			case Opcodes.IF_ICMPGE:
			case Opcodes.IF_ICMPGT:
			case Opcodes.IF_ICMPLE:
			case Opcodes.IF_ACMPEQ:
			case Opcodes.IF_ACMPNE:
			  popValue();
			  popValue();
			  break;
			case Opcodes.JSR:
			  pushValue(OTHER);
			  break;
			case Opcodes.GOTO:
			  endConstructorBasicBlockWithoutSuccessor();
			  break;
			default:
			  break;
		  }
		  addForwardJump(label);
		}
	  }

	  public override void visitLookupSwitchInsn(Label dflt, int[] keys, Label[] labels)
	  {
		base.visitLookupSwitchInsn(dflt, keys, labels);
		if (isConstructor && !superClassConstructorCalled)
		{
		  popValue();
		  addForwardJumps(dflt, labels);
		  endConstructorBasicBlockWithoutSuccessor();
		}
	  }

	  public override void visitTableSwitchInsn(int min, int max, Label dflt, params Label[] labels)
	  {
		base.visitTableSwitchInsn(min, max, dflt, labels);
		if (isConstructor && !superClassConstructorCalled)
		{
		  popValue();
		  addForwardJumps(dflt, labels);
		  endConstructorBasicBlockWithoutSuccessor();
		}
	  }

	  public override void visitTryCatchBlock(Label start, Label end, Label handler, string type)
	  {
		base.visitTryCatchBlock(start, end, handler, type);
		// By definition of 'forwardJumpStackFrames', 'handler' should be pushed only if there is an
		// instruction between 'start' and 'end' at which the super class constructor is not yet
		// called. Unfortunately, try catch blocks must be visited before their labels, so we have no
		// way to know this at this point. Instead, we suppose that the super class constructor has not
		// been called at the start of *any* exception handler. If this is wrong, normally there should
		// not be a second super class constructor call in the exception handler (an object can't be
		// initialized twice), so this is not issue (in the sense that there is no risk to emit a wrong
		// 'onMethodEnter').
		if (isConstructor && !forwardJumpStackFrames.ContainsKey(handler))
		{
		  IList<object> handlerStackFrame = new List<object>();
		  handlerStackFrame.Add(OTHER);
		  forwardJumpStackFrames[handler] = handlerStackFrame;
		}
	  }

	  private void addForwardJumps(Label dflt, Label[] labels)
	  {
		addForwardJump(dflt);
		foreach (Label label in labels)
		{
		  addForwardJump(label);
		}
	  }

	  private void addForwardJump(Label label)
	  {
		if (forwardJumpStackFrames.ContainsKey(label))
		{
		  return;
		}
		forwardJumpStackFrames[label] = new List<object>(stackFrame);
	  }

	  private void endConstructorBasicBlockWithoutSuccessor()
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
		superClassConstructorCalled = true;
	  }

	  private object popValue()
      {
          var index = stackFrame.Count - 1;
		  var oldValue = stackFrame[index];
          stackFrame.RemoveAt(index);
          return oldValue;
      }

	  private object peekValue()
	  {
		return stackFrame[stackFrame.Count - 1];
	  }

	  private void pushValue(object value)
	  {
		stackFrame.Add(value);
	  }

	  /// <summary>
	  /// Generates the "before" advice for the visited method. The default implementation of this method
	  /// does nothing. Subclasses can use or change all the local variables, but should not change state
	  /// of the stack. This method is called at the beginning of the method or after super class
	  /// constructor has been called (in constructors).
	  /// </summary>
	  public virtual void onMethodEnter()
	  {
	  }

	  /// <summary>
	  /// Generates the "after" advice for the visited method. The default implementation of this method
	  /// does nothing. Subclasses can use or change all the local variables, but should not change state
	  /// of the stack. This method is called at the end of the method, just before return and athrow
	  /// instructions. The top element on the stack contains the return value or the exception instance.
	  /// For example:
	  /// 
	  /// <pre>
	  /// public void onMethodExit(final int opcode) {
	  ///   if (opcode == RETURN) {
	  ///     visitInsn(ACONST_NULL);
	  ///   } else if (opcode == ARETURN || opcode == ATHROW) {
	  ///     dup();
	  ///   } else {
	  ///     if (opcode == LRETURN || opcode == DRETURN) {
	  ///       dup2();
	  ///     } else {
	  ///       dup();
	  ///     }
	  ///     box(Type.getReturnType(this.methodDesc));
	  ///   }
	  ///   visitIntInsn(SIPUSH, opcode);
	  ///   visitMethodInsn(INVOKESTATIC, owner, "onExit", "(Ljava/lang/Object;I)V");
	  /// }
	  /// 
	  /// // An actual call back method.
	  /// public static void onExit(final Object exitValue, final int opcode) {
	  ///   ...
	  /// }
	  /// </pre>
	  /// </summary>
	  /// <param name="opcode"> one of <seealso cref="Opcodes.RETURN"/>, <seealso cref="Opcodes.IRETURN"/>, <seealso cref="Opcodes.FRETURN"/>,
	  ///     <seealso cref="Opcodes.ARETURN"/>, <seealso cref="Opcodes.LRETURN"/>, <seealso cref="Opcodes.DRETURN"/> or {@link
	  ///     Opcodes#ATHROW}. </param>
	  public virtual void onMethodExit(int opcode)
	  {
	  }
	}

}