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
                "ShapeShifter edits polygons made of line segments, "
                + "cubic Bezier curves and circular arcs.");

            ImGui.Separator();

            DrawHelpSection("Controls", """
                LMB: drag a vertex, a Bezier handle or a polygon by its edge.
                Click an edge to select it.
                Ctrl + LMB: toggle edge selection, up to two edges.
                Shift + LMB: prefer a handle when it overlaps a vertex.
                Click empty space without Ctrl to clear selection.
                Insert over an edge: add a midpoint vertex.
                Delete over a vertex: remove it.
                Click a vertex or handle to edit its vertex's continuity.
                Release LMB before changing settings in the panel.
                """, ImGuiTreeNodeFlags.DefaultOpen);

            DrawHelpSection("Geometric constraints", """
                Length: select one line segment, enter Target length,
                then click Set length. The initial value is its current length.
                Use Remove length to remove the constraint.

                Perpendicularity: select two line segments with Ctrl + LMB,
                then click Make perpendicular. The edges may belong
                to different polygons. Use Remove perpendicular to remove it.

                An edge may have a length constraint and several
                perpendicularity constraints. L labels show target lengths;
                matching P numbers identify perpendicular edge pairs.
                """);

            DrawHelpSection("Curves and continuity", """
                Select an edge and choose its type in Edge geometry.
                Cubic Bezier displays a dashed control polygon and two handles.

                Select a vertex to choose its continuity:
                G0: shared position.
                G1: matching tangent directions.
                C1: matching tangent vectors, including their magnitude.
                Bezier joins default to C1 unless a value was already chosen.

                To create a circular arc:
                1. Select each endpoint and set Vertex continuity to G0.
                2. Select the edge and choose Circular arc.
                3. For tangency, set one endpoint to G1, leaving the other G0.
                G0/G0 creates a semicircle with radius half the chord length.
                Arcs do not support C1 or G1 at both endpoints.
                """);

            DrawHelpSection("Editing rules", """
                Polygons may intersect themselves and need at least 3 vertices.
                New polygons are defined in DemoSceneFactory.
                Length and perpendicularity apply to line segments.

                A successful edge type change removes length and
                perpendicularity constraints involving that edge.
                Splitting or deleting removes constraints on replaced edges.
                Splitting an arc creates two lines.
                Deleting a vertex joins its neighbors with a line.

                Related elements may move to preserve constraints.
                Commands are rejected if the solver cannot preserve
                the remaining relations; the previous state is restored.
                """);

            DrawHelpSection("Data model", """
                Scene stores polygons and explicit geometric constraints.
                Each polygon is a closed sequence of edges sharing vertices.
                An edge is a line or contains a Bezier or circular arc segment.
                Vertices store position and continuity.
                Bezier handles store offsets from their endpoint vertices.

                Controllers interpret input; views draw the scene and panels.
                GeometryEditor performs edits; ConstraintSolver resolves relations.
                """);

            DrawHelpSection("Algorithms", """
                Lines: Raylib or a custom Bresenham implementation,
                selected with radio buttons in Line drawing algorithm.
                Bezier curves: power basis followed by forward differences.
                Bezier splitting: de Casteljau at t = 0.5.
                Arcs: parametric circle sampling.
                Hit testing: point distances and distances to sampled segments.

                Constraints use iterative projections on a working snapshot,
                with up to 512 iterations. Dragged elements stay at the cursor.
                Handle dragging first tries to keep both curve endpoints fixed,
                then retries with movable endpoints if needed.
                If local dragging still fails, the connected group is translated.
                """);
        }

        ImGui.End();
    }

    private static void DrawHelpSection(
        string title,
        string text,
        ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.None)
    {
        if (ImGui.CollapsingHeader(title, flags))
        {
            ImGui.TextWrapped(text);
        }
    }
}