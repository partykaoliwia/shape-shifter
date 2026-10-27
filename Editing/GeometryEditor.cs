using System.Numerics;
using ShapeShifter.Editing.Constraints;
using ShapeShifter.Models;
using ShapeShifter.Models.Constraints;

namespace ShapeShifter.Editing
{
    /// <summary>
    /// Coordinates scene edits and adjusts geometry to preserve its constraints.
    /// </summary>
    public partial class GeometryEditor
    {
        private readonly Scene scene;
        private readonly ConstraintSolver solver = new();

        public GeometryEditor(Scene scene)
        {
            this.scene = scene;
        }

        /// <summary>
        /// Adds or replaces a line segment's length constraint, keeping its start vertex fixed.
        /// The constraint and adjusted geometry are applied only if solving succeeds.
        /// </summary>
        /// <returns>
        /// True if applied; false if the edge is not a line in this scene or solving fails.
        /// </returns>
        public bool TrySetLength(Edge edge, float targetLength)
        {
            if (!edge.IsLine || !scene.ContainsEdge(edge))
            {
                return false;
            }

            var previous = scene.GetLengthConstraint(edge);
            var replacement = new EdgeLengthConstraint(edge, targetLength);

            var constraints = GetSolverConstraints(scene.Constraints
                    .Where(constraint => !ReferenceEquals(constraint, previous))
                    .Append(replacement));

            var positions = CapturePositions();
            var fixedVertices = new HashSet<IGeometryVariable>{edge.Start};

            if (!solver.TrySolve(constraints, positions, fixedVertices))
            {
                return false;
            }

            scene.SetLengthConstraint(edge, targetLength);
            ApplyPositions(positions);

            return true;
        }

        /// <summary>
        /// Adds perpendicularity between two line segments, including segments in different polygons.
        /// Keeps the first segment's start vertex fixed and preserves the scene if solving fails.
        /// </summary>
        /// <returns>
        /// True if the relation already exists or was added; false if the edges are invalid or solving fails.
        /// </returns>
        public bool TryAddPerpendicular(Edge first, Edge second)
        {
            if (first == second
                || !first.IsLine
                || !second.IsLine
                || !scene.ContainsEdge(first)
                || !scene.ContainsEdge(second))
            {
                return false;
            }

            if (scene.GetPerpendicularConstraint(first, second) is not null)
            {
                return true;
            }

            var constraint = new PerpendicularConstraint(first, second);

            var constraints = GetSolverConstraints(scene.Constraints.Append(constraint));

            var positions = CapturePositions();
            var fixedVertices = new HashSet<IGeometryVariable> { first.Start };

            if (!solver.TrySolve(constraints, positions, fixedVertices))
            {
                return false;
            }

            scene.AddPerpendicularConstraint(first, second);
            ApplyPositions(positions);

            return true;
        }
        /// <summary>
        /// Moves a vertex to the requested position and adjusts related geometry.
        /// If local solving fails, moves the connected group to keep the vertex at its target.
        /// </summary>
        public void MoveVertexTo(Vertex vertex, Vector2 position)
        {
            var targets = new Dictionary<Vertex, Vector2>
            {
                [vertex] = position
            };

            ApplyMove(targets, position - vertex.Position);
        }

        /// <summary>
        /// Moves all polygon vertices by the same offset while preserving constraints.
        /// Geometry connected through constraints may move along with the polygon.
        /// </summary>
        public void MovePolygonBy(Polygon polygon, Vector2 offset)
        {
            var targets = polygon.Edges
                .Select(edge => edge.Start)
                .Distinct()
                .ToDictionary(
                    vertex => vertex,
                    vertex => vertex.Position + offset);

            ApplyMove(targets, offset);
        }
        /// <summary>
        /// Inserts a midpoint vertex on the selected edge.
        /// Updates relations for the new edges, or restores the original edges if solving fails.
        /// </summary>
        /// <returns>True if the split was accepted; otherwise, false.</returns>
        public bool TrySplitEdge(Polygon polygon, Edge edge)
        {
            if (!scene.Polygons.Contains(polygon))
            {
                return false;
            }

            var previousEdges = polygon.Edges.ToArray();

            if (!polygon.TrySplitEdge(edge))
            {
                return false;
            }

            return TryFinishTopologyChange(polygon, previousEdges);
        }

        /// <summary>
        /// Removes a vertex and joins its neighbors with a line segment.
        /// Drops constraints on replaced edges only after the remaining relations are satisfied.
        /// </summary>
        /// <returns>
        /// True if the removal was accepted; otherwise, false with the original state preserved.
        /// </returns>
        public bool TryRemoveVertex(Polygon polygon, Vertex vertex)
        {
            if (!scene.Polygons.Contains(polygon))
            {
                return false;
            }

            var previousEdges = polygon.Edges.ToArray();

            if (!polygon.TryRemoveVertex(vertex))
            {
                return false;
            }

            return TryFinishTopologyChange(polygon, previousEdges);
        }
    

        /// <summary>
        /// Removes an explicit constraint without changing the current geometry.
        /// </summary>
        /// <returns>True if the constraint was present and removed.</returns>
        public bool RemoveConstraint(IGeometricConstraint constraint)
        {
            return scene.RemoveConstraint(constraint);
        }
        private void EnsureEdgeBelongsToScene(Edge edge)
        {
            if (!scene.ContainsEdge(edge))
            {
                throw new ArgumentException(
                    "The edge does not belong to this scene.",
                    nameof(edge));
            }
        }

        /// <summary>
        /// Tries to satisfy constraints with the target vertices held at their requested positions.
        /// Falls back to translating their connected group if local solving fails.
        /// </summary>
        private void ApplyMove(
            Dictionary<Vertex, Vector2> targets,
            Vector2 offset)
        {
            var constraints = GetSolverConstraints(scene.Constraints);

            var positions = CapturePositions();

            // Keep dragged vertices at their targets; only related geometry may adjust.
            var fixedVertices = targets.Keys
                .Cast<IGeometryVariable>()
                .ToHashSet();

            foreach (var target in targets)
            {
                positions[target.Key] = target.Value;
            }

            if (solver.TrySolve(constraints, positions, fixedVertices))
            {
                ApplyPositions(positions);
                return;
            }

            // A shared translation preserves existing relations when local solving fails.
            foreach (var vertex in GetConnectedVertices(
                targets.Keys, constraints))
            {
                vertex.MoveTo(vertex.Position + offset);
            }

            foreach (var target in targets)
            {
                target.Key.MoveTo(target.Value);
            }
        }

        private Dictionary<IGeometryVariable, Vector2> CapturePositions()
        {
            return GeometrySnapshot.Capture(scene);
        }

        private static void ApplyPositions(
            IReadOnlyDictionary<IGeometryVariable, Vector2> positions)
        {
            GeometrySnapshot.Apply(positions);
        }

        /// <summary>
        /// Finds vertices linked to the starting vertices through constraints.
        /// Each constraint links the endpoints of all its affected edges, even across polygons.
        /// </summary>
        private static HashSet<Vertex> GetConnectedVertices(
            IEnumerable<Vertex> seeds,
            IReadOnlyList<IGeometricConstraint> constraints)
        {
            var connected = seeds.ToHashSet();

            var groups = constraints
                .Select(constraint => constraint.AffectedEdges
                    .SelectMany(edge => new[] { edge.Start, edge.End })
                    .Distinct()
                    .ToArray())
                .ToArray();

            bool changed;

            do
            {
                changed = false;

                foreach (var group in groups)
                {
                    if (!group.Any(connected.Contains))
                    {
                        continue;
                    }

                    foreach (var vertex in group)
                    {
                        changed |= connected.Add(vertex);
                    }
                }
            }
            while (changed);

            return connected;
        }
    }
}
