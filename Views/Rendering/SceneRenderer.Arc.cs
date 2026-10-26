using System.Numerics;
using Raylib_cs;
using ShapeShifter.Geometry;
using ShapeShifter.Models;

namespace ShapeShifter.Views.Rendering;

public partial class SceneRenderer
{
    public static void DrawCircularArc(
        CircularArcSegment arc,
        Color color,
        LineAlgorithm algorithm)
    {
        Vector2 previous = arc.Start.Position;

        foreach (var point in
            CircularArcPointGenerator.GeneratePoints(arc).Skip(1))
        {
            DrawLine(previous, point, color, algorithm);
            previous = point;
        }
    }
}