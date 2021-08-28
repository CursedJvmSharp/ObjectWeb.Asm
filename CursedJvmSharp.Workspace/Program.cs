using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using org.objectweb.asm;
using org.objectweb.asm.tree;

var node = new ClassNode();

new ClassReader(Unsafe.As<sbyte[]>(File.ReadAllBytes(@"D:\Downloads\Swapchain.class"))).accept(node, 0);

Console.WriteLine(node);
//Debugger.Break();

var classVisitor = new ClassWriter(0);
node.accept(classVisitor);

var byteArray = classVisitor.toByteArray();

File.WriteAllBytes(@"D:\Downloads\Swapchain-out.class", Unsafe.As<byte[]>(byteArray));