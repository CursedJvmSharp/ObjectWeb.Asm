

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
	/// Exception thrown when the constant pool of a class produced by a <seealso cref="ClassWriter"/> is too
	/// large.
	/// 
	/// @author Jason Zaugg
	/// </summary>
	public sealed class ClassTooLargeException : System.Exception
	{
	  private const long SerialVersionUid = 160715609518896765L;

	  private readonly string _className;
	  private readonly int _constantPoolCount;

	  /// <summary>
	  /// Constructs a new <seealso cref="ClassTooLargeException"/>.
	  /// </summary>
	  /// <param name="className"> the internal name of the class. </param>
	  /// <param name="constantPoolCount"> the number of constant pool items of the class. </param>
	  public ClassTooLargeException(string className, int constantPoolCount) : base("Class too large: " + className)
	  {
		this._className = className;
		this._constantPoolCount = constantPoolCount;
	  }

	  /// <summary>
	  /// Returns the internal name of the class.
	  /// </summary>
	  /// <returns> the internal name of the class. </returns>
	  public string ClassName => _className;

      /// <summary>
	  /// Returns the number of constant pool items of the class.
	  /// </summary>
	  /// <returns> the number of constant pool items of the class. </returns>
	  public int ConstantPoolCount => _constantPoolCount;
    }

}