using System;
using NUnit.Framework;
using RedChess.ChessCommon.Enumerations;
using Redchess.Engine;
using Redchess.Engine.Pieces.Black;
using Redchess.Engine.Pieces.White;
using Redchess.Engine.Pieces.Abstract;
using Redchess.Engine.Pieces;

namespace EngineTests
{
	[TestFixture]
	public class PieceTests
	{
		[Test]
		public void CreateABoard()
		{
			var b = new Board();
			Assert.NotNull(b);
		}

	    [Test]
	    public void CannotCreateNullPiece()
	    {
	        Assert.Throws(typeof (InvalidOperationException),
	            () => PieceFactory.CreatePiece(PieceType.NullPiece, Location.A1));
	    }
	}
}