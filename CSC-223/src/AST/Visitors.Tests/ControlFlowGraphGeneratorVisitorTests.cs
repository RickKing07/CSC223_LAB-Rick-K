using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;

using AST;
// Assuming your class is in this namespace. Adjust as needed.
namespace DataStructures.Tests
{
    public class DiGraphTests
    {
        /// <summary>
        /// Creates a pre-populated graph for testing.
        /// Graph state:
        /// Vertices: 1, 2, 3, 4
        /// Edges: 1->2, 1->3, 3->4
        /// VertexCount: 4
        /// EdgeCount: 3
        /// </summary>
        private DiGraph<int> SetupStandardGraph()
        {
            var graph = new DiGraph<int>();
            graph.AddVertex(1);
            graph.AddVertex(2);
            graph.AddVertex(3);
            graph.AddVertex(4);

            graph.AddEdge(1, 2);
            graph.AddEdge(1, 3);
            graph.AddEdge(3, 4);
            return graph;
        }

        // --- Constructor & Empty Graph Tests ---

        [Fact]
        public void NewGraph_ShouldBeEmpty()
        {
            var graph = new DiGraph<int>();

            Assert.Equal(0, graph.VertexCount());
            Assert.Equal(0, graph.EdgeCount());
            Assert.Empty(graph.GetVertices());
        }

        [Fact]
        public void ToString_ShouldNotBeNull_OnEmptyGraph()
        {
            var graph = new DiGraph<int>();
            var str = graph.ToString();
            Assert.NotNull(str);
            Assert.Contains("Vertices: 0", str, StringComparison.InvariantCultureIgnoreCase);
            Assert.Contains("Edges: 0", str, StringComparison.InvariantCultureIgnoreCase);
        }

        // --- AddVertex Tests ---

        [Fact]
        public void AddVertex_ShouldReturnTrueAndIncreaseCount_WhenVertexIsNew()
        {
            var graph = new DiGraph<int>();
            Assert.True(graph.AddVertex(10));
            Assert.Equal(1, graph.VertexCount());
            Assert.Contains(10, graph.GetVertices());
        }

        [Fact]
        public void AddVertex_ShouldReturnFalse_WhenVertexExists()
        {
            var graph = SetupStandardGraph(); // Already has 1, 2, 3, 4
            Assert.False(graph.AddVertex(1));
            Assert.Equal(4, graph.VertexCount());
        }

        // --- AddEdge Tests ---

        [Fact]
        public void AddEdge_ShouldReturnTrueAndIncreaseCount_WhenEdgeIsNew()
        {
            var graph = new DiGraph<int>();
            graph.AddVertex(1);
            graph.AddVertex(2);

            Assert.True(graph.AddEdge(1, 2));
            Assert.Equal(1, graph.EdgeCount());
            Assert.True(graph.HasEdge(1, 2));
        }

        [Fact]
        public void AddEdge_ShouldReturnFalse_WhenEdgeExists()
        {
            var graph = SetupStandardGraph(); // Already has 1->2
            Assert.False(graph.AddEdge(1, 2));
            Assert.Equal(3, graph.EdgeCount());
        }

        [Fact]
        public void AddEdge_ShouldAllowSelfLoops()
        {
            var graph = new DiGraph<int>();
            graph.AddVertex(1);

            Assert.True(graph.AddEdge(1, 1));
            Assert.Equal(1, graph.EdgeCount());
            Assert.True(graph.HasEdge(1, 1));
            Assert.Contains(1, graph.GetNeighbors(1));
        }

        [Theory]
        [InlineData(99, 1)] // Source does not exist
        [InlineData(1, 99)] // Destination does not exist
        [InlineData(98, 99)] // Neither exists
        public void AddEdge_ShouldThrowArgumentException_WhenVertexDoesNotExist(int source, int dest)
        {
            var graph = SetupStandardGraph();
            Assert.Throws<ArgumentException>(() => graph.AddEdge(source, dest));
        }

        // --- RemoveVertex Tests ---

        [Fact]
        public void RemoveVertex_ShouldReturnTrueAndRemoveVertex_WhenVertexExists()
        {
            var graph = SetupStandardGraph();
            Assert.True(graph.RemoveVertex(1));
            Assert.Equal(3, graph.VertexCount());
            Assert.False(graph.GetVertices().Contains(1));
        }

        [Fact]
        public void RemoveVertex_ShouldReturnFalse_WhenVertexDoesNotExist()
        {
            var graph = SetupStandardGraph();
            Assert.False(graph.RemoveVertex(99));
            Assert.Equal(4, graph.VertexCount());
        }

        [Fact]
        public void RemoveVertex_ShouldRemoveAllOutgoingEdges()
        {
            var graph = SetupStandardGraph(); // 1->2, 1->3

            graph.RemoveVertex(1);

            Assert.Equal(1, graph.EdgeCount()); // Only 3->4 should remain
            Assert.False(graph.HasEdge(1, 2)); // Use HasEdge for consistency
            Assert.False(graph.HasEdge(1, 3));
            Assert.True(graph.HasEdge(3, 4));
        }

        [Fact]
        public void RemoveVertex_ShouldRemoveAllIncomingEdges()
        {
            var graph = SetupStandardGraph();
            graph.AddEdge(2, 1); // 1->2, 1->3, 3->4, 2->1
            Assert.Equal(4, graph.EdgeCount());

            graph.RemoveVertex(1); // Should remove 1->2, 1->3, and 2->1

            Assert.Equal(1, graph.EdgeCount()); // Only 3->4 remains
            Assert.Equal(3, graph.VertexCount()); // 2, 3, 4 remain

            // Check that 2 no longer has an edge to 1 (because 1 doesn't exist)
            Assert.False(graph.HasEdge(2, 1));
            // Check that 2 still exists and now has no neighbors
            Assert.Empty(graph.GetNeighbors(2));
        }

        [Fact]
        public void RemoveVertex_ShouldHandleRemovingLastVertex()
        {
            var graph = new DiGraph<int>();
            graph.AddVertex(1);

            Assert.True(graph.RemoveVertex(1));
            Assert.Equal(0, graph.VertexCount());
            Assert.Equal(0, graph.EdgeCount());
            Assert.Empty(graph.GetVertices());
        }

        // --- RemoveEdge Tests ---

        [Fact]
        public void RemoveEdge_ShouldReturnTrueAndDecreaseCount_WhenEdgeExists()
        {
            var graph = SetupStandardGraph(); // Has 1->2
            Assert.True(graph.RemoveEdge(1, 2));
            Assert.Equal(2, graph.EdgeCount());
            Assert.False(graph.HasEdge(1, 2));
        }

        [Theory]
        [InlineData(2, 1)] // Edge does not exist (reverse)
        [InlineData(1, 4)] // Edge does not exist (vertices exist)
        public void RemoveEdge_ShouldReturnFalse_WhenEdgeDoesNotExist(int source, int dest)
        {
            var graph = SetupStandardGraph();
            Assert.False(graph.RemoveEdge(source, dest));
            Assert.Equal(3, graph.EdgeCount());
        }

        [Theory]
        [InlineData(99, 1)] // Source does not exist
        [InlineData(1, 99)] // Destination does not exist
        public void RemoveEdge_ShouldThrowArgumentException_WhenVertexDoesNotExist(int source, int dest)
        {
            var graph = SetupStandardGraph();
            Assert.Throws<ArgumentException>(() => graph.RemoveEdge(source, dest));
        }

        // --- HasEdge Tests ---

        [Theory]
        [InlineData(1, 2, true)]  // Existing edge
        [InlineData(3, 4, true)]  // Existing edge
        [InlineData(1, 4, false)] // Non-existing edge
        [InlineData(2, 1, false)] // Non-existing reverse edge
        [InlineData(4, 3, false)] // Non-existing reverse edge
        public void HasEdge_ShouldReturnCorrectBoolean(int source, int dest, bool expected)
        {
            var graph = SetupStandardGraph();
            Assert.Equal(expected, graph.HasEdge(source, dest));
        }

        [Theory]
        [InlineData(99, 1)] // Source does not exist
        [InlineData(1, 99)] // Destination does not exist
        public void HasEdge_ShouldReturnFalse_WhenVertexDoesNotExist(int source, int dest)
        {
            var graph = SetupStandardGraph();
            bool IsLocated = graph.HasEdge(source, dest);
            Assert.False(IsLocated);
        }

        // --- GetNeighbors Tests ---

        [Fact]
        public void GetNeighbors_ShouldReturnAllAdjacentVertices()
        {
            var graph = SetupStandardGraph(); // 1->2, 1->3
            var neighbors = graph.GetNeighbors(1);

            Assert.Equal(2, neighbors.Count);
            Assert.Contains(2, neighbors);
            Assert.Contains(3, neighbors);
        }

        [Theory]
        [InlineData(2)] // Has incoming, no outgoing
        [InlineData(4)] // Has incoming, no outgoing
        public void GetNeighbors_ShouldReturnEmptyList_ForSinkVertex(int vertex)
        {
            var graph = SetupStandardGraph();
            var neighbors = graph.GetNeighbors(vertex);
            Assert.Empty(neighbors);
        }

        [Fact]
        public void GetNeighbors_ShouldThrowArgumentException_WhenVertexDoesNotExist()
        {
            var graph = SetupStandardGraph();
            Assert.Throws<ArgumentException>(() => graph.GetNeighbors(99));
        }

        // --- GetVertices / VertexCount / EdgeCount Tests ---
        // These are implicitly tested by most other tests, 
        // but we can add explicit sanity checks.

        [Fact]
        public void GetVertices_ShouldReturnAllAddedVertices()
        {
            var graph = SetupStandardGraph();
            var vertices = graph.GetVertices().ToList();

            Assert.Equal(4, vertices.Count);
            Assert.Contains(1, vertices);
            Assert.Contains(2, vertices);
            Assert.Contains(3, vertices);
            Assert.Contains(4, vertices);
        }

        [Fact]
        public void Counts_ShouldBeCorrectAfterComplexOperations()
        {
            var graph = new DiGraph<int>();
            graph.AddVertex(1);
            graph.AddVertex(2);
            graph.AddVertex(3);
            graph.AddEdge(1, 2);
            graph.AddEdge(1, 3);
            // State: V=3, E=2
            Assert.Equal(3, graph.VertexCount());
            Assert.Equal(2, graph.EdgeCount());

            graph.AddVertex(4);
            graph.AddEdge(3, 4);
            // State: V=4, E=3
            Assert.Equal(4, graph.VertexCount());
            Assert.Equal(3, graph.EdgeCount());

            graph.RemoveEdge(1, 2);
            // State: V=4, E=2
            Assert.Equal(4, graph.VertexCount());
            Assert.Equal(2, graph.EdgeCount());

            graph.RemoveVertex(3); // Should remove vertex 3 and edges 1->3, 3->4
            // State: V=3, E=0
            Assert.Equal(3, graph.VertexCount());
            Assert.Equal(0, graph.EdgeCount());
        }
    }
}

namespace AST.Tests
{
    public class ControlFlowGraphGeneratorVisitorTests
    {
        // Helper method to create a simple assignment statement for testing
        private AssignmentStmt CreateAssignment(string varName, int value)
        {
            return new AssignmentStmt(
                new VariableNode(varName),
                new LiteralNode(value)
            );
        }

        private ReturnStmt CreateReturn(int value)
        {
            return new ReturnStmt(new LiteralNode(value));
        }

        #region Basic Initialization Tests

        [Fact]
        public void Constructor_WithValidStatement_InitializesCFGWithStartVertex()
        {
            // Arrange
            var startStmt = CreateAssignment("x", 1);

            // Act
            var visitor = new ControlFlowGraphGeneratorVisitor(startStmt);
            var cfg = visitor.GetCFG();

            // Assert
            Assert.NotNull(cfg);
            Assert.Equal(1, cfg.VertexCount());
            Assert.Contains(startStmt, cfg.GetVertices());
        }

        [Fact]
        public void Constructor_WithNullStatement_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new ControlFlowGraphGeneratorVisitor(null));
        }

        #endregion

        #region Single Statement Tests

        [Fact]
        public void Visit_SingleAssignmentStatement_AddsVertexAndEdge()
        {
            // Arrange
            var start = CreateAssignment("x", 1);
            var stmt = CreateAssignment("y", 2);
            var visitor = new ControlFlowGraphGeneratorVisitor(start);

            // Act
            visitor.Visit(stmt, start);
            var cfg = visitor.GetCFG();

            // Assert
            Assert.Equal(2, cfg.VertexCount());
            Assert.True(cfg.HasEdge(start, stmt));
        }

        [Fact]
        public void Visit_SingleReturnStatement_AddsVertexAndEdge()
        {
            // Arrange
            var start = CreateAssignment("x", 1);
            var returnStmt = CreateReturn(42);
            var visitor = new ControlFlowGraphGeneratorVisitor(start);

            // Act
            visitor.Visit(returnStmt, start);
            var cfg = visitor.GetCFG();

            // Assert
            Assert.Equal(2, cfg.VertexCount());
            Assert.True(cfg.HasEdge(start, returnStmt));
        }

        [Fact]
        public void Visit_ReturnStatement_HasNoOutgoingEdges()
        {
            // Arrange
            var start = CreateAssignment("x", 1);
            var returnStmt = CreateReturn(42);
            var visitor = new ControlFlowGraphGeneratorVisitor(start);

            // Act
            visitor.Visit(returnStmt, start);
            var cfg = visitor.GetCFG();

            // Assert
            var neighbors = cfg.GetNeighbors(returnStmt);
            Assert.Empty(neighbors);
        }

        #endregion

        #region Sequential Statement Tests

        [Fact]
        public void Visit_SequentialAssignments_CreatesLinearCFG()
        {
            // Arrange
            var stmt1 = CreateAssignment("x", 1);
            var stmt2 = CreateAssignment("y", 2);
            var stmt3 = CreateAssignment("z", 3);
            var visitor = new ControlFlowGraphGeneratorVisitor(stmt1);

            // Act
            visitor.Visit(stmt2, stmt1);
            visitor.Visit(stmt3, stmt2);
            var cfg = visitor.GetCFG();

            // Assert
            Assert.Equal(3, cfg.VertexCount());
            Assert.Equal(2, cfg.EdgeCount());
            Assert.True(cfg.HasEdge(stmt1, stmt2));
            Assert.True(cfg.HasEdge(stmt2, stmt3));
            Assert.False(cfg.HasEdge(stmt1, stmt3)); // No direct edge
        }

        [Theory]
        [InlineData(2)]
        [InlineData(5)]
        [InlineData(10)]
        public void Visit_MultipleSequentialStatements_CreatesCorrectNumberOfVerticesAndEdges(int count)
        {
            // Arrange
            var statements = new List<AssignmentStmt>();
            for (int i = 0; i < count; i++)
            {
                statements.Add(CreateAssignment($"var{i}", i));
            }
            var visitor = new ControlFlowGraphGeneratorVisitor(statements[0]);

            // Act
            for (int i = 1; i < count; i++)
            {
                visitor.Visit(statements[i], statements[i - 1]);
            }
            var cfg = visitor.GetCFG();

            // Assert
            Assert.Equal(count, cfg.VertexCount());
            Assert.Equal(count - 1, cfg.EdgeCount());
        }

        #endregion

        #region Block Statement Tests

        [Fact]
        public void Visit_EmptyBlockStatement_DoesNotAddVertices()
        {
            // Arrange
            var start = CreateAssignment("x", 1);
            var block = new BlockStmt(new List<Statement>());
            var visitor = new ControlFlowGraphGeneratorVisitor(start);

            // Act
            visitor.Visit(block, start);
            var cfg = visitor.GetCFG();

            // Assert
            Assert.Equal(1, cfg.VertexCount()); // Only start statement
        }

        [Fact]
        public void Visit_BlockWithSingleStatement_AddsVertexAndEdge()
        {
            // Arrange
            var start = CreateAssignment("x", 1);
            var innerStmt = CreateAssignment("y", 2);
            var block = new BlockStmt(new List<Statement> { innerStmt });
            var visitor = new ControlFlowGraphGeneratorVisitor(start);

            // Act
            visitor.Visit(block, start);
            var cfg = visitor.GetCFG();

            // Assert
            Assert.Equal(2, cfg.VertexCount());
            Assert.True(cfg.HasEdge(start, innerStmt));
        }

        [Fact]
        public void Visit_BlockWithMultipleStatements_CreatesSequentialFlow()
        {
            // Arrange
            var start = CreateAssignment("x", 1);
            var stmt1 = CreateAssignment("a", 10);
            var stmt2 = CreateAssignment("b", 20);
            var stmt3 = CreateAssignment("c", 30);
            var block = new BlockStmt(new List<Statement> { stmt1, stmt2, stmt3 });
            var visitor = new ControlFlowGraphGeneratorVisitor(start);

            // Act
            visitor.Visit(block, start);
            var cfg = visitor.GetCFG();

            // Assert
            Assert.Equal(4, cfg.VertexCount());
            Assert.True(cfg.HasEdge(start, stmt1));
            Assert.True(cfg.HasEdge(stmt1, stmt2));
            Assert.True(cfg.HasEdge(stmt2, stmt3));
        }

        [Fact]
        public void Visit_NestedBlocks_CreatesCorrectFlow()
        {
            // Arrange
            var start = CreateAssignment("x", 1);
            var stmt1 = CreateAssignment("a", 10);
            var stmt2 = CreateAssignment("b", 20);
            var innerBlock = new BlockStmt(new List<Statement> { stmt2 });
            var outerBlock = new BlockStmt(new List<Statement> { stmt1, innerBlock });
            var visitor = new ControlFlowGraphGeneratorVisitor(start);

            // Act
            visitor.Visit(outerBlock, start);
            var cfg = visitor.GetCFG();

            // Assert
            Assert.Equal(3, cfg.VertexCount());
            Assert.True(cfg.HasEdge(start, stmt1));
            Assert.True(cfg.HasEdge(stmt1, stmt2));
        }

        [Fact]
        public void Visit_DeeplyNestedBlocks_CreatesCorrectFlow()
        {
            // Arrange
            var start = CreateAssignment("x", 1);
            var stmt1 = CreateAssignment("a", 10);
            var stmt2 = CreateAssignment("b", 20);
            var stmt3 = CreateAssignment("c", 30);

            var innerMost = new BlockStmt(new List<Statement> { stmt3 });
            var middle = new BlockStmt(new List<Statement> { stmt2, innerMost });
            var outer = new BlockStmt(new List<Statement> { stmt1, middle });
            var visitor = new ControlFlowGraphGeneratorVisitor(start);

            // Act
            visitor.Visit(outer, start);
            var cfg = visitor.GetCFG();

            // Assert
            Assert.Equal(4, cfg.VertexCount());
            Assert.True(cfg.HasEdge(start, stmt1));
            Assert.True(cfg.HasEdge(stmt1, stmt2));
            Assert.True(cfg.HasEdge(stmt2, stmt3));
        }

        #endregion

        #region Return Statement Flow Tests

        [Fact]
        public void Visit_BlockWithReturnInMiddle_StopsFlowAfterReturn()
        {
            // Arrange
            var start = CreateAssignment("x", 1);
            var stmt1 = CreateAssignment("a", 10);
            var returnStmt = CreateReturn(42);
            var stmt2 = CreateAssignment("b", 20); // Unreachable
            var block = new BlockStmt(new List<Statement> { stmt1, returnStmt, stmt2 });
            var visitor = new ControlFlowGraphGeneratorVisitor(start);

            // Act
            visitor.Visit(block, start);
            var cfg = visitor.GetCFG();

            // Assert
            // The implementation should handle this correctly
            // stmt2 should either not be added or not be connected
            Assert.True(cfg.HasEdge(start, stmt1));
            Assert.True(cfg.HasEdge(stmt1, returnStmt));

            // Check that return has no outgoing edges
            var returnNeighbors = cfg.GetNeighbors(returnStmt);
            Assert.Empty(returnNeighbors);
        }

        [Fact]
        public void Visit_MultipleReturnsInDifferentBlocks_EachReturnHasNoOutgoingEdges()
        {
            // Arrange
            var start = CreateAssignment("x", 1);
            var return1 = CreateReturn(1);
            var return2 = CreateReturn(2);
            var visitor = new ControlFlowGraphGeneratorVisitor(start);

            // Act
            visitor.Visit(return1, start);
            visitor.Visit(return2, start);
            var cfg = visitor.GetCFG();

            // Assert
            Assert.Empty(cfg.GetNeighbors(return1));
            Assert.Empty(cfg.GetNeighbors(return2));
        }

        #endregion

        #region Expression Node Tests (Should Not Create Vertices)

        [Fact]
        public void Visit_LiteralNode_DoesNotAddVertex()
        {
            // Arrange
            var start = CreateAssignment("x", 1);
            var literal = new LiteralNode(42);
            var visitor = new ControlFlowGraphGeneratorVisitor(start);

            // Act
            visitor.Visit(literal, start);
            var cfg = visitor.GetCFG();

            // Assert
            Assert.Equal(1, cfg.VertexCount()); // Only start
        }

        [Fact]
        public void Visit_VariableNode_DoesNotAddVertex()
        {
            // Arrange
            var start = CreateAssignment("x", 1);
            var variable = new VariableNode("y");
            var visitor = new ControlFlowGraphGeneratorVisitor(start);

            // Act
            visitor.Visit(variable, start);
            var cfg = visitor.GetCFG();

            // Assert
            Assert.Equal(1, cfg.VertexCount()); // Only start
        }

        [Theory]
        [InlineData(typeof(PlusNode))]
        [InlineData(typeof(MinusNode))]
        [InlineData(typeof(TimesNode))]
        [InlineData(typeof(FloatDivNode))]
        [InlineData(typeof(IntDivNode))]
        [InlineData(typeof(ModulusNode))]
        [InlineData(typeof(ExponentiationNode))]
        public void Visit_BinaryOperatorNodes_DoNotAddVertices(Type nodeType)
        {
            // Arrange
            var start = CreateAssignment("x", 1);
            var left = new LiteralNode(1);
            var right = new LiteralNode(2);
            var node = (dynamic)Activator.CreateInstance(nodeType, left, right);
            var visitor = new ControlFlowGraphGeneratorVisitor(start);

            // Act
            visitor.Visit(node, start);
            var cfg = visitor.GetCFG();

            // Assert
            Assert.Equal(1, cfg.VertexCount()); // Only start
        }

        #endregion

        #region Complex Flow Tests

        [Fact]
        public void Visit_ComplexNestedStructure_BuildsCorrectCFG()
        {
            // Arrange: Create a complex structure with nested blocks
            var start = CreateAssignment("init", 0);
            var stmt1 = CreateAssignment("a", 1);
            var stmt2 = CreateAssignment("b", 2);
            var stmt3 = CreateAssignment("c", 3);
            var stmt4 = CreateAssignment("d", 4);
            var returnStmt = CreateReturn(100);

            var innerBlock = new BlockStmt(new List<Statement> { stmt3, stmt4 });
            var outerBlock = new BlockStmt(new List<Statement> { stmt1, stmt2, innerBlock, returnStmt });
            var visitor = new ControlFlowGraphGeneratorVisitor(start);

            // Act
            visitor.Visit(outerBlock, start);
            var cfg = visitor.GetCFG();

            // Assert
            Assert.Equal(6, cfg.VertexCount());
            Assert.True(cfg.HasEdge(start, stmt1));
            Assert.True(cfg.HasEdge(stmt1, stmt2));
            Assert.True(cfg.HasEdge(stmt2, stmt3));
            Assert.True(cfg.HasEdge(stmt3, stmt4));
            Assert.True(cfg.HasEdge(stmt4, returnStmt));
        }

        [Fact]
        public void Visit_MixedStatementsAndBlocks_MaintainsCorrectOrder()
        {
            // Arrange
            var start = CreateAssignment("start", 0);
            var stmt1 = CreateAssignment("before", 1);
            var blockStmt1 = CreateAssignment("in_block_1", 2);
            var blockStmt2 = CreateAssignment("in_block_2", 3);
            var stmt2 = CreateAssignment("after", 4);

            var block = new BlockStmt(new List<Statement> { blockStmt1, blockStmt2 });
            var visitor = new ControlFlowGraphGeneratorVisitor(start);

            // Act
            visitor.Visit(stmt1, start);
            visitor.Visit(block, stmt1);
            visitor.Visit(stmt2, blockStmt2); // Last statement in block

            var cfg = visitor.GetCFG();

            // Assert
            Assert.Equal(5, cfg.VertexCount());
            Assert.True(cfg.HasEdge(start, stmt1));
            Assert.True(cfg.HasEdge(stmt1, blockStmt1));
            Assert.True(cfg.HasEdge(blockStmt1, blockStmt2));
            Assert.True(cfg.HasEdge(blockStmt2, stmt2));
        }

        #endregion

        #region Edge Cases and Error Conditions

        [Fact]
        public void Visit_SameStatementTwice_DoesNotCreateDuplicateVertex()
        {
            // Arrange
            var start = CreateAssignment("x", 1);
            var stmt = CreateAssignment("y", 2);
            var visitor = new ControlFlowGraphGeneratorVisitor(start);

            // Act
            visitor.Visit(stmt, start);
            visitor.Visit(stmt, start); // Try to add same statement again
            var cfg = visitor.GetCFG();

            // Assert
            Assert.Equal(2, cfg.VertexCount()); // Should still be 2
        }

        [Fact]
        public void Visit_BlockWithOnlyReturnStatement_AddsOnlyReturn()
        {
            // Arrange
            var start = CreateAssignment("x", 1);
            var returnStmt = CreateReturn(42);
            var block = new BlockStmt(new List<Statement> { returnStmt });
            var visitor = new ControlFlowGraphGeneratorVisitor(start);

            // Act
            visitor.Visit(block, start);
            var cfg = visitor.GetCFG();

            // Assert
            Assert.Equal(2, cfg.VertexCount());
            Assert.True(cfg.HasEdge(start, returnStmt));
        }

        #endregion

        #region CFG Properties Tests

        [Fact]
        public void GetCFG_ReturnsNonNullGraph()
        {
            // Arrange
            var start = CreateAssignment("x", 1);
            var visitor = new ControlFlowGraphGeneratorVisitor(start);

            // Act
            var cfg = visitor.GetCFG();

            // Assert
            Assert.NotNull(cfg);
        }

        [Fact]
        public void Visit_LargeSequence_HandlesCorrectly()
        {
            // Arrange
            var statements = new List<AssignmentStmt>();
            for (int i = 0; i < 100; i++)
            {
                statements.Add(CreateAssignment($"var{i}", i));
            }
            var visitor = new ControlFlowGraphGeneratorVisitor(statements[0]);

            // Act
            for (int i = 1; i < 100; i++)
            {
                visitor.Visit(statements[i], statements[i - 1]);
            }
            var cfg = visitor.GetCFG();

            // Assert
            Assert.Equal(100, cfg.VertexCount());
            Assert.Equal(99, cfg.EdgeCount());
        }

        [Fact]
        public void Visit_AllVertices_AreReachableFromStart()
        {
            // Arrange
            var start = CreateAssignment("x", 1);
            var stmt1 = CreateAssignment("a", 10);
            var stmt2 = CreateAssignment("b", 20);
            var stmt3 = CreateAssignment("c", 30);
            var visitor = new ControlFlowGraphGeneratorVisitor(start);

            // Act
            visitor.Visit(stmt1, start);
            visitor.Visit(stmt2, stmt1);
            visitor.Visit(stmt3, stmt2);
            var cfg = visitor.GetCFG();

            // Assert - All vertices should be reachable via BFS/DFS from start
            var reachable = new HashSet<Statement>();
            var queue = new Queue<Statement>();
            queue.Enqueue(start);
            reachable.Add(start);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var neighbor in cfg.GetNeighbors(current))
                {
                    if (!reachable.Contains(neighbor))
                    {
                        reachable.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }

            Assert.Equal(cfg.VertexCount(), reachable.Count);
        }

        #endregion
    }
}