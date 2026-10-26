using System.Numerics;
using ShapeShifter.Editing;
using ShapeShifter.Models;

namespace ShapeShifter.Setup
{
    // sample data
    public static class DemoSceneFactory
    {
        public static Scene Create()
        {
            var scene = new Scene();

            Vertex[] vertices1 =
            {
                new Vertex(new Vector2(100,100)),
                new Vertex(new Vector2(150,200)),
                new Vertex(new Vector2(200,300)),
            };
            var polygon1 = new Polygon(vertices1);
            scene.AddPolygon(polygon1);
            Vertex[] vertices2 =
            {
                new Vertex(new Vector2(300,400)),
                new Vertex(new Vector2(450,550)),
                new Vertex(new Vector2(500,500)),
                new Vertex(new Vector2(400,400)),
            };
            var polygon2 = new Polygon(vertices2);
            scene.AddPolygon(polygon2);
            vertices1[0].ChangePosition(new Vector2(450, 550));

            var edge = polygon1.Edges[0];

            float length = Vector2.Distance(edge.Start.Position,edge.End.Position);
            scene.SetLengthConstraint(edge, length);
            var geometryEditor = new GeometryEditor(scene);

            bool success = geometryEditor.TryAddPerpendicular(
                polygon1.Edges[1],
                polygon2.Edges[1]);

            if (!success)
            {
                throw new InvalidOperationException( "Could not create the initial perpendicular constraint.");
            }
            
            return scene;
        }
    }
}