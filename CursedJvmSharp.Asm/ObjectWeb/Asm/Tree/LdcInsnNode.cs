using System.Collections.Generic;

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
	/// A node that represents an LDC instruction.
	/// 
	/// @author Eric Bruneton
	/// </summary>
	public class LdcInsnNode : AbstractInsnNode
	{

	  /// <summary>
	  /// The constant to be loaded on the stack. This field must be a non null <seealso cref="Integer"/>, a {@link
	  /// Float}, a <seealso cref="Long"/>, a <seealso cref="Double"/>, a <seealso cref="string"/>, a <seealso cref="Type"/> of OBJECT or ARRAY
	  /// sort for {@code .class} constants, for classes whose version is 49, a <seealso cref="Type"/> of METHOD
	  /// sort for MethodType, a <seealso cref="Handle"/> for MethodHandle constants, for classes whose version is
	  /// 51 or a <seealso cref="ConstantDynamic"/> for a constant dynamic for classes whose version is 55.
	  /// </summary>
	  public object cst;

	  /// <summary>
	  /// Constructs a new <seealso cref="LdcInsnNode"/>.
	  /// </summary>
	  /// <param name="value"> the constant to be loaded on the stack. This parameter mist be a non null {@link
	  ///     Integer}, a <seealso cref="Float"/>, a <seealso cref="Long"/>, a <seealso cref="Double"/>, a <seealso cref="string"/>, a {@link
	  ///     Type} of OBJECT or ARRAY sort for {@code .class} constants, for classes whose version is
	  ///     49, a <seealso cref="Type"/> of METHOD sort for MethodType, a <seealso cref="Handle"/> for MethodHandle
	  ///     constants, for classes whose version is 51 or a <seealso cref="ConstantDynamic"/> for a constant
	  ///     dynamic for classes whose version is 55. </param>
	  public LdcInsnNode(object value) : base(IOpcodes.Ldc)
	  {
		this.cst = value;
	  }

	  public override int Type => Ldc_Insn;

      public override void Accept(MethodVisitor methodVisitor)
	  {
		methodVisitor.VisitLdcInsn(cst);
		AcceptAnnotations(methodVisitor);
	  }

	  public override AbstractInsnNode Clone(IDictionary<LabelNode, LabelNode> clonedLabels)
	  {
		return (new LdcInsnNode(cst)).CloneAnnotations(this);
	  }
	}

}