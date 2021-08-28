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

using System;

namespace ObjectWeb.Asm
{
    /// <summary>
    ///     A visitor to visit a record component. The methods of this class must be called in the following
    ///     order: ( {@code visitAnnotation} | {@code visitTypeAnnotation} | {@code visitAttribute} )* {@code
    ///     visitEnd}.
    ///     @author Remi Forax
    ///     @author Eric Bruneton
    /// </summary>
    public abstract class RecordComponentVisitor
    {
        /// <summary>
        ///     The ASM API version implemented by this visitor. The value of this field must be one of {@link
        ///     Opcodes#ASM8} or <seealso cref="Opcodes.ASM9" />.
        /// </summary>
        protected internal readonly int api;

        /// <summary>
        ///     The record visitor to which this visitor must delegate method calls. May be {@literal null}.
        /// </summary>
        /*package-private*/
        internal RecordComponentVisitor @delegate;

        /// <summary>
        ///     Constructs a new <seealso cref="RecordComponentVisitor" />.
        /// </summary>
        /// <param name="api">
        ///     the ASM API version implemented by this visitor. Must be one of <seealso cref="Opcodes.ASM8" />
        ///     or <seealso cref="Opcodes.ASM9" />.
        /// </param>
        public RecordComponentVisitor(int api) : this(api, null)
        {
        }

        /// <summary>
        ///     Constructs a new <seealso cref="RecordComponentVisitor" />.
        /// </summary>
        /// <param name="api"> the ASM API version implemented by this visitor. Must be <seealso cref="Opcodes.ASM8" />. </param>
        /// <param name="recordComponentVisitor">
        ///     the record component visitor to which this visitor must delegate
        ///     method calls. May be null.
        /// </param>
        public RecordComponentVisitor(int api, RecordComponentVisitor recordComponentVisitor)
        {
            if (api != Opcodes.ASM9 && api != Opcodes.ASM8 && api != Opcodes.ASM7 && api != Opcodes.ASM6 &&
                api != Opcodes.ASM5 && api != Opcodes.ASM4 &&
                api != Opcodes.ASM10_EXPERIMENTAL) throw new ArgumentException("Unsupported api " + api);
            if (api == Opcodes.ASM10_EXPERIMENTAL) Constants.checkAsmExperimental(this);
            this.api = api;
            @delegate = recordComponentVisitor;
        }

        /// <summary>
        ///     The record visitor to which this visitor must delegate method calls. May be {@literal null}.
        /// </summary>
        /// <returns> the record visitor to which this visitor must delegate method calls or {@literal null}. </returns>
        public virtual RecordComponentVisitor Delegate => @delegate;

        /// <summary>
        ///     Visits an annotation of the record component.
        /// </summary>
        /// <param name="descriptor"> the class descriptor of the annotation class. </param>
        /// <param name="visible"> {@literal true} if the annotation is visible at runtime. </param>
        /// <returns>
        ///     a visitor to visit the annotation values, or {@literal null} if this visitor is not
        ///     interested in visiting this annotation.
        /// </returns>
        public virtual AnnotationVisitor visitAnnotation(string descriptor, bool visible)
        {
            if (@delegate != null) return @delegate.visitAnnotation(descriptor, visible);
            return null;
        }

        /// <summary>
        ///     Visits an annotation on a type in the record component signature.
        /// </summary>
        /// <param name="typeRef">
        ///     a reference to the annotated type. The sort of this type reference must be
        ///     <seealso cref="TypeReference.CLASS_TYPE_PARAMETER" />, {@link
        ///     TypeReference#CLASS_TYPE_PARAMETER_BOUND} or <seealso cref="TypeReference.CLASS_EXTENDS" />. See
        ///     <seealso cref="TypeReference" />.
        /// </param>
        /// <param name="typePath">
        ///     the path to the annotated type argument, wildcard bound, array element type, or
        ///     static inner type within 'typeRef'. May be {@literal null} if the annotation targets
        ///     'typeRef' as a whole.
        /// </param>
        /// <param name="descriptor"> the class descriptor of the annotation class. </param>
        /// <param name="visible"> {@literal true} if the annotation is visible at runtime. </param>
        /// <returns>
        ///     a visitor to visit the annotation values, or {@literal null} if this visitor is not
        ///     interested in visiting this annotation.
        /// </returns>
        public virtual AnnotationVisitor visitTypeAnnotation(int typeRef, TypePath typePath, string descriptor,
            bool visible)
        {
            if (@delegate != null) return @delegate.visitTypeAnnotation(typeRef, typePath, descriptor, visible);
            return null;
        }

        /// <summary>
        ///     Visits a non standard attribute of the record component.
        /// </summary>
        /// <param name="attribute"> an attribute. </param>
        public virtual void visitAttribute(Attribute attribute)
        {
            if (@delegate != null) @delegate.visitAttribute(attribute);
        }

        /// <summary>
        ///     Visits the end of the record component. This method, which is the last one to be called, is
        ///     used to inform the visitor that everything have been visited.
        /// </summary>
        public virtual void visitEnd()
        {
            if (@delegate != null) @delegate.visitEnd();
        }
    }
}