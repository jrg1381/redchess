using System;
using System.Collections.Generic;
using System.EnterpriseServices.Internal;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using RedChess.ChessboardRenderer;

namespace RendererWebRole.Controllers
{
    public class RenderController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage SimpleBitmap(string fen, int width)
        {
            return Bitmap(new RenderRequest() {Fen = fen, Width = width});
        }

        [HttpPost]
        public HttpResponseMessage Bitmap(RenderRequest request)
        {
            var result = new HttpResponseMessage(HttpStatusCode.OK);
            var stream = new MemoryStream();

            var options = new BoardRenderingOptions(
                request.BlackSquareColor ?? "DimGray",
                request.WhiteSquareColor ?? "AntiqueWhite",
                request.SurroundColor ?? "SaddleBrown");

            var renderer = new BoardRenderer(options);
            renderer.SaveDrawingToStream(request.Fen, stream, request.Width);

            stream.Position = 0;

            result.Content = new StreamContent(stream);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            result.Content.Headers.ContentLength = stream.Length;
            return result;
        }
    }
}
