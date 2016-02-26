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
            return ReachableSquares(game).Where(p => new Square(p).Y != Position.Y);
        }

        public override IEnumerable<Location> ReachableSquares(IBoardExtended game)
        {
            var enemyPieces = game.Pieces(~Color);
            var ownPieces = game.Pieces(Color);

            bool squareInFrontFree = false;

            var newY = Position.Y + DirectionOfTravel;

            if (newY < 0 || newY > 7) // This can happen when we calculate the moves available to a pre-promotion pawn
                yield break;

            var generatedSquare = new Square(Position.X, newY);
            if (!ownPieces.IsOccupied(generatedSquare.Location) && !enemyPieces.IsOccupied(generatedSquare.Location))
            {
                squareInFrontFree = true;
                yield return generatedSquare.Location;
            }

            if (Position.X - 1 >= 0)
            {
                var takeLeft = new Square(Position.X - 1, newY);
                if (enemyPieces.IsOccupied(takeLeft.Location) || game.EnPassantTarget == takeLeft.Location)
                {
                    yield return takeLeft.Location;
                }
            }

            if (Position.X + 1 <= 7)
            {
                var takeRight = new Square(Position.X + 1, newY);
                if (enemyPieces.IsOccupied(takeRight.Location) || game.EnPassantTarget == takeRight.Location)
                {
                    yield return takeRight.Location;
                }
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