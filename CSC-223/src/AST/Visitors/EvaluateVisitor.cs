/**
 * EvaluateVisitor.cs
 *
 * Provides an implementation of the visitor pattern for evaluating
 * Abstract Syntax Tree (AST) nodes. This class traverses expression
 * and statement nodes, performing arithmetic, variable lookup,
 * and assignment operations using a scoped symbol table.
 *
 * Bugs: None known at this time.
 * change the state of the return value in here and nameanalysis visitors!
 * @author Rahul, Rick, Zachary
 * @date 2025-11-10
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Reflection;
using System.Reflection.Metadata;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Utilities;

namespace AST
{
    /// <summary>
    /// Visitor implementation that evaluates AST nodes.
    /// Supports arithmetic operations, variable references,
    /// assignments, and return statements with scoping.
    /// </summary>
    public class EvaluateVisitor : IVisitor<SymbolTable<string, object>, object>
    {
        #region Inner Exception Class
        /// <summary>
        /// Custom exception type used to indicate evaluation errors
        /// such as undefined variables or divide-by-zero operations.
        /// </summary>
        public class EvaluationException : Exception
        {
            public EvaluationException(string message) : base(message) { }
        }
        #endregion

        // Flag to indicate if a return statement has been encountered
        private bool _returnEncountered;

        // Holds the return value after a return statement is evaluated
        private object _returnValue;

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="EvaluateVisitor"/> class.
        /// Sets default flags for return state tracking.
        /// </summary>
        public EvaluateVisitor()
        {
            _returnEncountered = false;
            _returnValue = null;
        }
        #endregion

        #region Evaluate Entry Point
        /// <summary>
        /// Evaluates the given AST starting from the root statement.
        /// </summary>
        /// <param name="ast">The root statement of the AST to evaluate.</param>
        /// <returns>
        /// The final result of evaluation, usually the return value from
        /// a function or block.
        /// </returns>
        public object Evaluate(Statement ast)
        {
            // Reset evaluation state before traversal
            _returnEncountered = false;
            _returnValue = null;

            // Begin AST traversal with no initial scope (handled internally)
            _returnValue = ast.Accept(this, null);

            return _returnValue;
        }
        #endregion

        #region Arithmetic Operations
        /// <summary>
        /// Evaluates an addition operation between two operands.
        /// </summary>
        public object Visit(PlusNode node, SymbolTable<string, object> symbolTable)
        {
            object left = node.Left.Accept(this, symbolTable);
            object right = node.Right.Accept(this, symbolTable);

            if (left is int l && right is int r)
                return l + r;

            return Convert.ToDouble(left) + Convert.ToDouble(right);
        }

        /// <summary>
        /// Evaluates a subtraction operation between two operands.
        /// </summary>
        public object Visit(MinusNode node, SymbolTable<string, object> symbolTable)
        {
            object left = node.Left.Accept(this, symbolTable);
            object right = node.Right.Accept(this, symbolTable);

            if (left is int l && right is int r)
                return l - r;

            return Convert.ToDouble(left) - Convert.ToDouble(right);
        }

        /// <summary>
        /// Evaluates a multiplication operation between two operands.
        /// </summary>
        public object Visit(TimesNode node, SymbolTable<string, object> symbolTable)
        {
            object left = node.Left.Accept(this, symbolTable);
            object right = node.Right.Accept(this, symbolTable);

            if (left is int l && right is int r)
                return l * r;

            return Convert.ToDouble(left) * Convert.ToDouble(right);
        }

        /// <summary>
        /// Evaluates a floating-point division operation.
        /// </summary>
        /// <exception cref="EvaluationException">Thrown when dividing by zero.</exception>
        public object Visit(FloatDivNode node, SymbolTable<string, object> symbolTable)
        {
            object left = node.Left.Accept(this, symbolTable);
            object right = node.Right.Accept(this, symbolTable);
            double r = Convert.ToDouble(right);

            if (r == 0.0)
                throw new EvaluationException("Cannot divide by zero");

            double l = Convert.ToDouble(left);
            return l / r;
        }

        /// <summary>
        /// Evaluates an integer division operation.
        /// </summary>
        /// <exception cref="EvaluationException">Thrown when dividing by zero.</exception>
        public object Visit(IntDivNode node, SymbolTable<string, object> symbolTable)
        {
            object left = node.Left.Accept(this, symbolTable);
            object right = node.Right.Accept(this, symbolTable);

            int r = Convert.ToInt32(right);
            if (r == 0)
                throw new EvaluationException("Cannot divide by zero");

            int l = Convert.ToInt32(left);
            return l / r;
        }

        /// <summary>
        /// Evaluates a modulus (remainder) operation.
        /// </summary>
        /// <exception cref="EvaluationException">Thrown when dividing by zero.</exception>
        public object Visit(ModulusNode node, SymbolTable<string, object> symbolTable)
        {
            object left = node.Left.Accept(this, symbolTable);
            object right = node.Right.Accept(this, symbolTable);

            // Integer modulus
            if (left is int l && right is int r)
            {
                if (r == 0)
                    throw new EvaluationException("Cannot divide by zero");
                return l % r;
            }

            // Floating-point modulus
            if (Convert.ToDouble(right) == 0.0)
                throw new EvaluationException("Cannot divide by float zero");

            return Convert.ToDouble(left) % Convert.ToDouble(right);
        }

        /// <summary>
        /// Evaluates exponentiation between two operands.
        /// </summary>
        public object Visit(ExponentiationNode node, SymbolTable<string, object> symbolTable)
        {
            object left = node.Left.Accept(this, symbolTable);
            object right = node.Right.Accept(this, symbolTable);

            if (left is int l && right is int r)
                return Math.Pow(l, r);

            return Math.Pow(Convert.ToDouble(left), Convert.ToDouble(right));
        }
        #endregion

        #region Variable and Literal Nodes
        /// <summary>
        /// Retrieves a variable's value from the current scope.
        /// </summary>
        /// <exception cref="EvaluationException">Thrown when a variable is undefined.</exception>
        public object Visit(VariableNode node, SymbolTable<string, object> symbolTable)
        {
            if (symbolTable.TryGetValue(node.Name, out object value))
                return value;

            throw new EvaluationException($"Undefined variable '{node.Name}'");
        }

        /// <summary>
        /// Returns the literal value directly.
        /// </summary>
        public object Visit(LiteralNode node, SymbolTable<string, object> symbolTable)
        {
            return node.Value;
        }
        #endregion

        #region Statements
        /// <summary>
        /// Executes an assignment statement.
        /// Adds or updates a variable in the current symbol table.
        /// </summary>
        public object Visit(AssignmentStmt node, SymbolTable<string, object> symbolTable)
        {
            // Evaluate expression on the right-hand side
            object value = node.Expression.Accept(this, symbolTable);
            string name = node.Variable.Name;

            // Update existing variable or create a new one
            if (symbolTable.ContainsKeyLocal(name))
                symbolTable[name] = value;
            else
                symbolTable.Add(name, value);

            return _returnValue;
        }

        /// <summary>
        /// Evaluates a return statement and produces a value.
        /// </summary>
        public object Visit(ReturnStmt node, SymbolTable<string, object> symbolTable)
        {
            return node.Expression.Accept(this, symbolTable);
        }

        /// <summary>
        /// Executes a block of statements with its own scoped symbol table.
        /// </summary>
        public object Visit(BlockStmt node, SymbolTable<string, object> symbolTable)
        {
            // Use block's own scope, chained to its parent
            SymbolTable<string, object> currentScope = node.SymbolTable;

            // Sequentially execute statements until a return value is produced
            foreach (var stmt in node.Statements)
            {
                object result = stmt.Accept(this, currentScope);
                if (result != null)
                    return result;
            }

            return _returnValue;
        }
        #endregion
    }
}
