using System;
using System.Text;
using AST;
using Utilities;

namespace AST
{
    // ----------------------------------------------------------------------
    // UnparseVisitor: Converts an Abstract Syntax Tree (AST) back into
    // its corresponding source code string representation.
    // Implements IVisitor<int, string>, where int is the indentation level
    // and string is the resulting code string.
    // ----------------------------------------------------------------------
    public class UnparseVisitor : IVisitor<int, string>
    {
        // ------------------------------------------------------------------
        // Public Unparse Methods: Entry points for unparsing different node types.
        // These methods use the Accept pattern to dispatch the visitor to the
        // appropriate Visit method for the concrete node type.
        // ------------------------------------------------------------------
        public string Unparse(ExpressionNode node, int level = 0)
        {
            return node.Accept(this, level);
        }

        public string Unparse(Statement stmt, int level = 0)
        {
            return stmt.Accept(this, level);
        }

        // ------------------------------------------------------------------
        // Expression Node Visitors (Arithmetic Operators):
        // These methods handle binary expressions by recursively unparsing
        // the left and right operands and formatting them with the correct
        // operator, enclosed in parentheses for correct precedence.
        // ------------------------------------------------------------------
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
            return $"({left} // {right})"; // Assumes // is the integer division operator
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
            return $"({left} ** {right})"; // Assumes ** is the exponentiation operator
        }

        // ------------------------------------------------------------------
        // Expression Node Visitors (Atomic Elements):
        // These methods handle leaf nodes in the expression tree.
        // ------------------------------------------------------------------
        public string Visit(LiteralNode node, int level)
        {
            // Converts the stored literal value (e.g., int, float) into its string form.
            return node.Value.ToString();
        }

        public string Visit(VariableNode node, int level)
        {
            // Simply returns the identifier name.
            return node.Name;
        }

        // ------------------------------------------------------------------
        // Statement Node Visitors:
        // These methods handle control flow and action statements, managing
        // indentation based on the provided 'level'.
        // ------------------------------------------------------------------
        public string Visit(AssignmentStmt node, int level)
        {
            // Formats an assignment statement: [indent] var := expression;
            string indent = GeneralUtils.GetIndentation(level);
            return $"{indent}{node.Variable.Unparse()} := {node.Expression.Unparse()};";
        }

        public string Visit(ReturnStmt node, int level)
        {
            // Formats a return statement: [indent] return expression;
            string indent = GeneralUtils.GetIndentation(level);
            return $"{indent}return {node.Expression.Unparse()};";
        }

        public string Visit(BlockStmt node, int level)
        {
            // Handles a block of statements (e.g., function body, if/loop body).
            // Manages braces and recursively unparses child statements with an increased indentation level.
            string indent = GeneralUtils.GetIndentation(level);
            string result = indent + "{\n";

            foreach (var stmt in node.Statements)
            {
                // Unparse child statements at the next indentation level (level + 1)
                result += stmt.Accept(this, level + 1) + "\n";
            }

            result += indent + "}";
            return result;
        }

    }

}