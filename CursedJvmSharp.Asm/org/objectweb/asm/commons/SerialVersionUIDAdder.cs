using org.objectweb.asm;
using CursedJvmSharp.Asm.Java.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClassVisitor = org.objectweb.asm.ClassVisitor;
using FieldVisitor = org.objectweb.asm.FieldVisitor;
using MethodVisitor = org.objectweb.asm.MethodVisitor;
using Opcodes = org.objectweb.asm.Opcodes;

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
namespace org.objectweb.asm.commons
{

	/// <summary>
	/// A <seealso cref="ClassVisitor"/> that adds a serial version unique identifier to a class if missing. A
	/// typical usage of this class is:
	/// 
	/// <pre>
	///   ClassWriter classWriter = new ClassWriter(...);
	///   ClassVisitor svuidAdder = new SerialVersionUIDAdder(classWriter);
	///   ClassVisitor classVisitor = new MyClassAdapter(svuidAdder);
	///   new ClassReader(orginalClass).accept(classVisitor, 0);
	/// </pre>
	/// 
	/// <para>The SVUID algorithm can be found at <a href=
	/// "https://docs.oracle.com/javase/10/docs/specs/serialization/class.html#stream-unique-identifiers"
	/// >https://docs.oracle.com/javase/10/docs/specs/serialization/class.html#stream-unique-identifiers</a>:
	/// 
	/// </para>
	/// <para>The serialVersionUID is computed using the signature of a stream of bytes that reflect the
	/// class definition. The National Institute of Standards and Technology (NIST) Secure Hash Algorithm
	/// (SHA-1) is used to compute a signature for the stream. The first two 32-bit quantities are used
	/// to form a 64-bit hash. A java.lang.DataOutputStream is used to convert primitive data types to a
	/// sequence of bytes. The values input to the stream are defined by the Java Virtual Machine (VM)
	/// specification for classes.
	/// 
	/// </para>
	/// <para>The sequence of items in the stream is as follows:
	/// 
	/// <ol>
	///   <li>The class name written using UTF encoding.
	///   <li>The class modifiers written as a 32-bit integer.
	///   <li>The name of each interface sorted by name written using UTF encoding.
	///   <li>For each field of the class sorted by field name (except private static and private
	///       transient fields):
	///       <ol>
	///         <li>The name of the field in UTF encoding.
	///         <li>The modifiers of the field written as a 32-bit integer.
	///         <li>The descriptor of the field in UTF encoding
	///       </ol>
	///   <li>If a class initializer exists, write out the following:
	///       <ol>
	///         <li>The name of the method, &lt;clinit&gt;, in UTF encoding.
	///         <li>The modifier of the method, STATIC, written as a 32-bit integer.
	///         <li>The descriptor of the method, ()V, in UTF encoding.
	///       </ol>
	///   <li>For each non-private constructor sorted by method name and signature:
	///       <ol>
	///         <li>The name of the method, &lt;init&gt;, in UTF encoding.
	///         <li>The modifiers of the method written as a 32-bit integer.
	///         <li>The descriptor of the method in UTF encoding.
	///       </ol>
	///   <li>For each non-private method sorted by method name and signature:
	///       <ol>
	///         <li>The name of the method in UTF encoding.
	///         <li>The modifiers of the method written as a 32-bit integer.
	///         <li>The descriptor of the method in UTF encoding.
	///       </ol>
	///   <li>The SHA-1 algorithm is executed on the stream of bytes produced by DataOutputStream and
	///       produces five 32-bit values sha[0..4].
	///   <li>The hash value is assembled from the first and second 32-bit values of the SHA-1 message
	///       digest. If the result of the message digest, the five 32-bit words H0 H1 H2 H3 H4, is in an
	///       array of five int values named sha, the hash value would be computed as follows: long hash
	///       = ((sha[0] &gt;&gt;&gt; 24) &amp; 0xFF) | ((sha[0] &gt;&gt;&gt; 16) &amp; 0xFF) &lt;&lt; 8
	///       | ((sha[0] &gt;&gt;&gt; 8) &amp; 0xFF) &lt;&lt; 16 | ((sha[0] &gt;&gt;&gt; 0) &amp; 0xFF)
	///       &lt;&lt; 24 | ((sha[1] &gt;&gt;&gt; 24) &amp; 0xFF) &lt;&lt; 32 | ((sha[1] &gt;&gt;&gt; 16)
	///       &amp; 0xFF) &lt;&lt; 40 | ((sha[1] &gt;&gt;&gt; 8) &amp; 0xFF) &lt;&lt; 48 | ((sha[1]
	///       &gt;&gt;&gt; 0) &amp; 0xFF) &lt;&lt; 56;
	/// </ol>
	/// 
	/// @author Rajendra Inamdar, Vishal Vishnoi
	/// </para>
	/// </summary>
	// DontCheck(AbbreviationAsWordInName): can't be renamed (for backward binary compatibility).
	public class SerialVersionUIDAdder : ClassVisitor
	{

	  /// <summary>
	  /// The JVM name of static initializer methods. </summary>
	  private const string CLINIT = "<clinit>";

	  /// <summary>
	  /// A flag that indicates if we need to compute SVUID. </summary>
	  private bool computeSvuid;

	  /// <summary>
	  /// Whether the class already has a SVUID. </summary>
	  private bool hasSvuid;

	  /// <summary>
	  /// The class access flags. </summary>
	  private int access;

	  /// <summary>
	  /// The internal name of the class. </summary>
	  private string name;

	  /// <summary>
	  /// The interfaces implemented by the class. </summary>
	  private string[] interfaces;

	  /// <summary>
	  /// The fields of the class that are needed to compute the SVUID. </summary>
	  private ICollection<Item> svuidFields;

	  /// <summary>
	  /// Whether the class has a static initializer. </summary>
	  private bool hasStaticInitializer;

	  /// <summary>
	  /// The constructors of the class that are needed to compute the SVUID. </summary>
	  private ICollection<Item> svuidConstructors;

	  /// <summary>
	  /// The methods of the class that are needed to compute the SVUID. </summary>
	  private ICollection<Item> svuidMethods;

	  /// <summary>
	  /// Constructs a new <seealso cref="SerialVersionUIDAdder"/>. <i>Subclasses must not use this
	  /// constructor</i>. Instead, they must use the <seealso cref="SerialVersionUIDAdder(int, ClassVisitor)"/>
	  /// version.
	  /// </summary>
	  /// <param name="classVisitor"> a <seealso cref="ClassVisitor"/> to which this visitor will delegate calls. </param>
	  /// <exception cref="IllegalStateException"> If a subclass calls this constructor. </exception>
	  public SerialVersionUIDAdder(ClassVisitor classVisitor) : this(Opcodes.ASM9, classVisitor)
	  {
		if (this.GetType() != typeof(SerialVersionUIDAdder))
		{
		  throw new System.InvalidOperationException();
		}
	  }

	  /// <summary>
	  /// Constructs a new <seealso cref="SerialVersionUIDAdder"/>.
	  /// </summary>
	  /// <param name="api"> the ASM API version implemented by this visitor. Must be one of the {@code
	  ///     ASM}<i>x</i> values in <seealso cref="Opcodes"/>. </param>
	  /// <param name="classVisitor"> a <seealso cref="ClassVisitor"/> to which this visitor will delegate calls. </param>
	  public SerialVersionUIDAdder(int api, ClassVisitor classVisitor) : base(api, classVisitor)
	  {
	  }

	  // -----------------------------------------------------------------------------------------------
	  // Overridden methods
	  // -----------------------------------------------------------------------------------------------

	  public override void visit(int version, int access, string name, string signature, string superName, string[] interfaces)
	  {
		// Get the class name, access flags, and interfaces information (step 1, 2 and 3) for SVUID
		// computation.
		computeSvuid = (access & Opcodes.ACC_ENUM) == 0;

		if (computeSvuid)
		{
		  this.name = name;
		  this.access = access;
		  this.interfaces = (string[])interfaces.Clone();
		  this.svuidFields = new List<Item>();
		  this.svuidConstructors = new List<Item>();
		  this.svuidMethods = new List<Item>();
		}

		base.visit(version, access, name, signature, superName, interfaces);
	  }

	  public override MethodVisitor visitMethod(int access, string name, string descriptor, string signature, string[] exceptions)
	  {
		// Get constructor and method information (step 5 and 7). Also determine if there is a class
		// initializer (step 6).
		if (computeSvuid)
		{
		  if (CLINIT.Equals(name))
		  {
			hasStaticInitializer = true;
		  }
		  // Collect the non private constructors and methods. Only the ACC_PUBLIC, ACC_PRIVATE,
		  // ACC_PROTECTED, ACC_STATIC, ACC_FINAL, ACC_SYNCHRONIZED, ACC_NATIVE, ACC_ABSTRACT and
		  // ACC_STRICT flags are used.
		  int mods = access & (Opcodes.ACC_PUBLIC | Opcodes.ACC_PRIVATE | Opcodes.ACC_PROTECTED | Opcodes.ACC_STATIC | Opcodes.ACC_FINAL | Opcodes.ACC_SYNCHRONIZED | Opcodes.ACC_NATIVE | Opcodes.ACC_ABSTRACT | Opcodes.ACC_STRICT);

		  if ((access & Opcodes.ACC_PRIVATE) == 0)
		  {
			if ("<init>".Equals(name))
			{
			  svuidConstructors.Add(new Item(name, mods, descriptor));
			}
			else if (!CLINIT.Equals(name))
			{
			  svuidMethods.Add(new Item(name, mods, descriptor));
			}
		  }
		}

		return base.visitMethod(access, name, descriptor, signature, exceptions);
	  }

	  public override FieldVisitor visitField(int access, string name, string desc, string signature, object value)
	  {
		// Get the class field information for step 4 of the algorithm. Also determine if the class
		// already has a SVUID.
		if (computeSvuid)
		{
		  if ("serialVersionUID".Equals(name))
		  {
			// Since the class already has SVUID, we won't be computing it.
			computeSvuid = false;
			hasSvuid = true;
		  }
		  // Collect the non private fields. Only the ACC_PUBLIC, ACC_PRIVATE, ACC_PROTECTED,
		  // ACC_STATIC, ACC_FINAL, ACC_VOLATILE, and ACC_TRANSIENT flags are used when computing
		  // serialVersionUID values.
		  if ((access & Opcodes.ACC_PRIVATE) == 0 || (access & (Opcodes.ACC_STATIC | Opcodes.ACC_TRANSIENT)) == 0)
		  {
			int mods = access & (Opcodes.ACC_PUBLIC | Opcodes.ACC_PRIVATE | Opcodes.ACC_PROTECTED | Opcodes.ACC_STATIC | Opcodes.ACC_FINAL | Opcodes.ACC_VOLATILE | Opcodes.ACC_TRANSIENT);
			svuidFields.Add(new Item(name, mods, desc));
		  }
		}

		return base.visitField(access, name, desc, signature, value);
	  }

	  public override void visitInnerClass(string innerClassName, string outerName, string innerName, int innerClassAccess)
	  {
		// Handles a bizarre special case. Nested classes (static classes declared inside another class)
		// that are protected have their access bit set to public in their class files to deal with some
		// odd reflection situation. Our SVUID computation must do as the JVM does and ignore access
		// bits in the class file in favor of the access bits of the InnerClass attribute.
		if ((!string.ReferenceEquals(name, null)) && name.Equals(innerClassName))
		{
		  this.access = innerClassAccess;
		}
		base.visitInnerClass(innerClassName, outerName, innerName, innerClassAccess);
	  }

	  public override void visitEnd()
	  {
		// Add the SVUID field to the class if it doesn't have one.
		if (computeSvuid && !hasSvuid)
		{
		  try
		  {
			addSVUID(computeSVUID());
		  }
		  catch (IOException e)
		  {
			throw new System.InvalidOperationException("Error while computing SVUID for " + name, e);
		  }
		}

		base.visitEnd();
	  }

	  // -----------------------------------------------------------------------------------------------
	  // Utility methods
	  // -----------------------------------------------------------------------------------------------

	  /// <summary>
	  /// Returns true if the class already has a SVUID field. The result of this method is only valid
	  /// when visitEnd has been called.
	  /// </summary>
	  /// <returns> true if the class already has a SVUID field. </returns>
	  // DontCheck(AbbreviationAsWordInName): can't be renamed (for backward binary compatibility).
	  public virtual bool hasSVUID()
	  {
		return hasSvuid;
	  }

	  /// <summary>
	  /// Adds a final static serialVersionUID field to the class, with the given value.
	  /// </summary>
	  /// <param name="svuid"> the serialVersionUID field value. </param>
	  // DontCheck(AbbreviationAsWordInName): can't be renamed (for backward binary compatibility).
	  public virtual void addSVUID(long svuid)
	  {
		FieldVisitor fieldVisitor = base.visitField(Opcodes.ACC_FINAL + Opcodes.ACC_STATIC, "serialVersionUID", "J", null, svuid);
		if (fieldVisitor != null)
		{
		  fieldVisitor.visitEnd();
		}
	  }

	  /// <summary>
	  /// Computes and returns the value of SVUID.
	  /// </summary>
	  /// <returns> the serial version UID. </returns>
	  /// <exception cref="IOException"> if an I/O error occurs. </exception>
	  // DontCheck(AbbreviationAsWordInName): can't be renamed (for backward binary compatibility).
	  public virtual long computeSVUID()
	  {
		long svuid = 0;

		using (MemoryStream byteArrayOutputStream = new MemoryStream(), DataOutputStream dataOutputStream = new DataOutputStream(byteArrayOutputStream))
		{

		  // 1. The class name written using UTF encoding.
		  dataOutputStream.writeUTF(name.Replace('/', '.'));

		  // 2. The class modifiers written as a 32-bit integer.
		  int mods = access;
		  if ((mods & Opcodes.ACC_INTERFACE) != 0)
		  {
			mods = svuidMethods.Count == 0 ? (mods & ~Opcodes.ACC_ABSTRACT) : (mods | Opcodes.ACC_ABSTRACT);
		  }
		  dataOutputStream.writeInt(mods & (Opcodes.ACC_PUBLIC | Opcodes.ACC_FINAL | Opcodes.ACC_INTERFACE | Opcodes.ACC_ABSTRACT));

		  // 3. The name of each interface sorted by name written using UTF encoding.
		  Array.Sort(interfaces);
		  foreach (string interfaceName in interfaces)
		  {
			dataOutputStream.writeUTF(interfaceName.Replace('/', '.'));
		  }

		  // 4. For each field of the class sorted by field name (except private static and private
		  // transient fields):
		  //   1. The name of the field in UTF encoding.
		  //   2. The modifiers of the field written as a 32-bit integer.
		  //   3. The descriptor of the field in UTF encoding.
		  // Note that field signatures are not dot separated. Method and constructor signatures are dot
		  // separated. Go figure...
		  writeItems(svuidFields, dataOutputStream, false);

		  // 5. If a class initializer exists, write out the following:
		  //   1. The name of the method, <clinit>, in UTF encoding.
		  //   2. The modifier of the method, ACC_STATIC, written as a 32-bit integer.
		  //   3. The descriptor of the method, ()V, in UTF encoding.
		  if (hasStaticInitializer)
		  {
			dataOutputStream.writeUTF(CLINIT);
			dataOutputStream.writeInt(Opcodes.ACC_STATIC);
			dataOutputStream.writeUTF("()V");
		  }

		  // 6. For each non-private constructor sorted by method name and signature:
		  //   1. The name of the method, <init>, in UTF encoding.
		  //   2. The modifiers of the method written as a 32-bit integer.
		  //   3. The descriptor of the method in UTF encoding.
		  writeItems(svuidConstructors, dataOutputStream, true);

		  // 7. For each non-private method sorted by method name and signature:
		  //   1. The name of the method in UTF encoding.
		  //   2. The modifiers of the method written as a 32-bit integer.
		  //   3. The descriptor of the method in UTF encoding.
		  writeItems(svuidMethods, dataOutputStream, true);

		  dataOutputStream.flush();

		  // 8. The SHA-1 algorithm is executed on the stream of bytes produced by DataOutputStream and
		  // produces five 32-bit values sha[0..4].
		  sbyte[] hashBytes = computeSHAdigest(byteArrayOutputStream.toByteArray());

		  // 9. The hash value is assembled from the first and second 32-bit values of the SHA-1 message
		  // digest. If the result of the message digest, the five 32-bit words H0 H1 H2 H3 H4, is in an
		  // array of five int values named sha, the hash value would be computed as follows:
		  for (int i = Math.Min(hashBytes.Length, 8) - 1; i >= 0; i--)
		  {
			svuid = (svuid << 8) | (hashBytes[i] & 0xFF);
		  }
		}

		return svuid;
	  }

	  /// <summary>
	  /// Returns the SHA-1 message digest of the given value.
	  /// </summary>
	  /// <param name="value"> the value whose SHA message digest must be computed. </param>
	  /// <returns> the SHA-1 message digest of the given value. </returns>
	  // DontCheck(AbbreviationAsWordInName): can't be renamed (for backward binary compatibility).
	  public virtual sbyte[] computeSHAdigest(sbyte[] value)
	  {
		try
		{
		  return MessageDigest.getInstance("SHA").digest(value);
		}
		catch (NoSuchAlgorithmException e)
		{
		  throw new System.NotSupportedException(e);
		}
	  }

	  /// <summary>
	  /// Sorts the items in the collection and writes it to the given output stream.
	  /// </summary>
	  /// <param name="itemCollection"> a collection of items. </param>
	  /// <param name="dataOutputStream"> where the items must be written. </param>
	  /// <param name="dotted"> whether package names must use dots, instead of slashes. </param>
	  /// <exception cref="IOException"> if an error occurs. </exception>
	  private static void writeItems(ICollection<Item> itemCollection, DataOutput dataOutputStream, bool dotted)
	  {
		Item[] items = itemCollection.ToArray();
		Array.Sort(items);
		foreach (Item item in items)
		{
		  dataOutputStream.writeUTF(item.name);
		  dataOutputStream.writeInt(item.access);
		  dataOutputStream.writeUTF(dotted ? item.descriptor.Replace('/', '.') : item.descriptor);
		}
	  }

	  // -----------------------------------------------------------------------------------------------
	  // Inner classes
	  // -----------------------------------------------------------------------------------------------

	  private sealed class Item : IComparable<Item>
	  {

		internal readonly string name;
		internal readonly int access;
		internal readonly string descriptor;

		public Item(string name, int access, string descriptor)
		{
		  this.name = name;
		  this.access = access;
		  this.descriptor = descriptor;
		}

		public int CompareTo(Item item)
		{
		  int result = string.CompareOrdinal(name, item.name);
		  if (result == 0)
		  {
			result = string.CompareOrdinal(descriptor, item.descriptor);
		  }
		  return result;
		}

		public override bool Equals(object other)
		{
		  if (other is Item)
		  {
			return CompareTo((Item) other) == 0;
		  }
		  return false;
		}

		public override int GetHashCode()
		{
		  return name.GetHashCode() ^ descriptor.GetHashCode();
		}
	  }
	}

}