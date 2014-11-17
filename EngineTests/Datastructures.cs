using System;
using System.Collections.Generic;
using NUnit.Framework;
using RedChess.ChessCommon;
using RedChess.ChessCommon.Enumerations;
using Redchess.Engine.Structures;

namespace Redchess.EngineTests
{
    [TestFixture]
    class Datastructures
    {
        [TestCase(Location.A2, Location.A2, true)]
        [TestCase(Location.A2, Location.H4, false)]
        [TestCase(Location.InvalidSquare, Location.InvalidSquare, true)]
        [TestCase(Location.C4, Location.InvalidSquare, false)]
        public void SquaresCompareCorrectly(Location firstLocation, Location secondLocation, bool expected)
        {
            var firstSquare = new Square(firstLocation);
            var secondSquare = new Square(secondLocation);
            Assert.That((firstSquare.Equals(secondSquare)) == expected, "Expected squares to compare correctly");
        }

        [Test]
        public void SquaresNotEqualToNonSquares()
        {
            Assert.False(new Square(Location.B2).Equals("B2"), "Square should not compare equal to another object");
        }

        [Test]
        public void SquaresCanEqualAnotherSquareAsAnObject()
        {
            Assert.True(new Square(Location.B2).Equals(new Square(Location.B2) as object), "Squares should be equal");
        }

        [Test]
        public void CheckHashCode()
        {
            var hashcodesSeen = new HashSet<int>();

            foreach (var location in Enum.GetValues(typeof(Location)))
            {
                var square = new Square((Location)location);
                var hashCode = square.GetHashCode();
                Assert.False(hashcodesSeen.Contains(hashCode), "Duplicate hashcode detected");
                hashcodesSeen.Add(hashCode);
            }
        }

        [TestCase(PieceType.WhiteKing, PieceType.King, true)]
        [TestCase(PieceType.BlackKing, PieceType.BlackRook, false)]
        [TestCase(PieceType.WhiteKing, PieceType.WhiteRook, false)]
        [TestCase(PieceType.BlackKing, PieceType.King, true)]
        [TestCase(PieceType.WhiteBishop, PieceType.Bishop, true)]
        public void IsOfTypeTest(PieceType x, PieceType y, bool expectedResult)
        {
            Assert.AreEqual(expectedResult, x.IsOfType(y), x + " IsOfType(" + y + ")");
        }
    }
}
