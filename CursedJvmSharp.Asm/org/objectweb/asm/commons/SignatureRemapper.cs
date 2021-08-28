using org.objectweb.asm;
using CursedJvmSharp.Asm.Java.IO;
using System;
using System.Collections.Generic;
using Opcodes = org.objectweb.asm.Opcodes;
using SignatureVisitor = org.objectweb.asm.signature.SignatureVisitor;

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
	/// A <seealso cref="SignatureVisitor"/> that remaps types with a <seealso cref="Remapper"/>.
	/// 
	/// @author Eugene Kuleshov
	/// </summary>
	public class SignatureRemapper : SignatureVisitor
	{

	  private readonly SignatureVisitor signatureVisitor;

	  private readonly Remapper remapper;

	  private List<string> classNames = new List<string>();

	  /// <summary>
	  /// Constructs a new <seealso cref="SignatureRemapper"/>. <i>Subclasses must not use this constructor</i>.
	  /// Instead, they must use the <seealso cref="SignatureRemapper(int,SignatureVisitor,Remapper)"/> version.
	  /// </summary>
	  /// <param name="signatureVisitor"> the signature visitor this remapper must delegate to. </param>
	  /// <param name="remapper"> the remapper to use to remap the types in the visited signature. </param>
	  public SignatureRemapper(SignatureVisitor signatureVisitor, Remapper remapper) : this(Opcodes.ASM9, signatureVisitor, remapper)
	  {
	  }

	  /// <summary>
	  /// Constructs a new <seealso cref="SignatureRemapper"/>.
	  /// </summary>
	  /// <param name="api"> the ASM API version supported by this remapper. Must be one of the {@code
	  ///     ASM}<i>x</i> values in <seealso cref="Opcodes"/>. </param>
	  /// <param name="signatureVisitor"> the signature visitor this remapper must delegate to. </param>
	  /// <param name="remapper"> the remapper to use to remap the types in the visited signature. </param>
	  public SignatureRemapper(int api, SignatureVisitor signatureVisitor, Remapper remapper) : base(api)
	  {
		this.signatureVisitor = signatureVisitor;
		this.remapper = remapper;
	  }

	  public override void visitClassType(string name)
	  {
		classNames.Add(name);
		signatureVisitor.visitClassType(remapper.mapType(name));
	  }

	  public override void visitInnerClassType(string name)
	  {
          var classNameIndex = classNames.Count - 1;
          string outerClassName = classNames[classNameIndex];classNames.RemoveAt(classNameIndex);
		string className = outerClassName + '$' + name;
		classNames.Add(className);
		string remappedOuter = remapper.mapType(outerClassName) + '$';
		string remappedName = remapper.mapType(className);
		int index = remappedName.StartsWith(remappedOuter, StringComparison.Ordinal) ? remappedOuter.Length : remappedName.LastIndexOf('$') + 1;
		signatureVisitor.visitInnerClassType(remappedName.Substring(index));
	  }

	  public override void visitFormalTypeParameter(string name)
	  {
		signatureVisitor.visitFormalTypeParameter(name);
	  }

	  public override void visitTypeVariable(string name)
	  {
		signatureVisitor.visitTypeVariable(name);
	  }

	  public override SignatureVisitor visitArrayType()
	  {
		signatureVisitor.visitArrayType();
		return this;
	  }

	  public override void visitBaseType(char descriptor)
	  {
		signatureVisitor.visitBaseType(descriptor);
	  }

	  public override SignatureVisitor visitClassBound()
	  {
		signatureVisitor.visitClassBound();
		return this;
	  }

	  public override SignatureVisitor visitExceptionType()
	  {
		signatureVisitor.visitExceptionType();
		return this;
	  }

	  public override SignatureVisitor visitInterface()
	  {
		signatureVisitor.visitInterface();
		return this;
	  }

	  public override SignatureVisitor visitInterfaceBound()
	  {
		signatureVisitor.visitInterfaceBound();
		return this;
	  }

	  public override SignatureVisitor visitParameterType()
	  {
		signatureVisitor.visitParameterType();
		return this;
	  }

	  public override SignatureVisitor visitReturnType()
	  {
		signatureVisitor.visitReturnType();
		return this;
	  }

	  public override SignatureVisitor visitSuperclass()
	  {
		signatureVisitor.visitSuperclass();
		return this;
	  }

	  public override void visitTypeArgument()
	  {
		signatureVisitor.visitTypeArgument();
	  }

	  public override SignatureVisitor visitTypeArgument(char wildcard)
	  {
		signatureVisitor.visitTypeArgument(wildcard);
		return this;
	  }

	  public override void visitEnd()
	  {
		signatureVisitor.visitEnd();
		classNames.RemoveAt(classNames.Count - 1);
	  }
	}

}