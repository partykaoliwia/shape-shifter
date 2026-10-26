using System.Numerics;
using ShapeShifter.Models;

namespace ShapeShifter.Geometry
{
    public static class CubicBezierPointGenerator
    {
        private const float TargetSampleSpacing = 4f;
        private const int MinSteps = 16;
        private const int MaxSteps = 512;
        public static IEnumerable<Vector2> GeneratePoints(
            CubicBezierSegment segment)
        {
            Vector2 v0 = segment.Start.Position;
            Vector2 v1 = segment.Control1.Position;
            Vector2 v2 = segment.Control2.Position;
            Vector2 v3 = segment.End.Position;

            float controlPolygonLength =
                Vector2.Distance(v0, v1)
                + Vector2.Distance(v1, v2)
                + Vector2.Distance(v2, v3);
            int steps = Math.Clamp(
                (int)MathF.Ceiling(controlPolygonLength / TargetSampleSpacing),MinSteps,MaxSteps);

            Vector2 A0 = v0;
            Vector2 A1 = 3f * (v1 - v0);
            Vector2 A2 = 3f * (v2 - 2f * v1 + v0);
            Vector2 A3 = v3 - 3f * v2 + 3f * v1 - v0;

            float d = 1f / steps;
            float d2 = d * d;
            float d3 = d2 * d;

            Vector2 point = A0;
            Vector2 deltaP = A3 * d3 + A2 * d2 + A1 * d;
            Vector2 delta2P = 6f * A3 * d3 + 2f * A2 * d2;
            Vector2 delta3P = 6f * A3 * d3;

            yield return v0;

            for (int i = 1; i < steps; i++)
            {
                point += deltaP;
                deltaP += delta2P;
                delta2P += delta3P;

                yield return point;
            }

            // Avoid an accumulated rounding error at the endpoint.
            yield return v3;
        }
    }
}