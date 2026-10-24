using System.Numerics;
using ShapeShifter.Models;

namespace ShapeShifter.Controllers
{
    public class EditorController
    {
        private const float VertexHitRadius = 8f;
        private readonly Scene scene;
        public EditorController(Scene scn)
        {
            scene = scn;
        }

        public Vertex? FindVertexAt(Vector2 position)
        {
            foreach (var polygon in scene.Polygons)
            {
                foreach(var edge in polygon.Edges)
                {
                    var dist = Vector2.Distance(position, edge.Start.Position);
                    if(dist <= VertexHitRadius)
                    {
                        return edge.Start;
                    }
                }
            }
            return null;
        }
    }
}