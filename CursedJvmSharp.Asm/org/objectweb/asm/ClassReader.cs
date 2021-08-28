using org.objectweb.asm;
using CursedJvmSharp.Asm.Java.IO;
using System;
using System.IO;
using System.Runtime.CompilerServices;

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
	/// A parser to make a <seealso cref="ClassVisitor"/> visit a ClassFile structure, as defined in the Java
	/// Virtual Machine Specification (JVMS). This class parses the ClassFile content and calls the
	/// appropriate visit methods of a given <seealso cref="ClassVisitor"/> for each field, method and bytecode
	/// instruction encountered.
	/// </summary>
	/// <seealso cref= <a href="https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html">JVMS 4</a>
	/// @author Eric Bruneton
	/// @author Eugene Kuleshov </seealso>
	public class ClassReader
	{

	  /// <summary>
	  /// A flag to skip the Code attributes. If this flag is set the Code attributes are neither parsed
	  /// nor visited.
	  /// </summary>
	  public const int SKIP_CODE = 1;

	  /// <summary>
	  /// A flag to skip the SourceFile, SourceDebugExtension, LocalVariableTable,
	  /// LocalVariableTypeTable, LineNumberTable and MethodParameters attributes. If this flag is set
	  /// these attributes are neither parsed nor visited (i.e. <seealso cref="ClassVisitor.visitSource"/>, {@link
	  /// MethodVisitor#visitLocalVariable}, <seealso cref="MethodVisitor.visitLineNumber"/> and {@link
	  /// MethodVisitor#visitParameter} are not called).
	  /// </summary>
	  public const int SKIP_DEBUG = 2;

	  /// <summary>
	  /// A flag to skip the StackMap and StackMapTable attributes. If this flag is set these attributes
	  /// are neither parsed nor visited (i.e. <seealso cref="MethodVisitor.visitFrame"/> is not called). This flag
	  /// is useful when the <seealso cref="ClassWriter.COMPUTE_FRAMES"/> option is used: it avoids visiting frames
	  /// that will be ignored and recomputed from scratch.
	  /// </summary>
	  public const int SKIP_FRAMES = 4;

	  /// <summary>
	  /// A flag to expand the stack map frames. By default stack map frames are visited in their
	  /// original format (i.e. "expanded" for classes whose version is less than V1_6, and "compressed"
	  /// for the other classes). If this flag is set, stack map frames are always visited in expanded
	  /// format (this option adds a decompression/compression step in ClassReader and ClassWriter which
	  /// degrades performance quite a lot).
	  /// </summary>
	  public const int EXPAND_FRAMES = 8;

	  /// <summary>
	  /// A flag to expand the ASM specific instructions into an equivalent sequence of standard bytecode
	  /// instructions. When resolving a forward jump it may happen that the signed 2 bytes offset
	  /// reserved for it is not sufficient to store the bytecode offset. In this case the jump
	  /// instruction is replaced with a temporary ASM specific instruction using an unsigned 2 bytes
	  /// offset (see <seealso cref="Label.resolve"/>). This internal flag is used to re-read classes containing
	  /// such instructions, in order to replace them with standard instructions. In addition, when this
	  /// flag is used, goto_w and jsr_w are <i>not</i> converted into goto and jsr, to make sure that
	  /// infinite loops where a goto_w is replaced with a goto in ClassReader and converted back to a
	  /// goto_w in ClassWriter cannot occur.
	  /// </summary>
	  internal const int EXPAND_ASM_INSNS = 256;

	  /// <summary>
	  /// The maximum size of array to allocate. </summary>
	  private const int MAX_BUFFER_SIZE = 1024 * 1024;

	  /// <summary>
	  /// The size of the temporary byte array used to read class input streams chunk by chunk. </summary>
	  private const int INPUT_STREAM_DATA_CHUNK_SIZE = 4096;

	  /// <summary>
	  /// A byte array containing the JVMS ClassFile structure to be parsed.
	  /// </summary>
	  /// @deprecated Use <seealso cref="readByte(int)"/> and the other read methods instead. This field will
	  ///     eventually be deleted. 
	  [Obsolete("Use <seealso cref=\"readByte(int)\"/> and the other read methods instead. This field will")]
	  public readonly sbyte[] b;

	  /// <summary>
	  /// The offset in bytes of the ClassFile's access_flags field. </summary>
	  public readonly int header;

	  /// <summary>
	  /// A byte array containing the JVMS ClassFile structure to be parsed. <i>The content of this array
	  /// must not be modified. This field is intended for <seealso cref="Attribute"/> sub classes, and is normally
	  /// not needed by class visitors.</i>
	  /// 
	  /// <para>NOTE: the ClassFile structure can start at any offset within this array, i.e. it does not
	  /// necessarily start at offset 0. Use <seealso cref="getItem"/> and <seealso cref="header"/> to get correct
	  /// ClassFile element offsets within this byte array.
	  /// </para>
	  /// </summary>
	  internal readonly sbyte[] classFileBuffer;

	  /// <summary>
	  /// The offset in bytes, in <seealso cref="classFileBuffer"/>, of each cp_info entry of the ClassFile's
	  /// constant_pool array, <i>plus one</i>. In other words, the offset of constant pool entry i is
	  /// given by cpInfoOffsets[i] - 1, i.e. its cp_info's tag field is given by b[cpInfoOffsets[i] -
	  /// 1].
	  /// </summary>
	  private readonly int[] cpInfoOffsets;

	  /// <summary>
	  /// The String objects corresponding to the CONSTANT_Utf8 constant pool items. This cache avoids
	  /// multiple parsing of a given CONSTANT_Utf8 constant pool item.
	  /// </summary>
	  private readonly string[] constantUtf8Values;

	  /// <summary>
	  /// The ConstantDynamic objects corresponding to the CONSTANT_Dynamic constant pool items. This
	  /// cache avoids multiple parsing of a given CONSTANT_Dynamic constant pool item.
	  /// </summary>
	  private readonly ConstantDynamic[] constantDynamicValues;

	  /// <summary>
	  /// The start offsets in <seealso cref="classFileBuffer"/> of each element of the bootstrap_methods array
	  /// (in the BootstrapMethods attribute).
	  /// </summary>
	  /// <seealso cref= <a href="https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.7.23">JVMS
	  ///     4.7.23</a> </seealso>
	  private readonly int[] bootstrapMethodOffsets;

	  /// <summary>
	  /// A conservative estimate of the maximum length of the strings contained in the constant pool of
	  /// the class.
	  /// </summary>
	  private readonly int maxStringLength;

	  // -----------------------------------------------------------------------------------------------
	  // Constructors
	  // -----------------------------------------------------------------------------------------------

	  /// <summary>
	  /// Constructs a new <seealso cref="ClassReader"/> object.
	  /// </summary>
	  /// <param name="classFile"> the JVMS ClassFile structure to be read. </param>
	  public ClassReader(sbyte[] classFile) : this(classFile, 0, classFile.Length)
	  {
	  }

	  /// <summary>
	  /// Constructs a new <seealso cref="ClassReader"/> object.
	  /// </summary>
	  /// <param name="classFileBuffer"> a byte array containing the JVMS ClassFile structure to be read. </param>
	  /// <param name="classFileOffset"> the offset in byteBuffer of the first byte of the ClassFile to be read. </param>
	  /// <param name="classFileLength"> the length in bytes of the ClassFile to be read. </param>
	  public ClassReader(sbyte[] classFileBuffer, int classFileOffset, int classFileLength) : this(classFileBuffer, classFileOffset, true)
	  { // NOPMD(UnusedFormalParameter) used for backward compatibility.
	  }

	  /// <summary>
	  /// Constructs a new <seealso cref="ClassReader"/> object. <i>This internal constructor must not be exposed
	  /// as a public API</i>.
	  /// </summary>
	  /// <param name="classFileBuffer"> a byte array containing the JVMS ClassFile structure to be read. </param>
	  /// <param name="classFileOffset"> the offset in byteBuffer of the first byte of the ClassFile to be read. </param>
	  /// <param name="checkClassVersion"> whether to check the class version or not. </param>
	  public ClassReader(sbyte[] classFileBuffer, int classFileOffset, bool checkClassVersion)
	  {
		this.classFileBuffer = classFileBuffer;
		this.b = classFileBuffer;
		// Check the class' major_version. This field is after the magic and minor_version fields, which
		// use 4 and 2 bytes respectively.
		if (checkClassVersion && readShort(classFileOffset + 6) > Opcodes.V18)
		{
		  throw new System.ArgumentException("Unsupported class file major version " + readShort(classFileOffset + 6));
		}
		// Create the constant pool arrays. The constant_pool_count field is after the magic,
		// minor_version and major_version fields, which use 4, 2 and 2 bytes respectively.
		int constantPoolCount = readUnsignedShort(classFileOffset + 8);
		cpInfoOffsets = new int[constantPoolCount];
		constantUtf8Values = new string[constantPoolCount];
		// Compute the offset of each constant pool entry, as well as a conservative estimate of the
		// maximum length of the constant pool strings. The first constant pool entry is after the
		// magic, minor_version, major_version and constant_pool_count fields, which use 4, 2, 2 and 2
		// bytes respectively.
		int currentCpInfoIndex = 1;
		int currentCpInfoOffset = classFileOffset + 10;
		int currentMaxStringLength = 0;
		bool hasBootstrapMethods = false;
		bool hasConstantDynamic = false;
		// The offset of the other entries depend on the total size of all the previous entries.
		while (currentCpInfoIndex < constantPoolCount)
		{
		  cpInfoOffsets[currentCpInfoIndex++] = currentCpInfoOffset + 1;
		  int cpInfoSize;
		  switch (classFileBuffer[currentCpInfoOffset])
		  {
			case Symbol.CONSTANT_FIELDREF_TAG:
			case Symbol.CONSTANT_METHODREF_TAG:
			case Symbol.CONSTANT_INTERFACE_METHODREF_TAG:
			case Symbol.CONSTANT_INTEGER_TAG:
			case Symbol.CONSTANT_FLOAT_TAG:
			case Symbol.CONSTANT_NAME_AND_TYPE_TAG:
			  cpInfoSize = 5;
			  break;
			case Symbol.CONSTANT_DYNAMIC_TAG:
			  cpInfoSize = 5;
			  hasBootstrapMethods = true;
			  hasConstantDynamic = true;
			  break;
			case Symbol.CONSTANT_INVOKE_DYNAMIC_TAG:
			  cpInfoSize = 5;
			  hasBootstrapMethods = true;
			  break;
			case Symbol.CONSTANT_LONG_TAG:
			case Symbol.CONSTANT_DOUBLE_TAG:
			  cpInfoSize = 9;
			  currentCpInfoIndex++;
			  break;
			case Symbol.CONSTANT_UTF8_TAG:
			  cpInfoSize = 3 + readUnsignedShort(currentCpInfoOffset + 1);
			  if (cpInfoSize > currentMaxStringLength)
			  {
				// The size in bytes of this CONSTANT_Utf8 structure provides a conservative estimate
				// of the length in characters of the corresponding string, and is much cheaper to
				// compute than this exact length.
				currentMaxStringLength = cpInfoSize;
			  }
			  break;
			case Symbol.CONSTANT_METHOD_HANDLE_TAG:
			  cpInfoSize = 4;
			  break;
			case Symbol.CONSTANT_CLASS_TAG:
			case Symbol.CONSTANT_STRING_TAG:
			case Symbol.CONSTANT_METHOD_TYPE_TAG:
			case Symbol.CONSTANT_PACKAGE_TAG:
			case Symbol.CONSTANT_MODULE_TAG:
			  cpInfoSize = 3;
			  break;
			default:
			  throw new System.ArgumentException();
		  }
		  currentCpInfoOffset += cpInfoSize;
		}
		maxStringLength = currentMaxStringLength;
		// The Classfile's access_flags field is just after the last constant pool entry.
		header = currentCpInfoOffset;

		// Allocate the cache of ConstantDynamic values, if there is at least one.
		constantDynamicValues = hasConstantDynamic ? new ConstantDynamic[constantPoolCount] : null;

		// Read the BootstrapMethods attribute, if any (only get the offset of each method).
		bootstrapMethodOffsets = hasBootstrapMethods ? readBootstrapMethodsAttribute(currentMaxStringLength) : null;
	  }

	  /// <summary>
	  /// Constructs a new <seealso cref="ClassReader"/> object.
	  /// </summary>
	  /// <param name="inputStream"> an input stream of the JVMS ClassFile structure to be read. This input
	  ///     stream must contain nothing more than the ClassFile structure itself. It is read from its
	  ///     current position to its end. </param>
	  /// <exception cref="IOException"> if a problem occurs during reading. </exception>
	  public ClassReader(Stream inputStream) : this(readStream(inputStream, false))
	  {
	  }

	  /// <summary>
	  /// Reads the given input stream and returns its content as a byte array.
	  /// </summary>
	  /// <param name="inputStream"> an input stream. </param>
	  /// <param name="close"> true to close the input stream after reading. </param>
	  /// <returns> the content of the given input stream. </returns>
	  /// <exception cref="IOException"> if a problem occurs during reading. </exception>
	  private static sbyte[] readStream(Stream inputStream, bool close)
	  {
		if (inputStream == null)
		{
		  throw new IOException("Class not found");
		}
		int bufferSize = calculateBufferSize(inputStream);
		try
		{
				using (MemoryStream outputStream = new MemoryStream())
				{
			  byte[] data = new byte[bufferSize];
			  int bytesRead;
			  int readCount = 0;
			  while ((bytesRead = inputStream.Read(data, 0, bufferSize)) != -1)
			  {
				outputStream.Write(data, 0, bytesRead);
				readCount++;
			  }
			  outputStream.Flush();
			  if (readCount == 1)
			  {
				return Unsafe.As<sbyte[]>(data);
			  }
			  return Unsafe.As<sbyte[]>(outputStream.ToArray());
				}
		}
		finally
		{
		  if (close)
		  {
			inputStream.Close();
		  }
		}
	  }

	  private static int calculateBufferSize(Stream inputStream)
      {
          long expectedLength = inputStream.Length;
		/*
		 * Some implementations can return 0 while holding available data
		 * (e.g. new FileInputStream("/proc/a_file"))
		 * Also in some pathological cases a very small number might be returned,
		 * and in this case we use default size
		 */
		if (expectedLength < 256)
		{
		  return INPUT_STREAM_DATA_CHUNK_SIZE;
		}
		return (int)Math.Min(expectedLength, MAX_BUFFER_SIZE);
	  }

	  // -----------------------------------------------------------------------------------------------
	  // Accessors
	  // -----------------------------------------------------------------------------------------------

	  /// <summary>
	  /// Returns the class's access flags (see <seealso cref="Opcodes"/>). This value may not reflect Deprecated
	  /// and Synthetic flags when bytecode is before 1.5 and those flags are represented by attributes.
	  /// </summary>
	  /// <returns> the class access flags. </returns>
	  /// <seealso cref= ClassVisitor#visit(int, int, String, String, String, String[]) </seealso>
	  public virtual int Access
	  {
		  get
		  {
			return readUnsignedShort(header);
		  }
	  }

	  /// <summary>
	  /// Returns the internal name of the class (see <seealso cref="Type.getInternalName()"/>).
	  /// </summary>
	  /// <returns> the internal class name. </returns>
	  /// <seealso cref= ClassVisitor#visit(int, int, String, String, String, String[]) </seealso>
	  public virtual string ClassName
	  {
		  get
		  {
			// this_class is just after the access_flags field (using 2 bytes).
			return readClass(header + 2, new char[maxStringLength]);
		  }
	  }

	  /// <summary>
	  /// Returns the internal of name of the super class (see <seealso cref="Type.getInternalName()"/>). For
	  /// interfaces, the super class is <seealso cref="object"/>.
	  /// </summary>
	  /// <returns> the internal name of the super class, or {@literal null} for <seealso cref="object"/> class. </returns>
	  /// <seealso cref= ClassVisitor#visit(int, int, String, String, String, String[]) </seealso>
	  public virtual string SuperName
	  {
		  get
		  {
			// super_class is after the access_flags and this_class fields (2 bytes each).
			return readClass(header + 4, new char[maxStringLength]);
		  }
	  }

	  /// <summary>
	  /// Returns the internal names of the implemented interfaces (see <seealso cref="Type.getInternalName()"/>).
	  /// </summary>
	  /// <returns> the internal names of the directly implemented interfaces. Inherited implemented
	  ///     interfaces are not returned. </returns>
	  /// <seealso cref= ClassVisitor#visit(int, int, String, String, String, String[]) </seealso>
	  public virtual string[] Interfaces
	  {
		  get
		  {
			// interfaces_count is after the access_flags, this_class and super_class fields (2 bytes each).
			int currentOffset = header + 6;
			int interfacesCount = readUnsignedShort(currentOffset);
			string[] interfaces = new string[interfacesCount];
			if (interfacesCount > 0)
			{
			  char[] charBuffer = new char[maxStringLength];
			  for (int i = 0; i < interfacesCount; ++i)
			  {
				currentOffset += 2;
				interfaces[i] = readClass(currentOffset, charBuffer);
			  }
			}
			return interfaces;
		  }
	  }

	  // -----------------------------------------------------------------------------------------------
	  // Public methods
	  // -----------------------------------------------------------------------------------------------

	  /// <summary>
	  /// Makes the given visitor visit the JVMS ClassFile structure passed to the constructor of this
	  /// <seealso cref="ClassReader"/>.
	  /// </summary>
	  /// <param name="classVisitor"> the visitor that must visit this class. </param>
	  /// <param name="parsingOptions"> the options to use to parse this class. One or more of {@link
	  ///     #SKIP_CODE}, <seealso cref="SKIP_DEBUG"/>, <seealso cref="SKIP_FRAMES"/> or <seealso cref="EXPAND_FRAMES"/>. </param>
	  public virtual void accept(ClassVisitor classVisitor, int parsingOptions)
	  {
		accept(classVisitor, new Attribute[0], parsingOptions);
	  }

	  /// <summary>
	  /// Makes the given visitor visit the JVMS ClassFile structure passed to the constructor of this
	  /// <seealso cref="ClassReader"/>.
	  /// </summary>
	  /// <param name="classVisitor"> the visitor that must visit this class. </param>
	  /// <param name="attributePrototypes"> prototypes of the attributes that must be parsed during the visit of
	  ///     the class. Any attribute whose type is not equal to the type of one the prototypes will not
	  ///     be parsed: its byte array value will be passed unchanged to the ClassWriter. <i>This may
	  ///     corrupt it if this value contains references to the constant pool, or has syntactic or
	  ///     semantic links with a class element that has been transformed by a class adapter between
	  ///     the reader and the writer</i>. </param>
	  /// <param name="parsingOptions"> the options to use to parse this class. One or more of {@link
	  ///     #SKIP_CODE}, <seealso cref="SKIP_DEBUG"/>, <seealso cref="SKIP_FRAMES"/> or <seealso cref="EXPAND_FRAMES"/>. </param>
	  public virtual void accept(ClassVisitor classVisitor, Attribute[] attributePrototypes, int parsingOptions)
	  {
		Context context = new Context();
		context.attributePrototypes = attributePrototypes;
		context.parsingOptions = parsingOptions;
		context.charBuffer = new char[maxStringLength];

		// Read the access_flags, this_class, super_class, interface_count and interfaces fields.
		char[] charBuffer = context.charBuffer;
		int currentOffset = header;
		int accessFlags = readUnsignedShort(currentOffset);
		string thisClass = readClass(currentOffset + 2, charBuffer);
		string superClass = readClass(currentOffset + 4, charBuffer);
		string[] interfaces = new string[readUnsignedShort(currentOffset + 6)];
		currentOffset += 8;
		for (int i = 0; i < interfaces.Length; ++i)
		{
		  interfaces[i] = readClass(currentOffset, charBuffer);
		  currentOffset += 2;
		}

		// Read the class attributes (the variables are ordered as in Section 4.7 of the JVMS).
		// Attribute offsets exclude the attribute_name_index and attribute_length fields.
		// - The offset of the InnerClasses attribute, or 0.
		int innerClassesOffset = 0;
		// - The offset of the EnclosingMethod attribute, or 0.
		int enclosingMethodOffset = 0;
		// - The string corresponding to the Signature attribute, or null.
		string signature = null;
		// - The string corresponding to the SourceFile attribute, or null.
		string sourceFile = null;
		// - The string corresponding to the SourceDebugExtension attribute, or null.
		string sourceDebugExtension = null;
		// - The offset of the RuntimeVisibleAnnotations attribute, or 0.
		int runtimeVisibleAnnotationsOffset = 0;
		// - The offset of the RuntimeInvisibleAnnotations attribute, or 0.
		int runtimeInvisibleAnnotationsOffset = 0;
		// - The offset of the RuntimeVisibleTypeAnnotations attribute, or 0.
		int runtimeVisibleTypeAnnotationsOffset = 0;
		// - The offset of the RuntimeInvisibleTypeAnnotations attribute, or 0.
		int runtimeInvisibleTypeAnnotationsOffset = 0;
		// - The offset of the Module attribute, or 0.
		int moduleOffset = 0;
		// - The offset of the ModulePackages attribute, or 0.
		int modulePackagesOffset = 0;
		// - The string corresponding to the ModuleMainClass attribute, or null.
		string moduleMainClass = null;
		// - The string corresponding to the NestHost attribute, or null.
		string nestHostClass = null;
		// - The offset of the NestMembers attribute, or 0.
		int nestMembersOffset = 0;
		// - The offset of the PermittedSubclasses attribute, or 0
		int permittedSubclassesOffset = 0;
		// - The offset of the Record attribute, or 0.
		int recordOffset = 0;
		// - The non standard attributes (linked with their {@link Attribute#nextAttribute} field).
		//   This list in the <i>reverse order</i> or their order in the ClassFile structure.
		Attribute attributes = null;

		int currentAttributeOffset = FirstAttributeOffset;
		for (int i = readUnsignedShort(currentAttributeOffset - 2); i > 0; --i)
		{
		  // Read the attribute_info's attribute_name and attribute_length fields.
		  string attributeName = readUTF8(currentAttributeOffset, charBuffer);
		  int attributeLength = readInt(currentAttributeOffset + 2);
		  currentAttributeOffset += 6;
		  // The tests are sorted in decreasing frequency order (based on frequencies observed on
		  // typical classes).
		  if (Constants.SOURCE_FILE.Equals(attributeName))
		  {
			sourceFile = readUTF8(currentAttributeOffset, charBuffer);
		  }
		  else if (Constants.INNER_CLASSES.Equals(attributeName))
		  {
			innerClassesOffset = currentAttributeOffset;
		  }
		  else if (Constants.ENCLOSING_METHOD.Equals(attributeName))
		  {
			enclosingMethodOffset = currentAttributeOffset;
		  }
		  else if (Constants.NEST_HOST.Equals(attributeName))
		  {
			nestHostClass = readClass(currentAttributeOffset, charBuffer);
		  }
		  else if (Constants.NEST_MEMBERS.Equals(attributeName))
		  {
			nestMembersOffset = currentAttributeOffset;
		  }
		  else if (Constants.PERMITTED_SUBCLASSES.Equals(attributeName))
		  {
			permittedSubclassesOffset = currentAttributeOffset;
		  }
		  else if (Constants.SIGNATURE.Equals(attributeName))
		  {
			signature = readUTF8(currentAttributeOffset, charBuffer);
		  }
		  else if (Constants.RUNTIME_VISIBLE_ANNOTATIONS.Equals(attributeName))
		  {
			runtimeVisibleAnnotationsOffset = currentAttributeOffset;
		  }
		  else if (Constants.RUNTIME_VISIBLE_TYPE_ANNOTATIONS.Equals(attributeName))
		  {
			runtimeVisibleTypeAnnotationsOffset = currentAttributeOffset;
		  }
		  else if (Constants.DEPRECATED.Equals(attributeName))
		  {
			accessFlags |= Opcodes.ACC_DEPRECATED;
		  }
		  else if (Constants.SYNTHETIC.Equals(attributeName))
		  {
			accessFlags |= Opcodes.ACC_SYNTHETIC;
		  }
		  else if (Constants.SOURCE_DEBUG_EXTENSION.Equals(attributeName))
		  {
			if (attributeLength > classFileBuffer.Length - currentAttributeOffset)
			{
			  throw new System.ArgumentException();
			}
			sourceDebugExtension = readUtf(currentAttributeOffset, attributeLength, new char[attributeLength]);
		  }
		  else if (Constants.RUNTIME_INVISIBLE_ANNOTATIONS.Equals(attributeName))
		  {
			runtimeInvisibleAnnotationsOffset = currentAttributeOffset;
		  }
		  else if (Constants.RUNTIME_INVISIBLE_TYPE_ANNOTATIONS.Equals(attributeName))
		  {
			runtimeInvisibleTypeAnnotationsOffset = currentAttributeOffset;
		  }
		  else if (Constants.RECORD.Equals(attributeName))
		  {
			recordOffset = currentAttributeOffset;
			accessFlags |= Opcodes.ACC_RECORD;
		  }
		  else if (Constants.MODULE.Equals(attributeName))
		  {
			moduleOffset = currentAttributeOffset;
		  }
		  else if (Constants.MODULE_MAIN_CLASS.Equals(attributeName))
		  {
			moduleMainClass = readClass(currentAttributeOffset, charBuffer);
		  }
		  else if (Constants.MODULE_PACKAGES.Equals(attributeName))
		  {
			modulePackagesOffset = currentAttributeOffset;
		  }
		  else if (!Constants.BOOTSTRAP_METHODS.Equals(attributeName))
		  {
			// The BootstrapMethods attribute is read in the constructor.
			Attribute attribute = readAttribute(attributePrototypes, attributeName, currentAttributeOffset, attributeLength, charBuffer, -1, null);
			attribute.nextAttribute = attributes;
			attributes = attribute;
		  }
		  currentAttributeOffset += attributeLength;
		}

		// Visit the class declaration. The minor_version and major_version fields start 6 bytes before
		// the first constant pool entry, which itself starts at cpInfoOffsets[1] - 1 (by definition).
		classVisitor.visit(readInt(cpInfoOffsets[1] - 7), accessFlags, thisClass, signature, superClass, interfaces);

		// Visit the SourceFile and SourceDebugExtenstion attributes.
		if ((parsingOptions & SKIP_DEBUG) == 0 && (!string.ReferenceEquals(sourceFile, null) || !string.ReferenceEquals(sourceDebugExtension, null)))
		{
		  classVisitor.visitSource(sourceFile, sourceDebugExtension);
		}

		// Visit the Module, ModulePackages and ModuleMainClass attributes.
		if (moduleOffset != 0)
		{
		  readModuleAttributes(classVisitor, context, moduleOffset, modulePackagesOffset, moduleMainClass);
		}

		// Visit the NestHost attribute.
		if (!string.ReferenceEquals(nestHostClass, null))
		{
		  classVisitor.visitNestHost(nestHostClass);
		}

		// Visit the EnclosingMethod attribute.
		if (enclosingMethodOffset != 0)
		{
		  string className = readClass(enclosingMethodOffset, charBuffer);
		  int methodIndex = readUnsignedShort(enclosingMethodOffset + 2);
		  string name = methodIndex == 0 ? null : readUTF8(cpInfoOffsets[methodIndex], charBuffer);
		  string type = methodIndex == 0 ? null : readUTF8(cpInfoOffsets[methodIndex] + 2, charBuffer);
		  classVisitor.visitOuterClass(className, name, type);
		}

		// Visit the RuntimeVisibleAnnotations attribute.
		if (runtimeVisibleAnnotationsOffset != 0)
		{
		  int numAnnotations = readUnsignedShort(runtimeVisibleAnnotationsOffset);
		  int currentAnnotationOffset = runtimeVisibleAnnotationsOffset + 2;
		  while (numAnnotations-- > 0)
		  {
			// Parse the type_index field.
			string annotationDescriptor = readUTF8(currentAnnotationOffset, charBuffer);
			currentAnnotationOffset += 2;
			// Parse num_element_value_pairs and element_value_pairs and visit these values.
			currentAnnotationOffset = readElementValues(classVisitor.visitAnnotation(annotationDescriptor, true), currentAnnotationOffset, true, charBuffer);
		  }
		}

		// Visit the RuntimeInvisibleAnnotations attribute.
		if (runtimeInvisibleAnnotationsOffset != 0)
		{
		  int numAnnotations = readUnsignedShort(runtimeInvisibleAnnotationsOffset);
		  int currentAnnotationOffset = runtimeInvisibleAnnotationsOffset + 2;
		  while (numAnnotations-- > 0)
		  {
			// Parse the type_index field.
			string annotationDescriptor = readUTF8(currentAnnotationOffset, charBuffer);
			currentAnnotationOffset += 2;
			// Parse num_element_value_pairs and element_value_pairs and visit these values.
			currentAnnotationOffset = readElementValues(classVisitor.visitAnnotation(annotationDescriptor, false), currentAnnotationOffset, true, charBuffer);
		  }
		}

		// Visit the RuntimeVisibleTypeAnnotations attribute.
		if (runtimeVisibleTypeAnnotationsOffset != 0)
		{
		  int numAnnotations = readUnsignedShort(runtimeVisibleTypeAnnotationsOffset);
		  int currentAnnotationOffset = runtimeVisibleTypeAnnotationsOffset + 2;
		  while (numAnnotations-- > 0)
		  {
			// Parse the target_type, target_info and target_path fields.
			currentAnnotationOffset = readTypeAnnotationTarget(context, currentAnnotationOffset);
			// Parse the type_index field.
			string annotationDescriptor = readUTF8(currentAnnotationOffset, charBuffer);
			currentAnnotationOffset += 2;
			// Parse num_element_value_pairs and element_value_pairs and visit these values.
			currentAnnotationOffset = readElementValues(classVisitor.visitTypeAnnotation(context.currentTypeAnnotationTarget, context.currentTypeAnnotationTargetPath, annotationDescriptor, true), currentAnnotationOffset, true, charBuffer);
		  }
		}

		// Visit the RuntimeInvisibleTypeAnnotations attribute.
		if (runtimeInvisibleTypeAnnotationsOffset != 0)
		{
		  int numAnnotations = readUnsignedShort(runtimeInvisibleTypeAnnotationsOffset);
		  int currentAnnotationOffset = runtimeInvisibleTypeAnnotationsOffset + 2;
		  while (numAnnotations-- > 0)
		  {
			// Parse the target_type, target_info and target_path fields.
			currentAnnotationOffset = readTypeAnnotationTarget(context, currentAnnotationOffset);
			// Parse the type_index field.
			string annotationDescriptor = readUTF8(currentAnnotationOffset, charBuffer);
			currentAnnotationOffset += 2;
			// Parse num_element_value_pairs and element_value_pairs and visit these values.
			currentAnnotationOffset = readElementValues(classVisitor.visitTypeAnnotation(context.currentTypeAnnotationTarget, context.currentTypeAnnotationTargetPath, annotationDescriptor, false), currentAnnotationOffset, true, charBuffer);
		  }
		}

		// Visit the non standard attributes.
		while (attributes != null)
		{
		  // Copy and reset the nextAttribute field so that it can also be used in ClassWriter.
		  Attribute nextAttribute = attributes.nextAttribute;
		  attributes.nextAttribute = null;
		  classVisitor.visitAttribute(attributes);
		  attributes = nextAttribute;
		}

		// Visit the NestedMembers attribute.
		if (nestMembersOffset != 0)
		{
		  int numberOfNestMembers = readUnsignedShort(nestMembersOffset);
		  int currentNestMemberOffset = nestMembersOffset + 2;
		  while (numberOfNestMembers-- > 0)
		  {
			classVisitor.visitNestMember(readClass(currentNestMemberOffset, charBuffer));
			currentNestMemberOffset += 2;
		  }
		}

		// Visit the PermittedSubclasses attribute.
		if (permittedSubclassesOffset != 0)
		{
		  int numberOfPermittedSubclasses = readUnsignedShort(permittedSubclassesOffset);
		  int currentPermittedSubclassesOffset = permittedSubclassesOffset + 2;
		  while (numberOfPermittedSubclasses-- > 0)
		  {
			classVisitor.visitPermittedSubclass(readClass(currentPermittedSubclassesOffset, charBuffer));
			currentPermittedSubclassesOffset += 2;
		  }
		}

		// Visit the InnerClasses attribute.
		if (innerClassesOffset != 0)
		{
		  int numberOfClasses = readUnsignedShort(innerClassesOffset);
		  int currentClassesOffset = innerClassesOffset + 2;
		  while (numberOfClasses-- > 0)
		  {
			classVisitor.visitInnerClass(readClass(currentClassesOffset, charBuffer), readClass(currentClassesOffset + 2, charBuffer), readUTF8(currentClassesOffset + 4, charBuffer), readUnsignedShort(currentClassesOffset + 6));
			currentClassesOffset += 8;
		  }
		}

		// Visit Record components.
		if (recordOffset != 0)
		{
		  int recordComponentsCount = readUnsignedShort(recordOffset);
		  recordOffset += 2;
		  while (recordComponentsCount-- > 0)
		  {
			recordOffset = readRecordComponent(classVisitor, context, recordOffset);
		  }
		}

		// Visit the fields and methods.
		int fieldsCount = readUnsignedShort(currentOffset);
		currentOffset += 2;
		while (fieldsCount-- > 0)
		{
		  currentOffset = readField(classVisitor, context, currentOffset);
		}
		int methodsCount = readUnsignedShort(currentOffset);
		currentOffset += 2;
		while (methodsCount-- > 0)
		{
		  currentOffset = readMethod(classVisitor, context, currentOffset);
		}

		// Visit the end of the class.
		classVisitor.visitEnd();
	  }

	  // ----------------------------------------------------------------------------------------------
	  // Methods to parse modules, fields and methods
	  // ----------------------------------------------------------------------------------------------

	  /// <summary>
	  /// Reads the Module, ModulePackages and ModuleMainClass attributes and visit them.
	  /// </summary>
	  /// <param name="classVisitor"> the current class visitor </param>
	  /// <param name="context"> information about the class being parsed. </param>
	  /// <param name="moduleOffset"> the offset of the Module attribute (excluding the attribute_info's
	  ///     attribute_name_index and attribute_length fields). </param>
	  /// <param name="modulePackagesOffset"> the offset of the ModulePackages attribute (excluding the
	  ///     attribute_info's attribute_name_index and attribute_length fields), or 0. </param>
	  /// <param name="moduleMainClass"> the string corresponding to the ModuleMainClass attribute, or {@literal
	  ///     null}. </param>
	  private void readModuleAttributes(ClassVisitor classVisitor, Context context, int moduleOffset, int modulePackagesOffset, string moduleMainClass)
	  {
		char[] buffer = context.charBuffer;

		// Read the module_name_index, module_flags and module_version_index fields and visit them.
		int currentOffset = moduleOffset;
		string moduleName = readModule(currentOffset, buffer);
		int moduleFlags = readUnsignedShort(currentOffset + 2);
		string moduleVersion = readUTF8(currentOffset + 4, buffer);
		currentOffset += 6;
		ModuleVisitor moduleVisitor = classVisitor.visitModule(moduleName, moduleFlags, moduleVersion);
		if (moduleVisitor == null)
		{
		  return;
		}

		// Visit the ModuleMainClass attribute.
		if (!string.ReferenceEquals(moduleMainClass, null))
		{
		  moduleVisitor.visitMainClass(moduleMainClass);
		}

		// Visit the ModulePackages attribute.
		if (modulePackagesOffset != 0)
		{
		  int packageCount = readUnsignedShort(modulePackagesOffset);
		  int currentPackageOffset = modulePackagesOffset + 2;
		  while (packageCount-- > 0)
		  {
			moduleVisitor.visitPackage(readPackage(currentPackageOffset, buffer));
			currentPackageOffset += 2;
		  }
		}

		// Read the 'requires_count' and 'requires' fields.
		int requiresCount = readUnsignedShort(currentOffset);
		currentOffset += 2;
		while (requiresCount-- > 0)
		{
		  // Read the requires_index, requires_flags and requires_version fields and visit them.
		  string requires = readModule(currentOffset, buffer);
		  int requiresFlags = readUnsignedShort(currentOffset + 2);
		  string requiresVersion = readUTF8(currentOffset + 4, buffer);
		  currentOffset += 6;
		  moduleVisitor.visitRequire(requires, requiresFlags, requiresVersion);
		}

		// Read the 'exports_count' and 'exports' fields.
		int exportsCount = readUnsignedShort(currentOffset);
		currentOffset += 2;
		while (exportsCount-- > 0)
		{
		  // Read the exports_index, exports_flags, exports_to_count and exports_to_index fields
		  // and visit them.
		  string exports = readPackage(currentOffset, buffer);
		  int exportsFlags = readUnsignedShort(currentOffset + 2);
		  int exportsToCount = readUnsignedShort(currentOffset + 4);
		  currentOffset += 6;
		  string[] exportsTo = null;
		  if (exportsToCount != 0)
		  {
			exportsTo = new string[exportsToCount];
			for (int i = 0; i < exportsToCount; ++i)
			{
			  exportsTo[i] = readModule(currentOffset, buffer);
			  currentOffset += 2;
			}
		  }
		  moduleVisitor.visitExport(exports, exportsFlags, exportsTo);
		}

		// Reads the 'opens_count' and 'opens' fields.
		int opensCount = readUnsignedShort(currentOffset);
		currentOffset += 2;
		while (opensCount-- > 0)
		{
		  // Read the opens_index, opens_flags, opens_to_count and opens_to_index fields and visit them.
		  string opens = readPackage(currentOffset, buffer);
		  int opensFlags = readUnsignedShort(currentOffset + 2);
		  int opensToCount = readUnsignedShort(currentOffset + 4);
		  currentOffset += 6;
		  string[] opensTo = null;
		  if (opensToCount != 0)
		  {
			opensTo = new string[opensToCount];
			for (int i = 0; i < opensToCount; ++i)
			{
			  opensTo[i] = readModule(currentOffset, buffer);
			  currentOffset += 2;
			}
		  }
		  moduleVisitor.visitOpen(opens, opensFlags, opensTo);
		}

		// Read the 'uses_count' and 'uses' fields.
		int usesCount = readUnsignedShort(currentOffset);
		currentOffset += 2;
		while (usesCount-- > 0)
		{
		  moduleVisitor.visitUse(readClass(currentOffset, buffer));
		  currentOffset += 2;
		}

		// Read the  'provides_count' and 'provides' fields.
		int providesCount = readUnsignedShort(currentOffset);
		currentOffset += 2;
		while (providesCount-- > 0)
		{
		  // Read the provides_index, provides_with_count and provides_with_index fields and visit them.
		  string provides = readClass(currentOffset, buffer);
		  int providesWithCount = readUnsignedShort(currentOffset + 2);
		  currentOffset += 4;
		  string[] providesWith = new string[providesWithCount];
		  for (int i = 0; i < providesWithCount; ++i)
		  {
			providesWith[i] = readClass(currentOffset, buffer);
			currentOffset += 2;
		  }
		  moduleVisitor.visitProvide(provides, providesWith);
		}

		// Visit the end of the module attributes.
		moduleVisitor.visitEnd();
	  }

	  /// <summary>
	  /// Reads a record component and visit it.
	  /// </summary>
	  /// <param name="classVisitor"> the current class visitor </param>
	  /// <param name="context"> information about the class being parsed. </param>
	  /// <param name="recordComponentOffset"> the offset of the current record component. </param>
	  /// <returns> the offset of the first byte following the record component. </returns>
	  private int readRecordComponent(ClassVisitor classVisitor, Context context, int recordComponentOffset)
	  {
		char[] charBuffer = context.charBuffer;

		int currentOffset = recordComponentOffset;
		string name = readUTF8(currentOffset, charBuffer);
		string descriptor = readUTF8(currentOffset + 2, charBuffer);
		currentOffset += 4;

		// Read the record component attributes (the variables are ordered as in Section 4.7 of the
		// JVMS).

		// Attribute offsets exclude the attribute_name_index and attribute_length fields.
		// - The string corresponding to the Signature attribute, or null.
		string signature = null;
		// - The offset of the RuntimeVisibleAnnotations attribute, or 0.
		int runtimeVisibleAnnotationsOffset = 0;
		// - The offset of the RuntimeInvisibleAnnotations attribute, or 0.
		int runtimeInvisibleAnnotationsOffset = 0;
		// - The offset of the RuntimeVisibleTypeAnnotations attribute, or 0.
		int runtimeVisibleTypeAnnotationsOffset = 0;
		// - The offset of the RuntimeInvisibleTypeAnnotations attribute, or 0.
		int runtimeInvisibleTypeAnnotationsOffset = 0;
		// - The non standard attributes (linked with their {@link Attribute#nextAttribute} field).
		//   This list in the <i>reverse order</i> or their order in the ClassFile structure.
		Attribute attributes = null;

		int attributesCount = readUnsignedShort(currentOffset);
		currentOffset += 2;
		while (attributesCount-- > 0)
		{
		  // Read the attribute_info's attribute_name and attribute_length fields.
		  string attributeName = readUTF8(currentOffset, charBuffer);
		  int attributeLength = readInt(currentOffset + 2);
		  currentOffset += 6;
		  // The tests are sorted in decreasing frequency order (based on frequencies observed on
		  // typical classes).
		  if (Constants.SIGNATURE.Equals(attributeName))
		  {
			signature = readUTF8(currentOffset, charBuffer);
		  }
		  else if (Constants.RUNTIME_VISIBLE_ANNOTATIONS.Equals(attributeName))
		  {
			runtimeVisibleAnnotationsOffset = currentOffset;
		  }
		  else if (Constants.RUNTIME_VISIBLE_TYPE_ANNOTATIONS.Equals(attributeName))
		  {
			runtimeVisibleTypeAnnotationsOffset = currentOffset;
		  }
		  else if (Constants.RUNTIME_INVISIBLE_ANNOTATIONS.Equals(attributeName))
		  {
			runtimeInvisibleAnnotationsOffset = currentOffset;
		  }
		  else if (Constants.RUNTIME_INVISIBLE_TYPE_ANNOTATIONS.Equals(attributeName))
		  {
			runtimeInvisibleTypeAnnotationsOffset = currentOffset;
		  }
		  else
		  {
			Attribute attribute = readAttribute(context.attributePrototypes, attributeName, currentOffset, attributeLength, charBuffer, -1, null);
			attribute.nextAttribute = attributes;
			attributes = attribute;
		  }
		  currentOffset += attributeLength;
		}

		RecordComponentVisitor recordComponentVisitor = classVisitor.visitRecordComponent(name, descriptor, signature);
		if (recordComponentVisitor == null)
		{
		  return currentOffset;
		}

		// Visit the RuntimeVisibleAnnotations attribute.
		if (runtimeVisibleAnnotationsOffset != 0)
		{
		  int numAnnotations = readUnsignedShort(runtimeVisibleAnnotationsOffset);
		  int currentAnnotationOffset = runtimeVisibleAnnotationsOffset + 2;
		  while (numAnnotations-- > 0)
		  {
			// Parse the type_index field.
			string annotationDescriptor = readUTF8(currentAnnotationOffset, charBuffer);
			currentAnnotationOffset += 2;
			// Parse num_element_value_pairs and element_value_pairs and visit these values.
			currentAnnotationOffset = readElementValues(recordComponentVisitor.visitAnnotation(annotationDescriptor, true), currentAnnotationOffset, true, charBuffer);
		  }
		}

		// Visit the RuntimeInvisibleAnnotations attribute.
		if (runtimeInvisibleAnnotationsOffset != 0)
		{
		  int numAnnotations = readUnsignedShort(runtimeInvisibleAnnotationsOffset);
		  int currentAnnotationOffset = runtimeInvisibleAnnotationsOffset + 2;
		  while (numAnnotations-- > 0)
		  {
			// Parse the type_index field.
			string annotationDescriptor = readUTF8(currentAnnotationOffset, charBuffer);
			currentAnnotationOffset += 2;
			// Parse num_element_value_pairs and element_value_pairs and visit these values.
			currentAnnotationOffset = readElementValues(recordComponentVisitor.visitAnnotation(annotationDescriptor, false), currentAnnotationOffset, true, charBuffer);
		  }
		}

		// Visit the RuntimeVisibleTypeAnnotations attribute.
		if (runtimeVisibleTypeAnnotationsOffset != 0)
		{
		  int numAnnotations = readUnsignedShort(runtimeVisibleTypeAnnotationsOffset);
		  int currentAnnotationOffset = runtimeVisibleTypeAnnotationsOffset + 2;
		  while (numAnnotations-- > 0)
		  {
			// Parse the target_type, target_info and target_path fields.
			currentAnnotationOffset = readTypeAnnotationTarget(context, currentAnnotationOffset);
			// Parse the type_index field.
			string annotationDescriptor = readUTF8(currentAnnotationOffset, charBuffer);
			currentAnnotationOffset += 2;
			// Parse num_element_value_pairs and element_value_pairs and visit these values.
			currentAnnotationOffset = readElementValues(recordComponentVisitor.visitTypeAnnotation(context.currentTypeAnnotationTarget, context.currentTypeAnnotationTargetPath, annotationDescriptor, true), currentAnnotationOffset, true, charBuffer);
		  }
		}

		// Visit the RuntimeInvisibleTypeAnnotations attribute.
		if (runtimeInvisibleTypeAnnotationsOffset != 0)
		{
		  int numAnnotations = readUnsignedShort(runtimeInvisibleTypeAnnotationsOffset);
		  int currentAnnotationOffset = runtimeInvisibleTypeAnnotationsOffset + 2;
		  while (numAnnotations-- > 0)
		  {
			// Parse the target_type, target_info and target_path fields.
			currentAnnotationOffset = readTypeAnnotationTarget(context, currentAnnotationOffset);
			// Parse the type_index field.
			string annotationDescriptor = readUTF8(currentAnnotationOffset, charBuffer);
			currentAnnotationOffset += 2;
			// Parse num_element_value_pairs and element_value_pairs and visit these values.
			currentAnnotationOffset = readElementValues(recordComponentVisitor.visitTypeAnnotation(context.currentTypeAnnotationTarget, context.currentTypeAnnotationTargetPath, annotationDescriptor, false), currentAnnotationOffset, true, charBuffer);
		  }
		}

		// Visit the non standard attributes.
		while (attributes != null)
		{
		  // Copy and reset the nextAttribute field so that it can also be used in FieldWriter.
		  Attribute nextAttribute = attributes.nextAttribute;
		  attributes.nextAttribute = null;
		  recordComponentVisitor.visitAttribute(attributes);
		  attributes = nextAttribute;
		}

		// Visit the end of the field.
		recordComponentVisitor.visitEnd();
		return currentOffset;
	  }

	  /// <summary>
	  /// Reads a JVMS field_info structure and makes the given visitor visit it.
	  /// </summary>
	  /// <param name="classVisitor"> the visitor that must visit the field. </param>
	  /// <param name="context"> information about the class being parsed. </param>
	  /// <param name="fieldInfoOffset"> the start offset of the field_info structure. </param>
	  /// <returns> the offset of the first byte following the field_info structure. </returns>
	  private int readField(ClassVisitor classVisitor, Context context, int fieldInfoOffset)
	  {
		char[] charBuffer = context.charBuffer;

		// Read the access_flags, name_index and descriptor_index fields.
		int currentOffset = fieldInfoOffset;
		int accessFlags = readUnsignedShort(currentOffset);
		string name = readUTF8(currentOffset + 2, charBuffer);
		string descriptor = readUTF8(currentOffset + 4, charBuffer);
		currentOffset += 6;

		// Read the field attributes (the variables are ordered as in Section 4.7 of the JVMS).
		// Attribute offsets exclude the attribute_name_index and attribute_length fields.
		// - The value corresponding to the ConstantValue attribute, or null.
		object constantValue = null;
		// - The string corresponding to the Signature attribute, or null.
		string signature = null;
		// - The offset of the RuntimeVisibleAnnotations attribute, or 0.
		int runtimeVisibleAnnotationsOffset = 0;
		// - The offset of the RuntimeInvisibleAnnotations attribute, or 0.
		int runtimeInvisibleAnnotationsOffset = 0;
		// - The offset of the RuntimeVisibleTypeAnnotations attribute, or 0.
		int runtimeVisibleTypeAnnotationsOffset = 0;
		// - The offset of the RuntimeInvisibleTypeAnnotations attribute, or 0.
		int runtimeInvisibleTypeAnnotationsOffset = 0;
		// - The non standard attributes (linked with their {@link Attribute#nextAttribute} field).
		//   This list in the <i>reverse order</i> or their order in the ClassFile structure.
		Attribute attributes = null;

		int attributesCount = readUnsignedShort(currentOffset);
		currentOffset += 2;
		while (attributesCount-- > 0)
		{
		  // Read the attribute_info's attribute_name and attribute_length fields.
		  string attributeName = readUTF8(currentOffset, charBuffer);
		  int attributeLength = readInt(currentOffset + 2);
		  currentOffset += 6;
		  // The tests are sorted in decreasing frequency order (based on frequencies observed on
		  // typical classes).
		  if (Constants.CONSTANT_VALUE.Equals(attributeName))
		  {
			int constantvalueIndex = readUnsignedShort(currentOffset);
			constantValue = constantvalueIndex == 0 ? null : readConst(constantvalueIndex, charBuffer);
		  }
		  else if (Constants.SIGNATURE.Equals(attributeName))
		  {
			signature = readUTF8(currentOffset, charBuffer);
		  }
		  else if (Constants.DEPRECATED.Equals(attributeName))
		  {
			accessFlags |= Opcodes.ACC_DEPRECATED;
		  }
		  else if (Constants.SYNTHETIC.Equals(attributeName))
		  {
			accessFlags |= Opcodes.ACC_SYNTHETIC;
		  }
		  else if (Constants.RUNTIME_VISIBLE_ANNOTATIONS.Equals(attributeName))
		  {
			runtimeVisibleAnnotationsOffset = currentOffset;
		  }
		  else if (Constants.RUNTIME_VISIBLE_TYPE_ANNOTATIONS.Equals(attributeName))
		  {
			runtimeVisibleTypeAnnotationsOffset = currentOffset;
		  }
		  else if (Constants.RUNTIME_INVISIBLE_ANNOTATIONS.Equals(attributeName))
		  {
			runtimeInvisibleAnnotationsOffset = currentOffset;
		  }
		  else if (Constants.RUNTIME_INVISIBLE_TYPE_ANNOTATIONS.Equals(attributeName))
		  {
			runtimeInvisibleTypeAnnotationsOffset = currentOffset;
		  }
		  else
		  {
			Attribute attribute = readAttribute(context.attributePrototypes, attributeName, currentOffset, attributeLength, charBuffer, -1, null);
			attribute.nextAttribute = attributes;
			attributes = attribute;
		  }
		  currentOffset += attributeLength;
		}

		// Visit the field declaration.
		FieldVisitor fieldVisitor = classVisitor.visitField(accessFlags, name, descriptor, signature, constantValue);
		if (fieldVisitor == null)
		{
		  return currentOffset;
		}

		// Visit the RuntimeVisibleAnnotations attribute.
		if (runtimeVisibleAnnotationsOffset != 0)
		{
		  int numAnnotations = readUnsignedShort(runtimeVisibleAnnotationsOffset);
		  int currentAnnotationOffset = runtimeVisibleAnnotationsOffset + 2;
		  while (numAnnotations-- > 0)
		  {
			// Parse the type_index field.
			string annotationDescriptor = readUTF8(currentAnnotationOffset, charBuffer);
			currentAnnotationOffset += 2;
			// Parse num_element_value_pairs and element_value_pairs and visit these values.
			currentAnnotationOffset = readElementValues(fieldVisitor.visitAnnotation(annotationDescriptor, true), currentAnnotationOffset, true, charBuffer);
		  }
		}

		// Visit the RuntimeInvisibleAnnotations attribute.
		if (runtimeInvisibleAnnotationsOffset != 0)
		{
		  int numAnnotations = readUnsignedShort(runtimeInvisibleAnnotationsOffset);
		  int currentAnnotationOffset = runtimeInvisibleAnnotationsOffset + 2;
		  while (numAnnotations-- > 0)
		  {
			// Parse the type_index field.
			string annotationDescriptor = readUTF8(currentAnnotationOffset, charBuffer);
			currentAnnotationOffset += 2;
			// Parse num_element_value_pairs and element_value_pairs and visit these values.
			currentAnnotationOffset = readElementValues(fieldVisitor.visitAnnotation(annotationDescriptor, false), currentAnnotationOffset, true, charBuffer);
		  }
		}

		// Visit the RuntimeVisibleTypeAnnotations attribute.
		if (runtimeVisibleTypeAnnotationsOffset != 0)
		{
		  int numAnnotations = readUnsignedShort(runtimeVisibleTypeAnnotationsOffset);
		  int currentAnnotationOffset = runtimeVisibleTypeAnnotationsOffset + 2;
		  while (numAnnotations-- > 0)
		  {
			// Parse the target_type, target_info and target_path fields.
			currentAnnotationOffset = readTypeAnnotationTarget(context, currentAnnotationOffset);
			// Parse the type_index field.
			string annotationDescriptor = readUTF8(currentAnnotationOffset, charBuffer);
			currentAnnotationOffset += 2;
			// Parse num_element_value_pairs and element_value_pairs and visit these values.
			currentAnnotationOffset = readElementValues(fieldVisitor.visitTypeAnnotation(context.currentTypeAnnotationTarget, context.currentTypeAnnotationTargetPath, annotationDescriptor, true), currentAnnotationOffset, true, charBuffer);
		  }
		}

		// Visit the RuntimeInvisibleTypeAnnotations attribute.
		if (runtimeInvisibleTypeAnnotationsOffset != 0)
		{
		  int numAnnotations = readUnsignedShort(runtimeInvisibleTypeAnnotationsOffset);
		  int currentAnnotationOffset = runtimeInvisibleTypeAnnotationsOffset + 2;
		  while (numAnnotations-- > 0)
		  {
			// Parse the target_type, target_info and target_path fields.
			currentAnnotationOffset = readTypeAnnotationTarget(context, currentAnnotationOffset);
			// Parse the type_index field.
			string annotationDescriptor = readUTF8(currentAnnotationOffset, charBuffer);
			currentAnnotationOffset += 2;
			// Parse num_element_value_pairs and element_value_pairs and visit these values.
			currentAnnotationOffset = readElementValues(fieldVisitor.visitTypeAnnotation(context.currentTypeAnnotationTarget, context.currentTypeAnnotationTargetPath, annotationDescriptor, false), currentAnnotationOffset, true, charBuffer);
		  }
		}

		// Visit the non standard attributes.
		while (attributes != null)
		{
		  // Copy and reset the nextAttribute field so that it can also be used in FieldWriter.
		  Attribute nextAttribute = attributes.nextAttribute;
		  attributes.nextAttribute = null;
		  fieldVisitor.visitAttribute(attributes);
		  attributes = nextAttribute;
		}

		// Visit the end of the field.
		fieldVisitor.visitEnd();
		return currentOffset;
	  }

	  /// <summary>
	  /// Reads a JVMS method_info structure and makes the given visitor visit it.
	  /// </summary>
	  /// <param name="classVisitor"> the visitor that must visit the method. </param>
	  /// <param name="context"> information about the class being parsed. </param>
	  /// <param name="methodInfoOffset"> the start offset of the method_info structure. </param>
	  /// <returns> the offset of the first byte following the method_info structure. </returns>
	  private int readMethod(ClassVisitor classVisitor, Context context, int methodInfoOffset)
	  {
		char[] charBuffer = context.charBuffer;

		// Read the access_flags, name_index and descriptor_index fields.
		int currentOffset = methodInfoOffset;
		context.currentMethodAccessFlags = readUnsignedShort(currentOffset);
		context.currentMethodName = readUTF8(currentOffset + 2, charBuffer);
		context.currentMethodDescriptor = readUTF8(currentOffset + 4, charBuffer);
		currentOffset += 6;

		// Read the method attributes (the variables are ordered as in Section 4.7 of the JVMS).
		// Attribute offsets exclude the attribute_name_index and attribute_length fields.
		// - The offset of the Code attribute, or 0.
		int codeOffset = 0;
		// - The offset of the Exceptions attribute, or 0.
		int exceptionsOffset = 0;
		// - The strings corresponding to the Exceptions attribute, or null.
		string[] exceptions = null;
		// - Whether the method has a Synthetic attribute.
		bool synthetic = false;
		// - The constant pool index contained in the Signature attribute, or 0.
		int signatureIndex = 0;
		// - The offset of the RuntimeVisibleAnnotations attribute, or 0.
		int runtimeVisibleAnnotationsOffset = 0;
		// - The offset of the RuntimeInvisibleAnnotations attribute, or 0.
		int runtimeInvisibleAnnotationsOffset = 0;
		// - The offset of the RuntimeVisibleParameterAnnotations attribute, or 0.
		int runtimeVisibleParameterAnnotationsOffset = 0;
		// - The offset of the RuntimeInvisibleParameterAnnotations attribute, or 0.
		int runtimeInvisibleParameterAnnotationsOffset = 0;
		// - The offset of the RuntimeVisibleTypeAnnotations attribute, or 0.
		int runtimeVisibleTypeAnnotationsOffset = 0;
		// - The offset of the RuntimeInvisibleTypeAnnotations attribute, or 0.
		int runtimeInvisibleTypeAnnotationsOffset = 0;
		// - The offset of the AnnotationDefault attribute, or 0.
		int annotationDefaultOffset = 0;
		// - The offset of the MethodParameters attribute, or 0.
		int methodParametersOffset = 0;
		// - The non standard attributes (linked with their {@link Attribute#nextAttribute} field).
		//   This list in the <i>reverse order</i> or their order in the ClassFile structure.
		Attribute attributes = null;

		int attributesCount = readUnsignedShort(currentOffset);
		currentOffset += 2;
		while (attributesCount-- > 0)
		{
		  // Read the attribute_info's attribute_name and attribute_length fields.
		  string attributeName = readUTF8(currentOffset, charBuffer);
		  int attributeLength = readInt(currentOffset + 2);
		  currentOffset += 6;
		  // The tests are sorted in decreasing frequency order (based on frequencies observed on
		  // typical classes).
		  if (Constants.CODE.Equals(attributeName))
		  {
			if ((context.parsingOptions & SKIP_CODE) == 0)
			{
			  codeOffset = currentOffset;
			}
		  }
		  else if (Constants.EXCEPTIONS.Equals(attributeName))
		  {
			exceptionsOffset = currentOffset;
			exceptions = new string[readUnsignedShort(exceptionsOffset)];
			int currentExceptionOffset = exceptionsOffset + 2;
			for (int i = 0; i < exceptions.Length; ++i)
			{
			  exceptions[i] = readClass(currentExceptionOffset, charBuffer);
			  currentExceptionOffset += 2;
			}
		  }
		  else if (Constants.SIGNATURE.Equals(attributeName))
		  {
			signatureIndex = readUnsignedShort(currentOffset);
		  }
		  else if (Constants.DEPRECATED.Equals(attributeName))
		  {
			context.currentMethodAccessFlags |= Opcodes.ACC_DEPRECATED;
		  }
		  else if (Constants.RUNTIME_VISIBLE_ANNOTATIONS.Equals(attributeName))
		  {
			runtimeVisibleAnnotationsOffset = currentOffset;
		  }
		  else if (Constants.RUNTIME_VISIBLE_TYPE_ANNOTATIONS.Equals(attributeName))
		  {
			runtimeVisibleTypeAnnotationsOffset = currentOffset;
		  }
		  else if (Constants.ANNOTATION_DEFAULT.Equals(attributeName))
		  {
			annotationDefaultOffset = currentOffset;
		  }
		  else if (Constants.SYNTHETIC.Equals(attributeName))
		  {
			synthetic = true;
			context.currentMethodAccessFlags |= Opcodes.ACC_SYNTHETIC;
		  }
		  else if (Constants.RUNTIME_INVISIBLE_ANNOTATIONS.Equals(attributeName))
		  {
			runtimeInvisibleAnnotationsOffset = currentOffset;
		  }
		  else if (Constants.RUNTIME_INVISIBLE_TYPE_ANNOTATIONS.Equals(attributeName))
		  {
			runtimeInvisibleTypeAnnotationsOffset = currentOffset;
		  }
		  else if (Constants.RUNTIME_VISIBLE_PARAMETER_ANNOTATIONS.Equals(attributeName))
		  {
			runtimeVisibleParameterAnnotationsOffset = currentOffset;
		  }
		  else if (Constants.RUNTIME_INVISIBLE_PARAMETER_ANNOTATIONS.Equals(attributeName))
		  {
			runtimeInvisibleParameterAnnotationsOffset = currentOffset;
		  }
		  else if (Constants.METHOD_PARAMETERS.Equals(attributeName))
		  {
			methodParametersOffset = currentOffset;
		  }
		  else
		  {
			Attribute attribute = readAttribute(context.attributePrototypes, attributeName, currentOffset, attributeLength, charBuffer, -1, null);
			attribute.nextAttribute = attributes;
			attributes = attribute;
		  }
		  currentOffset += attributeLength;
		}

		// Visit the method declaration.
		MethodVisitor methodVisitor = classVisitor.visitMethod(context.currentMethodAccessFlags, context.currentMethodName, context.currentMethodDescriptor, signatureIndex == 0 ? null : readUtf(signatureIndex, charBuffer), exceptions);
		if (methodVisitor == null)
		{
		  return currentOffset;
		}

		// If the returned MethodVisitor is in fact a MethodWriter, it means there is no method
		// adapter between the reader and the writer. In this case, it might be possible to copy
		// the method attributes directly into the writer. If so, return early without visiting
		// the content of these attributes.
		if (methodVisitor is MethodWriter)
		{
		  MethodWriter methodWriter = (MethodWriter) methodVisitor;
		  if (methodWriter.canCopyMethodAttributes(this, synthetic, (context.currentMethodAccessFlags & Opcodes.ACC_DEPRECATED) != 0, readUnsignedShort(methodInfoOffset + 4), signatureIndex, exceptionsOffset))
		  {
			methodWriter.setMethodAttributesSource(methodInfoOffset, currentOffset - methodInfoOffset);
			return currentOffset;
		  }
		}

		// Visit the MethodParameters attribute.
		if (methodParametersOffset != 0 && (context.parsingOptions & SKIP_DEBUG) == 0)
		{
		  int parametersCount = readByte(methodParametersOffset);
		  int currentParameterOffset = methodParametersOffset + 1;
		  while (parametersCount-- > 0)
		  {
			// Read the name_index and access_flags fields and visit them.
			methodVisitor.visitParameter(readUTF8(currentParameterOffset, charBuffer), readUnsignedShort(currentParameterOffset + 2));
			currentParameterOffset += 4;
		  }
		}

		// Visit the AnnotationDefault attribute.
		if (annotationDefaultOffset != 0)
		{
		  AnnotationVisitor annotationVisitor = methodVisitor.visitAnnotationDefault();
		  readElementValue(annotationVisitor, annotationDefaultOffset, null, charBuffer);
		  if (annotationVisitor != null)
		  {
			annotationVisitor.visitEnd();
		  }
		}

		// Visit the RuntimeVisibleAnnotations attribute.
		if (runtimeVisibleAnnotationsOffset != 0)
		{
		  int numAnnotations = readUnsignedShort(runtimeVisibleAnnotationsOffset);
		  int currentAnnotationOffset = runtimeVisibleAnnotationsOffset + 2;
		  while (numAnnotations-- > 0)
		  {
			// Parse the type_index field.
			string annotationDescriptor = readUTF8(currentAnnotationOffset, charBuffer);
			currentAnnotationOffset += 2;
			// Parse num_element_value_pairs and element_value_pairs and visit these values.
			currentAnnotationOffset = readElementValues(methodVisitor.visitAnnotation(annotationDescriptor, true), currentAnnotationOffset, true, charBuffer);
		  }
		}

		// Visit the RuntimeInvisibleAnnotations attribute.
		if (runtimeInvisibleAnnotationsOffset != 0)
		{
		  int numAnnotations = readUnsignedShort(runtimeInvisibleAnnotationsOffset);
		  int currentAnnotationOffset = runtimeInvisibleAnnotationsOffset + 2;
		  while (numAnnotations-- > 0)
		  {
			// Parse the type_index field.
			string annotationDescriptor = readUTF8(currentAnnotationOffset, charBuffer);
			currentAnnotationOffset += 2;
			// Parse num_element_value_pairs and element_value_pairs and visit these values.
			currentAnnotationOffset = readElementValues(methodVisitor.visitAnnotation(annotationDescriptor, false), currentAnnotationOffset, true, charBuffer);
		  }
		}

		// Visit the RuntimeVisibleTypeAnnotations attribute.
		if (runtimeVisibleTypeAnnotationsOffset != 0)
		{
		  int numAnnotations = readUnsignedShort(runtimeVisibleTypeAnnotationsOffset);
		  int currentAnnotationOffset = runtimeVisibleTypeAnnotationsOffset + 2;
		  while (numAnnotations-- > 0)
		  {
			// Parse the target_type, target_info and target_path fields.
			currentAnnotationOffset = readTypeAnnotationTarget(context, currentAnnotationOffset);
			// Parse the type_index field.
			string annotationDescriptor = readUTF8(currentAnnotationOffset, charBuffer);
			currentAnnotationOffset += 2;
			// Parse num_element_value_pairs and element_value_pairs and visit these values.
			currentAnnotationOffset = readElementValues(methodVisitor.visitTypeAnnotation(context.currentTypeAnnotationTarget, context.currentTypeAnnotationTargetPath, annotationDescriptor, true), currentAnnotationOffset, true, charBuffer);
		  }
		}

		// Visit the RuntimeInvisibleTypeAnnotations attribute.
		if (runtimeInvisibleTypeAnnotationsOffset != 0)
		{
		  int numAnnotations = readUnsignedShort(runtimeInvisibleTypeAnnotationsOffset);
		  int currentAnnotationOffset = runtimeInvisibleTypeAnnotationsOffset + 2;
		  while (numAnnotations-- > 0)
		  {
			// Parse the target_type, target_info and target_path fields.
			currentAnnotationOffset = readTypeAnnotationTarget(context, currentAnnotationOffset);
			// Parse the type_index field.
			string annotationDescriptor = readUTF8(currentAnnotationOffset, charBuffer);
			currentAnnotationOffset += 2;
			// Parse num_element_value_pairs and element_value_pairs and visit these values.
			currentAnnotationOffset = readElementValues(methodVisitor.visitTypeAnnotation(context.currentTypeAnnotationTarget, context.currentTypeAnnotationTargetPath, annotationDescriptor, false), currentAnnotationOffset, true, charBuffer);
		  }
		}

		// Visit the RuntimeVisibleParameterAnnotations attribute.
		if (runtimeVisibleParameterAnnotationsOffset != 0)
		{
		  readParameterAnnotations(methodVisitor, context, runtimeVisibleParameterAnnotationsOffset, true);
		}

		// Visit the RuntimeInvisibleParameterAnnotations attribute.
		if (runtimeInvisibleParameterAnnotationsOffset != 0)
		{
		  readParameterAnnotations(methodVisitor, context, runtimeInvisibleParameterAnnotationsOffset, false);
		}

		// Visit the non standard attributes.
		while (attributes != null)
		{
		  // Copy and reset the nextAttribute field so that it can also be used in MethodWriter.
		  Attribute nextAttribute = attributes.nextAttribute;
		  attributes.nextAttribute = null;
		  methodVisitor.visitAttribute(attributes);
		  attributes = nextAttribute;
		}

		// Visit the Code attribute.
		if (codeOffset != 0)
		{
		  methodVisitor.visitCode();
		  readCode(methodVisitor, context, codeOffset);
		}

		// Visit the end of the method.
		methodVisitor.visitEnd();
		return currentOffset;
	  }

	  // ----------------------------------------------------------------------------------------------
	  // Methods to parse a Code attribute
	  // ----------------------------------------------------------------------------------------------

	  /// <summary>
	  /// Reads a JVMS 'Code' attribute and makes the given visitor visit it.
	  /// </summary>
	  /// <param name="methodVisitor"> the visitor that must visit the Code attribute. </param>
	  /// <param name="context"> information about the class being parsed. </param>
	  /// <param name="codeOffset"> the start offset in <seealso cref="classFileBuffer"/> of the Code attribute, excluding
	  ///     its attribute_name_index and attribute_length fields. </param>
	  private void readCode(MethodVisitor methodVisitor, Context context, int codeOffset)
	  {
		int currentOffset = codeOffset;

		// Read the max_stack, max_locals and code_length fields.
		sbyte[] classBuffer = classFileBuffer;
		char[] charBuffer = context.charBuffer;
		int maxStack = readUnsignedShort(currentOffset);
		int maxLocals = readUnsignedShort(currentOffset + 2);
		int codeLength = readInt(currentOffset + 4);
		currentOffset += 8;
		if (codeLength > classFileBuffer.Length - currentOffset)
		{
		  throw new System.ArgumentException();
		}

		// Read the bytecode 'code' array to create a label for each referenced instruction.
		int bytecodeStartOffset = currentOffset;
		int bytecodeEndOffset = currentOffset + codeLength;
		Label[] labels = context.currentMethodLabels = new Label[codeLength + 1];
		while (currentOffset < bytecodeEndOffset)
		{
		  int bytecodeOffset = currentOffset - bytecodeStartOffset;
		  int opcode = classBuffer[currentOffset] & 0xFF;
		  switch (opcode)
		  {
			case Opcodes.NOP:
			case Opcodes.ACONST_NULL:
			case Opcodes.ICONST_M1:
			case Opcodes.ICONST_0:
			case Opcodes.ICONST_1:
			case Opcodes.ICONST_2:
			case Opcodes.ICONST_3:
			case Opcodes.ICONST_4:
			case Opcodes.ICONST_5:
			case Opcodes.LCONST_0:
			case Opcodes.LCONST_1:
			case Opcodes.FCONST_0:
			case Opcodes.FCONST_1:
			case Opcodes.FCONST_2:
			case Opcodes.DCONST_0:
			case Opcodes.DCONST_1:
			case Opcodes.IALOAD:
			case Opcodes.LALOAD:
			case Opcodes.FALOAD:
			case Opcodes.DALOAD:
			case Opcodes.AALOAD:
			case Opcodes.BALOAD:
			case Opcodes.CALOAD:
			case Opcodes.SALOAD:
			case Opcodes.IASTORE:
			case Opcodes.LASTORE:
			case Opcodes.FASTORE:
			case Opcodes.DASTORE:
			case Opcodes.AASTORE:
			case Opcodes.BASTORE:
			case Opcodes.CASTORE:
			case Opcodes.SASTORE:
			case Opcodes.POP:
			case Opcodes.POP2:
			case Opcodes.DUP:
			case Opcodes.DUP_X1:
			case Opcodes.DUP_X2:
			case Opcodes.DUP2:
			case Opcodes.DUP2_X1:
			case Opcodes.DUP2_X2:
			case Opcodes.SWAP:
			case Opcodes.IADD:
			case Opcodes.LADD:
			case Opcodes.FADD:
			case Opcodes.DADD:
			case Opcodes.ISUB:
			case Opcodes.LSUB:
			case Opcodes.FSUB:
			case Opcodes.DSUB:
			case Opcodes.IMUL:
			case Opcodes.LMUL:
			case Opcodes.FMUL:
			case Opcodes.DMUL:
			case Opcodes.IDIV:
			case Opcodes.LDIV:
			case Opcodes.FDIV:
			case Opcodes.DDIV:
			case Opcodes.IREM:
			case Opcodes.LREM:
			case Opcodes.FREM:
			case Opcodes.DREM:
			case Opcodes.INEG:
			case Opcodes.LNEG:
			case Opcodes.FNEG:
			case Opcodes.DNEG:
			case Opcodes.ISHL:
			case Opcodes.LSHL:
			case Opcodes.ISHR:
			case Opcodes.LSHR:
			case Opcodes.IUSHR:
			case Opcodes.LUSHR:
			case Opcodes.IAND:
			case Opcodes.LAND:
			case Opcodes.IOR:
			case Opcodes.LOR:
			case Opcodes.IXOR:
			case Opcodes.LXOR:
			case Opcodes.I2L:
			case Opcodes.I2F:
			case Opcodes.I2D:
			case Opcodes.L2I:
			case Opcodes.L2F:
			case Opcodes.L2D:
			case Opcodes.F2I:
			case Opcodes.F2L:
			case Opcodes.F2D:
			case Opcodes.D2I:
			case Opcodes.D2L:
			case Opcodes.D2F:
			case Opcodes.I2B:
			case Opcodes.I2C:
			case Opcodes.I2S:
			case Opcodes.LCMP:
			case Opcodes.FCMPL:
			case Opcodes.FCMPG:
			case Opcodes.DCMPL:
			case Opcodes.DCMPG:
			case Opcodes.IRETURN:
			case Opcodes.LRETURN:
			case Opcodes.FRETURN:
			case Opcodes.DRETURN:
			case Opcodes.ARETURN:
			case Opcodes.RETURN:
			case Opcodes.ARRAYLENGTH:
			case Opcodes.ATHROW:
			case Opcodes.MONITORENTER:
			case Opcodes.MONITOREXIT:
			case Constants.ILOAD_0:
			case Constants.ILOAD_1:
			case Constants.ILOAD_2:
			case Constants.ILOAD_3:
			case Constants.LLOAD_0:
			case Constants.LLOAD_1:
			case Constants.LLOAD_2:
			case Constants.LLOAD_3:
			case Constants.FLOAD_0:
			case Constants.FLOAD_1:
			case Constants.FLOAD_2:
			case Constants.FLOAD_3:
			case Constants.DLOAD_0:
			case Constants.DLOAD_1:
			case Constants.DLOAD_2:
			case Constants.DLOAD_3:
			case Constants.ALOAD_0:
			case Constants.ALOAD_1:
			case Constants.ALOAD_2:
			case Constants.ALOAD_3:
			case Constants.ISTORE_0:
			case Constants.ISTORE_1:
			case Constants.ISTORE_2:
			case Constants.ISTORE_3:
			case Constants.LSTORE_0:
			case Constants.LSTORE_1:
			case Constants.LSTORE_2:
			case Constants.LSTORE_3:
			case Constants.FSTORE_0:
			case Constants.FSTORE_1:
			case Constants.FSTORE_2:
			case Constants.FSTORE_3:
			case Constants.DSTORE_0:
			case Constants.DSTORE_1:
			case Constants.DSTORE_2:
			case Constants.DSTORE_3:
			case Constants.ASTORE_0:
			case Constants.ASTORE_1:
			case Constants.ASTORE_2:
			case Constants.ASTORE_3:
			  currentOffset += 1;
			  break;
			case Opcodes.IFEQ:
			case Opcodes.IFNE:
			case Opcodes.IFLT:
			case Opcodes.IFGE:
			case Opcodes.IFGT:
			case Opcodes.IFLE:
			case Opcodes.IF_ICMPEQ:
			case Opcodes.IF_ICMPNE:
			case Opcodes.IF_ICMPLT:
			case Opcodes.IF_ICMPGE:
			case Opcodes.IF_ICMPGT:
			case Opcodes.IF_ICMPLE:
			case Opcodes.IF_ACMPEQ:
			case Opcodes.IF_ACMPNE:
			case Opcodes.GOTO:
			case Opcodes.JSR:
			case Opcodes.IFNULL:
			case Opcodes.IFNONNULL:
			  createLabel(bytecodeOffset + readShort(currentOffset + 1), labels);
			  currentOffset += 3;
			  break;
			case Constants.ASM_IFEQ:
			case Constants.ASM_IFNE:
			case Constants.ASM_IFLT:
			case Constants.ASM_IFGE:
			case Constants.ASM_IFGT:
			case Constants.ASM_IFLE:
			case Constants.ASM_IF_ICMPEQ:
			case Constants.ASM_IF_ICMPNE:
			case Constants.ASM_IF_ICMPLT:
			case Constants.ASM_IF_ICMPGE:
			case Constants.ASM_IF_ICMPGT:
			case Constants.ASM_IF_ICMPLE:
			case Constants.ASM_IF_ACMPEQ:
			case Constants.ASM_IF_ACMPNE:
			case Constants.ASM_GOTO:
			case Constants.ASM_JSR:
			case Constants.ASM_IFNULL:
			case Constants.ASM_IFNONNULL:
			  createLabel(bytecodeOffset + readUnsignedShort(currentOffset + 1), labels);
			  currentOffset += 3;
			  break;
			case Constants.GOTO_W:
			case Constants.JSR_W:
			case Constants.ASM_GOTO_W:
			  createLabel(bytecodeOffset + readInt(currentOffset + 1), labels);
			  currentOffset += 5;
			  break;
			case Constants.WIDE:
			  switch (classBuffer[currentOffset + 1] & 0xFF)
			  {
				case Opcodes.ILOAD:
				case Opcodes.FLOAD:
				case Opcodes.ALOAD:
				case Opcodes.LLOAD:
				case Opcodes.DLOAD:
				case Opcodes.ISTORE:
				case Opcodes.FSTORE:
				case Opcodes.ASTORE:
				case Opcodes.LSTORE:
				case Opcodes.DSTORE:
				case Opcodes.RET:
				  currentOffset += 4;
				  break;
				case Opcodes.IINC:
				  currentOffset += 6;
				  break;
				default:
				  throw new System.ArgumentException();
			  }
			  break;
			case Opcodes.TABLESWITCH:
			  // Skip 0 to 3 padding bytes.
			  currentOffset += 4 - (bytecodeOffset & 3);
			  // Read the default label and the number of table entries.
			  createLabel(bytecodeOffset + readInt(currentOffset), labels);
			  int numTableEntries = readInt(currentOffset + 8) - readInt(currentOffset + 4) + 1;
			  currentOffset += 12;
			  // Read the table labels.
			  while (numTableEntries-- > 0)
			  {
				createLabel(bytecodeOffset + readInt(currentOffset), labels);
				currentOffset += 4;
			  }
			  break;
			case Opcodes.LOOKUPSWITCH:
			  // Skip 0 to 3 padding bytes.
			  currentOffset += 4 - (bytecodeOffset & 3);
			  // Read the default label and the number of switch cases.
			  createLabel(bytecodeOffset + readInt(currentOffset), labels);
			  int numSwitchCases = readInt(currentOffset + 4);
			  currentOffset += 8;
			  // Read the switch labels.
			  while (numSwitchCases-- > 0)
			  {
				createLabel(bytecodeOffset + readInt(currentOffset + 4), labels);
				currentOffset += 8;
			  }
			  break;
			case Opcodes.ILOAD:
			case Opcodes.LLOAD:
			case Opcodes.FLOAD:
			case Opcodes.DLOAD:
			case Opcodes.ALOAD:
			case Opcodes.ISTORE:
			case Opcodes.LSTORE:
			case Opcodes.FSTORE:
			case Opcodes.DSTORE:
			case Opcodes.ASTORE:
			case Opcodes.RET:
			case Opcodes.BIPUSH:
			case Opcodes.NEWARRAY:
			case Opcodes.LDC:
			  currentOffset += 2;
			  break;
			case Opcodes.SIPUSH:
			case Constants.LDC_W:
			case Constants.LDC2_W:
			case Opcodes.GETSTATIC:
			case Opcodes.PUTSTATIC:
			case Opcodes.GETFIELD:
			case Opcodes.PUTFIELD:
			case Opcodes.INVOKEVIRTUAL:
			case Opcodes.INVOKESPECIAL:
			case Opcodes.INVOKESTATIC:
			case Opcodes.NEW:
			case Opcodes.ANEWARRAY:
			case Opcodes.CHECKCAST:
			case Opcodes.INSTANCEOF:
			case Opcodes.IINC:
			  currentOffset += 3;
			  break;
			case Opcodes.INVOKEINTERFACE:
			case Opcodes.INVOKEDYNAMIC:
			  currentOffset += 5;
			  break;
			case Opcodes.MULTIANEWARRAY:
			  currentOffset += 4;
			  break;
			default:
			  throw new System.ArgumentException();
		  }
		}

		// Read the 'exception_table_length' and 'exception_table' field to create a label for each
		// referenced instruction, and to make methodVisitor visit the corresponding try catch blocks.
		int exceptionTableLength = readUnsignedShort(currentOffset);
		currentOffset += 2;
		while (exceptionTableLength-- > 0)
		{
		  Label start = createLabel(readUnsignedShort(currentOffset), labels);
		  Label end = createLabel(readUnsignedShort(currentOffset + 2), labels);
		  Label handler = createLabel(readUnsignedShort(currentOffset + 4), labels);
		  string catchType = readUTF8(cpInfoOffsets[readUnsignedShort(currentOffset + 6)], charBuffer);
		  currentOffset += 8;
		  methodVisitor.visitTryCatchBlock(start, end, handler, catchType);
		}

		// Read the Code attributes to create a label for each referenced instruction (the variables
		// are ordered as in Section 4.7 of the JVMS). Attribute offsets exclude the
		// attribute_name_index and attribute_length fields.
		// - The offset of the current 'stack_map_frame' in the StackMap[Table] attribute, or 0.
		// Initially, this is the offset of the first 'stack_map_frame' entry. Then this offset is
		// updated after each stack_map_frame is read.
		int stackMapFrameOffset = 0;
		// - The end offset of the StackMap[Table] attribute, or 0.
		int stackMapTableEndOffset = 0;
		// - Whether the stack map frames are compressed (i.e. in a StackMapTable) or not.
		bool compressedFrames = true;
		// - The offset of the LocalVariableTable attribute, or 0.
		int localVariableTableOffset = 0;
		// - The offset of the LocalVariableTypeTable attribute, or 0.
		int localVariableTypeTableOffset = 0;
		// - The offset of each 'type_annotation' entry in the RuntimeVisibleTypeAnnotations
		// attribute, or null.
		int[] visibleTypeAnnotationOffsets = null;
		// - The offset of each 'type_annotation' entry in the RuntimeInvisibleTypeAnnotations
		// attribute, or null.
		int[] invisibleTypeAnnotationOffsets = null;
		// - The non standard attributes (linked with their {@link Attribute#nextAttribute} field).
		//   This list in the <i>reverse order</i> or their order in the ClassFile structure.
		Attribute attributes = null;

		int attributesCount = readUnsignedShort(currentOffset);
		currentOffset += 2;
		while (attributesCount-- > 0)
		{
		  // Read the attribute_info's attribute_name and attribute_length fields.
		  string attributeName = readUTF8(currentOffset, charBuffer);
		  int attributeLength = readInt(currentOffset + 2);
		  currentOffset += 6;
		  if (Constants.LOCAL_VARIABLE_TABLE.Equals(attributeName))
		  {
			if ((context.parsingOptions & SKIP_DEBUG) == 0)
			{
			  localVariableTableOffset = currentOffset;
			  // Parse the attribute to find the corresponding (debug only) labels.
			  int currentLocalVariableTableOffset = currentOffset;
			  int localVariableTableLength = readUnsignedShort(currentLocalVariableTableOffset);
			  currentLocalVariableTableOffset += 2;
			  while (localVariableTableLength-- > 0)
			  {
				int startPc = readUnsignedShort(currentLocalVariableTableOffset);
				createDebugLabel(startPc, labels);
				int length = readUnsignedShort(currentLocalVariableTableOffset + 2);
				createDebugLabel(startPc + length, labels);
				// Skip the name_index, descriptor_index and index fields (2 bytes each).
				currentLocalVariableTableOffset += 10;
			  }
			}
		  }
		  else if (Constants.LOCAL_VARIABLE_TYPE_TABLE.Equals(attributeName))
		  {
			localVariableTypeTableOffset = currentOffset;
			// Here we do not extract the labels corresponding to the attribute content. We assume they
			// are the same or a subset of those of the LocalVariableTable attribute.
		  }
		  else if (Constants.LINE_NUMBER_TABLE.Equals(attributeName))
		  {
			if ((context.parsingOptions & SKIP_DEBUG) == 0)
			{
			  // Parse the attribute to find the corresponding (debug only) labels.
			  int currentLineNumberTableOffset = currentOffset;
			  int lineNumberTableLength = readUnsignedShort(currentLineNumberTableOffset);
			  currentLineNumberTableOffset += 2;
			  while (lineNumberTableLength-- > 0)
			  {
				int startPc = readUnsignedShort(currentLineNumberTableOffset);
				int lineNumber = readUnsignedShort(currentLineNumberTableOffset + 2);
				currentLineNumberTableOffset += 4;
				createDebugLabel(startPc, labels);
				labels[startPc].addLineNumber(lineNumber);
			  }
			}
		  }
		  else if (Constants.RUNTIME_VISIBLE_TYPE_ANNOTATIONS.Equals(attributeName))
		  {
			visibleTypeAnnotationOffsets = readTypeAnnotations(methodVisitor, context, currentOffset, true);
			// Here we do not extract the labels corresponding to the attribute content. This would
			// require a full parsing of the attribute, which would need to be repeated when parsing
			// the bytecode instructions (see below). Instead, the content of the attribute is read one
			// type annotation at a time (i.e. after a type annotation has been visited, the next type
			// annotation is read), and the labels it contains are also extracted one annotation at a
			// time. This assumes that type annotations are ordered by increasing bytecode offset.
		  }
		  else if (Constants.RUNTIME_INVISIBLE_TYPE_ANNOTATIONS.Equals(attributeName))
		  {
			invisibleTypeAnnotationOffsets = readTypeAnnotations(methodVisitor, context, currentOffset, false);
			// Same comment as above for the RuntimeVisibleTypeAnnotations attribute.
		  }
		  else if (Constants.STACK_MAP_TABLE.Equals(attributeName))
		  {
			if ((context.parsingOptions & SKIP_FRAMES) == 0)
			{
			  stackMapFrameOffset = currentOffset + 2;
			  stackMapTableEndOffset = currentOffset + attributeLength;
			}
			// Here we do not extract the labels corresponding to the attribute content. This would
			// require a full parsing of the attribute, which would need to be repeated when parsing
			// the bytecode instructions (see below). Instead, the content of the attribute is read one
			// frame at a time (i.e. after a frame has been visited, the next frame is read), and the
			// labels it contains are also extracted one frame at a time. Thanks to the ordering of
			// frames, having only a "one frame lookahead" is not a problem, i.e. it is not possible to
			// see an offset smaller than the offset of the current instruction and for which no Label
			// exist. Except for UNINITIALIZED type offsets. We solve this by parsing the stack map
			// table without a full decoding (see below).
		  }
		  else if ("StackMap".Equals(attributeName))
		  {
			if ((context.parsingOptions & SKIP_FRAMES) == 0)
			{
			  stackMapFrameOffset = currentOffset + 2;
			  stackMapTableEndOffset = currentOffset + attributeLength;
			  compressedFrames = false;
			}
			// IMPORTANT! Here we assume that the frames are ordered, as in the StackMapTable attribute,
			// although this is not guaranteed by the attribute format. This allows an incremental
			// extraction of the labels corresponding to this attribute (see the comment above for the
			// StackMapTable attribute).
		  }
		  else
		  {
			Attribute attribute = readAttribute(context.attributePrototypes, attributeName, currentOffset, attributeLength, charBuffer, codeOffset, labels);
			attribute.nextAttribute = attributes;
			attributes = attribute;
		  }
		  currentOffset += attributeLength;
		}

		// Initialize the context fields related to stack map frames, and generate the first
		// (implicit) stack map frame, if needed.
		bool expandFrames = (context.parsingOptions & EXPAND_FRAMES) != 0;
		if (stackMapFrameOffset != 0)
		{
		  // The bytecode offset of the first explicit frame is not offset_delta + 1 but only
		  // offset_delta. Setting the implicit frame offset to -1 allows us to use of the
		  // "offset_delta + 1" rule in all cases.
		  context.currentFrameOffset = -1;
		  context.currentFrameType = 0;
		  context.currentFrameLocalCount = 0;
		  context.currentFrameLocalCountDelta = 0;
		  context.currentFrameLocalTypes = new object[maxLocals];
		  context.currentFrameStackCount = 0;
		  context.currentFrameStackTypes = new object[maxStack];
		  if (expandFrames)
		  {
			computeImplicitFrame(context);
		  }
		  // Find the labels for UNINITIALIZED frame types. Instead of decoding each element of the
		  // stack map table, we look for 3 consecutive bytes that "look like" an UNINITIALIZED type
		  // (tag ITEM_Uninitialized, offset within bytecode bounds, NEW instruction at this offset).
		  // We may find false positives (i.e. not real UNINITIALIZED types), but this should be rare,
		  // and the only consequence will be the creation of an unneeded label. This is better than
		  // creating a label for each NEW instruction, and faster than fully decoding the whole stack
		  // map table.
		  for (int offset = stackMapFrameOffset; offset < stackMapTableEndOffset - 2; ++offset)
		  {
			if (classBuffer[offset] == Frame.ITEM_UNINITIALIZED)
			{
			  int potentialBytecodeOffset = readUnsignedShort(offset + 1);
			  if (potentialBytecodeOffset >= 0 && potentialBytecodeOffset < codeLength && (classBuffer[bytecodeStartOffset + potentialBytecodeOffset] & 0xFF) == Opcodes.NEW)
			  {
				createLabel(potentialBytecodeOffset, labels);
			  }
			}
		  }
		}
		if (expandFrames && (context.parsingOptions & EXPAND_ASM_INSNS) != 0)
		{
		  // Expanding the ASM specific instructions can introduce F_INSERT frames, even if the method
		  // does not currently have any frame. These inserted frames must be computed by simulating the
		  // effect of the bytecode instructions, one by one, starting from the implicit first frame.
		  // For this, MethodWriter needs to know maxLocals before the first instruction is visited. To
		  // ensure this, we visit the implicit first frame here (passing only maxLocals - the rest is
		  // computed in MethodWriter).
		  methodVisitor.visitFrame(Opcodes.F_NEW, maxLocals, null, 0, null);
		}

		// Visit the bytecode instructions. First, introduce state variables for the incremental parsing
		// of the type annotations.

		// Index of the next runtime visible type annotation to read (in the
		// visibleTypeAnnotationOffsets array).
		int currentVisibleTypeAnnotationIndex = 0;
		// The bytecode offset of the next runtime visible type annotation to read, or -1.
		int currentVisibleTypeAnnotationBytecodeOffset = getTypeAnnotationBytecodeOffset(visibleTypeAnnotationOffsets, 0);
		// Index of the next runtime invisible type annotation to read (in the
		// invisibleTypeAnnotationOffsets array).
		int currentInvisibleTypeAnnotationIndex = 0;
		// The bytecode offset of the next runtime invisible type annotation to read, or -1.
		int currentInvisibleTypeAnnotationBytecodeOffset = getTypeAnnotationBytecodeOffset(invisibleTypeAnnotationOffsets, 0);

		// Whether a F_INSERT stack map frame must be inserted before the current instruction.
		bool insertFrame = false;

		// The delta to subtract from a goto_w or jsr_w opcode to get the corresponding goto or jsr
		// opcode, or 0 if goto_w and jsr_w must be left unchanged (i.e. when expanding ASM specific
		// instructions).
		int wideJumpOpcodeDelta = (context.parsingOptions & EXPAND_ASM_INSNS) == 0 ? Constants.WIDE_JUMP_OPCODE_DELTA : 0;

		currentOffset = bytecodeStartOffset;
		while (currentOffset < bytecodeEndOffset)
		{
		  int currentBytecodeOffset = currentOffset - bytecodeStartOffset;

		  // Visit the label and the line number(s) for this bytecode offset, if any.
		  Label currentLabel = labels[currentBytecodeOffset];
		  if (currentLabel != null)
		  {
			currentLabel.accept(methodVisitor, (context.parsingOptions & SKIP_DEBUG) == 0);
		  }

		  // Visit the stack map frame for this bytecode offset, if any.
		  while (stackMapFrameOffset != 0 && (context.currentFrameOffset == currentBytecodeOffset || context.currentFrameOffset == -1))
		  {
			// If there is a stack map frame for this offset, make methodVisitor visit it, and read the
			// next stack map frame if there is one.
			if (context.currentFrameOffset != -1)
			{
			  if (!compressedFrames || expandFrames)
			  {
				methodVisitor.visitFrame(Opcodes.F_NEW, context.currentFrameLocalCount, context.currentFrameLocalTypes, context.currentFrameStackCount, context.currentFrameStackTypes);
			  }
			  else
			  {
				methodVisitor.visitFrame(context.currentFrameType, context.currentFrameLocalCountDelta, context.currentFrameLocalTypes, context.currentFrameStackCount, context.currentFrameStackTypes);
			  }
			  // Since there is already a stack map frame for this bytecode offset, there is no need to
			  // insert a new one.
			  insertFrame = false;
			}
			if (stackMapFrameOffset < stackMapTableEndOffset)
			{
			  stackMapFrameOffset = readStackMapFrame(stackMapFrameOffset, compressedFrames, expandFrames, context);
			}
			else
			{
			  stackMapFrameOffset = 0;
			}
		  }

		  // Insert a stack map frame for this bytecode offset, if requested by setting insertFrame to
		  // true during the previous iteration. The actual frame content is computed in MethodWriter.
		  if (insertFrame)
		  {
			if ((context.parsingOptions & EXPAND_FRAMES) != 0)
			{
			  methodVisitor.visitFrame(Constants.F_INSERT, 0, null, 0, null);
			}
			insertFrame = false;
		  }

		  // Visit the instruction at this bytecode offset.
		  int opcode = classBuffer[currentOffset] & 0xFF;
		  switch (opcode)
		  {
			case Opcodes.NOP:
			case Opcodes.ACONST_NULL:
			case Opcodes.ICONST_M1:
			case Opcodes.ICONST_0:
			case Opcodes.ICONST_1:
			case Opcodes.ICONST_2:
			case Opcodes.ICONST_3:
			case Opcodes.ICONST_4:
			case Opcodes.ICONST_5:
			case Opcodes.LCONST_0:
			case Opcodes.LCONST_1:
			case Opcodes.FCONST_0:
			case Opcodes.FCONST_1:
			case Opcodes.FCONST_2:
			case Opcodes.DCONST_0:
			case Opcodes.DCONST_1:
			case Opcodes.IALOAD:
			case Opcodes.LALOAD:
			case Opcodes.FALOAD:
			case Opcodes.DALOAD:
			case Opcodes.AALOAD:
			case Opcodes.BALOAD:
			case Opcodes.CALOAD:
			case Opcodes.SALOAD:
			case Opcodes.IASTORE:
			case Opcodes.LASTORE:
			case Opcodes.FASTORE:
			case Opcodes.DASTORE:
			case Opcodes.AASTORE:
			case Opcodes.BASTORE:
			case Opcodes.CASTORE:
			case Opcodes.SASTORE:
			case Opcodes.POP:
			case Opcodes.POP2:
			case Opcodes.DUP:
			case Opcodes.DUP_X1:
			case Opcodes.DUP_X2:
			case Opcodes.DUP2:
			case Opcodes.DUP2_X1:
			case Opcodes.DUP2_X2:
			case Opcodes.SWAP:
			case Opcodes.IADD:
			case Opcodes.LADD:
			case Opcodes.FADD:
			case Opcodes.DADD:
			case Opcodes.ISUB:
			case Opcodes.LSUB:
			case Opcodes.FSUB:
			case Opcodes.DSUB:
			case Opcodes.IMUL:
			case Opcodes.LMUL:
			case Opcodes.FMUL:
			case Opcodes.DMUL:
			case Opcodes.IDIV:
			case Opcodes.LDIV:
			case Opcodes.FDIV:
			case Opcodes.DDIV:
			case Opcodes.IREM:
			case Opcodes.LREM:
			case Opcodes.FREM:
			case Opcodes.DREM:
			case Opcodes.INEG:
			case Opcodes.LNEG:
			case Opcodes.FNEG:
			case Opcodes.DNEG:
			case Opcodes.ISHL:
			case Opcodes.LSHL:
			case Opcodes.ISHR:
			case Opcodes.LSHR:
			case Opcodes.IUSHR:
			case Opcodes.LUSHR:
			case Opcodes.IAND:
			case Opcodes.LAND:
			case Opcodes.IOR:
			case Opcodes.LOR:
			case Opcodes.IXOR:
			case Opcodes.LXOR:
			case Opcodes.I2L:
			case Opcodes.I2F:
			case Opcodes.I2D:
			case Opcodes.L2I:
			case Opcodes.L2F:
			case Opcodes.L2D:
			case Opcodes.F2I:
			case Opcodes.F2L:
			case Opcodes.F2D:
			case Opcodes.D2I:
			case Opcodes.D2L:
			case Opcodes.D2F:
			case Opcodes.I2B:
			case Opcodes.I2C:
			case Opcodes.I2S:
			case Opcodes.LCMP:
			case Opcodes.FCMPL:
			case Opcodes.FCMPG:
			case Opcodes.DCMPL:
			case Opcodes.DCMPG:
			case Opcodes.IRETURN:
			case Opcodes.LRETURN:
			case Opcodes.FRETURN:
			case Opcodes.DRETURN:
			case Opcodes.ARETURN:
			case Opcodes.RETURN:
			case Opcodes.ARRAYLENGTH:
			case Opcodes.ATHROW:
			case Opcodes.MONITORENTER:
			case Opcodes.MONITOREXIT:
			  methodVisitor.visitInsn(opcode);
			  currentOffset += 1;
			  break;
			case Constants.ILOAD_0:
			case Constants.ILOAD_1:
			case Constants.ILOAD_2:
			case Constants.ILOAD_3:
			case Constants.LLOAD_0:
			case Constants.LLOAD_1:
			case Constants.LLOAD_2:
			case Constants.LLOAD_3:
			case Constants.FLOAD_0:
			case Constants.FLOAD_1:
			case Constants.FLOAD_2:
			case Constants.FLOAD_3:
			case Constants.DLOAD_0:
			case Constants.DLOAD_1:
			case Constants.DLOAD_2:
			case Constants.DLOAD_3:
			case Constants.ALOAD_0:
			case Constants.ALOAD_1:
			case Constants.ALOAD_2:
			case Constants.ALOAD_3:
			  opcode -= Constants.ILOAD_0;
			  methodVisitor.visitVarInsn(Opcodes.ILOAD + (opcode >> 2), opcode & 0x3);
			  currentOffset += 1;
			  break;
			case Constants.ISTORE_0:
			case Constants.ISTORE_1:
			case Constants.ISTORE_2:
			case Constants.ISTORE_3:
			case Constants.LSTORE_0:
			case Constants.LSTORE_1:
			case Constants.LSTORE_2:
			case Constants.LSTORE_3:
			case Constants.FSTORE_0:
			case Constants.FSTORE_1:
			case Constants.FSTORE_2:
			case Constants.FSTORE_3:
			case Constants.DSTORE_0:
			case Constants.DSTORE_1:
			case Constants.DSTORE_2:
			case Constants.DSTORE_3:
			case Constants.ASTORE_0:
			case Constants.ASTORE_1:
			case Constants.ASTORE_2:
			case Constants.ASTORE_3:
			  opcode -= Constants.ISTORE_0;
			  methodVisitor.visitVarInsn(Opcodes.ISTORE + (opcode >> 2), opcode & 0x3);
			  currentOffset += 1;
			  break;
			case Opcodes.IFEQ:
			case Opcodes.IFNE:
			case Opcodes.IFLT:
			case Opcodes.IFGE:
			case Opcodes.IFGT:
			case Opcodes.IFLE:
			case Opcodes.IF_ICMPEQ:
			case Opcodes.IF_ICMPNE:
			case Opcodes.IF_ICMPLT:
			case Opcodes.IF_ICMPGE:
			case Opcodes.IF_ICMPGT:
			case Opcodes.IF_ICMPLE:
			case Opcodes.IF_ACMPEQ:
			case Opcodes.IF_ACMPNE:
			case Opcodes.GOTO:
			case Opcodes.JSR:
			case Opcodes.IFNULL:
			case Opcodes.IFNONNULL:
			  methodVisitor.visitJumpInsn(opcode, labels[currentBytecodeOffset + readShort(currentOffset + 1)]);
			  currentOffset += 3;
			  break;
			case Constants.GOTO_W:
			case Constants.JSR_W:
			  methodVisitor.visitJumpInsn(opcode - wideJumpOpcodeDelta, labels[currentBytecodeOffset + readInt(currentOffset + 1)]);
			  currentOffset += 5;
			  break;
			case Constants.ASM_IFEQ:
			case Constants.ASM_IFNE:
			case Constants.ASM_IFLT:
			case Constants.ASM_IFGE:
			case Constants.ASM_IFGT:
			case Constants.ASM_IFLE:
			case Constants.ASM_IF_ICMPEQ:
			case Constants.ASM_IF_ICMPNE:
			case Constants.ASM_IF_ICMPLT:
			case Constants.ASM_IF_ICMPGE:
			case Constants.ASM_IF_ICMPGT:
			case Constants.ASM_IF_ICMPLE:
			case Constants.ASM_IF_ACMPEQ:
			case Constants.ASM_IF_ACMPNE:
			case Constants.ASM_GOTO:
			case Constants.ASM_JSR:
			case Constants.ASM_IFNULL:
			case Constants.ASM_IFNONNULL:
			{
				// A forward jump with an offset > 32767. In this case we automatically replace ASM_GOTO
				// with GOTO_W, ASM_JSR with JSR_W and ASM_IFxxx <l> with IFNOTxxx <L> GOTO_W <l> L:...,
				// where IFNOTxxx is the "opposite" opcode of ASMS_IFxxx (e.g. IFNE for ASM_IFEQ) and
				// where <L> designates the instruction just after the GOTO_W.
				// First, change the ASM specific opcodes ASM_IFEQ ... ASM_JSR, ASM_IFNULL and
				// ASM_IFNONNULL to IFEQ ... JSR, IFNULL and IFNONNULL.
				opcode = opcode < Constants.ASM_IFNULL ? opcode - Constants.ASM_OPCODE_DELTA : opcode - Constants.ASM_IFNULL_OPCODE_DELTA;
				Label target = labels[currentBytecodeOffset + readUnsignedShort(currentOffset + 1)];
				if (opcode == Opcodes.GOTO || opcode == Opcodes.JSR)
				{
				  // Replace GOTO with GOTO_W and JSR with JSR_W.
				  methodVisitor.visitJumpInsn(opcode + Constants.WIDE_JUMP_OPCODE_DELTA, target);
				}
				else
				{
				  // Compute the "opposite" of opcode. This can be done by flipping the least
				  // significant bit for IFNULL and IFNONNULL, and similarly for IFEQ ... IF_ACMPEQ
				  // (with a pre and post offset by 1).
				  opcode = opcode < Opcodes.GOTO ? ((opcode + 1) ^ 1) - 1 : opcode ^ 1;
				  Label endif = createLabel(currentBytecodeOffset + 3, labels);
				  methodVisitor.visitJumpInsn(opcode, endif);
				  methodVisitor.visitJumpInsn(Constants.GOTO_W, target);
				  // endif designates the instruction just after GOTO_W, and is visited as part of the
				  // next instruction. Since it is a jump target, we need to insert a frame here.
				  insertFrame = true;
				}
				currentOffset += 3;
				break;
			}
			case Constants.ASM_GOTO_W:
			  // Replace ASM_GOTO_W with GOTO_W.
			  methodVisitor.visitJumpInsn(Constants.GOTO_W, labels[currentBytecodeOffset + readInt(currentOffset + 1)]);
			  // The instruction just after is a jump target (because ASM_GOTO_W is used in patterns
			  // IFNOTxxx <L> ASM_GOTO_W <l> L:..., see MethodWriter), so we need to insert a frame
			  // here.
			  insertFrame = true;
			  currentOffset += 5;
			  break;
			case Constants.WIDE:
			  opcode = classBuffer[currentOffset + 1] & 0xFF;
			  if (opcode == Opcodes.IINC)
			  {
				methodVisitor.visitIincInsn(readUnsignedShort(currentOffset + 2), readShort(currentOffset + 4));
				currentOffset += 6;
			  }
			  else
			  {
				methodVisitor.visitVarInsn(opcode, readUnsignedShort(currentOffset + 2));
				currentOffset += 4;
			  }
			  break;
			case Opcodes.TABLESWITCH:
			{
				// Skip 0 to 3 padding bytes.
				currentOffset += 4 - (currentBytecodeOffset & 3);
				// Read the instruction.
				Label defaultLabel = labels[currentBytecodeOffset + readInt(currentOffset)];
				int low = readInt(currentOffset + 4);
				int high = readInt(currentOffset + 8);
				currentOffset += 12;
				Label[] table = new Label[high - low + 1];
				for (int i = 0; i < table.Length; ++i)
				{
				  table[i] = labels[currentBytecodeOffset + readInt(currentOffset)];
				  currentOffset += 4;
				}
				methodVisitor.visitTableSwitchInsn(low, high, defaultLabel, table);
				break;
			}
			case Opcodes.LOOKUPSWITCH:
			{
				// Skip 0 to 3 padding bytes.
				currentOffset += 4 - (currentBytecodeOffset & 3);
				// Read the instruction.
				Label defaultLabel = labels[currentBytecodeOffset + readInt(currentOffset)];
				int numPairs = readInt(currentOffset + 4);
				currentOffset += 8;
				int[] keys = new int[numPairs];
				Label[] values = new Label[numPairs];
				for (int i = 0; i < numPairs; ++i)
				{
				  keys[i] = readInt(currentOffset);
				  values[i] = labels[currentBytecodeOffset + readInt(currentOffset + 4)];
				  currentOffset += 8;
				}
				methodVisitor.visitLookupSwitchInsn(defaultLabel, keys, values);
				break;
			}
			case Opcodes.ILOAD:
			case Opcodes.LLOAD:
			case Opcodes.FLOAD:
			case Opcodes.DLOAD:
			case Opcodes.ALOAD:
			case Opcodes.ISTORE:
			case Opcodes.LSTORE:
			case Opcodes.FSTORE:
			case Opcodes.DSTORE:
			case Opcodes.ASTORE:
			case Opcodes.RET:
			  methodVisitor.visitVarInsn(opcode, classBuffer[currentOffset + 1] & 0xFF);
			  currentOffset += 2;
			  break;
			case Opcodes.BIPUSH:
			case Opcodes.NEWARRAY:
			  methodVisitor.visitIntInsn(opcode, classBuffer[currentOffset + 1]);
			  currentOffset += 2;
			  break;
			case Opcodes.SIPUSH:
			  methodVisitor.visitIntInsn(opcode, readShort(currentOffset + 1));
			  currentOffset += 3;
			  break;
			case Opcodes.LDC:
			  methodVisitor.visitLdcInsn(readConst(classBuffer[currentOffset + 1] & 0xFF, charBuffer));
			  currentOffset += 2;
			  break;
			case Constants.LDC_W:
			case Constants.LDC2_W:
			  methodVisitor.visitLdcInsn(readConst(readUnsignedShort(currentOffset + 1), charBuffer));
			  currentOffset += 3;
			  break;
			case Opcodes.GETSTATIC:
			case Opcodes.PUTSTATIC:
			case Opcodes.GETFIELD:
			case Opcodes.PUTFIELD:
			case Opcodes.INVOKEVIRTUAL:
			case Opcodes.INVOKESPECIAL:
			case Opcodes.INVOKESTATIC:
			case Opcodes.INVOKEINTERFACE:
			{
				int cpInfoOffset = cpInfoOffsets[readUnsignedShort(currentOffset + 1)];
				int nameAndTypeCpInfoOffset = cpInfoOffsets[readUnsignedShort(cpInfoOffset + 2)];
				string owner = readClass(cpInfoOffset, charBuffer);
				string name = readUTF8(nameAndTypeCpInfoOffset, charBuffer);
				string descriptor = readUTF8(nameAndTypeCpInfoOffset + 2, charBuffer);
				if (opcode < Opcodes.INVOKEVIRTUAL)
				{
				  methodVisitor.visitFieldInsn(opcode, owner, name, descriptor);
				}
				else
				{
				  bool isInterface = classBuffer[cpInfoOffset - 1] == Symbol.CONSTANT_INTERFACE_METHODREF_TAG;
				  methodVisitor.visitMethodInsn(opcode, owner, name, descriptor, isInterface);
				}
				if (opcode == Opcodes.INVOKEINTERFACE)
				{
				  currentOffset += 5;
				}
				else
				{
				  currentOffset += 3;
				}
				break;
			}
			case Opcodes.INVOKEDYNAMIC:
			{
				int cpInfoOffset = cpInfoOffsets[readUnsignedShort(currentOffset + 1)];
				int nameAndTypeCpInfoOffset = cpInfoOffsets[readUnsignedShort(cpInfoOffset + 2)];
				string name = readUTF8(nameAndTypeCpInfoOffset, charBuffer);
				string descriptor = readUTF8(nameAndTypeCpInfoOffset + 2, charBuffer);
				int bootstrapMethodOffset = bootstrapMethodOffsets[readUnsignedShort(cpInfoOffset)];
				Handle handle = (Handle) readConst(readUnsignedShort(bootstrapMethodOffset), charBuffer);
				object[] bootstrapMethodArguments = new object[readUnsignedShort(bootstrapMethodOffset + 2)];
				bootstrapMethodOffset += 4;
				for (int i = 0; i < bootstrapMethodArguments.Length; i++)
				{
				  bootstrapMethodArguments[i] = readConst(readUnsignedShort(bootstrapMethodOffset), charBuffer);
				  bootstrapMethodOffset += 2;
				}
				methodVisitor.visitInvokeDynamicInsn(name, descriptor, handle, bootstrapMethodArguments);
				currentOffset += 5;
				break;
			}
			case Opcodes.NEW:
			case Opcodes.ANEWARRAY:
			case Opcodes.CHECKCAST:
			case Opcodes.INSTANCEOF:
			  methodVisitor.visitTypeInsn(opcode, readClass(currentOffset + 1, charBuffer));
			  currentOffset += 3;
			  break;
			case Opcodes.IINC:
			  methodVisitor.visitIincInsn(classBuffer[currentOffset + 1] & 0xFF, classBuffer[currentOffset + 2]);
			  currentOffset += 3;
			  break;
			case Opcodes.MULTIANEWARRAY:
			  methodVisitor.visitMultiANewArrayInsn(readClass(currentOffset + 1, charBuffer), classBuffer[currentOffset + 3] & 0xFF);
			  currentOffset += 4;
			  break;
			default:
			  throw new ("AssertionError");
		  }

		  // Visit the runtime visible instruction annotations, if any.
		  while (visibleTypeAnnotationOffsets != null && currentVisibleTypeAnnotationIndex < visibleTypeAnnotationOffsets.Length && currentVisibleTypeAnnotationBytecodeOffset <= currentBytecodeOffset)
		  {
			if (currentVisibleTypeAnnotationBytecodeOffset == currentBytecodeOffset)
			{
			  // Parse the target_type, target_info and target_path fields.
			  int currentAnnotationOffset = readTypeAnnotationTarget(context, visibleTypeAnnotationOffsets[currentVisibleTypeAnnotationIndex]);
			  // Parse the type_index field.
			  string annotationDescriptor = readUTF8(currentAnnotationOffset, charBuffer);
			  currentAnnotationOffset += 2;
			  // Parse num_element_value_pairs and element_value_pairs and visit these values.
			  readElementValues(methodVisitor.visitInsnAnnotation(context.currentTypeAnnotationTarget, context.currentTypeAnnotationTargetPath, annotationDescriptor, true), currentAnnotationOffset, true, charBuffer);
			}
			currentVisibleTypeAnnotationBytecodeOffset = getTypeAnnotationBytecodeOffset(visibleTypeAnnotationOffsets, ++currentVisibleTypeAnnotationIndex);
		  }

		  // Visit the runtime invisible instruction annotations, if any.
		  while (invisibleTypeAnnotationOffsets != null && currentInvisibleTypeAnnotationIndex < invisibleTypeAnnotationOffsets.Length && currentInvisibleTypeAnnotationBytecodeOffset <= currentBytecodeOffset)
		  {
			if (currentInvisibleTypeAnnotationBytecodeOffset == currentBytecodeOffset)
			{
			  // Parse the target_type, target_info and target_path fields.
			  int currentAnnotationOffset = readTypeAnnotationTarget(context, invisibleTypeAnnotationOffsets[currentInvisibleTypeAnnotationIndex]);
			  // Parse the type_index field.
			  string annotationDescriptor = readUTF8(currentAnnotationOffset, charBuffer);
			  currentAnnotationOffset += 2;
			  // Parse num_element_value_pairs and element_value_pairs and visit these values.
			  readElementValues(methodVisitor.visitInsnAnnotation(context.currentTypeAnnotationTarget, context.currentTypeAnnotationTargetPath, annotationDescriptor, false), currentAnnotationOffset, true, charBuffer);
			}
			currentInvisibleTypeAnnotationBytecodeOffset = getTypeAnnotationBytecodeOffset(invisibleTypeAnnotationOffsets, ++currentInvisibleTypeAnnotationIndex);
		  }
		}
		if (labels[codeLength] != null)
		{
		  methodVisitor.visitLabel(labels[codeLength]);
		}

		// Visit LocalVariableTable and LocalVariableTypeTable attributes.
		if (localVariableTableOffset != 0 && (context.parsingOptions & SKIP_DEBUG) == 0)
		{
		  // The (start_pc, index, signature_index) fields of each entry of the LocalVariableTypeTable.
		  int[] typeTable = null;
		  if (localVariableTypeTableOffset != 0)
		  {
			typeTable = new int[readUnsignedShort(localVariableTypeTableOffset) * 3];
			currentOffset = localVariableTypeTableOffset + 2;
			int typeTableIndex = typeTable.Length;
			while (typeTableIndex > 0)
			{
			  // Store the offset of 'signature_index', and the value of 'index' and 'start_pc'.
			  typeTable[--typeTableIndex] = currentOffset + 6;
			  typeTable[--typeTableIndex] = readUnsignedShort(currentOffset + 8);
			  typeTable[--typeTableIndex] = readUnsignedShort(currentOffset);
			  currentOffset += 10;
			}
		  }
		  int localVariableTableLength = readUnsignedShort(localVariableTableOffset);
		  currentOffset = localVariableTableOffset + 2;
		  while (localVariableTableLength-- > 0)
		  {
			int startPc = readUnsignedShort(currentOffset);
			int length = readUnsignedShort(currentOffset + 2);
			string name = readUTF8(currentOffset + 4, charBuffer);
			string descriptor = readUTF8(currentOffset + 6, charBuffer);
			int index = readUnsignedShort(currentOffset + 8);
			currentOffset += 10;
			string signature = null;
			if (typeTable != null)
			{
			  for (int i = 0; i < typeTable.Length; i += 3)
			  {
				if (typeTable[i] == startPc && typeTable[i + 1] == index)
				{
				  signature = readUTF8(typeTable[i + 2], charBuffer);
				  break;
				}
			  }
			}
			methodVisitor.visitLocalVariable(name, descriptor, signature, labels[startPc], labels[startPc + length], index);
		  }
		}

		// Visit the local variable type annotations of the RuntimeVisibleTypeAnnotations attribute.
		if (visibleTypeAnnotationOffsets != null)
		{
		  foreach (int typeAnnotationOffset in visibleTypeAnnotationOffsets)
		  {
			int targetType = readByte(typeAnnotationOffset);
			if (targetType == TypeReference.LOCAL_VARIABLE || targetType == TypeReference.RESOURCE_VARIABLE)
			{
			  // Parse the target_type, target_info and target_path fields.
			  currentOffset = readTypeAnnotationTarget(context, typeAnnotationOffset);
			  // Parse the type_index field.
			  string annotationDescriptor = readUTF8(currentOffset, charBuffer);
			  currentOffset += 2;
			  // Parse num_element_value_pairs and element_value_pairs and visit these values.
			  readElementValues(methodVisitor.visitLocalVariableAnnotation(context.currentTypeAnnotationTarget, context.currentTypeAnnotationTargetPath, context.currentLocalVariableAnnotationRangeStarts, context.currentLocalVariableAnnotationRangeEnds, context.currentLocalVariableAnnotationRangeIndices, annotationDescriptor, true), currentOffset, true, charBuffer);
			}
		  }
		}

		// Visit the local variable type annotations of the RuntimeInvisibleTypeAnnotations attribute.
		if (invisibleTypeAnnotationOffsets != null)
		{
		  foreach (int typeAnnotationOffset in invisibleTypeAnnotationOffsets)
		  {
			int targetType = readByte(typeAnnotationOffset);
			if (targetType == TypeReference.LOCAL_VARIABLE || targetType == TypeReference.RESOURCE_VARIABLE)
			{
			  // Parse the target_type, target_info and target_path fields.
			  currentOffset = readTypeAnnotationTarget(context, typeAnnotationOffset);
			  // Parse the type_index field.
			  string annotationDescriptor = readUTF8(currentOffset, charBuffer);
			  currentOffset += 2;
			  // Parse num_element_value_pairs and element_value_pairs and visit these values.
			  readElementValues(methodVisitor.visitLocalVariableAnnotation(context.currentTypeAnnotationTarget, context.currentTypeAnnotationTargetPath, context.currentLocalVariableAnnotationRangeStarts, context.currentLocalVariableAnnotationRangeEnds, context.currentLocalVariableAnnotationRangeIndices, annotationDescriptor, false), currentOffset, true, charBuffer);
			}
		  }
		}

		// Visit the non standard attributes.
		while (attributes != null)
		{
		  // Copy and reset the nextAttribute field so that it can also be used in MethodWriter.
		  Attribute nextAttribute = attributes.nextAttribute;
		  attributes.nextAttribute = null;
		  methodVisitor.visitAttribute(attributes);
		  attributes = nextAttribute;
		}

		// Visit the max stack and max locals values.
		methodVisitor.visitMaxs(maxStack, maxLocals);
	  }

	  /// <summary>
	  /// Returns the label corresponding to the given bytecode offset. The default implementation of
	  /// this method creates a label for the given offset if it has not been already created.
	  /// </summary>
	  /// <param name="bytecodeOffset"> a bytecode offset in a method. </param>
	  /// <param name="labels"> the already created labels, indexed by their offset. If a label already exists
	  ///     for bytecodeOffset this method must not create a new one. Otherwise it must store the new
	  ///     label in this array. </param>
	  /// <returns> a non null Label, which must be equal to labels[bytecodeOffset]. </returns>
	  public virtual Label readLabel(int bytecodeOffset, Label[] labels)
	  {
		if (labels[bytecodeOffset] == null)
		{
		  labels[bytecodeOffset] = new Label();
		}
		return labels[bytecodeOffset];
	  }

	  /// <summary>
	  /// Creates a label without the <seealso cref="Label.FLAG_DEBUG_ONLY"/> flag set, for the given bytecode
	  /// offset. The label is created with a call to <seealso cref="readLabel"/> and its {@link
	  /// Label#FLAG_DEBUG_ONLY} flag is cleared.
	  /// </summary>
	  /// <param name="bytecodeOffset"> a bytecode offset in a method. </param>
	  /// <param name="labels"> the already created labels, indexed by their offset. </param>
	  /// <returns> a Label without the <seealso cref="Label.FLAG_DEBUG_ONLY"/> flag set. </returns>
	  private Label createLabel(int bytecodeOffset, Label[] labels)
	  {
		Label label = readLabel(bytecodeOffset, labels);
		label.flags &= (short)(~Label.FLAG_DEBUG_ONLY);
		return label;
	  }

	  /// <summary>
	  /// Creates a label with the <seealso cref="Label.FLAG_DEBUG_ONLY"/> flag set, if there is no already
	  /// existing label for the given bytecode offset (otherwise does nothing). The label is created
	  /// with a call to <seealso cref="readLabel"/>.
	  /// </summary>
	  /// <param name="bytecodeOffset"> a bytecode offset in a method. </param>
	  /// <param name="labels"> the already created labels, indexed by their offset. </param>
	  private void createDebugLabel(int bytecodeOffset, Label[] labels)
	  {
		if (labels[bytecodeOffset] == null)
		{
		  readLabel(bytecodeOffset, labels).flags |= (short)Label.FLAG_DEBUG_ONLY;
		}
	  }

	  // ----------------------------------------------------------------------------------------------
	  // Methods to parse annotations, type annotations and parameter annotations
	  // ----------------------------------------------------------------------------------------------

	  /// <summary>
	  /// Parses a Runtime[In]VisibleTypeAnnotations attribute to find the offset of each type_annotation
	  /// entry it contains, to find the corresponding labels, and to visit the try catch block
	  /// annotations.
	  /// </summary>
	  /// <param name="methodVisitor"> the method visitor to be used to visit the try catch block annotations. </param>
	  /// <param name="context"> information about the class being parsed. </param>
	  /// <param name="runtimeTypeAnnotationsOffset"> the start offset of a Runtime[In]VisibleTypeAnnotations
	  ///     attribute, excluding the attribute_info's attribute_name_index and attribute_length fields. </param>
	  /// <param name="visible"> true if the attribute to parse is a RuntimeVisibleTypeAnnotations attribute,
	  ///     false it is a RuntimeInvisibleTypeAnnotations attribute. </param>
	  /// <returns> the start offset of each entry of the Runtime[In]VisibleTypeAnnotations_attribute's
	  ///     'annotations' array field. </returns>
	  private int[] readTypeAnnotations(MethodVisitor methodVisitor, Context context, int runtimeTypeAnnotationsOffset, bool visible)
	  {
		char[] charBuffer = context.charBuffer;
		int currentOffset = runtimeTypeAnnotationsOffset;
		// Read the num_annotations field and create an array to store the type_annotation offsets.
		int[] typeAnnotationsOffsets = new int[readUnsignedShort(currentOffset)];
		currentOffset += 2;
		// Parse the 'annotations' array field.
		for (int i = 0; i < typeAnnotationsOffsets.Length; ++i)
		{
		  typeAnnotationsOffsets[i] = currentOffset;
		  // Parse the type_annotation's target_type and the target_info fields. The size of the
		  // target_info field depends on the value of target_type.
		  int targetType = readInt(currentOffset);
		  switch ((int)((uint)targetType >> 24))
		  {
			case TypeReference.LOCAL_VARIABLE:
			case TypeReference.RESOURCE_VARIABLE:
			  // A localvar_target has a variable size, which depends on the value of their table_length
			  // field. It also references bytecode offsets, for which we need labels.
			  int tableLength = readUnsignedShort(currentOffset + 1);
			  currentOffset += 3;
			  while (tableLength-- > 0)
			  {
				int startPc = readUnsignedShort(currentOffset);
				int length = readUnsignedShort(currentOffset + 2);
				// Skip the index field (2 bytes).
				currentOffset += 6;
				createLabel(startPc, context.currentMethodLabels);
				createLabel(startPc + length, context.currentMethodLabels);
			  }
			  break;
			case TypeReference.CAST:
			case TypeReference.CONSTRUCTOR_INVOCATION_TYPE_ARGUMENT:
			case TypeReference.METHOD_INVOCATION_TYPE_ARGUMENT:
			case TypeReference.CONSTRUCTOR_REFERENCE_TYPE_ARGUMENT:
			case TypeReference.METHOD_REFERENCE_TYPE_ARGUMENT:
			  currentOffset += 4;
			  break;
			case TypeReference.CLASS_EXTENDS:
			case TypeReference.CLASS_TYPE_PARAMETER_BOUND:
			case TypeReference.METHOD_TYPE_PARAMETER_BOUND:
			case TypeReference.THROWS:
			case TypeReference.EXCEPTION_PARAMETER:
			case TypeReference.INSTANCEOF:
			case TypeReference.NEW:
			case TypeReference.CONSTRUCTOR_REFERENCE:
			case TypeReference.METHOD_REFERENCE:
			  currentOffset += 3;
			  break;
			case TypeReference.CLASS_TYPE_PARAMETER:
			case TypeReference.METHOD_TYPE_PARAMETER:
			case TypeReference.METHOD_FORMAL_PARAMETER:
			case TypeReference.FIELD:
			case TypeReference.METHOD_RETURN:
			case TypeReference.METHOD_RECEIVER:
			default:
			  // TypeReference type which can't be used in Code attribute, or which is unknown.
			  throw new System.ArgumentException();
		  }
		  // Parse the rest of the type_annotation structure, starting with the target_path structure
		  // (whose size depends on its path_length field).
		  int pathLength = readByte(currentOffset);
		  if (((int)((uint)targetType >> 24)) == TypeReference.EXCEPTION_PARAMETER)
		  {
			// Parse the target_path structure and create a corresponding TypePath.
			TypePath path = pathLength == 0 ? null : new TypePath(classFileBuffer, currentOffset);
			currentOffset += 1 + 2 * pathLength;
			// Parse the type_index field.
			string annotationDescriptor = readUTF8(currentOffset, charBuffer);
			currentOffset += 2;
			// Parse num_element_value_pairs and element_value_pairs and visit these values.
			currentOffset = readElementValues(methodVisitor.visitTryCatchAnnotation(targetType & unchecked((int)0xFFFFFF00), path, annotationDescriptor, visible), currentOffset, true, charBuffer);
		  }
		  else
		  {
			// We don't want to visit the other target_type annotations, so we just skip them (which
			// requires some parsing because the element_value_pairs array has a variable size). First,
			// skip the target_path structure:
			currentOffset += 3 + 2 * pathLength;
			// Then skip the num_element_value_pairs and element_value_pairs fields (by reading them
			// with a null AnnotationVisitor).
			currentOffset = readElementValues(null, currentOffset, true, charBuffer);
		  }
		}
		return typeAnnotationsOffsets;
	  }

	  /// <summary>
	  /// Returns the bytecode offset corresponding to the specified JVMS 'type_annotation' structure, or
	  /// -1 if there is no such type_annotation of if it does not have a bytecode offset.
	  /// </summary>
	  /// <param name="typeAnnotationOffsets"> the offset of each 'type_annotation' entry in a
	  ///     Runtime[In]VisibleTypeAnnotations attribute, or {@literal null}. </param>
	  /// <param name="typeAnnotationIndex"> the index a 'type_annotation' entry in typeAnnotationOffsets. </param>
	  /// <returns> bytecode offset corresponding to the specified JVMS 'type_annotation' structure, or -1
	  ///     if there is no such type_annotation of if it does not have a bytecode offset. </returns>
	  private int getTypeAnnotationBytecodeOffset(int[] typeAnnotationOffsets, int typeAnnotationIndex)
	  {
		if (typeAnnotationOffsets == null || typeAnnotationIndex >= typeAnnotationOffsets.Length || readByte(typeAnnotationOffsets[typeAnnotationIndex]) < TypeReference.INSTANCEOF)
		{
		  return -1;
		}
		return readUnsignedShort(typeAnnotationOffsets[typeAnnotationIndex] + 1);
	  }

	  /// <summary>
	  /// Parses the header of a JVMS type_annotation structure to extract its target_type, target_info
	  /// and target_path (the result is stored in the given context), and returns the start offset of
	  /// the rest of the type_annotation structure.
	  /// </summary>
	  /// <param name="context"> information about the class being parsed. This is where the extracted
	  ///     target_type and target_path must be stored. </param>
	  /// <param name="typeAnnotationOffset"> the start offset of a type_annotation structure. </param>
	  /// <returns> the start offset of the rest of the type_annotation structure. </returns>
	  private int readTypeAnnotationTarget(Context context, int typeAnnotationOffset)
	  {
		int currentOffset = typeAnnotationOffset;
		// Parse and store the target_type structure.
		int targetType = readInt(typeAnnotationOffset);
		switch ((int)((uint)targetType >> 24))
		{
		  case TypeReference.CLASS_TYPE_PARAMETER:
		  case TypeReference.METHOD_TYPE_PARAMETER:
		  case TypeReference.METHOD_FORMAL_PARAMETER:
			targetType &= unchecked((int)0xFFFF0000);
			currentOffset += 2;
			break;
		  case TypeReference.FIELD:
		  case TypeReference.METHOD_RETURN:
		  case TypeReference.METHOD_RECEIVER:
			targetType &= unchecked((int)0xFF000000);
			currentOffset += 1;
			break;
		  case TypeReference.LOCAL_VARIABLE:
		  case TypeReference.RESOURCE_VARIABLE:
			targetType &= unchecked((int)0xFF000000);
			int tableLength = readUnsignedShort(currentOffset + 1);
			currentOffset += 3;
			context.currentLocalVariableAnnotationRangeStarts = new Label[tableLength];
			context.currentLocalVariableAnnotationRangeEnds = new Label[tableLength];
			context.currentLocalVariableAnnotationRangeIndices = new int[tableLength];
			for (int i = 0; i < tableLength; ++i)
			{
			  int startPc = readUnsignedShort(currentOffset);
			  int length = readUnsignedShort(currentOffset + 2);
			  int index = readUnsignedShort(currentOffset + 4);
			  currentOffset += 6;
			  context.currentLocalVariableAnnotationRangeStarts[i] = createLabel(startPc, context.currentMethodLabels);
			  context.currentLocalVariableAnnotationRangeEnds[i] = createLabel(startPc + length, context.currentMethodLabels);
			  context.currentLocalVariableAnnotationRangeIndices[i] = index;
			}
			break;
		  case TypeReference.CAST:
		  case TypeReference.CONSTRUCTOR_INVOCATION_TYPE_ARGUMENT:
		  case TypeReference.METHOD_INVOCATION_TYPE_ARGUMENT:
		  case TypeReference.CONSTRUCTOR_REFERENCE_TYPE_ARGUMENT:
		  case TypeReference.METHOD_REFERENCE_TYPE_ARGUMENT:
			targetType &= unchecked((int)0xFF0000FF);
			currentOffset += 4;
			break;
		  case TypeReference.CLASS_EXTENDS:
		  case TypeReference.CLASS_TYPE_PARAMETER_BOUND:
		  case TypeReference.METHOD_TYPE_PARAMETER_BOUND:
		  case TypeReference.THROWS:
		  case TypeReference.EXCEPTION_PARAMETER:
			targetType &= unchecked((int)0xFFFFFF00);
			currentOffset += 3;
			break;
		  case TypeReference.INSTANCEOF:
		  case TypeReference.NEW:
		  case TypeReference.CONSTRUCTOR_REFERENCE:
		  case TypeReference.METHOD_REFERENCE:
			targetType &= unchecked((int)0xFF000000);
			currentOffset += 3;
			break;
		  default:
			throw new System.ArgumentException();
		}
		context.currentTypeAnnotationTarget = targetType;
		// Parse and store the target_path structure.
		int pathLength = readByte(currentOffset);
		context.currentTypeAnnotationTargetPath = pathLength == 0 ? null : new TypePath(classFileBuffer, currentOffset);
		// Return the start offset of the rest of the type_annotation structure.
		return currentOffset + 1 + 2 * pathLength;
	  }

	  /// <summary>
	  /// Reads a Runtime[In]VisibleParameterAnnotations attribute and makes the given visitor visit it.
	  /// </summary>
	  /// <param name="methodVisitor"> the visitor that must visit the parameter annotations. </param>
	  /// <param name="context"> information about the class being parsed. </param>
	  /// <param name="runtimeParameterAnnotationsOffset"> the start offset of a
	  ///     Runtime[In]VisibleParameterAnnotations attribute, excluding the attribute_info's
	  ///     attribute_name_index and attribute_length fields. </param>
	  /// <param name="visible"> true if the attribute to parse is a RuntimeVisibleParameterAnnotations
	  ///     attribute, false it is a RuntimeInvisibleParameterAnnotations attribute. </param>
	  private void readParameterAnnotations(MethodVisitor methodVisitor, Context context, int runtimeParameterAnnotationsOffset, bool visible)
	  {
		int currentOffset = runtimeParameterAnnotationsOffset;
		int numParameters = classFileBuffer[currentOffset++] & 0xFF;
		methodVisitor.visitAnnotableParameterCount(numParameters, visible);
		char[] charBuffer = context.charBuffer;
		for (int i = 0; i < numParameters; ++i)
		{
		  int numAnnotations = readUnsignedShort(currentOffset);
		  currentOffset += 2;
		  while (numAnnotations-- > 0)
		  {
			// Parse the type_index field.
			string annotationDescriptor = readUTF8(currentOffset, charBuffer);
			currentOffset += 2;
			// Parse num_element_value_pairs and element_value_pairs and visit these values.
			currentOffset = readElementValues(methodVisitor.visitParameterAnnotation(i, annotationDescriptor, visible), currentOffset, true, charBuffer);
		  }
		}
	  }

	  /// <summary>
	  /// Reads the element values of a JVMS 'annotation' structure and makes the given visitor visit
	  /// them. This method can also be used to read the values of the JVMS 'array_value' field of an
	  /// annotation's 'element_value'.
	  /// </summary>
	  /// <param name="annotationVisitor"> the visitor that must visit the values. </param>
	  /// <param name="annotationOffset"> the start offset of an 'annotation' structure (excluding its type_index
	  ///     field) or of an 'array_value' structure. </param>
	  /// <param name="named"> if the annotation values are named or not. This should be true to parse the values
	  ///     of a JVMS 'annotation' structure, and false to parse the JVMS 'array_value' of an
	  ///     annotation's element_value. </param>
	  /// <param name="charBuffer"> the buffer used to read strings in the constant pool. </param>
	  /// <returns> the end offset of the JVMS 'annotation' or 'array_value' structure. </returns>
	  private int readElementValues(AnnotationVisitor annotationVisitor, int annotationOffset, bool named, char[] charBuffer)
	  {
		int currentOffset = annotationOffset;
		// Read the num_element_value_pairs field (or num_values field for an array_value).
		int numElementValuePairs = readUnsignedShort(currentOffset);
		currentOffset += 2;
		if (named)
		{
		  // Parse the element_value_pairs array.
		  while (numElementValuePairs-- > 0)
		  {
			string elementName = readUTF8(currentOffset, charBuffer);
			currentOffset = readElementValue(annotationVisitor, currentOffset + 2, elementName, charBuffer);
		  }
		}
		else
		{
		  // Parse the array_value array.
		  while (numElementValuePairs-- > 0)
		  {
			currentOffset = readElementValue(annotationVisitor, currentOffset, null, charBuffer);
		  }
		}
		if (annotationVisitor != null)
		{
		  annotationVisitor.visitEnd();
		}
		return currentOffset;
	  }

	  /// <summary>
	  /// Reads a JVMS 'element_value' structure and makes the given visitor visit it.
	  /// </summary>
	  /// <param name="annotationVisitor"> the visitor that must visit the element_value structure. </param>
	  /// <param name="elementValueOffset"> the start offset in <seealso cref="classFileBuffer"/> of the element_value
	  ///     structure to be read. </param>
	  /// <param name="elementName"> the name of the element_value structure to be read, or {@literal null}. </param>
	  /// <param name="charBuffer"> the buffer used to read strings in the constant pool. </param>
	  /// <returns> the end offset of the JVMS 'element_value' structure. </returns>
	  private int readElementValue(AnnotationVisitor annotationVisitor, int elementValueOffset, string elementName, char[] charBuffer)
	  {
		int currentOffset = elementValueOffset;
		if (annotationVisitor == null)
		{
		  switch (classFileBuffer[currentOffset] & 0xFF)
		  {
			case 'e': // enum_const_value
			  return currentOffset + 5;
			case '@': // annotation_value
			  return readElementValues(null, currentOffset + 3, true, charBuffer);
			case '[': // array_value
			  return readElementValues(null, currentOffset + 1, false, charBuffer);
			default:
			  return currentOffset + 3;
		  }
		}
		switch (classFileBuffer[currentOffset++] & 0xFF)
		{
		  case 'B': // const_value_index, CONSTANT_Integer
			annotationVisitor.visit(elementName, (sbyte) readInt(cpInfoOffsets[readUnsignedShort(currentOffset)]));
			currentOffset += 2;
			break;
		  case 'C': // const_value_index, CONSTANT_Integer
			annotationVisitor.visit(elementName, (char) readInt(cpInfoOffsets[readUnsignedShort(currentOffset)]));
			currentOffset += 2;
			break;
		  case 'D': // const_value_index, CONSTANT_Double
		  case 'F': // const_value_index, CONSTANT_Float
		  case 'I': // const_value_index, CONSTANT_Integer
		  case 'J': // const_value_index, CONSTANT_Long
			annotationVisitor.visit(elementName, readConst(readUnsignedShort(currentOffset), charBuffer));
			currentOffset += 2;
			break;
		  case 'S': // const_value_index, CONSTANT_Integer
			annotationVisitor.visit(elementName, (short) readInt(cpInfoOffsets[readUnsignedShort(currentOffset)]));
			currentOffset += 2;
			break;

		  case 'Z': // const_value_index, CONSTANT_Integer
			annotationVisitor.visit(elementName, readInt(cpInfoOffsets[readUnsignedShort(currentOffset)]) == 0 ? false : true);
			currentOffset += 2;
			break;
		  case 's': // const_value_index, CONSTANT_Utf8
			annotationVisitor.visit(elementName, readUTF8(currentOffset, charBuffer));
			currentOffset += 2;
			break;
		  case 'e': // enum_const_value
			annotationVisitor.visitEnum(elementName, readUTF8(currentOffset, charBuffer), readUTF8(currentOffset + 2, charBuffer));
			currentOffset += 4;
			break;
		  case 'c': // class_info
			annotationVisitor.visit(elementName, org.objectweb.asm.JType.getType(readUTF8(currentOffset, charBuffer)));
			currentOffset += 2;
			break;
		  case '@': // annotation_value
			currentOffset = readElementValues(annotationVisitor.visitAnnotation(elementName, readUTF8(currentOffset, charBuffer)), currentOffset + 2, true, charBuffer);
			break;
		  case '[': // array_value
			int numValues = readUnsignedShort(currentOffset);
			currentOffset += 2;
			if (numValues == 0)
			{
			  return readElementValues(annotationVisitor.visitArray(elementName), currentOffset - 2, false, charBuffer);
			}
			switch (classFileBuffer[currentOffset] & 0xFF)
			{
			  case 'B':
				sbyte[] byteValues = new sbyte[numValues];
				for (int i = 0; i < numValues; i++)
				{
				  byteValues[i] = (sbyte) readInt(cpInfoOffsets[readUnsignedShort(currentOffset + 1)]);
				  currentOffset += 3;
				}
				annotationVisitor.visit(elementName, byteValues);
				break;
			  case 'Z':
				bool[] booleanValues = new bool[numValues];
				for (int i = 0; i < numValues; i++)
				{
				  booleanValues[i] = readInt(cpInfoOffsets[readUnsignedShort(currentOffset + 1)]) != 0;
				  currentOffset += 3;
				}
				annotationVisitor.visit(elementName, booleanValues);
				break;
			  case 'S':
				short[] shortValues = new short[numValues];
				for (int i = 0; i < numValues; i++)
				{
				  shortValues[i] = (short) readInt(cpInfoOffsets[readUnsignedShort(currentOffset + 1)]);
				  currentOffset += 3;
				}
				annotationVisitor.visit(elementName, shortValues);
				break;
			  case 'C':
				char[] charValues = new char[numValues];
				for (int i = 0; i < numValues; i++)
				{
				  charValues[i] = (char) readInt(cpInfoOffsets[readUnsignedShort(currentOffset + 1)]);
				  currentOffset += 3;
				}
				annotationVisitor.visit(elementName, charValues);
				break;
			  case 'I':
				int[] intValues = new int[numValues];
				for (int i = 0; i < numValues; i++)
				{
				  intValues[i] = readInt(cpInfoOffsets[readUnsignedShort(currentOffset + 1)]);
				  currentOffset += 3;
				}
				annotationVisitor.visit(elementName, intValues);
				break;
			  case 'J':
				long[] longValues = new long[numValues];
				for (int i = 0; i < numValues; i++)
				{
				  longValues[i] = readLong(cpInfoOffsets[readUnsignedShort(currentOffset + 1)]);
				  currentOffset += 3;
				}
				annotationVisitor.visit(elementName, longValues);
				break;
			  case 'F':
				float[] floatValues = new float[numValues];
				for (int i = 0; i < numValues; i++)
				{
				  floatValues[i] = BitConverter.Int32BitsToSingle(readInt(cpInfoOffsets[readUnsignedShort(currentOffset + 1)]));
				  currentOffset += 3;
				}
				annotationVisitor.visit(elementName, floatValues);
				break;
			  case 'D':
				double[] doubleValues = new double[numValues];
				for (int i = 0; i < numValues; i++)
				{
				  doubleValues[i] = BitConverter.Int64BitsToDouble(readLong(cpInfoOffsets[readUnsignedShort(currentOffset + 1)]));
				  currentOffset += 3;
				}
				annotationVisitor.visit(elementName, doubleValues);
				break;
			  default:
				currentOffset = readElementValues(annotationVisitor.visitArray(elementName), currentOffset - 2, false, charBuffer);
				break;
			}
			break;
		  default:
			throw new System.ArgumentException();
		}
		return currentOffset;
	  }

	  // ----------------------------------------------------------------------------------------------
	  // Methods to parse stack map frames
	  // ----------------------------------------------------------------------------------------------

	  /// <summary>
	  /// Computes the implicit frame of the method currently being parsed (as defined in the given
	  /// <seealso cref="Context"/>) and stores it in the given context.
	  /// </summary>
	  /// <param name="context"> information about the class being parsed. </param>
	  private void computeImplicitFrame(Context context)
	  {
		string methodDescriptor = context.currentMethodDescriptor;
		object[] locals = context.currentFrameLocalTypes;
		int numLocal = 0;
		if ((context.currentMethodAccessFlags & Opcodes.ACC_STATIC) == 0)
		{
		  if ("<init>".Equals(context.currentMethodName))
		  {
			locals[numLocal++] = Opcodes.UNINITIALIZED_THIS;
		  }
		  else
		  {
			locals[numLocal++] = readClass(header + 2, context.charBuffer);
		  }
		}
		// Parse the method descriptor, one argument type descriptor at each iteration. Start by
		// skipping the first method descriptor character, which is always '('.
		int currentMethodDescritorOffset = 1;
		while (true)
		{
		  int currentArgumentDescriptorStartOffset = currentMethodDescritorOffset;
		  switch (methodDescriptor[currentMethodDescritorOffset++])
		  {
			case 'Z':
			case 'C':
			case 'B':
			case 'S':
			case 'I':
			  locals[numLocal++] = Opcodes.INTEGER;
			  break;
			case 'F':
			  locals[numLocal++] = Opcodes.FLOAT;
			  break;
			case 'J':
			  locals[numLocal++] = Opcodes.LONG;
			  break;
			case 'D':
			  locals[numLocal++] = Opcodes.DOUBLE;
			  break;
			case '[':
			  while (methodDescriptor[currentMethodDescritorOffset] == '[')
			  {
				++currentMethodDescritorOffset;
			  }
			  if (methodDescriptor[currentMethodDescritorOffset] == 'L')
			  {
				++currentMethodDescritorOffset;
				while (methodDescriptor[currentMethodDescritorOffset] != ';')
				{
				  ++currentMethodDescritorOffset;
				}
			  }
			  locals[numLocal++] = methodDescriptor.Substring(currentArgumentDescriptorStartOffset, (++currentMethodDescritorOffset) - currentArgumentDescriptorStartOffset);
			  break;
			case 'L':
			  while (methodDescriptor[currentMethodDescritorOffset] != ';')
			  {
				++currentMethodDescritorOffset;
			  }
			  locals[numLocal++] = methodDescriptor.Substring(currentArgumentDescriptorStartOffset + 1, (currentMethodDescritorOffset++) - (currentArgumentDescriptorStartOffset + 1));
			  break;
			default:
			  context.currentFrameLocalCount = numLocal;
			  return;
		  }
		}
	  }

	  /// <summary>
	  /// Reads a JVMS 'stack_map_frame' structure and stores the result in the given <seealso cref="Context"/>
	  /// object. This method can also be used to read a full_frame structure, excluding its frame_type
	  /// field (this is used to parse the legacy StackMap attributes).
	  /// </summary>
	  /// <param name="stackMapFrameOffset"> the start offset in <seealso cref="classFileBuffer"/> of the
	  ///     stack_map_frame_value structure to be read, or the start offset of a full_frame structure
	  ///     (excluding its frame_type field). </param>
	  /// <param name="compressed"> true to read a 'stack_map_frame' structure, false to read a 'full_frame'
	  ///     structure without its frame_type field. </param>
	  /// <param name="expand"> if the stack map frame must be expanded. See <seealso cref="EXPAND_FRAMES"/>. </param>
	  /// <param name="context"> where the parsed stack map frame must be stored. </param>
	  /// <returns> the end offset of the JVMS 'stack_map_frame' or 'full_frame' structure. </returns>
	  private int readStackMapFrame(int stackMapFrameOffset, bool compressed, bool expand, Context context)
	  {
		int currentOffset = stackMapFrameOffset;
		char[] charBuffer = context.charBuffer;
		Label[] labels = context.currentMethodLabels;
		int frameType;
		if (compressed)
		{
		  // Read the frame_type field.
		  frameType = classFileBuffer[currentOffset++] & 0xFF;
		}
		else
		{
		  frameType = Frame.FULL_FRAME;
		  context.currentFrameOffset = -1;
		}
		int offsetDelta;
		context.currentFrameLocalCountDelta = 0;
		if (frameType < Frame.SAME_LOCALS_1_STACK_ITEM_FRAME)
		{
		  offsetDelta = frameType;
		  context.currentFrameType = Opcodes.F_SAME;
		  context.currentFrameStackCount = 0;
		}
		else if (frameType < Frame.RESERVED)
		{
		  offsetDelta = frameType - Frame.SAME_LOCALS_1_STACK_ITEM_FRAME;
		  currentOffset = readVerificationTypeInfo(currentOffset, context.currentFrameStackTypes, 0, charBuffer, labels);
		  context.currentFrameType = Opcodes.F_SAME1;
		  context.currentFrameStackCount = 1;
		}
		else if (frameType >= Frame.SAME_LOCALS_1_STACK_ITEM_FRAME_EXTENDED)
		{
		  offsetDelta = readUnsignedShort(currentOffset);
		  currentOffset += 2;
		  if (frameType == Frame.SAME_LOCALS_1_STACK_ITEM_FRAME_EXTENDED)
		  {
			currentOffset = readVerificationTypeInfo(currentOffset, context.currentFrameStackTypes, 0, charBuffer, labels);
			context.currentFrameType = Opcodes.F_SAME1;
			context.currentFrameStackCount = 1;
		  }
		  else if (frameType >= Frame.CHOP_FRAME && frameType < Frame.SAME_FRAME_EXTENDED)
		  {
			context.currentFrameType = Opcodes.F_CHOP;
			context.currentFrameLocalCountDelta = Frame.SAME_FRAME_EXTENDED - frameType;
			context.currentFrameLocalCount -= context.currentFrameLocalCountDelta;
			context.currentFrameStackCount = 0;
		  }
		  else if (frameType == Frame.SAME_FRAME_EXTENDED)
		  {
			context.currentFrameType = Opcodes.F_SAME;
			context.currentFrameStackCount = 0;
		  }
		  else if (frameType < Frame.FULL_FRAME)
		  {
			int local = expand ? context.currentFrameLocalCount : 0;
			for (int k = frameType - Frame.SAME_FRAME_EXTENDED; k > 0; k--)
			{
			  currentOffset = readVerificationTypeInfo(currentOffset, context.currentFrameLocalTypes, local++, charBuffer, labels);
			}
			context.currentFrameType = Opcodes.F_APPEND;
			context.currentFrameLocalCountDelta = frameType - Frame.SAME_FRAME_EXTENDED;
			context.currentFrameLocalCount += context.currentFrameLocalCountDelta;
			context.currentFrameStackCount = 0;
		  }
		  else
		  {
			int numberOfLocals = readUnsignedShort(currentOffset);
			currentOffset += 2;
			context.currentFrameType = Opcodes.F_FULL;
			context.currentFrameLocalCountDelta = numberOfLocals;
			context.currentFrameLocalCount = numberOfLocals;
			for (int local = 0; local < numberOfLocals; ++local)
			{
			  currentOffset = readVerificationTypeInfo(currentOffset, context.currentFrameLocalTypes, local, charBuffer, labels);
			}
			int numberOfStackItems = readUnsignedShort(currentOffset);
			currentOffset += 2;
			context.currentFrameStackCount = numberOfStackItems;
			for (int stack = 0; stack < numberOfStackItems; ++stack)
			{
			  currentOffset = readVerificationTypeInfo(currentOffset, context.currentFrameStackTypes, stack, charBuffer, labels);
			}
		  }
		}
		else
		{
		  throw new System.ArgumentException();
		}
		context.currentFrameOffset += offsetDelta + 1;
		createLabel(context.currentFrameOffset, labels);
		return currentOffset;
	  }

	  /// <summary>
	  /// Reads a JVMS 'verification_type_info' structure and stores it at the given index in the given
	  /// array.
	  /// </summary>
	  /// <param name="verificationTypeInfoOffset"> the start offset of the 'verification_type_info' structure to
	  ///     read. </param>
	  /// <param name="frame"> the array where the parsed type must be stored. </param>
	  /// <param name="index"> the index in 'frame' where the parsed type must be stored. </param>
	  /// <param name="charBuffer"> the buffer used to read strings in the constant pool. </param>
	  /// <param name="labels"> the labels of the method currently being parsed, indexed by their offset. If the
	  ///     parsed type is an ITEM_Uninitialized, a new label for the corresponding NEW instruction is
	  ///     stored in this array if it does not already exist. </param>
	  /// <returns> the end offset of the JVMS 'verification_type_info' structure. </returns>
	  private int readVerificationTypeInfo(int verificationTypeInfoOffset, object[] frame, int index, char[] charBuffer, Label[] labels)
	  {
		int currentOffset = verificationTypeInfoOffset;
		int tag = classFileBuffer[currentOffset++] & 0xFF;
		switch (tag)
		{
		  case Frame.ITEM_TOP:
			frame[index] = Opcodes.TOP;
			break;
		  case Frame.ITEM_INTEGER:
			frame[index] = Opcodes.INTEGER;
			break;
		  case Frame.ITEM_FLOAT:
			frame[index] = Opcodes.FLOAT;
			break;
		  case Frame.ITEM_DOUBLE:
			frame[index] = Opcodes.DOUBLE;
			break;
		  case Frame.ITEM_LONG:
			frame[index] = Opcodes.LONG;
			break;
		  case Frame.ITEM_NULL:
			frame[index] = Opcodes.NULL;
			break;
		  case Frame.ITEM_UNINITIALIZED_THIS:
			frame[index] = Opcodes.UNINITIALIZED_THIS;
			break;
		  case Frame.ITEM_OBJECT:
			frame[index] = readClass(currentOffset, charBuffer);
			currentOffset += 2;
			break;
		  case Frame.ITEM_UNINITIALIZED:
			frame[index] = createLabel(readUnsignedShort(currentOffset), labels);
			currentOffset += 2;
			break;
		  default:
			throw new System.ArgumentException();
		}
		return currentOffset;
	  }

	  // ----------------------------------------------------------------------------------------------
	  // Methods to parse attributes
	  // ----------------------------------------------------------------------------------------------

	  /// <summary>
	  /// Returns the offset in <seealso cref="classFileBuffer"/> of the first ClassFile's 'attributes' array
	  /// field entry.
	  /// </summary>
	  /// <returns> the offset in <seealso cref="classFileBuffer"/> of the first ClassFile's 'attributes' array
	  ///     field entry. </returns>
	  public int FirstAttributeOffset
	  {
		  get
		  {
			// Skip the access_flags, this_class, super_class, and interfaces_count fields (using 2 bytes
			// each), as well as the interfaces array field (2 bytes per interface).
			int currentOffset = header + 8 + readUnsignedShort(header + 6) * 2;
    
			// Read the fields_count field.
			int fieldsCount = readUnsignedShort(currentOffset);
			currentOffset += 2;
			// Skip the 'fields' array field.
			while (fieldsCount-- > 0)
			{
			  // Invariant: currentOffset is the offset of a field_info structure.
			  // Skip the access_flags, name_index and descriptor_index fields (2 bytes each), and read the
			  // attributes_count field.
			  int attributesCount = readUnsignedShort(currentOffset + 6);
			  currentOffset += 8;
			  // Skip the 'attributes' array field.
			  while (attributesCount-- > 0)
			  {
				// Invariant: currentOffset is the offset of an attribute_info structure.
				// Read the attribute_length field (2 bytes after the start of the attribute_info) and skip
				// this many bytes, plus 6 for the attribute_name_index and attribute_length fields
				// (yielding the total size of the attribute_info structure).
				currentOffset += 6 + readInt(currentOffset + 2);
			  }
			}
    
			// Skip the methods_count and 'methods' fields, using the same method as above.
			int methodsCount = readUnsignedShort(currentOffset);
			currentOffset += 2;
			while (methodsCount-- > 0)
			{
			  int attributesCount = readUnsignedShort(currentOffset + 6);
			  currentOffset += 8;
			  while (attributesCount-- > 0)
			  {
				currentOffset += 6 + readInt(currentOffset + 2);
			  }
			}
    
			// Skip the ClassFile's attributes_count field.
			return currentOffset + 2;
		  }
	  }

	  /// <summary>
	  /// Reads the BootstrapMethods attribute to compute the offset of each bootstrap method.
	  /// </summary>
	  /// <param name="maxStringLength"> a conservative estimate of the maximum length of the strings contained
	  ///     in the constant pool of the class. </param>
	  /// <returns> the offsets of the bootstrap methods. </returns>
	  private int[] readBootstrapMethodsAttribute(int maxStringLength)
	  {
		char[] charBuffer = new char[maxStringLength];
		int currentAttributeOffset = FirstAttributeOffset;
		for (int i = readUnsignedShort(currentAttributeOffset - 2); i > 0; --i)
		{
		  // Read the attribute_info's attribute_name and attribute_length fields.
		  string attributeName = readUTF8(currentAttributeOffset, charBuffer);
		  int attributeLength = readInt(currentAttributeOffset + 2);
		  currentAttributeOffset += 6;
		  if (Constants.BOOTSTRAP_METHODS.Equals(attributeName))
		  {
			// Read the num_bootstrap_methods field and create an array of this size.
			int[] result = new int[readUnsignedShort(currentAttributeOffset)];
			// Compute and store the offset of each 'bootstrap_methods' array field entry.
			int currentBootstrapMethodOffset = currentAttributeOffset + 2;
			for (int j = 0; j < result.Length; ++j)
			{
			  result[j] = currentBootstrapMethodOffset;
			  // Skip the bootstrap_method_ref and num_bootstrap_arguments fields (2 bytes each),
			  // as well as the bootstrap_arguments array field (of size num_bootstrap_arguments * 2).
			  currentBootstrapMethodOffset += 4 + readUnsignedShort(currentBootstrapMethodOffset + 2) * 2;
			}
			return result;
		  }
		  currentAttributeOffset += attributeLength;
		}
		throw new System.ArgumentException();
	  }

	  /// <summary>
	  /// Reads a non standard JVMS 'attribute' structure in <seealso cref="classFileBuffer"/>.
	  /// </summary>
	  /// <param name="attributePrototypes"> prototypes of the attributes that must be parsed during the visit of
	  ///     the class. Any attribute whose type is not equal to the type of one the prototypes will not
	  ///     be parsed: its byte array value will be passed unchanged to the ClassWriter. </param>
	  /// <param name="type"> the type of the attribute. </param>
	  /// <param name="offset"> the start offset of the JVMS 'attribute' structure in <seealso cref="classFileBuffer"/>.
	  ///     The 6 attribute header bytes (attribute_name_index and attribute_length) are not taken into
	  ///     account here. </param>
	  /// <param name="length"> the length of the attribute's content (excluding the 6 attribute header bytes). </param>
	  /// <param name="charBuffer"> the buffer to be used to read strings in the constant pool. </param>
	  /// <param name="codeAttributeOffset"> the start offset of the enclosing Code attribute in {@link
	  ///     #classFileBuffer}, or -1 if the attribute to be read is not a code attribute. The 6
	  ///     attribute header bytes (attribute_name_index and attribute_length) are not taken into
	  ///     account here. </param>
	  /// <param name="labels"> the labels of the method's code, or {@literal null} if the attribute to be read
	  ///     is not a code attribute. </param>
	  /// <returns> the attribute that has been read. </returns>
	  private Attribute readAttribute(Attribute[] attributePrototypes, string type, int offset, int length, char[] charBuffer, int codeAttributeOffset, Label[] labels)
	  {
		foreach (Attribute attributePrototype in attributePrototypes)
		{
		  if (attributePrototype.type.Equals(type))
		  {
			return attributePrototype.read(this, offset, length, charBuffer, codeAttributeOffset, labels);
		  }
		}
		return (new Attribute(type)).read(this, offset, length, null, -1, null);
	  }

	  // -----------------------------------------------------------------------------------------------
	  // Utility methods: low level parsing
	  // -----------------------------------------------------------------------------------------------

	  /// <summary>
	  /// Returns the number of entries in the class's constant pool table.
	  /// </summary>
	  /// <returns> the number of entries in the class's constant pool table. </returns>
	  public virtual int ItemCount
	  {
		  get
		  {
			return cpInfoOffsets.Length;
		  }
	  }

	  /// <summary>
	  /// Returns the start offset in this <seealso cref="ClassReader"/> of a JVMS 'cp_info' structure (i.e. a
	  /// constant pool entry), plus one. <i>This method is intended for <seealso cref="Attribute"/> sub classes,
	  /// and is normally not needed by class generators or adapters.</i>
	  /// </summary>
	  /// <param name="constantPoolEntryIndex"> the index a constant pool entry in the class's constant pool
	  ///     table. </param>
	  /// <returns> the start offset in this <seealso cref="ClassReader"/> of the corresponding JVMS 'cp_info'
	  ///     structure, plus one. </returns>
	  public virtual int getItem(int constantPoolEntryIndex)
	  {
		return cpInfoOffsets[constantPoolEntryIndex];
	  }

	  /// <summary>
	  /// Returns a conservative estimate of the maximum length of the strings contained in the class's
	  /// constant pool table.
	  /// </summary>
	  /// <returns> a conservative estimate of the maximum length of the strings contained in the class's
	  ///     constant pool table. </returns>
	  public virtual int MaxStringLength
	  {
		  get
		  {
			return maxStringLength;
		  }
	  }

	  /// <summary>
	  /// Reads a byte value in this <seealso cref="ClassReader"/>. <i>This method is intended for {@link
	  /// Attribute} sub classes, and is normally not needed by class generators or adapters.</i>
	  /// </summary>
	  /// <param name="offset"> the start offset of the value to be read in this <seealso cref="ClassReader"/>. </param>
	  /// <returns> the read value. </returns>
	  public virtual int readByte(int offset)
	  {
		return classFileBuffer[offset] & 0xFF;
	  }

	  /// <summary>
	  /// Reads an unsigned short value in this <seealso cref="ClassReader"/>. <i>This method is intended for
	  /// <seealso cref="Attribute"/> sub classes, and is normally not needed by class generators or adapters.</i>
	  /// </summary>
	  /// <param name="offset"> the start index of the value to be read in this <seealso cref="ClassReader"/>. </param>
	  /// <returns> the read value. </returns>
	  public virtual int readUnsignedShort(int offset)
	  {
		sbyte[] classBuffer = classFileBuffer;
		return ((classBuffer[offset] & 0xFF) << 8) | (classBuffer[offset + 1] & 0xFF);
	  }

	  /// <summary>
	  /// Reads a signed short value in this <seealso cref="ClassReader"/>. <i>This method is intended for {@link
	  /// Attribute} sub classes, and is normally not needed by class generators or adapters.</i>
	  /// </summary>
	  /// <param name="offset"> the start offset of the value to be read in this <seealso cref="ClassReader"/>. </param>
	  /// <returns> the read value. </returns>
	  public virtual short readShort(int offset)
	  {
		sbyte[] classBuffer = classFileBuffer;
		return (short)(((classBuffer[offset] & 0xFF) << 8) | (classBuffer[offset + 1] & 0xFF));
	  }

	  /// <summary>
	  /// Reads a signed int value in this <seealso cref="ClassReader"/>. <i>This method is intended for {@link
	  /// Attribute} sub classes, and is normally not needed by class generators or adapters.</i>
	  /// </summary>
	  /// <param name="offset"> the start offset of the value to be read in this <seealso cref="ClassReader"/>. </param>
	  /// <returns> the read value. </returns>
	  public virtual int readInt(int offset)
	  {
		sbyte[] classBuffer = classFileBuffer;
		return ((classBuffer[offset] & 0xFF) << 24) | ((classBuffer[offset + 1] & 0xFF) << 16) | ((classBuffer[offset + 2] & 0xFF) << 8) | (classBuffer[offset + 3] & 0xFF);
	  }

	  /// <summary>
	  /// Reads a signed long value in this <seealso cref="ClassReader"/>. <i>This method is intended for {@link
	  /// Attribute} sub classes, and is normally not needed by class generators or adapters.</i>
	  /// </summary>
	  /// <param name="offset"> the start offset of the value to be read in this <seealso cref="ClassReader"/>. </param>
	  /// <returns> the read value. </returns>
	  public virtual long readLong(int offset)
	  {
		long l1 = readInt(offset);
		long l0 = readInt(offset + 4) & 0xFFFFFFFFL;
		return (l1 << 32) | l0;
	  }

	  /// <summary>
	  /// Reads a CONSTANT_Utf8 constant pool entry in this <seealso cref="ClassReader"/>. <i>This method is
	  /// intended for <seealso cref="Attribute"/> sub classes, and is normally not needed by class generators or
	  /// adapters.</i>
	  /// </summary>
	  /// <param name="offset"> the start offset of an unsigned short value in this <seealso cref="ClassReader"/>, whose
	  ///     value is the index of a CONSTANT_Utf8 entry in the class's constant pool table. </param>
	  /// <param name="charBuffer"> the buffer to be used to read the string. This buffer must be sufficiently
	  ///     large. It is not automatically resized. </param>
	  /// <returns> the String corresponding to the specified CONSTANT_Utf8 entry. </returns>
	  // DontCheck(AbbreviationAsWordInName): can't be renamed (for backward binary compatibility).
	  public virtual string readUTF8(int offset, char[] charBuffer)
	  {
		int constantPoolEntryIndex = readUnsignedShort(offset);
		if (offset == 0 || constantPoolEntryIndex == 0)
		{
		  return null;
		}
		return readUtf(constantPoolEntryIndex, charBuffer);
	  }

	  /// <summary>
	  /// Reads a CONSTANT_Utf8 constant pool entry in <seealso cref="classFileBuffer"/>.
	  /// </summary>
	  /// <param name="constantPoolEntryIndex"> the index of a CONSTANT_Utf8 entry in the class's constant pool
	  ///     table. </param>
	  /// <param name="charBuffer"> the buffer to be used to read the string. This buffer must be sufficiently
	  ///     large. It is not automatically resized. </param>
	  /// <returns> the String corresponding to the specified CONSTANT_Utf8 entry. </returns>
	  public string readUtf(int constantPoolEntryIndex, char[] charBuffer)
	  {
		string value = constantUtf8Values[constantPoolEntryIndex];
		if (!string.ReferenceEquals(value, null))
		{
		  return value;
		}
		int cpInfoOffset = cpInfoOffsets[constantPoolEntryIndex];
		return constantUtf8Values[constantPoolEntryIndex] = readUtf(cpInfoOffset + 2, readUnsignedShort(cpInfoOffset), charBuffer);
	  }

	  /// <summary>
	  /// Reads an UTF8 string in <seealso cref="classFileBuffer"/>.
	  /// </summary>
	  /// <param name="utfOffset"> the start offset of the UTF8 string to be read. </param>
	  /// <param name="utfLength"> the length of the UTF8 string to be read. </param>
	  /// <param name="charBuffer"> the buffer to be used to read the string. This buffer must be sufficiently
	  ///     large. It is not automatically resized. </param>
	  /// <returns> the String corresponding to the specified UTF8 string. </returns>
	  private string readUtf(int utfOffset, int utfLength, char[] charBuffer)
	  {
		int currentOffset = utfOffset;
		int endOffset = currentOffset + utfLength;
		int strLength = 0;
		sbyte[] classBuffer = classFileBuffer;
		while (currentOffset < endOffset)
		{
		  int currentByte = classBuffer[currentOffset++];
		  if ((currentByte & 0x80) == 0)
		  {
			charBuffer[strLength++] = (char)(currentByte & 0x7F);
		  }
		  else if ((currentByte & 0xE0) == 0xC0)
		  {
			charBuffer[strLength++] = (char)(((currentByte & 0x1F) << 6) + (classBuffer[currentOffset++] & 0x3F));
		  }
		  else
		  {
			charBuffer[strLength++] = (char)(((currentByte & 0xF) << 12) + ((classBuffer[currentOffset++] & 0x3F) << 6) + (classBuffer[currentOffset++] & 0x3F));
		  }
		}
		return new string(charBuffer, 0, strLength);
	  }

	  /// <summary>
	  /// Reads a CONSTANT_Class, CONSTANT_String, CONSTANT_MethodType, CONSTANT_Module or
	  /// CONSTANT_Package constant pool entry in <seealso cref="classFileBuffer"/>. <i>This method is intended
	  /// for <seealso cref="Attribute"/> sub classes, and is normally not needed by class generators or
	  /// adapters.</i>
	  /// </summary>
	  /// <param name="offset"> the start offset of an unsigned short value in <seealso cref="classFileBuffer"/>, whose
	  ///     value is the index of a CONSTANT_Class, CONSTANT_String, CONSTANT_MethodType,
	  ///     CONSTANT_Module or CONSTANT_Package entry in class's constant pool table. </param>
	  /// <param name="charBuffer"> the buffer to be used to read the item. This buffer must be sufficiently
	  ///     large. It is not automatically resized. </param>
	  /// <returns> the String corresponding to the specified constant pool entry. </returns>
	  private string readStringish(int offset, char[] charBuffer)
	  {
		// Get the start offset of the cp_info structure (plus one), and read the CONSTANT_Utf8 entry
		// designated by the first two bytes of this cp_info.
		return readUTF8(cpInfoOffsets[readUnsignedShort(offset)], charBuffer);
	  }

	  /// <summary>
	  /// Reads a CONSTANT_Class constant pool entry in this <seealso cref="ClassReader"/>. <i>This method is
	  /// intended for <seealso cref="Attribute"/> sub classes, and is normally not needed by class generators or
	  /// adapters.</i>
	  /// </summary>
	  /// <param name="offset"> the start offset of an unsigned short value in this <seealso cref="ClassReader"/>, whose
	  ///     value is the index of a CONSTANT_Class entry in class's constant pool table. </param>
	  /// <param name="charBuffer"> the buffer to be used to read the item. This buffer must be sufficiently
	  ///     large. It is not automatically resized. </param>
	  /// <returns> the String corresponding to the specified CONSTANT_Class entry. </returns>
	  public virtual string readClass(int offset, char[] charBuffer)
	  {
		return readStringish(offset, charBuffer);
	  }

	  /// <summary>
	  /// Reads a CONSTANT_Module constant pool entry in this <seealso cref="ClassReader"/>. <i>This method is
	  /// intended for <seealso cref="Attribute"/> sub classes, and is normally not needed by class generators or
	  /// adapters.</i>
	  /// </summary>
	  /// <param name="offset"> the start offset of an unsigned short value in this <seealso cref="ClassReader"/>, whose
	  ///     value is the index of a CONSTANT_Module entry in class's constant pool table. </param>
	  /// <param name="charBuffer"> the buffer to be used to read the item. This buffer must be sufficiently
	  ///     large. It is not automatically resized. </param>
	  /// <returns> the String corresponding to the specified CONSTANT_Module entry. </returns>
	  public virtual string readModule(int offset, char[] charBuffer)
	  {
		return readStringish(offset, charBuffer);
	  }

	  /// <summary>
	  /// Reads a CONSTANT_Package constant pool entry in this <seealso cref="ClassReader"/>. <i>This method is
	  /// intended for <seealso cref="Attribute"/> sub classes, and is normally not needed by class generators or
	  /// adapters.</i>
	  /// </summary>
	  /// <param name="offset"> the start offset of an unsigned short value in this <seealso cref="ClassReader"/>, whose
	  ///     value is the index of a CONSTANT_Package entry in class's constant pool table. </param>
	  /// <param name="charBuffer"> the buffer to be used to read the item. This buffer must be sufficiently
	  ///     large. It is not automatically resized. </param>
	  /// <returns> the String corresponding to the specified CONSTANT_Package entry. </returns>
	  public virtual string readPackage(int offset, char[] charBuffer)
	  {
		return readStringish(offset, charBuffer);
	  }

	  /// <summary>
	  /// Reads a CONSTANT_Dynamic constant pool entry in <seealso cref="classFileBuffer"/>.
	  /// </summary>
	  /// <param name="constantPoolEntryIndex"> the index of a CONSTANT_Dynamic entry in the class's constant
	  ///     pool table. </param>
	  /// <param name="charBuffer"> the buffer to be used to read the string. This buffer must be sufficiently
	  ///     large. It is not automatically resized. </param>
	  /// <returns> the ConstantDynamic corresponding to the specified CONSTANT_Dynamic entry. </returns>
	  private ConstantDynamic readConstantDynamic(int constantPoolEntryIndex, char[] charBuffer)
	  {
		ConstantDynamic constantDynamic = constantDynamicValues[constantPoolEntryIndex];
		if (constantDynamic != null)
		{
		  return constantDynamic;
		}
		int cpInfoOffset = cpInfoOffsets[constantPoolEntryIndex];
		int nameAndTypeCpInfoOffset = cpInfoOffsets[readUnsignedShort(cpInfoOffset + 2)];
		string name = readUTF8(nameAndTypeCpInfoOffset, charBuffer);
		string descriptor = readUTF8(nameAndTypeCpInfoOffset + 2, charBuffer);
		int bootstrapMethodOffset = bootstrapMethodOffsets[readUnsignedShort(cpInfoOffset)];
		Handle handle = (Handle) readConst(readUnsignedShort(bootstrapMethodOffset), charBuffer);
		object[] bootstrapMethodArguments = new object[readUnsignedShort(bootstrapMethodOffset + 2)];
		bootstrapMethodOffset += 4;
		for (int i = 0; i < bootstrapMethodArguments.Length; i++)
		{
		  bootstrapMethodArguments[i] = readConst(readUnsignedShort(bootstrapMethodOffset), charBuffer);
		  bootstrapMethodOffset += 2;
		}
		return constantDynamicValues[constantPoolEntryIndex] = new ConstantDynamic(name, descriptor, handle, bootstrapMethodArguments);
	  }

	  /// <summary>
	  /// Reads a numeric or string constant pool entry in this <seealso cref="ClassReader"/>. <i>This method is
	  /// intended for <seealso cref="Attribute"/> sub classes, and is normally not needed by class generators or
	  /// adapters.</i>
	  /// </summary>
	  /// <param name="constantPoolEntryIndex"> the index of a CONSTANT_Integer, CONSTANT_Float, CONSTANT_Long,
	  ///     CONSTANT_Double, CONSTANT_Class, CONSTANT_String, CONSTANT_MethodType,
	  ///     CONSTANT_MethodHandle or CONSTANT_Dynamic entry in the class's constant pool. </param>
	  /// <param name="charBuffer"> the buffer to be used to read strings. This buffer must be sufficiently
	  ///     large. It is not automatically resized. </param>
	  /// <returns> the <seealso cref="Integer"/>, <seealso cref="Float"/>, <seealso cref="Long"/>, <seealso cref="Double"/>, <seealso cref="string"/>,
	  ///     <seealso cref="Type"/>, <seealso cref="Handle"/> or <seealso cref="ConstantDynamic"/> corresponding to the specified
	  ///     constant pool entry. </returns>
	  public virtual object readConst(int constantPoolEntryIndex, char[] charBuffer)
	  {
		int cpInfoOffset = cpInfoOffsets[constantPoolEntryIndex];
		switch (classFileBuffer[cpInfoOffset - 1])
		{
		  case Symbol.CONSTANT_INTEGER_TAG:
			return readInt(cpInfoOffset);
		  case Symbol.CONSTANT_FLOAT_TAG:
			return BitConverter.Int32BitsToSingle(readInt(cpInfoOffset));
		  case Symbol.CONSTANT_LONG_TAG:
			return readLong(cpInfoOffset);
		  case Symbol.CONSTANT_DOUBLE_TAG:
			return BitConverter.Int64BitsToDouble(readLong(cpInfoOffset));
		  case Symbol.CONSTANT_CLASS_TAG:
			return org.objectweb.asm.JType.getObjectType(readUTF8(cpInfoOffset, charBuffer));
		  case Symbol.CONSTANT_STRING_TAG:
			return readUTF8(cpInfoOffset, charBuffer);
		  case Symbol.CONSTANT_METHOD_TYPE_TAG:
			return org.objectweb.asm.JType.getMethodType(readUTF8(cpInfoOffset, charBuffer));
		  case Symbol.CONSTANT_METHOD_HANDLE_TAG:
			int referenceKind = readByte(cpInfoOffset);
			int referenceCpInfoOffset = cpInfoOffsets[readUnsignedShort(cpInfoOffset + 1)];
			int nameAndTypeCpInfoOffset = cpInfoOffsets[readUnsignedShort(referenceCpInfoOffset + 2)];
			string owner = readClass(referenceCpInfoOffset, charBuffer);
			string name = readUTF8(nameAndTypeCpInfoOffset, charBuffer);
			string descriptor = readUTF8(nameAndTypeCpInfoOffset + 2, charBuffer);
			bool isInterface = classFileBuffer[referenceCpInfoOffset - 1] == Symbol.CONSTANT_INTERFACE_METHODREF_TAG;
			return new Handle(referenceKind, owner, name, descriptor, isInterface);
		  case Symbol.CONSTANT_DYNAMIC_TAG:
			return readConstantDynamic(constantPoolEntryIndex, charBuffer);
		  default:
			throw new System.ArgumentException();
		}
	  }
	}

}