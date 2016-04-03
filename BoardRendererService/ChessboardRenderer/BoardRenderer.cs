namespace RedChess.ChessboardRenderer
{
    class BoardRenderer
    {
        private readonly PngGenerator m_Generator;
        private readonly string m_Fen;

        public BoardRenderer(PngGenerator generator, string fen)
        {
            m_Generator = generator;
            m_Fen = fen;
        }
    }
}