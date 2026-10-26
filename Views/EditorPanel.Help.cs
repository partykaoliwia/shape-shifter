using ImGuiNET;

namespace ShapeShifter.Views;

public partial class EditorPanel
{
    private bool showHelp;

    private void DrawHelpButton()
    {
        if (ImGui.Button("Help"))
        {
            showHelp = true;
        }
    }

    private void DrawHelpWindow()
    {
        if (!showHelp)
        {
            return;
        }

        if (ImGui.Begin("Help", ref showHelp))
        {
            ImGui.TextWrapped(
                """
                CONTROLS
                LMB: drag a vertex, handle or polygon by its edge.
                Ctrl + LMB: select up to two edges.
                Click empty space: clear selection.
                Insert over an edge: add a midpoint vertex.
                Delete over a vertex: remove it.
                Use the editing panel to change geometry and constraints.

                RULES
                Polygons may intersect themselves and need at least 3 vertices.
                Length and perpendicularity apply to line segments.
                Changing geometry removes the edge's constraints.
                Bezier joins support G0/G1/C1; the default is C1.
                Arcs support G0/G1, with G1 at only one endpoint.
                G0/G0 arcs are semicircles.
                Splitting an arc creates two lines.
                Removing a vertex joins its neighbors with a line.

                MODEL
                Scene contains polygons and constraints.
                Polygons contain edges sharing vertex objects.
                Edges are lines, Bezier segments or circular arcs.
                Controllers handle input; views display the editor.
                GeometryEditor and ConstraintSolver update geometry.

                ALGORITHMS
                Lines: Raylib or Bresenham.
                Bezier: power basis and forward differences.
                Bezier splitting: de Casteljau.
                Arcs: parametric circle sampling.
                Hit testing: distance to sampled line segments.
                Constraints: iterative projections on a working snapshot.
                Mouse targets remain fixed during solving.
                If local dragging fails, the connected group is translated.
                Invalid commands are rejected.
                """
            );
        }

        ImGui.End();
    }
}