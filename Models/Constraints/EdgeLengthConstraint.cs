namespace ShapeShifter.Models.Constraints
{
    /// <summary>
    /// Stores the positive target length of a line segment.
    /// The solver adjusts endpoint positions to satisfy this value.
    /// </summary>
    public class EdgeLengthConstraint : IGeometricConstraint
    {
        public Edge Edge { get; }
        public float TargetLength { get; }
        public IReadOnlyList<Edge> AffectedEdges { get; }

        public EdgeLengthConstraint(Edge edge, float targetLength)
        {
            ArgumentNullException.ThrowIfNull(edge);

            if (!float.IsFinite(targetLength) || targetLength <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(targetLength),"Target length must be a positive, finite number.");
            }

            Edge = edge;
            TargetLength = targetLength;
            AffectedEdges = Array.AsReadOnly(new[] { edge });
        }
    }
}
