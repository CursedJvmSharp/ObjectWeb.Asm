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
    ///     Exception thrown when the Code attribute of a method produced by a <seealso cref="ClassWriter" /> is too
    ///     large.
    ///     @author Jason Zaugg
    /// </summary>
    public sealed class MethodTooLargeException : Exception
    {
        private const long serialVersionUID = 6807380416709738314L;

        /// <summary>
        ///     Constructs a new <seealso cref="MethodTooLargeException" />.
        /// </summary>
        /// <param name="className"> the internal name of the owner class. </param>
        /// <param name="methodName"> the name of the method. </param>
        /// <param name="descriptor"> the descriptor of the method. </param>
        /// <param name="codeSize"> the size of the method's Code attribute, in bytes. </param>
        public MethodTooLargeException(string className, string methodName, string descriptor, int codeSize) : base(
            "Method too large: " + className + "." + methodName + " " + descriptor)
        {
            this.ClassName = className;
            this.MethodName = methodName;
            this.Descriptor = descriptor;
            this.CodeSize = codeSize;
        }

        /// <summary>
        ///     Returns the internal name of the owner class.
        /// </summary>
        /// <returns> the internal name of the owner class. </returns>
        public string ClassName { get; }

        /// <summary>
        ///     Returns the name of the method.
        /// </summary>
        /// <returns> the name of the method. </returns>
        public string MethodName { get; }

        /// <summary>
        ///     Returns the descriptor of the method.
        /// </summary>
        /// <returns> the descriptor of the method. </returns>
        public string Descriptor { get; }

        /// <summary>
        ///     Returns the size of the method's Code attribute, in bytes.
        /// </summary>
        /// <returns> the size of the method's Code attribute, in bytes. </returns>
        public int CodeSize { get; }
    }
}