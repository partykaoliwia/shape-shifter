using System.Numerics;
using ShapeShifter.Models;

namespace ShapeShifter.Controllers
{
    public partial class EditorController
    {
        private BezierControlPoint? draggedControlPoint;

        public BezierControlPoint? HoveredControlPoint { get; private set; }

        public void SetSelectedEdgeBezier(bool enabled)
        {
            var edge = SelectedEdge;

            if (IsDragging || edge is null)
            {
                StatusMessage =
                    "Select exactly one edge and finish dragging.";

                return;
            }

            if (enabled)
            {
                var affectedConstraints = scene.Constraints
                    .Where(constraint => constraint.AffectedEdges.Contains(edge))
                    .ToArray();

                edge.EnableBezier();

                foreach (var constraint in affectedConstraints)
                {
                    scene.RemoveConstraint(constraint);
                }

                StatusMessage = affectedConstraints.Length > 0
                    ? "Bezier enabled. Straight-edge constraints removed."
                    : "Bezier enabled.";
            }
            else
            {
                edge.DisableBezier();
                StatusMessage = "Line segment enabled.";
            }

            UpdateHover(previousMousePosition);
            UpdateBezierHover(previousMousePosition);
        }

        private void UpdateBezierHover(Vector2 position)
        {
            HoveredControlPoint = null;

            // Polygon vertices take precedence over control points.
            if (HoveredVertex is not null)
            {
                return;
            }

            float bestDistanceSquared =
                VertexHitRadius * VertexHitRadius;

            foreach (var polygon in scene.Polygons)
            {
                foreach (var edge in polygon.Edges)
                {
                    if (edge.Bezier is not { } segment)
                    {
                        continue;
                    }

                    foreach (var controlPoint in new[]
                    {
                        segment.Control1,
                        segment.Control2
                    })
                    {
                        float distanceSquared = Vector2.DistanceSquared(
                            position, controlPoint.Position);

                        if (distanceSquared <= bestDistanceSquared)
                        {
                            bestDistanceSquared = distanceSquared;
                            HoveredControlPoint = controlPoint;
                            HoveredPolygon = polygon;
                            HoveredEdge = null;
                        }
                    }
                }
            }
        }
    }
}