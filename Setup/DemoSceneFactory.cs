using System.Numerics;
using ShapeShifter.Editing;
using ShapeShifter.Models;

namespace ShapeShifter.Setup;

/// <summary>
/// Defines an editable startup scene that demonstrates the supported edge types and relations.
/// </summary>
public static class DemoSceneFactory
{
    /// <summary>
    /// Creates two polygons, length and perpendicularity constraints, a Bezier segment and a G1 arc.
    /// Uses the regular editor operations so the initial geometry satisfies the same rules as later edits.
    /// </summary>
    public static Scene Create()
    {
        var scene = new Scene();

        var first = AddPolygon(
            scene,
            new Vector2(350, 170),
            new Vector2(560, 170),
            new Vector2(560, 390),
            new Vector2(350, 390));

        var second = AddPolygon(
            scene,
            new Vector2(700, 180),
            new Vector2(870, 180),
            new Vector2(900, 420),
            new Vector2(700, 420));

        var editor = new GeometryEditor(scene);

        Require(
            editor.TrySetLength(first.Edges[0], 210f),
            "Could not create the initial length constraint.");

        Require(
            editor.TryAddPerpendicular(
                first.Edges[0],
                second.Edges[3]),
            "Could not create the initial perpendicular constraint.");

        Require(
            editor.TrySetEdgeGeometry(
                first.Edges[1],
                EdgeGeometryType.Bezier,
                out _),
            "Could not create the initial Bezier segment.");

        var arcEdge = second.Edges[2];

        Require(
            editor.TrySetContinuity(
                arcEdge.Start, ContinuityType.G0),
            "Could not set initial arc continuity.");

        Require(
            editor.TrySetContinuity(
                arcEdge.End, ContinuityType.G0),
            "Could not set initial arc continuity.");

        Require(
            editor.TrySetEdgeGeometry(
                arcEdge,
                EdgeGeometryType.CircularArc,
                out _),
            "Could not create the initial circular arc.");

        Require(
            editor.TrySetContinuity(
                arcEdge.Start, ContinuityType.G1),
            "Could not create the initial arc tangency.");

        return scene;
    }

    /// <summary>
    /// Builds and registers a polygon from coordinates, keeping startup scene definitions readable.
    /// </summary>
    private static Polygon AddPolygon(
        Scene scene,
        params Vector2[] positions)
    {
        var vertices = positions
            .Select(position => new Vertex(position))
            .ToArray();

        var polygon = new Polygon(vertices);
        scene.AddPolygon(polygon);

        return polygon;
    }

    /// <summary>
    /// Ensures every requested demo operation succeeded before the scene is returned.
    /// </summary>
    private static void Require(bool success, string message)
    {
        if (!success)
        {
            throw new InvalidOperationException(message);
        }
    }
}
