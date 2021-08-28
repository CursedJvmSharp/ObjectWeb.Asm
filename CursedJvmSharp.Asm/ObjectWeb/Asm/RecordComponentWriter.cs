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
    internal sealed class RecordComponentWriter : RecordComponentVisitor
    {
        /// <summary>
        ///     The descriptor_index field of the the Record attribute.
        /// </summary>
        private readonly int descriptorIndex;

        // Note: fields are ordered as in the record_component_info structure, and those related to
        // attributes are ordered as in Section 4.7 of the JVMS.

        /// <summary>
        ///     The name_index field of the Record attribute.
        /// </summary>
        private readonly int nameIndex;

        /// <summary>
        ///     Where the constants used in this RecordComponentWriter must be stored.
        /// </summary>
        private readonly SymbolTable symbolTable;

        /// <summary>
        ///     The first non standard attribute of this record component. The next ones can be accessed with
        ///     the <seealso cref="Attribute.nextAttribute" /> field. May be {@literal null}.
        ///     <para>
        ///         <b>WARNING</b>: this list stores the attributes in the <i>reverse</i> order of their visit.
        ///         firstAttribute is actually the last attribute visited in <seealso cref="visitAttribute(Attribute)" />.
        ///         The <seealso cref="putRecordComponentInfo(ByteVector)" /> method writes the attributes in the order
        ///         defined by this list, i.e. in the reverse order specified by the user.
        ///     </para>
        /// </summary>
        private Attribute firstAttribute;

        /// <summary>
        ///     The last runtime invisible annotation of this record component. The previous ones can be
        ///     accessed with the <seealso cref="AnnotationWriter.previousAnnotation" /> field. May be {@literal null}.
        /// </summary>
        private AnnotationWriter lastRuntimeInvisibleAnnotation;

        /// <summary>
        ///     The last runtime invisible type annotation of this record component. The previous ones can be
        ///     accessed with the <seealso cref="AnnotationWriter.previousAnnotation" /> field. May be {@literal null}.
        /// </summary>
        private AnnotationWriter lastRuntimeInvisibleTypeAnnotation;

        /// <summary>
        ///     The last runtime visible annotation of this record component. The previous ones can be accessed
        ///     with the <seealso cref="AnnotationWriter.previousAnnotation" /> field. May be {@literal null}.
        /// </summary>
        private AnnotationWriter lastRuntimeVisibleAnnotation;

        /// <summary>
        ///     The last runtime visible type annotation of this record component. The previous ones can be
        ///     accessed with the <seealso cref="AnnotationWriter.previousAnnotation" /> field. May be {@literal null}.
        /// </summary>
        private AnnotationWriter lastRuntimeVisibleTypeAnnotation;

        /// <summary>
        ///     The signature_index field of the Signature attribute of this record component, or 0 if there is
        ///     no Signature attribute.
        /// </summary>
        private readonly int signatureIndex;

        /// <summary>
        ///     Constructs a new <seealso cref="RecordComponentWriter" />.
        /// </summary>
        /// <param name="symbolTable"> where the constants used in this RecordComponentWriter must be stored. </param>
        /// <param name="name"> the record component name. </param>
        /// <param name="descriptor"> the record component descriptor (see <seealso cref="Type" />). </param>
        /// <param name="signature"> the record component signature. May be {@literal null}. </param>
        public RecordComponentWriter(SymbolTable symbolTable, string name, string descriptor, string signature) : base(
            Opcodes.ASM9)
        {
            this.symbolTable = symbolTable;
            nameIndex = symbolTable.addConstantUtf8(name);
            descriptorIndex = symbolTable.addConstantUtf8(descriptor);
            if (!ReferenceEquals(signature, null)) signatureIndex = symbolTable.addConstantUtf8(signature);
        }

        // -----------------------------------------------------------------------------------------------
        // Implementation of the FieldVisitor abstract class
        // -----------------------------------------------------------------------------------------------

        public override AnnotationVisitor visitAnnotation(string descriptor, bool visible)
        {
            if (visible)
                return lastRuntimeVisibleAnnotation =
                    AnnotationWriter.create(symbolTable, descriptor, lastRuntimeVisibleAnnotation);
            return lastRuntimeInvisibleAnnotation =
                AnnotationWriter.create(symbolTable, descriptor, lastRuntimeInvisibleAnnotation);
        }

        public override AnnotationVisitor visitTypeAnnotation(int typeRef, TypePath typePath, string descriptor,
            bool visible)
        {
            if (visible)
                return lastRuntimeVisibleTypeAnnotation = AnnotationWriter.create(symbolTable, typeRef, typePath,
                    descriptor, lastRuntimeVisibleTypeAnnotation);
            return lastRuntimeInvisibleTypeAnnotation = AnnotationWriter.create(symbolTable, typeRef, typePath,
                descriptor, lastRuntimeInvisibleTypeAnnotation);
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
        ///     Returns the size of the record component JVMS structure generated by this
        ///     RecordComponentWriter. Also adds the names of the attributes of this record component in the
        ///     constant pool.
        /// </summary>
        /// <returns> the size in bytes of the record_component_info of the Record attribute. </returns>
        public int computeRecordComponentInfoSize()
        {
            // name_index, descriptor_index and attributes_count fields use 6 bytes.
            var size = 6;
            size += Attribute.computeAttributesSize(symbolTable, 0, signatureIndex);
            size += AnnotationWriter.computeAnnotationsSize(lastRuntimeVisibleAnnotation,
                lastRuntimeInvisibleAnnotation, lastRuntimeVisibleTypeAnnotation, lastRuntimeInvisibleTypeAnnotation);
            if (firstAttribute != null) size += firstAttribute.computeAttributesSize(symbolTable);
            return size;
        }

        /// <summary>
        ///     Puts the content of the record component generated by this RecordComponentWriter into the given
        ///     ByteVector.
        /// </summary>
        /// <param name="output"> where the record_component_info structure must be put. </param>
        public void putRecordComponentInfo(ByteVector output)
        {
            output.putShort(nameIndex).putShort(descriptorIndex);
            // Compute and put the attributes_count field.
            // For ease of reference, we use here the same attribute order as in Section 4.7 of the JVMS.
            var attributesCount = 0;
            if (signatureIndex != 0) ++attributesCount;
            if (lastRuntimeVisibleAnnotation != null) ++attributesCount;
            if (lastRuntimeInvisibleAnnotation != null) ++attributesCount;
            if (lastRuntimeVisibleTypeAnnotation != null) ++attributesCount;
            if (lastRuntimeInvisibleTypeAnnotation != null) ++attributesCount;
            if (firstAttribute != null) attributesCount += firstAttribute.AttributeCount;
            output.putShort(attributesCount);
            Attribute.putAttributes(symbolTable, 0, signatureIndex, output);
            AnnotationWriter.putAnnotations(symbolTable, lastRuntimeVisibleAnnotation, lastRuntimeInvisibleAnnotation,
                lastRuntimeVisibleTypeAnnotation, lastRuntimeInvisibleTypeAnnotation, output);
            if (firstAttribute != null) firstAttribute.putAttributes(symbolTable, output);
        }

        /// <summary>
        ///     Collects the attributes of this record component into the given set of attribute prototypes.
        /// </summary>
        /// <param name="attributePrototypes"> a set of attribute prototypes. </param>
        public void collectAttributePrototypes(Attribute.Set attributePrototypes)
        {
            attributePrototypes.addAttributes(firstAttribute);
        }
    }
}