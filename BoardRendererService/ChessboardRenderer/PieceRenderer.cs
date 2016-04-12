using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Windows;
using System.Windows.Baml2006;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xaml;

namespace RedChess.ChessboardRenderer
{
    internal class PieceRenderer
    {
        private readonly DrawingContext m_Context;
        private readonly ResourceManager m_ResourceManager;
        private readonly Dictionary<string, VisualBrush> m_Brushes;

        internal PieceRenderer(DrawingContext context)
        {
            m_Brushes = new Dictionary<string, VisualBrush>();
            m_Context = context;
            m_ResourceManager = new ResourceManager("Redchess.ChessboardRenderer.g", GetType().Assembly);
        }

        private static string MapPieceToResourceName(string pieceName)
        {
            var color = char.IsLower(pieceName[0]) ? "d" : "l";
            return $"pieces/chess_{pieceName.ToLower()}{color}t45.baml";
        }

        private VisualBrush ReadPiece(string pieceName)
        {
            VisualBrush brush;

            if (m_Brushes.TryGetValue(pieceName, out brush))
                return brush;

            var resourceName = MapPieceToResourceName(pieceName);
            var baml = m_ResourceManager.GetObject(resourceName) as MemoryStream;

            var reader = new Baml2006Reader(baml);
            var writer = new XamlObjectWriter(reader.SchemaContext);
            while (reader.Read())
            {
                writer.WriteNode(reader);
            }

            var canvas = writer.Result as Viewbox;
            canvas.Measure(new Size(256, 256));
            canvas.Arrange(new Rect(0, 0, 256, 256));
            canvas.UpdateLayout();

            var vb = new VisualBrush(canvas);
            vb.Stretch = Stretch.Uniform;
            m_Brushes.Add(pieceName, vb);

            return vb;
        }

        /// <summary>
        /// Render a piece on the drawing context at specified position 
        /// </summary>
        /// <param name="pieceName">piecename in FEN style (e.g. p = black pawn, Q = white queen)</param>
        /// <param name="x">Between 0 and 7</param>
        /// <param name="y">Between 0 and 7</param>
        internal void RenderPiece(string pieceName, int x, int y)
        {
            if(x < 0 || x > 7)
                throw new ArgumentOutOfRangeException(nameof(x));
            if(y < 0 || y > 7)
                throw new ArgumentOutOfRangeException(nameof(y));
            if(pieceName.Length != 1 || !"rnkqpbRNKQPB".Contains(pieceName))
                throw new ArgumentOutOfRangeException(nameof(pieceName), "Invalid piece identifier");

            var brush = ReadPiece(pieceName);

            m_Context.DrawRectangle(
                brush,
                null,
                new Rect(new Point(
                    BoardDimensions.BorderWidth + BoardDimensions.PieceMargin + x*BoardDimensions.SquareSize,
                    BoardDimensions.BorderWidth + BoardDimensions.PieceMargin + y*BoardDimensions.SquareSize
                    ),
                    new Vector(
                        BoardDimensions.SquareSize - 2*BoardDimensions.PieceMargin,
                        BoardDimensions.SquareSize - 2*BoardDimensions.PieceMargin
                        )
                    )
                );
        }
    }
}