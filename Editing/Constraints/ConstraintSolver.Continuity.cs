using System.Numerics;
using ShapeShifter.Models;
using ShapeShifter.Models.Constraints;

namespace ShapeShifter.Editing.Constraints;

public partial class ConstraintSolver
{
    /// <summary>
    /// A tangent vector together with each geometry variable's contribution to it.
    /// </summary>
    private sealed record Tangent(Vector2 Value, Dictionary<IGeometryVariable, float> Coefficients);

    /// <summary>
    /// Expresses a line, Bezier or arc tangent in terms of snapshot variables.
    /// A dragged handle uses its absolute target so moving its anchor does not move the target.
    /// </summary>
    private static Tangent GetTangent(Edge edge, bool outgoing, IReadOnlyDictionary<IGeometryVariable, Vector2> positions, ControlPointTarget? controlTarget)
    {
        if (edge.Arc is { } arc)
        {
            return new Tangent(positions[arc], new Dictionary<IGeometryVariable, float> {[arc] = 1f});
        }
        if (edge.Bezier is not { } segment)
        {
            return new Tangent(positions[edge.End] - positions[edge.Start],new Dictionary<IGeometryVariable, float> {[edge.Start] = -1f,[edge.End] = 1f});
        }

        var controlPoint = outgoing
            ? segment.Control1
            : segment.Control2;

        // Cubic endpoint derivatives are three times the corresponding handle offset.
        float coefficient = outgoing 
            ? 3f 
            : -3f;

        if (controlTarget is { } target && target.Point == controlPoint)
        {
            return new Tangent(coefficient * (target.Position - positions[controlPoint.Anchor]), new Dictionary<IGeometryVariable, float> { [controlPoint.Anchor] = -coefficient });
        }

        return new Tangent(coefficient * positions[controlPoint], new Dictionary<IGeometryVariable, float> { [controlPoint] = coefficient });
    }

    /// <summary>
    /// Checks full tangent equality for C1 and matching nonzero tangent directions for G1.
    /// G0 needs no correction because the edges already share the same vertex.
    /// </summary>
    private static bool IsContinuitySatisfied(ContinuityConstraint constraint, IReadOnlyDictionary<IGeometryVariable, Vector2> positions, ControlPointTarget? controlTarget)
    {
        var incoming = GetTangent(constraint.Incoming, false, positions, controlTarget);

        var outgoing = GetTangent(constraint.Outgoing, true, positions, controlTarget);

        if (constraint.Type == ContinuityType.G0)
        {
            return true;
        }

        if (constraint.Type == ContinuityType.C1)
        {
            return Vector2.DistanceSquared(incoming.Value, outgoing.Value) <= LengthTolerance * LengthTolerance;
        }

        if (incoming.Value.LengthSquared() <= MinLengthSquared || outgoing.Value.LengthSquared() <= MinLengthSquared)
        {
            return false;
        }

        Vector2 u = Vector2.Normalize(incoming.Value);
        Vector2 v = Vector2.Normalize(outgoing.Value);

        float cross = u.X * v.Y - u.Y * v.X;

        // Collinear tangents must also point the same way to avoid a reversed join.
        return MathF.Abs(cross) <= PerpendicularTolerance && Vector2.Dot(u, v) > 0f;
    }

    /// <summary>
    /// Corrects C1 as a vector difference and G1 as an angle between tangents.
    /// Distributes the correction over the variables that are allowed to move.
    /// </summary>
    private static bool ProjectContinuity(ContinuityConstraint constraint, Dictionary<IGeometryVariable, Vector2> positions, IReadOnlySet<IGeometryVariable> fixedVariables, ControlPointTarget? controlTarget)
    {
        var incoming = GetTangent(constraint.Incoming, false, positions, controlTarget);

        var outgoing = GetTangent(constraint.Outgoing, true, positions, controlTarget);

        if (constraint.Type == ContinuityType.G0)
        {
            return true;
        }

        if (constraint.Type == ContinuityType.C1)
        {
            var coefficients = new Dictionary<IGeometryVariable, float>(outgoing.Coefficients);

            foreach (var entry in incoming.Coefficients)
            {
                coefficients.TryGetValue(entry.Key, out float existing);
                coefficients[entry.Key] = existing - entry.Value;
            }

            return ProjectVector(
                outgoing.Value - incoming.Value,
                coefficients,
                positions,
                fixedVariables);
        }

        if (incoming.Value.LengthSquared() <= MinLengthSquared)
        {
            return InitializeTangent(
                incoming, outgoing, positions, fixedVariables);
        }

        if (outgoing.Value.LengthSquared() <= MinLengthSquared)
        {
            return InitializeTangent(
                outgoing, incoming, positions, fixedVariables);
        }

        Vector2 u = incoming.Value;
        Vector2 v = outgoing.Value;

        Vector2 normalizedU = Vector2.Normalize(u);
        Vector2 normalizedV = Vector2.Normalize(v);

        float cross = normalizedU.X * normalizedV.Y
            - normalizedU.Y * normalizedV.X;

        float error = MathF.Atan2(
            cross,
            Vector2.Dot(normalizedU, normalizedV));

        Vector2 incomingGradient =
            -new Vector2(-u.Y, u.X) / u.LengthSquared();

        Vector2 outgoingGradient =
            new Vector2(-v.Y, v.X) / v.LengthSquared();

        var gradients = new Dictionary<IGeometryVariable, Vector2>();

        foreach (var entry in incoming.Coefficients)
        {
            AddGradient(
                gradients,
                entry.Key,
                entry.Value * incomingGradient);
        }

        foreach (var entry in outgoing.Coefficients)
        {
            AddGradient(
                gradients,
                entry.Key,
                entry.Value * outgoingGradient);
        }

        float denominator = gradients
            .Where(entry => !fixedVariables.Contains(entry.Key))
            .Sum(entry => entry.Value.LengthSquared());

        if (!float.IsFinite(denominator) || denominator <= 0f)
        {
            return false;
        }

        foreach (var entry in gradients)
        {
            if (!fixedVariables.Contains(entry.Key))
            {
                positions[entry.Key] -=
                    (error / denominator) * entry.Value;
            }
        }

        return true;
    }

    /// <summary>
    /// Gives a zero tangent a usable direction before a G1 angle can be evaluated.
    /// Follows the other tangent when possible, otherwise uses the horizontal direction.
    /// </summary>
    private static bool InitializeTangent(
        Tangent tangent,
        Tangent other,
        Dictionary<IGeometryVariable, Vector2> positions,
        IReadOnlySet<IGeometryVariable> fixedVariables)
    {
        Vector2 direction =
            other.Value.LengthSquared() > MinLengthSquared
                ? Vector2.Normalize(other.Value)
                : Vector2.UnitX;

        return ProjectVector(
            tangent.Value - direction,
            tangent.Coefficients,
            positions,
            fixedVariables);
    }

    /// <summary>
    /// Removes a linear vector error using the contributions of movable variables.
    /// Returns false when no free variable can affect the result.
    /// </summary>
    private static bool ProjectVector(
        Vector2 error,
        IReadOnlyDictionary<IGeometryVariable, float> coefficients,
        Dictionary<IGeometryVariable, Vector2> positions,
        IReadOnlySet<IGeometryVariable> fixedVariables)
    {
        float denominator = coefficients
            .Where(entry => !fixedVariables.Contains(entry.Key))
            .Sum(entry => entry.Value * entry.Value);

        if (!float.IsFinite(denominator) || denominator <= 0f)
        {
            return false;
        }

        foreach (var entry in coefficients)
        {
            if (!fixedVariables.Contains(entry.Key))
            {
                positions[entry.Key] -=
                    error * (entry.Value / denominator);
            }
        }

        return true;
    }
}
