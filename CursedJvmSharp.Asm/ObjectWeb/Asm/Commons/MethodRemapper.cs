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
    ///     A <seealso cref="MethodVisitor" /> that remaps types with a <seealso cref="Remapper" />.
    ///     @author Eugene Kuleshov
    /// </summary>
    public class MethodRemapper : MethodVisitor
    {
        /// <summary>
        ///     The remapper used to remap the types in the visited field.
        /// </summary>
        protected internal readonly Remapper remapper;

        /// <summary>
        ///     Constructs a new <seealso cref="MethodRemapper" />. <i>Subclasses must not use this constructor</i>.
        ///     Instead, they must use the <seealso cref="MethodRemapper(int,MethodVisitor,Remapper)" /> version.
        /// </summary>
        /// <param name="methodVisitor"> the method visitor this remapper must delegate to. </param>
        /// <param name="remapper"> the remapper to use to remap the types in the visited method. </param>
        public MethodRemapper(MethodVisitor methodVisitor, Remapper remapper) : this(IOpcodes.Asm9, methodVisitor,
            remapper)
        {
        }

        /// <summary>
        ///     Constructs a new <seealso cref="MethodRemapper" />.
        /// </summary>
        /// <param name="api">
        ///     the ASM API version supported by this remapper. Must be one of the {@code
        ///     ASM}<i>x</i> values in <seealso cref="IOpcodes" />.
        /// </param>
        /// <param name="methodVisitor"> the method visitor this remapper must delegate to. </param>
        /// <param name="remapper"> the remapper to use to remap the types in the visited method. </param>
        public MethodRemapper(int api, MethodVisitor methodVisitor, Remapper remapper) : base(api, methodVisitor)
        {
            this.remapper = remapper;
        }

        public override AnnotationVisitor VisitAnnotationDefault()
        {
            var annotationVisitor = base.VisitAnnotationDefault();
            return annotationVisitor == null ? annotationVisitor : CreateAnnotationRemapper(null, annotationVisitor);
        }

        public override AnnotationVisitor VisitAnnotation(string descriptor, bool visible)
        {
            var annotationVisitor = base.VisitAnnotation(remapper.MapDesc(descriptor), visible);
            return annotationVisitor == null
                ? annotationVisitor
                : CreateAnnotationRemapper(descriptor, annotationVisitor);
        }

        public override AnnotationVisitor VisitTypeAnnotation(int typeRef, TypePath typePath, string descriptor,
            bool visible)
        {
            var annotationVisitor = base.VisitTypeAnnotation(typeRef, typePath, remapper.MapDesc(descriptor), visible);
            return annotationVisitor == null
                ? annotationVisitor
                : CreateAnnotationRemapper(descriptor, annotationVisitor);
        }

        public override AnnotationVisitor VisitParameterAnnotation(int parameter, string descriptor, bool visible)
        {
            var annotationVisitor = base.VisitParameterAnnotation(parameter, remapper.MapDesc(descriptor), visible);
            return annotationVisitor == null
                ? annotationVisitor
                : CreateAnnotationRemapper(descriptor, annotationVisitor);
        }

        public override void VisitFrame(int type, int numLocal, object[] local, int numStack, object[] stack)
        {
            base.VisitFrame(type, numLocal, RemapFrameTypes(numLocal, local), numStack,
                RemapFrameTypes(numStack, stack));
        }

        private object[] RemapFrameTypes(int numTypes, object[] frameTypes)
        {
            if (frameTypes == null) return frameTypes;
            object[] remappedFrameTypes = null;
            for (var i = 0; i < numTypes; ++i)
                if (frameTypes[i] is string)
                {
                    if (remappedFrameTypes == null)
                    {
                        remappedFrameTypes = new object[numTypes];
                        Array.Copy(frameTypes, 0, remappedFrameTypes, 0, numTypes);
                    }

                    remappedFrameTypes[i] = remapper.MapType((string)frameTypes[i]);
                }

            return remappedFrameTypes == null ? frameTypes : remappedFrameTypes;
        }

        public override void VisitFieldInsn(int opcode, string owner, string name, string descriptor)
        {
            base.VisitFieldInsn(opcode, remapper.MapType(owner), remapper.MapFieldName(owner, name, descriptor),
                remapper.MapDesc(descriptor));
        }

        public override void VisitMethodInsn(int opcodeAndSource, string owner, string name, string descriptor,
            bool isInterface)
        {
            if (api < IOpcodes.Asm5 && (opcodeAndSource & IOpcodes.Source_Deprecated) == 0)
            {
                // Redirect the call to the deprecated version of this method.
                base.VisitMethodInsn(opcodeAndSource, owner, name, descriptor, isInterface);
                return;
            }

            base.VisitMethodInsn(opcodeAndSource, remapper.MapType(owner),
                remapper.MapMethodName(owner, name, descriptor), remapper.MapMethodDesc(descriptor), isInterface);
        }

        public override void VisitInvokeDynamicInsn(string name, string descriptor, Handle bootstrapMethodHandle,
            params object[] bootstrapMethodArguments)
        {
            var remappedBootstrapMethodArguments = new object[bootstrapMethodArguments.Length];
            for (var i = 0; i < bootstrapMethodArguments.Length; ++i)
                remappedBootstrapMethodArguments[i] = remapper.MapValue(bootstrapMethodArguments[i]);
            base.VisitInvokeDynamicInsn(remapper.MapInvokeDynamicMethodName(name, descriptor),
                remapper.MapMethodDesc(descriptor), (Handle)remapper.MapValue(bootstrapMethodHandle),
                remappedBootstrapMethodArguments);
        }

        public override void VisitTypeInsn(int opcode, string type)
        {
            base.VisitTypeInsn(opcode, remapper.MapType(type));
        }

        public override void VisitLdcInsn(object value)
        {
            base.VisitLdcInsn(remapper.MapValue(value));
        }

        public override void VisitMultiANewArrayInsn(string descriptor, int numDimensions)
        {
            base.VisitMultiANewArrayInsn(remapper.MapDesc(descriptor), numDimensions);
        }

        public override AnnotationVisitor VisitInsnAnnotation(int typeRef, TypePath typePath, string descriptor,
            bool visible)
        {
            var annotationVisitor = base.VisitInsnAnnotation(typeRef, typePath, remapper.MapDesc(descriptor), visible);
            return annotationVisitor == null
                ? annotationVisitor
                : CreateAnnotationRemapper(descriptor, annotationVisitor);
        }

        public override void VisitTryCatchBlock(Label start, Label end, Label handler, string type)
        {
            base.VisitTryCatchBlock(start, end, handler, ReferenceEquals(type, null) ? null : remapper.MapType(type));
        }

        public override AnnotationVisitor VisitTryCatchAnnotation(int typeRef, TypePath typePath, string descriptor,
            bool visible)
        {
            var annotationVisitor =
                base.VisitTryCatchAnnotation(typeRef, typePath, remapper.MapDesc(descriptor), visible);
            return annotationVisitor == null
                ? annotationVisitor
                : CreateAnnotationRemapper(descriptor, annotationVisitor);
        }

        public override void VisitLocalVariable(string name, string descriptor, string signature, Label start,
            Label end, int index)
        {
            base.VisitLocalVariable(name, remapper.MapDesc(descriptor), remapper.MapSignature(signature, true), start,
                end, index);
        }

        public override AnnotationVisitor VisitLocalVariableAnnotation(int typeRef, TypePath typePath, Label[] start,
            Label[] end, int[] index, string descriptor, bool visible)
        {
            var annotationVisitor = base.VisitLocalVariableAnnotation(typeRef, typePath, start, end, index,
                remapper.MapDesc(descriptor), visible);
            return annotationVisitor == null
                ? annotationVisitor
                : CreateAnnotationRemapper(descriptor, annotationVisitor);
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
    }
}