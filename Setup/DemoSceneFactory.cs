using System.Numerics;
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
                new Vertex(new System.Numerics.Vector2(100,100)),
                new Vertex(new System.Numerics.Vector2(150,200)),
                new Vertex(new System.Numerics.Vector2(200,300)),
            };
            var polygon1 = new Polygon(vertices1);
            scene.AddPolygon(polygon1);
            Vertex[] vertices2 =
            {
                new Vertex(new System.Numerics.Vector2(300,400)),
                new Vertex(new System.Numerics.Vector2(450,600)),
                new Vertex(new System.Numerics.Vector2(500,500)),
                new Vertex(new System.Numerics.Vector2(400,400)),
            };
            var polygon2 = new Polygon(vertices2);
            scene.AddPolygon(polygon2);
            vertices1[0].ChangePosition(new Vector2(450, 550));
            return scene;
        }
    }
}