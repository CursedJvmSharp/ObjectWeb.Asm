using org.objectweb.asm;
using CursedJvmSharp.Asm.Java.IO;
using ConstantDynamic = org.objectweb.asm.ConstantDynamic;
using Handle = org.objectweb.asm.Handle;
using Label = org.objectweb.asm.Label;
using MethodVisitor = org.objectweb.asm.MethodVisitor;
using Opcodes = org.objectweb.asm.Opcodes;

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
namespace org.objectweb.asm.commons
{

	/// <summary>
	/// A <seealso cref="MethodVisitor"/> that approximates the size of the methods it visits.
	/// 
	/// @author Eugene Kuleshov
	/// </summary>
	public class CodeSizeEvaluator : MethodVisitor, Opcodes
	{

	  /// <summary>
	  /// The minimum size in bytes of the visited method. </summary>
	  private int minSize;

	  /// <summary>
	  /// The maximum size in bytes of the visited method. </summary>
	  private int maxSize;

	  public CodeSizeEvaluator(MethodVisitor methodVisitor) : this(Opcodes.ASM9, methodVisitor)
	  {
	  }

	  public CodeSizeEvaluator(int api, MethodVisitor methodVisitor) : base(api, methodVisitor)
	  {
	  }

	  public virtual int MinSize
	  {
		  get
		  {
			return this.minSize;
		  }
	  }

	  public virtual int MaxSize
	  {
		  get
		  {
			return this.maxSize;
		  }
	  }

	  public override void visitInsn(int opcode)
	  {
		minSize += 1;
		maxSize += 1;
		base.visitInsn(opcode);
	  }

	  public override void visitIntInsn(int opcode, int operand)
	  {
		if (opcode == SIPUSH)
		{
		  minSize += 3;
		  maxSize += 3;
		}
		else
		{
		  minSize += 2;
		  maxSize += 2;
		}
		base.visitIntInsn(opcode, operand);
	  }

	  public override void visitVarInsn(int opcode, int var)
	  {
		if (var < 4 && opcode != RET)
		{
		  minSize += 1;
		  maxSize += 1;
		}
		else if (var >= 256)
		{
		  minSize += 4;
		  maxSize += 4;
		}
		else
		{
		  minSize += 2;
		  maxSize += 2;
		}
		base.visitVarInsn(opcode, var);
	  }

	  public override void visitTypeInsn(int opcode, string type)
	  {
		minSize += 3;
		maxSize += 3;
		base.visitTypeInsn(opcode, type);
	  }

	  public override void visitFieldInsn(int opcode, string owner, string name, string descriptor)
	  {
		minSize += 3;
		maxSize += 3;
		base.visitFieldInsn(opcode, owner, name, descriptor);
	  }

	  public override void visitMethodInsn(int opcodeAndSource, string owner, string name, string descriptor, bool isInterface)
	  {
		if (api < Opcodes.ASM5 && (opcodeAndSource & Opcodes.SOURCE_DEPRECATED) == 0)
		{
		  // Redirect the call to the deprecated version of this method.
		  base.visitMethodInsn(opcodeAndSource, owner, name, descriptor, isInterface);
		  return;
		}
		int opcode = opcodeAndSource & ~Opcodes.SOURCE_MASK;

		if (opcode == INVOKEINTERFACE)
		{
		  minSize += 5;
		  maxSize += 5;
		}
		else
		{
		  minSize += 3;
		  maxSize += 3;
		}
		base.visitMethodInsn(opcodeAndSource, owner, name, descriptor, isInterface);
	  }

	  public override void visitInvokeDynamicInsn(string name, string descriptor, Handle bootstrapMethodHandle, params object[] bootstrapMethodArguments)
	  {
		minSize += 5;
		maxSize += 5;
		base.visitInvokeDynamicInsn(name, descriptor, bootstrapMethodHandle, bootstrapMethodArguments);
	  }

	  public override void visitJumpInsn(int opcode, Label label)
	  {
		minSize += 3;
		if (opcode == GOTO || opcode == JSR)
		{
		  maxSize += 5;
		}
		else
		{
		  maxSize += 8;
		}
		base.visitJumpInsn(opcode, label);
	  }

	  public override void visitLdcInsn(object value)
	  {
		if (value is long? || value is double? || (value is ConstantDynamic && ((ConstantDynamic) value).Size == 2))
		{
		  minSize += 3;
		  maxSize += 3;
		}
		else
		{
		  minSize += 2;
		  maxSize += 3;
		}
		base.visitLdcInsn(value);
	  }

	  public override void visitIincInsn(int var, int increment)
	  {
		if (var > 255 || increment > 127 || increment < -128)
		{
		  minSize += 6;
		  maxSize += 6;
		}
		else
		{
		  minSize += 3;
		  maxSize += 3;
		}
		base.visitIincInsn(var, increment);
	  }

	  public override void visitTableSwitchInsn(int min, int max, Label dflt, params Label[] labels)
	  {
		minSize += 13 + labels.Length * 4;
		maxSize += 16 + labels.Length * 4;
		base.visitTableSwitchInsn(min, max, dflt, labels);
	  }

	  public override void visitLookupSwitchInsn(Label dflt, int[] keys, Label[] labels)
	  {
		minSize += 9 + keys.Length * 8;
		maxSize += 12 + keys.Length * 8;
		base.visitLookupSwitchInsn(dflt, keys, labels);
	  }

	  public override void visitMultiANewArrayInsn(string descriptor, int numDimensions)
	  {
		minSize += 4;
		maxSize += 4;
		base.visitMultiANewArrayInsn(descriptor, numDimensions);
	  }
	}

}