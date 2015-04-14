using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Chess.Controllers;
using Chess.Repositories;

namespace ControllerTests
{
    [TestFixture]
    class BoardControllerTests
    {
        [Test]
        public void BasicTest()
        {
            var fakeRepo = new GameRepository();
            var controller = new BoardController(fakeRepo);
            controller.Details(40);
        }
    }
}
