using System;
using System.Text;
using AST;
using Utilities;

namespace AST
{

    public class UnparseVisitor : IVisitor<int, string>
    {
        public string Unparse(ExpressionNode node, int level = 0)
        {
            return node.Accept(this, level);
        }

        public string Unparse(Statement stmt, int level = 0)
        {
            return stmt.Accept(this, level);
        }

        public string Visit(PlusNode node, int level)
        {
            string left = node.Left.Accept(this, level);
            string right = node.Right.Accept(this, level);
            return $"({left} + {right})";
        }

        public string Visit(MinusNode node, int level)
        {
            string left = node.Left.Accept(this, level);
            string right = node.Right.Accept(this, level);
            return $"({left} - {right})";
        }

        public string Visit(TimesNode node, int level)
        {
            string left = node.Left.Accept(this, level);
            string right = node.Right.Accept(this, level);
            return $"({left} * {right})";
        }

        public string Visit(FloatDivNode node, int level)
        {
            string left = node.Left.Accept(this, level);
            string right = node.Right.Accept(this, level);
            return $"({left} / {right})";
        }

        public string Visit(IntDivNode node, int level)
        {
            string left = node.Left.Accept(this, level);
            string right = node.Right.Accept(this, level);
            return $"({left} // {right})";
        }

        public string Visit(ModulusNode node, int level)
        {
            string left = node.Left.Accept(this, level);
            string right = node.Right.Accept(this, level);
            return $"({left} % {right})";
        }

        public string Visit(ExponentiationNode node, int level)
        {
            string left = node.Left.Accept(this, level);
            string right = node.Right.Accept(this, level);
            return $"({left} ** {right})";
        }

        public string Visit(LiteralNode node, int level)
        {
            return node.Value.ToString();
        }

        public string Visit(VariableNode node, int level)
        {
            return node.Name;
        }
        public string Visit(AssignmentStmt node, int level)
        {
            string indent = GeneralUtils.GetIndentation(level);
            return $"{indent}{node.Variable.Unparse()} := {node.Expression.Unparse()};";
        }
        public string Visit(ReturnStmt node, int level)
        {
            string indent = GeneralUtils.GetIndentation(level);
            return $"{indent}return {node.Expression.Unparse()};";
        }

        public string Visit(BlockStmt node, int level)
        {
            string indent = GeneralUtils.GetIndentation(level);
            string result = indent + "{\n";
            foreach (var stmt in node.Statements)
            {
                result += stmt.Accept(this, level + 1) + "\n";
            }
            result += indent + "}";
            return result;
        }

    }

}

// using System;
// using System.Collections.Generic;
// using System.Diagnostics.Tracing;
// using System.Reflection;
// using System.Reflection.Metadata;
// using Microsoft.VisualStudio.TestPlatform.ObjectModel;
// using Utilities;


// namespace AST
// {
//     public class UnparseVisitor : IVisitor<int, string>
//     {
//         public string Visit(PlusNode node, int indentLevel)
//         {
//             return $"{GeneralUtils.GetIndentation(indentLevel)}{node.Left.Accept(this, indentLevel)} + {node.Right.Accept(this, indentLevel)}";
//         }
//         public string Visit(MinusNode node, int indentLevel)
//         {
//             return $"{GeneralUtils.GetIndentation(indentLevel)}{node.Left.Accept(this, indentLevel)} - {node.Right.Accept(this, indentLevel)}";
//         }
//         public string Visit(TimesNode node, int indentLevel)
//         {
//             return $"{GeneralUtils.GetIndentation(indentLevel)}{node.Left.Accept(this, indentLevel)} * {node.Right.Accept(this, indentLevel)}";
//         }
//         public string Visit(FloatDivNode node, int indentLevel)
//         {
//             return $"{GeneralUtils.GetIndentation(indentLevel)}{node.Left.Accept(this, indentLevel)} / {node.Right.Accept(this, indentLevel)}";
//         }
//         public string Visit(IntDivNode node, int indentLevel)
//         {
//             return $"{GeneralUtils.GetIndentation(indentLevel)}{node.Left.Accept(this, indentLevel)} // {node.Right.Accept(this, indentLevel)}";
//         }
//         public string Visit(ModulusNode node, int indentLevel)
//         {
//             return $"{GeneralUtils.GetIndentation(indentLevel)}{node.Left.Accept(this, indentLevel)} % {node.Right.Accept(this, indentLevel)}";
//         }
//         public string Visit(ExponentiationNode node, int indentLevel)
//         {
//             return $"{GeneralUtils.GetIndentation(indentLevel)}{node.Left.Accept(this, indentLevel)} ** {node.Right.Accept(this, indentLevel)}";
//         }
//         public string Visit(LiteralNode node, int indentLevel)
//         {
//             return $"{GeneralUtils.GetIndentation(indentLevel)}{node.Value}";
//         }
//         public string Visit(VariableNode node, int indentLevel)
//         {
//             return $"{GeneralUtils.GetIndentation(indentLevel)}{node.Name}";
//         }
//         public string Visit(AssignmentStmt node, int indentLevel) //somewhere in here add parenth
//         {
//             return $"{GeneralUtils.GetIndentation(indentLevel)}{node.Variable.Accept(this, indentLevel)} := {node.Expression.Accept(this, indentLevel)}";
//         }
//         public string Visit(ReturnStmt node, int indentLevel)
//         {
//             return $"{GeneralUtils.GetIndentation(indentLevel)}return {node.Expression.Accept(this, indentLevel)}";
//         }
//         public string Visit(BlockStmt node, int indentLevel) //somehow incriment indent level
//         {
//             string result = "{\n";
//             foreach (var line in node.Statements)
//             {
//                 if (line.Equals('{')) { indentLevel++; } //untested, just guessing at what a indent level change could look like
//                 if (line.Equals('}')) { indentLevel--; }
//                 result += $"{GeneralUtils.GetIndentation(indentLevel)}{line}\n";
//             }
//             result += '}'; //Add indent level?
//             return result;
//         }
//     }
// }