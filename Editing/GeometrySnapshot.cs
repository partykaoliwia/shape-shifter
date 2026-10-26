using System.Numerics;
using ShapeShifter.Models;

namespace ShapeShifter.Editing;

internal static class GeometrySnapshot
{
    public static Dictionary<IGeometryVariable, Vector2> Capture(
        Scene scene)
    {
        var values = new Dictionary<IGeometryVariable, Vector2>();

        foreach (var edge in scene.Polygons.SelectMany(p => p.Edges))
        {
            values.TryAdd(edge.Start, edge.Start.Position);
            values.TryAdd(edge.End, edge.End.Position);

            if (edge.Arc is { } arc)
            {
                values.TryAdd(arc, arc.TangentDirection);
            }

            if (edge.Bezier is { } bezier)
            {
                values.TryAdd(bezier.Control1, bezier.Control1.Offset);
                values.TryAdd(bezier.Control2, bezier.Control2.Offset);
            }
        }

        return values;
    }

    public static void Apply(
        IReadOnlyDictionary<IGeometryVariable, Vector2> values)
    {
        foreach (var vertex in values.Keys.OfType<Vertex>())
        {
            vertex.MoveTo(values[vertex]);
        }

        foreach (var point in values.Keys.OfType<BezierControlPoint>())
        {
            point.MoveTo(point.Anchor.Position + values[point]);
        }

        foreach (var arc in values.Keys.OfType<CircularArcSegment>())
        {
            arc.SetTangentDirection(values[arc]);
        }
    }
}