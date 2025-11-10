using System;
using Xunit;
using AST;
using Utilities;

//  use parsers here to test the code
namespace AST.Visitors.Tests.FullProgramParserVisitor.Tests
{
    /// <summary>
    /// EvaluateVisitor integration tests.
    /// Validates mathematical correctness, variable binding,
    /// nested scope behavior, and runtime exception handling.
    /// </summary>
    public class EvaluateTest
    {
        private readonly EvaluateVisitor _evaluator = new();
        private SymbolTable<string, object> NewScope() => new SymbolTable<string, object>();

        // -----------------------
        // Arithmetic Verification
        // -----------------------

        [Theory(DisplayName = "Arithmetic operations produce correct results")]
        [InlineData(3, 5, "+", 8)]
        [InlineData(10, 4, "-", 6)]
        [InlineData(2, 5, "*", 10)]
        [InlineData(9, 3, "/", 3.0)]
        [InlineData(9, 2, "//", 4)]
        [InlineData(10, 3, "%", 1)]
        [InlineData(2, 3, "**", 8)]
        public void Evaluate_ArithmeticExpressions_ReturnExpected(object left, object right, string op, object expected)
        {
            ExpressionNode expr = op switch
            {
                "+" => new PlusNode(new LiteralNode(left), new LiteralNode(right)),
                "-" => new MinusNode(new LiteralNode(left), new LiteralNode(right)),
                "*" => new TimesNode(new LiteralNode(left), new LiteralNode(right)),
                "/" => new FloatDivNode(new LiteralNode(left), new LiteralNode(right)),
                "//" => new IntDivNode(new LiteralNode(left), new LiteralNode(right)),
                "%" => new ModulusNode(new LiteralNode(left), new LiteralNode(right)),
                "**" => new ExponentiationNode(new LiteralNode(left), new LiteralNode(right)),
                _ => throw new ArgumentOutOfRangeException(nameof(op))
            };

            var result = expr.Accept(_evaluator, NewScope());
            Assert.Equal(Convert.ToDouble(expected), Convert.ToDouble(result));
        }

        // -----------------------
        // Assignment + Lookup
        // -----------------------

        [Theory(DisplayName = "Assignment and variable lookup are consistent across data types")]
        [InlineData("x", 42)]
        [InlineData("pi", 3.14)]
        [InlineData("flag", true)]
        [InlineData("msg", "hello")]
        [InlineData("nullable", null)]
        public void Evaluate_AssignmentAndLookup_WorksCorrectly(string name, object value)
        {
            var scope = NewScope();

            var assignment = new AssignmentStmt(new VariableNode(name), new LiteralNode(value));
            assignment.Accept(_evaluator, scope);

            var lookup = new VariableNode(name);
            var result = lookup.Accept(_evaluator, scope);

            Assert.Equal(value, result);
        }

        // -----------------------
        // Return Behavior
        // -----------------------

        [Theory(DisplayName = "Return statement computes expression value correctly")]
        [InlineData(9, 4, "-", 5)]
        [InlineData(6, 3, "/", 2.0)]
        [InlineData(2, 8, "**", 256)]
        public void Evaluate_ReturnStatement_ReturnsExpected(object left, object right, string op, object expected)
        {
            ExpressionNode expr = op switch
            {
                "-" => new MinusNode(new LiteralNode(left), new LiteralNode(right)),
                "/" => new FloatDivNode(new LiteralNode(left), new LiteralNode(right)),
                "**" => new ExponentiationNode(new LiteralNode(left), new LiteralNode(right)),
                _ => throw new ArgumentOutOfRangeException(nameof(op))
            };

            var stmt = new ReturnStmt(expr);
            var result = stmt.Accept(_evaluator, NewScope());
            Assert.Equal(Convert.ToDouble(expected), Convert.ToDouble(result));
        }

        // -----------------------
        // Block + Scope Semantics
        // -----------------------

        [Fact(DisplayName = "Sequential block executes correctly and returns final value")]
        public void Evaluate_BlockExecutesSequentially_ReturnsExpected()
        {
            var global = NewScope();
            var block = new BlockStmt(global);

            block.Statements.Add(new AssignmentStmt(new VariableNode("a"), new LiteralNode(5)));
            block.Statements.Add(new AssignmentStmt(new VariableNode("b"),
                new TimesNode(new VariableNode("a"), new LiteralNode(3))));
            block.Statements.Add(new ReturnStmt(new PlusNode(new VariableNode("b"), new LiteralNode(2))));

            var result = block.Accept(_evaluator, global);
            Assert.Equal(17, Convert.ToInt32(result));
        }

        [Fact(DisplayName = "Nested block shadows outer variable correctly")]
        public void Evaluate_NestedBlock_ShadowingWorks()
        {
            var global = NewScope();
            var outer = new BlockStmt(global);

            outer.Statements.Add(new AssignmentStmt(new VariableNode("x"), new LiteralNode(2)));

            var innerScope = new SymbolTable<string, object>(global);
            var inner = new BlockStmt(innerScope);
            inner.Statements.Add(new AssignmentStmt(new VariableNode("x"),
                new PlusNode(new VariableNode("x"), new LiteralNode(10))));
            inner.Statements.Add(new ReturnStmt(new VariableNode("x")));

            outer.Statements.Add(inner);
            var result = outer.Accept(_evaluator, global);

            Assert.Equal(12, Convert.ToInt32(result));
        }

        // -----------------------
        // Errors & Edge Cases
        // -----------------------

        [Theory(DisplayName = "Division and modulus by zero throw EvaluationException")]
        [InlineData("/", 0)]
        [InlineData("//", 0)]
        [InlineData("%", 0)]
        public void Evaluate_DivideOrModulusByZero_ThrowsException(string op, int rhs)
        {
            ExpressionNode expr = op switch
            {
                "/" => new FloatDivNode(new LiteralNode(5), new LiteralNode(rhs)),
                "//" => new IntDivNode(new LiteralNode(5), new LiteralNode(rhs)),
                "%" => new ModulusNode(new LiteralNode(5), new LiteralNode(rhs)),
                _ => throw new ArgumentOutOfRangeException(nameof(op))
            };

            Assert.Throws<EvaluateVisitor.EvaluationException>(() => expr.Accept(_evaluator, NewScope()));
        }

        [Fact(DisplayName = "Accessing undefined variable throws EvaluationException")]
        public void Evaluate_UndefinedVariable_ThrowsEvaluationException()
        {
            var scope = NewScope();
            var expr = new VariableNode("ghost");
            Assert.Throws<EvaluateVisitor.EvaluationException>(() => expr.Accept(_evaluator, scope));
        }

        [Theory(DisplayName = "Literal nodes of various types return stored values directly")]
        [InlineData(42)]
        [InlineData(3.14)]
        [InlineData("hello")]
        [InlineData(true)]
        [InlineData(null)]
        public void Evaluate_LiteralNode_ReturnsRawValue(object value)
        {
            var scope = NewScope();
            var node = new LiteralNode(value);
            var result = node.Accept(_evaluator, scope);
            Assert.Equal(value, result);
        }

        [Fact(DisplayName = "Complex nested arithmetic computes mathematically correct value")]
        public void Evaluate_ComplexNestedExpression_ReturnsCorrectValue()
        {
            // ((3 + 2) * (4 - 1)) ** 2 = (5 * 3)^2 = 225
            var expr = new ExponentiationNode(
                new TimesNode(
                    new PlusNode(new LiteralNode(3), new LiteralNode(2)),
                    new MinusNode(new LiteralNode(4), new LiteralNode(1))
                ),
                new LiteralNode(2)
            );

            var result = expr.Accept(_evaluator, NewScope());
            Assert.Equal(225, Convert.ToInt32(result));
        }
    }
}
