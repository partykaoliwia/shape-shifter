using System.Numerics;
using Raylib_cs;
using ShapeShifter.Geometry;
using ShapeShifter.Models;

namespace ShapeShifter.Views.Rendering;

public partial class SceneRenderer
{
    /// <summary>
    /// Connects sampled arc points with the selected line algorithm.
    /// Skips the first sample because it is already used as the initial endpoint.
    /// </summary>
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
