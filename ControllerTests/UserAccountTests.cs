using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using Chess.Controllers;
using Chess.Models;
using NUnit.Framework;
using RedChess.ChessCommon.Interfaces;
using RedChess.WebEngine.Repositories.Interfaces;
using Rhino.Mocks;

namespace RedChess.ControllerTests
{
    [TestFixture]
    class UserAccountTests
    {
        const string c_Username = "CaptainFoo";
        const string c_Password = "Some password";

        private ICurrentUser IdentityProvider()
        {
            return IdentityProviders.StubIdentityProviderFor(c_Username);
        }

        [Test]
        public void AccountControllerConstructor()
        {
            AccountController c = null;
            Assert.DoesNotThrow(() => { c = new AccountController(); });
            Assert.NotNull(c);
        }

        [Test]
        public void LogoutSuccessfully()
        {
            var mockSecurity = MockRepository.GenerateMock<IWebSecurityProvider>();
            mockSecurity.Expect(x => x.Logout());
            var controller = new AccountController(IdentityProvider(), mockSecurity);
            var result = controller.LogOff() as RedirectToRouteResult;
            mockSecurity.VerifyAllExpectations();

            Assert.AreEqual("Index", result.RouteValues["action"], "Expected log off to redirect successfully");
            Assert.AreEqual("Home", result.RouteValues["controller"], "Expected log off to redirect successfully");
        }

        [Test]
        public void DefaultView()
        {
            AccountController.ManageMessageId? messageId = null;
            var controller = new AccountController();
            var result = controller.Manage(messageId) as ViewResult;

            // Not sure what to usefully assert here, the result doesn't seem to have anything in it to do with the view
            Assert.AreEqual("",result.ViewBag.StatusMessage,"Redirected to wrong view when requesting Manage on AccountController");
        }

        [Test]
        public void LoginSuccessfully()
        {
            var mockSecurity = MockRepository.GenerateMock<IWebSecurityProvider>();
            mockSecurity.Expect(x => x.Login(c_Username, c_Password)).Return(true);
            mockSecurity.Expect(x => x.SetAuthCookie(c_Username, false));

            var controller = new AccountController(IdentityProvider(), mockSecurity);

            var loginModel = new LoginModel
            {
                UserName = c_Username,
                Password = c_Password
            };

            var result = controller.JsonLogin(loginModel, "http://localhost/") as JsonResult;
            mockSecurity.VerifyAllExpectations();

            var loginOk = PropertyUtils.ExtractPropertyValue<bool>(result, "success");
            Assert.AreEqual(true , loginOk, "Expected login to succeed");

            var redirectUrl = PropertyUtils.ExtractPropertyValue<string>(result, "redirect");
            Assert.AreEqual("http://localhost/", redirectUrl, "Expected redirect URL not correct");
        }

        [Test]
        public void ChangePassword()
        {
            const string newPassword = "newpassword";
            var mockSecurity = MockRepository.GenerateMock<IWebSecurityProvider>();
            mockSecurity.Expect(x => x.ChangePassword(c_Username, c_Password, newPassword)).Return(true);
            mockSecurity.Expect(x => x.ChangeEmailHash("5676f35f5c011dbed45ae63e79c86ed4"));

            var controller = new AccountController(IdentityProvider(), mockSecurity);

            var passwordModel = new LocalPasswordModel
            {
                OldPassword = c_Password,
                NewPassword = newPassword,
                ConfirmPassword = newPassword,
                Email = "spam@example.com"
            };

            var result = controller.Manage(passwordModel) as JsonResult;
            mockSecurity.VerifyAllExpectations();

            var loginOk = PropertyUtils.ExtractPropertyValue<bool>(result.Data, "success");
            Assert.AreEqual(true, loginOk, "Expected change password to succeed");
        }

        [Test]
        public void ChangePasswordFailureReturningFalse()
        {
            const string expectedErrorMessage = "The current password is incorrect or the new password is invalid.";
            const string newPassword = "newpassword";

            var mockSecurity = MockRepository.GenerateMock<IWebSecurityProvider>();
            // Expect password change to fail
            mockSecurity.Expect(x => x.ChangePassword(c_Username, c_Password, newPassword)).Return(false);
            mockSecurity.Expect(x => x.ChangeEmailHash(Arg<string>.Is.Anything)).Repeat.Never();

            FailToChangePassword(mockSecurity, newPassword, expectedErrorMessage);
        }

        [Test]
        public void ChangePasswordFailureThrowingException()
        {
            const string expectedErrorMessage = "The current password is incorrect or the new password is invalid.";
            const string newPassword = "newpassword";

            var mockSecurity = MockRepository.GenerateMock<IWebSecurityProvider>();
            // Expect password change to fail
            mockSecurity.Expect(x => x.ChangePassword(c_Username, c_Password, newPassword)).Throw(new InvalidOperationException());
            mockSecurity.Expect(x => x.ChangeEmailHash(Arg<string>.Is.Anything)).Repeat.Never();

            FailToChangePassword(mockSecurity, newPassword, expectedErrorMessage);
        }

        [Test]
        public void ChangePasswordFailureChangingPasswordHashThrows()
        {
            const string expectedErrorMessage = "Password update successful. Unable to update email address.";
            const string newPassword = "newpassword";
            var mockSecurity = MockRepository.GenerateMock<IWebSecurityProvider>();
            mockSecurity.Expect(x => x.ChangePassword(c_Username, c_Password, newPassword)).Return(true);
            mockSecurity.Expect(x => x.ChangeEmailHash(Arg<string>.Is.Anything)).Throw(new InvalidOperationException());

            FailToChangePassword(mockSecurity, newPassword, expectedErrorMessage);
        }

        private void FailToChangePassword(IWebSecurityProvider mockSecurity, string newPassword, string expectedErrorMessage)
        {
            var controller = new AccountController(IdentityProvider(), mockSecurity);

            var passwordModel = new LocalPasswordModel
            {
                OldPassword = c_Password,
                NewPassword = newPassword,
                ConfirmPassword = newPassword,
                Email = "spam@example.com"
            };

            var result = controller.Manage(passwordModel) as JsonResult;
            mockSecurity.VerifyAllExpectations();

            Assert.Throws<ArgumentException>(() => PropertyUtils.ExtractPropertyValue<bool>(result.Data, "success"));

            var errorMessage = PropertyUtils.ExtractPropertyValue<IEnumerable<string>>(result.Data, "errors");
            Assert.AreEqual(expectedErrorMessage, errorMessage.First(), "Expected change password to fail with this error message");
        }

        [Test]
        public void LoginFailure()
        {
            var mockSecurity = MockRepository.GenerateMock<IWebSecurityProvider>();
            mockSecurity.Expect(x => x.Login(c_Username, c_Password)).Return(false);

            var controller = new AccountController(IdentityProvider(), mockSecurity);

            var loginModel = new LoginModel
            {
                UserName = c_Username,
                Password = c_Password
            };

            var result = controller.JsonLogin(loginModel, "http://localhost/") as JsonResult;
            mockSecurity.VerifyAllExpectations();

            Assert.IsFalse(PropertyUtils.ExtractPropertyValue<bool>(result, "success"), "Expected login to fail");
        }

        private void SetupCreateUserMock(IWebSecurityProvider mockSecurity, Action<string> hashCapturer)
        {
            mockSecurity.Expect(x => x.CreateUserAndAccount(
                Arg<string>.Is.Equal(c_Username),
                Arg<string>.Is.Equal(c_Password),
                Arg<object>.Is.Anything)).WhenCalled(
                    (invocation) =>
                    {
                        var anonymousTypeObject = invocation.Arguments[2];
                        var capturedEmailHash = PropertyUtils.ExtractPropertyValue<string>(anonymousTypeObject, "EmailHash");
                        hashCapturer(capturedEmailHash);
                    });
        }

        private void SetupFailingCreateUserMock(IWebSecurityProvider mockSecurity)
        {
            mockSecurity.Expect(x => x.CreateUserAndAccount(
                Arg<string>.Is.Equal(c_Username),
                Arg<string>.Is.Equal(c_Password),
                Arg<object>.Is.Anything)).Throw(new System.Web.Security.MembershipCreateUserException("Something went wrong"));
        }

        [TestCase("", "")]
        [TestCase(null, "")]
        [TestCase("captain.foo@example.com", "5f6dc0354cdc4f0c5d7e3d02efb97fbe")]
        public void CreateUser(string email, string expectedHash)
        {
            string capturedEmailHash = null;

            var mockSecurity = MockRepository.GenerateMock<IWebSecurityProvider>();
            SetupCreateUserMock(mockSecurity, (s) => { capturedEmailHash = s; });

            var mockGameManager = MockRepository.GenerateMock<IGameManager>();
            mockGameManager.Expect(x => x.UserIdIsAdministrator(33)).Return(true);

            var controller = new AccountController(IdentityProvider(), mockSecurity, mockGameManager);

            var registerModel = new RegisterModel()
            {
                Password = c_Password,
                ConfirmPassword = c_Password,
                UserName = c_Username,
            };

            if (email != null)
                registerModel.Email = email;

            var result = controller.JsonRegister(registerModel, "http://localhost/") as JsonResult;
            mockSecurity.VerifyAllExpectations();

            var success = PropertyUtils.ExtractPropertyValue<bool>(result, "success");
            var redirectUrl = PropertyUtils.ExtractPropertyValue<string>(result, "redirect");

            Assert.IsTrue(success, "Expected operation to return success");
            Assert.AreEqual("http://localhost/", redirectUrl, "Expected operation to set redirect url correctly");
            Assert.AreEqual(expectedHash, capturedEmailHash, "Expected email hash was incorrect");
        }

        private IEnumerable<MembershipCreateStatus> MembershipCreateStatusSource()
        {
            return Enum.GetValues(typeof (MembershipCreateStatus)).Cast<MembershipCreateStatus>().Where(p => p != MembershipCreateStatus.ProviderError);
        }

        [Test]
        public void NonAdminCannotRegisterNewUser()
        {
            var mockSecurity = MockRepository.GenerateMock<IWebSecurityProvider>();

            var mockGameManager = MockRepository.GenerateMock<IGameManager>();
            mockGameManager.Expect(x => x.UserIdIsAdministrator(33)).Return(false);

            var controller = new AccountController(IdentityProvider(), mockSecurity, mockGameManager);
            var registerModel = new RegisterModel();

            var result = controller.JsonRegister(registerModel, "http://localhost/") as JsonResult;
            mockSecurity.VerifyAllExpectations();

            Assert.Throws<ArgumentException>(() => PropertyUtils.ExtractPropertyValue<bool>(result, "success"));
            var errors = PropertyUtils.ExtractPropertyValue<IEnumerable<string>>(result.Data, "errors");
            Assert.AreEqual("Only administrators can perform this operation", errors.First(), "Expected error when non-admin created new user");
        }

        [TestCaseSource(nameof(MembershipCreateStatusSource))]
        public void ErrorCodeToStringCoversAllEnum(MembershipCreateStatus status)
        {
            var mockSecurity = MockRepository.GenerateMock<IWebSecurityProvider>();
            var exceptionToThrow = new MembershipCreateUserException(status);

            mockSecurity.Expect(x => x.CreateUserAndAccount(
                Arg<string>.Is.Anything,
                Arg<string>.Is.Anything,
                Arg<object>.Is.Anything)).Throw(exceptionToThrow);

            var mockGameManager = MockRepository.GenerateMock<IGameManager>();
            mockGameManager.Expect(x => x.UserIdIsAdministrator(33)).Return(true);

            var controller = new AccountController(IdentityProvider(), mockSecurity, mockGameManager);
            var registerModel = new RegisterModel();

            var result = controller.JsonRegister(registerModel, "http://localhost/") as JsonResult;
            mockSecurity.VerifyAllExpectations();

            // The test is that the message is not the default error message
            var errorMessage = PropertyUtils.ExtractPropertyValue<IEnumerable<string>>(result.Data, "errors");
            StringAssert.DoesNotStartWith("The authentication provider returned an error. Please verify", errorMessage.First(), "Got default error message instead of specialized one");
        }

        [Test]
        public void CreateUserFailure()
        {
            var mockSecurity = MockRepository.GenerateMock<IWebSecurityProvider>();
            SetupFailingCreateUserMock(mockSecurity);

            var mockGameManager = MockRepository.GenerateMock<IGameManager>();
            mockGameManager.Expect(x => x.UserIdIsAdministrator(33)).Return(true);

            var controller = new AccountController(IdentityProvider(), mockSecurity, mockGameManager);

            var registerModel = new RegisterModel()
            {
                Password = c_Password,
                ConfirmPassword = c_Password,
                UserName = c_Username,
            };

            var result = controller.JsonRegister(registerModel, "http://localhost/") as JsonResult;
            mockSecurity.VerifyAllExpectations();

            var errorMessage = PropertyUtils.ExtractPropertyValue<IEnumerable<string>>(result.Data, "errors");
            StringAssert.StartsWith("The authentication provider returned an error. Please verify", errorMessage.First(), "Expected change password to fail with this error message");
            Assert.Throws<ArgumentException>(() => PropertyUtils.ExtractPropertyValue<bool>(result, "success"));
        }
    }
}
