

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
	/// An <seealso cref="AnnotationVisitor"/> that generates a corresponding 'annotation' or 'type_annotation'
	/// structure, as defined in the Java Virtual Machine Specification (JVMS). AnnotationWriter
	/// instances can be chained in a doubly linked list, from which Runtime[In]Visible[Type]Annotations
	/// attributes can be generated with the <seealso cref="PutAnnotations"/> method. Similarly, arrays of such
	/// lists can be used to generate Runtime[In]VisibleParameterAnnotations attributes.
	/// </summary>
	/// <seealso cref= <a href="https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.7.16">JVMS
	///     4.7.16</a> </seealso>
	/// <seealso cref= <a href="https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.7.20">JVMS
	///     4.7.20</a>
	/// @author Eric Bruneton
	/// @author Eugene Kuleshov </seealso>
	internal sealed class AnnotationWriter : AnnotationVisitor
	{

	  /// <summary>
	  /// Where the constants used in this AnnotationWriter must be stored. </summary>
	  private readonly SymbolTable _symbolTable;

	  /// <summary>
	  /// Whether Values are named or not. AnnotationWriter instances used for annotation default and
	  /// annotation arrays use unnamed Values (i.e. they generate an 'element_value' structure for each
	  /// value, instead of an element_name_index followed by an element_value).
	  /// </summary>
	  private readonly bool _useNamedValues;

	  /// <summary>
	  /// The 'annotation' or 'type_annotation' JVMS structure corresponding to the annotation Values
	  /// visited so far. All the fields of these structures, except the last one - the
	  /// element_value_pairs array, must be set before this ByteVector is passed to the constructor
	  /// (num_element_value_pairs can be set to 0, it is reset to the correct value in {@link
	  /// #visitEnd()}). The element_value_pairs array is filled incrementally in the various visit()
	  /// methods.
	  /// 
	  /// <para>Note: as an exception to the above rules, for AnnotationDefault attributes (which contain a
	  /// single element_value by definition), this ByteVector is initially empty when passed to the
	  /// constructor, and <seealso cref="_numElementValuePairsOffset"/> is set to -1.
	  /// </para>
	  /// </summary>
	  private readonly ByteVector _annotation;

	  /// <summary>
	  /// The offset in <seealso cref="_annotation"/> where <seealso cref="_numElementValuePairs"/> must be stored (or -1 for
	  /// the case of AnnotationDefault attributes).
	  /// </summary>
	  private readonly int _numElementValuePairsOffset;

	  /// <summary>
	  /// The number of element value pairs visited so far. </summary>
	  private int _numElementValuePairs;

	  /// <summary>
	  /// The previous AnnotationWriter. This field is used to store the list of annotations of a
	  /// Runtime[In]Visible[Type]Annotations attribute. It is unused for nested or array annotations
	  /// (annotation Values of annotation type), or for AnnotationDefault attributes.
	  /// </summary>
	  private readonly AnnotationWriter _previousAnnotation;

	  /// <summary>
	  /// The next AnnotationWriter. This field is used to store the list of annotations of a
	  /// Runtime[In]Visible[Type]Annotations attribute. It is unused for nested or array annotations
	  /// (annotation Values of annotation type), or for AnnotationDefault attributes.
	  /// </summary>
	  private AnnotationWriter _nextAnnotation;

	  // -----------------------------------------------------------------------------------------------
	  // Constructors and factories
	  // -----------------------------------------------------------------------------------------------

	  /// <summary>
	  /// Constructs a new <seealso cref="AnnotationWriter"/>.
	  /// </summary>
	  /// <param name="symbolTable"> where the constants used in this AnnotationWriter must be stored. </param>
	  /// <param name="useNamedValues"> whether Values are named or not. AnnotationDefault and annotation arrays
	  ///     use unnamed Values. </param>
	  /// <param name="annotation"> where the 'annotation' or 'type_annotation' JVMS structure corresponding to
	  ///     the visited content must be stored. This ByteVector must already contain all the fields of
	  ///     the structure except the last one (the element_value_pairs array). </param>
	  /// <param name="previousAnnotation"> the previously visited annotation of the
	  ///     Runtime[In]Visible[Type]Annotations attribute to which this annotation belongs, or
	  ///     {@literal null} in other cases (e.g. nested or array annotations). </param>
	  public AnnotationWriter(SymbolTable symbolTable, bool useNamedValues, ByteVector annotation, AnnotationWriter previousAnnotation) : base(IOpcodes.Asm9)
	  {
		this._symbolTable = symbolTable;
		this._useNamedValues = useNamedValues;
		this._annotation = annotation;
		// By hypothesis, num_element_value_pairs is stored in the last unsigned short of 'annotation'.
		this._numElementValuePairsOffset = annotation.length == 0 ? -1 : annotation.length - 2;
		this._previousAnnotation = previousAnnotation;
		if (previousAnnotation != null)
		{
		  previousAnnotation._nextAnnotation = this;
		}
	  }

	  /// <summary>
	  /// Creates a new <seealso cref="AnnotationWriter"/> using named Values.
	  /// </summary>
	  /// <param name="symbolTable"> where the constants used in this AnnotationWriter must be stored. </param>
	  /// <param name="descriptor"> the class descriptor of the annotation class. </param>
	  /// <param name="previousAnnotation"> the previously visited annotation of the
	  ///     Runtime[In]Visible[Type]Annotations attribute to which this annotation belongs, or
	  ///     {@literal null} in other cases (e.g. nested or array annotations). </param>
	  /// <returns> a new <seealso cref="AnnotationWriter"/> for the given annotation descriptor. </returns>
	  internal static AnnotationWriter Create(SymbolTable symbolTable, string descriptor, AnnotationWriter previousAnnotation)
	  {
		// Create a ByteVector to hold an 'annotation' JVMS structure.
		// See https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.7.16.
		var annotation = new ByteVector();
		// Write type_index and reserve space for num_element_value_pairs.
		annotation.PutShort(symbolTable.AddConstantUtf8(descriptor)).PutShort(0);
		return new AnnotationWriter(symbolTable, true, annotation, previousAnnotation);
	  }

	  /// <summary>
	  /// Creates a new <seealso cref="AnnotationWriter"/> using named Values.
	  /// </summary>
	  /// <param name="symbolTable"> where the constants used in this AnnotationWriter must be stored. </param>
	  /// <param name="typeRef"> a reference to the annotated type. The sort of this type reference must be
	  ///     <seealso cref="TypeReference.Class_Type_Parameter"/>, {@link
	  ///     TypeReference#CLASS_TYPE_PARAMETER_BOUND} or <seealso cref="TypeReference.Class_Extends"/>. See
	  ///     <seealso cref="TypeReference"/>. </param>
	  /// <param name="typePath"> the path to the annotated type argument, wildcard bound, array element type, or
	  ///     static inner type within 'typeRef'. May be {@literal null} if the annotation targets
	  ///     'typeRef' as a whole. </param>
	  /// <param name="descriptor"> the class descriptor of the annotation class. </param>
	  /// <param name="previousAnnotation"> the previously visited annotation of the
	  ///     Runtime[In]Visible[Type]Annotations attribute to which this annotation belongs, or
	  ///     {@literal null} in other cases (e.g. nested or array annotations). </param>
	  /// <returns> a new <seealso cref="AnnotationWriter"/> for the given type annotation reference and descriptor. </returns>
	  internal static AnnotationWriter Create(SymbolTable symbolTable, int typeRef, TypePath typePath, string descriptor, AnnotationWriter previousAnnotation)
	  {
		// Create a ByteVector to hold a 'type_annotation' JVMS structure.
		// See https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.7.20.
		var typeAnnotation = new ByteVector();
		// Write target_type, target_info, and target_path.
		TypeReference.PutTarget(typeRef, typeAnnotation);
		TypePath.Put(typePath, typeAnnotation);
		// Write type_index and reserve space for num_element_value_pairs.
		typeAnnotation.PutShort(symbolTable.AddConstantUtf8(descriptor)).PutShort(0);
		return new AnnotationWriter(symbolTable, true, typeAnnotation, previousAnnotation);
	  }

	  // -----------------------------------------------------------------------------------------------
	  // Implementation of the AnnotationVisitor abstract class
	  // -----------------------------------------------------------------------------------------------

	  public override void Visit(string name, object value)
	  {
		// Case of an element_value with a const_value_index, class_info_index or array_index field.
		// See https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.7.16.1.
		++_numElementValuePairs;
		if (_useNamedValues)
		{
		  _annotation.PutShort(_symbolTable.AddConstantUtf8(name));
		}
		if (value is string)
		{
		  _annotation.Put12('s', _symbolTable.AddConstantUtf8((string) value));
		}
		else if (value is byte)
		{
		  _annotation.Put12('B', _symbolTable.AddConstantInteger(((sbyte?) value).Value).index);
		}
		else if (value is bool)
		{
		  var booleanValue = ((bool?) value).Value ? 1 : 0;
		  _annotation.Put12('Z', _symbolTable.AddConstantInteger(booleanValue).index);
		}
		else if (value is char)
		{
		  _annotation.Put12('C', _symbolTable.AddConstantInteger(((char?) value).Value).index);
		}
		else if (value is sbyte)
		{
		  _annotation.Put12('S', _symbolTable.AddConstantInteger(((short?) value).Value).index);
		}
		else if (value is JType)
		{
		  _annotation.Put12('c', _symbolTable.AddConstantUtf8(((JType) value).Descriptor));
		}
		else if (value is byte[] || value is byte[])
		{
		  var byteArray = (byte[]) value;
		  _annotation.Put12('[', byteArray.Length);
		  foreach (sbyte byteValue in byteArray)
		  {
			_annotation.Put12('B', _symbolTable.AddConstantInteger(byteValue).index);
		  }
		}
		else if (value is bool[])
		{
		  var booleanArray = (bool[]) value;
		  _annotation.Put12('[', booleanArray.Length);
		  foreach (var booleanValue in booleanArray)
		  {
			_annotation.Put12('Z', _symbolTable.AddConstantInteger(booleanValue ? 1 : 0).index);
		  }
		}
		else if (value is short[])
		{
		  var shortArray = (short[]) value;
		  _annotation.Put12('[', shortArray.Length);
		  foreach (var shortValue in shortArray)
		  {
			_annotation.Put12('S', _symbolTable.AddConstantInteger(shortValue).index);
		  }
		}
		else if (value is char[])
		{
		  var charArray = (char[]) value;
		  _annotation.Put12('[', charArray.Length);
		  foreach (var charValue in charArray)
		  {
			_annotation.Put12('C', _symbolTable.AddConstantInteger(charValue).index);
		  }
		}
		else if (value is int[])
		{
		  var intArray = (int[]) value;
		  _annotation.Put12('[', intArray.Length);
		  foreach (var intValue in intArray)
		  {
			_annotation.Put12('I', _symbolTable.AddConstantInteger(intValue).index);
		  }
		}
		else if (value is long[])
		{
		  var longArray = (long[]) value;
		  _annotation.Put12('[', longArray.Length);
		  foreach (var longValue in longArray)
		  {
			_annotation.Put12('J', _symbolTable.AddConstantLong(longValue).index);
		  }
		}
		else if (value is float[])
		{
		  var floatArray = (float[]) value;
		  _annotation.Put12('[', floatArray.Length);
		  foreach (var floatValue in floatArray)
		  {
			_annotation.Put12('F', _symbolTable.AddConstantFloat(floatValue).index);
		  }
		}
		else if (value is double[])
		{
		  var doubleArray = (double[]) value;
		  _annotation.Put12('[', doubleArray.Length);
		  foreach (var doubleValue in doubleArray)
		  {
			_annotation.Put12('D', _symbolTable.AddConstantDouble(doubleValue).index);
		  }
		}
		else
		{
		  var symbol = _symbolTable.AddConstant(value);
		  _annotation.Put12(".s.IFJDCS"[symbol.tag], symbol.index);
		}
	  }

	  public override void VisitEnum(string name, string descriptor, string value)
	  {
		// Case of an element_value with an enum_const_value field.
		// See https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.7.16.1.
		++_numElementValuePairs;
		if (_useNamedValues)
		{
		  _annotation.PutShort(_symbolTable.AddConstantUtf8(name));
		}
		_annotation.Put12('e', _symbolTable.AddConstantUtf8(descriptor)).PutShort(_symbolTable.AddConstantUtf8(value));
	  }

	  public override AnnotationVisitor VisitAnnotation(string name, string descriptor)
	  {
		// Case of an element_value with an annotation_value field.
		// See https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.7.16.1.
		++_numElementValuePairs;
		if (_useNamedValues)
		{
		  _annotation.PutShort(_symbolTable.AddConstantUtf8(name));
		}
		// Write tag and type_index, and reserve 2 bytes for num_element_value_pairs.
		_annotation.Put12('@', _symbolTable.AddConstantUtf8(descriptor)).PutShort(0);
		return new AnnotationWriter(_symbolTable, true, _annotation, null);
	  }

	  public override AnnotationVisitor VisitArray(string name)
	  {
		// Case of an element_value with an array_value field.
		// https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.7.16.1
		++_numElementValuePairs;
		if (_useNamedValues)
		{
		  _annotation.PutShort(_symbolTable.AddConstantUtf8(name));
		}
		// Write tag, and reserve 2 bytes for num_values. Here we take advantage of the fact that the
		// end of an element_value of array type is similar to the end of an 'annotation' structure: an
		// unsigned short num_values followed by num_values element_value, versus an unsigned short
		// num_element_value_pairs, followed by num_element_value_pairs { element_name_index,
		// element_value } tuples. This allows us to use an AnnotationWriter with unnamed Values to
		// visit the array elements. Its num_element_value_pairs will correspond to the number of array
		// elements and will be stored in what is in fact num_values.
		_annotation.Put12('[', 0);
		return new AnnotationWriter(_symbolTable, false, _annotation, null);
	  }

	  public override void VisitEnd()
	  {
		if (_numElementValuePairsOffset != -1)
		{
		  var data = _annotation.data;
		  data[_numElementValuePairsOffset] = (byte)((int)((uint)_numElementValuePairs >> 8));
		  data[_numElementValuePairsOffset + 1] = (byte) _numElementValuePairs;
		}
	  }

	  // -----------------------------------------------------------------------------------------------
	  // Utility methods
	  // -----------------------------------------------------------------------------------------------

	  /// <summary>
	  /// Returns the size of a Runtime[In]Visible[Type]Annotations attribute containing this annotation
	  /// and all its <i>predecessors</i> (see <seealso cref="_previousAnnotation"/>. Also adds the attribute name
	  /// to the constant pool of the class (if not null).
	  /// </summary>
	  /// <param name="attributeName"> one of "Runtime[In]Visible[Type]Annotations", or {@literal null}. </param>
	  /// <returns> the size in bytes of a Runtime[In]Visible[Type]Annotations attribute containing this
	  ///     annotation and all its predecessors. This includes the size of the attribute_name_index and
	  ///     attribute_length fields. </returns>
	  public int ComputeAnnotationsSize(string attributeName)
	  {
		if (!string.ReferenceEquals(attributeName, null))
		{
		  _symbolTable.AddConstantUtf8(attributeName);
		}
		// The attribute_name_index, attribute_length and num_annotations fields use 8 bytes.
		var attributeSize = 8;
		var annotationWriter = this;
		while (annotationWriter != null)
		{
		  attributeSize += annotationWriter._annotation.length;
		  annotationWriter = annotationWriter._previousAnnotation;
		}
		return attributeSize;
	  }

	  /// <summary>
	  /// Returns the size of the Runtime[In]Visible[Type]Annotations attributes containing the given
	  /// annotations and all their <i>predecessors</i> (see <seealso cref="_previousAnnotation"/>. Also adds the
	  /// attribute names to the constant pool of the class (if not null).
	  /// </summary>
	  /// <param name="lastRuntimeVisibleAnnotation"> The last runtime visible annotation of a field, method or
	  ///     class. The previous ones can be accessed with the <seealso cref="_previousAnnotation"/> field. May be
	  ///     {@literal null}. </param>
	  /// <param name="lastRuntimeInvisibleAnnotation"> The last runtime invisible annotation of this a field,
	  ///     method or class. The previous ones can be accessed with the <seealso cref="_previousAnnotation"/>
	  ///     field. May be {@literal null}. </param>
	  /// <param name="lastRuntimeVisibleTypeAnnotation"> The last runtime visible type annotation of this a
	  ///     field, method or class. The previous ones can be accessed with the {@link
	  ///     #previousAnnotation} field. May be {@literal null}. </param>
	  /// <param name="lastRuntimeInvisibleTypeAnnotation"> The last runtime invisible type annotation of a
	  ///     field, method or class field. The previous ones can be accessed with the {@link
	  ///     #previousAnnotation} field. May be {@literal null}. </param>
	  /// <returns> the size in bytes of a Runtime[In]Visible[Type]Annotations attribute containing the
	  ///     given annotations and all their predecessors. This includes the size of the
	  ///     attribute_name_index and attribute_length fields. </returns>
	  internal static int ComputeAnnotationsSize(AnnotationWriter lastRuntimeVisibleAnnotation, AnnotationWriter lastRuntimeInvisibleAnnotation, AnnotationWriter lastRuntimeVisibleTypeAnnotation, AnnotationWriter lastRuntimeInvisibleTypeAnnotation)
	  {
		var size = 0;
		if (lastRuntimeVisibleAnnotation != null)
		{
		  size += lastRuntimeVisibleAnnotation.ComputeAnnotationsSize(Constants.Runtime_Visible_Annotations);
		}
		if (lastRuntimeInvisibleAnnotation != null)
		{
		  size += lastRuntimeInvisibleAnnotation.ComputeAnnotationsSize(Constants.Runtime_Invisible_Annotations);
		}
		if (lastRuntimeVisibleTypeAnnotation != null)
		{
		  size += lastRuntimeVisibleTypeAnnotation.ComputeAnnotationsSize(Constants.Runtime_Visible_Type_Annotations);
		}
		if (lastRuntimeInvisibleTypeAnnotation != null)
		{
		  size += lastRuntimeInvisibleTypeAnnotation.ComputeAnnotationsSize(Constants.Runtime_Invisible_Type_Annotations);
		}
		return size;
	  }

	  /// <summary>
	  /// Puts a Runtime[In]Visible[Type]Annotations attribute containing this annotations and all its
	  /// <i>predecessors</i> (see <seealso cref="_previousAnnotation"/> in the given ByteVector. Annotations are
	  /// put in the same order they have been visited.
	  /// </summary>
	  /// <param name="attributeNameIndex"> the constant pool index of the attribute name (one of
	  ///     "Runtime[In]Visible[Type]Annotations"). </param>
	  /// <param name="output"> where the attribute must be put. </param>
	  public void PutAnnotations(int attributeNameIndex, ByteVector output)
	  {
		var attributeLength = 2; // For num_annotations.
		var numAnnotations = 0;
		var annotationWriter = this;
		AnnotationWriter firstAnnotation = null;
		while (annotationWriter != null)
		{
		  // In case the user forgot to call visitEnd().
		  annotationWriter.VisitEnd();
		  attributeLength += annotationWriter._annotation.length;
		  numAnnotations++;
		  firstAnnotation = annotationWriter;
		  annotationWriter = annotationWriter._previousAnnotation;
		}
		output.PutShort(attributeNameIndex);
		output.PutInt(attributeLength);
		output.PutShort(numAnnotations);
		annotationWriter = firstAnnotation;
		while (annotationWriter != null)
		{
		  output.PutByteArray(annotationWriter._annotation.data, 0, annotationWriter._annotation.length);
		  annotationWriter = annotationWriter._nextAnnotation;
		}
	  }

	  /// <summary>
	  /// Puts the Runtime[In]Visible[Type]Annotations attributes containing the given annotations and
	  /// all their <i>predecessors</i> (see <seealso cref="_previousAnnotation"/> in the given ByteVector.
	  /// Annotations are put in the same order they have been visited.
	  /// </summary>
	  /// <param name="symbolTable"> where the constants used in the AnnotationWriter instances are stored. </param>
	  /// <param name="lastRuntimeVisibleAnnotation"> The last runtime visible annotation of a field, method or
	  ///     class. The previous ones can be accessed with the <seealso cref="_previousAnnotation"/> field. May be
	  ///     {@literal null}. </param>
	  /// <param name="lastRuntimeInvisibleAnnotation"> The last runtime invisible annotation of this a field,
	  ///     method or class. The previous ones can be accessed with the <seealso cref="_previousAnnotation"/>
	  ///     field. May be {@literal null}. </param>
	  /// <param name="lastRuntimeVisibleTypeAnnotation"> The last runtime visible type annotation of this a
	  ///     field, method or class. The previous ones can be accessed with the {@link
	  ///     #previousAnnotation} field. May be {@literal null}. </param>
	  /// <param name="lastRuntimeInvisibleTypeAnnotation"> The last runtime invisible type annotation of a
	  ///     field, method or class field. The previous ones can be accessed with the {@link
	  ///     #previousAnnotation} field. May be {@literal null}. </param>
	  /// <param name="output"> where the attributes must be put. </param>
	  internal static void PutAnnotations(SymbolTable symbolTable, AnnotationWriter lastRuntimeVisibleAnnotation, AnnotationWriter lastRuntimeInvisibleAnnotation, AnnotationWriter lastRuntimeVisibleTypeAnnotation, AnnotationWriter lastRuntimeInvisibleTypeAnnotation, ByteVector output)
	  {
		if (lastRuntimeVisibleAnnotation != null)
		{
		  lastRuntimeVisibleAnnotation.PutAnnotations(symbolTable.AddConstantUtf8(Constants.Runtime_Visible_Annotations), output);
		}
		if (lastRuntimeInvisibleAnnotation != null)
		{
		  lastRuntimeInvisibleAnnotation.PutAnnotations(symbolTable.AddConstantUtf8(Constants.Runtime_Invisible_Annotations), output);
		}
		if (lastRuntimeVisibleTypeAnnotation != null)
		{
		  lastRuntimeVisibleTypeAnnotation.PutAnnotations(symbolTable.AddConstantUtf8(Constants.Runtime_Visible_Type_Annotations), output);
		}
		if (lastRuntimeInvisibleTypeAnnotation != null)
		{
		  lastRuntimeInvisibleTypeAnnotation.PutAnnotations(symbolTable.AddConstantUtf8(Constants.Runtime_Invisible_Type_Annotations), output);
		}
	  }

	  /// <summary>
	  /// Returns the size of a Runtime[In]VisibleParameterAnnotations attribute containing all the
	  /// annotation lists from the given AnnotationWriter sub-array. Also adds the attribute name to the
	  /// constant pool of the class.
	  /// </summary>
	  /// <param name="attributeName"> one of "Runtime[In]VisibleParameterAnnotations". </param>
	  /// <param name="annotationWriters"> an array of AnnotationWriter lists (designated by their <i>last</i>
	  ///     element). </param>
	  /// <param name="annotableParameterCount"> the number of elements in annotationWriters to take into account
	  ///     (elements [0..annotableParameterCount[ are taken into account). </param>
	  /// <returns> the size in bytes of a Runtime[In]VisibleParameterAnnotations attribute corresponding
	  ///     to the given sub-array of AnnotationWriter lists. This includes the size of the
	  ///     attribute_name_index and attribute_length fields. </returns>
	  internal static int ComputeParameterAnnotationsSize(string attributeName, AnnotationWriter[] annotationWriters, int annotableParameterCount)
	  {
		// Note: attributeName is added to the constant pool by the call to computeAnnotationsSize
		// below. This assumes that there is at least one non-null element in the annotationWriters
		// sub-array (which is ensured by the lazy instantiation of this array in MethodWriter).
		// The attribute_name_index, attribute_length and num_parameters fields use 7 bytes, and each
		// element of the parameter_annotations array uses 2 bytes for its num_annotations field.
		var attributeSize = 7 + 2 * annotableParameterCount;
		for (var i = 0; i < annotableParameterCount; ++i)
		{
		  var annotationWriter = annotationWriters[i];
		  attributeSize += annotationWriter == null ? 0 : annotationWriter.ComputeAnnotationsSize(attributeName) - 8;
		}
		return attributeSize;
	  }

	  /// <summary>
	  /// Puts a Runtime[In]VisibleParameterAnnotations attribute containing all the annotation lists
	  /// from the given AnnotationWriter sub-array in the given ByteVector.
	  /// </summary>
	  /// <param name="attributeNameIndex"> constant pool index of the attribute name (one of
	  ///     Runtime[In]VisibleParameterAnnotations). </param>
	  /// <param name="annotationWriters"> an array of AnnotationWriter lists (designated by their <i>last</i>
	  ///     element). </param>
	  /// <param name="annotableParameterCount"> the number of elements in annotationWriters to put (elements
	  ///     [0..annotableParameterCount[ are put). </param>
	  /// <param name="output"> where the attribute must be put. </param>
	  internal static void PutParameterAnnotations(int attributeNameIndex, AnnotationWriter[] annotationWriters, int annotableParameterCount, ByteVector output)
	  {
		// The num_parameters field uses 1 byte, and each element of the parameter_annotations array
		// uses 2 bytes for its num_annotations field.
		var attributeLength = 1 + 2 * annotableParameterCount;
		for (var i = 0; i < annotableParameterCount; ++i)
		{
		  var annotationWriter = annotationWriters[i];
		  attributeLength += annotationWriter == null ? 0 : annotationWriter.ComputeAnnotationsSize(null) - 8;
		}
		output.PutShort(attributeNameIndex);
		output.PutInt(attributeLength);
		output.PutByte(annotableParameterCount);
		for (var i = 0; i < annotableParameterCount; ++i)
		{
		  var annotationWriter = annotationWriters[i];
		  AnnotationWriter firstAnnotation = null;
		  var numAnnotations = 0;
		  while (annotationWriter != null)
		  {
			// In case user the forgot to call visitEnd().
			annotationWriter.VisitEnd();
			numAnnotations++;
			firstAnnotation = annotationWriter;
			annotationWriter = annotationWriter._previousAnnotation;
		  }
		  output.PutShort(numAnnotations);
		  annotationWriter = firstAnnotation;
		  while (annotationWriter != null)
		  {
			output.PutByteArray(annotationWriter._annotation.data, 0, annotationWriter._annotation.length);
			annotationWriter = annotationWriter._nextAnnotation;
		  }
		}
	  }
	}

}