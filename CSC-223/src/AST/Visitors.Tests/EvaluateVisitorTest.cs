using System;
using Xunit;
using AST;
using Utilities;
using System.Collections.Generic;

namespace AST.Tests
{
    /// <summary>
    /// Tests for EvaluateVisitor to verify correct evaluation of AST nodes.
    /// </summary>
    public class EvaluateVisitorTests
    {
        private readonly EvaluateVisitor _visitor;

        public EvaluateVisitorTests()
        {
            _visitor = new EvaluateVisitor();
        }

        private SymbolTable<string, object> CreateTable(SymbolTable<string, object> parent = null)
        {
            return new SymbolTable<string, object>(parent);
        }

        // ============================================================================
        // BASIC NODE TESTS
        // ============================================================================

        [Fact]
        public void TestVisit_LiteralNode_ReturnsValue()
        {
            var node = new LiteralNode(42);
            var result = node.Accept(_visitor, CreateTable());
            Assert.Equal(42, result);
        }

        [Fact]
        public void TestVisit_VariableNode_ReturnsValue()
        {
            var table = CreateTable();
            table.Add("x", 5);
            var node = new VariableNode("x");
            var result = node.Accept(_visitor, table);
            Assert.Equal(5, result);
        }

        [Fact]
        public void TestVisit_VariableNode_Undefined_ThrowsException()
        {
            var table = CreateTable();
            var node = new VariableNode("y");
            Assert.Throws<EvaluateVisitor.EvaluationException>(() => node.Accept(_visitor, table));
        }

        // ============================================================================
        // ARITHMETIC OPERATOR TESTS
        // ============================================================================

        [Fact]
        public void TestVisit_PlusNode_Ints_ReturnsSum()
        {
            var node = new PlusNode(new LiteralNode(3), new LiteralNode(4));
            var result = node.Accept(_visitor, CreateTable());
            Assert.Equal(7, result);
        }

        [Fact]
        public void TestVisit_MinusNode_MixedTypes_ReturnsDifference()
        {
            var node = new MinusNode(new LiteralNode(5.5), new LiteralNode(2));
            var result = node.Accept(_visitor, CreateTable());
            Assert.Equal(3.5, (double)result, 5);
        }

        [Fact]
        public void TestVisit_TimesNode_Ints_ReturnsProduct()
        {
            var node = new TimesNode(new LiteralNode(3), new LiteralNode(5));
            var result = node.Accept(_visitor, CreateTable());
            Assert.Equal(15, result);
        }

        [Fact]
        public void TestVisit_FloatDivNode_Valid_ReturnsQuotient()
        {
            var node = new FloatDivNode(new LiteralNode(10.0), new LiteralNode(2.0));
            var result = node.Accept(_visitor, CreateTable());
            Assert.Equal(5.0, (double)result, 5);
        }

        [Fact]
        public void TestVisit_FloatDivNode_DivideByZero_ThrowsException()
        {
            var node = new FloatDivNode(new LiteralNode(10.0), new LiteralNode(0.0));
            Assert.Throws<EvaluateVisitor.EvaluationException>(() => node.Accept(_visitor, CreateTable()));
        }

        [Fact]
        public void TestVisit_IntDivNode_Valid_ReturnsIntegerDivision()
        {
            var node = new IntDivNode(new LiteralNode(7), new LiteralNode(2));
            var result = node.Accept(_visitor, CreateTable());
            Assert.Equal(3, result);
        }

        [Fact]
        public void TestVisit_IntDivNode_DivideByZero_ThrowsException()
        {
            var node = new IntDivNode(new LiteralNode(10), new LiteralNode(0));
            Assert.Throws<EvaluateVisitor.EvaluationException>(() => node.Accept(_visitor, CreateTable()));
        }

        [Fact]
        public void TestVisit_ModulusNode_Ints_ReturnsRemainder()
        {
            var node = new ModulusNode(new LiteralNode(10), new LiteralNode(3));
            var result = node.Accept(_visitor, CreateTable());
            Assert.Equal(1, result);
        }

        [Fact]
        public void TestVisit_ModulusNode_DivideByZero_ThrowsException()
        {
            var node = new ModulusNode(new LiteralNode(10), new LiteralNode(0));
            Assert.Throws<EvaluateVisitor.EvaluationException>(() => node.Accept(_visitor, CreateTable()));
        }

        [Fact]
        public void TestVisit_ExponentiationNode_Ints_ReturnsPower()
        {
            var node = new ExponentiationNode(new LiteralNode(2), new LiteralNode(3));
            var result = node.Accept(_visitor, CreateTable());
            Assert.Equal(8.0, (double)result, 5);
        }

        // ============================================================================
        // ASSIGNMENT TESTS
        // ============================================================================

        [Fact]
        public void TestVisit_AssignmentStmt_NewVariable_AddsToTable()
        {
            var table = CreateTable();
            var stmt = new AssignmentStmt(new VariableNode("x"), new LiteralNode(9));
            stmt.Accept(_visitor, table);

            Assert.True(table.ContainsKey("x"));
            Assert.Equal(9, table["x"]);
        }

        [Fact]
        public void TestVisit_AssignmentStmt_ExistingVariable_UpdatesValue()
        {
            var table = CreateTable();
            table.Add("x", 1);
            var stmt = new AssignmentStmt(new VariableNode("x"), new LiteralNode(5));

            stmt.Accept(_visitor, table);
            Assert.Equal(5, table["x"]);
        }

        [Fact]
        public void TestVisit_AssignmentStmt_UsesExpressionResult()
        {
            var table = CreateTable();
            var expr = new PlusNode(new LiteralNode(2), new LiteralNode(3));
            var stmt = new AssignmentStmt(new VariableNode("sum"), expr);

            stmt.Accept(_visitor, table);
            Assert.Equal(5, table["sum"]);
        }

        // ============================================================================
        // BLOCK TESTS
        // ============================================================================

        [Fact]
        public void TestVisit_BlockStmt_ExecutesAllStatements()
        {
            var blockTable = CreateTable();
            var block = new BlockStmt(blockTable);
            block.Statements.Add(new AssignmentStmt(new VariableNode("a"), new LiteralNode(2)));
            block.Statements.Add(new AssignmentStmt(new VariableNode("b"), new PlusNode(new VariableNode("a"), new LiteralNode(3))));

            block.Accept(_visitor, blockTable);

            Assert.Equal(2, blockTable["a"]);
            Assert.Equal(5, blockTable["b"]);
        }

        [Fact]
        public void TestVisit_BlockStmt_InnerScopeCanAccessOuterVariables()
        {
            var outer = CreateTable();
            outer.Add("x", 10);
            var outerBlock = new BlockStmt(outer);

            var inner = new SymbolTable<string, object>(outer);
            var innerBlock = new BlockStmt(inner);
            innerBlock.Statements.Add(new AssignmentStmt(new VariableNode("y"), new PlusNode(new VariableNode("x"), new LiteralNode(5))));

            outerBlock.Statements.Add(innerBlock);
            outerBlock.Accept(_visitor, outer);

            Assert.Equal(15, inner["y"]);
        }

        [Fact]
        public void TestVisit_BlockStmt_InnerShadowVariable_DoesNotAffectOuter()
        {
            var outer = CreateTable();
            outer.Add("x", 5);
            var outerBlock = new BlockStmt(outer);

            var inner = new SymbolTable<string, object>(outer);
            var innerBlock = new BlockStmt(inner);
            innerBlock.Statements.Add(new AssignmentStmt(new VariableNode("x"), new LiteralNode(20)));

            outerBlock.Statements.Add(innerBlock);
            outerBlock.Accept(_visitor, outer);

            // Shadowed variable exists only in inner
            Assert.Equal(5, outer["x"]);
            Assert.Equal(20, inner["x"]);
        }

        // ============================================================================
        // RETURN TESTS
        // ============================================================================

        [Fact]
        public void TestVisit_ReturnStmt_ReturnsExpressionValue()
        {
            var table = CreateTable();
            var ret = new ReturnStmt(new PlusNode(new LiteralNode(2), new LiteralNode(3)));
            var result = ret.Accept(_visitor, table);

            Assert.Equal(5, result);
        }

        [Fact]
        public void TestEvaluate_BlockWithReturn_StopsAtReturn()
        {
            var block = new BlockStmt(new SymbolTable<string, object>(null));
            block.Statements.Add(new AssignmentStmt(new VariableNode("a"), new LiteralNode(10)));
            block.Statements.Add(new ReturnStmt(new PlusNode(new VariableNode("a"), new LiteralNode(5))));
            block.Statements.Add(new AssignmentStmt(new VariableNode("b"), new LiteralNode(100))); // should not execute

            var result = _visitor.Evaluate(block);

            Assert.Equal(15, result);
        }

        // ============================================================================
        // EXCEPTION TESTS
        // ============================================================================

        [Fact]
        public void TestVisit_ModulusNode_FloatDivideByZero_Throws()
        {
            var node = new ModulusNode(new LiteralNode(3.5), new LiteralNode(0.0));
            Assert.Throws<EvaluateVisitor.EvaluationException>(() => node.Accept(_visitor, CreateTable()));
        }

        [Fact]
        public void TestVisit_ComplexExpression_ReturnsExpected()
        {
            // ((2 + 3) * (4 - 1)) / 5 = 3
            var expr = new FloatDivNode(
                new TimesNode(
                    new PlusNode(new LiteralNode(2), new LiteralNode(3)),
                    new MinusNode(new LiteralNode(4), new LiteralNode(1))
                ),
                new LiteralNode(5)
            );

            var result = expr.Accept(_visitor, CreateTable());
            Assert.Equal(3.0, (double)result, 5);
        }
    }
}
