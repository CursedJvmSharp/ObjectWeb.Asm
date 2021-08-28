using org.objectweb.asm;
using CursedJvmSharp.Asm.Java.IO;
using System;
using AnnotationVisitor = org.objectweb.asm.AnnotationVisitor;
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

namespace org.objectweb.asm.commons
{

	/// <summary>
	/// An <seealso cref="AnnotationVisitor"/> that remaps types with a <seealso cref="Remapper"/>.
	/// 
	/// @author Eugene Kuleshov
	/// </summary>
	public class AnnotationRemapper : AnnotationVisitor
	{

	  /// <summary>
	  /// The descriptor of the visited annotation. May be {@literal null}, for instance for
	  /// AnnotationDefault.
	  /// </summary>
	  protected internal readonly string descriptor;

	  /// <summary>
	  /// The remapper used to remap the types in the visited annotation. </summary>
	  protected internal readonly Remapper remapper;

	  /// <summary>
	  /// Constructs a new <seealso cref="AnnotationRemapper"/>. <i>Subclasses must not use this constructor</i>.
	  /// Instead, they must use the <seealso cref="AnnotationRemapper(int,AnnotationVisitor,Remapper)"/> version.
	  /// </summary>
	  /// <param name="annotationVisitor"> the annotation visitor this remapper must delegate to. </param>
	  /// <param name="remapper"> the remapper to use to remap the types in the visited annotation. </param>
	  /// @deprecated use <seealso cref="AnnotationRemapper(String, AnnotationVisitor, Remapper)"/> instead. 
	  [Obsolete("use <seealso cref=\"AnnotationRemapper(String, AnnotationVisitor, Remapper)\"/> instead.")]
	  public AnnotationRemapper(AnnotationVisitor annotationVisitor, Remapper remapper) : this(null, annotationVisitor, remapper)
	  {
	  }

	  /// <summary>
	  /// Constructs a new <seealso cref="AnnotationRemapper"/>. <i>Subclasses must not use this constructor</i>.
	  /// Instead, they must use the <seealso cref="AnnotationRemapper(int,String,AnnotationVisitor,Remapper)"/>
	  /// version.
	  /// </summary>
	  /// <param name="descriptor"> the descriptor of the visited annotation. May be {@literal null}. </param>
	  /// <param name="annotationVisitor"> the annotation visitor this remapper must delegate to. </param>
	  /// <param name="remapper"> the remapper to use to remap the types in the visited annotation. </param>
	  public AnnotationRemapper(string descriptor, AnnotationVisitor annotationVisitor, Remapper remapper) : this(Opcodes.ASM9, descriptor, annotationVisitor, remapper)
	  {
	  }

	  /// <summary>
	  /// Constructs a new <seealso cref="AnnotationRemapper"/>.
	  /// </summary>
	  /// <param name="api"> the ASM API version supported by this remapper. Must be one of the {@code
	  ///     ASM}<i>x</i> values in <seealso cref="Opcodes"/>. </param>
	  /// <param name="annotationVisitor"> the annotation visitor this remapper must delegate to. </param>
	  /// <param name="remapper"> the remapper to use to remap the types in the visited annotation. </param>
	  /// @deprecated use <seealso cref="AnnotationRemapper(int, String, AnnotationVisitor, Remapper)"/> instead. 
	  [Obsolete("use <seealso cref=\"AnnotationRemapper(int, String, AnnotationVisitor, Remapper)\"/> instead.")]
	  public AnnotationRemapper(int api, AnnotationVisitor annotationVisitor, Remapper remapper) : this(api, null, annotationVisitor, remapper)
	  {
	  }

	  /// <summary>
	  /// Constructs a new <seealso cref="AnnotationRemapper"/>.
	  /// </summary>
	  /// <param name="api"> the ASM API version supported by this remapper. Must be one of the {@code
	  ///     ASM}<i>x</i> values in <seealso cref="Opcodes"/>. </param>
	  /// <param name="descriptor"> the descriptor of the visited annotation. May be {@literal null}. </param>
	  /// <param name="annotationVisitor"> the annotation visitor this remapper must delegate to. </param>
	  /// <param name="remapper"> the remapper to use to remap the types in the visited annotation. </param>
	  public AnnotationRemapper(int api, string descriptor, AnnotationVisitor annotationVisitor, Remapper remapper) : base(api, annotationVisitor)
	  {
		this.descriptor = descriptor;
		this.remapper = remapper;
	  }

	  public override void visit(string name, object value)
	  {
		base.visit(mapAnnotationAttributeName(name), remapper.mapValue(value));
	  }

	  public override void visitEnum(string name, string descriptor, string value)
	  {
		base.visitEnum(mapAnnotationAttributeName(name), remapper.mapDesc(descriptor), value);
	  }

	  public override AnnotationVisitor visitAnnotation(string name, string descriptor)
	  {
		AnnotationVisitor annotationVisitor = base.visitAnnotation(mapAnnotationAttributeName(name), remapper.mapDesc(descriptor));
		if (annotationVisitor == null)
		{
		  return null;
		}
		else
		{
		  return annotationVisitor == av ? this : createAnnotationRemapper(descriptor, annotationVisitor);
		}
	  }

	  public override AnnotationVisitor visitArray(string name)
	  {
		AnnotationVisitor annotationVisitor = base.visitArray(mapAnnotationAttributeName(name));
		if (annotationVisitor == null)
		{
		  return null;
		}
		else
		{
		  return annotationVisitor == av ? this : createAnnotationRemapper(null, annotationVisitor);
		}
	  }

	  /// <summary>
	  /// Constructs a new remapper for annotations. The default implementation of this method returns a
	  /// new <seealso cref="AnnotationRemapper"/>.
	  /// </summary>
	  /// <param name="annotationVisitor"> the AnnotationVisitor the remapper must delegate to. </param>
	  /// <returns> the newly created remapper. </returns>
	  /// @deprecated use <seealso cref="createAnnotationRemapper(String, AnnotationVisitor)"/> instead. 
	  [Obsolete("use <seealso cref=\"createAnnotationRemapper(String, AnnotationVisitor)\"/> instead.")]
	  public virtual AnnotationVisitor createAnnotationRemapper(AnnotationVisitor annotationVisitor)
	  {
		return new AnnotationRemapper(api, null, annotationVisitor, remapper);
	  }

	  /// <summary>
	  /// Constructs a new remapper for annotations. The default implementation of this method returns a
	  /// new <seealso cref="AnnotationRemapper"/>.
	  /// </summary>
	  /// <param name="descriptor"> the descriptor of the visited annotation. </param>
	  /// <param name="annotationVisitor"> the AnnotationVisitor the remapper must delegate to. </param>
	  /// <returns> the newly created remapper. </returns>
	  public virtual AnnotationVisitor createAnnotationRemapper(string descriptor, AnnotationVisitor annotationVisitor)
	  {
		return (new AnnotationRemapper(api, descriptor, annotationVisitor, remapper)).orDeprecatedValue(createAnnotationRemapper(annotationVisitor));
	  }

	  /// <summary>
	  /// Returns either this object, or the given one. If the given object is equal to the object
	  /// returned by the default implementation of the deprecated createAnnotationRemapper method,
	  /// meaning that this method has not been overridden (or only in minor ways, for instance to add
	  /// logging), then we can return this object instead, supposed to have been created by the new
	  /// createAnnotationRemapper method. Otherwise we must return the given object.
	  /// </summary>
	  /// <param name="deprecatedAnnotationVisitor"> the result of a call to the deprecated
	  ///     createAnnotationRemapper method. </param>
	  /// <returns> either this object, or the given one. </returns>
	  public AnnotationVisitor orDeprecatedValue(AnnotationVisitor deprecatedAnnotationVisitor)
	  {
		if (deprecatedAnnotationVisitor.GetType() == this.GetType())
		{
		  AnnotationRemapper deprecatedAnnotationRemapper = (AnnotationRemapper) deprecatedAnnotationVisitor;
		  if (deprecatedAnnotationRemapper.api == api && deprecatedAnnotationRemapper.av == av && deprecatedAnnotationRemapper.remapper == remapper)
		  {
			return this;
		  }
		}
		return deprecatedAnnotationVisitor;
	  }

	  /// <summary>
	  /// Maps an annotation attribute name with the remapper. Returns the original name unchanged if the
	  /// internal name of the annotation is {@literal null}.
	  /// </summary>
	  /// <param name="name"> the name of the annotation attribute. </param>
	  /// <returns> the new name of the annotation attribute. </returns>
	  private string mapAnnotationAttributeName(string name)
	  {
		if (string.ReferenceEquals(descriptor, null))
		{
		  return name;
		}
		return remapper.mapAnnotationAttributeName(descriptor, name);
	  }
	}

}