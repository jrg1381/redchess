using System;
using RedChess.ChessCommon.Enumerations;

namespace Redchess.Engine.Structures
{
    public struct Square : IEquatable<Square>
    {
        private readonly Location m_Location;

        public Square(Location location)
        {
            m_Location = location;
        }

        public Square(int x, int y)
        {
#if DEBUG
            if (!(x >= 0 && x <= 7 && y >= 0 && y <= 7))
                throw new InvalidOperationException("Location out of range");
#endif
            m_Location = (Location) (x + y*8);
        }

        public Location Location => m_Location;

        public int X => (int) m_Location%8;

        public int Y => (int) m_Location/8;

        #region IEquatable<Square> Members

        public bool Equals(Square other)
        {
            return other.Location == m_Location;
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
            return (int) m_Location;
        }
    }
}