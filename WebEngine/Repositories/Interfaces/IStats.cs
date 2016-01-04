namespace RedChess.WebEngine.Repositories.Interfaces
{
    public interface IStats
    {
        string White { get; set; }
        string Black { get; set; }
        string Winner { get; set; }
        int Count { get; set; }    
    }
}