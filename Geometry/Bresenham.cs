using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShapeShifter.Geometry
{
    public static class Bresenham
    {
        public static IEnumerable<(int X, int Y)> Rasterize(int x0, int y0, int x1, int y1)
        {
            if(x0>x1 || (x0==x1 && y0>y1))
            {
                (x0,x1)=(x1,x0);
                (y0,y1)=(y1,y0);
            }
            int dx = Math.Abs(x1-x0);
            int dy = -Math.Abs(y1-y0);
            int stepX = x0 < x1 ? 1 : -1;
            int stepY = y0 < y1 ? 1 : -1;
            int error = dx + dy;
            while (true)
            {
                yield return (x0,y0);
                if(x0==x1 && y0==y1)
                {
                    yield break;
                }
                int doubledError = 2*error;
                if(doubledError >= dy)
                {
                    error += dy;
                    x0 += stepX;
                }
                if (doubledError <= dx)
                {
                    error += dx;
                    y0 += stepY;
                }
            }
        }
    }
}