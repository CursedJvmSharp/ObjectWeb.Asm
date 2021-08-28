using System;
using System.Collections.Generic;
using System.Linq;

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
    /// A node that represents a method.
    /// 
    /// @author Eric Bruneton
    /// </summary>
    public class MethodNode : MethodVisitor
    {
        /// <summary>
        /// The method's access flags (see <seealso cref = "IOpcodes"/>). This field also indicates if the method is
        /// synthetic and/or deprecated.
        /// </summary>
        public int Access { get; set; }

        /// <summary>
        /// The method's name. </summary>
        public string Name { get; set; }

        /// <summary>
        /// The method's descriptor (see <seealso cref = "Type"/>). </summary>
        public string Desc { get; set; }

        /// <summary>
        /// The method's signature. May be {@literal null}. </summary>
        public string Signature { get; set; }

        /// <summary>
        /// The internal names of the method's exception classes (see <seealso cref = "Type.InternalName"/>). </summary>
        public List<string> Exceptions { get; set; }

        /// <summary>
        /// The method parameter info (access flags and name). </summary>
        public List<ParameterNode> Parameters { get; set; }

        /// <summary>
        /// The runtime visible annotations of this method. May be {@literal null}. </summary>
        public List<AnnotationNode> VisibleAnnotations { get; set; }

        /// <summary>
        /// The runtime invisible annotations of this method. May be {@literal null}. </summary>
        public List<AnnotationNode> InvisibleAnnotations { get; set; }

        /// <summary>
        /// The runtime visible type annotations of this method. May be {@literal null}. </summary>
        public List<TypeAnnotationNode> VisibleTypeAnnotations { get; set; }

        /// <summary>
        /// The runtime invisible type annotations of this method. May be {@literal null}. </summary>
        public List<TypeAnnotationNode> InVisibleTypeAnnotations { get; set; }

        /// <summary>
        /// The non standard attributes of this method. May be {@literal null}. </summary>
        public List<Attribute> Attrs { get; set; }

        /// <summary>
        /// The default value of this annotation interface method. This field must be a <seealso cref = "Byte"/>,
        /// <seealso cref = "Boolean"/>, <seealso cref = "Character"/>, <seealso cref = "Short"/>, <seealso cref = "Integer"/>, <seealso cref = "Long"/>, {@link
        /// Float}, <seealso cref = "Double"/>, <seealso cref = "string "/> or <seealso cref = "Type"/>, or an two elements String array (for
        /// enumeration values), a <seealso cref = "AnnotationNode"/>, or a <seealso cref = "System.Collections.IList"/> of values of one of the
        /// preceding types. May be {@literal null}.
        /// </summary>
        public object AnnotationDefault { get; set; }

        /// <summary>
        /// The number of method parameters than can have runtime visible annotations. This number must be
        /// less or equal than the number of parameter types in the method descriptor (the default value 0
        /// indicates that all the parameters described in the method descriptor can have annotations). It
        /// can be strictly less when a method has synthetic parameters and when these parameters are
        /// ignored when computing parameter indices for the purpose of parameter annotations (see
        /// https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.7.18).
        /// </summary>
        public int VisibleAnnotableParameterCount { get; set; }

        /// <summary>
        /// The runtime visible parameter annotations of this method. These lists are lists of {@link
        /// AnnotationNode} objects. May be {@literal null}.
        /// </summary>
        public List<AnnotationNode>[] VisibleParameterAnnotations { get; set; }

        /// <summary>
        /// The number of method parameters than can have runtime invisible annotations. This number must
        /// be less or equal than the number of parameter types in the method descriptor (the default value
        /// 0 indicates that all the parameters described in the method descriptor can have annotations).
        /// It can be strictly less when a method has synthetic parameters and when these parameters are
        /// ignored when computing parameter indices for the purpose of parameter annotations (see
        /// https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.7.18).
        /// </summary>
        public int InvisibleAnnotableParameterCount { get; set; }

        /// <summary>
        /// The runtime invisible parameter annotations of this method. These lists are lists of {@link
        /// AnnotationNode} objects. May be {@literal null}.
        /// </summary>
        public List<AnnotationNode>[] InvisibleParameterAnnotations { get; set; }

        /// <summary>
        /// The instructions of this method. </summary>
        public InsnList Instructions { get; set; }

        /// <summary>
        /// The try catch blocks of this method. </summary>
        public List<TryCatchBlockNode> TryCatchBlocks { get; set; }

        /// <summary>
        /// The maximum stack size of this method. </summary>
        public int MaxStack { get; set; }

        /// <summary>
        /// The maximum number of local variables of this method. </summary>
        public int MaxLocals { get; set; }

        /// <summary>
        /// The local variables of this method. May be {@literal null} </summary>
        public List<LocalVariableNode> LocalVariables { get; set; }

        /// <summary>
        /// The visible local variable annotations of this method. May be {@literal null} </summary>
        public List<LocalVariableAnnotationNode> VisibleLocalVariableAnnotations { get; set; }

        /// <summary>
        /// The invisible local variable annotations of this method. May be {@literal null} </summary>
        public List<LocalVariableAnnotationNode> InvisibleLocalVariableAnnotations { get; set; }

        /// <summary>
        /// Whether the accept method has been called on this object. </summary>
        private bool _visited;
        /// <summary>
        /// Constructs an uninitialized <seealso cref = "MethodNode"/>. <i>Subclasses must not use this
        /// constructor</i>. Instead, they must use the <seealso cref = "MethodNode(int)"/> version.
        /// </summary>
        /// <exception cref = "IllegalStateException"> If a subclass calls this constructor. </exception>
        public MethodNode(): this(IOpcodes.Asm9)
        {
            if (this.GetType() != typeof(MethodNode))
            {
                throw new System.InvalidOperationException();
            }
        }

        /// <summary>
        /// Constructs an uninitialized <seealso cref = "MethodNode"/>.
        /// </summary>
        /// <param name = "api"> the ASM API version implemented by this visitor. Must be one of the {@code
        ///     ASM}<i>x</i> values in <seealso cref = "IOpcodes"/>. </param>
        public MethodNode(int api): base(api)
        {
            this.Instructions = new InsnList();
        }

        /// <summary>
        /// Constructs a new <seealso cref = "MethodNode"/>. <i>Subclasses must not use this constructor</i>. Instead,
        /// they must use the <seealso cref = "MethodNode(int, int, String, String, String, string[])"/> version.
        /// </summary>
        /// <param name = "access"> the method's access flags (see <seealso cref = "IOpcodes"/>). This parameter also indicates if
        ///     the method is synthetic and/or deprecated. </param>
        /// <param name = "name"> the method's name. </param>
        /// <param name = "descriptor"> the method's descriptor (see <seealso cref = "Type"/>). </param>
        /// <param name = "signature"> the method's signature. May be {@literal null}. </param>
        /// <param name = "exceptions"> the internal names of the method's exception classes (see {@link
        ///     Type#getInternalName()}). May be {@literal null}. </param>
        /// <exception cref = "IllegalStateException"> If a subclass calls this constructor. </exception>
        public MethodNode(int access, string name, string descriptor, string signature, string[] exceptions): this(IOpcodes.Asm9, access, name, descriptor, signature, exceptions)
        {
            if (this.GetType() != typeof(MethodNode))
            {
                throw new System.InvalidOperationException();
            }
        }

        /// <summary>
        /// Constructs a new <seealso cref = "MethodNode"/>.
        /// </summary>
        /// <param name = "api"> the ASM API version implemented by this visitor. Must be one of the {@code
        ///     ASM}<i>x</i> values in <seealso cref = "IOpcodes"/>. </param>
        /// <param name = "access"> the method's access flags (see <seealso cref = "IOpcodes"/>). This parameter also indicates if
        ///     the method is synthetic and/or deprecated. </param>
        /// <param name = "name"> the method's name. </param>
        /// <param name = "descriptor"> the method's descriptor (see <seealso cref = "Type"/>). </param>
        /// <param name = "signature"> the method's signature. May be {@literal null}. </param>
        /// <param name = "exceptions"> the internal names of the method's exception classes (see {@link
        ///     Type#getInternalName()}). May be {@literal null}. </param>
        public MethodNode(int api, int access, string name, string descriptor, string signature, string[] exceptions): base(api)
        {
            this.Access = access;
            this.Name = name;
            this.Desc = descriptor;
            this.Signature = signature;
            this.Exceptions = Util.AsArrayList(exceptions);
            if ((access & IOpcodes.Acc_Abstract) == 0)
            {
                this.LocalVariables = new List<LocalVariableNode>(5);
            }

            this.TryCatchBlocks = new List<TryCatchBlockNode>();
            this.Instructions = new InsnList();
        }

        // -----------------------------------------------------------------------------------------------
        // Implementation of the MethodVisitor abstract class
        // -----------------------------------------------------------------------------------------------
        public override void VisitParameter(string name, int access)
        {
            if (Parameters == null)
            {
                Parameters = new List<ParameterNode>(5);
            }

            Parameters.Add(new ParameterNode(name, access));
        }

        public override AnnotationVisitor VisitAnnotationDefault()
        {
            return new AnnotationNode(new ArrayListAnonymousInnerClass(this));
        }

        private class ArrayListAnonymousInnerClass : List<object>
        {
            private readonly MethodNode _outerInstance;
            public ArrayListAnonymousInnerClass(MethodNode outerInstance): base(0)
            {
                this._outerInstance = outerInstance;
            }

            public new bool Add(object o)
            {
                _outerInstance.AnnotationDefault = o;
                base.Add(o);
                return true;
            }
        }

        public override AnnotationVisitor VisitAnnotation(string descriptor, bool visible)
        {
            var annotation = new AnnotationNode(descriptor);
            if (visible)
            {
                VisibleAnnotations = Util.Add(VisibleAnnotations, annotation);
            }
            else
            {
                InvisibleAnnotations = Util.Add(InvisibleAnnotations, annotation);
            }

            return annotation;
        }

        public override AnnotationVisitor VisitTypeAnnotation(int typeRef, TypePath typePath, string descriptor, bool visible)
        {
            var typeAnnotation = new TypeAnnotationNode(typeRef, typePath, descriptor);
            if (visible)
            {
                VisibleTypeAnnotations = Util.Add(VisibleTypeAnnotations, typeAnnotation);
            }
            else
            {
                InVisibleTypeAnnotations = Util.Add(InVisibleTypeAnnotations, typeAnnotation);
            }

            return typeAnnotation;
        }

        public override void VisitAnnotableParameterCount(int parameterCount, bool visible)
        {
            if (visible)
            {
                VisibleAnnotableParameterCount = parameterCount;
            }
            else
            {
                InvisibleAnnotableParameterCount = parameterCount;
            }
        }

        public override AnnotationVisitor VisitParameterAnnotation(int parameter, string descriptor, bool visible)
        {
            var annotation = new AnnotationNode(descriptor);
            if (visible)
            {
                if (VisibleParameterAnnotations == null)
                {
                    var @params = JType.GetArgumentTypes(Desc).Length;
                    VisibleParameterAnnotations = new List<AnnotationNode>[@params];
                }

                VisibleParameterAnnotations[parameter] = Util.Add(VisibleParameterAnnotations[parameter], annotation);
            }
            else
            {
                if (InvisibleParameterAnnotations == null)
                {
                    var @params = JType.GetArgumentTypes(Desc).Length;
                    InvisibleParameterAnnotations = new List<AnnotationNode>[@params];
                }

                InvisibleParameterAnnotations[parameter] = Util.Add(InvisibleParameterAnnotations[parameter], annotation);
            }

            return annotation;
        }

        public override void VisitAttribute(Attribute attribute)
        {
            Attrs = Util.Add(Attrs, attribute);
        }

        public override void VisitCode()
        {
        // Nothing to do.
        }

        public override void VisitFrame(int type, int numLocal, object[] local, int numStack, object[] stack)
        {
            Instructions.Add(new FrameNode(type, numLocal, local == null ? null : GetLabelNodes(local), numStack, stack == null ? null : GetLabelNodes(stack)));
        }

        public override void VisitInsn(int opcode)
        {
            Instructions.Add(new InsnNode(opcode));
        }

        public override void VisitIntInsn(int opcode, int operand)
        {
            Instructions.Add(new IntInsnNode(opcode, operand));
        }

        public override void VisitVarInsn(int opcode, int var)
        {
            Instructions.Add(new VarInsnNode(opcode, var));
        }

        public override void VisitTypeInsn(int opcode, string type)
        {
            Instructions.Add(new TypeInsnNode(opcode, type));
        }

        public override void VisitFieldInsn(int opcode, string owner, string name, string descriptor)
        {
            Instructions.Add(new FieldInsnNode(opcode, owner, name, descriptor));
        }

        public override void VisitMethodInsn(int opcodeAndSource, string owner, string name, string descriptor, bool isInterface)
        {
            if (api < IOpcodes.Asm5 && (opcodeAndSource & IOpcodes.Source_Deprecated) == 0)
            {
                // Redirect the call to the deprecated version of this method.
                base.VisitMethodInsn(opcodeAndSource, owner, name, descriptor, isInterface);
                return;
            }

            var opcode = opcodeAndSource & ~IOpcodes.Source_Mask;
            Instructions.Add(new MethodInsnNode(opcode, owner, name, descriptor, isInterface));
        }

        public override void VisitInvokeDynamicInsn(string name, string descriptor, Handle bootstrapMethodHandle, params object[] bootstrapMethodArguments)
        {
            Instructions.Add(new InvokeDynamicInsnNode(name, descriptor, bootstrapMethodHandle, bootstrapMethodArguments));
        }

        public override void VisitJumpInsn(int opcode, Label label)
        {
            Instructions.Add(new JumpInsnNode(opcode, GetLabelNode(label)));
        }

        public override void VisitLabel(Label label)
        {
            Instructions.Add(GetLabelNode(label));
        }

        public override void VisitLdcInsn(object value)
        {
            Instructions.Add(new LdcInsnNode(value));
        }

        public override void VisitIincInsn(int var, int increment)
        {
            Instructions.Add(new IincInsnNode(var, increment));
        }

        public override void VisitTableSwitchInsn(int min, int max, Label dflt, params Label[] labels)
        {
            Instructions.Add(new TableSwitchInsnNode(min, max, GetLabelNode(dflt), GetLabelNodes(labels)));
        }

        public override void VisitLookupSwitchInsn(Label dflt, int[] keys, Label[] labels)
        {
            Instructions.Add(new LookupSwitchInsnNode(GetLabelNode(dflt), keys, GetLabelNodes(labels)));
        }

        public override void VisitMultiANewArrayInsn(string descriptor, int numDimensions)
        {
            Instructions.Add(new MultiANewArrayInsnNode(descriptor, numDimensions));
        }

        public override AnnotationVisitor VisitInsnAnnotation(int typeRef, TypePath typePath, string descriptor, bool visible)
        {
            // Find the last real instruction, i.e. the instruction targeted by this annotation.
            var currentInsn = Instructions.Last;
            while (currentInsn.Opcode == -1)
            {
                currentInsn = currentInsn.Previous;
            }

            // Add the annotation to this instruction.
            var typeAnnotation = new TypeAnnotationNode(typeRef, typePath, descriptor);
            if (visible)
            {
                currentInsn.VisibleTypeAnnotations = Util.Add(currentInsn.VisibleTypeAnnotations, typeAnnotation);
            }
            else
            {
                currentInsn.InvisibleTypeAnnotations = Util.Add(currentInsn.InvisibleTypeAnnotations, typeAnnotation);
            }

            return typeAnnotation;
        }

        public override void VisitTryCatchBlock(Label start, Label end, Label handler, string type)
        {
            var tryCatchBlock = new TryCatchBlockNode(GetLabelNode(start), GetLabelNode(end), GetLabelNode(handler), type);
            TryCatchBlocks = Util.Add(TryCatchBlocks, tryCatchBlock);
        }

        public override AnnotationVisitor VisitTryCatchAnnotation(int typeRef, TypePath typePath, string descriptor, bool visible)
        {
            var tryCatchBlock = TryCatchBlocks[(typeRef & 0x00FFFF00) >> 8];
            var typeAnnotation = new TypeAnnotationNode(typeRef, typePath, descriptor);
            if (visible)
            {
                tryCatchBlock.VisibleTypeAnnotations = Util.Add(tryCatchBlock.VisibleTypeAnnotations, typeAnnotation);
            }
            else
            {
                tryCatchBlock.InvisibleTypeAnnotations = Util.Add(tryCatchBlock.InvisibleTypeAnnotations, typeAnnotation);
            }

            return typeAnnotation;
        }

        public override void VisitLocalVariable(string name, string descriptor, string signature, Label start, Label end, int index)
        {
            var localVariable = new LocalVariableNode(name, descriptor, signature, GetLabelNode(start), GetLabelNode(end), index);
            LocalVariables = Util.Add(LocalVariables, localVariable);
        }

        public override AnnotationVisitor VisitLocalVariableAnnotation(int typeRef, TypePath typePath, Label[] start, Label[] end, int[] index, string descriptor, bool visible)
        {
            var localVariableAnnotation = new LocalVariableAnnotationNode(typeRef, typePath, GetLabelNodes(start), GetLabelNodes(end), index, descriptor);
            if (visible)
            {
                VisibleLocalVariableAnnotations = Util.Add(VisibleLocalVariableAnnotations, localVariableAnnotation);
            }
            else
            {
                InvisibleLocalVariableAnnotations = Util.Add(InvisibleLocalVariableAnnotations, localVariableAnnotation);
            }

            return localVariableAnnotation;
        }

        public override void VisitLineNumber(int line, Label start)
        {
            Instructions.Add(new LineNumberNode(line, GetLabelNode(start)));
        }

        public override void VisitMaxs(int maxStack, int maxLocals)
        {
            this.MaxStack = maxStack;
            this.MaxLocals = maxLocals;
        }

        public override void VisitEnd()
        {
        // Nothing to do.
        }

        /// <summary>
        /// Returns the LabelNode corresponding to the given Label. Creates a new LabelNode if necessary.
        /// The default implementation of this method uses the <seealso cref = "Label.info"/> field to store
        /// associations between labels and label nodes.
        /// </summary>
        /// <param name = "label"> a Label. </param>
        /// <returns> the LabelNode corresponding to label. </returns>
        public virtual LabelNode GetLabelNode(Label label)
        {
            if (!(label.Info is LabelNode))
            {
                label.Info = new LabelNode();
            }

            return (LabelNode)label.Info;
        }

        private LabelNode[] GetLabelNodes(Label[] labels)
        {
            var labelNodes = new LabelNode[labels.Length];
            for (int i = 0, n = labels.Length; i < n; ++i)
            {
                labelNodes[i] = GetLabelNode(labels[i]);
            }

            return labelNodes;
        }

        private object[] GetLabelNodes(object[] objects)
        {
            var labelNodes = new object[objects.Length];
            for (int i = 0, n = objects.Length; i < n; ++i)
            {
                var o = objects[i];
                if (o is Label)
                {
                    o = GetLabelNode((Label)o);
                }

                labelNodes[i] = o;
            }

            return labelNodes;
        }

        // -----------------------------------------------------------------------------------------------
        // Accept method
        // -----------------------------------------------------------------------------------------------
        /// <summary>
        /// Checks that this method node is compatible with the given ASM API version. This method checks
        /// that this node, and all its children recursively, do not contain elements that were introduced
        /// in more recent versions of the ASM API than the given version.
        /// </summary>
        /// <param name = "api"> an ASM API version. Must be one of the {@code ASM}<i>x</i> values in {@link
        ///     Opcodes}. </param>
        public virtual void Check(int api)
        {
            if (api == IOpcodes.Asm4)
            {
                if (Parameters != null && Parameters.Count > 0)
                {
                    throw new UnsupportedClassVersionException();
                }

                if (VisibleTypeAnnotations != null && VisibleTypeAnnotations.Count > 0)
                {
                    throw new UnsupportedClassVersionException();
                }

                if (InVisibleTypeAnnotations != null && InVisibleTypeAnnotations.Count > 0)
                {
                    throw new UnsupportedClassVersionException();
                }

                if (TryCatchBlocks != null)
                {
                    for (var i = TryCatchBlocks.Count - 1; i >= 0; --i)
                    {
                        var tryCatchBlock = TryCatchBlocks[i];
                        if (tryCatchBlock.VisibleTypeAnnotations != null && tryCatchBlock.VisibleTypeAnnotations.Count > 0)
                        {
                            throw new UnsupportedClassVersionException();
                        }

                        if (tryCatchBlock.InvisibleTypeAnnotations != null && tryCatchBlock.InvisibleTypeAnnotations.Count > 0)
                        {
                            throw new UnsupportedClassVersionException();
                        }
                    }
                }

                for (var i = Instructions.Count() - 1; i >= 0; --i)
                {
                    var insn = Instructions.Get(i);
                    if (insn.VisibleTypeAnnotations != null && insn.VisibleTypeAnnotations.Count > 0)
                    {
                        throw new UnsupportedClassVersionException();
                    }

                    if (insn.InvisibleTypeAnnotations != null && insn.InvisibleTypeAnnotations.Count > 0)
                    {
                        throw new UnsupportedClassVersionException();
                    }

                    if (insn is MethodInsnNode)
                    {
                        var isInterface = ((MethodInsnNode)insn).Itf;
                        if (isInterface != (insn.opcode == IOpcodes.Invokeinterface))
                        {
                            throw new UnsupportedClassVersionException();
                        }
                    }
                    else if (insn is LdcInsnNode)
                    {
                        var value = ((LdcInsnNode)insn).Cst;
                        if (value is Handle || (value is JType && ((JType)value).Sort == JType.Method))
                        {
                            throw new UnsupportedClassVersionException();
                        }
                    }
                }

                if (VisibleLocalVariableAnnotations != null && VisibleLocalVariableAnnotations.Count > 0)
                {
                    throw new UnsupportedClassVersionException();
                }

                if (InvisibleLocalVariableAnnotations != null && InvisibleLocalVariableAnnotations.Count > 0)
                {
                    throw new UnsupportedClassVersionException();
                }
            }

            if (api < IOpcodes.Asm7)
            {
                for (var i = Instructions.Count() - 1; i >= 0; --i)
                {
                    var insn = Instructions.Get(i);
                    if (insn is LdcInsnNode)
                    {
                        var value = ((LdcInsnNode)insn).Cst;
                        if (value is ConstantDynamic)
                        {
                            throw new UnsupportedClassVersionException();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Makes the given class visitor visit this method.
        /// </summary>
        /// <param name = "classVisitor"> a class visitor. </param>
        public virtual void Accept(ClassVisitor classVisitor)
        {
            var exceptionsArray = Exceptions == null ? null : ((List<string>)Exceptions).ToArray();
            var methodVisitor = classVisitor.VisitMethod(Access, Name, Desc, Signature, exceptionsArray);
            if (methodVisitor != null)
            {
                Accept(methodVisitor);
            }
        }

        /// <summary>
        /// Makes the given method visitor visit this method.
        /// </summary>
        /// <param name = "methodVisitor"> a method visitor. </param>
        public virtual void Accept(MethodVisitor methodVisitor)
        {
            // Visit the parameters.
            if (Parameters != null)
            {
                for (int i = 0, n = Parameters.Count; i < n; i++)
                {
                    Parameters[i].Accept(methodVisitor);
                }
            }

            // Visit the annotations.
            if (AnnotationDefault != null)
            {
                var annotationVisitor = methodVisitor.VisitAnnotationDefault();
                AnnotationNode.Accept(annotationVisitor, null, AnnotationDefault);
                if (annotationVisitor != null)
                {
                    annotationVisitor.VisitEnd();
                }
            }

            if (VisibleAnnotations != null)
            {
                for (int i = 0, n = VisibleAnnotations.Count; i < n; ++i)
                {
                    var annotation = VisibleAnnotations[i];
                    annotation.Accept(methodVisitor.VisitAnnotation(annotation.Desc, true));
                }
            }

            if (InvisibleAnnotations != null)
            {
                for (int i = 0, n = InvisibleAnnotations.Count; i < n; ++i)
                {
                    var annotation = InvisibleAnnotations[i];
                    annotation.Accept(methodVisitor.VisitAnnotation(annotation.Desc, false));
                }
            }

            if (VisibleTypeAnnotations != null)
            {
                for (int i = 0, n = VisibleTypeAnnotations.Count; i < n; ++i)
                {
                    var typeAnnotation = VisibleTypeAnnotations[i];
                    typeAnnotation.Accept(methodVisitor.VisitTypeAnnotation(typeAnnotation.TypeRef, typeAnnotation.TypePath, typeAnnotation.Desc, true));
                }
            }

            if (InVisibleTypeAnnotations != null)
            {
                for (int i = 0, n = InVisibleTypeAnnotations.Count; i < n; ++i)
                {
                    var typeAnnotation = InVisibleTypeAnnotations[i];
                    typeAnnotation.Accept(methodVisitor.VisitTypeAnnotation(typeAnnotation.TypeRef, typeAnnotation.TypePath, typeAnnotation.Desc, false));
                }
            }

            if (VisibleAnnotableParameterCount > 0)
            {
                methodVisitor.VisitAnnotableParameterCount(VisibleAnnotableParameterCount, true);
            }

            if (VisibleParameterAnnotations != null)
            {
                for (int i = 0, n = VisibleParameterAnnotations.Length; i < n; ++i)
                {
                    var parameterAnnotations = VisibleParameterAnnotations[i];
                    if (parameterAnnotations == null)
                    {
                        continue;
                    }

                    for (int j = 0, m = parameterAnnotations.Count; j < m; ++j)
                    {
                        var annotation = parameterAnnotations[j];
                        annotation.Accept(methodVisitor.VisitParameterAnnotation(i, annotation.Desc, true));
                    }
                }
            }

            if (InvisibleAnnotableParameterCount > 0)
            {
                methodVisitor.VisitAnnotableParameterCount(InvisibleAnnotableParameterCount, false);
            }

            if (InvisibleParameterAnnotations != null)
            {
                for (int i = 0, n = InvisibleParameterAnnotations.Length; i < n; ++i)
                {
                    var parameterAnnotations = InvisibleParameterAnnotations[i];
                    if (parameterAnnotations == null)
                    {
                        continue;
                    }

                    for (int j = 0, m = parameterAnnotations.Count; j < m; ++j)
                    {
                        var annotation = parameterAnnotations[j];
                        annotation.Accept(methodVisitor.VisitParameterAnnotation(i, annotation.Desc, false));
                    }
                }
            }

            // Visit the non standard attributes.
            if (_visited)
            {
                Instructions.ResetLabels();
            }

            if (Attrs != null)
            {
                for (int i = 0, n = Attrs.Count; i < n; ++i)
                {
                    methodVisitor.VisitAttribute(Attrs[i]);
                }
            }

            // Visit the code.
            if (Instructions.Count() > 0)
            {
                methodVisitor.VisitCode();
                // Visits the try catch blocks.
                if (TryCatchBlocks != null)
                {
                    for (int i = 0, n = TryCatchBlocks.Count; i < n; ++i)
                    {
                        TryCatchBlocks[i].UpdateIndex(i);
                        TryCatchBlocks[i].Accept(methodVisitor);
                    }
                }

                // Visit the instructions.
                Instructions.Accept(methodVisitor);
                // Visits the local variables.
                if (LocalVariables != null)
                {
                    for (int i = 0, n = LocalVariables.Count; i < n; ++i)
                    {
                        LocalVariables[i].Accept(methodVisitor);
                    }
                }

                // Visits the local variable annotations.
                if (VisibleLocalVariableAnnotations != null)
                {
                    for (int i = 0, n = VisibleLocalVariableAnnotations.Count; i < n; ++i)
                    {
                        VisibleLocalVariableAnnotations[i].Accept(methodVisitor, true);
                    }
                }

                if (InvisibleLocalVariableAnnotations != null)
                {
                    for (int i = 0, n = InvisibleLocalVariableAnnotations.Count; i < n; ++i)
                    {
                        InvisibleLocalVariableAnnotations[i].Accept(methodVisitor, false);
                    }
                }

                methodVisitor.VisitMaxs(MaxStack, MaxStack);
                _visited = true;
            }

            methodVisitor.VisitEnd();
        }
    }
}