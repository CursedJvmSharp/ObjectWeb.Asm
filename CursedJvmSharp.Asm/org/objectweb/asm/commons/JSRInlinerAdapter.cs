using org.objectweb.asm;
using CursedJvmSharp.Asm.Java.IO;
using System.Collections;
using System.Collections.Generic;
using Label = org.objectweb.asm.Label;
using MethodVisitor = org.objectweb.asm.MethodVisitor;
using Opcodes = org.objectweb.asm.Opcodes;
using AbstractInsnNode = org.objectweb.asm.tree.AbstractInsnNode;
using InsnList = org.objectweb.asm.tree.InsnList;
using InsnNode = org.objectweb.asm.tree.InsnNode;
using JumpInsnNode = org.objectweb.asm.tree.JumpInsnNode;
using LabelNode = org.objectweb.asm.tree.LabelNode;
using LocalVariableNode = org.objectweb.asm.tree.LocalVariableNode;
using LookupSwitchInsnNode = org.objectweb.asm.tree.LookupSwitchInsnNode;
using MethodNode = org.objectweb.asm.tree.MethodNode;
using TableSwitchInsnNode = org.objectweb.asm.tree.TableSwitchInsnNode;
using TryCatchBlockNode = org.objectweb.asm.tree.TryCatchBlockNode;

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
	/// A <seealso cref="org.objectweb.asm.MethodVisitor"/> that removes JSR instructions and inlines the
	/// referenced subroutines.
	/// 
	/// @author Niko Matsakis
	/// </summary>
	// DontCheck(AbbreviationAsWordInName): can't be renamed (for backward binary compatibility).
	public class JSRInlinerAdapter : MethodNode, Opcodes
	{

	  /// <summary>
	  /// The instructions that belong to the main "subroutine". Bit i is set iff instruction at index i
	  /// belongs to this main "subroutine".
	  /// </summary>
	  private readonly BitArray mainSubroutineInsns = new BitArray();

	  /// <summary>
	  /// The instructions that belong to each subroutine. For each label which is the target of a JSR
	  /// instruction, bit i of the corresponding BitSet in this map is set iff instruction at index i
	  /// belongs to this subroutine.
	  /// </summary>
	  private readonly IDictionary<LabelNode, BitArray> subroutinesInsns = new Dictionary<LabelNode, BitArray>();

	  /// <summary>
	  /// The instructions that belong to more that one subroutine. Bit i is set iff instruction at index
	  /// i belongs to more than one subroutine.
	  /// </summary>
	  internal readonly BitArray sharedSubroutineInsns = new BitArray();

	  /// <summary>
	  /// Constructs a new <seealso cref="JSRInlinerAdapter"/>. <i>Subclasses must not use this constructor</i>.
	  /// Instead, they must use the {@link #JSRInlinerAdapter(int, MethodVisitor, int, String, String,
	  /// String, String[])} version.
	  /// </summary>
	  /// <param name="methodVisitor"> the method visitor to send the resulting inlined method code to, or <code>
	  ///     null</code>. </param>
	  /// <param name="access"> the method's access flags. </param>
	  /// <param name="name"> the method's name. </param>
	  /// <param name="descriptor"> the method's descriptor. </param>
	  /// <param name="signature"> the method's signature. May be {@literal null}. </param>
	  /// <param name="exceptions"> the internal names of the method's exception classes. May be {@literal null}. </param>
	  /// <exception cref="IllegalStateException"> if a subclass calls this constructor. </exception>
	  public JSRInlinerAdapter(MethodVisitor methodVisitor, int access, string name, string descriptor, string signature, string[] exceptions) : this(Opcodes.ASM9, methodVisitor, access, name, descriptor, signature, exceptions)
	  {
		if (this.GetType() != typeof(JSRInlinerAdapter))
		{
		  throw new System.InvalidOperationException();
		}
	  }

	  /// <summary>
	  /// Constructs a new <seealso cref="JSRInlinerAdapter"/>.
	  /// </summary>
	  /// <param name="api"> the ASM API version implemented by this visitor. Must be one of the {@code
	  ///     ASM}<i>x</i> values in <seealso cref="Opcodes"/>. </param>
	  /// <param name="methodVisitor"> the method visitor to send the resulting inlined method code to, or <code>
	  ///     null</code>. </param>
	  /// <param name="access"> the method's access flags (see <seealso cref="Opcodes"/>). This parameter also indicates if
	  ///     the method is synthetic and/or deprecated. </param>
	  /// <param name="name"> the method's name. </param>
	  /// <param name="descriptor"> the method's descriptor. </param>
	  /// <param name="signature"> the method's signature. May be {@literal null}. </param>
	  /// <param name="exceptions"> the internal names of the method's exception classes. May be {@literal null}. </param>
	  public JSRInlinerAdapter(int api, MethodVisitor methodVisitor, int access, string name, string descriptor, string signature, string[] exceptions) : base(api, access, name, descriptor, signature, exceptions)
	  {
		this.mv = methodVisitor;
	  }

	  public override void visitJumpInsn(int opcode, Label label)
	  {
		base.visitJumpInsn(opcode, label);
		LabelNode labelNode = ((JumpInsnNode) instructions.Last).label;
		if (opcode == JSR && !subroutinesInsns.ContainsKey(labelNode))
		{
		  subroutinesInsns[labelNode] = new BitArray();
		}
	  }

	  public override void visitEnd()
	  {
		if (subroutinesInsns.Count > 0)
		{
		  // If the code contains at least one JSR instruction, inline the subroutines.
		  findSubroutinesInsns();
		  emitCode();
		}
		if (mv != null)
		{
		  accept(mv);
		}
	  }

	  /// <summary>
	  /// Determines, for each instruction, to which subroutine(s) it belongs. </summary>
	  private void findSubroutinesInsns()
	  {
		// Find the instructions that belong to main subroutine.
		BitArray visitedInsns = new BitArray();
		findSubroutineInsns(0, mainSubroutineInsns, visitedInsns);
		// For each subroutine, find the instructions that belong to this subroutine.
		foreach (KeyValuePair<LabelNode, BitArray> entry in subroutinesInsns.SetOfKeyValuePairs())
		{
		  LabelNode jsrLabelNode = entry.Key;
		  BitArray subroutineInsns = entry.Value;
		  findSubroutineInsns(instructions.indexOf(jsrLabelNode), subroutineInsns, visitedInsns);
		}
	  }

	  /// <summary>
	  /// Finds the instructions that belong to the subroutine starting at the given instruction index.
	  /// For this the control flow graph is visited with a depth first search (this includes the normal
	  /// control flow and the exception handlers).
	  /// </summary>
	  /// <param name="startInsnIndex"> the index of the first instruction of the subroutine. </param>
	  /// <param name="subroutineInsns"> where the indices of the instructions of the subroutine must be stored. </param>
	  /// <param name="visitedInsns"> the indices of the instructions that have been visited so far (including in
	  ///     previous calls to this method). This bitset is updated by this method each time a new
	  ///     instruction is visited. It is used to make sure each instruction is visited at most once. </param>
	  private void findSubroutineInsns(int startInsnIndex, BitArray subroutineInsns, BitArray visitedInsns)
	  {
		// First find the instructions reachable via normal execution.
		findReachableInsns(startInsnIndex, subroutineInsns, visitedInsns);

		// Then find the instructions reachable via the applicable exception handlers.
		while (true)
		{
		  bool applicableHandlerFound = false;
		  foreach (TryCatchBlockNode tryCatchBlockNode in tryCatchBlocks)
		  {
			// If the handler has already been processed, skip it.
			int handlerIndex = instructions.indexOf(tryCatchBlockNode.handler);
			if (subroutineInsns.Get(handlerIndex))
			{
			  continue;
			}

			// If an instruction in the exception handler range belongs to the subroutine, the handler
			// can be reached from the routine, and its instructions must be added to the subroutine.
			int startIndex = instructions.indexOf(tryCatchBlockNode.start);
			int endIndex = instructions.indexOf(tryCatchBlockNode.end);
			int firstSubroutineInsnAfterTryCatchStart = subroutineInsns.nextSetBit(startIndex);
			if (firstSubroutineInsnAfterTryCatchStart >= startIndex && firstSubroutineInsnAfterTryCatchStart < endIndex)
			{
			  findReachableInsns(handlerIndex, subroutineInsns, visitedInsns);
			  applicableHandlerFound = true;
			}
		  }
		  // If an applicable exception handler has been found, other handlers may become applicable, so
		  // we must examine them again.
		  if (!applicableHandlerFound)
		  {
			return;
		  }
		}
	  }

	  /// <summary>
	  /// Finds the instructions that are reachable from the given instruction, without following any JSR
	  /// instruction nor any exception handler. For this the control flow graph is visited with a depth
	  /// first search.
	  /// </summary>
	  /// <param name="insnIndex"> the index of an instruction of the subroutine. </param>
	  /// <param name="subroutineInsns"> where the indices of the instructions of the subroutine must be stored. </param>
	  /// <param name="visitedInsns"> the indices of the instructions that have been visited so far (including in
	  ///     previous calls to this method). This bitset is updated by this method each time a new
	  ///     instruction is visited. It is used to make sure each instruction is visited at most once. </param>
	  private void findReachableInsns(int insnIndex, BitArray subroutineInsns, BitArray visitedInsns)
	  {
		int currentInsnIndex = insnIndex;
		// We implicitly assume below that execution can always fall through to the next instruction
		// after a JSR. But a subroutine may never return, in which case the code after the JSR is
		// unreachable and can be anything. In particular, it can seem to fall off the end of the
		// method, so we must handle this case here (we could instead detect whether execution can
		// return or not from a JSR, but this is more complicated).
		while (currentInsnIndex < instructions.size())
		{
		  // Visit each instruction at most once.
		  if (subroutineInsns.Get(currentInsnIndex))
		  {
			return;
		  }
		  subroutineInsns.Set(currentInsnIndex, true);

		  // Check if this instruction has already been visited by another subroutine.
		  if (visitedInsns.Get(currentInsnIndex))
		  {
			sharedSubroutineInsns.Set(currentInsnIndex, true);
		  }
		  visitedInsns.Set(currentInsnIndex, true);

		  AbstractInsnNode currentInsnNode = instructions.get(currentInsnIndex);
		  if (currentInsnNode.Type == AbstractInsnNode.JUMP_INSN && currentInsnNode.Opcode != JSR)
		  {
			// Don't follow JSR instructions in the control flow graph.
			JumpInsnNode jumpInsnNode = (JumpInsnNode) currentInsnNode;
			findReachableInsns(instructions.indexOf(jumpInsnNode.label), subroutineInsns, visitedInsns);
		  }
		  else if (currentInsnNode.Type == AbstractInsnNode.TABLESWITCH_INSN)
		  {
			TableSwitchInsnNode tableSwitchInsnNode = (TableSwitchInsnNode) currentInsnNode;
			findReachableInsns(instructions.indexOf(tableSwitchInsnNode.dflt), subroutineInsns, visitedInsns);
			foreach (LabelNode labelNode in tableSwitchInsnNode.labels)
			{
			  findReachableInsns(instructions.indexOf(labelNode), subroutineInsns, visitedInsns);
			}
		  }
		  else if (currentInsnNode.Type == AbstractInsnNode.LOOKUPSWITCH_INSN)
		  {
			LookupSwitchInsnNode lookupSwitchInsnNode = (LookupSwitchInsnNode) currentInsnNode;
			findReachableInsns(instructions.indexOf(lookupSwitchInsnNode.dflt), subroutineInsns, visitedInsns);
			foreach (LabelNode labelNode in lookupSwitchInsnNode.labels)
			{
			  findReachableInsns(instructions.indexOf(labelNode), subroutineInsns, visitedInsns);
			}
		  }

		  // Check if this instruction falls through to the next instruction; if not, return.
		  switch (instructions.get(currentInsnIndex).getOpcode())
		  {
			case GOTO:
			case RET:
			case TABLESWITCH:
			case LOOKUPSWITCH:
			case IRETURN:
			case LRETURN:
			case FRETURN:
			case DRETURN:
			case ARETURN:
			case RETURN:
			case ATHROW:
			  // Note: this either returns from this subroutine, or from a parent subroutine.
			  return;
			default:
			  // Go to the next instruction.
			  currentInsnIndex++;
			  break;
		  }
		}
	  }

	  /// <summary>
	  /// Creates the new instructions, inlining each instantiation of each subroutine until the code is
	  /// fully elaborated.
	  /// </summary>
	  private void emitCode()
	  {
		LinkedList<Instantiation> worklist = new LinkedList<Instantiation>();
		// Create an instantiation of the main "subroutine", which is just the main routine.
		worklist.AddLast(new Instantiation(this, null, mainSubroutineInsns));

		// Emit instantiations of each subroutine we encounter, including the main subroutine.
		InsnList newInstructions = new InsnList();
		IList<TryCatchBlockNode> newTryCatchBlocks = new List<TryCatchBlockNode>();
		IList<LocalVariableNode> newLocalVariables = new List<LocalVariableNode>();
		while (worklist.Count > 0)
		{
		  Instantiation instantiation = worklist.RemoveFirst();
		  emitInstantiation(instantiation, worklist, newInstructions, newTryCatchBlocks, newLocalVariables);
		}
		instructions = newInstructions;
		tryCatchBlocks = newTryCatchBlocks;
		localVariables = newLocalVariables;
	  }

	  /// <summary>
	  /// Emits an instantiation of a subroutine, specified by <code>instantiation</code>. May add new
	  /// instantiations that are invoked by this one to the <code>worklist</code>, and new try/catch
	  /// blocks to <code>newTryCatchBlocks</code>.
	  /// </summary>
	  /// <param name="instantiation"> the instantiation that must be performed. </param>
	  /// <param name="worklist"> list of the instantiations that remain to be done. </param>
	  /// <param name="newInstructions"> the instruction list to which the instantiated code must be appended. </param>
	  /// <param name="newTryCatchBlocks"> the exception handler list to which the instantiated handlers must be
	  ///     appended. </param>
	  /// <param name="newLocalVariables"> the local variables list to which the instantiated local variables
	  ///     must be appended. </param>
	  private void emitInstantiation(Instantiation instantiation, IList<Instantiation> worklist, InsnList newInstructions, IList<TryCatchBlockNode> newTryCatchBlocks, IList<LocalVariableNode> newLocalVariables)
	  {
		LabelNode previousLabelNode = null;
		for (int i = 0; i < instructions.size(); ++i)
		{
		  AbstractInsnNode insnNode = instructions.get(i);
		  if (insnNode.Type == AbstractInsnNode.LABEL)
		  {
			// Always clone all labels, while avoiding to add the same label more than once.
			LabelNode labelNode = (LabelNode) insnNode;
			LabelNode clonedLabelNode = instantiation.getClonedLabel(labelNode);
			if (clonedLabelNode != previousLabelNode)
			{
			  newInstructions.add(clonedLabelNode);
			  previousLabelNode = clonedLabelNode;
			}
		  }
		  else if (instantiation.findOwner(i) == instantiation)
		  {
			// Don't emit instructions that were already emitted by an ancestor subroutine. Note that it
			// is still possible for a given instruction to be emitted twice because it may belong to
			// two subroutines that do not invoke each other.

			if (insnNode.Opcode == RET)
			{
			  // Translate RET instruction(s) to a jump to the return label for the appropriate
			  // instantiation. The problem is that the subroutine may "fall through" to the ret of a
			  // parent subroutine; therefore, to find the appropriate ret label we find the oldest
			  // instantiation that claims to own this instruction.
			  LabelNode retLabel = null;
			  for (Instantiation retLabelOwner = instantiation; retLabelOwner != null; retLabelOwner = retLabelOwner.parent)
			  {
				if (retLabelOwner.subroutineInsns.Get(i))
				{
				  retLabel = retLabelOwner.returnLabel;
				}
			  }
			  if (retLabel == null)
			  {
				// This is only possible if the mainSubroutine owns a RET instruction, which should
				// never happen for verifiable code.
				throw new System.ArgumentException("Instruction #" + i + " is a RET not owned by any subroutine");
			  }
			  newInstructions.add(new JumpInsnNode(GOTO, retLabel));
			}
			else if (insnNode.Opcode == JSR)
			{
			  LabelNode jsrLabelNode = ((JumpInsnNode) insnNode).label;
			  BitArray subroutineInsns = subroutinesInsns.GetValueOrNull(jsrLabelNode);
			  Instantiation newInstantiation = new Instantiation(this, instantiation, subroutineInsns);
			  LabelNode clonedJsrLabelNode = newInstantiation.getClonedLabelForJumpInsn(jsrLabelNode);
			  // Replace the JSR instruction with a GOTO to the instantiated subroutine, and push NULL
			  // for what was once the return address value. This hack allows us to avoid doing any sort
			  // of data flow analysis to figure out which instructions manipulate the old return
			  // address value pointer which is now known to be unneeded.
			  newInstructions.add(new InsnNode(ACONST_NULL));
			  newInstructions.add(new JumpInsnNode(GOTO, clonedJsrLabelNode));
			  newInstructions.add(newInstantiation.returnLabel);
			  // Insert this new instantiation into the queue to be emitted later.
			  worklist.Add(newInstantiation);
			}
			else
			{
			  newInstructions.add(insnNode.clone(instantiation));
			}
		  }
		}

		// Emit the try/catch blocks that are relevant for this instantiation.
		foreach (TryCatchBlockNode tryCatchBlockNode in tryCatchBlocks)
		{
		  LabelNode start = instantiation.getClonedLabel(tryCatchBlockNode.start);
		  LabelNode end = instantiation.getClonedLabel(tryCatchBlockNode.end);
		  if (start != end)
		  {
			LabelNode handler = instantiation.getClonedLabelForJumpInsn(tryCatchBlockNode.handler);
			if (start == null || end == null || handler == null)
			{
			  throw new AssertionError("Internal error!");
			}
			newTryCatchBlocks.Add(new TryCatchBlockNode(start, end, handler, tryCatchBlockNode.type));
		  }
		}

		// Emit the local variable nodes that are relevant for this instantiation.
		foreach (LocalVariableNode localVariableNode in localVariables)
		{
		  LabelNode start = instantiation.getClonedLabel(localVariableNode.start);
		  LabelNode end = instantiation.getClonedLabel(localVariableNode.end);
		  if (start != end)
		  {
			newLocalVariables.Add(new LocalVariableNode(localVariableNode.name, localVariableNode.desc, localVariableNode.signature, start, end, localVariableNode.index));
		  }
		}
	  }

	  /// <summary>
	  /// An instantiation of a subroutine. </summary>
	  private class Instantiation : AbstractMap<LabelNode, LabelNode>
	  {
		  private readonly JSRInlinerAdapter outerInstance;


		/// <summary>
		/// The instantiation from which this one was created (or {@literal null} for the instantiation
		/// of the main "subroutine").
		/// </summary>
		internal readonly Instantiation parent;

		/// <summary>
		/// The original instructions that belong to the subroutine which is instantiated. Bit i is set
		/// iff instruction at index i belongs to this subroutine.
		/// </summary>
		internal readonly BitArray subroutineInsns;

		/// <summary>
		/// A map from labels from the original code to labels pointing at code specific to this
		/// instantiation, for use in remapping try/catch blocks, as well as jumps.
		/// 
		/// <para>Note that in the presence of instructions belonging to several subroutines, we map the
		/// target label of a GOTO to the label used by the oldest instantiation (parent instantiations
		/// are older than their children). This avoids code duplication during inlining in most cases.
		/// </para>
		/// </summary>
		internal readonly IDictionary<LabelNode, LabelNode> clonedLabels;

		/// <summary>
		/// The return label for this instantiation, to which all original returns will be mapped. </summary>
		internal readonly LabelNode returnLabel;

		public Instantiation(JSRInlinerAdapter outerInstance, Instantiation parent, BitArray subroutineInsns)
		{
			this.outerInstance = outerInstance;
		  for (Instantiation instantiation = parent; instantiation != null; instantiation = instantiation.parent)
		  {
			if (instantiation.subroutineInsns == subroutineInsns)
			{
			  throw new System.ArgumentException("Recursive invocation of " + subroutineInsns);
			}
		  }

		  this.parent = parent;
		  this.subroutineInsns = subroutineInsns;
		  this.returnLabel = parent == null ? null : new LabelNode();
		  this.clonedLabels = new Dictionary<LabelNode, LabelNode>();

		  // Create a clone of each label in the original code of the subroutine. Note that we collapse
		  // labels which point at the same instruction into one.
		  LabelNode clonedLabelNode = null;
		  for (int insnIndex = 0; insnIndex < outerInstance.instructions.size(); insnIndex++)
		  {
			AbstractInsnNode insnNode = outerInstance.instructions.get(insnIndex);
			if (insnNode.Type == AbstractInsnNode.LABEL)
			{
			  LabelNode labelNode = (LabelNode) insnNode;
			  // If we already have a label pointing at this spot, don't recreate it.
			  if (clonedLabelNode == null)
			  {
				clonedLabelNode = new LabelNode();
			  }
			  clonedLabels[labelNode] = clonedLabelNode;
			}
			else if (findOwner(insnIndex) == this)
			{
			  // We will emit this instruction, so clear the duplicateLabelNode flag since the next
			  // Label will refer to a distinct instruction.
			  clonedLabelNode = null;
			}
		  }
		}

		/// <summary>
		/// Returns the "owner" of a particular instruction relative to this instantiation: the owner
		/// refers to the Instantiation which will emit the version of this instruction that we will
		/// execute.
		/// 
		/// <para>Typically, the return value is either <code>this</code> or <code>null</code>. <code>this
		/// </code> indicates that this instantiation will generate the version of this instruction that
		/// we will execute, and <code>null</code> indicates that this instantiation never executes the
		/// given instruction.
		/// 
		/// </para>
		/// <para>Sometimes, however, an instruction can belong to multiple subroutines; this is called a
		/// shared instruction, and occurs when multiple subroutines branch to common points of control.
		/// In this case, the owner is the oldest instantiation which owns the instruction in question
		/// (parent instantiations are older than their children).
		/// 
		/// </para>
		/// </summary>
		/// <param name="insnIndex"> the index of an instruction in the original code. </param>
		/// <returns> the "owner" of a particular instruction relative to this instantiation. </returns>
		public virtual Instantiation findOwner(int insnIndex)
		{
		  if (!subroutineInsns.Get(insnIndex))
		  {
			return null;
		  }
		  if (!outerInstance.sharedSubroutineInsns.Get(insnIndex))
		  {
			return this;
		  }
		  Instantiation owner = this;
		  for (Instantiation instantiation = parent; instantiation != null; instantiation = instantiation.parent)
		  {
			if (instantiation.subroutineInsns.Get(insnIndex))
			{
			  owner = instantiation;
			}
		  }
		  return owner;
		}

		/// <summary>
		/// Returns the clone of the given original label that is appropriate for use in a jump
		/// instruction.
		/// </summary>
		/// <param name="labelNode"> a label of the original code. </param>
		/// <returns> a clone of the given label for use in a jump instruction in the inlined code. </returns>
		public virtual LabelNode getClonedLabelForJumpInsn(LabelNode labelNode)
		{
		  // findOwner should never return null, because owner is null only if an instruction cannot be
		  // reached from this subroutine.
		  return findOwner(outerInstance.instructions.indexOf(labelNode)).clonedLabels.GetValueOrNull(labelNode);
		}

		/// <summary>
		/// Returns the clone of the given original label that is appropriate for use by a try/catch
		/// block or a variable annotation.
		/// </summary>
		/// <param name="labelNode"> a label of the original code. </param>
		/// <returns> a clone of the given label for use by a try/catch block or a variable annotation in
		///     the inlined code. </returns>
		public virtual LabelNode getClonedLabel(LabelNode labelNode)
		{
		  return clonedLabels.GetValueOrNull(labelNode);
		}

		// AbstractMap implementation

		public override ISet<KeyValuePair<LabelNode, LabelNode>> entrySet()
		{
		  throw new System.NotSupportedException();
		}

		public override LabelNode get(object key)
		{
		  return getClonedLabelForJumpInsn((LabelNode) key);
		}

		public override bool Equals(object other)
		{
		  throw new System.NotSupportedException();
		}

		public override int GetHashCode()
		{
		  throw new System.NotSupportedException();
		}
	  }
	}

}