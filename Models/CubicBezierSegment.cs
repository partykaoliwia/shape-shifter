using System.Numerics;

namespace ShapeShifter.Models
{
    public class CubicBezierSegment
    {
        public Vertex Start { get; }
        public Vertex End { get; }

        public BezierControlPoint Control1 { get; }
        public BezierControlPoint Control2 { get; }

        public CubicBezierSegment(Vertex start, Vertex end)
            : this(
                start,
                end,
                Vector2.Lerp(start.Position, end.Position, 1f / 3f),
                Vector2.Lerp(start.Position, end.Position, 2f / 3f))
        {
        }

        public CubicBezierSegment(
            Vertex start,
            Vertex end,
            Vector2 control1,
            Vector2 control2)
        {
            Start = start;
            End = end;

            Control1 = new BezierControlPoint(start, control1);
            Control2 = new BezierControlPoint(end, control2);
        }

        public (CubicBezierSegment First, CubicBezierSegment Second) Split()
        {
            Vector2 v0 = Start.Position;
            Vector2 v1 = Control1.Position;
            Vector2 v2 = Control2.Position;
            Vector2 v3 = End.Position;

            Vector2 a = (v0 + v1) / 2f;
            Vector2 b = (v1 + v2) / 2f;
            Vector2 c = (v2 + v3) / 2f;

            Vector2 d = (a + b) / 2f;
            Vector2 e = (b + c) / 2f;

            var midpoint = new Vertex((d + e) / 2f);

            return (
                new CubicBezierSegment(Start, midpoint, a, d),
                new CubicBezierSegment(midpoint, End, e, c));
        }
    }
}