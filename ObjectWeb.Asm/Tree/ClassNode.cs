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
    /// A node that represents a class.
    /// 
    /// @author Eric Bruneton
    /// </summary>
    public class ClassNode : ClassVisitor
    {
        /// <summary>
        /// The class version. The minor version is stored in the 16 most significant bits, and the major
        /// version in the 16 least significant bits.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// The class's access flags (see <seealso cref = "IOpcodes"/>). This field also indicates if
        /// the class is deprecated <seealso cref = "IIOpcodes.Acc_Deprecated / > or  a  record <seealso cref = "IIOpcodes.Acc_Record / > .
        /// </summary>
        public int Access { get; set; }

        /// <summary>
        /// The internal name of this class (see <seealso cref = "org.objectweb.asm.Type.getInternalName"/>). </summary>
        public string Name { get; set; }

        /// <summary>
        /// The signature of this class. May be {@literal null}. </summary>
        public string Signature { get; set; }

        /// <summary>
        /// The internal of name of the super class (see <seealso cref = "org.objectweb.asm.Type.getInternalName"/>).
        /// For interfaces, the super class is <seealso cref = "object "/>. May be {@literal null}, but only for the
        /// <seealso cref = "object "/> class.
        /// </summary>
        public string SuperName { get; set; }

        /// <summary>
        /// The internal names of the interfaces directly implemented by this class (see {@link
        /// org.objectweb.asm.Type#getInternalName}).
        /// </summary>
        public List<string> Interfaces { get; set; }

        /// <summary>
        /// The name of the source file from which this class was compiled. May be {@literal null}. </summary>
        public string SourceFile { get; set; }

        /// <summary>
        /// The correspondence between source and compiled elements of this class. May be {@literal null}.
        /// </summary>
        public string SourceDebug { get; set; }

        /// <summary>
        /// The module stored in this class. May be {@literal null}. </summary>
        public ModuleNode Module { get; set; }

        /// <summary>
        /// The internal name of the enclosing class of this class. May be {@literal null}. </summary>
        public string OuterClass { get; set; }

        /// <summary>
        /// The name of the method that contains this class, or {@literal null} if this class is not
        /// enclosed in a method.
        /// </summary>
        public string OuterMethod { get; set; }

        /// <summary>
        /// The descriptor of the method that contains this class, or {@literal null} if this class is not
        /// enclosed in a method.
        /// </summary>
        public string OuterMethodDesc { get; set; }

        /// <summary>
        /// The runtime visible annotations of this class. May be {@literal null}. </summary>
        public List<AnnotationNode> VisibleAnnotations { get; set; }

        /// <summary>
        /// The runtime invisible annotations of this class. May be {@literal null}. </summary>
        public List<AnnotationNode> InvisibleAnnotations { get; set; }

        /// <summary>
        /// The runtime visible type annotations of this class. May be {@literal null}. </summary>
        public List<TypeAnnotationNode> VisibleTypeAnnotations { get; set; }

        /// <summary>
        /// The runtime invisible type annotations of this class. May be {@literal null}. </summary>
        public List<TypeAnnotationNode> InvisibleTypeAnnotations { get; set; }

        /// <summary>
        /// The non standard attributes of this class. May be {@literal null}. </summary>
        public List<Attribute> Attrs { get; set; }

        /// <summary>
        /// The inner classes of this class. </summary>
        public List<InnerClassNode> InnerClasses { get; set; }

        /// <summary>
        /// The internal name of the nest host class of this class. May be {@literal null}. </summary>
        public string NestHostClass { get; set; }

        /// <summary>
        /// The internal names of the nest members of this class. May be {@literal null}. </summary>
        public List<string> NestMembers { get; set; }

        /// <summary>
        /// The internal names of the permitted subclasses of this class. May be {@literal null}. </summary>
        public List<string> PermittedSubclasses { get; set; }

        /// <summary>
        /// The record components of this class. May be {@literal null}. </summary>
        public List<RecordComponentNode> RecordComponents { get; set; }

        /// <summary>
        /// The fields of this class. </summary>
        public List<FieldNode> Fields { get; set; }

        /// <summary>
        /// The methods of this class. </summary>
        public List<MethodNode> Methods { get; set; }

        /// <summary>
        /// Constructs a new <seealso cref = "ClassNode"/>. <i>Subclasses must not use this constructor</i>. Instead,
        /// they must use the <seealso cref = "ClassNode(int)"/> version.
        /// </summary>
        /// <exception cref = "IllegalStateException"> If a subclass calls this constructor. </exception>
        public ClassNode() : this(IOpcodes.Asm9)
        {
            if (this.GetType() != typeof(ClassNode))
            {
                throw new System.InvalidOperationException();
            }
        }

        /// <summary>
        /// Constructs a new <seealso cref = "ClassNode"/>.
        /// </summary>
        /// <param name = "api"> the ASM API version implemented by this visitor. Must be one of the {@code
        ///     ASM}<i>x</i> values in <seealso cref = "IOpcodes"/>. </param>
        public ClassNode(int api) : base(api)
        {
            this.Interfaces = new List<string>();
            this.InnerClasses = new List<InnerClassNode>();
            this.Fields = new List<FieldNode>();
            this.Methods = new List<MethodNode>();
        }

        // -----------------------------------------------------------------------------------------------
        // Implementation of the ClassVisitor abstract class
        // -----------------------------------------------------------------------------------------------
        public override void Visit(int version, int access, string name, string signature, string superName,
            string[] interfaces)
        {
            this.Version = version;
            this.Access = access;
            this.Name = name;
            this.Signature = signature;
            this.SuperName = superName;
            this.Interfaces = Util.AsArrayList(interfaces);
        }

        public override void VisitSource(string file, string debug)
        {
            SourceFile = file;
            SourceDebug = debug;
        }

        public override ModuleVisitor VisitModule(string name, int access, string version)
        {
            Module = new ModuleNode(name, access, version);
            return Module;
        }

        public override void VisitNestHost(string nestHost)
        {
            this.NestHostClass = nestHost;
        }

        public override void VisitOuterClass(string owner, string name, string descriptor)
        {
            OuterClass = owner;
            OuterMethod = name;
            OuterMethodDesc = descriptor;
        }

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

        public override void VisitNestMember(string nestMember)
        {
            NestMembers = Util.Add(NestMembers, nestMember);
        }

        public override void VisitPermittedSubclass(string permittedSubclass)
        {
            PermittedSubclasses = Util.Add(PermittedSubclasses, permittedSubclass);
        }

        public override void VisitInnerClass(string name, string outerName, string innerName, int access)
        {
            var innerClass = new InnerClassNode(name, outerName, innerName, access);
            InnerClasses.Add(innerClass);
        }

        public override RecordComponentVisitor VisitRecordComponent(string name, string descriptor, string signature)
        {
            var recordComponent = new RecordComponentNode(name, descriptor, signature);
            RecordComponents = Util.Add(RecordComponents, recordComponent);
            return recordComponent;
        }

        public override FieldVisitor VisitField(int access, string name, string descriptor, string signature,
            object value)
        {
            var field = new FieldNode(access, name, descriptor, signature, value);
            Fields.Add(field);
            return field;
        }

        public override MethodVisitor VisitMethod(int access, string name, string descriptor, string signature,
            string[] exceptions)
        {
            var method = new MethodNode(access, name, descriptor, signature, exceptions);
            Methods.Add(method);
            return method;
        }

        public override void VisitEnd()
        {
            // Nothing to do.
        }

        // -----------------------------------------------------------------------------------------------
        // Accept method
        // -----------------------------------------------------------------------------------------------
        /// <summary>
        /// Checks that this class node is compatible with the given ASM API version. This method checks
        /// that this node, and all its children recursively, do not contain elements that were introduced
        /// in more recent versions of the ASM API than the given version.
        /// </summary>
        /// <param name = "api"> an ASM API version. Must be one of the {@code ASM}<i>x</i> values in {@link
        ///     Opcodes}. </param>
        public virtual void Check(int api)
        {
            if (api < IOpcodes.Asm9 && PermittedSubclasses != null)
            {
                throw new UnsupportedClassVersionException();
            }

            if (api < IOpcodes.Asm8 && ((Access & IOpcodes.Acc_Record) != 0 || RecordComponents != null))
            {
                throw new UnsupportedClassVersionException();
            }

            if (api < IOpcodes.Asm7 && (!string.ReferenceEquals(NestHostClass, null) || NestMembers != null))
            {
                throw new UnsupportedClassVersionException();
            }

            if (api < IOpcodes.Asm6 && Module != null)
            {
                throw new UnsupportedClassVersionException();
            }

            if (api < IOpcodes.Asm5)
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

            // Check the annotations.
            if (VisibleAnnotations != null)
            {
                for (var i = VisibleAnnotations.Count - 1; i >= 0; --i)
                {
                    VisibleAnnotations[i].Check(api);
                }
            }

            if (InvisibleAnnotations != null)
            {
                for (var i = InvisibleAnnotations.Count - 1; i >= 0; --i)
                {
                    InvisibleAnnotations[i].Check(api);
                }
            }

            if (VisibleTypeAnnotations != null)
            {
                for (var i = VisibleTypeAnnotations.Count - 1; i >= 0; --i)
                {
                    VisibleTypeAnnotations[i].Check(api);
                }
            }

            if (InvisibleTypeAnnotations != null)
            {
                for (var i = InvisibleTypeAnnotations.Count - 1; i >= 0; --i)
                {
                    InvisibleTypeAnnotations[i].Check(api);
                }
            }

            if (RecordComponents != null)
            {
                for (var i = RecordComponents.Count - 1; i >= 0; --i)
                {
                    RecordComponents[i].Check(api);
                }
            }

            for (var i = Fields.Count - 1; i >= 0; --i)
            {
                Fields[i].Check(api);
            }

            for (var i = Methods.Count - 1; i >= 0; --i)
            {
                Methods[i].Check(api);
            }
        }

        /// <summary>
        /// Makes the given class visitor visit this class.
        /// </summary>
        /// <param name = "classVisitor"> a class visitor. </param>
        public virtual void Accept(ClassVisitor classVisitor)
        {
            // Visit the header.
            var interfacesArray = this.Interfaces.ToArray();
            classVisitor.Visit(Version, Access, Name, Signature, SuperName, interfacesArray);
            // Visit the source.
            if (!string.ReferenceEquals(SourceFile, null) || !string.ReferenceEquals(SourceDebug, null))
            {
                classVisitor.VisitSource(SourceFile, SourceDebug);
            }

            // Visit the module.
            if (Module != null)
            {
                Module.Accept(classVisitor);
            }

            // Visit the nest host class.
            if (!string.ReferenceEquals(NestHostClass, null))
            {
                classVisitor.VisitNestHost(NestHostClass);
            }

            // Visit the outer class.
            if (!string.ReferenceEquals(OuterClass, null))
            {
                classVisitor.VisitOuterClass(OuterClass, OuterMethod, OuterMethodDesc);
            }

            // Visit the annotations.
            if (VisibleAnnotations != null)
            {
                for (int i = 0, n = VisibleAnnotations.Count; i < n; ++i)
                {
                    var annotation = VisibleAnnotations[i];
                    annotation.Accept(classVisitor.VisitAnnotation(annotation.Desc, true));
                }
            }

            if (InvisibleAnnotations != null)
            {
                for (int i = 0, n = InvisibleAnnotations.Count; i < n; ++i)
                {
                    var annotation = InvisibleAnnotations[i];
                    annotation.Accept(classVisitor.VisitAnnotation(annotation.Desc, false));
                }
            }

            if (VisibleTypeAnnotations != null)
            {
                for (int i = 0, n = VisibleTypeAnnotations.Count; i < n; ++i)
                {
                    var typeAnnotation = VisibleTypeAnnotations[i];
                    typeAnnotation.Accept(classVisitor.VisitTypeAnnotation(typeAnnotation.TypeRef,
                        typeAnnotation.TypePath, typeAnnotation.Desc, true));
                }
            }

            if (InvisibleTypeAnnotations != null)
            {
                for (int i = 0, n = InvisibleTypeAnnotations.Count; i < n; ++i)
                {
                    var typeAnnotation = InvisibleTypeAnnotations[i];
                    typeAnnotation.Accept(classVisitor.VisitTypeAnnotation(typeAnnotation.TypeRef,
                        typeAnnotation.TypePath, typeAnnotation.Desc, false));
                }
            }

            // Visit the non standard attributes.
            if (Attrs != null)
            {
                for (int i = 0, n = Attrs.Count; i < n; ++i)
                {
                    classVisitor.VisitAttribute(Attrs[i]);
                }
            }

            // Visit the nest members.
            if (NestMembers != null)
            {
                for (int i = 0, n = NestMembers.Count; i < n; ++i)
                {
                    classVisitor.VisitNestMember(NestMembers[i]);
                }
            }

            // Visit the permitted subclasses.
            if (PermittedSubclasses != null)
            {
                for (int i = 0, n = PermittedSubclasses.Count; i < n; ++i)
                {
                    classVisitor.VisitPermittedSubclass(PermittedSubclasses[i]);
                }
            }

            // Visit the inner classes.
            for (int i = 0, n = InnerClasses.Count; i < n; ++i)
            {
                InnerClasses[i].Accept(classVisitor);
            }

            // Visit the record components.
            if (RecordComponents != null)
            {
                for (int i = 0, n = RecordComponents.Count; i < n; ++i)
                {
                    RecordComponents[i].Accept(classVisitor);
                }
            }

            // Visit the fields.
            for (int i = 0, n = Fields.Count; i < n; ++i)
            {
                Fields[i].Accept(classVisitor);
            }

            // Visit the methods.
            for (int i = 0, n = Methods.Count; i < n; ++i)
            {
                Methods[i].Accept(classVisitor);
            }

            classVisitor.VisitEnd();
        }
    }
}