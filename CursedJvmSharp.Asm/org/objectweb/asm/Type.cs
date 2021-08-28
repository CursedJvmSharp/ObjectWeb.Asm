using org.objectweb.asm;
using CursedJvmSharp.Asm.Java.IO;
using System;
using System.Linq;
using System.Text;

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
	/// A Java field or method type. This class can be used to make it easier to manipulate type and
	/// method descriptors.
	/// 
	/// @author Eric Bruneton
	/// @author Chris Nokleberg
	/// </summary>
	public sealed class JType
	{

	  /// <summary>
	  /// The sort of the {@code void} type. See <seealso cref="getSort"/>. </summary>
	  public const int VOID = 0;

	  /// <summary>
	  /// The sort of the {@code boolean} type. See <seealso cref="getSort"/>. </summary>
	  public const int BOOLEAN = 1;

	  /// <summary>
	  /// The sort of the {@code char} type. See <seealso cref="getSort"/>. </summary>
	  public const int CHAR = 2;

	  /// <summary>
	  /// The sort of the {@code byte} type. See <seealso cref="getSort"/>. </summary>
	  public const int BYTE = 3;

	  /// <summary>
	  /// The sort of the {@code short} type. See <seealso cref="getSort"/>. </summary>
	  public const int SHORT = 4;

	  /// <summary>
	  /// The sort of the {@code int} type. See <seealso cref="getSort"/>. </summary>
	  public const int INT = 5;

	  /// <summary>
	  /// The sort of the {@code float} type. See <seealso cref="getSort"/>. </summary>
	  public const int FLOAT = 6;

	  /// <summary>
	  /// The sort of the {@code long} type. See <seealso cref="getSort"/>. </summary>
	  public const int LONG = 7;

	  /// <summary>
	  /// The sort of the {@code double} type. See <seealso cref="getSort"/>. </summary>
	  public const int DOUBLE = 8;

	  /// <summary>
	  /// The sort of array reference types. See <seealso cref="getSort"/>. </summary>
	  public const int ARRAY = 9;

	  /// <summary>
	  /// The sort of object reference types. See <seealso cref="getSort"/>. </summary>
	  public const int OBJECT = 10;

	  /// <summary>
	  /// The sort of method types. See <seealso cref="getSort"/>. </summary>
	  public const int METHOD = 11;

	  /// <summary>
	  /// The (private) sort of object reference types represented with an internal name. </summary>
	  private const int INTERNAL = 12;

	  /// <summary>
	  /// The descriptors of the primitive types. </summary>
	  private const string PRIMITIVE_DESCRIPTORS = "VZCBSIFJD";

	  /// <summary>
	  /// The {@code void} type. </summary>
	  public static readonly org.objectweb.asm.JType VOID_TYPE = new org.objectweb.asm.JType(VOID, PRIMITIVE_DESCRIPTORS, VOID, VOID + 1);

	  /// <summary>
	  /// The {@code boolean} type. </summary>
	  public static readonly org.objectweb.asm.JType BOOLEAN_TYPE = new org.objectweb.asm.JType(BOOLEAN, PRIMITIVE_DESCRIPTORS, BOOLEAN, BOOLEAN + 1);

	  /// <summary>
	  /// The {@code char} type. </summary>
	  public static readonly org.objectweb.asm.JType CHAR_TYPE = new org.objectweb.asm.JType(CHAR, PRIMITIVE_DESCRIPTORS, CHAR, CHAR + 1);

	  /// <summary>
	  /// The {@code byte} type. </summary>
	  public static readonly org.objectweb.asm.JType BYTE_TYPE = new org.objectweb.asm.JType(BYTE, PRIMITIVE_DESCRIPTORS, BYTE, BYTE + 1);

	  /// <summary>
	  /// The {@code short} type. </summary>
	  public static readonly org.objectweb.asm.JType SHORT_TYPE = new org.objectweb.asm.JType(SHORT, PRIMITIVE_DESCRIPTORS, SHORT, SHORT + 1);

	  /// <summary>
	  /// The {@code int} type. </summary>
	  public static readonly org.objectweb.asm.JType INT_TYPE = new org.objectweb.asm.JType(INT, PRIMITIVE_DESCRIPTORS, INT, INT + 1);

	  /// <summary>
	  /// The {@code float} type. </summary>
	  public static readonly org.objectweb.asm.JType FLOAT_TYPE = new org.objectweb.asm.JType(FLOAT, PRIMITIVE_DESCRIPTORS, FLOAT, FLOAT + 1);

	  /// <summary>
	  /// The {@code long} type. </summary>
	  public static readonly org.objectweb.asm.JType LONG_TYPE = new org.objectweb.asm.JType(LONG, PRIMITIVE_DESCRIPTORS, LONG, LONG + 1);

	  /// <summary>
	  /// The {@code double} type. </summary>
	  public static readonly org.objectweb.asm.JType DOUBLE_TYPE = new org.objectweb.asm.JType(DOUBLE, PRIMITIVE_DESCRIPTORS, DOUBLE, DOUBLE + 1);

	  // -----------------------------------------------------------------------------------------------
	  // Fields
	  // -----------------------------------------------------------------------------------------------

	  /// <summary>
	  /// The sort of this type. Either <seealso cref="VOID"/>, <seealso cref="BOOLEAN"/>, <seealso cref="CHAR"/>, <seealso cref="BYTE"/>,
	  /// <seealso cref="SHORT"/>, <seealso cref="INT"/>, <seealso cref="FLOAT"/>, <seealso cref="LONG"/>, <seealso cref="DOUBLE"/>, <seealso cref="ARRAY"/>,
	  /// <seealso cref="OBJECT"/>, <seealso cref="METHOD"/> or <seealso cref="INTERNAL"/>.
	  /// </summary>
	  private readonly int sort;

	  /// <summary>
	  /// A buffer containing the value of this field or method type. This value is an internal name for
	  /// <seealso cref="OBJECT"/> and <seealso cref="INTERNAL"/> types, and a field or method descriptor in the other
	  /// cases.
	  /// 
	  /// <para>For <seealso cref="OBJECT"/> types, this field also contains the descriptor: the characters in
	  /// [<seealso cref="valueBegin"/>,<seealso cref="valueEnd"/>) contain the internal name, and those in [{@link
	  /// #valueBegin} - 1, <seealso cref="valueEnd"/> + 1) contain the descriptor.
	  /// </para>
	  /// </summary>
	  private readonly string valueBuffer;

	  /// <summary>
	  /// The beginning index, inclusive, of the value of this Java field or method type in {@link
	  /// #valueBuffer}. This value is an internal name for <seealso cref="OBJECT"/> and <seealso cref="INTERNAL"/> types,
	  /// and a field or method descriptor in the other cases.
	  /// </summary>
	  private readonly int valueBegin;

	  /// <summary>
	  /// The end index, exclusive, of the value of this Java field or method type in {@link
	  /// #valueBuffer}. This value is an internal name for <seealso cref="OBJECT"/> and <seealso cref="INTERNAL"/> types,
	  /// and a field or method descriptor in the other cases.
	  /// </summary>
	  private readonly int valueEnd;

	  /// <summary>
	  /// Constructs a reference type.
	  /// </summary>
	  /// <param name="sort"> the sort of this type, see <seealso cref="sort"/>. </param>
	  /// <param name="valueBuffer"> a buffer containing the value of this field or method type. </param>
	  /// <param name="valueBegin"> the beginning index, inclusive, of the value of this field or method type in
	  ///     valueBuffer. </param>
	  /// <param name="valueEnd"> the end index, exclusive, of the value of this field or method type in
	  ///     valueBuffer. </param>
	  private JType(int sort, string valueBuffer, int valueBegin, int valueEnd)
	  {
		this.sort = sort;
		this.valueBuffer = valueBuffer;
		this.valueBegin = valueBegin;
		this.valueEnd = valueEnd;
	  }

	  // -----------------------------------------------------------------------------------------------
	  // Methods to get Type(s) from a descriptor, a reflected Method or Constructor, other types, etc.
	  // -----------------------------------------------------------------------------------------------

	  /// <summary>
	  /// Returns the <seealso cref="Type"/> corresponding to the given type descriptor.
	  /// </summary>
	  /// <param name="typeDescriptor"> a field or method type descriptor. </param>
	  /// <returns> the <seealso cref="Type"/> corresponding to the given type descriptor. </returns>
	  public static org.objectweb.asm.JType getType(string typeDescriptor)
	  {
		return getTypeInternal(typeDescriptor, 0, typeDescriptor.Length);
	  }

	  /// <summary>
	  /// Returns the <seealso cref="Type"/> corresponding to the given class.
	  /// </summary>
	  /// <param name="clazz"> a class. </param>
	  /// <returns> the <seealso cref="Type"/> corresponding to the given class. </returns>
	  public static org.objectweb.asm.JType getType(System.Type clazz)
	  {
		if (clazz.IsPrimitive)
		{
		  if (clazz == typeof(int))
		  {
			return INT_TYPE;
		  }
		  else if (clazz == typeof(void))
		  {
			return VOID_TYPE;
		  }
		  else if (clazz == typeof(bool))
		  {
			return BOOLEAN_TYPE;
		  }
		  else if (clazz == typeof(byte) || clazz == typeof(sbyte))
		  {
			return BYTE_TYPE;
		  }
		  else if (clazz == typeof(char))
		  {
			return CHAR_TYPE;
		  }
		  else if (clazz == typeof(short))
		  {
			return SHORT_TYPE;
		  }
		  else if (clazz == typeof(double))
		  {
			return DOUBLE_TYPE;
		  }
		  else if (clazz == typeof(float))
		  {
			return FLOAT_TYPE;
		  }
		  else if (clazz == typeof(long))
		  {
			return LONG_TYPE;
		  }
		  else
		  {
			throw new Exception("Unknown primitive type");
		  }
		}
		else
		{
		  return getType(getDescriptor(clazz));
		}
	  }

	  /// <summary>
	  /// Returns the method <seealso cref="Type"/> corresponding to the given constructor.
	  /// </summary>
	  /// <param name="constructor"> a <seealso cref="System.Reflection.ConstructorInfo"/> object. </param>
	  /// <returns> the method <seealso cref="Type"/> corresponding to the given constructor. </returns>
	  public static org.objectweb.asm.JType getType(System.Reflection.ConstructorInfo constructor)
	  {
		return getType(getConstructorDescriptor(constructor));
	  }

	  /// <summary>
	  /// Returns the method <seealso cref="Type"/> corresponding to the given method.
	  /// </summary>
	  /// <param name="method"> a <seealso cref="System.Reflection.MethodInfo"/> object. </param>
	  /// <returns> the method <seealso cref="Type"/> corresponding to the given method. </returns>
	  public static org.objectweb.asm.JType getType(System.Reflection.MethodInfo method)
	  {
		return getType(getMethodDescriptor(method));
	  }

	  /// <summary>
	  /// Returns the type of the elements of this array type. This method should only be used for an
	  /// array type.
	  /// </summary>
	  /// <returns> Returns the type of the elements of this array type. </returns>
	  public org.objectweb.asm.JType ElementType
	  {
		  get
		  {
			int numDimensions = Dimensions;
			return getTypeInternal(valueBuffer, valueBegin + numDimensions, valueEnd);
		  }
	  }

	  /// <summary>
	  /// Returns the <seealso cref="Type"/> corresponding to the given internal name.
	  /// </summary>
	  /// <param name="internalName"> an internal name. </param>
	  /// <returns> the <seealso cref="Type"/> corresponding to the given internal name. </returns>
	  public static org.objectweb.asm.JType getObjectType(string internalName)
	  {
		return new org.objectweb.asm.JType(internalName[0] == '[' ? ARRAY : INTERNAL, internalName, 0, internalName.Length);
	  }

	  /// <summary>
	  /// Returns the <seealso cref="Type"/> corresponding to the given method descriptor. Equivalent to <code>
	  /// Type.getType(methodDescriptor)</code>.
	  /// </summary>
	  /// <param name="methodDescriptor"> a method descriptor. </param>
	  /// <returns> the <seealso cref="Type"/> corresponding to the given method descriptor. </returns>
	  public static org.objectweb.asm.JType getMethodType(string methodDescriptor)
	  {
		return new org.objectweb.asm.JType(METHOD, methodDescriptor, 0, methodDescriptor.Length);
	  }

	  /// <summary>
	  /// Returns the method <seealso cref="Type"/> corresponding to the given argument and return types.
	  /// </summary>
	  /// <param name="returnType"> the return type of the method. </param>
	  /// <param name="argumentTypes"> the argument types of the method. </param>
	  /// <returns> the method <seealso cref="Type"/> corresponding to the given argument and return types. </returns>
	  public static org.objectweb.asm.JType getMethodType(org.objectweb.asm.JType returnType, params org.objectweb.asm.JType[] argumentTypes)
	  {
		return getType(getMethodDescriptor(returnType, argumentTypes));
	  }

	  /// <summary>
	  /// Returns the argument types of methods of this type. This method should only be used for method
	  /// types.
	  /// </summary>
	  /// <returns> the argument types of methods of this type. </returns>
	  public org.objectweb.asm.JType[] ArgumentTypes
	  {
		  get
		  {
			return getArgumentTypes(Descriptor);
		  }
	  }

	  /// <summary>
	  /// Returns the <seealso cref="Type"/> values corresponding to the argument types of the given method
	  /// descriptor.
	  /// </summary>
	  /// <param name="methodDescriptor"> a method descriptor. </param>
	  /// <returns> the <seealso cref="Type"/> values corresponding to the argument types of the given method
	  ///     descriptor. </returns>
	  public static org.objectweb.asm.JType[] getArgumentTypes(string methodDescriptor)
	  {
		// First step: compute the number of argument types in methodDescriptor.
		int numArgumentTypes = 0;
		// Skip the first character, which is always a '('.
		int currentOffset = 1;
		// Parse the argument types, one at a each loop iteration.
		while (methodDescriptor[currentOffset] != ')')
		{
		  while (methodDescriptor[currentOffset] == '[')
		  {
			currentOffset++;
		  }
		  if (methodDescriptor[currentOffset++] == 'L')
		  {
			// Skip the argument descriptor content.
			int semiColumnOffset = methodDescriptor.IndexOf(';', currentOffset);
			currentOffset = Math.Max(currentOffset, semiColumnOffset + 1);
		  }
		  ++numArgumentTypes;
		}

		// Second step: create a Type instance for each argument type.
		org.objectweb.asm.JType[] argumentTypes = new org.objectweb.asm.JType[numArgumentTypes];
		// Skip the first character, which is always a '('.
		currentOffset = 1;
		// Parse and create the argument types, one at each loop iteration.
		int currentArgumentTypeIndex = 0;
		while (methodDescriptor[currentOffset] != ')')
		{
		  int currentArgumentTypeOffset = currentOffset;
		  while (methodDescriptor[currentOffset] == '[')
		  {
			currentOffset++;
		  }
		  if (methodDescriptor[currentOffset++] == 'L')
		  {
			// Skip the argument descriptor content.
			int semiColumnOffset = methodDescriptor.IndexOf(';', currentOffset);
			currentOffset = Math.Max(currentOffset, semiColumnOffset + 1);
		  }
		  argumentTypes[currentArgumentTypeIndex++] = getTypeInternal(methodDescriptor, currentArgumentTypeOffset, currentOffset);
		}
		return argumentTypes;
	  }

	  /// <summary>
	  /// Returns the <seealso cref="Type"/> values corresponding to the argument types of the given method.
	  /// </summary>
	  /// <param name="method"> a method. </param>
	  /// <returns> the <seealso cref="Type"/> values corresponding to the argument types of the given method. </returns>
	  public static org.objectweb.asm.JType[] getArgumentTypes(System.Reflection.MethodInfo method)
	  {
		System.Type[] classes = method.GetParameters().Select(p => p.ParameterType).ToArray();
		org.objectweb.asm.JType[] types = new org.objectweb.asm.JType[classes.Length];
		for (int i = classes.Length - 1; i >= 0; --i)
		{
		  types[i] = getType(classes[i]);
		}
		return types;
	  }

	  /// <summary>
	  /// Returns the return type of methods of this type. This method should only be used for method
	  /// types.
	  /// </summary>
	  /// <returns> the return type of methods of this type. </returns>
	  public org.objectweb.asm.JType ReturnType
	  {
		  get
		  {
			return getReturnType(Descriptor);
		  }
	  }

	  /// <summary>
	  /// Returns the <seealso cref="Type"/> corresponding to the return type of the given method descriptor.
	  /// </summary>
	  /// <param name="methodDescriptor"> a method descriptor. </param>
	  /// <returns> the <seealso cref="Type"/> corresponding to the return type of the given method descriptor. </returns>
	  public static org.objectweb.asm.JType getReturnType(string methodDescriptor)
	  {
		return getTypeInternal(methodDescriptor, getReturnTypeOffset(methodDescriptor), methodDescriptor.Length);
	  }

	  /// <summary>
	  /// Returns the <seealso cref="Type"/> corresponding to the return type of the given method.
	  /// </summary>
	  /// <param name="method"> a method. </param>
	  /// <returns> the <seealso cref="Type"/> corresponding to the return type of the given method. </returns>
	  public static org.objectweb.asm.JType getReturnType(System.Reflection.MethodInfo method)
	  {
		return getType(method.ReturnType);
	  }

	  /// <summary>
	  /// Returns the start index of the return type of the given method descriptor.
	  /// </summary>
	  /// <param name="methodDescriptor"> a method descriptor. </param>
	  /// <returns> the start index of the return type of the given method descriptor. </returns>
	  internal static int getReturnTypeOffset(string methodDescriptor)
	  {
		// Skip the first character, which is always a '('.
		int currentOffset = 1;
		// Skip the argument types, one at a each loop iteration.
		while (methodDescriptor[currentOffset] != ')')
		{
		  while (methodDescriptor[currentOffset] == '[')
		  {
			currentOffset++;
		  }
		  if (methodDescriptor[currentOffset++] == 'L')
		  {
			// Skip the argument descriptor content.
			int semiColumnOffset = methodDescriptor.IndexOf(';', currentOffset);
			currentOffset = Math.Max(currentOffset, semiColumnOffset + 1);
		  }
		}
		return currentOffset + 1;
	  }

	  /// <summary>
	  /// Returns the <seealso cref="Type"/> corresponding to the given field or method descriptor.
	  /// </summary>
	  /// <param name="descriptorBuffer"> a buffer containing the field or method descriptor. </param>
	  /// <param name="descriptorBegin"> the beginning index, inclusive, of the field or method descriptor in
	  ///     descriptorBuffer. </param>
	  /// <param name="descriptorEnd"> the end index, exclusive, of the field or method descriptor in
	  ///     descriptorBuffer. </param>
	  /// <returns> the <seealso cref="Type"/> corresponding to the given type descriptor. </returns>
	  private static org.objectweb.asm.JType getTypeInternal(string descriptorBuffer, int descriptorBegin, int descriptorEnd)
	  {
		switch (descriptorBuffer[descriptorBegin])
		{
		  case 'V':
			return VOID_TYPE;
		  case 'Z':
			return BOOLEAN_TYPE;
		  case 'C':
			return CHAR_TYPE;
		  case 'B':
			return BYTE_TYPE;
		  case 'S':
			return SHORT_TYPE;
		  case 'I':
			return INT_TYPE;
		  case 'F':
			return FLOAT_TYPE;
		  case 'J':
			return LONG_TYPE;
		  case 'D':
			return DOUBLE_TYPE;
		  case '[':
			return new org.objectweb.asm.JType(ARRAY, descriptorBuffer, descriptorBegin, descriptorEnd);
		  case 'L':
			return new org.objectweb.asm.JType(OBJECT, descriptorBuffer, descriptorBegin + 1, descriptorEnd - 1);
		  case '(':
			return new org.objectweb.asm.JType(METHOD, descriptorBuffer, descriptorBegin, descriptorEnd);
		  default:
			throw new System.ArgumentException();
		}
	  }

	  // -----------------------------------------------------------------------------------------------
	  // Methods to get class names, internal names or descriptors.
	  // -----------------------------------------------------------------------------------------------

	  /// <summary>
	  /// Returns the binary name of the class corresponding to this type. This method must not be used
	  /// on method types.
	  /// </summary>
	  /// <returns> the binary name of the class corresponding to this type. </returns>
	  public string ClassName
	  {
		  get
		  {
			switch (sort)
			{
			  case VOID:
				return "void";
			  case BOOLEAN:
				return "boolean";
			  case CHAR:
				return "char";
			  case BYTE:
				return "byte";
			  case SHORT:
				return "short";
			  case INT:
				return "int";
			  case FLOAT:
				return "float";
			  case LONG:
				return "long";
			  case DOUBLE:
				return "double";
			  case ARRAY:
				StringBuilder stringBuilder = new StringBuilder(ElementType.ClassName);
				for (int i = Dimensions; i > 0; --i)
				{
				  stringBuilder.Append("[]");
				}
				return stringBuilder.ToString();
			  case OBJECT:
			  case INTERNAL:
				return valueBuffer.Substring(valueBegin, valueEnd - valueBegin).Replace('/', '.');
			  default:
				throw new Exception("Unknown type sort");
			}
		  }
	  }

	  /// <summary>
	  /// Returns the internal name of the class corresponding to this object or array type. The internal
	  /// name of a class is its fully qualified name (as returned by Class.getName(), where '.' are
	  /// replaced by '/'). This method should only be used for an object or array type.
	  /// </summary>
	  /// <returns> the internal name of the class corresponding to this object type. </returns>
	  public string InternalName
	  {
		  get
		  {
			return valueBuffer.Substring(valueBegin, valueEnd - valueBegin);
		  }
	  }

	  /// <summary>
	  /// Returns the internal name of the given class. The internal name of a class is its fully
	  /// qualified name, as returned by Class.getName(), where '.' are replaced by '/'.
	  /// </summary>
	  /// <param name="clazz"> an object or array class. </param>
	  /// <returns> the internal name of the given class. </returns>
	  public static string getInternalName(System.Type clazz)
	  {
		return clazz.FullName.Replace('.', '/');
	  }

	  /// <summary>
	  /// Returns the descriptor corresponding to this type.
	  /// </summary>
	  /// <returns> the descriptor corresponding to this type. </returns>
	  public string Descriptor
	  {
		  get
		  {
			if (sort == OBJECT)
			{
			  return valueBuffer.Substring(valueBegin - 1, (valueEnd + 1) - (valueBegin - 1));
			}
			else if (sort == INTERNAL)
			{
			  return 'L' + valueBuffer.Substring(valueBegin, valueEnd - valueBegin) + ';';
			}
			else
			{
			  return valueBuffer.Substring(valueBegin, valueEnd - valueBegin);
			}
		  }
	  }

	  /// <summary>
	  /// Returns the descriptor corresponding to the given class.
	  /// </summary>
	  /// <param name="clazz"> an object class, a primitive class or an array class. </param>
	  /// <returns> the descriptor corresponding to the given class. </returns>
	  public static string getDescriptor(System.Type clazz)
	  {
		StringBuilder stringBuilder = new StringBuilder();
		appendDescriptor(clazz, stringBuilder);
		return stringBuilder.ToString();
	  }

	  /// <summary>
	  /// Returns the descriptor corresponding to the given constructor.
	  /// </summary>
	  /// <param name="constructor"> a <seealso cref="System.Reflection.ConstructorInfo"/> object. </param>
	  /// <returns> the descriptor of the given constructor. </returns>
	  public static string getConstructorDescriptor(System.Reflection.ConstructorInfo constructor)
	  {
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append('(');
		System.Type[] parameters = constructor.GetParameters().Select(p => p.ParameterType).ToArray();
		foreach (System.Type parameter in parameters)
		{
		  appendDescriptor(parameter, stringBuilder);
		}
		return stringBuilder.Append(")V").ToString();
	  }

	  /// <summary>
	  /// Returns the descriptor corresponding to the given argument and return types.
	  /// </summary>
	  /// <param name="returnType"> the return type of the method. </param>
	  /// <param name="argumentTypes"> the argument types of the method. </param>
	  /// <returns> the descriptor corresponding to the given argument and return types. </returns>
	  public static string getMethodDescriptor(org.objectweb.asm.JType returnType, params org.objectweb.asm.JType[] argumentTypes)
	  {
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append('(');
		foreach (org.objectweb.asm.JType argumentType in argumentTypes)
		{
		  argumentType.appendDescriptor(stringBuilder);
		}
		stringBuilder.Append(')');
		returnType.appendDescriptor(stringBuilder);
		return stringBuilder.ToString();
	  }

	  /// <summary>
	  /// Returns the descriptor corresponding to the given method.
	  /// </summary>
	  /// <param name="method"> a <seealso cref="System.Reflection.MethodInfo"/> object. </param>
	  /// <returns> the descriptor of the given method. </returns>
	  public static string getMethodDescriptor(System.Reflection.MethodInfo method)
	  {
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append('(');
		System.Type[] parameters = method.GetParameters().Select(p => p.ParameterType).ToArray();
		foreach (System.Type parameter in parameters)
		{
		  appendDescriptor(parameter, stringBuilder);
		}
		stringBuilder.Append(')');
		appendDescriptor(method.ReturnType, stringBuilder);
		return stringBuilder.ToString();
	  }

	  /// <summary>
	  /// Appends the descriptor corresponding to this type to the given string buffer.
	  /// </summary>
	  /// <param name="stringBuilder"> the string builder to which the descriptor must be appended. </param>
	  private void appendDescriptor(StringBuilder stringBuilder)
	  {
		if (sort == OBJECT)
		{
		  stringBuilder.Append(valueBuffer, valueBegin - 1, valueEnd + 1);
		}
		else if (sort == INTERNAL)
		{
		  stringBuilder.Append('L').Append(valueBuffer, valueBegin, valueEnd).Append(';');
		}
		else
		{
		  stringBuilder.Append(valueBuffer, valueBegin, valueEnd);
		}
	  }

	  /// <summary>
	  /// Appends the descriptor of the given class to the given string builder.
	  /// </summary>
	  /// <param name="clazz"> the class whose descriptor must be computed. </param>
	  /// <param name="stringBuilder"> the string builder to which the descriptor must be appended. </param>
	  private static void appendDescriptor(System.Type clazz, StringBuilder stringBuilder)
	  {
		System.Type currentClass = clazz;
		while (currentClass.IsArray)
		{
		  stringBuilder.Append('[');
		  currentClass = currentClass.ElementType;
		}
		if (currentClass.IsPrimitive)
		{
		  char descriptor;
		  if (currentClass == typeof(int) || currentClass == typeof(byte))
		  {
			descriptor = 'I';
		  }
		  else if (currentClass == typeof(void))
		  {
			descriptor = 'V';
		  }
		  else if (currentClass == typeof(bool))
		  {
			descriptor = 'Z';
		  }
		  else if (currentClass == typeof(sbyte))
		  {
			descriptor = 'B';
		  }
		  else if (currentClass == typeof(char))
		  {
			descriptor = 'C';
		  }
		  else if (currentClass == typeof(short))
		  {
			descriptor = 'S';
		  }
		  else if (currentClass == typeof(double))
		  {
			descriptor = 'D';
		  }
		  else if (currentClass == typeof(float))
		  {
			descriptor = 'F';
		  }
		  else if (currentClass == typeof(long))
		  {
			descriptor = 'J';
		  }
		  else
		  {
			throw new Exception("Unknown primitive descriptor type");
		  }
		  stringBuilder.Append(descriptor);
		}
		else
		{
		  stringBuilder.Append('L').Append(getInternalName(currentClass)).Append(';');
		}
	  }

	  // -----------------------------------------------------------------------------------------------
	  // Methods to get the sort, dimension, size, and opcodes corresponding to a Type or descriptor.
	  // -----------------------------------------------------------------------------------------------

	  /// <summary>
	  /// Returns the sort of this type.
	  /// </summary>
	  /// <returns> <seealso cref="VOID"/>, <seealso cref="BOOLEAN"/>, <seealso cref="CHAR"/>, <seealso cref="BYTE"/>, <seealso cref="SHORT"/>, {@link
	  ///     #INT}, <seealso cref="FLOAT"/>, <seealso cref="LONG"/>, <seealso cref="DOUBLE"/>, <seealso cref="ARRAY"/>, <seealso cref="OBJECT"/> or
	  ///     <seealso cref="METHOD"/>. </returns>
	  public int Sort
	  {
		  get
		  {
			return sort == INTERNAL ? OBJECT : sort;
		  }
	  }

	  /// <summary>
	  /// Returns the number of dimensions of this array type. This method should only be used for an
	  /// array type.
	  /// </summary>
	  /// <returns> the number of dimensions of this array type. </returns>
	  public int Dimensions
	  {
		  get
		  {
			int numDimensions = 1;
			while (valueBuffer[valueBegin + numDimensions] == '[')
			{
			  numDimensions++;
			}
			return numDimensions;
		  }
	  }

	  /// <summary>
	  /// Returns the size of values of this type. This method must not be used for method types.
	  /// </summary>
	  /// <returns> the size of values of this type, i.e., 2 for {@code long} and {@code double}, 0 for
	  ///     {@code void} and 1 otherwise. </returns>
	  public int Size
	  {
		  get
		  {
			switch (sort)
			{
			  case VOID:
				return 0;
			  case BOOLEAN:
			  case CHAR:
			  case BYTE:
			  case SHORT:
			  case INT:
			  case FLOAT:
			  case ARRAY:
			  case OBJECT:
			  case INTERNAL:
				return 1;
			  case LONG:
			  case DOUBLE:
				return 2;
			  default:
				throw new ("AssertionError");
			}
		  }
	  }

	  /// <summary>
	  /// Returns the size of the arguments and of the return value of methods of this type. This method
	  /// should only be used for method types.
	  /// </summary>
	  /// <returns> the size of the arguments of the method (plus one for the implicit this argument),
	  ///     argumentsSize, and the size of its return value, returnSize, packed into a single int i =
	  ///     {@code (argumentsSize &lt;&lt; 2) | returnSize} (argumentsSize is therefore equal to {@code
	  ///     i &gt;&gt; 2}, and returnSize to {@code i &amp; 0x03}). </returns>
	  public int ArgumentsAndReturnSizes
	  {
		  get
		  {
			return getArgumentsAndReturnSizes(Descriptor);
		  }
	  }

	  /// <summary>
	  /// Computes the size of the arguments and of the return value of a method.
	  /// </summary>
	  /// <param name="methodDescriptor"> a method descriptor. </param>
	  /// <returns> the size of the arguments of the method (plus one for the implicit this argument),
	  ///     argumentsSize, and the size of its return value, returnSize, packed into a single int i =
	  ///     {@code (argumentsSize &lt;&lt; 2) | returnSize} (argumentsSize is therefore equal to {@code
	  ///     i &gt;&gt; 2}, and returnSize to {@code i &amp; 0x03}). </returns>
	  public static int getArgumentsAndReturnSizes(string methodDescriptor)
	  {
		int argumentsSize = 1;
		// Skip the first character, which is always a '('.
		int currentOffset = 1;
		int currentChar = methodDescriptor[currentOffset];
		// Parse the argument types and compute their size, one at a each loop iteration.
		while (currentChar != ')')
		{
		  if (currentChar == 'J' || currentChar == 'D')
		  {
			currentOffset++;
			argumentsSize += 2;
		  }
		  else
		  {
			while (methodDescriptor[currentOffset] == '[')
			{
			  currentOffset++;
			}
			if (methodDescriptor[currentOffset++] == 'L')
			{
			  // Skip the argument descriptor content.
			  int semiColumnOffset = methodDescriptor.IndexOf(';', currentOffset);
			  currentOffset = Math.Max(currentOffset, semiColumnOffset + 1);
			}
			argumentsSize += 1;
		  }
		  currentChar = methodDescriptor[currentOffset];
		}
		currentChar = methodDescriptor[currentOffset + 1];
		if (currentChar == 'V')
		{
		  return argumentsSize << 2;
		}
		else
		{
		  int returnSize = (currentChar == 'J' || currentChar == 'D') ? 2 : 1;
		  return argumentsSize << 2 | returnSize;
		}
	  }

	  /// <summary>
	  /// Returns a JVM instruction opcode adapted to this <seealso cref="Type"/>. This method must not be used for
	  /// method types.
	  /// </summary>
	  /// <param name="opcode"> a JVM instruction opcode. This opcode must be one of ILOAD, ISTORE, IALOAD,
	  ///     IASTORE, IADD, ISUB, IMUL, IDIV, IREM, INEG, ISHL, ISHR, IUSHR, IAND, IOR, IXOR and
	  ///     IRETURN. </param>
	  /// <returns> an opcode that is similar to the given opcode, but adapted to this <seealso cref="Type"/>. For
	  ///     example, if this type is {@code float} and {@code opcode} is IRETURN, this method returns
	  ///     FRETURN. </returns>
	  public int getOpcode(int opcode)
	  {
		if (opcode == Opcodes.IALOAD || opcode == Opcodes.IASTORE)
		{
		  switch (sort)
		  {
			case BOOLEAN:
			case BYTE:
			  return opcode + (Opcodes.BALOAD - Opcodes.IALOAD);
			case CHAR:
			  return opcode + (Opcodes.CALOAD - Opcodes.IALOAD);
			case SHORT:
			  return opcode + (Opcodes.SALOAD - Opcodes.IALOAD);
			case INT:
			  return opcode;
			case FLOAT:
			  return opcode + (Opcodes.FALOAD - Opcodes.IALOAD);
			case LONG:
			  return opcode + (Opcodes.LALOAD - Opcodes.IALOAD);
			case DOUBLE:
			  return opcode + (Opcodes.DALOAD - Opcodes.IALOAD);
			case ARRAY:
			case OBJECT:
			case INTERNAL:
			  return opcode + (Opcodes.AALOAD - Opcodes.IALOAD);
			case METHOD:
			case VOID:
			  throw new System.NotSupportedException();
			default:
			  throw new ("AssertionError");
		  }
		}
		else
		{
		  switch (sort)
		  {
			case VOID:
			  if (opcode != Opcodes.IRETURN)
			  {
				throw new System.NotSupportedException();
			  }
			  return Opcodes.RETURN;
			case BOOLEAN:
			case BYTE:
			case CHAR:
			case SHORT:
			case INT:
			  return opcode;
			case FLOAT:
			  return opcode + (Opcodes.FRETURN - Opcodes.IRETURN);
			case LONG:
			  return opcode + (Opcodes.LRETURN - Opcodes.IRETURN);
			case DOUBLE:
			  return opcode + (Opcodes.DRETURN - Opcodes.IRETURN);
			case ARRAY:
			case OBJECT:
			case INTERNAL:
			  if (opcode != Opcodes.ILOAD && opcode != Opcodes.ISTORE && opcode != Opcodes.IRETURN)
			  {
				throw new System.NotSupportedException();
			  }
			  return opcode + (Opcodes.ARETURN - Opcodes.IRETURN);
			case METHOD:
			  throw new System.NotSupportedException();
			default:
			  throw new ("AssertionError");
		  }
		}
	  }

	  // -----------------------------------------------------------------------------------------------
	  // Equals, hashCode and toString.
	  // -----------------------------------------------------------------------------------------------

	  /// <summary>
	  /// Tests if the given object is equal to this type.
	  /// </summary>
	  /// <param name="object"> the object to be compared to this type. </param>
	  /// <returns> {@literal true} if the given object is equal to this type. </returns>
	  public override bool Equals(object @object)
	  {
		if (this == @object)
		{
		  return true;
		}
		if (!(@object is Type))
		{
		  return false;
		}
		org.objectweb.asm.JType other = (org.objectweb.asm.JType) @object;
		if ((sort == INTERNAL ? OBJECT : sort) != (other.sort == INTERNAL ? OBJECT : other.sort))
		{
		  return false;
		}
		int begin = valueBegin;
		int end = valueEnd;
		int otherBegin = other.valueBegin;
		int otherEnd = other.valueEnd;
		// Compare the values.
		if (end - begin != otherEnd - otherBegin)
		{
		  return false;
		}
		for (int i = begin, j = otherBegin; i < end; i++, j++)
		{
		  if (valueBuffer[i] != other.valueBuffer[j])
		  {
			return false;
		  }
		}
		return true;
	  }

	  /// <summary>
	  /// Returns a hash code value for this type.
	  /// </summary>
	  /// <returns> a hash code value for this type. </returns>
	  public override int GetHashCode()
	  {
		int hashCode = 13 * (sort == INTERNAL ? OBJECT : sort);
		if (sort >= ARRAY)
		{
		  for (int i = valueBegin, end = valueEnd; i < end; i++)
		  {
			hashCode = 17 * (hashCode + valueBuffer[i]);
		  }
		}
		return hashCode;
	  }

	  /// <summary>
	  /// Returns a string representation of this type.
	  /// </summary>
	  /// <returns> the descriptor of this type. </returns>
	  public override string ToString()
	  {
		return Descriptor;
	  }
	}

}