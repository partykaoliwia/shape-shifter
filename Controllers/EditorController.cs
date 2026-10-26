using System.Numerics;
using ShapeShifter.Editing;
using ShapeShifter.Models;
using ShapeShifter.Models.Constraints;

namespace ShapeShifter.Controllers
{
    public partial class EditorController
    {
        private readonly Scene scene;
        private readonly GeometryEditor geometryEditor;
        private readonly SceneHitTester hitTester;

        private Vertex? draggedVertex;
        private Polygon? draggedPolygon;
        private Vector2 previousMousePosition;

        private HitTestResult? hoveredElement;
        public Vertex? HoveredVertex => hoveredElement?.Vertex;
        public Edge? HoveredEdge => hoveredElement?.Edge;
        public Polygon? HoveredPolygon => hoveredElement?.Polygon;
        public string? StatusMessage { get; private set; }

        private readonly List<Edge> selectedEdges = new();
        public IReadOnlyList<Edge> SelectedEdges => selectedEdges;
        public Edge? SelectedEdge => selectedEdges.Count == 1 ? selectedEdges[0] : null;

        public bool IsDragging => draggedVertex != null || draggedPolygon != null || draggedControlPoint != null;
        public EditorController(Scene scene)
        {
            this.scene = scene;
            geometryEditor = new GeometryEditor(scene);
            hitTester = new SceneHitTester(scene);
        }
        public EditorState GetState()
        {
            return new EditorState(hoveredElement,selectedEdges.ToArray());
        }

        public void UpdateMouse(MouseInput input)
        {
            Vector2 offset = input.Position - previousMousePosition;
            previousMousePosition = input.Position;

            if (!input.LeftDown)
            {
                StopDragging();
            }
            if (draggedControlPoint != null)
            {
                geometryEditor.MoveControlPointTo(draggedControlPoint,input.Position);
                return;
            }

            if (draggedVertex != null)
            {
                geometryEditor.MoveVertexTo(draggedVertex, input.Position);
                return;
            }

            if (draggedPolygon != null)
            {
                geometryEditor.MovePolygonBy(draggedPolygon, offset);
                return;
            }

            if(!input.MouseAvailable)
            {
                ClearHover();
                return;
            }
            UpdateHover(input.Position);
            if (!input.LeftPressed)
            {
                return;
            }
            StatusMessage = null;
            if (HoveredControlPoint != null)
            {
                if (!input.ExtendSelection)
                {
                    draggedControlPoint = HoveredControlPoint;
                    geometryEditor.MoveControlPointTo(draggedControlPoint,input.Position);
                }

                return;
            }
            if (HoveredVertex != null)
            {
                if (input.ExtendSelection)
                {
                    return;
                }

                draggedVertex = HoveredVertex;
                geometryEditor.MoveVertexTo(draggedVertex, input.Position);
            }
            else if (HoveredEdge != null)
            {
                SelectEdge(HoveredEdge, input.ExtendSelection);

                if (!input.ExtendSelection)
                {
                    draggedPolygon = HoveredPolygon;
                }
            }
            else if (!input.ExtendSelection)
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
        public void SelectEdge(Edge edge, bool extendSelection)
        {
            StatusMessage = null;

            if (!extendSelection)
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
            hoveredElement = null;
        }
        private void UpdateHover(Vector2 position)
        {
             hoveredElement = hitTester.HitTest(position);
        }

        private void RemoveInvalidSelections()
        {
            selectedEdges.RemoveAll(edge => !scene.ContainsEdge(edge));
        }
    }
}