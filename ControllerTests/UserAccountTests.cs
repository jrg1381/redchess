using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Security;
using Chess.Controllers;
using Chess.Models;
using Newtonsoft.Json.Serialization;
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

            var loginOk = ExtractPropertyValue<bool>(result, "success");
            Assert.AreEqual(true , loginOk, "Expected login to succeed");

            var redirectUrl = ExtractPropertyValue<string>(result, "redirect");
            Assert.AreEqual("http://localhost/", redirectUrl, "Expected redirect URL not correct");
        }

        private static T ExtractPropertyValue<T>(JsonResult result, string propertyName)
        {
            var res = result.Data;
            var propertyInfo = res.GetType().GetProperty(propertyName);
            var value = propertyInfo.GetValue(res, null);
            return (T)value;
        }
    }
}
