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