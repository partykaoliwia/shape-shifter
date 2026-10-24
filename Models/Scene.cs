using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShapeShifter.Models
{
    public class Scene
    {
        private readonly List<Polygon> polygons = new();
        public IReadOnlyList<Polygon> Polygons => polygons;
        public void AddPolygon(Polygon polygon)
        {
            polygons.Add(polygon);
        }
    }
}