using System.Web.Mvc;
using Chess.Controllers;
using NUnit.Framework;

namespace RedChess.ControllerTests
{
    [TestFixture]
    public class StatsControllerTests
    {
        [Test]
        public void IndexActionReturnsIndexView()
        {
            var controller = new StatsController();
            var result = controller.Index() as ViewResult;
            Assert.AreEqual("Index", result.ViewName);
        }
    }
}
