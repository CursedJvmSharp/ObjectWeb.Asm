using org.objectweb.asm;
using CursedJvmSharp.Asm.Java.IO;
using System.Collections.Generic;
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
namespace org.objectweb.asm.tree
{

	/// <summary>
	/// A node that represents a stack map frame. These nodes are pseudo instruction nodes in order to be
	/// inserted in an instruction list. In fact these nodes must(*) be inserted <i>just before</i> any
	/// instruction node <b>i</b> that follows an unconditionnal branch instruction such as GOTO or
	/// THROW, that is the target of a jump instruction, or that starts an exception handler block. The
	/// stack map frame types must describe the values of the local variables and of the operand stack
	/// elements <i>just before</i> <b>i</b> is executed. <br>
	/// <br>
	/// (*) this is mandatory only for classes whose version is greater than or equal to {@link
	/// Opcodes#V1_6}.
	/// 
	/// @author Eric Bruneton
	/// </summary>
	public class FrameNode : AbstractInsnNode
	{

	  /// <summary>
	  /// The type of this frame. Must be <seealso cref="Opcodes.F_NEW"/> for expanded frames, or {@link
	  /// Opcodes#F_FULL}, <seealso cref="Opcodes.F_APPEND"/>, <seealso cref="Opcodes.F_CHOP"/>, <seealso cref="Opcodes.F_SAME"/> or
	  /// <seealso cref="Opcodes.F_APPEND"/>, <seealso cref="Opcodes.F_SAME1"/> for compressed frames.
	  /// </summary>
	  public int type;

	  /// <summary>
	  /// The types of the local variables of this stack map frame. Elements of this list can be Integer,
	  /// String or LabelNode objects (for primitive, reference and uninitialized types respectively -
	  /// see <seealso cref="MethodVisitor"/>).
	  /// </summary>
	  public IList<object> local;

	  /// <summary>
	  /// The types of the operand stack elements of this stack map frame. Elements of this list can be
	  /// Integer, String or LabelNode objects (for primitive, reference and uninitialized types
	  /// respectively - see <seealso cref="MethodVisitor"/>).
	  /// </summary>
	  public IList<object> stack;

	  private FrameNode() : base(-1)
	  {
	  }

	  /// <summary>
	  /// Constructs a new <seealso cref="FrameNode"/>.
	  /// </summary>
	  /// <param name="type"> the type of this frame. Must be <seealso cref="Opcodes.F_NEW"/> for expanded frames, or
	  ///     <seealso cref="Opcodes.F_FULL"/>, <seealso cref="Opcodes.F_APPEND"/>, <seealso cref="Opcodes.F_CHOP"/>, {@link
	  ///     Opcodes#F_SAME} or <seealso cref="Opcodes.F_APPEND"/>, <seealso cref="Opcodes.F_SAME1"/> for compressed frames. </param>
	  /// <param name="numLocal"> number of local variables of this stack map frame. </param>
	  /// <param name="local"> the types of the local variables of this stack map frame. Elements of this list
	  ///     can be Integer, String or LabelNode objects (for primitive, reference and uninitialized
	  ///     types respectively - see <seealso cref="MethodVisitor"/>). </param>
	  /// <param name="numStack"> number of operand stack elements of this stack map frame. </param>
	  /// <param name="stack"> the types of the operand stack elements of this stack map frame. Elements of this
	  ///     list can be Integer, String or LabelNode objects (for primitive, reference and
	  ///     uninitialized types respectively - see <seealso cref="MethodVisitor"/>). </param>
	  public FrameNode(int type, int numLocal, object[] local, int numStack, object[] stack) : base(-1)
	  {
		this.type = type;
		switch (type)
		{
		  case Opcodes.F_NEW:
		  case Opcodes.F_FULL:
			this.local = Util.asArrayList(numLocal, local);
			this.stack = Util.asArrayList(numStack, stack);
			break;
		  case Opcodes.F_APPEND:
			this.local = Util.asArrayList(numLocal, local);
			break;
		  case Opcodes.F_CHOP:
			this.local = Util.asArrayList(numLocal);
			break;
		  case Opcodes.F_SAME:
			break;
		  case Opcodes.F_SAME1:
			this.stack = Util.asArrayList(1, stack);
			break;
		  default:
			throw new System.ArgumentException();
		}
	  }

	  public override int Type
	  {
		  get
		  {
			return FRAME;
		  }
	  }

	  public override void accept(MethodVisitor methodVisitor)
	  {
		switch (type)
		{
		  case Opcodes.F_NEW:
		  case Opcodes.F_FULL:
			methodVisitor.visitFrame(type, local.Count, asArray(local), stack.Count, asArray(stack));
			break;
		  case Opcodes.F_APPEND:
			methodVisitor.visitFrame(type, local.Count, asArray(local), 0, null);
			break;
		  case Opcodes.F_CHOP:
			methodVisitor.visitFrame(type, local.Count, null, 0, null);
			break;
		  case Opcodes.F_SAME:
			methodVisitor.visitFrame(type, 0, null, 0, null);
			break;
		  case Opcodes.F_SAME1:
			methodVisitor.visitFrame(type, 0, null, 1, asArray(stack));
			break;
		  default:
			throw new System.ArgumentException();
		}
	  }

	  public override AbstractInsnNode clone(IDictionary<LabelNode, LabelNode> clonedLabels)
	  {
		FrameNode clone = new FrameNode();
		clone.type = type;
		if (local != null)
		{
		  clone.local = new List<object>();
		  for (int i = 0, n = local.Count; i < n; ++i)
		  {
			object localElement = local[i];
			if (localElement is LabelNode)
			{
			  localElement = clonedLabels.GetValueOrNull(localElement);
			}
			clone.local.Add(localElement);
		  }
		}
		if (stack != null)
		{
		  clone.stack = new List<object>();
		  for (int i = 0, n = stack.Count; i < n; ++i)
		  {
			object stackElement = stack[i];
			if (stackElement is LabelNode)
			{
			  stackElement = clonedLabels.GetValueOrNull(stackElement);
			}
			clone.stack.Add(stackElement);
		  }
		}
		return clone;
	  }

	  private static object[] asArray(IList<object> list)
	  {
		object[] array = new object[list.Count];
		for (int i = 0, n = array.Length; i < n; ++i)
		{
		  object o = list[i];
		  if (o is LabelNode)
		  {
			o = ((LabelNode) o).Label;
		  }
		  array[i] = o;
		}
		return array;
	  }
	}

}