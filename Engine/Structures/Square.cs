using System;
using RedChess.ChessCommon.Enumerations;

namespace Redchess.Engine.Structures
{
    public struct Square : IEquatable<Square>
    {
        private readonly Location m_location;

        public Square(Location location)
        {
            m_location = location;
        }

        public Square(int x, int y)
        {
#if DEBUG
            if (!(x >= 0 && x <= 7 && y >= 0 && y <= 7))
                throw new InvalidOperationException("Location out of range");
#endif
            m_location = (Location) (x + y*8);
        }

        public Location Location => m_location;

        public int X => (int) m_location%8;

        public int Y => (int) m_location/8;

        #region IEquatable<Square> Members

        public bool Equals(Square other)
        {
            return other.Location == m_location;
        }

        #endregion

        public override bool Equals(object obj)
        {
            if (obj is Square)
                return Equals((Square) obj);
            return false;
        }

        public static bool operator ==(Square s1, Square s2)
        {
            return s1.Equals(s2);
        }

        public static bool operator !=(Square s1, Square s2) => !s1.Equals(s2);

        public override int GetHashCode()
        {
            return (int) m_location;
        }
    }
}