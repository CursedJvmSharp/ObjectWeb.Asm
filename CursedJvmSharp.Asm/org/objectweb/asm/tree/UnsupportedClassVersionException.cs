using org.objectweb.asm;
using CursedJvmSharp.Asm.Java.IO;
using System;

namespace org.objectweb.asm.tree
{
	/// <summary>
	/// Exception thrown in <seealso cref="AnnotationNode.check"/>, <seealso cref="ClassNode.check"/>, {@link
	/// FieldNode#check} and <seealso cref="MethodNode.check"/> when these nodes (or their children, recursively)
	/// contain elements that were introduced in more recent versions of the ASM API than version passed
	/// to these methods.
	/// 
	/// @author Eric Bruneton
	/// </summary>
	public class UnsupportedClassVersionException : Exception
	{

	  private const long serialVersionUID = -3502347765891805831L;
	}

}