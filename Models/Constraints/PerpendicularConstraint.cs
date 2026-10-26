namespace ShapeShifter.Models.Constraints
{
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

        public bool Matches(Edge first, Edge second)
        {
            return (First == first && Second == second) || (First == second && Second == first);
        }
    }
}