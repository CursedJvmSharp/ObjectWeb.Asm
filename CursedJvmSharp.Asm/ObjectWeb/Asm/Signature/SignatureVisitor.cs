

// ASM: a very small and fast Java bytecode manipulation framework
// Copyright (c) 2000-2011 INRIA, France Telecom
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
// 1. Redistributions of source code must retain the above copyright
// notice, this list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright
// notice, this list of conditions and the following disclaimer in the
// documentation and/or other materials provided with the distribution.
// 3. Neither the name of the copyright holders nor the names of its
// contributors may be used to endorse or promote products derived from
// this software without specific prior written permission.
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
namespace ObjectWeb.Asm.Signature
{

	/// <summary>
	/// A visitor to visit a generic signature. The methods of this interface must be called in one of
	/// the three following orders (the last one is the only valid order for a <seealso cref="SignatureVisitor"/>
	/// that is returned by a method of this interface):
	/// 
	/// <ul>
	///   <li><i>ClassSignature</i> = ( {@code visitFormalTypeParameter} {@code visitClassBound}? {@code
	///       visitInterfaceBound}* )* ({@code visitSuperclass} {@code visitInterface}* )
	///   <li><i>MethodSignature</i> = ( {@code visitFormalTypeParameter} {@code visitClassBound}? {@code
	///       visitInterfaceBound}* )* ({@code visitParameterType}* {@code visitReturnType} {@code
	///       visitExceptionType}* )
	///   <li><i>TypeSignature</i> = {@code visitBaseType} | {@code visitTypeVariable} | {@code
	///       visitArrayType} | ( {@code visitClassType} {@code visitTypeArgument}* ( {@code
	///       visitInnerClassType} {@code visitTypeArgument}* )* {@code visitEnd} ) )
	/// </ul>
	/// 
	/// @author Thomas Hallgren
	/// @author Eric Bruneton
	/// </summary>
	public abstract class SignatureVisitor
	{

	  /// <summary>
	  /// Wildcard for an "extends" type argument. </summary>
	  public const char EXTENDS = '+';

	  /// <summary>
	  /// Wildcard for a "super" type argument. </summary>
	  public const char SUPER = '-';

	  /// <summary>
	  /// Wildcard for a normal type argument. </summary>
	  public const char INSTANCEOF = '=';

	  /// <summary>
	  /// The ASM API version implemented by this visitor. The value of this field must be one of the
	  /// {@code ASM}<i>x</i> values in <seealso cref="Opcodes"/>.
	  /// </summary>
	  protected internal readonly int api;

	  /// <summary>
	  /// Constructs a new <seealso cref="SignatureVisitor"/>.
	  /// </summary>
	  /// <param name="api"> the ASM API version implemented by this visitor. Must be one of the {@code
	  ///     ASM}<i>x</i> values in <seealso cref="Opcodes"/>. </param>
	  public SignatureVisitor(int api)
	  {
		if (api != Opcodes.ASM9 && api != Opcodes.ASM8 && api != Opcodes.ASM7 && api != Opcodes.ASM6 && api != Opcodes.ASM5 && api != Opcodes.ASM4 && api != Opcodes.ASM10_EXPERIMENTAL)
		{
		  throw new System.ArgumentException("Unsupported api " + api);
		}
		this.api = api;
	  }

	  /// <summary>
	  /// Visits a formal type parameter.
	  /// </summary>
	  /// <param name="name"> the name of the formal parameter. </param>
	  public virtual void visitFormalTypeParameter(string name)
	  {
	  }

	  /// <summary>
	  /// Visits the class bound of the last visited formal type parameter.
	  /// </summary>
	  /// <returns> a non null visitor to visit the signature of the class bound. </returns>
	  public virtual SignatureVisitor visitClassBound()
	  {
		return this;
	  }

	  /// <summary>
	  /// Visits an interface bound of the last visited formal type parameter.
	  /// </summary>
	  /// <returns> a non null visitor to visit the signature of the interface bound. </returns>
	  public virtual SignatureVisitor visitInterfaceBound()
	  {
		return this;
	  }

	  /// <summary>
	  /// Visits the type of the super class.
	  /// </summary>
	  /// <returns> a non null visitor to visit the signature of the super class type. </returns>
	  public virtual SignatureVisitor visitSuperclass()
	  {
		return this;
	  }

	  /// <summary>
	  /// Visits the type of an interface implemented by the class.
	  /// </summary>
	  /// <returns> a non null visitor to visit the signature of the interface type. </returns>
	  public virtual SignatureVisitor visitInterface()
	  {
		return this;
	  }

	  /// <summary>
	  /// Visits the type of a method parameter.
	  /// </summary>
	  /// <returns> a non null visitor to visit the signature of the parameter type. </returns>
	  public virtual SignatureVisitor visitParameterType()
	  {
		return this;
	  }

	  /// <summary>
	  /// Visits the return type of the method.
	  /// </summary>
	  /// <returns> a non null visitor to visit the signature of the return type. </returns>
	  public virtual SignatureVisitor visitReturnType()
	  {
		return this;
	  }

	  /// <summary>
	  /// Visits the type of a method exception.
	  /// </summary>
	  /// <returns> a non null visitor to visit the signature of the exception type. </returns>
	  public virtual SignatureVisitor visitExceptionType()
	  {
		return this;
	  }

	  /// <summary>
	  /// Visits a signature corresponding to a primitive type.
	  /// </summary>
	  /// <param name="descriptor"> the descriptor of the primitive type, or 'V' for {@code void} . </param>
	  public virtual void visitBaseType(char descriptor)
	  {
	  }

	  /// <summary>
	  /// Visits a signature corresponding to a type variable.
	  /// </summary>
	  /// <param name="name"> the name of the type variable. </param>
	  public virtual void visitTypeVariable(string name)
	  {
	  }

	  /// <summary>
	  /// Visits a signature corresponding to an array type.
	  /// </summary>
	  /// <returns> a non null visitor to visit the signature of the array element type. </returns>
	  public virtual SignatureVisitor visitArrayType()
	  {
		return this;
	  }

	  /// <summary>
	  /// Starts the visit of a signature corresponding to a class or interface type.
	  /// </summary>
	  /// <param name="name"> the internal name of the class or interface. </param>
	  public virtual void visitClassType(string name)
	  {
	  }

	  /// <summary>
	  /// Visits an inner class.
	  /// </summary>
	  /// <param name="name"> the local name of the inner class in its enclosing class. </param>
	  public virtual void visitInnerClassType(string name)
	  {
	  }

	  /// <summary>
	  /// Visits an unbounded type argument of the last visited class or inner class type. </summary>
	  public virtual void visitTypeArgument()
	  {
	  }

	  /// <summary>
	  /// Visits a type argument of the last visited class or inner class type.
	  /// </summary>
	  /// <param name="wildcard"> '+', '-' or '='. </param>
	  /// <returns> a non null visitor to visit the signature of the type argument. </returns>
	  public virtual SignatureVisitor visitTypeArgument(char wildcard)
	  {
		return this;
	  }

	  /// <summary>
	  /// Ends the visit of a signature corresponding to a class or interface type. </summary>
	  public virtual void visitEnd()
	  {
	  }
	}

}