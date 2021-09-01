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
namespace ObjectWeb.Asm
{
    /// <summary>
    ///     The JVM opcodes, access flags and array type codes. This interface does not define all the JVM
    ///     opcodes because some opcodes are automatically handled. For example, the xLOAD and xSTORE opcodes
    ///     are automatically replaced by xLOAD_n and xSTORE_n opcodes when possible. The xLOAD_n and
    ///     xSTORE_n opcodes are therefore not defined in this interface. Likewise for LDC, automatically
    ///     replaced by LDC_W or LDC2_W when necessary, WIDE, GOTO_W and JSR_W.
    /// </summary>
    /// <seealso cref=
    /// <a href="https://docs.oracle.com/javase/specs/jvms/se11/html/jvms-6.html">JVMS 6</a>
    /// @author Eric Bruneton
    /// @author Eugene Kuleshov
    /// </seealso>
    // DontCheck(InterfaceIsType): can't be fixed (for backward binary compatibility).
    public interface IOpcodes
    {
        // ASM API versions.

        public const int Asm4 = (4 << 16) | (0 << 8);
        public const int Asm5 = (5 << 16) | (0 << 8);
        public const int Asm6 = (6 << 16) | (0 << 8);
        public const int Asm7 = (7 << 16) | (0 << 8);
        public const int Asm8 = (8 << 16) | (0 << 8);
        public const int Asm9 = (9 << 16) | (0 << 8);

        /// <summary>
        ///     <i>
        ///         Experimental, use at your own risk. This field will be renamed when it becomes stable, this
        ///         will break existing code using it. Only code compiled with --enable-preview can use this.
        ///     </i>
        /// </summary>
        /// @deprecated This API is experimental.
        [Obsolete("This API is experimental.")]
        public const int Asm10_Experimental = (1 << 24) | (10 << 16) | (0 << 8);

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

        public const int Source_Deprecated = 0x100;
        public const int Source_Mask = Source_Deprecated;

        // Java ClassFile versions (the minor version is stored in the 16 most significant bits, and the
        // major version in the 16 least significant bits).

        public const int V1_1 = (3 << 16) | 45;
        public const int V1_2 = (0 << 16) | 46;
        public const int V1_3 = (0 << 16) | 47;
        public const int V1_4 = (0 << 16) | 48;
        public const int V1_5 = (0 << 16) | 49;
        public const int V1_6 = (0 << 16) | 50;
        public const int V1_7 = (0 << 16) | 51;
        public const int V1_8 = (0 << 16) | 52;
        public const int V9 = (0 << 16) | 53;
        public const int V10 = (0 << 16) | 54;
        public const int V11 = (0 << 16) | 55;
        public const int V12 = (0 << 16) | 56;
        public const int V13 = (0 << 16) | 57;
        public const int V14 = (0 << 16) | 58;
        public const int V15 = (0 << 16) | 59;
        public const int V16 = (0 << 16) | 60;
        public const int V17 = (0 << 16) | 61;
        public const int V18 = (0 << 16) | 62;

        /// <summary>
        ///     Version flag indicating that the class is using 'preview' features.
        ///     <para>
        ///         {@code version & V_PREVIEW == V_PREVIEW} tests if a version is flagged with {@code
        ///         V_PREVIEW}.
        ///     </para>
        /// </summary>
        public const uint V_Preview = 0xFFFF0000;

        // Access flags values, defined in
        // - https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.1-200-E.1
        // - https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.5-200-A.1
        // - https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.6-200-A.1
        // - https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.7.25

        public const int Acc_Public = 0x0001; // class, field, method
        public const int Acc_Private = 0x0002; // class, field, method
        public const int Acc_Protected = 0x0004; // class, field, method
        public const int Acc_Static = 0x0008; // field, method
        public const int Acc_Final = 0x0010; // class, field, method, parameter
        public const int Acc_Super = 0x0020; // class
        public const int Acc_Synchronized = 0x0020; // method
        public const int Acc_Open = 0x0020; // module
        public const int Acc_Transitive = 0x0020; // module requires
        public const int Acc_Volatile = 0x0040; // field
        public const int Acc_Bridge = 0x0040; // method
        public const int Acc_Static_Phase = 0x0040; // module requires
        public const int Acc_Varargs = 0x0080; // method
        public const int Acc_Transient = 0x0080; // field
        public const int Acc_Native = 0x0100; // method
        public const int Acc_Interface = 0x0200; // class
        public const int Acc_Abstract = 0x0400; // class, method
        public const int Acc_Strict = 0x0800; // method
        public const int Acc_Synthetic = 0x1000; // class, field, method, parameter, module *
        public const int Acc_Annotation = 0x2000; // class
        public const int Acc_Enum = 0x4000; // class(?) field inner
        public const int Acc_Mandated = 0x8000; // field, method, parameter, module, module *
        public const int Acc_Module = 0x8000; // class

        // ASM specific access flags.
        // WARNING: the 16 least significant bits must NOT be used, to avoid conflicts with standard
        // access flags, and also to make sure that these flags are automatically filtered out when
        // written in class files (because access flags are stored using 16 bits only).

        public const int Acc_Record = 0x10000; // class
        public const int Acc_Deprecated = 0x20000; // class, field, method

        // Possible values for the type operand of the NEWARRAY instruction.
        // See https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-6.html#jvms-6.5.newarray.

        public const int Boolean = 4;
        public const int Char = 5;
        public const int Float = 6;
        public const int Double = 7;
        public const int Byte = 8;
        public const int Short = 9;
        public const int Int = 10;
        public const int Long = 11;

        // Possible values for the reference_kind field of CONSTANT_MethodHandle_info structures.
        // See https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.4.8.

        public const int H_Getfield = 1;
        public const int H_Getstatic = 2;
        public const int H_Putfield = 3;
        public const int H_Putstatic = 4;
        public const int H_Invokevirtual = 5;
        public const int H_Invokestatic = 6;
        public const int H_Invokespecial = 7;
        public const int H_Newinvokespecial = 8;
        public const int H_Invokeinterface = 9;

        // ASM specific stack map frame types, used in {@link ClassVisitor#visitFrame}.

        /// <summary>
        ///     An expanded frame. See <seealso cref="ClassReader.Expand_Frames" />.
        /// </summary>
        public const int F_New = -1;

        /// <summary>
        ///     A compressed frame with complete frame data.
        /// </summary>
        public const int F_Full = 0;

        /// <summary>
        ///     A compressed frame where locals are the same as the locals in the previous frame, except that
        ///     additional 1-3 locals are defined, and with an empty stack.
        /// </summary>
        public const int F_Append = 1;

        /// <summary>
        ///     A compressed frame where locals are the same as the locals in the previous frame, except that
        ///     the last 1-3 locals are absent and with an empty stack.
        /// </summary>
        public const int F_Chop = 2;

        /// <summary>
        ///     A compressed frame with exactly the same locals as the previous frame and with an empty stack.
        /// </summary>
        public const int F_Same = 3;

        /// <summary>
        ///     A compressed frame with exactly the same locals as the previous frame and with a single value
        ///     on the stack.
        /// </summary>
        public const int F_Same1 = 4;

        // The JVM opcode values (with the MethodVisitor method name used to visit them in comment, and
        // where '-' means 'same method name as on the previous line').
        // See https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-6.html.

        public const int Nop = 0; // visitInsn
        public const int Aconst_Null = 1; // -
        public const int Iconst_M1 = 2; // -
        public const int Iconst_0 = 3; // -
        public const int Iconst_1 = 4; // -
        public const int Iconst_2 = 5; // -
        public const int Iconst_3 = 6; // -
        public const int Iconst_4 = 7; // -
        public const int Iconst_5 = 8; // -
        public const int Lconst_0 = 9; // -
        public const int Lconst_1 = 10; // -
        public const int Fconst_0 = 11; // -
        public const int Fconst_1 = 12; // -
        public const int Fconst_2 = 13; // -
        public const int Dconst_0 = 14; // -
        public const int Dconst_1 = 15; // -
        public const int Bipush = 16; // visitIntInsn
        public const int Sipush = 17; // -
        public const int Ldc = 18; // visitLdcInsn
        public const int Iload = 21; // visitVarInsn
        public const int Lload = 22; // -
        public const int Fload = 23; // -
        public const int Dload = 24; // -
        public const int Aload = 25; // -
        public const int Iaload = 46; // visitInsn
        public const int Laload = 47; // -
        public const int Faload = 48; // -
        public const int Daload = 49; // -
        public const int Aaload = 50; // -
        public const int Baload = 51; // -
        public const int Caload = 52; // -
        public const int Saload = 53; // -
        public const int Istore = 54; // visitVarInsn
        public const int Lstore = 55; // -
        public const int Fstore = 56; // -
        public const int Dstore = 57; // -
        public const int Astore = 58; // -
        public const int Iastore = 79; // visitInsn
        public const int Lastore = 80; // -
        public const int Fastore = 81; // -
        public const int Dastore = 82; // -
        public const int Aastore = 83; // -
        public const int Bastore = 84; // -
        public const int Castore = 85; // -
        public const int Sastore = 86; // -
        public const int Pop = 87; // -
        public const int Pop2 = 88; // -
        public const int Dup = 89; // -
        public const int Dup_X1 = 90; // -
        public const int Dup_X2 = 91; // -
        public const int Dup2 = 92; // -
        public const int Dup2_X1 = 93; // -
        public const int Dup2_X2 = 94; // -
        public const int Swap = 95; // -
        public const int Iadd = 96; // -
        public const int Ladd = 97; // -
        public const int Fadd = 98; // -
        public const int Dadd = 99; // -
        public const int Isub = 100; // -
        public const int Lsub = 101; // -
        public const int Fsub = 102; // -
        public const int Dsub = 103; // -
        public const int Imul = 104; // -
        public const int Lmul = 105; // -
        public const int Fmul = 106; // -
        public const int Dmul = 107; // -
        public const int Idiv = 108; // -
        public const int Ldiv = 109; // -
        public const int Fdiv = 110; // -
        public const int Ddiv = 111; // -
        public const int Irem = 112; // -
        public const int Lrem = 113; // -
        public const int Frem = 114; // -
        public const int Drem = 115; // -
        public const int Ineg = 116; // -
        public const int Lneg = 117; // -
        public const int Fneg = 118; // -
        public const int Dneg = 119; // -
        public const int Ishl = 120; // -
        public const int Lshl = 121; // -
        public const int Ishr = 122; // -
        public const int Lshr = 123; // -
        public const int Iushr = 124; // -
        public const int Lushr = 125; // -
        public const int Iand = 126; // -
        public const int Land = 127; // -
        public const int Ior = 128; // -
        public const int Lor = 129; // -
        public const int Ixor = 130; // -
        public const int Lxor = 131; // -
        public const int Iinc = 132; // visitIincInsn
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
        public const int Lcmp = 148; // -
        public const int Fcmpl = 149; // -
        public const int Fcmpg = 150; // -
        public const int Dcmpl = 151; // -
        public const int Dcmpg = 152; // -
        public const int Ifeq = 153; // visitJumpInsn
        public const int Ifne = 154; // -
        public const int Iflt = 155; // -
        public const int Ifge = 156; // -
        public const int Ifgt = 157; // -
        public const int Ifle = 158; // -
        public const int If_Icmpeq = 159; // -
        public const int If_Icmpne = 160; // -
        public const int If_Icmplt = 161; // -
        public const int If_Icmpge = 162; // -
        public const int If_Icmpgt = 163; // -
        public const int If_Icmple = 164; // -
        public const int If_Acmpeq = 165; // -
        public const int If_Acmpne = 166; // -
        public const int Goto = 167; // -
        public const int Jsr = 168; // -
        public const int Ret = 169; // visitVarInsn
        public const int Tableswitch = 170; // visiTableSwitchInsn
        public const int Lookupswitch = 171; // visitLookupSwitch
        public const int Ireturn = 172; // visitInsn
        public const int Lreturn = 173; // -
        public const int Freturn = 174; // -
        public const int Dreturn = 175; // -
        public const int Areturn = 176; // -
        public const int Return = 177; // -
        public const int Getstatic = 178; // visitFieldInsn
        public const int Putstatic = 179; // -
        public const int Getfield = 180; // -
        public const int Putfield = 181; // -
        public const int Invokevirtual = 182; // visitMethodInsn
        public const int Invokespecial = 183; // -
        public const int Invokestatic = 184; // -
        public const int Invokeinterface = 185; // -
        public const int Invokedynamic = 186; // visitInvokeDynamicInsn
        public const int New = 187; // visitTypeInsn
        public const int Newarray = 188; // visitIntInsn
        public const int Anewarray = 189; // visitTypeInsn
        public const int Arraylength = 190; // visitInsn
        public const int Athrow = 191; // -
        public const int Checkcast = 192; // visitTypeInsn
        public const int Instanceof = 193; // -
        public const int Monitorenter = 194; // visitInsn
        public const int Monitorexit = 195; // -
        public const int Multianewarray = 197; // visitMultiANewArrayInsn
        public const int Ifnull = 198; // visitJumpInsn
        public const int Ifnonnull = 199; // -

        // Standard stack map frame element types, used in {@link ClassVisitor#visitFrame}.

        public const int top = Frame.Item_Top;
        public const int integer = Frame.Item_Integer;
        public const int @float = Frame.Item_Float;
        public const int @double = Frame.Item_Double;
        public const int @long = Frame.Item_Long;
        public const int @null = Frame.Item_Null;
        public const int uninitializedThis = Frame.Item_Uninitialized_This;
    }
}