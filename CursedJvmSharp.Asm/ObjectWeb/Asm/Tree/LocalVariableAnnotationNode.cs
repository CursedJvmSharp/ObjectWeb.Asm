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
	/// A node that represents a type annotation on a local or resource variable.
	/// 
	/// @author Eric Bruneton
	/// </summary>
	public class LocalVariableAnnotationNode : TypeAnnotationNode
	{

	  /// <summary>
	  /// The fist instructions corresponding to the continuous ranges that make the scope of this local
	  /// variable (inclusive). Must not be {@literal null}.
	  /// </summary>
	  public List<LabelNode> start;

	  /// <summary>
	  /// The last instructions corresponding to the continuous ranges that make the scope of this local
	  /// variable (exclusive). This list must have the same size as the 'start' list. Must not be
	  /// {@literal null}.
	  /// </summary>
	  public List<LabelNode> end;

	  /// <summary>
	  /// The local variable's index in each range. This list must have the same size as the 'start'
	  /// list. Must not be {@literal null}.
	  /// </summary>
	  public List<int> index;

	  /// <summary>
	  /// Constructs a new <seealso cref="LocalVariableAnnotationNode"/>. <i>Subclasses must not use this
	  /// constructor</i>. Instead, they must use the {@link #LocalVariableAnnotationNode(int, TypePath,
	  /// LabelNode[], LabelNode[], int[], String)} version.
	  /// </summary>
	  /// <param name="typeRef"> a reference to the annotated type. See <seealso cref="TypeReference"/>. </param>
	  /// <param name="typePath"> the path to the annotated type argument, wildcard bound, array element type, or
	  ///     static inner type within 'typeRef'. May be {@literal null} if the annotation targets
	  ///     'typeRef' as a whole. </param>
	  /// <param name="start"> the fist instructions corresponding to the continuous ranges that make the scope
	  ///     of this local variable (inclusive). </param>
	  /// <param name="end"> the last instructions corresponding to the continuous ranges that make the scope of
	  ///     this local variable (exclusive). This array must have the same size as the 'start' array. </param>
	  /// <param name="index"> the local variable's index in each range. This array must have the same size as
	  ///     the 'start' array. </param>
	  /// <param name="descriptor"> the class descriptor of the annotation class. </param>
	  public LocalVariableAnnotationNode(int typeRef, TypePath typePath, LabelNode[] start, LabelNode[] end, int[] index, string descriptor) : this(IOpcodes.Asm9, typeRef, typePath, start, end, index, descriptor)
	  {
	  }

	  /// <summary>
	  /// Constructs a new <seealso cref="LocalVariableAnnotationNode"/>.
	  /// </summary>
	  /// <param name="api"> the ASM API version implemented by this visitor. Must be one of the {@code
	  ///     ASM}<i>x</i> values in <seealso cref="IOpcodes"/>. </param>
	  /// <param name="typeRef"> a reference to the annotated type. See <seealso cref="TypeReference"/>. </param>
	  /// <param name="start"> the fist instructions corresponding to the continuous ranges that make the scope
	  ///     of this local variable (inclusive). </param>
	  /// <param name="end"> the last instructions corresponding to the continuous ranges that make the scope of
	  ///     this local variable (exclusive). This array must have the same size as the 'start' array. </param>
	  /// <param name="index"> the local variable's index in each range. This array must have the same size as
	  ///     the 'start' array. </param>
	  /// <param name="typePath"> the path to the annotated type argument, wildcard bound, array element type, or
	  ///     static inner type within 'typeRef'. May be {@literal null} if the annotation targets
	  ///     'typeRef' as a whole. </param>
	  /// <param name="descriptor"> the class descriptor of the annotation class. </param>
	  public LocalVariableAnnotationNode(int api, int typeRef, TypePath typePath, LabelNode[] start, LabelNode[] end, int[] index, string descriptor) : base(api, typeRef, typePath, descriptor)
	  {
		this.start = Util.AsArrayList(start);
		this.end = Util.AsArrayList(end);
		this.index = Util.AsArrayList(index);
	  }

	  /// <summary>
	  /// Makes the given visitor visit this type annotation.
	  /// </summary>
	  /// <param name="methodVisitor"> the visitor that must visit this annotation. </param>
	  /// <param name="visible"> {@literal true} if the annotation is visible at runtime. </param>
	  public virtual void Accept(MethodVisitor methodVisitor, bool visible)
	  {
		Label[] startLabels = new Label[this.start.Count];
		Label[] endLabels = new Label[this.end.Count];
		int[] indices = new int[this.index.Count];
		for (int i = 0, n = startLabels.Length; i < n; ++i)
		{
		  startLabels[i] = this.start[i].Label;
		  endLabels[i] = this.end[i].Label;
		  indices[i] = this.index[i];
		}
		Accept(methodVisitor.VisitLocalVariableAnnotation(typeRef, typePath, startLabels, endLabels, indices, desc, visible));
	  }
	}

}