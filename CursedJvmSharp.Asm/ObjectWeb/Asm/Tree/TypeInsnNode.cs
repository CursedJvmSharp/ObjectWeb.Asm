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
    /// A node that represents a type instruction. A type instruction is an instruction that takes a type
    /// descriptor as parameter.
    /// 
    /// @author Eric Bruneton
    /// </summary>
    public class TypeInsnNode : AbstractInsnNode
    {
        /// <summary>
        /// The operand of this instruction. This operand is an internal name (see {@link
        /// org.objectweb.asm.Type}).
        /// </summary>
        public string Desc { get; set; }

        /// <summary>
        /// Constructs a new <seealso cref = "TypeInsnNode"/>.
        /// </summary>
        /// <param name = "opcode"> the opcode of the type instruction to be constructed. This opcode must be NEW,
        ///     ANEWARRAY, CHECKCAST or INSTANCEOF. </param>
        /// <param name = "descriptor"> the operand of the instruction to be constructed. This operand is an internal
        ///     name (see <seealso cref = "org.objectweb.asm.Type"/>). </param>
        public TypeInsnNode(int opcode, string descriptor): base(opcode)
        {
            this.Desc = descriptor;
        }

        /// <summary>
        /// Sets the opcode of this instruction.
        /// </summary>
        /// <param name = "opcode"> the new instruction opcode. This opcode must be NEW, ANEWARRAY, CHECKCAST or
        ///     INSTANCEOF. </param>
        public virtual int Opcode { set => this.opcode = value; }

        public override int Type => Type_Insn;
        public override void Accept(MethodVisitor methodVisitor)
        {
            methodVisitor.VisitTypeInsn(opcode, desc);
            AcceptAnnotations(methodVisitor);
        }

        public override AbstractInsnNode Clone(IDictionary<LabelNode, LabelNode> clonedLabels)
        {
            return (new TypeInsnNode(opcode, desc)).CloneAnnotations(this);
        }
    }
}