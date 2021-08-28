

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
	/// A <seealso cref="ModuleVisitor"/> that remaps types with a <seealso cref="Remapper"/>.
	/// 
	/// @author Remi Forax
	/// </summary>
	public class ModuleRemapper : ModuleVisitor
	{

	  /// <summary>
	  /// The remapper used to remap the types in the visited module. </summary>
	  protected internal readonly Remapper remapper;

	  /// <summary>
	  /// Constructs a new <seealso cref="ModuleRemapper"/>. <i>Subclasses must not use this constructor</i>.
	  /// Instead, they must use the <seealso cref="ModuleRemapper(int,ModuleVisitor,Remapper)"/> version.
	  /// </summary>
	  /// <param name="moduleVisitor"> the module visitor this remapper must delegate to. </param>
	  /// <param name="remapper"> the remapper to use to remap the types in the visited module. </param>
	  public ModuleRemapper(ModuleVisitor moduleVisitor, Remapper remapper) : this(Opcodes.ASM9, moduleVisitor, remapper)
	  {
	  }

	  /// <summary>
	  /// Constructs a new <seealso cref="ModuleRemapper"/>.
	  /// </summary>
	  /// <param name="api"> the ASM API version supported by this remapper. Must be one of the {@code
	  ///     ASM}<i>x</i> values in <seealso cref="Opcodes"/>. </param>
	  /// <param name="moduleVisitor"> the module visitor this remapper must delegate to. </param>
	  /// <param name="remapper"> the remapper to use to remap the types in the visited module. </param>
	  public ModuleRemapper(int api, ModuleVisitor moduleVisitor, Remapper remapper) : base(api, moduleVisitor)
	  {
		this.remapper = remapper;
	  }

	  public override void visitMainClass(string mainClass)
	  {
		base.visitMainClass(remapper.mapType(mainClass));
	  }

	  public override void visitPackage(string packaze)
	  {
		base.visitPackage(remapper.mapPackageName(packaze));
	  }

	  public override void visitRequire(string module, int access, string version)
	  {
		base.visitRequire(remapper.mapModuleName(module), access, version);
	  }

	  public override void visitExport(string packaze, int access, params string[] modules)
	  {
		string[] remappedModules = null;
		if (modules != null)
		{
		  remappedModules = new string[modules.Length];
		  for (int i = 0; i < modules.Length; ++i)
		  {
			remappedModules[i] = remapper.mapModuleName(modules[i]);
		  }
		}
		base.visitExport(remapper.mapPackageName(packaze), access, remappedModules);
	  }

	  public override void visitOpen(string packaze, int access, params string[] modules)
	  {
		string[] remappedModules = null;
		if (modules != null)
		{
		  remappedModules = new string[modules.Length];
		  for (int i = 0; i < modules.Length; ++i)
		  {
			remappedModules[i] = remapper.mapModuleName(modules[i]);
		  }
		}
		base.visitOpen(remapper.mapPackageName(packaze), access, remappedModules);
	  }

	  public override void visitUse(string service)
	  {
		base.visitUse(remapper.mapType(service));
	  }

	  public override void visitProvide(string service, params string[] providers)
	  {
		string[] remappedProviders = new string[providers.Length];
		for (int i = 0; i < providers.Length; ++i)
		{
		  remappedProviders[i] = remapper.mapType(providers[i]);
		}
		base.visitProvide(remapper.mapType(service), remappedProviders);
	  }
	}

}