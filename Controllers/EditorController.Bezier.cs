using System.Numerics;
using ShapeShifter.Models;

namespace ShapeShifter.Controllers
{
    public partial class EditorController
    {
        private BezierControlPoint? draggedControlPoint;

        public BezierControlPoint? HoveredControlPoint => hoveredElement?.ControlPoint;

        public void SetSelectedEdgeBezier(bool enabled)
        {
            var edge = SelectedEdge;

            if (IsDragging || edge == null)
            {
                StatusMessage = "Select exactly one edge and finish dragging.";

                return;
            }

            if (enabled)
            {
                int removedConstraintCount = geometryEditor.ConvertEdgeToBezier(edge);

                StatusMessage = removedConstraintCount > 0
                    ? "Bezier enabled. Straight-edge constraints removed."
                    : "Bezier enabled.";
            }
            else
            {
                geometryEditor.ConvertEdgeToLine(edge);
                StatusMessage = "Line segment enabled.";
            }

            UpdateHover(previousMousePosition);
        }

    
    }
}