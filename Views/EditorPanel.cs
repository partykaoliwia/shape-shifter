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
    public partial class EditorPanel
    {
        private Edge? lengthEditorEdge;
        private float targetLength;
        public void Draw(EditorController controller, ref LineAlgorithm lineAlgorithm)
        {
            if (ImGui.Begin("Editing"))
            {
                DrawPerpendicularEditor(controller);
                DrawLengthEditor(controller);
                DrawBezierEditor(controller);
                ImGui.Text("Drag a vertex with LMB to move it.");
                ImGui.Text("Drag an edge with LMB to move the polygon.");
                ImGui.Text("Insert over an edge: add a midpoint vertex.");
                ImGui.Text("Delete over a vertex: remove it.");
                ImGui.Text("Click an edge to select it.");
                ImGui.Text("Ctrl + click: toggle selection, up to two edges.");
                ImGui.Text("Click empty space without Ctrl to clear selection.");
                if (!string.IsNullOrEmpty(controller.StatusMessage))
                {
                    ImGui.Separator();
                    ImGui.TextWrapped(controller.StatusMessage);
                }

                ImGui.Separator();
                ImGui.Text("Line drawing algorithm");

                if (ImGui.RadioButton("Library",lineAlgorithm == LineAlgorithm.Library))
                {
                    lineAlgorithm = LineAlgorithm.Library;
                }

                if (ImGui.RadioButton("Bresenham",lineAlgorithm == LineAlgorithm.Bresenham))
                {
                    lineAlgorithm = LineAlgorithm.Bresenham;
                }
            }
            ImGui.End();
        }
        private void DrawPerpendicularEditor(EditorController controller)
        {
            ImGui.Separator();
            ImGui.Text("Perpendicularity");

            if (controller.SelectedEdges.Count != 2)
            {
                ImGui.Text("Select exactly two edges using Ctrl + click.");
                return;
            }
            if (controller.SelectedEdges.Any(edge => edge.Bezier != null))
            {
                ImGui.Text("Select two line segments.");
                return;
            }

            if (controller.IsDragging)
            {
                ImGui.Text("Finish dragging to edit the constraint.");
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
        private void DrawLengthEditor(EditorController controller)
        {
            ImGui.Separator();
            ImGui.Text("Edge length");

            var edge = controller.SelectedEdge;

            if (edge == null)
            {
                lengthEditorEdge = null;
                ImGui.Text("Select exactly one edge.");
                return;
            }
            if (edge.Bezier != null)
            {
                lengthEditorEdge = null;
                ImGui.Text("Length constraints are available for line segments.");
                return;
            }

            float currentLength = Vector2.Distance(edge.Start.Position,edge.End.Position);

            // Initialize the input only when the selected edge changes.
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
                ImGui.Text("Finish dragging to edit the constraint.");
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