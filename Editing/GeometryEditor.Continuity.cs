using ShapeShifter.Models;
using ShapeShifter.Models.Constraints;

namespace ShapeShifter.Editing;

public partial class GeometryEditor
{
    public bool TrySetContinuity(
        Vertex vertex,
        ContinuityType continuity)
    {
        bool belongsToScene = scene.Polygons
            .SelectMany(polygon => polygon.Edges)
            .Any(edge => edge.Start == vertex);

        if (!belongsToScene)
        {
            return false;
        }

        var previous = vertex.Continuity;
        vertex.SetContinuity(continuity);

        if (TryApplyCurrentGeometry(scene.Constraints))
        {
            return true;
        }

        vertex.SetContinuity(previous);
        return false;
    }

    private IGeometricConstraint[] GetSolverConstraints(
        IEnumerable<IGeometricConstraint> constraints)
    {
        return constraints
            .Concat(GetContinuityConstraints())
            .ToArray();
    }

    private IEnumerable<ContinuityConstraint> GetContinuityConstraints()
    {
        foreach (var polygon in scene.Polygons)
        {
            foreach (var outgoing in polygon.Edges)
            {
                var vertex = outgoing.Start;

                if (vertex.Continuity == ContinuityType.G0)
                {
                    continue;
                }

                var incoming = polygon.Edges.First(
                    edge => edge.End == vertex);

                if (incoming.Bezier == null && outgoing.Bezier == null)
                {
                    continue;
                }

                yield return new ContinuityConstraint(
                    vertex, incoming, outgoing);
            }
        }
    }

    private bool TryApplyCurrentGeometry(
        IEnumerable<IGeometricConstraint> constraints)
    {
        var positions = CapturePositions();

        var fixedVertices = positions.Keys
            .OfType<Vertex>()
            .Cast<IGeometryVariable>()
            .ToHashSet();

        if (!solver.TrySolve(
            GetSolverConstraints(constraints),
            positions,
            fixedVertices))
        {
            return false;
        }

        ApplyPositions(positions);
        return true;
    }

    private bool TryFinishTopologyChange(
        Polygon polygon,
        IReadOnlyList<Edge> previousEdges)
    {
        var validConstraints = scene.Constraints
            .Where(constraint =>
                constraint.AffectedEdges.All(scene.ContainsEdge))
            .ToArray();

        if (!TryApplyCurrentGeometry(validConstraints))
        {
            polygon.RestoreEdges(previousEdges);
            return false;
        }

        scene.RemoveInvalidConstraints();
        return true;
    }
}