using System;
using Xunit;
using AST;
using Utilities;

//  add use parsers to test the code

namespace AST.Visitors.Tests.FullProgramParserVisitor.Tests
{
    /// <summary>
    /// Tests the NameAnalysisVisitor to ensure that
    /// variable declarations and lookups are semantically valid.
    /// It checks correct scoping, redeclarations, and detection of
    /// undeclared variables.
    /// </summary>
    public class NameAnalysisTest
    {
        private readonly NameAnalysisVisitor _analyzer = new();

        private Tuple<SymbolTable<string, object>, Statement> Scope() =>
            new Tuple<SymbolTable<string, object>, Statement>(new SymbolTable<string, object>(), null);

        // -------------------------------
        // Variable Declaration Validation
        // -------------------------------

        [Fact(DisplayName = "Declared variable passes name analysis")]
        public void DeclaredVariable_IsValid()
        {
            var symbolTable = new SymbolTable<string, object>();
            symbolTable.Add("x", null);

            var variable = new VariableNode("x");
            bool result = variable.Accept(_analyzer, Tuple.Create(symbolTable, (Statement)null));

            Assert.True(result);
        }

        [Fact(DisplayName = "Undeclared variable fails name analysis")]
        public void UndeclaredVariable_FailsAnalysis()
        {
            var symbolTable = new SymbolTable<string, object>();
            var variable = new VariableNode("ghost");

            bool result = variable.Accept(_analyzer, Tuple.Create(symbolTable, (Statement)null));

            Assert.False(result);
        }

        // -------------------------------
        // Assignment Handling
        // -------------------------------

        [Fact(DisplayName = "Assignment declares variable and validates expression")]
        public void Assignment_DeclaresVariable_AndExpressionIsValid()
        {
            var symbolTable = new SymbolTable<string, object>();
            var stmt = new AssignmentStmt(
                new VariableNode("y"),
                new PlusNode(new LiteralNode(5), new LiteralNode(10))
            );

            bool result = stmt.Accept(_analyzer, Tuple.Create(symbolTable, (Statement)null));

            Assert.True(result);
            Assert.True(symbolTable.ContainsKey("y"));
        }

        [Fact(DisplayName = "Assignment referencing undeclared variable fails")]
        public void Assignment_UsingUndeclaredVariable_Fails()
        {
            var symbolTable = new SymbolTable<string, object>();
            var stmt = new AssignmentStmt(
                new VariableNode("z"),
                new PlusNode(new VariableNode("undef"), new LiteralNode(10))
            );

            bool result = stmt.Accept(_analyzer, Tuple.Create(symbolTable, (Statement)null));

            Assert.False(result);
        }

        // -------------------------------
        // Return Statement Analysis
        // -------------------------------

        [Fact(DisplayName = "Return statement with valid expression passes analysis")]
        public void Return_ValidExpression_Passes()
        {
            var symbolTable = new SymbolTable<string, object>();
            symbolTable.Add("val", 42);

            var stmt = new ReturnStmt(new PlusNode(new VariableNode("val"), new LiteralNode(8)));
            bool result = stmt.Accept(_analyzer, Tuple.Create(symbolTable, (Statement)null));

            Assert.True(result);
        }

        [Fact(DisplayName = "Return statement with undeclared variable fails")]
        public void Return_UndeclaredVariable_Fails()
        {
            var symbolTable = new SymbolTable<string, object>();
            var stmt = new ReturnStmt(new VariableNode("notDeclared"));

            bool result = stmt.Accept(_analyzer, Tuple.Create(symbolTable, (Statement)null));

            Assert.False(result);
        }

        // -------------------------------
        // Block Scope Behavior
        // -------------------------------

        [Fact(DisplayName = "Block with valid statements passes name analysis")]
        public void Block_AllValidStatements_Passes()
        {
            var global = new SymbolTable<string, object>();
            var block = new BlockStmt(global);

            block.Statements.Add(new AssignmentStmt(
                new VariableNode("a"),
                new LiteralNode(5)
            ));
            block.Statements.Add(new AssignmentStmt(
                new VariableNode("b"),
                new PlusNode(new VariableNode("a"), new LiteralNode(2))
            ));
            block.Statements.Add(new ReturnStmt(new VariableNode("b")));

            bool result = block.Accept(_analyzer, Tuple.Create(global, (Statement)null));

            Assert.True(result);
        }

        [Fact(DisplayName = "Block with undeclared variable inside fails")]
        public void Block_WithUndeclaredVariable_Fails()
        {
            var global = new SymbolTable<string, object>();
            var block = new BlockStmt(global);

            block.Statements.Add(new AssignmentStmt(
                new VariableNode("a"),
                new LiteralNode(5)
            ));
            block.Statements.Add(new AssignmentStmt(
                new VariableNode("b"),
                new PlusNode(new VariableNode("notDefined"), new LiteralNode(2))
            ));

            bool result = block.Accept(_analyzer, Tuple.Create(global, (Statement)null));

            Assert.False(result);
        }

        // -------------------------------
        // Nested Block Shadowing
        // -------------------------------

        [Fact(DisplayName = "Nested block correctly resolves shadowed variable")]
        public void NestedBlock_ShadowedVariable_Valid()
        {
            var global = new SymbolTable<string, object>();
            global.Add("x", null);

            var outer = new BlockStmt(global);
            var innerScope = new SymbolTable<string, object>(global);
            var inner = new BlockStmt(innerScope);

            // Re-declare x inside
            inner.Statements.Add(new AssignmentStmt(
                new VariableNode("x"),
                new LiteralNode(99)
            ));

            // reference is valid
            inner.Statements.Add(new ReturnStmt(new VariableNode("x")));

            outer.Statements.Add(inner);

            bool result = outer.Accept(_analyzer, Tuple.Create(global, (Statement)null));

            Assert.True(result);
        }

        [Fact(DisplayName = "Nested block referencing undeclared variable fails")]
        public void NestedBlock_UsesUndeclared_Fails()
        {
            var global = new SymbolTable<string, object>();
            var outer = new BlockStmt(global);
            var innerScope = new SymbolTable<string, object>(global);
            var inner = new BlockStmt(innerScope);

            // Inner references variable not declared in any scope
            inner.Statements.Add(new ReturnStmt(new VariableNode("missing")));

            outer.Statements.Add(inner);

            bool result = outer.Accept(_analyzer, Tuple.Create(global, (Statement)null));

            Assert.False(result);
        }

        // -------------------------------
        // Literal Nodes
        // -------------------------------

        [Theory(DisplayName = "Literal nodes are always valid in name analysis")]
        [InlineData(42)]
        [InlineData(3.14)]
        [InlineData("string")]
        [InlineData(true)]
        [InlineData(null)]
        public void LiteralNode_AlwaysValid(object value)
        {
            var literal = new LiteralNode(value);
            bool result = literal.Accept(_analyzer, Scope());
            Assert.True(result);
        }
    }
}
