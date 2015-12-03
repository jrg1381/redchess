namespace RedChess.ChessCommon.Interfaces
{
    public interface IHistoryEntry
    {
        int MoveNumber { get; set; }
        string Fen { get; set; }
        string Move { get; set; }
    }
}