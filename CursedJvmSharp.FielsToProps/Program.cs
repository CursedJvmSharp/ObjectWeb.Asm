using System;
using System.Diagnostics;
using System.IO;
using Buildalyzer;
using Buildalyzer.Workspaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.VisualBasic;
using SyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using SyntaxKind = Microsoft.CodeAnalysis.CSharp.SyntaxKind;

AnalyzerManager manager = new AnalyzerManager();
IProjectAnalyzer analyzer =
    manager.GetProject(@"D:\Visual Studio - Projects\CursedJvmSharp\CursedJvmSharp.Asm\CursedJvmSharp.Asm.csproj");
IAnalyzerResults results = analyzer.Build();

foreach (var analyzerResult in results)
{
    var workspace = analyzerResult.GetWorkspace(true);

    foreach (var project in workspace.CurrentSolution.Projects)
    {
        foreach (var projectDocument in project.Documents)
        {
            var rewriter = new FieldToPropRewriter(projectDocument.GetSemanticModelAsync().Result);

            var syntaxNode = projectDocument.GetSyntaxRootAsync().Result;
            var newNode = rewriter.Visit(syntaxNode);
            var projectDocumentFilePath = projectDocument.FilePath;
            if (projectDocumentFilePath != null && syntaxNode != newNode)
            {
                Debugger.Break();
                File.WriteAllText(projectDocumentFilePath, newNode.NormalizeWhitespace().ToFullString());
            }
        }
    }
}


class FieldToPropRewriter : CSharpSyntaxRewriter
{
    private readonly SemanticModel model;

    public FieldToPropRewriter(SemanticModel model)
    {
        this.model = model;
    }

    public override SyntaxNode VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
    {
        var declaredSymbol = model.GetSymbolInfo(node);
        var declaredSymbolSymbol = declaredSymbol.Symbol;
        if (declaredSymbolSymbol is { Kind: SymbolKind.Field } &&
            declaredSymbolSymbol.ContainingModule.Name.StartsWith("CursedJvmSharp.Asm"))
        {
            var actualFieldSymbol = declaredSymbolSymbol as IFieldSymbol;
            if (actualFieldSymbol is { IsConst: false, IsReadOnly: false, DeclaredAccessibility: Accessibility.Public })
            {
                return SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, node.Expression,
                    SyntaxFactory.IdentifierName(
                        GetFieldToPropName(actualFieldSymbol
                            .Name)));
            }
        }

        return base.VisitMemberAccessExpression(node);
    }

    public static string TitleCaseString(string s)
    {
        if (s.Length == 1)
            return s.ToUpper();
        return s[0].ToString().ToUpper() + s[1..];
    }


    private string GetFieldToPropName(string name)
    {
        return TitleCaseString(name);
    }

    public override SyntaxNode VisitFieldDeclaration(FieldDeclarationSyntax node)
    {
        if (!node.Modifiers.Any(SyntaxKind.ConstKeyword) && !node.Modifiers.Any(SyntaxKind.ReadOnlyKeyword) &&
            node.Modifiers.Any(SyntaxKind.PublicKeyword))
        {
            var variable = node.Declaration.Variables.First();
            var resultProp = SyntaxFactory.PropertyDeclaration(node.AttributeLists, node.Modifiers,
                node.Declaration.Type, null,
                SyntaxFactory.Identifier(GetFieldToPropName(variable.Identifier.ToFullString())),
                SyntaxFactory.AccessorList(
                    SyntaxFactory.List<AccessorDeclarationSyntax>(
                        new AccessorDeclarationSyntax[]
                        {
                            SyntaxFactory.AccessorDeclaration(
                                    SyntaxKind.GetAccessorDeclaration)
                                .WithSemicolonToken(
                                    SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                            SyntaxFactory.AccessorDeclaration(
                                SyntaxKind.SetAccessorDeclaration)
                                .WithSemicolonToken(
                                    SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                        })), null, variable.Initializer);
            if (variable.Initializer != null)
            {
                Debugger.Break();
                resultProp = resultProp.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
            }

            resultProp = resultProp.NormalizeWhitespace();

            return resultProp;
        }

        return base.VisitFieldDeclaration(node);
    }
}