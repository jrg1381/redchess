using System.Collections.Generic;
using RedChess.ChessCommon.Interfaces;

namespace RedChess.WebEngine.Repositories.Interfaces
{
    internal interface IGameRepository
    {
        GameDto FindById(int id);
        void Delete(int id);
        void AddOrUpdate(GameDto data);
        void AddAnalysis(int id, int moveNumber, IProcessedAnalysis boardAnalysis);
        string Fen(int gameId);
        object FindWhere(string queryString);
    }
}