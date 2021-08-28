using org.objectweb.asm;
using CursedJvmSharp.Asm.Java.IO;
using System;

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
	/// The JVM opcodes, access flags and array type codes. This interface does not define all the JVM
	/// opcodes because some opcodes are automatically handled. For example, the xLOAD and xSTORE opcodes
	/// are automatically replaced by xLOAD_n and xSTORE_n opcodes when possible. The xLOAD_n and
	/// xSTORE_n opcodes are therefore not defined in this interface. Likewise for LDC, automatically
	/// replaced by LDC_W or LDC2_W when necessary, WIDE, GOTO_W and JSR_W.
	/// </summary>
	/// <seealso cref= <a href="https://docs.oracle.com/javase/specs/jvms/se11/html/jvms-6.html">JVMS 6</a>
	/// @author Eric Bruneton
	/// @author Eugene Kuleshov </seealso>
	// DontCheck(InterfaceIsType): can't be fixed (for backward binary compatibility).
	public interface Opcodes
	{

	  // ASM API versions.

	  public const int ASM4 = 4 << 16 | 0 << 8;
	  public const int ASM5 = 5 << 16 | 0 << 8;
	  public const int ASM6 = 6 << 16 | 0 << 8;
	  public const int ASM7 = 7 << 16 | 0 << 8;
	  public const int ASM8 = 8 << 16 | 0 << 8;
	  public const int ASM9 = 9 << 16 | 0 << 8;

	  /// <summary>
	  /// <i>Experimental, use at your own risk. This field will be renamed when it becomes stable, this
	  /// will break existing code using it. Only code compiled with --enable-preview can use this.</i>
	  /// </summary>
	  /// @deprecated This API is experimental. 
	  [Obsolete("This API is experimental.")]
	  public const int ASM10_EXPERIMENTAL = 1 << 24 | 10 << 16 | 0 << 8;

	  /*
	   * Internal flags used to redirect calls to deprecated methods. For instance, if a visitOldStuff
	   * method in API_OLD is deprecated and replaced with visitNewStuff in API_NEW, then the
	   * redirection should be done as follows:
	   *
	   * <pre>
	   * public class StuffVisitor {
	   *   ...
	   *
	   *   &#64;Deprecated public void visitOldStuff(int arg, ...) {
	   *     // SOURCE_DEPRECATED means "a call from a deprecated method using the old 'api' value".
	   *     visitNewStuf(arg | (api &#60; API_NEW ? SOURCE_DEPRECATED : 0), ...);
	   *   }
	   *
	   *   public void visitNewStuff(int argAndSource, ...) {
	   *     if (api &#60; API_NEW &#38;&#38; (argAndSource &#38; SOURCE_DEPRECATED) == 0) {
	   *       visitOldStuff(argAndSource, ...);
	   *     } else {
	   *       int arg = argAndSource &#38; ~SOURCE_MASK;
	   *       [ do stuff ]
	   *     }
	   *   }
	   * }
	   * </pre>
	   *
	   * <p>If 'api' is equal to API_NEW, there are two cases:
	   *
	   * <ul>
	   *   <li>call visitNewStuff: the redirection test is skipped and 'do stuff' is executed directly.
	   *   <li>call visitOldSuff: the source is not set to SOURCE_DEPRECATED before calling
	   *       visitNewStuff, but the redirection test is skipped anyway in visitNewStuff, which
	   *       directly executes 'do stuff'.
	   * </ul>
	   *
	   * <p>If 'api' is equal to API_OLD, there are two cases:
	   *
	   * <ul>
	   *   <li>call visitOldSuff: the source is set to SOURCE_DEPRECATED before calling visitNewStuff.
	   *       Because of this visitNewStuff does not redirect back to visitOldStuff, and instead
	   *       executes 'do stuff'.
	   *   <li>call visitNewStuff: the call is redirected to visitOldStuff because the source is 0.
	   *       visitOldStuff now sets the source to SOURCE_DEPRECATED and calls visitNewStuff back. This
	   *       time visitNewStuff does not redirect the call, and instead executes 'do stuff'.
	   * </ul>
	   *
	   * <h1>User subclasses</h1>
	   *
	   * <p>If a user subclass overrides one of these methods, there are only two cases: either 'api' is
	   * API_OLD and visitOldStuff is overridden (and visitNewStuff is not), or 'api' is API_NEW or
	   * more, and visitNewStuff is overridden (and visitOldStuff is not). Any other case is a user
	   * programming error.
	   *
	   * <p>If 'api' is equal to API_NEW, the class hierarchy is equivalent to
	   *
	   * <pre>
	   * public class StuffVisitor {
	   *   &#64;Deprecated public void visitOldStuff(int arg, ...) { visitNewStuf(arg, ...); }
	   *   public void visitNewStuff(int arg, ...) { [ do stuff ] }
	   * }
	   * class UserStuffVisitor extends StuffVisitor {
	   *   &#64;Override public void visitNewStuff(int arg, ...) {
	   *     super.visitNewStuff(int arg, ...); // optional
	   *     [ do user stuff ]
	   *   }
	   * }
	   * </pre>
	   *
	   * <p>It is then obvious that whether visitNewStuff or visitOldStuff is called, 'do stuff' and 'do
	   * user stuff' will be executed, in this order.
	   *
	   * <p>If 'api' is equal to API_OLD, the class hierarchy is equivalent to
	   *
	   * <pre>
	   * public class StuffVisitor {
	   *   &#64;Deprecated public void visitOldStuff(int arg, ...) {
	   *     visitNewStuff(arg | SOURCE_DEPRECATED, ...);
	   *   }
	   *   public void visitNewStuff(int argAndSource...) {
	   *     if ((argAndSource & SOURCE_DEPRECATED) == 0) {
	   *       visitOldStuff(argAndSource, ...);
	   *     } else {
	   *       int arg = argAndSource &#38; ~SOURCE_MASK;
	   *       [ do stuff ]
	   *     }
	   *   }
	   * }
	   * class UserStuffVisitor extends StuffVisitor {
	   *   &#64;Override public void visitOldStuff(int arg, ...) {
	   *     super.visitOldStuff(int arg, ...); // optional
	   *     [ do user stuff ]
	   *   }
	   * }
	   * </pre>
	   *
	   * <p>and there are two cases:
	   *
	   * <ul>
	   *   <li>call visitOldStuff: in the call to super.visitOldStuff, the source is set to
	   *       SOURCE_DEPRECATED and visitNewStuff is called. Here 'do stuff' is run because the source
	   *       was previously set to SOURCE_DEPRECATED, and execution eventually returns to
	   *       UserStuffVisitor.visitOldStuff, where 'do user stuff' is run.
	   *   <li>call visitNewStuff: the call is redirected to UserStuffVisitor.visitOldStuff because the
	   *       source is 0. Execution continues as in the previous case, resulting in 'do stuff' and 'do
	   *       user stuff' being executed, in this order.
	   * </ul>
	   *
	   * <h1>ASM subclasses</h1>
	   *
	   * <p>In ASM packages, subclasses of StuffVisitor can typically be sub classed again by the user,
	   * and can be used with API_OLD or API_NEW. Because of this, if such a subclass must override
	   * visitNewStuff, it must do so in the following way (and must not override visitOldStuff):
	   *
	   * <pre>
	   * public class AsmStuffVisitor extends StuffVisitor {
	   *   &#64;Override public void visitNewStuff(int argAndSource, ...) {
	   *     if (api &#60; API_NEW &#38;&#38; (argAndSource &#38; SOURCE_DEPRECATED) == 0) {
	   *       super.visitNewStuff(argAndSource, ...);
	   *       return;
	   *     }
	   *     super.visitNewStuff(argAndSource, ...); // optional
	   *     int arg = argAndSource &#38; ~SOURCE_MASK;
	   *     [ do other stuff ]
	   *   }
	   * }
	   * </pre>
	   *
	   * <p>If a user class extends this with 'api' equal to API_NEW, the class hierarchy is equivalent
	   * to
	   *
	   * <pre>
	   * public class StuffVisitor {
	   *   &#64;Deprecated public void visitOldStuff(int arg, ...) { visitNewStuf(arg, ...); }
	   *   public void visitNewStuff(int arg, ...) { [ do stuff ] }
	   * }
	   * public class AsmStuffVisitor extends StuffVisitor {
	   *   &#64;Override public void visitNewStuff(int arg, ...) {
	   *     super.visitNewStuff(arg, ...);
	   *     [ do other stuff ]
	   *   }
	   * }
	   * class UserStuffVisitor extends StuffVisitor {
	   *   &#64;Override public void visitNewStuff(int arg, ...) {
	   *     super.visitNewStuff(int arg, ...);
	   *     [ do user stuff ]
	   *   }
	   * }
	   * </pre>
	   *
	   * <p>It is then obvious that whether visitNewStuff or visitOldStuff is called, 'do stuff', 'do
	   * other stuff' and 'do user stuff' will be executed, in this order. If, on the other hand, a user
	   * class extends AsmStuffVisitor with 'api' equal to API_OLD, the class hierarchy is equivalent to
	   *
	   * <pre>
	   * public class StuffVisitor {
	   *   &#64;Deprecated public void visitOldStuff(int arg, ...) {
	   *     visitNewStuf(arg | SOURCE_DEPRECATED, ...);
	   *   }
	   *   public void visitNewStuff(int argAndSource, ...) {
	   *     if ((argAndSource & SOURCE_DEPRECATED) == 0) {
	   *       visitOldStuff(argAndSource, ...);
	   *     } else {
	   *       int arg = argAndSource &#38; ~SOURCE_MASK;
	   *       [ do stuff ]
	   *     }
	   *   }
	   * }
	   * public class AsmStuffVisitor extends StuffVisitor {
	   *   &#64;Override public void visitNewStuff(int argAndSource, ...) {
	   *     if ((argAndSource &#38; SOURCE_DEPRECATED) == 0) {
	   *       super.visitNewStuff(argAndSource, ...);
	   *       return;
	   *     }
	   *     super.visitNewStuff(argAndSource, ...); // optional
	   *     int arg = argAndSource &#38; ~SOURCE_MASK;
	   *     [ do other stuff ]
	   *   }
	   * }
	   * class UserStuffVisitor extends StuffVisitor {
	   *   &#64;Override public void visitOldStuff(int arg, ...) {
	   *     super.visitOldStuff(arg, ...);
	   *     [ do user stuff ]
	   *   }
	   * }
	   * </pre>
	   *
	   * <p>and, here again, whether visitNewStuff or visitOldStuff is called, 'do stuff', 'do other
	   * stuff' and 'do user stuff' will be executed, in this order (exercise left to the reader).
	   *
	   * <h1>Notes</h1>
	   *
	   * <ul>
	   *   <li>the SOURCE_DEPRECATED flag is set only if 'api' is API_OLD, just before calling
	   *       visitNewStuff. By hypothesis, this method is not overridden by the user. Therefore, user
	   *       classes can never see this flag. Only ASM subclasses must take care of extracting the
	   *       actual argument value by clearing the source flags.
	   *   <li>because the SOURCE_DEPRECATED flag is immediately cleared in the caller, the caller can
	   *       call visitOldStuff or visitNewStuff (in 'do stuff' and 'do user stuff') on a delegate
	   *       visitor without any risks (breaking the redirection logic, "leaking" the flag, etc).
	   *   <li>all the scenarios discussed above are unit tested in MethodVisitorTest.
	   * </ul>
	   */

	  public const int SOURCE_DEPRECATED = 0x100;
	  public const int SOURCE_MASK = SOURCE_DEPRECATED;

	  // Java ClassFile versions (the minor version is stored in the 16 most significant bits, and the
	  // major version in the 16 least significant bits).

	  public const int V1_1 = 3 << 16 | 45;
	  public const int V1_2 = 0 << 16 | 46;
	  public const int V1_3 = 0 << 16 | 47;
	  public const int V1_4 = 0 << 16 | 48;
	  public const int V1_5 = 0 << 16 | 49;
	  public const int V1_6 = 0 << 16 | 50;
	  public const int V1_7 = 0 << 16 | 51;
	  public const int V1_8 = 0 << 16 | 52;
	  public const int V9 = 0 << 16 | 53;
	  public const int V10 = 0 << 16 | 54;
	  public const int V11 = 0 << 16 | 55;
	  public const int V12 = 0 << 16 | 56;
	  public const int V13 = 0 << 16 | 57;
	  public const int V14 = 0 << 16 | 58;
	  public const int V15 = 0 << 16 | 59;
	  public const int V16 = 0 << 16 | 60;
	  public const int V17 = 0 << 16 | 61;
	  public const int V18 = 0 << 16 | 62;

	  /// <summary>
	  /// Version flag indicating that the class is using 'preview' features.
	  /// 
	  /// <para>{@code version & V_PREVIEW == V_PREVIEW} tests if a version is flagged with {@code
	  /// V_PREVIEW}.
	  /// </para>
	  /// </summary>
	  public const int V_PREVIEW = 0xFFFF0000;

	  // Access flags values, defined in
	  // - https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.1-200-E.1
	  // - https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.5-200-A.1
	  // - https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.6-200-A.1
	  // - https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.7.25

	  public const int ACC_PUBLIC = 0x0001; // class, field, method
	  public const int ACC_PRIVATE = 0x0002; // class, field, method
	  public const int ACC_PROTECTED = 0x0004; // class, field, method
	  public const int ACC_STATIC = 0x0008; // field, method
	  public const int ACC_FINAL = 0x0010; // class, field, method, parameter
	  public const int ACC_SUPER = 0x0020; // class
	  public const int ACC_SYNCHRONIZED = 0x0020; // method
	  public const int ACC_OPEN = 0x0020; // module
	  public const int ACC_TRANSITIVE = 0x0020; // module requires
	  public const int ACC_VOLATILE = 0x0040; // field
	  public const int ACC_BRIDGE = 0x0040; // method
	  public const int ACC_STATIC_PHASE = 0x0040; // module requires
	  public const int ACC_VARARGS = 0x0080; // method
	  public const int ACC_TRANSIENT = 0x0080; // field
	  public const int ACC_NATIVE = 0x0100; // method
	  public const int ACC_INTERFACE = 0x0200; // class
	  public const int ACC_ABSTRACT = 0x0400; // class, method
	  public const int ACC_STRICT = 0x0800; // method
	  public const int ACC_SYNTHETIC = 0x1000; // class, field, method, parameter, module *
	  public const int ACC_ANNOTATION = 0x2000; // class
	  public const int ACC_ENUM = 0x4000; // class(?) field inner
	  public const int ACC_MANDATED = 0x8000; // field, method, parameter, module, module *
	  public const int ACC_MODULE = 0x8000; // class

	  // ASM specific access flags.
	  // WARNING: the 16 least significant bits must NOT be used, to avoid conflicts with standard
	  // access flags, and also to make sure that these flags are automatically filtered out when
	  // written in class files (because access flags are stored using 16 bits only).

	  public const int ACC_RECORD = 0x10000; // class
	  public const int ACC_DEPRECATED = 0x20000; // class, field, method

	  // Possible values for the type operand of the NEWARRAY instruction.
	  // See https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-6.html#jvms-6.5.newarray.

	  public const int T_BOOLEAN = 4;
	  public const int T_CHAR = 5;
	  public const int T_FLOAT = 6;
	  public const int T_DOUBLE = 7;
	  public const int T_BYTE = 8;
	  public const int T_SHORT = 9;
	  public const int T_INT = 10;
	  public const int T_LONG = 11;

	  // Possible values for the reference_kind field of CONSTANT_MethodHandle_info structures.
	  // See https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.4.8.

	  public const int H_GETFIELD = 1;
	  public const int H_GETSTATIC = 2;
	  public const int H_PUTFIELD = 3;
	  public const int H_PUTSTATIC = 4;
	  public const int H_INVOKEVIRTUAL = 5;
	  public const int H_INVOKESTATIC = 6;
	  public const int H_INVOKESPECIAL = 7;
	  public const int H_NEWINVOKESPECIAL = 8;
	  public const int H_INVOKEINTERFACE = 9;

	  // ASM specific stack map frame types, used in {@link ClassVisitor#visitFrame}.

	  /// <summary>
	  /// An expanded frame. See <seealso cref="ClassReader.EXPAND_FRAMES"/>. </summary>
	  public const int F_NEW = -1;

	  /// <summary>
	  /// A compressed frame with complete frame data. </summary>
	  public const int F_FULL = 0;

	  /// <summary>
	  /// A compressed frame where locals are the same as the locals in the previous frame, except that
	  /// additional 1-3 locals are defined, and with an empty stack.
	  /// </summary>
	  public const int F_APPEND = 1;

	  /// <summary>
	  /// A compressed frame where locals are the same as the locals in the previous frame, except that
	  /// the last 1-3 locals are absent and with an empty stack.
	  /// </summary>
	  public const int F_CHOP = 2;

	  /// <summary>
	  /// A compressed frame with exactly the same locals as the previous frame and with an empty stack.
	  /// </summary>
	  public const int F_SAME = 3;

	  /// <summary>
	  /// A compressed frame with exactly the same locals as the previous frame and with a single value
	  /// on the stack.
	  /// </summary>
	  public const int F_SAME1 = 4;

	  // Standard stack map frame element types, used in {@link ClassVisitor#visitFrame}.

	  public static int? TOP = Frame.ITEM_TOP;
	  public static int? INTEGER = Frame.ITEM_INTEGER;
	  public static int? FLOAT = Frame.ITEM_FLOAT;
	  public static int? DOUBLE = Frame.ITEM_DOUBLE;
	  public static int? LONG = Frame.ITEM_LONG;
	  public static int? NULL = Frame.ITEM_NULL;
	  public static int? UNINITIALIZED_THIS = Frame.ITEM_UNINITIALIZED_THIS;

	  // The JVM opcode values (with the MethodVisitor method name used to visit them in comment, and
	  // where '-' means 'same method name as on the previous line').
	  // See https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-6.html.

	  public const int NOP = 0; // visitInsn
	  public const int ACONST_NULL = 1; // -
	  public const int ICONST_M1 = 2; // -
	  public const int ICONST_0 = 3; // -
	  public const int ICONST_1 = 4; // -
	  public const int ICONST_2 = 5; // -
	  public const int ICONST_3 = 6; // -
	  public const int ICONST_4 = 7; // -
	  public const int ICONST_5 = 8; // -
	  public const int LCONST_0 = 9; // -
	  public const int LCONST_1 = 10; // -
	  public const int FCONST_0 = 11; // -
	  public const int FCONST_1 = 12; // -
	  public const int FCONST_2 = 13; // -
	  public const int DCONST_0 = 14; // -
	  public const int DCONST_1 = 15; // -
	  public const int BIPUSH = 16; // visitIntInsn
	  public const int SIPUSH = 17; // -
	  public const int LDC = 18; // visitLdcInsn
	  public const int ILOAD = 21; // visitVarInsn
	  public const int LLOAD = 22; // -
	  public const int FLOAD = 23; // -
	  public const int DLOAD = 24; // -
	  public const int ALOAD = 25; // -
	  public const int IALOAD = 46; // visitInsn
	  public const int LALOAD = 47; // -
	  public const int FALOAD = 48; // -
	  public const int DALOAD = 49; // -
	  public const int AALOAD = 50; // -
	  public const int BALOAD = 51; // -
	  public const int CALOAD = 52; // -
	  public const int SALOAD = 53; // -
	  public const int ISTORE = 54; // visitVarInsn
	  public const int LSTORE = 55; // -
	  public const int FSTORE = 56; // -
	  public const int DSTORE = 57; // -
	  public const int ASTORE = 58; // -
	  public const int IASTORE = 79; // visitInsn
	  public const int LASTORE = 80; // -
	  public const int FASTORE = 81; // -
	  public const int DASTORE = 82; // -
	  public const int AASTORE = 83; // -
	  public const int BASTORE = 84; // -
	  public const int CASTORE = 85; // -
	  public const int SASTORE = 86; // -
	  public const int POP = 87; // -
	  public const int POP2 = 88; // -
	  public const int DUP = 89; // -
	  public const int DUP_X1 = 90; // -
	  public const int DUP_X2 = 91; // -
	  public const int DUP2 = 92; // -
	  public const int DUP2_X1 = 93; // -
	  public const int DUP2_X2 = 94; // -
	  public const int SWAP = 95; // -
	  public const int IADD = 96; // -
	  public const int LADD = 97; // -
	  public const int FADD = 98; // -
	  public const int DADD = 99; // -
	  public const int ISUB = 100; // -
	  public const int LSUB = 101; // -
	  public const int FSUB = 102; // -
	  public const int DSUB = 103; // -
	  public const int IMUL = 104; // -
	  public const int LMUL = 105; // -
	  public const int FMUL = 106; // -
	  public const int DMUL = 107; // -
	  public const int IDIV = 108; // -
	  public const int LDIV = 109; // -
	  public const int FDIV = 110; // -
	  public const int DDIV = 111; // -
	  public const int IREM = 112; // -
	  public const int LREM = 113; // -
	  public const int FREM = 114; // -
	  public const int DREM = 115; // -
	  public const int INEG = 116; // -
	  public const int LNEG = 117; // -
	  public const int FNEG = 118; // -
	  public const int DNEG = 119; // -
	  public const int ISHL = 120; // -
	  public const int LSHL = 121; // -
	  public const int ISHR = 122; // -
	  public const int LSHR = 123; // -
	  public const int IUSHR = 124; // -
	  public const int LUSHR = 125; // -
	  public const int IAND = 126; // -
	  public const int LAND = 127; // -
	  public const int IOR = 128; // -
	  public const int LOR = 129; // -
	  public const int IXOR = 130; // -
	  public const int LXOR = 131; // -
	  public const int IINC = 132; // visitIincInsn
	  public const int I2L = 133; // visitInsn
	  public const int I2F = 134; // -
	  public const int I2D = 135; // -
	  public const int L2I = 136; // -
	  public const int L2F = 137; // -
	  public const int L2D = 138; // -
	  public const int F2I = 139; // -
	  public const int F2L = 140; // -
	  public const int F2D = 141; // -
	  public const int D2I = 142; // -
	  public const int D2L = 143; // -
	  public const int D2F = 144; // -
	  public const int I2B = 145; // -
	  public const int I2C = 146; // -
	  public const int I2S = 147; // -
	  public const int LCMP = 148; // -
	  public const int FCMPL = 149; // -
	  public const int FCMPG = 150; // -
	  public const int DCMPL = 151; // -
	  public const int DCMPG = 152; // -
	  public const int IFEQ = 153; // visitJumpInsn
	  public const int IFNE = 154; // -
	  public const int IFLT = 155; // -
	  public const int IFGE = 156; // -
	  public const int IFGT = 157; // -
	  public const int IFLE = 158; // -
	  public const int IF_ICMPEQ = 159; // -
	  public const int IF_ICMPNE = 160; // -
	  public const int IF_ICMPLT = 161; // -
	  public const int IF_ICMPGE = 162; // -
	  public const int IF_ICMPGT = 163; // -
	  public const int IF_ICMPLE = 164; // -
	  public const int IF_ACMPEQ = 165; // -
	  public const int IF_ACMPNE = 166; // -
	  public const int GOTO = 167; // -
	  public const int JSR = 168; // -
	  public const int RET = 169; // visitVarInsn
	  public const int TABLESWITCH = 170; // visiTableSwitchInsn
	  public const int LOOKUPSWITCH = 171; // visitLookupSwitch
	  public const int IRETURN = 172; // visitInsn
	  public const int LRETURN = 173; // -
	  public const int FRETURN = 174; // -
	  public const int DRETURN = 175; // -
	  public const int ARETURN = 176; // -
	  public const int RETURN = 177; // -
	  public const int GETSTATIC = 178; // visitFieldInsn
	  public const int PUTSTATIC = 179; // -
	  public const int GETFIELD = 180; // -
	  public const int PUTFIELD = 181; // -
	  public const int INVOKEVIRTUAL = 182; // visitMethodInsn
	  public const int INVOKESPECIAL = 183; // -
	  public const int INVOKESTATIC = 184; // -
	  public const int INVOKEINTERFACE = 185; // -
	  public const int INVOKEDYNAMIC = 186; // visitInvokeDynamicInsn
	  public const int NEW = 187; // visitTypeInsn
	  public const int NEWARRAY = 188; // visitIntInsn
	  public const int ANEWARRAY = 189; // visitTypeInsn
	  public const int ARRAYLENGTH = 190; // visitInsn
	  public const int ATHROW = 191; // -
	  public const int CHECKCAST = 192; // visitTypeInsn
	  public const int INSTANCEOF = 193; // -
	  public const int MONITORENTER = 194; // visitInsn
	  public const int MONITOREXIT = 195; // -
	  public const int MULTIANEWARRAY = 197; // visitMultiANewArrayInsn
	  public const int IFNULL = 198; // visitJumpInsn
	  public const int IFNONNULL = 199; // -
	}

}