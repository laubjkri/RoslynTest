using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoslynTest.SyntaxReaders
{
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
}
