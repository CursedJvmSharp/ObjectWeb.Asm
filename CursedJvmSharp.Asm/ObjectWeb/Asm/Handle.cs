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
	  /// The kind of field or method designated by this Handle. Should be <seealso cref="IIOpcodes.H_Getfield/>,
	  /// <seealso cref="IIOpcodes.H_Getstatic/>, <seealso cref="IIOpcodes.H_Putfield/>, <seealso cref="IIOpcodes.H_Putstatic/>, {@link
	  /// Opcodes#H_INVOKEVIRTUAL}, <seealso cref="IIOpcodes.H_Invokestatic/>, <seealso cref="IIOpcodes.H_Invokespecial/>,
	  /// <seealso cref="IIOpcodes.H_Newinvokespecial/> or <seealso cref="IIOpcodes.H_Invokeinterface/>.
	  /// </summary>
	  private readonly int _tag;

	  /// <summary>
	  /// The internal name of the class that owns the field or method designated by this handle. </summary>
	  private readonly string _owner;

	  /// <summary>
	  /// The name of the field or method designated by this handle. </summary>
	  private readonly string _name;

	  /// <summary>
	  /// The descriptor of the field or method designated by this handle. </summary>
	  private readonly string _descriptor;

	  /// <summary>
	  /// Whether the owner is an interface or not. </summary>
	  private readonly bool _isInterface;

	  /// <summary>
	  /// Constructs a new field or method handle.
	  /// </summary>
	  /// <param name="tag"> the kind of field or method designated by this Handle. Must be {@link
	  ///     Opcodes#H_GETFIELD}, <seealso cref="IIOpcodes.H_Getstatic/>, <seealso cref="IIOpcodes.H_Putfield/>, {@link
	  ///     Opcodes#H_PUTSTATIC}, <seealso cref="IIOpcodes.H_Invokevirtual/>, <seealso cref="IIOpcodes.H_Invokestatic/>,
	  ///     <seealso cref="IIOpcodes.H_Invokespecial/>, <seealso cref="IIOpcodes.H_Newinvokespecial/> or {@link
	  ///     Opcodes#H_INVOKEINTERFACE}. </param>
	  /// <param name="owner"> the internal name of the class that owns the field or method designated by this
	  ///     handle. </param>
	  /// <param name="name"> the name of the field or method designated by this handle. </param>
	  /// <param name="descriptor"> the descriptor of the field or method designated by this handle. </param>
	  /// @deprecated this constructor has been superseded by {@link #Handle(int, String, String, String,
	  ///     boolean)}. 
	  [Obsolete("this constructor has been superseded by {@link #Handle(int, String, String, String,")]
	  public Handle(int tag, string owner, string name, string descriptor) : this(tag, owner, name, descriptor, tag == IOpcodes.H_Invokeinterface)
	  {
	  }

	  /// <summary>
	  /// Constructs a new field or method handle.
	  /// </summary>
	  /// <param name="tag"> the kind of field or method designated by this Handle. Must be {@link
	  ///     Opcodes#H_GETFIELD}, <seealso cref="IIOpcodes.H_Getstatic/>, <seealso cref="IIOpcodes.H_Putfield/>, {@link
	  ///     Opcodes#H_PUTSTATIC}, <seealso cref="IIOpcodes.H_Invokevirtual/>, <seealso cref="IIOpcodes.H_Invokestatic/>,
	  ///     <seealso cref="IIOpcodes.H_Invokespecial/>, <seealso cref="IIOpcodes.H_Newinvokespecial/> or {@link
	  ///     Opcodes#H_INVOKEINTERFACE}. </param>
	  /// <param name="owner"> the internal name of the class that owns the field or method designated by this
	  ///     handle. </param>
	  /// <param name="name"> the name of the field or method designated by this handle. </param>
	  /// <param name="descriptor"> the descriptor of the field or method designated by this handle. </param>
	  /// <param name="isInterface"> whether the owner is an interface or not. </param>
	  public Handle(int tag, string owner, string name, string descriptor, bool isInterface)
	  {
		this._tag = tag;
		this._owner = owner;
		this._name = name;
		this._descriptor = descriptor;
		this._isInterface = isInterface;
	  }

	  /// <summary>
	  /// Returns the kind of field or method designated by this handle.
	  /// </summary>
	  /// <returns> <seealso cref="IIOpcodes.H_Getfield/>, <seealso cref="IIOpcodes.H_Getstatic/>, <seealso cref="IIOpcodes.H_Putfield/>,
	  ///     <seealso cref="IIOpcodes.H_Putstatic/>, <seealso cref="IIOpcodes.H_Invokevirtual/>, {@link
	  ///     Opcodes#H_INVOKESTATIC}, <seealso cref="IIOpcodes.H_Invokespecial/>, {@link
	  ///     Opcodes#H_NEWINVOKESPECIAL} or <seealso cref="IIOpcodes.H_Invokeinterface/>. </returns>
	  public int Tag => _tag;

      /// <summary>
	  /// Returns the internal name of the class that owns the field or method designated by this handle.
	  /// </summary>
	  /// <returns> the internal name of the class that owns the field or method designated by this handle. </returns>
	  public string Owner => _owner;

      /// <summary>
	  /// Returns the name of the field or method designated by this handle.
	  /// </summary>
	  /// <returns> the name of the field or method designated by this handle. </returns>
	  public string Name => _name;

      /// <summary>
	  /// Returns the descriptor of the field or method designated by this handle.
	  /// </summary>
	  /// <returns> the descriptor of the field or method designated by this handle. </returns>
	  public string Desc => _descriptor;

      /// <summary>
	  /// Returns true if the owner of the field or method designated by this handle is an interface.
	  /// </summary>
	  /// <returns> true if the owner of the field or method designated by this handle is an interface. </returns>
	  public bool Interface => _isInterface;

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
		return _tag == handle._tag && _isInterface == handle._isInterface && _owner.Equals(handle._owner) && _name.Equals(handle._name) && _descriptor.Equals(handle._descriptor);
	  }

	  public override int GetHashCode()
	  {
		return _tag + (_isInterface ? 64 : 0) + _owner.GetHashCode() * _name.GetHashCode() * _descriptor.GetHashCode();
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
		return _owner + '.' + _name + _descriptor + " (" + _tag + (_isInterface ? " itf" : "") + ')';
	  }
	}

}