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
    /// A node that represents a field.
    /// 
    /// @author Eric Bruneton
    /// </summary>
    public class FieldNode : FieldVisitor
    {
        /// <summary>
        /// The field's access flags (see <seealso cref = "IOpcodes"/>). This field also indicates if
        /// the field is synthetic and/or deprecated.
        /// </summary>
        public int Access { get; set; }

        /// <summary>
        /// The field's name. </summary>
        public string Name { get; set; }

        /// <summary>
        /// The field's descriptor (see <seealso cref = "org.objectweb.asm.Type"/>). </summary>
        public string Desc { get; set; }

        /// <summary>
        /// The field's signature. May be {@literal null}. </summary>
        public string Signature { get; set; }

        /// <summary>
        /// The field's initial value. This field, which may be {@literal null} if the field does not have
        /// an initial value, must be an <seealso cref = "Integer"/>, a <seealso cref = "Float"/>, a <seealso cref = "Long"/>, a <seealso cref = "Double"/>
        /// or a <seealso cref = "string "/>.
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// The runtime visible annotations of this field. May be {@literal null}. </summary>
        public List<AnnotationNode> VisibleAnnotations { get; set; }

        /// <summary>
        /// The runtime invisible annotations of this field. May be {@literal null}. </summary>
        public List<AnnotationNode> InvisibleAnnotations { get; set; }

        /// <summary>
        /// The runtime visible type annotations of this field. May be {@literal null}. </summary>
        public List<TypeAnnotationNode> VisibleTypeAnnotations { get; set; }

        /// <summary>
        /// The runtime invisible type annotations of this field. May be {@literal null}. </summary>
        public List<TypeAnnotationNode> InvisibleTypeAnnotations { get; set; }

        /// <summary>
        /// The non standard attributes of this field. * May be {@literal null}. </summary>
        public List<Attribute> Attrs { get; set; }

        /// <summary>
        /// Constructs a new <seealso cref = "FieldNode"/>. <i>Subclasses must not use this constructor</i>. Instead,
        /// they must use the <seealso cref = "FieldNode(int, int, String, String, String, Object)"/> version.
        /// </summary>
        /// <param name = "access"> the field's access flags (see <seealso cref = "org.objectweb.asm.Opcodes"/>). This parameter
        ///     also indicates if the field is synthetic and/or deprecated. </param>
        /// <param name = "name"> the field's name. </param>
        /// <param name = "descriptor"> the field's descriptor (see <seealso cref = "org.objectweb.asm.Type"/>). </param>
        /// <param name = "signature"> the field's signature. </param>
        /// <param name = "value"> the field's initial value. This parameter, which may be {@literal null} if the
        ///     field does not have an initial value, must be an <seealso cref = "Integer"/>, a <seealso cref = "Float"/>, a {@link
        ///     Long}, a <seealso cref = "Double"/> or a <seealso cref = "string "/>. </param>
        /// <exception cref = "IllegalStateException"> If a subclass calls this constructor. </exception>
        public FieldNode(int access, string name, string descriptor, string signature, object value) : this(
            IOpcodes.Asm9, access, name, descriptor, signature, value)
        {
            if (this.GetType() != typeof(FieldNode))
            {
                throw new System.InvalidOperationException();
            }
        }

        /// <summary>
        /// Constructs a new <seealso cref = "FieldNode"/>.
        /// </summary>
        /// <param name = "api"> the ASM API version implemented by this visitor. Must be one of the {@code
        ///     ASM}<i>x</i> Values in <seealso cref = "IOpcodes"/>. </param>
        /// <param name = "access"> the field's access flags (see <seealso cref = "org.objectweb.asm.Opcodes"/>). This parameter
        ///     also indicates if the field is synthetic and/or deprecated. </param>
        /// <param name = "name"> the field's name. </param>
        /// <param name = "descriptor"> the field's descriptor (see <seealso cref = "org.objectweb.asm.Type"/>). </param>
        /// <param name = "signature"> the field's signature. </param>
        /// <param name = "value"> the field's initial value. This parameter, which may be {@literal null} if the
        ///     field does not have an initial value, must be an <seealso cref = "Integer"/>, a <seealso cref = "Float"/>, a {@link
        ///     Long}, a <seealso cref = "Double"/> or a <seealso cref = "string "/>. </param>
        public FieldNode(int api, int access, string name, string descriptor, string signature, object value) :
            base(api)
        {
            this.Access = access;
            this.Name = name;
            this.Desc = descriptor;
            this.Signature = signature;
            this.Value = value;
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
        /// Checks that this field node is compatible with the given ASM API version. This method checks
        /// that this node, and all its children recursively, do not contain elements that were introduced
        /// in more recent versions of the ASM API than the given version.
        /// </summary>
        /// <param name = "api"> an ASM API version. Must be one of the {@code ASM}<i>x</i> Values in {@link
        ///     Opcodes}. </param>
        public virtual void Check(int api)
        {
            if (api == IOpcodes.Asm4)
            {
                if (VisibleTypeAnnotations != null && VisibleTypeAnnotations.Count > 0)
                {
                    throw new UnsupportedClassVersionException();
                }

                if (InvisibleTypeAnnotations != null && InvisibleTypeAnnotations.Count > 0)
                {
                    throw new UnsupportedClassVersionException();
                }
            }
        }

        /// <summary>
        /// Makes the given class visitor visit this field.
        /// </summary>
        /// <param name = "classVisitor"> a class visitor. </param>
        public virtual void Accept(ClassVisitor classVisitor)
        {
            var fieldVisitor = classVisitor.VisitField(Access, Name, Desc, Signature, Value);
            if (fieldVisitor == null)
            {
                return;
            }

            // Visit the annotations.
            if (VisibleAnnotations != null)
            {
                for (int i = 0, n = VisibleAnnotations.Count; i < n; ++i)
                {
                    var annotation = VisibleAnnotations[i];
                    annotation.Accept(fieldVisitor.VisitAnnotation(annotation.Desc, true));
                }
            }

            if (InvisibleAnnotations != null)
            {
                for (int i = 0, n = InvisibleAnnotations.Count; i < n; ++i)
                {
                    var annotation = InvisibleAnnotations[i];
                    annotation.Accept(fieldVisitor.VisitAnnotation(annotation.Desc, false));
                }
            }

            if (VisibleTypeAnnotations != null)
            {
                for (int i = 0, n = VisibleTypeAnnotations.Count; i < n; ++i)
                {
                    var typeAnnotation = VisibleTypeAnnotations[i];
                    typeAnnotation.Accept(fieldVisitor.VisitTypeAnnotation(typeAnnotation.TypeRef,
                        typeAnnotation.TypePath, typeAnnotation.Desc, true));
                }
            }

            if (InvisibleTypeAnnotations != null)
            {
                for (int i = 0, n = InvisibleTypeAnnotations.Count; i < n; ++i)
                {
                    var typeAnnotation = InvisibleTypeAnnotations[i];
                    typeAnnotation.Accept(fieldVisitor.VisitTypeAnnotation(typeAnnotation.TypeRef,
                        typeAnnotation.TypePath, typeAnnotation.Desc, false));
                }
            }

            // Visit the non standard attributes.
            if (Attrs != null)
            {
                for (int i = 0, n = Attrs.Count; i < n; ++i)
                {
                    fieldVisitor.VisitAttribute(Attrs[i]);
                }
            }

            fieldVisitor.VisitEnd();
        }
    }
}