using ShapeShifter.Models;
using ShapeShifter.Models.Constraints;

namespace ShapeShifter.Editing;

public partial class GeometryEditor
{
    public bool TrySetContinuity(
        Vertex vertex,
        ContinuityType continuity)
    {
        if (!Enum.IsDefined(continuity))
        {
            return false;
        }

        var incidentEdges = scene.Polygons
            .SelectMany(polygon => polygon.Edges)
            .Where(edge => edge.Start == vertex || edge.End == vertex)
            .ToArray();

        if (incidentEdges.Length == 0)
        {
            return false;
        }

        var previous = vertex.Continuity;
        vertex.SetContinuity(continuity);

        bool invalidArc = incidentEdges.Any(edge =>
            edge.Arc != null
            && !CircularArcSegment.SupportsContinuity(
                edge.Start, edge.End));

        if (invalidArc)
        {
            vertex.SetContinuity(previous);
            return false;
        }

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
            .Concat(GetArcGeometryConstraints())
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

                if (incoming.IsLine && outgoing.IsLine)
                {
                    continue;
                }

                yield return new ContinuityConstraint(
                    vertex, incoming, outgoing);
            }
        }
    }

    private IEnumerable<ArcGeometryConstraint> GetArcGeometryConstraints()
    {
        foreach (var edge in scene.Polygons.SelectMany(p => p.Edges))
        {
            if (edge.Arc != null)
            {
                yield return new ArcGeometryConstraint(edge);
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