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
	/// A reference to a type appearing in a class, field or method declaration, or on an instruction.
	/// Such a reference designates the part of the class where the referenced type is appearing (e.g. an
	/// 'extends', 'implements' or 'throws' clause, a 'new' instruction, a 'catch' clause, a type cast, a
	/// local variable declaration, etc).
	/// 
	/// @author Eric Bruneton
	/// </summary>
	public class TypeReference
	{

	  /// <summary>
	  /// The sort of type references that target a type parameter of a generic class. See {@link
	  /// #getSort}.
	  /// </summary>
	  public const int CLASS_TYPE_PARAMETER = 0x00;

	  /// <summary>
	  /// The sort of type references that target a type parameter of a generic method. See {@link
	  /// #getSort}.
	  /// </summary>
	  public const int METHOD_TYPE_PARAMETER = 0x01;

	  /// <summary>
	  /// The sort of type references that target the super class of a class or one of the interfaces it
	  /// implements. See <seealso cref="getSort"/>.
	  /// </summary>
	  public const int CLASS_EXTENDS = 0x10;

	  /// <summary>
	  /// The sort of type references that target a bound of a type parameter of a generic class. See
	  /// <seealso cref="getSort"/>.
	  /// </summary>
	  public const int CLASS_TYPE_PARAMETER_BOUND = 0x11;

	  /// <summary>
	  /// The sort of type references that target a bound of a type parameter of a generic method. See
	  /// <seealso cref="getSort"/>.
	  /// </summary>
	  public const int METHOD_TYPE_PARAMETER_BOUND = 0x12;

	  /// <summary>
	  /// The sort of type references that target the type of a field. See <seealso cref="getSort"/>. </summary>
	  public const int FIELD = 0x13;

	  /// <summary>
	  /// The sort of type references that target the return type of a method. See <seealso cref="getSort"/>. </summary>
	  public const int METHOD_RETURN = 0x14;

	  /// <summary>
	  /// The sort of type references that target the receiver type of a method. See <seealso cref="getSort"/>.
	  /// </summary>
	  public const int METHOD_RECEIVER = 0x15;

	  /// <summary>
	  /// The sort of type references that target the type of a formal parameter of a method. See {@link
	  /// #getSort}.
	  /// </summary>
	  public const int METHOD_FORMAL_PARAMETER = 0x16;

	  /// <summary>
	  /// The sort of type references that target the type of an exception declared in the throws clause
	  /// of a method. See <seealso cref="getSort"/>.
	  /// </summary>
	  public const int THROWS = 0x17;

	  /// <summary>
	  /// The sort of type references that target the type of a local variable in a method. See {@link
	  /// #getSort}.
	  /// </summary>
	  public const int LOCAL_VARIABLE = 0x40;

	  /// <summary>
	  /// The sort of type references that target the type of a resource variable in a method. See {@link
	  /// #getSort}.
	  /// </summary>
	  public const int RESOURCE_VARIABLE = 0x41;

	  /// <summary>
	  /// The sort of type references that target the type of the exception of a 'catch' clause in a
	  /// method. See <seealso cref="getSort"/>.
	  /// </summary>
	  public const int EXCEPTION_PARAMETER = 0x42;

	  /// <summary>
	  /// The sort of type references that target the type declared in an 'instanceof' instruction. See
	  /// <seealso cref="getSort"/>.
	  /// </summary>
	  public const int INSTANCEOF = 0x43;

	  /// <summary>
	  /// The sort of type references that target the type of the object created by a 'new' instruction.
	  /// See <seealso cref="getSort"/>.
	  /// </summary>
	  public const int NEW = 0x44;

	  /// <summary>
	  /// The sort of type references that target the receiver type of a constructor reference. See
	  /// <seealso cref="getSort"/>.
	  /// </summary>
	  public const int CONSTRUCTOR_REFERENCE = 0x45;

	  /// <summary>
	  /// The sort of type references that target the receiver type of a method reference. See {@link
	  /// #getSort}.
	  /// </summary>
	  public const int METHOD_REFERENCE = 0x46;

	  /// <summary>
	  /// The sort of type references that target the type declared in an explicit or implicit cast
	  /// instruction. See <seealso cref="getSort"/>.
	  /// </summary>
	  public const int CAST = 0x47;

	  /// <summary>
	  /// The sort of type references that target a type parameter of a generic constructor in a
	  /// constructor call. See <seealso cref="getSort"/>.
	  /// </summary>
	  public const int CONSTRUCTOR_INVOCATION_TYPE_ARGUMENT = 0x48;

	  /// <summary>
	  /// The sort of type references that target a type parameter of a generic method in a method call.
	  /// See <seealso cref="getSort"/>.
	  /// </summary>
	  public const int METHOD_INVOCATION_TYPE_ARGUMENT = 0x49;

	  /// <summary>
	  /// The sort of type references that target a type parameter of a generic constructor in a
	  /// constructor reference. See <seealso cref="getSort"/>.
	  /// </summary>
	  public const int CONSTRUCTOR_REFERENCE_TYPE_ARGUMENT = 0x4A;

	  /// <summary>
	  /// The sort of type references that target a type parameter of a generic method in a method
	  /// reference. See <seealso cref="getSort"/>.
	  /// </summary>
	  public const int METHOD_REFERENCE_TYPE_ARGUMENT = 0x4B;

	  /// <summary>
	  /// The target_type and target_info structures - as defined in the Java Virtual Machine
	  /// Specification (JVMS) - corresponding to this type reference. target_type uses one byte, and all
	  /// the target_info union fields use up to 3 bytes (except localvar_target, handled with the
	  /// specific method <seealso cref="MethodVisitor.visitLocalVariableAnnotation"/>). Thus, both structures can
	  /// be stored in an int.
	  /// 
	  /// <para>This int field stores target_type (called the TypeReference 'sort' in the public API of this
	  /// class) in its most significant byte, followed by the target_info fields. Depending on
	  /// target_type, 1, 2 or even 3 least significant bytes of this field are unused. target_info
	  /// fields which reference bytecode offsets are set to 0 (these offsets are ignored in ClassReader,
	  /// and recomputed in MethodWriter).
	  /// 
	  /// </para>
	  /// </summary>
	  /// <seealso cref= <a href="https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.7.20">JVMS
	  ///     4.7.20</a> </seealso>
	  /// <seealso cref= <a
	  ///     href="https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.7.20.1">JVMS
	  ///     4.7.20.1</a> </seealso>
	  private readonly int targetTypeAndInfo;

	  /// <summary>
	  /// Constructs a new TypeReference.
	  /// </summary>
	  /// <param name="typeRef"> the int encoded value of the type reference, as received in a visit method
	  ///     related to type annotations, such as <seealso cref="ClassVisitor.visitTypeAnnotation"/>. </param>
	  public TypeReference(int typeRef)
	  {
		this.targetTypeAndInfo = typeRef;
	  }

	  /// <summary>
	  /// Returns a type reference of the given sort.
	  /// </summary>
	  /// <param name="sort"> one of <seealso cref="FIELD"/>, <seealso cref="METHOD_RETURN"/>, <seealso cref="METHOD_RECEIVER"/>, {@link
	  ///     #LOCAL_VARIABLE}, <seealso cref="RESOURCE_VARIABLE"/>, <seealso cref="INSTANCEOF"/>, <seealso cref="NEW"/>, {@link
	  ///     #CONSTRUCTOR_REFERENCE}, or <seealso cref="METHOD_REFERENCE"/>. </param>
	  /// <returns> a type reference of the given sort. </returns>
	  public static TypeReference newTypeReference(int sort)
	  {
		return new TypeReference(sort << 24);
	  }

	  /// <summary>
	  /// Returns a reference to a type parameter of a generic class or method.
	  /// </summary>
	  /// <param name="sort"> one of <seealso cref="CLASS_TYPE_PARAMETER"/> or <seealso cref="METHOD_TYPE_PARAMETER"/>. </param>
	  /// <param name="paramIndex"> the type parameter index. </param>
	  /// <returns> a reference to the given generic class or method type parameter. </returns>
	  public static TypeReference newTypeParameterReference(int sort, int paramIndex)
	  {
		return new TypeReference((sort << 24) | (paramIndex << 16));
	  }

	  /// <summary>
	  /// Returns a reference to a type parameter bound of a generic class or method.
	  /// </summary>
	  /// <param name="sort"> one of <seealso cref="CLASS_TYPE_PARAMETER"/> or <seealso cref="METHOD_TYPE_PARAMETER"/>. </param>
	  /// <param name="paramIndex"> the type parameter index. </param>
	  /// <param name="boundIndex"> the type bound index within the above type parameters. </param>
	  /// <returns> a reference to the given generic class or method type parameter bound. </returns>
	  public static TypeReference newTypeParameterBoundReference(int sort, int paramIndex, int boundIndex)
	  {
		return new TypeReference((sort << 24) | (paramIndex << 16) | (boundIndex << 8));
	  }

	  /// <summary>
	  /// Returns a reference to the super class or to an interface of the 'implements' clause of a
	  /// class.
	  /// </summary>
	  /// <param name="itfIndex"> the index of an interface in the 'implements' clause of a class, or -1 to
	  ///     reference the super class of the class. </param>
	  /// <returns> a reference to the given super type of a class. </returns>
	  public static TypeReference newSuperTypeReference(int itfIndex)
	  {
		return new TypeReference((CLASS_EXTENDS << 24) | ((itfIndex & 0xFFFF) << 8));
	  }

	  /// <summary>
	  /// Returns a reference to the type of a formal parameter of a method.
	  /// </summary>
	  /// <param name="paramIndex"> the formal parameter index. </param>
	  /// <returns> a reference to the type of the given method formal parameter. </returns>
	  public static TypeReference newFormalParameterReference(int paramIndex)
	  {
		return new TypeReference((METHOD_FORMAL_PARAMETER << 24) | (paramIndex << 16));
	  }

	  /// <summary>
	  /// Returns a reference to the type of an exception, in a 'throws' clause of a method.
	  /// </summary>
	  /// <param name="exceptionIndex"> the index of an exception in a 'throws' clause of a method. </param>
	  /// <returns> a reference to the type of the given exception. </returns>
	  public static TypeReference newExceptionReference(int exceptionIndex)
	  {
		return new TypeReference((THROWS << 24) | (exceptionIndex << 8));
	  }

	  /// <summary>
	  /// Returns a reference to the type of the exception declared in a 'catch' clause of a method.
	  /// </summary>
	  /// <param name="tryCatchBlockIndex"> the index of a try catch block (using the order in which they are
	  ///     visited with visitTryCatchBlock). </param>
	  /// <returns> a reference to the type of the given exception. </returns>
	  public static TypeReference newTryCatchReference(int tryCatchBlockIndex)
	  {
		return new TypeReference((EXCEPTION_PARAMETER << 24) | (tryCatchBlockIndex << 8));
	  }

	  /// <summary>
	  /// Returns a reference to the type of a type argument in a constructor or method call or
	  /// reference.
	  /// </summary>
	  /// <param name="sort"> one of <seealso cref="CAST"/>, <seealso cref="CONSTRUCTOR_INVOCATION_TYPE_ARGUMENT"/>, {@link
	  ///     #METHOD_INVOCATION_TYPE_ARGUMENT}, <seealso cref="CONSTRUCTOR_REFERENCE_TYPE_ARGUMENT"/>, or {@link
	  ///     #METHOD_REFERENCE_TYPE_ARGUMENT}. </param>
	  /// <param name="argIndex"> the type argument index. </param>
	  /// <returns> a reference to the type of the given type argument. </returns>
	  public static TypeReference newTypeArgumentReference(int sort, int argIndex)
	  {
		return new TypeReference((sort << 24) | argIndex);
	  }

	  /// <summary>
	  /// Returns the sort of this type reference.
	  /// </summary>
	  /// <returns> one of <seealso cref="CLASS_TYPE_PARAMETER"/>, <seealso cref="METHOD_TYPE_PARAMETER"/>, {@link
	  ///     #CLASS_EXTENDS}, <seealso cref="CLASS_TYPE_PARAMETER_BOUND"/>, <seealso cref="METHOD_TYPE_PARAMETER_BOUND"/>,
	  ///     <seealso cref="FIELD"/>, <seealso cref="METHOD_RETURN"/>, <seealso cref="METHOD_RECEIVER"/>, {@link
	  ///     #METHOD_FORMAL_PARAMETER}, <seealso cref="THROWS"/>, <seealso cref="LOCAL_VARIABLE"/>, {@link
	  ///     #RESOURCE_VARIABLE}, <seealso cref="EXCEPTION_PARAMETER"/>, <seealso cref="INSTANCEOF"/>, <seealso cref="NEW"/>,
	  ///     <seealso cref="CONSTRUCTOR_REFERENCE"/>, <seealso cref="METHOD_REFERENCE"/>, <seealso cref="CAST"/>, {@link
	  ///     #CONSTRUCTOR_INVOCATION_TYPE_ARGUMENT}, <seealso cref="METHOD_INVOCATION_TYPE_ARGUMENT"/>, {@link
	  ///     #CONSTRUCTOR_REFERENCE_TYPE_ARGUMENT}, or <seealso cref="METHOD_REFERENCE_TYPE_ARGUMENT"/>. </returns>
	  public virtual int Sort
	  {
		  get
		  {
			return (int)((uint)targetTypeAndInfo >> 24);
		  }
	  }

	  /// <summary>
	  /// Returns the index of the type parameter referenced by this type reference. This method must
	  /// only be used for type references whose sort is <seealso cref="CLASS_TYPE_PARAMETER"/>, {@link
	  /// #METHOD_TYPE_PARAMETER}, <seealso cref="CLASS_TYPE_PARAMETER_BOUND"/> or {@link
	  /// #METHOD_TYPE_PARAMETER_BOUND}.
	  /// </summary>
	  /// <returns> a type parameter index. </returns>
	  public virtual int TypeParameterIndex
	  {
		  get
		  {
			return (targetTypeAndInfo & 0x00FF0000) >> 16;
		  }
	  }

	  /// <summary>
	  /// Returns the index of the type parameter bound, within the type parameter {@link
	  /// #getTypeParameterIndex}, referenced by this type reference. This method must only be used for
	  /// type references whose sort is <seealso cref="CLASS_TYPE_PARAMETER_BOUND"/> or {@link
	  /// #METHOD_TYPE_PARAMETER_BOUND}.
	  /// </summary>
	  /// <returns> a type parameter bound index. </returns>
	  public virtual int TypeParameterBoundIndex
	  {
		  get
		  {
			return (targetTypeAndInfo & 0x0000FF00) >> 8;
		  }
	  }

	  /// <summary>
	  /// Returns the index of the "super type" of a class that is referenced by this type reference.
	  /// This method must only be used for type references whose sort is <seealso cref="CLASS_EXTENDS"/>.
	  /// </summary>
	  /// <returns> the index of an interface in the 'implements' clause of a class, or -1 if this type
	  ///     reference references the type of the super class. </returns>
	  public virtual int SuperTypeIndex
	  {
		  get
		  {
			return (short)((targetTypeAndInfo & 0x00FFFF00) >> 8);
		  }
	  }

	  /// <summary>
	  /// Returns the index of the formal parameter whose type is referenced by this type reference. This
	  /// method must only be used for type references whose sort is <seealso cref="METHOD_FORMAL_PARAMETER"/>.
	  /// </summary>
	  /// <returns> a formal parameter index. </returns>
	  public virtual int FormalParameterIndex
	  {
		  get
		  {
			return (targetTypeAndInfo & 0x00FF0000) >> 16;
		  }
	  }

	  /// <summary>
	  /// Returns the index of the exception, in a 'throws' clause of a method, whose type is referenced
	  /// by this type reference. This method must only be used for type references whose sort is {@link
	  /// #THROWS}.
	  /// </summary>
	  /// <returns> the index of an exception in the 'throws' clause of a method. </returns>
	  public virtual int ExceptionIndex
	  {
		  get
		  {
			return (targetTypeAndInfo & 0x00FFFF00) >> 8;
		  }
	  }

	  /// <summary>
	  /// Returns the index of the try catch block (using the order in which they are visited with
	  /// visitTryCatchBlock), whose 'catch' type is referenced by this type reference. This method must
	  /// only be used for type references whose sort is <seealso cref="EXCEPTION_PARAMETER"/> .
	  /// </summary>
	  /// <returns> the index of an exception in the 'throws' clause of a method. </returns>
	  public virtual int TryCatchBlockIndex
	  {
		  get
		  {
			return (targetTypeAndInfo & 0x00FFFF00) >> 8;
		  }
	  }

	  /// <summary>
	  /// Returns the index of the type argument referenced by this type reference. This method must only
	  /// be used for type references whose sort is <seealso cref="CAST"/>, {@link
	  /// #CONSTRUCTOR_INVOCATION_TYPE_ARGUMENT}, <seealso cref="METHOD_INVOCATION_TYPE_ARGUMENT"/>, {@link
	  /// #CONSTRUCTOR_REFERENCE_TYPE_ARGUMENT}, or <seealso cref="METHOD_REFERENCE_TYPE_ARGUMENT"/>.
	  /// </summary>
	  /// <returns> a type parameter index. </returns>
	  public virtual int TypeArgumentIndex
	  {
		  get
		  {
			return targetTypeAndInfo & 0xFF;
		  }
	  }

	  /// <summary>
	  /// Returns the int encoded value of this type reference, suitable for use in visit methods related
	  /// to type annotations, like visitTypeAnnotation.
	  /// </summary>
	  /// <returns> the int encoded value of this type reference. </returns>
	  public virtual int Value
	  {
		  get
		  {
			return targetTypeAndInfo;
		  }
	  }

	  /// <summary>
	  /// Puts the given target_type and target_info JVMS structures into the given ByteVector.
	  /// </summary>
	  /// <param name="targetTypeAndInfo"> a target_type and a target_info structures encoded as in {@link
	  ///     #targetTypeAndInfo}. LOCAL_VARIABLE and RESOURCE_VARIABLE target types are not supported. </param>
	  /// <param name="output"> where the type reference must be put. </param>
	  internal static void putTarget(int targetTypeAndInfo, ByteVector output)
	  {
		switch ((int)((uint)targetTypeAndInfo >> 24))
		{
		  case CLASS_TYPE_PARAMETER:
		  case METHOD_TYPE_PARAMETER:
		  case METHOD_FORMAL_PARAMETER:
			output.putShort((int)((uint)targetTypeAndInfo >> 16));
			break;
		  case FIELD:
		  case METHOD_RETURN:
		  case METHOD_RECEIVER:
			output.putByte((int)((uint)targetTypeAndInfo >> 24));
			break;
		  case CAST:
		  case CONSTRUCTOR_INVOCATION_TYPE_ARGUMENT:
		  case METHOD_INVOCATION_TYPE_ARGUMENT:
		  case CONSTRUCTOR_REFERENCE_TYPE_ARGUMENT:
		  case METHOD_REFERENCE_TYPE_ARGUMENT:
			output.putInt(targetTypeAndInfo);
			break;
		  case CLASS_EXTENDS:
		  case CLASS_TYPE_PARAMETER_BOUND:
		  case METHOD_TYPE_PARAMETER_BOUND:
		  case THROWS:
		  case EXCEPTION_PARAMETER:
		  case INSTANCEOF:
		  case NEW:
		  case CONSTRUCTOR_REFERENCE:
		  case METHOD_REFERENCE:
			output.put12((int)((uint)targetTypeAndInfo >> 24), (targetTypeAndInfo & 0xFFFF00) >> 8);
			break;
		  default:
			throw new System.ArgumentException();
		}
	  }
	}

}