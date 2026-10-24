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
    }
}