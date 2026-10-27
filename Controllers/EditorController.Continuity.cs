using ShapeShifter.Models;

namespace ShapeShifter.Controllers;

public partial class EditorController
{
    public Vertex? SelectedVertex { get; private set; }
    public bool SelectedVertexHasArc => SelectedVertex != null 
        && scene.Polygons
            .SelectMany(polygon => polygon.Edges)
            .Any(edge => edge.Arc != null && (edge.Start == SelectedVertex || edge.End == SelectedVertex));

    public bool SelectedVertexHasBezier => SelectedVertex != null
        && scene.Polygons
            .SelectMany(polygon => polygon.Edges)
            .Any(edge => edge.Bezier != null && (edge.Start == SelectedVertex || edge.End == SelectedVertex));

    /// <summary>
    /// Requests a continuity change for the selected vertex after dragging ends.
    /// Reports rejection when arc settings or other relations prevent the change.
    /// </summary>
    public void SetSelectedVertexContinuity(ContinuityType continuity)
    {
        if (IsDragging || SelectedVertex == null)
        {
            StatusMessage = "Select a vertex and finish dragging.";
            return;
        }


        StatusMessage = geometryEditor.TrySetContinuity(SelectedVertex, continuity)
                ? $"Continuity set to {continuity}."
                : "Cannot apply continuity. Check arc endpoint settings and other constraints.";;
    }
}
