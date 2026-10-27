using System.Numerics;
using ShapeShifter.Models;

namespace ShapeShifter.Geometry;

/// <summary>
/// Measures distances to line and curved edges for mouse hit testing.
/// </summary>
public static class EdgeGeometry
{
    /// <summary>
    /// Measures a line directly and approximates curves with sampled line segments.
    /// Returns squared distance so hit tests can compare radii without taking square roots.
    /// </summary>
    public static float DistanceToEdgeSquared(
        Edge edge,
        Vector2 position)
    {
        if (edge.Arc is { } arc)
        {
            return DistanceToPolylineSquared(
                position,
                CircularArcPointGenerator.GeneratePoints(arc));
        }

        if (edge.Bezier is { } bezier)
        {
            return DistanceToPolylineSquared(
                position,
                CubicBezierPointGenerator.GeneratePoints(bezier));
        }

        return DistanceToSegmentSquared(
            position,
            edge.Start.Position,
            edge.End.Position);
    }

    /// <summary>
    /// Finds the closest segment in a sequence of points.
    /// Returns positive infinity when the sequence contains no segments.
    /// </summary>
    private static float DistanceToPolylineSquared(
        Vector2 position,
        IEnumerable<Vector2> points)
    {
        using var iterator = points.GetEnumerator();

        if (!iterator.MoveNext())
        {
            return float.PositiveInfinity;
        }

        Vector2 previous = iterator.Current;
        float bestDistance = float.PositiveInfinity;

        while (iterator.MoveNext())
        {
            Vector2 current = iterator.Current;

            float distance = DistanceToSegmentSquared(
                position, previous, current);

            bestDistance = MathF.Min(bestDistance, distance);
            previous = current;
        }

        return bestDistance;
    }

    /// <summary>
    /// Projects onto the segment and clamps to its endpoints.
    /// Treats a zero-length segment as a single point.
    /// </summary>
    private static float DistanceToSegmentSquared(
        Vector2 point,
        Vector2 start,
        Vector2 end)
    {
        Vector2 direction = end - start;
        float lengthSquared = direction.LengthSquared();

        if (lengthSquared == 0f)
        {
            return Vector2.DistanceSquared(point, start);
        }

        float t = Vector2.Dot(point - start, direction)
            / lengthSquared;

        t = Math.Clamp(t, 0f, 1f);

        Vector2 closestPoint = start + t * direction;

        return Vector2.DistanceSquared(point, closestPoint);
    }
}
