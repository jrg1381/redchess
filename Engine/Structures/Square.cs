using System;
using RedChess.ChessCommon.Enumerations;

namespace Redchess.Engine.Structures
{
    public struct Square : IEquatable<Square>
    {
        private static readonly Square s_invalidSquare = new Square(Location.InvalidSquare);

        private readonly Location m_location;

        public Square(Location location)
        {
            m_location = location;
        }

        public Square(int x, int y)
        {
            if (x >= 0 && x <= 7 && y >= 0 && y <= 7)
            {
                m_location = (Location) (x + y*8);
            }
            else
            {
                this = InvalidSquare;
            }
        }

        public static Square InvalidSquare
        {
            get { return s_invalidSquare; }
        }

        public Location Location
        {
            get { return m_location; }
        }

        public int X
        {
            get { return ((int) m_location%8); }
        }

        public int Y
        {
            get { return ((int) m_location/8); }
        }

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

        public override int GetHashCode()
        {
            return (int) m_location;
        }
    }
}