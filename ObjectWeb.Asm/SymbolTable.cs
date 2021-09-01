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
    ///     The constant pool entries, the BootstrapMethods attribute entries and the (ASM specific) type
    ///     table entries of a class.
    ///     @author Eric Bruneton
    /// </summary>
    /// <seealso cref=
    /// <a href="https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.4">
    ///     JVMS
    ///     4.4
    /// </a>
    /// </seealso>
    /// <seealso cref=
    /// <a href="https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.7.23">
    ///     JVMS
    ///     4.7.23
    /// </a>
    /// </seealso>
    public sealed class SymbolTable
    {
        /// <summary>
        ///     The ClassWriter to which this SymbolTable belongs. This is only used to get access to {@link
        ///     ClassWriter#getCommonSuperClass} and to serialize custom attributes with {@link
        ///     Attribute#write}.
        /// </summary>
        internal readonly ClassWriter classWriter;

        /// <summary>
        ///     The ClassReader from which this SymbolTable was constructed, or {@literal null} if it was
        ///     constructed from scratch.
        /// </summary>
        private readonly ClassReader _sourceClassReader;

        /// <summary>
        ///     The number of bootstrap methods in <seealso cref="_bootstrapMethods" />. Corresponds to the
        ///     BootstrapMethods_attribute's num_bootstrap_methods field value.
        /// </summary>
        private int _bootstrapMethodCount;

        /// <summary>
        ///     The content of the BootstrapMethods attribute 'bootstrap_methods' array corresponding to this
        ///     SymbolTable. Note that the first 6 bytes of the BootstrapMethods_attribute, and its
        ///     num_bootstrap_methods field, are <i>not</i> included.
        /// </summary>
        private ByteVector _bootstrapMethods;

        /// <summary>
        ///     The internal name of the class to which this symbol table belongs.
        /// </summary>
        private string _className;

        /// <summary>
        ///     The content of the ClassFile's constant_pool JVMS structure corresponding to this SymbolTable.
        ///     The ClassFile's constant_pool_count field is <i>not</i> included.
        /// </summary>
        private readonly ByteVector _constantPool;

        /// <summary>
        ///     The number of constant pool items in <seealso cref="_constantPool" />, plus 1. The first constant pool
        ///     item has index 1, and long and double items count for two items.
        /// </summary>
        private int _constantPoolCount;

        /// <summary>
        ///     A hash set of all the entries in this SymbolTable (this includes the constant pool entries, the
        ///     bootstrap method entries and the type table entries). Each <seealso cref="Entry" /> instance is stored at
        ///     the array index given by its hash code modulo the array size. If several entries must be stored
        ///     at the same array index, they are linked together via their <seealso cref="Entry.next" /> field. The
        ///     factory methods of this class make sure that this table does not contain duplicated entries.
        /// </summary>
        private Entry[] _entries;

        /// <summary>
        ///     The total number of <seealso cref="Entry" /> instances in <seealso cref="_entries" />. This includes entries that
        ///     are
        ///     accessible (recursively) via <seealso cref="Entry.next" />.
        /// </summary>
        private int _entryCount;

        /// <summary>
        ///     The major version number of the class to which this symbol table belongs.
        /// </summary>
        private int _majorVersion;

        /// <summary>
        ///     The actual number of elements in <seealso cref="_typeTable" />. These elements are stored from index 0 to
        ///     typeCount (excluded). The other array entries are empty.
        /// </summary>
        private int _typeCount;

        /// <summary>
        ///     An ASM specific type table used to temporarily store internal names that will not necessarily
        ///     be stored in the constant pool. This type table is used by the control flow and data flow
        ///     analysis algorithm used to compute stack map frames from scratch. This array stores {@link
        ///     Symbol#TYPE_TAG} and <seealso cref="Symbol.Uninitialized_Type_Tag" />) Symbol. The type symbol at index
        ///     {@code i} has its <seealso cref="Symbol.index" /> equal to {@code i} (and vice versa).
        /// </summary>
        private Entry[] _typeTable;

        /// <summary>
        ///     Constructs a new, empty SymbolTable for the given ClassWriter.
        /// </summary>
        /// <param name="classWriter"> a ClassWriter. </param>
        public SymbolTable(ClassWriter classWriter)
        {
            this.classWriter = classWriter;
            _sourceClassReader = null;
            _entries = new Entry[256];
            _constantPoolCount = 1;
            _constantPool = new ByteVector();
        }

        /// <summary>
        ///     Constructs a new SymbolTable for the given ClassWriter, initialized with the constant pool and
        ///     bootstrap methods of the given ClassReader.
        /// </summary>
        /// <param name="classWriter"> a ClassWriter. </param>
        /// <param name="classReader">
        ///     the ClassReader whose constant pool and bootstrap methods must be copied to
        ///     initialize the SymbolTable.
        /// </param>
        public SymbolTable(ClassWriter classWriter, ClassReader classReader)
        {
            this.classWriter = classWriter;
            _sourceClassReader = classReader;

            // Copy the constant pool binary content.
            var inputBytes = classReader.classFileBuffer;
            var constantPoolOffset = classReader.GetItem(1) - 1;
            var constantPoolLength = classReader.header - constantPoolOffset;
            _constantPoolCount = classReader.ItemCount;
            _constantPool = new ByteVector(constantPoolLength);
            _constantPool.PutByteArray(inputBytes, constantPoolOffset, constantPoolLength);

            // Add the constant pool items in the symbol table entries. Reserve enough space in 'entries' to
            // avoid too many hash set collisions (entries is not dynamically resized by the addConstant*
            // method calls below), and to account for bootstrap method entries.
            _entries = new Entry[_constantPoolCount * 2];
            var charBuffer = new char[classReader.MaxStringLength];
            var hasBootstrapMethods = false;
            var itemIndex = 1;
            while (itemIndex < _constantPoolCount)
            {
                var itemOffset = classReader.GetItem(itemIndex);
                int itemTag = inputBytes[itemOffset - 1];
                int nameAndTypeItemOffset;
                switch (itemTag)
                {
                    case Symbol.Constant_Fieldref_Tag:
                    case Symbol.Constant_Methodref_Tag:
                    case Symbol.Constant_Interface_Methodref_Tag:
                        nameAndTypeItemOffset = classReader.GetItem(classReader.ReadUnsignedShort(itemOffset + 2));
                        AddConstantMemberReference(itemIndex, itemTag, classReader.ReadClass(itemOffset, charBuffer),
                            classReader.ReadUtf8(nameAndTypeItemOffset, charBuffer),
                            classReader.ReadUtf8(nameAndTypeItemOffset + 2, charBuffer));
                        break;
                    case Symbol.Constant_Integer_Tag:
                    case Symbol.Constant_Float_Tag:
                        AddConstantIntegerOrFloat(itemIndex, itemTag, classReader.ReadInt(itemOffset));
                        break;
                    case Symbol.Constant_Name_And_Type_Tag:
                        AddConstantNameAndType(itemIndex, classReader.ReadUtf8(itemOffset, charBuffer),
                            classReader.ReadUtf8(itemOffset + 2, charBuffer));
                        break;
                    case Symbol.Constant_Long_Tag:
                    case Symbol.Constant_Double_Tag:
                        AddConstantLongOrDouble(itemIndex, itemTag, classReader.ReadLong(itemOffset));
                        break;
                    case Symbol.Constant_Utf8_Tag:
                        AddConstantUtf8(itemIndex, classReader.ReadUtf(itemIndex, charBuffer));
                        break;
                    case Symbol.Constant_Method_Handle_Tag:
                        var memberRefItemOffset = classReader.GetItem(classReader.ReadUnsignedShort(itemOffset + 1));
                        nameAndTypeItemOffset =
                            classReader.GetItem(classReader.ReadUnsignedShort(memberRefItemOffset + 2));
                        AddConstantMethodHandle(itemIndex, classReader.ReadByte(itemOffset),
                            classReader.ReadClass(memberRefItemOffset, charBuffer),
                            classReader.ReadUtf8(nameAndTypeItemOffset, charBuffer),
                            classReader.ReadUtf8(nameAndTypeItemOffset + 2, charBuffer));
                        break;
                    case Symbol.Constant_Dynamic_Tag:
                    case Symbol.Constant_Invoke_Dynamic_Tag:
                        hasBootstrapMethods = true;
                        nameAndTypeItemOffset = classReader.GetItem(classReader.ReadUnsignedShort(itemOffset + 2));
                        AddConstantDynamicOrInvokeDynamicReference(itemTag, itemIndex,
                            classReader.ReadUtf8(nameAndTypeItemOffset, charBuffer),
                            classReader.ReadUtf8(nameAndTypeItemOffset + 2, charBuffer),
                            classReader.ReadUnsignedShort(itemOffset));
                        break;
                    case Symbol.Constant_String_Tag:
                    case Symbol.Constant_Class_Tag:
                    case Symbol.Constant_Method_Type_Tag:
                    case Symbol.Constant_Module_Tag:
                    case Symbol.Constant_Package_Tag:
                        AddConstantUtf8Reference(itemIndex, itemTag, classReader.ReadUtf8(itemOffset, charBuffer));
                        break;
                    default:
                        throw new ArgumentException();
                }

                itemIndex += itemTag == Symbol.Constant_Long_Tag || itemTag == Symbol.Constant_Double_Tag ? 2 : 1;
            }

            // Copy the BootstrapMethods, if any.
            if (hasBootstrapMethods) CopyBootstrapMethods(classReader, charBuffer);
        }

        /// <summary>
        ///     Returns the ClassReader from which this SymbolTable was constructed.
        /// </summary>
        /// <returns>
        ///     the ClassReader from which this SymbolTable was constructed, or {@literal null} if it
        ///     was constructed from scratch.
        /// </returns>
        public ClassReader Source => _sourceClassReader;

        /// <summary>
        ///     Returns the major version of the class to which this symbol table belongs.
        /// </summary>
        /// <returns> the major version of the class to which this symbol table belongs. </returns>
        public int MajorVersion => _majorVersion;

        /// <summary>
        ///     Returns the internal name of the class to which this symbol table belongs.
        /// </summary>
        /// <returns> the internal name of the class to which this symbol table belongs. </returns>
        public string ClassName => _className;

        /// <summary>
        ///     Returns the number of items in this symbol table's constant_pool array (plus 1).
        /// </summary>
        /// <returns> the number of items in this symbol table's constant_pool array (plus 1). </returns>
        public int ConstantPoolCount => _constantPoolCount;

        /// <summary>
        ///     Returns the length in bytes of this symbol table's constant_pool array.
        /// </summary>
        /// <returns> the length in bytes of this symbol table's constant_pool array. </returns>
        public int ConstantPoolLength => _constantPool.length;

        /// <summary>
        ///     Read the BootstrapMethods 'bootstrap_methods' array binary content and add them as entries of
        ///     the SymbolTable.
        /// </summary>
        /// <param name="classReader">
        ///     the ClassReader whose bootstrap methods must be copied to initialize the
        ///     SymbolTable.
        /// </param>
        /// <param name="charBuffer"> a buffer used to read strings in the constant pool. </param>
        private void CopyBootstrapMethods(ClassReader classReader, char[] charBuffer)
        {
            // Find attributOffset of the 'bootstrap_methods' array.
            var inputBytes = classReader.classFileBuffer;
            var currentAttributeOffset = classReader.FirstAttributeOffset;
            for (var i = classReader.ReadUnsignedShort(currentAttributeOffset - 2); i > 0; --i)
            {
                var attributeName = classReader.ReadUtf8(currentAttributeOffset, charBuffer);
                if (Constants.Bootstrap_Methods.Equals(attributeName))
                {
                    _bootstrapMethodCount = classReader.ReadUnsignedShort(currentAttributeOffset + 6);
                    break;
                }

                currentAttributeOffset += 6 + classReader.ReadInt(currentAttributeOffset + 2);
            }

            if (_bootstrapMethodCount > 0)
            {
                // Compute the offset and the length of the BootstrapMethods 'bootstrap_methods' array.
                var bootstrapMethodsOffset = currentAttributeOffset + 8;
                var bootstrapMethodsLength = classReader.ReadInt(currentAttributeOffset + 2) - 2;
                _bootstrapMethods = new ByteVector(bootstrapMethodsLength);
                _bootstrapMethods.PutByteArray(inputBytes, bootstrapMethodsOffset, bootstrapMethodsLength);

                // Add each bootstrap method in the symbol table entries.
                var currentOffset = bootstrapMethodsOffset;
                for (var i = 0; i < _bootstrapMethodCount; i++)
                {
                    var offset = currentOffset - bootstrapMethodsOffset;
                    var bootstrapMethodRef = classReader.ReadUnsignedShort(currentOffset);
                    currentOffset += 2;
                    var numBootstrapArguments = classReader.ReadUnsignedShort(currentOffset);
                    currentOffset += 2;
                    var hashCode = classReader.ReadConst(bootstrapMethodRef, charBuffer).GetHashCode();
                    while (numBootstrapArguments-- > 0)
                    {
                        var bootstrapArgument = classReader.ReadUnsignedShort(currentOffset);
                        currentOffset += 2;
                        hashCode ^= classReader.ReadConst(bootstrapArgument, charBuffer).GetHashCode();
                    }

                    Add(new Entry(i, Symbol.Bootstrap_Method_Tag, offset, hashCode & 0x7FFFFFFF));
                }
            }
        }

        /// <summary>
        ///     Sets the major version and the name of the class to which this symbol table belongs. Also adds
        ///     the class name to the constant pool.
        /// </summary>
        /// <param name="majorVersion"> a major ClassFile version number. </param>
        /// <param name="className"> an internal class name. </param>
        /// <returns> the constant pool index of a new or already existing Symbol with the given class name. </returns>
        public int SetMajorVersionAndClassName(int majorVersion, string className)
        {
            this._majorVersion = majorVersion;
            this._className = className;
            return AddConstantClass(className).index;
        }

        /// <summary>
        ///     Puts this symbol table's constant_pool array in the given ByteVector, preceded by the
        ///     constant_pool_count value.
        /// </summary>
        /// <param name="output"> where the JVMS ClassFile's constant_pool array must be put. </param>
        public void PutConstantPool(ByteVector output)
        {
            output.PutShort(_constantPoolCount).PutByteArray(_constantPool.data, 0, _constantPool.length);
        }

        /// <summary>
        ///     Returns the size in bytes of this symbol table's BootstrapMethods attribute. Also adds the
        ///     attribute name in the constant pool.
        /// </summary>
        /// <returns> the size in bytes of this symbol table's BootstrapMethods attribute. </returns>
        public int ComputeBootstrapMethodsSize()
        {
            if (_bootstrapMethods != null)
            {
                AddConstantUtf8(Constants.Bootstrap_Methods);
                return 8 + _bootstrapMethods.length;
            }

            return 0;
        }

        /// <summary>
        ///     Puts this symbol table's BootstrapMethods attribute in the given ByteVector. This includes the
        ///     6 attribute header bytes and the num_bootstrap_methods value.
        /// </summary>
        /// <param name="output"> where the JVMS BootstrapMethods attribute must be put. </param>
        public void PutBootstrapMethods(ByteVector output)
        {
            if (_bootstrapMethods != null)
                output.PutShort(AddConstantUtf8(Constants.Bootstrap_Methods)).PutInt(_bootstrapMethods.length + 2)
                    .PutShort(_bootstrapMethodCount).PutByteArray(_bootstrapMethods.data, 0, _bootstrapMethods.length);
        }

        // -----------------------------------------------------------------------------------------------
        // Generic symbol table entries management.
        // -----------------------------------------------------------------------------------------------

        /// <summary>
        ///     Returns the list of entries which can potentially have the given hash code.
        /// </summary>
        /// <param name="hashCode"> a <seealso cref="Entry.hashCode" /> value. </param>
        /// <returns>
        ///     the list of entries which can potentially have the given hash code. The list is stored
        ///     via the <seealso cref="Entry.next" /> field.
        /// </returns>
        private Entry Get(int hashCode)
        {
            return _entries[hashCode % _entries.Length];
        }

        /// <summary>
        ///     Puts the given entry in the <seealso cref="_entries" /> hash set. This method does <i>not</i> check
        ///     whether <seealso cref="_entries" /> already contains a similar entry or not. <seealso cref="_entries" /> is resized
        ///     if necessary to avoid hash collisions (multiple entries needing to be stored at the same {@link
        ///     #entries} array index) as much as possible, with reasonable memory usage.
        /// </summary>
        /// <param name="entry"> an Entry (which must not already be contained in <seealso cref="_entries" />). </param>
        /// <returns> the given entry </returns>
        private Entry Put(Entry entry)
        {
            if (_entryCount > _entries.Length * 3 / 4)
            {
                var currentCapacity = _entries.Length;
                var newCapacity = currentCapacity * 2 + 1;
                var newEntries = new Entry[newCapacity];
                for (var i = currentCapacity - 1; i >= 0; --i)
                {
                    var currentEntry = _entries[i];
                    while (currentEntry != null)
                    {
                        var newCurrentEntryIndex = currentEntry.hashCode % newCapacity;
                        var nextEntry = currentEntry.next;
                        currentEntry.next = newEntries[newCurrentEntryIndex];
                        newEntries[newCurrentEntryIndex] = currentEntry;
                        currentEntry = nextEntry;
                    }
                }

                _entries = newEntries;
            }

            _entryCount++;
            var index = entry.hashCode % _entries.Length;
            entry.next = _entries[index];
            return _entries[index] = entry;
        }

        /// <summary>
        ///     Adds the given entry in the <seealso cref="_entries" /> hash set. This method does <i>not</i> check
        ///     whether <seealso cref="_entries" /> already contains a similar entry or not, and does <i>not</i> resize
        ///     <seealso cref="_entries" /> if necessary.
        /// </summary>
        /// <param name="entry"> an Entry (which must not already be contained in <seealso cref="_entries" />). </param>
        private void Add(Entry entry)
        {
            _entryCount++;
            var index = entry.hashCode % _entries.Length;
            entry.next = _entries[index];
            _entries[index] = entry;
        }

        // -----------------------------------------------------------------------------------------------
        // Constant pool entries management.
        // -----------------------------------------------------------------------------------------------

        /// <summary>
        ///     Adds a number or string constant to the constant pool of this symbol table. Does nothing if the
        ///     constant pool already contains a similar item.
        /// </summary>
        /// <param name="value">
        ///     the value of the constant to be added to the constant pool. This parameter must be
        ///     an <seealso cref="Integer" />, <seealso cref="byte" />, <seealso cref="Character" />, <seealso cref="Short" />,
        ///     <seealso cref="bool" />, {@link
        ///     Float}, <seealso cref="Long" />, <seealso cref="double" />, <seealso cref="string" />, <seealso cref="Type" /> or
        ///     <seealso cref="Handle" />.
        /// </param>
        /// <returns> a new or already existing Symbol with the given value. </returns>
        public Symbol AddConstant(object value)
        {
            if (value is int || value is byte) return AddConstantInteger(((int?)value).Value);

            if (value is sbyte) return AddConstantInteger(((sbyte?)value).Value);

            if (value is char) return AddConstantInteger(((char?)value).Value);

            if (value is short) return AddConstantInteger(((short?)value).Value);

            if (value is bool) return AddConstantInteger(((bool?)value).Value ? 1 : 0);

            if (value is float) return AddConstantFloat(((float?)value).Value);

            if (value is long) return AddConstantLong(((long?)value).Value);

            if (value is double) return AddConstantDouble(((double?)value).Value);

            if (value is string) return AddConstantString((string)value);

            if (value is JType)
            {
                var type = (JType)value;
                var typeSort = type.Sort;
                if (typeSort == JType.Object)
                    return AddConstantClass(type.InternalName);
                if (typeSort == JType.Method)
                    return AddConstantMethodType(type.Descriptor);
                return AddConstantClass(type.Descriptor);
            }

            if (value is Handle)
            {
                var handle = (Handle)value;
                return AddConstantMethodHandle(handle.Tag, handle.Owner, handle.Name, handle.Desc, handle.Interface);
            }

            if (value is ConstantDynamic)
            {
                var constantDynamic = (ConstantDynamic)value;
                return AddConstantDynamic(constantDynamic.Name, constantDynamic.Descriptor,
                    constantDynamic.BootstrapMethod, constantDynamic.BootstrapMethodArgumentsUnsafe);
            }

            throw new ArgumentException("value " + value);
        }

        /// <summary>
        ///     Adds a CONSTANT_Class_info to the constant pool of this symbol table. Does nothing if the
        ///     constant pool already contains a similar item.
        /// </summary>
        /// <param name="value"> the internal name of a class. </param>
        /// <returns> a new or already existing Symbol with the given value. </returns>
        public Symbol AddConstantClass(string value)
        {
            return AddConstantUtf8Reference(Symbol.Constant_Class_Tag, value);
        }

        /// <summary>
        ///     Adds a CONSTANT_Fieldref_info to the constant pool of this symbol table. Does nothing if the
        ///     constant pool already contains a similar item.
        /// </summary>
        /// <param name="owner"> the internal name of a class. </param>
        /// <param name="name"> a field name. </param>
        /// <param name="descriptor"> a field descriptor. </param>
        /// <returns> a new or already existing Symbol with the given value. </returns>
        public Symbol AddConstantFieldref(string owner, string name, string descriptor)
        {
            return AddConstantMemberReference(Symbol.Constant_Fieldref_Tag, owner, name, descriptor);
        }

        /// <summary>
        ///     Adds a CONSTANT_Methodref_info or CONSTANT_InterfaceMethodref_info to the constant pool of this
        ///     symbol table. Does nothing if the constant pool already contains a similar item.
        /// </summary>
        /// <param name="owner"> the internal name of a class. </param>
        /// <param name="name"> a method name. </param>
        /// <param name="descriptor"> a method descriptor. </param>
        /// <param name="isInterface"> whether owner is an interface or not. </param>
        /// <returns> a new or already existing Symbol with the given value. </returns>
        public Symbol AddConstantMethodref(string owner, string name, string descriptor, bool isInterface)
        {
            var tag = isInterface ? Symbol.Constant_Interface_Methodref_Tag : Symbol.Constant_Methodref_Tag;
            return AddConstantMemberReference(tag, owner, name, descriptor);
        }

        /// <summary>
        ///     Adds a CONSTANT_Fieldref_info, CONSTANT_Methodref_info or CONSTANT_InterfaceMethodref_info to
        ///     the constant pool of this symbol table. Does nothing if the constant pool already contains a
        ///     similar item.
        /// </summary>
        /// <param name="tag">
        ///     one of <seealso cref="Symbol.Constant_Fieldref_Tag" />, <seealso cref="Symbol.Constant_Methodref_Tag" />
        ///     or <seealso cref="Symbol.Constant_Interface_Methodref_Tag" />.
        /// </param>
        /// <param name="owner"> the internal name of a class. </param>
        /// <param name="name"> a field or method name. </param>
        /// <param name="descriptor"> a field or method descriptor. </param>
        /// <returns> a new or already existing Symbol with the given value. </returns>
        private Entry AddConstantMemberReference(int tag, string owner, string name, string descriptor)
        {
            var hashCode = Hash(tag, owner, name, descriptor);
            var entry = Get(hashCode);
            while (entry != null)
            {
                if (entry.tag == tag && entry.hashCode == hashCode && entry.owner.Equals(owner) &&
                    entry.name.Equals(name) && entry.value.Equals(descriptor)) return entry;
                entry = entry.next;
            }

            _constantPool.Put122(tag, AddConstantClass(owner).index, AddConstantNameAndType(name, descriptor));
            return Put(new Entry(_constantPoolCount++, tag, owner, name, descriptor, 0, hashCode));
        }

        /// <summary>
        ///     Adds a new CONSTANT_Fieldref_info, CONSTANT_Methodref_info or CONSTANT_InterfaceMethodref_info
        ///     to the constant pool of this symbol table.
        /// </summary>
        /// <param name="index"> the constant pool index of the new Symbol. </param>
        /// <param name="tag">
        ///     one of <seealso cref="Symbol.Constant_Fieldref_Tag" />, <seealso cref="Symbol.Constant_Methodref_Tag" />
        ///     or <seealso cref="Symbol.Constant_Interface_Methodref_Tag" />.
        /// </param>
        /// <param name="owner"> the internal name of a class. </param>
        /// <param name="name"> a field or method name. </param>
        /// <param name="descriptor"> a field or method descriptor. </param>
        private void AddConstantMemberReference(int index, int tag, string owner, string name, string descriptor)
        {
            Add(new Entry(index, tag, owner, name, descriptor, 0, Hash(tag, owner, name, descriptor)));
        }

        /// <summary>
        ///     Adds a CONSTANT_String_info to the constant pool of this symbol table. Does nothing if the
        ///     constant pool already contains a similar item.
        /// </summary>
        /// <param name="value"> a string. </param>
        /// <returns> a new or already existing Symbol with the given value. </returns>
        public Symbol AddConstantString(string value)
        {
            return AddConstantUtf8Reference(Symbol.Constant_String_Tag, value);
        }

        /// <summary>
        ///     Adds a CONSTANT_Integer_info to the constant pool of this symbol table. Does nothing if the
        ///     constant pool already contains a similar item.
        /// </summary>
        /// <param name="value"> an int. </param>
        /// <returns> a new or already existing Symbol with the given value. </returns>
        public Symbol AddConstantInteger(int value)
        {
            return AddConstantIntegerOrFloat(Symbol.Constant_Integer_Tag, value);
        }

        /// <summary>
        ///     Adds a CONSTANT_Float_info to the constant pool of this symbol table. Does nothing if the
        ///     constant pool already contains a similar item.
        /// </summary>
        /// <param name="value"> a float. </param>
        /// <returns> a new or already existing Symbol with the given value. </returns>
        public Symbol AddConstantFloat(float value)
        {
            return AddConstantIntegerOrFloat(Symbol.Constant_Float_Tag, BitConverter.SingleToInt32Bits(value));
        }

        /// <summary>
        ///     Adds a CONSTANT_Integer_info or CONSTANT_Float_info to the constant pool of this symbol table.
        ///     Does nothing if the constant pool already contains a similar item.
        /// </summary>
        /// <param name="tag">
        ///     one of <seealso cref="Symbol.Constant_Integer_Tag" /> or
        ///     <seealso cref="Symbol.Constant_Float_Tag" />.
        /// </param>
        /// <param name="value"> an int or float. </param>
        /// <returns> a constant pool constant with the given tag and primitive values. </returns>
        private Symbol AddConstantIntegerOrFloat(int tag, int value)
        {
            var hashCode = Hash(tag, value);
            var entry = Get(hashCode);
            while (entry != null)
            {
                if (entry.tag == tag && entry.hashCode == hashCode && entry.data == value) return entry;
                entry = entry.next;
            }

            _constantPool.PutByte(tag).PutInt(value);
            return Put(new Entry(_constantPoolCount++, tag, value, hashCode));
        }

        /// <summary>
        ///     Adds a new CONSTANT_Integer_info or CONSTANT_Float_info to the constant pool of this symbol
        ///     table.
        /// </summary>
        /// <param name="index"> the constant pool index of the new Symbol. </param>
        /// <param name="tag">
        ///     one of <seealso cref="Symbol.Constant_Integer_Tag" /> or
        ///     <seealso cref="Symbol.Constant_Float_Tag" />.
        /// </param>
        /// <param name="value"> an int or float. </param>
        private void AddConstantIntegerOrFloat(int index, int tag, int value)
        {
            Add(new Entry(index, tag, value, Hash(tag, value)));
        }

        /// <summary>
        ///     Adds a CONSTANT_Long_info to the constant pool of this symbol table. Does nothing if the
        ///     constant pool already contains a similar item.
        /// </summary>
        /// <param name="value"> a long. </param>
        /// <returns> a new or already existing Symbol with the given value. </returns>
        public Symbol AddConstantLong(long value)
        {
            return AddConstantLongOrDouble(Symbol.Constant_Long_Tag, value);
        }

        /// <summary>
        ///     Adds a CONSTANT_Double_info to the constant pool of this symbol table. Does nothing if the
        ///     constant pool already contains a similar item.
        /// </summary>
        /// <param name="value"> a double. </param>
        /// <returns> a new or already existing Symbol with the given value. </returns>
        public Symbol AddConstantDouble(double value)
        {
            return AddConstantLongOrDouble(Symbol.Constant_Double_Tag, BitConverter.DoubleToInt64Bits(value));
        }

        /// <summary>
        ///     Adds a CONSTANT_Long_info or CONSTANT_Double_info to the constant pool of this symbol table.
        ///     Does nothing if the constant pool already contains a similar item.
        /// </summary>
        /// <param name="tag"> one of <seealso cref="Symbol.Constant_Long_Tag" /> or <seealso cref="Symbol.Constant_Double_Tag" />. </param>
        /// <param name="value"> a long or double. </param>
        /// <returns> a constant pool constant with the given tag and primitive values. </returns>
        private Symbol AddConstantLongOrDouble(int tag, long value)
        {
            var hashCode = Hash(tag, value);
            var entry = Get(hashCode);
            while (entry != null)
            {
                if (entry.tag == tag && entry.hashCode == hashCode && entry.data == value) return entry;
                entry = entry.next;
            }

            var index = _constantPoolCount;
            _constantPool.PutByte(tag).PutLong(value);
            _constantPoolCount += 2;
            return Put(new Entry(index, tag, value, hashCode));
        }

        /// <summary>
        ///     Adds a new CONSTANT_Long_info or CONSTANT_Double_info to the constant pool of this symbol
        ///     table.
        /// </summary>
        /// <param name="index"> the constant pool index of the new Symbol. </param>
        /// <param name="tag"> one of <seealso cref="Symbol.Constant_Long_Tag" /> or <seealso cref="Symbol.Constant_Double_Tag" />. </param>
        /// <param name="value"> a long or double. </param>
        private void AddConstantLongOrDouble(int index, int tag, long value)
        {
            Add(new Entry(index, tag, value, Hash(tag, value)));
        }

        /// <summary>
        ///     Adds a CONSTANT_NameAndType_info to the constant pool of this symbol table. Does nothing if the
        ///     constant pool already contains a similar item.
        /// </summary>
        /// <param name="name"> a field or method name. </param>
        /// <param name="descriptor"> a field or method descriptor. </param>
        /// <returns> a new or already existing Symbol with the given value. </returns>
        public int AddConstantNameAndType(string name, string descriptor)
        {
            const int tag = Symbol.Constant_Name_And_Type_Tag;
            var hashCode = Hash(tag, name, descriptor);
            var entry = Get(hashCode);
            while (entry != null)
            {
                if (entry.tag == tag && entry.hashCode == hashCode && entry.name.Equals(name) &&
                    entry.value.Equals(descriptor)) return entry.index;
                entry = entry.next;
            }

            _constantPool.Put122(tag, AddConstantUtf8(name), AddConstantUtf8(descriptor));
            return Put(new Entry(_constantPoolCount++, tag, name, descriptor, hashCode)).index;
        }

        /// <summary>
        ///     Adds a new CONSTANT_NameAndType_info to the constant pool of this symbol table.
        /// </summary>
        /// <param name="index"> the constant pool index of the new Symbol. </param>
        /// <param name="name"> a field or method name. </param>
        /// <param name="descriptor"> a field or method descriptor. </param>
        private void AddConstantNameAndType(int index, string name, string descriptor)
        {
            const int tag = Symbol.Constant_Name_And_Type_Tag;
            Add(new Entry(index, tag, name, descriptor, Hash(tag, name, descriptor)));
        }

        /// <summary>
        ///     Adds a CONSTANT_Utf8_info to the constant pool of this symbol table. Does nothing if the
        ///     constant pool already contains a similar item.
        /// </summary>
        /// <param name="value"> a string. </param>
        /// <returns> a new or already existing Symbol with the given value. </returns>
        public int AddConstantUtf8(string value)
        {
            var hashCode = Hash(Symbol.Constant_Utf8_Tag, value);
            var entry = Get(hashCode);
            while (entry != null)
            {
                if (entry.tag == Symbol.Constant_Utf8_Tag && entry.hashCode == hashCode && entry.value.Equals(value))
                    return entry.index;
                entry = entry.next;
            }

            _constantPool.PutByte(Symbol.Constant_Utf8_Tag).PutUtf8(value);
            return Put(new Entry(_constantPoolCount++, Symbol.Constant_Utf8_Tag, value, hashCode)).index;
        }

        /// <summary>
        ///     Adds a new CONSTANT_String_info to the constant pool of this symbol table.
        /// </summary>
        /// <param name="index"> the constant pool index of the new Symbol. </param>
        /// <param name="value"> a string. </param>
        private void AddConstantUtf8(int index, string value)
        {
            Add(new Entry(index, Symbol.Constant_Utf8_Tag, value, Hash(Symbol.Constant_Utf8_Tag, value)));
        }

        /// <summary>
        ///     Adds a CONSTANT_MethodHandle_info to the constant pool of this symbol table. Does nothing if
        ///     the constant pool already contains a similar item.
        /// </summary>
        /// <param name="referenceKind">
        ///     one of <seealso cref="IIOpcodes.H_Getfield />, <seealso cref="IIOpcodes.H_Getstatic />, {@link
        ///     Opcodes#H_PUTFIELD}, <seealso cref="IIOpcodes.H_Putstatic />, <seealso cref="IIOpcodes.H_Invokevirtual />, {@link
        ///     Opcodes#H_INVOKESTATIC}, <seealso cref="IIOpcodes.H_Invokespecial />, {@link
        ///     Opcodes#H_NEWINVOKESPECIAL} or <seealso cref="IIOpcodes.H_Invokeinterface />.
        /// </param>
        /// <param name="owner"> the internal name of a class of interface. </param>
        /// <param name="name"> a field or method name. </param>
        /// <param name="descriptor"> a field or method descriptor. </param>
        /// <param name="isInterface"> whether owner is an interface or not. </param>
        /// <returns> a new or already existing Symbol with the given value. </returns>
        public Symbol AddConstantMethodHandle(int referenceKind, string owner, string name, string descriptor,
            bool isInterface)
        {
            const int tag = Symbol.Constant_Method_Handle_Tag;
            // Note that we don't need to include isInterface in the hash computation, because it is
            // redundant with owner (we can't have the same owner with different isInterface values).
            var hashCode = Hash(tag, owner, name, descriptor, referenceKind);
            var entry = Get(hashCode);
            while (entry != null)
            {
                if (entry.tag == tag && entry.hashCode == hashCode && entry.data == referenceKind &&
                    entry.owner.Equals(owner) && entry.name.Equals(name) &&
                    entry.value.Equals(descriptor)) return entry;
                entry = entry.next;
            }

            if (referenceKind <= IOpcodes.H_Putstatic)
                _constantPool.Put112(tag, referenceKind, AddConstantFieldref(owner, name, descriptor).index);
            else
                _constantPool.Put112(tag, referenceKind,
                    AddConstantMethodref(owner, name, descriptor, isInterface).index);
            return Put(new Entry(_constantPoolCount++, tag, owner, name, descriptor, referenceKind, hashCode));
        }

        /// <summary>
        ///     Adds a new CONSTANT_MethodHandle_info to the constant pool of this symbol table.
        /// </summary>
        /// <param name="index"> the constant pool index of the new Symbol. </param>
        /// <param name="referenceKind">
        ///     one of <seealso cref="IIOpcodes.H_Getfield />, <seealso cref="IIOpcodes.H_Getstatic />, {@link
        ///     Opcodes#H_PUTFIELD}, <seealso cref="IIOpcodes.H_Putstatic />, <seealso cref="IIOpcodes.H_Invokevirtual />, {@link
        ///     Opcodes#H_INVOKESTATIC}, <seealso cref="IIOpcodes.H_Invokespecial />, {@link
        ///     Opcodes#H_NEWINVOKESPECIAL} or <seealso cref="IIOpcodes.H_Invokeinterface />.
        /// </param>
        /// <param name="owner"> the internal name of a class of interface. </param>
        /// <param name="name"> a field or method name. </param>
        /// <param name="descriptor"> a field or method descriptor. </param>
        private void AddConstantMethodHandle(int index, int referenceKind, string owner, string name, string descriptor)
        {
            const int tag = Symbol.Constant_Method_Handle_Tag;
            var hashCode = Hash(tag, owner, name, descriptor, referenceKind);
            Add(new Entry(index, tag, owner, name, descriptor, referenceKind, hashCode));
        }

        /// <summary>
        ///     Adds a CONSTANT_MethodType_info to the constant pool of this symbol table. Does nothing if the
        ///     constant pool already contains a similar item.
        /// </summary>
        /// <param name="methodDescriptor"> a method descriptor. </param>
        /// <returns> a new or already existing Symbol with the given value. </returns>
        public Symbol AddConstantMethodType(string methodDescriptor)
        {
            return AddConstantUtf8Reference(Symbol.Constant_Method_Type_Tag, methodDescriptor);
        }

        /// <summary>
        ///     Adds a CONSTANT_Dynamic_info to the constant pool of this symbol table. Also adds the related
        ///     bootstrap method to the BootstrapMethods of this symbol table. Does nothing if the constant
        ///     pool already contains a similar item.
        /// </summary>
        /// <param name="name"> a method name. </param>
        /// <param name="descriptor"> a field descriptor. </param>
        /// <param name="bootstrapMethodHandle"> a bootstrap method handle. </param>
        /// <param name="bootstrapMethodArguments"> the bootstrap method arguments. </param>
        /// <returns> a new or already existing Symbol with the given value. </returns>
        public Symbol AddConstantDynamic(string name, string descriptor, Handle bootstrapMethodHandle,
            params object[] bootstrapMethodArguments)
        {
            var bootstrapMethod = AddBootstrapMethod(bootstrapMethodHandle, bootstrapMethodArguments);
            return AddConstantDynamicOrInvokeDynamicReference(Symbol.Constant_Dynamic_Tag, name, descriptor,
                bootstrapMethod.index);
        }

        /// <summary>
        ///     Adds a CONSTANT_InvokeDynamic_info to the constant pool of this symbol table. Also adds the
        ///     related bootstrap method to the BootstrapMethods of this symbol table. Does nothing if the
        ///     constant pool already contains a similar item.
        /// </summary>
        /// <param name="name"> a method name. </param>
        /// <param name="descriptor"> a method descriptor. </param>
        /// <param name="bootstrapMethodHandle"> a bootstrap method handle. </param>
        /// <param name="bootstrapMethodArguments"> the bootstrap method arguments. </param>
        /// <returns> a new or already existing Symbol with the given value. </returns>
        public Symbol AddConstantInvokeDynamic(string name, string descriptor, Handle bootstrapMethodHandle,
            params object[] bootstrapMethodArguments)
        {
            var bootstrapMethod = AddBootstrapMethod(bootstrapMethodHandle, bootstrapMethodArguments);
            return AddConstantDynamicOrInvokeDynamicReference(Symbol.Constant_Invoke_Dynamic_Tag, name, descriptor,
                bootstrapMethod.index);
        }

        /// <summary>
        ///     Adds a CONSTANT_Dynamic or a CONSTANT_InvokeDynamic_info to the constant pool of this symbol
        ///     table. Does nothing if the constant pool already contains a similar item.
        /// </summary>
        /// <param name="tag">
        ///     one of <seealso cref="Symbol.Constant_Dynamic_Tag" /> or {@link
        ///     Symbol#CONSTANT_INVOKE_DYNAMIC_TAG}.
        /// </param>
        /// <param name="name"> a method name. </param>
        /// <param name="descriptor">
        ///     a field descriptor for CONSTANT_DYNAMIC_TAG) or a method descriptor for
        ///     CONSTANT_INVOKE_DYNAMIC_TAG.
        /// </param>
        /// <param name="bootstrapMethodIndex"> the index of a bootstrap method in the BootstrapMethods attribute. </param>
        /// <returns> a new or already existing Symbol with the given value. </returns>
        private Symbol AddConstantDynamicOrInvokeDynamicReference(int tag, string name, string descriptor,
            int bootstrapMethodIndex)
        {
            var hashCode = Hash(tag, name, descriptor, bootstrapMethodIndex);
            var entry = Get(hashCode);
            while (entry != null)
            {
                if (entry.tag == tag && entry.hashCode == hashCode && entry.data == bootstrapMethodIndex &&
                    entry.name.Equals(name) && entry.value.Equals(descriptor)) return entry;
                entry = entry.next;
            }

            _constantPool.Put122(tag, bootstrapMethodIndex, AddConstantNameAndType(name, descriptor));
            return Put(new Entry(_constantPoolCount++, tag, null, name, descriptor, bootstrapMethodIndex, hashCode));
        }

        /// <summary>
        ///     Adds a new CONSTANT_Dynamic_info or CONSTANT_InvokeDynamic_info to the constant pool of this
        ///     symbol table.
        /// </summary>
        /// <param name="tag">
        ///     one of <seealso cref="Symbol.Constant_Dynamic_Tag" /> or {@link
        ///     Symbol#CONSTANT_INVOKE_DYNAMIC_TAG}.
        /// </param>
        /// <param name="index"> the constant pool index of the new Symbol. </param>
        /// <param name="name"> a method name. </param>
        /// <param name="descriptor">
        ///     a field descriptor for CONSTANT_DYNAMIC_TAG or a method descriptor for
        ///     CONSTANT_INVOKE_DYNAMIC_TAG.
        /// </param>
        /// <param name="bootstrapMethodIndex"> the index of a bootstrap method in the BootstrapMethods attribute. </param>
        private void AddConstantDynamicOrInvokeDynamicReference(int tag, int index, string name, string descriptor,
            int bootstrapMethodIndex)
        {
            var hashCode = Hash(tag, name, descriptor, bootstrapMethodIndex);
            Add(new Entry(index, tag, null, name, descriptor, bootstrapMethodIndex, hashCode));
        }

        /// <summary>
        ///     Adds a CONSTANT_Module_info to the constant pool of this symbol table. Does nothing if the
        ///     constant pool already contains a similar item.
        /// </summary>
        /// <param name="moduleName"> a fully qualified name (using dots) of a module. </param>
        /// <returns> a new or already existing Symbol with the given value. </returns>
        public Symbol AddConstantModule(string moduleName)
        {
            return AddConstantUtf8Reference(Symbol.Constant_Module_Tag, moduleName);
        }

        /// <summary>
        ///     Adds a CONSTANT_Package_info to the constant pool of this symbol table. Does nothing if the
        ///     constant pool already contains a similar item.
        /// </summary>
        /// <param name="packageName"> the internal name of a package. </param>
        /// <returns> a new or already existing Symbol with the given value. </returns>
        public Symbol AddConstantPackage(string packageName)
        {
            return AddConstantUtf8Reference(Symbol.Constant_Package_Tag, packageName);
        }

        /// <summary>
        ///     Adds a CONSTANT_Class_info, CONSTANT_String_info, CONSTANT_MethodType_info,
        ///     CONSTANT_Module_info or CONSTANT_Package_info to the constant pool of this symbol table. Does
        ///     nothing if the constant pool already contains a similar item.
        /// </summary>
        /// <param name="tag">
        ///     one of <seealso cref="Symbol.Constant_Class_Tag" />, <seealso cref="Symbol.Constant_String_Tag" />, {@link
        ///     Symbol#CONSTANT_METHOD_TYPE_TAG}, <seealso cref="Symbol.Constant_Module_Tag" /> or {@link
        ///     Symbol#CONSTANT_PACKAGE_TAG}.
        /// </param>
        /// <param name="value">
        ///     an internal class name, an arbitrary string, a method descriptor, a module or a
        ///     package name, depending on tag.
        /// </param>
        /// <returns> a new or already existing Symbol with the given value. </returns>
        private Symbol AddConstantUtf8Reference(int tag, string value)
        {
            var hashCode = Hash(tag, value);
            var entry = Get(hashCode);
            while (entry != null)
            {
                if (entry.tag == tag && entry.hashCode == hashCode && entry.value.Equals(value)) return entry;
                entry = entry.next;
            }

            _constantPool.Put12(tag, AddConstantUtf8(value));
            return Put(new Entry(_constantPoolCount++, tag, value, hashCode));
        }

        /// <summary>
        ///     Adds a new CONSTANT_Class_info, CONSTANT_String_info, CONSTANT_MethodType_info,
        ///     CONSTANT_Module_info or CONSTANT_Package_info to the constant pool of this symbol table.
        /// </summary>
        /// <param name="index"> the constant pool index of the new Symbol. </param>
        /// <param name="tag">
        ///     one of <seealso cref="Symbol.Constant_Class_Tag" />, <seealso cref="Symbol.Constant_String_Tag" />, {@link
        ///     Symbol#CONSTANT_METHOD_TYPE_TAG}, <seealso cref="Symbol.Constant_Module_Tag" /> or {@link
        ///     Symbol#CONSTANT_PACKAGE_TAG}.
        /// </param>
        /// <param name="value">
        ///     an internal class name, an arbitrary string, a method descriptor, a module or a
        ///     package name, depending on tag.
        /// </param>
        private void AddConstantUtf8Reference(int index, int tag, string value)
        {
            Add(new Entry(index, tag, value, Hash(tag, value)));
        }

        // -----------------------------------------------------------------------------------------------
        // Bootstrap method entries management.
        // -----------------------------------------------------------------------------------------------

        /// <summary>
        ///     Adds a bootstrap method to the BootstrapMethods attribute of this symbol table. Does nothing if
        ///     the BootstrapMethods already contains a similar bootstrap method.
        /// </summary>
        /// <param name="bootstrapMethodHandle"> a bootstrap method handle. </param>
        /// <param name="bootstrapMethodArguments"> the bootstrap method arguments. </param>
        /// <returns> a new or already existing Symbol with the given value. </returns>
        public Symbol AddBootstrapMethod(Handle bootstrapMethodHandle, params object[] bootstrapMethodArguments)
        {
            var bootstrapMethodsAttribute = _bootstrapMethods;
            if (bootstrapMethodsAttribute == null) bootstrapMethodsAttribute = _bootstrapMethods = new ByteVector();

            // The bootstrap method arguments can be Constant_Dynamic values, which reference other
            // bootstrap methods. We must therefore add the bootstrap method arguments to the constant pool
            // and BootstrapMethods attribute first, so that the BootstrapMethods attribute is not modified
            // while adding the given bootstrap method to it, in the rest of this method.
            var numBootstrapArguments = bootstrapMethodArguments.Length;
            var bootstrapMethodArgumentIndexes = new int[numBootstrapArguments];
            for (var i = 0; i < numBootstrapArguments; i++)
                bootstrapMethodArgumentIndexes[i] = AddConstant(bootstrapMethodArguments[i]).index;

            // Write the bootstrap method in the BootstrapMethods table. This is necessary to be able to
            // compare it with existing ones, and will be reverted below if there is already a similar
            // bootstrap method.
            var bootstrapMethodOffset = bootstrapMethodsAttribute.length;
            bootstrapMethodsAttribute.PutShort(AddConstantMethodHandle(bootstrapMethodHandle.Tag,
                bootstrapMethodHandle.Owner, bootstrapMethodHandle.Name, bootstrapMethodHandle.Desc,
                bootstrapMethodHandle.Interface).index);

            bootstrapMethodsAttribute.PutShort(numBootstrapArguments);
            for (var i = 0; i < numBootstrapArguments; i++)
                bootstrapMethodsAttribute.PutShort(bootstrapMethodArgumentIndexes[i]);

            // Compute the length and the hash code of the bootstrap method.
            var bootstrapMethodlength = bootstrapMethodsAttribute.length - bootstrapMethodOffset;
            var hashCode = bootstrapMethodHandle.GetHashCode();
            foreach (var bootstrapMethodArgument in bootstrapMethodArguments)
                hashCode ^= bootstrapMethodArgument.GetHashCode();
            hashCode &= 0x7FFFFFFF;

            // Add the bootstrap method to the symbol table or revert the above changes.
            return AddBootstrapMethod(bootstrapMethodOffset, bootstrapMethodlength, hashCode);
        }

        /// <summary>
        ///     Adds a bootstrap method to the BootstrapMethods attribute of this symbol table. Does nothing if
        ///     the BootstrapMethods already contains a similar bootstrap method (more precisely, reverts the
        ///     content of <seealso cref="_bootstrapMethods" /> to remove the last, duplicate bootstrap method).
        /// </summary>
        /// <param name="offset"> the offset of the last bootstrap method in <seealso cref="_bootstrapMethods" />, in bytes. </param>
        /// <param name="length"> the length of this bootstrap method in <seealso cref="_bootstrapMethods" />, in bytes. </param>
        /// <param name="hashCode"> the hash code of this bootstrap method. </param>
        /// <returns> a new or already existing Symbol with the given value. </returns>
        private Symbol AddBootstrapMethod(int offset, int length, int hashCode)
        {
            var bootstrapMethodsData = _bootstrapMethods.data;
            var entry = Get(hashCode);
            while (entry != null)
            {
                if (entry.tag == Symbol.Bootstrap_Method_Tag && entry.hashCode == hashCode)
                {
                    var otherOffset = (int)entry.data;
                    var isSameBootstrapMethod = true;
                    for (var i = 0; i < length; ++i)
                        if (bootstrapMethodsData[offset + i] != bootstrapMethodsData[otherOffset + i])
                        {
                            isSameBootstrapMethod = false;
                            break;
                        }

                    if (isSameBootstrapMethod)
                    {
                        _bootstrapMethods.length = offset; // Revert to old position.
                        return entry;
                    }
                }

                entry = entry.next;
            }

            return Put(new Entry(_bootstrapMethodCount++, Symbol.Bootstrap_Method_Tag, offset, hashCode));
        }

        // -----------------------------------------------------------------------------------------------
        // Type table entries management.
        // -----------------------------------------------------------------------------------------------

        /// <summary>
        ///     Returns the type table element whose index is given.
        /// </summary>
        /// <param name="typeIndex"> a type table index. </param>
        /// <returns> the type table element whose index is given. </returns>
        public Symbol GetType(int typeIndex)
        {
            return _typeTable[typeIndex];
        }

        /// <summary>
        ///     Adds a type in the type table of this symbol table. Does nothing if the type table already
        ///     contains a similar type.
        /// </summary>
        /// <param name="value"> an internal class name. </param>
        /// <returns> the index of a new or already existing type Symbol with the given value. </returns>
        public int AddType(string value)
        {
            var hashCode = Hash(Symbol.Type_Tag, value);
            var entry = Get(hashCode);
            while (entry != null)
            {
                if (entry.tag == Symbol.Type_Tag && entry.hashCode == hashCode && entry.value.Equals(value))
                    return entry.index;
                entry = entry.next;
            }

            return AddTypeInternal(new Entry(_typeCount, Symbol.Type_Tag, value, hashCode));
        }

        /// <summary>
        ///     Adds an <seealso cref="Frame.Item_Uninitialized" /> type in the type table of this symbol table. Does
        ///     nothing if the type table already contains a similar type.
        /// </summary>
        /// <param name="value"> an internal class name. </param>
        /// <param name="bytecodeOffset">
        ///     the bytecode offset of the NEW instruction that created this {@link
        ///     Frame#ITEM_UNINITIALIZED} type value.
        /// </param>
        /// <returns> the index of a new or already existing type Symbol with the given value. </returns>
        public int AddUninitializedType(string value, int bytecodeOffset)
        {
            var hashCode = Hash(Symbol.Uninitialized_Type_Tag, value, bytecodeOffset);
            var entry = Get(hashCode);
            while (entry != null)
            {
                if (entry.tag == Symbol.Uninitialized_Type_Tag && entry.hashCode == hashCode &&
                    entry.data == bytecodeOffset && entry.value.Equals(value)) return entry.index;
                entry = entry.next;
            }

            return AddTypeInternal(new Entry(_typeCount, Symbol.Uninitialized_Type_Tag, value, bytecodeOffset,
                hashCode));
        }

        /// <summary>
        ///     Adds a merged type in the type table of this symbol table. Does nothing if the type table
        ///     already contains a similar type.
        /// </summary>
        /// <param name="typeTableIndex1">
        ///     a <seealso cref="Symbol.Type_Tag" /> type, specified by its index in the type
        ///     table.
        /// </param>
        /// <param name="typeTableIndex2">
        ///     another <seealso cref="Symbol.Type_Tag" /> type, specified by its index in the type
        ///     table.
        /// </param>
        /// <returns>
        ///     the index of a new or already existing <seealso cref="Symbol.Type_Tag" /> type Symbol,
        ///     corresponding to the common super class of the given types.
        /// </returns>
        public int AddMergedType(int typeTableIndex1, int typeTableIndex2)
        {
            var data = typeTableIndex1 < typeTableIndex2
                ? (uint)typeTableIndex1 | ((long)typeTableIndex2 << 32)
                : typeTableIndex2 | ((long)typeTableIndex1 << 32);
            var hashCode = Hash(Symbol.Merged_Type_Tag, typeTableIndex1 + typeTableIndex2);
            var entry = Get(hashCode);
            while (entry != null)
            {
                if (entry.tag == Symbol.Merged_Type_Tag && entry.hashCode == hashCode && entry.data == data)
                    return entry.info;
                entry = entry.next;
            }

            var type1 = _typeTable[typeTableIndex1].value;
            var type2 = _typeTable[typeTableIndex2].value;
            var commonSuperTypeIndex = AddType(classWriter.GetCommonSuperClass(type1, type2));
            Put(new Entry(_typeCount, Symbol.Merged_Type_Tag, data, hashCode)).info = commonSuperTypeIndex;
            return commonSuperTypeIndex;
        }

        /// <summary>
        ///     Adds the given type Symbol to <seealso cref="_typeTable" />.
        /// </summary>
        /// <param name="entry">
        ///     a <seealso cref="Symbol.Type_Tag" /> or <seealso cref="Symbol.Uninitialized_Type_Tag" /> type symbol.
        ///     The index of this Symbol must be equal to the current value of <seealso cref="_typeCount" />.
        /// </param>
        /// <returns>
        ///     the index in <seealso cref="_typeTable" /> where the given type was added, which is also equal to
        ///     entry's index by hypothesis.
        /// </returns>
        private int AddTypeInternal(Entry entry)
        {
            if (_typeTable == null) _typeTable = new Entry[16];
            if (_typeCount == _typeTable.Length)
            {
                var newTypeTable = new Entry[2 * _typeTable.Length];
                Array.Copy(_typeTable, 0, newTypeTable, 0, _typeTable.Length);
                _typeTable = newTypeTable;
            }

            _typeTable[_typeCount++] = entry;
            return Put(entry).index;
        }

        // -----------------------------------------------------------------------------------------------
        // Static helper methods to compute hash codes.
        // -----------------------------------------------------------------------------------------------

        private static int Hash(int tag, int value)
        {
            return 0x7FFFFFFF & (tag + value);
        }

        private static int Hash(int tag, long value)
        {
            return 0x7FFFFFFF & (tag + (int)value + (int)(long)((ulong)value >> 32));
        }

        private static int Hash(int tag, string value)
        {
            return 0x7FFFFFFF & (tag + value.GetHashCode());
        }

        private static int Hash(int tag, string value1, int value2)
        {
            return 0x7FFFFFFF & (tag + value1.GetHashCode() + value2);
        }

        private static int Hash(int tag, string value1, string value2)
        {
            return 0x7FFFFFFF & (tag + value1.GetHashCode() * value2.GetHashCode());
        }

        private static int Hash(int tag, string value1, string value2, int value3)
        {
            return 0x7FFFFFFF & (tag + value1.GetHashCode() * value2.GetHashCode() * (value3 + 1));
        }

        private static int Hash(int tag, string value1, string value2, string value3)
        {
            return 0x7FFFFFFF & (tag + value1.GetHashCode() * value2.GetHashCode() * value3.GetHashCode());
        }

        private static int Hash(int tag, string value1, string value2, string value3, int value4)
        {
            return 0x7FFFFFFF & (tag + value1.GetHashCode() * value2.GetHashCode() * value3.GetHashCode() * value4);
        }

        /// <summary>
        ///     An entry of a SymbolTable. This concrete and private subclass of <seealso cref="Symbol" /> adds two fields
        ///     which are only used inside SymbolTable, to implement hash sets of symbols (in order to avoid
        ///     duplicate symbols). See <seealso cref="SymbolTable._entries" />.
        ///     @author Eric Bruneton
        /// </summary>
        private class Entry : Symbol
        {
            /// <summary>
            ///     The hash code of this entry.
            /// </summary>
            internal readonly int hashCode;

            /// <summary>
            ///     Another entry (and so on recursively) having the same hash code (modulo the size of {@link
            ///     #entries}) as this one.
            /// </summary>
            internal Entry next;

            public Entry(int index, int tag, string owner, string name, string value, long data, int hashCode) : base(
                index, tag, owner, name, value, data)
            {
                this.hashCode = hashCode;
            }

            public Entry(int index, int tag, string value, int hashCode) : base(index, tag, null, null, value, 0)
            {
                this.hashCode = hashCode;
            }

            public Entry(int index, int tag, string value, long data, int hashCode) : base(index, tag, null, null,
                value, data)
            {
                this.hashCode = hashCode;
            }

            public Entry(int index, int tag, string name, string value, int hashCode) : base(index, tag, null, name,
                value, 0)
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