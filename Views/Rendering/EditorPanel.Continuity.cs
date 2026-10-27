using ImGuiNET;
using ShapeShifter.Controllers;
using ShapeShifter.Models;

namespace ShapeShifter.Views;

public partial class EditorPanel
{
    /// <summary>
    /// Shows continuity choices for the selected vertex, including settings for future curves.
    /// Hides C1 at arc endpoints and leaves validation of G1 combinations to the editor.
    /// </summary>
    private static void DrawContinuityEditor(EditorController controller)
    {
        DrawSectionTitle("Vertex continuity");

        var vertex = controller.SelectedVertex;

        if (vertex == null)
        {
            ImGui.TextWrapped(
                "Click a vertex or a Bezier handle to select a vertex.");
            return;
        }

        if (controller.IsDragging)
        {
            ImGui.TextWrapped("Finish dragging to change continuity.");
            return;
        }

        bool hasArc = controller.SelectedVertexHasArc;

        if (hasArc)
        {
            ImGui.TextWrapped(
                "An arc supports G0/G1, with G1 at only one endpoint.");
        }
        else if (!controller.SelectedVertexHasBezier)
        {
            ImGui.TextWrapped(
                "This setting applies when an adjacent edge becomes a curve.");
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

        if (!hasArc
            && ImGui.RadioButton(
                "C1 - tangent vector",
                vertex.Continuity == ContinuityType.C1))
        {
            controller.SetSelectedVertexContinuity(ContinuityType.C1);
        }
    }
}
