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

        // helper function to parse the content of the expression
        private static AST.ExpressionNode ParseExpression(List<Tokenizer.Token> expression)
        {
            if (expression[0]._tkntype == TokenType.LEFT_PAREN && expression[expression.Count - 1]._tkntype == TokenType.RIGHT_PAREN) //check for parenth
            {
                List<Tokenizer.Token> sublist = expression.GetRange(1, expression.Count - 2); //feeds list of tokens to parseexpression content (does not include the left paren)
                return ParseExpressionContent(sublist);
            }
            else
            {
                throw new ParseException("Expression syntax is invald, must begin with a ( and must end with a )"); //if it does not start and end with '(' and ')' it is invalid
            }

            //Feed all but the initial left paren into parse expresssion content
            //in expression content, if you find a left paren, feed it to parse expression
            //if you see a right paren, leave
            //ultimately, in parseexpressioncontent, save a l and a r to call createbinary node (the l might be composed of a parse expression content)
        }



        public static AST.ExpressionNode ParseExpressionContent(List<Tokenizer.Token> content)
        {
            //keep track of a left and right and call cbn on them
            if (content.Count == 0) { throw new ParseException("No content"); }
            if (content.Count == 1) { return HandleSingleToken(content[0]); } //This will handle things like 4

            for (int i = 0; i < content.Count; i++)
            {
                if (content[i]._tkntype == TokenType.LEFT_PAREN)
                {
                    // if (content[content.Count - 1]._tkntype == TokenType.RIGHT_PAREN) { return ParseExpression(content); } ((3+4) * (6+4))
                    while (content[i]._tkntype != TokenType.RIGHT_PAREN) { i++; }
                }

                if (content[i]._tkntype == TokenType.OPERATOR)   //if its an operator, left stuff is an expression node, right stuff is an expression node
                {
                    return CreateBinaryOperatorNode(content[i]._value, ParseExpressionContent(content.GetRange(0, i)), ParseExpressionContent(content.GetRange(i + 1, content.Count - i - 1))); //Return binaryopnode
                }
            }
            throw new ParseException("Not a valid expression syntax");
        }


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
            List<Tokenizer.Token> content = [];



            foreach (string line in lines)
            {
                content = [];
                content.AddRange(tknzier.Tokenize(line));
                if (content[0]._tkntype == TokenType.LEFT_CURLY)
                {
                    ParseBlockStmt(lines.GetRange(1, lines.Count - 1), Data);
                }
                else if (content[0]._tkntype == TokenType.RIGHT_CURLY)       //add something to ensure balance
                {
                    //ParseStmtList(lines.GetRange(1, lines.Count - 1), DefaultBuilder.CreateBlockStmt(lines)); //write this correctly, and create BlockStmnt might need to be modified
                }
                else
                {
                    ParseStatement(content, Data);
                }

            }


            //if left parenthasi, call parseblockstatment

            //if right parenthasi, escape recursion by passing back all info in a symbol table, and it will return if list<string> is empty
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