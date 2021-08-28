using System;
using System.Collections.Generic;
using SignatureVisitor = ObjectWeb.Asm.Signature.SignatureVisitor;

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
	/// A <seealso cref="SignatureVisitor"/> that remaps types with a <seealso cref="Remapper"/>.
	/// 
	/// @author Eugene Kuleshov
	/// </summary>
	public class SignatureRemapper : Signature.SignatureVisitor
	{

	  private readonly SignatureVisitor _signatureVisitor;

	  private readonly Remapper _remapper;

	  private List<string> _classNames = new List<string>();

	  /// <summary>
	  /// Constructs a new <seealso cref="SignatureRemapper"/>. <i>Subclasses must not use this constructor</i>.
	  /// Instead, they must use the <seealso cref="SignatureRemapper(int,SignatureVisitor,Remapper)"/> version.
	  /// </summary>
	  /// <param name="signatureVisitor"> the signature visitor this remapper must delegate to. </param>
	  /// <param name="remapper"> the remapper to use to remap the types in the visited signature. </param>
	  public SignatureRemapper(SignatureVisitor signatureVisitor, Remapper remapper) : this(IOpcodes.Asm9, signatureVisitor, remapper)
	  {
	  }

	  /// <summary>
	  /// Constructs a new <seealso cref="SignatureRemapper"/>.
	  /// </summary>
	  /// <param name="api"> the ASM API version supported by this remapper. Must be one of the {@code
	  ///     ASM}<i>x</i> values in <seealso cref="IOpcodes"/>. </param>
	  /// <param name="signatureVisitor"> the signature visitor this remapper must delegate to. </param>
	  /// <param name="remapper"> the remapper to use to remap the types in the visited signature. </param>
	  public SignatureRemapper(int api, SignatureVisitor signatureVisitor, Remapper remapper) : base(api)
	  {
		this._signatureVisitor = signatureVisitor;
		this._remapper = remapper;
	  }

	  public override void VisitClassType(string name)
	  {
		_classNames.Add(name);
		_signatureVisitor.VisitClassType(_remapper.MapType(name));
	  }

	  public override void VisitInnerClassType(string name)
	  {
          var classNameIndex = _classNames.Count - 1;
          string outerClassName = _classNames[classNameIndex];_classNames.RemoveAt(classNameIndex);
		string className = outerClassName + '$' + name;
		_classNames.Add(className);
		string remappedOuter = _remapper.MapType(outerClassName) + '$';
		string remappedName = _remapper.MapType(className);
		int index = remappedName.StartsWith(remappedOuter, StringComparison.Ordinal) ? remappedOuter.Length : remappedName.LastIndexOf('$') + 1;
		_signatureVisitor.VisitInnerClassType(remappedName.Substring(index));
	  }

	  public override void VisitFormalTypeParameter(string name)
	  {
		_signatureVisitor.VisitFormalTypeParameter(name);
	  }

	  public override void VisitTypeVariable(string name)
	  {
		_signatureVisitor.VisitTypeVariable(name);
	  }

	  public override SignatureVisitor VisitArrayType()
	  {
		_signatureVisitor.VisitArrayType();
		return this;
	  }

	  public override void VisitBaseType(char descriptor)
	  {
		_signatureVisitor.VisitBaseType(descriptor);
	  }

	  public override SignatureVisitor VisitClassBound()
	  {
		_signatureVisitor.VisitClassBound();
		return this;
	  }

	  public override SignatureVisitor VisitExceptionType()
	  {
		_signatureVisitor.VisitExceptionType();
		return this;
	  }

	  public override SignatureVisitor VisitInterface()
	  {
		_signatureVisitor.VisitInterface();
		return this;
	  }

	  public override SignatureVisitor VisitInterfaceBound()
	  {
		_signatureVisitor.VisitInterfaceBound();
		return this;
	  }

	  public override SignatureVisitor VisitParameterType()
	  {
		_signatureVisitor.VisitParameterType();
		return this;
	  }

	  public override SignatureVisitor VisitReturnType()
	  {
		_signatureVisitor.VisitReturnType();
		return this;
	  }

	  public override SignatureVisitor VisitSuperclass()
	  {
		_signatureVisitor.VisitSuperclass();
		return this;
	  }

	  public override void VisitTypeArgument()
	  {
		_signatureVisitor.VisitTypeArgument();
	  }

	  public override SignatureVisitor VisitTypeArgument(char wildcard)
	  {
		_signatureVisitor.VisitTypeArgument(wildcard);
		return this;
	  }

	  public override void VisitEnd()
	  {
		_signatureVisitor.VisitEnd();
		_classNames.RemoveAt(_classNames.Count - 1);
	  }
	}

}