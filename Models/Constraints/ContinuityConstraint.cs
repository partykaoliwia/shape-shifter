namespace ShapeShifter.Models.Constraints;

public sealed class ContinuityConstraint : IGeometricConstraint
{
    public Vertex Vertex { get; }

    public Edge Incoming { get; }

    public Edge Outgoing { get; }

    public ContinuityType Type => Vertex.Continuity;

    public IReadOnlyList<Edge> AffectedEdges { get; }

    public ContinuityConstraint(
        Vertex vertex,
        Edge incoming,
        Edge outgoing)
    {
        Vertex = vertex;
        Incoming = incoming;
        Outgoing = outgoing;
        AffectedEdges = new[] { incoming, outgoing };
    }
}