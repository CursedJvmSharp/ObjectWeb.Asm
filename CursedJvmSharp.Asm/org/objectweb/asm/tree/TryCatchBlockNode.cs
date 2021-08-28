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
	/// A node that represents a try catch block.
	/// 
	/// @author Eric Bruneton
	/// </summary>
	public class TryCatchBlockNode
	{

	  /// <summary>
	  /// The beginning of the exception handler's scope (inclusive). </summary>
	  public LabelNode start;

	  /// <summary>
	  /// The end of the exception handler's scope (exclusive). </summary>
	  public LabelNode end;

	  /// <summary>
	  /// The beginning of the exception handler's code. </summary>
	  public LabelNode handler;

	  /// <summary>
	  /// The internal name of the type of exceptions handled by the handler. May be {@literal null} to
	  /// catch any exceptions (for "finally" blocks).
	  /// </summary>
	  public string type;

	  /// <summary>
	  /// The runtime visible type annotations on the exception handler type. May be {@literal null}. </summary>
	  public IList<TypeAnnotationNode> visibleTypeAnnotations;

	  /// <summary>
	  /// The runtime invisible type annotations on the exception handler type. May be {@literal null}.
	  /// </summary>
	  public IList<TypeAnnotationNode> invisibleTypeAnnotations;

	  /// <summary>
	  /// Constructs a new <seealso cref="TryCatchBlockNode"/>.
	  /// </summary>
	  /// <param name="start"> the beginning of the exception handler's scope (inclusive). </param>
	  /// <param name="end"> the end of the exception handler's scope (exclusive). </param>
	  /// <param name="handler"> the beginning of the exception handler's code. </param>
	  /// <param name="type"> the internal name of the type of exceptions handled by the handler, or {@literal
	  ///     null} to catch any exceptions (for "finally" blocks). </param>
	  public TryCatchBlockNode(LabelNode start, LabelNode end, LabelNode handler, string type)
	  {
		this.start = start;
		this.end = end;
		this.handler = handler;
		this.type = type;
	  }

	  /// <summary>
	  /// Updates the index of this try catch block in the method's list of try catch block nodes. This
	  /// index maybe stored in the 'target' field of the type annotations of this block.
	  /// </summary>
	  /// <param name="index"> the new index of this try catch block in the method's list of try catch block
	  ///     nodes. </param>
	  public virtual void updateIndex(int index)
	  {
		int newTypeRef = 0x42000000 | (index << 8);
		if (visibleTypeAnnotations != null)
		{
		  for (int i = 0, n = visibleTypeAnnotations.Count; i < n; ++i)
		  {
			visibleTypeAnnotations[i].typeRef = newTypeRef;
		  }
		}
		if (invisibleTypeAnnotations != null)
		{
		  for (int i = 0, n = invisibleTypeAnnotations.Count; i < n; ++i)
		  {
			invisibleTypeAnnotations[i].typeRef = newTypeRef;
		  }
		}
	  }

	  /// <summary>
	  /// Makes the given visitor visit this try catch block.
	  /// </summary>
	  /// <param name="methodVisitor"> a method visitor. </param>
	  public virtual void accept(MethodVisitor methodVisitor)
	  {
		methodVisitor.visitTryCatchBlock(start.Label, end.Label, handler == null ? null : handler.Label, type);
		if (visibleTypeAnnotations != null)
		{
		  for (int i = 0, n = visibleTypeAnnotations.Count; i < n; ++i)
		  {
			TypeAnnotationNode typeAnnotation = visibleTypeAnnotations[i];
			typeAnnotation.accept(methodVisitor.visitTryCatchAnnotation(typeAnnotation.typeRef, typeAnnotation.typePath, typeAnnotation.desc, true));
		  }
		}
		if (invisibleTypeAnnotations != null)
		{
		  for (int i = 0, n = invisibleTypeAnnotations.Count; i < n; ++i)
		  {
			TypeAnnotationNode typeAnnotation = invisibleTypeAnnotations[i];
			typeAnnotation.accept(methodVisitor.visitTryCatchAnnotation(typeAnnotation.typeRef, typeAnnotation.typePath, typeAnnotation.desc, false));
		  }
		}
	  }
	}

}