namespace ShapeShifter.Models.Constraints
{
    public interface IGeometricConstraint
    {
        IReadOnlyList<Edge> AffectedEdges { get; }
    }
}