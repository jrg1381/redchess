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
using Rhino.Mocks.Expectations;

namespace RedChess.ControllerTests
{
    [TestFixture]
    class UserAccountTests
    {
        const string c_username = "CaptainFoo";
        const string c_password = "Some password";

        [Test]
        public void LoginSuccessfully()
        {
            var mockSecurity = MockRepository.GenerateMock<IWebSecurityProvider>();
            mockSecurity.Expect(x => x.Login(c_username, c_password)).Return(true);
            mockSecurity.Expect(x => x.SetAuthCookie(c_username, false));

            var controller = new AccountController(mockSecurity);

            var loginModel = new LoginModel
            {
                UserName = c_username,
                Password = c_password
            };

            var result = controller.JsonLogin(loginModel, "http://localhost/") as JsonResult;
            mockSecurity.VerifyAllExpectations();

            var loginOk = ExtractPropertyValue<bool>(result, "success");
            Assert.AreEqual(true , loginOk, "Expected login to succeed");

            var redirectUrl = ExtractPropertyValue<string>(result, "redirect");
            Assert.AreEqual("http://localhost/", redirectUrl, "Expected redirect URL not correct");
        }

        [Test]
        public void LoginFailure()
        {
            var mockSecurity = MockRepository.GenerateMock<IWebSecurityProvider>();
            mockSecurity.Expect(x => x.Login(c_username, c_password)).Return(false);

            var controller = new AccountController(mockSecurity);

            var loginModel = new LoginModel
            {
                UserName = c_username,
                Password = c_password
            };

            var result = controller.JsonLogin(loginModel, "http://localhost/") as JsonResult;
            mockSecurity.VerifyAllExpectations();

            Assert.Throws<ArgumentException>(() => ExtractPropertyValue<bool>(result, "success"));
        }

        private void SetupCreateUserMock(IWebSecurityProvider mockSecurity, Action<string> hashCapturer)
        {
            mockSecurity.Expect(x => x.CreateUserAndAccount(
                Arg<string>.Is.Equal(c_username),
                Arg<string>.Is.Equal(c_password),
                Arg<object>.Is.Anything)).WhenCalled(
                    (invocation) =>
                    {
                        var anonymousTypeObject = invocation.Arguments[2];
                        var capturedEmailHash = ExtractPropertyValue<string>(anonymousTypeObject, "EmailHash");
                        hashCapturer(capturedEmailHash);
                    });
            mockSecurity.Expect(x => x.Login(c_username, c_password, false)).Return(true);
            mockSecurity.Expect(x => x.SetAuthCookie(c_username, false));
        }

        [TestCase("", "")]
        [TestCase(null, "")]
        [TestCase("captain.foo@example.com", "5f6dc0354cdc4f0c5d7e3d02efb97fbe")]
        public void CreateUser(string email, string expectedHash)

        {
            string capturedEmailHash = null;

            var mockSecurity = MockRepository.GenerateMock<IWebSecurityProvider>();
            SetupCreateUserMock(mockSecurity, (s) => { capturedEmailHash = s; });

            var controller = new AccountController(mockSecurity);

            var registerModel = new RegisterModel()
            {
                Password = c_password,
                ConfirmPassword = c_password,
                UserName = c_username,
            };

            if (email != null)
                registerModel.Email = email;

            var result = controller.JsonRegister(registerModel, "http://localhost/") as JsonResult;
            mockSecurity.VerifyAllExpectations();

            var success = ExtractPropertyValue<bool>(result, "success");
            var redirectUrl = ExtractPropertyValue<string>(result, "redirect");

            Assert.IsTrue(success, "Expected operation to return success");
            Assert.AreEqual("http://localhost/", redirectUrl, "Expected operation to set redirect url correctly");
            Assert.AreEqual(expectedHash, capturedEmailHash, "Expected email hash was incorrect");
        }

        private static T ExtractPropertyValue<T>(JsonResult result, string propertyName)
        {
            var res = result.Data;
            var propertyInfo = res.GetType().GetProperty(propertyName);
            if(propertyInfo == null)
                throw new ArgumentException("Property '" + propertyName + "' not found on JsonResult");
            var value = propertyInfo.GetValue(res, null);
            return (T)value;
        }

        private static T ExtractPropertyValue<T>(object result, string propertyName)
        {
            var propertyInfo = result.GetType().GetProperty(propertyName);
            if (propertyInfo == null)
                throw new ArgumentException("Property '" + propertyName + "' not found on object");
            var value = propertyInfo.GetValue(result, null);
            return (T)value;
        }
    }
}
