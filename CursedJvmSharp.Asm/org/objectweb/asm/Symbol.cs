using org.objectweb.asm;
using CursedJvmSharp.Asm.Java.IO;
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
    /// An entry of the constant pool, of the BootstrapMethods attribute, or of the (ASM specific) type
    /// table of a class.
    /// </summary>
    /// <seealso cref= <a href="https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.4">JVMS
    ///     4.4</a> </seealso>
    /// <seealso cref= <a href="https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.7.23">JVMS
    ///     4.7.23</a>
    /// @author Eric Bruneton </seealso>
    public abstract class Symbol
	{

	  // Tag values for the constant pool entries (using the same order as in the JVMS).

	  /// <summary>
	  /// The tag value of CONSTANT_Class_info JVMS structures. </summary>
	  internal const int CONSTANT_CLASS_TAG = 7;

	  /// <summary>
	  /// The tag value of CONSTANT_Fieldref_info JVMS structures. </summary>
	  internal const int CONSTANT_FIELDREF_TAG = 9;

	  /// <summary>
	  /// The tag value of CONSTANT_Methodref_info JVMS structures. </summary>
	  internal const int CONSTANT_METHODREF_TAG = 10;

	  /// <summary>
	  /// The tag value of CONSTANT_InterfaceMethodref_info JVMS structures. </summary>
	  internal const int CONSTANT_INTERFACE_METHODREF_TAG = 11;

	  /// <summary>
	  /// The tag value of CONSTANT_String_info JVMS structures. </summary>
	  internal const int CONSTANT_STRING_TAG = 8;

	  /// <summary>
	  /// The tag value of CONSTANT_Integer_info JVMS structures. </summary>
	  internal const int CONSTANT_INTEGER_TAG = 3;

	  /// <summary>
	  /// The tag value of CONSTANT_Float_info JVMS structures. </summary>
	  internal const int CONSTANT_FLOAT_TAG = 4;

	  /// <summary>
	  /// The tag value of CONSTANT_Long_info JVMS structures. </summary>
	  internal const int CONSTANT_LONG_TAG = 5;

	  /// <summary>
	  /// The tag value of CONSTANT_Double_info JVMS structures. </summary>
	  internal const int CONSTANT_DOUBLE_TAG = 6;

	  /// <summary>
	  /// The tag value of CONSTANT_NameAndType_info JVMS structures. </summary>
	  internal const int CONSTANT_NAME_AND_TYPE_TAG = 12;

	  /// <summary>
	  /// The tag value of CONSTANT_Utf8_info JVMS structures. </summary>
	  internal const int CONSTANT_UTF8_TAG = 1;

	  /// <summary>
	  /// The tag value of CONSTANT_MethodHandle_info JVMS structures. </summary>
	  internal const int CONSTANT_METHOD_HANDLE_TAG = 15;

	  /// <summary>
	  /// The tag value of CONSTANT_MethodType_info JVMS structures. </summary>
	  internal const int CONSTANT_METHOD_TYPE_TAG = 16;

	  /// <summary>
	  /// The tag value of CONSTANT_Dynamic_info JVMS structures. </summary>
	  internal const int CONSTANT_DYNAMIC_TAG = 17;

	  /// <summary>
	  /// The tag value of CONSTANT_InvokeDynamic_info JVMS structures. </summary>
	  internal const int CONSTANT_INVOKE_DYNAMIC_TAG = 18;

	  /// <summary>
	  /// The tag value of CONSTANT_Module_info JVMS structures. </summary>
	  internal const int CONSTANT_MODULE_TAG = 19;

	  /// <summary>
	  /// The tag value of CONSTANT_Package_info JVMS structures. </summary>
	  internal const int CONSTANT_PACKAGE_TAG = 20;

	  // Tag values for the BootstrapMethods attribute entries (ASM specific tag).

	  /// <summary>
	  /// The tag value of the BootstrapMethods attribute entries. </summary>
	  internal const int BOOTSTRAP_METHOD_TAG = 64;

	  // Tag values for the type table entries (ASM specific tags).

	  /// <summary>
	  /// The tag value of a normal type entry in the (ASM specific) type table of a class. </summary>
	  internal const int TYPE_TAG = 128;

	  /// <summary>
	  /// The tag value of an <seealso cref="Frame.ITEM_UNINITIALIZED"/> type entry in the type table of a class.
	  /// </summary>
	  internal const int UNINITIALIZED_TYPE_TAG = 129;

	  /// <summary>
	  /// The tag value of a merged type entry in the (ASM specific) type table of a class. </summary>
	  internal const int MERGED_TYPE_TAG = 130;

	  // Instance fields.

	  /// <summary>
	  /// The index of this symbol in the constant pool, in the BootstrapMethods attribute, or in the
	  /// (ASM specific) type table of a class (depending on the <seealso cref="tag"/> value).
	  /// </summary>
	  internal readonly int index;

	  /// <summary>
	  /// A tag indicating the type of this symbol. Must be one of the static tag values defined in this
	  /// class.
	  /// </summary>
	  internal readonly int tag;

	  /// <summary>
	  /// The internal name of the owner class of this symbol. Only used for {@link
	  /// #CONSTANT_FIELDREF_TAG}, <seealso cref="CONSTANT_METHODREF_TAG"/>, {@link
	  /// #CONSTANT_INTERFACE_METHODREF_TAG}, and <seealso cref="CONSTANT_METHOD_HANDLE_TAG"/> symbols.
	  /// </summary>
	  internal readonly string owner;

	  /// <summary>
	  /// The name of the class field or method corresponding to this symbol. Only used for {@link
	  /// #CONSTANT_FIELDREF_TAG}, <seealso cref="CONSTANT_METHODREF_TAG"/>, {@link
	  /// #CONSTANT_INTERFACE_METHODREF_TAG}, <seealso cref="CONSTANT_NAME_AND_TYPE_TAG"/>, {@link
	  /// #CONSTANT_METHOD_HANDLE_TAG}, <seealso cref="CONSTANT_DYNAMIC_TAG"/> and {@link
	  /// #CONSTANT_INVOKE_DYNAMIC_TAG} symbols.
	  /// </summary>
	  internal readonly string name;

	  /// <summary>
	  /// The string value of this symbol. This is:
	  /// 
	  /// <ul>
	  ///   <li>a field or method descriptor for <seealso cref="CONSTANT_FIELDREF_TAG"/>, {@link
	  ///       #CONSTANT_METHODREF_TAG}, <seealso cref="CONSTANT_INTERFACE_METHODREF_TAG"/>, {@link
	  ///       #CONSTANT_NAME_AND_TYPE_TAG}, <seealso cref="CONSTANT_METHOD_HANDLE_TAG"/>, {@link
	  ///       #CONSTANT_METHOD_TYPE_TAG}, <seealso cref="CONSTANT_DYNAMIC_TAG"/> and {@link
	  ///       #CONSTANT_INVOKE_DYNAMIC_TAG} symbols,
	  ///   <li>an arbitrary string for <seealso cref="CONSTANT_UTF8_TAG"/> and <seealso cref="CONSTANT_STRING_TAG"/>
	  ///       symbols,
	  ///   <li>an internal class name for <seealso cref="CONSTANT_CLASS_TAG"/>, <seealso cref="TYPE_TAG"/> and {@link
	  ///       #UNINITIALIZED_TYPE_TAG} symbols,
	  ///   <li>{@literal null} for the other types of symbol.
	  /// </ul>
	  /// </summary>
	  internal readonly string value;

	  /// <summary>
	  /// The numeric value of this symbol. This is:
	  /// 
	  /// <ul>
	  ///   <li>the symbol's value for <seealso cref="CONSTANT_INTEGER_TAG"/>,<seealso cref="CONSTANT_FLOAT_TAG"/>, {@link
	  ///       #CONSTANT_LONG_TAG}, <seealso cref="CONSTANT_DOUBLE_TAG"/>,
	  ///   <li>the CONSTANT_MethodHandle_info reference_kind field value for {@link
	  ///       #CONSTANT_METHOD_HANDLE_TAG} symbols,
	  ///   <li>the CONSTANT_InvokeDynamic_info bootstrap_method_attr_index field value for {@link
	  ///       #CONSTANT_INVOKE_DYNAMIC_TAG} symbols,
	  ///   <li>the offset of a bootstrap method in the BootstrapMethods boostrap_methods array, for
	  ///       <seealso cref="CONSTANT_DYNAMIC_TAG"/> or <seealso cref="BOOTSTRAP_METHOD_TAG"/> symbols,
	  ///   <li>the bytecode offset of the NEW instruction that created an {@link
	  ///       Frame#ITEM_UNINITIALIZED} type for <seealso cref="UNINITIALIZED_TYPE_TAG"/> symbols,
	  ///   <li>the indices (in the class' type table) of two <seealso cref="TYPE_TAG"/> source types for {@link
	  ///       #MERGED_TYPE_TAG} symbols,
	  ///   <li>0 for the other types of symbol.
	  /// </ul>
	  /// </summary>
	  internal readonly long data;

	  /// <summary>
	  /// Additional information about this symbol, generally computed lazily. <i>Warning: the value of
	  /// this field is ignored when comparing Symbol instances</i> (to avoid duplicate entries in a
	  /// SymbolTable). Therefore, this field should only contain data that can be computed from the
	  /// other fields of this class. It contains:
	  /// 
	  /// <ul>
	  ///   <li>the <seealso cref="Type.getArgumentsAndReturnSizes"/> of the symbol's method descriptor for {@link
	  ///       #CONSTANT_METHODREF_TAG}, <seealso cref="CONSTANT_INTERFACE_METHODREF_TAG"/> and {@link
	  ///       #CONSTANT_INVOKE_DYNAMIC_TAG} symbols,
	  ///   <li>the index in the InnerClasses_attribute 'classes' array (plus one) corresponding to this
	  ///       class, for <seealso cref="CONSTANT_CLASS_TAG"/> symbols,
	  ///   <li>the index (in the class' type table) of the merged type of the two source types for
	  ///       <seealso cref="MERGED_TYPE_TAG"/> symbols,
	  ///   <li>0 for the other types of symbol, or if this field has not been computed yet.
	  /// </ul>
	  /// </summary>
	  internal int info;

	  /// <summary>
	  /// Constructs a new Symbol. This constructor can't be used directly because the Symbol class is
	  /// abstract. Instead, use the factory methods of the <seealso cref="SymbolTable"/> class.
	  /// </summary>
	  /// <param name="index"> the symbol index in the constant pool, in the BootstrapMethods attribute, or in
	  ///     the (ASM specific) type table of a class (depending on 'tag'). </param>
	  /// <param name="tag"> the symbol type. Must be one of the static tag values defined in this class. </param>
	  /// <param name="owner"> The internal name of the symbol's owner class. Maybe {@literal null}. </param>
	  /// <param name="name"> The name of the symbol's corresponding class field or method. Maybe {@literal
	  ///     null}. </param>
	  /// <param name="value"> The string value of this symbol. Maybe {@literal null}. </param>
	  /// <param name="data"> The numeric value of this symbol. </param>
	  public Symbol(int index, int tag, string owner, string name, string value, long data)
	  {
		this.index = index;
		this.tag = tag;
		this.owner = owner;
		this.name = name;
		this.value = value;
		this.data = data;
	  }

	  /// <summary>
	  /// Returns the result <seealso cref="Type.getArgumentsAndReturnSizes"/> on <seealso cref="value"/>.
	  /// </summary>
	  /// <returns> the result <seealso cref="Type.getArgumentsAndReturnSizes"/> on <seealso cref="value"/> (memoized in
	  ///     <seealso cref="info"/> for efficiency). This should only be used for {@link
	  ///     #CONSTANT_METHODREF_TAG}, <seealso cref="CONSTANT_INTERFACE_METHODREF_TAG"/> and {@link
	  ///     #CONSTANT_INVOKE_DYNAMIC_TAG} symbols. </returns>
	  public virtual int ArgumentsAndReturnSizes
	  {
		  get
		  {
			if (info == 0)
			{
			  info = org.objectweb.asm.JType.getArgumentsAndReturnSizes(value);
			}
			return info;
		  }
	  }
	}

}