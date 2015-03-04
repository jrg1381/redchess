using System;
using System.Text;
using RedChess.ChessCommon.Enumerations;
using Redchess.Engine.Interfaces;

namespace Redchess.Engine
{
    internal sealed class MoveTranscriber : IObserver<IBoardExtended>, IDisposable
    {
        private readonly IBoardExtended m_board;
        private readonly CastlingRules m_castlingRules;
        private string m_fen;
        private readonly IDisposable m_unsubscriber;

        internal MoveTranscriber(IBoardExtended board, CastlingRules castlingRules)
        {
            m_board = board;
            m_castlingRules = castlingRules;
            m_unsubscriber = m_board.Subscribe(this);
        }

        private void UpdateMove()
        {
            
        }

        public void OnCompleted()
        {
            UpdateMove();
        }

        public void OnError(Exception error)
        {
            return;
        }

        public void OnNext(IBoardExtended value)
        {
            return;
        }

        public void Dispose()
        {
            m_unsubscriber.Dispose();
        }
    }

    internal sealed class Fen : IObserver<IBoardExtended>, IDisposable
    {
        private readonly IBoardExtended m_board;
        private readonly CastlingRules m_castlingRules;
        private string m_fen;
        private readonly IDisposable m_unsubscriber;

        internal Fen(IBoardExtended board, CastlingRules castlingRules)
        {
            m_board = board;
            m_castlingRules = castlingRules;
            m_unsubscriber = m_board.Subscribe(this);
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

            m_fen = sb.ToString();
        }

        public void OnCompleted()
        {
            UpdateFen();
        }

        public void OnError(Exception error)
        {
            return;
        }

        public void OnNext(IBoardExtended value)
        {
            return;
        }

        public void Dispose()
        {
            m_unsubscriber.Dispose();
        }

        internal void ForceFen(string fen)
        {
            m_fen = fen;
        }
    }
}