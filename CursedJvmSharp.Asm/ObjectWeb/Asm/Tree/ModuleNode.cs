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
	/// A node that represents a module declaration.
	/// 
	/// @author Remi Forax
	/// </summary>
	public class ModuleNode : ModuleVisitor
	{

	  /// <summary>
	  /// The fully qualified name (using dots) of this module. </summary>
	  public string name;

	  /// <summary>
	  /// The module's access flags, among {@code ACC_OPEN}, {@code ACC_SYNTHETIC} and {@code
	  /// ACC_MANDATED}.
	  /// </summary>
	  public int access;

	  /// <summary>
	  /// The version of this module. May be {@literal null}. </summary>
	  public string version;

	  /// <summary>
	  /// The internal name of the main class of this module. May be {@literal null}. </summary>
	  public string mainClass;

	  /// <summary>
	  /// The internal name of the packages declared by this module. May be {@literal null}. </summary>
	  public List<string> packages;

	  /// <summary>
	  /// The dependencies of this module. May be {@literal null}. </summary>
	  public List<ModuleRequireNode> requires;

	  /// <summary>
	  /// The packages exported by this module. May be {@literal null}. </summary>
	  public List<ModuleExportNode> exports;

	  /// <summary>
	  /// The packages opened by this module. May be {@literal null}. </summary>
	  public List<ModuleOpenNode> opens;

	  /// <summary>
	  /// The internal names of the services used by this module. May be {@literal null}. </summary>
	  public List<string> uses;

	  /// <summary>
	  /// The services provided by this module. May be {@literal null}. </summary>
	  public List<ModuleProvideNode> provides;

	  /// <summary>
	  /// Constructs a <seealso cref="ModuleNode"/>. <i>Subclasses must not use this constructor</i>. Instead, they
	  /// must use the <seealso cref="ModuleNode(int,String,int,String,List,List,List,List,List)"/> version.
	  /// </summary>
	  /// <param name="name"> the fully qualified name (using dots) of the module. </param>
	  /// <param name="access"> the module access flags, among {@code ACC_OPEN}, {@code ACC_SYNTHETIC} and {@code
	  ///     ACC_MANDATED}. </param>
	  /// <param name="version"> the module version, or {@literal null}. </param>
	  /// <exception cref="IllegalStateException"> If a subclass calls this constructor. </exception>
	  public ModuleNode(string name, int access, string version) : base(IOpcodes.Asm9)
	  {
		if (this.GetType() != typeof(ModuleNode))
		{
		  throw new System.InvalidOperationException();
		}
		this.name = name;
		this.access = access;
		this.version = version;
	  }

	  // TODO(forax): why is there no 'mainClass' and 'packages' parameters in this constructor?
	  /// <summary>
	  /// Constructs a <seealso cref="ModuleNode"/>.
	  /// </summary>
	  /// <param name="api"> the ASM API version implemented by this visitor. Must be one of {@link
	  ///     Opcodes#ASM6}, <seealso cref="IIOpcodes.Asm7/>, <seealso cref="IIOpcodes.Asm8/> or <seealso cref="IIOpcodes.Asm9/>. </param>
	  /// <param name="name"> the fully qualified name (using dots) of the module. </param>
	  /// <param name="access"> the module access flags, among {@code ACC_OPEN}, {@code ACC_SYNTHETIC} and {@code
	  ///     ACC_MANDATED}. </param>
	  /// <param name="version"> the module version, or {@literal null}. </param>
	  /// <param name="requires"> The dependencies of this module. May be {@literal null}. </param>
	  /// <param name="exports"> The packages exported by this module. May be {@literal null}. </param>
	  /// <param name="opens"> The packages opened by this module. May be {@literal null}. </param>
	  /// <param name="uses"> The internal names of the services used by this module. May be {@literal null}. </param>
	  /// <param name="provides"> The services provided by this module. May be {@literal null}. </param>
	  public ModuleNode(int api, string name, int access, string version, List<ModuleRequireNode> requires, List<ModuleExportNode> exports, List<ModuleOpenNode> opens, List<string> uses, List<ModuleProvideNode> provides) : base(api)
	  {
		this.name = name;
		this.access = access;
		this.version = version;
		this.requires = requires;
		this.exports = exports;
		this.opens = opens;
		this.uses = uses;
		this.provides = provides;
	  }

	  public override void VisitMainClass(string mainClass)
	  {
		this.mainClass = mainClass;
	  }

	  public override void VisitPackage(string packaze)
	  {
		if (packages == null)
		{
	  packages = new List<string>(5);
		}
	packages.Add(packaze);
	  }

	  public override void VisitRequire(string module, int access, string version)
	  {
		if (requires == null)
		{
		  requires = new List<ModuleRequireNode>(5);
		}
		requires.Add(new ModuleRequireNode(module, access, version));
	  }

	  public override void VisitExport(string packaze, int access, params string[] modules)
	  {
		if (exports == null)
		{
		  exports = new List<ModuleExportNode>(5);
		}
		exports.Add(new ModuleExportNode(packaze, access, Util.AsArrayList(modules)));
	  }

	  public override void VisitOpen(string packaze, int access, params string[] modules)
	  {
		if (opens == null)
		{
		  opens = new List<ModuleOpenNode>(5);
		}
		opens.Add(new ModuleOpenNode(packaze, access, Util.AsArrayList(modules)));
	  }

	  public override void VisitUse(string service)
	  {
		if (uses == null)
		{
		  uses = new List<string>(5);
		}
		uses.Add(service);
	  }

	  public override void VisitProvide(string service, params string[] providers)
	  {
		if (provides == null)
		{
		  provides = new List<ModuleProvideNode>(5);
		}
		provides.Add(new ModuleProvideNode(service, Util.AsArrayList(providers)));
	  }

	  public override void VisitEnd()
	  {
		// Nothing to do.
	  }

	  /// <summary>
	  /// Makes the given class visitor visit this module.
	  /// </summary>
	  /// <param name="classVisitor"> a class visitor. </param>
	  public virtual void Accept(ClassVisitor classVisitor)
	  {
		var moduleVisitor = classVisitor.VisitModule(name, access, version);
		if (moduleVisitor == null)
		{
		  return;
		}
		if (!string.ReferenceEquals(mainClass, null))
		{
		  moduleVisitor.VisitMainClass(mainClass);
		}
		if (packages != null)
		{
		  for (int i = 0, n = packages.Count; i < n; i++)
		  {
			moduleVisitor.VisitPackage(packages[i]);
		  }
		}
		if (requires != null)
		{
		  for (int i = 0, n = requires.Count; i < n; i++)
		  {
			requires[i].Accept(moduleVisitor);
		  }
		}
		if (exports != null)
		{
		  for (int i = 0, n = exports.Count; i < n; i++)
		  {
			exports[i].Accept(moduleVisitor);
		  }
		}
		if (opens != null)
		{
		  for (int i = 0, n = opens.Count; i < n; i++)
		  {
			opens[i].Accept(moduleVisitor);
		  }
		}
		if (uses != null)
		{
		  for (int i = 0, n = uses.Count; i < n; i++)
		  {
			moduleVisitor.VisitUse(uses[i]);
		  }
		}
		if (provides != null)
		{
		  for (int i = 0, n = provides.Count; i < n; i++)
		  {
			provides[i].Accept(moduleVisitor);
		  }
		}
	  }
	}

}