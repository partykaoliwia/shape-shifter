using ShapeShifter.Models;

namespace ShapeShifter.Editing.Interaction
{
    public sealed class HitTestResult
    {
        public Polygon Polygon { get; }
        public Vertex? Vertex { get; }
        public BezierControlPoint? ControlPoint { get; }
        public Edge? Edge { get; }

        public HitTestResult(Polygon polygon, Vertex vertex)
        {
            Polygon = polygon;
            Vertex = vertex;
        }

        public HitTestResult(Polygon polygon,BezierControlPoint controlPoint)
        {
            Polygon = polygon;
            ControlPoint = controlPoint;
        }

        public HitTestResult(Polygon polygon, Edge edge)
        {
            Polygon = polygon;
            Edge = edge;
        }
    }
}