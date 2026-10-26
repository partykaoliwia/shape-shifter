using System.Numerics;
using ShapeShifter.Models;

namespace ShapeShifter.Controllers
{
    public partial class EditorController
    {
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
                if (!geometryEditor.TryConvertEdgeToBezier(
                    edge, out int removedConstraintCount))
                {
                    StatusMessage =
                        "Cannot enable Bezier while preserving continuity.";
                    return;
                }

                StatusMessage = removedConstraintCount > 0
                    ? "Bezier enabled. Straight-edge constraints removed."
                    : "Bezier enabled.";
            }
            else
            {
                if (!geometryEditor.TryConvertEdgeToLine(edge))
                {
                    StatusMessage =
                        "Cannot enable a line segment while preserving continuity.";
                    return;
                }

                StatusMessage = "Line segment enabled.";
            }

            UpdateHover(previousMousePosition);
        }
    }
}