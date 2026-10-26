using System.Numerics;

namespace ShapeShifter.Models
{
    public class BezierControlPoint
    {
        private readonly Vertex anchor;
        private Vector2 offset;

        public Vector2 Position => anchor.Position + offset;

        public BezierControlPoint(Vertex anchor, Vector2 position)
        {
            this.anchor = anchor;
            offset = position - anchor.Position;
        }

        public void MoveTo(Vector2 position)
        {
            offset = position - anchor.Position;
        }
    }
}