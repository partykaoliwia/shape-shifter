using System.Numerics;
using ShapeShifter.Geometry;
using ShapeShifter.Models;

namespace ShapeShifter.Editing.Interaction
{
    /// <summary>
    /// Finds scene elements near the cursor using separate point and edge hit radii.
    /// </summary>
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

        /// <summary>
        /// Prefers vertices, then handles, then edges unless handle priority is requested.
        /// Handle priority makes a handle selectable when it overlaps a vertex.
        /// </summary>
        /// <returns>The nearest hit in the first matching category, or null for empty space.</returns>
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

        /// <summary>
        /// Finds the nearest edge within the hit radius, following sampled curves rather than chords.
        /// </summary>
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
