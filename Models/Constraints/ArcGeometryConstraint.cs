namespace ShapeShifter.Models.Constraints;

/// <summary>
/// Requires an arc edge's candidate endpoints and tangent to define valid circular geometry.
/// Generated for solving rather than stored as a user-assigned scene relation.
/// </summary>
public sealed class ArcGeometryConstraint : IGeometricConstraint
{
    public Edge Edge { get; }

    public IReadOnlyList<Edge> AffectedEdges { get; }

    public ArcGeometryConstraint(Edge edge)
    {
        Edge = edge;
        AffectedEdges = new[] { edge };
    }
}
