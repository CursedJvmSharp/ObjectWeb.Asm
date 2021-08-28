using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using ObjectWeb.Asm;
using ObjectWeb.Asm.Tree;

var node = new ClassNode();

new ClassReader(Unsafe.As<sbyte[]>(File.ReadAllBytes(@"D:\Downloads\Swapchain.class"))).Accept(node, 0);

Console.WriteLine(node);
//Debugger.Break();

var classVisitor = new ClassWriter(0);
node.Accept(classVisitor);

var byteArray = classVisitor.ToByteArray();

File.WriteAllBytes(@"D:\Downloads\Swapchain-out.class", Unsafe.As<byte[]>(byteArray));