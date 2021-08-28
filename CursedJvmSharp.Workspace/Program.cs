using System.IO;
using ObjectWeb.Asm;
using ObjectWeb.Asm.Tree;

var node = new ClassNode();

new ClassReader(File.ReadAllBytes(@"D:\Downloads\Swapchain.class")).Accept(node, 0);

var classVisitor = new ClassWriter(0);
node.Accept(classVisitor);

var byteArray = classVisitor.ToByteArray();

File.WriteAllBytes(@"D:\Downloads\Swapchain-out.class", byteArray);

