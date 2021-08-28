using org.objectweb.asm;
using CursedJvmSharp.Asm.Java.IO;
using System.Collections.Generic;
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
namespace org.objectweb.asm.tree
{

	/// <summary>
	/// A node that represents a LOOKUPSWITCH instruction.
	/// 
	/// @author Eric Bruneton
	/// </summary>
	public class LookupSwitchInsnNode : AbstractInsnNode
	{

	  /// <summary>
	  /// Beginning of the default handler block. </summary>
	  public LabelNode dflt;

	  /// <summary>
	  /// The values of the keys. </summary>
	  public List<int> keys;

	  /// <summary>
	  /// Beginnings of the handler blocks. </summary>
	  public List<LabelNode> labels;

	  /// <summary>
	  /// Constructs a new <seealso cref="LookupSwitchInsnNode"/>.
	  /// </summary>
	  /// <param name="dflt"> beginning of the default handler block. </param>
	  /// <param name="keys"> the values of the keys. </param>
	  /// <param name="labels"> beginnings of the handler blocks. {@code labels[i]} is the beginning of the
	  ///     handler block for the {@code keys[i]} key. </param>
	  public LookupSwitchInsnNode(LabelNode dflt, int[] keys, LabelNode[] labels) : base(Opcodes.LOOKUPSWITCH)
	  {
		this.dflt = dflt;
		this.keys = Util.asArrayList(keys);
		this.labels = Util.asArrayList(labels);
	  }

	  public override int Type
	  {
		  get
		  {
			return LOOKUPSWITCH_INSN;
		  }
	  }

	  public override void accept(MethodVisitor methodVisitor)
	  {
		int[] keysArray = new int[this.keys.Count];
		for (int i = 0, n = keysArray.Length; i < n; ++i)
		{
		  keysArray[i] = this.keys[i];
		}
		Label[] labelsArray = new Label[this.labels.Count];
		for (int i = 0, n = labelsArray.Length; i < n; ++i)
		{
		  labelsArray[i] = this.labels[i].Label;
		}
		methodVisitor.visitLookupSwitchInsn(dflt.Label, keysArray, labelsArray);
		acceptAnnotations(methodVisitor);
	  }

	  public override AbstractInsnNode clone(IDictionary<LabelNode, LabelNode> clonedLabels)
	  {
		LookupSwitchInsnNode clone = new LookupSwitchInsnNode(LookupSwitchInsnNode.clone(dflt, clonedLabels), null, LookupSwitchInsnNode.clone(labels, clonedLabels));
		((List<int>)clone.keys).AddRange(keys);
		return clone.cloneAnnotations(this);
	  }
	}

}