using System.Collections.Generic;
using ShapeShifter.Models;

namespace ShapeShifter.Editing.Interaction
{
    /// <summary>
    /// The hover and selection state needed to highlight scene elements during rendering.
    /// </summary>
    public sealed record EditorViewState(
        HitTestResult? HoveredElement,
        IReadOnlyList<Edge> SelectedEdges,
        Vertex? SelectedVertex = null);
}
