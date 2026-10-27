using System.Numerics;
using System.Text.Json;
using ShapeShifter.Models;

namespace ShapeShifter.SaveLoad;

public static class SceneFile
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        IncludeFields = true
    };

    public static void Save(Scene scene, string path)
    {
        var allEdges = scene.Polygons.SelectMany(p => p.Edges).ToArray();

        var polygons = scene.Polygons.Select(p => p.Edges.Select(e =>
            new EdgeData(
                e.Start.Position,
                e.Start.Continuity,
                e.GeometryType,
                e.Bezier?.Control1.Position ?? default,
                e.Bezier?.Control2.Position ?? default,
                e.Arc?.TangentDirection ?? default,
                e.Arc?.Clockwise ?? true,
                scene.GetLengthConstraint(e)?.TargetLength,
                e.Thickness
            )).ToArray()
        ).ToArray();

        var perpendicular = scene.GetPerpendicularConstraints()
            .Select(c => new[]
            {
                Array.IndexOf(allEdges, c.First),
                Array.IndexOf(allEdges, c.Second)
            }).ToArray();

        var data = new SceneData(polygons, perpendicular);
        File.WriteAllText(path, JsonSerializer.Serialize(data, Options));
    }

    public static Scene Load(string path)
    {
        var data = JsonSerializer.Deserialize<SceneData>(
            File.ReadAllText(path), Options)
            ?? throw new InvalidDataException("Empty scene file.");

        var scene = new Scene();
        var allEdges = new List<Edge>();

        foreach (var savedEdges in data.Polygons)
        {
            var vertices = savedEdges.Select(saved =>
            {
                var vertex = new Vertex(saved.Position);
                vertex.SetContinuity(saved.Continuity);
                return vertex;
            }).ToArray();

            var polygon = new Polygon(vertices);
            scene.AddPolygon(polygon);

            for (int i = 0; i < savedEdges.Length; i++)
            {
                var saved = savedEdges[i];
                var edge = polygon.Edges[i];
                edge.Thickness = Math.Clamp(saved.Thickness, 1, 10);

                switch (saved.Type)
                {
                    case EdgeGeometryType.Line:
                        break;

                    case EdgeGeometryType.Bezier:
                        edge.EnableBezier(new CubicBezierSegment(
                            edge.Start, edge.End,
                            saved.Control1, saved.Control2));
                        break;

                    case EdgeGeometryType.CircularArc:
                        var arc = new CircularArcSegment(edge.Start, edge.End, saved.Clockwise);
                        arc.SetTangentDirection(saved.Tangent);
                        edge.EnableArc(arc);
                        break;

                    default:
                        throw new InvalidDataException("Unknown edge type.");
                }

                if (saved.Length is float length)
                    scene.SetLengthConstraint(edge, length);

                allEdges.Add(edge);
            }
        }

        foreach (var pair in data.Perpendicular)
        {
            if (pair.Length != 2
                || pair[0] < 0 || pair[0] >= allEdges.Count
                || pair[1] < 0 || pair[1] >= allEdges.Count)
            {
                throw new InvalidDataException("Invalid edge indices.");
            }

            scene.AddPerpendicularConstraint(allEdges[pair[0]], allEdges[pair[1]]);
        }
        return scene;
    }

    public sealed record SceneData(EdgeData[][] Polygons,int[][] Perpendicular);
    public sealed record EdgeData(
        Vector2 Position,
        ContinuityType Continuity,
        EdgeGeometryType Type,
        Vector2 Control1,
        Vector2 Control2,
        Vector2 Tangent,
        bool Clockwise,
        float? Length,
        int Thickness = 1);
}