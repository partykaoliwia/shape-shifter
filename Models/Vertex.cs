using System.Numerics;

namespace ShapeShifter.Models
{
    public class Vertex
    {
        public Vector2 Position { get; private set; }
        public Vertex(Vector2 position)
        {
            Position=position;
        }

        public void MoveTo(Vector2 newPosition)
        {
            Position = newPosition;       
        }
    }
}