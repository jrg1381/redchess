using System;

namespace RedChess.WebEngine.Repositories
{
    class AnalysisBinding : IAnalysisBinding
    {
        internal static AnalysisBinding Empty = new AnalysisBinding();

        public string AnalysisText { get; set; }
        public int Evaluation { get; set; }

        public bool IsEmpty()
        {
            return String.IsNullOrEmpty(AnalysisText);
        }
    }
}