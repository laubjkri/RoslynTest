using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System;
using System.Reflection;
using System.Dynamic;
using System.Reflection.Metadata;
using RoslynTest.SyntaxReaders;

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
                //.AddReferences(MetadataReference.CreateFromFile(typeof(string).Assembly.Location))
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
                Parameter parameter = ParameterReader.GetParameter(parameterSyntax);

                function.Parameters.Add(parameter);
            }

            foreach (var item in function.Parameters)
            {
                Console.WriteLine(item);
            }



            // Transpile the block
            BlockSyntax block = functionSyntaxNode.ChildNodes().OfType<BlockSyntax>().Single();

            string st = StructuredTextTranspiler.GenerateCode(block);

            Console.Write(st);






        }
    }

    public class ParameterDataTypeIsUnsupportedException : Exception
    {
        public ParameterDataTypeIsUnsupportedException(string parameter)
            : base($"Unsupported data type for parameter \"{parameter}\"") { }
    }

    public class ParameterDataTypeIsNullException : Exception
    {
        public ParameterDataTypeIsNullException(string parameter) 
            : base($"No data type found for parameter \"{parameter}\"") { }
    }

    public class ParameterTypeException : Exception
    {
        public ParameterTypeException(string parameter) 
            : base($"Unknown type for parameter \"{parameter}\"") { }
    }

    public class WrongSyntaxPassedException : Exception
    {
        public WrongSyntaxPassedException(string syntaxPassed, string syntaxExpected) 
            : base("Wrong syntax passed. Passed: " + syntaxPassed + " Expected: " + syntaxExpected) { }
    }

    public class Parameter
    {
        public string Name;
        public TypeEnum Type;
        public DataTypeEnum DataType;

        public override string ToString()
        {            
            return $"Parameter: ({Type}) {Name} [{DataType}]";
        }

        public enum DataTypeEnum
        {
            dataTypeBool,
            dataTypeInt,
            dataTypeReal
        }

        public enum TypeEnum
        {
            typeIn,
            typeOut,
            typeRef
        }
    }

    public class StructuredTextTranspiler
    {
        /// <summary>
        /// Traverse the roslyn generated syntax tree recursively to generate structured text
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        static public string GenerateCode(SyntaxNode node)
        {
            // Todo include new lines and comment tokens




            if (node is BlockSyntax blockNode)
            {
                // In ST we do nothing about a block node                
                return GenerateCode(blockNode.ChildNodes().First());
            }                        
            else if(node is ExpressionStatementSyntax expressionStatementNode)
            {                
                return GetComments(expressionStatementNode) + GenerateCode(expressionStatementNode.ChildNodes().First()) + ";\n";
            }
            else if (node is AssignmentExpressionSyntax assignmentExpressionNode)
            {
                // Get the enumator so we can iterate across children
                var nodesEnumerator = assignmentExpressionNode.ChildNodes().GetEnumerator();

                // Get the identifier beeing assigned to
                nodesEnumerator.MoveNext();
                IdentifierNameSyntax identifierNameSyntax = (IdentifierNameSyntax)nodesEnumerator.Current;                
                string identifier = identifierNameSyntax.Identifier.ValueText;
                nodesEnumerator.MoveNext();
                string expression = GenerateCode(nodesEnumerator.Current);
                  

                return $"{identifier} := {expression}"; // ST uses := for assignment
            }
            else if (node is BinaryExpressionSyntax binaryExpressionNode)
            {// A binary expression contains two operands separated by one operator (operand1 + operand2)
                string left = GenerateCode(binaryExpressionNode.Left);
                string op = binaryExpressionNode.OperatorToken.ValueText; // We just transfer the operator directly for now. ST supports most of the same operators.
                string right = GenerateCode(binaryExpressionNode.Right);               
                
                return $"{left} {op} {right}"; 
            }
            else if (node is IdentifierNameSyntax identifierNameNode)
            {
                return identifierNameNode.Identifier.ValueText;
            }
            else
            {
                throw new NotImplementedException("Node transpilation not implemented.");
            }
        }


        static public string GetComments(SyntaxNode node)
        {
            // Get comments
            string result = string.Empty;
            if (node.HasLeadingTrivia)
            {
                var comments = node.GetLeadingTrivia().Where(trivia => trivia.IsKind(SyntaxKind.SingleLineCommentTrivia));
                if (comments.Count() > 0)
                {
                    foreach (var comment in comments)
                    {
                        result += comment + "\n"; // Maybe the span already contains newline?
                    }
                }
            }

            return result;
        }




        public string GenerateExpression(SyntaxNode node)
        {
            if (node is not ExpressionSyntax) 
                throw new ArgumentException("Expression expected");

            if (node is BinaryExpressionSyntax binaryExpressionNode)
            {// A binary expression contains two operands separated by one operator (operand1 + operand2)
                string left = GenerateCode(binaryExpressionNode.Left);
                string op = binaryExpressionNode.OperatorToken.ValueText; // We just transfer the operator directly for now. ST supports most of the same operators.
                string right = GenerateCode(binaryExpressionNode.Right);

                return $"{left} {op} {right}";
            }
            else if (node is IdentifierNameSyntax identifierNameNode)
            {
                return identifierNameNode.Identifier.ValueText;
            }
            else
            {
                throw new ArgumentException("Unknown expression syntax");
            }

        }

    }

    public class Variable
    {
        public string DataType;
        public string Name;
    }




    public class Function
    {
        public string Name;
        public List<Parameter> Parameters = new();

    }
}