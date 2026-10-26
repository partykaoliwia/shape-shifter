using System.Numerics;

namespace ShapeShifter.Models
{
    public class Polygon
    {
        // polygons contain only edges and edges contain vertices
        private readonly List<Edge>  edges = new();
        public IReadOnlyList<Edge> Edges => edges;

        public Polygon(IReadOnlyList<Vertex> vertices)
        {
            if (vertices.Count < 3)
            {
                throw new ArgumentException("Not enough vertices.");
            }
            
            for (int i=0; i<vertices.Count; i++)
            {
                int nextIndex = (i + 1) % vertices.Count;
                var edge = new Edge(vertices[i],vertices[nextIndex]);
                edges.Add(edge);
            }
        }

        public bool SplitEdge(Edge edge)
        {
            int index = edges.IndexOf(edge);

            if (index < 0)
            {
                return false;
            }
            var segment = edge.Bezier;

            if (segment != null)
            { 
                var (first, second) = segment.Split();

                var firstEdge = new Edge(first.Start, first.End);
                firstEdge.EnableBezier(first);

                var secondEdge = new Edge(second.Start, second.End);
                secondEdge.EnableBezier(second);

                edges[index] = firstEdge;
                edges.Insert(index + 1, secondEdge);

                return true;
            }

            Vector2 midpoint = (edge.Start.Position + edge.End.Position) / 2f;
            var vertex = new Vertex(midpoint);

            edges[index] = new Edge(edge.Start, vertex);
            edges.Insert(index + 1, new Edge(vertex, edge.End));

            return true;
        }

        public bool TryRemoveVertex(Vertex vertex)
        {
            if (edges.Count <= 3)
            {
                return false;
            }

            int outgoingIndex = edges.FindIndex(edge => ReferenceEquals(edge.Start, vertex));

            if (outgoingIndex < 0)
            {
                return false;
            }

            int incomingIndex = (outgoingIndex - 1 + edges.Count) % edges.Count;

            Edge incoming = edges[incomingIndex];
            Edge outgoing = edges[outgoingIndex];

            var replacement = new Edge(incoming.Start, outgoing.End);

            edges[incomingIndex] = replacement;
            edges.RemoveAt(outgoingIndex);

            return true;
        }
    }
}