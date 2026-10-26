namespace ShapeShifter.Models.Constraints;

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