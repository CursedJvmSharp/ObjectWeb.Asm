using org.objectweb.asm;
using CursedJvmSharp.Asm.Java.IO;
using System;
using System.Collections.Generic;
using AnnotationVisitor = org.objectweb.asm.AnnotationVisitor;
using Attribute = org.objectweb.asm.Attribute;
using ClassVisitor = org.objectweb.asm.ClassVisitor;
using FieldVisitor = org.objectweb.asm.FieldVisitor;
using MethodVisitor = org.objectweb.asm.MethodVisitor;
using ModuleVisitor = org.objectweb.asm.ModuleVisitor;
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

namespace org.objectweb.asm.commons
{

	/// <summary>
	/// A <seealso cref="ClassVisitor"/> that remaps types with a <seealso cref="Remapper"/>.
	/// 
	/// <para><i>This visitor has several limitations</i>. A non-exhaustive list is the following:
	/// 
	/// <ul>
	///   <li>it cannot remap type names in dynamically computed strings (remapping of type names in
	///       static values is supported).
	///   <li>it cannot remap values derived from type names at compile time, such as
	///       <ul>
	///         <li>type name hashcodes used by some Java compilers to implement the string switch
	///             statement.
	///         <li>some compound strings used by some Java compilers to implement lambda
	///             deserialization.
	///       </ul>
	/// </ul>
	/// 
	/// @author Eugene Kuleshov
	/// </para>
	/// </summary>
	public class ClassRemapper : ClassVisitor
	{

	  /// <summary>
	  /// The remapper used to remap the types in the visited class. </summary>
	  protected internal readonly Remapper remapper;

	  /// <summary>
	  /// The internal name of the visited class. </summary>
	  protected internal string className;

	  /// <summary>
	  /// Constructs a new <seealso cref="ClassRemapper"/>. <i>Subclasses must not use this constructor</i>.
	  /// Instead, they must use the <seealso cref="ClassRemapper(int,ClassVisitor,Remapper)"/> version.
	  /// </summary>
	  /// <param name="classVisitor"> the class visitor this remapper must delegate to. </param>
	  /// <param name="remapper"> the remapper to use to remap the types in the visited class. </param>
	  public ClassRemapper(ClassVisitor classVisitor, Remapper remapper) : this(Opcodes.ASM9, classVisitor, remapper)
	  {
	  }

	  /// <summary>
	  /// Constructs a new <seealso cref="ClassRemapper"/>.
	  /// </summary>
	  /// <param name="api"> the ASM API version supported by this remapper. Must be one of the {@code
	  ///     ASM}<i>x</i> values in <seealso cref="Opcodes"/>. </param>
	  /// <param name="classVisitor"> the class visitor this remapper must delegate to. </param>
	  /// <param name="remapper"> the remapper to use to remap the types in the visited class. </param>
	  public ClassRemapper(int api, ClassVisitor classVisitor, Remapper remapper) : base(api, classVisitor)
	  {
		this.remapper = remapper;
	  }

	  public override void visit(int version, int access, string name, string signature, string superName, string[] interfaces)
	  {
		this.className = name;
		base.visit(version, access, remapper.mapType(name), remapper.mapSignature(signature, false), remapper.mapType(superName), interfaces == null ? null : remapper.mapTypes(interfaces));
	  }

	  public override ModuleVisitor visitModule(string name, int flags, string version)
	  {
		ModuleVisitor moduleVisitor = base.visitModule(remapper.mapModuleName(name), flags, version);
		return moduleVisitor == null ? null : createModuleRemapper(moduleVisitor);
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

	  public override void visitAttribute(Attribute attribute)
	  {
		if (attribute is ModuleHashesAttribute)
		{
		  ModuleHashesAttribute moduleHashesAttribute = (ModuleHashesAttribute) attribute;
		  List<string> modules = moduleHashesAttribute.modules;
		  for (int i = 0; i < modules.Count; ++i)
		  {
			modules[i] = remapper.mapModuleName(modules[i]);
		  }
		}
		base.visitAttribute(attribute);
	  }

	  public override RecordComponentVisitor visitRecordComponent(string name, string descriptor, string signature)
	  {
		RecordComponentVisitor recordComponentVisitor = base.visitRecordComponent(remapper.mapRecordComponentName(className, name, descriptor), remapper.mapDesc(descriptor), remapper.mapSignature(signature, true));
		return recordComponentVisitor == null ? null : createRecordComponentRemapper(recordComponentVisitor);
	  }

	  public override FieldVisitor visitField(int access, string name, string descriptor, string signature, object value)
	  {
		FieldVisitor fieldVisitor = base.visitField(access, remapper.mapFieldName(className, name, descriptor), remapper.mapDesc(descriptor), remapper.mapSignature(signature, true), (value == null) ? null : remapper.mapValue(value));
		return fieldVisitor == null ? null : createFieldRemapper(fieldVisitor);
	  }

	  public override MethodVisitor visitMethod(int access, string name, string descriptor, string signature, string[] exceptions)
	  {
		string remappedDescriptor = remapper.mapMethodDesc(descriptor);
		MethodVisitor methodVisitor = base.visitMethod(access, remapper.mapMethodName(className, name, descriptor), remappedDescriptor, remapper.mapSignature(signature, false), exceptions == null ? null : remapper.mapTypes(exceptions));
		return methodVisitor == null ? null : createMethodRemapper(methodVisitor);
	  }

	  public override void visitInnerClass(string name, string outerName, string innerName, int access)
	  {
		base.visitInnerClass(remapper.mapType(name), string.ReferenceEquals(outerName, null) ? null : remapper.mapType(outerName), string.ReferenceEquals(innerName, null) ? null : remapper.mapInnerClassName(name, outerName, innerName), access);
	  }

	  public override void visitOuterClass(string owner, string name, string descriptor)
	  {
		base.visitOuterClass(remapper.mapType(owner), string.ReferenceEquals(name, null) ? null : remapper.mapMethodName(owner, name, descriptor), string.ReferenceEquals(descriptor, null) ? null : remapper.mapMethodDesc(descriptor));
	  }

	  public override void visitNestHost(string nestHost)
	  {
		base.visitNestHost(remapper.mapType(nestHost));
	  }

	  public override void visitNestMember(string nestMember)
	  {
		base.visitNestMember(remapper.mapType(nestMember));
	  }

	  public override void visitPermittedSubclass(string permittedSubclass)
	  {
		base.visitPermittedSubclass(remapper.mapType(permittedSubclass));
	  }

	  /// <summary>
	  /// Constructs a new remapper for fields. The default implementation of this method returns a new
	  /// <seealso cref="FieldRemapper"/>.
	  /// </summary>
	  /// <param name="fieldVisitor"> the FieldVisitor the remapper must delegate to. </param>
	  /// <returns> the newly created remapper. </returns>
	  public virtual FieldVisitor createFieldRemapper(FieldVisitor fieldVisitor)
	  {
		return new FieldRemapper(api, fieldVisitor, remapper);
	  }

	  /// <summary>
	  /// Constructs a new remapper for methods. The default implementation of this method returns a new
	  /// <seealso cref="MethodRemapper"/>.
	  /// </summary>
	  /// <param name="methodVisitor"> the MethodVisitor the remapper must delegate to. </param>
	  /// <returns> the newly created remapper. </returns>
	  public virtual MethodVisitor createMethodRemapper(MethodVisitor methodVisitor)
	  {
		return new MethodRemapper(api, methodVisitor, remapper);
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
	  /// Constructs a new remapper for modules. The default implementation of this method returns a new
	  /// <seealso cref="ModuleRemapper"/>.
	  /// </summary>
	  /// <param name="moduleVisitor"> the ModuleVisitor the remapper must delegate to. </param>
	  /// <returns> the newly created remapper. </returns>
	  public virtual ModuleVisitor createModuleRemapper(ModuleVisitor moduleVisitor)
	  {
		return new ModuleRemapper(api, moduleVisitor, remapper);
	  }

	  /// <summary>
	  /// Constructs a new remapper for record components. The default implementation of this method
	  /// returns a new <seealso cref="RecordComponentRemapper"/>.
	  /// </summary>
	  /// <param name="recordComponentVisitor"> the RecordComponentVisitor the remapper must delegate to. </param>
	  /// <returns> the newly created remapper. </returns>
	  public virtual RecordComponentVisitor createRecordComponentRemapper(RecordComponentVisitor recordComponentVisitor)
	  {
		return new RecordComponentRemapper(api, recordComponentVisitor, remapper);
	  }
	}

}