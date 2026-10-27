namespace ShapeShifter.Models.Constraints
{
    /// <summary>
    /// Requires two distinct line segments to have perpendicular directions.
    /// The segments may belong to different polygons.
    /// </summary>
    public class PerpendicularConstraint : IGeometricConstraint
    {
        public Edge First { get; }
        public Edge Second { get; }

        public IReadOnlyList<Edge> AffectedEdges { get; }

        public PerpendicularConstraint(Edge first, Edge second)
        {
            ArgumentNullException.ThrowIfNull(first);
            ArgumentNullException.ThrowIfNull(second);

            if (first == second)
            {
                throw new ArgumentException("Two different edges are required.",nameof(second));
            }

            First = first;
            Second = second;

            AffectedEdges = Array.AsReadOnly(new[] { first, second });
        }

        /// <summary>
        /// Checks the edge pair in either order, since the relation is symmetric.
        /// </summary>
        public bool Matches(Edge first, Edge second)
        {
            return (First == first && Second == second) || (First == second && Second == first);
        }
    }
}
