using System.Collections.Generic;
using System.Linq;
using DataLinker.Services.Exceptions;
using System.Net.Mail;
using System;
using DataLinker.Services.Tokens;
using DataLinker.Services.Emails;
using System.Threading.Tasks;
using DataLinker.Services.Urls;
using DataLinker.Models.Enums;
using DataLinker.Services.Mappings;
using DataLinker.Services.Identities;
using Rezare.CommandBuilder.Services;
using DataLinker.Database.Models;
using DataLinker.Models;

namespace DataLinker.Services.Users
{
    public class UserService : IUserService
    {
        private IService<Organization, int> _organisations;
        private IService<User, int> _users;
        private ITokenService _tokens;
        private INotificationService _notifications;
        private DateTime GetDate => DateTime.UtcNow;
        private IUrlProvider _urls;
        private IIdsApiService _identities;

        public UserService(IService<Organization, int> organisations,
            IService<User, int> users,
            ITokenService tokens,
            INotificationService notifications,
            IUrlProvider urls,
            IIdsApiService ids)
        {
            _organisations = organisations;
            _users = users;
            _tokens = tokens;
            _notifications = notifications;
            _urls = urls;
            _identities = ids;
        }

        public Organization AddNewOrganisationUser(int organizationId, UserDetailsModel model, LoggedInUserDetails loggedInUser)
        {
            // Validate new user
            if (!loggedInUser.IsSysAdmin)
            {
                if (loggedInUser.Organization.ID != organizationId)
                {
                    throw new BaseException("Access denied");
                }
            }

            // Get organisation
            var organization = _organisations.FirstOrDefault(i=>i.ID == organizationId);

            // Return error if organisation not found
            if (organization == null)
            {
                throw new BaseException("Specified organization not found");
            }

            // Validate email address
            try
            {
                new MailAddress(model.Email);
            }
            catch (FormatException)
            {
                throw new BaseException("Invalid email format.");
            }

            // Check whether is not used
            if (CheckWhetherEmailInUse(model.Email, string.Empty))
            {
                throw new BaseException("Email already in use.");
            }

            // Setup new user
            var newUser = new User
            {
                Email = model.Email,
                IsIntroducedAsLegalOfficer = model.IsIntroducedAsLegalOfficer,
                Phone = model.Phone,
                OrganizationID = organization.ID,
                Name = model.Name
            };

            // Save new user
            this._users.Add(newUser);

            // Generate confirmation link for saved user id
            var tokenInfo = _tokens.GenerateEmailConfirmationToken(newUser.ID);

            // Setup confirmation details
            newUser.NewEmail = newUser.Email;
            newUser.Token = tokenInfo.Token;
            newUser.TokenExpire = tokenInfo.TokenExpire;

            // Save confirmation changes
            _users.Update(newUser);

            // Send confirmation email to Users email address
            this.SendNewUserInvitation(newUser.ID, tokenInfo.TokenInfoEncoded, loggedInUser.ID.Value);
            if (!organization.IsActive && newUser.IsIntroducedAsLegalOfficer)
            {
                // Notify active organization members about manual verification process
                var members = _users.Where(i => i.OrganizationID == organization.ID).Where(i => i.IsActive == true);

                foreach (var member in members)
                {
                    _notifications.User.LegalOfficerRegisteredInBackground(member.ID);
                }
            }

            return organization;
        }
        public User ResendEmailConfirmation(int id, LoggedInUserDetails LoggedInUser)
        {
            // Check whether user is not admin
            if (!LoggedInUser.IsSysAdmin)
            {
                throw new BaseException("Access denied");
            }

            // Get user
            var user = _users.FirstOrDefault(i=>i.ID == id);

            // Check whether user is active
            if (user.IsActive)
            {
                throw new BaseException("Not Allowed. User is already active");
            }

            // Generate token details
            var tokenInfo = _tokens.GenerateEmailConfirmationToken(user.ID);

            // Setup token details for user
            user.NewEmail = user.Email;
            user.Token = tokenInfo.Token;
            user.TokenExpire = tokenInfo.TokenExpire;

            // Send email with confirmation link
            SendNewUserConfirmation(user.ID, tokenInfo.TokenInfoEncoded);

            // Save changes
            _users.Update(user);
            return user;
        }

        public void ApproveLegalOfficerRegistration(int id, LoggedInUserDetails LoggedInUser)
        {
            // Check whether user is not admin
            if (!LoggedInUser.IsSysAdmin)
            {
                throw new BaseException("Access denied.");
            }

            // Get user
            var user = _users.FirstOrDefault(i=>i.ID == id);

            // Check whether user is alread legal officer
            if (user.IsLegalOfficer)
            {
                throw new BaseException("Legal officer was already checked by admin.");
            }

            // Setup legal officer details
            user.IsVerifiedAsLegalOfficer = true;

            // Save changes
            _users.Update(user);
        }

        public void DeclineLegalOfficerRegistration(int id, LoggedInUserDetails LoggedInUser)
        {
            // Check whether user is admin
            if (!LoggedInUser.IsSysAdmin)
            {
                throw new BaseException("Access denied.");
            }

            // Get user
            var user = _users.FirstOrDefault(i=>i.ID == id);

            // Check whether user is alread legal officer
            if (user.IsLegalOfficer)
            {
                throw new BaseException("Legal officer was already checked by admin.");
            }

            // Setup legal officer details
            user.IsIntroducedAsLegalOfficer = false;

            // Save changes
            _users.Update(user);
        }

        public async Task<User> SaveUserCredentials(string token, string password)
        {
            // Get user for a given token
            var user = this.GetUserByEmailConfirmationToken(token);

            // Update email address
            user.UpdateEmailAddress(user.NewEmail);

            // Save changes
            _users.Update(user);

            // Check whether user marked as legal officer
            if (user.IsIntroducedAsLegalOfficer)
            {
                // Get organisation
                var organization = _organisations.FirstOrDefault(i=>i.ID==user.OrganizationID.Value);

                // Get legal officers for organisation
                var legalOfficers = _users.Where(i => i.OrganizationID == user.OrganizationID.Value)
                    .Where(i => i.IsActive == true && i.IsIntroducedAsLegalOfficer == true && i.IsVerifiedAsLegalOfficer == true);

                var userDetailsLink = _urls.ToOrganisationUsers(organization.ID);
                // Check whether legal officers found
                if (!legalOfficers.Any())
                {
                    // Get urls
                    var orgDetailsLink = _urls.ToOrganisationDetails(organization.ID);

                    // Notify admin about new organisation
                    _notifications.Admin.NewOrganizationInBackground(user.Name, organization.Name,
                        userDetailsLink, orgDetailsLink);
                }

                // Notify about new legal officer
                _notifications.Admin.NewLegalOfficerInBackground(userDetailsLink, organization.Name);
            }

            // Add user to Identity Server
            var idsUser = await _identities.CreateUser(user.Email, password, user.Name, user.Phone);

            // Map IDS user to DL user database by id
            user.UserID = idsUser.Id;

            // Save changes
            _users.Update(user);
            return user;
        }

        public User GetUserByEmailConfirmationToken(string token)
        {
            // Check whether token was not provided
            if (string.IsNullOrEmpty(token))
            {
                throw new BaseException("Invalid token");
            }

            // Parse provided token
            var tokenInfo = this._tokens.ParseEmailConfirmationToken(token);

            // Get user
            var user = _users.FirstOrDefault(i=>i.ID == tokenInfo.ReceiverId);

            // Check whether tokens does not match
            if (user.Token != tokenInfo.Token)
            {
                throw new BaseException("Access denied");
            }

            // Check whether token expired
            if (user.TokenExpire != tokenInfo.TokenExpire || user.TokenExpire < GetDate)
            {
                throw new EmailExpiredException();
            }

            return user;
        }

        public async Task<User> ChangeEmailAddress(string token)
        {
            // Check whether token not provided
            if (string.IsNullOrEmpty(token))
            {
                throw new BaseException("Invalid token");
            }

            // Decode details from token
            var data = Convert.FromBase64String(token);
            var id = BitConverter.ToInt32(data, 0);
            var expire = DateTime.FromBinary(BitConverter.ToInt64(data, 4));
            var guid = new Guid(data.Skip(12).ToArray()).ToString();

            // Get user
            var user = _users.FirstOrDefault(i => i.ID == id);

            // Check whether user tokens does not match
            if (user.Token != guid)
            {
                throw new BaseException("Access denied");
            }

            // Check whether confirmation is expired
            if (user.TokenExpire != expire || user.TokenExpire < GetDate)
            {
                throw new EmailExpiredException();

            }

            // Update login details in identity server
            await _identities.ChangeEmail(user.UserID, user.NewEmail);

            // Setup update details with new email address
            user.UpdateEmailAddress(user.NewEmail);

            // Save changes
            _users.Update(user);

            // Check whether user is marked as Legal Officer
            if (user.IsIntroducedAsLegalOfficer)
            {
                // Get organisation
                var organization = this._organisations.FirstOrDefault(i=>i.ID ==user.OrganizationID.Value);

                // Get legal officers
                var legalOfficers = _users.Where(i => i.OrganizationID == user.OrganizationID.Value).Where(i => i.IsActive == true && i.IsIntroducedAsLegalOfficer == true && i.IsVerifiedAsLegalOfficer == true);

                var userDetailsLink = _urls.ToOrganisationUsers(organization.ID);
                var orgDetailsLink = _urls.ToOrganisationDetails(organization.ID);

                // Check whether legal officers were not found
                if (!legalOfficers.Any())
                {
                    // Notify admin about new organization (legal officer registered)
                    _notifications.Admin.NewOrganizationInBackground(user.Name, organization.Name,
                        orgDetailsLink, userDetailsLink);
                }

                // Notify admin about new legal officer registration
                _notifications.Admin.NewLegalOfficerInBackground(userDetailsLink, organization.Name);
            }

            return user;
        }

        public bool CheckWhetherEmailInUse(string email, string InitialEmail)
        {
            var result = false;
            if (!string.Equals(email, InitialEmail, StringComparison.CurrentCultureIgnoreCase))
            {
                var users = _users.All();
                result = users.Any(x => String.Equals(x.Email, email, StringComparison.CurrentCultureIgnoreCase));
            }

            return result;
        }

        public User SetupNewOrganisation(UserOrganizationModel model)
        {
            // Check whether email address is in a valid format
            try
            {
                new MailAddress(model.Email);
            }
            catch (FormatException)
            {
                throw new BaseException("Invalid email format.");
            }

            // Check whether email address already in use
            if (CheckWhetherEmailInUse(model.Email, string.Empty))
            {
                throw new BaseException("Email already in use.");
            }

            // Check whether required fields are present
            if (string.IsNullOrEmpty(model.OrganizationName) || string.IsNullOrEmpty(model.AdministrativeContact) ||
                string.IsNullOrEmpty(model.AdministrativePhone))
            {
                throw new BaseException("User's email address, organization name, administrative contact and administrative phone fields are required!");
            }

            // Check whether organisation name already in Use
            if (_organisations.All().Any(x => x.Name == model.OrganizationName))
            {
                throw new BaseException("Organization name already in use.");
            }

            // Chekc whether T&C ticked
            if (!model.IsAgreeWithTerms)
            {
                throw new BaseException("New user should agree with terms of service.");
            }

            // Setup new organisation details
            var newOrganization = new Organization
            {
                Name = model.OrganizationName,
                AdministrativeContact = model.AdministrativeContact,
                AdministrativePhone = model.AdministrativePhone,
                IsAgreeWithTerms = model.IsAgreeWithTerms,
                Address = model.OrganizationAddress,
                Phone = model.OrganizationPhone,
                TermsOfService = model.TermsOfService,
                CreatedAt = GetDate,
                IsActive = false
            };

            // Save changes
            this._organisations.Add(newOrganization);

            // Setup user details
            var newUser = new User
            {
                Name = model.Name,
                Email = model.Email,
                NewEmail = model.Email,
                IsActive = false,
                IsSysAdmin = false,
                OrganizationID = newOrganization.ID,
                Phone = model.Phone
            };

            // Save user details
            _users.Add(newUser);

            // Setup token to validate email address
            var tokenInfo = _tokens.GenerateEmailConfirmationToken(newUser.ID);
            newUser.Token = tokenInfo.Token;
            newUser.TokenExpire = tokenInfo.TokenExpire;

            // Send token to email address
            SendNewUserConfirmation(newUser.ID, tokenInfo.TokenInfoEncoded);

            // Save token details to user details
            _users.Update(newUser);

            return newUser;
        }

        public void UpdateStatus(int id, bool value, LoggedInUserDetails loggedInUser)
        {
            // Get user
            var user = _users.FirstOrDefault(i => i.ID == id);

            // Check whether user has no access to organisation
            if (!loggedInUser.IsSysAdmin)
            {
                if (loggedInUser.Organization.ID != user.OrganizationID.Value || value)
                {
                    throw new BaseException("Access denied.");
                }
            }

            // Setup status value
            var state = value ? ActivationState.Active : ActivationState.NotActive;

            // Notify user about status change
            _notifications.User.UpdatedAccountStateInBackground(user.ID, state);

            // Setup update details
            user.IsActive = value;

            // Save changes
            _users.Update(user);

            // Get organisation
            var organization = _organisations.FirstOrDefault(i=>i.ID == user.OrganizationID.Value);

            // Deactivate organisation if legal officer has been deactivated
            DeactivateOrganizationIfNoLegalOfficers(organization);
        }

        public List<UserDetailsModel> GetOrganisationUsers(int organizationId, bool includeInActive, LoggedInUserDetails user)
        {
            // Check whether user has access to organisation
            if (!user.IsSysAdmin && user.Organization.ID != organizationId)
            {
                throw new BaseException("Access denied");
            }

            // Get users
            var users = _users.Where(i => i.OrganizationID == organizationId);

            // Filter active users
            if (!includeInActive)
            {
                users = users.Where(i => i.IsActive == true);
            }

            var organization = _organisations.FirstOrDefault(i=>i.ID == organizationId);
            var legalOfficers = _users.Where(i => i.OrganizationID == organization.ID).Where(i => i.IsActive == true && i.IsIntroducedAsLegalOfficer == true && i.IsVerifiedAsLegalOfficer == true).ToList();

            // Setup result model
            var result = new List<UserDetailsModel>();
            foreach (var item in users)
            {
                var isSingleLegalOfficer = legalOfficers.Count == 1 && legalOfficers.First().ID == item.ID;
                var userModel = item.ToUserModel();
                userModel.OrganizationName = organization.Name;
                userModel.IsSingleLegalOfficer = isSingleLegalOfficer;
                // Add user to result model
                result.Add(userModel);
            }

            // Return result
            return result;
        }

        public UserDetailsModel GetUserModel(int userId, LoggedInUserDetails user)
        {
            // Check whether user has access
            if (!user.IsSysAdmin && userId != user.ID)
            {
                throw new BaseException("Access denied.");
            }

            // Get user
            var data = _users.FirstOrDefault(i=>i.ID == userId);

            // Get organization
            var organization = _organisations.FirstOrDefault(i=>i.ID == data.OrganizationID);

            // Get legal officers
            var legalOfficers = _users.Where(i => i.OrganizationID == organization.ID)
                .Where(i => i.IsActive == true && i.IsIntroducedAsLegalOfficer == true && i.IsVerifiedAsLegalOfficer == true)
                .ToList();

            // Check if this user is a single legal officer
            var isSingleLegalOfficer = legalOfficers.Count == 1 &&
                                       legalOfficers.FirstOrDefault(i => i.ID == data.ID) != null;
            
            // Setup model
            var model = data.ToUserModel();
            model.OrganizationName = organization.Name;
            model.IsSingleLegalOfficer = isSingleLegalOfficer;

            // Return result
            return model;
        }

        public async Task<string> EditUserDetails(int userId, UserDetailsModel model, LoggedInUserDetails loggedInUser)
        {
            // Get user
            var user = _users.FirstOrDefault(i=>i.ID == userId);
            var organization = this._organisations.FirstOrDefault(i=>i.ID == user.OrganizationID.Value);

            // Check if user has access
            if (!loggedInUser.IsSysAdmin && loggedInUser.ID != userId)
            {
                throw new BaseException("Access denied.");
            }

            // Setup update details
            if (loggedInUser.IsSysAdmin)
            {
                // Admin can activate user
                user.IsActive = model.IsActive;
            }

            user.Phone = model.Phone;
            user.Name = model.Name;

            // Email updating
            try
            {
                new MailAddress(model.Email);
            }
            catch (FormatException)
            {
                throw new BaseException("Invalid email format.");
            }
            var additionalMsg = string.Empty;

            // User has changed email
            if (model.Email != user.Email)
            {
                additionalMsg = ProcessChangeEmailRequest(model, user);
            }

            // If Legal officer changed into true from false and was not verified before
            if (model.IsIntroducedAsLegalOfficer && !user.IsIntroducedAsLegalOfficer && !user.IsVerifiedAsLegalOfficer)
            {
                additionalMsg = ProcessLegalOfficerRequest(user, organization);
            }

            // Update user details in IdentityServer
            await _identities.EditUser(user.UserID, user.Name, user.Phone, true);

            user.IsIntroducedAsLegalOfficer = model.IsIntroducedAsLegalOfficer;

            // Save changes
            _users.Update(user);

            // Deactivate organization if no legal officers registered
            DeactivateOrganizationIfNoLegalOfficers(organization);
            return additionalMsg;
        }

        private void DeactivateOrganizationIfNoLegalOfficers(Organization organization)
        {
            var users = _users.Where(i => i.OrganizationID == organization.ID).Where(i => i.IsActive == true);
            var isNoLegalOfficers = users.All(u => u.IsLegalOfficer != true);
            if (organization.IsActive && isNoLegalOfficers)
            {
                organization.IsActive = false;
                _organisations.Update(organization);
            }
        }

        private string ProcessLegalOfficerRequest(User user, Organization organization)
        {
            var legalOfficers = _users.Where(i => i.OrganizationID == user.OrganizationID.Value).Where(i => i.IsActive == true && i.IsIntroducedAsLegalOfficer == true && i.IsVerifiedAsLegalOfficer == true);

            var userDetailsLink = _urls.ToOrganisationUsers(organization.ID);
            if (!legalOfficers.Any())
            {
                // Notify admin about this
                var orgDetailsLink = _urls.ToOrganisationDetails(organization.ID);

                _notifications.Admin.NewOrganizationInBackground(user.Name, organization.Name,
                    userDetailsLink, orgDetailsLink);
            }
            else
            {
                this._notifications.Admin.NewLegalOfficerInBackground(userDetailsLink, organization.Name);
            }
            return " 'Legal Officer': Datalinker will check this with your organization and notify you once the check is complete.";
        }

        private string ProcessChangeEmailRequest(UserDetailsModel model, User user)
        {
            if (CheckWhetherEmailInUse(model.Email,user.Email))
            {
                throw new BaseException("Email already in use.");
            }

            var tokenInfo = _tokens.GenerateEmailConfirmationToken(user.ID);
            user.Token = tokenInfo.Token;
            user.TokenExpire = tokenInfo.TokenExpire;
            user.NewEmail = model.Email;
            SendChangeEmailConfirmation(user.ID, tokenInfo.TokenInfoEncoded);

            return " 'Change Email': Activation link was sent to your new email. It will expire soon.";
        }
        
        private void SendNewUserConfirmation(int userId, string token)
        {
            var url = _urls.ToSetupUserCredentials(token);
            _notifications.User.EmailVerificationInBackground(userId, url);
        }

        private void SendNewUserInvitation(int userId, string token, int inviterID)
        {
            var url = _urls.ToSetupUserCredentials(token);
            _notifications.User.EmailInvitationInBackground(userId, url, inviterID);
        }
        
        private void SendChangeEmailConfirmation(int userId, string token)
        {
            var url = _urls.ToConfirmEmailAddress(token);
            _notifications.User.EmailVerificationInBackground(userId, url);
        }
    }
}
