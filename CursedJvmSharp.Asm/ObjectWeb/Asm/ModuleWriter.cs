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
        private readonly ByteVector exports;

        /// <summary>
        ///     The module_flags field of the JVMS Module attribute.
        /// </summary>
        private readonly int moduleFlags;

        /// <summary>
        ///     The module_name_index field of the JVMS Module attribute.
        /// </summary>
        private readonly int moduleNameIndex;

        /// <summary>
        ///     The module_version_index field of the JVMS Module attribute.
        /// </summary>
        private readonly int moduleVersionIndex;

        /// <summary>
        ///     The binary content of the 'opens' array of the JVMS Module attribute.
        /// </summary>
        private readonly ByteVector opens;

        /// <summary>
        ///     The binary content of the 'package_index' array of the JVMS ModulePackages attribute.
        /// </summary>
        private readonly ByteVector packageIndex;

        /// <summary>
        ///     The binary content of the 'provides' array of the JVMS Module attribute.
        /// </summary>
        private readonly ByteVector provides;

        /// <summary>
        ///     The binary content of the 'requires' array of the JVMS Module attribute.
        /// </summary>
        private readonly ByteVector requires;

        /// <summary>
        ///     Where the constants used in this AnnotationWriter must be stored.
        /// </summary>
        private readonly SymbolTable symbolTable;

        /// <summary>
        ///     The binary content of the 'uses_index' array of the JVMS Module attribute.
        /// </summary>
        private readonly ByteVector usesIndex;

        /// <summary>
        ///     The exports_count field of the JVMS Module attribute.
        /// </summary>
        private int exportsCount;

        /// <summary>
        ///     The main_class_index field of the JVMS ModuleMainClass attribute, or 0.
        /// </summary>
        private int mainClassIndex;

        /// <summary>
        ///     The opens_count field of the JVMS Module attribute.
        /// </summary>
        private int opensCount;

        /// <summary>
        ///     The provides_count field of the JVMS ModulePackages attribute.
        /// </summary>
        private int packageCount;

        /// <summary>
        ///     The provides_count field of the JVMS Module attribute.
        /// </summary>
        private int providesCount;

        /// <summary>
        ///     The requires_count field of the JVMS Module attribute.
        /// </summary>
        private int requiresCount;

        /// <summary>
        ///     The uses_count field of the JVMS Module attribute.
        /// </summary>
        private int usesCount;

        public ModuleWriter(SymbolTable symbolTable, int name, int access, int version) : base(Opcodes.ASM9)
        {
            this.symbolTable = symbolTable;
            moduleNameIndex = name;
            moduleFlags = access;
            moduleVersionIndex = version;
            requires = new ByteVector();
            exports = new ByteVector();
            opens = new ByteVector();
            usesIndex = new ByteVector();
            provides = new ByteVector();
            packageIndex = new ByteVector();
        }

        /// <summary>
        ///     Returns the number of Module, ModulePackages and ModuleMainClass attributes generated by this
        ///     ModuleWriter.
        /// </summary>
        /// <returns> the number of Module, ModulePackages and ModuleMainClass attributes (between 1 and 3). </returns>
        public int AttributeCount => 1 + (packageCount > 0 ? 1 : 0) + (mainClassIndex > 0 ? 1 : 0);

        public override void visitMainClass(string mainClass)
        {
            mainClassIndex = symbolTable.addConstantClass(mainClass).index;
        }

        public override void visitPackage(string packaze)
        {
            packageIndex.putShort(symbolTable.addConstantPackage(packaze).index);
            packageCount++;
        }

        public override void visitRequire(string module, int access, string version)
        {
            requires.putShort(symbolTable.addConstantModule(module).index).putShort(access)
                .putShort(ReferenceEquals(version, null) ? 0 : symbolTable.addConstantUtf8(version));
            requiresCount++;
        }

        public override void visitExport(string packaze, int access, params string[] modules)
        {
            exports.putShort(symbolTable.addConstantPackage(packaze).index).putShort(access);
            if (modules == null)
            {
                exports.putShort(0);
            }
            else
            {
                exports.putShort(modules.Length);
                foreach (var module in modules) exports.putShort(symbolTable.addConstantModule(module).index);
            }

            exportsCount++;
        }

        public override void visitOpen(string packaze, int access, params string[] modules)
        {
            opens.putShort(symbolTable.addConstantPackage(packaze).index).putShort(access);
            if (modules == null)
            {
                opens.putShort(0);
            }
            else
            {
                opens.putShort(modules.Length);
                foreach (var module in modules) opens.putShort(symbolTable.addConstantModule(module).index);
            }

            opensCount++;
        }

        public override void visitUse(string service)
        {
            usesIndex.putShort(symbolTable.addConstantClass(service).index);
            usesCount++;
        }

        public override void visitProvide(string service, params string[] providers)
        {
            provides.putShort(symbolTable.addConstantClass(service).index);
            provides.putShort(providers.Length);
            foreach (var provider in providers) provides.putShort(symbolTable.addConstantClass(provider).index);
            providesCount++;
        }

        public override void visitEnd()
        {
            // Nothing to do.
        }

        /// <summary>
        ///     Returns the size of the Module, ModulePackages and ModuleMainClass attributes generated by this
        ///     ModuleWriter. Also add the names of these attributes in the constant pool.
        /// </summary>
        /// <returns> the size in bytes of the Module, ModulePackages and ModuleMainClass attributes. </returns>
        public int computeAttributesSize()
        {
            symbolTable.addConstantUtf8(Constants.MODULE);
            // 6 attribute header bytes, 6 bytes for name, flags and version, and 5 * 2 bytes for counts.
            var size = 22 + requires.length + exports.length + opens.length + usesIndex.length + provides.length;
            if (packageCount > 0)
            {
                symbolTable.addConstantUtf8(Constants.MODULE_PACKAGES);
                // 6 attribute header bytes, and 2 bytes for package_count.
                size += 8 + packageIndex.length;
            }

            if (mainClassIndex > 0)
            {
                symbolTable.addConstantUtf8(Constants.MODULE_MAIN_CLASS);
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
        public void putAttributes(ByteVector output)
        {
            // 6 bytes for name, flags and version, and 5 * 2 bytes for counts.
            var moduleAttributeLength = 16 + requires.length + exports.length + opens.length + usesIndex.length +
                                        provides.length;
            output.putShort(symbolTable.addConstantUtf8(Constants.MODULE)).putInt(moduleAttributeLength)
                .putShort(moduleNameIndex).putShort(moduleFlags).putShort(moduleVersionIndex).putShort(requiresCount)
                .putByteArray(requires.data, 0, requires.length).putShort(exportsCount)
                .putByteArray(exports.data, 0, exports.length).putShort(opensCount)
                .putByteArray(opens.data, 0, opens.length).putShort(usesCount)
                .putByteArray(usesIndex.data, 0, usesIndex.length).putShort(providesCount)
                .putByteArray(provides.data, 0, provides.length);
            if (packageCount > 0)
                output.putShort(symbolTable.addConstantUtf8(Constants.MODULE_PACKAGES)).putInt(2 + packageIndex.length)
                    .putShort(packageCount).putByteArray(packageIndex.data, 0, packageIndex.length);
            if (mainClassIndex > 0)
                output.putShort(symbolTable.addConstantUtf8(Constants.MODULE_MAIN_CLASS)).putInt(2)
                    .putShort(mainClassIndex);
        }
    }
}