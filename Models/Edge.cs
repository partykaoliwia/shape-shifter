
namespace ShapeShifter.Models
{
    public class Edge
    {
        public Vertex Start { get; }
        public Vertex End { get; }
        public Edge(Vertex start, Vertex end)
        {
            Start = start;
            End = end;
        }
    }
}