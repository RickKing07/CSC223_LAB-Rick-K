using System.ComponentModel;

namespace AST
{
    public class DiGraph<T> where T : notnull
    {
        protected Dictionary<T, DLL<T>> _adjacencyList;

        public DiGraph()
        {
            this._adjacencyList = new Dictionary<T, DLL<T>>();
        }

        public bool AddVertex(T vertex)
        {
            if (_adjacencyList.Keys.Contains(vertex))
            {
                return false;
            }
            _adjacencyList.Add(vertex, new DLL<T>());
            return true;
        }

        public bool AddEdge(T source, T destination)
        {
            if (!_adjacencyList.Keys.Contains(source) || !_adjacencyList.Keys.Contains(destination)) { throw new ArgumentException($"Node {source} or {destination} not in DiGraph"); }
            if (_adjacencyList[source].Contains(destination)) return false;
            _adjacencyList[source].Add(destination);
            return true;
        }
        public bool RemoveVertex(T vertex)
        {
            if (!_adjacencyList.Keys.Contains(vertex)) return false;

            foreach (T node in _adjacencyList.Keys)     //Removes all associated edges
            {
                if (_adjacencyList[node].Contains(vertex))
                {
                    _adjacencyList[node].Remove(vertex);
                }
            }

            return _adjacencyList.Remove(vertex); //removes the veretx itself
        }
        public bool RemoveEdge(T source, T destination)
        {
            //Ensure valid args
            if (!_adjacencyList.Keys.Contains(source) || !_adjacencyList.Keys.Contains(destination)) { throw new ArgumentException($"Node {source} or {destination} not in DiGraph"); }
            if (!_adjacencyList[source].Contains(destination)) return false;

            //Remove
            return _adjacencyList[source].Remove(destination);
        }
        public bool HasEdge(T source, T destination)
        {
            if (!_adjacencyList.Keys.Contains(source)) return false; //Should this use contains key we built or is .keys.contains okay?
            return _adjacencyList[source].Contains(destination);
        }
        public List<T> GetNeighbors(T vertex)
        {
            if (!_adjacencyList.Keys.Contains(vertex)) throw new ArgumentException($"{vertex} not found in DiGraph");
            List<T> AdjacentNodes = new List<T>();
            foreach (T node in _adjacencyList[vertex])
            {
                AdjacentNodes.Add(node);
            }
            return AdjacentNodes;
        }

        public IEnumerable<T> GetVertices()
        {
            foreach (T node in _adjacencyList.Keys)
            {
                yield return node;
            }
        }

        public int VertexCount()
        {
            return _adjacencyList.Count();
        }
        public int EdgeCount()
        {

            int count = 0;
            //Loop the entire Dict keeping count of values
            foreach (T node in _adjacencyList.Keys)
            {
                count += _adjacencyList[node].Count;
            }
            return count;
        }

        public string ToString()
        {

            return $"Vertices: {VertexCount()} Edges: {EdgeCount()}. \n {_adjacencyList.ToString()}";
        }
    }


    /// <summary>
    /// Exception thrown when an evaluation error occurs
    /// </summary>
    public class EvaluationException : Exception
    {
        public EvaluationException(string message) : base(message) { }
    }

    public class ControlFlowGraphGeneratorVisitor : IVisitor<Statement, Statement>
    {
        private DiGraph<Statement>? CFG;
        public ControlFlowGraphGeneratorVisitor()
        {
            CFG = new DiGraph<Statement>();
        }
        public Statement Visit(PlusNode node, Statement prev)
        {
            return null;
        }

        public Statement Visit(MinusNode node, Statement prev)
        {
            return null;
        }

        public Statement Visit(TimesNode node, Statement prev)
        {
            return null;
        }

        public Statement Visit(FloatDivNode node, Statement prev)
        {
            return null;
        }

        public Statement Visit(IntDivNode node, Statement prev)
        {
            return null;
        }

        public Statement Visit(ModulusNode node, Statement prev)
        {
            return null;
        }

        public Statement Visit(ExponentiationNode node, Statement prev)
        {
            return null;

        }


        #region Expression Node Visit Methods

        public Statement Visit(VariableNode node, Statement prev)
        {
            return null;
        }


        public Statement Visit(LiteralNode node, Statement prev)
        {
            return null;
        }

        #endregion


        public Statement Visit(AssignmentStmt node, Statement prev)
        {
            CFG.AddVertex(node);
            if (prev != null)
            {
                CFG.AddEdge(prev, node);
            }
            return node;
        }

        public Statement Visit(ReturnStmt node, Statement prev)
        {
            CFG.AddVertex(node);
            if (prev != null)
            {
                CFG.AddEdge(prev, node);
            }

            return node;
        }

        // BlockStmt: Process statements sequentially, "folding" the block
        public Statement Visit(BlockStmt node, Statement prev)
        {
            Statement lastStmt = prev;  // Start with the previous statement

            foreach (var stmt in node.Statements)
            {
                // If we hit a return, stop (unreachable code after return)
                if (lastStmt is ReturnStmt)
                {
                    // Still add unreachable statements as vertices (but no edges)
                    if (stmt is AssignmentStmt || stmt is ReturnStmt)
                    {
                        CFG.AddVertex(stmt);
                    }
                    continue;
                }

                // Process the statement and get the new "last statement"
                Statement newLast = stmt.Accept(this, lastStmt);

                // Update lastStmt for next iteration
                if (newLast != null)
                {
                    lastStmt = newLast;
                }
            }

            return lastStmt;  // Return the last statement from this block
        }


        public DiGraph<Statement> GetCFG() //Method to help test, might just be able to set up the get; property
        {
            return CFG;
        }
    }

}
