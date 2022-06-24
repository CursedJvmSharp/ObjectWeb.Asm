using System.Collections.Generic;

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
namespace ObjectWeb.Asm.Tree
{
    /// <summary>
    /// A node that represents a record component.
    /// 
    /// @author Remi Forax
    /// </summary>
    public class RecordComponentNode : RecordComponentVisitor
    {
        /// <summary>
        /// The record component name. </summary>
        public string Name { get; set; }

        /// <summary>
        /// The record component descriptor (see <seealso cref = "org.objectweb.asm.Type"/>). </summary>
        public string Descriptor { get; set; }

        /// <summary>
        /// The record component signature. May be {@literal null}. </summary>
        public string Signature { get; set; }

        /// <summary>
        /// The runtime visible annotations of this record component. May be {@literal null}. </summary>
        public List<AnnotationNode> VisibleAnnotations { get; set; }

        /// <summary>
        /// The runtime invisible annotations of this record component. May be {@literal null}. </summary>
        public List<AnnotationNode> InvisibleAnnotations { get; set; }

        /// <summary>
        /// The runtime visible type annotations of this record component. May be {@literal null}. </summary>
        public List<TypeAnnotationNode> VisibleTypeAnnotations { get; set; }

        /// <summary>
        /// The runtime invisible type annotations of this record component. May be {@literal null}. </summary>
        public List<TypeAnnotationNode> InvisibleTypeAnnotations { get; set; }

        /// <summary>
        /// The non standard attributes of this record component. * May be {@literal null}. </summary>
        public List<Attribute> Attrs { get; set; }

        /// <summary>
        /// Constructs a new <seealso cref = "RecordComponentNode"/>. <i>Subclasses must not use this constructor</i>.
        /// Instead, they must use the <seealso cref = "RecordComponentNode(int, String, String, String)"/> version.
        /// </summary>
        /// <param name = "name"> the record component name. </param>
        /// <param name = "descriptor"> the record component descriptor (see <seealso cref = "org.objectweb.asm.Type"/>). </param>
        /// <param name = "signature"> the record component signature. </param>
        /// <exception cref = "IllegalStateException"> If a subclass calls this constructor. </exception>
        public RecordComponentNode(string name, string descriptor, string signature) : this(IOpcodes.Asm9, name,
            descriptor, signature)
        {
            if (this.GetType() != typeof(RecordComponentNode))
            {
                throw new System.InvalidOperationException();
            }
        }

        /// <summary>
        /// Constructs a new <seealso cref = "RecordComponentNode"/>.
        /// </summary>
        /// <param name = "api"> the ASM API version implemented by this visitor. Must be one of <seealso cref = "IIOpcodes.Asm8 / > ///or <seealso cref = "IIOpcodes.Asm9 / > . </param>
        /// <param name = "name"> the record component name. </param>
        /// <param name = "descriptor"> the record component descriptor (see <seealso cref = "org.objectweb.asm.Type"/>). </param>
        /// <param name = "signature"> the record component signature. </param>
        public RecordComponentNode(int api, string name, string descriptor, string signature) : base(api)
        {
            this.Name = name;
            this.Descriptor = descriptor;
            this.Signature = signature;
        }

        // -----------------------------------------------------------------------------------------------
        // Implementation of the FieldVisitor abstract class
        // -----------------------------------------------------------------------------------------------
        public override AnnotationVisitor VisitAnnotation(string descriptor, bool visible)
        {
            var annotation = new AnnotationNode(descriptor);
            if (visible)
            {
                VisibleAnnotations = Util.Add(VisibleAnnotations, annotation);
            }
            else
            {
                InvisibleAnnotations = Util.Add(InvisibleAnnotations, annotation);
            }

            return annotation;
        }

        public override AnnotationVisitor VisitTypeAnnotation(int typeRef, TypePath typePath, string descriptor,
            bool visible)
        {
            var typeAnnotation = new TypeAnnotationNode(typeRef, typePath, descriptor);
            if (visible)
            {
                VisibleTypeAnnotations = Util.Add(VisibleTypeAnnotations, typeAnnotation);
            }
            else
            {
                InvisibleTypeAnnotations = Util.Add(InvisibleTypeAnnotations, typeAnnotation);
            }

            return typeAnnotation;
        }

        public override void VisitAttribute(Attribute attribute)
        {
            Attrs = Util.Add(Attrs, attribute);
        }

        public override void VisitEnd()
        {
            // Nothing to do.
        }

        // -----------------------------------------------------------------------------------------------
        // Accept methods
        // -----------------------------------------------------------------------------------------------
        /// <summary>
        /// Checks that this record component node is compatible with the given ASM API version. This
        /// method checks that this node, and all its children recursively, do not contain elements that
        /// were introduced in more recent versions of the ASM API than the given version.
        /// </summary>
        /// <param name = "api"> an ASM API version. Must be one of <seealso cref = "IIOpcodes.Asm8 / > or <seealso cref = "IIOpcodes.Asm9 / > . </param>
        public virtual void Check(int api)
        {
            if (api < IOpcodes.Asm8)
            {
                throw new UnsupportedClassVersionException();
            }
        }

        /// <summary>
        /// Makes the given class visitor visit this record component.
        /// </summary>
        /// <param name = "classVisitor"> a class visitor. </param>
        public virtual void Accept(ClassVisitor classVisitor)
        {
            var recordComponentVisitor = classVisitor.VisitRecordComponent(Name, Descriptor, Signature);
            if (recordComponentVisitor == null)
            {
                return;
            }

            // Visit the annotations.
            if (VisibleAnnotations != null)
            {
                for (int i = 0, n = VisibleAnnotations.Count; i < n; ++i)
                {
                    var annotation = VisibleAnnotations[i];
                    annotation.Accept(recordComponentVisitor.VisitAnnotation(annotation.Desc, true));
                }
            }

            if (InvisibleAnnotations != null)
            {
                for (int i = 0, n = InvisibleAnnotations.Count; i < n; ++i)
                {
                    var annotation = InvisibleAnnotations[i];
                    annotation.Accept(recordComponentVisitor.VisitAnnotation(annotation.Desc, false));
                }
            }

            if (VisibleTypeAnnotations != null)
            {
                for (int i = 0, n = VisibleTypeAnnotations.Count; i < n; ++i)
                {
                    var typeAnnotation = VisibleTypeAnnotations[i];
                    typeAnnotation.Accept(recordComponentVisitor.VisitTypeAnnotation(typeAnnotation.TypeRef,
                        typeAnnotation.TypePath, typeAnnotation.Desc, true));
                }
            }

            if (InvisibleTypeAnnotations != null)
            {
                for (int i = 0, n = InvisibleTypeAnnotations.Count; i < n; ++i)
                {
                    var typeAnnotation = InvisibleTypeAnnotations[i];
                    typeAnnotation.Accept(recordComponentVisitor.VisitTypeAnnotation(typeAnnotation.TypeRef,
                        typeAnnotation.TypePath, typeAnnotation.Desc, false));
                }
            }

            // Visit the non standard attributes.
            if (Attrs != null)
            {
                for (int i = 0, n = Attrs.Count; i < n; ++i)
                {
                    recordComponentVisitor.VisitAttribute(Attrs[i]);
                }
            }

            recordComponentVisitor.VisitEnd();
        }
    }
}