using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System;
using System.Reflection;

namespace RoslynTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string code = File.ReadAllText(@"C:\Temp\RoslynTest\Function.cs");


            // Create a syntax tree from the method declaration code
            SyntaxTree tree = CSharpSyntaxTree.ParseText(code);

            // Get the root node of the syntax tree
            CompilationUnitSyntax root = tree.GetCompilationUnitRoot();

            // Get the semantic model (checks for errors and meaning)
            var compilation = CSharpCompilation.Create("MyCompilation")
                .AddReferences(MetadataReference.CreateFromFile(typeof(string).Assembly.Location))
                .AddSyntaxTrees(tree);

            SemanticModel semanticModel = compilation.GetSemanticModel(tree);



            // Find the function
            LocalFunctionStatementSyntax functionSyntaxNode = root.DescendantNodes().OfType<LocalFunctionStatementSyntax>().Single();

            var symbolInfo = semanticModel.GetDeclaredSymbol(functionSyntaxNode);
            

            Function function = new();
            function.Name = functionSyntaxNode.Identifier.Text;
            
            Console.WriteLine(function.Name);

            if (functionSyntaxNode.ReturnType.ToString() != "void")
            {
                Console.WriteLine("Return type must be void");
            }
            else
            {
                Console.WriteLine("Return type: " + functionSyntaxNode.ReturnType.ToString());
            }
            
            foreach (var item in functionSyntaxNode.ParameterList.ChildNodes())
            {
                ParameterSyntax parameterSyntax = (ParameterSyntax)item;
                Parameter parameter = new();
                parameter.Name = parameterSyntax.Identifier.Text;
                parameter.DataType = parameterSyntax.Type.ToString();
                parameter.Type = semanticModel.GetDeclaredSymbol(parameterSyntax).RefKind.ToString();

                function.Parameters.Add(parameter);

            }

            foreach (var item in function.Parameters)
            {
                Console.WriteLine(item);
            }
        }

    }

    public class Parameter
    {
        public string Type;
        public string DataType;
        public string Name;

        public override string ToString()
        {            
            return $"Parameter: {Name} [{DataType}] ({Type})";
        }

    }

    public class Function
    {
        public string Name;
        public List<Parameter> Parameters = new();
    }
}