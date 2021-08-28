﻿using System.Collections.Generic;

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
	/// A node that represents a TABLESWITCH instruction.
	/// 
	/// @author Eric Bruneton
	/// </summary>
	public class TableSwitchInsnNode : AbstractInsnNode
	{

	  /// <summary>
	  /// The minimum key value. </summary>
	  public int min;

	  /// <summary>
	  /// The maximum key value. </summary>
	  public int max;

	  /// <summary>
	  /// Beginning of the default handler block. </summary>
	  public LabelNode dflt;

	  /// <summary>
	  /// Beginnings of the handler blocks. This list is a list of <seealso cref="LabelNode"/> objects. </summary>
	  public List<LabelNode> labels;

	  /// <summary>
	  /// Constructs a new <seealso cref="TableSwitchInsnNode"/>.
	  /// </summary>
	  /// <param name="min"> the minimum key value. </param>
	  /// <param name="max"> the maximum key value. </param>
	  /// <param name="dflt"> beginning of the default handler block. </param>
	  /// <param name="labels"> beginnings of the handler blocks. {@code labels[i]} is the beginning of the
	  ///     handler block for the {@code min + i} key. </param>
	  public TableSwitchInsnNode(int min, int max, LabelNode dflt, params LabelNode[] labels) : base(IOpcodes.Tableswitch)
	  {
		this.min = min;
		this.max = max;
		this.dflt = dflt;
		this.labels = Util.AsArrayList(labels);
	  }

	  public override int Type
	  {
		  get
		  {
			return Tableswitch_Insn;
		  }
	  }

	  public override void Accept(MethodVisitor methodVisitor)
	  {
		Label[] labelsArray = new Label[this.labels.Count];
		for (int i = 0, n = labelsArray.Length; i < n; ++i)
		{
		  labelsArray[i] = this.labels[i].Label;
		}
		methodVisitor.VisitTableSwitchInsn(min, max, dflt.Label, labelsArray);
		AcceptAnnotations(methodVisitor);
	  }

	  public override AbstractInsnNode Clone(IDictionary<LabelNode, LabelNode> clonedLabels)
	  {
		return (new TableSwitchInsnNode(min, max, Clone(dflt, clonedLabels), Clone(labels, clonedLabels))).CloneAnnotations(this);
	  }
	}

}