using org.objectweb.asm;
using CursedJvmSharp.Asm.Java.IO;
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
namespace org.objectweb.asm
{
	/// <summary>
	/// A <seealso cref="ClassVisitor"/> that generates a corresponding ClassFile structure, as defined in the Java
	/// Virtual Machine Specification (JVMS). It can be used alone, to generate a Java class "from
	/// scratch", or with one or more {@link ClassReader} and adapter {@link ClassVisitor} to generate a
	/// modified class from one or more existing Java classes.
	/// </summary>
	/// <seealso cref= <a href="https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html">JVMS 4</a>
	/// @author Eric Bruneton </seealso>
	public class ClassWriter : ClassVisitor
	{

	  /// <summary>
	  /// A flag to automatically compute the maximum stack size and the maximum number of local
	  /// variables of methods. If this flag is set, then the arguments of the {@link
	  /// MethodVisitor#visitMaxs} method of the <seealso cref="MethodVisitor"/> returned by the {@link
	  /// #visitMethod} method will be ignored, and computed automatically from the signature and the
	  /// bytecode of each method.
	  /// 
	  /// <para><b>Note:</b> for classes whose version is <seealso cref="Opcodes.V1_7"/> of more, this option requires
	  /// valid stack map frames. The maximum stack size is then computed from these frames, and from the
	  /// bytecode instructions in between. If stack map frames are not present or must be recomputed,
	  /// used <seealso cref="COMPUTE_FRAMES"/> instead.
	  /// 
	  /// </para>
	  /// </summary>
	  /// <seealso cref= #ClassWriter(int) </seealso>
	  public const int COMPUTE_MAXS = 1;

	  /// <summary>
	  /// A flag to automatically compute the stack map frames of methods from scratch. If this flag is
	  /// set, then the calls to the <seealso cref="MethodVisitor.visitFrame"/> method are ignored, and the stack
	  /// map frames are recomputed from the methods bytecode. The arguments of the {@link
	  /// MethodVisitor#visitMaxs} method are also ignored and recomputed from the bytecode. In other
	  /// words, <seealso cref="COMPUTE_FRAMES"/> implies <seealso cref="COMPUTE_MAXS"/>.
	  /// </summary>
	  /// <seealso cref= #ClassWriter(int) </seealso>
	  public const int COMPUTE_FRAMES = 2;

	  // Note: fields are ordered as in the ClassFile structure, and those related to attributes are
	  // ordered as in Section 4.7 of the JVMS.

	  /// <summary>
	  /// The minor_version and major_version fields of the JVMS ClassFile structure. minor_version is
	  /// stored in the 16 most significant bits, and major_version in the 16 least significant bits.
	  /// </summary>
	  private int version;

	  /// <summary>
	  /// The symbol table for this class (contains the constant_pool and the BootstrapMethods). </summary>
	  private readonly SymbolTable symbolTable;

	  /// <summary>
	  /// The access_flags field of the JVMS ClassFile structure. This field can contain ASM specific
	  /// access flags, such as <seealso cref="Opcodes.ACC_DEPRECATED"/> or <seealso cref="Opcodes.ACC_RECORD"/>, which are
	  /// removed when generating the ClassFile structure.
	  /// </summary>
	  private int accessFlags;

	  /// <summary>
	  /// The this_class field of the JVMS ClassFile structure. </summary>
	  private int thisClass;

	  /// <summary>
	  /// The super_class field of the JVMS ClassFile structure. </summary>
	  private int superClass;

	  /// <summary>
	  /// The interface_count field of the JVMS ClassFile structure. </summary>
	  private int interfaceCount;

	  /// <summary>
	  /// The 'interfaces' array of the JVMS ClassFile structure. </summary>
	  private int[] interfaces;

	  /// <summary>
	  /// The fields of this class, stored in a linked list of <seealso cref="FieldWriter"/> linked via their
	  /// <seealso cref="FieldWriter.fv"/> field. This field stores the first element of this list.
	  /// </summary>
	  private FieldWriter firstField;

	  /// <summary>
	  /// The fields of this class, stored in a linked list of <seealso cref="FieldWriter"/> linked via their
	  /// <seealso cref="FieldWriter.fv"/> field. This field stores the last element of this list.
	  /// </summary>
	  private FieldWriter lastField;

	  /// <summary>
	  /// The methods of this class, stored in a linked list of <seealso cref="MethodWriter"/> linked via their
	  /// <seealso cref="MethodWriter.mv"/> field. This field stores the first element of this list.
	  /// </summary>
	  private MethodWriter firstMethod;

	  /// <summary>
	  /// The methods of this class, stored in a linked list of <seealso cref="MethodWriter"/> linked via their
	  /// <seealso cref="MethodWriter.mv"/> field. This field stores the last element of this list.
	  /// </summary>
	  private MethodWriter lastMethod;

	  /// <summary>
	  /// The number_of_classes field of the InnerClasses attribute, or 0. </summary>
	  private int numberOfInnerClasses;

	  /// <summary>
	  /// The 'classes' array of the InnerClasses attribute, or {@literal null}. </summary>
	  private ByteVector innerClasses;

	  /// <summary>
	  /// The class_index field of the EnclosingMethod attribute, or 0. </summary>
	  private int enclosingClassIndex;

	  /// <summary>
	  /// The method_index field of the EnclosingMethod attribute. </summary>
	  private int enclosingMethodIndex;

	  /// <summary>
	  /// The signature_index field of the Signature attribute, or 0. </summary>
	  private int signatureIndex;

	  /// <summary>
	  /// The source_file_index field of the SourceFile attribute, or 0. </summary>
	  private int sourceFileIndex;

	  /// <summary>
	  /// The debug_extension field of the SourceDebugExtension attribute, or {@literal null}. </summary>
	  private ByteVector debugExtension;

	  /// <summary>
	  /// The last runtime visible annotation of this class. The previous ones can be accessed with the
	  /// <seealso cref="AnnotationWriter.previousAnnotation"/> field. May be {@literal null}.
	  /// </summary>
	  private AnnotationWriter lastRuntimeVisibleAnnotation;

	  /// <summary>
	  /// The last runtime invisible annotation of this class. The previous ones can be accessed with the
	  /// <seealso cref="AnnotationWriter.previousAnnotation"/> field. May be {@literal null}.
	  /// </summary>
	  private AnnotationWriter lastRuntimeInvisibleAnnotation;

	  /// <summary>
	  /// The last runtime visible type annotation of this class. The previous ones can be accessed with
	  /// the <seealso cref="AnnotationWriter.previousAnnotation"/> field. May be {@literal null}.
	  /// </summary>
	  private AnnotationWriter lastRuntimeVisibleTypeAnnotation;

	  /// <summary>
	  /// The last runtime invisible type annotation of this class. The previous ones can be accessed
	  /// with the <seealso cref="AnnotationWriter.previousAnnotation"/> field. May be {@literal null}.
	  /// </summary>
	  private AnnotationWriter lastRuntimeInvisibleTypeAnnotation;

	  /// <summary>
	  /// The Module attribute of this class, or {@literal null}. </summary>
	  private ModuleWriter moduleWriter;

	  /// <summary>
	  /// The host_class_index field of the NestHost attribute, or 0. </summary>
	  private int nestHostClassIndex;

	  /// <summary>
	  /// The number_of_classes field of the NestMembers attribute, or 0. </summary>
	  private int numberOfNestMemberClasses;

	  /// <summary>
	  /// The 'classes' array of the NestMembers attribute, or {@literal null}. </summary>
	  private ByteVector nestMemberClasses;

	  /// <summary>
	  /// The number_of_classes field of the PermittedSubclasses attribute, or 0. </summary>
	  private int numberOfPermittedSubclasses;

	  /// <summary>
	  /// The 'classes' array of the PermittedSubclasses attribute, or {@literal null}. </summary>
	  private ByteVector permittedSubclasses;

	  /// <summary>
	  /// The record components of this class, stored in a linked list of <seealso cref="RecordComponentWriter"/>
	  /// linked via their <seealso cref="RecordComponentWriter.delegate"/> field. This field stores the first
	  /// element of this list.
	  /// </summary>
	  private RecordComponentWriter firstRecordComponent;

	  /// <summary>
	  /// The record components of this class, stored in a linked list of <seealso cref="RecordComponentWriter"/>
	  /// linked via their <seealso cref="RecordComponentWriter.delegate"/> field. This field stores the last
	  /// element of this list.
	  /// </summary>
	  private RecordComponentWriter lastRecordComponent;

	  /// <summary>
	  /// The first non standard attribute of this class. The next ones can be accessed with the {@link
	  /// Attribute#nextAttribute} field. May be {@literal null}.
	  /// 
	  /// <para><b>WARNING</b>: this list stores the attributes in the <i>reverse</i> order of their visit.
	  /// firstAttribute is actually the last attribute visited in <seealso cref="visitAttribute"/>. The {@link
	  /// #toByteArray} method writes the attributes in the order defined by this list, i.e. in the
	  /// reverse order specified by the user.
	  /// </para>
	  /// </summary>
	  private Attribute firstAttribute;

	  /// <summary>
	  /// Indicates what must be automatically computed in <seealso cref="MethodWriter"/>. Must be one of {@link
	  /// MethodWriter#COMPUTE_NOTHING}, <seealso cref="MethodWriter.COMPUTE_MAX_STACK_AND_LOCAL"/>, {@link
	  /// MethodWriter#COMPUTE_INSERTED_FRAMES}, or <seealso cref="MethodWriter.COMPUTE_ALL_FRAMES"/>.
	  /// </summary>
	  private int compute;

	  // -----------------------------------------------------------------------------------------------
	  // Constructor
	  // -----------------------------------------------------------------------------------------------

	  /// <summary>
	  /// Constructs a new <seealso cref="ClassWriter"/> object.
	  /// </summary>
	  /// <param name="flags"> option flags that can be used to modify the default behavior of this class. Must
	  ///     be zero or more of <seealso cref="COMPUTE_MAXS"/> and <seealso cref="COMPUTE_FRAMES"/>. </param>
	  public ClassWriter(int flags) : this(null, flags)
	  {
	  }

	  /// <summary>
	  /// Constructs a new <seealso cref="ClassWriter"/> object and enables optimizations for "mostly add" bytecode
	  /// transformations. These optimizations are the following:
	  /// 
	  /// <ul>
	  ///   <li>The constant pool and bootstrap methods from the original class are copied as is in the
	  ///       new class, which saves time. New constant pool entries and new bootstrap methods will be
	  ///       added at the end if necessary, but unused constant pool entries or bootstrap methods
	  ///       <i>won't be removed</i>.
	  ///   <li>Methods that are not transformed are copied as is in the new class, directly from the
	  ///       original class bytecode (i.e. without emitting visit events for all the method
	  ///       instructions), which saves a <i>lot</i> of time. Untransformed methods are detected by
	  ///       the fact that the <seealso cref="ClassReader"/> receives <seealso cref="MethodVisitor"/> objects that come
	  ///       from a <seealso cref="ClassWriter"/> (and not from any other <seealso cref="ClassVisitor"/> instance).
	  /// </ul>
	  /// </summary>
	  /// <param name="classReader"> the <seealso cref="ClassReader"/> used to read the original class. It will be used to
	  ///     copy the entire constant pool and bootstrap methods from the original class and also to
	  ///     copy other fragments of original bytecode where applicable. </param>
	  /// <param name="flags"> option flags that can be used to modify the default behavior of this class.Must be
	  ///     zero or more of <seealso cref="COMPUTE_MAXS"/> and <seealso cref="COMPUTE_FRAMES"/>. <i>These option flags do
	  ///     not affect methods that are copied as is in the new class. This means that neither the
	  ///     maximum stack size nor the stack frames will be computed for these methods</i>. </param>
	  public ClassWriter(ClassReader classReader, int flags) : base(Opcodes.ASM9)
	  {
		symbolTable = classReader == null ? new SymbolTable(this) : new SymbolTable(this, classReader);
		if ((flags & COMPUTE_FRAMES) != 0)
		{
		  this.compute = MethodWriter.COMPUTE_ALL_FRAMES;
		}
		else if ((flags & COMPUTE_MAXS) != 0)
		{
		  this.compute = MethodWriter.COMPUTE_MAX_STACK_AND_LOCAL;
		}
		else
		{
		  this.compute = MethodWriter.COMPUTE_NOTHING;
		}
	  }

	  // -----------------------------------------------------------------------------------------------
	  // Implementation of the ClassVisitor abstract class
	  // -----------------------------------------------------------------------------------------------

	  public override sealed void visit(int version, int access, string name, string signature, string superName, string[] interfaces)
	  {
		this.version = version;
		this.accessFlags = access;
		this.thisClass = symbolTable.setMajorVersionAndClassName(version & 0xFFFF, name);
		if (!string.ReferenceEquals(signature, null))
		{
		  this.signatureIndex = symbolTable.addConstantUtf8(signature);
		}
		this.superClass = string.ReferenceEquals(superName, null) ? 0 : symbolTable.addConstantClass(superName).index;
		if (interfaces != null && interfaces.Length > 0)
		{
		  interfaceCount = interfaces.Length;
		  this.interfaces = new int[interfaceCount];
		  for (int i = 0; i < interfaceCount; ++i)
		  {
			this.interfaces[i] = symbolTable.addConstantClass(interfaces[i]).index;
		  }
		}
		if (compute == MethodWriter.COMPUTE_MAX_STACK_AND_LOCAL && (version & 0xFFFF) >= Opcodes.V1_7)
		{
		  compute = MethodWriter.COMPUTE_MAX_STACK_AND_LOCAL_FROM_FRAMES;
		}
	  }

	  public override sealed void visitSource(string file, string debug)
	  {
		if (!string.ReferenceEquals(file, null))
		{
		  sourceFileIndex = symbolTable.addConstantUtf8(file);
		}
		if (!string.ReferenceEquals(debug, null))
		{
		  debugExtension = (new ByteVector()).encodeUtf8(debug, 0, int.MaxValue);
		}
	  }

	  public override sealed ModuleVisitor visitModule(string name, int access, string version)
	  {
		return moduleWriter = new ModuleWriter(symbolTable, symbolTable.addConstantModule(name).index, access, string.ReferenceEquals(version, null) ? 0 : symbolTable.addConstantUtf8(version));
	  }

	  public override sealed void visitNestHost(string nestHost)
	  {
		nestHostClassIndex = symbolTable.addConstantClass(nestHost).index;
	  }

	  public override sealed void visitOuterClass(string owner, string name, string descriptor)
	  {
		enclosingClassIndex = symbolTable.addConstantClass(owner).index;
		if (!string.ReferenceEquals(name, null) && !string.ReferenceEquals(descriptor, null))
		{
		  enclosingMethodIndex = symbolTable.addConstantNameAndType(name, descriptor);
		}
	  }

	  public override sealed AnnotationVisitor visitAnnotation(string descriptor, bool visible)
	  {
		if (visible)
		{
		  return lastRuntimeVisibleAnnotation = AnnotationWriter.create(symbolTable, descriptor, lastRuntimeVisibleAnnotation);
		}
		else
		{
		  return lastRuntimeInvisibleAnnotation = AnnotationWriter.create(symbolTable, descriptor, lastRuntimeInvisibleAnnotation);
		}
	  }

	  public override sealed AnnotationVisitor visitTypeAnnotation(int typeRef, TypePath typePath, string descriptor, bool visible)
	  {
		if (visible)
		{
		  return lastRuntimeVisibleTypeAnnotation = AnnotationWriter.create(symbolTable, typeRef, typePath, descriptor, lastRuntimeVisibleTypeAnnotation);
		}
		else
		{
		  return lastRuntimeInvisibleTypeAnnotation = AnnotationWriter.create(symbolTable, typeRef, typePath, descriptor, lastRuntimeInvisibleTypeAnnotation);
		}
	  }

	  public override sealed void visitAttribute(Attribute attribute)
	  {
		// Store the attributes in the <i>reverse</i> order of their visit by this method.
		attribute.nextAttribute = firstAttribute;
		firstAttribute = attribute;
	  }

	  public override sealed void visitNestMember(string nestMember)
	  {
		if (nestMemberClasses == null)
		{
		  nestMemberClasses = new ByteVector();
		}
		++numberOfNestMemberClasses;
		nestMemberClasses.putShort(symbolTable.addConstantClass(nestMember).index);
	  }

	  public override sealed void visitPermittedSubclass(string permittedSubclass)
	  {
		if (permittedSubclasses == null)
		{
		  permittedSubclasses = new ByteVector();
		}
		++numberOfPermittedSubclasses;
		permittedSubclasses.putShort(symbolTable.addConstantClass(permittedSubclass).index);
	  }

	  public override sealed void visitInnerClass(string name, string outerName, string innerName, int access)
	  {
		if (innerClasses == null)
		{
		  innerClasses = new ByteVector();
		}
		// Section 4.7.6 of the JVMS states "Every CONSTANT_Class_info entry in the constant_pool table
		// which represents a class or interface C that is not a package member must have exactly one
		// corresponding entry in the classes array". To avoid duplicates we keep track in the info
		// field of the Symbol of each CONSTANT_Class_info entry C whether an inner class entry has
		// already been added for C. If so, we store the index of this inner class entry (plus one) in
		// the info field. This trick allows duplicate detection in O(1) time.
		Symbol nameSymbol = symbolTable.addConstantClass(name);
		if (nameSymbol.info == 0)
		{
		  ++numberOfInnerClasses;
		  innerClasses.putShort(nameSymbol.index);
		  innerClasses.putShort(string.ReferenceEquals(outerName, null) ? 0 : symbolTable.addConstantClass(outerName).index);
		  innerClasses.putShort(string.ReferenceEquals(innerName, null) ? 0 : symbolTable.addConstantUtf8(innerName));
		  innerClasses.putShort(access);
		  nameSymbol.info = numberOfInnerClasses;
		}
		// Else, compare the inner classes entry nameSymbol.info - 1 with the arguments of this method
		// and throw an exception if there is a difference?
	  }

	  public override sealed RecordComponentVisitor visitRecordComponent(string name, string descriptor, string signature)
	  {
		RecordComponentWriter recordComponentWriter = new RecordComponentWriter(symbolTable, name, descriptor, signature);
		if (firstRecordComponent == null)
		{
		  firstRecordComponent = recordComponentWriter;
		}
		else
		{
		  lastRecordComponent.@delegate = recordComponentWriter;
		}
		return lastRecordComponent = recordComponentWriter;
	  }

	  public override sealed FieldVisitor visitField(int access, string name, string descriptor, string signature, object value)
	  {
		FieldWriter fieldWriter = new FieldWriter(symbolTable, access, name, descriptor, signature, value);
		if (firstField == null)
		{
		  firstField = fieldWriter;
		}
		else
		{
		  lastField.fv = fieldWriter;
		}
		return lastField = fieldWriter;
	  }

	  public override sealed MethodVisitor visitMethod(int access, string name, string descriptor, string signature, string[] exceptions)
	  {
		MethodWriter methodWriter = new MethodWriter(symbolTable, access, name, descriptor, signature, exceptions, compute);
		if (firstMethod == null)
		{
		  firstMethod = methodWriter;
		}
		else
		{
		  lastMethod.mv = methodWriter;
		}
		return lastMethod = methodWriter;
	  }

	  public override sealed void visitEnd()
	  {
		// Nothing to do.
	  }

	  // -----------------------------------------------------------------------------------------------
	  // Other public methods
	  // -----------------------------------------------------------------------------------------------

	  /// <summary>
	  /// Returns the content of the class file that was built by this ClassWriter.
	  /// </summary>
	  /// <returns> the binary content of the JVMS ClassFile structure that was built by this ClassWriter. </returns>
	  /// <exception cref="ClassTooLargeException"> if the constant pool of the class is too large. </exception>
	  /// <exception cref="MethodTooLargeException"> if the Code attribute of a method is too large. </exception>
	  public virtual sbyte[] toByteArray()
	  {
		// First step: compute the size in bytes of the ClassFile structure.
		// The magic field uses 4 bytes, 10 mandatory fields (minor_version, major_version,
		// constant_pool_count, access_flags, this_class, super_class, interfaces_count, fields_count,
		// methods_count and attributes_count) use 2 bytes each, and each interface uses 2 bytes too.
		int size = 24 + 2 * interfaceCount;
		int fieldsCount = 0;
		FieldWriter fieldWriter = firstField;
		while (fieldWriter != null)
		{
		  ++fieldsCount;
		  size += fieldWriter.computeFieldInfoSize();
		  fieldWriter = (FieldWriter) fieldWriter.fv;
		}
		int methodsCount = 0;
		MethodWriter methodWriter = firstMethod;
		while (methodWriter != null)
		{
		  ++methodsCount;
		  size += methodWriter.computeMethodInfoSize();
		  methodWriter = (MethodWriter) methodWriter.mv;
		}

		// For ease of reference, we use here the same attribute order as in Section 4.7 of the JVMS.
		int attributesCount = 0;
		if (innerClasses != null)
		{
		  ++attributesCount;
		  size += 8 + innerClasses.length;
		  symbolTable.addConstantUtf8(Constants.INNER_CLASSES);
		}
		if (enclosingClassIndex != 0)
		{
		  ++attributesCount;
		  size += 10;
		  symbolTable.addConstantUtf8(Constants.ENCLOSING_METHOD);
		}
		if ((accessFlags & Opcodes.ACC_SYNTHETIC) != 0 && (version & 0xFFFF) < Opcodes.V1_5)
		{
		  ++attributesCount;
		  size += 6;
		  symbolTable.addConstantUtf8(Constants.SYNTHETIC);
		}
		if (signatureIndex != 0)
		{
		  ++attributesCount;
		  size += 8;
		  symbolTable.addConstantUtf8(Constants.SIGNATURE);
		}
		if (sourceFileIndex != 0)
		{
		  ++attributesCount;
		  size += 8;
		  symbolTable.addConstantUtf8(Constants.SOURCE_FILE);
		}
		if (debugExtension != null)
		{
		  ++attributesCount;
		  size += 6 + debugExtension.length;
		  symbolTable.addConstantUtf8(Constants.SOURCE_DEBUG_EXTENSION);
		}
		if ((accessFlags & Opcodes.ACC_DEPRECATED) != 0)
		{
		  ++attributesCount;
		  size += 6;
		  symbolTable.addConstantUtf8(Constants.DEPRECATED);
		}
		if (lastRuntimeVisibleAnnotation != null)
		{
		  ++attributesCount;
		  size += lastRuntimeVisibleAnnotation.computeAnnotationsSize(Constants.RUNTIME_VISIBLE_ANNOTATIONS);
		}
		if (lastRuntimeInvisibleAnnotation != null)
		{
		  ++attributesCount;
		  size += lastRuntimeInvisibleAnnotation.computeAnnotationsSize(Constants.RUNTIME_INVISIBLE_ANNOTATIONS);
		}
		if (lastRuntimeVisibleTypeAnnotation != null)
		{
		  ++attributesCount;
		  size += lastRuntimeVisibleTypeAnnotation.computeAnnotationsSize(Constants.RUNTIME_VISIBLE_TYPE_ANNOTATIONS);
		}
		if (lastRuntimeInvisibleTypeAnnotation != null)
		{
		  ++attributesCount;
		  size += lastRuntimeInvisibleTypeAnnotation.computeAnnotationsSize(Constants.RUNTIME_INVISIBLE_TYPE_ANNOTATIONS);
		}
		if (symbolTable.computeBootstrapMethodsSize() > 0)
		{
		  ++attributesCount;
		  size += symbolTable.computeBootstrapMethodsSize();
		}
		if (moduleWriter != null)
		{
		  attributesCount += moduleWriter.AttributeCount;
		  size += moduleWriter.computeAttributesSize();
		}
		if (nestHostClassIndex != 0)
		{
		  ++attributesCount;
		  size += 8;
		  symbolTable.addConstantUtf8(Constants.NEST_HOST);
		}
		if (nestMemberClasses != null)
		{
		  ++attributesCount;
		  size += 8 + nestMemberClasses.length;
		  symbolTable.addConstantUtf8(Constants.NEST_MEMBERS);
		}
		if (permittedSubclasses != null)
		{
		  ++attributesCount;
		  size += 8 + permittedSubclasses.length;
		  symbolTable.addConstantUtf8(Constants.PERMITTED_SUBCLASSES);
		}
		int recordComponentCount = 0;
		int recordSize = 0;
		if ((accessFlags & Opcodes.ACC_RECORD) != 0 || firstRecordComponent != null)
		{
		  RecordComponentWriter recordComponentWriter = firstRecordComponent;
		  while (recordComponentWriter != null)
		  {
			++recordComponentCount;
			recordSize += recordComponentWriter.computeRecordComponentInfoSize();
			recordComponentWriter = (RecordComponentWriter) recordComponentWriter.@delegate;
		  }
		  ++attributesCount;
		  size += 8 + recordSize;
		  symbolTable.addConstantUtf8(Constants.RECORD);
		}
		if (firstAttribute != null)
		{
		  attributesCount += firstAttribute.AttributeCount;
		  size += firstAttribute.computeAttributesSize(symbolTable);
		}
		// IMPORTANT: this must be the last part of the ClassFile size computation, because the previous
		// statements can add attribute names to the constant pool, thereby changing its size!
		size += symbolTable.ConstantPoolLength;
		int constantPoolCount = symbolTable.ConstantPoolCount;
		if (constantPoolCount > 0xFFFF)
		{
		  throw new ClassTooLargeException(symbolTable.ClassName, constantPoolCount);
		}

		// Second step: allocate a ByteVector of the correct size (in order to avoid any array copy in
		// dynamic resizes) and fill it with the ClassFile content.
		ByteVector result = new ByteVector(size);
		result.putInt(unchecked((int)0xCAFEBABE)).putInt(version);
		symbolTable.putConstantPool(result);
		int mask = (version & 0xFFFF) < Opcodes.V1_5 ? Opcodes.ACC_SYNTHETIC : 0;
		result.putShort(accessFlags & ~mask).putShort(thisClass).putShort(superClass);
		result.putShort(interfaceCount);
		for (int i = 0; i < interfaceCount; ++i)
		{
		  result.putShort(interfaces[i]);
		}
		result.putShort(fieldsCount);
		fieldWriter = firstField;
		while (fieldWriter != null)
		{
		  fieldWriter.putFieldInfo(result);
		  fieldWriter = (FieldWriter) fieldWriter.fv;
		}
		result.putShort(methodsCount);
		bool hasFrames = false;
		bool hasAsmInstructions = false;
		methodWriter = firstMethod;
		while (methodWriter != null)
		{
		  hasFrames |= methodWriter.hasFrames();
		  hasAsmInstructions |= methodWriter.hasAsmInstructions();
		  methodWriter.putMethodInfo(result);
		  methodWriter = (MethodWriter) methodWriter.mv;
		}
		// For ease of reference, we use here the same attribute order as in Section 4.7 of the JVMS.
		result.putShort(attributesCount);
		if (innerClasses != null)
		{
		  result.putShort(symbolTable.addConstantUtf8(Constants.INNER_CLASSES)).putInt(innerClasses.length + 2).putShort(numberOfInnerClasses).putByteArray(innerClasses.data, 0, innerClasses.length);
		}
		if (enclosingClassIndex != 0)
		{
		  result.putShort(symbolTable.addConstantUtf8(Constants.ENCLOSING_METHOD)).putInt(4).putShort(enclosingClassIndex).putShort(enclosingMethodIndex);
		}
		if ((accessFlags & Opcodes.ACC_SYNTHETIC) != 0 && (version & 0xFFFF) < Opcodes.V1_5)
		{
		  result.putShort(symbolTable.addConstantUtf8(Constants.SYNTHETIC)).putInt(0);
		}
		if (signatureIndex != 0)
		{
		  result.putShort(symbolTable.addConstantUtf8(Constants.SIGNATURE)).putInt(2).putShort(signatureIndex);
		}
		if (sourceFileIndex != 0)
		{
		  result.putShort(symbolTable.addConstantUtf8(Constants.SOURCE_FILE)).putInt(2).putShort(sourceFileIndex);
		}
		if (debugExtension != null)
		{
		  int length = debugExtension.length;
		  result.putShort(symbolTable.addConstantUtf8(Constants.SOURCE_DEBUG_EXTENSION)).putInt(length).putByteArray(debugExtension.data, 0, length);
		}
		if ((accessFlags & Opcodes.ACC_DEPRECATED) != 0)
		{
		  result.putShort(symbolTable.addConstantUtf8(Constants.DEPRECATED)).putInt(0);
		}
		AnnotationWriter.putAnnotations(symbolTable, lastRuntimeVisibleAnnotation, lastRuntimeInvisibleAnnotation, lastRuntimeVisibleTypeAnnotation, lastRuntimeInvisibleTypeAnnotation, result);
		symbolTable.putBootstrapMethods(result);
		if (moduleWriter != null)
		{
		  moduleWriter.putAttributes(result);
		}
		if (nestHostClassIndex != 0)
		{
		  result.putShort(symbolTable.addConstantUtf8(Constants.NEST_HOST)).putInt(2).putShort(nestHostClassIndex);
		}
		if (nestMemberClasses != null)
		{
		  result.putShort(symbolTable.addConstantUtf8(Constants.NEST_MEMBERS)).putInt(nestMemberClasses.length + 2).putShort(numberOfNestMemberClasses).putByteArray(nestMemberClasses.data, 0, nestMemberClasses.length);
		}
		if (permittedSubclasses != null)
		{
		  result.putShort(symbolTable.addConstantUtf8(Constants.PERMITTED_SUBCLASSES)).putInt(permittedSubclasses.length + 2).putShort(numberOfPermittedSubclasses).putByteArray(permittedSubclasses.data, 0, permittedSubclasses.length);
		}
		if ((accessFlags & Opcodes.ACC_RECORD) != 0 || firstRecordComponent != null)
		{
		  result.putShort(symbolTable.addConstantUtf8(Constants.RECORD)).putInt(recordSize + 2).putShort(recordComponentCount);
		  RecordComponentWriter recordComponentWriter = firstRecordComponent;
		  while (recordComponentWriter != null)
		  {
			recordComponentWriter.putRecordComponentInfo(result);
			recordComponentWriter = (RecordComponentWriter) recordComponentWriter.@delegate;
		  }
		}
		if (firstAttribute != null)
		{
		  firstAttribute.putAttributes(symbolTable, result);
		}

		// Third step: replace the ASM specific instructions, if any.
		if (hasAsmInstructions)
		{
		  return replaceAsmInstructions(result.data, hasFrames);
		}
		else
		{
		  return result.data;
		}
	  }

	  /// <summary>
	  /// Returns the equivalent of the given class file, with the ASM specific instructions replaced
	  /// with standard ones. This is done with a ClassReader -&gt; ClassWriter round trip.
	  /// </summary>
	  /// <param name="classFile"> a class file containing ASM specific instructions, generated by this
	  ///     ClassWriter. </param>
	  /// <param name="hasFrames"> whether there is at least one stack map frames in 'classFile'. </param>
	  /// <returns> an equivalent of 'classFile', with the ASM specific instructions replaced with standard
	  ///     ones. </returns>
	  private sbyte[] replaceAsmInstructions(sbyte[] classFile, bool hasFrames)
	  {
		Attribute[] attributes = AttributePrototypes;
		firstField = null;
		lastField = null;
		firstMethod = null;
		lastMethod = null;
		lastRuntimeVisibleAnnotation = null;
		lastRuntimeInvisibleAnnotation = null;
		lastRuntimeVisibleTypeAnnotation = null;
		lastRuntimeInvisibleTypeAnnotation = null;
		moduleWriter = null;
		nestHostClassIndex = 0;
		numberOfNestMemberClasses = 0;
		nestMemberClasses = null;
		numberOfPermittedSubclasses = 0;
		permittedSubclasses = null;
		firstRecordComponent = null;
		lastRecordComponent = null;
		firstAttribute = null;
		compute = hasFrames ? MethodWriter.COMPUTE_INSERTED_FRAMES : MethodWriter.COMPUTE_NOTHING;
		(new ClassReader(classFile, 0, false)).accept(this, attributes, (hasFrames ? ClassReader.EXPAND_FRAMES : 0) | ClassReader.EXPAND_ASM_INSNS);
		return toByteArray();
	  }

	  /// <summary>
	  /// Returns the prototypes of the attributes used by this class, its fields and its methods.
	  /// </summary>
	  /// <returns> the prototypes of the attributes used by this class, its fields and its methods. </returns>
	  private Attribute[] AttributePrototypes
	  {
		  get
		  {
			Attribute.Set attributePrototypes = new Attribute.Set();
			attributePrototypes.addAttributes(firstAttribute);
			FieldWriter fieldWriter = firstField;
			while (fieldWriter != null)
			{
			  fieldWriter.collectAttributePrototypes(attributePrototypes);
			  fieldWriter = (FieldWriter) fieldWriter.fv;
			}
			MethodWriter methodWriter = firstMethod;
			while (methodWriter != null)
			{
			  methodWriter.collectAttributePrototypes(attributePrototypes);
			  methodWriter = (MethodWriter) methodWriter.mv;
			}
			RecordComponentWriter recordComponentWriter = firstRecordComponent;
			while (recordComponentWriter != null)
			{
			  recordComponentWriter.collectAttributePrototypes(attributePrototypes);
			  recordComponentWriter = (RecordComponentWriter) recordComponentWriter.@delegate;
			}
			return attributePrototypes.toArray();
		  }
	  }

	  // -----------------------------------------------------------------------------------------------
	  // Utility methods: constant pool management for Attribute sub classes
	  // -----------------------------------------------------------------------------------------------

	  /// <summary>
	  /// Adds a number or string constant to the constant pool of the class being build. Does nothing if
	  /// the constant pool already contains a similar item. <i>This method is intended for {@link
	  /// Attribute} sub classes, and is normally not needed by class generators or adapters.</i>
	  /// </summary>
	  /// <param name="value"> the value of the constant to be added to the constant pool. This parameter must be
	  ///     an <seealso cref="Integer"/>, a <seealso cref="Float"/>, a <seealso cref="Long"/>, a <seealso cref="Double"/> or a <seealso cref="string"/>. </param>
	  /// <returns> the index of a new or already existing constant item with the given value. </returns>
	  public virtual int newConst(object value)
	  {
		return symbolTable.addConstant(value).index;
	  }

	  /// <summary>
	  /// Adds an UTF8 string to the constant pool of the class being build. Does nothing if the constant
	  /// pool already contains a similar item. <i>This method is intended for <seealso cref="Attribute"/> sub
	  /// classes, and is normally not needed by class generators or adapters.</i>
	  /// </summary>
	  /// <param name="value"> the String value. </param>
	  /// <returns> the index of a new or already existing UTF8 item. </returns>
	  // DontCheck(AbbreviationAsWordInName): can't be renamed (for backward binary compatibility).
	  public virtual int newUTF8(string value)
	  {
		return symbolTable.addConstantUtf8(value);
	  }

	  /// <summary>
	  /// Adds a class reference to the constant pool of the class being build. Does nothing if the
	  /// constant pool already contains a similar item. <i>This method is intended for <seealso cref="Attribute"/>
	  /// sub classes, and is normally not needed by class generators or adapters.</i>
	  /// </summary>
	  /// <param name="value"> the internal name of the class. </param>
	  /// <returns> the index of a new or already existing class reference item. </returns>
	  public virtual int newClass(string value)
	  {
		return symbolTable.addConstantClass(value).index;
	  }

	  /// <summary>
	  /// Adds a method type reference to the constant pool of the class being build. Does nothing if the
	  /// constant pool already contains a similar item. <i>This method is intended for <seealso cref="Attribute"/>
	  /// sub classes, and is normally not needed by class generators or adapters.</i>
	  /// </summary>
	  /// <param name="methodDescriptor"> method descriptor of the method type. </param>
	  /// <returns> the index of a new or already existing method type reference item. </returns>
	  public virtual int newMethodType(string methodDescriptor)
	  {
		return symbolTable.addConstantMethodType(methodDescriptor).index;
	  }

	  /// <summary>
	  /// Adds a module reference to the constant pool of the class being build. Does nothing if the
	  /// constant pool already contains a similar item. <i>This method is intended for <seealso cref="Attribute"/>
	  /// sub classes, and is normally not needed by class generators or adapters.</i>
	  /// </summary>
	  /// <param name="moduleName"> name of the module. </param>
	  /// <returns> the index of a new or already existing module reference item. </returns>
	  public virtual int newModule(string moduleName)
	  {
		return symbolTable.addConstantModule(moduleName).index;
	  }

	  /// <summary>
	  /// Adds a package reference to the constant pool of the class being build. Does nothing if the
	  /// constant pool already contains a similar item. <i>This method is intended for <seealso cref="Attribute"/>
	  /// sub classes, and is normally not needed by class generators or adapters.</i>
	  /// </summary>
	  /// <param name="packageName"> name of the package in its internal form. </param>
	  /// <returns> the index of a new or already existing module reference item. </returns>
	  public virtual int newPackage(string packageName)
	  {
		return symbolTable.addConstantPackage(packageName).index;
	  }

	  /// <summary>
	  /// Adds a handle to the constant pool of the class being build. Does nothing if the constant pool
	  /// already contains a similar item. <i>This method is intended for <seealso cref="Attribute"/> sub classes,
	  /// and is normally not needed by class generators or adapters.</i>
	  /// </summary>
	  /// <param name="tag"> the kind of this handle. Must be <seealso cref="Opcodes.H_GETFIELD"/>, {@link
	  ///     Opcodes#H_GETSTATIC}, <seealso cref="Opcodes.H_PUTFIELD"/>, <seealso cref="Opcodes.H_PUTSTATIC"/>, {@link
	  ///     Opcodes#H_INVOKEVIRTUAL}, <seealso cref="Opcodes.H_INVOKESTATIC"/>, <seealso cref="Opcodes.H_INVOKESPECIAL"/>,
	  ///     <seealso cref="Opcodes.H_NEWINVOKESPECIAL"/> or <seealso cref="Opcodes.H_INVOKEINTERFACE"/>. </param>
	  /// <param name="owner"> the internal name of the field or method owner class. </param>
	  /// <param name="name"> the name of the field or method. </param>
	  /// <param name="descriptor"> the descriptor of the field or method. </param>
	  /// <returns> the index of a new or already existing method type reference item. </returns>
	  /// @deprecated this method is superseded by {@link #newHandle(int, String, String, String,
	  ///     boolean)}. 
	  [Obsolete("this method is superseded by {@link #newHandle(int, String, String, String,")]
	  public virtual int newHandle(int tag, string owner, string name, string descriptor)
	  {
		return newHandle(tag, owner, name, descriptor, tag == Opcodes.H_INVOKEINTERFACE);
	  }

	  /// <summary>
	  /// Adds a handle to the constant pool of the class being build. Does nothing if the constant pool
	  /// already contains a similar item. <i>This method is intended for <seealso cref="Attribute"/> sub classes,
	  /// and is normally not needed by class generators or adapters.</i>
	  /// </summary>
	  /// <param name="tag"> the kind of this handle. Must be <seealso cref="Opcodes.H_GETFIELD"/>, {@link
	  ///     Opcodes#H_GETSTATIC}, <seealso cref="Opcodes.H_PUTFIELD"/>, <seealso cref="Opcodes.H_PUTSTATIC"/>, {@link
	  ///     Opcodes#H_INVOKEVIRTUAL}, <seealso cref="Opcodes.H_INVOKESTATIC"/>, <seealso cref="Opcodes.H_INVOKESPECIAL"/>,
	  ///     <seealso cref="Opcodes.H_NEWINVOKESPECIAL"/> or <seealso cref="Opcodes.H_INVOKEINTERFACE"/>. </param>
	  /// <param name="owner"> the internal name of the field or method owner class. </param>
	  /// <param name="name"> the name of the field or method. </param>
	  /// <param name="descriptor"> the descriptor of the field or method. </param>
	  /// <param name="isInterface"> true if the owner is an interface. </param>
	  /// <returns> the index of a new or already existing method type reference item. </returns>
	  public virtual int newHandle(int tag, string owner, string name, string descriptor, bool isInterface)
	  {
		return symbolTable.addConstantMethodHandle(tag, owner, name, descriptor, isInterface).index;
	  }

	  /// <summary>
	  /// Adds a dynamic constant reference to the constant pool of the class being build. Does nothing
	  /// if the constant pool already contains a similar item. <i>This method is intended for {@link
	  /// Attribute} sub classes, and is normally not needed by class generators or adapters.</i>
	  /// </summary>
	  /// <param name="name"> name of the invoked method. </param>
	  /// <param name="descriptor"> field descriptor of the constant type. </param>
	  /// <param name="bootstrapMethodHandle"> the bootstrap method. </param>
	  /// <param name="bootstrapMethodArguments"> the bootstrap method constant arguments. </param>
	  /// <returns> the index of a new or already existing dynamic constant reference item. </returns>
	  public virtual int newConstantDynamic(string name, string descriptor, Handle bootstrapMethodHandle, params object[] bootstrapMethodArguments)
	  {
		return symbolTable.addConstantDynamic(name, descriptor, bootstrapMethodHandle, bootstrapMethodArguments).index;
	  }

	  /// <summary>
	  /// Adds an invokedynamic reference to the constant pool of the class being build. Does nothing if
	  /// the constant pool already contains a similar item. <i>This method is intended for {@link
	  /// Attribute} sub classes, and is normally not needed by class generators or adapters.</i>
	  /// </summary>
	  /// <param name="name"> name of the invoked method. </param>
	  /// <param name="descriptor"> descriptor of the invoke method. </param>
	  /// <param name="bootstrapMethodHandle"> the bootstrap method. </param>
	  /// <param name="bootstrapMethodArguments"> the bootstrap method constant arguments. </param>
	  /// <returns> the index of a new or already existing invokedynamic reference item. </returns>
	  public virtual int newInvokeDynamic(string name, string descriptor, Handle bootstrapMethodHandle, params object[] bootstrapMethodArguments)
	  {
		return symbolTable.addConstantInvokeDynamic(name, descriptor, bootstrapMethodHandle, bootstrapMethodArguments).index;
	  }

	  /// <summary>
	  /// Adds a field reference to the constant pool of the class being build. Does nothing if the
	  /// constant pool already contains a similar item. <i>This method is intended for <seealso cref="Attribute"/>
	  /// sub classes, and is normally not needed by class generators or adapters.</i>
	  /// </summary>
	  /// <param name="owner"> the internal name of the field's owner class. </param>
	  /// <param name="name"> the field's name. </param>
	  /// <param name="descriptor"> the field's descriptor. </param>
	  /// <returns> the index of a new or already existing field reference item. </returns>
	  public virtual int newField(string owner, string name, string descriptor)
	  {
		return symbolTable.addConstantFieldref(owner, name, descriptor).index;
	  }

	  /// <summary>
	  /// Adds a method reference to the constant pool of the class being build. Does nothing if the
	  /// constant pool already contains a similar item. <i>This method is intended for <seealso cref="Attribute"/>
	  /// sub classes, and is normally not needed by class generators or adapters.</i>
	  /// </summary>
	  /// <param name="owner"> the internal name of the method's owner class. </param>
	  /// <param name="name"> the method's name. </param>
	  /// <param name="descriptor"> the method's descriptor. </param>
	  /// <param name="isInterface"> {@literal true} if {@code owner} is an interface. </param>
	  /// <returns> the index of a new or already existing method reference item. </returns>
	  public virtual int newMethod(string owner, string name, string descriptor, bool isInterface)
	  {
		return symbolTable.addConstantMethodref(owner, name, descriptor, isInterface).index;
	  }

	  /// <summary>
	  /// Adds a name and type to the constant pool of the class being build. Does nothing if the
	  /// constant pool already contains a similar item. <i>This method is intended for <seealso cref="Attribute"/>
	  /// sub classes, and is normally not needed by class generators or adapters.</i>
	  /// </summary>
	  /// <param name="name"> a name. </param>
	  /// <param name="descriptor"> a type descriptor. </param>
	  /// <returns> the index of a new or already existing name and type item. </returns>
	  public virtual int newNameType(string name, string descriptor)
	  {
		return symbolTable.addConstantNameAndType(name, descriptor);
	  }

	  // -----------------------------------------------------------------------------------------------
	  // Default method to compute common super classes when computing stack map frames
	  // -----------------------------------------------------------------------------------------------

	  /// <summary>
	  /// Returns the common super type of the two given types. The default implementation of this method
	  /// <i>loads</i> the two given classes and uses the java.lang.Class methods to find the common
	  /// super class. It can be overridden to compute this common super type in other ways, in
	  /// particular without actually loading any class, or to take into account the class that is
	  /// currently being generated by this ClassWriter, which can of course not be loaded since it is
	  /// under construction.
	  /// </summary>
	  /// <param name="type1"> the internal name of a class. </param>
	  /// <param name="type2"> the internal name of another class. </param>
	  /// <returns> the internal name of the common super class of the two given classes. </returns>
	  public virtual string getCommonSuperClass(string type1, string type2)
	  {
		ClassLoader classLoader = ClassLoader;
		System.Type class1;
		try
		{
		  class1 = System.Type.GetType(type1.Replace('/', '.'), false, classLoader);
		}
		catch (System.Exception e)
		{
		  throw new TypeNotPresentException(type1, e);
		}
		System.Type class2;
		try
		{
		  class2 = System.Type.GetType(type2.Replace('/', '.'), false, classLoader);
		}
		catch (System.Exception e)
		{
		  throw new TypeNotPresentException(type2, e);
		}
		if (class1.IsAssignableFrom(class2))
		{
		  return type1;
		}
		if (class2.IsAssignableFrom(class1))
		{
		  return type2;
		}
		if (class1.IsInterface || class2.IsInterface)
		{
		  return "java/lang/Object";
		}
		else
		{
		  do
		  {
			class1 = class1.BaseType;
		  } while (!class1.IsAssignableFrom(class2));
		  return class1.FullName.Replace('.', '/');
		}
	  }

	  /// <summary>
	  /// Returns the <seealso cref="ClassLoader"/> to be used by the default implementation of {@link
	  /// #getCommonSuperClass(String, String)}, that of this <seealso cref="ClassWriter"/>'s runtime type by
	  /// default.
	  /// </summary>
	  /// <returns> ClassLoader </returns>
	  public virtual ClassLoader ClassLoader
	  {
		  get
		  {
			return this.GetType().getClassLoader();
		  }
	  }
	}

}