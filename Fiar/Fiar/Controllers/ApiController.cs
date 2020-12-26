﻿using Fiar.Attributes;
using Ixs.DNA;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fiar
{
    /// <summary>
    /// Manages the Web API calls
    /// </summary>
    [AuthorizeToken]
    [ApiController]
    public class ApiController : Controller
    {
        #region Protected Members

        /// <summary>
        /// The scoped Application context
        /// </summary>
        protected ApplicationDbContext mContext;

        /// <summary>
        /// The manager for handling user creation, deletion, searching, roles, etc.
        /// </summary>
        protected UserManager<ApplicationUser> mUserManager;

        /// <summary>
        /// The manager for handling signing in and out for our users
        /// </summary>
        protected SignInManager<ApplicationUser> mSignInManager;

        /// <summary>
        /// The manager for handling user roles
        /// </summary>
        protected RoleManager<IdentityRole> mRoleManager;

        /// <summary>
        /// Injection - <inheritdoc cref="ILogger"/>
        /// </summary>
        private readonly ILogger mLogger;

        /// <summary>
        /// Injection - <inheritdoc cref="IConfigBox"/>
        /// </summary>
        private readonly IConfigBox mConfigBox;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="context">The injected context</param>
        /// <param name="userManager">The Identity user manager</param>
        /// <param name="signInManager">The Identity sign in manager</param>
        /// <param name="roleManager">The Identity role manager</param>
        public ApiController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager, IConfigBox configBox)
        {
            mContext = context;
            mUserManager = userManager;
            mSignInManager = signInManager;
            mRoleManager = roleManager;
            mConfigBox = configBox ?? throw new ArgumentNullException(nameof(configBox));
            mLogger = FrameworkDI.Logger ?? throw new ArgumentNullException(nameof(mLogger));
        }

        #endregion

        #region Login / Register / Verify

        /// <summary>
        /// Logs in a user using tkone-based authentication
        /// </summary>
        /// <param name="loginCredentials">Returns the result of the login request</param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [Route(ApiRoutes.Login)]
        public async Task<ApiResponse<Result_UserProfileDetailsApiModel>> LoginAsync([FromBody] Put_LoginCredentialsApiModel loginCredentials)
        {
            // The error message to user
            var errorResponse = new ApiResponse<Result_UserProfileDetailsApiModel>
            {
                // Set error message
                ErrorMessage = Localization.Resource.ApiController_Login_InvalidCredentialsErrorMsg,
            };

            // Make sure we have a user name
            if (loginCredentials?.UsernameOrEmail == null || string.IsNullOrWhiteSpace(loginCredentials.UsernameOrEmail))
                // Return error response
                return errorResponse;

            // Validate if the user credentials are correct

            // Is it an email?
            var isEmail = loginCredentials.UsernameOrEmail.Contains("@");
            // Get the user details
            var user = isEmail
                // Find by email
                ? await mUserManager.FindByEmailAsync(loginCredentials.UsernameOrEmail)
                // Find by username
                : await mUserManager.FindByNameAsync(loginCredentials.UsernameOrEmail);

            // If we failed to find a user...
            if (user == null)
                // Return error response
                return errorResponse;

            // If we got here we have a user...
            // Let's validate the password

            // Get if password is valid
            var isValidPassword = await mUserManager.CheckPasswordAsync(user, loginCredentials.Password);

            // If the password was wrong
            if (!isValidPassword)
                // Return error response
                return errorResponse;

            // If we get here, we are valid and the user passed the correct login details

            // Get user roles
            var userRoles = await mUserManager.GetRolesAsync(user);

            // Return token to user
            return new ApiResponse<Result_UserProfileDetailsApiModel>
            {
                // Pass back the user details and the token
                Response = new Result_UserProfileDetailsApiModel
                {
                    Token = user.GenerateJwtToken(userRoles),
                    Model = UserDataModel.Convert(user)
                }
            };
        }

        /// <summary>
        /// Tries to register for a new account on the server
        /// </summary>
        /// <param name="registerCredentials">The registration details</param>
        /// <returns>Returns the result of the register request</returns>
        [HttpPost]
        [AllowAnonymous]
        [Route(ApiRoutes.Register)]
        public async Task<ApiResponse> RegisterAsync([FromBody] Put_RegisterCredentialsApiModel registerCredentials)
        {
            // The error message to user
            var errorResponse = new ApiResponse { ErrorMessage = Localization.Resource.ApiController_ValidationErrorMsg };

            // If we have no credentials...
            if (registerCredentials == null)
                // Return failed response
                return errorResponse;

            // Validate user
            if (!typeof(UserDataModel).GetPropertyAttribute<ValidateStringAttribute>(nameof(UserDataModel.Username)).Validate(registerCredentials.Username).Succeeded ||
                !typeof(UserDataModel).GetPropertyAttribute<ValidateStringAttribute>(nameof(UserDataModel.Email)).Validate(registerCredentials.Email).Succeeded ||
                registerCredentials.Password == null)
                // Return failed response
                return errorResponse;

            // Create the desired user from the given details
            var newUser = new ApplicationUser
            {
                // USER-SPEC-HERE (specific for registration)
                UserName = registerCredentials.Username,
                Email = registerCredentials.Email,
                Nickname = registerCredentials.Username,
            };

            // Try and create a user
            var result = await mUserManager.CreateAsync(newUser, registerCredentials.Password);

            // If the registration was successful...
            if (result.Succeeded)
            {
                // Add the user to desired role
                await mUserManager.AddToRoleAsync(newUser, RoleNames.Player);

                // Generate an email verification code
                var emailVerificationCode = mUserManager.GenerateEmailConfirmationTokenAsync(newUser);

                // TODO: Email the user the verification code (api-side) https://youtu.be/iYFP26_zI98?t=2407

                // Return valid response
                return new ApiResponse();
            }
            // Otherwise if it failed...
            else
            {
                // Return the failed response
                return new ApiResponse
                {
                    // Aggregate all erros into a single error string
                    ErrorMessage = result.Errors.AggregateErrors()
                };
            }
        }

        #endregion

        #region User Profile

        /// <summary>
        /// Returns the users profile details based on the authenticated user
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.GetUserProfile)]
        public async Task<ApiResponse<Result_UserProfileDetailsApiModel>> GetUserProfileAsync([FromBody] Get_UserProfileDetailsApiModel model)
        {
            #region Get/Check User

            // Get user by the claims
            var user = await mUserManager.GetUserAsync(HttpContext.User);
            // If user does not have authorization...
            if (!await AuthorizeUserAsync(user, PolicyNames.PlayerLevel))
                // Return user auth error response
                return GetAuthorizationErrorApiResponse<Result_UserProfileDetailsApiModel>(Localization.Resource.AuthError_UserNotFound);

            #endregion

            // The error message to user
            var errorResponse = new ApiResponse<Result_UserProfileDetailsApiModel> { ErrorMessage = Localization.Resource.ApiController_ValidationErrorMsg };
            // If we have no model...
            if (model == null)
                // Return failed response
                return errorResponse;

            // Create the user model
            var resultUser = UserDataModel.Convert(user);
            if (model.IncludeRoleList)
                // Get user roles
                resultUser.RoleList = (await mUserManager.GetRolesAsync(user)).ToList();

            // Return user details to user
            return new ApiResponse<Result_UserProfileDetailsApiModel>
            {
                // Pass back the user details
                Response = new Result_UserProfileDetailsApiModel
                {
                    Model = resultUser
                }
            };
        }

        /// <summary>
        /// Get list of user roles
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.GetUserRoles)]
        public async Task<ApiResponse<Result_UserRolesApiModel>> GetUserRolesAsync()
        {
            #region Get/Check User

            // Get user by the claims
            var user = await mUserManager.GetUserAsync(HttpContext.User);
            // If user does not have authorization...
            if (!await AuthorizeUserAsync(user, PolicyNames.PlayerLevel))
                // Return user auth error response
                return GetAuthorizationErrorApiResponse<Result_UserRolesApiModel>(Localization.Resource.AuthError_UserNotFound);

            #endregion

            // Get user roles
            var roleList = await mUserManager.GetRolesAsync(user);

            // Return user roles
            return new ApiResponse<Result_UserRolesApiModel>
            {
                // Pass back the user role list
                Response = new Result_UserRolesApiModel
                {
                    RoleList = roleList.ToList()
                }
            };
        }

        #endregion

        #region Private Helpers

        /// <summary>
        /// Authorize user by policy
        /// </summary>
        /// <param name="user">The user</param>
        /// <param name="policyName">The policy name identifier</param>
        /// <returns>TRUE = authorized, FALSE = otherwise</returns>
        private async Task<bool> AuthorizeUserAsync(ApplicationUser user, string policyName)
        {
            // User does not exist...
            if (user == null)
                return false;

            // Get user roles
            var userRoles = await mUserManager.GetRolesAsync(user);
            // Go thourgh user roles to find a match...
            foreach (var role in userRoles)
                // If user has authorization according to policy...
                if (Policies.Dict[policyName].Contains(role))
                    return true;

            return false;
        }

        /// <summary>
        /// Get api response with authorization error indicator
        /// </summary>
        /// <param name="message">Custom response error message</param>
        /// <returns>Api model response</returns>
        private ApiResponse GetAuthorizationErrorApiResponse(string message)
        {
            // Return error response with raised authorization error
            return new ApiResponse()
            {
                HasUnauthorizedUserError = true,
                ErrorMessage = message
            };
        }

        /// <summary>
        /// Get api response with authorization error indicator
        /// </summary>
        /// <typeparam name="T">Return type for api model</typeparam>
        /// <param name="message">Custom response error message</param>
        /// <returns>Api model response</returns>
        private ApiResponse<T> GetAuthorizationErrorApiResponse<T>(string message)
        {
            // Return error response with raised authorization error
            return new ApiResponse<T>()
            {
                HasUnauthorizedUserError = true,
                ErrorMessage = message
            };
        }

        /// <summary>
        /// Sends the given user a new verify email link
        /// </summary>
        /// <param name="user">The user to send the link to</param>
        /// <returns></returns>
        private async Task SendUserEmailVerificationAsync(ApplicationUser user)
        {
            await Task.Delay(1);
            // TODO: Send email verif wrap https://youtu.be/W2y32Kp2e4E?t=2022
        }

        /// <summary>
        /// Creates simple passowrd
        /// </summary>
        /// <param name="length">Length of the passowrd</param>
        /// <returns>Generated password</returns>
        private string GenerateSimplePassword(int length)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();
        }

        #endregion
    }
}