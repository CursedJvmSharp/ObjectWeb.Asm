using System;
using System.Collections;
using org.objectweb.asm;
using CursedJvmSharp.Asm.Java.IO;
using System.Collections.Generic;
using System.Linq;
using MethodVisitor = org.objectweb.asm.MethodVisitor;

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
namespace org.objectweb.asm.tree
{

	/// <summary>
	/// A doubly linked list of <seealso cref="AbstractInsnNode"/> objects. <i>This implementation is not thread
	/// safe</i>.
	/// </summary>
	public class InsnList : IEnumerable<AbstractInsnNode>
	{

	  /// <summary>
	  /// The number of instructions in this list. </summary>
	  private int size_Conflict;

	  /// <summary>
	  /// The first instruction in this list. May be {@literal null}. </summary>
	  private AbstractInsnNode firstInsn;

	  /// <summary>
	  /// The last instruction in this list. May be {@literal null}. </summary>
	  private AbstractInsnNode lastInsn;

	  /// <summary>
	  /// A cache of the instructions of this list. This cache is used to improve the performance of the
	  /// <seealso cref="get"/> method.
	  /// </summary>
	  internal AbstractInsnNode[] cache;

	  /// <summary>
	  /// Returns the number of instructions in this list.
	  /// </summary>
	  /// <returns> the number of instructions in this list. </returns>
	  public int Size => size_Conflict;

      /// <summary>
	  /// Returns the first instruction in this list.
	  /// </summary>
	  /// <returns> the first instruction in this list, or {@literal null} if the list is empty. </returns>
	  public virtual AbstractInsnNode First
	  {
		  get
		  {
			return firstInsn;
		  }
	  }

	  /// <summary>
	  /// Returns the last instruction in this list.
	  /// </summary>
	  /// <returns> the last instruction in this list, or {@literal null} if the list is empty. </returns>
	  public virtual AbstractInsnNode Last
	  {
		  get
		  {
			return lastInsn;
		  }
	  }

	  /// <summary>
	  /// Returns the instruction whose index is given. This method builds a cache of the instructions in
	  /// this list to avoid scanning the whole list each time it is called. Once the cache is built,
	  /// this method runs in constant time. This cache is invalidated by all the methods that modify the
	  /// list.
	  /// </summary>
	  /// <param name="index"> the index of the instruction that must be returned. </param>
	  /// <returns> the instruction whose index is given. </returns>
	  /// <exception cref="IndexOutOfBoundsException"> if (index &lt; 0 || index &gt;= size()). </exception>
	  public virtual AbstractInsnNode get(int index)
	  {
		if (index < 0 || index >= size_Conflict)
		{
		  throw new System.IndexOutOfRangeException();
		}
		if (cache == null)
		{
		  cache = toArray();
		}
		return cache[index];
	  }

	  /// <summary>
	  /// Returns {@literal true} if the given instruction belongs to this list. This method always scans
	  /// the instructions of this list until it finds the given instruction or reaches the end of the
	  /// list.
	  /// </summary>
	  /// <param name="insnNode"> an instruction. </param>
	  /// <returns> {@literal true} if the given instruction belongs to this list. </returns>
	  public virtual bool contains(AbstractInsnNode insnNode)
	  {
		AbstractInsnNode currentInsn = firstInsn;
		while (currentInsn != null && currentInsn != insnNode)
		{
		  currentInsn = currentInsn.nextInsn;
		}
		return currentInsn != null;
	  }

	  /// <summary>
	  /// Returns the index of the given instruction in this list. This method builds a cache of the
	  /// instruction indexes to avoid scanning the whole list each time it is called. Once the cache is
	  /// built, this method run in constant time. The cache is invalidated by all the methods that
	  /// modify the list.
	  /// </summary>
	  /// <param name="insnNode"> an instruction <i>of this list</i>. </param>
	  /// <returns> the index of the given instruction in this list. <i>The result of this method is
	  ///     undefined if the given instruction does not belong to this list</i>. Use <seealso cref="contains "/>
	  ///     to test if an instruction belongs to an instruction list or not. </returns>
	  public virtual int indexOf(AbstractInsnNode insnNode)
	  {
		if (cache == null)
		{
		  cache = toArray();
		}
		return insnNode.index;
	  }

	  /// <summary>
	  /// Makes the given visitor visit all the instructions in this list.
	  /// </summary>
	  /// <param name="methodVisitor"> the method visitor that must visit the instructions. </param>
	  public virtual void accept(MethodVisitor methodVisitor)
	  {
		AbstractInsnNode currentInsn = firstInsn;
		while (currentInsn != null)
		{
		  currentInsn.accept(methodVisitor);
		  currentInsn = currentInsn.nextInsn;
		}
	  }

	  /// <summary>
	  /// Returns an iterator over the instructions in this list.
	  /// </summary>
	  /// <returns> an iterator over the instructions in this list. </returns>
	  public virtual IEnumerator<AbstractInsnNode> GetEnumerator()
	  {
		return GetEnumerator(0);
	  }

	  /// <summary>
	  /// Returns an iterator over the instructions in this list.
	  /// </summary>
	  /// <param name="index"> index of instruction for the iterator to start at. </param>
	  /// <returns> an iterator over the instructions in this list. </returns>
	  public virtual IEnumerator<AbstractInsnNode> GetEnumerator(int index)
	  {
		return new InsnListIterator(this, index);
	  }

	  /// <summary>
	  /// Returns an array containing all the instructions in this list.
	  /// </summary>
	  /// <returns> an array containing all the instructions in this list. </returns>
	  public virtual AbstractInsnNode[] toArray()
	  {
		int currentInsnIndex = 0;
		AbstractInsnNode currentInsn = firstInsn;
		AbstractInsnNode[] insnNodeArray = new AbstractInsnNode[size_Conflict];
		while (currentInsn != null)
		{
		  insnNodeArray[currentInsnIndex] = currentInsn;
		  currentInsn.index = currentInsnIndex++;
		  currentInsn = currentInsn.nextInsn;
		}
		return insnNodeArray;
	  }

	  /// <summary>
	  /// Replaces an instruction of this list with another instruction.
	  /// </summary>
	  /// <param name="oldInsnNode"> an instruction <i>of this list</i>. </param>
	  /// <param name="newInsnNode"> another instruction, <i>which must not belong to any <seealso cref="InsnList"/></i>. </param>
	  public virtual void set(AbstractInsnNode oldInsnNode, AbstractInsnNode newInsnNode)
	  {
		AbstractInsnNode nextInsn = oldInsnNode.nextInsn;
		newInsnNode.nextInsn = nextInsn;
		if (nextInsn != null)
		{
		  nextInsn.previousInsn = newInsnNode;
		}
		else
		{
		  lastInsn = newInsnNode;
		}
		AbstractInsnNode previousInsn = oldInsnNode.previousInsn;
		newInsnNode.previousInsn = previousInsn;
		if (previousInsn != null)
		{
		  previousInsn.nextInsn = newInsnNode;
		}
		else
		{
		  firstInsn = newInsnNode;
		}
		if (cache != null)
		{
		  int index = oldInsnNode.index;
		  cache[index] = newInsnNode;
		  newInsnNode.index = index;
		}
		else
		{
		  newInsnNode.index = 0; // newInsnNode now belongs to an InsnList.
		}
		oldInsnNode.index = -1; // oldInsnNode no longer belongs to an InsnList.
		oldInsnNode.previousInsn = null;
		oldInsnNode.nextInsn = null;
	  }

	  /// <summary>
	  /// Adds the given instruction to the end of this list.
	  /// </summary>
	  /// <param name="insnNode"> an instruction, <i>which must not belong to any <seealso cref="InsnList"/></i>. </param>
	  public virtual void add(AbstractInsnNode insnNode)
	  {
		++size_Conflict;
		if (lastInsn == null)
		{
		  firstInsn = insnNode;
		  lastInsn = insnNode;
		}
		else
		{
		  lastInsn.nextInsn = insnNode;
		  insnNode.previousInsn = lastInsn;
		}
		lastInsn = insnNode;
		cache = null;
		insnNode.index = 0; // insnNode now belongs to an InsnList.
	  }

	  /// <summary>
	  /// Adds the given instructions to the end of this list.
	  /// </summary>
	  /// <param name="insnList"> an instruction list, which is cleared during the process. This list must be
	  ///     different from 'this'. </param>
	  public virtual void add(InsnList insnList)
	  {
		if (insnList.Size == 0)
		{
		  return;
		}
		size_Conflict += insnList.Size;
		if (lastInsn == null)
		{
		  firstInsn = insnList.firstInsn;
		  lastInsn = insnList.lastInsn;
		}
		else
		{
		  AbstractInsnNode firstInsnListElement = insnList.firstInsn;
		  lastInsn.nextInsn = firstInsnListElement;
		  firstInsnListElement.previousInsn = lastInsn;
		  lastInsn = insnList.lastInsn;
		}
		cache = null;
		insnList.removeAll(false);
	  }

	  /// <summary>
	  /// Inserts the given instruction at the beginning of this list.
	  /// </summary>
	  /// <param name="insnNode"> an instruction, <i>which must not belong to any <seealso cref="InsnList"/></i>. </param>
	  public virtual void insert(AbstractInsnNode insnNode)
	  {
		++size_Conflict;
		if (firstInsn == null)
		{
		  firstInsn = insnNode;
		  lastInsn = insnNode;
		}
		else
		{
		  firstInsn.previousInsn = insnNode;
		  insnNode.nextInsn = firstInsn;
		}
		firstInsn = insnNode;
		cache = null;
		insnNode.index = 0; // insnNode now belongs to an InsnList.
	  }

	  /// <summary>
	  /// Inserts the given instructions at the beginning of this list.
	  /// </summary>
	  /// <param name="insnList"> an instruction list, which is cleared during the process. This list must be
	  ///     different from 'this'. </param>
	  public virtual void insert(InsnList insnList)
	  {
		if (insnList.Size == 0)
		{
		  return;
		}
		size_Conflict += insnList.Size;
		if (firstInsn == null)
		{
		  firstInsn = insnList.firstInsn;
		  lastInsn = insnList.lastInsn;
		}
		else
		{
		  AbstractInsnNode lastInsnListElement = insnList.lastInsn;
		  firstInsn.previousInsn = lastInsnListElement;
		  lastInsnListElement.nextInsn = firstInsn;
		  firstInsn = insnList.firstInsn;
		}
		cache = null;
		insnList.removeAll(false);
	  }

	  /// <summary>
	  /// Inserts the given instruction after the specified instruction.
	  /// </summary>
	  /// <param name="previousInsn"> an instruction <i>of this list</i> after which insnNode must be inserted. </param>
	  /// <param name="insnNode"> the instruction to be inserted, <i>which must not belong to any {@link
	  ///     InsnList}</i>. </param>
	  public virtual void insert(AbstractInsnNode previousInsn, AbstractInsnNode insnNode)
	  {
		++size_Conflict;
		AbstractInsnNode nextInsn = previousInsn.nextInsn;
		if (nextInsn == null)
		{
		  lastInsn = insnNode;
		}
		else
		{
		  nextInsn.previousInsn = insnNode;
		}
		previousInsn.nextInsn = insnNode;
		insnNode.nextInsn = nextInsn;
		insnNode.previousInsn = previousInsn;
		cache = null;
		insnNode.index = 0; // insnNode now belongs to an InsnList.
	  }

	  /// <summary>
	  /// Inserts the given instructions after the specified instruction.
	  /// </summary>
	  /// <param name="previousInsn"> an instruction <i>of this list</i> after which the instructions must be
	  ///     inserted. </param>
	  /// <param name="insnList"> the instruction list to be inserted, which is cleared during the process. This
	  ///     list must be different from 'this'. </param>
	  public virtual void insert(AbstractInsnNode previousInsn, InsnList insnList)
	  {
		if (insnList.Size == 0)
		{
		  return;
		}
		size_Conflict += insnList.Size;
		AbstractInsnNode firstInsnListElement = insnList.firstInsn;
		AbstractInsnNode lastInsnListElement = insnList.lastInsn;
		AbstractInsnNode nextInsn = previousInsn.nextInsn;
		if (nextInsn == null)
		{
		  lastInsn = lastInsnListElement;
		}
		else
		{
		  nextInsn.previousInsn = lastInsnListElement;
		}
		previousInsn.nextInsn = firstInsnListElement;
		lastInsnListElement.nextInsn = nextInsn;
		firstInsnListElement.previousInsn = previousInsn;
		cache = null;
		insnList.removeAll(false);
	  }

	  /// <summary>
	  /// Inserts the given instruction before the specified instruction.
	  /// </summary>
	  /// <param name="nextInsn"> an instruction <i>of this list</i> before which insnNode must be inserted. </param>
	  /// <param name="insnNode"> the instruction to be inserted, <i>which must not belong to any {@link
	  ///     InsnList}</i>. </param>
	  public virtual void insertBefore(AbstractInsnNode nextInsn, AbstractInsnNode insnNode)
	  {
		++size_Conflict;
		AbstractInsnNode previousInsn = nextInsn.previousInsn;
		if (previousInsn == null)
		{
		  firstInsn = insnNode;
		}
		else
		{
		  previousInsn.nextInsn = insnNode;
		}
		nextInsn.previousInsn = insnNode;
		insnNode.nextInsn = nextInsn;
		insnNode.previousInsn = previousInsn;
		cache = null;
		insnNode.index = 0; // insnNode now belongs to an InsnList.
	  }

	  /// <summary>
	  /// Inserts the given instructions before the specified instruction.
	  /// </summary>
	  /// <param name="nextInsn"> an instruction <i>of this list</i> before which the instructions must be
	  ///     inserted. </param>
	  /// <param name="insnList"> the instruction list to be inserted, which is cleared during the process. This
	  ///     list must be different from 'this'. </param>
	  public virtual void insertBefore(AbstractInsnNode nextInsn, InsnList insnList)
	  {
		if (insnList.Size == 0)
		{
		  return;
		}
		size_Conflict += insnList.Size;
		AbstractInsnNode firstInsnListElement = insnList.firstInsn;
		AbstractInsnNode lastInsnListElement = insnList.lastInsn;
		AbstractInsnNode previousInsn = nextInsn.previousInsn;
		if (previousInsn == null)
		{
		  firstInsn = firstInsnListElement;
		}
		else
		{
		  previousInsn.nextInsn = firstInsnListElement;
		}
		nextInsn.previousInsn = lastInsnListElement;
		lastInsnListElement.nextInsn = nextInsn;
		firstInsnListElement.previousInsn = previousInsn;
		cache = null;
		insnList.removeAll(false);
	  }

	  /// <summary>
	  /// Removes the given instruction from this list.
	  /// </summary>
	  /// <param name="insnNode"> the instruction <i>of this list</i> that must be removed. </param>
	  public virtual void remove(AbstractInsnNode insnNode)
	  {
		--size_Conflict;
		AbstractInsnNode nextInsn = insnNode.nextInsn;
		AbstractInsnNode previousInsn = insnNode.previousInsn;
		if (nextInsn == null)
		{
		  if (previousInsn == null)
		  {
			firstInsn = null;
			lastInsn = null;
		  }
		  else
		  {
			previousInsn.nextInsn = null;
			lastInsn = previousInsn;
		  }
		}
		else
		{
		  if (previousInsn == null)
		  {
			firstInsn = nextInsn;
			nextInsn.previousInsn = null;
		  }
		  else
		  {
			previousInsn.nextInsn = nextInsn;
			nextInsn.previousInsn = previousInsn;
		  }
		}
		cache = null;
		insnNode.index = -1; // insnNode no longer belongs to an InsnList.
		insnNode.previousInsn = null;
		insnNode.nextInsn = null;
	  }

	  /// <summary>
	  /// Removes all the instructions of this list.
	  /// </summary>
	  /// <param name="mark"> if the instructions must be marked as no longer belonging to any <seealso cref="InsnList"/>. </param>
	  public virtual void removeAll(bool mark)
	  {
		if (mark)
		{
		  AbstractInsnNode currentInsn = firstInsn;
		  while (currentInsn != null)
		  {
			AbstractInsnNode next = currentInsn.nextInsn;
			currentInsn.index = -1; // currentInsn no longer belongs to an InsnList.
			currentInsn.previousInsn = null;
			currentInsn.nextInsn = null;
			currentInsn = next;
		  }
		}
		size_Conflict = 0;
		firstInsn = null;
		lastInsn = null;
		cache = null;
	  }

	  /// <summary>
	  /// Removes all the instructions of this list. </summary>
	  public virtual void clear()
	  {
		removeAll(false);
	  }

	  /// <summary>
	  /// Resets all the labels in the instruction list. This method should be called before reusing an
	  /// instruction list between several <code>ClassWriter</code>s.
	  /// </summary>
	  public virtual void resetLabels()
	  {
		AbstractInsnNode currentInsn = firstInsn;
		while (currentInsn != null)
		{
		  if (currentInsn is LabelNode)
		  {
			((LabelNode) currentInsn).resetLabel();
		  }
		  currentInsn = currentInsn.nextInsn;
		}
	  }

	  // Note: this class is not generified because it would create bridges.
	  private sealed class InsnListIterator : IEnumerator<AbstractInsnNode>
	  {
		  private readonly InsnList outerInstance;


		internal AbstractInsnNode nextInsn;

		internal AbstractInsnNode previousInsn;

		internal AbstractInsnNode remove_Conflict;
        private AbstractInsnNode _current;

        public InsnListIterator(InsnList outerInstance, int index)
		{
			this.outerInstance = outerInstance;
            var outerInstanceCount = outerInstance.Count();
            if (index < 0 || index > outerInstanceCount)
		  {
			throw new System.IndexOutOfRangeException();
		  }
		  else if (index == outerInstanceCount)
		  {
			nextInsn = null;
			previousInsn = outerInstance.Last;
		  }
		  else
		  {
			AbstractInsnNode currentInsn = outerInstance.First;
			for (int i = 0; i < index; i++)
			{
			  currentInsn = currentInsn.nextInsn;
			}

			nextInsn = currentInsn;
			previousInsn = currentInsn.previousInsn;
		  }
		}

		public bool hasNext()
		{
		  return nextInsn != null;
		}

		public AbstractInsnNode next()
		{
		  if (nextInsn == null)
		  {
			throw new System.InvalidOperationException();
		  }
		  AbstractInsnNode result = nextInsn;
		  previousInsn = result;
		  nextInsn = result.nextInsn;
		  remove_Conflict = result;
		  return result;
		}

		public void remove()
		{
		  if (remove_Conflict != null)
		  {
			if (remove_Conflict == nextInsn)
			{
			  nextInsn = nextInsn.nextInsn;
			}
			else
			{
			  previousInsn = previousInsn.previousInsn;
			}
			outerInstance.remove(remove_Conflict);
			remove_Conflict = null;
		  }
		  else
		  {
			throw new System.InvalidOperationException();
		  }
		}

		public bool hasPrevious()
		{
		  return previousInsn != null;
		}

		public object previous()
		{
		  if (previousInsn == null)
		  {
			throw new InvalidOperationException("NoSuchElement");
		  }
		  AbstractInsnNode result = previousInsn;
		  nextInsn = result;
		  previousInsn = result.previousInsn;
		  remove_Conflict = result;
		  return result;
		}

		public int nextIndex()
		{
		  if (nextInsn == null)
		  {
			return outerInstance.Count();
		  }
		  if (outerInstance.cache == null)
		  {
			outerInstance.cache = outerInstance.toArray();
		  }
		  return nextInsn.index;
		}

		public int previousIndex()
		{
		  if (previousInsn == null)
		  {
			return -1;
		  }
		  if (outerInstance.cache == null)
		  {
			outerInstance.cache = outerInstance.toArray();
		  }
		  return previousInsn.index;
		}

		public void add(object o)
		{
		  if (nextInsn != null)
		  {
			outerInstance.insertBefore(nextInsn, (AbstractInsnNode) o);
		  }
		  else if (previousInsn != null)
		  {
			outerInstance.insert(previousInsn, (AbstractInsnNode) o);
		  }
		  else
		  {
			outerInstance.add((AbstractInsnNode) o);
		  }
		  previousInsn = (AbstractInsnNode) o;
		  remove_Conflict = null;
		}

		public void set(object o)
		{
		  if (remove_Conflict != null)
		  {
			outerInstance.set(remove_Conflict, (AbstractInsnNode) o);
			if (remove_Conflict == previousInsn)
			{
			  previousInsn = (AbstractInsnNode) o;
			}
			else
			{
			  nextInsn = (AbstractInsnNode) o;
			}
		  }
		  else
		  {
			throw new System.InvalidOperationException();
		  }
		}

		
        public bool MoveNext()
        {
            Current = next();
            return true;
        }

        public void Reset()
        {
            throw new System.NotImplementedException();
        }

        object IEnumerator.Current => Current;

        public AbstractInsnNode Current { get; private set;  }

        public void Dispose()
        {
        }
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
          return GetEnumerator();
      }
    }

}