using ShapeShifter.Models;

namespace ShapeShifter.Controllers;

public partial class EditorController
{
    public Vertex? SelectedVertex { get; private set; }

    public bool SelectedVertexHasBezier =>
        SelectedVertex != null
        && scene.Polygons
            .SelectMany(polygon => polygon.Edges)
            .Any(edge =>
                edge.Bezier != null
                && (edge.Start == SelectedVertex
                    || edge.End == SelectedVertex));

    public void SetSelectedVertexContinuity(ContinuityType continuity)
    {
        if (IsDragging || SelectedVertex == null)
        {
            StatusMessage = "Select a vertex and finish dragging.";
            return;
        }

        StatusMessage = geometryEditor.TrySetContinuity(
            SelectedVertex, continuity)
                ? $"Continuity set to {continuity}."
                : "Cannot apply the requested continuity.";
    }
}