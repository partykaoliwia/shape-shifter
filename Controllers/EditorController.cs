using System.Numerics;
using ShapeShifter.Editing;
using ShapeShifter.Models;
using ShapeShifter.Editing.Interaction;

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
        public EditorViewState GetViewState()
        {
            return new EditorViewState(hoveredElement,selectedEdges.ToArray());
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

            if(!input.CanInteractWithScene)
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