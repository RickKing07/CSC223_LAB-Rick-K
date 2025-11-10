using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using AST;
using Utilities;

namespace AST
{
    // ----------------------------------------------------------------------
    // EvaluateVisitor: Executes the Abstract Syntax Tree (AST) to compute
    // results and perform state changes (like variable assignments).
    //
    // IVisitor parameters:
    // TParam (SymbolTable): The symbol table representing the current scope.
    // TResult (object): The calculated value of an expression or null/return
    //                   value for a statement.
    // ----------------------------------------------------------------------
    public class EvaluateVisitor : IVisitor<SymbolTable<string, object>, object>
    {

        // Custom exception for runtime errors during evaluation, like division by zero.
        public class EvaluationException : Exception
        {
            public EvaluationException(string message) : base(message) { }
        }

        // Flag to control execution flow when a return statement is hit.
        private bool _returnEncountered;

        // Stores the value returned by a ReturnStmt.
        private object _returnValue;

        // Initializes a new instance of the EvaluateVisitor class, resetting state.
        public EvaluateVisitor()
        {
            _returnEncountered = false;
            _returnValue = null;
        }

        // ------------------------------------------------------------------
        // Public Evaluate Method: The entry point for starting the execution of the AST.
        // It resets the return state and initiates the visitor pattern on the root node.
        // ------------------------------------------------------------------
        public object Evaluate(Statement ast)
        {
            _returnEncountered = false;
            _returnValue = null;

            // Execute the AST (typically a BlockStmt) using the visitor pattern.
            // The initial SymbolTable is often null, expecting the root block
            // to create the global scope.
            _returnValue = ast.Accept(this, null);

            // Returns the final accumulated return value.
            return _returnValue;
        }

        // ------------------------------------------------------------------
        // Expression Node Visitors (Arithmetic Operators):
        // These methods recursively evaluate both sides of the operation, then
        // perform the corresponding arithmetic calculation, handling both
        // integer and floating-point types (with type promotion).
        // ------------------------------------------------------------------
        public object Visit(PlusNode node, SymbolTable<string, object> symbolTable)
        {
            object left = node.Left.Accept(this, symbolTable);
            object right = node.Right.Accept(this, symbolTable);

            // Prioritize integer addition for performance/type correctness.
            if (left is int l && right is int r) { return l + r; }
            // Otherwise, promote to double for float addition.
            return Convert.ToDouble(left) + Convert.ToDouble(right);
        }

        public object Visit(MinusNode node, SymbolTable<string, object> symbolTable)
        {
            object left = node.Left.Accept(this, symbolTable);
            object right = node.Right.Accept(this, symbolTable);

            // Prioritize integer subtraction.
            if (left is int l && right is int r) { return l - r; }
            // Otherwise, promote to double for float subtraction.
            return Convert.ToDouble(left) - Convert.ToDouble(right);
        }

        public object Visit(TimesNode node, SymbolTable<string, object> symbolTable)
        {
            object left = node.Left.Accept(this, symbolTable);
            object right = node.Right.Accept(this, symbolTable);

            // Prioritize integer multiplication.
            if (left is int l && right is int r) { return l * r; }
            // Otherwise, promote to double for float multiplication.
            return Convert.ToDouble(left) * Convert.ToDouble(right);
        }

        public object Visit(FloatDivNode node, SymbolTable<string, object> symbolTable)
        {
            object left = node.Left.Accept(this, symbolTable);
            object right = node.Right.Accept(this, symbolTable);
            double r = Convert.ToDouble(right);

            // Check for division by zero.
            if (r == 0.0)
                throw new EvaluationException("Cannot divide by zero");

            // Perform floating-point division.
            double l = Convert.ToDouble(left);
            return l / r;
        }

        public object Visit(IntDivNode node, SymbolTable<string, object> symbolTable)
        {
            object left = node.Left.Accept(this, symbolTable);
            object right = node.Right.Accept(this, symbolTable);
            int r = Convert.ToInt32(right);

            // Check for integer division by zero.
            if (r == 0)
                throw new EvaluationException("Cannot divide by zero");

            // Perform integer division.
            int l = Convert.ToInt32(left);
            return l / r;
        }

        public object Visit(ModulusNode node, SymbolTable<string, object> symbolTable)
        {
            object left = node.Left.Accept(this, symbolTable);
            object right = node.Right.Accept(this, symbolTable);

            if (left is int l && right is int r)
            {
                if (r == 0) { throw new EvaluationException("Cannot divide by zero"); }
                return l % r;
            }
            // Check for float division by zero before converting and calculating.
            if (Convert.ToDouble(right) == 0.0) { throw new EvaluationException("Cannot divide by float zero"); }

            // Perform floating-point modulus.
            return Convert.ToDouble(left) % Convert.ToDouble(right);
        }

        public object Visit(ExponentiationNode node, SymbolTable<string, object> symbolTable)
        {
            object left = node.Left.Accept(this, symbolTable);
            object right = node.Right.Accept(this, symbolTable);

            // Use Math.Pow for exponentiation, prioritizing integer types if possible.
            if (left is int l && right is int r)
            {
                return Math.Pow(l, r);
            }

            // Fallback to double-based exponentiation.
            return Math.Pow(Convert.ToDouble(left), Convert.ToDouble(right));
        }

        // ------------------------------------------------------------------
        // Expression Node Visitors (Atomic Elements):
        // These methods retrieve values from the SymbolTable or the node itself.
        // ------------------------------------------------------------------

        public object Visit(VariableNode node, SymbolTable<string, object> symbolTable)
        {
            // Looks up the variable value in the SymbolTable, checking scopes.
            if (symbolTable.TryGetValue(node.Name, out object value)) { return value; }

            // Throws an exception if the variable is not found in any accessible scope.
            throw new EvaluationException($"Undefined variable '{node.Name}'");
        }


        public object Visit(LiteralNode node, SymbolTable<string, object> symbolTable)
        {
            // Returns the constant value stored in the literal node.
            return node.Value;
        }


        // ------------------------------------------------------------------
        // Statement Node Visitors (Program Logic):
        // These methods manage state (SymbolTable) and control flow (Block, Return).
        // ------------------------------------------------------------------

        public object Visit(AssignmentStmt node, SymbolTable<string, object> symbolTable)
        {
            // 1. Evaluate the expression to get the value.
            object value = node.Expression.Accept(this, symbolTable);
            string name = node.Variable.Name;

            // 2. Store the value in the symbol table.
            if (symbolTable.ContainsKeyLocal(name))
            {
                // If the variable is defined in the current local scope, update its value.
                symbolTable[name] = value;
            }
            else
            {
                // Otherwise, add it as a new variable in the current local scope.
                symbolTable.Add(name, value);
            }

            // Assignments typically return the current return value or null.
            return _returnValue;
        }

        public object Visit(ReturnStmt node, SymbolTable<string, object> symbolTable)
        {
            // 1. Evaluate the expression to get the return value.
            _returnValue = node.Expression.Accept(this, symbolTable);

            // 2. Set the return flag (though the block logic handles the immediate exit).
            _returnEncountered = true;

            // 3. Return the value immediately to stop block execution.
            return _returnValue;
        }

        public object Visit(BlockStmt node, SymbolTable<string, object> symbolTable)
        {
            // Blocks act as a new scope. The node's SymbolTable should be properly
            // initialized with a parent scope before this visit.
            SymbolTable<string, object> currentScope = node.SymbolTable;

            // Iterate through statements until a return or the end of the block is reached.
            foreach (var stmt in node.Statements)
            {
                object result = stmt.Accept(this, currentScope);

                // If a non-null result is returned (signifying an explicit return statement
                // was hit), propagate that result to exit the block and outer scopes.
                if (result != null)
                {
                    return result;
                }
            }

            // If no return was encountered, return the current state of the return value (often null).
            return _returnValue;
        }
    }
}