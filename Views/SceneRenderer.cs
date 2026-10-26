using ShapeShifter.Models;
using Raylib_cs;
using ShapeShifter.Rendering;
using System.Numerics;
using ShapeShifter.Geometry;

namespace ShapeShifter.Views
{
    public class SceneRenderer
    {
        public void Draw(Scene scene,Vertex? hoveredVertex,Edge? hoveredEdge,Polygon? hoveredPolygon, LineAlgorithm lineAlgorithm, IReadOnlyList<Edge> selectedEdges)
        {
            foreach (var polygon in scene.Polygons)
            {
                foreach (var edge in polygon.Edges)
                {
                    var color = polygon == hoveredPolygon ? Color.Blue : Color.Black;
                    
                    if (selectedEdges.Contains(edge))
                    {
                        color = Color.Green;
                    }
                    else if (edge == hoveredEdge)
                    {
                        color = Color.Orange;
                    }

                    DrawLine(edge.Start.Position,edge.End.Position,color,lineAlgorithm);
                }
            }

            foreach (var polygon in scene.Polygons)
            {
                foreach (var edge in polygon.Edges)
                {
                    var color = edge.Start == hoveredVertex ? Color.Blue : Color.Red;

                    Raylib.DrawCircleV(edge.Start.Position, 5f, color);
                }
            }
            DrawLengthLabels(scene);
        }
        private static void DrawLengthLabels(Scene scene)
        {
            const int fontSize = 12;
            const int padding = 2;
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
                    int textWidth = Raylib.MeasureText(text, fontSize);

                    int x = (int)MathF.Round(midpoint.X) + offset;
                    int y = (int)MathF.Round(midpoint.Y) - fontSize - offset;

                    // Raylib.DrawRectangle(
                    //     x - padding,
                    //     y - padding,
                    //     textWidth + 2 * padding,
                    //     fontSize + 2 * padding,
                    //     Color.White);

                    Raylib.DrawText(text, x, y, fontSize, Color.DarkGray);
                }
            }
        }
        private static void DrawLine(Vector2 start, Vector2 end, Color color, LineAlgorithm lineAlgorithm)
        {
            int x0 = (int)MathF.Round(start.X);
            int y0 = (int)MathF.Round(start.Y);
            int x1 = (int)MathF.Round(end.X);
            int y1 = (int)MathF.Round(end.Y);
            if (lineAlgorithm == LineAlgorithm.Library)
            {
                Raylib.DrawLine(x0, y0, x1, y1, color);
                return;
            }

            foreach (var pixel in Bresenham.Rasterize(x0, y0, x1, y1))
            {
                Raylib.DrawPixel(pixel.X, pixel.Y, color);
            }
        }
    }
}