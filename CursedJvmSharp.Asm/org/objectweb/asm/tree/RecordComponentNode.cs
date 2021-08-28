using org.objectweb.asm;
using CursedJvmSharp.Asm.Java.IO;
using System.Collections.Generic;
using AnnotationVisitor = org.objectweb.asm.AnnotationVisitor;
using Attribute = org.objectweb.asm.Attribute;
using ClassVisitor = org.objectweb.asm.ClassVisitor;
using Opcodes = org.objectweb.asm.Opcodes;
using RecordComponentVisitor = org.objectweb.asm.RecordComponentVisitor;
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
	/// A node that represents a record component.
	/// 
	/// @author Remi Forax
	/// </summary>
	public class RecordComponentNode : RecordComponentVisitor
	{

	  /// <summary>
	  /// The record component name. </summary>
	  public string name;

	  /// <summary>
	  /// The record component descriptor (see <seealso cref="org.objectweb.asm.Type"/>). </summary>
	  public string descriptor;

	  /// <summary>
	  /// The record component signature. May be {@literal null}. </summary>
	  public string signature;

	  /// <summary>
	  /// The runtime visible annotations of this record component. May be {@literal null}. </summary>
	  public IList<AnnotationNode> visibleAnnotations;

	  /// <summary>
	  /// The runtime invisible annotations of this record component. May be {@literal null}. </summary>
	  public IList<AnnotationNode> invisibleAnnotations;

	  /// <summary>
	  /// The runtime visible type annotations of this record component. May be {@literal null}. </summary>
	  public IList<TypeAnnotationNode> visibleTypeAnnotations;

	  /// <summary>
	  /// The runtime invisible type annotations of this record component. May be {@literal null}. </summary>
	  public IList<TypeAnnotationNode> invisibleTypeAnnotations;

	  /// <summary>
	  /// The non standard attributes of this record component. * May be {@literal null}. </summary>
	  public IList<Attribute> attrs;

	  /// <summary>
	  /// Constructs a new <seealso cref="RecordComponentNode"/>. <i>Subclasses must not use this constructor</i>.
	  /// Instead, they must use the <seealso cref="RecordComponentNode(int, String, String, String)"/> version.
	  /// </summary>
	  /// <param name="name"> the record component name. </param>
	  /// <param name="descriptor"> the record component descriptor (see <seealso cref="org.objectweb.asm.Type"/>). </param>
	  /// <param name="signature"> the record component signature. </param>
	  /// <exception cref="IllegalStateException"> If a subclass calls this constructor. </exception>
	  public RecordComponentNode(string name, string descriptor, string signature) : this(Opcodes.ASM9, name, descriptor, signature)
	  {
		if (this.GetType() != typeof(RecordComponentNode))
		{
		  throw new System.InvalidOperationException();
		}
	  }

	  /// <summary>
	  /// Constructs a new <seealso cref="RecordComponentNode"/>.
	  /// </summary>
	  /// <param name="api"> the ASM API version implemented by this visitor. Must be one of <seealso cref="Opcodes.ASM8"/>
	  ///     or <seealso cref="Opcodes.ASM9"/>. </param>
	  /// <param name="name"> the record component name. </param>
	  /// <param name="descriptor"> the record component descriptor (see <seealso cref="org.objectweb.asm.Type"/>). </param>
	  /// <param name="signature"> the record component signature. </param>
	  public RecordComponentNode(int api, string name, string descriptor, string signature) : base(api)
	  {
		this.name = name;
		this.descriptor = descriptor;
		this.signature = signature;
	  }

	  // -----------------------------------------------------------------------------------------------
	  // Implementation of the FieldVisitor abstract class
	  // -----------------------------------------------------------------------------------------------

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

	  public override void visitAttribute(Attribute attribute)
	  {
		attrs = Util.add(attrs, attribute);
	  }

	  public override void visitEnd()
	  {
		// Nothing to do.
	  }

	  // -----------------------------------------------------------------------------------------------
	  // Accept methods
	  // -----------------------------------------------------------------------------------------------

	  /// <summary>
	  /// Checks that this record component node is compatible with the given ASM API version. This
	  /// method checks that this node, and all its children recursively, do not contain elements that
	  /// were introduced in more recent versions of the ASM API than the given version.
	  /// </summary>
	  /// <param name="api"> an ASM API version. Must be one of <seealso cref="Opcodes.ASM8"/> or <seealso cref="Opcodes.ASM9"/>. </param>
	  public virtual void check(int api)
	  {
		if (api < Opcodes.ASM8)
		{
		  throw new UnsupportedClassVersionException();
		}
	  }

	  /// <summary>
	  /// Makes the given class visitor visit this record component.
	  /// </summary>
	  /// <param name="classVisitor"> a class visitor. </param>
	  public virtual void accept(ClassVisitor classVisitor)
	  {
		RecordComponentVisitor recordComponentVisitor = classVisitor.visitRecordComponent(name, descriptor, signature);
		if (recordComponentVisitor == null)
		{
		  return;
		}
		// Visit the annotations.
		if (visibleAnnotations != null)
		{
		  for (int i = 0, n = visibleAnnotations.Count; i < n; ++i)
		  {
			AnnotationNode annotation = visibleAnnotations[i];
			annotation.accept(recordComponentVisitor.visitAnnotation(annotation.desc, true));
		  }
		}
		if (invisibleAnnotations != null)
		{
		  for (int i = 0, n = invisibleAnnotations.Count; i < n; ++i)
		  {
			AnnotationNode annotation = invisibleAnnotations[i];
			annotation.accept(recordComponentVisitor.visitAnnotation(annotation.desc, false));
		  }
		}
		if (visibleTypeAnnotations != null)
		{
		  for (int i = 0, n = visibleTypeAnnotations.Count; i < n; ++i)
		  {
			TypeAnnotationNode typeAnnotation = visibleTypeAnnotations[i];
			typeAnnotation.accept(recordComponentVisitor.visitTypeAnnotation(typeAnnotation.typeRef, typeAnnotation.typePath, typeAnnotation.desc, true));
		  }
		}
		if (invisibleTypeAnnotations != null)
		{
		  for (int i = 0, n = invisibleTypeAnnotations.Count; i < n; ++i)
		  {
			TypeAnnotationNode typeAnnotation = invisibleTypeAnnotations[i];
			typeAnnotation.accept(recordComponentVisitor.visitTypeAnnotation(typeAnnotation.typeRef, typeAnnotation.typePath, typeAnnotation.desc, false));
		  }
		}
		// Visit the non standard attributes.
		if (attrs != null)
		{
		  for (int i = 0, n = attrs.Count; i < n; ++i)
		  {
			recordComponentVisitor.visitAttribute(attrs[i]);
		  }
		}
		recordComponentVisitor.visitEnd();
	  }
	}

}