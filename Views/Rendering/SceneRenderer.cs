using ShapeShifter.Models;
using Raylib_cs;
using System.Numerics;
using ShapeShifter.Geometry;
using ShapeShifter.Editing.Interaction;

namespace ShapeShifter.Views.Rendering
{
    public partial class SceneRenderer
    {
       public void Draw(Scene scene, EditorViewState state,LineAlgorithm lineAlgorithm)
        {
            var hovered = state.HoveredElement;

            foreach (var polygon in scene.Polygons)
            {
                foreach (var edge in polygon.Edges)
                {
                    var color = polygon == hovered?.Polygon
                        ? Color.Blue
                        : Color.Black;

                    if (state.SelectedEdges.Contains(edge))
                    {
                        color = Color.Green;
                    }
                    else if (edge == hovered?.Edge)
                    {
                        color = Color.Orange;
                    }

                    DrawEdge(edge, color, lineAlgorithm);
                }
            }

            foreach (var polygon in scene.Polygons)
            {
                foreach (var edge in polygon.Edges)
                {
                    var color = edge.Start == state.SelectedVertex
                        ? Color.Green
                        : edge.Start == hovered?.Vertex
                            ? Color.Blue
                            : Color.Red;

                    Raylib.DrawCircleV(edge.Start.Position, 5f, color);
                }
            }

            DrawBezierHandles(scene, hovered?.ControlPoint);
            DrawLengthLabels(scene);
            DrawPerpendicularLabels(scene);
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

        private static void DrawEdge(
            Edge edge,
            Color color,
            LineAlgorithm algorithm)
        {
            var segment = edge.Bezier;

            if (segment == null)
            {
                DrawLine(
                    edge.Start.Position,
                    edge.End.Position,
                    color,
                    algorithm);

                return;
            }

            DrawDashedLine(
                segment.Start.Position, segment.Control1.Position);

            DrawDashedLine(
                segment.Control1.Position, segment.Control2.Position);

            DrawDashedLine(
                segment.Control2.Position, segment.End.Position);

            Vector2 previous = segment.Start.Position;

            foreach (var point in CubicBezierPointGenerator.GeneratePoints(segment))
            {
                DrawLine(previous, point, color, algorithm);
                previous = point;
            }
        }

        private static void DrawBezierHandles(
            Scene scene,
            BezierControlPoint? hoveredControlPoint)
        {
            foreach (var polygon in scene.Polygons)
            {
                foreach (var edge in polygon.Edges)
                {
                    if (edge.Bezier is not { } segment)
                    {
                        continue;
                    }

                    DrawHandle(segment.Control1, hoveredControlPoint);
                    DrawHandle(segment.Control2, hoveredControlPoint);
                }
            }
        }

        private static void DrawHandle(
            BezierControlPoint controlPoint,
            BezierControlPoint? hoveredControlPoint)
        {
            var color = controlPoint == hoveredControlPoint
                ? Color.Blue
                : Color.Orange;

            Vector2 position = controlPoint.Position;

            Raylib.DrawRectangle(
                (int)MathF.Round(position.X) - 4,
                (int)MathF.Round(position.Y) - 4,
                8,
                8,
                color);
        }

        private static void DrawDashedLine(Vector2 start, Vector2 end)
        {
            Vector2 delta = end - start;
            float length = delta.Length();

            if (length <= 0f)
            {
                return;
            }

            Vector2 direction = delta / length;

            for (float distance = 0f; distance < length; distance += 10f)
            {
                Raylib.DrawLineV(
                    start + direction * distance,
                    start + direction * MathF.Min(distance + 5f, length),
                    Color.Gray);
            }
        }
    }
}