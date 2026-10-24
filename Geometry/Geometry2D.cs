using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace ShapeShifter.Geometry
{
    public static class Geometry2D
    {
        // odleglosc punktu od odcinka
        public static float DistanceToSegmentSquared(Vector2 point, Vector2 start, Vector2 end)
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