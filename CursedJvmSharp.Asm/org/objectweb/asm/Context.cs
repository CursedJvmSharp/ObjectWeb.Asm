using org.objectweb.asm;
using CursedJvmSharp.Asm.Java.IO;
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

namespace org.objectweb.asm
{
	/// <summary>
	/// Information about a class being parsed in a <seealso cref="ClassReader"/>.
	/// 
	/// @author Eric Bruneton
	/// </summary>
	internal sealed class Context
	{

	  /// <summary>
	  /// The prototypes of the attributes that must be parsed in this class. </summary>
	  internal Attribute[] attributePrototypes;

	  /// <summary>
	  /// The options used to parse this class. One or more of <seealso cref="ClassReader.SKIP_CODE"/>, {@link
	  /// ClassReader#SKIP_DEBUG}, <seealso cref="ClassReader.SKIP_FRAMES"/>, <seealso cref="ClassReader.EXPAND_FRAMES"/> or
	  /// <seealso cref="ClassReader.EXPAND_ASM_INSNS"/>.
	  /// </summary>
	  internal int parsingOptions;

	  /// <summary>
	  /// The buffer used to read strings in the constant pool. </summary>
	  internal char[] charBuffer;

	  // Information about the current method, i.e. the one read in the current (or latest) call
	  // to {@link ClassReader#readMethod()}.

	  /// <summary>
	  /// The access flags of the current method. </summary>
	  internal int currentMethodAccessFlags;

	  /// <summary>
	  /// The name of the current method. </summary>
	  internal string currentMethodName;

	  /// <summary>
	  /// The descriptor of the current method. </summary>
	  internal string currentMethodDescriptor;

	  /// <summary>
	  /// The labels of the current method, indexed by bytecode offset (only bytecode offsets for which a
	  /// label is needed have a non null associated Label).
	  /// </summary>
	  internal Label[] currentMethodLabels;

	  // Information about the current type annotation target, i.e. the one read in the current
	  // (or latest) call to {@link ClassReader#readAnnotationTarget()}.

	  /// <summary>
	  /// The target_type and target_info of the current type annotation target, encoded as described in
	  /// <seealso cref="TypeReference"/>.
	  /// </summary>
	  internal int currentTypeAnnotationTarget;

	  /// <summary>
	  /// The target_path of the current type annotation target. </summary>
	  internal TypePath currentTypeAnnotationTargetPath;

	  /// <summary>
	  /// The start of each local variable range in the current local variable annotation. </summary>
	  internal Label[] currentLocalVariableAnnotationRangeStarts;

	  /// <summary>
	  /// The end of each local variable range in the current local variable annotation. </summary>
	  internal Label[] currentLocalVariableAnnotationRangeEnds;

	  /// <summary>
	  /// The local variable index of each local variable range in the current local variable annotation.
	  /// </summary>
	  internal int[] currentLocalVariableAnnotationRangeIndices;

	  // Information about the current stack map frame, i.e. the one read in the current (or latest)
	  // call to {@link ClassReader#readFrame()}.

	  /// <summary>
	  /// The bytecode offset of the current stack map frame. </summary>
	  internal int currentFrameOffset;

	  /// <summary>
	  /// The type of the current stack map frame. One of <seealso cref="Opcodes.F_FULL"/>, {@link
	  /// Opcodes#F_APPEND}, <seealso cref="Opcodes.F_CHOP"/>, <seealso cref="Opcodes.F_SAME"/> or <seealso cref="Opcodes.F_SAME1"/>.
	  /// </summary>
	  internal int currentFrameType;

	  /// <summary>
	  /// The number of local variable types in the current stack map frame. Each type is represented
	  /// with a single array element (even long and double).
	  /// </summary>
	  internal int currentFrameLocalCount;

	  /// <summary>
	  /// The delta number of local variable types in the current stack map frame (each type is
	  /// represented with a single array element - even long and double). This is the number of local
	  /// variable types in this frame, minus the number of local variable types in the previous frame.
	  /// </summary>
	  internal int currentFrameLocalCountDelta;

	  /// <summary>
	  /// The types of the local variables in the current stack map frame. Each type is represented with
	  /// a single array element (even long and double), using the format described in {@link
	  /// MethodVisitor#visitFrame}. Depending on <seealso cref="currentFrameType"/>, this contains the types of
	  /// all the local variables, or only those of the additional ones (compared to the previous frame).
	  /// </summary>
	  internal object[] currentFrameLocalTypes;

	  /// <summary>
	  /// The number stack element types in the current stack map frame. Each type is represented with a
	  /// single array element (even long and double).
	  /// </summary>
	  internal int currentFrameStackCount;

	  /// <summary>
	  /// The types of the stack elements in the current stack map frame. Each type is represented with a
	  /// single array element (even long and double), using the format described in {@link
	  /// MethodVisitor#visitFrame}.
	  /// </summary>
	  internal object[] currentFrameStackTypes;
	}

}