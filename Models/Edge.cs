namespace ShapeShifter.Models;

/// <summary>
/// Connects two shared vertices and holds at most one curved representation.
/// An edge with neither a Bezier segment nor an arc is a line segment.
/// </summary>
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

    /// <summary>
    /// Returns to a line segment by discarding the current curved representation.
    /// </summary>
    public void EnableLine()
    {
        Bezier = null;
        Arc = null;
    }

    /// <summary>
    /// Keeps the existing Bezier segment or creates one with handles along the chord.
    /// </summary>
    public void EnableBezier()
    {
        EnableBezier(Bezier ?? new CubicBezierSegment(Start, End));
    }

    /// <summary>
    /// Uses a Bezier segment with this edge's endpoint objects and discards any arc.
    /// </summary>
    public void EnableBezier(CubicBezierSegment segment)
    {
        ArgumentNullException.ThrowIfNull(segment);
        EnsureMatchingVertices(segment.Start, segment.End);

        Bezier = segment;
        Arc = null;
    }

    /// <summary>
    /// Creates an arc with an initial tangent perpendicular to the endpoint chord.
    /// </summary>
    public void EnableArc(bool clockwise = true)
    {
        EnableArc(new CircularArcSegment(Start, End, clockwise));
    }

    /// <summary>
    /// Uses an arc with this edge's endpoint objects and discards any Bezier segment.
    /// </summary>
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
