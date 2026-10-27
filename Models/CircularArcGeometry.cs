using System.Numerics;

namespace ShapeShifter.Models;

/// <summary>
/// A calculated circle and signed angular sweep, with angles expressed in radians.
/// Separate from the editable segment so candidate geometry can be checked without applying it.
/// </summary>
public readonly record struct CircularArcGeometry(
    Vector2 Center,
    float Radius,
    float StartAngle,
    float SweepAngle)
{
    private const float MinLength = 0.0001f;
    private const float MinRelativeSeparation = 0.001f;

    /// <summary>
    /// Evaluates the arc, with t = 0 at the start and t = 1 at the end.
    /// </summary>
    public Vector2 PointAt(float t)
    {
        float angle = StartAngle + SweepAngle * t;

        return Center + Radius * new Vector2(
            MathF.Cos(angle),
            MathF.Sin(angle));
    }

    /// <summary>
    /// Builds a semicircle for G0/G0 or derives a circle from the tangent at the G1 endpoint.
    /// Rejects unsupported continuity and tangent configurations that cannot form a finite arc.
    /// </summary>
    /// <returns>True if a circle and sweep could be calculated.</returns>
    public static bool TryCreate(
        Vector2 start,
        Vector2 end,
        Vector2 tangent,
        bool startG1,
        bool endG1,
        bool clockwise,
        out CircularArcGeometry geometry)
    {
        geometry = default;

        if (!IsFinite(start) || !IsFinite(end) || (startG1 && endG1))
        {
            return false;
        }

        Vector2 chord = end - start;
        float length = chord.Length();

        if (!float.IsFinite(length))
        {
            return false;
        }

        Vector2 center;
        float radius;
        float sweepAngle;

        if (!startG1 && !endG1)
        {
            center = (start + end) / 2f;
            radius = length / 2f;
            sweepAngle = clockwise ? MathF.PI : -MathF.PI;
        }
        else
        {
            float tangentLengthSquared = tangent.LengthSquared();

            if (length <= MinLength
                || !float.IsFinite(tangentLengthSquared)
                || tangentLengthSquared <= MinLength * MinLength)
            {
                return false;
            }

            Vector2 direction = Vector2.Normalize(tangent);
            Vector2 normal = new(-direction.Y, direction.X);

            Vector2 join = startG1 ? start : end;
            Vector2 other = startG1 ? end : start;
            Vector2 joinChord = other - join;

            float denominator = Vector2.Dot(joinChord, normal);

            // A tangent parallel to the chord would require an infinite radius.
            if (MathF.Abs(denominator)
                <= MinRelativeSeparation * length)
            {
                return false;
            }

            // The center lies on the tangent's normal, equally far from both endpoints.
            float offset = joinChord.LengthSquared()
                / (2f * denominator);

            center = join + offset * normal;
            radius = MathF.Abs(offset);

            Vector2 radial = join - center;
            Vector2 positiveTangent = new(-radial.Y, radial.X);

            bool positiveSweep =
                Vector2.Dot(positiveTangent, direction) > 0f;

            Vector2 first = start - center;
            Vector2 second = end - center;

            float cross = first.X * second.Y
                - first.Y * second.X;

            sweepAngle = MathF.Atan2(
                cross,
                Vector2.Dot(first, second));

            // Choose the signed sweep that follows the requested tangent direction.
            if (positiveSweep && sweepAngle <= 0f)
            {
                sweepAngle += 2f * MathF.PI;
            }
            else if (!positiveSweep && sweepAngle >= 0f)
            {
                sweepAngle -= 2f * MathF.PI;
            }
        }

        float startAngle = MathF.Atan2(
            start.Y - center.Y,
            start.X - center.X);

        if (!IsFinite(center)
            || !float.IsFinite(radius)
            || !float.IsFinite(startAngle)
            || !float.IsFinite(sweepAngle))
        {
            return false;
        }

        geometry = new CircularArcGeometry(
            center, radius, startAngle, sweepAngle);

        return true;
    }

    private static bool IsFinite(Vector2 value)
    {
        return float.IsFinite(value.X)
            && float.IsFinite(value.Y);
    }
}
