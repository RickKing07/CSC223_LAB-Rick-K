using System;
using Xunit;
using AST;
using Utilities;

namespace AST.Tests
{
    /// <summary>
    /// Comprehensive test suite for UnparseVisitor class.
    /// Tests the visitor's ability to convert AST nodes back to source-like text.
    /// </summary>
    public class UnparseVisitorTests
    {
        private UnparseVisitor _visitor;

        public UnparseVisitorTests()
        {
            _visitor = new UnparseVisitor();
        }

        // ============================================================================
        // LITERAL AND VARIABLE TESTS
        // ============================================================================

        [Fact]
        public void TestUnparse_IntegerLiteral()
        {
            var literal = new LiteralNode(42);

            var result = literal.Accept(_visitor, 0);

            Assert.Contains("42", result);
        }

        [Fact]
        public void TestUnparse_FloatingPointLiteral()
        {
            var literal = new LiteralNode(3.14159);

            var result = literal.Accept(_visitor, 0);

            Assert.Contains("3.14159", result);
        }

        [Fact]
        public void TestUnparse_NegativeLiteral()
        {
            var literal = new LiteralNode(-100);

            var result = literal.Accept(_visitor, 0);

            Assert.Contains("-100", result);
        }

        [Fact]
        public void TestUnparse_ZeroLiteral()
        {
            var literal = new LiteralNode(0);

            var result = literal.Accept(_visitor, 0);

            Assert.Contains("0", result);
        }

        [Fact]
        public void TestUnparse_SimpleVariable()
        {
            var variable = new VariableNode("x");

            var result = variable.Accept(_visitor, 0);

            Assert.Contains("x", result);
        }

        [Fact]
        public void TestUnparse_LongVariableName()
        {
            var variable = new VariableNode("myLongVariableName");

            var result = variable.Accept(_visitor, 0);

            Assert.Contains("myLongVariableName", result);
        }

        [Fact]
        public void TestUnparse_VariableWithNumbers()
        {
            var variable = new VariableNode("var123");

            var result = variable.Accept(_visitor, 0);

            Assert.Contains("var123", result);
        }

        // ============================================================================
        // BINARY OPERATOR TESTS
        // ============================================================================

        [Fact]
        public void TestUnparse_Addition()
        {
            // 5 + 3
            var expr = new PlusNode(new LiteralNode(5), new LiteralNode(3));

            var result = expr.Accept(_visitor, 0);

            Assert.Contains("5", result);
            Assert.Contains("+", result);
            Assert.Contains("3", result);
        }

        [Fact]
        public void TestUnparse_Subtraction()
        {
            // 10 - 4
            var expr = new MinusNode(new LiteralNode(10), new LiteralNode(4));

            var result = expr.Accept(_visitor, 0);

            Assert.Contains("10", result);
            Assert.Contains("-", result);
            Assert.Contains("4", result);
        }

        [Fact]
        public void TestUnparse_Multiplication()
        {
            // 6 * 7
            var expr = new TimesNode(new LiteralNode(6), new LiteralNode(7));

            var result = expr.Accept(_visitor, 0);

            Assert.Contains("6", result);
            Assert.Contains("*", result);
            Assert.Contains("7", result);
        }

        [Fact]
        public void TestUnparse_FloatDivision()
        {
            // 8 / 2
            var expr = new FloatDivNode(new LiteralNode(8), new LiteralNode(2));

            var result = expr.Accept(_visitor, 0);

            Assert.Contains("8", result);
            Assert.Contains("/", result);
            Assert.Contains("2", result);
            // Should NOT contain "//" (that's integer division)
            Assert.DoesNotContain("//", result);
        }

        [Fact]
        public void TestUnparse_IntegerDivision()
        {
            // 10 // 3
            var expr = new IntDivNode(new LiteralNode(10), new LiteralNode(3));

            var result = expr.Accept(_visitor, 0);

            Assert.Contains("10", result);
            Assert.Contains("//", result);
            Assert.Contains("3", result);
        }

        [Fact]
        public void TestUnparse_Modulus()
        {
            // 17 % 5
            var expr = new ModulusNode(new LiteralNode(17), new LiteralNode(5));

            var result = expr.Accept(_visitor, 0);

            Assert.Contains("17", result);
            Assert.Contains("%", result);
            Assert.Contains("5", result);
        }

        [Fact]
        public void TestUnparse_Exponentiation()
        {
            // 2 ** 8
            var expr = new ExponentiationNode(new LiteralNode(2), new LiteralNode(8));

            var result = expr.Accept(_visitor, 0);

            Assert.Contains("2", result);
            Assert.Contains("**", result);
            Assert.Contains("8", result);
        }

        [Fact]
        public void TestUnparse_AdditionWithVariables()
        {
            // x + y
            var expr = new PlusNode(new VariableNode("x"), new VariableNode("y"));

            var result = expr.Accept(_visitor, 0);

            Assert.Contains("x", result);
            Assert.Contains("+", result);
            Assert.Contains("y", result);
        }

        [Fact]
        public void TestUnparse_MixedLiteralsAndVariables()
        {
            // x + 5
            var expr = new PlusNode(new VariableNode("x"), new LiteralNode(5));

            var result = expr.Accept(_visitor, 0);

            Assert.Contains("x", result);
            Assert.Contains("+", result);
            Assert.Contains("5", result);
        }

        // ============================================================================
        // COMPLEX EXPRESSION TESTS
        // ============================================================================

        [Fact]
        public void TestUnparse_TwoLevelExpression()
        {
            // (2 + 3) * 4
            var left = new PlusNode(new LiteralNode(2), new LiteralNode(3));
            var expr = new TimesNode(left, new LiteralNode(4));

            var result = expr.Accept(_visitor, 0);

            Assert.Contains("2", result);
            Assert.Contains("3", result);
            Assert.Contains("4", result);
            Assert.Contains("+", result);
            Assert.Contains("*", result);
        }

        [Fact]
        public void TestUnparse_ThreeLevelExpression()
        {
            // ((5 + 3) * 2) - 1
            var innermost = new PlusNode(new LiteralNode(5), new LiteralNode(3));
            var middle = new TimesNode(innermost, new LiteralNode(2));
            var expr = new MinusNode(middle, new LiteralNode(1));

            var result = expr.Accept(_visitor, 0);

            Assert.Contains("5", result);
            Assert.Contains("3", result);
            Assert.Contains("2", result);
            Assert.Contains("1", result);
            Assert.Contains("+", result);
            Assert.Contains("*", result);
            Assert.Contains("-", result);
        }

        [Fact]
        public void TestUnparse_ComplexNestedExpression()
        {
            // (2 + 3) * (10 - 4)
            var left = new PlusNode(new LiteralNode(2), new LiteralNode(3));
            var right = new MinusNode(new LiteralNode(10), new LiteralNode(4));
            var expr = new TimesNode(left, right);

            var result = expr.Accept(_visitor, 0);

            Assert.Contains("2", result);
            Assert.Contains("3", result);
            Assert.Contains("10", result);
            Assert.Contains("4", result);
            Assert.Contains("+", result);
            Assert.Contains("-", result);
            Assert.Contains("*", result);
        }

        [Fact]
        public void TestUnparse_DeeplyNestedExpression()
        {
            // (((a + b) * c) - d) / e
            var innermost = new PlusNode(new VariableNode("a"), new VariableNode("b"));
            var level2 = new TimesNode(innermost, new VariableNode("c"));
            var level3 = new MinusNode(level2, new VariableNode("d"));
            var expr = new FloatDivNode(level3, new VariableNode("e"));

            var result = expr.Accept(_visitor, 0);

            Assert.Contains("a", result);
            Assert.Contains("b", result);
            Assert.Contains("c", result);
            Assert.Contains("d", result);
            Assert.Contains("e", result);
            Assert.Contains("+", result);
            Assert.Contains("*", result);
            Assert.Contains("-", result);
            Assert.Contains("/", result);
        }

        [Fact]
        public void TestUnparse_AllOperatorsCombined()
        {
            // (a + b) - (c * d) / (e // f) % (g ** h)
            var add = new PlusNode(new VariableNode("a"), new VariableNode("b"));
            var mult = new TimesNode(new VariableNode("c"), new VariableNode("d"));
            var intDiv = new IntDivNode(new VariableNode("e"), new VariableNode("f"));
            var floatDiv = new FloatDivNode(mult, intDiv);
            var exp = new ExponentiationNode(new VariableNode("g"), new VariableNode("h"));
            var mod = new ModulusNode(floatDiv, exp);
            var expr = new MinusNode(add, mod);

            var result = expr.Accept(_visitor, 0);

            // Verify all operators are present
            Assert.Contains("+", result);
            Assert.Contains("-", result);
            Assert.Contains("*", result);
            Assert.Contains("/", result);
            Assert.Contains("//", result);
            Assert.Contains("%", result);
            Assert.Contains("**", result);

            // Verify all variables are present
            Assert.Contains("a", result);
            Assert.Contains("b", result);
            Assert.Contains("c", result);
            Assert.Contains("d", result);
            Assert.Contains("e", result);
            Assert.Contains("f", result);
            Assert.Contains("g", result);
            Assert.Contains("h", result);
        }

        [Fact]
        public void TestUnparse_LongChainOfOperations()
        {
            // a + b - c * d / e
            var mult = new TimesNode(new VariableNode("c"), new VariableNode("d"));
            var div = new FloatDivNode(mult, new VariableNode("e"));
            var add = new PlusNode(new VariableNode("a"), new VariableNode("b"));
            var expr = new MinusNode(add, div);

            var result = expr.Accept(_visitor, 0);

            Assert.Contains("a", result);
            Assert.Contains("b", result);
            Assert.Contains("c", result);
            Assert.Contains("d", result);
            Assert.Contains("e", result);
            Assert.Contains("+", result);
            Assert.Contains("-", result);
            Assert.Contains("*", result);
            Assert.Contains("/", result);
        }

        // ============================================================================
        // STATEMENT TESTS
        // ============================================================================

        [Fact]
        public void TestUnparse_SimpleAssignment()
        {
            // x := 5;
            var stmt = new AssignmentStmt(
                new VariableNode("x"),
                new LiteralNode(5));

            var result = stmt.Accept(_visitor, 0);

            Assert.Contains("x", result);
            Assert.Contains(":=", result);
            Assert.Contains("5", result);
        }

        [Fact]
        public void TestUnparse_AssignmentWithExpression()
        {
            // result := x + y;
            var stmt = new AssignmentStmt(
                new VariableNode("result"),
                new PlusNode(new VariableNode("x"), new VariableNode("y")));

            var result = stmt.Accept(_visitor, 0);

            Assert.Contains("result", result);
            Assert.Contains(":=", result);
            Assert.Contains("x", result);
            Assert.Contains("+", result);
            Assert.Contains("y", result);
        }

        [Fact]
        public void TestUnparse_AssignmentWithComplexExpression()
        {
            // total := (x * 2) + (y / 3);
            var stmt = new AssignmentStmt(
                new VariableNode("total"),
                new PlusNode(
                    new TimesNode(new VariableNode("x"), new LiteralNode(2)),
                    new FloatDivNode(new VariableNode("y"), new LiteralNode(3))));

            var result = stmt.Accept(_visitor, 0);

            Assert.Contains("total", result);
            Assert.Contains(":=", result);
            Assert.Contains("x", result);
            Assert.Contains("y", result);
            Assert.Contains("*", result);
            Assert.Contains("+", result);
            Assert.Contains("/", result);
            Assert.Contains("2", result);
            Assert.Contains("3", result);
        }

        [Fact]
        public void TestUnparse_SimpleReturn()
        {
            // return 42;
            var stmt = new ReturnStmt(new LiteralNode(42));

            var result = stmt.Accept(_visitor, 0);

            Assert.Contains("return", result);
            Assert.Contains("42", result);
        }

        [Fact]
        public void TestUnparse_ReturnVariable()
        {
            // return x;
            var stmt = new ReturnStmt(new VariableNode("x"));

            var result = stmt.Accept(_visitor, 0);

            Assert.Contains("return", result);
            Assert.Contains("x", result);
        }

        [Fact]
        public void TestUnparse_ReturnWithExpression()
        {
            // return x + y;
            var stmt = new ReturnStmt(
                new PlusNode(new VariableNode("x"), new VariableNode("y")));

            var result = stmt.Accept(_visitor, 0);

            Assert.Contains("return", result);
            Assert.Contains("x", result);
            Assert.Contains("+", result);
            Assert.Contains("y", result);
        }

        [Fact]
        public void TestUnparse_ReturnWithComplexExpression()
        {
            // return (a ** 2) + (b ** 2);
            var stmt = new ReturnStmt(
                new PlusNode(
                    new ExponentiationNode(new VariableNode("a"), new LiteralNode(2)),
                    new ExponentiationNode(new VariableNode("b"), new LiteralNode(2))));

            var result = stmt.Accept(_visitor, 0);

            Assert.Contains("return", result);
            Assert.Contains("a", result);
            Assert.Contains("b", result);
            Assert.Contains("**", result);
            Assert.Contains("+", result);
            Assert.Contains("2", result);
        }

        // ============================================================================
        // BLOCK STATEMENT TESTS
        // ============================================================================

        [Fact]
        public void TestUnparse_EmptyBlock()
        {
            // { }
            var symbolTable = new SymbolTable<string, object>(null);
            var block = new BlockStmt(symbolTable);

            var result = block.Accept(_visitor, 0);

            Assert.Contains("{", result);
            Assert.Contains("}", result);
        }

        [Fact]
        public void TestUnparse_BlockWithSingleStatement()
        {
            // { x := 5; }
            var symbolTable = new SymbolTable<string, object>(null);
            var block = new BlockStmt(symbolTable);
            block.Statements.Add(new AssignmentStmt(
                new VariableNode("x"),
                new LiteralNode(5)));

            var result = block.Accept(_visitor, 0);

            Assert.Contains("{", result);
            Assert.Contains("}", result);
            Assert.Contains("x", result);
            Assert.Contains(":=", result);
            Assert.Contains("5", result);
        }

        [Fact]
        public void TestUnparse_BlockWithMultipleStatements()
        {
            // { x := 5; y := 10; return x + y; }
            var symbolTable = new SymbolTable<string, object>(null);
            var block = new BlockStmt(symbolTable);
            block.Statements.Add(new AssignmentStmt(
                new VariableNode("x"),
                new LiteralNode(5)));
            block.Statements.Add(new AssignmentStmt(
                new VariableNode("y"),
                new LiteralNode(10)));
            block.Statements.Add(new ReturnStmt(
                new PlusNode(new VariableNode("x"), new VariableNode("y"))));

            var result = block.Accept(_visitor, 0);

            Assert.Contains("{", result);
            Assert.Contains("}", result);
            Assert.Contains("x", result);
            Assert.Contains("y", result);
            Assert.Contains(":=", result);
            Assert.Contains("return", result);
            Assert.Contains("+", result);
        }

        [Fact]
        public void TestUnparse_BlockWithComplexStatements()
        {
            // { a := 1; b := 2; c := a + b; result := c * 2; return result; }
            var symbolTable = new SymbolTable<string, object>(null);
            var block = new BlockStmt(symbolTable);
            block.Statements.Add(new AssignmentStmt(
                new VariableNode("a"),
                new LiteralNode(1)));
            block.Statements.Add(new AssignmentStmt(
                new VariableNode("b"),
                new LiteralNode(2)));
            block.Statements.Add(new AssignmentStmt(
                new VariableNode("c"),
                new PlusNode(new VariableNode("a"), new VariableNode("b"))));
            block.Statements.Add(new AssignmentStmt(
                new VariableNode("result"),
                new TimesNode(new VariableNode("c"), new LiteralNode(2))));
            block.Statements.Add(new ReturnStmt(new VariableNode("result")));

            var result = block.Accept(_visitor, 0);

            Assert.Contains("a", result);
            Assert.Contains("b", result);
            Assert.Contains("c", result);
            Assert.Contains("result", result);
            Assert.Contains(":=", result);
            Assert.Contains("return", result);
        }

        [Fact]
        public void TestUnparse_NestedBlocks()
        {
            // { x := 1; { y := 2; return x + y; } }
            var outerTable = new SymbolTable<string, object>(null);
            var outerBlock = new BlockStmt(outerTable);
            outerBlock.Statements.Add(new AssignmentStmt(
                new VariableNode("x"),
                new LiteralNode(1)));

            var innerTable = new SymbolTable<string, object>(outerTable);
            var innerBlock = new BlockStmt(innerTable);
            innerBlock.Statements.Add(new AssignmentStmt(
                new VariableNode("y"),
                new LiteralNode(2)));
            innerBlock.Statements.Add(new ReturnStmt(
                new PlusNode(new VariableNode("x"), new VariableNode("y"))));

            outerBlock.Statements.Add(innerBlock);

            var result = outerBlock.Accept(_visitor, 0);

            Assert.Contains("x", result);
            Assert.Contains("y", result);
            Assert.Contains(":=", result);
            Assert.Contains("return", result);

            // Verify we have two pairs of braces
            int openBraceCount = 0;
            int closeBraceCount = 0;
            foreach (char c in result)
            {
                if (c == '{') openBraceCount++;
                if (c == '}') closeBraceCount++;
            }
            Assert.Equal(2, openBraceCount);
            Assert.Equal(2, closeBraceCount);
        }

        [Fact]
        public void TestUnparse_DeeplyNestedBlocks()
        {
            // { a := 1; { b := 2; { c := 3; return a + b + c; } } }
            var table1 = new SymbolTable<string, object>(null);
            var block1 = new BlockStmt(table1);
            block1.Statements.Add(new AssignmentStmt(
                new VariableNode("a"),
                new LiteralNode(1)));

            var table2 = new SymbolTable<string, object>(table1);
            var block2 = new BlockStmt(table2);
            block2.Statements.Add(new AssignmentStmt(
                new VariableNode("b"),
                new LiteralNode(2)));

            var table3 = new SymbolTable<string, object>(table2);
            var block3 = new BlockStmt(table3);
            block3.Statements.Add(new AssignmentStmt(
                new VariableNode("c"),
                new LiteralNode(3)));
            block3.Statements.Add(new ReturnStmt(
                new PlusNode(
                    new PlusNode(new VariableNode("a"), new VariableNode("b")),
                    new VariableNode("c"))));

            block2.Statements.Add(block3);
            block1.Statements.Add(block2);

            var result = block1.Accept(_visitor, 0);

            Assert.Contains("a", result);
            Assert.Contains("b", result);
            Assert.Contains("c", result);

            // Should have 3 levels of braces
            int openBraceCount = 0;
            int closeBraceCount = 0;
            foreach (char c in result)
            {
                if (c == '{') openBraceCount++;
                if (c == '}') closeBraceCount++;
            }
            Assert.Equal(3, openBraceCount);
            Assert.Equal(3, closeBraceCount);
        }

        [Fact]
        public void TestUnparse_NestedBlocksWithComplexExpressions()
        {
            // { x := 10; { y := x * 2; { z := y + x; return z / 2; } } }
            var table1 = new SymbolTable<string, object>(null);
            var block1 = new BlockStmt(table1);
            block1.Statements.Add(new AssignmentStmt(
                new VariableNode("x"),
                new LiteralNode(10)));

            var table2 = new SymbolTable<string, object>(table1);
            var block2 = new BlockStmt(table2);
            block2.Statements.Add(new AssignmentStmt(
                new VariableNode("y"),
                new TimesNode(new VariableNode("x"), new LiteralNode(2))));

            var table3 = new SymbolTable<string, object>(table2);
            var block3 = new BlockStmt(table3);
            block3.Statements.Add(new AssignmentStmt(
                new VariableNode("z"),
                new PlusNode(new VariableNode("y"), new VariableNode("x"))));
            block3.Statements.Add(new ReturnStmt(
                new FloatDivNode(new VariableNode("z"), new LiteralNode(2))));

            block2.Statements.Add(block3);
            block1.Statements.Add(block2);

            var result = block1.Accept(_visitor, 0);

            Assert.Contains("x", result);
            Assert.Contains("y", result);
            Assert.Contains("z", result);
            Assert.Contains("*", result);
            Assert.Contains("+", result);
            Assert.Contains("/", result);
        }

        // ============================================================================
        // INDENTATION TESTS
        // ============================================================================

        [Fact]
        public void TestUnparse_AssignmentWithDifferentIndentLevels()
        {
            // Test that indentation parameter affects output
            var stmt = new AssignmentStmt(
                new VariableNode("x"),
                new LiteralNode(5));

            var level0 = stmt.Accept(_visitor, 0);
            var level1 = stmt.Accept(_visitor, 1);
            var level2 = stmt.Accept(_visitor, 2);

            // All should contain the core elements
            Assert.Contains("x", level0);
            Assert.Contains("x", level1);
            Assert.Contains("x", level2);

            Assert.Contains(":=", level0);
            Assert.Contains(":=", level1);
            Assert.Contains(":=", level2);

            // Different indent levels should produce different output
            Assert.NotEqual(level0, level1);
            Assert.NotEqual(level1, level2);
        }

        [Fact]
        public void TestUnparse_ReturnWithDifferentIndentLevels()
        {
            var stmt = new ReturnStmt(new LiteralNode(42));

            var level0 = stmt.Accept(_visitor, 0);
            var level1 = stmt.Accept(_visitor, 1);
            var level3 = stmt.Accept(_visitor, 3);

            // All should contain return and value
            Assert.Contains("return", level0);
            Assert.Contains("return", level1);
            Assert.Contains("return", level3);

            Assert.Contains("42", level0);
            Assert.Contains("42", level1);
            Assert.Contains("42", level3);
        }

        [Fact]
        public void TestUnparse_BlockWithIndentation()
        {
            // Test that blocks handle indentation levels
            var symbolTable = new SymbolTable<string, object>(null);
            var block = new BlockStmt(symbolTable);
            block.Statements.Add(new AssignmentStmt(
                new VariableNode("x"),
                new LiteralNode(5)));

            var level0 = block.Accept(_visitor, 0);
            var level1 = block.Accept(_visitor, 1);

            Assert.NotEqual(level0, level1);
        }

        // ============================================================================
        // EDGE CASE AND SPECIAL TESTS
        // ============================================================================

        [Fact]
        public void TestUnparse_VeryLargeNumber()
        {
            var literal = new LiteralNode(999999999);

            var result = literal.Accept(_visitor, 0);

            Assert.Contains("999999999", result);
        }

        [Fact]
        public void TestUnparse_VerySmallNumber()
        {
            var literal = new LiteralNode(0.000001);

            var result = literal.Accept(_visitor, 0);

            // Should contain some representation of the number
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void TestUnparse_ScientificNotation()
        {
            var literal = new LiteralNode(1.23e10);

            var result = literal.Accept(_visitor, 0);

            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void TestUnparse_ExpressionWithOnlyLiterals()
        {
            // 1 + 2 + 3 + 4 + 5
            var expr = new PlusNode(
                new PlusNode(
                    new PlusNode(
                        new PlusNode(new LiteralNode(1), new LiteralNode(2)),
                        new LiteralNode(3)),
                    new LiteralNode(4)),
                new LiteralNode(5));

            var result = expr.Accept(_visitor, 0);

            Assert.Contains("1", result);
            Assert.Contains("2", result);
            Assert.Contains("3", result);
            Assert.Contains("4", result);
            Assert.Contains("5", result);
        }

        [Fact]
        public void TestUnparse_ExpressionWithOnlyVariables()
        {
            // a + b + c + d
            var expr = new PlusNode(
                new PlusNode(
                    new PlusNode(new VariableNode("a"), new VariableNode("b")),
                    new VariableNode("c")),
                new VariableNode("d"));

            var result = expr.Accept(_visitor, 0);

            Assert.Contains("a", result);
            Assert.Contains("b", result);
            Assert.Contains("c", result);
            Assert.Contains("d", result);
        }

        [Fact]
        public void TestUnparse_SingleVariableInBlock()
        {
            // { return x; }
            var symbolTable = new SymbolTable<string, object>(null);
            var block = new BlockStmt(symbolTable);
            block.Statements.Add(new ReturnStmt(new VariableNode("x")));

            var result = block.Accept(_visitor, 0);

            Assert.Contains("{", result);
            Assert.Contains("}", result);
            Assert.Contains("return", result);
            Assert.Contains("x", result);
        }

        [Fact]
        public void TestUnparse_SingleLiteralInBlock()
        {
            // { return 42; }
            var symbolTable = new SymbolTable<string, object>(null);
            var block = new BlockStmt(symbolTable);
            block.Statements.Add(new ReturnStmt(new LiteralNode(42)));

            var result = block.Accept(_visitor, 0);

            Assert.Contains("{", result);
            Assert.Contains("}", result);
            Assert.Contains("return", result);
            Assert.Contains("42", result);
        }
    }
}