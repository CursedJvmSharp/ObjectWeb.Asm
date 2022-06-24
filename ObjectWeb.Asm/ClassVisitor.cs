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
    /// A visitor to visit a Java class. The methods of this class must be called in the following order:
    /// {@code visit} [ {@code visitSource} ] [ {@code visitModule} ][ {@code visitNestHost} ][ {@code
    /// visitOuterClass} ] ( {@code visitAnnotation} | {@code visitTypeAnnotation} | {@code
    /// visitAttribute} )* ( {@code visitNestMember} | [ {@code * visitPermittedSubclass} ] | {@code
    /// visitInnerClass} | {@code visitRecordComponent} | {@code visitField} | {@code visitMethod} )*
    /// {@code visitEnd}.
    /// 
    /// @author Eric Bruneton
    /// </summary>
    public abstract class ClassVisitor
    {
        /// <summary>
        /// The ASM API version implemented by this visitor. The value of this field must be one of the
        /// {@code ASM}<i>x</i> Values in <seealso cref="IOpcodes"/>.
        /// </summary>
        protected internal readonly int api;

        /// <summary>
        /// The class visitor to which this visitor must delegate method calls. May be {@literal null}. </summary>
        protected internal ClassVisitor cv;

        /// <summary>
        /// Constructs a new <seealso cref="ClassVisitor"/>.
        /// </summary>
        /// <param name="api"> the ASM API version implemented by this visitor. Must be one of the {@code
        ///     ASM}<i>x</i> Values in <seealso cref="IOpcodes"/>. </param>
        public ClassVisitor(int api) : this(api, null)
        {
        }

        /// <summary>
        /// Constructs a new <seealso cref="ClassVisitor"/>.
        /// </summary>
        /// <param name="api"> the ASM API version implemented by this visitor. Must be one of the {@code
        ///     ASM}<i>x</i> Values in <seealso cref="IOpcodes"/>. </param>
        /// <param name="classVisitor"> the class visitor to which this visitor must delegate method calls. May be
        ///     null. </param>
        public ClassVisitor(int api, ClassVisitor classVisitor)
        {
            if (api != IOpcodes.Asm9 && api != IOpcodes.Asm8 && api != IOpcodes.Asm7 && api != IOpcodes.Asm6 &&
                api != IOpcodes.Asm5 && api != IOpcodes.Asm4 && api != IOpcodes.Asm10_Experimental)
            {
                throw new System.ArgumentException("Unsupported api " + api);
            }

            if (api == IOpcodes.Asm10_Experimental)
            {
                Constants.CheckAsmExperimental(this);
            }

            this.api = api;
            this.cv = classVisitor;
        }

        /// <summary>
        /// Visits the header of the class.
        /// </summary>
        /// <param name="version"> the class version. The minor version is stored in the 16 most significant bits,
        ///     and the major version in the 16 least significant bits. </param>
        /// <param name="access"> the class's access flags (see <seealso cref="IOpcodes"/>). This parameter also indicates if
        ///     the class is deprecated <seealso cref="IIOpcodes.Acc_Deprecated/> or a record {@link
        ///     Opcodes#ACC_RECORD}. </param>
        /// <param name="name"> the internal name of the class (see <seealso cref="Type.InternalName"/>). </param>
        /// <param name="signature"> the signature of this class. May be {@literal null} if the class is not a
        ///     generic one, and does not extend or implement generic classes or interfaces. </param>
        /// <param name="superName"> the internal of name of the super class (see <seealso cref="Type.InternalName"/>).
        ///     For interfaces, the super class is <seealso cref="object"/>. May be {@literal null}, but only for the
        ///     <seealso cref="object"/> class. </param>
        /// <param name="interfaces"> the internal names of the class's interfaces (see {@link
        ///     Type#getInternalName()}). May be {@literal null}. </param>
        public virtual void Visit(int version, int access, string name, string signature, string superName,
            string[] interfaces)
        {
            if (api < IOpcodes.Asm8 && (access & IOpcodes.Acc_Record) != 0)
            {
                throw new System.NotSupportedException("Records requires ASM8");
            }

            if (cv != null)
            {
                cv.Visit(version, access, name, signature, superName, interfaces);
            }
        }

        /// <summary>
        /// Visits the source of the class.
        /// </summary>
        /// <param name="source"> the name of the source file from which the class was compiled. May be {@literal
        ///     null}. </param>
        /// <param name="debug"> additional debug information to compute the correspondence between source and
        ///     compiled elements of the class. May be {@literal null}. </param>
        public virtual void VisitSource(string source, string debug)
        {
            if (cv != null)
            {
                cv.VisitSource(source, debug);
            }
        }

        /// <summary>
        /// Visit the module corresponding to the class.
        /// </summary>
        /// <param name="name"> the fully qualified name (using dots) of the module. </param>
        /// <param name="access"> the module access flags, among {@code ACC_OPEN}, {@code ACC_SYNTHETIC} and {@code
        ///     ACC_MANDATED}. </param>
        /// <param name="version"> the module version, or {@literal null}. </param>
        /// <returns> a visitor to visit the module Values, or {@literal null} if this visitor is not
        ///     interested in visiting this module. </returns>
        public virtual ModuleVisitor VisitModule(string name, int access, string version)
        {
            if (api < IOpcodes.Asm6)
            {
                throw new System.NotSupportedException("Module requires ASM6");
            }

            if (cv != null)
            {
                return cv.VisitModule(name, access, version);
            }

            return null;
        }

        /// <summary>
        /// Visits the nest host class of the class. A nest is a set of classes of the same package that
        /// share access to their private members. One of these classes, called the host, lists the other
        /// members of the nest, which in turn should link to the host of their nest. This method must be
        /// called only once and only if the visited class is a non-host member of a nest. A class is
        /// implicitly its own nest, so it's invalid to call this method with the visited class name as
        /// argument.
        /// </summary>
        /// <param name="nestHost"> the internal name of the host class of the nest. </param>
        public virtual void VisitNestHost(string nestHost)
        {
            if (api < IOpcodes.Asm7)
            {
                throw new System.NotSupportedException("NestHost requires ASM7");
            }

            if (cv != null)
            {
                cv.VisitNestHost(nestHost);
            }
        }

        /// <summary>
        /// Visits the enclosing class of the class. This method must be called only if the class has an
        /// enclosing class.
        /// </summary>
        /// <param name="owner"> internal name of the enclosing class of the class. </param>
        /// <param name="name"> the name of the method that contains the class, or {@literal null} if the class is
        ///     not enclosed in a method of its enclosing class. </param>
        /// <param name="descriptor"> the descriptor of the method that contains the class, or {@literal null} if
        ///     the class is not enclosed in a method of its enclosing class. </param>
        public virtual void VisitOuterClass(string owner, string name, string descriptor)
        {
            if (cv != null)
            {
                cv.VisitOuterClass(owner, name, descriptor);
            }
        }

        /// <summary>
        /// Visits an annotation of the class.
        /// </summary>
        /// <param name="descriptor"> the class descriptor of the annotation class. </param>
        /// <param name="visible"> {@literal true} if the annotation is visible at runtime. </param>
        /// <returns> a visitor to visit the annotation Values, or {@literal null} if this visitor is not
        ///     interested in visiting this annotation. </returns>
        public virtual AnnotationVisitor VisitAnnotation(string descriptor, bool visible)
        {
            if (cv != null)
            {
                return cv.VisitAnnotation(descriptor, visible);
            }

            return null;
        }

        /// <summary>
        /// Visits an annotation on a type in the class signature.
        /// </summary>
        /// <param name="typeRef"> a reference to the annotated type. The sort of this type reference must be
        ///     <seealso cref="TypeReference.Class_Type_Parameter"/>, {@link
        ///     TypeReference#CLASS_TYPE_PARAMETER_BOUND} or <seealso cref="TypeReference.Class_Extends"/>. See
        ///     <seealso cref="TypeReference"/>. </param>
        /// <param name="typePath"> the path to the annotated type argument, wildcard bound, array element type, or
        ///     static inner type within 'typeRef'. May be {@literal null} if the annotation targets
        ///     'typeRef' as a whole. </param>
        /// <param name="descriptor"> the class descriptor of the annotation class. </param>
        /// <param name="visible"> {@literal true} if the annotation is visible at runtime. </param>
        /// <returns> a visitor to visit the annotation Values, or {@literal null} if this visitor is not
        ///     interested in visiting this annotation. </returns>
        public virtual AnnotationVisitor VisitTypeAnnotation(int typeRef, TypePath typePath, string descriptor,
            bool visible)
        {
            if (api < IOpcodes.Asm5)
            {
                throw new System.NotSupportedException("TypeAnnotation requires ASM5");
            }

            if (cv != null)
            {
                return cv.VisitTypeAnnotation(typeRef, typePath, descriptor, visible);
            }

            return null;
        }

        /// <summary>
        /// Visits a non standard attribute of the class.
        /// </summary>
        /// <param name="attribute"> an attribute. </param>
        public virtual void VisitAttribute(Attribute attribute)
        {
            if (cv != null)
            {
                cv.VisitAttribute(attribute);
            }
        }

        /// <summary>
        /// Visits a member of the nest. A nest is a set of classes of the same package that share access
        /// to their private members. One of these classes, called the host, lists the other members of the
        /// nest, which in turn should link to the host of their nest. This method must be called only if
        /// the visited class is the host of a nest. A nest host is implicitly a member of its own nest, so
        /// it's invalid to call this method with the visited class name as argument.
        /// </summary>
        /// <param name="nestMember"> the internal name of a nest member. </param>
        public virtual void VisitNestMember(string nestMember)
        {
            if (api < IOpcodes.Asm7)
            {
                throw new System.NotSupportedException("NestMember requires ASM7");
            }

            if (cv != null)
            {
                cv.VisitNestMember(nestMember);
            }
        }

        /// <summary>
        /// Visits a permitted subclasses. A permitted subclass is one of the allowed subclasses of the
        /// current class.
        /// </summary>
        /// <param name="permittedSubclass"> the internal name of a permitted subclass. </param>
        public virtual void VisitPermittedSubclass(string permittedSubclass)
        {
            if (api < IOpcodes.Asm9)
            {
                throw new System.NotSupportedException("PermittedSubclasses requires ASM9");
            }

            if (cv != null)
            {
                cv.VisitPermittedSubclass(permittedSubclass);
            }
        }

        /// <summary>
        /// Visits information about an inner class. This inner class is not necessarily a member of the
        /// class being visited.
        /// </summary>
        /// <param name="name"> the internal name of an inner class (see <seealso cref="Type.InternalName"/>). </param>
        /// <param name="outerName"> the internal name of the class to which the inner class belongs (see {@link
        ///     Type#getInternalName()}). May be {@literal null} for not member classes. </param>
        /// <param name="innerName"> the (simple) name of the inner class inside its enclosing class. May be
        ///     {@literal null} for anonymous inner classes. </param>
        /// <param name="access"> the access flags of the inner class as originally declared in the enclosing
        ///     class. </param>
        public virtual void VisitInnerClass(string name, string outerName, string innerName, int access)
        {
            if (cv != null)
            {
                cv.VisitInnerClass(name, outerName, innerName, access);
            }
        }

        /// <summary>
        /// Visits a record component of the class.
        /// </summary>
        /// <param name="name"> the record component name. </param>
        /// <param name="descriptor"> the record component descriptor (see <seealso cref="Type"/>). </param>
        /// <param name="signature"> the record component signature. May be {@literal null} if the record component
        ///     type does not use generic types. </param>
        /// <returns> a visitor to visit this record component annotations and attributes, or {@literal null}
        ///     if this class visitor is not interested in visiting these annotations and attributes. </returns>
        public virtual RecordComponentVisitor VisitRecordComponent(string name, string descriptor, string signature)
        {
            if (api < IOpcodes.Asm8)
            {
                throw new System.NotSupportedException("Record requires ASM8");
            }

            if (cv != null)
            {
                return cv.VisitRecordComponent(name, descriptor, signature);
            }

            return null;
        }

        /// <summary>
        /// Visits a field of the class.
        /// </summary>
        /// <param name="access"> the field's access flags (see <seealso cref="IOpcodes"/>). This parameter also indicates if
        ///     the field is synthetic and/or deprecated. </param>
        /// <param name="name"> the field's name. </param>
        /// <param name="descriptor"> the field's descriptor (see <seealso cref="Type"/>). </param>
        /// <param name="signature"> the field's signature. May be {@literal null} if the field's type does not use
        ///     generic types. </param>
        /// <param name="value"> the field's initial value. This parameter, which may be {@literal null} if the
        ///     field does not have an initial value, must be an <seealso cref="Integer"/>, a <seealso cref="Float"/>, a {@link
        ///     Long}, a <seealso cref="Double"/> or a <seealso cref="string"/> (for {@code int}, {@code float}, {@code long}
        ///     or {@code String} fields respectively). <i>This parameter is only used for static
        ///     fields</i>. Its value is ignored for non static fields, which must be initialized through
        ///     bytecode instructions in constructors or methods. </param>
        /// <returns> a visitor to visit field annotations and attributes, or {@literal null} if this class
        ///     visitor is not interested in visiting these annotations and attributes. </returns>
        public virtual FieldVisitor VisitField(int access, string name, string descriptor, string signature,
            object value)
        {
            if (cv != null)
            {
                return cv.VisitField(access, name, descriptor, signature, value);
            }

            return null;
        }

        /// <summary>
        /// Visits a method of the class. This method <i>must</i> return a new <seealso cref="MethodVisitor"/>
        /// instance (or {@literal null}) each time it is called, i.e., it should not return a previously
        /// returned visitor.
        /// </summary>
        /// <param name="access"> the method's access flags (see <seealso cref="IOpcodes"/>). This parameter also indicates if
        ///     the method is synthetic and/or deprecated. </param>
        /// <param name="name"> the method's name. </param>
        /// <param name="descriptor"> the method's descriptor (see <seealso cref="Type"/>). </param>
        /// <param name="signature"> the method's signature. May be {@literal null} if the method parameters,
        ///     return type and exceptions do not use generic types. </param>
        /// <param name="exceptions"> the internal names of the method's exception classes (see {@link
        ///     Type#getInternalName()}). May be {@literal null}. </param>
        /// <returns> an object to visit the byte code of the method, or {@literal null} if this class
        ///     visitor is not interested in visiting the code of this method. </returns>
        public virtual MethodVisitor VisitMethod(int access, string name, string descriptor, string signature,
            string[] exceptions)
        {
            if (cv != null)
            {
                return cv.VisitMethod(access, name, descriptor, signature, exceptions);
            }

            return null;
        }

        /// <summary>
        /// Visits the end of the class. This method, which is the last one to be called, is used to inform
        /// the visitor that all the fields and methods of the class have been visited.
        /// </summary>
        public virtual void VisitEnd()
        {
            if (cv != null)
            {
                cv.VisitEnd();
            }
        }
    }
}