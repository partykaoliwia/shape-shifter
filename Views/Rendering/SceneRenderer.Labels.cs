using ShapeShifter.Models;
using Raylib_cs;
using System.Numerics;

namespace ShapeShifter.Views.Rendering
{
    public partial class SceneRenderer
    {
        private static void DrawLengthLabels(Scene scene)
        {
            const int fontSize = 12;
            const int offset = 4;

            foreach (var polygon in scene.Polygons)
            {
                foreach (var edge in polygon.Edges)
                {
                    var constraint = scene.GetLengthConstraint(edge);

                    if (constraint == null)
                    {
                        continue;
                    }

                    Vector2 midpoint = (edge.Start.Position + edge.End.Position) / 2f;

                    string text = $"L = {constraint.TargetLength:F2}";

                    int x = (int)MathF.Round(midpoint.X) + offset;
                    int y = (int)MathF.Round(midpoint.Y) - fontSize - offset;

                    Raylib.DrawText(text, x, y, fontSize, Color.DarkGray);
                }
            }
        }
        private static void DrawPerpendicularLabels(Scene scene)
        {
            var rows = new Dictionary<Edge, int>();
            int pairNumber = 1;

            foreach (var constraint in scene.GetPerpendicularConstraints())
            {
                foreach (var edge in constraint.AffectedEdges)
                {
                    rows.TryGetValue(edge, out int row);

                    Vector2 midpoint =
                        (edge.Start.Position + edge.End.Position) / 2f;

                    int x = (int)MathF.Round(midpoint.X) + 4;
                    int y = (int)MathF.Round(midpoint.Y) + 8 + row * 14;

                    Raylib.DrawText(
                        $"P{pairNumber}",
                        x,
                        y,
                        12,
                        Color.DarkGray);

                    rows[edge] = row + 1;
                }

                pairNumber++;
            }
        }
    }
}