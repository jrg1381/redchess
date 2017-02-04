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
        private readonly DateTime m_Start;
        private int m_MoveCount;
        private static readonly Dictionary<char, PieceType> Lookup;
        private IBoard m_Board;
        private readonly Action<string, string, ChessMove> m_OnMoveAction;
        private readonly Action m_OnGameOverAction;

        static PgnProcessor()
        {
            Lookup = new Dictionary<char, PieceType>();
            Lookup['Q'] = PieceType.Queen;
            Lookup['K'] = PieceType.King;
            Lookup['N'] = PieceType.Knight;
            Lookup['R'] = PieceType.Rook;
            Lookup['B'] = PieceType.Bishop;
            Lookup['P'] = PieceType.Pawn;
        }

        internal PgnProcessor(Action<string, string, ChessMove> onMoveAction, Action onGameOverAction)
        {
            m_OnMoveAction = onMoveAction;
            m_OnGameOverAction = onGameOverAction;
            m_Board = BoardFactory.CreateInstance();
            m_Start = DateTime.UtcNow;
            m_MoveCount = 0;
            if (m_OnMoveAction == null)
                return;

            m_OnMoveAction(m_Board.ToFen(), String.Empty, null);
        }

        public void DoFen(string fen)
        {
            m_Board.FromFen(fen);
        }

        internal string Stats()
        {
            var timeTaken = DateTime.UtcNow - m_Start;
            return String.Format("{0} moves in {1} ({2} moves per second)", m_MoveCount, timeTaken, m_MoveCount / timeTaken.TotalSeconds);
        }

        public void ResetGame()
        {
            m_Board = BoardFactory.CreateInstance();
            if(m_OnGameOverAction != null)
                m_OnGameOverAction();
        }

        private static void SymbolToBasicPieceType(char symbol, out PieceType answer)
        {
            // This mimics the PGN rules, if a piece isn't specified it's a pawn.
            if (!Lookup.TryGetValue(symbol, out answer))
                answer = PieceType.Pawn;
        }

        private ChessMove MakePieceMove(PieceType pieceType, Location targetLocation, string disambiguation, string promotedPiece)
        {
            m_MoveCount++;

            foreach (var location in m_Board.FindPieces(pieceType))
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

                if (m_Board.Move(location, targetLocation))
                    return new ChessMove(location, targetLocation, promotedPiece);
            }

            throw new InvalidDataException();
        }
        
        private PieceType CalculatePieceTypeFromSymbol(char symbol)
        {
            PieceType basicPieceType;

            SymbolToBasicPieceType(symbol, out basicPieceType);

            if (m_Board.CurrentTurn == PieceColor.Black)
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
                    if (m_Board.CurrentTurn == PieceColor.Black)
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
                    if (m_Board.CurrentTurn == PieceColor.Black)
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

            try
            {
                var moveMade = MakePieceMove(movingPiece, targetLocation, disambiguationToken, promotedPiece);

                if (m_Board.IsAwaitingPromotionDecision())
                {
                    m_Board.PromotePiece(promotedPiece);
                    tokenText += "=" + promotedPiece;
                }

                tokenText += checkOrMate;
                m_OnMoveAction?.Invoke(m_Board.ToFen(), tokenText, moveMade);
            }
            catch (InvalidOperationException e)
            {
                e.Data["ParseError"] = $"Line {token.Line} Position {token.Column} : {tokenText}";
                throw;
            }
        }
    }
}