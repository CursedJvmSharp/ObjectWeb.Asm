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
	/// A node that represents a class.
	/// 
	/// @author Eric Bruneton
	/// </summary>
	public class ClassNode : ClassVisitor
	{

	  /// <summary>
	  /// The class version. The minor version is stored in the 16 most significant bits, and the major
	  /// version in the 16 least significant bits.
	  /// </summary>
	  public int version;

	  /// <summary>
	  /// The class's access flags (see <seealso cref="Opcodes"/>). This field also indicates if
	  /// the class is deprecated <seealso cref="Opcodes.ACC_DEPRECATED"/> or a record <seealso cref="Opcodes.ACC_RECORD"/>.
	  /// </summary>
	  public int access;

	  /// <summary>
	  /// The internal name of this class (see <seealso cref="org.objectweb.asm.Type.getInternalName"/>). </summary>
	  public string name;

	  /// <summary>
	  /// The signature of this class. May be {@literal null}. </summary>
	  public string signature;

	  /// <summary>
	  /// The internal of name of the super class (see <seealso cref="org.objectweb.asm.Type.getInternalName"/>).
	  /// For interfaces, the super class is <seealso cref="object"/>. May be {@literal null}, but only for the
	  /// <seealso cref="object"/> class.
	  /// </summary>
	  public string superName;

	  /// <summary>
	  /// The internal names of the interfaces directly implemented by this class (see {@link
	  /// org.objectweb.asm.Type#getInternalName}).
	  /// </summary>
	  public List<string> interfaces;

	  /// <summary>
	  /// The name of the source file from which this class was compiled. May be {@literal null}. </summary>
	  public string sourceFile;

	  /// <summary>
	  /// The correspondence between source and compiled elements of this class. May be {@literal null}.
	  /// </summary>
	  public string sourceDebug;

	  /// <summary>
	  /// The module stored in this class. May be {@literal null}. </summary>
	  public ModuleNode module;

	  /// <summary>
	  /// The internal name of the enclosing class of this class. May be {@literal null}. </summary>
	  public string outerClass;

	  /// <summary>
	  /// The name of the method that contains this class, or {@literal null} if this class is not
	  /// enclosed in a method.
	  /// </summary>
	  public string outerMethod;

	  /// <summary>
	  /// The descriptor of the method that contains this class, or {@literal null} if this class is not
	  /// enclosed in a method.
	  /// </summary>
	  public string outerMethodDesc;

	  /// <summary>
	  /// The runtime visible annotations of this class. May be {@literal null}. </summary>
	  public List<AnnotationNode> visibleAnnotations;

	  /// <summary>
	  /// The runtime invisible annotations of this class. May be {@literal null}. </summary>
	  public List<AnnotationNode> invisibleAnnotations;

	  /// <summary>
	  /// The runtime visible type annotations of this class. May be {@literal null}. </summary>
	  public List<TypeAnnotationNode> visibleTypeAnnotations;

	  /// <summary>
	  /// The runtime invisible type annotations of this class. May be {@literal null}. </summary>
	  public List<TypeAnnotationNode> invisibleTypeAnnotations;

	  /// <summary>
	  /// The non standard attributes of this class. May be {@literal null}. </summary>
	  public List<Attribute> attrs;

	  /// <summary>
	  /// The inner classes of this class. </summary>
	  public List<InnerClassNode> innerClasses;

	  /// <summary>
	  /// The internal name of the nest host class of this class. May be {@literal null}. </summary>
	  public string nestHostClass;

	  /// <summary>
	  /// The internal names of the nest members of this class. May be {@literal null}. </summary>
	  public List<string> nestMembers;

	  /// <summary>
	  /// The internal names of the permitted subclasses of this class. May be {@literal null}. </summary>
	  public List<string> permittedSubclasses;

	  /// <summary>
	  /// The record components of this class. May be {@literal null}. </summary>
	  public List<RecordComponentNode> recordComponents;

	  /// <summary>
	  /// The fields of this class. </summary>
	  public List<FieldNode> fields;

	  /// <summary>
	  /// The methods of this class. </summary>
	  public List<MethodNode> methods;

	  /// <summary>
	  /// Constructs a new <seealso cref="ClassNode"/>. <i>Subclasses must not use this constructor</i>. Instead,
	  /// they must use the <seealso cref="ClassNode(int)"/> version.
	  /// </summary>
	  /// <exception cref="IllegalStateException"> If a subclass calls this constructor. </exception>
	  public ClassNode() : this(Opcodes.ASM9)
	  {
		if (this.GetType() != typeof(ClassNode))
		{
		  throw new System.InvalidOperationException();
		}
	  }

	  /// <summary>
	  /// Constructs a new <seealso cref="ClassNode"/>.
	  /// </summary>
	  /// <param name="api"> the ASM API version implemented by this visitor. Must be one of the {@code
	  ///     ASM}<i>x</i> values in <seealso cref="Opcodes"/>. </param>
	  public ClassNode(int api) : base(api)
	  {
		this.interfaces = new List<string>();
		this.innerClasses = new List<InnerClassNode>();
		this.fields = new List<FieldNode>();
		this.methods = new List<MethodNode>();
	  }

	  // -----------------------------------------------------------------------------------------------
	  // Implementation of the ClassVisitor abstract class
	  // -----------------------------------------------------------------------------------------------

	  public override void visit(int version, int access, string name, string signature, string superName, string[] interfaces)
	  {
		this.version = version;
		this.access = access;
		this.name = name;
		this.signature = signature;
		this.superName = superName;
		this.interfaces = Util.asArrayList(interfaces);
	  }

	  public override void visitSource(string file, string debug)
	  {
		sourceFile = file;
		sourceDebug = debug;
	  }

	  public override ModuleVisitor visitModule(string name, int access, string version)
	  {
		module = new ModuleNode(name, access, version);
		return module;
	  }

	  public override void visitNestHost(string nestHost)
	  {
		this.nestHostClass = nestHost;
	  }

	  public override void visitOuterClass(string owner, string name, string descriptor)
	  {
		outerClass = owner;
		outerMethod = name;
		outerMethodDesc = descriptor;
	  }

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

	  public override void visitNestMember(string nestMember)
	  {
		nestMembers = Util.add(nestMembers, nestMember);
	  }

	  public override void visitPermittedSubclass(string permittedSubclass)
	  {
		permittedSubclasses = Util.add(permittedSubclasses, permittedSubclass);
	  }

	  public override void visitInnerClass(string name, string outerName, string innerName, int access)
	  {
		InnerClassNode innerClass = new InnerClassNode(name, outerName, innerName, access);
		innerClasses.Add(innerClass);
	  }

	  public override RecordComponentVisitor visitRecordComponent(string name, string descriptor, string signature)
	  {
		RecordComponentNode recordComponent = new RecordComponentNode(name, descriptor, signature);
		recordComponents = Util.add(recordComponents, recordComponent);
		return recordComponent;
	  }

	  public override FieldVisitor visitField(int access, string name, string descriptor, string signature, object value)
	  {
		FieldNode field = new FieldNode(access, name, descriptor, signature, value);
		fields.Add(field);
		return field;
	  }

	  public override MethodVisitor visitMethod(int access, string name, string descriptor, string signature, string[] exceptions)
	  {
		MethodNode method = new MethodNode(access, name, descriptor, signature, exceptions);
		methods.Add(method);
		return method;
	  }

	  public override void visitEnd()
	  {
		// Nothing to do.
	  }

	  // -----------------------------------------------------------------------------------------------
	  // Accept method
	  // -----------------------------------------------------------------------------------------------

	  /// <summary>
	  /// Checks that this class node is compatible with the given ASM API version. This method checks
	  /// that this node, and all its children recursively, do not contain elements that were introduced
	  /// in more recent versions of the ASM API than the given version.
	  /// </summary>
	  /// <param name="api"> an ASM API version. Must be one of the {@code ASM}<i>x</i> values in {@link
	  ///     Opcodes}. </param>
	  public virtual void check(int api)
	  {
		if (api < Opcodes.ASM9 && permittedSubclasses != null)
		{
		  throw new UnsupportedClassVersionException();
		}
		if (api < Opcodes.ASM8 && ((access & Opcodes.ACC_RECORD) != 0 || recordComponents != null))
		{
		  throw new UnsupportedClassVersionException();
		}
		if (api < Opcodes.ASM7 && (!string.ReferenceEquals(nestHostClass, null) || nestMembers != null))
		{
		  throw new UnsupportedClassVersionException();
		}
		if (api < Opcodes.ASM6 && module != null)
		{
		  throw new UnsupportedClassVersionException();
		}
		if (api < Opcodes.ASM5)
		{
		  if (visibleTypeAnnotations != null && visibleTypeAnnotations.Count > 0)
		  {
			throw new UnsupportedClassVersionException();
		  }
		  if (invisibleTypeAnnotations != null && invisibleTypeAnnotations.Count > 0)
		  {
			throw new UnsupportedClassVersionException();
		  }
		}
		// Check the annotations.
		if (visibleAnnotations != null)
		{
		  for (int i = visibleAnnotations.Count - 1; i >= 0; --i)
		  {
			visibleAnnotations[i].check(api);
		  }
		}
		if (invisibleAnnotations != null)
		{
		  for (int i = invisibleAnnotations.Count - 1; i >= 0; --i)
		  {
			invisibleAnnotations[i].check(api);
		  }
		}
		if (visibleTypeAnnotations != null)
		{
		  for (int i = visibleTypeAnnotations.Count - 1; i >= 0; --i)
		  {
			visibleTypeAnnotations[i].check(api);
		  }
		}
		if (invisibleTypeAnnotations != null)
		{
		  for (int i = invisibleTypeAnnotations.Count - 1; i >= 0; --i)
		  {
			invisibleTypeAnnotations[i].check(api);
		  }
		}
		if (recordComponents != null)
		{
		  for (int i = recordComponents.Count - 1; i >= 0; --i)
		  {
			recordComponents[i].check(api);
		  }
		}
		for (int i = fields.Count - 1; i >= 0; --i)
		{
		  fields[i].check(api);
		}
		for (int i = methods.Count - 1; i >= 0; --i)
		{
		  methods[i].check(api);
		}
	  }

	  /// <summary>
	  /// Makes the given class visitor visit this class.
	  /// </summary>
	  /// <param name="classVisitor"> a class visitor. </param>
	  public virtual void accept(ClassVisitor classVisitor)
	  {
		// Visit the header.
		string[] interfacesArray = this.interfaces.ToArray();
		classVisitor.visit(version, access, name, signature, superName, interfacesArray);
		// Visit the source.
		if (!string.ReferenceEquals(sourceFile, null) || !string.ReferenceEquals(sourceDebug, null))
		{
		  classVisitor.visitSource(sourceFile, sourceDebug);
		}
		// Visit the module.
		if (module != null)
		{
		  module.accept(classVisitor);
		}
		// Visit the nest host class.
		if (!string.ReferenceEquals(nestHostClass, null))
		{
		  classVisitor.visitNestHost(nestHostClass);
		}
		// Visit the outer class.
		if (!string.ReferenceEquals(outerClass, null))
		{
		  classVisitor.visitOuterClass(outerClass, outerMethod, outerMethodDesc);
		}
		// Visit the annotations.
		if (visibleAnnotations != null)
		{
		  for (int i = 0, n = visibleAnnotations.Count; i < n; ++i)
		  {
			AnnotationNode annotation = visibleAnnotations[i];
			annotation.accept(classVisitor.visitAnnotation(annotation.desc, true));
		  }
		}
		if (invisibleAnnotations != null)
		{
		  for (int i = 0, n = invisibleAnnotations.Count; i < n; ++i)
		  {
			AnnotationNode annotation = invisibleAnnotations[i];
			annotation.accept(classVisitor.visitAnnotation(annotation.desc, false));
		  }
		}
		if (visibleTypeAnnotations != null)
		{
		  for (int i = 0, n = visibleTypeAnnotations.Count; i < n; ++i)
		  {
			TypeAnnotationNode typeAnnotation = visibleTypeAnnotations[i];
			typeAnnotation.accept(classVisitor.visitTypeAnnotation(typeAnnotation.typeRef, typeAnnotation.typePath, typeAnnotation.desc, true));
		  }
		}
		if (invisibleTypeAnnotations != null)
		{
		  for (int i = 0, n = invisibleTypeAnnotations.Count; i < n; ++i)
		  {
			TypeAnnotationNode typeAnnotation = invisibleTypeAnnotations[i];
			typeAnnotation.accept(classVisitor.visitTypeAnnotation(typeAnnotation.typeRef, typeAnnotation.typePath, typeAnnotation.desc, false));
		  }
		}
		// Visit the non standard attributes.
		if (attrs != null)
		{
		  for (int i = 0, n = attrs.Count; i < n; ++i)
		  {
			classVisitor.visitAttribute(attrs[i]);
		  }
		}
		// Visit the nest members.
		if (nestMembers != null)
		{
		  for (int i = 0, n = nestMembers.Count; i < n; ++i)
		  {
			classVisitor.visitNestMember(nestMembers[i]);
		  }
		}
		// Visit the permitted subclasses.
		if (permittedSubclasses != null)
		{
		  for (int i = 0, n = permittedSubclasses.Count; i < n; ++i)
		  {
			classVisitor.visitPermittedSubclass(permittedSubclasses[i]);
		  }
		}
		// Visit the inner classes.
		for (int i = 0, n = innerClasses.Count; i < n; ++i)
		{
		  innerClasses[i].accept(classVisitor);
		}
		// Visit the record components.
		if (recordComponents != null)
		{
		  for (int i = 0, n = recordComponents.Count; i < n; ++i)
		  {
			recordComponents[i].accept(classVisitor);
		  }
		}
		// Visit the fields.
		for (int i = 0, n = fields.Count; i < n; ++i)
		{
		  fields[i].accept(classVisitor);
		}
		// Visit the methods.
		for (int i = 0, n = methods.Count; i < n; ++i)
		{
		  methods[i].accept(classVisitor);
		}
		classVisitor.visitEnd();
	  }
	}

}