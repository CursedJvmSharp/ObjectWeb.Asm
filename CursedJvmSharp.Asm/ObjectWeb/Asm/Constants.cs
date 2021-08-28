using System;
using System.IO;
using System.Text.RegularExpressions;
using Java.IO;

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
namespace ObjectWeb.Asm
{

	/// <summary>
	/// Defines additional JVM opcodes, access flags and constants which are not part of the ASM public
	/// API.
	/// </summary>
	/// <seealso cref= <a href="https://docs.oracle.com/javase/specs/jvms/se11/html/jvms-6.html">JVMS 6</a>
	/// @author Eric Bruneton </seealso>
	internal sealed class Constants
	{

	  // The ClassFile attribute names, in the order they are defined in
	  // https://docs.oracle.com/javase/specs/jvms/se11/html/jvms-4.html#jvms-4.7-300.

	  internal const string Constant_Value = "ConstantValue";
	  internal const string Code = "Code";
	  internal const string Stack_Map_Table = "StackMapTable";
	  internal const string Exceptions = "Exceptions";
	  internal const string Inner_Classes = "InnerClasses";
	  internal const string Enclosing_Method = "EnclosingMethod";
	  internal const string Synthetic = "Synthetic";
	  internal const string Signature = "Signature";
	  internal const string Source_File = "SourceFile";
	  internal const string Source_Debug_Extension = "SourceDebugExtension";
	  internal const string Line_Number_Table = "LineNumberTable";
	  internal const string Local_Variable_Table = "LocalVariableTable";
	  internal const string Local_Variable_Type_Table = "LocalVariableTypeTable";
	  internal const string Deprecated = "Deprecated";
	  internal const string Runtime_Visible_Annotations = "RuntimeVisibleAnnotations";
	  internal const string Runtime_Invisible_Annotations = "RuntimeInvisibleAnnotations";
	  internal const string Runtime_Visible_Parameter_Annotations = "RuntimeVisibleParameterAnnotations";
	  internal const string Runtime_Invisible_Parameter_Annotations = "RuntimeInvisibleParameterAnnotations";
	  internal const string Runtime_Visible_Type_Annotations = "RuntimeVisibleTypeAnnotations";
	  internal const string Runtime_Invisible_Type_Annotations = "RuntimeInvisibleTypeAnnotations";
	  internal const string Annotation_Default = "AnnotationDefault";
	  internal const string Bootstrap_Methods = "BootstrapMethods";
	  internal const string Method_Parameters = "MethodParameters";
	  internal const string Module = "Module";
	  internal const string Module_Packages = "ModulePackages";
	  internal const string Module_Main_Class = "ModuleMainClass";
	  internal const string Nest_Host = "NestHost";
	  internal const string Nest_Members = "NestMembers";
	  internal const string Permitted_Subclasses = "PermittedSubclasses";
	  internal const string Record = "Record";

	  // ASM specific access flags.
	  // WARNING: the 16 least significant bits must NOT be used, to avoid conflicts with standard
	  // access flags, and also to make sure that these flags are automatically filtered out when
	  // written in class files (because access flags are stored using 16 bits only).

	  internal const int Acc_Constructor = 0x40000; // method access flag.

	  // ASM specific stack map frame types, used in {@link ClassVisitor#visitFrame}.

	  /// <summary>
	  /// A frame inserted between already existing frames. This internal stack map frame type (in
	  /// addition to the ones declared in <seealso cref="IOpcodes"/>) can only be used if the frame content can be
	  /// computed from the previous existing frame and from the instructions between this existing frame
	  /// and the inserted one, without any knowledge of the type hierarchy. This kind of frame is only
	  /// used when an unconditional jump is inserted in a method while expanding an ASM specific
	  /// instruction. Keep in sync with Opcodes.java.
	  /// </summary>
	  internal const int F_Insert = 256;

	  // The JVM opcode Values which are not part of the ASM public API.
	  // See https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-6.html.

	  internal const int Ldc_W = 19;
	  internal const int Ldc2_W = 20;
	  internal const int Iload_0 = 26;
	  internal const int Iload_1 = 27;
	  internal const int Iload_2 = 28;
	  internal const int Iload_3 = 29;
	  internal const int Lload_0 = 30;
	  internal const int Lload_1 = 31;
	  internal const int Lload_2 = 32;
	  internal const int Lload_3 = 33;
	  internal const int Fload_0 = 34;
	  internal const int Fload_1 = 35;
	  internal const int Fload_2 = 36;
	  internal const int Fload_3 = 37;
	  internal const int Dload_0 = 38;
	  internal const int Dload_1 = 39;
	  internal const int Dload_2 = 40;
	  internal const int Dload_3 = 41;
	  internal const int Aload_0 = 42;
	  internal const int Aload_1 = 43;
	  internal const int Aload_2 = 44;
	  internal const int Aload_3 = 45;
	  internal const int Istore_0 = 59;
	  internal const int Istore_1 = 60;
	  internal const int Istore_2 = 61;
	  internal const int Istore_3 = 62;
	  internal const int Lstore_0 = 63;
	  internal const int Lstore_1 = 64;
	  internal const int Lstore_2 = 65;
	  internal const int Lstore_3 = 66;
	  internal const int Fstore_0 = 67;
	  internal const int Fstore_1 = 68;
	  internal const int Fstore_2 = 69;
	  internal const int Fstore_3 = 70;
	  internal const int Dstore_0 = 71;
	  internal const int Dstore_1 = 72;
	  internal const int Dstore_2 = 73;
	  internal const int Dstore_3 = 74;
	  internal const int Astore_0 = 75;
	  internal const int Astore_1 = 76;
	  internal const int Astore_2 = 77;
	  internal const int Astore_3 = 78;
	  internal const int Wide = 196;
	  internal const int Goto_W = 200;
	  internal const int Jsr_W = 201;

	  // Constants to convert between normal and wide jump instructions.

	  // The delta between the GOTO_W and JSR_W opcodes and GOTO and JUMP.
	  internal static readonly int WideJumpOpcodeDelta = Goto_W - IOpcodes.Goto;

	  // Constants to convert JVM opcodes to the equivalent ASM specific opcodes, and vice versa.

	  // The delta between the ASM_IFEQ, ..., ASM_IF_ACMPNE, ASM_GOTO and ASM_JSR opcodes
	  // and IFEQ, ..., IF_ACMPNE, GOTO and JSR.
	  internal const int Asm_Opcode_Delta = 49;

	  // The delta between the ASM_IFNULL and ASM_IFNONNULL opcodes and IFNULL and IFNONNULL.
	  internal const int Asm_Ifnull_Opcode_Delta = 20;

	  // ASM specific opcodes, used for long forward jump instructions.

	  internal const int Asm_Ifeq = IOpcodes.Ifeq + Asm_Opcode_Delta;
	  internal const int Asm_Ifne = IOpcodes.Ifne + Asm_Opcode_Delta;
	  internal const int Asm_Iflt = IOpcodes.Iflt + Asm_Opcode_Delta;
	  internal const int Asm_Ifge = IOpcodes.Ifge + Asm_Opcode_Delta;
	  internal const int Asm_Ifgt = IOpcodes.Ifgt + Asm_Opcode_Delta;
	  internal const int Asm_Ifle = IOpcodes.Ifle + Asm_Opcode_Delta;
	  internal const int Asm_If_Icmpeq = IOpcodes.If_Icmpeq + Asm_Opcode_Delta;
	  internal const int Asm_If_Icmpne = IOpcodes.If_Icmpne + Asm_Opcode_Delta;
	  internal const int Asm_If_Icmplt = IOpcodes.If_Icmplt + Asm_Opcode_Delta;
	  internal const int Asm_If_Icmpge = IOpcodes.If_Icmpge + Asm_Opcode_Delta;
	  internal const int Asm_If_Icmpgt = IOpcodes.If_Icmpgt + Asm_Opcode_Delta;
	  internal const int Asm_If_Icmple = IOpcodes.If_Icmple + Asm_Opcode_Delta;
	  internal const int Asm_If_Acmpeq = IOpcodes.If_Acmpeq + Asm_Opcode_Delta;
	  internal const int Asm_If_Acmpne = IOpcodes.If_Acmpne + Asm_Opcode_Delta;
	  internal const int Asm_Goto = IOpcodes.Goto + Asm_Opcode_Delta;
	  internal const int Asm_Jsr = IOpcodes.Jsr + Asm_Opcode_Delta;
	  internal const int Asm_Ifnull = IOpcodes.Ifnull + Asm_Ifnull_Opcode_Delta;
	  internal const int Asm_Ifnonnull = IOpcodes.Ifnonnull + Asm_Ifnull_Opcode_Delta;
	  internal const int Asm_Goto_W = 220;

	  private Constants()
	  {
	  }

	  internal static void CheckAsmExperimental(object caller)
	  {
	  }

	  internal static bool IsWhitelisted(string internalName)
	  {
		if (!internalName.StartsWith("org/objectweb/asm/", StringComparison.Ordinal))
		{
		  return false;
		}
		var member = "(Annotation|Class|Field|Method|Module|RecordComponent|Signature)";
		return internalName.Contains("Test$") || Regex.IsMatch(internalName, "org/objectweb/asm/util/Trace" + member + "Visitor(\\$.*)?") || Regex.IsMatch(internalName, "org/objectweb/asm/util/Check" + member + "Adapter(\\$.*)?");
	  }

	  internal static void CheckIsPreview(MemoryStream classInputStream)
	  {
		if (classInputStream == null)
		{
		  throw new System.InvalidOperationException("Bytecode not available, can't check class version");
		}
		int minorVersion;
		try
		{
				using (var callerClassStream = new DataInputStream(classInputStream))
				{
			  callerClassStream.ReadInt();
			  minorVersion = callerClassStream.ReadUnsignedShort();
				}
		}
		catch (IOException ioe)
		{
		  throw new System.InvalidOperationException("I/O error, can't check class version", ioe);
		}
		if (minorVersion != 0xFFFF)
		{
		  throw new System.InvalidOperationException("ASM9_EXPERIMENTAL can only be used by classes compiled with --enable-preview");
		}
	  }
	}

}