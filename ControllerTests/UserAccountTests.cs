using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Security;
using Chess.Controllers;
using Chess.Models;
using NUnit.Framework;
using RedChess.ChessCommon.Interfaces;
using Rhino.Mocks;

namespace RedChess.ControllerTests
{
    [TestFixture]
    class UserAccountTests
    {
        [Test]
        public void LoginSuccessfully()
        {
            FormsAuthentication.Initialize();

            const string userName = "CaptainFoo";
            const string password = "Some password";

            var mockSecurity = MockRepository.GenerateMock<IWebSecurityProvider>();
            mockSecurity.Expect(x => x.Login(userName, password)).Return(true);

            var controller = new AccountController(mockSecurity);

            var loginModel = new LoginModel
            {
                UserName = userName,
                Password = password 
            };

            var result = controller.JsonLogin(loginModel, "http://localhost/") as JsonResult;
            mockSecurity.VerifyAllExpectations();

            dynamic res = result.Data;
            Assert.AreEqual(true ,res.success, "Expected login to succeed");
        }
    }
}
