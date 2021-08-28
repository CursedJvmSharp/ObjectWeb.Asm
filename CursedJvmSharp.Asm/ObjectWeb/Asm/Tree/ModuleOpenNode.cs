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
    /// A node that represents an opened package with its name and the module that can access it.
    /// 
    /// @author Remi Forax
    /// </summary>
    public class ModuleOpenNode
    {
        /// <summary>
        /// The internal name of the opened package. </summary>
        public string Packaze { get; set; }

        /// <summary>
        /// The access flag of the opened package, valid values are among {@code ACC_SYNTHETIC} and {@code
        /// ACC_MANDATED}.
        /// </summary>
        public int Access { get; set; }

        /// <summary>
        /// The fully qualified names (using dots) of the modules that can use deep reflection to the
        /// classes of the open package, or {@literal null}.
        /// </summary>
        public List<string> Modules { get; set; }

        /// <summary>
        /// Constructs a new <seealso cref = "ModuleOpenNode"/>.
        /// </summary>
        /// <param name = "packaze"> the internal name of the opened package. </param>
        /// <param name = "access"> the access flag of the opened package, valid values are among {@code
        ///     ACC_SYNTHETIC} and {@code ACC_MANDATED}. </param>
        /// <param name = "modules"> the fully qualified names (using dots) of the modules that can use deep
        ///     reflection to the classes of the open package, or {@literal null}. </param>
        public ModuleOpenNode(string packaze, int access, List<string> modules)
        {
            this.Packaze = packaze;
            this.Access = access;
            this.Modules = modules;
        }

        /// <summary>
        /// Makes the given module visitor visit this opened package.
        /// </summary>
        /// <param name = "moduleVisitor"> a module visitor. </param>
        public virtual void Accept(ModuleVisitor moduleVisitor)
        {
            moduleVisitor.VisitOpen(packaze, access, modules == null ? null : ((List<string>)modules).ToArray());
        }
    }
}