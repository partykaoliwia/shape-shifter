using System.Numerics;
using ShapeShifter.Geometry;
using ShapeShifter.Models;

namespace ShapeShifter.Editing.Interaction
{
    public sealed class SceneHitTester
    {
        private const float VertexHitRadius = 8f;
        private const float ControlPointHitRadius = 8f;
        private const float EdgeHitRadius = 6f;

        private readonly Scene scene;

        public SceneHitTester(Scene scene)
        {
            this.scene = scene;
        }

        public HitTestResult? HitTest(Vector2 position,bool preferControlPoints = false)
        {
            if (preferControlPoints)
            {
                return FindControlPoint(position)
                    ?? FindVertex(position)
                    ?? FindEdge(position);
            }

            return FindVertex(position)
                ?? FindControlPoint(position)
                ?? FindEdge(position);
        }

        private HitTestResult? FindVertex(Vector2 position)
        {
            HitTestResult? result = null;
            float bestDistanceSquared = VertexHitRadius * VertexHitRadius;

            foreach (var polygon in scene.Polygons)
            {
                foreach (var edge in polygon.Edges)
                {
                    float distanceSquared = Vector2.DistanceSquared(position, edge.Start.Position);

                    if (distanceSquared <= bestDistanceSquared)
                    {
                        bestDistanceSquared = distanceSquared;
                        result = new HitTestResult(polygon, edge.Start);
                    }
                }
            }

            return result;
        }

        private HitTestResult? FindControlPoint(Vector2 position)
        {
            HitTestResult? result = null;
            float bestDistanceSquared = ControlPointHitRadius * ControlPointHitRadius;

            foreach (var polygon in scene.Polygons)
            {
                foreach (var edge in polygon.Edges)
                {
                    var segment = edge.Bezier;
                    if (segment ==null)
                    {
                        continue;
                    }

                    foreach (var controlPoint in new[] {segment.Control1,segment.Control2})
                    {
                        float distanceSquared = Vector2.DistanceSquared(position,controlPoint.Position);

                        if (distanceSquared <= bestDistanceSquared)
                        {
                            bestDistanceSquared = distanceSquared;
                            result = new HitTestResult(polygon,controlPoint);
                        }
                    }
                }
            }

            return result;
        }

        private HitTestResult? FindEdge(Vector2 position)
        {
            HitTestResult? result = null;
            float bestDistanceSquared =EdgeHitRadius * EdgeHitRadius;

            foreach (var polygon in scene.Polygons)
            {
                foreach (var edge in polygon.Edges)
                {
                    float distanceSquared = EdgeGeometry.DistanceToEdgeSquared(edge, position);

                    if (distanceSquared <= bestDistanceSquared)
                    {
                        bestDistanceSquared = distanceSquared;
                        result = new HitTestResult(polygon, edge);
                    }
                }
            }

            return result;
        }
    }
}