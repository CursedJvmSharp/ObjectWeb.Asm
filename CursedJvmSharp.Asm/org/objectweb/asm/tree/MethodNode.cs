using System;
using org.objectweb.asm;
using CursedJvmSharp.Asm.Java.IO;
using System.Collections.Generic;
using System.Linq;
using AnnotationVisitor = org.objectweb.asm.AnnotationVisitor;
using Attribute = org.objectweb.asm.Attribute;
using ClassVisitor = org.objectweb.asm.ClassVisitor;
using ConstantDynamic = org.objectweb.asm.ConstantDynamic;
using Handle = org.objectweb.asm.Handle;
using Label = org.objectweb.asm.Label;
using MethodVisitor = org.objectweb.asm.MethodVisitor;
using Opcodes = org.objectweb.asm.Opcodes;
using TypePath = org.objectweb.asm.TypePath;

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
namespace org.objectweb.asm.tree
{

	/// <summary>
	/// A node that represents a method.
	/// 
	/// @author Eric Bruneton
	/// </summary>
	public class MethodNode : MethodVisitor
	{

	  /// <summary>
	  /// The method's access flags (see <seealso cref="Opcodes"/>). This field also indicates if the method is
	  /// synthetic and/or deprecated.
	  /// </summary>
	  public int access;

	  /// <summary>
	  /// The method's name. </summary>
	  public string name;

	  /// <summary>
	  /// The method's descriptor (see <seealso cref="Type"/>). </summary>
	  public string desc;

	  /// <summary>
	  /// The method's signature. May be {@literal null}. </summary>
	  public string signature;

	  /// <summary>
	  /// The internal names of the method's exception classes (see <seealso cref="Type.InternalName"/>). </summary>
	  public List<string> exceptions;

	  /// <summary>
	  /// The method parameter info (access flags and name). </summary>
	  public List<ParameterNode> parameters;

	  /// <summary>
	  /// The runtime visible annotations of this method. May be {@literal null}. </summary>
	  public List<AnnotationNode> visibleAnnotations;

	  /// <summary>
	  /// The runtime invisible annotations of this method. May be {@literal null}. </summary>
	  public List<AnnotationNode> invisibleAnnotations;

	  /// <summary>
	  /// The runtime visible type annotations of this method. May be {@literal null}. </summary>
	  public List<TypeAnnotationNode> visibleTypeAnnotations;

	  /// <summary>
	  /// The runtime invisible type annotations of this method. May be {@literal null}. </summary>
	  public List<TypeAnnotationNode> invisibleTypeAnnotations;

	  /// <summary>
	  /// The non standard attributes of this method. May be {@literal null}. </summary>
	  public List<Attribute> attrs;

	  /// <summary>
	  /// The default value of this annotation interface method. This field must be a <seealso cref="Byte"/>,
	  /// <seealso cref="Boolean"/>, <seealso cref="Character"/>, <seealso cref="Short"/>, <seealso cref="Integer"/>, <seealso cref="Long"/>, {@link
	  /// Float}, <seealso cref="Double"/>, <seealso cref="string"/> or <seealso cref="Type"/>, or an two elements String array (for
	  /// enumeration values), a <seealso cref="AnnotationNode"/>, or a <seealso cref="System.Collections.IList"/> of values of one of the
	  /// preceding types. May be {@literal null}.
	  /// </summary>
	  public object annotationDefault;

	  /// <summary>
	  /// The number of method parameters than can have runtime visible annotations. This number must be
	  /// less or equal than the number of parameter types in the method descriptor (the default value 0
	  /// indicates that all the parameters described in the method descriptor can have annotations). It
	  /// can be strictly less when a method has synthetic parameters and when these parameters are
	  /// ignored when computing parameter indices for the purpose of parameter annotations (see
	  /// https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.7.18).
	  /// </summary>
	  public int visibleAnnotableParameterCount;

	  /// <summary>
	  /// The runtime visible parameter annotations of this method. These lists are lists of {@link
	  /// AnnotationNode} objects. May be {@literal null}.
	  /// </summary>
	  public List<AnnotationNode>[] visibleParameterAnnotations;

	  /// <summary>
	  /// The number of method parameters than can have runtime invisible annotations. This number must
	  /// be less or equal than the number of parameter types in the method descriptor (the default value
	  /// 0 indicates that all the parameters described in the method descriptor can have annotations).
	  /// It can be strictly less when a method has synthetic parameters and when these parameters are
	  /// ignored when computing parameter indices for the purpose of parameter annotations (see
	  /// https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.7.18).
	  /// </summary>
	  public int invisibleAnnotableParameterCount;

	  /// <summary>
	  /// The runtime invisible parameter annotations of this method. These lists are lists of {@link
	  /// AnnotationNode} objects. May be {@literal null}.
	  /// </summary>
	  public List<AnnotationNode>[] invisibleParameterAnnotations;

	  /// <summary>
	  /// The instructions of this method. </summary>
	  public InsnList instructions;

	  /// <summary>
	  /// The try catch blocks of this method. </summary>
	  public List<TryCatchBlockNode> tryCatchBlocks;

	  /// <summary>
	  /// The maximum stack size of this method. </summary>
	  public int maxStack;

	  /// <summary>
	  /// The maximum number of local variables of this method. </summary>
	  public int maxLocals;

	  /// <summary>
	  /// The local variables of this method. May be {@literal null} </summary>
	  public List<LocalVariableNode> localVariables;

	  /// <summary>
	  /// The visible local variable annotations of this method. May be {@literal null} </summary>
	  public List<LocalVariableAnnotationNode> visibleLocalVariableAnnotations;

	  /// <summary>
	  /// The invisible local variable annotations of this method. May be {@literal null} </summary>
	  public List<LocalVariableAnnotationNode> invisibleLocalVariableAnnotations;

	  /// <summary>
	  /// Whether the accept method has been called on this object. </summary>
	  private bool visited;

	  /// <summary>
	  /// Constructs an uninitialized <seealso cref="MethodNode"/>. <i>Subclasses must not use this
	  /// constructor</i>. Instead, they must use the <seealso cref="MethodNode(int)"/> version.
	  /// </summary>
	  /// <exception cref="IllegalStateException"> If a subclass calls this constructor. </exception>
	  public MethodNode() : this(Opcodes.ASM9)
	  {
		if (this.GetType() != typeof(MethodNode))
		{
		  throw new System.InvalidOperationException();
		}
	  }

	  /// <summary>
	  /// Constructs an uninitialized <seealso cref="MethodNode"/>.
	  /// </summary>
	  /// <param name="api"> the ASM API version implemented by this visitor. Must be one of the {@code
	  ///     ASM}<i>x</i> values in <seealso cref="Opcodes"/>. </param>
	  public MethodNode(int api) : base(api)
	  {
		this.instructions = new InsnList();
	  }

	  /// <summary>
	  /// Constructs a new <seealso cref="MethodNode"/>. <i>Subclasses must not use this constructor</i>. Instead,
	  /// they must use the <seealso cref="MethodNode(int, int, String, String, String, string[])"/> version.
	  /// </summary>
	  /// <param name="access"> the method's access flags (see <seealso cref="Opcodes"/>). This parameter also indicates if
	  ///     the method is synthetic and/or deprecated. </param>
	  /// <param name="name"> the method's name. </param>
	  /// <param name="descriptor"> the method's descriptor (see <seealso cref="Type"/>). </param>
	  /// <param name="signature"> the method's signature. May be {@literal null}. </param>
	  /// <param name="exceptions"> the internal names of the method's exception classes (see {@link
	  ///     Type#getInternalName()}). May be {@literal null}. </param>
	  /// <exception cref="IllegalStateException"> If a subclass calls this constructor. </exception>
	  public MethodNode(int access, string name, string descriptor, string signature, string[] exceptions) : this(Opcodes.ASM9, access, name, descriptor, signature, exceptions)
	  {
		if (this.GetType() != typeof(MethodNode))
		{
		  throw new System.InvalidOperationException();
		}
	  }

	  /// <summary>
	  /// Constructs a new <seealso cref="MethodNode"/>.
	  /// </summary>
	  /// <param name="api"> the ASM API version implemented by this visitor. Must be one of the {@code
	  ///     ASM}<i>x</i> values in <seealso cref="Opcodes"/>. </param>
	  /// <param name="access"> the method's access flags (see <seealso cref="Opcodes"/>). This parameter also indicates if
	  ///     the method is synthetic and/or deprecated. </param>
	  /// <param name="name"> the method's name. </param>
	  /// <param name="descriptor"> the method's descriptor (see <seealso cref="Type"/>). </param>
	  /// <param name="signature"> the method's signature. May be {@literal null}. </param>
	  /// <param name="exceptions"> the internal names of the method's exception classes (see {@link
	  ///     Type#getInternalName()}). May be {@literal null}. </param>
	  public MethodNode(int api, int access, string name, string descriptor, string signature, string[] exceptions) : base(api)
	  {
		this.access = access;
		this.name = name;
		this.desc = descriptor;
		this.signature = signature;
		this.exceptions = Util.asArrayList(exceptions);
		if ((access & Opcodes.ACC_ABSTRACT) == 0)
		{
		  this.localVariables = new List<LocalVariableNode>(5);
		}
		this.tryCatchBlocks = new List<TryCatchBlockNode>();
		this.instructions = new InsnList();
	  }

	  // -----------------------------------------------------------------------------------------------
	  // Implementation of the MethodVisitor abstract class
	  // -----------------------------------------------------------------------------------------------

	  public override void visitParameter(string name, int access)
	  {
		if (parameters == null)
		{
		  parameters = new List<ParameterNode>(5);
		}
		parameters.Add(new ParameterNode(name, access));
	  }

	  public override AnnotationVisitor visitAnnotationDefault()
	  {
		return new AnnotationNode(new ArrayListAnonymousInnerClass(this));
	  }

	  private class ArrayListAnonymousInnerClass : List<object>
	  {
		  private readonly MethodNode outerInstance;

		  public ArrayListAnonymousInnerClass(MethodNode outerInstance) : base(0)
		  {
			  this.outerInstance = outerInstance;
		  }

		  public new bool Add(object o)
		  {
			outerInstance.annotationDefault = o;
			base.Add(o);
            return true;
          }
	  }

	  public override AnnotationVisitor visitAnnotation(string descriptor, bool visible)
	  {
		AnnotationNode annotation = new AnnotationNode(descriptor);
		if (visible)
		{
		  visibleAnnotations = Util.add(visibleAnnotations, annotation);
		}
		else
		{
		  invisibleAnnotations = Util.add(invisibleAnnotations, annotation);
		}
		return annotation;
	  }

	  public override AnnotationVisitor visitTypeAnnotation(int typeRef, TypePath typePath, string descriptor, bool visible)
	  {
		TypeAnnotationNode typeAnnotation = new TypeAnnotationNode(typeRef, typePath, descriptor);
		if (visible)
		{
		  visibleTypeAnnotations = Util.add(visibleTypeAnnotations, typeAnnotation);
		}
		else
		{
		  invisibleTypeAnnotations = Util.add(invisibleTypeAnnotations, typeAnnotation);
		}
		return typeAnnotation;
	  }

	  public override void visitAnnotableParameterCount(int parameterCount, bool visible)
	  {
		if (visible)
		{
		  visibleAnnotableParameterCount = parameterCount;
		}
		else
		{
		  invisibleAnnotableParameterCount = parameterCount;
		}
	  }

	  public override AnnotationVisitor visitParameterAnnotation(int parameter, string descriptor, bool visible)
	  {
		AnnotationNode annotation = new AnnotationNode(descriptor);
		if (visible)
		{
		  if (visibleParameterAnnotations == null)
		  {
			int @params = org.objectweb.asm.JType.getArgumentTypes(desc).Length;
			visibleParameterAnnotations = (List<AnnotationNode>[]) new List<object>[@params];
		  }
		  visibleParameterAnnotations[parameter] = Util.add(visibleParameterAnnotations[parameter], annotation);
		}
		else
		{
		  if (invisibleParameterAnnotations == null)
		  {
			int @params = org.objectweb.asm.JType.getArgumentTypes(desc).Length;
			invisibleParameterAnnotations = (List<AnnotationNode>[]) new List<object>[@params];
		  }
		  invisibleParameterAnnotations[parameter] = Util.add(invisibleParameterAnnotations[parameter], annotation);
		}
		return annotation;
	  }

	  public override void visitAttribute(Attribute attribute)
	  {
		attrs = Util.add(attrs, attribute);
	  }

	  public override void visitCode()
	  {
		// Nothing to do.
	  }

	  public override void visitFrame(int type, int numLocal, object[] local, int numStack, object[] stack)
	  {
		instructions.add(new FrameNode(type, numLocal, local == null ? null : getLabelNodes(local), numStack, stack == null ? null : getLabelNodes(stack)));
	  }

	  public override void visitInsn(int opcode)
	  {
		instructions.add(new InsnNode(opcode));
	  }

	  public override void visitIntInsn(int opcode, int operand)
	  {
		instructions.add(new IntInsnNode(opcode, operand));
	  }

	  public override void visitVarInsn(int opcode, int var)
	  {
		instructions.add(new VarInsnNode(opcode, var));
	  }

	  public override void visitTypeInsn(int opcode, string type)
	  {
		instructions.add(new TypeInsnNode(opcode, type));
	  }

	  public override void visitFieldInsn(int opcode, string owner, string name, string descriptor)
	  {
		instructions.add(new FieldInsnNode(opcode, owner, name, descriptor));
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

		instructions.add(new MethodInsnNode(opcode, owner, name, descriptor, isInterface));
	  }

	  public override void visitInvokeDynamicInsn(string name, string descriptor, Handle bootstrapMethodHandle, params object[] bootstrapMethodArguments)
	  {
		instructions.add(new InvokeDynamicInsnNode(name, descriptor, bootstrapMethodHandle, bootstrapMethodArguments));
	  }

	  public override void visitJumpInsn(int opcode, Label label)
	  {
		instructions.add(new JumpInsnNode(opcode, getLabelNode(label)));
	  }

	  public override void visitLabel(Label label)
	  {
		instructions.add(getLabelNode(label));
	  }

	  public override void visitLdcInsn(object value)
	  {
		instructions.add(new LdcInsnNode(value));
	  }

	  public override void visitIincInsn(int var, int increment)
	  {
		instructions.add(new IincInsnNode(var, increment));
	  }

	  public override void visitTableSwitchInsn(int min, int max, Label dflt, params Label[] labels)
	  {
		instructions.add(new TableSwitchInsnNode(min, max, getLabelNode(dflt), getLabelNodes(labels)));
	  }

	  public override void visitLookupSwitchInsn(Label dflt, int[] keys, Label[] labels)
	  {
		instructions.add(new LookupSwitchInsnNode(getLabelNode(dflt), keys, getLabelNodes(labels)));
	  }

	  public override void visitMultiANewArrayInsn(string descriptor, int numDimensions)
	  {
		instructions.add(new MultiANewArrayInsnNode(descriptor, numDimensions));
	  }

	  public override AnnotationVisitor visitInsnAnnotation(int typeRef, TypePath typePath, string descriptor, bool visible)
	  {
		// Find the last real instruction, i.e. the instruction targeted by this annotation.
		AbstractInsnNode currentInsn = instructions.Last;
		while (currentInsn.Opcode == -1)
		{
		  currentInsn = currentInsn.Previous;
		}
		// Add the annotation to this instruction.
		TypeAnnotationNode typeAnnotation = new TypeAnnotationNode(typeRef, typePath, descriptor);
		if (visible)
		{
		  currentInsn.visibleTypeAnnotations = Util.add(currentInsn.visibleTypeAnnotations, typeAnnotation);
		}
		else
		{
		  currentInsn.invisibleTypeAnnotations = Util.add(currentInsn.invisibleTypeAnnotations, typeAnnotation);
		}
		return typeAnnotation;
	  }

	  public override void visitTryCatchBlock(Label start, Label end, Label handler, string type)
	  {
		TryCatchBlockNode tryCatchBlock = new TryCatchBlockNode(getLabelNode(start), getLabelNode(end), getLabelNode(handler), type);
		tryCatchBlocks = Util.add(tryCatchBlocks, tryCatchBlock);
	  }

	  public override AnnotationVisitor visitTryCatchAnnotation(int typeRef, TypePath typePath, string descriptor, bool visible)
	  {
		TryCatchBlockNode tryCatchBlock = tryCatchBlocks[(typeRef & 0x00FFFF00) >> 8];
		TypeAnnotationNode typeAnnotation = new TypeAnnotationNode(typeRef, typePath, descriptor);
		if (visible)
		{
		  tryCatchBlock.visibleTypeAnnotations = Util.add(tryCatchBlock.visibleTypeAnnotations, typeAnnotation);
		}
		else
		{
		  tryCatchBlock.invisibleTypeAnnotations = Util.add(tryCatchBlock.invisibleTypeAnnotations, typeAnnotation);
		}
		return typeAnnotation;
	  }

	  public override void visitLocalVariable(string name, string descriptor, string signature, Label start, Label end, int index)
	  {
		LocalVariableNode localVariable = new LocalVariableNode(name, descriptor, signature, getLabelNode(start), getLabelNode(end), index);
		localVariables = Util.add(localVariables, localVariable);
	  }

	  public override AnnotationVisitor visitLocalVariableAnnotation(int typeRef, TypePath typePath, Label[] start, Label[] end, int[] index, string descriptor, bool visible)
	  {
		LocalVariableAnnotationNode localVariableAnnotation = new LocalVariableAnnotationNode(typeRef, typePath, getLabelNodes(start), getLabelNodes(end), index, descriptor);
		if (visible)
		{
		  visibleLocalVariableAnnotations = Util.add(visibleLocalVariableAnnotations, localVariableAnnotation);
		}
		else
		{
		  invisibleLocalVariableAnnotations = Util.add(invisibleLocalVariableAnnotations, localVariableAnnotation);
		}
		return localVariableAnnotation;
	  }

	  public override void visitLineNumber(int line, Label start)
	  {
		instructions.add(new LineNumberNode(line, getLabelNode(start)));
	  }

	  public override void visitMaxs(int maxStack, int maxLocals)
	  {
		this.maxStack = maxStack;
		this.maxLocals = maxLocals;
	  }

	  public override void visitEnd()
	  {
		// Nothing to do.
	  }

	  /// <summary>
	  /// Returns the LabelNode corresponding to the given Label. Creates a new LabelNode if necessary.
	  /// The default implementation of this method uses the <seealso cref="Label.info"/> field to store
	  /// associations between labels and label nodes.
	  /// </summary>
	  /// <param name="label"> a Label. </param>
	  /// <returns> the LabelNode corresponding to label. </returns>
	  public virtual LabelNode getLabelNode(Label label)
	  {
		if (!(label.info is LabelNode))
		{
		  label.info = new LabelNode();
		}
		return (LabelNode) label.info;
	  }

	  private LabelNode[] getLabelNodes(Label[] labels)
	  {
		LabelNode[] labelNodes = new LabelNode[labels.Length];
		for (int i = 0, n = labels.Length; i < n; ++i)
		{
		  labelNodes[i] = getLabelNode(labels[i]);
		}
		return labelNodes;
	  }

	  private object[] getLabelNodes(object[] objects)
	  {
		object[] labelNodes = new object[objects.Length];
		for (int i = 0, n = objects.Length; i < n; ++i)
		{
		  object o = objects[i];
		  if (o is Label)
		  {
			o = getLabelNode((Label) o);
		  }
		  labelNodes[i] = o;
		}
		return labelNodes;
	  }

	  // -----------------------------------------------------------------------------------------------
	  // Accept method
	  // -----------------------------------------------------------------------------------------------

	  /// <summary>
	  /// Checks that this method node is compatible with the given ASM API version. This method checks
	  /// that this node, and all its children recursively, do not contain elements that were introduced
	  /// in more recent versions of the ASM API than the given version.
	  /// </summary>
	  /// <param name="api"> an ASM API version. Must be one of the {@code ASM}<i>x</i> values in {@link
	  ///     Opcodes}. </param>
	  public virtual void check(int api)
	  {
		if (api == Opcodes.ASM4)
		{
		  if (parameters != null && parameters.Count > 0)
		  {
			throw new UnsupportedClassVersionException();
		  }
		  if (visibleTypeAnnotations != null && visibleTypeAnnotations.Count > 0)
		  {
			throw new UnsupportedClassVersionException();
		  }
		  if (invisibleTypeAnnotations != null && invisibleTypeAnnotations.Count > 0)
		  {
			throw new UnsupportedClassVersionException();
		  }
		  if (tryCatchBlocks != null)
		  {
			for (int i = tryCatchBlocks.Count - 1; i >= 0; --i)
			{
			  TryCatchBlockNode tryCatchBlock = tryCatchBlocks[i];
			  if (tryCatchBlock.visibleTypeAnnotations != null && tryCatchBlock.visibleTypeAnnotations.Count > 0)
			  {
				throw new UnsupportedClassVersionException();
			  }
			  if (tryCatchBlock.invisibleTypeAnnotations != null && tryCatchBlock.invisibleTypeAnnotations.Count > 0)
			  {
				throw new UnsupportedClassVersionException();
			  }
			}
		  }
		  for (int i = instructions.Count() - 1; i >= 0; --i)
		  {
			AbstractInsnNode insn = instructions.get(i);
			if (insn.visibleTypeAnnotations != null && insn.visibleTypeAnnotations.Count > 0)
			{
			  throw new UnsupportedClassVersionException();
			}
			if (insn.invisibleTypeAnnotations != null && insn.invisibleTypeAnnotations.Count > 0)
			{
			  throw new UnsupportedClassVersionException();
			}
			if (insn is MethodInsnNode)
			{
			  bool isInterface = ((MethodInsnNode) insn).itf;
			  if (isInterface != (insn.opcode == Opcodes.INVOKEINTERFACE))
			  {
				throw new UnsupportedClassVersionException();
			  }
			}
			else if (insn is LdcInsnNode)
			{
			  object value = ((LdcInsnNode) insn).cst;
			  if (value is Handle || (value is org.objectweb.asm.JType && ((org.objectweb.asm.JType) value).Sort == org.objectweb.asm.JType.METHOD))
			  {
				throw new UnsupportedClassVersionException();
			  }
			}
		  }
		  if (visibleLocalVariableAnnotations != null && visibleLocalVariableAnnotations.Count > 0)
		  {
			throw new UnsupportedClassVersionException();
		  }
		  if (invisibleLocalVariableAnnotations != null && invisibleLocalVariableAnnotations.Count > 0)
		  {
			throw new UnsupportedClassVersionException();
		  }
		}
		if (api < Opcodes.ASM7)
		{
		  for (int i = instructions.Count() - 1; i >= 0; --i)
		  {
			AbstractInsnNode insn = instructions.get(i);
			if (insn is LdcInsnNode)
			{
			  object value = ((LdcInsnNode) insn).cst;
			  if (value is ConstantDynamic)
			  {
				throw new UnsupportedClassVersionException();
			  }
			}
		  }
		}
	  }

	  /// <summary>
	  /// Makes the given class visitor visit this method.
	  /// </summary>
	  /// <param name="classVisitor"> a class visitor. </param>
	  public virtual void accept(ClassVisitor classVisitor)
	  {
		string[] exceptionsArray = exceptions == null ? null : ((List<string>)exceptions).ToArray();
		MethodVisitor methodVisitor = classVisitor.visitMethod(access, name, desc, signature, exceptionsArray);
		if (methodVisitor != null)
		{
		  accept(methodVisitor);
		}
	  }

	  /// <summary>
	  /// Makes the given method visitor visit this method.
	  /// </summary>
	  /// <param name="methodVisitor"> a method visitor. </param>
	  public virtual void accept(MethodVisitor methodVisitor)
	  {
		// Visit the parameters.
		if (parameters != null)
		{
		  for (int i = 0, n = parameters.Count; i < n; i++)
		  {
			parameters[i].accept(methodVisitor);
		  }
		}
		// Visit the annotations.
		if (annotationDefault != null)
		{
		  AnnotationVisitor annotationVisitor = methodVisitor.visitAnnotationDefault();
		  AnnotationNode.accept(annotationVisitor, null, annotationDefault);
		  if (annotationVisitor != null)
		  {
			annotationVisitor.visitEnd();
		  }
		}
		if (visibleAnnotations != null)
		{
		  for (int i = 0, n = visibleAnnotations.Count; i < n; ++i)
		  {
			AnnotationNode annotation = visibleAnnotations[i];
			annotation.accept(methodVisitor.visitAnnotation(annotation.desc, true));
		  }
		}
		if (invisibleAnnotations != null)
		{
		  for (int i = 0, n = invisibleAnnotations.Count; i < n; ++i)
		  {
			AnnotationNode annotation = invisibleAnnotations[i];
			annotation.accept(methodVisitor.visitAnnotation(annotation.desc, false));
		  }
		}
		if (visibleTypeAnnotations != null)
		{
		  for (int i = 0, n = visibleTypeAnnotations.Count; i < n; ++i)
		  {
			TypeAnnotationNode typeAnnotation = visibleTypeAnnotations[i];
			typeAnnotation.accept(methodVisitor.visitTypeAnnotation(typeAnnotation.typeRef, typeAnnotation.typePath, typeAnnotation.desc, true));
		  }
		}
		if (invisibleTypeAnnotations != null)
		{
		  for (int i = 0, n = invisibleTypeAnnotations.Count; i < n; ++i)
		  {
			TypeAnnotationNode typeAnnotation = invisibleTypeAnnotations[i];
			typeAnnotation.accept(methodVisitor.visitTypeAnnotation(typeAnnotation.typeRef, typeAnnotation.typePath, typeAnnotation.desc, false));
		  }
		}
		if (visibleAnnotableParameterCount > 0)
		{
		  methodVisitor.visitAnnotableParameterCount(visibleAnnotableParameterCount, true);
		}
		if (visibleParameterAnnotations != null)
		{
		  for (int i = 0, n = visibleParameterAnnotations.Length; i < n; ++i)
		  {
			List<AnnotationNode> parameterAnnotations = visibleParameterAnnotations[i];
			if (parameterAnnotations == null)
			{
			  continue;
			}
			for (int j = 0, m = parameterAnnotations.Count; j < m; ++j)
			{
			  AnnotationNode annotation = parameterAnnotations[j];
			  annotation.accept(methodVisitor.visitParameterAnnotation(i, annotation.desc, true));
			}
		  }
		}
		if (invisibleAnnotableParameterCount > 0)
		{
		  methodVisitor.visitAnnotableParameterCount(invisibleAnnotableParameterCount, false);
		}
		if (invisibleParameterAnnotations != null)
		{
		  for (int i = 0, n = invisibleParameterAnnotations.Length; i < n; ++i)
		  {
			List<AnnotationNode> parameterAnnotations = invisibleParameterAnnotations[i];
			if (parameterAnnotations == null)
			{
			  continue;
			}
			for (int j = 0, m = parameterAnnotations.Count; j < m; ++j)
			{
			  AnnotationNode annotation = parameterAnnotations[j];
			  annotation.accept(methodVisitor.visitParameterAnnotation(i, annotation.desc, false));
			}
		  }
		}
		// Visit the non standard attributes.
		if (visited)
		{
		  instructions.resetLabels();
		}
		if (attrs != null)
		{
		  for (int i = 0, n = attrs.Count; i < n; ++i)
		  {
			methodVisitor.visitAttribute(attrs[i]);
		  }
		}
		// Visit the code.
		if (instructions.Count() > 0)
		{
		  methodVisitor.visitCode();
		  // Visits the try catch blocks.
		  if (tryCatchBlocks != null)
		  {
			for (int i = 0, n = tryCatchBlocks.Count; i < n; ++i)
			{
			  tryCatchBlocks[i].updateIndex(i);
			  tryCatchBlocks[i].accept(methodVisitor);
			}
		  }
		  // Visit the instructions.
		  instructions.accept(methodVisitor);
		  // Visits the local variables.
		  if (localVariables != null)
		  {
			for (int i = 0, n = localVariables.Count; i < n; ++i)
			{
			  localVariables[i].accept(methodVisitor);
			}
		  }
		  // Visits the local variable annotations.
		  if (visibleLocalVariableAnnotations != null)
		  {
			for (int i = 0, n = visibleLocalVariableAnnotations.Count; i < n; ++i)
			{
			  visibleLocalVariableAnnotations[i].accept(methodVisitor, true);
			}
		  }
		  if (invisibleLocalVariableAnnotations != null)
		  {
			for (int i = 0, n = invisibleLocalVariableAnnotations.Count; i < n; ++i)
			{
			  invisibleLocalVariableAnnotations[i].accept(methodVisitor, false);
			}
		  }
		  methodVisitor.visitMaxs(maxStack, maxLocals);
		  visited = true;
		}
		methodVisitor.visitEnd();
	  }
	}

}