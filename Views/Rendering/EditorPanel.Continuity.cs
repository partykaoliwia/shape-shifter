using ImGuiNET;
using ShapeShifter.Controllers;
using ShapeShifter.Models;

namespace ShapeShifter.Views;

public partial class EditorPanel
{
    private static void DrawContinuityEditor(EditorController controller)
    {
        ImGui.Separator();
        ImGui.Text("Vertex continuity");

        var vertex = controller.SelectedVertex;

        if (vertex == null)
        {
            ImGui.TextWrapped(
                "Click a vertex or a Bezier handle to select a vertex.");
            return;
        }

        if (controller.IsDragging)
        {
            ImGui.Text("Finish dragging to change continuity.");
            return;
        }

        if (!controller.SelectedVertexHasBezier)
        {
            ImGui.TextWrapped(
                "This setting will apply when an adjacent edge "
                + "becomes a Bezier segment.");
        }

        if (ImGui.RadioButton(
            "G0 - position",
            vertex.Continuity == ContinuityType.G0))
        {
            controller.SetSelectedVertexContinuity(ContinuityType.G0);
        }

        if (ImGui.RadioButton(
            "G1 - tangent direction",
            vertex.Continuity == ContinuityType.G1))
        {
            controller.SetSelectedVertexContinuity(ContinuityType.G1);
        }

        if (ImGui.RadioButton(
            "C1 - tangent vector",
            vertex.Continuity == ContinuityType.C1))
        {
            controller.SetSelectedVertexContinuity(ContinuityType.C1);
        }
    }
}