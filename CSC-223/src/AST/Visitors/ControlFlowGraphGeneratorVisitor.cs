using System.ComponentModel;

namespace AST
{
    public class DiGraph<T> where T : notnull
    {
        protected Dictionary<T, DLL<T>> _adjacencyList;

        public bool AddVertex(T vertex) //does there need to be a return false case?
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
            //removes a directed edge from source to destination, add argument exeptiopn iof source not in adj
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
    }
}