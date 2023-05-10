﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System;
using System.Reflection;
using System.Dynamic;
using System.Reflection.Metadata;

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

            // Find the block that contains the body of the function
            // There can be only one block for a function definition
            BlockSyntax block = functionSyntaxNode.ChildNodes().OfType<BlockSyntax>().Single();
            

            // Get all children of the block
            foreach (SyntaxNode item in block.ChildNodes())
            {
                Console.WriteLine(item.);
            }
        }
    }

    public static class ParameterReader
    {
        public static Parameter GetParameter(ParameterSyntax parameterSyntax)
        {
            if (parameterSyntax.Kind() != SyntaxKind.Parameter) throw new WrongSyntaxPassedException(parameterSyntax.Kind().ToString(), "parameter");
            Parameter parameter = new();
            parameter.Name = parameterSyntax.Identifier.Text;
            parameter.Type = GetParameterTypeEnum(parameterSyntax);
            parameter.DataType = GetParameterDataTypeEnum(parameterSyntax);

            return parameter;
        }
        
        public static Parameter.TypeEnum GetParameterTypeEnum(ParameterSyntax parameterSyntax)
        {
            if (parameterSyntax.Kind() != SyntaxKind.Parameter) throw new WrongSyntaxPassedException(parameterSyntax.Kind().ToString(), "parameter");
            // If there are no modifiers the parameter is an "in" type.
            if (parameterSyntax.Modifiers.Count == 0 || parameterSyntax.Modifiers[0].Text == "in")
            {
                return Parameter.TypeEnum.typeIn;
            }
            else if (parameterSyntax.Modifiers[0].Text == "out")
            {
                return Parameter.TypeEnum.typeOut;
            }
            else if (parameterSyntax.Modifiers[0].Text == "ref")
            {
                return Parameter.TypeEnum.typeRef;
            }
            else
            {
                throw new ParameterTypeException(parameterSyntax.Identifier.Text);
            }
        }

        public static Parameter.DataTypeEnum GetParameterDataTypeEnum(ParameterSyntax parameterSyntax)
        {
            if (parameterSyntax.Kind() != SyntaxKind.Parameter) throw new WrongSyntaxPassedException(parameterSyntax.Kind().ToString(), "parameter");
            if (parameterSyntax.Type == null) throw new ParameterDataTypeIsNullException(parameterSyntax.Identifier.Text);
            
            switch (parameterSyntax.Type.ToString())
            {
                case "bool":
                    return Parameter.DataTypeEnum.dataTypeBool;
                case "int":
                    return Parameter.DataTypeEnum.dataTypeInt;
                case "real":
                    return Parameter.DataTypeEnum.dataTypeReal;
                default:
                    throw new ParameterDataTypeIsUnsupportedException(parameterSyntax.Identifier.Text);                    
            }
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

    public class Expression
    {
        public string Operator;


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