namespace ShapeShifter.Models.Constraints
{
    /// <summary>
    /// Describes a relation and the edges it connects.
    /// Affected edges are used to track dependencies and remove stale relations.
    /// </summary>
    public interface IGeometricConstraint
    {
        IReadOnlyList<Edge> AffectedEdges { get; }
    }
}
