namespace Wallpapers.Service.Source
{
    public class Point
    {
        public int X { get; }
        public int Y { get; }

        public Point() : this(0, 0) { }
        public Point(int size) : this(size, size) { }
        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}
