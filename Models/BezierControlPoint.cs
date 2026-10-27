using System.Numerics;

namespace ShapeShifter.Models
{
    /// <summary>
    /// A Bezier handle stored as an offset from its endpoint vertex.
    /// Moving the vertex therefore carries the handle along without changing its offset.
    /// </summary>
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

        /// <summary>
        /// Sets the handle's absolute position by updating its offset from the anchor.
        /// </summary>
        public void MoveTo(Vector2 position)
        {
            offset = position - anchor.Position;
        }
    }
}
