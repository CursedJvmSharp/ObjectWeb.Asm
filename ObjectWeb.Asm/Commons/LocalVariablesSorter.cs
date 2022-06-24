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
namespace ObjectWeb.Asm.Commons
{
    /// <summary>
    ///     A <seealso cref="MethodVisitor" /> that renumbers local variables in their order of appearance. This adapter
    ///     allows one to easily add new local variables to a method. It may be used by inheriting from this
    ///     class, but the preferred way of using it is via delegation: the next visitor in the chain can
    ///     indeed add new locals when needed by calling <seealso cref="NewLocal" /> on this adapter (this requires a
    ///     reference back to this <seealso cref="LocalVariablesSorter" />).
    ///     @author Chris Nokleberg
    ///     @author Eugene Kuleshov
    ///     @author Eric Bruneton
    /// </summary>
    public class LocalVariablesSorter : MethodVisitor
    {
        /// <summary>
        ///     The type of the java.lang.Object class.
        /// </summary>
        private static readonly JType OBJECT_TYPE = JType.GetObjectType("java/lang/Object");

        /// <summary>
        ///     The index of the first local variable, after formal parameters.
        /// </summary>
        protected internal readonly int firstLocal;

        /// <summary>
        ///     The index of the next local variable to be created by <seealso cref="NewLocal" />.
        /// </summary>
        protected internal int nextLocal;

        /// <summary>
        ///     The local variable types after remapping. The format of this array is the same as in {@link
        ///     MethodVisitor#visitFrame}, except that long and double types use two slots.
        /// </summary>
        private object[] _remappedLocalTypes = new object[20];

        /// <summary>
        ///     The mapping from old to new local variable indices. A local variable at index i of size 1 is
        ///     remapped to 'mapping[2*i]', while a local variable at index i of size 2 is remapped to
        ///     'mapping[2*i+1]'.
        /// </summary>
        private int[] _remappedVariableIndices = new int[40];

        /// <summary>
        ///     Constructs a new <seealso cref="LocalVariablesSorter" />. <i>Subclasses must not use this constructor</i>.
        ///     Instead, they must use the <seealso cref="LocalVariablesSorter(int, int, string, MethodVisitor)" />
        ///     version.
        /// </summary>
        /// <param name="access"> access flags of the adapted method. </param>
        /// <param name="descriptor"> the method's descriptor (see <seealso cref="Type" />). </param>
        /// <param name="methodVisitor"> the method visitor to which this adapter delegates calls. </param>
        /// <exception cref="IllegalStateException"> if a subclass calls this constructor. </exception>
        public LocalVariablesSorter(int access, string descriptor, MethodVisitor methodVisitor) : this(IOpcodes.Asm9,
            access, descriptor, methodVisitor)
        {
            if (GetType() != typeof(LocalVariablesSorter)) throw new InvalidOperationException();
        }

        /// <summary>
        ///     Constructs a new <seealso cref="LocalVariablesSorter" />.
        /// </summary>
        /// <param name="api">
        ///     the ASM API version implemented by this visitor. Must be one of the {@code
        ///     ASM}<i>x</i> values in <seealso cref="IOpcodes" />.
        /// </param>
        /// <param name="access"> access flags of the adapted method. </param>
        /// <param name="descriptor"> the method's descriptor (see <seealso cref="Type" />). </param>
        /// <param name="methodVisitor"> the method visitor to which this adapter delegates calls. </param>
        public LocalVariablesSorter(int api, int access, string descriptor, MethodVisitor methodVisitor) : base(api,
            methodVisitor)
        {
            nextLocal = (IOpcodes.Acc_Static & access) == 0 ? 1 : 0;
            foreach (var argumentType in JType.GetArgumentTypes(descriptor)) nextLocal += argumentType.Size;
            firstLocal = nextLocal;
        }

        public override void VisitVarInsn(int opcode, int varIndex)
        {
            JType varType;
            switch (opcode)
            {
                case IOpcodes.Lload:
                case IOpcodes.Lstore:
                    varType = JType.LongType;
                    break;
                case IOpcodes.Dload:
                case IOpcodes.Dstore:
                    varType = JType.DoubleType;
                    break;
                case IOpcodes.Fload:
                case IOpcodes.Fstore:
                    varType = JType.FloatType;
                    break;
                case IOpcodes.Iload:
                case IOpcodes.Istore:
                    varType = JType.IntType;
                    break;
                case IOpcodes.Aload:
                case IOpcodes.Astore:
                case IOpcodes.Ret:
                    varType = OBJECT_TYPE;
                    break;
                default:
                    throw new ArgumentException("Invalid opcode " + opcode);
            }

            base.VisitVarInsn(opcode, Remap(varIndex, varType));
        }

        public override void VisitIincInsn(int varIndex, int increment)
        {
            base.VisitIincInsn(Remap(varIndex, JType.IntType), increment);
        }

        public override void VisitMaxs(int maxStack, int maxLocals)
        {
            base.VisitMaxs(maxStack, nextLocal);
        }

        public override void VisitLocalVariable(string name, string descriptor, string signature, Label start,
            Label end, int index)
        {
            var remappedIndex = Remap(index, JType.GetType(descriptor));
            base.VisitLocalVariable(name, descriptor, signature, start, end, remappedIndex);
        }

        public override AnnotationVisitor VisitLocalVariableAnnotation(int typeRef, TypePath typePath, Label[] start,
            Label[] end, int[] index, string descriptor, bool visible)
        {
            var type = JType.GetType(descriptor);
            var remappedIndex = new int[index.Length];
            for (var i = 0; i < remappedIndex.Length; ++i) remappedIndex[i] = Remap(index[i], type);
            return base.VisitLocalVariableAnnotation(typeRef, typePath, start, end, remappedIndex, descriptor, visible);
        }

        public override void VisitFrame(int type, int numLocal, object[] local, int numStack, object[] stack)
        {
            if (type != IOpcodes.F_New)
                // Uncompressed frame.
                throw new ArgumentException(
                    "LocalVariablesSorter only accepts expanded frames (see ClassReader.EXPAND_FRAMES)");

            // Create a copy of remappedLocals.
            var oldRemappedLocals = new object[_remappedLocalTypes.Length];
            Array.Copy(_remappedLocalTypes, 0, oldRemappedLocals, 0, oldRemappedLocals.Length);

            UpdateNewLocals(_remappedLocalTypes);

            // Copy the types from 'local' to 'remappedLocals'. 'remappedLocals' already contains the
            // variables added with 'newLocal'.
            var oldVar = 0; // Old local variable index.
            for (var i = 0; i < numLocal; ++i)
            {
                var localType = local[i];
                if (!Equals(localType, IOpcodes.top))
                {
                    var varType = OBJECT_TYPE;
                    if (Equals(localType, IOpcodes.integer))
                        varType = JType.IntType;
                    else if (Equals(localType, IOpcodes.@float))
                        varType = JType.FloatType;
                    else if (Equals(localType, IOpcodes.@long))
                        varType = JType.LongType;
                    else if (Equals(localType, IOpcodes.@double))
                        varType = JType.DoubleType;
                    else if (localType is string) varType = JType.GetObjectType((string)localType);
                    SetFrameLocal(Remap(oldVar, varType), localType);
                }

                oldVar += Equals(localType, IOpcodes.@long) || Equals(localType, IOpcodes.@double) ? 2 : 1;
            }

            // Remove TOP after long and double types as well as trailing TOPs.
            oldVar = 0;
            var newVar = 0;
            var remappedNumLocal = 0;
            while (oldVar < _remappedLocalTypes.Length)
            {
                var localType = _remappedLocalTypes[oldVar];
                oldVar += Equals(localType, IOpcodes.@long) || Equals(localType, IOpcodes.@double) ? 2 : 1;
                if (localType != null && localType != (object)IOpcodes.top)
                {
                    _remappedLocalTypes[newVar++] = localType;
                    remappedNumLocal = newVar;
                }
                else
                {
                    _remappedLocalTypes[newVar++] = IOpcodes.top;
                }
            }

            // Visit the remapped frame.
            base.VisitFrame(type, remappedNumLocal, _remappedLocalTypes, numStack, stack);

            // Restore the original value of 'remappedLocals'.
            _remappedLocalTypes = oldRemappedLocals;
        }

        // -----------------------------------------------------------------------------------------------

        /// <summary>
        ///     Constructs a new local variable of the given type.
        /// </summary>
        /// <param name="type"> the type of the local variable to be created. </param>
        /// <returns> the identifier of the newly created local variable. </returns>
        public virtual int NewLocal(JType type)
        {
            object localType;
            switch (type.Sort)
            {
                case JType.Boolean:
                case JType.Char:
                case JType.Byte:
                case JType.Short:
                case JType.Int:
                    localType = IOpcodes.integer;
                    break;
                case JType.Float:
                    localType = IOpcodes.@float;
                    break;
                case JType.Long:
                    localType = IOpcodes.@long;
                    break;
                case JType.Double:
                    localType = IOpcodes.@double;
                    break;
                case JType.Array:
                    localType = type.Descriptor;
                    break;
                case JType.Object:
                    localType = type.InternalName;
                    break;
                default:
                    throw new Exception("AssertionError");
            }

            var local = NewLocalMapping(type);
            SetLocalType(local, type);
            SetFrameLocal(local, localType);
            return local;
        }

        /// <summary>
        ///     Notifies subclasses that a new stack map frame is being visited. The array argument contains
        ///     the stack map frame types corresponding to the local variables added with <seealso cref="NewLocal" />.
        ///     This method can update these types in place for the stack map frame being visited. The default
        ///     implementation of this method does nothing, i.e. a local variable added with <seealso cref="NewLocal" />
        ///     will have the same type in all stack map frames. But this behavior is not always the desired
        ///     one, for instance if a local variable is added in the middle of a try/catch block: the frame
        ///     for the exception handler should have a TOP type for this new local.
        /// </summary>
        /// <param name="newLocals">
        ///     the stack map frame types corresponding to the local variables added with
        ///     <seealso cref="NewLocal" /> (and null for the others). The format of this array is the same as in
        ///     <seealso cref="MethodVisitor.VisitFrame" />, except that long and double types use two slots. The
        ///     types for the current stack map frame must be updated in place in this array.
        /// </param>
        public virtual void UpdateNewLocals(object[] newLocals)
        {
            // The default implementation does nothing.
        }

        /// <summary>
        ///     Notifies subclasses that a local variable has been added or remapped. The default
        ///     implementation of this method does nothing.
        /// </summary>
        /// <param name="local"> a local variable identifier, as returned by <seealso cref="NewLocal" />. </param>
        /// <param name="type"> the type of the value being stored in the local variable. </param>
        public virtual void SetLocalType(int local, JType type)
        {
            // The default implementation does nothing.
        }

        private void SetFrameLocal(int local, object type)
        {
            var numLocals = _remappedLocalTypes.Length;
            if (local >= numLocals)
            {
                var newRemappedLocalTypes = new object[Math.Max(2 * numLocals, local + 1)];
                Array.Copy(_remappedLocalTypes, 0, newRemappedLocalTypes, 0, numLocals);
                _remappedLocalTypes = newRemappedLocalTypes;
            }

            _remappedLocalTypes[local] = type;
        }

        private int Remap(int var, JType type)
        {
            if (var + type.Size <= firstLocal) return var;
            var key = 2 * var + type.Size - 1;
            var size = _remappedVariableIndices.Length;
            if (key >= size)
            {
                var newRemappedVariableIndices = new int[Math.Max(2 * size, key + 1)];
                Array.Copy(_remappedVariableIndices, 0, newRemappedVariableIndices, 0, size);
                _remappedVariableIndices = newRemappedVariableIndices;
            }

            var value = _remappedVariableIndices[key];
            if (value == 0)
            {
                value = NewLocalMapping(type);
                SetLocalType(value, type);
                _remappedVariableIndices[key] = value + 1;
            }
            else
            {
                value--;
            }

            return value;
        }

        public virtual int NewLocalMapping(JType type)
        {
            var local = nextLocal;
            nextLocal += type.Size;
            return local;
        }
    }
}