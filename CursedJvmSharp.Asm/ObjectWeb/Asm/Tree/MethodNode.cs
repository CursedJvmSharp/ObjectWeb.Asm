using System;
using System.Collections.Generic;
using System.Linq;

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
namespace ObjectWeb.Asm.Tree
{

	/// <summary>
	/// A node that represents a method.
	/// 
	/// @author Eric Bruneton
	/// </summary>
	public class MethodNode : MethodVisitor
	{

	  /// <summary>
	  /// The method's access flags (see <seealso cref="IOpcodes"/>). This field also indicates if the method is
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
	  private bool _visited;

	  /// <summary>
	  /// Constructs an uninitialized <seealso cref="MethodNode"/>. <i>Subclasses must not use this
	  /// constructor</i>. Instead, they must use the <seealso cref="MethodNode(int)"/> version.
	  /// </summary>
	  /// <exception cref="IllegalStateException"> If a subclass calls this constructor. </exception>
	  public MethodNode() : this(IOpcodes.Asm9)
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
	  ///     ASM}<i>x</i> values in <seealso cref="IOpcodes"/>. </param>
	  public MethodNode(int api) : base(api)
	  {
		this.instructions = new InsnList();
	  }

	  /// <summary>
	  /// Constructs a new <seealso cref="MethodNode"/>. <i>Subclasses must not use this constructor</i>. Instead,
	  /// they must use the <seealso cref="MethodNode(int, int, String, String, String, string[])"/> version.
	  /// </summary>
	  /// <param name="access"> the method's access flags (see <seealso cref="IOpcodes"/>). This parameter also indicates if
	  ///     the method is synthetic and/or deprecated. </param>
	  /// <param name="name"> the method's name. </param>
	  /// <param name="descriptor"> the method's descriptor (see <seealso cref="Type"/>). </param>
	  /// <param name="signature"> the method's signature. May be {@literal null}. </param>
	  /// <param name="exceptions"> the internal names of the method's exception classes (see {@link
	  ///     Type#getInternalName()}). May be {@literal null}. </param>
	  /// <exception cref="IllegalStateException"> If a subclass calls this constructor. </exception>
	  public MethodNode(int access, string name, string descriptor, string signature, string[] exceptions) : this(IOpcodes.Asm9, access, name, descriptor, signature, exceptions)
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
	  ///     ASM}<i>x</i> values in <seealso cref="IOpcodes"/>. </param>
	  /// <param name="access"> the method's access flags (see <seealso cref="IOpcodes"/>). This parameter also indicates if
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
		this.exceptions = Util.AsArrayList(exceptions);
		if ((access & IOpcodes.Acc_Abstract) == 0)
		{
		  this.localVariables = new List<LocalVariableNode>(5);
		}
		this.tryCatchBlocks = new List<TryCatchBlockNode>();
		this.instructions = new InsnList();
	  }

	  // -----------------------------------------------------------------------------------------------
	  // Implementation of the MethodVisitor abstract class
	  // -----------------------------------------------------------------------------------------------

	  public override void VisitParameter(string name, int access)
	  {
		if (parameters == null)
		{
		  parameters = new List<ParameterNode>(5);
		}
		parameters.Add(new ParameterNode(name, access));
	  }

	  public override AnnotationVisitor VisitAnnotationDefault()
	  {
		return new AnnotationNode(new ArrayListAnonymousInnerClass(this));
	  }

	  private class ArrayListAnonymousInnerClass : List<object>
	  {
		  private readonly MethodNode _outerInstance;

		  public ArrayListAnonymousInnerClass(MethodNode outerInstance) : base(0)
		  {
			  this._outerInstance = outerInstance;
		  }

		  public new bool Add(object o)
		  {
			_outerInstance.annotationDefault = o;
			base.Add(o);
            return true;
          }
	  }

	  public override AnnotationVisitor VisitAnnotation(string descriptor, bool visible)
	  {
		var annotation = new AnnotationNode(descriptor);
		if (visible)
		{
		  visibleAnnotations = Util.Add(visibleAnnotations, annotation);
		}
		else
		{
		  invisibleAnnotations = Util.Add(invisibleAnnotations, annotation);
		}
		return annotation;
	  }

	  public override AnnotationVisitor VisitTypeAnnotation(int typeRef, TypePath typePath, string descriptor, bool visible)
	  {
		var typeAnnotation = new TypeAnnotationNode(typeRef, typePath, descriptor);
		if (visible)
		{
		  visibleTypeAnnotations = Util.Add(visibleTypeAnnotations, typeAnnotation);
		}
		else
		{
		  invisibleTypeAnnotations = Util.Add(invisibleTypeAnnotations, typeAnnotation);
		}
		return typeAnnotation;
	  }

	  public override void VisitAnnotableParameterCount(int parameterCount, bool visible)
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

	  public override AnnotationVisitor VisitParameterAnnotation(int parameter, string descriptor, bool visible)
	  {
		var annotation = new AnnotationNode(descriptor);
		if (visible)
		{
		  if (visibleParameterAnnotations == null)
		  {
			var @params = JType.GetArgumentTypes(desc).Length;
			visibleParameterAnnotations = new List<AnnotationNode>[@params];
		  }
		  visibleParameterAnnotations[parameter] = Util.Add(visibleParameterAnnotations[parameter], annotation);
		}
		else
		{
		  if (invisibleParameterAnnotations == null)
		  {
			var @params = JType.GetArgumentTypes(desc).Length;
			invisibleParameterAnnotations = new List<AnnotationNode>[@params];
		  }
		  invisibleParameterAnnotations[parameter] = Util.Add(invisibleParameterAnnotations[parameter], annotation);
		}
		return annotation;
	  }

	  public override void VisitAttribute(Attribute attribute)
	  {
		attrs = Util.Add(attrs, attribute);
	  }

	  public override void VisitCode()
	  {
		// Nothing to do.
	  }

	  public override void VisitFrame(int type, int numLocal, object[] local, int numStack, object[] stack)
	  {
		instructions.Add(new FrameNode(type, numLocal, local == null ? null : GetLabelNodes(local), numStack, stack == null ? null : GetLabelNodes(stack)));
	  }

	  public override void VisitInsn(int opcode)
	  {
		instructions.Add(new InsnNode(opcode));
	  }

	  public override void VisitIntInsn(int opcode, int operand)
	  {
		instructions.Add(new IntInsnNode(opcode, operand));
	  }

	  public override void VisitVarInsn(int opcode, int var)
	  {
		instructions.Add(new VarInsnNode(opcode, var));
	  }

	  public override void VisitTypeInsn(int opcode, string type)
	  {
		instructions.Add(new TypeInsnNode(opcode, type));
	  }

	  public override void VisitFieldInsn(int opcode, string owner, string name, string descriptor)
	  {
		instructions.Add(new FieldInsnNode(opcode, owner, name, descriptor));
	  }

	  public override void VisitMethodInsn(int opcodeAndSource, string owner, string name, string descriptor, bool isInterface)
	  {
		if (api < IOpcodes.Asm5 && (opcodeAndSource & IOpcodes.Source_Deprecated) == 0)
		{
		  // Redirect the call to the deprecated version of this method.
		  base.VisitMethodInsn(opcodeAndSource, owner, name, descriptor, isInterface);
		  return;
		}
		var opcode = opcodeAndSource & ~IOpcodes.Source_Mask;

		instructions.Add(new MethodInsnNode(opcode, owner, name, descriptor, isInterface));
	  }

	  public override void VisitInvokeDynamicInsn(string name, string descriptor, Handle bootstrapMethodHandle, params object[] bootstrapMethodArguments)
	  {
		instructions.Add(new InvokeDynamicInsnNode(name, descriptor, bootstrapMethodHandle, bootstrapMethodArguments));
	  }

	  public override void VisitJumpInsn(int opcode, Label label)
	  {
		instructions.Add(new JumpInsnNode(opcode, GetLabelNode(label)));
	  }

	  public override void VisitLabel(Label label)
	  {
		instructions.Add(GetLabelNode(label));
	  }

	  public override void VisitLdcInsn(object value)
	  {
		instructions.Add(new LdcInsnNode(value));
	  }

	  public override void VisitIincInsn(int var, int increment)
	  {
		instructions.Add(new IincInsnNode(var, increment));
	  }

	  public override void VisitTableSwitchInsn(int min, int max, Label dflt, params Label[] labels)
	  {
		instructions.Add(new TableSwitchInsnNode(min, max, GetLabelNode(dflt), GetLabelNodes(labels)));
	  }

	  public override void VisitLookupSwitchInsn(Label dflt, int[] keys, Label[] labels)
	  {
		instructions.Add(new LookupSwitchInsnNode(GetLabelNode(dflt), keys, GetLabelNodes(labels)));
	  }

	  public override void VisitMultiANewArrayInsn(string descriptor, int numDimensions)
	  {
		instructions.Add(new MultiANewArrayInsnNode(descriptor, numDimensions));
	  }

	  public override AnnotationVisitor VisitInsnAnnotation(int typeRef, TypePath typePath, string descriptor, bool visible)
	  {
		// Find the last real instruction, i.e. the instruction targeted by this annotation.
		var currentInsn = instructions.Last;
		while (currentInsn.Opcode == -1)
		{
		  currentInsn = currentInsn.Previous;
		}
		// Add the annotation to this instruction.
		var typeAnnotation = new TypeAnnotationNode(typeRef, typePath, descriptor);
		if (visible)
		{
		  currentInsn.visibleTypeAnnotations = Util.Add(currentInsn.visibleTypeAnnotations, typeAnnotation);
		}
		else
		{
		  currentInsn.invisibleTypeAnnotations = Util.Add(currentInsn.invisibleTypeAnnotations, typeAnnotation);
		}
		return typeAnnotation;
	  }

	  public override void VisitTryCatchBlock(Label start, Label end, Label handler, string type)
	  {
		var tryCatchBlock = new TryCatchBlockNode(GetLabelNode(start), GetLabelNode(end), GetLabelNode(handler), type);
		tryCatchBlocks = Util.Add(tryCatchBlocks, tryCatchBlock);
	  }

	  public override AnnotationVisitor VisitTryCatchAnnotation(int typeRef, TypePath typePath, string descriptor, bool visible)
	  {
		var tryCatchBlock = tryCatchBlocks[(typeRef & 0x00FFFF00) >> 8];
		var typeAnnotation = new TypeAnnotationNode(typeRef, typePath, descriptor);
		if (visible)
		{
		  tryCatchBlock.visibleTypeAnnotations = Util.Add(tryCatchBlock.visibleTypeAnnotations, typeAnnotation);
		}
		else
		{
		  tryCatchBlock.invisibleTypeAnnotations = Util.Add(tryCatchBlock.invisibleTypeAnnotations, typeAnnotation);
		}
		return typeAnnotation;
	  }

	  public override void VisitLocalVariable(string name, string descriptor, string signature, Label start, Label end, int index)
	  {
		var localVariable = new LocalVariableNode(name, descriptor, signature, GetLabelNode(start), GetLabelNode(end), index);
		localVariables = Util.Add(localVariables, localVariable);
	  }

	  public override AnnotationVisitor VisitLocalVariableAnnotation(int typeRef, TypePath typePath, Label[] start, Label[] end, int[] index, string descriptor, bool visible)
	  {
		var localVariableAnnotation = new LocalVariableAnnotationNode(typeRef, typePath, GetLabelNodes(start), GetLabelNodes(end), index, descriptor);
		if (visible)
		{
		  visibleLocalVariableAnnotations = Util.Add(visibleLocalVariableAnnotations, localVariableAnnotation);
		}
		else
		{
		  invisibleLocalVariableAnnotations = Util.Add(invisibleLocalVariableAnnotations, localVariableAnnotation);
		}
		return localVariableAnnotation;
	  }

	  public override void VisitLineNumber(int line, Label start)
	  {
		instructions.Add(new LineNumberNode(line, GetLabelNode(start)));
	  }

	  public override void VisitMaxs(int maxStack, int maxLocals)
	  {
		this.maxStack = maxStack;
		this.maxLocals = maxLocals;
	  }

	  public override void VisitEnd()
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
	  public virtual LabelNode GetLabelNode(Label label)
	  {
		if (!(label.info is LabelNode))
		{
		  label.info = new LabelNode();
		}
		return (LabelNode) label.info;
	  }

	  private LabelNode[] GetLabelNodes(Label[] labels)
	  {
		var labelNodes = new LabelNode[labels.Length];
		for (int i = 0, n = labels.Length; i < n; ++i)
		{
		  labelNodes[i] = GetLabelNode(labels[i]);
		}
		return labelNodes;
	  }

	  private object[] GetLabelNodes(object[] objects)
	  {
		var labelNodes = new object[objects.Length];
		for (int i = 0, n = objects.Length; i < n; ++i)
		{
		  var o = objects[i];
		  if (o is Label)
		  {
			o = GetLabelNode((Label) o);
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
	  public virtual void Check(int api)
	  {
		if (api == IOpcodes.Asm4)
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
			for (var i = tryCatchBlocks.Count - 1; i >= 0; --i)
			{
			  var tryCatchBlock = tryCatchBlocks[i];
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
		  for (var i = instructions.Count() - 1; i >= 0; --i)
		  {
			var insn = instructions.Get(i);
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
			  var isInterface = ((MethodInsnNode) insn).itf;
			  if (isInterface != (insn.opcode == IOpcodes.Invokeinterface))
			  {
				throw new UnsupportedClassVersionException();
			  }
			}
			else if (insn is LdcInsnNode)
			{
			  var value = ((LdcInsnNode) insn).cst;
			  if (value is Handle || (value is JType && ((JType) value).Sort == JType.Method))
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
		if (api < IOpcodes.Asm7)
		{
		  for (var i = instructions.Count() - 1; i >= 0; --i)
		  {
			var insn = instructions.Get(i);
			if (insn is LdcInsnNode)
			{
			  var value = ((LdcInsnNode) insn).cst;
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
	  public virtual void Accept(ClassVisitor classVisitor)
	  {
		var exceptionsArray = exceptions == null ? null : ((List<string>)exceptions).ToArray();
		var methodVisitor = classVisitor.VisitMethod(access, name, desc, signature, exceptionsArray);
		if (methodVisitor != null)
		{
		  Accept(methodVisitor);
		}
	  }

	  /// <summary>
	  /// Makes the given method visitor visit this method.
	  /// </summary>
	  /// <param name="methodVisitor"> a method visitor. </param>
	  public virtual void Accept(MethodVisitor methodVisitor)
	  {
		// Visit the parameters.
		if (parameters != null)
		{
		  for (int i = 0, n = parameters.Count; i < n; i++)
		  {
			parameters[i].Accept(methodVisitor);
		  }
		}
		// Visit the annotations.
		if (annotationDefault != null)
		{
		  var annotationVisitor = methodVisitor.VisitAnnotationDefault();
		  AnnotationNode.Accept(annotationVisitor, null, annotationDefault);
		  if (annotationVisitor != null)
		  {
			annotationVisitor.VisitEnd();
		  }
		}
		if (visibleAnnotations != null)
		{
		  for (int i = 0, n = visibleAnnotations.Count; i < n; ++i)
		  {
			var annotation = visibleAnnotations[i];
			annotation.Accept(methodVisitor.VisitAnnotation(annotation.desc, true));
		  }
		}
		if (invisibleAnnotations != null)
		{
		  for (int i = 0, n = invisibleAnnotations.Count; i < n; ++i)
		  {
			var annotation = invisibleAnnotations[i];
			annotation.Accept(methodVisitor.VisitAnnotation(annotation.desc, false));
		  }
		}
		if (visibleTypeAnnotations != null)
		{
		  for (int i = 0, n = visibleTypeAnnotations.Count; i < n; ++i)
		  {
			var typeAnnotation = visibleTypeAnnotations[i];
			typeAnnotation.Accept(methodVisitor.VisitTypeAnnotation(typeAnnotation.typeRef, typeAnnotation.typePath, typeAnnotation.desc, true));
		  }
		}
		if (invisibleTypeAnnotations != null)
		{
		  for (int i = 0, n = invisibleTypeAnnotations.Count; i < n; ++i)
		  {
			var typeAnnotation = invisibleTypeAnnotations[i];
			typeAnnotation.Accept(methodVisitor.VisitTypeAnnotation(typeAnnotation.typeRef, typeAnnotation.typePath, typeAnnotation.desc, false));
		  }
		}
		if (visibleAnnotableParameterCount > 0)
		{
		  methodVisitor.VisitAnnotableParameterCount(visibleAnnotableParameterCount, true);
		}
		if (visibleParameterAnnotations != null)
		{
		  for (int i = 0, n = visibleParameterAnnotations.Length; i < n; ++i)
		  {
			var parameterAnnotations = visibleParameterAnnotations[i];
			if (parameterAnnotations == null)
			{
			  continue;
			}
			for (int j = 0, m = parameterAnnotations.Count; j < m; ++j)
			{
			  var annotation = parameterAnnotations[j];
			  annotation.Accept(methodVisitor.VisitParameterAnnotation(i, annotation.desc, true));
			}
		  }
		}
		if (invisibleAnnotableParameterCount > 0)
		{
		  methodVisitor.VisitAnnotableParameterCount(invisibleAnnotableParameterCount, false);
		}
		if (invisibleParameterAnnotations != null)
		{
		  for (int i = 0, n = invisibleParameterAnnotations.Length; i < n; ++i)
		  {
			var parameterAnnotations = invisibleParameterAnnotations[i];
			if (parameterAnnotations == null)
			{
			  continue;
			}
			for (int j = 0, m = parameterAnnotations.Count; j < m; ++j)
			{
			  var annotation = parameterAnnotations[j];
			  annotation.Accept(methodVisitor.VisitParameterAnnotation(i, annotation.desc, false));
			}
		  }
		}
		// Visit the non standard attributes.
		if (_visited)
		{
		  instructions.ResetLabels();
		}
		if (attrs != null)
		{
		  for (int i = 0, n = attrs.Count; i < n; ++i)
		  {
			methodVisitor.VisitAttribute(attrs[i]);
		  }
		}
		// Visit the code.
		if (instructions.Count() > 0)
		{
		  methodVisitor.VisitCode();
		  // Visits the try catch blocks.
		  if (tryCatchBlocks != null)
		  {
			for (int i = 0, n = tryCatchBlocks.Count; i < n; ++i)
			{
			  tryCatchBlocks[i].UpdateIndex(i);
			  tryCatchBlocks[i].Accept(methodVisitor);
			}
		  }
		  // Visit the instructions.
		  instructions.Accept(methodVisitor);
		  // Visits the local variables.
		  if (localVariables != null)
		  {
			for (int i = 0, n = localVariables.Count; i < n; ++i)
			{
			  localVariables[i].Accept(methodVisitor);
			}
		  }
		  // Visits the local variable annotations.
		  if (visibleLocalVariableAnnotations != null)
		  {
			for (int i = 0, n = visibleLocalVariableAnnotations.Count; i < n; ++i)
			{
			  visibleLocalVariableAnnotations[i].Accept(methodVisitor, true);
			}
		  }
		  if (invisibleLocalVariableAnnotations != null)
		  {
			for (int i = 0, n = invisibleLocalVariableAnnotations.Count; i < n; ++i)
			{
			  invisibleLocalVariableAnnotations[i].Accept(methodVisitor, false);
			}
		  }
		  methodVisitor.VisitMaxs(maxStack, maxLocals);
		  _visited = true;
		}
		methodVisitor.VisitEnd();
	  }
	}

}