using System.Collections.Generic;
using ShapeShifter.Models;

namespace ShapeShifter.Editing.Interaction
{
    public sealed record EditorViewState(
        HitTestResult? HoveredElement,
        IReadOnlyList<Edge> SelectedEdges,
        Vertex? SelectedVertex = null);
}