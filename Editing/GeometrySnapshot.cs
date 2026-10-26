using System.Numerics;
using ShapeShifter.Models;

namespace ShapeShifter.Editing;

internal static class GeometrySnapshot
{
    public static Dictionary<IGeometryVariable, Vector2> Capture(Scene scene)
    {
        var values = new Dictionary<IGeometryVariable, Vector2>();

        foreach (var edge in scene.Polygons.SelectMany(p => p.Edges))
        {
            values.TryAdd(edge.Start, edge.Start.Position);
            values.TryAdd(edge.End, edge.End.Position);

            var segment = edge.Bezier;
            if (segment == null)
            {
                continue;
            }

            values.TryAdd(segment.Control1, segment.Control1.Offset);
            values.TryAdd(segment.Control2, segment.Control2.Offset);
        }

        return values;
    }

    public static void Apply(IReadOnlyDictionary<IGeometryVariable, Vector2> values)
    {
        foreach (var vertex in values.Keys.OfType<Vertex>())
        {
            vertex.MoveTo(values[vertex]);
        }

        foreach (var controlPoint in values.Keys.OfType<BezierControlPoint>())
        {
            controlPoint.MoveTo(controlPoint.Anchor.Position + values[controlPoint]);
        }
    }
}