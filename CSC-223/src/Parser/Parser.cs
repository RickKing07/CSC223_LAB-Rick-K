using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Formats.Asn1;
using System.Reflection;
using Utilities;
using AST;
using Tokenizer;
using System.Security.Principal;
using System.ComponentModel.Design;
using Parser.Tests;
using Xunit.Sdk;
//You are currently stuck on finishing out ParseExpression Content, Dealing with nested parenthasis
//ParseBlockStmnt, how to pass symboltable? how to pass block?
namespace Parser
{
    public class ParseException : Exception
    {
        // constructor needs since not static and base because calls parent and requires message to be stored.
        public ParseException(string message) : base(message) { }
    }
    public static class Parser
    {
        public static AST.BlockStmt Parse(string Program) //Needs to be tokenized
        {
            SymbolTable<string, object> blockScope = new SymbolTable<string, object>();
            List<string> lines = new List<string>();
            foreach (string line in Program.Split('\n'))
            {
                lines.Add(line);
            }
            if (lines.Count < 2) { throw new ParseException("Program must have atleas opening '{' and closing '}'."); }
            return ParseBlockStmt(lines, blockScope);

        }


        private static AST.ExpressionNode ParseExpression(List<Tokenizer.Token> expression)
        {
            // Handle base cases
            if (expression.Count == 0)
                throw new ParseException("Empty expression");
            if (expression.Count == 1)
                return HandleSingleToken(expression[0]);

            // Unwrap outer parentheses if they wrap the entire expression
            while (expression.Count > 1 &&
                   expression[0]._tkntype == TokenType.LEFT_PAREN &&
                   FindMatchingParen(expression, 0) == expression.Count - 1)
            {
                expression = expression.GetRange(1, expression.Count - 2);
            }

            // Re-check after unwrapping
            if (expression.Count == 1)
                return HandleSingleToken(expression[0]);

            return ParseExpressionContent(expression);
        }

        private static int FindMatchingParen(List<Tokenizer.Token> tokens, int openIndex)
        {
            int depth = 1;
            for (int i = openIndex + 1; i < tokens.Count; i++)
            {
                if (tokens[i]._tkntype == TokenType.LEFT_PAREN) depth++;
                else if (tokens[i]._tkntype == TokenType.RIGHT_PAREN) depth--;
                if (depth == 0) return i;
            }
            return -1; // Unmatched parenthesis
        }

        private static AST.ExpressionNode ParseExpressionContent(List<Tokenizer.Token> content)
        {
            if (content.Count == 0)
                throw new ParseException("Null Expression");
            if (content.Count == 1)
                return HandleSingleToken(content[0]);

            // Scan for operators at depth 0
            int depth = 0;
            int operatorCount = 0;

            for (int i = 0; i < content.Count; i++)
            {
                if (content[i]._tkntype == TokenType.LEFT_PAREN)
                {
                    depth++;
                }
                else if (content[i]._tkntype == TokenType.RIGHT_PAREN)
                {
                    depth--;
                }
                else if (depth == 0 && content[i]._tkntype == TokenType.OPERATOR)
                {
                    operatorCount++;
                    if (operatorCount > 1)
                    {
                        throw new ParseException("Multiple operators at same level - expression must be fully parenthesized");
                    }

                    // Found the operator at depth 0 - split here
                    var left = content.GetRange(0, i);
                    var right = content.GetRange(i + 1, content.Count - i - 1);

                    return CreateBinaryOperatorNode(
                        content[i]._value,
                        ParseExpression(left),
                        ParseExpression(right)
                    );
                }
            }

            // Check for unbalanced parentheses
            if (depth != 0)
                throw new ParseException("must begin with a ( and must end with a ). Missing )");

            // If we get here, no operator was found at depth 0
            throw new ParseException("Invalid operator");
        }

        // helper function to parse the content of the expression
        // private static AST.ExpressionNode ParseExpression(List<Tokenizer.Token> expression)
        // {
        //     // Parses an expression enclosed in parentheses.
        //     // Consumes only the first '(' and lets ParseExpressionContent handle the rest.
        //     if (expression.Count == 1) return HandleSingleToken(expression[0]);
        //     if (expression.Count == 0 || expression[0]._tkntype != TokenType.LEFT_PAREN || expression[expression.Count - 1]._tkntype != TokenType.RIGHT_PAREN)
        //         throw new ParseException("Missing ) or must begin with a (");

        //     // Pass everything after the first '(' into ParseExpressionContent
        //     var inner = expression.GetRange(1, expression.Count - 2);
        //     return ParseExpressionContent(inner);
        // }


        // public static AST.ExpressionNode ParseExpressionContent(List<Tokenizer.Token> content)
        // {
        //     //(5*3)+1
        //     for (int i = 0; i < content.Count; i++)
        //     {

        //         if (content.Count == 0) throw new ParseException("Null Expression");
        //         if (content.Count == 1) return HandleSingleToken(content[i]);
        //         if (content[0]._tkntype == TokenType.LEFT_PAREN && content[content.Count - 1]._tkntype == TokenType.RIGHT_PAREN) return ParseExpression(content.GetRange(1, content.Count - 2));

        //         if (content[i]._tkntype == TokenType.LEFT_PAREN)
        //         {
        //             int depth = 1;
        //             i++;

        //             while (depth > 0 && i < content.Count)
        //             {
        //                 if (content[i]._tkntype == TokenType.RIGHT_PAREN)
        //                 {
        //                     depth--;
        //                 }
        //                 if (content[i]._tkntype == TokenType.LEFT_PAREN)
        //                 {
        //                     depth++;
        //                 }
        //                 i++;
        //             }
        //         }
        //         if (i >= content.Count) throw new ParseException("Imalenced PArenthesisisisisisisisisis");
        //         if (content[i]._tkntype == TokenType.OPERATOR)
        //         {
        //             return CreateBinaryOperatorNode(content[i]._value, ParseExpression(content.GetRange(0, i)), ParseExpression(content.GetRange(i + 1, content.Count - i - 1)));
        //         }
        //     }
        //     throw new ParseException("Invalid operator");
        // }

        public static AST.ExpressionNode HandleSingleToken(Tokenizer.Token token)
        {
            if (token._tkntype == TokenType.INTEGER) { return new LiteralNode(int.Parse(token._value)); }
            if (token._tkntype == TokenType.FLOAT) { return new LiteralNode(double.Parse(token._value)); }
            if (token._tkntype == TokenType.VARIABLE) { return new VariableNode(token._value); }
            throw new ParseException("Unexpected token may not not float or integer or variable");
        }

        public static AST.ExpressionNode CreateBinaryOperatorNode(string op, AST.ExpressionNode l, AST.ExpressionNode r)
        {
            if (op == TokenConstants.PLUS) { return new AST.PlusNode(l, r); }
            if (op == TokenConstants.MINUS) { return new AST.MinusNode(l, r); }
            if (op == TokenConstants.TIMES) { return new AST.TimesNode(l, r); }
            if (op == TokenConstants.INT_DIV) { return new AST.IntDivNode(l, r); } // This is a float division
            if (op == TokenConstants.FLOAT_DIV) { return new AST.FloatDivNode(l, r); }
            if (op == TokenConstants.MOD) { return new AST.ModulusNode(l, r); }
            if (op == TokenConstants.EXP) { return new AST.ExponentiationNode(l, r); }

            throw new ParseException($"Invalid operator has been used: {op}");
        }

        public static AST.VariableNode ParseVariableNode(string variable)
        {
            if (variable == null) { throw new ParseException("Variable is null"); }
            return new AST.VariableNode(variable);
        }

        // Individual Statements
        public static AST.AssignmentStmt ParseAssignmentStmt(List<Tokenizer.Token> content, SymbolTable<string, object> keyval)
        {
            if (content.Count < 3) throw new ParseException("Assignement statement at least need three tokens");

            // check if the first token is a variable
            if (content[1]._tkntype == TokenType.ASSIGNMENT)
            {
                keyval.Add(new KeyValuePair<string, object>(content[0]._value, null));
                return new AST.AssignmentStmt(ParseVariableNode(content[0]._value), ParseExpression(content.GetRange(2, content.Count - 2)));
            }
            throw new ParseException("Assignement statement must have an assignment operator");
        }

        public static AST.ReturnStmt ParseReturnStatement(List<Tokenizer.Token> content)
        {
            if (content.Count < 2) { throw new ParseException("Return statement must have at least two tokens"); }

            if (content[0]._tkntype == TokenType.RETURN)
            {
                return new AST.ReturnStmt(ParseExpression(content.GetRange(1, content.Count - 1)));
            }
            throw new ParseException("Return statement must start with return keyword");
        }

        public static AST.Statement ParseStatement(List<Tokenizer.Token> content, SymbolTable<string, object> keyval)
        {
            if (content[0]._tkntype == TokenType.RETURN) { return ParseReturnStatement(content); }
            if (content[1]._tkntype == TokenType.ASSIGNMENT) //CHANGED: from content[0] to content[1]
            {
                return ParseAssignmentStmt(content, keyval);
            }
            throw new ParseException("Invalid statement");
        }

        // Blocks
        public static void ParseStmtList(List<string> lines, BlockStmt stmt)
        {
            SymbolTable<string, object> Data = new SymbolTable<string, object>();

            //line by line
            var tknzier = new TokenizerImpl();
            int i = 0;
            while (i < lines.Count)
            {
                string line = lines[i].Trim();
                var content = tknzier.Tokenize(line);

                // Skip lines with no tokens (including empty lines)
                if (content.Count == 0)
                {
                    i++;
                    continue;
                }

                if (content[0]._tkntype == TokenType.LEFT_CURLY)
                {
                    // add everything Blockstmt handles it with peeling head recursion
                    var block = ParseBlockStmt(lines.GetRange(i, lines.Count - i), Data);
                    stmt.Statements.Add(block);

                    // eat all the lines outer and recursion will take care of inner
                    int curlyCount = 1;
                    int lineBeingEaten = i + 1;
                    while (lineBeingEaten < lines.Count && curlyCount > 0)
                    {
                        foreach (var token in tknzier.Tokenize(lines[lineBeingEaten]))
                        {
                            if (token._tkntype == TokenType.LEFT_CURLY) { curlyCount++; }
                            else if (token._tkntype == TokenType.RIGHT_CURLY) { curlyCount--; }
                        }
                        lineBeingEaten++;
                    }
                    i += lineBeingEaten;

                }
                else if (content[0]._tkntype == TokenType.RIGHT_CURLY)
                {
                    return;
                }
                else
                {
                    var onelinerStmt = ParseStatement(content, Data);
                    stmt.Statements.Add(onelinerStmt);
                    i++;
                }
            }
        }

        public static AST.BlockStmt ParseBlockStmt(List<string> lines, SymbolTable<string, object> keyval)
        {
            var tknzier = new TokenizerImpl();
            List<Tokenizer.Token> content = [];
            content.AddRange(tknzier.Tokenize(lines[0]));
            content.AddRange(tknzier.Tokenize(lines[lines.Count - 1]));

            if (content[0]._tkntype != TokenType.LEFT_CURLY || content[1]._tkntype != TokenType.RIGHT_CURLY)
            {
                throw new ParseException("Block must start with '{' and end with '}'");
            }
            SymbolTable<string, object> blockScope = new SymbolTable<string, object>(keyval); //how to use symbol table here?
            AST.BlockStmt Block = new BlockStmt([]);

            ParseStmtList(lines.GetRange(1, lines.Count - 1), Block);

            return Block;

        }
    }
}