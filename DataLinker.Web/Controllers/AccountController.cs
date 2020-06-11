using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using DataLinker.Services.Configuration;
using DataLinker.Services.Exceptions;
using DataLinker.Services.Users;
using DataLinker.Web.Helpers;
using DataLinker.Web.Models.Users;
using PagedList;
using Rezare.AuditLog;
using DataLinker.Services.Authorisation;
using DataLinker.Models;
using DataLinker.Database.Models;

namespace DataLinker.Web.Controllers
{
    public class AccountController : BaseController
    {
        private readonly IUserService _users;
        private readonly IConfigurationService _configService;

        private readonly IAuditLogger _auditLog = AuditLogManager.GetAuditLogger();

        public AccountController(IAuthorisationService auth,
            IConfigurationService configService,
            IUserService users)
            : base(auth)
        {
            _configService = configService;
            _users = users;
        }

        public DateTime GetDate => DateTime.UtcNow;

        [Route("organisations/{organizationId}/users")]
        public ActionResult Index(int? page, int organizationId, bool includeInActive = true)
        {
            // Get user details
            List<UserDetailsModel> result = _users.GetOrganisationUsers(organizationId, includeInActive, LoggedInUser);

            ViewBag.PreviousUrl = Url.Action("Index", "Home");
            var pageSize = _configService.ManageUsersPageSize;
            var pageNumber = page ?? 1;
            var userModels = result.Select(i => GetUserModel(i)).ToList();

            // Setup view variables
            var model = new OrganisationUsers
            {
                IncludeInActive = includeInActive,
                IsForSysAdmin = LoggedInUser.IsSysAdmin,
                Users = userModels.ToPagedList(pageNumber, pageSize)
            };

            // Return result
            return View(model);
        }

        [AjaxOnly]
        [Route("organisations/{organizationId}/users/{userId}/edit")]
        public ActionResult Edit(int organizationId, int userId)
        {
            // Setup user model
            var model =_users.GetUserModel(userId, LoggedInUser);

            var result = GetUserModel(model);

            // Return result
            return PartialView("_Edit", result);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("organisations/{organizationId}/users/{userId}/edit")]
        public async Task<ActionResult> Edit(int organizationId, int userId, UserModel model)
        {
            // Edit user details
            var data = (UserDetailsModel)model;
            data.Email = model.Email;
            string statusMsg = await _users.EditUserDetails(userId, data, LoggedInUser);

            // Setup status message
            Toastr.Success("Profile was successfully updated." + statusMsg);

            // Return result
            return new JsonResult { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { isSuccess = true } };
        }

        [AjaxOnly]
        [Route("organisations/{organizationId}/get-users")]
        public ActionResult GetUsers(int? page, int organizationId, bool includeInActive = true)
        {
            List<UserDetailsModel> result =_users.GetOrganisationUsers(organizationId, includeInActive, LoggedInUser);

            var pageSize = _configService.ManageUsersPageSize;
            var pageNumber = page ?? 1;
            var userModels = result.Select(i => GetUserModel(i)).ToList();

            // Setup view variables
            var model = new OrganisationUsers
            {
                IncludeInActive = includeInActive,
                IsForSysAdmin = LoggedInUser.IsSysAdmin,
                Users = userModels.ToPagedList(pageNumber, pageSize)
            };

            return PartialView("_UsersTable", model);
        }

        [AjaxOnly]
        [Route("organisations/{organizationId}/users/{id}/change-status")]
        public void ChangeStatus(int id, bool value)
        {
            // Update user status
            _users.UpdateStatus(id, value, LoggedInUser);
            
            // Audit log
            _auditLog.Log(AuditStream.UserActivity, $"User becomes activate '{value}'",
                new
                {
                    id = LoggedInUser.ID,
                    email = LoggedInUser.Email,
                    userId = id
                },
                new
                {
                    remote_ip = Request.UserHostAddress,
                    browser = Request.Browser.Browser
                });
        }
                
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("organisations/{organizationId}/users")]
        public ActionResult AddNewMember(int organizationId, UserModel model)
        {
            var data = (UserDetailsModel)model;
            data.Email = model.Email;
            _users.AddNewOrganisationUser(organizationId, data, LoggedInUser);

            // Redirect user to list view page
            return RedirectToAction("Index", new { includeInActive = true, organizationId });
        }
        
        [AllowAnonymous]
        [Route("register")]
        public ActionResult Create()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("register")]
        public ActionResult Create(RegisterModel model)
        {
            var data = (UserOrganizationModel)model;
            data.Email = model.Email;
            data.OrganizationName = model.OrganizationName;

            var user = _users.SetupNewOrganisation(data);

            // Log user activity
            _auditLog.Log(AuditStream.UserActivity, "New Registration",
                new
                {
                    id = user.ID,
                    email = user.Email,
                    name = user.Name,
                },
                new
                {
                    remote_ip = Request.UserHostAddress,
                    browser = Request.Browser.Browser
                });

            // Return result
            return View("RegistrationSuccess", new Models.ErrorModel { Message = user.NewEmail });
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult SignIn()
        {
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SignInCallback()
        {
            return RedirectToAction("Index", "Home", null);
        }

        public ActionResult SignOut()
        {
            // abandon session
            Session.Abandon();

            // sign out of identity server
            Request.GetOwinContext().Authentication.SignOut();

            // redirect to root (should trigger login)
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        [AjaxOnly]
        [Route("get-authentication-status")]
        public JsonResult IsAuthorized()
        {
            var result = new {value = User.Identity.IsAuthenticated};
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        [Route("users/check-email-address")]
        public JsonResult IsEmailNotExists(string email, string InitialEmail)
        {
            // Check whether email address already in use
            bool result =_users.CheckWhetherEmailInUse(email, InitialEmail);
            
            // Return result
            return Json(!result, JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        [Route("users/change-email-address")]
        public async Task<ActionResult> ConfirmEmail(string token)
        {
            try
            {
                // Change user email address
                User user = await _users.ChangeEmailAddress(token);

                // Audit log
                _auditLog.Log(AuditStream.UserSecurity, "Email Confirmed",
                    new
                    {
                        id = user.ID,
                        email = user.Email
                    },
                    new
                    {
                        remote_ip = Request.UserHostAddress,
                        browser = Request.Browser.Browser
                    });
            }
            catch (EmailExpiredException)
            {
                return View("EmailLinkExpired", new Models.ErrorModel { Message = ConfigurationManager.AppSettings["DataLinkerContactEmail"] });
            }
            
            // Return result
            return View("EmailConfirmed", new Models.ErrorModel());
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("users/confirm-email-address")]
        public ActionResult ConfirmUser(string token)
        {
            try
            {
                // Check whether token is valid for setup users credentials
                _users.GetUserByEmailConfirmationToken(token);
            }
            catch (EmailExpiredException)
            {
                return View("EmailLinkExpired", new Models.ErrorModel { Message = ConfigurationManager.AppSettings["DataLinkerContactEmail"] });
            }

            // Setup model
            var model = new UserConfirmModel
            {
                Token = token
            };

            // Return result
            return View(model);
        }
        
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("users/confirm-email-address")]
        public async Task<ActionResult> ConfirmUser(UserConfirmModel model)
        {
            try
            {
                // Save user credentials
                User user = await _users.SaveUserCredentials(model.Token, model.Password);

                // Audit log
                _auditLog.Log(AuditStream.UserSecurity, "Registration Completed",
                    new
                    {
                        userId = user.ID
                    },
                    new
                    {
                        remote_ip = Request.UserHostAddress,
                        browser = Request.Browser.Browser
                    });
            }
            catch (EmailExpiredException)
            {
                return View("EmailLinkExpired", new Models.ErrorModel { Message = ConfigurationManager.AppSettings["DataLinkerContactEmail"] });
            }

            Toastr.Success("Email verification process was successfully completed.");
            
            return View("EmailConfirmed", new Models.ErrorModel());
        }

        [Route("organisations/{organizationId}/users/{id}/approve-legal-officer")]
        public void ApproveLegalOfficer(int organizationId, int id)
        {
            // approve legal officer registration
            _users.ApproveLegalOfficerRegistration(id, LoggedInUser);
        }

        [Route("organisations/{organizationId}/users/{id}/decline-legal-officer")]
        public void DeclineLegalOfficer(int id)
        {
            // Decline legal officer registration
            _users.DeclineLegalOfficerRegistration(id, LoggedInUser);
        }

        [Route("users/{id}/resend-email-verification-link")]
        public ActionResult ResendConfirmationEmail(int id)
        {
            User user = _users.ResendEmailConfirmation(id, LoggedInUser);

            // Log details
            _auditLog.Log(AuditStream.UserActivity, "Resend Confirmation Email",
                new
                {
                    userId = user.ID,
                    userEmail = user.Email,
                    triggeredBy = LoggedInUser.ID,
                    triggeredByEmail = LoggedInUser.Email
                },
                new
                {
                    remote_ip = Request.UserHostAddress,
                    browser = Request.Browser.Browser
                });

            // Return details
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        private static UserModel GetUserModel(UserDetailsModel i)
        {
            return new UserModel
            {
                Name = i.Name,
                Email = i.Email,
                ID = i.ID,
                IsActive = i.IsActive,
                Phone = i.Phone,
                IsLegalOfficer = i.IsLegalOfficer,
                IsIntroducedAsLegalOfficer = i.IsIntroducedAsLegalOfficer,
                IsSingleLegalOfficer = i.IsSingleLegalOfficer,
                OrganizationName = i.OrganizationName
            };
        }
    }
}