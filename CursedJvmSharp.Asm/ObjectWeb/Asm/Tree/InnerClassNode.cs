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
    /// A node that represents an inner class.
    /// 
    /// @author Eric Bruneton
    /// </summary>
    public class InnerClassNode
    {
        /// <summary>
        /// The internal name of an inner class (see <seealso cref = "org.objectweb.asm.Type.InternalName"/>). </summary>
        public string Name { get; set; }

        /// <summary>
        /// The internal name of the class to which the inner class belongs (see {@link
        /// org.objectweb.asm.Type#getInternalName()}). May be {@literal null}.
        /// </summary>
        public string OuterName { get; set; }

        /// <summary>
        /// The (simple) name of the inner class inside its enclosing class. May be {@literal null} for
        /// anonymous inner classes.
        /// </summary>
        public string InnerName { get; set; }

        /// <summary>
        /// The access flags of the inner class as originally declared in the enclosing class. </summary>
        public int Access { get; set; }

        /// <summary>
        /// Constructs a new <seealso cref = "InnerClassNode"/>.
        /// </summary>
        /// <param name = "name"> the internal name of an inner class (see {@link
        ///     org.objectweb.asm.Type#getInternalName()}). </param>
        /// <param name = "outerName"> the internal name of the class to which the inner class belongs (see {@link
        ///     org.objectweb.asm.Type#getInternalName()}). May be {@literal null}. </param>
        /// <param name = "innerName"> the (simple) name of the inner class inside its enclosing class. May be
        ///     {@literal null} for anonymous inner classes. </param>
        /// <param name = "access"> the access flags of the inner class as originally declared in the enclosing
        ///     class. </param>
        public InnerClassNode(string name, string outerName, string innerName, int access)
        {
            this.Name = name;
            this.OuterName = outerName;
            this.InnerName = innerName;
            this.Access = access;
        }

        /// <summary>
        /// Makes the given class visitor visit this inner class.
        /// </summary>
        /// <param name = "classVisitor"> a class visitor. </param>
        public virtual void Accept(ClassVisitor classVisitor)
        {
            classVisitor.VisitInnerClass(name, outerName, innerName, access);
        }
    }
}