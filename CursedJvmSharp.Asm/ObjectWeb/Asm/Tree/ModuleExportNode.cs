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
	/// A node that represents an exported package with its name and the module that can access to it.
	/// 
	/// @author Remi Forax
	/// </summary>
	public class ModuleExportNode
	{

	  /// <summary>
	  /// The internal name of the exported package. </summary>
	  public string packaze;

	  /// <summary>
	  /// The access flags (see <seealso cref="IOpcodes"/>). Valid values are {@code
	  /// ACC_SYNTHETIC} and {@code ACC_MANDATED}.
	  /// </summary>
	  public int access;

	  /// <summary>
	  /// The list of modules that can access this exported package, specified with fully qualified names
	  /// (using dots). May be {@literal null}.
	  /// </summary>
	  public List<string> modules;

	  /// <summary>
	  /// Constructs a new <seealso cref="ModuleExportNode"/>.
	  /// </summary>
	  /// <param name="packaze"> the internal name of the exported package. </param>
	  /// <param name="access"> the package access flags, one or more of {@code ACC_SYNTHETIC} and {@code
	  ///     ACC_MANDATED}. </param>
	  /// <param name="modules"> a list of modules that can access this exported package, specified with fully
	  ///     qualified names (using dots). </param>
	  public ModuleExportNode(string packaze, int access, List<string> modules)
	  {
		this.packaze = packaze;
		this.access = access;
		this.modules = modules;
	  }

	  /// <summary>
	  /// Makes the given module visitor visit this export declaration.
	  /// </summary>
	  /// <param name="moduleVisitor"> a module visitor. </param>
	  public virtual void Accept(ModuleVisitor moduleVisitor)
	  {
		moduleVisitor.VisitExport(packaze, access, modules == null ? null : ((List<string>)modules).ToArray());
	  }
	}

}