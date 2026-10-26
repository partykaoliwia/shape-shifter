using ImGuiNET;
using ShapeShifter.Controllers;

namespace ShapeShifter.Views
{
    public partial class EditorPanel
    {
        private static void DrawBezierEditor(EditorController controller)
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

            bool enabled = edge.Bezier != null;

            if (ImGui.Checkbox("Cubic Bezier", ref enabled))
            {
                controller.SetSelectedEdgeBezier(enabled);
            }

            ImGui.Text("Drag the square handles to shape the curve.");
            ImGui.TextWrapped(
                "Changing to Bezier removes length and perpendicular "
                + "constraints involving this edge.");
        }
    }
}