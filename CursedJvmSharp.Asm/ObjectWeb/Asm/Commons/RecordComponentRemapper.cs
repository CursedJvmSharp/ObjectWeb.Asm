using System;

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

namespace ObjectWeb.Asm.Commons
{

	/// <summary>
	/// A <seealso cref="RecordComponentVisitor"/> that remaps types with a <seealso cref="Remapper"/>.
	/// 
	/// @author Remi Forax
	/// </summary>
	public class RecordComponentRemapper : RecordComponentVisitor
	{

	  /// <summary>
	  /// The remapper used to remap the types in the visited field. </summary>
	  protected internal readonly Remapper remapper;

	  /// <summary>
	  /// Constructs a new <seealso cref="RecordComponentRemapper"/>. <i>Subclasses must not use this
	  /// constructor</i>. Instead, they must use the {@link
	  /// #RecordComponentRemapper(int,RecordComponentVisitor,Remapper)} version.
	  /// </summary>
	  /// <param name="recordComponentVisitor"> the record component visitor this remapper must delegate to. </param>
	  /// <param name="remapper"> the remapper to use to remap the types in the visited record component. </param>
	  public RecordComponentRemapper(RecordComponentVisitor recordComponentVisitor, Remapper remapper) : this(Opcodes.ASM9, recordComponentVisitor, remapper)
	  {
	  }

	  /// <summary>
	  /// Constructs a new <seealso cref="RecordComponentRemapper"/>.
	  /// </summary>
	  /// <param name="api"> the ASM API version supported by this remapper. Must be one of {@link
	  ///     org.objectweb.asm.Opcodes#ASM8} or <seealso cref="Opcodes.ASM9"/>. </param>
	  /// <param name="recordComponentVisitor"> the record component visitor this remapper must delegate to. </param>
	  /// <param name="remapper"> the remapper to use to remap the types in the visited record component. </param>
	  public RecordComponentRemapper(int api, RecordComponentVisitor recordComponentVisitor, Remapper remapper) : base(api, recordComponentVisitor)
	  {
		this.remapper = remapper;
	  }

	  public override AnnotationVisitor visitAnnotation(string descriptor, bool visible)
	  {
		AnnotationVisitor annotationVisitor = base.visitAnnotation(remapper.mapDesc(descriptor), visible);
		return annotationVisitor == null ? null : createAnnotationRemapper(descriptor, annotationVisitor);
	  }

	  public override AnnotationVisitor visitTypeAnnotation(int typeRef, TypePath typePath, string descriptor, bool visible)
	  {
		AnnotationVisitor annotationVisitor = base.visitTypeAnnotation(typeRef, typePath, remapper.mapDesc(descriptor), visible);
		return annotationVisitor == null ? null : createAnnotationRemapper(descriptor, annotationVisitor);
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
	  /// <param name="descriptor"> the descriptor sof the visited annotation. </param>
	  /// <param name="annotationVisitor"> the AnnotationVisitor the remapper must delegate to. </param>
	  /// <returns> the newly created remapper. </returns>
	  public virtual AnnotationVisitor createAnnotationRemapper(string descriptor, AnnotationVisitor annotationVisitor)
	  {
		return (new AnnotationRemapper(api, descriptor, annotationVisitor, remapper)).orDeprecatedValue(createAnnotationRemapper(annotationVisitor));
	  }
	}

}