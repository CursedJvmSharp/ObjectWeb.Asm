

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
	/// A <seealso cref="FieldVisitor"/> that generates a corresponding 'field_info' structure, as defined in the
	/// Java Virtual Machine Specification (JVMS).
	/// </summary>
	/// <seealso cref= <a href="https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.5">JVMS
	///     4.5</a>
	/// @author Eric Bruneton </seealso>
	internal sealed class FieldWriter : FieldVisitor
	{

	  /// <summary>
	  /// Where the constants used in this FieldWriter must be stored. </summary>
	  private readonly SymbolTable symbolTable;

	  // Note: fields are ordered as in the field_info structure, and those related to attributes are
	  // ordered as in Section 4.7 of the JVMS.

	  /// <summary>
	  /// The access_flags field of the field_info JVMS structure. This field can contain ASM specific
	  /// access flags, such as <seealso cref="Opcodes.ACC_DEPRECATED"/>, which are removed when generating the
	  /// ClassFile structure.
	  /// </summary>
	  private readonly int accessFlags;

	  /// <summary>
	  /// The name_index field of the field_info JVMS structure. </summary>
	  private readonly int nameIndex;

	  /// <summary>
	  /// The descriptor_index field of the field_info JVMS structure. </summary>
	  private readonly int descriptorIndex;

	  /// <summary>
	  /// The signature_index field of the Signature attribute of this field_info, or 0 if there is no
	  /// Signature attribute.
	  /// </summary>
	  private int signatureIndex;

	  /// <summary>
	  /// The constantvalue_index field of the ConstantValue attribute of this field_info, or 0 if there
	  /// is no ConstantValue attribute.
	  /// </summary>
	  private int constantValueIndex;

	  /// <summary>
	  /// The last runtime visible annotation of this field. The previous ones can be accessed with the
	  /// <seealso cref="AnnotationWriter.previousAnnotation"/> field. May be {@literal null}.
	  /// </summary>
	  private AnnotationWriter lastRuntimeVisibleAnnotation;

	  /// <summary>
	  /// The last runtime invisible annotation of this field. The previous ones can be accessed with the
	  /// <seealso cref="AnnotationWriter.previousAnnotation"/> field. May be {@literal null}.
	  /// </summary>
	  private AnnotationWriter lastRuntimeInvisibleAnnotation;

	  /// <summary>
	  /// The last runtime visible type annotation of this field. The previous ones can be accessed with
	  /// the <seealso cref="AnnotationWriter.previousAnnotation"/> field. May be {@literal null}.
	  /// </summary>
	  private AnnotationWriter lastRuntimeVisibleTypeAnnotation;

	  /// <summary>
	  /// The last runtime invisible type annotation of this field. The previous ones can be accessed
	  /// with the <seealso cref="AnnotationWriter.previousAnnotation"/> field. May be {@literal null}.
	  /// </summary>
	  private AnnotationWriter lastRuntimeInvisibleTypeAnnotation;

	  /// <summary>
	  /// The first non standard attribute of this field. The next ones can be accessed with the {@link
	  /// Attribute#nextAttribute} field. May be {@literal null}.
	  /// 
	  /// <para><b>WARNING</b>: this list stores the attributes in the <i>reverse</i> order of their visit.
	  /// firstAttribute is actually the last attribute visited in <seealso cref="visitAttribute"/>. The {@link
	  /// #putFieldInfo} method writes the attributes in the order defined by this list, i.e. in the
	  /// reverse order specified by the user.
	  /// </para>
	  /// </summary>
	  private Attribute firstAttribute;

	  // -----------------------------------------------------------------------------------------------
	  // Constructor
	  // -----------------------------------------------------------------------------------------------

	  /// <summary>
	  /// Constructs a new <seealso cref="FieldWriter"/>.
	  /// </summary>
	  /// <param name="symbolTable"> where the constants used in this FieldWriter must be stored. </param>
	  /// <param name="access"> the field's access flags (see <seealso cref="Opcodes"/>). </param>
	  /// <param name="name"> the field's name. </param>
	  /// <param name="descriptor"> the field's descriptor (see <seealso cref="Type"/>). </param>
	  /// <param name="signature"> the field's signature. May be {@literal null}. </param>
	  /// <param name="constantValue"> the field's constant value. May be {@literal null}. </param>
	  public FieldWriter(SymbolTable symbolTable, int access, string name, string descriptor, string signature, object constantValue) : base(Opcodes.ASM9)
	  {
		this.symbolTable = symbolTable;
		this.accessFlags = access;
		this.nameIndex = symbolTable.addConstantUtf8(name);
		this.descriptorIndex = symbolTable.addConstantUtf8(descriptor);
		if (!string.ReferenceEquals(signature, null))
		{
		  this.signatureIndex = symbolTable.addConstantUtf8(signature);
		}
		if (constantValue != null)
		{
		  this.constantValueIndex = symbolTable.addConstant(constantValue).index;
		}
	  }

	  // -----------------------------------------------------------------------------------------------
	  // Implementation of the FieldVisitor abstract class
	  // -----------------------------------------------------------------------------------------------

	  public override AnnotationVisitor visitAnnotation(string descriptor, bool visible)
	  {
		if (visible)
		{
		  return lastRuntimeVisibleAnnotation = AnnotationWriter.create(symbolTable, descriptor, lastRuntimeVisibleAnnotation);
		}
		else
		{
		  return lastRuntimeInvisibleAnnotation = AnnotationWriter.create(symbolTable, descriptor, lastRuntimeInvisibleAnnotation);
		}
	  }

	  public override AnnotationVisitor visitTypeAnnotation(int typeRef, TypePath typePath, string descriptor, bool visible)
	  {
		if (visible)
		{
		  return lastRuntimeVisibleTypeAnnotation = AnnotationWriter.create(symbolTable, typeRef, typePath, descriptor, lastRuntimeVisibleTypeAnnotation);
		}
		else
		{
		  return lastRuntimeInvisibleTypeAnnotation = AnnotationWriter.create(symbolTable, typeRef, typePath, descriptor, lastRuntimeInvisibleTypeAnnotation);
		}
	  }

	  public override void visitAttribute(Attribute attribute)
	  {
		// Store the attributes in the <i>reverse</i> order of their visit by this method.
		attribute.nextAttribute = firstAttribute;
		firstAttribute = attribute;
	  }

	  public override void visitEnd()
	  {
		// Nothing to do.
	  }

	  // -----------------------------------------------------------------------------------------------
	  // Utility methods
	  // -----------------------------------------------------------------------------------------------

	  /// <summary>
	  /// Returns the size of the field_info JVMS structure generated by this FieldWriter. Also adds the
	  /// names of the attributes of this field in the constant pool.
	  /// </summary>
	  /// <returns> the size in bytes of the field_info JVMS structure. </returns>
	  public int computeFieldInfoSize()
	  {
		// The access_flags, name_index, descriptor_index and attributes_count fields use 8 bytes.
		int size = 8;
		// For ease of reference, we use here the same attribute order as in Section 4.7 of the JVMS.
		if (constantValueIndex != 0)
		{
		  // ConstantValue attributes always use 8 bytes.
		  symbolTable.addConstantUtf8(Constants.CONSTANT_VALUE);
		  size += 8;
		}
		size += Attribute.computeAttributesSize(symbolTable, accessFlags, signatureIndex);
		size += AnnotationWriter.computeAnnotationsSize(lastRuntimeVisibleAnnotation, lastRuntimeInvisibleAnnotation, lastRuntimeVisibleTypeAnnotation, lastRuntimeInvisibleTypeAnnotation);
		if (firstAttribute != null)
		{
		  size += firstAttribute.computeAttributesSize(symbolTable);
		}
		return size;
	  }

	  /// <summary>
	  /// Puts the content of the field_info JVMS structure generated by this FieldWriter into the given
	  /// ByteVector.
	  /// </summary>
	  /// <param name="output"> where the field_info structure must be put. </param>
	  public void putFieldInfo(ByteVector output)
	  {
		bool useSyntheticAttribute = symbolTable.MajorVersion < Opcodes.V1_5;
		// Put the access_flags, name_index and descriptor_index fields.
		int mask = useSyntheticAttribute ? Opcodes.ACC_SYNTHETIC : 0;
		output.putShort(accessFlags & ~mask).putShort(nameIndex).putShort(descriptorIndex);
		// Compute and put the attributes_count field.
		// For ease of reference, we use here the same attribute order as in Section 4.7 of the JVMS.
		int attributesCount = 0;
		if (constantValueIndex != 0)
		{
		  ++attributesCount;
		}
		if ((accessFlags & Opcodes.ACC_SYNTHETIC) != 0 && useSyntheticAttribute)
		{
		  ++attributesCount;
		}
		if (signatureIndex != 0)
		{
		  ++attributesCount;
		}
		if ((accessFlags & Opcodes.ACC_DEPRECATED) != 0)
		{
		  ++attributesCount;
		}
		if (lastRuntimeVisibleAnnotation != null)
		{
		  ++attributesCount;
		}
		if (lastRuntimeInvisibleAnnotation != null)
		{
		  ++attributesCount;
		}
		if (lastRuntimeVisibleTypeAnnotation != null)
		{
		  ++attributesCount;
		}
		if (lastRuntimeInvisibleTypeAnnotation != null)
		{
		  ++attributesCount;
		}
		if (firstAttribute != null)
		{
		  attributesCount += firstAttribute.AttributeCount;
		}
		output.putShort(attributesCount);
		// Put the field_info attributes.
		// For ease of reference, we use here the same attribute order as in Section 4.7 of the JVMS.
		if (constantValueIndex != 0)
		{
		  output.putShort(symbolTable.addConstantUtf8(Constants.CONSTANT_VALUE)).putInt(2).putShort(constantValueIndex);
		}
		Attribute.putAttributes(symbolTable, accessFlags, signatureIndex, output);
		AnnotationWriter.putAnnotations(symbolTable, lastRuntimeVisibleAnnotation, lastRuntimeInvisibleAnnotation, lastRuntimeVisibleTypeAnnotation, lastRuntimeInvisibleTypeAnnotation, output);
		if (firstAttribute != null)
		{
		  firstAttribute.putAttributes(symbolTable, output);
		}
	  }

	  /// <summary>
	  /// Collects the attributes of this field into the given set of attribute prototypes.
	  /// </summary>
	  /// <param name="attributePrototypes"> a set of attribute prototypes. </param>
	  public void collectAttributePrototypes(Attribute.Set attributePrototypes)
	  {
		attributePrototypes.addAttributes(firstAttribute);
	  }
	}

}