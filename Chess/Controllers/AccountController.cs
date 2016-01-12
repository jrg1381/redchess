using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using System.Web.Security;
using Chess.Filters;
using Chess.Models;
using RedChess.ChessCommon.Interfaces;

namespace Chess.Controllers
{
    [Authorize]
	public class AccountController : Controller
	{
	    private readonly IWebSecurityProvider m_webSecurityProvider;

        public AccountController() : this(null)
        { }

	    public AccountController(IWebSecurityProvider webSecurity = null)
	    {
	        m_webSecurityProvider = webSecurity ?? new DefaultWebSecurityProvider();
	    }

		//
		// POST: /Account/JsonLogin
        [AllowAnonymous]
		[HttpPost]
		public JsonResult JsonLogin(LoginModel model, string returnUrl)
		{
			if (ModelState.IsValid)
			{
				if (m_webSecurityProvider.Login(model.UserName, model.Password, persistCookie: model.RememberMe))
				{
                    m_webSecurityProvider.SetAuthCookie(model.UserName, model.RememberMe);
					return Json(new { success = true, redirect = returnUrl });
				}
				else
				{
					ModelState.AddModelError("", "The user name or password provided is incorrect.");
				}
			}

			// If we got this far, something failed
			return Json(new { errors = GetErrorsFromModelState() });
		}

		//
		// POST: /Account/LogOff

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult LogOff()
		{
            m_webSecurityProvider.Logout();

			return RedirectToAction("Index", "Home");
		}

		//
		// POST: /Account/JsonRegister
		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public ActionResult JsonRegister(RegisterModel model, string returnUrl)
		{
			if (ModelState.IsValid)
			{
				// Attempt to register the user
				try
				{
					m_webSecurityProvider.CreateUserAndAccount(model.UserName, model.Password, new { EmailHash = EmailHashForAddress(model.Email)});
                    m_webSecurityProvider.Login(model.UserName, model.Password);

                    m_webSecurityProvider.SetAuthCookie(model.UserName, rememberMe: false);
					return Json(new { success = true, redirect = returnUrl });
				}
				catch (MembershipCreateUserException e)
				{
					ModelState.AddModelError("", ErrorCodeToString(e.StatusCode));
				}
			}

			// If we got this far, something failed
			return Json(new { errors = GetErrorsFromModelState() });
		}

	    private static string EmailHashForAddress(string email)
	    {
	        if (String.IsNullOrEmpty(email))
	            return String.Empty;

	        using (var md5 = MD5.Create())
	        {
	            var bytes = Encoding.UTF8.GetBytes(email);
	            var digestBytes = md5.ComputeHash(bytes);
	            return BitConverter.ToString(digestBytes).Replace("-", "").ToLower();
	        }
	    }

		//
		// POST: /Account/Manage

		#region Helpers
		private ActionResult RedirectToLocal(string returnUrl)
		{
			if (Url.IsLocalUrl(returnUrl))
			{
				return Redirect(returnUrl);
			}
			else
			{
				return RedirectToAction("Index", "Home");
			}
		}

		public enum ManageMessageId
		{
			ChangePasswordSuccess,
			SetPasswordSuccess,
			RemoveLoginSuccess,
		}

		private IEnumerable<string> GetErrorsFromModelState()
		{
			return ModelState.SelectMany(x => x.Value.Errors.Select(error => error.ErrorMessage));
		}

		private static string ErrorCodeToString(MembershipCreateStatus createStatus)
		{
			// See http://go.microsoft.com/fwlink/?LinkID=177550 for
			// a full list of status codes.
			switch (createStatus)
			{
				case MembershipCreateStatus.DuplicateUserName:
					return "User name already exists. Please enter a different user name.";

				case MembershipCreateStatus.DuplicateEmail:
					return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

				case MembershipCreateStatus.InvalidPassword:
					return "The password provided is invalid. Please enter a valid password value.";

				case MembershipCreateStatus.InvalidEmail:
					return "The e-mail address provided is invalid. Please check the value and try again.";

				case MembershipCreateStatus.InvalidAnswer:
					return "The password retrieval answer provided is invalid. Please check the value and try again.";

				case MembershipCreateStatus.InvalidQuestion:
					return "The password retrieval question provided is invalid. Please check the value and try again.";

				case MembershipCreateStatus.InvalidUserName:
					return "The user name provided is invalid. Please check the value and try again.";

				case MembershipCreateStatus.ProviderError:
					return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

				case MembershipCreateStatus.UserRejected:
					return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

				default:
					return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
			}
		}
		#endregion

		//
		// GET: /Account/Manage

		public ActionResult Manage(ManageMessageId? message)
		{
			ViewBag.StatusMessage =
				message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
				: message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
				: message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
				: "";
			ViewBag.HasLocalPassword = true;
			ViewBag.ReturnUrl = Url.Action("Manage");
			return View();
		}

		//
		// POST: /Account/Manage

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Manage(LocalPasswordModel model)
		{
			ViewBag.HasLocalPassword = true;

			if (ModelState.IsValid)
			{
				// ChangePassword will throw an exception rather than return false in certain failure scenarios.
				bool changePasswordSucceeded;
				try
				{
					changePasswordSucceeded = m_webSecurityProvider.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword);
				}
				catch (Exception)
				{
					changePasswordSucceeded = false;
				}

				if (changePasswordSucceeded)
				{
                    return Json(new { success = true, redirect = Url.Action("Index", "Board") });
				}
				else
				{
					ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
				}
			}

            // If we got this far, something failed
            return Json(new { errors = GetErrorsFromModelState() });
		}

	}
}