using System;
using System.Linq;
using NUnit.Framework;
using RedChess.ChessCommon.Enumerations;

namespace Redchess.EngineTests
{
	[TestFixture]
	internal class SlowTests : AbstractChessTest
	{
		[Test]
        [Category("Slow")]
		public void PlayRandomlyFor100Moves()
		{
            DateTime startTime = DateTime.UtcNow;
		    var seed = (int)((DateTime.Now - DateTime.MinValue).TotalMilliseconds%Int32.MaxValue);
		    var random = new Random(seed);

		    Console.WriteLine("Starting random moves with seed {0}", seed);

			for (int j = 0; j < 1000; j++)
			{
				for (int i = 63; i >= 0; i--)
				{
					var loc = (Location)i;
					var piece = NormalBoard.GetContents(loc);
                    if (piece == null || piece.Color != NormalBoard.CurrentTurn)
						continue;
				    var validMoves = piece.ValidMoves(NormalBoard);
                    Assert.NotNull(validMoves, "Expected not null");
                    var validMovesArray = validMoves.ToArray();
                    if (validMovesArray.Any())
					{
                        // Take a random valid move
                        Location targetSquare = validMovesArray.Skip(random.Next(validMovesArray.Count() - 1)).First();
						Console.WriteLine("[{0}] Moving {1} from {2} to {3}", i, piece, piece.Position.Location, targetSquare);
                        NormalBoard.Move(piece.Position.Location, targetSquare);
                        if (NormalBoard.IsAwaitingPromotionDecision())
                        {
                            var promoteTo = NormalBoard.CurrentTurn == PieceColor.White ? "Queen" : "Rook";
                            NormalBoard.PromotePiece(promoteTo);
						}

                        if (NormalBoard.StatusForBoard() == GameStatus.DrawInsufficientMaterial)
                        {
                            Console.WriteLine("Draw!");
                            goto GAME_OVER;
                        }
					}
					else
					{
						Console.WriteLine("No moves for {0} on {1}", piece, piece.Position.Location);
					    var status = NormalBoard.StatusForBoard();
                        if (status == GameStatus.CheckmateBlackWins || status == GameStatus.CheckmateWhiteWins)
						{
							Console.WriteLine("Checkmate!");
							goto GAME_OVER;
						}
                        if (status == GameStatus.Stalemate)
						{
							Console.WriteLine("Stalemate!");
							goto GAME_OVER;
						}
					}
				}
			}

			GAME_OVER:

		    Console.WriteLine("Game took : {0}", DateTime.UtcNow - startTime);
		}
	}
}