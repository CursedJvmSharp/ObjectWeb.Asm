using org.objectweb.asm;
using CursedJvmSharp.Asm.Java.IO;
using System;
using System.Text;
using ConstantDynamic = org.objectweb.asm.ConstantDynamic;
using Handle = org.objectweb.asm.Handle;
using Opcodes = org.objectweb.asm.Opcodes;
using SignatureReader = org.objectweb.asm.signature.SignatureReader;
using SignatureVisitor = org.objectweb.asm.signature.SignatureVisitor;
using SignatureWriter = org.objectweb.asm.signature.SignatureWriter;

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

namespace org.objectweb.asm.commons
{

	/// <summary>
	/// A class responsible for remapping types and names.
	/// 
	/// @author Eugene Kuleshov
	/// </summary>
	public abstract class Remapper
	{

	  /// <summary>
	  /// Returns the given descriptor, remapped with <seealso cref="map(String)"/>.
	  /// </summary>
	  /// <param name="descriptor"> a type descriptor. </param>
	  /// <returns> the given descriptor, with its [array element type] internal name remapped with {@link
	  ///     #map(String)} (if the descriptor corresponds to an array or object type, otherwise the
	  ///     descriptor is returned as is). </returns>
	  public virtual string mapDesc(string descriptor)
	  {
		return mapType(org.objectweb.asm.JType.getType(descriptor)).Descriptor;
	  }

	  /// <summary>
	  /// Returns the given <seealso cref="Type"/>, remapped with <seealso cref="map(String)"/> or {@link
	  /// #mapMethodDesc(String)}.
	  /// </summary>
	  /// <param name="type"> a type, which can be a method type. </param>
	  /// <returns> the given type, with its [array element type] internal name remapped with {@link
	  ///     #map(String)} (if the type is an array or object type, otherwise the type is returned as
	  ///     is) or, of the type is a method type, with its descriptor remapped with {@link
	  ///     #mapMethodDesc(String)}. </returns>
	  private org.objectweb.asm.JType mapType(org.objectweb.asm.JType type)
	  {
		switch (type.Sort)
		{
		  case org.objectweb.asm.JType.ARRAY:
			StringBuilder remappedDescriptor = new StringBuilder();
			for (int i = 0; i < type.Dimensions; ++i)
			{
			  remappedDescriptor.Append('[');
			}
			remappedDescriptor.Append(mapType(type.ElementType).Descriptor);
			return org.objectweb.asm.JType.getType(remappedDescriptor.ToString());
		  case org.objectweb.asm.JType.OBJECT:
			string remappedInternalName = map(type.InternalName);
			return !string.ReferenceEquals(remappedInternalName, null) ? org.objectweb.asm.JType.getObjectType(remappedInternalName) : type;
		  case org.objectweb.asm.JType.METHOD:
			return org.objectweb.asm.JType.getMethodType(mapMethodDesc(type.Descriptor));
		  default:
			return type;
		}
	  }

	  /// <summary>
	  /// Returns the given internal name, remapped with <seealso cref="map(String)"/>.
	  /// </summary>
	  /// <param name="internalName"> the internal name (or array type descriptor) of some (array) class. </param>
	  /// <returns> the given internal name, remapped with <seealso cref="map(String)"/>. </returns>
	  public virtual string mapType(string internalName)
	  {
		if (string.ReferenceEquals(internalName, null))
		{
		  return null;
		}
		return mapType(org.objectweb.asm.JType.getObjectType(internalName)).InternalName;
	  }

	  /// <summary>
	  /// Returns the given internal names, remapped with <seealso cref="map(String)"/>.
	  /// </summary>
	  /// <param name="internalNames"> the internal names (or array type descriptors) of some (array) classes. </param>
	  /// <returns> the given internal name, remapped with <seealso cref="map(String)"/>. </returns>
	  public virtual string[] mapTypes(string[] internalNames)
	  {
		string[] remappedInternalNames = null;
		for (int i = 0; i < internalNames.Length; ++i)
		{
		  string internalName = internalNames[i];
		  string remappedInternalName = mapType(internalName);
		  if (!string.ReferenceEquals(remappedInternalName, null))
		  {
			if (remappedInternalNames == null)
			{
			  remappedInternalNames = (string[])internalNames.Clone();
			}
			remappedInternalNames[i] = remappedInternalName;
		  }
		}
		return remappedInternalNames != null ? remappedInternalNames : internalNames;
	  }

	  /// <summary>
	  /// Returns the given method descriptor, with its argument and return type descriptors remapped
	  /// with <seealso cref="mapDesc(String)"/>.
	  /// </summary>
	  /// <param name="methodDescriptor"> a method descriptor. </param>
	  /// <returns> the given method descriptor, with its argument and return type descriptors remapped
	  ///     with <seealso cref="mapDesc(String)"/>. </returns>
	  public virtual string mapMethodDesc(string methodDescriptor)
	  {
		if ("()V".Equals(methodDescriptor))
		{
		  return methodDescriptor;
		}

		StringBuilder stringBuilder = new StringBuilder("(");
		foreach (org.objectweb.asm.JType argumentType in org.objectweb.asm.JType.getArgumentTypes(methodDescriptor))
		{
		  stringBuilder.Append(mapType(argumentType).Descriptor);
		}
		org.objectweb.asm.JType returnType = org.objectweb.asm.JType.getReturnType(methodDescriptor);
		if (returnType == org.objectweb.asm.JType.VOID_TYPE)
		{
		  stringBuilder.Append(")V");
		}
		else
		{
		  stringBuilder.Append(')').Append(mapType(returnType).Descriptor);
		}
		return stringBuilder.ToString();
	  }

	  /// <summary>
	  /// Returns the given value, remapped with this remapper. Possible values are <seealso cref="Boolean"/>,
	  /// <seealso cref="Byte"/>, <seealso cref="Short"/>, <seealso cref="Character"/>, <seealso cref="Integer"/>, <seealso cref="Long"/>, <seealso cref="Double"/>,
	  /// <seealso cref="Float"/>, <seealso cref="string"/>, <seealso cref="Type"/>, <seealso cref="Handle"/>, <seealso cref="ConstantDynamic"/> or arrays
	  /// of primitive types .
	  /// </summary>
	  /// <param name="value"> an object. Only <seealso cref="Type"/>, <seealso cref="Handle"/> and <seealso cref="ConstantDynamic"/> values
	  ///     are remapped. </param>
	  /// <returns> the given value, remapped with this remapper. </returns>
	  public virtual object mapValue(object value)
	  {
		if (value is Type)
		{
		  return mapType((org.objectweb.asm.JType) value);
		}
		if (value is Handle)
		{
		  Handle handle = (Handle) value;
		  return new Handle(handle.Tag, mapType(handle.Owner), mapMethodName(handle.Owner, handle.Name, handle.Desc), handle.Tag <= Opcodes.H_PUTSTATIC ? mapDesc(handle.Desc) : mapMethodDesc(handle.Desc), handle.Interface);
		}
		if (value is ConstantDynamic)
		{
		  ConstantDynamic constantDynamic = (ConstantDynamic) value;
		  int bootstrapMethodArgumentCount = constantDynamic.BootstrapMethodArgumentCount;
		  object[] remappedBootstrapMethodArguments = new object[bootstrapMethodArgumentCount];
		  for (int i = 0; i < bootstrapMethodArgumentCount; ++i)
		  {
			remappedBootstrapMethodArguments[i] = mapValue(constantDynamic.getBootstrapMethodArgument(i));
		  }
		  string descriptor = constantDynamic.Descriptor;
		  return new ConstantDynamic(mapInvokeDynamicMethodName(constantDynamic.Name, descriptor), mapDesc(descriptor), (Handle) mapValue(constantDynamic.BootstrapMethod), remappedBootstrapMethodArguments);
		}
		return value;
	  }

	  /// <summary>
	  /// Returns the given signature, remapped with the <seealso cref="SignatureVisitor"/> returned by {@link
	  /// #createSignatureRemapper(SignatureVisitor)}.
	  /// </summary>
	  /// <param name="signature"> a <i>JavaTypeSignature</i>, <i>ClassSignature</i> or <i>MethodSignature</i>. </param>
	  /// <param name="typeSignature"> whether the given signature is a <i>JavaTypeSignature</i>. </param>
	  /// <returns> signature the given signature, remapped with the <seealso cref="SignatureVisitor"/> returned by
	  ///     <seealso cref="createSignatureRemapper(SignatureVisitor)"/>. </returns>
	  public virtual string mapSignature(string signature, bool typeSignature)
	  {
		if (string.ReferenceEquals(signature, null))
		{
		  return null;
		}
		SignatureReader signatureReader = new SignatureReader(signature);
		SignatureWriter signatureWriter = new SignatureWriter();
		SignatureVisitor signatureRemapper = createSignatureRemapper(signatureWriter);
		if (typeSignature)
		{
		  signatureReader.acceptType(signatureRemapper);
		}
		else
		{
		  signatureReader.accept(signatureRemapper);
		}
		return signatureWriter.ToString();
	  }

	  /// <summary>
	  /// Constructs a new remapper for signatures. The default implementation of this method returns a
	  /// new <seealso cref="SignatureRemapper"/>.
	  /// </summary>
	  /// <param name="signatureVisitor"> the SignatureVisitor the remapper must delegate to. </param>
	  /// <returns> the newly created remapper. </returns>
	  /// @deprecated use <seealso cref="createSignatureRemapper"/> instead. 
	  [Obsolete("use <seealso cref=\"createSignatureRemapper\"/> instead.")]
	  public virtual SignatureVisitor createRemappingSignatureAdapter(SignatureVisitor signatureVisitor)
	  {
		return createSignatureRemapper(signatureVisitor);
	  }

	  /// <summary>
	  /// Constructs a new remapper for signatures. The default implementation of this method returns a
	  /// new <seealso cref="SignatureRemapper"/>.
	  /// </summary>
	  /// <param name="signatureVisitor"> the SignatureVisitor the remapper must delegate to. </param>
	  /// <returns> the newly created remapper. </returns>
	  public virtual SignatureVisitor createSignatureRemapper(SignatureVisitor signatureVisitor)
	  {
		return new SignatureRemapper(signatureVisitor, this);
	  }

	  /// <summary>
	  /// Maps an annotation attribute name. The default implementation of this method returns the given
	  /// name, unchanged. Subclasses can override.
	  /// </summary>
	  /// <param name="descriptor"> the descriptor of the annotation class. </param>
	  /// <param name="name"> the name of the annotation attribute. </param>
	  /// <returns> the new name of the annotation attribute. </returns>
	  public virtual string mapAnnotationAttributeName(string descriptor, string name)
	  {
		return name;
	  }

	  /// <summary>
	  /// Maps an inner class name to its new name. The default implementation of this method provides a
	  /// strategy that will work for inner classes produced by Java, but not necessarily other
	  /// languages. Subclasses can override.
	  /// </summary>
	  /// <param name="name"> the fully-qualified internal name of the inner class. </param>
	  /// <param name="ownerName"> the internal name of the owner class of the inner class. </param>
	  /// <param name="innerName"> the internal name of the inner class. </param>
	  /// <returns> the new inner name of the inner class. </returns>
	  public virtual string mapInnerClassName(string name, string ownerName, string innerName)
	  {
		string remappedInnerName = this.mapType(name);
		if (remappedInnerName.Contains("$"))
		{
		  int index = remappedInnerName.LastIndexOf('$') + 1;
		  while (index < remappedInnerName.Length && char.IsDigit(remappedInnerName[index]))
		  {
			index++;
		  }
		  return remappedInnerName.Substring(index);
		}
		else
		{
		  return innerName;
		}
	  }

	  /// <summary>
	  /// Maps a method name to its new name. The default implementation of this method returns the given
	  /// name, unchanged. Subclasses can override.
	  /// </summary>
	  /// <param name="owner"> the internal name of the owner class of the method. </param>
	  /// <param name="name"> the name of the method. </param>
	  /// <param name="descriptor"> the descriptor of the method. </param>
	  /// <returns> the new name of the method. </returns>
	  public virtual string mapMethodName(string owner, string name, string descriptor)
	  {
		return name;
	  }

	  /// <summary>
	  /// Maps an invokedynamic or a constant dynamic method name to its new name. The default
	  /// implementation of this method returns the given name, unchanged. Subclasses can override.
	  /// </summary>
	  /// <param name="name"> the name of the method. </param>
	  /// <param name="descriptor"> the descriptor of the method. </param>
	  /// <returns> the new name of the method. </returns>
	  public virtual string mapInvokeDynamicMethodName(string name, string descriptor)
	  {
		return name;
	  }

	  /// <summary>
	  /// Maps a record component name to its new name. The default implementation of this method returns
	  /// the given name, unchanged. Subclasses can override.
	  /// </summary>
	  /// <param name="owner"> the internal name of the owner class of the field. </param>
	  /// <param name="name"> the name of the field. </param>
	  /// <param name="descriptor"> the descriptor of the field. </param>
	  /// <returns> the new name of the field. </returns>
	  public virtual string mapRecordComponentName(string owner, string name, string descriptor)
	  {
		return name;
	  }

	  /// <summary>
	  /// Maps a field name to its new name. The default implementation of this method returns the given
	  /// name, unchanged. Subclasses can override.
	  /// </summary>
	  /// <param name="owner"> the internal name of the owner class of the field. </param>
	  /// <param name="name"> the name of the field. </param>
	  /// <param name="descriptor"> the descriptor of the field. </param>
	  /// <returns> the new name of the field. </returns>
	  public virtual string mapFieldName(string owner, string name, string descriptor)
	  {
		return name;
	  }

	  /// <summary>
	  /// Maps a package name to its new name. The default implementation of this method returns the
	  /// given name, unchanged. Subclasses can override.
	  /// </summary>
	  /// <param name="name"> the fully qualified name of the package (using dots). </param>
	  /// <returns> the new name of the package. </returns>
	  public virtual string mapPackageName(string name)
	  {
		return name;
	  }

	  /// <summary>
	  /// Maps a module name to its new name. The default implementation of this method returns the given
	  /// name, unchanged. Subclasses can override.
	  /// </summary>
	  /// <param name="name"> the fully qualified name (using dots) of a module. </param>
	  /// <returns> the new name of the module. </returns>
	  public virtual string mapModuleName(string name)
	  {
		return name;
	  }

	  /// <summary>
	  /// Maps the internal name of a class to its new name. The default implementation of this method
	  /// returns the given name, unchanged. Subclasses can override.
	  /// </summary>
	  /// <param name="internalName"> the internal name of a class. </param>
	  /// <returns> the new internal name. </returns>
	  public virtual string map(string internalName)
	  {
		return internalName;
	  }
	}

}