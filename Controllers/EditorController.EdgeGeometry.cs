using ShapeShifter.Models;

namespace ShapeShifter.Controllers;

public partial class EditorController
{
    public void SetSelectedEdgeGeometry(EdgeGeometryType geometryType)
    {
        var edge = SelectedEdge;

        if (IsDragging || edge == null)
        {
            StatusMessage = "Select exactly one edge and finish dragging.";
            return;
        }

        if (geometryType == EdgeGeometryType.CircularArc
            && (edge.Start.Continuity != ContinuityType.G0
                || edge.End.Continuity != ContinuityType.G0))
        {
            StatusMessage =
                "Set G0 continuity at both endpoints before enabling an arc.";
            return;
        }

        if (!geometryEditor.TrySetEdgeGeometry(
            edge,
            geometryType,
            out int removedConstraintCount))
        {
            StatusMessage =
                "Cannot change the edge geometry while preserving constraints.";
            return;
        }

        StatusMessage = geometryType switch
        {
            EdgeGeometryType.Line => "Line segment enabled.",
            EdgeGeometryType.Bezier => "Bezier segment enabled.",
            EdgeGeometryType.CircularArc => "Circular arc enabled.",
            _ => null
        };

        if (removedConstraintCount > 0)
        {
            StatusMessage += " Constraints involving this edge were removed.";
        }

        UpdateHover(previousMousePosition);
    }
}