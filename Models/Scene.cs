using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShapeShifter.Models.Constraints;

namespace ShapeShifter.Models
{
    /// <summary>
    /// Owns polygons and their explicitly assigned geometric constraints.
    /// Stores relations without solving them; geometry edits are handled by the editor.
    /// </summary>
    public class Scene
    {
        private readonly List<Polygon> polygons = new();
        public IReadOnlyList<Polygon> Polygons => polygons;
        private readonly List<IGeometricConstraint> constraints = new();

        public IReadOnlyList<IGeometricConstraint> Constraints => constraints;
        public void AddPolygon(Polygon polygon)
        {
            polygons.Add(polygon);
        }
        public bool ContainsEdge(Edge edge)
        {
            return polygons.Any(polygon => polygon.Edges.Contains(edge));
        }

        /// <summary>
        /// Stores a length constraint, replacing any previous length for this edge.
        /// Does not move vertices or check whether all scene relations can be satisfied.
        /// </summary>
        public void SetLengthConstraint(Edge edge, float targetLength)
        {
            ArgumentNullException.ThrowIfNull(edge);

            if (!ContainsEdge(edge))
            {
                throw new ArgumentException("The edge does not belong to this scene.",nameof(edge));
            }

            var constraint = new EdgeLengthConstraint(edge, targetLength);

            constraints.RemoveAll(existing => existing is EdgeLengthConstraint lengthConstraint && lengthConstraint.Edge == edge);

            constraints.Add(constraint);
        }

        /// <summary>
        /// Removes a stored relation while leaving the current geometry unchanged.
        /// </summary>
        /// <returns>True if the constraint was present and removed.</returns>
        public bool RemoveConstraint(IGeometricConstraint constraint)
        {
            return constraints.Remove(constraint);
        }

        /// <summary>
        /// Drops relations that reference edges no longer present after a topology change.
        /// </summary>
        public void RemoveInvalidConstraints()
        {
            constraints.RemoveAll(constraint => constraint.AffectedEdges.Any(edge => !ContainsEdge(edge)));
        }
        public EdgeLengthConstraint? GetLengthConstraint(Edge edge)
        {
            return constraints
                .OfType<EdgeLengthConstraint>()
                .FirstOrDefault(constraint => constraint.Edge == edge);
        }
        /// <summary>
        /// Finds the relation for an edge pair, regardless of the order of the two edges.
        /// </summary>
        public PerpendicularConstraint? GetPerpendicularConstraint(Edge first, Edge second)
        {
            return constraints
                .OfType<PerpendicularConstraint>()
                .FirstOrDefault(constraint => constraint.Matches(first, second));
        }

        public IEnumerable<PerpendicularConstraint> GetPerpendicularConstraints()
        {
            return constraints.OfType<PerpendicularConstraint>();
        }

        /// <summary>
        /// Stores perpendicularity between two scene edges unless that pair already has it.
        /// Does not adjust geometry; the editor solves the proposed relation before adding it.
        /// </summary>
        public void AddPerpendicularConstraint(Edge first, Edge second)
        {
            ArgumentNullException.ThrowIfNull(first);
            ArgumentNullException.ThrowIfNull(second);

            if (!ContainsEdge(first) || !ContainsEdge(second))
            {
                throw new ArgumentException(
                    "Both edges must belong to this scene.");
            }

            if (GetPerpendicularConstraint(first, second) is not null)
            {
                return;
            }

            constraints.Add(new PerpendicularConstraint(first, second));
        }
    }
}
