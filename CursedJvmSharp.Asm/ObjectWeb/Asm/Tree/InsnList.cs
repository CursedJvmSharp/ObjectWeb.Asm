using System;
using System.Collections;
using System.Collections.Generic;

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
	/// A doubly linked list of <seealso cref="AbstractInsnNode"/> objects. <i>This implementation is not thread
	/// safe</i>.
	/// </summary>
	public class InsnList : IEnumerable<AbstractInsnNode>
	{

	  /// <summary>
	  /// The number of instructions in this list. </summary>
	  private int _sizeConflict;

	  /// <summary>
	  /// The first instruction in this list. May be {@literal null}. </summary>
	  private AbstractInsnNode _firstInsn;

	  /// <summary>
	  /// The last instruction in this list. May be {@literal null}. </summary>
	  private AbstractInsnNode _lastInsn;

	  /// <summary>
	  /// A cache of the instructions of this list. This cache is used to improve the performance of the
	  /// <seealso cref="Get"/> method.
	  /// </summary>
	  internal AbstractInsnNode[] cache;

	  /// <summary>
	  /// Returns the number of instructions in this list.
	  /// </summary>
	  /// <returns> the number of instructions in this list. </returns>
	  public int Size => _sizeConflict;

      /// <summary>
	  /// Returns the first instruction in this list.
	  /// </summary>
	  /// <returns> the first instruction in this list, or {@literal null} if the list is empty. </returns>
	  public virtual AbstractInsnNode First => _firstInsn;

      /// <summary>
	  /// Returns the last instruction in this list.
	  /// </summary>
	  /// <returns> the last instruction in this list, or {@literal null} if the list is empty. </returns>
	  public virtual AbstractInsnNode Last => _lastInsn;

      /// <summary>
	  /// Returns the instruction whose index is given. This method builds a cache of the instructions in
	  /// this list to avoid scanning the whole list each time it is called. Once the cache is built,
	  /// this method runs in constant time. This cache is invalidated by all the methods that modify the
	  /// list.
	  /// </summary>
	  /// <param name="index"> the index of the instruction that must be returned. </param>
	  /// <returns> the instruction whose index is given. </returns>
	  /// <exception cref="IndexOutOfBoundsException"> if (index &lt; 0 || index &gt;= size()). </exception>
	  public virtual AbstractInsnNode Get(int index)
	  {
		if (index < 0 || index >= _sizeConflict)
		{
		  throw new System.IndexOutOfRangeException();
		}
		if (cache == null)
		{
		  cache = ToArray();
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
	  public virtual bool Contains(AbstractInsnNode insnNode)
	  {
		AbstractInsnNode currentInsn = _firstInsn;
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
	  ///     undefined if the given instruction does not belong to this list</i>. Use <seealso cref="Contains "/>
	  ///     to test if an instruction belongs to an instruction list or not. </returns>
	  public virtual int IndexOf(AbstractInsnNode insnNode)
	  {
		if (cache == null)
		{
		  cache = ToArray();
		}
		return insnNode.index;
	  }

	  /// <summary>
	  /// Makes the given visitor visit all the instructions in this list.
	  /// </summary>
	  /// <param name="methodVisitor"> the method visitor that must visit the instructions. </param>
	  public virtual void Accept(MethodVisitor methodVisitor)
	  {
		AbstractInsnNode currentInsn = _firstInsn;
		while (currentInsn != null)
		{
		  currentInsn.Accept(methodVisitor);
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
	  public virtual AbstractInsnNode[] ToArray()
	  {
		int currentInsnIndex = 0;
		AbstractInsnNode currentInsn = _firstInsn;
		AbstractInsnNode[] insnNodeArray = new AbstractInsnNode[_sizeConflict];
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
	  public virtual void Set(AbstractInsnNode oldInsnNode, AbstractInsnNode newInsnNode)
	  {
		AbstractInsnNode nextInsn = oldInsnNode.nextInsn;
		newInsnNode.nextInsn = nextInsn;
		if (nextInsn != null)
		{
		  nextInsn.previousInsn = newInsnNode;
		}
		else
		{
		  _lastInsn = newInsnNode;
		}
		AbstractInsnNode previousInsn = oldInsnNode.previousInsn;
		newInsnNode.previousInsn = previousInsn;
		if (previousInsn != null)
		{
		  previousInsn.nextInsn = newInsnNode;
		}
		else
		{
		  _firstInsn = newInsnNode;
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
	  public virtual void Add(AbstractInsnNode insnNode)
	  {
		++_sizeConflict;
		if (_lastInsn == null)
		{
		  _firstInsn = insnNode;
		  _lastInsn = insnNode;
		}
		else
		{
		  _lastInsn.nextInsn = insnNode;
		  insnNode.previousInsn = _lastInsn;
		}
		_lastInsn = insnNode;
		cache = null;
		insnNode.index = 0; // insnNode now belongs to an InsnList.
	  }

	  /// <summary>
	  /// Adds the given instructions to the end of this list.
	  /// </summary>
	  /// <param name="insnList"> an instruction list, which is cleared during the process. This list must be
	  ///     different from 'this'. </param>
	  public virtual void Add(InsnList insnList)
	  {
		if (insnList.Size == 0)
		{
		  return;
		}
		_sizeConflict += insnList.Size;
		if (_lastInsn == null)
		{
		  _firstInsn = insnList._firstInsn;
		  _lastInsn = insnList._lastInsn;
		}
		else
		{
		  AbstractInsnNode firstInsnListElement = insnList._firstInsn;
		  _lastInsn.nextInsn = firstInsnListElement;
		  firstInsnListElement.previousInsn = _lastInsn;
		  _lastInsn = insnList._lastInsn;
		}
		cache = null;
		insnList.RemoveAll(false);
	  }

	  /// <summary>
	  /// Inserts the given instruction at the beginning of this list.
	  /// </summary>
	  /// <param name="insnNode"> an instruction, <i>which must not belong to any <seealso cref="InsnList"/></i>. </param>
	  public virtual void Insert(AbstractInsnNode insnNode)
	  {
		++_sizeConflict;
		if (_firstInsn == null)
		{
		  _firstInsn = insnNode;
		  _lastInsn = insnNode;
		}
		else
		{
		  _firstInsn.previousInsn = insnNode;
		  insnNode.nextInsn = _firstInsn;
		}
		_firstInsn = insnNode;
		cache = null;
		insnNode.index = 0; // insnNode now belongs to an InsnList.
	  }

	  /// <summary>
	  /// Inserts the given instructions at the beginning of this list.
	  /// </summary>
	  /// <param name="insnList"> an instruction list, which is cleared during the process. This list must be
	  ///     different from 'this'. </param>
	  public virtual void Insert(InsnList insnList)
	  {
		if (insnList.Size == 0)
		{
		  return;
		}
		_sizeConflict += insnList.Size;
		if (_firstInsn == null)
		{
		  _firstInsn = insnList._firstInsn;
		  _lastInsn = insnList._lastInsn;
		}
		else
		{
		  AbstractInsnNode lastInsnListElement = insnList._lastInsn;
		  _firstInsn.previousInsn = lastInsnListElement;
		  lastInsnListElement.nextInsn = _firstInsn;
		  _firstInsn = insnList._firstInsn;
		}
		cache = null;
		insnList.RemoveAll(false);
	  }

	  /// <summary>
	  /// Inserts the given instruction after the specified instruction.
	  /// </summary>
	  /// <param name="previousInsn"> an instruction <i>of this list</i> after which insnNode must be inserted. </param>
	  /// <param name="insnNode"> the instruction to be inserted, <i>which must not belong to any {@link
	  ///     InsnList}</i>. </param>
	  public virtual void Insert(AbstractInsnNode previousInsn, AbstractInsnNode insnNode)
	  {
		++_sizeConflict;
		AbstractInsnNode nextInsn = previousInsn.nextInsn;
		if (nextInsn == null)
		{
		  _lastInsn = insnNode;
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
	  public virtual void Insert(AbstractInsnNode previousInsn, InsnList insnList)
	  {
		if (insnList.Size == 0)
		{
		  return;
		}
		_sizeConflict += insnList.Size;
		AbstractInsnNode firstInsnListElement = insnList._firstInsn;
		AbstractInsnNode lastInsnListElement = insnList._lastInsn;
		AbstractInsnNode nextInsn = previousInsn.nextInsn;
		if (nextInsn == null)
		{
		  _lastInsn = lastInsnListElement;
		}
		else
		{
		  nextInsn.previousInsn = lastInsnListElement;
		}
		previousInsn.nextInsn = firstInsnListElement;
		lastInsnListElement.nextInsn = nextInsn;
		firstInsnListElement.previousInsn = previousInsn;
		cache = null;
		insnList.RemoveAll(false);
	  }

	  /// <summary>
	  /// Inserts the given instruction before the specified instruction.
	  /// </summary>
	  /// <param name="nextInsn"> an instruction <i>of this list</i> before which insnNode must be inserted. </param>
	  /// <param name="insnNode"> the instruction to be inserted, <i>which must not belong to any {@link
	  ///     InsnList}</i>. </param>
	  public virtual void InsertBefore(AbstractInsnNode nextInsn, AbstractInsnNode insnNode)
	  {
		++_sizeConflict;
		AbstractInsnNode previousInsn = nextInsn.previousInsn;
		if (previousInsn == null)
		{
		  _firstInsn = insnNode;
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
	  public virtual void InsertBefore(AbstractInsnNode nextInsn, InsnList insnList)
	  {
		if (insnList.Size == 0)
		{
		  return;
		}
		_sizeConflict += insnList.Size;
		AbstractInsnNode firstInsnListElement = insnList._firstInsn;
		AbstractInsnNode lastInsnListElement = insnList._lastInsn;
		AbstractInsnNode previousInsn = nextInsn.previousInsn;
		if (previousInsn == null)
		{
		  _firstInsn = firstInsnListElement;
		}
		else
		{
		  previousInsn.nextInsn = firstInsnListElement;
		}
		nextInsn.previousInsn = lastInsnListElement;
		lastInsnListElement.nextInsn = nextInsn;
		firstInsnListElement.previousInsn = previousInsn;
		cache = null;
		insnList.RemoveAll(false);
	  }

	  /// <summary>
	  /// Removes the given instruction from this list.
	  /// </summary>
	  /// <param name="insnNode"> the instruction <i>of this list</i> that must be removed. </param>
	  public virtual void Remove(AbstractInsnNode insnNode)
	  {
		--_sizeConflict;
		AbstractInsnNode nextInsn = insnNode.nextInsn;
		AbstractInsnNode previousInsn = insnNode.previousInsn;
		if (nextInsn == null)
		{
		  if (previousInsn == null)
		  {
			_firstInsn = null;
			_lastInsn = null;
		  }
		  else
		  {
			previousInsn.nextInsn = null;
			_lastInsn = previousInsn;
		  }
		}
		else
		{
		  if (previousInsn == null)
		  {
			_firstInsn = nextInsn;
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
	  public virtual void RemoveAll(bool mark)
	  {
		if (mark)
		{
		  AbstractInsnNode currentInsn = _firstInsn;
		  while (currentInsn != null)
		  {
			AbstractInsnNode next = currentInsn.nextInsn;
			currentInsn.index = -1; // currentInsn no longer belongs to an InsnList.
			currentInsn.previousInsn = null;
			currentInsn.nextInsn = null;
			currentInsn = next;
		  }
		}
		_sizeConflict = 0;
		_firstInsn = null;
		_lastInsn = null;
		cache = null;
	  }

	  /// <summary>
	  /// Removes all the instructions of this list. </summary>
	  public virtual void Clear()
	  {
		RemoveAll(false);
	  }

	  /// <summary>
	  /// Resets all the labels in the instruction list. This method should be called before reusing an
	  /// instruction list between several <code>ClassWriter</code>s.
	  /// </summary>
	  public virtual void ResetLabels()
	  {
		AbstractInsnNode currentInsn = _firstInsn;
		while (currentInsn != null)
		{
		  if (currentInsn is LabelNode)
		  {
			((LabelNode) currentInsn).ResetLabel();
		  }
		  currentInsn = currentInsn.nextInsn;
		}
	  }

	  // Note: this class is not generified because it would create bridges.
	  private sealed class InsnListIterator : IEnumerator<AbstractInsnNode>
	  {
		  private readonly InsnList _outerInstance;


		internal AbstractInsnNode nextInsn;

		internal AbstractInsnNode previousInsn;

		internal AbstractInsnNode removeConflict;
        private AbstractInsnNode _current;

        public InsnListIterator(InsnList outerInstance, int index)
		{
			this._outerInstance = outerInstance;
            var outerInstanceCount = outerInstance._sizeConflict;
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

		public bool HasNext()
		{
		  return nextInsn != null;
		}

		public AbstractInsnNode Next()
		{
		  if (nextInsn == null)
		  {
			throw new System.InvalidOperationException();
		  }
		  AbstractInsnNode result = nextInsn;
		  previousInsn = result;
		  nextInsn = result.nextInsn;
		  removeConflict = result;
		  return result;
		}

		public void Remove()
		{
		  if (removeConflict != null)
		  {
			if (removeConflict == nextInsn)
			{
			  nextInsn = nextInsn.nextInsn;
			}
			else
			{
			  previousInsn = previousInsn.previousInsn;
			}
			_outerInstance.Remove(removeConflict);
			removeConflict = null;
		  }
		  else
		  {
			throw new System.InvalidOperationException();
		  }
		}

		public bool HasPrevious()
		{
		  return previousInsn != null;
		}

		public object Previous()
		{
		  if (previousInsn == null)
		  {
			throw new InvalidOperationException("NoSuchElement");
		  }
		  AbstractInsnNode result = previousInsn;
		  nextInsn = result;
		  previousInsn = result.previousInsn;
		  removeConflict = result;
		  return result;
		}

		public int NextIndex()
		{
		  if (nextInsn == null)
		  {
			return _outerInstance.Size;
		  }
		  if (_outerInstance.cache == null)
		  {
			_outerInstance.cache = _outerInstance.ToArray();
		  }
		  return nextInsn.index;
		}

		public int PreviousIndex()
		{
		  if (previousInsn == null)
		  {
			return -1;
		  }
		  if (_outerInstance.cache == null)
		  {
			_outerInstance.cache = _outerInstance.ToArray();
		  }
		  return previousInsn.index;
		}

		public void Add(object o)
		{
		  if (nextInsn != null)
		  {
			_outerInstance.InsertBefore(nextInsn, (AbstractInsnNode) o);
		  }
		  else if (previousInsn != null)
		  {
			_outerInstance.Insert(previousInsn, (AbstractInsnNode) o);
		  }
		  else
		  {
			_outerInstance.Add((AbstractInsnNode) o);
		  }
		  previousInsn = (AbstractInsnNode) o;
		  removeConflict = null;
		}

		public void Set(object o)
		{
		  if (removeConflict != null)
		  {
			_outerInstance.Set(removeConflict, (AbstractInsnNode) o);
			if (removeConflict == previousInsn)
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
            if (!HasNext()) return false;
            Current = Next();
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