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
        private readonly int _descriptorIndex;

        // Note: fields are ordered as in the record_component_info structure, and those related to
        // attributes are ordered as in Section 4.7 of the JVMS.

        /// <summary>
        ///     The name_index field of the Record attribute.
        /// </summary>
        private readonly int _nameIndex;

        /// <summary>
        ///     Where the constants used in this RecordComponentWriter must be stored.
        /// </summary>
        private readonly SymbolTable _symbolTable;

        /// <summary>
        ///     The first non standard attribute of this record component. The next ones can be accessed with
        ///     the <seealso cref="Attribute.nextAttribute" /> field. May be {@literal null}.
        ///     <para>
        ///         <b>WARNING</b>: this list stores the attributes in the <i>reverse</i> order of their visit.
        ///         firstAttribute is actually the last attribute visited in <seealso cref="VisitAttribute" />.
        ///         The <seealso cref="PutRecordComponentInfo" /> method writes the attributes in the order
        ///         defined by this list, i.e. in the reverse order specified by the user.
        ///     </para>
        /// </summary>
        private Attribute _firstAttribute;

        /// <summary>
        ///     The last runtime invisible annotation of this record component. The previous ones can be
        ///     accessed with the <seealso cref="AnnotationWriter._previousAnnotation" /> field. May be {@literal null}.
        /// </summary>
        private AnnotationWriter _lastRuntimeInvisibleAnnotation;

        /// <summary>
        ///     The last runtime invisible type annotation of this record component. The previous ones can be
        ///     accessed with the <seealso cref="AnnotationWriter._previousAnnotation" /> field. May be {@literal null}.
        /// </summary>
        private AnnotationWriter _lastRuntimeInvisibleTypeAnnotation;

        /// <summary>
        ///     The last runtime visible annotation of this record component. The previous ones can be accessed
        ///     with the <seealso cref="AnnotationWriter._previousAnnotation" /> field. May be {@literal null}.
        /// </summary>
        private AnnotationWriter _lastRuntimeVisibleAnnotation;

        /// <summary>
        ///     The last runtime visible type annotation of this record component. The previous ones can be
        ///     accessed with the <seealso cref="AnnotationWriter._previousAnnotation" /> field. May be {@literal null}.
        /// </summary>
        private AnnotationWriter _lastRuntimeVisibleTypeAnnotation;

        /// <summary>
        ///     The signature_index field of the Signature attribute of this record component, or 0 if there is
        ///     no Signature attribute.
        /// </summary>
        private readonly int _signatureIndex;

        /// <summary>
        ///     Constructs a new <seealso cref="RecordComponentWriter" />.
        /// </summary>
        /// <param name="symbolTable"> where the constants used in this RecordComponentWriter must be stored. </param>
        /// <param name="name"> the record component name. </param>
        /// <param name="descriptor"> the record component descriptor (see <seealso cref="Type" />). </param>
        /// <param name="signature"> the record component signature. May be {@literal null}. </param>
        public RecordComponentWriter(SymbolTable symbolTable, string name, string descriptor, string signature) : base(
            IOpcodes.Asm9)
        {
            this._symbolTable = symbolTable;
            _nameIndex = symbolTable.AddConstantUtf8(name);
            _descriptorIndex = symbolTable.AddConstantUtf8(descriptor);
            if (!ReferenceEquals(signature, null)) _signatureIndex = symbolTable.AddConstantUtf8(signature);
        }

        // -----------------------------------------------------------------------------------------------
        // Implementation of the FieldVisitor abstract class
        // -----------------------------------------------------------------------------------------------

        public override AnnotationVisitor VisitAnnotation(string descriptor, bool visible)
        {
            if (visible)
                return _lastRuntimeVisibleAnnotation =
                    AnnotationWriter.Create(_symbolTable, descriptor, _lastRuntimeVisibleAnnotation);
            return _lastRuntimeInvisibleAnnotation =
                AnnotationWriter.Create(_symbolTable, descriptor, _lastRuntimeInvisibleAnnotation);
        }

        public override AnnotationVisitor VisitTypeAnnotation(int typeRef, TypePath typePath, string descriptor,
            bool visible)
        {
            if (visible)
                return _lastRuntimeVisibleTypeAnnotation = AnnotationWriter.Create(_symbolTable, typeRef, typePath,
                    descriptor, _lastRuntimeVisibleTypeAnnotation);
            return _lastRuntimeInvisibleTypeAnnotation = AnnotationWriter.Create(_symbolTable, typeRef, typePath,
                descriptor, _lastRuntimeInvisibleTypeAnnotation);
        }

        public override void VisitAttribute(Attribute attribute)
        {
            // Store the attributes in the <i>reverse</i> order of their visit by this method.
            attribute.nextAttribute = _firstAttribute;
            _firstAttribute = attribute;
        }

        public override void VisitEnd()
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
        public int ComputeRecordComponentInfoSize()
        {
            // name_index, descriptor_index and attributes_count fields use 6 bytes.
            var size = 6;
            size += Attribute.ComputeAttributesSize(_symbolTable, 0, _signatureIndex);
            size += AnnotationWriter.ComputeAnnotationsSize(_lastRuntimeVisibleAnnotation,
                _lastRuntimeInvisibleAnnotation, _lastRuntimeVisibleTypeAnnotation, _lastRuntimeInvisibleTypeAnnotation);
            if (_firstAttribute != null) size += _firstAttribute.ComputeAttributesSize(_symbolTable);
            return size;
        }

        /// <summary>
        ///     Puts the content of the record component generated by this RecordComponentWriter into the given
        ///     ByteVector.
        /// </summary>
        /// <param name="output"> where the record_component_info structure must be put. </param>
        public void PutRecordComponentInfo(ByteVector output)
        {
            output.PutShort(_nameIndex).PutShort(_descriptorIndex);
            // Compute and put the attributes_count field.
            // For ease of reference, we use here the same attribute order as in Section 4.7 of the JVMS.
            var attributesCount = 0;
            if (_signatureIndex != 0) ++attributesCount;
            if (_lastRuntimeVisibleAnnotation != null) ++attributesCount;
            if (_lastRuntimeInvisibleAnnotation != null) ++attributesCount;
            if (_lastRuntimeVisibleTypeAnnotation != null) ++attributesCount;
            if (_lastRuntimeInvisibleTypeAnnotation != null) ++attributesCount;
            if (_firstAttribute != null) attributesCount += _firstAttribute.AttributeCount;
            output.PutShort(attributesCount);
            Attribute.PutAttributes(_symbolTable, 0, _signatureIndex, output);
            AnnotationWriter.PutAnnotations(_symbolTable, _lastRuntimeVisibleAnnotation, _lastRuntimeInvisibleAnnotation,
                _lastRuntimeVisibleTypeAnnotation, _lastRuntimeInvisibleTypeAnnotation, output);
            if (_firstAttribute != null) _firstAttribute.PutAttributes(_symbolTable, output);
        }

        /// <summary>
        ///     Collects the attributes of this record component into the given set of attribute prototypes.
        /// </summary>
        /// <param name="attributePrototypes"> a set of attribute prototypes. </param>
        public void CollectAttributePrototypes(Attribute.Set attributePrototypes)
        {
            attributePrototypes.AddAttributes(_firstAttribute);
        }
    }
}