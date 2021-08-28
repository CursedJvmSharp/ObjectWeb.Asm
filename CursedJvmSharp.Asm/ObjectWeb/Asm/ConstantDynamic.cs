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
namespace ObjectWeb.Asm
{

	/// <summary>
	/// A constant whose value is computed at runtime, with a bootstrap method.
	/// 
	/// @author Remi Forax
	/// </summary>
	public sealed class ConstantDynamic
	{

	  /// <summary>
	  /// The constant name (can be arbitrary). </summary>
	  private readonly string _name;

	  /// <summary>
	  /// The constant type (must be a field descriptor). </summary>
	  private readonly string _descriptor;

	  /// <summary>
	  /// The bootstrap method to use to compute the constant value at runtime. </summary>
	  private readonly Handle _bootstrapMethod;

	  /// <summary>
	  /// The arguments to pass to the bootstrap method, in order to compute the constant value at
	  /// runtime.
	  /// </summary>
	  private readonly object[] _bootstrapMethodArguments;

	  /// <summary>
	  /// Constructs a new <seealso cref="ConstantDynamic"/>.
	  /// </summary>
	  /// <param name="name"> the constant name (can be arbitrary). </param>
	  /// <param name="descriptor"> the constant type (must be a field descriptor). </param>
	  /// <param name="bootstrapMethod"> the bootstrap method to use to compute the constant value at runtime. </param>
	  /// <param name="bootstrapMethodArguments"> the arguments to pass to the bootstrap method, in order to
	  ///     compute the constant value at runtime. </param>
	  public ConstantDynamic(string name, string descriptor, Handle bootstrapMethod, params object[] bootstrapMethodArguments)
	  {
		this._name = name;
		this._descriptor = descriptor;
		this._bootstrapMethod = bootstrapMethod;
		this._bootstrapMethodArguments = bootstrapMethodArguments;
	  }

	  /// <summary>
	  /// Returns the name of this constant.
	  /// </summary>
	  /// <returns> the name of this constant. </returns>
	  public string Name => _name;

      /// <summary>
	  /// Returns the type of this constant.
	  /// </summary>
	  /// <returns> the type of this constant, as a field descriptor. </returns>
	  public string Descriptor => _descriptor;

      /// <summary>
	  /// Returns the bootstrap method used to compute the value of this constant.
	  /// </summary>
	  /// <returns> the bootstrap method used to compute the value of this constant. </returns>
	  public Handle BootstrapMethod => _bootstrapMethod;

      /// <summary>
	  /// Returns the number of arguments passed to the bootstrap method, in order to compute the value
	  /// of this constant.
	  /// </summary>
	  /// <returns> the number of arguments passed to the bootstrap method, in order to compute the value
	  ///     of this constant. </returns>
	  public int BootstrapMethodArgumentCount => _bootstrapMethodArguments.Length;

      /// <summary>
	  /// Returns an argument passed to the bootstrap method, in order to compute the value of this
	  /// constant.
	  /// </summary>
	  /// <param name="index"> an argument index, between 0 and <seealso cref="getBootstrapMethodArgumentCount()"/>
	  ///     (exclusive). </param>
	  /// <returns> the argument passed to the bootstrap method, with the given index. </returns>
	  public object GetBootstrapMethodArgument(int index)
	  {
		return _bootstrapMethodArguments[index];
	  }

	  /// <summary>
	  /// Returns the arguments to pass to the bootstrap method, in order to compute the value of this
	  /// constant. WARNING: this array must not be modified, and must not be returned to the user.
	  /// </summary>
	  /// <returns> the arguments to pass to the bootstrap method, in order to compute the value of this
	  ///     constant. </returns>
	  public object[] BootstrapMethodArgumentsUnsafe => _bootstrapMethodArguments;

      /// <summary>
	  /// Returns the size of this constant.
	  /// </summary>
	  /// <returns> the size of this constant, i.e., 2 for {@code long} and {@code double}, 1 otherwise. </returns>
	  public int Size
	  {
		  get
		  {
			var firstCharOfDescriptor = _descriptor[0];
			return (firstCharOfDescriptor == 'J' || firstCharOfDescriptor == 'D') ? 2 : 1;
		  }
	  }

      public static int RotateLeft(int i, int distance)
      {
          return (i << distance) | ((int)((uint)i >> -distance));
      }
      public static int RotateRight(int i, int distance)
      {
          return ((int)((uint)i >> distance)) | (i << -distance);
      }


	  public override bool Equals(object @object)
	  {
		if (@object == this)
		{
		  return true;
		}
		if (!(@object is ConstantDynamic))
		{
		  return false;
		}
		var constantDynamic = (ConstantDynamic) @object;
		return _name.Equals(constantDynamic._name) && _descriptor.Equals(constantDynamic._descriptor) && _bootstrapMethod.Equals(constantDynamic._bootstrapMethod) && _bootstrapMethodArguments.SequenceEqual(constantDynamic._bootstrapMethodArguments);
	  }

	  public override int GetHashCode()
	  {
		return _name.GetHashCode() ^ RotateLeft(_descriptor.GetHashCode(), 8) ^ RotateLeft(_bootstrapMethod.GetHashCode(), 16) ^ RotateLeft((_bootstrapMethodArguments).GetHashCode(), 24);
	  }

	  public override string ToString()
	  {
		return _name + " : " + _descriptor + ' ' + _bootstrapMethod + ' ' + $"[{string.Join(", ", _bootstrapMethodArguments)}]";
	  }
	}

}