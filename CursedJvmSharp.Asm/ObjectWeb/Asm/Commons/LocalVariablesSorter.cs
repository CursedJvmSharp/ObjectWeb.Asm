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
	/// A <seealso cref="MethodVisitor"/> that renumbers local variables in their order of appearance. This adapter
	/// allows one to easily add new local variables to a method. It may be used by inheriting from this
	/// class, but the preferred way of using it is via delegation: the next visitor in the chain can
	/// indeed add new locals when needed by calling <seealso cref="newLocal"/> on this adapter (this requires a
	/// reference back to this <seealso cref="LocalVariablesSorter"/>).
	/// 
	/// @author Chris Nokleberg
	/// @author Eugene Kuleshov
	/// @author Eric Bruneton
	/// </summary>
	public class LocalVariablesSorter : MethodVisitor
	{

	  /// <summary>
	  /// The type of the java.lang.Object class. </summary>
	  private static readonly JType OBJECT_TYPE = JType.getObjectType("java/lang/Object");

	  /// <summary>
	  /// The mapping from old to new local variable indices. A local variable at index i of size 1 is
	  /// remapped to 'mapping[2*i]', while a local variable at index i of size 2 is remapped to
	  /// 'mapping[2*i+1]'.
	  /// </summary>
	  private int[] remappedVariableIndices = new int[40];

	  /// <summary>
	  /// The local variable types after remapping. The format of this array is the same as in {@link
	  /// MethodVisitor#visitFrame}, except that long and double types use two slots.
	  /// </summary>
	  private object[] remappedLocalTypes = new object[20];

	  /// <summary>
	  /// The index of the first local variable, after formal parameters. </summary>
	  protected internal readonly int firstLocal;

	  /// <summary>
	  /// The index of the next local variable to be created by <seealso cref="newLocal"/>. </summary>
	  protected internal int nextLocal;

	  /// <summary>
	  /// Constructs a new <seealso cref="LocalVariablesSorter"/>. <i>Subclasses must not use this constructor</i>.
	  /// Instead, they must use the <seealso cref="LocalVariablesSorter(int, int, String, MethodVisitor)"/>
	  /// version.
	  /// </summary>
	  /// <param name="access"> access flags of the adapted method. </param>
	  /// <param name="descriptor"> the method's descriptor (see <seealso cref="Type"/>). </param>
	  /// <param name="methodVisitor"> the method visitor to which this adapter delegates calls. </param>
	  /// <exception cref="IllegalStateException"> if a subclass calls this constructor. </exception>
	  public LocalVariablesSorter(int access, string descriptor, MethodVisitor methodVisitor) : this(Opcodes.ASM9, access, descriptor, methodVisitor)
	  {
		if (this.GetType() != typeof(LocalVariablesSorter))
		{
		  throw new System.InvalidOperationException();
		}
	  }

	  /// <summary>
	  /// Constructs a new <seealso cref="LocalVariablesSorter"/>.
	  /// </summary>
	  /// <param name="api"> the ASM API version implemented by this visitor. Must be one of the {@code
	  ///     ASM}<i>x</i> values in <seealso cref="Opcodes"/>. </param>
	  /// <param name="access"> access flags of the adapted method. </param>
	  /// <param name="descriptor"> the method's descriptor (see <seealso cref="Type"/>). </param>
	  /// <param name="methodVisitor"> the method visitor to which this adapter delegates calls. </param>
	  public LocalVariablesSorter(int api, int access, string descriptor, MethodVisitor methodVisitor) : base(api, methodVisitor)
	  {
		nextLocal = (Opcodes.ACC_STATIC & access) == 0 ? 1 : 0;
		foreach (JType argumentType in JType.getArgumentTypes(descriptor))
		{
		  nextLocal += argumentType.Size;
		}
		firstLocal = nextLocal;
	  }

	  public override void visitVarInsn(int opcode, int var)
	  {
		JType varType;
		switch (opcode)
		{
		  case Opcodes.LLOAD:
		  case Opcodes.LSTORE:
			varType = JType.LONG_TYPE;
			break;
		  case Opcodes.DLOAD:
		  case Opcodes.DSTORE:
			varType = JType.DOUBLE_TYPE;
			break;
		  case Opcodes.FLOAD:
		  case Opcodes.FSTORE:
			varType = JType.FLOAT_TYPE;
			break;
		  case Opcodes.ILOAD:
		  case Opcodes.ISTORE:
			varType = JType.INT_TYPE;
			break;
		  case Opcodes.ALOAD:
		  case Opcodes.ASTORE:
		  case Opcodes.RET:
			varType = OBJECT_TYPE;
			break;
		  default:
			throw new System.ArgumentException("Invalid opcode " + opcode);
		}
		base.visitVarInsn(opcode, remap(var, varType));
	  }

	  public override void visitIincInsn(int var, int increment)
	  {
		base.visitIincInsn(remap(var, JType.INT_TYPE), increment);
	  }

	  public override void visitMaxs(int maxStack, int maxLocals)
	  {
		base.visitMaxs(maxStack, nextLocal);
	  }

	  public override void visitLocalVariable(string name, string descriptor, string signature, Label start, Label end, int index)
	  {
		int remappedIndex = remap(index, JType.getType(descriptor));
		base.visitLocalVariable(name, descriptor, signature, start, end, remappedIndex);
	  }

	  public override AnnotationVisitor visitLocalVariableAnnotation(int typeRef, TypePath typePath, Label[] start, Label[] end, int[] index, string descriptor, bool visible)
	  {
		JType type = JType.getType(descriptor);
		int[] remappedIndex = new int[index.Length];
		for (int i = 0; i < remappedIndex.Length; ++i)
		{
		  remappedIndex[i] = remap(index[i], type);
		}
		return base.visitLocalVariableAnnotation(typeRef, typePath, start, end, remappedIndex, descriptor, visible);
	  }

	  public override void visitFrame(int type, int numLocal, object[] local, int numStack, object[] stack)
	  {
		if (type != Opcodes.F_NEW)
		{ // Uncompressed frame.
		  throw new System.ArgumentException("LocalVariablesSorter only accepts expanded frames (see ClassReader.EXPAND_FRAMES)");
		}

		// Create a copy of remappedLocals.
		object[] oldRemappedLocals = new object[remappedLocalTypes.Length];
		Array.Copy(remappedLocalTypes, 0, oldRemappedLocals, 0, oldRemappedLocals.Length);

		updateNewLocals(remappedLocalTypes);

		// Copy the types from 'local' to 'remappedLocals'. 'remappedLocals' already contains the
		// variables added with 'newLocal'.
		int oldVar = 0; // Old local variable index.
		for (int i = 0; i < numLocal; ++i)
		{
		  object localType = local[i];
		  if (!Equals(localType, Opcodes.TOP))
		  {
			JType varType = OBJECT_TYPE;
			if (Equals(localType, Opcodes.INTEGER))
			{
			  varType = JType.INT_TYPE;
			}
			else if (Equals(localType, Opcodes.FLOAT))
			{
			  varType = JType.FLOAT_TYPE;
			}
			else if (Equals(localType, Opcodes.LONG))
			{
			  varType = JType.LONG_TYPE;
			}
			else if (Equals(localType, Opcodes.DOUBLE))
			{
			  varType = JType.DOUBLE_TYPE;
			}
			else if (localType is string)
			{
			  varType = JType.getObjectType((string) localType);
			}
			setFrameLocal(remap(oldVar, varType), localType);
		  }
		  oldVar += Equals(localType, Opcodes.LONG) || Equals(localType, Opcodes.DOUBLE) ? 2 : 1;
		}

		// Remove TOP after long and double types as well as trailing TOPs.
		oldVar = 0;
		int newVar = 0;
		int remappedNumLocal = 0;
		while (oldVar < remappedLocalTypes.Length)
		{
		  object localType = remappedLocalTypes[oldVar];
		  oldVar += Equals(localType, Opcodes.LONG) || Equals(localType, Opcodes.DOUBLE) ? 2 : 1;
		  if (localType != null && localType != (object)Opcodes.TOP)
		  {
			remappedLocalTypes[newVar++] = localType;
			remappedNumLocal = newVar;
		  }
		  else
		  {
			remappedLocalTypes[newVar++] = Opcodes.TOP;
		  }
		}

		// Visit the remapped frame.
		base.visitFrame(type, remappedNumLocal, remappedLocalTypes, numStack, stack);

		// Restore the original value of 'remappedLocals'.
		remappedLocalTypes = oldRemappedLocals;
	  }

	  // -----------------------------------------------------------------------------------------------

	  /// <summary>
	  /// Constructs a new local variable of the given type.
	  /// </summary>
	  /// <param name="type"> the type of the local variable to be created. </param>
	  /// <returns> the identifier of the newly created local variable. </returns>
	  public virtual int newLocal(JType type)
	  {
		object localType;
		switch (type.Sort)
		{
		  case JType.BOOLEAN:
		  case JType.CHAR:
		  case JType.BYTE:
		  case JType.SHORT:
		  case JType.INT:
			localType = Opcodes.INTEGER;
			break;
		  case JType.FLOAT:
			localType = Opcodes.FLOAT;
			break;
		  case JType.LONG:
			localType = Opcodes.LONG;
			break;
		  case JType.DOUBLE:
			localType = Opcodes.DOUBLE;
			break;
		  case JType.ARRAY:
			localType = type.Descriptor;
			break;
		  case JType.OBJECT:
			localType = type.InternalName;
			break;
		  default:
			throw new ("AssertionError");
		}
		int local = newLocalMapping(type);
		setLocalType(local, type);
		setFrameLocal(local, localType);
		return local;
	  }

	  /// <summary>
	  /// Notifies subclasses that a new stack map frame is being visited. The array argument contains
	  /// the stack map frame types corresponding to the local variables added with <seealso cref="newLocal"/>.
	  /// This method can update these types in place for the stack map frame being visited. The default
	  /// implementation of this method does nothing, i.e. a local variable added with <seealso cref="newLocal"/>
	  /// will have the same type in all stack map frames. But this behavior is not always the desired
	  /// one, for instance if a local variable is added in the middle of a try/catch block: the frame
	  /// for the exception handler should have a TOP type for this new local.
	  /// </summary>
	  /// <param name="newLocals"> the stack map frame types corresponding to the local variables added with
	  ///     <seealso cref="newLocal"/> (and null for the others). The format of this array is the same as in
	  ///     <seealso cref="MethodVisitor.visitFrame"/>, except that long and double types use two slots. The
	  ///     types for the current stack map frame must be updated in place in this array. </param>
	  public virtual void updateNewLocals(object[] newLocals)
	  {
		// The default implementation does nothing.
	  }

	  /// <summary>
	  /// Notifies subclasses that a local variable has been added or remapped. The default
	  /// implementation of this method does nothing.
	  /// </summary>
	  /// <param name="local"> a local variable identifier, as returned by <seealso cref="newLocal"/>. </param>
	  /// <param name="type"> the type of the value being stored in the local variable. </param>
	  public virtual void setLocalType(int local, JType type)
	  {
		// The default implementation does nothing.
	  }

	  private void setFrameLocal(int local, object type)
	  {
		int numLocals = remappedLocalTypes.Length;
		if (local >= numLocals)
		{
		  object[] newRemappedLocalTypes = new object[Math.Max(2 * numLocals, local + 1)];
		  Array.Copy(remappedLocalTypes, 0, newRemappedLocalTypes, 0, numLocals);
		  remappedLocalTypes = newRemappedLocalTypes;
		}
		remappedLocalTypes[local] = type;
	  }

	  private int remap(int var, JType type)
	  {
		if (var + type.Size <= firstLocal)
		{
		  return var;
		}
		int key = 2 * var + type.Size - 1;
		int size = remappedVariableIndices.Length;
		if (key >= size)
		{
		  int[] newRemappedVariableIndices = new int[Math.Max(2 * size, key + 1)];
		  Array.Copy(remappedVariableIndices, 0, newRemappedVariableIndices, 0, size);
		  remappedVariableIndices = newRemappedVariableIndices;
		}
		int value = remappedVariableIndices[key];
		if (value == 0)
		{
		  value = newLocalMapping(type);
		  setLocalType(value, type);
		  remappedVariableIndices[key] = value + 1;
		}
		else
		{
		  value--;
		}
		return value;
	  }

	  public virtual int newLocalMapping(JType type)
	  {
		int local = nextLocal;
		nextLocal += type.Size;
		return local;
	  }
	}

}