using System;
using System.Collections.Generic;
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
namespace ObjectWeb.Asm.Commons
{

	/// <summary>
	/// A named method descriptor.
	/// 
	/// @author Juozas Baliuka
	/// @author Chris Nokleberg
	/// @author Eric Bruneton
	/// </summary>
	public class Method
	{

	  /// <summary>
	  /// The method name. </summary>
	  private readonly string name;

	  /// <summary>
	  /// The method descriptor. </summary>
	  private readonly string descriptor;

	  /// <summary>
	  /// The descriptors of the primitive Java types (plus void). </summary>
	  private static readonly IDictionary<string, string> PRIMITIVE_TYPE_DESCRIPTORS;

	  static Method()
	  {
		Dictionary<string, string> descriptors = new Dictionary<string, string>();
		descriptors["void"] = "V";
		descriptors["byte"] = "B";
		descriptors["char"] = "C";
		descriptors["double"] = "D";
		descriptors["float"] = "F";
		descriptors["int"] = "I";
		descriptors["long"] = "J";
		descriptors["short"] = "S";
		descriptors["boolean"] = "Z";
		PRIMITIVE_TYPE_DESCRIPTORS = descriptors;
	  }

	  /// <summary>
	  /// Constructs a new <seealso cref="System.Reflection.MethodInfo"/>.
	  /// </summary>
	  /// <param name="name"> the method's name. </param>
	  /// <param name="descriptor"> the method's descriptor. </param>
	  public Method(string name, string descriptor)
	  {
		this.name = name;
		this.descriptor = descriptor;
	  }

	  /// <summary>
	  /// Constructs a new <seealso cref="System.Reflection.MethodInfo"/>.
	  /// </summary>
	  /// <param name="name"> the method's name. </param>
	  /// <param name="returnType"> the method's return type. </param>
	  /// <param name="argumentTypes"> the method's argument types. </param>
	  public Method(string name, JType returnType, JType[] argumentTypes) : this(name, JType.getMethodDescriptor(returnType, argumentTypes))
	  {
	  }

	  /// <summary>
	  /// Creates a new <seealso cref="System.Reflection.MethodInfo"/>.
	  /// </summary>
	  /// <param name="method"> a java.lang.reflect method descriptor </param>
	  /// <returns> a <seealso cref="System.Reflection.MethodInfo"/> corresponding to the given Java method declaration. </returns>
	  public static Method getMethod(System.Reflection.MethodInfo method)
	  {
		return new Method(method.Name, JType.getMethodDescriptor(method));
	  }

	  /// <summary>
	  /// Creates a new <seealso cref="System.Reflection.MethodInfo"/>.
	  /// </summary>
	  /// <param name="constructor"> a java.lang.reflect constructor descriptor </param>
	  /// <returns> a <seealso cref="System.Reflection.MethodInfo"/> corresponding to the given Java constructor declaration. </returns>
	  public static Method getMethod<T1>(System.Reflection.ConstructorInfo constructor)
	  {
		return new Method("<init>", JType.getConstructorDescriptor(constructor));
	  }

	  /// <summary>
	  /// Returns a <seealso cref="System.Reflection.MethodInfo"/> corresponding to the given Java method declaration.
	  /// </summary>
	  /// <param name="method"> a Java method declaration, without argument names, of the form "returnType name
	  ///     (argumentType1, ... argumentTypeN)", where the types are in plain Java (e.g. "int",
	  ///     "float", "java.util.List", ...). Classes of the java.lang package can be specified by their
	  ///     unqualified name; all other classes names must be fully qualified. </param>
	  /// <returns> a <seealso cref="System.Reflection.MethodInfo"/> corresponding to the given Java method declaration. </returns>
	  /// <exception cref="IllegalArgumentException"> if <code>method</code> could not get parsed. </exception>
	  public static Method getMethod(string method)
	  {
		return getMethod(method, false);
	  }

	  /// <summary>
	  /// Returns a <seealso cref="System.Reflection.MethodInfo"/> corresponding to the given Java method declaration.
	  /// </summary>
	  /// <param name="method"> a Java method declaration, without argument names, of the form "returnType name
	  ///     (argumentType1, ... argumentTypeN)", where the types are in plain Java (e.g. "int",
	  ///     "float", "java.util.List", ...). Classes of the java.lang package may be specified by their
	  ///     unqualified name, depending on the defaultPackage argument; all other classes names must be
	  ///     fully qualified. </param>
	  /// <param name="defaultPackage"> true if unqualified class names belong to the default package, or false
	  ///     if they correspond to java.lang classes. For instance "Object" means "Object" if this
	  ///     option is true, or "java.lang.Object" otherwise. </param>
	  /// <returns> a <seealso cref="System.Reflection.MethodInfo"/> corresponding to the given Java method declaration. </returns>
	  /// <exception cref="IllegalArgumentException"> if <code>method</code> could not get parsed. </exception>
	  public static Method getMethod(string method, bool defaultPackage)
	  {
		int spaceIndex = method.IndexOf(' ');
		int currentArgumentStartIndex = method.IndexOf('(', spaceIndex) + 1;
		int endIndex = method.IndexOf(')', currentArgumentStartIndex);
		if (spaceIndex == -1 || currentArgumentStartIndex == 0 || endIndex == -1)
		{
		  throw new System.ArgumentException();
		}
		string returnType = method.Substring(0, spaceIndex);
		string methodName = method.Substring(spaceIndex + 1, (currentArgumentStartIndex - 1) - (spaceIndex + 1)).Trim();
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append('(');
		int currentArgumentEndIndex;
		do
		{
		  string argumentDescriptor;
		  currentArgumentEndIndex = method.IndexOf(',', currentArgumentStartIndex);
		  if (currentArgumentEndIndex == -1)
		  {
			argumentDescriptor = getDescriptorInternal(method.Substring(currentArgumentStartIndex, endIndex - currentArgumentStartIndex).Trim(), defaultPackage);
		  }
		  else
		  {
			argumentDescriptor = getDescriptorInternal(method.Substring(currentArgumentStartIndex, currentArgumentEndIndex - currentArgumentStartIndex).Trim(), defaultPackage);
			currentArgumentStartIndex = currentArgumentEndIndex + 1;
		  }
		  stringBuilder.Append(argumentDescriptor);
		} while (currentArgumentEndIndex != -1);
		stringBuilder.Append(')').Append(getDescriptorInternal(returnType, defaultPackage));
		return new Method(methodName, stringBuilder.ToString());
	  }

	  /// <summary>
	  /// Returns the descriptor corresponding to the given type name.
	  /// </summary>
	  /// <param name="type"> a Java type name. </param>
	  /// <param name="defaultPackage"> true if unqualified class names belong to the default package, or false
	  ///     if they correspond to java.lang classes. For instance "Object" means "Object" if this
	  ///     option is true, or "java.lang.Object" otherwise. </param>
	  /// <returns> the descriptor corresponding to the given type name. </returns>
	  private static string getDescriptorInternal(string type, bool defaultPackage)
	  {
		if ("".Equals(type))
		{
		  return type;
		}

		StringBuilder stringBuilder = new StringBuilder();
		int arrayBracketsIndex = 0;
		while ((arrayBracketsIndex = type.IndexOf("[]", arrayBracketsIndex, StringComparison.Ordinal) + 1) > 0)
		{
		  stringBuilder.Append('[');
		}

		string elementType = type.Substring(0, type.Length - stringBuilder.Length * 2);
		string descriptor = PRIMITIVE_TYPE_DESCRIPTORS.GetValueOrNull(elementType);
		if (!string.ReferenceEquals(descriptor, null))
		{
		  stringBuilder.Append(descriptor);
		}
		else
		{
		  stringBuilder.Append('L');
		  if (elementType.IndexOf('.') < 0)
		  {
			if (!defaultPackage)
			{
			  stringBuilder.Append("java/lang/");
			}
			stringBuilder.Append(elementType);
		  }
		  else
		  {
			stringBuilder.Append(elementType.Replace('.', '/'));
		  }
		  stringBuilder.Append(';');
		}
		return stringBuilder.ToString();
	  }

	  /// <summary>
	  /// Returns the name of the method described by this object.
	  /// </summary>
	  /// <returns> the name of the method described by this object. </returns>
	  public virtual string Name
	  {
		  get
		  {
			return name;
		  }
	  }

	  /// <summary>
	  /// Returns the descriptor of the method described by this object.
	  /// </summary>
	  /// <returns> the descriptor of the method described by this object. </returns>
	  public virtual string Descriptor
	  {
		  get
		  {
			return descriptor;
		  }
	  }

	  /// <summary>
	  /// Returns the return type of the method described by this object.
	  /// </summary>
	  /// <returns> the return type of the method described by this object. </returns>
	  public virtual JType ReturnType
	  {
		  get
		  {
			return JType.getReturnType(descriptor);
		  }
	  }

	  /// <summary>
	  /// Returns the argument types of the method described by this object.
	  /// </summary>
	  /// <returns> the argument types of the method described by this object. </returns>
	  public virtual JType[] ArgumentTypes
	  {
		  get
		  {
			return JType.getArgumentTypes(descriptor);
		  }
	  }

	  public override string ToString()
	  {
		return name + descriptor;
	  }

	  public override bool Equals(object other)
	  {
		if (!(other is Method))
		{
		  return false;
		}
		Method otherMethod = (Method) other;
		return name.Equals(otherMethod.name) && descriptor.Equals(otherMethod.descriptor);
	  }

	  public override int GetHashCode()
	  {
		return name.GetHashCode() ^ descriptor.GetHashCode();
	  }
	}

}