using System.Numerics;

namespace ShapeShifter.Models
{
    public class Vertex : IGeometryVariable
    {
        public Vector2 Position { get; private set; }
        public ContinuityType Continuity { get; private set; } = ContinuityType.C1;
        public void SetContinuity(ContinuityType continuity)
        {
            Continuity = continuity;
        }
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