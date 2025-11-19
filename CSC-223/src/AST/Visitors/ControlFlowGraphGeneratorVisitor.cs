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

    public class ControlFlowGraphGeneratorVisitor : IVisitor<Statement, object>
    {
        private DiGraph<Statement>? CFG;
        public ControlFlowGraphGeneratorVisitor(Statement start)
        {
            CFG = new DiGraph<Statement>();
            CFG.AddVertex(start); //verify its not a block stmnt "{" when you start
        }
        public object Visit(PlusNode node, Statement prev)
        {
            return null;
        }

        public object Visit(MinusNode node, Statement prev)
        {
            return null;
        }

        public object Visit(TimesNode node, Statement prev)
        {
            return null;
        }

        public object Visit(FloatDivNode node, Statement prev)
        {
            return null;
        }

        public object Visit(IntDivNode node, Statement prev)
        {
            return null;
        }

        public object Visit(ModulusNode node, Statement prev)
        {
            return null;
        }

        public object Visit(ExponentiationNode node, Statement prev)
        {
            return null;

        }


        #region Expression Node Visit Methods

        public object Visit(VariableNode node, Statement prev)
        {
            return null;
        }


        public object Visit(LiteralNode node, Statement prev)
        {
            return null;
        }

        #endregion


        public object Visit(AssignmentStmt node, Statement prev)
        {
            CFG.AddVertex(node);
            CFG.AddEdge(prev, node);
            return null;
        }

        public object Visit(ReturnStmt node, Statement prev)
        {
            CFG.AddVertex(node);
            CFG.AddEdge(prev, node);
            return null;
        }

        public object Visit(BlockStmt node, Statement prev)
        {
            // Use this block's symbol table, which is already linked to its parent
            //prev = node.accept

            foreach (var stmt in node.Statements)
            {
                if (stmt is BlockStmt)
                {
                    Visit((BlockStmt)stmt, prev);
                }
                if (prev is ReturnStmt) //should this continue or break, should we even think about the program after the return statment? 
                //Also make this not a continue, use an if else or something, switch up the order
                {
                    CFG.AddVertex(stmt);
                    continue;
                }
                stmt.Accept(this, prev);
                prev = stmt;
            }
            return null;
        }

        public DiGraph<Statement> GetCFG() //Method to help test, might just be able to set up the get; property
        {
            return CFG;
        }
    }

}
