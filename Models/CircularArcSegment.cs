using System.Numerics;

namespace ShapeShifter.Models;

public sealed class CircularArcSegment : IGeometryVariable
{
    public Vertex Start { get; }
    public Vertex End { get; }

    public bool Clockwise { get; }

    public Vector2 TangentDirection { get; private set; }

    public Vector2 Center =>
        TryGetGeometry(out var geometry)
            ? geometry.Center
            : (Start.Position + End.Position) / 2f;

    public float Radius =>
        TryGetGeometry(out var geometry)
            ? geometry.Radius
            : 0f;

    public CircularArcSegment(
        Vertex start,
        Vertex end,
        bool clockwise = true)
    {
        Start = start;
        End = end;
        Clockwise = clockwise;

        Vector2 chord = end.Position - start.Position;

        TangentDirection = clockwise
            ? new Vector2(chord.Y, -chord.X)
            : new Vector2(-chord.Y, chord.X);
    }

    public void SetTangentDirection(Vector2 direction)
    {
        TangentDirection = direction;
    }

    public bool TryGetGeometry(out CircularArcGeometry geometry)
    {
        return TryGetGeometry(
            Start.Position,
            End.Position,
            TangentDirection,
            out geometry);
    }

    public bool TryGetGeometry(
        Vector2 start,
        Vector2 end,
        Vector2 tangent,
        out CircularArcGeometry geometry)
    {
        geometry = default;

        if (!SupportsContinuity(Start, End))
        {
            return false;
        }

        return CircularArcGeometry.TryCreate(
            start,
            end,
            tangent,
            Start.Continuity == ContinuityType.G1,
            End.Continuity == ContinuityType.G1,
            Clockwise,
            out geometry);
    }

    public static bool SupportsContinuity(Vertex start, Vertex end)
    {
        return start.Continuity != ContinuityType.C1
            && end.Continuity != ContinuityType.C1
            && !(start.Continuity == ContinuityType.G1
                && end.Continuity == ContinuityType.G1);
    }
}