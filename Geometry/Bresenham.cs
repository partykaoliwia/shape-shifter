namespace ShapeShifter.Geometry
{
    /// <summary>
    /// Rasterizes line segments with integer error updates instead of floating-point slopes.
    /// </summary>
    public static class Bresenham
    {
        /// <summary>
        /// Returns line pixels in any octant, including both endpoints.
        /// May reorder the endpoints to traverse the transformed segment from left to right.
        /// </summary>
        public static IEnumerable<(int X, int Y)> Rasterize(int x1,int y1,int x2,int y2)
        {
            // Swap axes for steep lines.
            bool steep = Math.Abs(y2 - y1) > Math.Abs(x2 - x1);

            if (steep)
            {
                (x1, y1) = (y1, x1);
                (x2, y2) = (y2, x2);
            }

            // Process the transformed segment from left to right.
            if (x1 > x2)
            {
                (x1, x2) = (x2, x1);
                (y1, y2) = (y2, y1);
            }

            int dx = x2 - x1;
            int dy = Math.Abs(y2 - y1);
            int stepY = y1 < y2 ? 1 : -1;

            int d = 2 * dy - dx;
            int incrE = 2 * dy;
            int incrNE = 2 * (dy - dx);

            int x = x1;
            int y = y1;

            yield return steep ? (y, x) : (x, y);

            while (x < x2)
            {
                if (d < 0)
                {
                    d += incrE;
                }
                else
                {
                    d += incrNE;
                    y += stepY;
                }

                x++;

                yield return steep ? (y, x) : (x, y);
            }
        }
    }
}
