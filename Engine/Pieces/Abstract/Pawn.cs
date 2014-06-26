using System.Collections.Generic;
using System.Linq;
using RedChess.ChessCommon.Enumerations;
using Redchess.Engine.Interfaces;
using Redchess.Engine.Structures;

namespace Redchess.Engine.Pieces.Abstract
{
    internal abstract class Pawn : Piece
    {
        protected int DirectionOfTravel;
        protected int StartRow;

        protected Pawn(Location loc, PieceType pieceType) : base(loc, pieceType)
        {
        }

        public override IEnumerable<Location> AttackedSquares(IBoardExtended game)
        {
            return ReachableSquares(game).Where(p => (new Square(p)).Y != Position.Y);
        }

        public override IEnumerable<Location> ReachableSquares(IBoardExtended game)
        {
            var enemyPieces = game.Pieces(~Color);
            var ownPieces = game.Pieces(Color);

            bool squareInFrontFree = false;

            var generatedSquare = new Square(Position.X, Position.Y + DirectionOfTravel);
            if (!generatedSquare.Equals(Square.InvalidSquare) && !ownPieces.IsOccupied(generatedSquare.Location) &&
                !enemyPieces.IsOccupied(generatedSquare.Location))
            {
                squareInFrontFree = true;
                yield return generatedSquare.Location;
            }

            var takeLeft = new Square(Position.X - 1, Position.Y + DirectionOfTravel);
            if (!takeLeft.Equals(Square.InvalidSquare) &&
                (enemyPieces.IsOccupied(takeLeft.Location) || game.EnPassantTarget == takeLeft.Location))
            {
                yield return takeLeft.Location;
            }

            var takeRight = new Square(Position.X + 1, Position.Y + DirectionOfTravel);
            if (!takeRight.Equals(Square.InvalidSquare) &&
                (enemyPieces.IsOccupied(takeRight.Location) || game.EnPassantTarget == takeRight.Location))
            {
                yield return takeRight.Location;
            }

            if (Position.Y == StartRow && squareInFrontFree)
                // If in starting position, allow double hop (provided the single hop square is empty)
            {
                var generatedSquareDoubleHop = new Square(Position.X, Position.Y + DirectionOfTravel*2);
                if (!ownPieces.IsOccupied(generatedSquareDoubleHop.Location) &&
                    !enemyPieces.IsOccupied(generatedSquareDoubleHop.Location))
                {
                    yield return generatedSquareDoubleHop.Location;
                }
            }
        }
    }
}