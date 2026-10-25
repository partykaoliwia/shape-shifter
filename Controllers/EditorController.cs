using System.Numerics;
using ShapeShifter.Geometry;
using ShapeShifter.Models;

namespace ShapeShifter.Controllers
{
    public class EditorController
    {
        private const float VertexHitRadius = 8f;
        private const float EdgeHitRadius = 6f;
        private readonly Scene scene;

        private Vertex? draggedVertex;
        private Polygon? draggedPolygon;
        private Vector2 previousMousePosition;

        public Vertex? HoveredVertex { get; private set; }
        public Edge? HoveredEdge { get; private set; }
        public Polygon? HoveredPolygon { get; private set; }
        public string? StatusMessage { get; private set; }

        private readonly List<Edge> selectedEdges = new();
        public IReadOnlyList<Edge> SelectedEdges => selectedEdges;

        public bool IsDragging => draggedVertex != null || draggedPolygon != null;
        public EditorController(Scene scene)
        {
            this.scene = scene;
        }

        public void UpdatePointer(Vector2 mousePosition, bool leftPressed, bool leftDown, bool pointerAvailable, bool addAnother)
        {
            Vector2 offset = mousePosition - previousMousePosition;
            previousMousePosition = mousePosition;

            if (!leftDown)
            {
                StopDragging();
            }

            if (draggedVertex != null)
            {
                draggedVertex.ChangePosition(mousePosition);
                return;
            }

            if (draggedPolygon != null)
            {
                draggedPolygon.MoveBy(offset);
                return;
            }

            if(!pointerAvailable)
            {
                ClearHover();
                return;
            }
            UpdateHover(mousePosition);
            if (!leftPressed)
            {
                return;
            }
            StatusMessage = null;
            if (HoveredVertex != null)
            {
                if (addAnother)
                {
                    return;
                }

                draggedVertex = HoveredVertex;
                draggedVertex.ChangePosition(mousePosition);
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
        public void InsertVertexAtHoveredEdge()
        {
            if (IsDragging || HoveredPolygon == null || HoveredEdge == null)
            {
                return;
            }
            if (HoveredPolygon.SplitEdge(HoveredEdge))
            {
                RemoveInvalidSelections();
                StatusMessage = "Vertex added.";
                UpdateHover(previousMousePosition);
            }
        }
        public void DeleteHoveredVertex()
        {
            if (IsDragging || HoveredPolygon == null || HoveredVertex == null)
            {
                return;
            }
            if (HoveredPolygon.TryRemoveVertex(HoveredVertex))
            {
                RemoveInvalidSelections();
                StatusMessage = "Vertex deleted.";
                UpdateHover(previousMousePosition);
            }
            else
            {
                StatusMessage = "A polygon must have at least 3 vertices.";
            }
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
        }
        private void ClearHover()
        {
            HoveredVertex = null;
            HoveredEdge = null;
            HoveredPolygon = null;
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
                    float distanceSquared = Geometry2D.DistanceToSegmentSquared(position, edge.Start.Position,edge.End.Position);
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