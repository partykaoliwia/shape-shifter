using System.Numerics;
using ShapeShifter.Models;
using ShapeShifter.Models.Constraints;

namespace ShapeShifter.Editing.Constraints
{
    public partial class ConstraintSolver
    {
        private const int MaxIterations = 512;
        private const float LengthTolerance = 0.001f;
        private const float PerpendicularTolerance = 0.0001f;
        private const float MinLengthSquared = 0.00000001f;

        public bool TrySolve(
            IReadOnlyList<IGeometricConstraint> constraints,
            Dictionary<IGeometryVariable, Vector2> positions,
            IReadOnlySet<IGeometryVariable>? fixedVertices = null,
            ControlPointTarget? controlTarget = null)
        {
            fixedVertices ??= new HashSet<IGeometryVariable>();

            if (!AreFinite(positions))
            {
                return false;
            }

            for (int iteration = 0; iteration < MaxIterations; iteration++)
            {
                if (constraints.All(constraint =>
                    IsSatisfied(constraint, positions, controlTarget)))
                {
                    return true;
                }

                foreach (var constraint in constraints)
                {
                    if (IsSatisfied(constraint, positions, controlTarget))
                    {
                        continue;
                    }

                    bool projected = constraint switch
                    {
                        EdgeLengthConstraint length =>
                            ProjectLength(length, positions, fixedVertices),

                        PerpendicularConstraint perpendicular =>
                            ProjectPerpendicular(
                                perpendicular, positions, fixedVertices),

                        ContinuityConstraint continuity =>
                            ProjectContinuity(
                                continuity,
                                positions,
                                fixedVertices,
                                controlTarget),

                        _ => false
                    };

                    if (!projected)
                    {
                        return false;
                    }
                }

                if (!AreFinite(positions))
                {
                    return false;
                }
            }

            return constraints.All(constraint =>
                IsSatisfied(constraint, positions, controlTarget));
        }

        private static bool IsSatisfied(
            IGeometricConstraint constraint,
            IReadOnlyDictionary<IGeometryVariable, Vector2> positions,
            ControlPointTarget? controlTarget)
        {
            switch (constraint)
            {
                case EdgeLengthConstraint lengthConstraint:
                {
                    float length = Vector2.Distance(
                        positions[lengthConstraint.Edge.Start],
                        positions[lengthConstraint.Edge.End]);

                    return float.IsFinite(length)
                        && MathF.Abs(length - lengthConstraint.TargetLength)
                            <= LengthTolerance;
                }

                case PerpendicularConstraint perpendicular:
                {
                    if (!TryGetDirections(
                        perpendicular, positions, out var u, out var v))
                    {
                        return false;
                    }

                    float cosine = Vector2.Dot(
                        Vector2.Normalize(u),
                        Vector2.Normalize(v));

                    return MathF.Abs(cosine) <= PerpendicularTolerance;
                }
                case ContinuityConstraint continuity:
                    return IsContinuitySatisfied(
                        continuity,
                        positions,
                        controlTarget);

                default:
                    return false;
            }
        }

        private static bool ProjectLength(
            EdgeLengthConstraint constraint,
            Dictionary<IGeometryVariable, Vector2> positions,
            IReadOnlySet<IGeometryVariable> fixedVertices)
        {
            Vertex start = constraint.Edge.Start;
            Vertex end = constraint.Edge.End;

            Vector2 delta = positions[end] - positions[start];
            float length = delta.Length();

            if (!float.IsFinite(length))
            {
                return false;
            }

            float startWeight = fixedVertices.Contains(start) ? 0f : 1f;
            float endWeight = fixedVertices.Contains(end) ? 0f : 1f;
            float totalWeight = startWeight + endWeight;

            if (totalWeight == 0f)
            {
                return false;
            }

            Vector2 direction = length > 0.000001f
                ? delta / length
                : Vector2.UnitX;

            Vector2 correction = direction
                * ((length - constraint.TargetLength) / totalWeight);

            positions[start] += correction * startWeight;
            positions[end] -= correction * endWeight;

            return true;
        }

        private static bool ProjectPerpendicular(
            PerpendicularConstraint constraint,
            Dictionary<IGeometryVariable, Vector2> positions,
            IReadOnlySet<IGeometryVariable> fixedVertices)
        {
            if (!TryGetDirections(
                constraint, positions, out var u, out var v))
            {
                return false;
            }

            Vector2 normalizedU = Vector2.Normalize(u);
            Vector2 normalizedV = Vector2.Normalize(v);

            float cross = normalizedU.X * normalizedV.Y
                - normalizedU.Y * normalizedV.X;

            float angle = MathF.Atan2(
                cross,
                Vector2.Dot(normalizedU, normalizedV));

            float targetAngle = angle >= 0f
                ? MathF.PI / 2f
                : -MathF.PI / 2f;

            float error = angle - targetAngle;

            Vector2 gradientU =
                new Vector2(-u.Y, u.X) / u.LengthSquared();

            Vector2 gradientV =
                new Vector2(-v.Y, v.X) / v.LengthSquared();

            var gradients = new Dictionary<IGeometryVariable, Vector2>();

            AddGradient(gradients, constraint.First.Start, gradientU);
            AddGradient(gradients, constraint.First.End, -gradientU);
            AddGradient(gradients, constraint.Second.Start, -gradientV);
            AddGradient(gradients, constraint.Second.End, gradientV);

            float denominator = gradients
                .Where(entry => !fixedVertices.Contains(entry.Key))
                .Sum(entry => entry.Value.LengthSquared());

            if (!float.IsFinite(denominator) || denominator <= 0f)
            {
                return false;
            }

            float scale = error / denominator;

            foreach (var entry in gradients)
            {
                if (!fixedVertices.Contains(entry.Key))
                {
                    positions[entry.Key] -= scale * entry.Value;
                }
            }

            return true;
        }

        private static void AddGradient(
            Dictionary<IGeometryVariable, Vector2> gradients,
            IGeometryVariable vertex,
            Vector2 gradient)
        {
            gradients.TryGetValue(vertex, out var existing);
            gradients[vertex] = existing + gradient;
        }

        private static bool TryGetDirections(
            PerpendicularConstraint constraint,
            IReadOnlyDictionary<IGeometryVariable, Vector2>positions,
            out Vector2 u,
            out Vector2 v)
        {
            u = positions[constraint.First.End]
                - positions[constraint.First.Start];

            v = positions[constraint.Second.End]
                - positions[constraint.Second.Start];

            float uSquared = u.LengthSquared();
            float vSquared = v.LengthSquared();

            return float.IsFinite(uSquared)
                && float.IsFinite(vSquared)
                && uSquared > MinLengthSquared
                && vSquared > MinLengthSquared;
        }

        private static bool AreFinite(
            IReadOnlyDictionary<IGeometryVariable, Vector2> positions)
        {
            return positions.Values.All(position =>
                float.IsFinite(position.X)
                && float.IsFinite(position.Y));
        }
    }
}