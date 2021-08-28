using System;
using System.Text;
using SignatureReader = ObjectWeb.Asm.Signature.SignatureReader;
using SignatureVisitor = ObjectWeb.Asm.Signature.SignatureVisitor;
using SignatureWriter = ObjectWeb.Asm.Signature.SignatureWriter;

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
	/// A class responsible for remapping types and names.
	/// 
	/// @author Eugene Kuleshov
	/// </summary>
	public abstract class Remapper
	{

	  /// <summary>
	  /// Returns the given descriptor, remapped with <seealso cref="Map"/>.
	  /// </summary>
	  /// <param name="descriptor"> a type descriptor. </param>
	  /// <returns> the given descriptor, with its [array element type] internal name remapped with {@link
	  ///     #map(String)} (if the descriptor corresponds to an array or object type, otherwise the
	  ///     descriptor is returned as is). </returns>
	  public virtual string MapDesc(string descriptor)
	  {
		return MapType(JType.GetType(descriptor)).Descriptor;
	  }

	  /// <summary>
	  /// Returns the given <seealso cref="Type"/>, remapped with <seealso cref="Map"/> or {@link
	  /// #mapMethodDesc(String)}.
	  /// </summary>
	  /// <param name="type"> a type, which can be a method type. </param>
	  /// <returns> the given type, with its [array element type] internal name remapped with {@link
	  ///     #map(String)} (if the type is an array or object type, otherwise the type is returned as
	  ///     is) or, of the type is a method type, with its descriptor remapped with {@link
	  ///     #mapMethodDesc(String)}. </returns>
	  private JType MapType(JType type)
	  {
		switch (type.Sort)
		{
		  case JType.Array:
			StringBuilder remappedDescriptor = new StringBuilder();
			for (int i = 0; i < type.Dimensions; ++i)
			{
			  remappedDescriptor.Append('[');
			}
			remappedDescriptor.Append(MapType(type.ElementType).Descriptor);
			return JType.GetType(remappedDescriptor.ToString());
		  case JType.Object:
			string remappedInternalName = Map(type.InternalName);
			return !string.ReferenceEquals(remappedInternalName, null) ? JType.GetObjectType(remappedInternalName) : type;
		  case JType.Method:
			return JType.GetMethodType(MapMethodDesc(type.Descriptor));
		  default:
			return type;
		}
	  }

	  /// <summary>
	  /// Returns the given internal name, remapped with <seealso cref="Map"/>.
	  /// </summary>
	  /// <param name="internalName"> the internal name (or array type descriptor) of some (array) class. </param>
	  /// <returns> the given internal name, remapped with <seealso cref="Map"/>. </returns>
	  public virtual string MapType(string internalName)
	  {
		if (string.ReferenceEquals(internalName, null))
		{
		  return null;
		}
		return MapType(JType.GetObjectType(internalName)).InternalName;
	  }

	  /// <summary>
	  /// Returns the given internal names, remapped with <seealso cref="Map"/>.
	  /// </summary>
	  /// <param name="internalNames"> the internal names (or array type descriptors) of some (array) classes. </param>
	  /// <returns> the given internal name, remapped with <seealso cref="Map"/>. </returns>
	  public virtual string[] MapTypes(string[] internalNames)
	  {
		string[] remappedInternalNames = null;
		for (int i = 0; i < internalNames.Length; ++i)
		{
		  string internalName = internalNames[i];
		  string remappedInternalName = MapType(internalName);
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
	  /// with <seealso cref="MapDesc"/>.
	  /// </summary>
	  /// <param name="methodDescriptor"> a method descriptor. </param>
	  /// <returns> the given method descriptor, with its argument and return type descriptors remapped
	  ///     with <seealso cref="MapDesc"/>. </returns>
	  public virtual string MapMethodDesc(string methodDescriptor)
	  {
		if ("()V".Equals(methodDescriptor))
		{
		  return methodDescriptor;
		}

		StringBuilder stringBuilder = new StringBuilder("(");
		foreach (JType argumentType in JType.GetArgumentTypes(methodDescriptor))
		{
		  stringBuilder.Append(MapType(argumentType).Descriptor);
		}
		JType returnType = JType.GetReturnType(methodDescriptor);
		if (returnType == JType.VoidType)
		{
		  stringBuilder.Append(")V");
		}
		else
		{
		  stringBuilder.Append(')').Append(MapType(returnType).Descriptor);
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
	  public virtual object MapValue(object value)
	  {
		if (value is Type)
		{
		  return MapType((JType) value);
		}
		if (value is Handle)
		{
		  Handle handle = (Handle) value;
		  return new Handle(handle.Tag, MapType(handle.Owner), MapMethodName(handle.Owner, handle.Name, handle.Desc), handle.Tag <= IOpcodes.H_Putstatic ? MapDesc(handle.Desc) : MapMethodDesc(handle.Desc), handle.Interface);
		}
		if (value is ConstantDynamic)
		{
		  ConstantDynamic constantDynamic = (ConstantDynamic) value;
		  int bootstrapMethodArgumentCount = constantDynamic.BootstrapMethodArgumentCount;
		  object[] remappedBootstrapMethodArguments = new object[bootstrapMethodArgumentCount];
		  for (int i = 0; i < bootstrapMethodArgumentCount; ++i)
		  {
			remappedBootstrapMethodArguments[i] = MapValue(constantDynamic.GetBootstrapMethodArgument(i));
		  }
		  string descriptor = constantDynamic.Descriptor;
		  return new ConstantDynamic(MapInvokeDynamicMethodName(constantDynamic.Name, descriptor), MapDesc(descriptor), (Handle) MapValue(constantDynamic.BootstrapMethod), remappedBootstrapMethodArguments);
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
	  ///     <seealso cref="CreateSignatureRemapper"/>. </returns>
	  public virtual string MapSignature(string signature, bool typeSignature)
	  {
		if (string.ReferenceEquals(signature, null))
		{
		  return null;
		}
		SignatureReader signatureReader = new SignatureReader(signature);
		SignatureWriter signatureWriter = new SignatureWriter();
		SignatureVisitor signatureRemapper = CreateSignatureRemapper(signatureWriter);
		if (typeSignature)
		{
		  signatureReader.AcceptType(signatureRemapper);
		}
		else
		{
		  signatureReader.Accept(signatureRemapper);
		}
		return signatureWriter.ToString();
	  }

	  /// <summary>
	  /// Constructs a new remapper for signatures. The default implementation of this method returns a
	  /// new <seealso cref="SignatureRemapper"/>.
	  /// </summary>
	  /// <param name="signatureVisitor"> the SignatureVisitor the remapper must delegate to. </param>
	  /// <returns> the newly created remapper. </returns>
	  /// @deprecated use <seealso cref="CreateSignatureRemapper"/> instead. 
	  [Obsolete("use <seealso cref=\"createSignatureRemapper\"/> instead.")]
	  public virtual SignatureVisitor CreateRemappingSignatureAdapter(SignatureVisitor signatureVisitor)
	  {
		return CreateSignatureRemapper(signatureVisitor);
	  }

	  /// <summary>
	  /// Constructs a new remapper for signatures. The default implementation of this method returns a
	  /// new <seealso cref="SignatureRemapper"/>.
	  /// </summary>
	  /// <param name="signatureVisitor"> the SignatureVisitor the remapper must delegate to. </param>
	  /// <returns> the newly created remapper. </returns>
	  public virtual SignatureVisitor CreateSignatureRemapper(SignatureVisitor signatureVisitor)
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
	  public virtual string MapAnnotationAttributeName(string descriptor, string name)
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
	  public virtual string MapInnerClassName(string name, string ownerName, string innerName)
	  {
		string remappedInnerName = this.MapType(name);
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
	  public virtual string MapMethodName(string owner, string name, string descriptor)
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
	  public virtual string MapInvokeDynamicMethodName(string name, string descriptor)
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
	  public virtual string MapRecordComponentName(string owner, string name, string descriptor)
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
	  public virtual string MapFieldName(string owner, string name, string descriptor)
	  {
		return name;
	  }

	  /// <summary>
	  /// Maps a package name to its new name. The default implementation of this method returns the
	  /// given name, unchanged. Subclasses can override.
	  /// </summary>
	  /// <param name="name"> the fully qualified name of the package (using dots). </param>
	  /// <returns> the new name of the package. </returns>
	  public virtual string MapPackageName(string name)
	  {
		return name;
	  }

	  /// <summary>
	  /// Maps a module name to its new name. The default implementation of this method returns the given
	  /// name, unchanged. Subclasses can override.
	  /// </summary>
	  /// <param name="name"> the fully qualified name (using dots) of a module. </param>
	  /// <returns> the new name of the module. </returns>
	  public virtual string MapModuleName(string name)
	  {
		return name;
	  }

	  /// <summary>
	  /// Maps the internal name of a class to its new name. The default implementation of this method
	  /// returns the given name, unchanged. Subclasses can override.
	  /// </summary>
	  /// <param name="internalName"> the internal name of a class. </param>
	  /// <returns> the new internal name. </returns>
	  public virtual string Map(string internalName)
	  {
		return internalName;
	  }
	}

}