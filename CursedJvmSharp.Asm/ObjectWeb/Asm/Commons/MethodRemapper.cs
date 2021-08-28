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
        public MethodRemapper(MethodVisitor methodVisitor, Remapper remapper) : this(Opcodes.ASM9, methodVisitor,
            remapper)
        {
        }

        /// <summary>
        ///     Constructs a new <seealso cref="MethodRemapper" />.
        /// </summary>
        /// <param name="api">
        ///     the ASM API version supported by this remapper. Must be one of the {@code
        ///     ASM}<i>x</i> values in <seealso cref="Opcodes" />.
        /// </param>
        /// <param name="methodVisitor"> the method visitor this remapper must delegate to. </param>
        /// <param name="remapper"> the remapper to use to remap the types in the visited method. </param>
        public MethodRemapper(int api, MethodVisitor methodVisitor, Remapper remapper) : base(api, methodVisitor)
        {
            this.remapper = remapper;
        }

        public override AnnotationVisitor visitAnnotationDefault()
        {
            var annotationVisitor = base.visitAnnotationDefault();
            return annotationVisitor == null ? annotationVisitor : createAnnotationRemapper(null, annotationVisitor);
        }

        public override AnnotationVisitor visitAnnotation(string descriptor, bool visible)
        {
            var annotationVisitor = base.visitAnnotation(remapper.mapDesc(descriptor), visible);
            return annotationVisitor == null
                ? annotationVisitor
                : createAnnotationRemapper(descriptor, annotationVisitor);
        }

        public override AnnotationVisitor visitTypeAnnotation(int typeRef, TypePath typePath, string descriptor,
            bool visible)
        {
            var annotationVisitor = base.visitTypeAnnotation(typeRef, typePath, remapper.mapDesc(descriptor), visible);
            return annotationVisitor == null
                ? annotationVisitor
                : createAnnotationRemapper(descriptor, annotationVisitor);
        }

        public override AnnotationVisitor visitParameterAnnotation(int parameter, string descriptor, bool visible)
        {
            var annotationVisitor = base.visitParameterAnnotation(parameter, remapper.mapDesc(descriptor), visible);
            return annotationVisitor == null
                ? annotationVisitor
                : createAnnotationRemapper(descriptor, annotationVisitor);
        }

        public override void visitFrame(int type, int numLocal, object[] local, int numStack, object[] stack)
        {
            base.visitFrame(type, numLocal, remapFrameTypes(numLocal, local), numStack,
                remapFrameTypes(numStack, stack));
        }

        private object[] remapFrameTypes(int numTypes, object[] frameTypes)
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

                    remappedFrameTypes[i] = remapper.mapType((string)frameTypes[i]);
                }

            return remappedFrameTypes == null ? frameTypes : remappedFrameTypes;
        }

        public override void visitFieldInsn(int opcode, string owner, string name, string descriptor)
        {
            base.visitFieldInsn(opcode, remapper.mapType(owner), remapper.mapFieldName(owner, name, descriptor),
                remapper.mapDesc(descriptor));
        }

        public override void visitMethodInsn(int opcodeAndSource, string owner, string name, string descriptor,
            bool isInterface)
        {
            if (api < Opcodes.ASM5 && (opcodeAndSource & Opcodes.SOURCE_DEPRECATED) == 0)
            {
                // Redirect the call to the deprecated version of this method.
                base.visitMethodInsn(opcodeAndSource, owner, name, descriptor, isInterface);
                return;
            }

            base.visitMethodInsn(opcodeAndSource, remapper.mapType(owner),
                remapper.mapMethodName(owner, name, descriptor), remapper.mapMethodDesc(descriptor), isInterface);
        }

        public override void visitInvokeDynamicInsn(string name, string descriptor, Handle bootstrapMethodHandle,
            params object[] bootstrapMethodArguments)
        {
            var remappedBootstrapMethodArguments = new object[bootstrapMethodArguments.Length];
            for (var i = 0; i < bootstrapMethodArguments.Length; ++i)
                remappedBootstrapMethodArguments[i] = remapper.mapValue(bootstrapMethodArguments[i]);
            base.visitInvokeDynamicInsn(remapper.mapInvokeDynamicMethodName(name, descriptor),
                remapper.mapMethodDesc(descriptor), (Handle)remapper.mapValue(bootstrapMethodHandle),
                remappedBootstrapMethodArguments);
        }

        public override void visitTypeInsn(int opcode, string type)
        {
            base.visitTypeInsn(opcode, remapper.mapType(type));
        }

        public override void visitLdcInsn(object value)
        {
            base.visitLdcInsn(remapper.mapValue(value));
        }

        public override void visitMultiANewArrayInsn(string descriptor, int numDimensions)
        {
            base.visitMultiANewArrayInsn(remapper.mapDesc(descriptor), numDimensions);
        }

        public override AnnotationVisitor visitInsnAnnotation(int typeRef, TypePath typePath, string descriptor,
            bool visible)
        {
            var annotationVisitor = base.visitInsnAnnotation(typeRef, typePath, remapper.mapDesc(descriptor), visible);
            return annotationVisitor == null
                ? annotationVisitor
                : createAnnotationRemapper(descriptor, annotationVisitor);
        }

        public override void visitTryCatchBlock(Label start, Label end, Label handler, string type)
        {
            base.visitTryCatchBlock(start, end, handler, ReferenceEquals(type, null) ? null : remapper.mapType(type));
        }

        public override AnnotationVisitor visitTryCatchAnnotation(int typeRef, TypePath typePath, string descriptor,
            bool visible)
        {
            var annotationVisitor =
                base.visitTryCatchAnnotation(typeRef, typePath, remapper.mapDesc(descriptor), visible);
            return annotationVisitor == null
                ? annotationVisitor
                : createAnnotationRemapper(descriptor, annotationVisitor);
        }

        public override void visitLocalVariable(string name, string descriptor, string signature, Label start,
            Label end, int index)
        {
            base.visitLocalVariable(name, remapper.mapDesc(descriptor), remapper.mapSignature(signature, true), start,
                end, index);
        }

        public override AnnotationVisitor visitLocalVariableAnnotation(int typeRef, TypePath typePath, Label[] start,
            Label[] end, int[] index, string descriptor, bool visible)
        {
            var annotationVisitor = base.visitLocalVariableAnnotation(typeRef, typePath, start, end, index,
                remapper.mapDesc(descriptor), visible);
            return annotationVisitor == null
                ? annotationVisitor
                : createAnnotationRemapper(descriptor, annotationVisitor);
        }

        /// <summary>
        ///     Constructs a new remapper for annotations. The default implementation of this method returns a
        ///     new <seealso cref="AnnotationRemapper" />.
        /// </summary>
        /// <param name="annotationVisitor"> the AnnotationVisitor the remapper must delegate to. </param>
        /// <returns> the newly created remapper. </returns>
        /// @deprecated use
        /// <seealso cref="createAnnotationRemapper(string, AnnotationVisitor)" />
        /// instead.
        [Obsolete("use <seealso cref=\"createAnnotationRemapper(String, AnnotationVisitor)\"/> instead.")]
        public virtual AnnotationVisitor createAnnotationRemapper(AnnotationVisitor annotationVisitor)
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
        public virtual AnnotationVisitor createAnnotationRemapper(string descriptor,
            AnnotationVisitor annotationVisitor)
        {
            return new AnnotationRemapper(api, descriptor, annotationVisitor, remapper).orDeprecatedValue(
                createAnnotationRemapper(annotationVisitor));
        }
    }
}