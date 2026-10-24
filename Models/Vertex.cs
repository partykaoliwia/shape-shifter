using System.Numerics;

namespace ShapeShifter.Models
{
    public class Vertex
    {
        private Vector2 position;
        public Vector2 Position { get => position;}
        public Vertex(Vector2 pos)
        {
            position=pos;
        }

        public void ChangePosition(Vector2 pos)
        {
            position = pos;       
        }
    }
}