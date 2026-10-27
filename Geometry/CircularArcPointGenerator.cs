using System.Numerics;
using ShapeShifter.Models;

namespace ShapeShifter.Geometry;

/// <summary>
/// Converts a valid circular arc into points for rendering and hit testing.
/// </summary>
public static class CircularArcPointGenerator
{
    private const float TargetSampleSpacing = 4f;
    private const int MinSteps = 16;
    private const int MaxSteps = 4096;

    /// <summary>
    /// Samples the angular sweep with a step count based on arc length.
    /// Includes exact endpoints and returns no points for invalid arc geometry.
    /// </summary>
    public static IEnumerable<Vector2> GeneratePoints(
        CircularArcSegment arc)
    {
        if (!arc.TryGetGeometry(out var geometry))
        {
            yield break;
        }

        yield return arc.Start.Position;

        if (geometry.Radius <= 0.000001f)
        {
            yield return arc.End.Position;
            yield break;
        }

        float arcLength =
            MathF.Abs(geometry.SweepAngle) * geometry.Radius;

        int steps = (int)Math.Clamp(
            MathF.Ceiling(arcLength / TargetSampleSpacing),
            MinSteps,
            MaxSteps);

        for (int i = 1; i < steps; i++)
        {
            yield return geometry.PointAt((float)i / steps);
        }

        yield return arc.End.Position;
    }
}
