using org.objectweb.asm;
using CursedJvmSharp.Asm.Java.IO;
using System.Collections.Generic;
using MethodVisitor = org.objectweb.asm.MethodVisitor;

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
	/// A node that represents a bytecode instruction. <i>An instruction can appear at most once in at
	/// most one <seealso cref="InsnList"/> at a time</i>.
	/// 
	/// @author Eric Bruneton
	/// </summary>
	public abstract class AbstractInsnNode
	{

	  /// <summary>
	  /// The type of <seealso cref="InsnNode"/> instructions. </summary>
	  public const int INSN = 0;

	  /// <summary>
	  /// The type of <seealso cref="IntInsnNode"/> instructions. </summary>
	  public const int INT_INSN = 1;

	  /// <summary>
	  /// The type of <seealso cref="VarInsnNode"/> instructions. </summary>
	  public const int VAR_INSN = 2;

	  /// <summary>
	  /// The type of <seealso cref="TypeInsnNode"/> instructions. </summary>
	  public const int TYPE_INSN = 3;

	  /// <summary>
	  /// The type of <seealso cref="FieldInsnNode"/> instructions. </summary>
	  public const int FIELD_INSN = 4;

	  /// <summary>
	  /// The type of <seealso cref="MethodInsnNode"/> instructions. </summary>
	  public const int METHOD_INSN = 5;

	  /// <summary>
	  /// The type of <seealso cref="InvokeDynamicInsnNode"/> instructions. </summary>
	  public const int INVOKE_DYNAMIC_INSN = 6;

	  /// <summary>
	  /// The type of <seealso cref="JumpInsnNode"/> instructions. </summary>
	  public const int JUMP_INSN = 7;

	  /// <summary>
	  /// The type of <seealso cref="LabelNode"/> "instructions". </summary>
	  public const int LABEL = 8;

	  /// <summary>
	  /// The type of <seealso cref="LdcInsnNode"/> instructions. </summary>
	  public const int LDC_INSN = 9;

	  /// <summary>
	  /// The type of <seealso cref="IincInsnNode"/> instructions. </summary>
	  public const int IINC_INSN = 10;

	  /// <summary>
	  /// The type of <seealso cref="TableSwitchInsnNode"/> instructions. </summary>
	  public const int TABLESWITCH_INSN = 11;

	  /// <summary>
	  /// The type of <seealso cref="LookupSwitchInsnNode"/> instructions. </summary>
	  public const int LOOKUPSWITCH_INSN = 12;

	  /// <summary>
	  /// The type of <seealso cref="MultiANewArrayInsnNode"/> instructions. </summary>
	  public const int MULTIANEWARRAY_INSN = 13;

	  /// <summary>
	  /// The type of <seealso cref="FrameNode"/> "instructions". </summary>
	  public const int FRAME = 14;

	  /// <summary>
	  /// The type of <seealso cref="LineNumberNode"/> "instructions". </summary>
	  public const int LINE = 15;

	  /// <summary>
	  /// The opcode of this instruction. </summary>
	  protected internal int opcode;

	  /// <summary>
	  /// The runtime visible type annotations of this instruction. This field is only used for real
	  /// instructions (i.e. not for labels, frames, or line number nodes). This list is a list of {@link
	  /// TypeAnnotationNode} objects. May be {@literal null}.
	  /// </summary>
	  public IList<TypeAnnotationNode> visibleTypeAnnotations;

	  /// <summary>
	  /// The runtime invisible type annotations of this instruction. This field is only used for real
	  /// instructions (i.e. not for labels, frames, or line number nodes). This list is a list of {@link
	  /// TypeAnnotationNode} objects. May be {@literal null}.
	  /// </summary>
	  public IList<TypeAnnotationNode> invisibleTypeAnnotations;

	  /// <summary>
	  /// The previous instruction in the list to which this instruction belongs. </summary>
	  internal AbstractInsnNode previousInsn;

	  /// <summary>
	  /// The next instruction in the list to which this instruction belongs. </summary>
	  internal AbstractInsnNode nextInsn;

	  /// <summary>
	  /// The index of this instruction in the list to which it belongs. The value of this field is
	  /// correct only when <seealso cref="InsnList.cache"/> is not null. A value of -1 indicates that this
	  /// instruction does not belong to any <seealso cref="InsnList"/>.
	  /// </summary>
	  internal int index;

	  /// <summary>
	  /// Constructs a new <seealso cref="AbstractInsnNode"/>.
	  /// </summary>
	  /// <param name="opcode"> the opcode of the instruction to be constructed. </param>
	  public AbstractInsnNode(int opcode)
	  {
		this.opcode = opcode;
		this.index = -1;
	  }

	  /// <summary>
	  /// Returns the opcode of this instruction.
	  /// </summary>
	  /// <returns> the opcode of this instruction. </returns>
	  public virtual int Opcode
	  {
		  get
		  {
			return opcode;
		  }
	  }

	  /// <summary>
	  /// Returns the type of this instruction.
	  /// </summary>
	  /// <returns> the type of this instruction, i.e. one the constants defined in this class. </returns>
	  public abstract int Type {get;}

	  /// <summary>
	  /// Returns the previous instruction in the list to which this instruction belongs, if any.
	  /// </summary>
	  /// <returns> the previous instruction in the list to which this instruction belongs, if any. May be
	  ///     {@literal null}. </returns>
	  public virtual AbstractInsnNode Previous
	  {
		  get
		  {
			return previousInsn;
		  }
	  }

	  /// <summary>
	  /// Returns the next instruction in the list to which this instruction belongs, if any.
	  /// </summary>
	  /// <returns> the next instruction in the list to which this instruction belongs, if any. May be
	  ///     {@literal null}. </returns>
	  public virtual AbstractInsnNode Next
	  {
		  get
		  {
			return nextInsn;
		  }
	  }

	  /// <summary>
	  /// Makes the given method visitor visit this instruction.
	  /// </summary>
	  /// <param name="methodVisitor"> a method visitor. </param>
	  public abstract void accept(MethodVisitor methodVisitor);

	  /// <summary>
	  /// Makes the given visitor visit the annotations of this instruction.
	  /// </summary>
	  /// <param name="methodVisitor"> a method visitor. </param>
	  public void acceptAnnotations(MethodVisitor methodVisitor)
	  {
		if (visibleTypeAnnotations != null)
		{
		  for (int i = 0, n = visibleTypeAnnotations.Count; i < n; ++i)
		  {
			TypeAnnotationNode typeAnnotation = visibleTypeAnnotations[i];
			typeAnnotation.accept(methodVisitor.visitInsnAnnotation(typeAnnotation.typeRef, typeAnnotation.typePath, typeAnnotation.desc, true));
		  }
		}
		if (invisibleTypeAnnotations != null)
		{
		  for (int i = 0, n = invisibleTypeAnnotations.Count; i < n; ++i)
		  {
			TypeAnnotationNode typeAnnotation = invisibleTypeAnnotations[i];
			typeAnnotation.accept(methodVisitor.visitInsnAnnotation(typeAnnotation.typeRef, typeAnnotation.typePath, typeAnnotation.desc, false));
		  }
		}
	  }

	  /// <summary>
	  /// Returns a copy of this instruction.
	  /// </summary>
	  /// <param name="clonedLabels"> a map from LabelNodes to cloned LabelNodes. </param>
	  /// <returns> a copy of this instruction. The returned instruction does not belong to any {@link
	  ///     InsnList}. </returns>
	  public abstract AbstractInsnNode clone(IDictionary<LabelNode, LabelNode> clonedLabels);

	  /// <summary>
	  /// Returns the clone of the given label.
	  /// </summary>
	  /// <param name="label"> a label. </param>
	  /// <param name="clonedLabels"> a map from LabelNodes to cloned LabelNodes. </param>
	  /// <returns> the clone of the given label. </returns>
	  internal static LabelNode clone(LabelNode label, IDictionary<LabelNode, LabelNode> clonedLabels)
	  {
		return clonedLabels.GetValueOrNull(label);
	  }

	  /// <summary>
	  /// Returns the clones of the given labels.
	  /// </summary>
	  /// <param name="labels"> a list of labels. </param>
	  /// <param name="clonedLabels"> a map from LabelNodes to cloned LabelNodes. </param>
	  /// <returns> the clones of the given labels. </returns>
	  internal static LabelNode[] clone(IList<LabelNode> labels, IDictionary<LabelNode, LabelNode> clonedLabels)
	  {
		LabelNode[] clones = new LabelNode[labels.Count];
		for (int i = 0, n = clones.Length; i < n; ++i)
		{
		  clones[i] = clonedLabels.GetValueOrNull(labels[i]);
		}
		return clones;
	  }

	  /// <summary>
	  /// Clones the annotations of the given instruction into this instruction.
	  /// </summary>
	  /// <param name="insnNode"> the source instruction. </param>
	  /// <returns> this instruction. </returns>
	  public AbstractInsnNode cloneAnnotations(AbstractInsnNode insnNode)
	  {
		if (insnNode.visibleTypeAnnotations != null)
		{
		  this.visibleTypeAnnotations = new List<TypeAnnotationNode>();
		  for (int i = 0, n = insnNode.visibleTypeAnnotations.Count; i < n; ++i)
		  {
			TypeAnnotationNode sourceAnnotation = insnNode.visibleTypeAnnotations[i];
			TypeAnnotationNode cloneAnnotation = new TypeAnnotationNode(sourceAnnotation.typeRef, sourceAnnotation.typePath, sourceAnnotation.desc);
			sourceAnnotation.accept(cloneAnnotation);
			this.visibleTypeAnnotations.Add(cloneAnnotation);
		  }
		}
		if (insnNode.invisibleTypeAnnotations != null)
		{
		  this.invisibleTypeAnnotations = new List<TypeAnnotationNode>();
		  for (int i = 0, n = insnNode.invisibleTypeAnnotations.Count; i < n; ++i)
		  {
			TypeAnnotationNode sourceAnnotation = insnNode.invisibleTypeAnnotations[i];
			TypeAnnotationNode cloneAnnotation = new TypeAnnotationNode(sourceAnnotation.typeRef, sourceAnnotation.typePath, sourceAnnotation.desc);
			sourceAnnotation.accept(cloneAnnotation);
			this.invisibleTypeAnnotations.Add(cloneAnnotation);
		  }
		}
		return this;
	  }
	}

}