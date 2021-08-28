

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
	/// A visitor to visit a Java module. The methods of this class must be called in the following
	/// order: ( {@code visitMainClass} | ( {@code visitPackage} | {@code visitRequire} | {@code
	/// visitExport} | {@code visitOpen} | {@code visitUse} | {@code visitProvide} )* ) {@code visitEnd}.
	/// 
	/// @author Remi Forax
	/// @author Eric Bruneton
	/// </summary>
	public abstract class ModuleVisitor
	{
	  /// <summary>
	  /// The ASM API version implemented by this visitor. The value of this field must be one of {@link
	  /// Opcodes#ASM6} or <seealso cref="Opcodes.ASM7"/>.
	  /// </summary>
	  protected internal readonly int api;

	  /// <summary>
	  /// The module visitor to which this visitor must delegate method calls. May be {@literal null}.
	  /// </summary>
	  protected internal ModuleVisitor mv;

	  /// <summary>
	  /// Constructs a new <seealso cref="ModuleVisitor"/>.
	  /// </summary>
	  /// <param name="api"> the ASM API version implemented by this visitor. Must be one of <seealso cref="Opcodes.ASM6"/>
	  ///     or <seealso cref="Opcodes.ASM7"/>. </param>
	  public ModuleVisitor(int api) : this(api, null)
	  {
	  }

	  /// <summary>
	  /// Constructs a new <seealso cref="ModuleVisitor"/>.
	  /// </summary>
	  /// <param name="api"> the ASM API version implemented by this visitor. Must be one of <seealso cref="Opcodes.ASM6"/>
	  ///     or <seealso cref="Opcodes.ASM7"/>. </param>
	  /// <param name="moduleVisitor"> the module visitor to which this visitor must delegate method calls. May
	  ///     be null. </param>
	  public ModuleVisitor(int api, ModuleVisitor moduleVisitor)
	  {
		if (api != Opcodes.ASM9 && api != Opcodes.ASM8 && api != Opcodes.ASM7 && api != Opcodes.ASM6 && api != Opcodes.ASM5 && api != Opcodes.ASM4 && api != Opcodes.ASM10_EXPERIMENTAL)
		{
		  throw new System.ArgumentException("Unsupported api " + api);
		}
		if (api == Opcodes.ASM10_EXPERIMENTAL)
		{
		  Constants.checkAsmExperimental(this);
		}
		this.api = api;
		this.mv = moduleVisitor;
	  }

	  /// <summary>
	  /// Visit the main class of the current module.
	  /// </summary>
	  /// <param name="mainClass"> the internal name of the main class of the current module. </param>
	  public virtual void visitMainClass(string mainClass)
	  {
		if (mv != null)
		{
		  mv.visitMainClass(mainClass);
		}
	  }

	  /// <summary>
	  /// Visit a package of the current module.
	  /// </summary>
	  /// <param name="packaze"> the internal name of a package. </param>
	  public virtual void visitPackage(string packaze)
	  {
		if (mv != null)
		{
		  mv.visitPackage(packaze);
		}
	  }

	  /// <summary>
	  /// Visits a dependence of the current module.
	  /// </summary>
	  /// <param name="module"> the fully qualified name (using dots) of the dependence. </param>
	  /// <param name="access"> the access flag of the dependence among {@code ACC_TRANSITIVE}, {@code
	  ///     ACC_STATIC_PHASE}, {@code ACC_SYNTHETIC} and {@code ACC_MANDATED}. </param>
	  /// <param name="version"> the module version at compile time, or {@literal null}. </param>
	  public virtual void visitRequire(string module, int access, string version)
	  {
		if (mv != null)
		{
		  mv.visitRequire(module, access, version);
		}
	  }

	  /// <summary>
	  /// Visit an exported package of the current module.
	  /// </summary>
	  /// <param name="packaze"> the internal name of the exported package. </param>
	  /// <param name="access"> the access flag of the exported package, valid values are among {@code
	  ///     ACC_SYNTHETIC} and {@code ACC_MANDATED}. </param>
	  /// <param name="modules"> the fully qualified names (using dots) of the modules that can access the public
	  ///     classes of the exported package, or {@literal null}. </param>
	  public virtual void visitExport(string packaze, int access, params string[] modules)
	  {
		if (mv != null)
		{
		  mv.visitExport(packaze, access, modules);
		}
	  }

	  /// <summary>
	  /// Visit an open package of the current module.
	  /// </summary>
	  /// <param name="packaze"> the internal name of the opened package. </param>
	  /// <param name="access"> the access flag of the opened package, valid values are among {@code
	  ///     ACC_SYNTHETIC} and {@code ACC_MANDATED}. </param>
	  /// <param name="modules"> the fully qualified names (using dots) of the modules that can use deep
	  ///     reflection to the classes of the open package, or {@literal null}. </param>
	  public virtual void visitOpen(string packaze, int access, params string[] modules)
	  {
		if (mv != null)
		{
		  mv.visitOpen(packaze, access, modules);
		}
	  }

	  /// <summary>
	  /// Visit a service used by the current module. The name must be the internal name of an interface
	  /// or a class.
	  /// </summary>
	  /// <param name="service"> the internal name of the service. </param>
	  public virtual void visitUse(string service)
	  {
		if (mv != null)
		{
		  mv.visitUse(service);
		}
	  }

	  /// <summary>
	  /// Visit an implementation of a service.
	  /// </summary>
	  /// <param name="service"> the internal name of the service. </param>
	  /// <param name="providers"> the internal names of the implementations of the service (there is at least
	  ///     one provider). </param>
	  public virtual void visitProvide(string service, params string[] providers)
	  {
		if (mv != null)
		{
		  mv.visitProvide(service, providers);
		}
	  }

	  /// <summary>
	  /// Visits the end of the module. This method, which is the last one to be called, is used to
	  /// inform the visitor that everything have been visited.
	  /// </summary>
	  public virtual void visitEnd()
	  {
		if (mv != null)
		{
		  mv.visitEnd();
		}
	  }
	}

}