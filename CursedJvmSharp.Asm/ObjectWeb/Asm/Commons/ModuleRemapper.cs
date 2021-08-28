

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
	  public ModuleRemapper(ModuleVisitor moduleVisitor, Remapper remapper) : this(IOpcodes.Asm9, moduleVisitor, remapper)
	  {
	  }

	  /// <summary>
	  /// Constructs a new <seealso cref="ModuleRemapper"/>.
	  /// </summary>
	  /// <param name="api"> the ASM API version supported by this remapper. Must be one of the {@code
	  ///     ASM}<i>x</i> values in <seealso cref="IOpcodes"/>. </param>
	  /// <param name="moduleVisitor"> the module visitor this remapper must delegate to. </param>
	  /// <param name="remapper"> the remapper to use to remap the types in the visited module. </param>
	  public ModuleRemapper(int api, ModuleVisitor moduleVisitor, Remapper remapper) : base(api, moduleVisitor)
	  {
		this.remapper = remapper;
	  }

	  public override void VisitMainClass(string mainClass)
	  {
		base.VisitMainClass(remapper.MapType(mainClass));
	  }

	  public override void VisitPackage(string packaze)
	  {
		base.VisitPackage(remapper.MapPackageName(packaze));
	  }

	  public override void VisitRequire(string module, int access, string version)
	  {
		base.VisitRequire(remapper.MapModuleName(module), access, version);
	  }

	  public override void VisitExport(string packaze, int access, params string[] modules)
	  {
		string[] remappedModules = null;
		if (modules != null)
		{
		  remappedModules = new string[modules.Length];
		  for (var i = 0; i < modules.Length; ++i)
		  {
			remappedModules[i] = remapper.MapModuleName(modules[i]);
		  }
		}
		base.VisitExport(remapper.MapPackageName(packaze), access, remappedModules);
	  }

	  public override void VisitOpen(string packaze, int access, params string[] modules)
	  {
		string[] remappedModules = null;
		if (modules != null)
		{
		  remappedModules = new string[modules.Length];
		  for (var i = 0; i < modules.Length; ++i)
		  {
			remappedModules[i] = remapper.MapModuleName(modules[i]);
		  }
		}
		base.VisitOpen(remapper.MapPackageName(packaze), access, remappedModules);
	  }

	  public override void VisitUse(string service)
	  {
		base.VisitUse(remapper.MapType(service));
	  }

	  public override void VisitProvide(string service, params string[] providers)
	  {
		var remappedProviders = new string[providers.Length];
		for (var i = 0; i < providers.Length; ++i)
		{
		  remappedProviders[i] = remapper.MapType(providers[i]);
		}
		base.VisitProvide(remapper.MapType(service), remappedProviders);
	  }
	}

}