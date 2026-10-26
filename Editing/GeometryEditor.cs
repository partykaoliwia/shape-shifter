using System.Numerics;
using ShapeShifter.Editing.Constraints;
using ShapeShifter.Models;
using ShapeShifter.Models.Constraints;

namespace ShapeShifter.Editing
{
    public class GeometryEditor
    {
        private readonly Scene scene;
        private readonly ConstraintSolver solver = new();

        public GeometryEditor(Scene scene)
        {
            this.scene = scene;
        }

        public bool TrySetLength(Edge edge, float targetLength)
        {
            if (edge.Bezier != null || !scene.ContainsEdge(edge))
            {
                return false;
            }

            var previous = scene.GetLengthConstraint(edge);
            var replacement = new EdgeLengthConstraint(edge, targetLength);

            var constraints = scene.Constraints
                .Where(constraint => !ReferenceEquals(constraint, previous))
                .Append(replacement)
                .ToArray();

            var positions = CapturePositions();
            var fixedVertices = new HashSet<Vertex> { edge.Start };

            if (!solver.TrySolve(constraints, positions, fixedVertices))
            {
                return false;
            }

            scene.SetLengthConstraint(edge, targetLength);
            ApplyPositions(positions);

            return true;
        }

        public bool TryAddPerpendicular(Edge first, Edge second)
        {
            if (first == second
                || first.Bezier !=  null
                || second.Bezier != null
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

            var constraints = scene.Constraints
                .Append(constraint)
                .ToArray();

            var positions = CapturePositions();
            var fixedVertices = new HashSet<Vertex> { first.Start };

            if (!solver.TrySolve(constraints, positions, fixedVertices))
            {
                return false;
            }

            scene.AddPerpendicularConstraint(first, second);
            ApplyPositions(positions);

            return true;
        }
        public void MoveControlPointTo(BezierControlPoint controlPoint,Vector2 position)
        {
            controlPoint.MoveTo(position);
        }
        public void MoveVertexTo(Vertex vertex, Vector2 position)
        {
            var targets = new Dictionary<Vertex, Vector2>
            {
                [vertex] = position
            };

            ApplyMove(targets, position - vertex.Position);
        }

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
        public bool TrySplitEdge(Polygon polygon, Edge edge)
        {
            if (!scene.Polygons.Contains(polygon))
            {
                return false;
            }

            if (!polygon.TrySplitEdge(edge))
            {
                return false;
            }

            scene.RemoveInvalidConstraints();
            return true;
        }

        public bool TryRemoveVertex(Polygon polygon, Vertex vertex)
        {
            if (!scene.Polygons.Contains(polygon))
            {
                return false;
            }

            if (!polygon.TryRemoveVertex(vertex))
            {
                return false;
            }

            scene.RemoveInvalidConstraints();
            return true;
        }
        public int ConvertEdgeToBezier(Edge edge)
        {
            EnsureEdgeBelongsToScene(edge);

            var affectedConstraints = scene.Constraints
                .Where(constraint => constraint.AffectedEdges.Contains(edge))
                .ToArray();

            edge.EnableBezier();

            foreach (var constraint in affectedConstraints)
            {
                scene.RemoveConstraint(constraint);
            }

            return affectedConstraints.Length;
        }

        public void ConvertEdgeToLine(Edge edge)
        {
            EnsureEdgeBelongsToScene(edge);
            edge.DisableBezier();
        }

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

        private void ApplyMove(
            Dictionary<Vertex, Vector2> targets,
            Vector2 offset)
        {
            var constraints = scene.Constraints.ToArray();
            var positions = CapturePositions();
            var fixedVertices = targets.Keys.ToHashSet();

            foreach (var target in targets)
            {
                positions[target.Key] = target.Value;
            }

            if (solver.TrySolve(constraints, positions, fixedVertices))
            {
                ApplyPositions(positions);
                return;
            }

            // Translate the connected component without changing its shape.
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

        private Dictionary<Vertex, Vector2> CapturePositions()
        {
            return scene.Polygons
                .SelectMany(polygon => polygon.Edges)
                .SelectMany(edge => new[] { edge.Start, edge.End })
                .Distinct()
                .ToDictionary(vertex => vertex, vertex => vertex.Position);
        }

        private static void ApplyPositions(
            IReadOnlyDictionary<Vertex, Vector2> positions)
        {
            foreach (var entry in positions)
            {
                entry.Key.MoveTo(entry.Value);
            }
        }

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