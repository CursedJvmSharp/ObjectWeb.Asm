

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
	/// A ModuleResolution attribute. This attribute is specific to the OpenJDK and may change in the
	/// future.
	/// 
	/// @author Remi Forax
	/// </summary>
	public sealed class ModuleResolutionAttribute : Attribute
	{
	  /// <summary>
	  /// The resolution state of a module meaning that the module is not available from the class-path
	  /// by default.
	  /// </summary>
	  public const int Resolution_Do_Not_Resolve_By_Default = 1;

	  /// <summary>
	  /// The resolution state of a module meaning the module is marked as deprecated. </summary>
	  public const int Resolution_Warn_Deprecated = 2;

	  /// <summary>
	  /// The resolution state of a module meaning the module is marked as deprecated and will be removed
	  /// in a future release.
	  /// </summary>
	  public const int Resolution_Warn_Deprecated_For_Removal = 4;

	  /// <summary>
	  /// The resolution state of a module meaning the module is not yet standardized, so in incubating
	  /// mode.
	  /// </summary>
	  public const int Resolution_Warn_Incubating = 8;

	  /// <summary>
	  /// The resolution state of the module. Must be one of <seealso cref="Resolution_Warn_Deprecated"/>, {@link
	  /// #RESOLUTION_WARN_DEPRECATED_FOR_REMOVAL}, and <seealso cref="Resolution_Warn_Incubating"/>.
	  /// </summary>
	  public int resolution;

	  /// <summary>
	  /// Constructs a new <seealso cref="ModuleResolutionAttribute"/>.
	  /// </summary>
	  /// <param name="resolution"> the resolution state of the module. Must be one of {@link
	  ///     #RESOLUTION_WARN_DEPRECATED}, <seealso cref="Resolution_Warn_Deprecated_For_Removal"/>, and {@link
	  ///     #RESOLUTION_WARN_INCUBATING}. </param>
	  public ModuleResolutionAttribute(int resolution) : base("ModuleResolution")
	  {
		this.resolution = resolution;
	  }

	  /// <summary>
	  /// Constructs an empty <seealso cref="ModuleResolutionAttribute"/>. This object can be passed as a prototype
	  /// to the <seealso cref="ClassReader.Accept(ObjectWeb.Asm.ClassVisitor,ObjectWeb.Asm.Attribute[],int)"/> method.
	  /// </summary>
	  public ModuleResolutionAttribute() : this(0)
	  {
	  }

	  public override Attribute Read(ClassReader classReader, int offset, int length, char[] charBuffer, int codeOffset, Label[] labels)
	  {
		return new ModuleResolutionAttribute(classReader.ReadUnsignedShort(offset));
	  }

	  public override ByteVector Write(ClassWriter classWriter, byte[] code, int codeLength, int maxStack, int maxLocals)
	  {
		ByteVector byteVector = new ByteVector();
		byteVector.PutShort(resolution);
		return byteVector;
	  }
	}

}