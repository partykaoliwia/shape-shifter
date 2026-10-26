using System.Numerics;
using ShapeShifter.Editing;
using ShapeShifter.Geometry;
using ShapeShifter.Models;
using ShapeShifter.Models.Constraints;

namespace ShapeShifter.Controllers
{
    public partial class EditorController
    {
        private const float VertexHitRadius = 8f;
        private const float EdgeHitRadius = 6f;
        private readonly Scene scene;
        private readonly GeometryEditor geometryEditor;

        private Vertex? draggedVertex;
        private Polygon? draggedPolygon;
        private Vector2 previousMousePosition;

        public Vertex? HoveredVertex { get; private set; }
        public Edge? HoveredEdge { get; private set; }
        public Polygon? HoveredPolygon { get; private set; }
        public string? StatusMessage { get; private set; }

        private readonly List<Edge> selectedEdges = new();
        public IReadOnlyList<Edge> SelectedEdges => selectedEdges;
        public Edge? SelectedEdge => selectedEdges.Count == 1 ? selectedEdges[0] : null;

        public bool IsDragging => draggedVertex != null || draggedPolygon != null || draggedControlPoint != null;
        public EditorController(Scene scene)
        {
            this.scene = scene;
            geometryEditor = new GeometryEditor(scene);
        }

        public void UpdatePointer(Vector2 mousePosition, bool leftPressed, bool leftDown, bool pointerAvailable, bool addAnother)
        {
            Vector2 offset = mousePosition - previousMousePosition;
            previousMousePosition = mousePosition;

            if (!leftDown)
            {
                StopDragging();
            }
            if (draggedControlPoint != null)
            {
                geometryEditor.MoveControlPointTo(draggedControlPoint,mousePosition);
                return;
            }

            if (draggedVertex != null)
            {
                geometryEditor.MoveVertexTo(draggedVertex, mousePosition);
                return;
            }

            if (draggedPolygon != null)
            {
                geometryEditor.MovePolygonBy(draggedPolygon, offset);
                return;
            }

            if(!pointerAvailable)
            {
                ClearHover();
                return;
            }
            UpdateHover(mousePosition);
            UpdateBezierHover(mousePosition);
            if (!leftPressed)
            {
                return;
            }
            StatusMessage = null;
            if (HoveredControlPoint != null)
            {
                if (!addAnother)
                {
                    draggedControlPoint = HoveredControlPoint;
                    geometryEditor.MoveControlPointTo(draggedControlPoint,mousePosition);
                }

                return;
            }
            if (HoveredVertex != null)
            {
                if (addAnother)
                {
                    return;
                }

                draggedVertex = HoveredVertex;
                geometryEditor.MoveVertexTo(draggedVertex, mousePosition);
            }
            else if (HoveredEdge != null)
            {
                SelectEdge(HoveredEdge, addAnother);

                if (!addAnother)
                {
                    draggedPolygon = HoveredPolygon;
                }
            }
            else if (!addAnother)
            {
                selectedEdges.Clear();
            }
        }
        public PerpendicularConstraint? GetSelectedPerpendicularConstraint()
        {
            if (selectedEdges.Count != 2)
            {
                return null;
            }

            return scene.GetPerpendicularConstraint(
                selectedEdges[0],
                selectedEdges[1]);
        }

        public void AddSelectedPerpendicular()
        {
            if (IsDragging || selectedEdges.Count != 2)
            {
                StatusMessage = "Select exactly two edges and finish dragging.";
                return;
            }

            if (GetSelectedPerpendicularConstraint() is not null)
            {
                StatusMessage = "These edges are already constrained.";
                return;
            }

            if (geometryEditor.TryAddPerpendicular(
                selectedEdges[0], selectedEdges[1]))
            {
                StatusMessage = "Perpendicular constraint applied.";
            }
            else
            {
                StatusMessage =
                    "Could not apply perpendicularity. "
                    + "Previous geometry and constraints were preserved.";
            }
        }

        public void RemoveSelectedPerpendicular()
        {
            if (IsDragging || selectedEdges.Count != 2)
            {
                StatusMessage = "Select exactly two edges and finish dragging.";
                return;
            }

            var constraint = GetSelectedPerpendicularConstraint();

            if (constraint is null)
            {
                StatusMessage = "These edges have no perpendicular constraint.";
                return;
            }

            geometryEditor.RemoveConstraint(constraint);
            StatusMessage = "Perpendicular constraint removed.";
        }
        public void InsertVertexAtHoveredEdge()
        {
            if (IsDragging || HoveredPolygon == null || HoveredEdge == null)
            {
                return;
            }
            if (geometryEditor.TrySplitEdge(HoveredPolygon, HoveredEdge))
            {
                RemoveInvalidSelections();
                UpdateHover(previousMousePosition);
                UpdateBezierHover(previousMousePosition);

                StatusMessage = "Vertex added.";
            }
        }
        public void DeleteHoveredVertex()
        {
            if (IsDragging || HoveredPolygon == null || HoveredVertex == null)
            {
                return;
            }
            if (geometryEditor.TryRemoveVertex(HoveredPolygon, HoveredVertex))
            {
                RemoveInvalidSelections();
                UpdateHover(previousMousePosition);
                UpdateBezierHover(previousMousePosition);

                StatusMessage = "Vertex deleted.";
            }
            else
            {
                StatusMessage = "A polygon must have at least 3 vertices.";
            }
        }
        public EdgeLengthConstraint? GetSelectedLengthConstraint()
        {
            var edge = SelectedEdge;
            return edge == null ? null : scene.GetLengthConstraint(edge);
        }
        public void SetSelectedEdgeLength(float targetLength)
        {
            var edge = SelectedEdge;

            if (IsDragging || edge == null)
            {
                StatusMessage = "Select exactly one edge and finish dragging.";
                return;
            }

            if (!float.IsFinite(targetLength) || targetLength <= 0f)
            {
                StatusMessage = "Length must be a positive, finite number.";
                return;
            }

            if (geometryEditor.TrySetLength(edge, targetLength))
            {
                StatusMessage = "Length constraint applied.";
            }
            else
            {
                StatusMessage = "Could not satisfy the requested length. " + "The previous geometry and constraints were preserved.";
            }
        }
        public void RemoveSelectedEdgeLength()
        {
            if (IsDragging || SelectedEdge == null)
            {
                StatusMessage = "Select exactly one edge and finish dragging.";
                return;
            }

            var constraint = GetSelectedLengthConstraint();

            if (constraint == null)
            {
                StatusMessage = "The selected edge has no length constraint.";
                return;
            }

            geometryEditor.RemoveConstraint(constraint);
            StatusMessage = "Length constraint removed.";
        }
        public void SelectEdge(Edge edge, bool addAnother)
        {
            StatusMessage = null;

            if (!addAnother)
            {
                selectedEdges.Clear();
                selectedEdges.Add(edge);
                return;
            }

            if (selectedEdges.Remove(edge))
            {
                return;
            }

            if (selectedEdges.Count >= 2)
            {
                StatusMessage = "You can select at most two edges.";
                return;
            }

            selectedEdges.Add(edge);
        }
        private void StopDragging()
        {
            draggedVertex = null;
            draggedPolygon = null;
            draggedControlPoint = null;
        }
        private void ClearHover()
        {
            HoveredVertex = null;
            HoveredEdge = null;
            HoveredPolygon = null;
            HoveredControlPoint = null;
        }
        private void UpdateHover(Vector2 position)
        {
            ClearHover();
            float bestDistanceSquared = VertexHitRadius * VertexHitRadius;
            foreach (var polygon in scene.Polygons)
            {
                foreach (var edge in polygon.Edges)
                {
                    float distanceSquared = Vector2.DistanceSquared(position,edge.Start.Position);
                    if (distanceSquared<=bestDistanceSquared)
                    {
                        bestDistanceSquared = distanceSquared;
                        HoveredVertex = edge.Start;
                        HoveredPolygon = polygon;
                    }
                }

            }
            if (HoveredVertex != null)
            {
                return;
            }

            bestDistanceSquared = EdgeHitRadius * EdgeHitRadius;
            foreach (var polygon in scene.Polygons)
            {
                foreach (var edge in polygon.Edges)
                {
                    float distanceSquared = EdgeGeometry.DistanceSquared(edge, position);
                    if (distanceSquared<=bestDistanceSquared)
                    {
                        bestDistanceSquared = distanceSquared;
                        HoveredEdge = edge;
                        HoveredPolygon = polygon;
                    }
                }
            }
        }

        private void RemoveInvalidSelections()
        {
            selectedEdges.RemoveAll(edge => !scene.Polygons.Any(polygon => polygon.Edges.Contains(edge)));
        }
    }
}