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
    /// A non standard class, field, method or Code attribute, as defined in the Java Virtual Machine
    /// Specification (JVMS).
    /// </summary>
    /// <seealso cref= <a href= "https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.7">JVMS
    ///     4.7</a> </seealso>
    /// <seealso cref= <a href= "https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.7.3">JVMS
    ///     4.7.3</a>
    /// @author Eric Bruneton
    /// @author Eugene Kuleshov </seealso>
    public class Attribute
    {
        /// <summary>
        /// The type of this attribute, also called its name in the JVMS. </summary>
        public readonly string type;

        /// <summary>
        /// The raw content of this attribute, only used for unknown attributes (see <seealso cref="isUnknown()"/>).
        /// The 6 header bytes of the attribute (attribute_name_index and attribute_length) are <i>not</i>
        /// included.
        /// </summary>
        private byte[] _content;

        /// <summary>
        /// The next attribute in this attribute list (Attribute instances can be linked via this field to
        /// store a list of class, field, method or Code attributes). May be {@literal null}.
        /// </summary>
        internal Attribute nextAttribute;

        /// <summary>
        /// Constructs a new empty attribute.
        /// </summary>
        /// <param name="type"> the type of the attribute. </param>
        public Attribute(string type)
        {
            this.type = type;
        }

        /// <summary>
        /// Returns {@literal true} if this type of attribute is unknown. This means that the attribute
        /// content can't be parsed to extract constant pool references, labels, etc. Instead, the
        /// attribute content is read as an opaque byte array, and written back as is. This can lead to
        /// invalid attributes, if the content actually contains constant pool references, labels, or other
        /// symbolic references that need to be updated when there are changes to the constant pool, the
        /// method bytecode, etc. The default implementation of this method always returns {@literal true}.
        /// </summary>
        /// <returns> {@literal true} if this type of attribute is unknown. </returns>
        public virtual bool Unknown => true;

        /// <summary>
        /// Returns {@literal true} if this type of attribute is a Code attribute.
        /// </summary>
        /// <returns> {@literal true} if this type of attribute is a Code attribute. </returns>
        public virtual bool CodeAttribute => false;

        /// <summary>
        /// Returns the labels corresponding to this attribute.
        /// </summary>
        /// <returns> the labels corresponding to this attribute, or {@literal null} if this attribute is not
        ///     a Code attribute that contains labels. </returns>
        public virtual Label[] Labels => new Label[0];

        /// <summary>
        /// Reads a <seealso cref="type"/> attribute. This method must return a <i>new</i> <seealso cref="Attribute"/> object,
        /// of type <seealso cref="type"/>, corresponding to the 'length' bytes starting at 'offset', in the given
        /// ClassReader.
        /// </summary>
        /// <param name="classReader"> the class that contains the attribute to be read. </param>
        /// <param name="offset"> index of the first byte of the attribute's content in <seealso cref="ClassReader"/>. The 6
        ///     attribute header bytes (attribute_name_index and attribute_length) are not taken into
        ///     account here. </param>
        /// <param name="length"> the length of the attribute's content (excluding the 6 attribute header bytes). </param>
        /// <param name="charBuffer"> the buffer to be used to call the ClassReader methods requiring a
        ///     'charBuffer' parameter. </param>
        /// <param name="codeAttributeOffset"> index of the first byte of content of the enclosing Code attribute
        ///     in <seealso cref="ClassReader"/>, or -1 if the attribute to be read is not a Code attribute. The 6
        ///     attribute header bytes (attribute_name_index and attribute_length) are not taken into
        ///     account here. </param>
        /// <param name="labels"> the labels of the method's code, or {@literal null} if the attribute to be read
        ///     is not a Code attribute. </param>
        /// <returns> a <i>new</i> <seealso cref="Attribute"/> object corresponding to the specified bytes. </returns>
        public virtual Attribute Read(ClassReader classReader, int offset, int length, char[] charBuffer,
            int codeAttributeOffset, Label[] labels)
        {
            var attribute = new Attribute(type);
            attribute._content = new byte[length];
            Array.Copy(classReader.classFileBuffer, offset, attribute._content, 0, length);
            return attribute;
        }

        /// <summary>
        /// Returns the byte array form of the content of this attribute. The 6 header bytes
        /// (attribute_name_index and attribute_length) must <i>not</i> be added in the returned
        /// ByteVector.
        /// </summary>
        /// <param name="classWriter"> the class to which this attribute must be added. This parameter can be used
        ///     to add the items that corresponds to this attribute to the constant pool of this class. </param>
        /// <param name="code"> the bytecode of the method corresponding to this Code attribute, or {@literal null}
        ///     if this attribute is not a Code attribute. Corresponds to the 'code' field of the Code
        ///     attribute. </param>
        /// <param name="codeLength"> the length of the bytecode of the method corresponding to this code
        ///     attribute, or 0 if this attribute is not a Code attribute. Corresponds to the 'code_length'
        ///     field of the Code attribute. </param>
        /// <param name="maxStack"> the maximum stack size of the method corresponding to this Code attribute, or
        ///     -1 if this attribute is not a Code attribute. </param>
        /// <param name="maxLocals"> the maximum number of local variables of the method corresponding to this code
        ///     attribute, or -1 if this attribute is not a Code attribute. </param>
        /// <returns> the byte array form of this attribute. </returns>
        public virtual ByteVector Write(ClassWriter classWriter, byte[] code, int codeLength, int maxStack,
            int maxLocals)
        {
            return new ByteVector(_content);
        }

        /// <summary>
        /// Returns the number of attributes of the attribute list that begins with this attribute.
        /// </summary>
        /// <returns> the number of attributes of the attribute list that begins with this attribute. </returns>
        public int AttributeCount
        {
            get
            {
                var count = 0;
                var attribute = this;
                while (attribute != null)
                {
                    count += 1;
                    attribute = attribute.nextAttribute;
                }

                return count;
            }
        }

        /// <summary>
        /// Returns the total size in bytes of all the attributes in the attribute list that begins with
        /// this attribute. This size includes the 6 header bytes (attribute_name_index and
        /// attribute_length) per attribute. Also adds the attribute type names to the constant pool.
        /// </summary>
        /// <param name="symbolTable"> where the constants used in the attributes must be stored. </param>
        /// <returns> the size of all the attributes in this attribute list. This size includes the size of
        ///     the attribute headers. </returns>
        public int ComputeAttributesSize(SymbolTable symbolTable)
        {
            const byte[] code = null;
            const int codeLength = 0;
            const int maxStack = -1;
            const int maxLocals = -1;
            return ComputeAttributesSize(symbolTable, code, codeLength, maxStack, maxLocals);
        }

        /// <summary>
        /// Returns the total size in bytes of all the attributes in the attribute list that begins with
        /// this attribute. This size includes the 6 header bytes (attribute_name_index and
        /// attribute_length) per attribute. Also adds the attribute type names to the constant pool.
        /// </summary>
        /// <param name="symbolTable"> where the constants used in the attributes must be stored. </param>
        /// <param name="code"> the bytecode of the method corresponding to these Code attributes, or {@literal
        ///     null} if they are not Code attributes. Corresponds to the 'code' field of the Code
        ///     attribute. </param>
        /// <param name="codeLength"> the length of the bytecode of the method corresponding to these code
        ///     attributes, or 0 if they are not Code attributes. Corresponds to the 'code_length' field of
        ///     the Code attribute. </param>
        /// <param name="maxStack"> the maximum stack size of the method corresponding to these Code attributes, or
        ///     -1 if they are not Code attributes. </param>
        /// <param name="maxLocals"> the maximum number of local variables of the method corresponding to these
        ///     Code attributes, or -1 if they are not Code attribute. </param>
        /// <returns> the size of all the attributes in this attribute list. This size includes the size of
        ///     the attribute headers. </returns>
        public int ComputeAttributesSize(SymbolTable symbolTable, byte[] code, int codeLength, int maxStack,
            int maxLocals)
        {
            var classWriter = symbolTable.classWriter;
            var size = 0;
            var attribute = this;
            while (attribute != null)
            {
                symbolTable.AddConstantUtf8(attribute.type);
                size += 6 + attribute.Write(classWriter, code, codeLength, maxStack, maxLocals).length;
                attribute = attribute.nextAttribute;
            }

            return size;
        }

        /// <summary>
        /// Returns the total size in bytes of all the attributes that correspond to the given field,
        /// method or class access flags and signature. This size includes the 6 header bytes
        /// (attribute_name_index and attribute_length) per attribute. Also adds the attribute type names
        /// to the constant pool.
        /// </summary>
        /// <param name="symbolTable"> where the constants used in the attributes must be stored. </param>
        /// <param name="accessFlags"> some field, method or class access flags. </param>
        /// <param name="signatureIndex"> the constant pool index of a field, method of class signature. </param>
        /// <returns> the size of all the attributes in bytes. This size includes the size of the attribute
        ///     headers. </returns>
        internal static int ComputeAttributesSize(SymbolTable symbolTable, int accessFlags, int signatureIndex)
        {
            var size = 0;
            // Before Java 1.5, synthetic fields are represented with a Synthetic attribute.
            if ((accessFlags & IOpcodes.Acc_Synthetic) != 0 && symbolTable.MajorVersion < IOpcodes.V1_5)
            {
                // Synthetic attributes always use 6 bytes.
                symbolTable.AddConstantUtf8(Constants.Synthetic);
                size += 6;
            }

            if (signatureIndex != 0)
            {
                // Signature attributes always use 8 bytes.
                symbolTable.AddConstantUtf8(Constants.Signature);
                size += 8;
            }

            // ACC_DEPRECATED is ASM specific, the ClassFile format uses a Deprecated attribute instead.
            if ((accessFlags & IOpcodes.Acc_Deprecated) != 0)
            {
                // Deprecated attributes always use 6 bytes.
                symbolTable.AddConstantUtf8(Constants.Deprecated);
                size += 6;
            }

            return size;
        }

        /// <summary>
        /// Puts all the attributes of the attribute list that begins with this attribute, in the given
        /// byte vector. This includes the 6 header bytes (attribute_name_index and attribute_length) per
        /// attribute.
        /// </summary>
        /// <param name="symbolTable"> where the constants used in the attributes must be stored. </param>
        /// <param name="output"> where the attributes must be written. </param>
        public void PutAttributes(SymbolTable symbolTable, ByteVector output)
        {
            const byte[] code = null;
            const int codeLength = 0;
            const int maxStack = -1;
            const int maxLocals = -1;
            PutAttributes(symbolTable, code, codeLength, maxStack, maxLocals, output);
        }

        /// <summary>
        /// Puts all the attributes of the attribute list that begins with this attribute, in the given
        /// byte vector. This includes the 6 header bytes (attribute_name_index and attribute_length) per
        /// attribute.
        /// </summary>
        /// <param name="symbolTable"> where the constants used in the attributes must be stored. </param>
        /// <param name="code"> the bytecode of the method corresponding to these Code attributes, or {@literal
        ///     null} if they are not Code attributes. Corresponds to the 'code' field of the Code
        ///     attribute. </param>
        /// <param name="codeLength"> the length of the bytecode of the method corresponding to these code
        ///     attributes, or 0 if they are not Code attributes. Corresponds to the 'code_length' field of
        ///     the Code attribute. </param>
        /// <param name="maxStack"> the maximum stack size of the method corresponding to these Code attributes, or
        ///     -1 if they are not Code attributes. </param>
        /// <param name="maxLocals"> the maximum number of local variables of the method corresponding to these
        ///     Code attributes, or -1 if they are not Code attribute. </param>
        /// <param name="output"> where the attributes must be written. </param>
        public void PutAttributes(SymbolTable symbolTable, byte[] code, int codeLength, int maxStack, int maxLocals,
            ByteVector output)
        {
            var classWriter = symbolTable.classWriter;
            var attribute = this;
            while (attribute != null)
            {
                var attributeContent = attribute.Write(classWriter, code, codeLength, maxStack, maxLocals);
                // Put attribute_name_index and attribute_length.
                output.PutShort(symbolTable.AddConstantUtf8(attribute.type)).PutInt(attributeContent.length);
                output.PutByteArray(attributeContent.data, 0, attributeContent.length);
                attribute = attribute.nextAttribute;
            }
        }

        /// <summary>
        /// Puts all the attributes that correspond to the given field, method or class access flags and
        /// signature, in the given byte vector. This includes the 6 header bytes (attribute_name_index and
        /// attribute_length) per attribute.
        /// </summary>
        /// <param name="symbolTable"> where the constants used in the attributes must be stored. </param>
        /// <param name="accessFlags"> some field, method or class access flags. </param>
        /// <param name="signatureIndex"> the constant pool index of a field, method of class signature. </param>
        /// <param name="output"> where the attributes must be written. </param>
        internal static void PutAttributes(SymbolTable symbolTable, int accessFlags, int signatureIndex,
            ByteVector output)
        {
            // Before Java 1.5, synthetic fields are represented with a Synthetic attribute.
            if ((accessFlags & IOpcodes.Acc_Synthetic) != 0 && symbolTable.MajorVersion < IOpcodes.V1_5)
            {
                output.PutShort(symbolTable.AddConstantUtf8(Constants.Synthetic)).PutInt(0);
            }

            if (signatureIndex != 0)
            {
                output.PutShort(symbolTable.AddConstantUtf8(Constants.Signature)).PutInt(2).PutShort(signatureIndex);
            }

            if ((accessFlags & IOpcodes.Acc_Deprecated) != 0)
            {
                output.PutShort(symbolTable.AddConstantUtf8(Constants.Deprecated)).PutInt(0);
            }
        }

        /// <summary>
        /// A set of attribute prototypes (attributes with the same type are considered equal). </summary>
        internal sealed class Set
        {
            internal const int Size_Increment = 6;

            internal int size;
            internal Attribute[] data = new Attribute[Size_Increment];

            public void AddAttributes(Attribute attributeList)
            {
                var attribute = attributeList;
                while (attribute != null)
                {
                    if (!Contains(attribute))
                    {
                        Add(attribute);
                    }

                    attribute = attribute.nextAttribute;
                }
            }

            public Attribute[] ToArray()
            {
                var result = new Attribute[size];
                Array.Copy(data, 0, result, 0, size);
                return result;
            }

            public bool Contains(Attribute attribute)
            {
                for (var i = 0; i < size; ++i)
                {
                    if (data[i].type.Equals(attribute.type))
                    {
                        return true;
                    }
                }

                return false;
            }

            public void Add(Attribute attribute)
            {
                if (size >= data.Length)
                {
                    var newData = new Attribute[data.Length + Size_Increment];
                    Array.Copy(data, 0, newData, 0, size);
                    data = newData;
                }

                data[size++] = attribute;
            }
        }
    }
}