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
	}
}