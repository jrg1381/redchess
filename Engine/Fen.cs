using System.Text;
using RedChess.ChessCommon.Enumerations;
using Redchess.Engine.Interfaces;

namespace Redchess.Engine
{
    internal sealed class Fen
    {
        private readonly IBoardExtended m_board;
        private readonly CastlingRules m_castlingRules;

        internal Fen(IBoardExtended board, CastlingRules castlingRules)
        {
            m_board = board;
            m_castlingRules = castlingRules;
        }

        internal string ToFen()
        {
            var counter = 0; // number of consecutive empty squares seen
            var index = 0; // number of squares seen so far (0-63)
            var sb = new StringBuilder();

            for(var y=7; y >= 0; y--)
            {
                for (var x= 0; x < 8; x++)
                {
                    var p = m_board.GetContents((Location) (y*8+x));

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

            sb.AppendFormat(" {0} {1} {2} {3}", m_board.CurrentTurn.ToString().ToLower()[0],
                                            m_castlingRules.FenCastleString(),
                                            m_board.EnPassantTarget == Location.InvalidSquare ? "-" : m_board.EnPassantTarget.ToString(),
                                            m_board.FiftyMoveCounter);

            return sb.ToString();
        }
    }
}