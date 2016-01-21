using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
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
        const string c_username = "CaptainFoo";
        const string c_password = "Some password";

        private ICurrentUser IdentityProvider()
        {
            var identityProvider = MockRepository.GenerateStub<ICurrentUser>();
            identityProvider.Expect(x => x.CurrentUser).Return(c_username);
            return identityProvider;
        }

        [Test]
        public void LoginSuccessfully()
        {
            var mockSecurity = MockRepository.GenerateMock<IWebSecurityProvider>();
            mockSecurity.Expect(x => x.Login(c_username, c_password)).Return(true);
            mockSecurity.Expect(x => x.SetAuthCookie(c_username, false));

            var controller = new AccountController(IdentityProvider(), mockSecurity);

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
        public void ChangePassword()
        {
            const string newPassword = "newpassword";
            var mockSecurity = MockRepository.GenerateMock<IWebSecurityProvider>();
            mockSecurity.Expect(x => x.ChangePassword(c_username, c_password, newPassword)).Return(true);
            mockSecurity.Expect(x => x.ChangeEmailHash("5676f35f5c011dbed45ae63e79c86ed4"));

            var controller = new AccountController(IdentityProvider(), mockSecurity);

            var passwordModel = new LocalPasswordModel
            {
                OldPassword = c_password,
                NewPassword = newPassword,
                ConfirmPassword = newPassword,
                Email = "spam@example.com"
            };

            var result = controller.Manage(passwordModel) as JsonResult;
            mockSecurity.VerifyAllExpectations();

            var loginOk = ExtractPropertyValue<bool>(result.Data, "success");
            Assert.AreEqual(true, loginOk, "Expected change password to succeed");
        }

        [Test]
        public void ChangePasswordFailure()
        {
            const string newPassword = "newpassword";
            var mockSecurity = MockRepository.GenerateMock<IWebSecurityProvider>();
            // Expect password change to fail
            mockSecurity.Expect(x => x.ChangePassword(c_username, c_password, newPassword)).Return(false);
            mockSecurity.Expect(x => x.ChangeEmailHash(Arg<string>.Is.Anything)).Repeat.Never();

            var controller = new AccountController(IdentityProvider(), mockSecurity);

            var passwordModel = new LocalPasswordModel
            {
                OldPassword = c_password,
                NewPassword = newPassword,
                ConfirmPassword = newPassword,
                Email = "spam@example.com"
            };

            var result = controller.Manage(passwordModel) as JsonResult;
            mockSecurity.VerifyAllExpectations();

            Assert.Throws<ArgumentException>(() => ExtractPropertyValue<bool>(result.Data, "success"));

            var errorMessage = ExtractPropertyValue<IEnumerable<string>>(result.Data, "errors");
            Assert.AreEqual("The current password is incorrect or the new password is invalid.", errorMessage.First(), "Expected change password to fail with this error message");
        }

        public void ChangeEmailAddress()
        {
            var mockSecurity = MockRepository.GenerateMock<IWebSecurityProvider>();
            mockSecurity.Expect(x => x.ChangePassword(c_username, c_password, "newpassword")).Return(true);
            mockSecurity.Expect(x => x.SetAuthCookie(c_username, false));

            var controller = new AccountController(IdentityProvider(), mockSecurity);

            var loginModel = new LoginModel
            {
                UserName = c_username,
                Password = c_password,
            };

            var result = controller.JsonLogin(loginModel, "http://localhost/") as JsonResult;
            mockSecurity.VerifyAllExpectations();

            var loginOk = ExtractPropertyValue<bool>(result, "success");
            Assert.AreEqual(true, loginOk, "Expected login to succeed");

            var redirectUrl = ExtractPropertyValue<string>(result, "redirect");
            Assert.AreEqual("http://localhost/", redirectUrl, "Expected redirect URL not correct");
        }

        [Test]
        public void LoginFailure()
        {
            var mockSecurity = MockRepository.GenerateMock<IWebSecurityProvider>();
            mockSecurity.Expect(x => x.Login(c_username, c_password)).Return(false);

            var controller = new AccountController(IdentityProvider(), mockSecurity);

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

            var controller = new AccountController(IdentityProvider(), mockSecurity);

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
