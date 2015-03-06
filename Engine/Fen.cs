using System;
using System.Text;
using RedChess.ChessCommon.Enumerations;
using Redchess.Engine.Interfaces;

namespace Redchess.Engine
{
    internal sealed class Fen : AbstractBoardObserver
    {
        private readonly CastlingRules m_castlingRules;
        private string m_fen;

        internal Fen(IBoardExtended board, CastlingRules castlingRules) : base(board)
        {
            m_castlingRules = castlingRules;
            UpdateFen();
        }

        internal string ToFen()
        {
            return m_fen;
        }

        private void UpdateFen()
        {
            var counter = 0; // number of consecutive empty squares seen
            var index = 0; // number of squares seen so far (0-63)
            var sb = new StringBuilder();

            for(var y=7; y >= 0; y--)
            {
                for (var x= 0; x < 8; x++)
                {
                    var p = Board.GetContents((Location) (y*8+x));

                    if (p == null) // Square is empty
                    {
                        counter++;

                        if ((index + 1)%8 == 0) // hit the edge of the board
                        {
                            sb.Append(counter);
                            if (index != 63)
                            {
                                sb.Append("/"); // Don't do this if it's the last row
                            }
                            counter = 0; // start counting blank squares at 0 again
                        }

                        index++;
                        continue;
                    }

                    if (counter > 0)
                    {
                        sb.Append(counter);
                    }

                    sb.Append(PieceData.Symbol(p.Type));

                    if ((index + 1)%8 == 0 && index != 63) // It's the edge of the board, but not the last square
                    {
                        sb.Append("/");
                    }

                    counter = 0;
                    index++;
                }
            }

            sb.AppendFormat(" {0} {1} {2} {3}", Board.CurrentTurn.ToString().ToLower()[0],
                                            m_castlingRules.FenCastleString(),
                                            Board.EnPassantTarget == Location.InvalidSquare ? "-" : Board.EnPassantTarget.ToString(),
                                            Board.FiftyMoveCounter);

            m_fen = sb.ToString();
        }

        public override void OnCompleted()
        {
            UpdateFen();
        }

        internal void ForceFen(string fen)
        {
            m_fen = fen;
        }
    }
}