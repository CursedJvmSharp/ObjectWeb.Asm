

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
	/// A ModuleTarget attribute. This attribute is specific to the OpenJDK and may change in the future.
	/// 
	/// @author Remi Forax
	/// </summary>
	public sealed class ModuleTargetAttribute : Attribute
	{

	  /// <summary>
	  /// The name of the platform on which the module can run. </summary>
	  public string platform;

	  /// <summary>
	  /// Constructs a new <seealso cref="ModuleTargetAttribute"/>.
	  /// </summary>
	  /// <param name="platform"> the name of the platform on which the module can run. </param>
	  public ModuleTargetAttribute(string platform) : base("ModuleTarget")
	  {
		this.platform = platform;
	  }

	  /// <summary>
	  /// Constructs an empty <seealso cref="ModuleTargetAttribute"/>. This object can be passed as a prototype to
	  /// the <seealso cref="ClassReader.Accept(ObjectWeb.Asm.ClassVisitor,ObjectWeb.Asm.Attribute[],int)"/> method.
	  /// </summary>
	  public ModuleTargetAttribute() : this(null)
	  {
	  }

	  public override Attribute Read(ClassReader classReader, int offset, int length, char[] charBuffer, int codeOffset, Label[] labels)
	  {
		return new ModuleTargetAttribute(classReader.ReadUtf8(offset, charBuffer));
	  }

	  public override ByteVector Write(ClassWriter classWriter, byte[] code, int codeLength, int maxStack, int maxLocals)
	  {
		var byteVector = new ByteVector();
		byteVector.PutShort(string.ReferenceEquals(platform, null) ? 0 : classWriter.NewUtf8(platform));
		return byteVector;
	  }
	}

}