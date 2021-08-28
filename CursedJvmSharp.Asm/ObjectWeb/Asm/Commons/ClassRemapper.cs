using System;

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

namespace ObjectWeb.Asm.Commons
{
    /// <summary>
    ///     A <seealso cref="ClassVisitor" /> that remaps types with a <seealso cref="Remapper" />.
    ///     <para>
    ///         <i>This visitor has several limitations</i>. A non-exhaustive list is the following:
    ///         <ul>
    ///             <li>
    ///                 it cannot remap type names in dynamically computed strings (remapping of type names in
    ///                 static values is supported).
    ///                 <li>
    ///                     it cannot remap values derived from type names at compile time, such as
    ///                     <ul>
    ///                         <li>
    ///                             type name hashcodes used by some Java compilers to implement the string switch
    ///                             statement.
    ///                             <li>
    ///                                 some compound strings used by some Java compilers to implement lambda
    ///                                 deserialization.
    ///                     </ul>
    ///         </ul>
    ///         @author Eugene Kuleshov
    ///     </para>
    /// </summary>
    public class ClassRemapper : ClassVisitor
    {
        /// <summary>
        ///     The remapper used to remap the types in the visited class.
        /// </summary>
        protected internal readonly Remapper remapper;

        /// <summary>
        ///     The internal name of the visited class.
        /// </summary>
        protected internal string className;

        /// <summary>
        ///     Constructs a new <seealso cref="ClassRemapper" />. <i>Subclasses must not use this constructor</i>.
        ///     Instead, they must use the <seealso cref="ClassRemapper(int,ClassVisitor,Remapper)" /> version.
        /// </summary>
        /// <param name="classVisitor"> the class visitor this remapper must delegate to. </param>
        /// <param name="remapper"> the remapper to use to remap the types in the visited class. </param>
        public ClassRemapper(ClassVisitor classVisitor, Remapper remapper) : this(IOpcodes.Asm9, classVisitor, remapper)
        {
        }

        /// <summary>
        ///     Constructs a new <seealso cref="ClassRemapper" />.
        /// </summary>
        /// <param name="api">
        ///     the ASM API version supported by this remapper. Must be one of the {@code
        ///     ASM}<i>x</i> values in <seealso cref="IOpcodes" />.
        /// </param>
        /// <param name="classVisitor"> the class visitor this remapper must delegate to. </param>
        /// <param name="remapper"> the remapper to use to remap the types in the visited class. </param>
        public ClassRemapper(int api, ClassVisitor classVisitor, Remapper remapper) : base(api, classVisitor)
        {
            this.remapper = remapper;
        }

        public override void Visit(int version, int access, string name, string signature, string superName,
            string[] interfaces)
        {
            className = name;
            base.Visit(version, access, remapper.MapType(name), remapper.MapSignature(signature, false),
                remapper.MapType(superName), interfaces == null ? null : remapper.MapTypes(interfaces));
        }

        public override ModuleVisitor VisitModule(string name, int flags, string version)
        {
            var moduleVisitor = base.VisitModule(remapper.MapModuleName(name), flags, version);
            return moduleVisitor == null ? null : CreateModuleRemapper(moduleVisitor);
        }

        public override AnnotationVisitor VisitAnnotation(string descriptor, bool visible)
        {
            var annotationVisitor = base.VisitAnnotation(remapper.MapDesc(descriptor), visible);
            return annotationVisitor == null ? null : CreateAnnotationRemapper(descriptor, annotationVisitor);
        }

        public override AnnotationVisitor VisitTypeAnnotation(int typeRef, TypePath typePath, string descriptor,
            bool visible)
        {
            var annotationVisitor = base.VisitTypeAnnotation(typeRef, typePath, remapper.MapDesc(descriptor), visible);
            return annotationVisitor == null ? null : CreateAnnotationRemapper(descriptor, annotationVisitor);
        }

        public override void VisitAttribute(Attribute attribute)
        {
            if (attribute is ModuleHashesAttribute)
            {
                var moduleHashesAttribute = (ModuleHashesAttribute)attribute;
                var modules = moduleHashesAttribute.modules;
                for (var i = 0; i < modules.Count; ++i) modules[i] = remapper.MapModuleName(modules[i]);
            }

            base.VisitAttribute(attribute);
        }

        public override RecordComponentVisitor VisitRecordComponent(string name, string descriptor, string signature)
        {
            var recordComponentVisitor = base.VisitRecordComponent(
                remapper.MapRecordComponentName(className, name, descriptor), remapper.MapDesc(descriptor),
                remapper.MapSignature(signature, true));
            return recordComponentVisitor == null ? null : CreateRecordComponentRemapper(recordComponentVisitor);
        }

        public override FieldVisitor VisitField(int access, string name, string descriptor, string signature,
            object value)
        {
            var fieldVisitor = base.VisitField(access, remapper.MapFieldName(className, name, descriptor),
                remapper.MapDesc(descriptor), remapper.MapSignature(signature, true),
                value == null ? null : remapper.MapValue(value));
            return fieldVisitor == null ? null : CreateFieldRemapper(fieldVisitor);
        }

        public override MethodVisitor VisitMethod(int access, string name, string descriptor, string signature,
            string[] exceptions)
        {
            var remappedDescriptor = remapper.MapMethodDesc(descriptor);
            var methodVisitor = base.VisitMethod(access, remapper.MapMethodName(className, name, descriptor),
                remappedDescriptor, remapper.MapSignature(signature, false),
                exceptions == null ? null : remapper.MapTypes(exceptions));
            return methodVisitor == null ? null : CreateMethodRemapper(methodVisitor);
        }

        public override void VisitInnerClass(string name, string outerName, string innerName, int access)
        {
            base.VisitInnerClass(remapper.MapType(name),
                ReferenceEquals(outerName, null) ? null : remapper.MapType(outerName),
                ReferenceEquals(innerName, null) ? null : remapper.MapInnerClassName(name, outerName, innerName),
                access);
        }

        public override void VisitOuterClass(string owner, string name, string descriptor)
        {
            base.VisitOuterClass(remapper.MapType(owner),
                ReferenceEquals(name, null) ? null : remapper.MapMethodName(owner, name, descriptor),
                ReferenceEquals(descriptor, null) ? null : remapper.MapMethodDesc(descriptor));
        }

        public override void VisitNestHost(string nestHost)
        {
            base.VisitNestHost(remapper.MapType(nestHost));
        }

        public override void VisitNestMember(string nestMember)
        {
            base.VisitNestMember(remapper.MapType(nestMember));
        }

        public override void VisitPermittedSubclass(string permittedSubclass)
        {
            base.VisitPermittedSubclass(remapper.MapType(permittedSubclass));
        }

        /// <summary>
        ///     Constructs a new remapper for fields. The default implementation of this method returns a new
        ///     <seealso cref="FieldRemapper" />.
        /// </summary>
        /// <param name="fieldVisitor"> the FieldVisitor the remapper must delegate to. </param>
        /// <returns> the newly created remapper. </returns>
        public virtual FieldVisitor CreateFieldRemapper(FieldVisitor fieldVisitor)
        {
            return new FieldRemapper(api, fieldVisitor, remapper);
        }

        /// <summary>
        ///     Constructs a new remapper for methods. The default implementation of this method returns a new
        ///     <seealso cref="MethodRemapper" />.
        /// </summary>
        /// <param name="methodVisitor"> the MethodVisitor the remapper must delegate to. </param>
        /// <returns> the newly created remapper. </returns>
        public virtual MethodVisitor CreateMethodRemapper(MethodVisitor methodVisitor)
        {
            return new MethodRemapper(api, methodVisitor, remapper);
        }

        /// <summary>
        ///     Constructs a new remapper for annotations. The default implementation of this method returns a
        ///     new <seealso cref="AnnotationRemapper" />.
        /// </summary>
        /// <param name="annotationVisitor"> the AnnotationVisitor the remapper must delegate to. </param>
        /// <returns> the newly created remapper. </returns>
        /// @deprecated use
        /// <seealso cref="CreateAnnotationRemapper(string,ObjectWeb.Asm.AnnotationVisitor)" />
        /// instead.
        [Obsolete("use <seealso cref=\"createAnnotationRemapper(String, AnnotationVisitor)\"/> instead.")]
        public virtual AnnotationVisitor CreateAnnotationRemapper(AnnotationVisitor annotationVisitor)
        {
            return new AnnotationRemapper(api, null, annotationVisitor, remapper);
        }

        /// <summary>
        ///     Constructs a new remapper for annotations. The default implementation of this method returns a
        ///     new <seealso cref="AnnotationRemapper" />.
        /// </summary>
        /// <param name="descriptor"> the descriptor of the visited annotation. </param>
        /// <param name="annotationVisitor"> the AnnotationVisitor the remapper must delegate to. </param>
        /// <returns> the newly created remapper. </returns>
        public virtual AnnotationVisitor CreateAnnotationRemapper(string descriptor,
            AnnotationVisitor annotationVisitor)
        {
            return new AnnotationRemapper(api, descriptor, annotationVisitor, remapper).OrDeprecatedValue(
                CreateAnnotationRemapper(annotationVisitor));
        }

        /// <summary>
        ///     Constructs a new remapper for modules. The default implementation of this method returns a new
        ///     <seealso cref="ModuleRemapper" />.
        /// </summary>
        /// <param name="moduleVisitor"> the ModuleVisitor the remapper must delegate to. </param>
        /// <returns> the newly created remapper. </returns>
        public virtual ModuleVisitor CreateModuleRemapper(ModuleVisitor moduleVisitor)
        {
            return new ModuleRemapper(api, moduleVisitor, remapper);
        }

        /// <summary>
        ///     Constructs a new remapper for record components. The default implementation of this method
        ///     returns a new <seealso cref="RecordComponentRemapper" />.
        /// </summary>
        /// <param name="recordComponentVisitor"> the RecordComponentVisitor the remapper must delegate to. </param>
        /// <returns> the newly created remapper. </returns>
        public virtual RecordComponentVisitor CreateRecordComponentRemapper(
            RecordComponentVisitor recordComponentVisitor)
        {
            return new RecordComponentRemapper(api, recordComponentVisitor, remapper);
        }
    }
}