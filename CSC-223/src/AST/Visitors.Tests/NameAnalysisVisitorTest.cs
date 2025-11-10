using System;
using Xunit;
using AST;
using Utilities;

namespace AST.Tests
{
    /// <summary>
    /// Comprehensive tests for NameAnalysisVisitor to verify correct variable 
    /// declaration and usage checking across all AST node types.
    /// </summary>
    public class NameAnalysisVisitorTests
    {
        private readonly NameAnalysisVisitor _visitor;

        public NameAnalysisVisitorTests()
        {
            _visitor = new NameAnalysisVisitor();
        }

        private Tuple<SymbolTable<string, object>, Statement> CreateContext(SymbolTable<string, object> table = null)
        {
            return new Tuple<SymbolTable<string, object>, Statement>(table ?? new SymbolTable<string, object>(null), null);
        }

        // ============================================================================
        // LITERAL AND VARIABLE TESTS
        // ============================================================================

        [Fact]
        public void TestVisit_LiteralNode_AlwaysValid()
        {
            var node = new LiteralNode(5);
            var result = node.Accept(_visitor, CreateContext());
            Assert.True(result);
        }

        [Fact]
        public void TestVisit_LiteralNode_FloatingPoint_AlwaysValid()
        {
            var node = new LiteralNode(3.14);
            var result = node.Accept(_visitor, CreateContext());
            Assert.True(result);
        }

        [Fact]
        public void TestVisit_LiteralNode_NegativeNumber_AlwaysValid()
        {
            var node = new LiteralNode(-100);
            var result = node.Accept(_visitor, CreateContext());
            Assert.True(result);
        }

        [Fact]
        public void TestVisit_VariableNode_DeclaredVariable_ReturnsTrue()
        {
            var table = new SymbolTable<string, object>(null);
            table.Add("x", null);
            var node = new VariableNode("x");

            var result = node.Accept(_visitor, CreateContext(table));
            Assert.True(result);
        }

        [Fact]
        public void TestVisit_VariableNode_UndeclaredVariable_ReturnsFalse()
        {
            var table = new SymbolTable<string, object>(null);
            var node = new VariableNode("y");

            var result = node.Accept(_visitor, CreateContext(table));
            Assert.False(result);
        }

        [Fact]
        public void TestVisit_VariableNode_DeclaredInParentScope_ReturnsTrue()
        {
            var parentTable = new SymbolTable<string, object>(null);
            parentTable.Add("x", null);
            var childTable = new SymbolTable<string, object>(parentTable);
            var node = new VariableNode("x");

            var result = node.Accept(_visitor, CreateContext(childTable));
            Assert.True(result);
        }

        // ============================================================================
        // ASSIGNMENT TESTS
        // ============================================================================

        [Fact]
        public void TestVisit_AssignmentStmt_SimpleLiteral_AddsVariable()
        {
            var table = new SymbolTable<string, object>(null);
            var assign = new AssignmentStmt(new VariableNode("a"), new LiteralNode(10));

            var result = assign.Accept(_visitor, CreateContext(table));

            Assert.True(result);
            Assert.True(table.ContainsKey("a"));
        }

        [Fact]
        public void TestVisit_AssignmentStmt_ExpressionWithUndeclaredVar_ReturnsFalse()
        {
            var table = new SymbolTable<string, object>(null);
            var expr = new PlusNode(new VariableNode("x"), new LiteralNode(5));
            var assign = new AssignmentStmt(new VariableNode("result"), expr);

            var result = assign.Accept(_visitor, CreateContext(table));

            Assert.False(result);
            Assert.True(table.ContainsKey("result")); // variable still declared
        }

        [Fact]
        public void TestVisit_AssignmentStmt_ExpressionWithDeclaredVar_ReturnsTrue()
        {
            var table = new SymbolTable<string, object>(null);
            table.Add("x", null);
            var expr = new PlusNode(new VariableNode("x"), new LiteralNode(5));
            var assign = new AssignmentStmt(new VariableNode("result"), expr);

            var result = assign.Accept(_visitor, CreateContext(table));

            Assert.True(result);
            Assert.True(table.ContainsKey("result"));
        }

        [Fact]
        public void TestVisit_AssignmentStmt_ComplexExpression_AllVariablesDeclared()
        {
            var table = new SymbolTable<string, object>(null);
            table.Add("a", null);
            table.Add("b", null);
            var expr = new TimesNode(
                new PlusNode(new VariableNode("a"), new LiteralNode(5)),
                new VariableNode("b"));
            var assign = new AssignmentStmt(new VariableNode("result"), expr);

            var result = assign.Accept(_visitor, CreateContext(table));

            Assert.True(result);
            Assert.True(table.ContainsKey("result"));
        }

        [Fact]
        public void TestVisit_AssignmentStmt_ComplexExpression_OneUndeclared_ReturnsFalse()
        {
            var table = new SymbolTable<string, object>(null);
            table.Add("a", null);
            var expr = new TimesNode(
                new PlusNode(new VariableNode("a"), new LiteralNode(5)),
                new VariableNode("b")); // b undeclared
            var assign = new AssignmentStmt(new VariableNode("result"), expr);

            var result = assign.Accept(_visitor, CreateContext(table));

            Assert.False(result);
        }

        [Fact]
        public void TestVisit_AssignmentStmt_ChainedAssignments_Valid()
        {
            var table = new SymbolTable<string, object>(null);
            var block = new BlockStmt(table);
            block.Statements.Add(new AssignmentStmt(new VariableNode("x"), new LiteralNode(5)));
            block.Statements.Add(new AssignmentStmt(new VariableNode("y"), new VariableNode("x")));
            block.Statements.Add(new AssignmentStmt(new VariableNode("z"), new VariableNode("y")));

            var result = block.Accept(_visitor, CreateContext(table));

            Assert.True(result);
        }

        // ============================================================================
        // BINARY OPERATOR TESTS
        // ============================================================================

        [Theory]
        [InlineData(typeof(PlusNode))]
        [InlineData(typeof(MinusNode))]
        [InlineData(typeof(TimesNode))]
        [InlineData(typeof(FloatDivNode))]
        [InlineData(typeof(IntDivNode))]
        [InlineData(typeof(ModulusNode))]
        [InlineData(typeof(ExponentiationNode))]
        public void TestVisit_BinaryOperators_BothOperandsDeclared_ReturnsTrue(Type nodeType)
        {
            var table = new SymbolTable<string, object>(null);
            table.Add("a", null);
            table.Add("b", null);

            var left = new VariableNode("a");
            var right = new VariableNode("b");
            dynamic node = Activator.CreateInstance(nodeType, left, right);

            var result = node.Accept(_visitor, CreateContext(table));

            Assert.True(result);
        }

        [Fact]
        public void TestVisit_BinaryOperators_LeftUndeclared_ReturnsFalse()
        {
            var table = new SymbolTable<string, object>(null);
            table.Add("b", null);

            var expr = new PlusNode(new VariableNode("a"), new VariableNode("b"));
            var result = expr.Accept(_visitor, CreateContext(table));

            Assert.False(result);
        }

        [Fact]
        public void TestVisit_BinaryOperators_RightUndeclared_ReturnsFalse()
        {
            var table = new SymbolTable<string, object>(null);
            table.Add("a", null);

            var expr = new PlusNode(new VariableNode("a"), new VariableNode("b"));
            var result = expr.Accept(_visitor, CreateContext(table));

            Assert.False(result);
        }

        [Fact]
        public void TestVisit_BinaryOperators_BothUndeclared_ReturnsFalse()
        {
            var table = new SymbolTable<string, object>(null);

            var expr = new PlusNode(new VariableNode("a"), new VariableNode("b"));
            var result = expr.Accept(_visitor, CreateContext(table));

            Assert.False(result);
        }

        [Fact]
        public void TestVisit_BinaryOperators_WithLiterals_ReturnsTrue()
        {
            var table = new SymbolTable<string, object>(null);

            var expr = new PlusNode(new LiteralNode(5), new LiteralNode(3));
            var result = expr.Accept(_visitor, CreateContext(table));

            Assert.True(result);
        }

        [Fact]
        public void TestVisit_NestedBinaryOperators_AllDeclared_ReturnsTrue()
        {
            var table = new SymbolTable<string, object>(null);
            table.Add("a", null);
            table.Add("b", null);
            table.Add("c", null);

            // (a + b) * c
            var expr = new TimesNode(
                new PlusNode(new VariableNode("a"), new VariableNode("b")),
                new VariableNode("c"));

            var result = expr.Accept(_visitor, CreateContext(table));

            Assert.True(result);
        }

        [Fact]
        public void TestVisit_DeeplyNestedOperators_OneUndeclared_ReturnsFalse()
        {
            var table = new SymbolTable<string, object>(null);
            table.Add("a", null);
            table.Add("b", null);

            // ((a + b) * c) - d  -- c and d undeclared
            var expr = new MinusNode(
                new TimesNode(
                    new PlusNode(new VariableNode("a"), new VariableNode("b")),
                    new VariableNode("c")),
                new VariableNode("d"));

            var result = expr.Accept(_visitor, CreateContext(table));

            Assert.False(result);
        }

        // ============================================================================
        // RETURN STATEMENT TESTS
        // ============================================================================

        [Fact]
        public void TestVisit_ReturnStmt_Literal_ReturnsTrue()
        {
            var table = new SymbolTable<string, object>(null);
            var stmt = new ReturnStmt(new LiteralNode(42));

            var result = stmt.Accept(_visitor, CreateContext(table));

            Assert.True(result);
        }

        [Fact]
        public void TestVisit_ReturnStmt_DeclaredVariable_ReturnsTrue()
        {
            var table = new SymbolTable<string, object>(null);
            table.Add("x", null);
            var stmt = new ReturnStmt(new VariableNode("x"));

            var result = stmt.Accept(_visitor, CreateContext(table));

            Assert.True(result);
        }

        [Fact]
        public void TestVisit_ReturnStmt_UndeclaredVariable_ReturnsFalse()
        {
            var table = new SymbolTable<string, object>(null);
            var stmt = new ReturnStmt(new VariableNode("z"));

            var result = stmt.Accept(_visitor, CreateContext(table));

            Assert.False(result);
        }

        [Fact]
        public void TestVisit_ReturnStmt_Expression_AllDeclared_ReturnsTrue()
        {
            var table = new SymbolTable<string, object>(null);
            table.Add("x", null);
            table.Add("y", null);
            var stmt = new ReturnStmt(
                new PlusNode(new VariableNode("x"), new VariableNode("y")));

            var result = stmt.Accept(_visitor, CreateContext(table));

            Assert.True(result);
        }

        [Fact]
        public void TestVisit_ReturnStmt_ComplexExpression_OneUndeclared_ReturnsFalse()
        {
            var table = new SymbolTable<string, object>(null);
            table.Add("x", null);
            var stmt = new ReturnStmt(
                new TimesNode(new VariableNode("x"), new VariableNode("y")));

            var result = stmt.Accept(_visitor, CreateContext(table));

            Assert.False(result);
        }

        // ============================================================================
        // BLOCK STATEMENT TESTS
        // ============================================================================

        [Fact]
        public void TestVisit_EmptyBlock_ReturnsTrue()
        {
            var table = new SymbolTable<string, object>(null);
            var block = new BlockStmt(table);

            var result = block.Accept(_visitor, CreateContext(table));
            Assert.True(result);
        }

        [Fact]
        public void TestVisit_BlockWithSingleAssignment_ReturnsTrue()
        {
            var table = new SymbolTable<string, object>(null);
            var block = new BlockStmt(table);
            block.Statements.Add(new AssignmentStmt(new VariableNode("x"), new LiteralNode(5)));

            var result = block.Accept(_visitor, CreateContext(table));

            Assert.True(result);
            Assert.True(table.ContainsKey("x"));
        }

        [Fact]
        public void TestVisit_BlockWithMultipleAssignments_Sequential_ReturnsTrue()
        {
            var table = new SymbolTable<string, object>(null);
            var block = new BlockStmt(table);
            block.Statements.Add(new AssignmentStmt(new VariableNode("a"), new LiteralNode(1)));
            block.Statements.Add(new AssignmentStmt(new VariableNode("b"), new PlusNode(new VariableNode("a"), new LiteralNode(2))));

            var result = block.Accept(_visitor, CreateContext(table));

            Assert.True(result);
        }

        [Fact]
        public void TestVisit_BlockWithUndeclaredUse_ReturnsFalse()
        {
            var table = new SymbolTable<string, object>(null);
            var block = new BlockStmt(table);
            block.Statements.Add(new AssignmentStmt(new VariableNode("a"), new VariableNode("b"))); // b undeclared

            var result = block.Accept(_visitor, CreateContext(table));

            Assert.False(result);
        }

        [Fact]
        public void TestVisit_BlockWithMultipleStatements_OneInvalid_ReturnsFalse()
        {
            var table = new SymbolTable<string, object>(null);
            var block = new BlockStmt(table);
            block.Statements.Add(new AssignmentStmt(new VariableNode("a"), new LiteralNode(1)));
            block.Statements.Add(new AssignmentStmt(new VariableNode("b"), new VariableNode("c"))); // c undeclared
            block.Statements.Add(new ReturnStmt(new VariableNode("a")));

            var result = block.Accept(_visitor, CreateContext(table));

            Assert.False(result);
        }

        [Fact]
        public void TestVisit_BlockWithComplexStatements_AllValid_ReturnsTrue()
        {
            var table = new SymbolTable<string, object>(null);
            var block = new BlockStmt(table);
            block.Statements.Add(new AssignmentStmt(new VariableNode("x"), new LiteralNode(10)));
            block.Statements.Add(new AssignmentStmt(new VariableNode("y"), new LiteralNode(20)));
            block.Statements.Add(new AssignmentStmt(
                new VariableNode("sum"),
                new PlusNode(new VariableNode("x"), new VariableNode("y"))));
            block.Statements.Add(new ReturnStmt(new VariableNode("sum")));

            var result = block.Accept(_visitor, CreateContext(table));

            Assert.True(result);
        }

        // ============================================================================
        // NESTED BLOCK TESTS
        // ============================================================================

        [Fact]
        public void TestVisit_NestedBlock_AccessParentVariable_ReturnsTrue()
        {
            var outerTable = new SymbolTable<string, object>(null);
            var outerBlock = new BlockStmt(outerTable);
            outerBlock.Statements.Add(new AssignmentStmt(new VariableNode("x"), new LiteralNode(1)));

            var innerTable = new SymbolTable<string, object>(outerTable);
            var innerBlock = new BlockStmt(innerTable);
            innerBlock.Statements.Add(new ReturnStmt(new VariableNode("x"))); // from outer scope

            outerBlock.Statements.Add(innerBlock);

            var result = outerBlock.Accept(_visitor, CreateContext(outerTable));

            Assert.True(result);
        }

        [Fact]
        public void TestVisit_NestedBlock_InnerVariableNotInOuter_ReturnsTrue()
        {
            var outerTable = new SymbolTable<string, object>(null);
            var outerBlock = new BlockStmt(outerTable);

            var innerTable = new SymbolTable<string, object>(outerTable);
            var innerBlock = new BlockStmt(innerTable);
            innerBlock.Statements.Add(new AssignmentStmt(new VariableNode("y"), new LiteralNode(2)));
            innerBlock.Statements.Add(new ReturnStmt(new VariableNode("y")));

            outerBlock.Statements.Add(innerBlock);

            var result = outerBlock.Accept(_visitor, CreateContext(outerTable));

            Assert.True(result);
            Assert.True(outerTable.ContainsKey("y")); // y  in inner scope, meaning also in outer scope
        }

        [Fact]
        public void TestVisit_NestedBlock_UndeclaredInAllScopes_ReturnsFalse()
        {
            var outerTable = new SymbolTable<string, object>(null);
            var outerBlock = new BlockStmt(outerTable);
            var innerTable = new SymbolTable<string, object>(outerTable);
            var innerBlock = new BlockStmt(innerTable);

            innerBlock.Statements.Add(new ReturnStmt(new VariableNode("missing")));
            outerBlock.Statements.Add(innerBlock);

            var result = outerBlock.Accept(_visitor, CreateContext(outerTable));

            Assert.False(result);
        }

        [Fact]
        public void TestVisit_NestedBlock_CombineOuterAndInnerVariables_ReturnsTrue()
        {
            var outerTable = new SymbolTable<string, object>(null);
            var outerBlock = new BlockStmt(outerTable);
            outerBlock.Statements.Add(new AssignmentStmt(new VariableNode("x"), new LiteralNode(10)));

            var innerTable = new SymbolTable<string, object>(outerTable);
            var innerBlock = new BlockStmt(innerTable);
            innerBlock.Statements.Add(new AssignmentStmt(new VariableNode("y"), new LiteralNode(20)));
            innerBlock.Statements.Add(new ReturnStmt(
                new PlusNode(new VariableNode("x"), new VariableNode("y"))));

            outerBlock.Statements.Add(innerBlock);

            var result = outerBlock.Accept(_visitor, CreateContext(outerTable));

            Assert.True(result);
        }

        [Fact]
        public void TestVisit_DeeplyNestedBlocks_ThreeLevels_ReturnsTrue()
        {
            var table1 = new SymbolTable<string, object>(null);
            var block1 = new BlockStmt(table1);
            block1.Statements.Add(new AssignmentStmt(new VariableNode("a"), new LiteralNode(1)));

            var table2 = new SymbolTable<string, object>(table1);
            var block2 = new BlockStmt(table2);
            block2.Statements.Add(new AssignmentStmt(new VariableNode("b"), new LiteralNode(2)));

            var table3 = new SymbolTable<string, object>(table2);
            var block3 = new BlockStmt(table3);
            block3.Statements.Add(new AssignmentStmt(new VariableNode("c"), new LiteralNode(3)));
            block3.Statements.Add(new ReturnStmt(
                new PlusNode(
                    new PlusNode(new VariableNode("a"), new VariableNode("b")),
                    new VariableNode("c"))));

            block2.Statements.Add(block3);
            block1.Statements.Add(block2);

            var result = block1.Accept(_visitor, CreateContext(table1));

            Assert.True(result);
        }

        [Fact]
        public void TestVisit_DeeplyNestedBlocks_UndeclaredAtDeepLevel_ReturnsFalse()
        {
            var table1 = new SymbolTable<string, object>(null);
            var block1 = new BlockStmt(table1);
            block1.Statements.Add(new AssignmentStmt(new VariableNode("a"), new LiteralNode(1)));

            var table2 = new SymbolTable<string, object>(table1);
            var block2 = new BlockStmt(table2);
            block2.Statements.Add(new AssignmentStmt(new VariableNode("b"), new LiteralNode(2)));

            var table3 = new SymbolTable<string, object>(table2);
            var block3 = new BlockStmt(table3);
            block3.Statements.Add(new ReturnStmt(new VariableNode("missing")));

            block2.Statements.Add(block3);
            block1.Statements.Add(block2);

            var result = block1.Accept(_visitor, CreateContext(table1));

            Assert.False(result);
        }

        // ============================================================================
        // COMPLEX SCENARIO TESTS
        // ============================================================================

        [Fact]
        public void TestVisit_ComplexProgram_AllOperators_Valid()
        {
            var table = new SymbolTable<string, object>(null);
            var block = new BlockStmt(table);
            block.Statements.Add(new AssignmentStmt(new VariableNode("a"), new LiteralNode(10)));
            block.Statements.Add(new AssignmentStmt(new VariableNode("b"), new LiteralNode(5)));

            // result := ((a + b) * 2) / (a - b) % 3 ** 2
            var expr = new ModulusNode(
                new FloatDivNode(
                    new TimesNode(
                        new PlusNode(new VariableNode("a"), new VariableNode("b")),
                        new LiteralNode(2)),
                    new MinusNode(new VariableNode("a"), new VariableNode("b"))),
                new ExponentiationNode(new LiteralNode(3), new LiteralNode(2)));

            block.Statements.Add(new AssignmentStmt(new VariableNode("result"), expr));
            block.Statements.Add(new ReturnStmt(new VariableNode("result")));

            var result = block.Accept(_visitor, CreateContext(table));

            Assert.True(result);
        }

        [Fact]
        public void TestVisit_ComplexNestedProgram_ValidScoping()
        {
            var outerTable = new SymbolTable<string, object>(null);
            var outerBlock = new BlockStmt(outerTable);
            outerBlock.Statements.Add(new AssignmentStmt(new VariableNode("x"), new LiteralNode(100)));

            var midTable = new SymbolTable<string, object>(outerTable);
            var midBlock = new BlockStmt(midTable);
            midBlock.Statements.Add(new AssignmentStmt(
                new VariableNode("y"),
                new TimesNode(new VariableNode("x"), new LiteralNode(2))));

            var innerTable = new SymbolTable<string, object>(midTable);
            var innerBlock = new BlockStmt(innerTable);
            innerBlock.Statements.Add(new AssignmentStmt(
                new VariableNode("z"),
                new PlusNode(new VariableNode("x"), new VariableNode("y"))));
            innerBlock.Statements.Add(new ReturnStmt(
                new FloatDivNode(new VariableNode("z"), new LiteralNode(3))));

            midBlock.Statements.Add(innerBlock);
            outerBlock.Statements.Add(midBlock);

            var result = outerBlock.Accept(_visitor, CreateContext(outerTable));

            Assert.True(result);
        }

        [Fact]
        public void TestVisit_MultipleNestedBlocks_Parallel_Valid()
        {
            var outerTable = new SymbolTable<string, object>(null);
            var outerBlock = new BlockStmt(outerTable);
            outerBlock.Statements.Add(new AssignmentStmt(new VariableNode("shared"), new LiteralNode(42)));

            // First inner block
            var inner1Table = new SymbolTable<string, object>(outerTable);
            var inner1Block = new BlockStmt(inner1Table);
            inner1Block.Statements.Add(new AssignmentStmt(
                new VariableNode("x"),
                new PlusNode(new VariableNode("shared"), new LiteralNode(1))));

            // Second inner block
            var inner2Table = new SymbolTable<string, object>(outerTable);
            var inner2Block = new BlockStmt(inner2Table);
            inner2Block.Statements.Add(new AssignmentStmt(
                new VariableNode("y"),
                new MinusNode(new VariableNode("shared"), new LiteralNode(1))));

            outerBlock.Statements.Add(inner1Block);
            outerBlock.Statements.Add(inner2Block);
            outerBlock.Statements.Add(new ReturnStmt(new VariableNode("shared")));

            var result = outerBlock.Accept(_visitor, CreateContext(outerTable));

            Assert.True(result);
        }

        [Fact]
        public void TestVisit_SequentialBlocks_VariableNotSharedBetweenSiblings()
        {
            var outerTable = new SymbolTable<string, object>(null);
            var outerBlock = new BlockStmt(outerTable);

            var inner1Table = new SymbolTable<string, object>(outerTable);
            var inner1Block = new BlockStmt(inner1Table);
            inner1Block.Statements.Add(new AssignmentStmt(new VariableNode("local"), new LiteralNode(10)));

            var inner2Table = new SymbolTable<string, object>(outerTable);
            var inner2Block = new BlockStmt(inner2Table);
            inner2Block.Statements.Add(new ReturnStmt(new VariableNode("local"))); // not accessible

            outerBlock.Statements.Add(inner1Block);
            outerBlock.Statements.Add(inner2Block);

            var result = outerBlock.Accept(_visitor, CreateContext(outerTable));

            Assert.False(result);
        }

        [Fact]
        public void TestVisit_AllOperatorsWithVariables_OneUndeclared_ReturnsFalse()
        {
            var table = new SymbolTable<string, object>(null);
            table.Add("a", null);
            table.Add("b", null);

            // Missing 'c' in: (a + b) * c
            var expr = new TimesNode(
                new PlusNode(new VariableNode("a"), new VariableNode("b")),
                new VariableNode("c"));

            var result = expr.Accept(_visitor, CreateContext(table));

            Assert.False(result);
        }
    }
}