namespace RendererWebRole.Controllers
{
    public class RenderRequest
    {
        public string Fen { get; set; }
        public int Width { get; set; }
        public string BlackSquareColor { get; set; }
        public string WhiteSquareColor { get; set; }
        public string SurroundColor { get; set; }
    }
}