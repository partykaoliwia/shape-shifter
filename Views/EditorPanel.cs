using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImGuiNET;
using ShapeShifter.Views.Rendering;
using System.Numerics;
using ShapeShifter.Controllers;
using ShapeShifter.Models;

namespace ShapeShifter.Views
{
    /// <summary>
    /// Builds the fixed editing sidebar and documentation window.
    /// Sends user actions to the controller instead of changing scene geometry directly.
    /// </summary>
    public partial class EditorPanel
    {
        public const float SidebarWidth = 320f;

        private Edge? lengthEditorEdge;
        private float targetLength;
        /// <summary>
        /// Draws the full-height sidebar, selected-element controls and drawing algorithm choice.
        /// Opens the separate Help window when requested.
        /// </summary>
        public void Draw(EditorController controller, ref LineAlgorithm lineAlgorithm)
        {
            // Override saved window placement so the sidebar remains part of the application layout.
            ImGui.SetNextWindowPos(Vector2.Zero, ImGuiCond.Always);
            ImGui.SetNextWindowSize(
                new Vector2(SidebarWidth, ImGui.GetIO().DisplaySize.Y),
                ImGuiCond.Always);

            const ImGuiWindowFlags sidebarFlags =
                ImGuiWindowFlags.NoTitleBar
                | ImGuiWindowFlags.NoMove
                | ImGuiWindowFlags.NoResize
                | ImGuiWindowFlags.NoCollapse
                | ImGuiWindowFlags.NoSavedSettings
                | ImGuiWindowFlags.NoBringToFrontOnFocus;

            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0f);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0f);

            if (ImGui.Begin("Editing", sidebarFlags))
            {
                ImGui.SetWindowFontScale(1.35f);
                ImGui.TextColored(EditorTheme.Accent, "ShapeShifter");
                ImGui.SetWindowFontScale(1f);
                ImGui.TextColored(EditorTheme.MutedText, "POLYGON EDITOR");
                ImGui.Spacing();
                DrawHelpButton();

                if (!string.IsNullOrEmpty(controller.StatusMessage))
                {
                    ImGui.Spacing();
                    ImGui.TextWrapped(controller.StatusMessage);
                }

                ImGui.PushItemWidth(150f);
                DrawEdgeGeometryEditor(controller);
                DrawContinuityEditor(controller);
                DrawLengthEditor(controller);
                DrawPerpendicularEditor(controller);
                ImGui.PopItemWidth();

                DrawSectionTitle("Line drawing algorithm");

                if (ImGui.RadioButton("Library",lineAlgorithm == LineAlgorithm.Library))
                {
                    lineAlgorithm = LineAlgorithm.Library;
                }

                if (ImGui.RadioButton("Bresenham",lineAlgorithm == LineAlgorithm.Bresenham))
                {
                    lineAlgorithm = LineAlgorithm.Bresenham;
                }

                DrawSectionTitle("Quick controls");
                ImGui.TextWrapped("LMB: drag a vertex, handle or polygon by its edge.");
                ImGui.TextWrapped("Ctrl + click: select up to two edges.");
                ImGui.TextWrapped("Shift + click: prefer an overlapping handle.");
                ImGui.TextWrapped("Insert over an edge: add a midpoint vertex.");
                ImGui.TextWrapped("Delete over a vertex: remove it.");
                ImGui.TextWrapped("Click empty space without Ctrl to clear selection.");
            }
            ImGui.End();
            ImGui.PopStyleVar(2);
            DrawHelpWindow();
        }

        /// <summary>
        /// Keeps section headings consistent through shared spacing and the theme's accent color.
        /// </summary>
        private static void DrawSectionTitle(string title)
        {
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();
            ImGui.TextColored(EditorTheme.Accent, title);
        }

        /// <summary>
        /// Shows relation controls for two selected line segments after dragging ends.
        /// Delegates adding and removing perpendicularity to the controller.
        /// </summary>
        private void DrawPerpendicularEditor(EditorController controller)
        {
            DrawSectionTitle("Perpendicularity");

            if (controller.SelectedEdges.Count != 2)
            {
                ImGui.TextWrapped("Select exactly two edges using Ctrl + click.");
                return;
            }
            if (controller.SelectedEdges.Any(edge => !edge.IsLine))
            {
                ImGui.Text("Select two line segments.");
                return;
            }

            if (controller.IsDragging)
            {
                ImGui.TextWrapped("Finish dragging to edit the constraint.");
                return;
            }
            if (controller.SelectedVertexHasArc)
            {
                ImGui.Text("G0 - position");
                ImGui.TextWrapped(
                    "Circular arc endpoints use G0 continuity.");
                return;
            }

            if (controller.GetSelectedPerpendicularConstraint() is not null)
            {
                ImGui.Text("Perpendicular constraint active.");
            }

            if (ImGui.Button("Make perpendicular"))
            {
                controller.AddSelectedPerpendicular();
            }

            if (ImGui.Button("Remove perpendicular"))
            {
                controller.RemoveSelectedPerpendicular();
            }
        }
        /// <summary>
        /// Shows the current and requested lengths for a single selected line segment.
        /// Initializes the input from the edge's current length and delegates changes to the controller.
        /// </summary>
        private void DrawLengthEditor(EditorController controller)
        {
            DrawSectionTitle("Edge length");

            var edge = controller.SelectedEdge;

            if (edge == null)
            {
                lengthEditorEdge = null;
                ImGui.Text("Select exactly one edge.");
                return;
            }
            if (!edge.IsLine)
            {
                lengthEditorEdge = null;
                ImGui.TextWrapped("Length constraints are available for line segments.");
                return;
            }

            float currentLength = Vector2.Distance(edge.Start.Position,edge.End.Position);

            // Preserve the user's typed value across frames until another edge is selected.
            if (lengthEditorEdge != edge)
            {
                lengthEditorEdge = edge;
                targetLength = currentLength;
            }

            ImGui.Text($"Current length: {currentLength:F2}");

            var constraint = controller.GetSelectedLengthConstraint();

            if (constraint != null)
            {
                ImGui.Text($"Stored target: {constraint.TargetLength:F2}");
            }
            else
            {
                ImGui.Text("No length constraint.");
            }

            if (controller.IsDragging)
            {
                ImGui.TextWrapped("Finish dragging to edit the constraint.");
                return;
            }
            if (controller.SelectedVertexHasArc)
            {
                ImGui.Text("G0 - position");
                ImGui.TextWrapped(
                    "Circular arc endpoints use G0 continuity.");
                return;
            }

            ImGui.InputFloat("Target length",ref targetLength,1f,10f,"%.2f");

            if (ImGui.Button("Set length"))
            {
                controller.SetSelectedEdgeLength(targetLength);
            }

            ImGui.SameLine();

            if (ImGui.Button("Remove length"))
            {
                controller.RemoveSelectedEdgeLength();
            }
        }
    }
}
