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
    ///     A <seealso cref="ModuleVisitor" /> that generates the corresponding Module, ModulePackages and
    ///     ModuleMainClass attributes, as defined in the Java Virtual Machine Specification (JVMS).
    /// </summary>
    /// <seealso cref=
    /// <a href="https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.7.25">
    ///     JVMS
    ///     4.7.25
    /// </a>
    /// </seealso>
    /// <seealso cref=
    /// <a href="https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.7.26">
    ///     JVMS
    ///     4.7.26
    /// </a>
    /// </seealso>
    /// <seealso cref=
    /// <a href="https://docs.oracle.com/javase/specs/jvms/se9/html/jvms-4.html#jvms-4.7.27">
    ///     JVMS
    ///     4.7.27
    /// </a>
    /// @author Remi Forax
    /// @author Eric Bruneton
    /// </seealso>
    internal sealed class ModuleWriter : ModuleVisitor
    {
        /// <summary>
        ///     The binary content of the 'exports' array of the JVMS Module attribute.
        /// </summary>
        private readonly ByteVector _exports;

        /// <summary>
        ///     The module_flags field of the JVMS Module attribute.
        /// </summary>
        private readonly int _moduleFlags;

        /// <summary>
        ///     The module_name_index field of the JVMS Module attribute.
        /// </summary>
        private readonly int _moduleNameIndex;

        /// <summary>
        ///     The module_version_index field of the JVMS Module attribute.
        /// </summary>
        private readonly int _moduleVersionIndex;

        /// <summary>
        ///     The binary content of the 'opens' array of the JVMS Module attribute.
        /// </summary>
        private readonly ByteVector _opens;

        /// <summary>
        ///     The binary content of the 'package_index' array of the JVMS ModulePackages attribute.
        /// </summary>
        private readonly ByteVector _packageIndex;

        /// <summary>
        ///     The binary content of the 'provides' array of the JVMS Module attribute.
        /// </summary>
        private readonly ByteVector _provides;

        /// <summary>
        ///     The binary content of the 'requires' array of the JVMS Module attribute.
        /// </summary>
        private readonly ByteVector _requires;

        /// <summary>
        ///     Where the constants used in this AnnotationWriter must be stored.
        /// </summary>
        private readonly SymbolTable _symbolTable;

        /// <summary>
        ///     The binary content of the 'uses_index' array of the JVMS Module attribute.
        /// </summary>
        private readonly ByteVector _usesIndex;

        /// <summary>
        ///     The exports_count field of the JVMS Module attribute.
        /// </summary>
        private int _exportsCount;

        /// <summary>
        ///     The main_class_index field of the JVMS ModuleMainClass attribute, or 0.
        /// </summary>
        private int _mainClassIndex;

        /// <summary>
        ///     The opens_count field of the JVMS Module attribute.
        /// </summary>
        private int _opensCount;

        /// <summary>
        ///     The provides_count field of the JVMS ModulePackages attribute.
        /// </summary>
        private int _packageCount;

        /// <summary>
        ///     The provides_count field of the JVMS Module attribute.
        /// </summary>
        private int _providesCount;

        /// <summary>
        ///     The requires_count field of the JVMS Module attribute.
        /// </summary>
        private int _requiresCount;

        /// <summary>
        ///     The uses_count field of the JVMS Module attribute.
        /// </summary>
        private int _usesCount;

        public ModuleWriter(SymbolTable symbolTable, int name, int access, int version) : base(IOpcodes.Asm9)
        {
            this._symbolTable = symbolTable;
            _moduleNameIndex = name;
            _moduleFlags = access;
            _moduleVersionIndex = version;
            _requires = new ByteVector();
            _exports = new ByteVector();
            _opens = new ByteVector();
            _usesIndex = new ByteVector();
            _provides = new ByteVector();
            _packageIndex = new ByteVector();
        }

        /// <summary>
        ///     Returns the number of Module, ModulePackages and ModuleMainClass attributes generated by this
        ///     ModuleWriter.
        /// </summary>
        /// <returns> the number of Module, ModulePackages and ModuleMainClass attributes (between 1 and 3). </returns>
        public int AttributeCount => 1 + (_packageCount > 0 ? 1 : 0) + (_mainClassIndex > 0 ? 1 : 0);

        public override void VisitMainClass(string mainClass)
        {
            _mainClassIndex = _symbolTable.AddConstantClass(mainClass).index;
        }

        public override void VisitPackage(string packaze)
        {
            _packageIndex.PutShort(_symbolTable.AddConstantPackage(packaze).index);
            _packageCount++;
        }

        public override void VisitRequire(string module, int access, string version)
        {
            _requires.PutShort(_symbolTable.AddConstantModule(module).index).PutShort(access)
                .PutShort(ReferenceEquals(version, null) ? 0 : _symbolTable.AddConstantUtf8(version));
            _requiresCount++;
        }

        public override void VisitExport(string packaze, int access, params string[] modules)
        {
            _exports.PutShort(_symbolTable.AddConstantPackage(packaze).index).PutShort(access);
            if (modules == null)
            {
                _exports.PutShort(0);
            }
            else
            {
                _exports.PutShort(modules.Length);
                foreach (var module in modules) _exports.PutShort(_symbolTable.AddConstantModule(module).index);
            }

            _exportsCount++;
        }

        public override void VisitOpen(string packaze, int access, params string[] modules)
        {
            _opens.PutShort(_symbolTable.AddConstantPackage(packaze).index).PutShort(access);
            if (modules == null)
            {
                _opens.PutShort(0);
            }
            else
            {
                _opens.PutShort(modules.Length);
                foreach (var module in modules) _opens.PutShort(_symbolTable.AddConstantModule(module).index);
            }

            _opensCount++;
        }

        public override void VisitUse(string service)
        {
            _usesIndex.PutShort(_symbolTable.AddConstantClass(service).index);
            _usesCount++;
        }

        public override void VisitProvide(string service, params string[] providers)
        {
            _provides.PutShort(_symbolTable.AddConstantClass(service).index);
            _provides.PutShort(providers.Length);
            foreach (var provider in providers) _provides.PutShort(_symbolTable.AddConstantClass(provider).index);
            _providesCount++;
        }

        public override void VisitEnd()
        {
            // Nothing to do.
        }

        /// <summary>
        ///     Returns the size of the Module, ModulePackages and ModuleMainClass attributes generated by this
        ///     ModuleWriter. Also add the names of these attributes in the constant pool.
        /// </summary>
        /// <returns> the size in bytes of the Module, ModulePackages and ModuleMainClass attributes. </returns>
        public int ComputeAttributesSize()
        {
            _symbolTable.AddConstantUtf8(Constants.Module);
            // 6 attribute header bytes, 6 bytes for name, flags and version, and 5 * 2 bytes for counts.
            var size = 22 + _requires.length + _exports.length + _opens.length + _usesIndex.length + _provides.length;
            if (_packageCount > 0)
            {
                _symbolTable.AddConstantUtf8(Constants.Module_Packages);
                // 6 attribute header bytes, and 2 bytes for package_count.
                size += 8 + _packageIndex.length;
            }

            if (_mainClassIndex > 0)
            {
                _symbolTable.AddConstantUtf8(Constants.Module_Main_Class);
                // 6 attribute header bytes, and 2 bytes for main_class_index.
                size += 8;
            }

            return size;
        }

        /// <summary>
        ///     Puts the Module, ModulePackages and ModuleMainClass attributes generated by this ModuleWriter
        ///     in the given ByteVector.
        /// </summary>
        /// <param name="output"> where the attributes must be put. </param>
        public void PutAttributes(ByteVector output)
        {
            // 6 bytes for name, flags and version, and 5 * 2 bytes for counts.
            var moduleAttributeLength = 16 + _requires.length + _exports.length + _opens.length + _usesIndex.length +
                                        _provides.length;
            output.PutShort(_symbolTable.AddConstantUtf8(Constants.Module)).PutInt(moduleAttributeLength)
                .PutShort(_moduleNameIndex).PutShort(_moduleFlags).PutShort(_moduleVersionIndex).PutShort(_requiresCount)
                .PutByteArray(_requires.data, 0, _requires.length).PutShort(_exportsCount)
                .PutByteArray(_exports.data, 0, _exports.length).PutShort(_opensCount)
                .PutByteArray(_opens.data, 0, _opens.length).PutShort(_usesCount)
                .PutByteArray(_usesIndex.data, 0, _usesIndex.length).PutShort(_providesCount)
                .PutByteArray(_provides.data, 0, _provides.length);
            if (_packageCount > 0)
                output.PutShort(_symbolTable.AddConstantUtf8(Constants.Module_Packages)).PutInt(2 + _packageIndex.length)
                    .PutShort(_packageCount).PutByteArray(_packageIndex.data, 0, _packageIndex.length);
            if (_mainClassIndex > 0)
                output.PutShort(_symbolTable.AddConstantUtf8(Constants.Module_Main_Class)).PutInt(2)
                    .PutShort(_mainClassIndex);
        }
    }
}