using System;
using System.Linq;
using NUnit.Framework;
using RedChess.ChessCommon.Enumerations;
using Redchess.Engine;
using Redchess.Engine.Pieces.Abstract;
using Redchess.Engine.Pieces.Black;
using Redchess.Engine.Pieces.White;

namespace EngineTests
{
	[TestFixture]
	internal class SlowTests : AbstractChessTest
	{
		[Test]
        [Category("Slow")]
		public void PlayRandomlyFor100Moves()
		{
            DateTime startTime = DateTime.UtcNow;
			Random random = new Random();

			for (int j = 0; j < 1000; j++)
			{
				for (int i = 63; i >= 0; i--)
				{
					var loc = (Location)i;
					var piece = m_normalBoard.GetContents(loc);
					if (piece == null || piece.Color != m_normalBoard.CurrentTurn)
						continue;
                    var validMoves = piece.ValidMoves(m_normalBoard).ToArray();
					if (validMoves.Any())
					{
                        // Take a random valid move
						Location targetSquare = validMoves.Skip(random.Next(validMoves.Count() - 1)).First();
						Console.WriteLine("[{0}] Moving {1} from {2} to {3}", i, piece, piece.Position.Location, targetSquare);
						m_normalBoard.MovePiece(piece, targetSquare);
						if (m_normalBoard.IsAwaitingPromotionDecision())
						{
							PieceType promoteTo = m_normalBoard.CurrentTurn == PieceColor.White ? PieceType.WhiteQueen : PieceType.BlackRook;
							m_normalBoard.PromotePiece(promoteTo);
						}
						Console.WriteLine(m_normalBoard);

                        if (m_normalBoard.IsDraw())
                        {
                            Console.WriteLine("Draw!");
                            goto GAME_OVER;
                        }
					}
					else
					{
						Console.WriteLine("No moves for {0} on {1}", piece, piece.Position.Location);
						if (m_normalBoard.IsCheckmate())
						{
							Console.WriteLine("Checkmate!");
							goto GAME_OVER;
						}
						if (m_normalBoard.IsStalemate())
						{
							Console.WriteLine("Stalemate!");
							goto GAME_OVER;
						}
					}
				}
			}

			GAME_OVER:
			;

            Console.WriteLine("Game took : {0}", DateTime.UtcNow - startTime);
		}
	}
}