using System;
using System.Collections.Generic;
using System.IO;
using Antlr4.Runtime;
using Redchess.Pgn;
using RedChess.ChessCommon;
using RedChess.ChessCommon.Enumerations;
using RedChess.ChessCommon.Interfaces;
using RedChess.EngineFactory;

namespace RedChess.PgnProcessor
{
    class PgnProcessor : IPgnProcessor
    {
        private readonly DateTime m_start;
        private int m_moveCount;
        private static readonly Dictionary<char, PieceType> s_lookup;
        private IBoard m_board;
        private readonly Action<string, string, ChessMove> m_onMoveAction;
        private readonly Action m_onGameOverAction;

        static PgnProcessor()
        {
            s_lookup = new Dictionary<char, PieceType>();
            s_lookup['Q'] = PieceType.Queen;
            s_lookup['K'] = PieceType.King;
            s_lookup['N'] = PieceType.Knight;
            s_lookup['R'] = PieceType.Rook;
            s_lookup['B'] = PieceType.Bishop;
            s_lookup['P'] = PieceType.Pawn;
        }

        internal PgnProcessor(Action<string, string, ChessMove> onMoveAction, Action onGameOverAction)
        {
            m_onMoveAction = onMoveAction;
            m_onGameOverAction = onGameOverAction;
            m_board = BoardFactory.CreateInstance();
            m_start = DateTime.UtcNow;
            m_moveCount = 0;
            if (m_onMoveAction == null)
                return;

            m_onMoveAction(m_board.ToFen(), String.Empty, null);
        }

        public void DoFen(string fen)
        {
            m_board.FromFen(fen);
        }

        internal string Stats()
        {
            var timeTaken = DateTime.UtcNow - m_start;
            return String.Format("{0} moves in {1} ({2} moves per second)", m_moveCount, timeTaken, m_moveCount / timeTaken.TotalSeconds);
        }

        public void ResetGame()
        {
            m_board = BoardFactory.CreateInstance();
            if(m_onGameOverAction != null)
                m_onGameOverAction();
        }

        private static void SymbolToBasicPieceType(char symbol, out PieceType answer)
        {
            // This mimics the PGN rules, if a piece isn't specified it's a pawn.
            if (!s_lookup.TryGetValue(symbol, out answer))
                answer = PieceType.Pawn;
        }

        private Tuple<Location,Location> MakePieceMove(PieceType pieceType, Location targetLocation, string disambiguation, string debug)
        {
            m_moveCount++;

            foreach (var location in m_board.FindPieces(pieceType))
            {
                // Disambiguator can be a column, a row, or a whole square (which would require four pieces of the same type on the board...)
                if (!String.IsNullOrEmpty(disambiguation))
                {
                    if (disambiguation.Length == 1)
                    {
                        if (Char.IsLetter(disambiguation[0]))
                        {
                            if (!location.ToString().ToLower().StartsWith(disambiguation))
                                continue;
                        }
                        else if (Char.IsDigit(disambiguation[0]))
                        {
                            if (!location.ToString().EndsWith(disambiguation))
                                continue;
                        }
                    }
                    else
                    {
                        if (location.ToString().ToLower() != disambiguation)
                            continue;
                    }
                }

                if (m_board.Move(location, targetLocation))
                    return new Tuple<Location,Location>(location, targetLocation);
            }

            throw new InvalidDataException(debug);
        }
        
        private PieceType CalculatePieceTypeFromSymbol(char symbol)
        {
            PieceType basicPieceType;

            SymbolToBasicPieceType(symbol, out basicPieceType);

            if (m_board.CurrentTurn == PieceColor.Black)
                basicPieceType |= PieceType.Black;
            else
                basicPieceType |= PieceType.White;

            return basicPieceType;
        }

        private static Location LowerCaseSquareToLocation(string square)
        {
            return (Location)Enum.Parse(typeof(Location), square.ToUpper());
        }

        public void ProcessMove(IToken token, string promotedPiece, string checkOrMate, string annotationGlyph)
        {
            string tokenText = token.Text;
            Location targetLocation;
            PieceType movingPiece;
            string disambiguationToken = null;

            switch (token.Type)
            {
                case PgnParser.PIECE_TO_SQUARE:
                {
                    if (tokenText.Length == 2)
                    {
                        targetLocation = LowerCaseSquareToLocation(tokenText);
                        movingPiece = CalculatePieceTypeFromSymbol('P');
                    }
                    else
                    {
                        disambiguationToken = tokenText.Substring(1, tokenText.Length - 3);
                        targetLocation = LowerCaseSquareToLocation(tokenText.Substring(tokenText.Length - 2));
                        movingPiece = CalculatePieceTypeFromSymbol(tokenText[0]);
                    }
                    break;
                }
                case PgnParser.CAPTURE:
                {
                    disambiguationToken = tokenText.Substring(1, tokenText.IndexOf('x') - 1);
                    movingPiece = CalculatePieceTypeFromSymbol(tokenText[0]);
                    if (movingPiece.IsOfType(PieceType.Pawn))
                        disambiguationToken = tokenText[0].ToString(); // Pawn capture looks like dxc6
                    targetLocation = LowerCaseSquareToLocation(token.Text.Substring(tokenText.Length - 2));
                    break;
                }
                case PgnParser.CASTLE_KINGSIDE:
                {
                    if (m_board.CurrentTurn == PieceColor.Black)
                    {
                        targetLocation = Location.G8;
                        movingPiece = PieceType.BlackKing;
                    }
                    else
                    {
                        targetLocation = Location.G1;
                        movingPiece = PieceType.WhiteKing;
                    }
                    break;
                }
                case PgnParser.CASTLE_QUEENSIDE:
                {
                    if (m_board.CurrentTurn == PieceColor.Black)
                    {
                        targetLocation = Location.C8;
                        movingPiece = PieceType.BlackKing;
                    }
                    else
                    {
                        targetLocation = Location.C1;
                        movingPiece = PieceType.WhiteKing;
                    }
                    break;
                }
                default:
                {
                    throw new NotImplementedException();
                }
            }

            string debugMessage = String.Format("Line {0} Position {1} : {2}", token.Line, token.Column, tokenText);

            var moveMade = MakePieceMove(movingPiece, targetLocation, disambiguationToken, debugMessage);

            if (m_board.IsAwaitingPromotionDecision())
            {
                m_board.PromotePiece(promotedPiece);
                tokenText += "=" + promotedPiece;
            }

            tokenText += checkOrMate;

            if (m_onMoveAction == null)
                return;

            m_onMoveAction(m_board.ToFen(), tokenText, new ChessMove() { Start = moveMade.Item1, End = moveMade.Item2, Promotion = promotedPiece });
        }
    }
}