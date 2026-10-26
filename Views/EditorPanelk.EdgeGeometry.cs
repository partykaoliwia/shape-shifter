using ImGuiNET;
using ShapeShifter.Controllers;
using ShapeShifter.Models;

namespace ShapeShifter.Views;

public partial class EditorPanel
{
    private static void DrawEdgeGeometryEditor(EditorController controller)
    {
        ImGui.Separator();
        ImGui.Text("Edge geometry");

        var edge = controller.SelectedEdge;

        if (edge == null)
        {
            ImGui.Text("Select exactly one edge.");
            return;
        }

        if (controller.IsDragging)
        {
            ImGui.Text("Finish dragging to change the edge type.");
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
            ImGui.Text("Drag the square handles to shape the curve.");
        }

        if (edge.Arc is { } arc)
        {
            ImGui.Text($"Radius: {arc.Radius:F2}");
        }

        ImGui.TextWrapped(
            "Circular arcs require G0 continuity at both endpoints.");

        ImGui.TextWrapped(
            "Changing geometry removes constraints involving this edge.");
    }
}