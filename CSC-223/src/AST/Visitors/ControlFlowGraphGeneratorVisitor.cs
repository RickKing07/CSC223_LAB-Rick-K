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
}