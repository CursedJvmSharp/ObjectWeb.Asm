using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Java.IO;

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
	public class SerialVersionUidAdder : ClassVisitor
	{

	  /// <summary>
	  /// The JVM name of static initializer methods. </summary>
	  private const string Clinit = "<clinit>";

	  /// <summary>
	  /// A flag that indicates if we need to compute SVUID. </summary>
	  private bool _computeSvuid;

	  /// <summary>
	  /// Whether the class already has a SVUID. </summary>
	  private bool _hasSvuid;

	  /// <summary>
	  /// The class access flags. </summary>
	  private int _access;

	  /// <summary>
	  /// The internal name of the class. </summary>
	  private string _name;

	  /// <summary>
	  /// The interfaces implemented by the class. </summary>
	  private string[] _interfaces;

	  /// <summary>
	  /// The fields of the class that are needed to compute the SVUID. </summary>
	  private ICollection<Item> _svuidFields;

	  /// <summary>
	  /// Whether the class has a static initializer. </summary>
	  private bool _hasStaticInitializer;

	  /// <summary>
	  /// The constructors of the class that are needed to compute the SVUID. </summary>
	  private ICollection<Item> _svuidConstructors;

	  /// <summary>
	  /// The methods of the class that are needed to compute the SVUID. </summary>
	  private ICollection<Item> _svuidMethods;

	  /// <summary>
	  /// Constructs a new <seealso cref="SerialVersionUidAdder"/>. <i>Subclasses must not use this
	  /// constructor</i>. Instead, they must use the <seealso cref="SerialVersionUidAdder(int,ObjectWeb.Asm.ClassVisitor)"/>
	  /// version.
	  /// </summary>
	  /// <param name="classVisitor"> a <seealso cref="ClassVisitor"/> to which this visitor will delegate calls. </param>
	  /// <exception cref="IllegalStateException"> If a subclass calls this constructor. </exception>
	  public SerialVersionUidAdder(ClassVisitor classVisitor) : this(IOpcodes.Asm9, classVisitor)
	  {
		if (this.GetType() != typeof(SerialVersionUidAdder))
		{
		  throw new System.InvalidOperationException();
		}
	  }

	  /// <summary>
	  /// Constructs a new <seealso cref="SerialVersionUidAdder"/>.
	  /// </summary>
	  /// <param name="api"> the ASM API version implemented by this visitor. Must be one of the {@code
	  ///     ASM}<i>x</i> values in <seealso cref="IOpcodes"/>. </param>
	  /// <param name="classVisitor"> a <seealso cref="ClassVisitor"/> to which this visitor will delegate calls. </param>
	  public SerialVersionUidAdder(int api, ClassVisitor classVisitor) : base(api, classVisitor)
	  {
	  }

	  // -----------------------------------------------------------------------------------------------
	  // Overridden methods
	  // -----------------------------------------------------------------------------------------------

	  public override void Visit(int version, int access, string name, string signature, string superName, string[] interfaces)
	  {
		// Get the class name, access flags, and interfaces information (step 1, 2 and 3) for SVUID
		// computation.
		_computeSvuid = (access & IOpcodes.Acc_Enum) == 0;

		if (_computeSvuid)
		{
		  this._name = name;
		  this._access = access;
		  this._interfaces = (string[])interfaces.Clone();
		  this._svuidFields = new List<Item>();
		  this._svuidConstructors = new List<Item>();
		  this._svuidMethods = new List<Item>();
		}

		base.Visit(version, access, name, signature, superName, interfaces);
	  }

	  public override MethodVisitor VisitMethod(int access, string name, string descriptor, string signature, string[] exceptions)
	  {
		// Get constructor and method information (step 5 and 7). Also determine if there is a class
		// initializer (step 6).
		if (_computeSvuid)
		{
		  if (Clinit.Equals(name))
		  {
			_hasStaticInitializer = true;
		  }
		  // Collect the non private constructors and methods. Only the ACC_PUBLIC, ACC_PRIVATE,
		  // ACC_PROTECTED, ACC_STATIC, ACC_FINAL, ACC_SYNCHRONIZED, ACC_NATIVE, ACC_ABSTRACT and
		  // ACC_STRICT flags are used.
		  var mods = access & (IOpcodes.Acc_Public | IOpcodes.Acc_Private | IOpcodes.Acc_Protected | IOpcodes.Acc_Static | IOpcodes.Acc_Final | IOpcodes.Acc_Synchronized | IOpcodes.Acc_Native | IOpcodes.Acc_Abstract | IOpcodes.Acc_Strict);

		  if ((access & IOpcodes.Acc_Private) == 0)
		  {
			if ("<init>".Equals(name))
			{
			  _svuidConstructors.Add(new Item(name, mods, descriptor));
			}
			else if (!Clinit.Equals(name))
			{
			  _svuidMethods.Add(new Item(name, mods, descriptor));
			}
		  }
		}

		return base.VisitMethod(access, name, descriptor, signature, exceptions);
	  }

	  public override FieldVisitor VisitField(int access, string name, string desc, string signature, object value)
	  {
		// Get the class field information for step 4 of the algorithm. Also determine if the class
		// already has a SVUID.
		if (_computeSvuid)
		{
		  if ("serialVersionUID".Equals(name))
		  {
			// Since the class already has SVUID, we won't be computing it.
			_computeSvuid = false;
			_hasSvuid = true;
		  }
		  // Collect the non private fields. Only the ACC_PUBLIC, ACC_PRIVATE, ACC_PROTECTED,
		  // ACC_STATIC, ACC_FINAL, ACC_VOLATILE, and ACC_TRANSIENT flags are used when computing
		  // serialVersionUID values.
		  if ((access & IOpcodes.Acc_Private) == 0 || (access & (IOpcodes.Acc_Static | IOpcodes.Acc_Transient)) == 0)
		  {
			var mods = access & (IOpcodes.Acc_Public | IOpcodes.Acc_Private | IOpcodes.Acc_Protected | IOpcodes.Acc_Static | IOpcodes.Acc_Final | IOpcodes.Acc_Volatile | IOpcodes.Acc_Transient);
			_svuidFields.Add(new Item(name, mods, desc));
		  }
		}

		return base.VisitField(access, name, desc, signature, value);
	  }

	  public override void VisitInnerClass(string innerClassName, string outerName, string innerName, int innerClassAccess)
	  {
		// Handles a bizarre special case. Nested classes (static classes declared inside another class)
		// that are protected have their access bit set to public in their class files to deal with some
		// odd reflection situation. Our SVUID computation must do as the JVM does and ignore access
		// bits in the class file in favor of the access bits of the InnerClass attribute.
		if ((!string.ReferenceEquals(_name, null)) && _name.Equals(innerClassName))
		{
		  this._access = innerClassAccess;
		}
		base.VisitInnerClass(innerClassName, outerName, innerName, innerClassAccess);
	  }

	  public override void VisitEnd()
	  {
		// Add the SVUID field to the class if it doesn't have one.
		if (_computeSvuid && !_hasSvuid)
		{
		  try
		  {
			AddSvuid(ComputeSvuid());
		  }
		  catch (IOException e)
		  {
			throw new System.InvalidOperationException("Error while computing SVUID for " + _name, e);
		  }
		}

		base.VisitEnd();
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
	  public virtual bool HasSvuid()
	  {
		return _hasSvuid;
	  }

	  /// <summary>
	  /// Adds a final static serialVersionUID field to the class, with the given value.
	  /// </summary>
	  /// <param name="svuid"> the serialVersionUID field value. </param>
	  // DontCheck(AbbreviationAsWordInName): can't be renamed (for backward binary compatibility).
	  public virtual void AddSvuid(long svuid)
	  {
		var fieldVisitor = base.VisitField(IOpcodes.Acc_Final + IOpcodes.Acc_Static, "serialVersionUID", "J", null, svuid);
		if (fieldVisitor != null)
		{
		  fieldVisitor.VisitEnd();
		}
	  }

	  /// <summary>
	  /// Computes and returns the value of SVUID.
	  /// </summary>
	  /// <returns> the serial version UID. </returns>
	  /// <exception cref="IOException"> if an I/O error occurs. </exception>
	  // DontCheck(AbbreviationAsWordInName): can't be renamed (for backward binary compatibility).
	  public virtual long ComputeSvuid()
	  {
		long svuid = 0;

		using (var byteArrayOutputStream = new MemoryStream()) 
        using(var dataOutputStream = new DataOutputStream(byteArrayOutputStream))
		{

		  // 1. The class name written using UTF encoding.
		  dataOutputStream.WriteUtf(_name.Replace('/', '.'));

		  // 2. The class modifiers written as a 32-bit integer.
		  var mods = _access;
		  if ((mods & IOpcodes.Acc_Interface) != 0)
		  {
			mods = _svuidMethods.Count == 0 ? (mods & ~IOpcodes.Acc_Abstract) : (mods | IOpcodes.Acc_Abstract);
		  }
		  dataOutputStream.WriteInt(mods & (IOpcodes.Acc_Public | IOpcodes.Acc_Final | IOpcodes.Acc_Interface | IOpcodes.Acc_Abstract));

		  // 3. The name of each interface sorted by name written using UTF encoding.
		  Array.Sort(_interfaces);
		  foreach (var interfaceName in _interfaces)
		  {
			dataOutputStream.WriteUtf(interfaceName.Replace('/', '.'));
		  }

		  // 4. For each field of the class sorted by field name (except private static and private
		  // transient fields):
		  //   1. The name of the field in UTF encoding.
		  //   2. The modifiers of the field written as a 32-bit integer.
		  //   3. The descriptor of the field in UTF encoding.
		  // Note that field signatures are not dot separated. Method and constructor signatures are dot
		  // separated. Go figure...
		  WriteItems(_svuidFields, dataOutputStream, false);

		  // 5. If a class initializer exists, write out the following:
		  //   1. The name of the method, <clinit>, in UTF encoding.
		  //   2. The modifier of the method, ACC_STATIC, written as a 32-bit integer.
		  //   3. The descriptor of the method, ()V, in UTF encoding.
		  if (_hasStaticInitializer)
		  {
			dataOutputStream.WriteUtf(Clinit);
			dataOutputStream.WriteInt(IOpcodes.Acc_Static);
			dataOutputStream.WriteUtf("()V");
		  }

		  // 6. For each non-private constructor sorted by method name and signature:
		  //   1. The name of the method, <init>, in UTF encoding.
		  //   2. The modifiers of the method written as a 32-bit integer.
		  //   3. The descriptor of the method in UTF encoding.
		  WriteItems(_svuidConstructors, dataOutputStream, true);

		  // 7. For each non-private method sorted by method name and signature:
		  //   1. The name of the method in UTF encoding.
		  //   2. The modifiers of the method written as a 32-bit integer.
		  //   3. The descriptor of the method in UTF encoding.
		  WriteItems(_svuidMethods, dataOutputStream, true);

		  dataOutputStream.Flush();

		  // 8. The SHA-1 algorithm is executed on the stream of bytes produced by DataOutputStream and
		  // produces five 32-bit values sha[0..4].
		  var hashBytes = ComputeShAdigest(byteArrayOutputStream.ToArray());

		  // 9. The hash value is assembled from the first and second 32-bit values of the SHA-1 message
		  // digest. If the result of the message digest, the five 32-bit words H0 H1 H2 H3 H4, is in an
		  // array of five int values named sha, the hash value would be computed as follows:
		  for (var i = Math.Min(hashBytes.Length, 8) - 1; i >= 0; i--)
		  {
			svuid = (svuid << 8) | (uint)(hashBytes[i] & 0xFF);
		  }
		}

		return svuid;
	  }

      private SHA1 _sha1 = SHA1.Create();
	  /// <summary>
	  /// Returns the SHA-1 message digest of the given value.
	  /// </summary>
	  /// <param name="value"> the value whose SHA message digest must be computed. </param>
	  /// <returns> the SHA-1 message digest of the given value. </returns>
	  // DontCheck(AbbreviationAsWordInName): can't be renamed (for backward binary compatibility).
	  public virtual byte[] ComputeShAdigest(byte[] value)
      {
          return _sha1.ComputeHash(value);
      }

	  /// <summary>
	  /// Sorts the items in the collection and writes it to the given output stream.
	  /// </summary>
	  /// <param name="itemCollection"> a collection of items. </param>
	  /// <param name="dataOutputStream"> where the items must be written. </param>
	  /// <param name="dotted"> whether package names must use dots, instead of slashes. </param>
	  /// <exception cref="IOException"> if an error occurs. </exception>
	  private static void WriteItems(ICollection<Item> itemCollection, IDataOutput dataOutputStream, bool dotted)
	  {
		var items = itemCollection.ToArray();
		Array.Sort(items);
		foreach (var item in items)
		{
		  dataOutputStream.WriteUtf(item.name);
		  dataOutputStream.WriteInt(item.access);
		  dataOutputStream.WriteUtf(dotted ? item.descriptor.Replace('/', '.') : item.descriptor);
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
		  var result = string.CompareOrdinal(name, item.name);
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