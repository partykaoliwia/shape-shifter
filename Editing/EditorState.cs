using System.Collections.Generic;
using ShapeShifter.Models;

namespace ShapeShifter.Editing
{
    public sealed record EditorState(
        HitTestResult? HoveredElement,
        IReadOnlyList<Edge> SelectedEdges);
}