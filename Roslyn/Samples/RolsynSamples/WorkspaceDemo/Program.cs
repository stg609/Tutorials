using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace WorkspaceDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            WorkspaceDemo workspaceDemo = new();
            workspaceDemo.AddDocument("using System;Guid.NewGuid();");
            workspaceDemo.GetInvocation(0, 26).Wait();
        }
    }

    /// <summary>cccc</summary>
    class WorkspaceDemo
    {
        static WorkspaceDemo()
        {
            Assembly[] lst = new[] {
                Assembly.Load("Microsoft.CodeAnalysis.Workspaces"),
                Assembly.Load("Microsoft.CodeAnalysis.CSharp.Workspaces"),
                Assembly.Load("Microsoft.CodeAnalysis.Features"),
                Assembly.Load("Microsoft.CodeAnalysis.CSharp.Features")
            };


            var xml = @"C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\5.0.0\ref\net5.0\System.Runtime.xml";

            DefaultMetadataReferences = ImmutableArray.Create(new MetadataReference[]
            {
                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location, documentation: XmlDocumentationProvider.CreateFromFile(xml)),
                MetadataReference.CreateFromFile(Assembly.Load("netstandard").Location),
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location, documentation: XmlDocumentationProvider.CreateFromFile(xml)),
                MetadataReference.CreateFromFile(Assembly.Load("System.Collections").Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Dynamic.ExpandoObject).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Microsoft.CSharp.RuntimeBinder.RuntimeBinderException).Assembly.Location),
            });
    
            DefaultUsing = ImmutableArray.Create(new string[]
            {
                "System",
                "System.Collections",
                "System.Collections.Generic",
                "System.Dynamic",
                "System.Linq",
                "System.Net.Http",
                "System.Text",
                "System.Threading",
                "System.Threading.Tasks"
            });

            _hostServices = MefHostServices.Create(lst);
        }


        public static ImmutableArray<MetadataReference> DefaultMetadataReferences { get; }
        public static ImmutableArray<string> DefaultUsing { get; }
        public Project Project { get; }

        private static MefHostServices _hostServices;
        private readonly AdhocWorkspace _workspace;

        public WorkspaceDemo()
        {
            _workspace = new AdhocWorkspace(_hostServices);

            var compilationOptions = new CSharpCompilationOptions(
               OutputKind.DynamicallyLinkedLibrary,
               usings: DefaultUsing);

            var projectInfo = ProjectInfo.Create(ProjectId.CreateNewId(), VersionStamp.Create(), "TempProject", "TempProject", LanguageNames.CSharp)
               .WithMetadataReferences(DefaultMetadataReferences)
               .WithCompilationOptions(compilationOptions);

            Project = _workspace.AddProject(projectInfo);
        }

        public void AddDocument(string code)
        {
            DocumentInfo doc = DocumentInfo.Create(DocumentId.CreateNewId(Project.Id),
                Path.GetTempFileName() + ".cs",
                sourceCodeKind: SourceCodeKind.Regular,
                loader: TextLoader.From(TextAndVersion.Create(SourceText.From(code),
                VersionStamp.Create())));
            _workspace.AddDocument(doc);

        }

        public async Task GetInvocation(int line, int column)
        {
            var invocations = new List<InvocationContext>();


            Project p = _workspace.CurrentSolution.Projects.FirstOrDefault();
            foreach (var document in p.Documents)
            {
                var invocation = await GetInvocation(document, line, column);
                if (invocation != null)
                {
                    invocations.Add(invocation);
                }
            }

            foreach (var invocation in invocations)
            {
                var types = invocation.ArgumentTypes;
                ISymbol throughSymbol = null;
                ISymbol throughType = null;
                var methodGroup = invocation.SemanticModel.GetMemberGroup(invocation.Receiver).OfType<IMethodSymbol>();
                if (invocation.Receiver is MemberAccessExpressionSyntax)
                {
                    var throughExpression = ((MemberAccessExpressionSyntax)invocation.Receiver).Expression;
                    throughSymbol = invocation.SemanticModel.GetSpeculativeSymbolInfo(invocation.Position, throughExpression, SpeculativeBindingOption.BindAsExpression).Symbol;
                    throughType = invocation.SemanticModel.GetSpeculativeTypeInfo(invocation.Position, throughExpression, SpeculativeBindingOption.BindAsTypeOrNamespace).Type;
                    var includeInstance = (throughSymbol != null && !(throughSymbol is ITypeSymbol)) ||
                        throughExpression is LiteralExpressionSyntax ||
                        throughExpression is TypeOfExpressionSyntax;
                    var includeStatic = (throughSymbol is INamedTypeSymbol) || throughType != null;
                    methodGroup = methodGroup.Where(m => (m.IsStatic && includeStatic) || (!m.IsStatic && includeInstance));
                }
                else if (invocation.Receiver is SimpleNameSyntax && invocation.IsInStaticContext)
                {
                    methodGroup = methodGroup.Where(m => m.IsStatic || m.MethodKind == MethodKind.LocalFunction);
                }
                foreach (var methodOverload in methodGroup)
                {
                    string doc = methodOverload.GetDocumentationCommentXml();
                }
            }
            
        }


        private async Task<InvocationContext> GetInvocation(Document document, int line, int column)
        {
            var sourceText = await document.GetTextAsync();
            var position = sourceText.Lines.GetPosition(new LinePosition(line, column));
            var tree = await document.GetSyntaxTreeAsync();
            var root = await tree.GetRootAsync();
            var node = root.FindToken(position).Parent;

            // Walk up until we find a node that we're interested in.
            while (node != null)
            {
                if (node is InvocationExpressionSyntax invocation && invocation.ArgumentList.Span.Contains(position))
                {
                    var semanticModel = await document.GetSemanticModelAsync();
                    return new InvocationContext(semanticModel, position, invocation.Expression, invocation.ArgumentList, false);
                }

                if (node is BaseObjectCreationExpressionSyntax objectCreation && (objectCreation.ArgumentList?.Span.Contains(position) ?? false))
                {
                    var semanticModel = await document.GetSemanticModelAsync();
                    return new InvocationContext(semanticModel, position, objectCreation, objectCreation.ArgumentList, false);
                }

                node = node.Parent;
            }

            return null;
        }


    }


}
