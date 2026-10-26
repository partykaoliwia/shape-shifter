using System.Numerics;
using ShapeShifter.Models;

namespace ShapeShifter.Geometry;

public static class CircularArcPointGenerator
{
    private const float TargetSampleSpacing = 4f;
    private const int MinSteps = 16;
    private const int MaxSteps = 512;

    public static IEnumerable<Vector2> GeneratePoints(
        CircularArcSegment arc)
    {
        Vector2 start = arc.Start.Position;
        Vector2 end = arc.End.Position;

        Vector2 center = arc.Center;
        float radius = arc.Radius;

        yield return start;

        if (radius <= 0.000001f)
        {
            yield return end;
            yield break;
        }

        Vector2 startDirection = start - center;

        float startAngle = MathF.Atan2(
            startDirection.Y,
            startDirection.X);

        float sweepAngle = arc.Clockwise
            ? MathF.PI
            : -MathF.PI;

        float arcLength = MathF.PI * radius;

        int steps = Math.Clamp(
            (int)MathF.Ceiling(arcLength / TargetSampleSpacing),
            MinSteps,
            MaxSteps);

        for (int i = 1; i < steps; i++)
        {
            float t = (float)i / steps;
            float angle = startAngle + sweepAngle * t;

            Vector2 direction = new(
                MathF.Cos(angle),
                MathF.Sin(angle));

            yield return center + radius * direction;
        }

        // Use the exact endpoint instead of its approximation.
        yield return end;
    }
}