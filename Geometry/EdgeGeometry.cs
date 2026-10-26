using System.Numerics;
using ShapeShifter.Models;

namespace ShapeShifter.Geometry
{
    public static class EdgeGeometry
    {
        public static float DistanceToEdgeSquared(Edge edge, Vector2 position)
        {
            var segment = edge.Bezier;

            if (segment == null)
            {
                return DistanceToSegmentSquared(position,edge.Start.Position,edge.End.Position);
            }

            float bestDistance = float.PositiveInfinity;
            Vector2 previous = segment.Start.Position;

            foreach (var point in CubicBezierPointGenerator.GeneratePoints(segment))
            {
                float distance = DistanceToSegmentSquared(position, previous, point);

                bestDistance = MathF.Min(bestDistance, distance);
                previous = point;
            }

            return bestDistance;
        }
         private static float DistanceToSegmentSquared(Vector2 point, Vector2 start, Vector2 end)
        {
            Vector2 direction = end - start;
            float lengthSquared = direction.LengthSquared();
            if (lengthSquared==0f)
            {
                return Vector2.DistanceSquared(point,start);
            }
            float t = Vector2.Dot(point-start,direction) / lengthSquared;
            t = Math.Clamp(t,0f,1f);
            Vector2 closestPoint = start + t * direction;
            return Vector2.DistanceSquared(point,closestPoint);

        }
    }
}