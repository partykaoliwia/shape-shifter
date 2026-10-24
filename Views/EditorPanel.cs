using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImGuiNET;
using ShapeShifter.Rendering;

namespace ShapeShifter.Views
{
    public class EditorPanel
    {
        public void Draw(string? statusMessage, ref LineAlgorithm lineAlgorithm)
        {
            if (ImGui.Begin("Editing"))
            {
                ImGui.Text("Drag a vertex with LMB to move it.");
                ImGui.Text("Drag an edge with LMB to move the polygon.");
                ImGui.Text("Insert over an edge: add a midpoint vertex.");
                ImGui.Text("Delete over a vertex: remove it.");
                if (!string.IsNullOrEmpty(statusMessage))
                {
                    ImGui.Separator();
                    ImGui.TextWrapped(statusMessage);
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
    }
}