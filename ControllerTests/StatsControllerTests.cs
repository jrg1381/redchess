using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;
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
