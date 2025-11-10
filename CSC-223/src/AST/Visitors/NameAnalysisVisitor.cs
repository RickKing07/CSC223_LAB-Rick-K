using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Reflection;
using System.Reflection.Metadata;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Utilities;

namespace AST
{
    public class NameAnalysisVisitor : IVisitor<Tuple<SymbolTable<string, object>, Statement>, bool>
    {
        public bool Visit(PlusNode node, Tuple<SymbolTable<string, object>, Statement> param)
        {
            return node.Left.Accept(this, param) && node.Right.Accept(this, param);
        }
        public bool Visit(MinusNode node, Tuple<SymbolTable<string, object>, Statement> param)
        {
            return node.Left.Accept(this, param) && node.Right.Accept(this, param);
        }
        public bool Visit(TimesNode node, Tuple<SymbolTable<string, object>, Statement> param)
        {
            return node.Left.Accept(this, param) && node.Right.Accept(this, param);
        }
        public bool Visit(FloatDivNode node, Tuple<SymbolTable<string, object>, Statement> param)
        {
            return node.Left.Accept(this, param) && node.Right.Accept(this, param);
        }
        public bool Visit(IntDivNode node, Tuple<SymbolTable<string, object>, Statement> param)
        {
            return node.Left.Accept(this, param) && node.Right.Accept(this, param);
        }
        public bool Visit(ModulusNode node, Tuple<SymbolTable<string, object>, Statement> param)
        {
            return node.Left.Accept(this, param) && node.Right.Accept(this, param);
        }
        public bool Visit(ExponentiationNode node, Tuple<SymbolTable<string, object>, Statement> param)
        {
            return node.Left.Accept(this, param) && node.Right.Accept(this, param);
        }
        public bool Visit(VariableNode node, Tuple<SymbolTable<string, object>, Statement> param)
        {
            SymbolTable<string, object> symbolTable = param.Item1;

            string varName = node.Name;
            if (symbolTable.ContainsKey(varName))
            {
                return true;
            }
            else
            {
                Console.WriteLine($"{varName} is not declared in this scope"); //Check for nested scope issues stuff
                return false;
            }
        }
        public bool Visit(LiteralNode node, Tuple<SymbolTable<string, object>, Statement> param)
        {
            return true;
        }
        public bool Visit(AssignmentStmt node, Tuple<SymbolTable<string, object>, Statement> param)
        {
            SymbolTable<string, object> symbolTable = param.Item1;
            bool expressionValid = node.Expression.Accept(this, param);
            string varName = node.Variable.Name;
            symbolTable.Add(varName, null);
            return expressionValid;
        }
        public bool Visit(ReturnStmt node, Tuple<SymbolTable<string, object>, Statement> param)
        {
            return node.Expression.Accept(this, param);
        }
        public bool Visit(BlockStmt node, Tuple<SymbolTable<string, object>, Statement> param)
        {
            SymbolTable<string, object> symbolTable = param.Item1;
            bool valid = true;

            foreach (var statment in node.Statements)
            {
                if (!statment.Accept(this, param))
                {
                    valid = false;
                }

            }
            return valid;
        }
    }

}