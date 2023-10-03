namespace Cs2GlobalAdrTracker.Models
{
    internal readonly struct WindowLocation
    {
        internal int X { get; }
        internal int Y { get; }

        internal WindowLocation(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public static bool operator ==(WindowLocation windowLocation1, WindowLocation windowLocation2)
        {
            return windowLocation1.X == windowLocation2.X && windowLocation1.Y == windowLocation2.Y;
        }

        public static bool operator !=(WindowLocation windowLocation1, WindowLocation windowLocation2)
        {
            return !(windowLocation1 == windowLocation2);
        }

        public override readonly bool Equals(object obj)
        {
            return obj is WindowLocation wl && this == wl;
        }

        public override readonly int GetHashCode()
        {
            return this.X.GetHashCode() ^ this.Y.GetHashCode();
        }
    }
}