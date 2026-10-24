using ShapeShifter.Models;
using Raylib_cs;

namespace ShapeShifter.Views
{
    public class SceneRenderer
    {
        public void Draw(Scene scene, Vertex? hoveredVertex)
        {
            foreach (var polygon in scene.Polygons)
            {
                foreach (var edge in polygon.Edges)
                {
                    Raylib.DrawLineV(edge.Start.Position,edge.End.Position,Color.Black);
                }
                foreach (var edge in polygon.Edges)
                {
                    var color = edge.Start == hoveredVertex ? Color.Blue : Color.Red; 
                    Raylib.DrawCircleV(edge.Start.Position,5f,color);
                }
            }
        }
    }
}