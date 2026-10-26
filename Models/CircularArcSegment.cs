using System.Numerics;

namespace ShapeShifter.Models;

public sealed class CircularArcSegment
{
    public Vertex Start { get; }
    public Vertex End { get; }

    public bool Clockwise { get; }

    public Vector2 Center =>
        (Start.Position + End.Position) / 2f;

    public float Radius =>
        Vector2.Distance(Start.Position, End.Position) / 2f;

    public CircularArcSegment(
        Vertex start,
        Vertex end,
        bool clockwise = true)
    {
        Start = start;
        End = end;
        Clockwise = clockwise;
    }
}