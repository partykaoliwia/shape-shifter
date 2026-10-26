using System.Numerics;

namespace ShapeShifter.Models
{
    public class BezierControlPoint : IGeometryVariable
    {
        private readonly Vertex anchor;
        public Vertex Anchor => anchor;
        public Vector2 Offset => offset;
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