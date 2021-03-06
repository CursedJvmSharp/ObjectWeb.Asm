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
    /// A node that represents a stack map frame. These nodes are pseudo instruction nodes in order to be
    /// inserted in an instruction list. In fact these nodes must(*) be inserted <i>just before</i> any
    /// instruction node <b>i</b> that follows an unconditionnal branch instruction such as GOTO or
    /// THROW, that is the target of a jump instruction, or that starts an exception handler block. The
    /// stack map frame types must describe the values of the local variables and of the operand stack
    /// elements <i>just before</i> <b>i</b> is executed. <br>
    /// <br>
    /// (*) this is mandatory only for classes whose version is greater than or equal to {@link
    /// Opcodes#V1_6}.
    /// 
    /// @author Eric Bruneton
    /// </summary>
    public class FrameNode : AbstractInsnNode
    {
        /// <summary>
        /// The type of this frame. Must be <seealso cref = "IIOpcodes.F_New / > for  expanded  frames ,  or { @link  ///Opcodes # F_FULL } , <seealso cref = "IIOpcodes.F_Append / > , <seealso cref = "IIOpcodes.F_Chop / > , <seealso cref = "IIOpcodes.F_Same / > or
        /// <seealso cref = "IIOpcodes.F_Append / > , <seealso cref = "IIOpcodes.F_Same1 / > for  compressed  frames .
        /// </summary>
        public int FrameType { get; set; }

        /// <summary>
        /// The types of the local variables of this stack map frame. Elements of this list can be Integer,
        /// String or LabelNode objects (for primitive, reference and uninitialized types respectively -
        /// see <seealso cref = "MethodVisitor"/>).
        /// </summary>
        public List<object> Local { get; set; }

        /// <summary>
        /// The types of the operand stack elements of this stack map frame. Elements of this list can be
        /// Integer, String or LabelNode objects (for primitive, reference and uninitialized types
        /// respectively - see <seealso cref = "MethodVisitor"/>).
        /// </summary>
        public List<object> Stack { get; set; }

        private FrameNode() : base(-1)
        {
        }

        /// <summary>
        /// Constructs a new <seealso cref = "FrameNode"/>.
        /// </summary>
        /// <param name = "type"> the type of this frame. Must be <seealso cref = "IIOpcodes.F_New / > for  expanded  frames ,  or
        ///     <seealso cref = "IIOpcodes.F_Full / > , <seealso cref = "IIOpcodes.F_Append / > , <seealso cref = "IIOpcodes.F_Chop / > ,  { @link  ///Opcodes # F_SAME } or <seealso cref = "IIOpcodes.F_Append / > , <seealso cref = "IIOpcodes.F_Same1 / > for  compressed  frames . </param>
        /// <param name = "numLocal"> number of local variables of this stack map frame. </param>
        /// <param name = "local"> the types of the local variables of this stack map frame. Elements of this list
        ///     can be Integer, String or LabelNode objects (for primitive, reference and uninitialized
        ///     types respectively - see <seealso cref = "MethodVisitor"/>). </param>
        /// <param name = "numStack"> number of operand stack elements of this stack map frame. </param>
        /// <param name = "stack"> the types of the operand stack elements of this stack map frame. Elements of this
        ///     list can be Integer, String or LabelNode objects (for primitive, reference and
        ///     uninitialized types respectively - see <seealso cref = "MethodVisitor"/>). </param>
        public FrameNode(int type, int numLocal, object[] local, int numStack, object[] stack) : base(-1)
        {
            this.FrameType = type;
            switch (type)
            {
                case IOpcodes.F_New:
                case IOpcodes.F_Full:
                    this.Local = Util.AsArrayList(numLocal, local);
                    this.Stack = Util.AsArrayList(numStack, stack);
                    break;
                case IOpcodes.F_Append:
                    this.Local = Util.AsArrayList(numLocal, local);
                    break;
                case IOpcodes.F_Chop:
                    this.Local = Util.AsArrayList<object>(numLocal);
                    break;
                case IOpcodes.F_Same:
                    break;
                case IOpcodes.F_Same1:
                    this.Stack = Util.AsArrayList(1, stack);
                    break;
                default:
                    throw new System.ArgumentException();
            }
        }

        public override int Type => Frame;

        public override void Accept(MethodVisitor methodVisitor)
        {
            switch (FrameType)
            {
                case IOpcodes.F_New:
                case IOpcodes.F_Full:
                    methodVisitor.VisitFrame(FrameType, Local.Count, AsArray(Local), Stack.Count, AsArray(Stack));
                    break;
                case IOpcodes.F_Append:
                    methodVisitor.VisitFrame(FrameType, Local.Count, AsArray(Local), 0, null);
                    break;
                case IOpcodes.F_Chop:
                    methodVisitor.VisitFrame(FrameType, Local.Count, null, 0, null);
                    break;
                case IOpcodes.F_Same:
                    methodVisitor.VisitFrame(FrameType, 0, null, 0, null);
                    break;
                case IOpcodes.F_Same1:
                    methodVisitor.VisitFrame(FrameType, 0, null, 1, AsArray(Stack));
                    break;
                default:
                    throw new System.ArgumentException();
            }
        }

        public override AbstractInsnNode Clone(IDictionary<LabelNode, LabelNode> clonedLabels)
        {
            var clone = new FrameNode();
            clone.FrameType = FrameType;
            if (Local != null)
            {
                clone.Local = new List<object>();
                for (int i = 0, n = Local.Count; i < n; ++i)
                {
                    var localElement = Local[i];
                    if (localElement is LabelNode)
                    {
                        clonedLabels.TryGetValue((LabelNode)localElement, out var ret);
                        localElement = ret;
                    }

                    clone.Local.Add(localElement);
                }
            }

            if (Stack != null)
            {
                clone.Stack = new List<object>();
                for (int i = 0, n = Stack.Count; i < n; ++i)
                {
                    var stackElement = Stack[i];
                    if (stackElement is LabelNode)
                    {
                        clonedLabels.TryGetValue((LabelNode)stackElement, out var ret);
                        stackElement = ret;
                    }

                    clone.Stack.Add(stackElement);
                }
            }

            return clone;
        }

        private static object[] AsArray(List<object> list)
        {
            var array = new object[list.Count];
            for (int i = 0, n = array.Length; i < n; ++i)
            {
                var o = list[i];
                if (o is LabelNode)
                {
                    o = ((LabelNode)o).Label;
                }

                array[i] = o;
            }

            return array;
        }
    }
}