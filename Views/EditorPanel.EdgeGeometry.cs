using ImGuiNET;
using ShapeShifter.Controllers;
using ShapeShifter.Models;

namespace ShapeShifter.Views;

public partial class EditorPanel
{
    private static void DrawEdgeGeometryEditor(EditorController controller)
    {
        DrawSectionTitle("Edge geometry");

        var edge = controller.SelectedEdge;

        if (edge == null)
        {
            ImGui.Text("Select exactly one edge.");
            return;
        }

        if (controller.IsDragging)
        {
            ImGui.TextWrapped("Finish dragging to change the edge type.");
            return;
        }

        if (ImGui.RadioButton(
            "Line segment",
            edge.GeometryType == EdgeGeometryType.Line))
        {
            controller.SetSelectedEdgeGeometry(EdgeGeometryType.Line);
        }

        if (ImGui.RadioButton(
            "Cubic Bezier",
            edge.GeometryType == EdgeGeometryType.Bezier))
        {
            controller.SetSelectedEdgeGeometry(EdgeGeometryType.Bezier);
        }

        if (ImGui.RadioButton(
            "Circular arc",
            edge.GeometryType == EdgeGeometryType.CircularArc))
        {
            controller.SetSelectedEdgeGeometry(EdgeGeometryType.CircularArc);
        }

        if (edge.Bezier != null)
        {
            ImGui.TextWrapped("Drag the square handles to shape the curve.");
        }

        if (edge.Arc is { } arc)
        {
            ImGui.Text($"Radius: {arc.Radius:F2}");
        }

        ImGui.TextWrapped(
            "Circular arcs support G0/G1, with G1 at only one endpoint.");

        ImGui.TextWrapped(
            "Changing geometry removes constraints involving this edge.");
    }
}
