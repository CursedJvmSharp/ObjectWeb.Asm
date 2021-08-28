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
    /// The constant pool entries, the BootstrapMethods attribute entries and the (ASM specific) type
    /// table entries of a class.
    /// 
    /// @author Eric Bruneton </summary>
    /// <seealso cref= <a href="https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.4">JVMS
    ///     4.4</a> </seealso>
    /// <seealso cref= <a href="https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.7.23">JVMS
    ///     4.7.23</a> </seealso>
    public sealed class SymbolTable
	{

	  /// <summary>
	  /// The ClassWriter to which this SymbolTable belongs. This is only used to get access to {@link
	  /// ClassWriter#getCommonSuperClass} and to serialize custom attributes with {@link
	  /// Attribute#write}.
	  /// </summary>
	  internal readonly ClassWriter classWriter;

	  /// <summary>
	  /// The ClassReader from which this SymbolTable was constructed, or {@literal null} if it was
	  /// constructed from scratch.
	  /// </summary>
	  private readonly ClassReader sourceClassReader;

	  /// <summary>
	  /// The major version number of the class to which this symbol table belongs. </summary>
	  private int majorVersion;

	  /// <summary>
	  /// The internal name of the class to which this symbol table belongs. </summary>
	  private string className;

	  /// <summary>
	  /// The total number of <seealso cref="Entry"/> instances in <seealso cref="entries"/>. This includes entries that are
	  /// accessible (recursively) via <seealso cref="Entry.next"/>.
	  /// </summary>
	  private int entryCount;

	  /// <summary>
	  /// A hash set of all the entries in this SymbolTable (this includes the constant pool entries, the
	  /// bootstrap method entries and the type table entries). Each <seealso cref="Entry"/> instance is stored at
	  /// the array index given by its hash code modulo the array size. If several entries must be stored
	  /// at the same array index, they are linked together via their <seealso cref="Entry.next"/> field. The
	  /// factory methods of this class make sure that this table does not contain duplicated entries.
	  /// </summary>
	  private Entry[] entries;

	  /// <summary>
	  /// The number of constant pool items in <seealso cref="constantPool"/>, plus 1. The first constant pool
	  /// item has index 1, and long and double items count for two items.
	  /// </summary>
	  private int constantPoolCount;

	  /// <summary>
	  /// The content of the ClassFile's constant_pool JVMS structure corresponding to this SymbolTable.
	  /// The ClassFile's constant_pool_count field is <i>not</i> included.
	  /// </summary>
	  private ByteVector constantPool;

	  /// <summary>
	  /// The number of bootstrap methods in <seealso cref="bootstrapMethods"/>. Corresponds to the
	  /// BootstrapMethods_attribute's num_bootstrap_methods field value.
	  /// </summary>
	  private int bootstrapMethodCount;

	  /// <summary>
	  /// The content of the BootstrapMethods attribute 'bootstrap_methods' array corresponding to this
	  /// SymbolTable. Note that the first 6 bytes of the BootstrapMethods_attribute, and its
	  /// num_bootstrap_methods field, are <i>not</i> included.
	  /// </summary>
	  private ByteVector bootstrapMethods;

	  /// <summary>
	  /// The actual number of elements in <seealso cref="typeTable"/>. These elements are stored from index 0 to
	  /// typeCount (excluded). The other array entries are empty.
	  /// </summary>
	  private int typeCount;

	  /// <summary>
	  /// An ASM specific type table used to temporarily store internal names that will not necessarily
	  /// be stored in the constant pool. This type table is used by the control flow and data flow
	  /// analysis algorithm used to compute stack map frames from scratch. This array stores {@link
	  /// Symbol#TYPE_TAG} and <seealso cref="Symbol.UNINITIALIZED_TYPE_TAG"/>) Symbol. The type symbol at index
	  /// {@code i} has its <seealso cref="Symbol.index"/> equal to {@code i} (and vice versa).
	  /// </summary>
	  private Entry[] typeTable;

	  /// <summary>
	  /// Constructs a new, empty SymbolTable for the given ClassWriter.
	  /// </summary>
	  /// <param name="classWriter"> a ClassWriter. </param>
	  public SymbolTable(ClassWriter classWriter)
	  {
		this.classWriter = classWriter;
		this.sourceClassReader = null;
		this.entries = new Entry[256];
		this.constantPoolCount = 1;
		this.constantPool = new ByteVector();
	  }

	  /// <summary>
	  /// Constructs a new SymbolTable for the given ClassWriter, initialized with the constant pool and
	  /// bootstrap methods of the given ClassReader.
	  /// </summary>
	  /// <param name="classWriter"> a ClassWriter. </param>
	  /// <param name="classReader"> the ClassReader whose constant pool and bootstrap methods must be copied to
	  ///     initialize the SymbolTable. </param>
	  public SymbolTable(ClassWriter classWriter, ClassReader classReader)
	  {
		this.classWriter = classWriter;
		this.sourceClassReader = classReader;

		// Copy the constant pool binary content.
		sbyte[] inputBytes = classReader.classFileBuffer;
		int constantPoolOffset = classReader.getItem(1) - 1;
		int constantPoolLength = classReader.header - constantPoolOffset;
		constantPoolCount = classReader.ItemCount;
		constantPool = new ByteVector(constantPoolLength);
		constantPool.putByteArray(inputBytes, constantPoolOffset, constantPoolLength);

		// Add the constant pool items in the symbol table entries. Reserve enough space in 'entries' to
		// avoid too many hash set collisions (entries is not dynamically resized by the addConstant*
		// method calls below), and to account for bootstrap method entries.
		entries = new Entry[constantPoolCount * 2];
		char[] charBuffer = new char[classReader.MaxStringLength];
		bool hasBootstrapMethods = false;
		int itemIndex = 1;
		while (itemIndex < constantPoolCount)
		{
		  int itemOffset = classReader.getItem(itemIndex);
		  int itemTag = inputBytes[itemOffset - 1];
		  int nameAndTypeItemOffset;
		  switch (itemTag)
		  {
			case Symbol.CONSTANT_FIELDREF_TAG:
			case Symbol.CONSTANT_METHODREF_TAG:
			case Symbol.CONSTANT_INTERFACE_METHODREF_TAG:
			  nameAndTypeItemOffset = classReader.getItem(classReader.readUnsignedShort(itemOffset + 2));
			  addConstantMemberReference(itemIndex, itemTag, classReader.readClass(itemOffset, charBuffer), classReader.readUTF8(nameAndTypeItemOffset, charBuffer), classReader.readUTF8(nameAndTypeItemOffset + 2, charBuffer));
			  break;
			case Symbol.CONSTANT_INTEGER_TAG:
			case Symbol.CONSTANT_FLOAT_TAG:
			  addConstantIntegerOrFloat(itemIndex, itemTag, classReader.readInt(itemOffset));
			  break;
			case Symbol.CONSTANT_NAME_AND_TYPE_TAG:
			  addConstantNameAndType(itemIndex, classReader.readUTF8(itemOffset, charBuffer), classReader.readUTF8(itemOffset + 2, charBuffer));
			  break;
			case Symbol.CONSTANT_LONG_TAG:
			case Symbol.CONSTANT_DOUBLE_TAG:
			  addConstantLongOrDouble(itemIndex, itemTag, classReader.readLong(itemOffset));
			  break;
			case Symbol.CONSTANT_UTF8_TAG:
			  addConstantUtf8(itemIndex, classReader.readUtf(itemIndex, charBuffer));
			  break;
			case Symbol.CONSTANT_METHOD_HANDLE_TAG:
			  int memberRefItemOffset = classReader.getItem(classReader.readUnsignedShort(itemOffset + 1));
			  nameAndTypeItemOffset = classReader.getItem(classReader.readUnsignedShort(memberRefItemOffset + 2));
			  addConstantMethodHandle(itemIndex, classReader.readByte(itemOffset), classReader.readClass(memberRefItemOffset, charBuffer), classReader.readUTF8(nameAndTypeItemOffset, charBuffer), classReader.readUTF8(nameAndTypeItemOffset + 2, charBuffer));
			  break;
			case Symbol.CONSTANT_DYNAMIC_TAG:
			case Symbol.CONSTANT_INVOKE_DYNAMIC_TAG:
			  hasBootstrapMethods = true;
			  nameAndTypeItemOffset = classReader.getItem(classReader.readUnsignedShort(itemOffset + 2));
			  addConstantDynamicOrInvokeDynamicReference(itemTag, itemIndex, classReader.readUTF8(nameAndTypeItemOffset, charBuffer), classReader.readUTF8(nameAndTypeItemOffset + 2, charBuffer), classReader.readUnsignedShort(itemOffset));
			  break;
			case Symbol.CONSTANT_STRING_TAG:
			case Symbol.CONSTANT_CLASS_TAG:
			case Symbol.CONSTANT_METHOD_TYPE_TAG:
			case Symbol.CONSTANT_MODULE_TAG:
			case Symbol.CONSTANT_PACKAGE_TAG:
			  addConstantUtf8Reference(itemIndex, itemTag, classReader.readUTF8(itemOffset, charBuffer));
			  break;
			default:
			  throw new System.ArgumentException();
		  }
		  itemIndex += (itemTag == Symbol.CONSTANT_LONG_TAG || itemTag == Symbol.CONSTANT_DOUBLE_TAG) ? 2 : 1;
		}

		// Copy the BootstrapMethods, if any.
		if (hasBootstrapMethods)
		{
		  copyBootstrapMethods(classReader, charBuffer);
		}
	  }

	  /// <summary>
	  /// Read the BootstrapMethods 'bootstrap_methods' array binary content and add them as entries of
	  /// the SymbolTable.
	  /// </summary>
	  /// <param name="classReader"> the ClassReader whose bootstrap methods must be copied to initialize the
	  ///     SymbolTable. </param>
	  /// <param name="charBuffer"> a buffer used to read strings in the constant pool. </param>
	  private void copyBootstrapMethods(ClassReader classReader, char[] charBuffer)
	  {
		// Find attributOffset of the 'bootstrap_methods' array.
		sbyte[] inputBytes = classReader.classFileBuffer;
		int currentAttributeOffset = classReader.FirstAttributeOffset;
		for (int i = classReader.readUnsignedShort(currentAttributeOffset - 2); i > 0; --i)
		{
		  string attributeName = classReader.readUTF8(currentAttributeOffset, charBuffer);
		  if (Constants.BOOTSTRAP_METHODS.Equals(attributeName))
		  {
			bootstrapMethodCount = classReader.readUnsignedShort(currentAttributeOffset + 6);
			break;
		  }
		  currentAttributeOffset += 6 + classReader.readInt(currentAttributeOffset + 2);
		}
		if (bootstrapMethodCount > 0)
		{
		  // Compute the offset and the length of the BootstrapMethods 'bootstrap_methods' array.
		  int bootstrapMethodsOffset = currentAttributeOffset + 8;
		  int bootstrapMethodsLength = classReader.readInt(currentAttributeOffset + 2) - 2;
		  bootstrapMethods = new ByteVector(bootstrapMethodsLength);
		  bootstrapMethods.putByteArray(inputBytes, bootstrapMethodsOffset, bootstrapMethodsLength);

		  // Add each bootstrap method in the symbol table entries.
		  int currentOffset = bootstrapMethodsOffset;
		  for (int i = 0; i < bootstrapMethodCount; i++)
		  {
			int offset = currentOffset - bootstrapMethodsOffset;
			int bootstrapMethodRef = classReader.readUnsignedShort(currentOffset);
			currentOffset += 2;
			int numBootstrapArguments = classReader.readUnsignedShort(currentOffset);
			currentOffset += 2;
			int hashCode = classReader.readConst(bootstrapMethodRef, charBuffer).GetHashCode();
			while (numBootstrapArguments-- > 0)
			{
			  int bootstrapArgument = classReader.readUnsignedShort(currentOffset);
			  currentOffset += 2;
			  hashCode ^= classReader.readConst(bootstrapArgument, charBuffer).GetHashCode();
			}
			add(new Entry(i, Symbol.BOOTSTRAP_METHOD_TAG, offset, hashCode & 0x7FFFFFFF));
		  }
		}
	  }

	  /// <summary>
	  /// Returns the ClassReader from which this SymbolTable was constructed.
	  /// </summary>
	  /// <returns> the ClassReader from which this SymbolTable was constructed, or {@literal null} if it
	  ///     was constructed from scratch. </returns>
	  public ClassReader Source
	  {
		  get
		  {
			return sourceClassReader;
		  }
	  }

	  /// <summary>
	  /// Returns the major version of the class to which this symbol table belongs.
	  /// </summary>
	  /// <returns> the major version of the class to which this symbol table belongs. </returns>
	  public int MajorVersion
	  {
		  get
		  {
			return majorVersion;
		  }
	  }

	  /// <summary>
	  /// Returns the internal name of the class to which this symbol table belongs.
	  /// </summary>
	  /// <returns> the internal name of the class to which this symbol table belongs. </returns>
	  public string ClassName
	  {
		  get
		  {
			return className;
		  }
	  }

	  /// <summary>
	  /// Sets the major version and the name of the class to which this symbol table belongs. Also adds
	  /// the class name to the constant pool.
	  /// </summary>
	  /// <param name="majorVersion"> a major ClassFile version number. </param>
	  /// <param name="className"> an internal class name. </param>
	  /// <returns> the constant pool index of a new or already existing Symbol with the given class name. </returns>
	  public int setMajorVersionAndClassName(int majorVersion, string className)
	  {
		this.majorVersion = majorVersion;
		this.className = className;
		return addConstantClass(className).index;
	  }

	  /// <summary>
	  /// Returns the number of items in this symbol table's constant_pool array (plus 1).
	  /// </summary>
	  /// <returns> the number of items in this symbol table's constant_pool array (plus 1). </returns>
	  public int ConstantPoolCount
	  {
		  get
		  {
			return constantPoolCount;
		  }
	  }

	  /// <summary>
	  /// Returns the length in bytes of this symbol table's constant_pool array.
	  /// </summary>
	  /// <returns> the length in bytes of this symbol table's constant_pool array. </returns>
	  public int ConstantPoolLength
	  {
		  get
		  {
			return constantPool.length;
		  }
	  }

	  /// <summary>
	  /// Puts this symbol table's constant_pool array in the given ByteVector, preceded by the
	  /// constant_pool_count value.
	  /// </summary>
	  /// <param name="output"> where the JVMS ClassFile's constant_pool array must be put. </param>
	  public void putConstantPool(ByteVector output)
	  {
		output.putShort(constantPoolCount).putByteArray(constantPool.data, 0, constantPool.length);
	  }

	  /// <summary>
	  /// Returns the size in bytes of this symbol table's BootstrapMethods attribute. Also adds the
	  /// attribute name in the constant pool.
	  /// </summary>
	  /// <returns> the size in bytes of this symbol table's BootstrapMethods attribute. </returns>
	  public int computeBootstrapMethodsSize()
	  {
		if (bootstrapMethods != null)
		{
		  addConstantUtf8(Constants.BOOTSTRAP_METHODS);
		  return 8 + bootstrapMethods.length;
		}
		else
		{
		  return 0;
		}
	  }

	  /// <summary>
	  /// Puts this symbol table's BootstrapMethods attribute in the given ByteVector. This includes the
	  /// 6 attribute header bytes and the num_bootstrap_methods value.
	  /// </summary>
	  /// <param name="output"> where the JVMS BootstrapMethods attribute must be put. </param>
	  public void putBootstrapMethods(ByteVector output)
	  {
		if (bootstrapMethods != null)
		{
		  output.putShort(addConstantUtf8(Constants.BOOTSTRAP_METHODS)).putInt(bootstrapMethods.length + 2).putShort(bootstrapMethodCount).putByteArray(bootstrapMethods.data, 0, bootstrapMethods.length);
		}
	  }

	  // -----------------------------------------------------------------------------------------------
	  // Generic symbol table entries management.
	  // -----------------------------------------------------------------------------------------------

	  /// <summary>
	  /// Returns the list of entries which can potentially have the given hash code.
	  /// </summary>
	  /// <param name="hashCode"> a <seealso cref="Entry.hashCode"/> value. </param>
	  /// <returns> the list of entries which can potentially have the given hash code. The list is stored
	  ///     via the <seealso cref="Entry.next"/> field. </returns>
	  private Entry get(int hashCode)
	  {
		return entries[hashCode % entries.Length];
	  }

	  /// <summary>
	  /// Puts the given entry in the <seealso cref="entries"/> hash set. This method does <i>not</i> check
	  /// whether <seealso cref="entries"/> already contains a similar entry or not. <seealso cref="entries"/> is resized
	  /// if necessary to avoid hash collisions (multiple entries needing to be stored at the same {@link
	  /// #entries} array index) as much as possible, with reasonable memory usage.
	  /// </summary>
	  /// <param name="entry"> an Entry (which must not already be contained in <seealso cref="entries"/>). </param>
	  /// <returns> the given entry </returns>
	  private Entry put(Entry entry)
	  {
		if (entryCount > (entries.Length * 3) / 4)
		{
		  int currentCapacity = entries.Length;
		  int newCapacity = currentCapacity * 2 + 1;
		  Entry[] newEntries = new Entry[newCapacity];
		  for (int i = currentCapacity - 1; i >= 0; --i)
		  {
			Entry currentEntry = entries[i];
			while (currentEntry != null)
			{
			  int newCurrentEntryIndex = currentEntry.hashCode % newCapacity;
			  Entry nextEntry = currentEntry.next;
			  currentEntry.next = newEntries[newCurrentEntryIndex];
			  newEntries[newCurrentEntryIndex] = currentEntry;
			  currentEntry = nextEntry;
			}
		  }
		  entries = newEntries;
		}
		entryCount++;
		int index = entry.hashCode % entries.Length;
		entry.next = entries[index];
		return entries[index] = entry;
	  }

	  /// <summary>
	  /// Adds the given entry in the <seealso cref="entries"/> hash set. This method does <i>not</i> check
	  /// whether <seealso cref="entries"/> already contains a similar entry or not, and does <i>not</i> resize
	  /// <seealso cref="entries"/> if necessary.
	  /// </summary>
	  /// <param name="entry"> an Entry (which must not already be contained in <seealso cref="entries"/>). </param>
	  private void add(Entry entry)
	  {
		entryCount++;
		int index = entry.hashCode % entries.Length;
		entry.next = entries[index];
		entries[index] = entry;
	  }

	  // -----------------------------------------------------------------------------------------------
	  // Constant pool entries management.
	  // -----------------------------------------------------------------------------------------------

	  /// <summary>
	  /// Adds a number or string constant to the constant pool of this symbol table. Does nothing if the
	  /// constant pool already contains a similar item.
	  /// </summary>
	  /// <param name="value"> the value of the constant to be added to the constant pool. This parameter must be
	  ///     an <seealso cref="Integer"/>, <seealso cref="Byte"/>, <seealso cref="Character"/>, <seealso cref="Short"/>, <seealso cref="Boolean"/>, {@link
	  ///     Float}, <seealso cref="Long"/>, <seealso cref="Double"/>, <seealso cref="string"/>, <seealso cref="Type"/> or <seealso cref="Handle"/>. </param>
	  /// <returns> a new or already existing Symbol with the given value. </returns>
	  public Symbol addConstant(object value)
	  {
		if (value is int || value is byte)
		{
		  return addConstantInteger(((int?) value).Value);
		}
		else if (value is sbyte)
		{
		  return addConstantInteger(((sbyte?) value).Value);
		}
		else if (value is char)
		{
		  return addConstantInteger(((char?) value).Value);
		}
		else if (value is short)
		{
		  return addConstantInteger(((short?) value).Value);
		}
		else if (value is Boolean)
		{
		  return addConstantInteger(((bool?) value).Value ? 1 : 0);
		}
		else if (value is float)
		{
		  return addConstantFloat(((float?) value).Value);
		}
		else if (value is long)
		{
		  return addConstantLong(((long?) value).Value);
		}
		else if (value is Double)
		{
		  return addConstantDouble(((double?) value).Value);
		}
		else if (value is string)
		{
		  return addConstantString((string) value);
		}
		else if (value is JType)
		{
		  JType type = (JType) value;
		  int typeSort = type.Sort;
		  if (typeSort == JType.OBJECT)
		  {
			return addConstantClass(type.InternalName);
		  }
		  else if (typeSort == JType.METHOD)
		  {
			return addConstantMethodType(type.Descriptor);
		  }
		  else
		  { // type is a primitive or array type.
			return addConstantClass(type.Descriptor);
		  }
		}
		else if (value is Handle)
		{
		  Handle handle = (Handle) value;
		  return addConstantMethodHandle(handle.Tag, handle.Owner, handle.Name, handle.Desc, handle.Interface);
		}
		else if (value is ConstantDynamic)
		{
		  ConstantDynamic constantDynamic = (ConstantDynamic) value;
		  return addConstantDynamic(constantDynamic.Name, constantDynamic.Descriptor, constantDynamic.BootstrapMethod, constantDynamic.BootstrapMethodArgumentsUnsafe);
		}
		else
		{
		  throw new System.ArgumentException("value " + value);
		}
	  }

	  /// <summary>
	  /// Adds a CONSTANT_Class_info to the constant pool of this symbol table. Does nothing if the
	  /// constant pool already contains a similar item.
	  /// </summary>
	  /// <param name="value"> the internal name of a class. </param>
	  /// <returns> a new or already existing Symbol with the given value. </returns>
	  public Symbol addConstantClass(string value)
	  {
		return addConstantUtf8Reference(Symbol.CONSTANT_CLASS_TAG, value);
	  }

	  /// <summary>
	  /// Adds a CONSTANT_Fieldref_info to the constant pool of this symbol table. Does nothing if the
	  /// constant pool already contains a similar item.
	  /// </summary>
	  /// <param name="owner"> the internal name of a class. </param>
	  /// <param name="name"> a field name. </param>
	  /// <param name="descriptor"> a field descriptor. </param>
	  /// <returns> a new or already existing Symbol with the given value. </returns>
	  public Symbol addConstantFieldref(string owner, string name, string descriptor)
	  {
		return addConstantMemberReference(Symbol.CONSTANT_FIELDREF_TAG, owner, name, descriptor);
	  }

	  /// <summary>
	  /// Adds a CONSTANT_Methodref_info or CONSTANT_InterfaceMethodref_info to the constant pool of this
	  /// symbol table. Does nothing if the constant pool already contains a similar item.
	  /// </summary>
	  /// <param name="owner"> the internal name of a class. </param>
	  /// <param name="name"> a method name. </param>
	  /// <param name="descriptor"> a method descriptor. </param>
	  /// <param name="isInterface"> whether owner is an interface or not. </param>
	  /// <returns> a new or already existing Symbol with the given value. </returns>
	  public Symbol addConstantMethodref(string owner, string name, string descriptor, bool isInterface)
	  {
		int tag = isInterface ? Symbol.CONSTANT_INTERFACE_METHODREF_TAG : Symbol.CONSTANT_METHODREF_TAG;
		return addConstantMemberReference(tag, owner, name, descriptor);
	  }

	  /// <summary>
	  /// Adds a CONSTANT_Fieldref_info, CONSTANT_Methodref_info or CONSTANT_InterfaceMethodref_info to
	  /// the constant pool of this symbol table. Does nothing if the constant pool already contains a
	  /// similar item.
	  /// </summary>
	  /// <param name="tag"> one of <seealso cref="Symbol.CONSTANT_FIELDREF_TAG"/>, <seealso cref="Symbol.CONSTANT_METHODREF_TAG"/>
	  ///     or <seealso cref="Symbol.CONSTANT_INTERFACE_METHODREF_TAG"/>. </param>
	  /// <param name="owner"> the internal name of a class. </param>
	  /// <param name="name"> a field or method name. </param>
	  /// <param name="descriptor"> a field or method descriptor. </param>
	  /// <returns> a new or already existing Symbol with the given value. </returns>
	  private Entry addConstantMemberReference(int tag, string owner, string name, string descriptor)
	  {
		int hashCode = hash(tag, owner, name, descriptor);
		Entry entry = get(hashCode);
		while (entry != null)
		{
		  if (entry.tag == tag && entry.hashCode == hashCode && entry.owner.Equals(owner) && entry.name.Equals(name) && entry.value.Equals(descriptor))
		  {
			return entry;
		  }
		  entry = entry.next;
		}
		constantPool.put122(tag, addConstantClass(owner).index, addConstantNameAndType(name, descriptor));
		return put(new Entry(constantPoolCount++, tag, owner, name, descriptor, 0, hashCode));
	  }

	  /// <summary>
	  /// Adds a new CONSTANT_Fieldref_info, CONSTANT_Methodref_info or CONSTANT_InterfaceMethodref_info
	  /// to the constant pool of this symbol table.
	  /// </summary>
	  /// <param name="index"> the constant pool index of the new Symbol. </param>
	  /// <param name="tag"> one of <seealso cref="Symbol.CONSTANT_FIELDREF_TAG"/>, <seealso cref="Symbol.CONSTANT_METHODREF_TAG"/>
	  ///     or <seealso cref="Symbol.CONSTANT_INTERFACE_METHODREF_TAG"/>. </param>
	  /// <param name="owner"> the internal name of a class. </param>
	  /// <param name="name"> a field or method name. </param>
	  /// <param name="descriptor"> a field or method descriptor. </param>
	  private void addConstantMemberReference(int index, int tag, string owner, string name, string descriptor)
	  {
		add(new Entry(index, tag, owner, name, descriptor, 0, hash(tag, owner, name, descriptor)));
	  }

	  /// <summary>
	  /// Adds a CONSTANT_String_info to the constant pool of this symbol table. Does nothing if the
	  /// constant pool already contains a similar item.
	  /// </summary>
	  /// <param name="value"> a string. </param>
	  /// <returns> a new or already existing Symbol with the given value. </returns>
	  public Symbol addConstantString(string value)
	  {
		return addConstantUtf8Reference(Symbol.CONSTANT_STRING_TAG, value);
	  }

	  /// <summary>
	  /// Adds a CONSTANT_Integer_info to the constant pool of this symbol table. Does nothing if the
	  /// constant pool already contains a similar item.
	  /// </summary>
	  /// <param name="value"> an int. </param>
	  /// <returns> a new or already existing Symbol with the given value. </returns>
	  public Symbol addConstantInteger(int value)
	  {
		return addConstantIntegerOrFloat(Symbol.CONSTANT_INTEGER_TAG, value);
	  }

	  /// <summary>
	  /// Adds a CONSTANT_Float_info to the constant pool of this symbol table. Does nothing if the
	  /// constant pool already contains a similar item.
	  /// </summary>
	  /// <param name="value"> a float. </param>
	  /// <returns> a new or already existing Symbol with the given value. </returns>
	  public Symbol addConstantFloat(float value)
	  {
		return addConstantIntegerOrFloat(Symbol.CONSTANT_FLOAT_TAG, BitConverter.SingleToInt32Bits(value));
	  }

	  /// <summary>
	  /// Adds a CONSTANT_Integer_info or CONSTANT_Float_info to the constant pool of this symbol table.
	  /// Does nothing if the constant pool already contains a similar item.
	  /// </summary>
	  /// <param name="tag"> one of <seealso cref="Symbol.CONSTANT_INTEGER_TAG"/> or <seealso cref="Symbol.CONSTANT_FLOAT_TAG"/>. </param>
	  /// <param name="value"> an int or float. </param>
	  /// <returns> a constant pool constant with the given tag and primitive values. </returns>
	  private Symbol addConstantIntegerOrFloat(int tag, int value)
	  {
		int hashCode = hash(tag, value);
		Entry entry = get(hashCode);
		while (entry != null)
		{
		  if (entry.tag == tag && entry.hashCode == hashCode && entry.data == value)
		  {
			return entry;
		  }
		  entry = entry.next;
		}
		constantPool.putByte(tag).putInt(value);
		return put(new Entry(constantPoolCount++, tag, value, hashCode));
	  }

	  /// <summary>
	  /// Adds a new CONSTANT_Integer_info or CONSTANT_Float_info to the constant pool of this symbol
	  /// table.
	  /// </summary>
	  /// <param name="index"> the constant pool index of the new Symbol. </param>
	  /// <param name="tag"> one of <seealso cref="Symbol.CONSTANT_INTEGER_TAG"/> or <seealso cref="Symbol.CONSTANT_FLOAT_TAG"/>. </param>
	  /// <param name="value"> an int or float. </param>
	  private void addConstantIntegerOrFloat(int index, int tag, int value)
	  {
		add(new Entry(index, tag, value, hash(tag, value)));
	  }

	  /// <summary>
	  /// Adds a CONSTANT_Long_info to the constant pool of this symbol table. Does nothing if the
	  /// constant pool already contains a similar item.
	  /// </summary>
	  /// <param name="value"> a long. </param>
	  /// <returns> a new or already existing Symbol with the given value. </returns>
	  public Symbol addConstantLong(long value)
	  {
		return addConstantLongOrDouble(Symbol.CONSTANT_LONG_TAG, value);
	  }

	  /// <summary>
	  /// Adds a CONSTANT_Double_info to the constant pool of this symbol table. Does nothing if the
	  /// constant pool already contains a similar item.
	  /// </summary>
	  /// <param name="value"> a double. </param>
	  /// <returns> a new or already existing Symbol with the given value. </returns>
	  public Symbol addConstantDouble(double value)
	  {
		return addConstantLongOrDouble(Symbol.CONSTANT_DOUBLE_TAG, BitConverter.DoubleToInt64Bits(value));
	  }

	  /// <summary>
	  /// Adds a CONSTANT_Long_info or CONSTANT_Double_info to the constant pool of this symbol table.
	  /// Does nothing if the constant pool already contains a similar item.
	  /// </summary>
	  /// <param name="tag"> one of <seealso cref="Symbol.CONSTANT_LONG_TAG"/> or <seealso cref="Symbol.CONSTANT_DOUBLE_TAG"/>. </param>
	  /// <param name="value"> a long or double. </param>
	  /// <returns> a constant pool constant with the given tag and primitive values. </returns>
	  private Symbol addConstantLongOrDouble(int tag, long value)
	  {
		int hashCode = hash(tag, value);
		Entry entry = get(hashCode);
		while (entry != null)
		{
		  if (entry.tag == tag && entry.hashCode == hashCode && entry.data == value)
		  {
			return entry;
		  }
		  entry = entry.next;
		}
		int index = constantPoolCount;
		constantPool.putByte(tag).putLong(value);
		constantPoolCount += 2;
		return put(new Entry(index, tag, value, hashCode));
	  }

	  /// <summary>
	  /// Adds a new CONSTANT_Long_info or CONSTANT_Double_info to the constant pool of this symbol
	  /// table.
	  /// </summary>
	  /// <param name="index"> the constant pool index of the new Symbol. </param>
	  /// <param name="tag"> one of <seealso cref="Symbol.CONSTANT_LONG_TAG"/> or <seealso cref="Symbol.CONSTANT_DOUBLE_TAG"/>. </param>
	  /// <param name="value"> a long or double. </param>
	  private void addConstantLongOrDouble(int index, int tag, long value)
	  {
		add(new Entry(index, tag, value, hash(tag, value)));
	  }

	  /// <summary>
	  /// Adds a CONSTANT_NameAndType_info to the constant pool of this symbol table. Does nothing if the
	  /// constant pool already contains a similar item.
	  /// </summary>
	  /// <param name="name"> a field or method name. </param>
	  /// <param name="descriptor"> a field or method descriptor. </param>
	  /// <returns> a new or already existing Symbol with the given value. </returns>
	  public int addConstantNameAndType(string name, string descriptor)
	  {
		const int tag = Symbol.CONSTANT_NAME_AND_TYPE_TAG;
		int hashCode = hash(tag, name, descriptor);
		Entry entry = get(hashCode);
		while (entry != null)
		{
		  if (entry.tag == tag && entry.hashCode == hashCode && entry.name.Equals(name) && entry.value.Equals(descriptor))
		  {
			return entry.index;
		  }
		  entry = entry.next;
		}
		constantPool.put122(tag, addConstantUtf8(name), addConstantUtf8(descriptor));
		return put(new Entry(constantPoolCount++, tag, name, descriptor, hashCode)).index;
	  }

	  /// <summary>
	  /// Adds a new CONSTANT_NameAndType_info to the constant pool of this symbol table.
	  /// </summary>
	  /// <param name="index"> the constant pool index of the new Symbol. </param>
	  /// <param name="name"> a field or method name. </param>
	  /// <param name="descriptor"> a field or method descriptor. </param>
	  private void addConstantNameAndType(int index, string name, string descriptor)
	  {
		const int tag = Symbol.CONSTANT_NAME_AND_TYPE_TAG;
		add(new Entry(index, tag, name, descriptor, hash(tag, name, descriptor)));
	  }

	  /// <summary>
	  /// Adds a CONSTANT_Utf8_info to the constant pool of this symbol table. Does nothing if the
	  /// constant pool already contains a similar item.
	  /// </summary>
	  /// <param name="value"> a string. </param>
	  /// <returns> a new or already existing Symbol with the given value. </returns>
	  public int addConstantUtf8(string value)
	  {
		int hashCode = hash(Symbol.CONSTANT_UTF8_TAG, value);
		Entry entry = get(hashCode);
		while (entry != null)
		{
		  if (entry.tag == Symbol.CONSTANT_UTF8_TAG && entry.hashCode == hashCode && entry.value.Equals(value))
		  {
			return entry.index;
		  }
		  entry = entry.next;
		}
		constantPool.putByte(Symbol.CONSTANT_UTF8_TAG).putUTF8(value);
		return put(new Entry(constantPoolCount++, Symbol.CONSTANT_UTF8_TAG, value, hashCode)).index;
	  }

	  /// <summary>
	  /// Adds a new CONSTANT_String_info to the constant pool of this symbol table.
	  /// </summary>
	  /// <param name="index"> the constant pool index of the new Symbol. </param>
	  /// <param name="value"> a string. </param>
	  private void addConstantUtf8(int index, string value)
	  {
		add(new Entry(index, Symbol.CONSTANT_UTF8_TAG, value, hash(Symbol.CONSTANT_UTF8_TAG, value)));
	  }

	  /// <summary>
	  /// Adds a CONSTANT_MethodHandle_info to the constant pool of this symbol table. Does nothing if
	  /// the constant pool already contains a similar item.
	  /// </summary>
	  /// <param name="referenceKind"> one of <seealso cref="Opcodes.H_GETFIELD"/>, <seealso cref="Opcodes.H_GETSTATIC"/>, {@link
	  ///     Opcodes#H_PUTFIELD}, <seealso cref="Opcodes.H_PUTSTATIC"/>, <seealso cref="Opcodes.H_INVOKEVIRTUAL"/>, {@link
	  ///     Opcodes#H_INVOKESTATIC}, <seealso cref="Opcodes.H_INVOKESPECIAL"/>, {@link
	  ///     Opcodes#H_NEWINVOKESPECIAL} or <seealso cref="Opcodes.H_INVOKEINTERFACE"/>. </param>
	  /// <param name="owner"> the internal name of a class of interface. </param>
	  /// <param name="name"> a field or method name. </param>
	  /// <param name="descriptor"> a field or method descriptor. </param>
	  /// <param name="isInterface"> whether owner is an interface or not. </param>
	  /// <returns> a new or already existing Symbol with the given value. </returns>
	  public Symbol addConstantMethodHandle(int referenceKind, string owner, string name, string descriptor, bool isInterface)
	  {
		const int tag = Symbol.CONSTANT_METHOD_HANDLE_TAG;
		// Note that we don't need to include isInterface in the hash computation, because it is
		// redundant with owner (we can't have the same owner with different isInterface values).
		int hashCode = hash(tag, owner, name, descriptor, referenceKind);
		Entry entry = get(hashCode);
		while (entry != null)
		{
		  if (entry.tag == tag && entry.hashCode == hashCode && entry.data == referenceKind && entry.owner.Equals(owner) && entry.name.Equals(name) && entry.value.Equals(descriptor))
		  {
			return entry;
		  }
		  entry = entry.next;
		}
		if (referenceKind <= Opcodes.H_PUTSTATIC)
		{
		  constantPool.put112(tag, referenceKind, addConstantFieldref(owner, name, descriptor).index);
		}
		else
		{
		  constantPool.put112(tag, referenceKind, addConstantMethodref(owner, name, descriptor, isInterface).index);
		}
		return put(new Entry(constantPoolCount++, tag, owner, name, descriptor, referenceKind, hashCode));
	  }

	  /// <summary>
	  /// Adds a new CONSTANT_MethodHandle_info to the constant pool of this symbol table.
	  /// </summary>
	  /// <param name="index"> the constant pool index of the new Symbol. </param>
	  /// <param name="referenceKind"> one of <seealso cref="Opcodes.H_GETFIELD"/>, <seealso cref="Opcodes.H_GETSTATIC"/>, {@link
	  ///     Opcodes#H_PUTFIELD}, <seealso cref="Opcodes.H_PUTSTATIC"/>, <seealso cref="Opcodes.H_INVOKEVIRTUAL"/>, {@link
	  ///     Opcodes#H_INVOKESTATIC}, <seealso cref="Opcodes.H_INVOKESPECIAL"/>, {@link
	  ///     Opcodes#H_NEWINVOKESPECIAL} or <seealso cref="Opcodes.H_INVOKEINTERFACE"/>. </param>
	  /// <param name="owner"> the internal name of a class of interface. </param>
	  /// <param name="name"> a field or method name. </param>
	  /// <param name="descriptor"> a field or method descriptor. </param>
	  private void addConstantMethodHandle(int index, int referenceKind, string owner, string name, string descriptor)
	  {
		const int tag = Symbol.CONSTANT_METHOD_HANDLE_TAG;
		int hashCode = hash(tag, owner, name, descriptor, referenceKind);
		add(new Entry(index, tag, owner, name, descriptor, referenceKind, hashCode));
	  }

	  /// <summary>
	  /// Adds a CONSTANT_MethodType_info to the constant pool of this symbol table. Does nothing if the
	  /// constant pool already contains a similar item.
	  /// </summary>
	  /// <param name="methodDescriptor"> a method descriptor. </param>
	  /// <returns> a new or already existing Symbol with the given value. </returns>
	  public Symbol addConstantMethodType(string methodDescriptor)
	  {
		return addConstantUtf8Reference(Symbol.CONSTANT_METHOD_TYPE_TAG, methodDescriptor);
	  }

	  /// <summary>
	  /// Adds a CONSTANT_Dynamic_info to the constant pool of this symbol table. Also adds the related
	  /// bootstrap method to the BootstrapMethods of this symbol table. Does nothing if the constant
	  /// pool already contains a similar item.
	  /// </summary>
	  /// <param name="name"> a method name. </param>
	  /// <param name="descriptor"> a field descriptor. </param>
	  /// <param name="bootstrapMethodHandle"> a bootstrap method handle. </param>
	  /// <param name="bootstrapMethodArguments"> the bootstrap method arguments. </param>
	  /// <returns> a new or already existing Symbol with the given value. </returns>
	  public Symbol addConstantDynamic(string name, string descriptor, Handle bootstrapMethodHandle, params object[] bootstrapMethodArguments)
	  {
		Symbol bootstrapMethod = addBootstrapMethod(bootstrapMethodHandle, bootstrapMethodArguments);
		return addConstantDynamicOrInvokeDynamicReference(Symbol.CONSTANT_DYNAMIC_TAG, name, descriptor, bootstrapMethod.index);
	  }

	  /// <summary>
	  /// Adds a CONSTANT_InvokeDynamic_info to the constant pool of this symbol table. Also adds the
	  /// related bootstrap method to the BootstrapMethods of this symbol table. Does nothing if the
	  /// constant pool already contains a similar item.
	  /// </summary>
	  /// <param name="name"> a method name. </param>
	  /// <param name="descriptor"> a method descriptor. </param>
	  /// <param name="bootstrapMethodHandle"> a bootstrap method handle. </param>
	  /// <param name="bootstrapMethodArguments"> the bootstrap method arguments. </param>
	  /// <returns> a new or already existing Symbol with the given value. </returns>
	  public Symbol addConstantInvokeDynamic(string name, string descriptor, Handle bootstrapMethodHandle, params object[] bootstrapMethodArguments)
	  {
		Symbol bootstrapMethod = addBootstrapMethod(bootstrapMethodHandle, bootstrapMethodArguments);
		return addConstantDynamicOrInvokeDynamicReference(Symbol.CONSTANT_INVOKE_DYNAMIC_TAG, name, descriptor, bootstrapMethod.index);
	  }

	  /// <summary>
	  /// Adds a CONSTANT_Dynamic or a CONSTANT_InvokeDynamic_info to the constant pool of this symbol
	  /// table. Does nothing if the constant pool already contains a similar item.
	  /// </summary>
	  /// <param name="tag"> one of <seealso cref="Symbol.CONSTANT_DYNAMIC_TAG"/> or {@link
	  ///     Symbol#CONSTANT_INVOKE_DYNAMIC_TAG}. </param>
	  /// <param name="name"> a method name. </param>
	  /// <param name="descriptor"> a field descriptor for CONSTANT_DYNAMIC_TAG) or a method descriptor for
	  ///     CONSTANT_INVOKE_DYNAMIC_TAG. </param>
	  /// <param name="bootstrapMethodIndex"> the index of a bootstrap method in the BootstrapMethods attribute. </param>
	  /// <returns> a new or already existing Symbol with the given value. </returns>
	  private Symbol addConstantDynamicOrInvokeDynamicReference(int tag, string name, string descriptor, int bootstrapMethodIndex)
	  {
		int hashCode = hash(tag, name, descriptor, bootstrapMethodIndex);
		Entry entry = get(hashCode);
		while (entry != null)
		{
		  if (entry.tag == tag && entry.hashCode == hashCode && entry.data == bootstrapMethodIndex && entry.name.Equals(name) && entry.value.Equals(descriptor))
		  {
			return entry;
		  }
		  entry = entry.next;
		}
		constantPool.put122(tag, bootstrapMethodIndex, addConstantNameAndType(name, descriptor));
		return put(new Entry(constantPoolCount++, tag, null, name, descriptor, bootstrapMethodIndex, hashCode));
	  }

	  /// <summary>
	  /// Adds a new CONSTANT_Dynamic_info or CONSTANT_InvokeDynamic_info to the constant pool of this
	  /// symbol table.
	  /// </summary>
	  /// <param name="tag"> one of <seealso cref="Symbol.CONSTANT_DYNAMIC_TAG"/> or {@link
	  ///     Symbol#CONSTANT_INVOKE_DYNAMIC_TAG}. </param>
	  /// <param name="index"> the constant pool index of the new Symbol. </param>
	  /// <param name="name"> a method name. </param>
	  /// <param name="descriptor"> a field descriptor for CONSTANT_DYNAMIC_TAG or a method descriptor for
	  ///     CONSTANT_INVOKE_DYNAMIC_TAG. </param>
	  /// <param name="bootstrapMethodIndex"> the index of a bootstrap method in the BootstrapMethods attribute. </param>
	  private void addConstantDynamicOrInvokeDynamicReference(int tag, int index, string name, string descriptor, int bootstrapMethodIndex)
	  {
		int hashCode = hash(tag, name, descriptor, bootstrapMethodIndex);
		add(new Entry(index, tag, null, name, descriptor, bootstrapMethodIndex, hashCode));
	  }

	  /// <summary>
	  /// Adds a CONSTANT_Module_info to the constant pool of this symbol table. Does nothing if the
	  /// constant pool already contains a similar item.
	  /// </summary>
	  /// <param name="moduleName"> a fully qualified name (using dots) of a module. </param>
	  /// <returns> a new or already existing Symbol with the given value. </returns>
	  public Symbol addConstantModule(string moduleName)
	  {
		return addConstantUtf8Reference(Symbol.CONSTANT_MODULE_TAG, moduleName);
	  }

	  /// <summary>
	  /// Adds a CONSTANT_Package_info to the constant pool of this symbol table. Does nothing if the
	  /// constant pool already contains a similar item.
	  /// </summary>
	  /// <param name="packageName"> the internal name of a package. </param>
	  /// <returns> a new or already existing Symbol with the given value. </returns>
	  public Symbol addConstantPackage(string packageName)
	  {
		return addConstantUtf8Reference(Symbol.CONSTANT_PACKAGE_TAG, packageName);
	  }

	  /// <summary>
	  /// Adds a CONSTANT_Class_info, CONSTANT_String_info, CONSTANT_MethodType_info,
	  /// CONSTANT_Module_info or CONSTANT_Package_info to the constant pool of this symbol table. Does
	  /// nothing if the constant pool already contains a similar item.
	  /// </summary>
	  /// <param name="tag"> one of <seealso cref="Symbol.CONSTANT_CLASS_TAG"/>, <seealso cref="Symbol.CONSTANT_STRING_TAG"/>, {@link
	  ///     Symbol#CONSTANT_METHOD_TYPE_TAG}, <seealso cref="Symbol.CONSTANT_MODULE_TAG"/> or {@link
	  ///     Symbol#CONSTANT_PACKAGE_TAG}. </param>
	  /// <param name="value"> an internal class name, an arbitrary string, a method descriptor, a module or a
	  ///     package name, depending on tag. </param>
	  /// <returns> a new or already existing Symbol with the given value. </returns>
	  private Symbol addConstantUtf8Reference(int tag, string value)
	  {
		int hashCode = hash(tag, value);
		Entry entry = get(hashCode);
		while (entry != null)
		{
		  if (entry.tag == tag && entry.hashCode == hashCode && entry.value.Equals(value))
		  {
			return entry;
		  }
		  entry = entry.next;
		}
		constantPool.put12(tag, addConstantUtf8(value));
		return put(new Entry(constantPoolCount++, tag, value, hashCode));
	  }

	  /// <summary>
	  /// Adds a new CONSTANT_Class_info, CONSTANT_String_info, CONSTANT_MethodType_info,
	  /// CONSTANT_Module_info or CONSTANT_Package_info to the constant pool of this symbol table.
	  /// </summary>
	  /// <param name="index"> the constant pool index of the new Symbol. </param>
	  /// <param name="tag"> one of <seealso cref="Symbol.CONSTANT_CLASS_TAG"/>, <seealso cref="Symbol.CONSTANT_STRING_TAG"/>, {@link
	  ///     Symbol#CONSTANT_METHOD_TYPE_TAG}, <seealso cref="Symbol.CONSTANT_MODULE_TAG"/> or {@link
	  ///     Symbol#CONSTANT_PACKAGE_TAG}. </param>
	  /// <param name="value"> an internal class name, an arbitrary string, a method descriptor, a module or a
	  ///     package name, depending on tag. </param>
	  private void addConstantUtf8Reference(int index, int tag, string value)
	  {
		add(new Entry(index, tag, value, hash(tag, value)));
	  }

	  // -----------------------------------------------------------------------------------------------
	  // Bootstrap method entries management.
	  // -----------------------------------------------------------------------------------------------

	  /// <summary>
	  /// Adds a bootstrap method to the BootstrapMethods attribute of this symbol table. Does nothing if
	  /// the BootstrapMethods already contains a similar bootstrap method.
	  /// </summary>
	  /// <param name="bootstrapMethodHandle"> a bootstrap method handle. </param>
	  /// <param name="bootstrapMethodArguments"> the bootstrap method arguments. </param>
	  /// <returns> a new or already existing Symbol with the given value. </returns>
	  public Symbol addBootstrapMethod(Handle bootstrapMethodHandle, params object[] bootstrapMethodArguments)
	  {
		ByteVector bootstrapMethodsAttribute = bootstrapMethods;
		if (bootstrapMethodsAttribute == null)
		{
		  bootstrapMethodsAttribute = bootstrapMethods = new ByteVector();
		}

		// The bootstrap method arguments can be Constant_Dynamic values, which reference other
		// bootstrap methods. We must therefore add the bootstrap method arguments to the constant pool
		// and BootstrapMethods attribute first, so that the BootstrapMethods attribute is not modified
		// while adding the given bootstrap method to it, in the rest of this method.
		int numBootstrapArguments = bootstrapMethodArguments.Length;
		int[] bootstrapMethodArgumentIndexes = new int[numBootstrapArguments];
		for (int i = 0; i < numBootstrapArguments; i++)
		{
		  bootstrapMethodArgumentIndexes[i] = addConstant(bootstrapMethodArguments[i]).index;
		}

		// Write the bootstrap method in the BootstrapMethods table. This is necessary to be able to
		// compare it with existing ones, and will be reverted below if there is already a similar
		// bootstrap method.
		int bootstrapMethodOffset = bootstrapMethodsAttribute.length;
		bootstrapMethodsAttribute.putShort(addConstantMethodHandle(bootstrapMethodHandle.Tag, bootstrapMethodHandle.Owner, bootstrapMethodHandle.Name, bootstrapMethodHandle.Desc, bootstrapMethodHandle.Interface).index);

		bootstrapMethodsAttribute.putShort(numBootstrapArguments);
		for (int i = 0; i < numBootstrapArguments; i++)
		{
		  bootstrapMethodsAttribute.putShort(bootstrapMethodArgumentIndexes[i]);
		}

		// Compute the length and the hash code of the bootstrap method.
		int bootstrapMethodlength = bootstrapMethodsAttribute.length - bootstrapMethodOffset;
		int hashCode = bootstrapMethodHandle.GetHashCode();
		foreach (object bootstrapMethodArgument in bootstrapMethodArguments)
		{
		  hashCode ^= bootstrapMethodArgument.GetHashCode();
		}
		hashCode &= 0x7FFFFFFF;

		// Add the bootstrap method to the symbol table or revert the above changes.
		return addBootstrapMethod(bootstrapMethodOffset, bootstrapMethodlength, hashCode);
	  }

	  /// <summary>
	  /// Adds a bootstrap method to the BootstrapMethods attribute of this symbol table. Does nothing if
	  /// the BootstrapMethods already contains a similar bootstrap method (more precisely, reverts the
	  /// content of <seealso cref="bootstrapMethods"/> to remove the last, duplicate bootstrap method).
	  /// </summary>
	  /// <param name="offset"> the offset of the last bootstrap method in <seealso cref="bootstrapMethods"/>, in bytes. </param>
	  /// <param name="length"> the length of this bootstrap method in <seealso cref="bootstrapMethods"/>, in bytes. </param>
	  /// <param name="hashCode"> the hash code of this bootstrap method. </param>
	  /// <returns> a new or already existing Symbol with the given value. </returns>
	  private Symbol addBootstrapMethod(int offset, int length, int hashCode)
	  {
		sbyte[] bootstrapMethodsData = bootstrapMethods.data;
		Entry entry = get(hashCode);
		while (entry != null)
		{
		  if (entry.tag == Symbol.BOOTSTRAP_METHOD_TAG && entry.hashCode == hashCode)
		  {
			int otherOffset = (int) entry.data;
			bool isSameBootstrapMethod = true;
			for (int i = 0; i < length; ++i)
			{
			  if (bootstrapMethodsData[offset + i] != bootstrapMethodsData[otherOffset + i])
			  {
				isSameBootstrapMethod = false;
				break;
			  }
			}
			if (isSameBootstrapMethod)
			{
			  bootstrapMethods.length = offset; // Revert to old position.
			  return entry;
			}
		  }
		  entry = entry.next;
		}
		return put(new Entry(bootstrapMethodCount++, Symbol.BOOTSTRAP_METHOD_TAG, offset, hashCode));
	  }

	  // -----------------------------------------------------------------------------------------------
	  // Type table entries management.
	  // -----------------------------------------------------------------------------------------------

	  /// <summary>
	  /// Returns the type table element whose index is given.
	  /// </summary>
	  /// <param name="typeIndex"> a type table index. </param>
	  /// <returns> the type table element whose index is given. </returns>
	  public Symbol getType(int typeIndex)
	  {
		return typeTable[typeIndex];
	  }

	  /// <summary>
	  /// Adds a type in the type table of this symbol table. Does nothing if the type table already
	  /// contains a similar type.
	  /// </summary>
	  /// <param name="value"> an internal class name. </param>
	  /// <returns> the index of a new or already existing type Symbol with the given value. </returns>
	  public int addType(string value)
	  {
		int hashCode = hash(Symbol.TYPE_TAG, value);
		Entry entry = get(hashCode);
		while (entry != null)
		{
		  if (entry.tag == Symbol.TYPE_TAG && entry.hashCode == hashCode && entry.value.Equals(value))
		  {
			return entry.index;
		  }
		  entry = entry.next;
		}
		return addTypeInternal(new Entry(typeCount, Symbol.TYPE_TAG, value, hashCode));
	  }

	  /// <summary>
	  /// Adds an <seealso cref="Frame.ITEM_UNINITIALIZED"/> type in the type table of this symbol table. Does
	  /// nothing if the type table already contains a similar type.
	  /// </summary>
	  /// <param name="value"> an internal class name. </param>
	  /// <param name="bytecodeOffset"> the bytecode offset of the NEW instruction that created this {@link
	  ///     Frame#ITEM_UNINITIALIZED} type value. </param>
	  /// <returns> the index of a new or already existing type Symbol with the given value. </returns>
	  public int addUninitializedType(string value, int bytecodeOffset)
	  {
		int hashCode = hash(Symbol.UNINITIALIZED_TYPE_TAG, value, bytecodeOffset);
		Entry entry = get(hashCode);
		while (entry != null)
		{
		  if (entry.tag == Symbol.UNINITIALIZED_TYPE_TAG && entry.hashCode == hashCode && entry.data == bytecodeOffset && entry.value.Equals(value))
		  {
			return entry.index;
		  }
		  entry = entry.next;
		}
		return addTypeInternal(new Entry(typeCount, Symbol.UNINITIALIZED_TYPE_TAG, value, bytecodeOffset, hashCode));
	  }

	  /// <summary>
	  /// Adds a merged type in the type table of this symbol table. Does nothing if the type table
	  /// already contains a similar type.
	  /// </summary>
	  /// <param name="typeTableIndex1"> a <seealso cref="Symbol.TYPE_TAG"/> type, specified by its index in the type
	  ///     table. </param>
	  /// <param name="typeTableIndex2"> another <seealso cref="Symbol.TYPE_TAG"/> type, specified by its index in the type
	  ///     table. </param>
	  /// <returns> the index of a new or already existing <seealso cref="Symbol.TYPE_TAG"/> type Symbol,
	  ///     corresponding to the common super class of the given types. </returns>
	  public int addMergedType(int typeTableIndex1, int typeTableIndex2)
	  {
		long data = typeTableIndex1 < typeTableIndex2 ? (uint)typeTableIndex1 | (((long) typeTableIndex2) << 32) : typeTableIndex2 | (((long) typeTableIndex1) << 32);
		int hashCode = hash(Symbol.MERGED_TYPE_TAG, typeTableIndex1 + typeTableIndex2);
		Entry entry = get(hashCode);
		while (entry != null)
		{
		  if (entry.tag == Symbol.MERGED_TYPE_TAG && entry.hashCode == hashCode && entry.data == data)
		  {
			return entry.info;
		  }
		  entry = entry.next;
		}
		string type1 = typeTable[typeTableIndex1].value;
		string type2 = typeTable[typeTableIndex2].value;
		int commonSuperTypeIndex = addType(classWriter.getCommonSuperClass(type1, type2));
		put(new Entry(typeCount, Symbol.MERGED_TYPE_TAG, data, hashCode)).info = commonSuperTypeIndex;
		return commonSuperTypeIndex;
	  }

	  /// <summary>
	  /// Adds the given type Symbol to <seealso cref="typeTable"/>.
	  /// </summary>
	  /// <param name="entry"> a <seealso cref="Symbol.TYPE_TAG"/> or <seealso cref="Symbol.UNINITIALIZED_TYPE_TAG"/> type symbol.
	  ///     The index of this Symbol must be equal to the current value of <seealso cref="typeCount"/>. </param>
	  /// <returns> the index in <seealso cref="typeTable"/> where the given type was added, which is also equal to
	  ///     entry's index by hypothesis. </returns>
	  private int addTypeInternal(Entry entry)
	  {
		if (typeTable == null)
		{
		  typeTable = new Entry[16];
		}
		if (typeCount == typeTable.Length)
		{
		  Entry[] newTypeTable = new Entry[2 * typeTable.Length];
		  Array.Copy(typeTable, 0, newTypeTable, 0, typeTable.Length);
		  typeTable = newTypeTable;
		}
		typeTable[typeCount++] = entry;
		return put(entry).index;
	  }

	  // -----------------------------------------------------------------------------------------------
	  // Static helper methods to compute hash codes.
	  // -----------------------------------------------------------------------------------------------

	  private static int hash(int tag, int value)
	  {
		return 0x7FFFFFFF & (tag + value);
	  }

	  private static int hash(int tag, long value)
	  {
		return 0x7FFFFFFF & (tag + (int) value + (int)((long)((ulong)value >> 32)));
	  }

	  private static int hash(int tag, string value)
	  {
		return 0x7FFFFFFF & (tag + value.GetHashCode());
	  }

	  private static int hash(int tag, string value1, int value2)
	  {
		return 0x7FFFFFFF & (tag + value1.GetHashCode() + value2);
	  }

	  private static int hash(int tag, string value1, string value2)
	  {
		return 0x7FFFFFFF & (tag + value1.GetHashCode() * value2.GetHashCode());
	  }

	  private static int hash(int tag, string value1, string value2, int value3)
	  {
		return 0x7FFFFFFF & (tag + value1.GetHashCode() * value2.GetHashCode() * (value3 + 1));
	  }

	  private static int hash(int tag, string value1, string value2, string value3)
	  {
		return 0x7FFFFFFF & (tag + value1.GetHashCode() * value2.GetHashCode() * value3.GetHashCode());
	  }

	  private static int hash(int tag, string value1, string value2, string value3, int value4)
	  {
		return 0x7FFFFFFF & (tag + value1.GetHashCode() * value2.GetHashCode() * value3.GetHashCode() * value4);
	  }

	  /// <summary>
	  /// An entry of a SymbolTable. This concrete and private subclass of <seealso cref="Symbol"/> adds two fields
	  /// which are only used inside SymbolTable, to implement hash sets of symbols (in order to avoid
	  /// duplicate symbols). See <seealso cref="entries"/>.
	  /// 
	  /// @author Eric Bruneton
	  /// </summary>
	  private class Entry : Symbol
	  {

		/// <summary>
		/// The hash code of this entry. </summary>
		internal readonly int hashCode;

		/// <summary>
		/// Another entry (and so on recursively) having the same hash code (modulo the size of {@link
		/// #entries}) as this one.
		/// </summary>
		internal Entry next;

		public Entry(int index, int tag, string owner, string name, string value, long data, int hashCode) : base(index, tag, owner, name, value, data)
		{
		  this.hashCode = hashCode;
		}

		public Entry(int index, int tag, string value, int hashCode) : base(index, tag, null, null, value, 0)
		{
		  this.hashCode = hashCode;
		}

		public Entry(int index, int tag, string value, long data, int hashCode) : base(index, tag, null, null, value, data)
		{
		  this.hashCode = hashCode;
		}

		public Entry(int index, int tag, string name, string value, int hashCode) : base(index, tag, null, name, value, 0)
		{
		  this.hashCode = hashCode;
		}

		public Entry(int index, int tag, long data, int hashCode) : base(index, tag, null, null, null, data)
		{
		  this.hashCode = hashCode;
		}
	  }
	}

}