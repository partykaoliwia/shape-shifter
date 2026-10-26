// using System.Numerics;
// using ShapeShifter.Models;
// using ShapeShifter.Models.Constraints;

// namespace ShapeShifter.Geometry
// {
//     public class LengthConstraintSolver
//     {
//         private const int MaxIterations = 256;
//         private const float LengthTolerance = 0.001f;
//         private const float DirectionEpsilon = 0.000001f;

//         public bool TrySolve(IReadOnlyList<EdgeLengthConstraint> constraints,Dictionary<Vertex, Vector2> positions, Vertex? fixedVertex = null)
//         {
//             for (int iteration = 0; iteration < MaxIterations; iteration++)
//             {
//                 if (AreSatisfied(constraints, positions))
//                 {
//                     return true;
//                 }

//                 foreach (var constraint in constraints)
//                 {
//                     Vertex start = constraint.Edge.Start;
//                     Vertex end = constraint.Edge.End;

//                     Vector2 delta = positions[end] - positions[start];
//                     float length = delta.Length();

//                     if (!float.IsFinite(length))
//                     {
//                         return false;
//                     }

//                     float startWeight = start == fixedVertex ? 0f : 1f;
//                     float endWeight = end == fixedVertex ? 0f : 1f;
//                     float totalWeight = startWeight + endWeight;

//                     if (totalWeight == 0f)
//                     {
//                         continue;
//                     }

//                     Vector2 direction = length > DirectionEpsilon
//                         ? delta / length
//                         : Vector2.UnitX;

//                     Vector2 correction = direction
//                         * ((length - constraint.TargetLength) / totalWeight);

//                     positions[start] += correction * startWeight;
//                     positions[end] -= correction * endWeight;
//                 }
//             }

//             return AreSatisfied(constraints, positions);
//         }

//         private static bool AreSatisfied(IReadOnlyList<EdgeLengthConstraint> constraints,IReadOnlyDictionary<Vertex, Vector2> positions)
//         {
//             foreach (var constraint in constraints)
//             {
//                 float length = Vector2.Distance(
//                     positions[constraint.Edge.Start],
//                     positions[constraint.Edge.End]);

//                 if (!float.IsFinite(length)
//                     || MathF.Abs(length - constraint.TargetLength)
//                         > LengthTolerance)
//                 {
//                     return false;
//                 }
//             }

//             return true;
//         }
//     }
// }