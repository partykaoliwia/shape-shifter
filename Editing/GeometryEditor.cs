using System.Numerics;
using ShapeShifter.Geometry;
using ShapeShifter.Models;
using ShapeShifter.Models.Constraints;

namespace ShapeShifter.Editing
{
    public class GeometryEditor
    {
        private readonly Scene scene;
        private readonly LengthConstraintSolver solver = new();

        public GeometryEditor(Scene scene)
        {
            this.scene = scene;
        }

        public bool TrySetLength(Edge edge, float targetLength)
        {
            if (!scene.ContainsEdge(edge))
            {
                return false;
            }

            var newConstraint = new EdgeLengthConstraint(edge, targetLength);

            var constraints = GetLengthConstraints()
                .Where(constraint => constraint.Edge != edge)
                .Append(newConstraint)
                .ToArray();

            var positions = CapturePositions();

            if (!solver.TrySolve(constraints, positions, edge.Start))
            {
                return false;
            }

            scene.SetLengthConstraint(edge, targetLength);
            ApplyPositions(positions);

            return true;
        }

        public void MoveVertexTo(Vertex vertex, Vector2 position)
        {
            var constraints = GetLengthConstraints();
            var positions = CapturePositions();

            Vector2 offset = position - vertex.Position;
            positions[vertex] = position;

            if (solver.TrySolve(constraints, positions, vertex))
            {
                ApplyPositions(positions);
                return;
            }

            // Translate the constrained component if local solving fails.
            foreach (var connectedVertex in GetConnectedVertices(vertex, constraints))
            {
                connectedVertex.ChangePosition(connectedVertex.Position + offset);
            }

            vertex.ChangePosition(position);
        }

        private EdgeLengthConstraint[] GetLengthConstraints()
        {
            return scene.Constraints
                .OfType<EdgeLengthConstraint>()
                .ToArray();
        }

        private Dictionary<Vertex, Vector2> CapturePositions()
        {
            return scene.Polygons
                .SelectMany(polygon => polygon.Edges)
                .SelectMany(edge => new[] { edge.Start, edge.End })
                .Distinct()
                .ToDictionary(vertex => vertex, vertex => vertex.Position);
        }

        private static void ApplyPositions(IReadOnlyDictionary<Vertex, Vector2> positions)
        {
            foreach (var entry in positions)
            {
                entry.Key.ChangePosition(entry.Value);
            }
        }

        private static HashSet<Vertex> GetConnectedVertices(Vertex vertex, IReadOnlyList<EdgeLengthConstraint> constraints)
        {
            var connected = new HashSet<Vertex> { vertex };
            bool changed;

            do
            {
                changed = false;

                foreach (var constraint in constraints)
                {
                    Vertex start = constraint.Edge.Start;
                    Vertex end = constraint.Edge.End;

                    if (connected.Contains(start) || connected.Contains(end))
                    {
                        changed |= connected.Add(start);
                        changed |= connected.Add(end);
                    }
                }
            }
            while (changed);

            return connected;
        }
    }
}