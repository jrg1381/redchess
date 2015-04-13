using NUnit.Framework;
using Redchess.Engine;

namespace Redchess.EngineTests
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
	    public void DisposeABoard()
	    {
	        var b = new Board();
            b.Dispose();
            Assert.Pass();
	    }
	}
}