namespace ShapeShifter.Models.Constraints
{
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