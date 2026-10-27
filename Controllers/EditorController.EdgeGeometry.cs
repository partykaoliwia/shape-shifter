using ShapeShifter.Models;

namespace ShapeShifter.Controllers;

public partial class EditorController
{
    /// <summary>
    /// Changes the single selected edge's type after checking arc continuity requirements.
    /// Reports removed constraints and refreshes hover after an accepted change.
    /// </summary>
    public void SetSelectedEdgeGeometry(EdgeGeometryType geometryType)
    {
        var edge = SelectedEdge;

        if (IsDragging || edge == null)
        {
            StatusMessage = "Select exactly one edge and finish dragging.";
            return;
        }

        if (geometryType == EdgeGeometryType.CircularArc && !CircularArcSegment.SupportsContinuity(edge.Start, edge.End))
        {
            StatusMessage = "An arc requires G0/G1 continuity, with G1 at only one endpoint.";
            return;
        }

        if (!geometryEditor.TrySetEdgeGeometry(edge, geometryType, out int removedConstraintCount))
        {
            StatusMessage = "Cannot change the edge geometry while preserving constraints.";
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
    public void SetSelectedEdgeThickness(int thickness)
    {
        if (IsDragging || SelectedEdge is not { } edge || edge.Bezier != null) return;
        edge.Thickness = Math.Clamp(thickness, 1, 10);
    }
}
