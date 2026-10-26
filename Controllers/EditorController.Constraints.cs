using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShapeShifter.Models.Constraints;

namespace ShapeShifter.Controllers
{
    public partial class EditorController
    {
        // Perpendicular constraint
        public PerpendicularConstraint? GetSelectedPerpendicularConstraint()
        {
            if (selectedEdges.Count != 2)
            {
                return null;
            }

            return scene.GetPerpendicularConstraint(
                selectedEdges[0],
                selectedEdges[1]);
        }

        public void AddSelectedPerpendicular()
        {
            if (IsDragging || selectedEdges.Count != 2)
            {
                StatusMessage = "Select exactly two edges and finish dragging.";
                return;
            }

            if (GetSelectedPerpendicularConstraint() is not null)
            {
                StatusMessage = "These edges are already constrained.";
                return;
            }

            if (geometryEditor.TryAddPerpendicular(
                selectedEdges[0], selectedEdges[1]))
            {
                StatusMessage = "Perpendicular constraint applied.";
            }
            else
            {
                StatusMessage =
                    "Could not apply perpendicularity. "
                    + "Previous geometry and constraints were preserved.";
            }
        }

        public void RemoveSelectedPerpendicular()
        {
            if (IsDragging || selectedEdges.Count != 2)
            {
                StatusMessage = "Select exactly two edges and finish dragging.";
                return;
            }

            var constraint = GetSelectedPerpendicularConstraint();

            if (constraint is null)
            {
                StatusMessage = "These edges have no perpendicular constraint.";
                return;
            }

            geometryEditor.RemoveConstraint(constraint);
            StatusMessage = "Perpendicular constraint removed.";
        }

        // Length constraint
        public EdgeLengthConstraint? GetSelectedLengthConstraint()
        {
            var edge = SelectedEdge;
            return edge == null ? null : scene.GetLengthConstraint(edge);
        }
        public void SetSelectedEdgeLength(float targetLength)
        {
            var edge = SelectedEdge;

            if (IsDragging || edge == null)
            {
                StatusMessage = "Select exactly one edge and finish dragging.";
                return;
            }

            if (!float.IsFinite(targetLength) || targetLength <= 0f)
            {
                StatusMessage = "Length must be a positive, finite number.";
                return;
            }

            if (geometryEditor.TrySetLength(edge, targetLength))
            {
                StatusMessage = "Length constraint applied.";
            }
            else
            {
                StatusMessage = "Could not satisfy the requested length. " + "The previous geometry and constraints were preserved.";
            }
        }
        public void RemoveSelectedEdgeLength()
        {
            if (IsDragging || SelectedEdge == null)
            {
                StatusMessage = "Select exactly one edge and finish dragging.";
                return;
            }

            var constraint = GetSelectedLengthConstraint();

            if (constraint == null)
            {
                StatusMessage = "The selected edge has no length constraint.";
                return;
            }

            geometryEditor.RemoveConstraint(constraint);
            StatusMessage = "Length constraint removed.";
        }
    }
}