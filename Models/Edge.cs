
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
        public CubicBezierSegment? Bezier { get; private set; }

        public void EnableBezier()
        {
            Bezier ??= new CubicBezierSegment(Start, End);
        }

        public void EnableBezier(CubicBezierSegment segment)
        {
            ArgumentNullException.ThrowIfNull(segment);

            if (segment.Start != Start || segment.End != End)
            {
                throw new ArgumentException(
                    "The segment must use this edge's vertices.",
                    nameof(segment));
            }

            Bezier = segment;
        }

        public void DisableBezier()
        {
            Bezier = null;
        }
    }
}