using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Reflection;
using System.Reflection.Metadata;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Utilities;

namespace AST
{
    // ----------------------------------------------------------------------
    // NameAnalysisVisitor: Performs a static analysis pass (likely a form of
    // semantic analysis) to ensure all variables used in expressions have been
    // declared (or assigned) in the current scope.
    //
    // IVisitor parameters:
    // TParam (Tuple<SymbolTable, Statement>): Holds the current SymbolTable
    // and potentially the enclosing statement (unused in most visits here).
    // TResult (bool): Indicates success (true) or failure (false) of the analysis.
    // ----------------------------------------------------------------------
    public class NameAnalysisVisitor : IVisitor<Tuple<SymbolTable<string, object>, Statement>, bool>
    {
        // ------------------------------------------------------------------
        // Expression Node Visitors (Binary Operators):
        // For all binary expressions, name analysis is simply forwarded to
        // the left and right operands. The result is true only if both sub-
        // expressions are valid.
        // ------------------------------------------------------------------
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

        // ------------------------------------------------------------------
        // Expression Node Visitor (Variable Check):
        // This is the core check for name analysis, ensuring a variable is
        // defined before it is used in an expression.
        // ------------------------------------------------------------------
        public bool Visit(VariableNode node, Tuple<SymbolTable<string, object>, Statement> param)
        {
            SymbolTable<string, object> symbolTable = param.Item1;

            string varName = node.Name;
            // Checks if the variable name exists in the current symbol table (scope).
            if (symbolTable.ContainsKey(varName)) // Should this be contains key local? check this out please
            {
                return true;
            }
            else
            {
                // Report an error for an undeclared variable.
                Console.WriteLine($"{varName} is not declared in this scope"); // Check for nested scope issues stuff
                return false;
            }
        }

        // ------------------------------------------------------------------
        // Expression Node Visitor (Literal):
        // Literal values are always valid and require no symbol table check.
        // ------------------------------------------------------------------
        public bool Visit(LiteralNode node, Tuple<SymbolTable<string, object>, Statement> param)
        {
            return true;
        }

        // ------------------------------------------------------------------
        // Statement Node Visitor (Assignment):
        // Checks the validity of the expression first, then adds the variable
        // to the symbol table (effectively a declaration/definition) for subsequent use.
        // ------------------------------------------------------------------
        public bool Visit(AssignmentStmt node, Tuple<SymbolTable<string, object>, Statement> param)
        {
            SymbolTable<string, object> symbolTable = param.Item1;
            // First, analyze the expression on the right-hand side.
            bool expressionValid = node.Expression.Accept(this, param);

            // If expression is valid, add/re-add the variable to the symbol table
            // to mark it as defined in the current scope for future references.
            string varName = node.Variable.Name;
            symbolTable.Add(varName, null); // Note: Assuming Add handles both first-time and re-assignment.

            return expressionValid;
        }

        // ------------------------------------------------------------------
        // Statement Node Visitor (Return):
        // Name analysis for a return statement only requires checking the
        // validity of the returned expression.
        // ------------------------------------------------------------------
        public bool Visit(ReturnStmt node, Tuple<SymbolTable<string, object>, Statement> param)
        {
            return node.Expression.Accept(this, param);
        }

        // ------------------------------------------------------------------
        // Statement Node Visitor (Block):
        // Sequentially checks all statements within the block. If any statement
        // is invalid (returns false), the entire block is considered invalid.
        // **Note**: This visitor does not appear to handle scope creation/destruction
        // (i.e., pushing/popping symbol tables) which is typically required for blocks.
        // ------------------------------------------------------------------
        public bool Visit(BlockStmt node, Tuple<SymbolTable<string, object>, Statement> param)
        {
            bool valid = true;

            foreach (var statment in node.Statements)
            {
                // If any statement returns false, the entire block is invalid.
                if (!statment.Accept(this, param))
                {
                    valid = false;
                }
            }
            return valid;
        }
    }

}