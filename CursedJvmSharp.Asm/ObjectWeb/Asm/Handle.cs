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
	/// A reference to a field or a method.
	/// 
	/// @author Remi Forax
	/// @author Eric Bruneton
	/// </summary>
	public sealed class Handle
	{

	  /// <summary>
	  /// The kind of field or method designated by this Handle. Should be <seealso cref="Opcodes.H_GETFIELD"/>,
	  /// <seealso cref="Opcodes.H_GETSTATIC"/>, <seealso cref="Opcodes.H_PUTFIELD"/>, <seealso cref="Opcodes.H_PUTSTATIC"/>, {@link
	  /// Opcodes#H_INVOKEVIRTUAL}, <seealso cref="Opcodes.H_INVOKESTATIC"/>, <seealso cref="Opcodes.H_INVOKESPECIAL"/>,
	  /// <seealso cref="Opcodes.H_NEWINVOKESPECIAL"/> or <seealso cref="Opcodes.H_INVOKEINTERFACE"/>.
	  /// </summary>
	  private readonly int tag;

	  /// <summary>
	  /// The internal name of the class that owns the field or method designated by this handle. </summary>
	  private readonly string owner;

	  /// <summary>
	  /// The name of the field or method designated by this handle. </summary>
	  private readonly string name;

	  /// <summary>
	  /// The descriptor of the field or method designated by this handle. </summary>
	  private readonly string descriptor;

	  /// <summary>
	  /// Whether the owner is an interface or not. </summary>
	  private readonly bool isInterface;

	  /// <summary>
	  /// Constructs a new field or method handle.
	  /// </summary>
	  /// <param name="tag"> the kind of field or method designated by this Handle. Must be {@link
	  ///     Opcodes#H_GETFIELD}, <seealso cref="Opcodes.H_GETSTATIC"/>, <seealso cref="Opcodes.H_PUTFIELD"/>, {@link
	  ///     Opcodes#H_PUTSTATIC}, <seealso cref="Opcodes.H_INVOKEVIRTUAL"/>, <seealso cref="Opcodes.H_INVOKESTATIC"/>,
	  ///     <seealso cref="Opcodes.H_INVOKESPECIAL"/>, <seealso cref="Opcodes.H_NEWINVOKESPECIAL"/> or {@link
	  ///     Opcodes#H_INVOKEINTERFACE}. </param>
	  /// <param name="owner"> the internal name of the class that owns the field or method designated by this
	  ///     handle. </param>
	  /// <param name="name"> the name of the field or method designated by this handle. </param>
	  /// <param name="descriptor"> the descriptor of the field or method designated by this handle. </param>
	  /// @deprecated this constructor has been superseded by {@link #Handle(int, String, String, String,
	  ///     boolean)}. 
	  [Obsolete("this constructor has been superseded by {@link #Handle(int, String, String, String,")]
	  public Handle(int tag, string owner, string name, string descriptor) : this(tag, owner, name, descriptor, tag == Opcodes.H_INVOKEINTERFACE)
	  {
	  }

	  /// <summary>
	  /// Constructs a new field or method handle.
	  /// </summary>
	  /// <param name="tag"> the kind of field or method designated by this Handle. Must be {@link
	  ///     Opcodes#H_GETFIELD}, <seealso cref="Opcodes.H_GETSTATIC"/>, <seealso cref="Opcodes.H_PUTFIELD"/>, {@link
	  ///     Opcodes#H_PUTSTATIC}, <seealso cref="Opcodes.H_INVOKEVIRTUAL"/>, <seealso cref="Opcodes.H_INVOKESTATIC"/>,
	  ///     <seealso cref="Opcodes.H_INVOKESPECIAL"/>, <seealso cref="Opcodes.H_NEWINVOKESPECIAL"/> or {@link
	  ///     Opcodes#H_INVOKEINTERFACE}. </param>
	  /// <param name="owner"> the internal name of the class that owns the field or method designated by this
	  ///     handle. </param>
	  /// <param name="name"> the name of the field or method designated by this handle. </param>
	  /// <param name="descriptor"> the descriptor of the field or method designated by this handle. </param>
	  /// <param name="isInterface"> whether the owner is an interface or not. </param>
	  public Handle(int tag, string owner, string name, string descriptor, bool isInterface)
	  {
		this.tag = tag;
		this.owner = owner;
		this.name = name;
		this.descriptor = descriptor;
		this.isInterface = isInterface;
	  }

	  /// <summary>
	  /// Returns the kind of field or method designated by this handle.
	  /// </summary>
	  /// <returns> <seealso cref="Opcodes.H_GETFIELD"/>, <seealso cref="Opcodes.H_GETSTATIC"/>, <seealso cref="Opcodes.H_PUTFIELD"/>,
	  ///     <seealso cref="Opcodes.H_PUTSTATIC"/>, <seealso cref="Opcodes.H_INVOKEVIRTUAL"/>, {@link
	  ///     Opcodes#H_INVOKESTATIC}, <seealso cref="Opcodes.H_INVOKESPECIAL"/>, {@link
	  ///     Opcodes#H_NEWINVOKESPECIAL} or <seealso cref="Opcodes.H_INVOKEINTERFACE"/>. </returns>
	  public int Tag
	  {
		  get
		  {
			return tag;
		  }
	  }

	  /// <summary>
	  /// Returns the internal name of the class that owns the field or method designated by this handle.
	  /// </summary>
	  /// <returns> the internal name of the class that owns the field or method designated by this handle. </returns>
	  public string Owner
	  {
		  get
		  {
			return owner;
		  }
	  }

	  /// <summary>
	  /// Returns the name of the field or method designated by this handle.
	  /// </summary>
	  /// <returns> the name of the field or method designated by this handle. </returns>
	  public string Name
	  {
		  get
		  {
			return name;
		  }
	  }

	  /// <summary>
	  /// Returns the descriptor of the field or method designated by this handle.
	  /// </summary>
	  /// <returns> the descriptor of the field or method designated by this handle. </returns>
	  public string Desc
	  {
		  get
		  {
			return descriptor;
		  }
	  }

	  /// <summary>
	  /// Returns true if the owner of the field or method designated by this handle is an interface.
	  /// </summary>
	  /// <returns> true if the owner of the field or method designated by this handle is an interface. </returns>
	  public bool Interface
	  {
		  get
		  {
			return isInterface;
		  }
	  }

	  public override bool Equals(object @object)
	  {
		if (@object == this)
		{
		  return true;
		}
		if (!(@object is Handle))
		{
		  return false;
		}
		Handle handle = (Handle) @object;
		return tag == handle.tag && isInterface == handle.isInterface && owner.Equals(handle.owner) && name.Equals(handle.name) && descriptor.Equals(handle.descriptor);
	  }

	  public override int GetHashCode()
	  {
		return tag + (isInterface ? 64 : 0) + owner.GetHashCode() * name.GetHashCode() * descriptor.GetHashCode();
	  }

	  /// <summary>
	  /// Returns the textual representation of this handle. The textual representation is:
	  /// 
	  /// <ul>
	  ///   <li>for a reference to a class: owner "." name descriptor " (" tag ")",
	  ///   <li>for a reference to an interface: owner "." name descriptor " (" tag " itf)".
	  /// </ul>
	  /// </summary>
	  public override string ToString()
	  {
		return owner + '.' + name + descriptor + " (" + tag + (isInterface ? " itf" : "") + ')';
	  }
	}

}