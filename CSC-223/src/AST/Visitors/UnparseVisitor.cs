/**
 * UnparseVisitor.cs
 *
 * Converts an Abstract Syntax Tree (AST) back into source-like code.
 * This visitor traverses expression and statement nodes, reconstructing
 * their textual representations with correct syntax and indentation.
 *
 * Bugs: None known at this time.
 *
 * @author Rahul, Rick, Zachary
 * @date 2025-11-10
 */

using System;
using System.Text;
using AST;
using Utilities;

namespace AST
{
    /// <summary>
    /// Visitor class that converts AST nodes back into a string representation
    /// resembling source code. Handles expressions, assignments, blocks, and
    /// return statements while maintaining indentation levels for readability.
    /// </summary>
    public class UnparseVisitor : IVisitor<int, string>
    {
        #region Entry Points
        /// <summary>
        /// Converts an expression node into its source-like string representation.
        /// </summary>
        /// <param name="node">The expression node to unparse.</param>
        /// <param name="level">Indentation level for formatting (default: 0).</param>
        /// <returns>A string containing the unparsed expression.</returns>
        public string Unparse(ExpressionNode node, int level = 0)
        {
            return node.Accept(this, level);
        }

        /// <summary>
        /// Converts a statement node into its source-like string representation.
        /// </summary>
        /// <param name="stmt">The statement node to unparse.</param>
        /// <param name="level">Indentation level for formatting (default: 0).</param>
        /// <returns>A string containing the unparsed statement.</returns>
        public string Unparse(Statement stmt, int level = 0)
        {
            return stmt.Accept(this, level);
        }
        #endregion

        #region Expression Visitors
        /// <summary>
        /// Converts a PlusNode into a parenthesized addition expression.
        /// </summary>
        public string Visit(PlusNode node, int level)
        {
            // Recursively unparse both operands
            string left = node.Left.Accept(this, level);
            string right = node.Right.Accept(this, level);

            return $"({left} + {right})";
        }

        /// <summary>
        /// Converts a MinusNode into a parenthesized subtraction expression.
        /// </summary>
        public string Visit(MinusNode node, int level)
        {
            string left = node.Left.Accept(this, level);
            string right = node.Right.Accept(this, level);

            return $"({left} - {right})";
        }

        /// <summary>
        /// Converts a TimesNode into a parenthesized multiplication expression.
        /// </summary>
        public string Visit(TimesNode node, int level)
        {
            string left = node.Left.Accept(this, level);
            string right = node.Right.Accept(this, level);

            return $"({left} * {right})";
        }

        /// <summary>
        /// Converts a FloatDivNode into a parenthesized floating-point division expression.
        /// </summary>
        public string Visit(FloatDivNode node, int level)
        {
            string left = node.Left.Accept(this, level);
            string right = node.Right.Accept(this, level);

            return $"({left} / {right})";
        }

        /// <summary>
        /// Converts an IntDivNode into a parenthesized integer division expression.
        /// </summary>
        public string Visit(IntDivNode node, int level)
        {
            string left = node.Left.Accept(this, level);
            string right = node.Right.Accept(this, level);

            return $"({left} // {right})";
        }

        /// <summary>
        /// Converts a ModulusNode into a parenthesized modulus expression.
        /// </summary>
        public string Visit(ModulusNode node, int level)
        {
            string left = node.Left.Accept(this, level);
            string right = node.Right.Accept(this, level);

            return $"({left} % {right})";
        }

        /// <summary>
        /// Converts an ExponentiationNode into a parenthesized exponentiation expression.
        /// </summary>
        public string Visit(ExponentiationNode node, int level)
        {
            string left = node.Left.Accept(this, level);
            string right = node.Right.Accept(this, level);

            return $"({left} ** {right})";
        }

        /// <summary>
        /// Converts a literal node (e.g., number, string) to its text representation.
        /// </summary>
        public string Visit(LiteralNode node, int level)
        {
            return node.Value.ToString();
        }

        /// <summary>
        /// Converts a variable node to its identifier name.
        /// </summary>
        public string Visit(VariableNode node, int level)
        {
            return node.Name;
        }
        #endregion

        #region Statement Visitors
        /// <summary>
        /// Converts an assignment statement into its source representation.
        /// Includes indentation and the ":=" operator syntax.
        /// </summary>
        /// <param name="node">Assignment statement node.</param>
        /// <param name="level">Current indentation level.</param>
        /// <returns>Formatted assignment statement as string.</returns>
        public string Visit(AssignmentStmt node, int level)
        {
            // Apply indentation for proper code structure
            string indent = GeneralUtils.GetIndentation(level);

            // Combine variable, expression, and ending semicolon
            return $"{indent}{node.Variable.Unparse()} := {node.Expression.Unparse()};";
        }

        /// <summary>
        /// Converts a return statement into its string representation.
        /// </summary>
        /// <param name="node">Return statement node.</param>
        /// <param name="level">Indentation level.</param>
        /// <returns>Formatted return statement string.</returns>
        public string Visit(ReturnStmt node, int level)
        {
            string indent = GeneralUtils.GetIndentation(level);
            return $"{indent}return {node.Expression.Unparse()};";
        }

        /// <summary>
        /// Converts a block statement into a properly indented string block.
        /// </summary>
        /// <param name="node">Block statement containing multiple statements.</param>
        /// <param name="level">Indentation level for nesting.</param>
        /// <returns>Formatted multi-line code block.</returns>
        public string Visit(BlockStmt node, int level)
        {
            string indent = GeneralUtils.GetIndentation(level);

            // Begin block with opening brace and newline
            string result = indent + "{\n";

            // Recursively unparse each inner statement with one level deeper indentation
            foreach (var stmt in node.Statements)
            {
                result += stmt.Accept(this, level + 1) + "\n";
            }

            // Close block with matching indentation
            result += indent + "}";

            return result;
        }
        #endregion
    }
}
