using System.ComponentModel;

namespace AST
{
    public class DiGraph<T> where T : notnull
    {
        protected Dictionary<T, DLL<T>> _adjacencyList;

        public bool AddVertex(T vertex)
        {
            if (_adjacencyList.Keys.Contains(vertex))
            {
                return false;
            }
            _adjacencyList.Add(vertex, null);
            return true;
        }

        public bool AddEdge(T source, T destination) //if not already exists, addd argument exeption if source not in _adj
        {
            if (!_adjacencyList.Keys.Contains(source) || !_adjacencyList.Keys.Contains(destination)) { throw new ArgumentException($"Node {source} or {destination} not in DiGraph"); }
            _adjacencyList[source].Add(destination);
            return true;
        }
        public bool RemoveVertex(T vertex)
        {
            //im assuming garbage collection type things are happenign
            _adjacencyList.Remove(vertex);
            return true;
        }
        public bool RemoveEdge(T source, T destination)
        {
            if (!_adjacencyList.Keys.Contains(source) || !_adjacencyList.Keys.Contains(destination)) { throw new ArgumentException($"Node {source} or {destination} not in DiGraph"); }

            _adjacencyList[source].Remove(destination);
            return true;
        }
        public bool HasEdge(T source, T destination)
        {
            return _adjacencyList[source].Contains(destination);
        }
        public List<T> GetNeighbors(T vertex) //add exeption throw
        {
            List<T> AdjacentNodes = null;
            foreach (T node in _adjacencyList[vertex])
            {
                AdjacentNodes.Add(node);
            }
            return AdjacentNodes;
        }

        public IEnumerable<T> GetVertices()
        {
            return null;
            //to be implimented
        }

        public int VertexCount()
        {
            return _adjacencyList.Count();
        }
        public int EdgeCount()
        {
            int count = 0;
            foreach (T node in _adjacencyList.Keys)
            {
                count += _adjacencyList[node].Count;
            }
            return count;
        }

        public string ToString()
        {
            return null;
            //to be implimented
        }
    }
}