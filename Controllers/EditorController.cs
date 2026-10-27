using System.Numerics;
using ShapeShifter.Editing;
using ShapeShifter.Models;
using ShapeShifter.Editing.Interaction;

namespace ShapeShifter.Controllers
{
    /// <summary>
    /// Tracks selection, hover and dragging, and delegates geometry changes to the editor.
    /// Supplies rendering state and feedback messages without drawing the interface.
    /// </summary>
    public partial class EditorController
    {
        private readonly Scene scene;
        private readonly GeometryEditor geometryEditor;
        private readonly SceneHitTester hitTester;

        private Vertex? draggedVertex;
        private Polygon? draggedPolygon;
        private Vector2 previousMousePosition;
        private BezierControlPoint? draggedControlPoint;

        private HitTestResult? hoveredElement;
        public Vertex? HoveredVertex => hoveredElement?.Vertex;
        public Edge? HoveredEdge => hoveredElement?.Edge;
        public Polygon? HoveredPolygon => hoveredElement?.Polygon;
        public BezierControlPoint? HoveredControlPoint => hoveredElement?.ControlPoint;
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
        /// <summary>
        /// Creates rendering state with a copy of the selected-edge list.
        /// </summary>
        public EditorViewState GetViewState()
        {
            return new EditorViewState(
                hoveredElement,
                selectedEdges.ToArray(),
                SelectedVertex);
        }

        /// <summary>
        /// Continues an active drag or handles hover and selection for a new mouse update.
        /// Captured drags keep following the cursor until the left button is released.
        /// </summary>
        public void UpdateMouse(MouseInput input)
        {
            Vector2 offset = input.Position - previousMousePosition;
            previousMousePosition = input.Position;

            if (!input.LeftDown)
            {
                StopDragging();
            }
            // Finish active drags even when the cursor moves over an interface panel.
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

            if(!input.CanInteractWithScene)
            {
                ClearHover();
                return;
            }
            UpdateHover(input.Position, input.PreferControlPoints);
            if (!input.LeftPressed)
            {
                return;
            }
            StatusMessage = null;
            if (HoveredControlPoint != null)
            {
                if (!input.ExtendSelection)
                {
                    SelectedVertex = HoveredControlPoint.Anchor;
                    selectedEdges.Clear();
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
                SelectedVertex = HoveredVertex;
                selectedEdges.Clear();
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
                SelectedVertex = null;
            }
        }
        
        /// <summary>
        /// Splits the hovered edge when no drag is active and refreshes stale selection and hover state.
        /// Reports when the edit cannot preserve the remaining relations.
        /// </summary>
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
            else
            {
                StatusMessage = "Cannot split this edge while preserving constraints.";
            }
        }
        /// <summary>
        /// Removes the hovered vertex when no drag is active, keeping at least three vertices.
        /// Refreshes selection on success or reports why the edit was rejected.
        /// </summary>
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
                StatusMessage = HoveredPolygon.Edges.Count <= 3
                    ? "A polygon must have at least 3 vertices."
                    : "Cannot delete this vertex while preserving constraints.";
            }
        }
        
        /// <summary>
        /// Replaces edge selection, or toggles one edge when extending the selection.
        /// Allows at most two selected edges and clears the selected vertex.
        /// </summary>
        public void SelectEdge(Edge edge, bool extendSelection)
        {
            SelectedVertex = null;
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
        private void UpdateHover(Vector2 position,bool preferControlPoints = false)
        {
             hoveredElement = hitTester.HitTest(position,preferControlPoints);
        }

        /// <summary>
        /// Clears references to edges and vertices removed by an accepted topology change.
        /// </summary>
        private void RemoveInvalidSelections()
        {
            selectedEdges.RemoveAll(edge => !scene.ContainsEdge(edge));
            if (SelectedVertex != null && !scene.Polygons
                    .SelectMany(polygon => polygon.Edges)
                    .Any(edge => edge.Start == SelectedVertex))
            {
                SelectedVertex = null;
            }
        }
    }
}
