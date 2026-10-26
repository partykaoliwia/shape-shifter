using System.Numerics;
using ShapeShifter.Models;

namespace ShapeShifter.Geometry
{
    public static class EdgeGeometry
    {
        public static float DistanceSquared(Edge edge, Vector2 position)
        {
            var segment = edge.Bezier;

            if (segment == null)
            {
                return Geometry2D.DistanceToSegmentSquared(
                    position,
                    edge.Start.Position,
                    edge.End.Position);
            }

            float bestDistance = float.PositiveInfinity;
            Vector2 previous = segment.Start.Position;

            foreach (var point in CubicBezierPointGenerator.GeneratePoints(segment))
            {
                float distance = Geometry2D.DistanceToSegmentSquared(
                    position, previous, point);

                bestDistance = MathF.Min(bestDistance, distance);
                previous = point;
            }

            return bestDistance;
        }
    }
}