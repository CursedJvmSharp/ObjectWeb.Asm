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
namespace ObjectWeb.Asm
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
	  /// <para><b>Note:</b> for classes whose version is <seealso cref="IOpcodes.V1_7"/> of more, this option requires
	  /// valid stack map frames. The maximum stack size is then computed from these frames, and from the
	  /// bytecode instructions in between. If stack map frames are not present or must be recomputed,
	  /// used <seealso cref="Compute_Frames"/> instead.
	  /// 
	  /// </para>
	  /// </summary>
	  /// <seealso cref= #ClassWriter(int) </seealso>
	  public const int Compute_Maxs = 1;

	  /// <summary>
	  /// A flag to automatically compute the stack map frames of methods from scratch. If this flag is
	  /// set, then the calls to the <seealso cref="MethodVisitor.VisitFrame"/> method are ignored, and the stack
	  /// map frames are recomputed from the methods bytecode. The arguments of the {@link
	  /// MethodVisitor#visitMaxs} method are also ignored and recomputed from the bytecode. In other
	  /// words, <seealso cref="Compute_Frames"/> implies <seealso cref="Compute_Maxs"/>.
	  /// </summary>
	  /// <seealso cref= #ClassWriter(int) </seealso>
	  public const int Compute_Frames = 2;

	  // Note: fields are ordered as in the ClassFile structure, and those related to attributes are
	  // ordered as in Section 4.7 of the JVMS.

	  /// <summary>
	  /// The minor_version and major_version fields of the JVMS ClassFile structure. minor_version is
	  /// stored in the 16 most significant bits, and major_version in the 16 least significant bits.
	  /// </summary>
	  private int _version;

	  /// <summary>
	  /// The symbol table for this class (contains the constant_pool and the BootstrapMethods). </summary>
	  private readonly SymbolTable _symbolTable;

	  /// <summary>
	  /// The access_flags field of the JVMS ClassFile structure. This field can contain ASM specific
	  /// access flags, such as <seealso cref="IIOpcodes.Acc_Deprecated/> or <seealso cref="IIOpcodes.Acc_Record/>, which are
	  /// removed when generating the ClassFile structure.
	  /// </summary>
	  private int _accessFlags;

	  /// <summary>
	  /// The this_class field of the JVMS ClassFile structure. </summary>
	  private int _thisClass;

	  /// <summary>
	  /// The super_class field of the JVMS ClassFile structure. </summary>
	  private int _superClass;

	  /// <summary>
	  /// The interface_count field of the JVMS ClassFile structure. </summary>
	  private int _interfaceCount;

	  /// <summary>
	  /// The 'interfaces' array of the JVMS ClassFile structure. </summary>
	  private int[] _interfaces;

	  /// <summary>
	  /// The fields of this class, stored in a linked list of <seealso cref="FieldWriter"/> linked via their
	  /// <seealso cref="FieldWriter.fv"/> field. This field stores the first element of this list.
	  /// </summary>
	  private FieldWriter _firstField;

	  /// <summary>
	  /// The fields of this class, stored in a linked list of <seealso cref="FieldWriter"/> linked via their
	  /// <seealso cref="FieldWriter.fv"/> field. This field stores the last element of this list.
	  /// </summary>
	  private FieldWriter _lastField;

	  /// <summary>
	  /// The methods of this class, stored in a linked list of <seealso cref="MethodWriter"/> linked via their
	  /// <seealso cref="MethodWriter.mv"/> field. This field stores the first element of this list.
	  /// </summary>
	  private MethodWriter _firstMethod;

	  /// <summary>
	  /// The methods of this class, stored in a linked list of <seealso cref="MethodWriter"/> linked via their
	  /// <seealso cref="MethodWriter.mv"/> field. This field stores the last element of this list.
	  /// </summary>
	  private MethodWriter _lastMethod;

	  /// <summary>
	  /// The number_of_classes field of the InnerClasses attribute, or 0. </summary>
	  private int _numberOfInnerClasses;

	  /// <summary>
	  /// The 'classes' array of the InnerClasses attribute, or {@literal null}. </summary>
	  private ByteVector _innerClasses;

	  /// <summary>
	  /// The class_index field of the EnclosingMethod attribute, or 0. </summary>
	  private int _enclosingClassIndex;

	  /// <summary>
	  /// The method_index field of the EnclosingMethod attribute. </summary>
	  private int _enclosingMethodIndex;

	  /// <summary>
	  /// The signature_index field of the Signature attribute, or 0. </summary>
	  private int _signatureIndex;

	  /// <summary>
	  /// The source_file_index field of the SourceFile attribute, or 0. </summary>
	  private int _sourceFileIndex;

	  /// <summary>
	  /// The debug_extension field of the SourceDebugExtension attribute, or {@literal null}. </summary>
	  private ByteVector _debugExtension;

	  /// <summary>
	  /// The last runtime visible annotation of this class. The previous ones can be accessed with the
	  /// <seealso cref="AnnotationWriter._previousAnnotation"/> field. May be {@literal null}.
	  /// </summary>
	  private AnnotationWriter _lastRuntimeVisibleAnnotation;

	  /// <summary>
	  /// The last runtime invisible annotation of this class. The previous ones can be accessed with the
	  /// <seealso cref="AnnotationWriter._previousAnnotation"/> field. May be {@literal null}.
	  /// </summary>
	  private AnnotationWriter _lastRuntimeInvisibleAnnotation;

	  /// <summary>
	  /// The last runtime visible type annotation of this class. The previous ones can be accessed with
	  /// the <seealso cref="AnnotationWriter._previousAnnotation"/> field. May be {@literal null}.
	  /// </summary>
	  private AnnotationWriter _lastRuntimeVisibleTypeAnnotation;

	  /// <summary>
	  /// The last runtime invisible type annotation of this class. The previous ones can be accessed
	  /// with the <seealso cref="AnnotationWriter._previousAnnotation"/> field. May be {@literal null}.
	  /// </summary>
	  private AnnotationWriter _lastRuntimeInvisibleTypeAnnotation;

	  /// <summary>
	  /// The Module attribute of this class, or {@literal null}. </summary>
	  private ModuleWriter _moduleWriter;

	  /// <summary>
	  /// The host_class_index field of the NestHost attribute, or 0. </summary>
	  private int _nestHostClassIndex;

	  /// <summary>
	  /// The number_of_classes field of the NestMembers attribute, or 0. </summary>
	  private int _numberOfNestMemberClasses;

	  /// <summary>
	  /// The 'classes' array of the NestMembers attribute, or {@literal null}. </summary>
	  private ByteVector _nestMemberClasses;

	  /// <summary>
	  /// The number_of_classes field of the PermittedSubclasses attribute, or 0. </summary>
	  private int _numberOfPermittedSubclasses;

	  /// <summary>
	  /// The 'classes' array of the PermittedSubclasses attribute, or {@literal null}. </summary>
	  private ByteVector _permittedSubclasses;

	  /// <summary>
	  /// The record components of this class, stored in a linked list of <seealso cref="RecordComponentWriter"/>
	  /// linked via their <seealso cref="RecordComponentWriter.delegate"/> field. This field stores the first
	  /// element of this list.
	  /// </summary>
	  private RecordComponentWriter _firstRecordComponent;

	  /// <summary>
	  /// The record components of this class, stored in a linked list of <seealso cref="RecordComponentWriter"/>
	  /// linked via their <seealso cref="RecordComponentWriter.delegate"/> field. This field stores the last
	  /// element of this list.
	  /// </summary>
	  private RecordComponentWriter _lastRecordComponent;

	  /// <summary>
	  /// The first non standard attribute of this class. The next ones can be accessed with the {@link
	  /// Attribute#nextAttribute} field. May be {@literal null}.
	  /// 
	  /// <para><b>WARNING</b>: this list stores the attributes in the <i>reverse</i> order of their visit.
	  /// firstAttribute is actually the last attribute visited in <seealso cref="VisitAttribute"/>. The {@link
	  /// #toByteArray} method writes the attributes in the order defined by this list, i.e. in the
	  /// reverse order specified by the user.
	  /// </para>
	  /// </summary>
	  private Attribute _firstAttribute;

	  /// <summary>
	  /// Indicates what must be automatically computed in <seealso cref="MethodWriter"/>. Must be one of {@link
	  /// MethodWriter#COMPUTE_NOTHING}, <seealso cref="MethodWriter.Compute_Max_Stack_And_Local"/>, {@link
	  /// MethodWriter#COMPUTE_INSERTED_FRAMES}, or <seealso cref="MethodWriter.Compute_All_Frames"/>.
	  /// </summary>
	  private int _compute;

	  // -----------------------------------------------------------------------------------------------
	  // Constructor
	  // -----------------------------------------------------------------------------------------------

	  /// <summary>
	  /// Constructs a new <seealso cref="ClassWriter"/> object.
	  /// </summary>
	  /// <param name="flags"> option flags that can be used to modify the default behavior of this class. Must
	  ///     be zero or more of <seealso cref="Compute_Maxs"/> and <seealso cref="Compute_Frames"/>. </param>
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
	  ///     zero or more of <seealso cref="Compute_Maxs"/> and <seealso cref="Compute_Frames"/>. <i>These option flags do
	  ///     not affect methods that are copied as is in the new class. This means that neither the
	  ///     maximum stack size nor the stack frames will be computed for these methods</i>. </param>
	  public ClassWriter(ClassReader classReader, int flags) : base(IOpcodes.Asm9)
	  {
		_symbolTable = classReader == null ? new SymbolTable(this) : new SymbolTable(this, classReader);
		if ((flags & Compute_Frames) != 0)
		{
		  this._compute = MethodWriter.Compute_All_Frames;
		}
		else if ((flags & Compute_Maxs) != 0)
		{
		  this._compute = MethodWriter.Compute_Max_Stack_And_Local;
		}
		else
		{
		  this._compute = MethodWriter.Compute_Nothing;
		}
	  }

	  // -----------------------------------------------------------------------------------------------
	  // Implementation of the ClassVisitor abstract class
	  // -----------------------------------------------------------------------------------------------

	  public override sealed void Visit(int version, int access, string name, string signature, string superName, string[] interfaces)
	  {
		this._version = version;
		this._accessFlags = access;
		this._thisClass = _symbolTable.SetMajorVersionAndClassName(version & 0xFFFF, name);
		if (!string.ReferenceEquals(signature, null))
		{
		  this._signatureIndex = _symbolTable.AddConstantUtf8(signature);
		}
		this._superClass = string.ReferenceEquals(superName, null) ? 0 : _symbolTable.AddConstantClass(superName).index;
		if (interfaces != null && interfaces.Length > 0)
		{
		  _interfaceCount = interfaces.Length;
		  this._interfaces = new int[_interfaceCount];
		  for (int i = 0; i < _interfaceCount; ++i)
		  {
			this._interfaces[i] = _symbolTable.AddConstantClass(interfaces[i]).index;
		  }
		}
		if (_compute == MethodWriter.Compute_Max_Stack_And_Local && (version & 0xFFFF) >= IOpcodes.V1_7)
		{
		  _compute = MethodWriter.Compute_Max_Stack_And_Local_From_Frames;
		}
	  }

	  public override sealed void VisitSource(string file, string debug)
	  {
		if (!string.ReferenceEquals(file, null))
		{
		  _sourceFileIndex = _symbolTable.AddConstantUtf8(file);
		}
		if (!string.ReferenceEquals(debug, null))
		{
		  _debugExtension = (new ByteVector()).EncodeUtf8(debug, 0, int.MaxValue);
		}
	  }

	  public override sealed ModuleVisitor VisitModule(string name, int access, string version)
	  {
		return _moduleWriter = new ModuleWriter(_symbolTable, _symbolTable.AddConstantModule(name).index, access, string.ReferenceEquals(version, null) ? 0 : _symbolTable.AddConstantUtf8(version));
	  }

	  public override sealed void VisitNestHost(string nestHost)
	  {
		_nestHostClassIndex = _symbolTable.AddConstantClass(nestHost).index;
	  }

	  public override sealed void VisitOuterClass(string owner, string name, string descriptor)
	  {
		_enclosingClassIndex = _symbolTable.AddConstantClass(owner).index;
		if (!string.ReferenceEquals(name, null) && !string.ReferenceEquals(descriptor, null))
		{
		  _enclosingMethodIndex = _symbolTable.AddConstantNameAndType(name, descriptor);
		}
	  }

	  public override sealed AnnotationVisitor VisitAnnotation(string descriptor, bool visible)
	  {
		if (visible)
		{
		  return _lastRuntimeVisibleAnnotation = AnnotationWriter.Create(_symbolTable, descriptor, _lastRuntimeVisibleAnnotation);
		}
		else
		{
		  return _lastRuntimeInvisibleAnnotation = AnnotationWriter.Create(_symbolTable, descriptor, _lastRuntimeInvisibleAnnotation);
		}
	  }

	  public override sealed AnnotationVisitor VisitTypeAnnotation(int typeRef, TypePath typePath, string descriptor, bool visible)
	  {
		if (visible)
		{
		  return _lastRuntimeVisibleTypeAnnotation = AnnotationWriter.Create(_symbolTable, typeRef, typePath, descriptor, _lastRuntimeVisibleTypeAnnotation);
		}
		else
		{
		  return _lastRuntimeInvisibleTypeAnnotation = AnnotationWriter.Create(_symbolTable, typeRef, typePath, descriptor, _lastRuntimeInvisibleTypeAnnotation);
		}
	  }

	  public override sealed void VisitAttribute(Attribute attribute)
	  {
		// Store the attributes in the <i>reverse</i> order of their visit by this method.
		attribute.nextAttribute = _firstAttribute;
		_firstAttribute = attribute;
	  }

	  public override sealed void VisitNestMember(string nestMember)
	  {
		if (_nestMemberClasses == null)
		{
		  _nestMemberClasses = new ByteVector();
		}
		++_numberOfNestMemberClasses;
		_nestMemberClasses.PutShort(_symbolTable.AddConstantClass(nestMember).index);
	  }

	  public override sealed void VisitPermittedSubclass(string permittedSubclass)
	  {
		if (_permittedSubclasses == null)
		{
		  _permittedSubclasses = new ByteVector();
		}
		++_numberOfPermittedSubclasses;
		_permittedSubclasses.PutShort(_symbolTable.AddConstantClass(permittedSubclass).index);
	  }

	  public override sealed void VisitInnerClass(string name, string outerName, string innerName, int access)
	  {
		if (_innerClasses == null)
		{
		  _innerClasses = new ByteVector();
		}
		// Section 4.7.6 of the JVMS states "Every CONSTANT_Class_info entry in the constant_pool table
		// which represents a class or interface C that is not a package member must have exactly one
		// corresponding entry in the classes array". To avoid duplicates we keep track in the info
		// field of the Symbol of each CONSTANT_Class_info entry C whether an inner class entry has
		// already been added for C. If so, we store the index of this inner class entry (plus one) in
		// the info field. This trick allows duplicate detection in O(1) time.
		Symbol nameSymbol = _symbolTable.AddConstantClass(name);
		if (nameSymbol.info == 0)
		{
		  ++_numberOfInnerClasses;
		  _innerClasses.PutShort(nameSymbol.index);
		  _innerClasses.PutShort(string.ReferenceEquals(outerName, null) ? 0 : _symbolTable.AddConstantClass(outerName).index);
		  _innerClasses.PutShort(string.ReferenceEquals(innerName, null) ? 0 : _symbolTable.AddConstantUtf8(innerName));
		  _innerClasses.PutShort(access);
		  nameSymbol.info = _numberOfInnerClasses;
		}
		// Else, compare the inner classes entry nameSymbol.info - 1 with the arguments of this method
		// and throw an exception if there is a difference?
	  }

	  public override sealed RecordComponentVisitor VisitRecordComponent(string name, string descriptor, string signature)
	  {
		RecordComponentWriter recordComponentWriter = new RecordComponentWriter(_symbolTable, name, descriptor, signature);
		if (_firstRecordComponent == null)
		{
		  _firstRecordComponent = recordComponentWriter;
		}
		else
		{
		  _lastRecordComponent.@delegate = recordComponentWriter;
		}
		return _lastRecordComponent = recordComponentWriter;
	  }

	  public override sealed FieldVisitor VisitField(int access, string name, string descriptor, string signature, object value)
	  {
		FieldWriter fieldWriter = new FieldWriter(_symbolTable, access, name, descriptor, signature, value);
		if (_firstField == null)
		{
		  _firstField = fieldWriter;
		}
		else
		{
		  _lastField.fv = fieldWriter;
		}
		return _lastField = fieldWriter;
	  }

	  public override sealed MethodVisitor VisitMethod(int access, string name, string descriptor, string signature, string[] exceptions)
	  {
		MethodWriter methodWriter = new MethodWriter(_symbolTable, access, name, descriptor, signature, exceptions, _compute);
		if (_firstMethod == null)
		{
		  _firstMethod = methodWriter;
		}
		else
		{
		  _lastMethod.mv = methodWriter;
		}
		return _lastMethod = methodWriter;
	  }

	  public override sealed void VisitEnd()
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
	  public virtual sbyte[] ToByteArray()
	  {
		// First step: compute the size in bytes of the ClassFile structure.
		// The magic field uses 4 bytes, 10 mandatory fields (minor_version, major_version,
		// constant_pool_count, access_flags, this_class, super_class, interfaces_count, fields_count,
		// methods_count and attributes_count) use 2 bytes each, and each interface uses 2 bytes too.
		int size = 24 + 2 * _interfaceCount;
		int fieldsCount = 0;
		FieldWriter fieldWriter = _firstField;
		while (fieldWriter != null)
		{
		  ++fieldsCount;
		  size += fieldWriter.ComputeFieldInfoSize();
		  fieldWriter = (FieldWriter) fieldWriter.fv;
		}
		int methodsCount = 0;
		MethodWriter methodWriter = _firstMethod;
		while (methodWriter != null)
		{
		  ++methodsCount;
		  size += methodWriter.ComputeMethodInfoSize();
		  methodWriter = (MethodWriter) methodWriter.mv;
		}

		// For ease of reference, we use here the same attribute order as in Section 4.7 of the JVMS.
		int attributesCount = 0;
		if (_innerClasses != null)
		{
		  ++attributesCount;
		  size += 8 + _innerClasses.length;
		  _symbolTable.AddConstantUtf8(Constants.Inner_Classes);
		}
		if (_enclosingClassIndex != 0)
		{
		  ++attributesCount;
		  size += 10;
		  _symbolTable.AddConstantUtf8(Constants.Enclosing_Method);
		}
		if ((_accessFlags & IOpcodes.Acc_Synthetic) != 0 && (_version & 0xFFFF) < IOpcodes.V1_5)
		{
		  ++attributesCount;
		  size += 6;
		  _symbolTable.AddConstantUtf8(Constants.Synthetic);
		}
		if (_signatureIndex != 0)
		{
		  ++attributesCount;
		  size += 8;
		  _symbolTable.AddConstantUtf8(Constants.Signature);
		}
		if (_sourceFileIndex != 0)
		{
		  ++attributesCount;
		  size += 8;
		  _symbolTable.AddConstantUtf8(Constants.Source_File);
		}
		if (_debugExtension != null)
		{
		  ++attributesCount;
		  size += 6 + _debugExtension.length;
		  _symbolTable.AddConstantUtf8(Constants.Source_Debug_Extension);
		}
		if ((_accessFlags & IOpcodes.Acc_Deprecated) != 0)
		{
		  ++attributesCount;
		  size += 6;
		  _symbolTable.AddConstantUtf8(Constants.Deprecated);
		}
		if (_lastRuntimeVisibleAnnotation != null)
		{
		  ++attributesCount;
		  size += _lastRuntimeVisibleAnnotation.ComputeAnnotationsSize(Constants.Runtime_Visible_Annotations);
		}
		if (_lastRuntimeInvisibleAnnotation != null)
		{
		  ++attributesCount;
		  size += _lastRuntimeInvisibleAnnotation.ComputeAnnotationsSize(Constants.Runtime_Invisible_Annotations);
		}
		if (_lastRuntimeVisibleTypeAnnotation != null)
		{
		  ++attributesCount;
		  size += _lastRuntimeVisibleTypeAnnotation.ComputeAnnotationsSize(Constants.Runtime_Visible_Type_Annotations);
		}
		if (_lastRuntimeInvisibleTypeAnnotation != null)
		{
		  ++attributesCount;
		  size += _lastRuntimeInvisibleTypeAnnotation.ComputeAnnotationsSize(Constants.Runtime_Invisible_Type_Annotations);
		}
		if (_symbolTable.ComputeBootstrapMethodsSize() > 0)
		{
		  ++attributesCount;
		  size += _symbolTable.ComputeBootstrapMethodsSize();
		}
		if (_moduleWriter != null)
		{
		  attributesCount += _moduleWriter.AttributeCount;
		  size += _moduleWriter.ComputeAttributesSize();
		}
		if (_nestHostClassIndex != 0)
		{
		  ++attributesCount;
		  size += 8;
		  _symbolTable.AddConstantUtf8(Constants.Nest_Host);
		}
		if (_nestMemberClasses != null)
		{
		  ++attributesCount;
		  size += 8 + _nestMemberClasses.length;
		  _symbolTable.AddConstantUtf8(Constants.Nest_Members);
		}
		if (_permittedSubclasses != null)
		{
		  ++attributesCount;
		  size += 8 + _permittedSubclasses.length;
		  _symbolTable.AddConstantUtf8(Constants.Permitted_Subclasses);
		}
		int recordComponentCount = 0;
		int recordSize = 0;
		if ((_accessFlags & IOpcodes.Acc_Record) != 0 || _firstRecordComponent != null)
		{
		  RecordComponentWriter recordComponentWriter = _firstRecordComponent;
		  while (recordComponentWriter != null)
		  {
			++recordComponentCount;
			recordSize += recordComponentWriter.ComputeRecordComponentInfoSize();
			recordComponentWriter = (RecordComponentWriter) recordComponentWriter.@delegate;
		  }
		  ++attributesCount;
		  size += 8 + recordSize;
		  _symbolTable.AddConstantUtf8(Constants.Record);
		}
		if (_firstAttribute != null)
		{
		  attributesCount += _firstAttribute.AttributeCount;
		  size += _firstAttribute.ComputeAttributesSize(_symbolTable);
		}
		// IMPORTANT: this must be the last part of the ClassFile size computation, because the previous
		// statements can add attribute names to the constant pool, thereby changing its size!
		size += _symbolTable.ConstantPoolLength;
		int constantPoolCount = _symbolTable.ConstantPoolCount;
		if (constantPoolCount > 0xFFFF)
		{
		  throw new ClassTooLargeException(_symbolTable.ClassName, constantPoolCount);
		}

		// Second step: allocate a ByteVector of the correct size (in order to avoid any array copy in
		// dynamic resizes) and fill it with the ClassFile content.
		ByteVector result = new ByteVector(size);
		result.PutInt(unchecked((int)0xCAFEBABE)).PutInt(_version);
		_symbolTable.PutConstantPool(result);
		int mask = (_version & 0xFFFF) < IOpcodes.V1_5 ? IOpcodes.Acc_Synthetic : 0;
		result.PutShort(_accessFlags & ~mask).PutShort(_thisClass).PutShort(_superClass);
		result.PutShort(_interfaceCount);
		for (int i = 0; i < _interfaceCount; ++i)
		{
		  result.PutShort(_interfaces[i]);
		}
		result.PutShort(fieldsCount);
		fieldWriter = _firstField;
		while (fieldWriter != null)
		{
		  fieldWriter.PutFieldInfo(result);
		  fieldWriter = (FieldWriter) fieldWriter.fv;
		}
		result.PutShort(methodsCount);
		bool hasFrames = false;
		bool hasAsmInstructions = false;
		methodWriter = _firstMethod;
		while (methodWriter != null)
		{
		  hasFrames |= methodWriter.HasFrames();
		  hasAsmInstructions |= methodWriter.HasAsmInstructions();
		  methodWriter.PutMethodInfo(result);
		  methodWriter = (MethodWriter) methodWriter.mv;
		}
		// For ease of reference, we use here the same attribute order as in Section 4.7 of the JVMS.
		result.PutShort(attributesCount);
		if (_innerClasses != null)
		{
		  result.PutShort(_symbolTable.AddConstantUtf8(Constants.Inner_Classes)).PutInt(_innerClasses.length + 2).PutShort(_numberOfInnerClasses).PutByteArray(_innerClasses.data, 0, _innerClasses.length);
		}
		if (_enclosingClassIndex != 0)
		{
		  result.PutShort(_symbolTable.AddConstantUtf8(Constants.Enclosing_Method)).PutInt(4).PutShort(_enclosingClassIndex).PutShort(_enclosingMethodIndex);
		}
		if ((_accessFlags & IOpcodes.Acc_Synthetic) != 0 && (_version & 0xFFFF) < IOpcodes.V1_5)
		{
		  result.PutShort(_symbolTable.AddConstantUtf8(Constants.Synthetic)).PutInt(0);
		}
		if (_signatureIndex != 0)
		{
		  result.PutShort(_symbolTable.AddConstantUtf8(Constants.Signature)).PutInt(2).PutShort(_signatureIndex);
		}
		if (_sourceFileIndex != 0)
		{
		  result.PutShort(_symbolTable.AddConstantUtf8(Constants.Source_File)).PutInt(2).PutShort(_sourceFileIndex);
		}
		if (_debugExtension != null)
		{
		  int length = _debugExtension.length;
		  result.PutShort(_symbolTable.AddConstantUtf8(Constants.Source_Debug_Extension)).PutInt(length).PutByteArray(_debugExtension.data, 0, length);
		}
		if ((_accessFlags & IOpcodes.Acc_Deprecated) != 0)
		{
		  result.PutShort(_symbolTable.AddConstantUtf8(Constants.Deprecated)).PutInt(0);
		}
		AnnotationWriter.PutAnnotations(_symbolTable, _lastRuntimeVisibleAnnotation, _lastRuntimeInvisibleAnnotation, _lastRuntimeVisibleTypeAnnotation, _lastRuntimeInvisibleTypeAnnotation, result);
		_symbolTable.PutBootstrapMethods(result);
		if (_moduleWriter != null)
		{
		  _moduleWriter.PutAttributes(result);
		}
		if (_nestHostClassIndex != 0)
		{
		  result.PutShort(_symbolTable.AddConstantUtf8(Constants.Nest_Host)).PutInt(2).PutShort(_nestHostClassIndex);
		}
		if (_nestMemberClasses != null)
		{
		  result.PutShort(_symbolTable.AddConstantUtf8(Constants.Nest_Members)).PutInt(_nestMemberClasses.length + 2).PutShort(_numberOfNestMemberClasses).PutByteArray(_nestMemberClasses.data, 0, _nestMemberClasses.length);
		}
		if (_permittedSubclasses != null)
		{
		  result.PutShort(_symbolTable.AddConstantUtf8(Constants.Permitted_Subclasses)).PutInt(_permittedSubclasses.length + 2).PutShort(_numberOfPermittedSubclasses).PutByteArray(_permittedSubclasses.data, 0, _permittedSubclasses.length);
		}
		if ((_accessFlags & IOpcodes.Acc_Record) != 0 || _firstRecordComponent != null)
		{
		  result.PutShort(_symbolTable.AddConstantUtf8(Constants.Record)).PutInt(recordSize + 2).PutShort(recordComponentCount);
		  RecordComponentWriter recordComponentWriter = _firstRecordComponent;
		  while (recordComponentWriter != null)
		  {
			recordComponentWriter.PutRecordComponentInfo(result);
			recordComponentWriter = (RecordComponentWriter) recordComponentWriter.@delegate;
		  }
		}
		if (_firstAttribute != null)
		{
		  _firstAttribute.PutAttributes(_symbolTable, result);
		}

		// Third step: replace the ASM specific instructions, if any.
		if (hasAsmInstructions)
		{
		  return ReplaceAsmInstructions(result.data, hasFrames);
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
	  private sbyte[] ReplaceAsmInstructions(sbyte[] classFile, bool hasFrames)
	  {
		Attribute[] attributes = AttributePrototypes;
		_firstField = null;
		_lastField = null;
		_firstMethod = null;
		_lastMethod = null;
		_lastRuntimeVisibleAnnotation = null;
		_lastRuntimeInvisibleAnnotation = null;
		_lastRuntimeVisibleTypeAnnotation = null;
		_lastRuntimeInvisibleTypeAnnotation = null;
		_moduleWriter = null;
		_nestHostClassIndex = 0;
		_numberOfNestMemberClasses = 0;
		_nestMemberClasses = null;
		_numberOfPermittedSubclasses = 0;
		_permittedSubclasses = null;
		_firstRecordComponent = null;
		_lastRecordComponent = null;
		_firstAttribute = null;
		_compute = hasFrames ? MethodWriter.Compute_Inserted_Frames : MethodWriter.Compute_Nothing;
		(new ClassReader(classFile, 0, false)).Accept(this, attributes, (hasFrames ? ClassReader.Expand_Frames : 0) | ClassReader.Expand_Asm_Insns);
		return ToByteArray();
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
			attributePrototypes.AddAttributes(_firstAttribute);
			FieldWriter fieldWriter = _firstField;
			while (fieldWriter != null)
			{
			  fieldWriter.CollectAttributePrototypes(attributePrototypes);
			  fieldWriter = (FieldWriter) fieldWriter.fv;
			}
			MethodWriter methodWriter = _firstMethod;
			while (methodWriter != null)
			{
			  methodWriter.CollectAttributePrototypes(attributePrototypes);
			  methodWriter = (MethodWriter) methodWriter.mv;
			}
			RecordComponentWriter recordComponentWriter = _firstRecordComponent;
			while (recordComponentWriter != null)
			{
			  recordComponentWriter.CollectAttributePrototypes(attributePrototypes);
			  recordComponentWriter = (RecordComponentWriter) recordComponentWriter.@delegate;
			}
			return attributePrototypes.ToArray();
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
	  public virtual int NewConst(object value)
	  {
		return _symbolTable.AddConstant(value).index;
	  }

	  /// <summary>
	  /// Adds an UTF8 string to the constant pool of the class being build. Does nothing if the constant
	  /// pool already contains a similar item. <i>This method is intended for <seealso cref="Attribute"/> sub
	  /// classes, and is normally not needed by class generators or adapters.</i>
	  /// </summary>
	  /// <param name="value"> the String value. </param>
	  /// <returns> the index of a new or already existing UTF8 item. </returns>
	  // DontCheck(AbbreviationAsWordInName): can't be renamed (for backward binary compatibility).
	  public virtual int NewUtf8(string value)
	  {
		return _symbolTable.AddConstantUtf8(value);
	  }

	  /// <summary>
	  /// Adds a class reference to the constant pool of the class being build. Does nothing if the
	  /// constant pool already contains a similar item. <i>This method is intended for <seealso cref="Attribute"/>
	  /// sub classes, and is normally not needed by class generators or adapters.</i>
	  /// </summary>
	  /// <param name="value"> the internal name of the class. </param>
	  /// <returns> the index of a new or already existing class reference item. </returns>
	  public virtual int NewClass(string value)
	  {
		return _symbolTable.AddConstantClass(value).index;
	  }

	  /// <summary>
	  /// Adds a method type reference to the constant pool of the class being build. Does nothing if the
	  /// constant pool already contains a similar item. <i>This method is intended for <seealso cref="Attribute"/>
	  /// sub classes, and is normally not needed by class generators or adapters.</i>
	  /// </summary>
	  /// <param name="methodDescriptor"> method descriptor of the method type. </param>
	  /// <returns> the index of a new or already existing method type reference item. </returns>
	  public virtual int NewMethodType(string methodDescriptor)
	  {
		return _symbolTable.AddConstantMethodType(methodDescriptor).index;
	  }

	  /// <summary>
	  /// Adds a module reference to the constant pool of the class being build. Does nothing if the
	  /// constant pool already contains a similar item. <i>This method is intended for <seealso cref="Attribute"/>
	  /// sub classes, and is normally not needed by class generators or adapters.</i>
	  /// </summary>
	  /// <param name="moduleName"> name of the module. </param>
	  /// <returns> the index of a new or already existing module reference item. </returns>
	  public virtual int NewModule(string moduleName)
	  {
		return _symbolTable.AddConstantModule(moduleName).index;
	  }

	  /// <summary>
	  /// Adds a package reference to the constant pool of the class being build. Does nothing if the
	  /// constant pool already contains a similar item. <i>This method is intended for <seealso cref="Attribute"/>
	  /// sub classes, and is normally not needed by class generators or adapters.</i>
	  /// </summary>
	  /// <param name="packageName"> name of the package in its internal form. </param>
	  /// <returns> the index of a new or already existing module reference item. </returns>
	  public virtual int NewPackage(string packageName)
	  {
		return _symbolTable.AddConstantPackage(packageName).index;
	  }

	  /// <summary>
	  /// Adds a handle to the constant pool of the class being build. Does nothing if the constant pool
	  /// already contains a similar item. <i>This method is intended for <seealso cref="Attribute"/> sub classes,
	  /// and is normally not needed by class generators or adapters.</i>
	  /// </summary>
	  /// <param name="tag"> the kind of this handle. Must be <seealso cref="IIOpcodes.H_Getfield/>, {@link
	  ///     Opcodes#H_GETSTATIC}, <seealso cref="IIOpcodes.H_Putfield/>, <seealso cref="IIOpcodes.H_Putstatic/>, {@link
	  ///     Opcodes#H_INVOKEVIRTUAL}, <seealso cref="IIOpcodes.H_Invokestatic/>, <seealso cref="IIOpcodes.H_Invokespecial/>,
	  ///     <seealso cref="IIOpcodes.H_Newinvokespecial/> or <seealso cref="IIOpcodes.H_Invokeinterface/>. </param>
	  /// <param name="owner"> the internal name of the field or method owner class. </param>
	  /// <param name="name"> the name of the field or method. </param>
	  /// <param name="descriptor"> the descriptor of the field or method. </param>
	  /// <returns> the index of a new or already existing method type reference item. </returns>
	  /// @deprecated this method is superseded by {@link #newHandle(int, String, String, String,
	  ///     boolean)}. 
	  [Obsolete("this method is superseded by {@link #newHandle(int, String, String, String,")]
	  public virtual int NewHandle(int tag, string owner, string name, string descriptor)
	  {
		return NewHandle(tag, owner, name, descriptor, tag == IOpcodes.H_Invokeinterface);
	  }

	  /// <summary>
	  /// Adds a handle to the constant pool of the class being build. Does nothing if the constant pool
	  /// already contains a similar item. <i>This method is intended for <seealso cref="Attribute"/> sub classes,
	  /// and is normally not needed by class generators or adapters.</i>
	  /// </summary>
	  /// <param name="tag"> the kind of this handle. Must be <seealso cref="IIOpcodes.H_Getfield/>, {@link
	  ///     Opcodes#H_GETSTATIC}, <seealso cref="IIOpcodes.H_Putfield/>, <seealso cref="IIOpcodes.H_Putstatic/>, {@link
	  ///     Opcodes#H_INVOKEVIRTUAL}, <seealso cref="IIOpcodes.H_Invokestatic/>, <seealso cref="IIOpcodes.H_Invokespecial/>,
	  ///     <seealso cref="IIOpcodes.H_Newinvokespecial/> or <seealso cref="IIOpcodes.H_Invokeinterface/>. </param>
	  /// <param name="owner"> the internal name of the field or method owner class. </param>
	  /// <param name="name"> the name of the field or method. </param>
	  /// <param name="descriptor"> the descriptor of the field or method. </param>
	  /// <param name="isInterface"> true if the owner is an interface. </param>
	  /// <returns> the index of a new or already existing method type reference item. </returns>
	  public virtual int NewHandle(int tag, string owner, string name, string descriptor, bool isInterface)
	  {
		return _symbolTable.AddConstantMethodHandle(tag, owner, name, descriptor, isInterface).index;
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
	  public virtual int NewConstantDynamic(string name, string descriptor, Handle bootstrapMethodHandle, params object[] bootstrapMethodArguments)
	  {
		return _symbolTable.AddConstantDynamic(name, descriptor, bootstrapMethodHandle, bootstrapMethodArguments).index;
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
	  public virtual int NewInvokeDynamic(string name, string descriptor, Handle bootstrapMethodHandle, params object[] bootstrapMethodArguments)
	  {
		return _symbolTable.AddConstantInvokeDynamic(name, descriptor, bootstrapMethodHandle, bootstrapMethodArguments).index;
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
	  public virtual int NewField(string owner, string name, string descriptor)
	  {
		return _symbolTable.AddConstantFieldref(owner, name, descriptor).index;
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
	  public virtual int NewMethod(string owner, string name, string descriptor, bool isInterface)
	  {
		return _symbolTable.AddConstantMethodref(owner, name, descriptor, isInterface).index;
	  }

	  /// <summary>
	  /// Adds a name and type to the constant pool of the class being build. Does nothing if the
	  /// constant pool already contains a similar item. <i>This method is intended for <seealso cref="Attribute"/>
	  /// sub classes, and is normally not needed by class generators or adapters.</i>
	  /// </summary>
	  /// <param name="name"> a name. </param>
	  /// <param name="descriptor"> a type descriptor. </param>
	  /// <returns> the index of a new or already existing name and type item. </returns>
	  public virtual int NewNameType(string name, string descriptor)
	  {
		return _symbolTable.AddConstantNameAndType(name, descriptor);
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
	  public virtual string GetCommonSuperClass(string type1, string type2)
	  {
		  return "java/lang/Object";
	  }
    }

}