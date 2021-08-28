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
    ///     A <seealso cref="MethodVisitor" /> that approximates the size of the methods it visits.
    ///     @author Eugene Kuleshov
    /// </summary>
    public class CodeSizeEvaluator : MethodVisitor, IOpcodes
    {
        /// <summary>
        ///     The maximum size in bytes of the visited method.
        /// </summary>
        private int _maxSize;

        /// <summary>
        ///     The minimum size in bytes of the visited method.
        /// </summary>
        private int _minSize;

        public CodeSizeEvaluator(MethodVisitor methodVisitor) : this(IOpcodes.Asm9, methodVisitor)
        {
        }

        public CodeSizeEvaluator(int api, MethodVisitor methodVisitor) : base(api, methodVisitor)
        {
        }

        public virtual int MinSize => _minSize;

        public virtual int MaxSize => _maxSize;

        public override void VisitInsn(int opcode)
        {
            _minSize += 1;
            _maxSize += 1;
            base.VisitInsn(opcode);
        }

        public override void VisitIntInsn(int opcode, int operand)
        {
            if (opcode == IOpcodes.Sipush)
            {
                _minSize += 3;
                _maxSize += 3;
            }
            else
            {
                _minSize += 2;
                _maxSize += 2;
            }

            base.VisitIntInsn(opcode, operand);
        }

        public override void VisitVarInsn(int opcode, int var)
        {
            if (var < 4 && opcode != IOpcodes.Ret)
            {
                _minSize += 1;
                _maxSize += 1;
            }
            else if (var >= 256)
            {
                _minSize += 4;
                _maxSize += 4;
            }
            else
            {
                _minSize += 2;
                _maxSize += 2;
            }

            base.VisitVarInsn(opcode, var);
        }

        public override void VisitTypeInsn(int opcode, string type)
        {
            _minSize += 3;
            _maxSize += 3;
            base.VisitTypeInsn(opcode, type);
        }

        public override void VisitFieldInsn(int opcode, string owner, string name, string descriptor)
        {
            _minSize += 3;
            _maxSize += 3;
            base.VisitFieldInsn(opcode, owner, name, descriptor);
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

            var opcode = opcodeAndSource & ~IOpcodes.Source_Mask;

            if (opcode == IOpcodes.Invokeinterface)
            {
                _minSize += 5;
                _maxSize += 5;
            }
            else
            {
                _minSize += 3;
                _maxSize += 3;
            }

            base.VisitMethodInsn(opcodeAndSource, owner, name, descriptor, isInterface);
        }

        public override void VisitInvokeDynamicInsn(string name, string descriptor, Handle bootstrapMethodHandle,
            params object[] bootstrapMethodArguments)
        {
            _minSize += 5;
            _maxSize += 5;
            base.VisitInvokeDynamicInsn(name, descriptor, bootstrapMethodHandle, bootstrapMethodArguments);
        }

        public override void VisitJumpInsn(int opcode, Label label)
        {
            _minSize += 3;
            if (opcode == IOpcodes.Goto || opcode == IOpcodes.Jsr)
                _maxSize += 5;
            else
                _maxSize += 8;
            base.VisitJumpInsn(opcode, label);
        }

        public override void VisitLdcInsn(object value)
        {
            if (value is long? || value is double? || value is ConstantDynamic && ((ConstantDynamic)value).Size == 2)
            {
                _minSize += 3;
                _maxSize += 3;
            }
            else
            {
                _minSize += 2;
                _maxSize += 3;
            }

            base.VisitLdcInsn(value);
        }

        public override void VisitIincInsn(int var, int increment)
        {
            if (var > 255 || increment > 127 || increment < -128)
            {
                _minSize += 6;
                _maxSize += 6;
            }
            else
            {
                _minSize += 3;
                _maxSize += 3;
            }

            base.VisitIincInsn(var, increment);
        }

        public override void VisitTableSwitchInsn(int min, int max, Label dflt, params Label[] labels)
        {
            _minSize += 13 + labels.Length * 4;
            _maxSize += 16 + labels.Length * 4;
            base.VisitTableSwitchInsn(min, max, dflt, labels);
        }

        public override void VisitLookupSwitchInsn(Label dflt, int[] keys, Label[] labels)
        {
            _minSize += 9 + keys.Length * 8;
            _maxSize += 12 + keys.Length * 8;
            base.VisitLookupSwitchInsn(dflt, keys, labels);
        }

        public override void VisitMultiANewArrayInsn(string descriptor, int numDimensions)
        {
            _minSize += 4;
            _maxSize += 4;
            base.VisitMultiANewArrayInsn(descriptor, numDimensions);
        }
    }
}