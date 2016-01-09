using System;
using System.Collections.Generic;
using RedChess.ChessCommon.Enumerations;
using RedChess.ChessCommon.Interfaces;
using RedChess.WebEngine.Models;

namespace RedChess.WebEngine.Repositories.Interfaces
{
    public interface IGameManager
    {
        IGameBinding FetchGame(int gameId);
        int Add(IBoard board, int opponentId, string currentUser, bool playAsBlack, int timeLimitMs);
        int Add(IBoard board, string currentUser);
        void SaveClock(IClock clock);
        IClock Clock(int gameId);
        HistoryEntry FindByGameIdAndMoveNumber(int gameId, int moveNumber);
        IEnumerable<HistoryEntry> FindAllMoves(int game);
        int CloneBoard(IBoard newBoard, int opponent, string currentUser, bool playAsBlack, int oldGameId, int movesToClone);
        IEnumerable<IGameBinding> FindAll();
        void Delete(int gameId);
        bool IsUsersTurn(int gameId, string currentUser);
        void TimeGameOut(int id, string message, string currentUser);
        void EndGameWithMessage(int id, string message, int? userIdWinner = null);
        bool Move(int id, Location startLocation, Location endLocation, string promoteTo);
        void UpdateMessage(int id);
        IEnumerable<IGameBinding> WithPlayer(string userName);
        string PgnText(int id);
        IEnumerable<UserProfile> AllUserProfiles();
        IEnumerable<IAnalysisBinding> AnalysisForGameMoves(int gameId);
        IEnumerable<IStats> Stats();
        PlayerReadyStatus PlayerReady(int gameId, string playerColor);
        IEnumerable<IEloHistory> EloTable();
        DateTime LastEloUpdate();
        void UpdateEloTable();
    }
}