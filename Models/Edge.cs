namespace ShapeShifter.Models;

public class Edge
{
    public Vertex Start { get; }
    public Vertex End { get; }

    public CubicBezierSegment? Bezier { get; private set; }
    public CircularArcSegment? Arc { get; private set; }

    public EdgeGeometryType GeometryType =>
        Arc != null
            ? EdgeGeometryType.CircularArc
            : Bezier != null
                ? EdgeGeometryType.Bezier
                : EdgeGeometryType.Line;

    public bool IsLine => GeometryType == EdgeGeometryType.Line;

    public Edge(Vertex start, Vertex end)
    {
        Start = start;
        End = end;
    }

    public void EnableLine()
    {
        Bezier = null;
        Arc = null;
    }

    public void EnableBezier()
    {
        EnableBezier(Bezier ?? new CubicBezierSegment(Start, End));
    }

    public void EnableBezier(CubicBezierSegment segment)
    {
        ArgumentNullException.ThrowIfNull(segment);
        EnsureMatchingVertices(segment.Start, segment.End);

        Bezier = segment;
        Arc = null;
    }

    public void EnableArc(bool clockwise = true)
    {
        EnableArc(new CircularArcSegment(Start, End, clockwise));
    }

    public void EnableArc(CircularArcSegment segment)
    {
        ArgumentNullException.ThrowIfNull(segment);
        EnsureMatchingVertices(segment.Start, segment.End);

        Arc = segment;
        Bezier = null;
    }

    private void EnsureMatchingVertices(Vertex start, Vertex end)
    {
        if (start != Start || end != End)
        {
            throw new ArgumentException(
                "The segment must use this edge's vertices.");
        }
    }
}