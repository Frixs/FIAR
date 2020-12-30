using Fiar.Attributes;
using Ixs.DNA;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fiar.ViewModels;

namespace Fiar
{
    /// <summary>
    /// Manages the Web API calls
    /// </summary>
    //[AuthorizeToken] // Allow only to JWT auth
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
        /// The manager for handling memory cache
        /// </summary>
        protected IMemoryCache mMemoryCache;

        /// <summary>
        /// Injection - <inheritdoc cref="ILogger"/>
        /// </summary>
        protected readonly ILogger mLogger;

        /// <summary>
        /// Injection - <inheritdoc cref="IConfigBox"/>
        /// </summary>
        protected readonly IConfigBox mConfigBox;

        /// <summary>
        /// Injection - <inheritdoc cref="IRepository<GameSession>"/>
        /// </summary>
        protected readonly IRepository<GameSession> mGameRepository;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="context">The injected context</param>
        /// <param name="userManager">The Identity user manager</param>
        /// <param name="signInManager">The Identity sign in manager</param>
        /// <param name="roleManager">The Identity role manager</param>
        public ApiController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager, IMemoryCache memoryCache, ILogger logger, IConfigBox configBox, IRepository<GameSession> gameRepository)
        {
            mContext = context;
            mUserManager = userManager;
            mSignInManager = signInManager;
            mRoleManager = roleManager;
            mMemoryCache = memoryCache;
            mConfigBox = configBox ?? throw new ArgumentNullException(nameof(configBox));
            mGameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository));
            mLogger = logger ?? throw new ArgumentNullException(nameof(logger));
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

            // Get online status
            Dictionary<string, DateTime> loggedInUsers;
            if (!mMemoryCache.TryGetValue(MemoryCacheIdentifiers.LoggedInUsers, out loggedInUsers))
                loggedInUsers = new Dictionary<string, DateTime>();

            // Create the user model
            var resultUser = UserDataModel.Convert(user);
            
            // Include additional data
            await IncludeAdditionalDataAsync(resultUser, user, model.IncludeRoleList, model.IncludeOnlineStatus, loggedInUsers, model.IncludeIsFriendWith, model.IncludeIsChallanged, model.IncludeIsPlaying);

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

        /// <summary>
        /// Returns the users profile details based on the authenticated user
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.GetUserFriendProfiles)]
        public async Task<ApiResponse<Result_UserFriendProfilesApiModel>> GetUserFriendProfilesAsync([FromBody] Get_UserProfileDetailsApiModel model)
        {
            #region Get/Check User

            // Get user by the claims
            var user = await mUserManager.GetUserAsync(HttpContext.User);
            // If user does not have authorization...
            if (!await AuthorizeUserAsync(user, PolicyNames.PlayerLevel))
                // Return user auth error response
                return GetAuthorizationErrorApiResponse<Result_UserFriendProfilesApiModel>(Localization.Resource.AuthError_UserNotFound);

            #endregion

            // The error message to user
            var errorResponse = new ApiResponse<Result_UserFriendProfilesApiModel> { ErrorMessage = Localization.Resource.ApiController_ValidationErrorMsg };
            // If we have no model...
            if (model == null)
                // Return failed response
                return errorResponse;

            // Get online status
            Dictionary<string, DateTime> loggedInUsers;
            if (!mMemoryCache.TryGetValue(MemoryCacheIdentifiers.LoggedInUsers, out loggedInUsers))
                loggedInUsers = new Dictionary<string, DateTime>();

            // Get all friends of the curretnly logged in user
            var friends = mContext.FriendUserRelations
                .Where(o => o.UserId.Equals(user.Id))
                .Include(o => o.FriendUser)
                .Select(o => UserDataModel.Convert(o.FriendUser))
                .ToList().OrderBy(o => o.Nickname)
                .ToList();

            // Set additional user data
            foreach (var u in friends)
                await IncludeAdditionalDataAsync(u, user, model.IncludeRoleList, model.IncludeOnlineStatus, loggedInUsers, model.IncludeIsFriendWith, model.IncludeIsChallanged, model.IncludeIsPlaying);

            // Return successful response
            return new ApiResponse<Result_UserFriendProfilesApiModel>
            {
                // Pass back the user details
                Response = new Result_UserFriendProfilesApiModel
                {
                    FriendModels = friends
                }
            };
        }

        /// <summary>
        /// Returns the users profile details for all users
        /// </summary>
        [HttpPost]
        [Route(ApiRoutes.GetAllUserProfiles)]
        public async Task<ApiResponse<Result_AllUserProfilesApiModel>> GetAllUserProfilesAsync([FromBody] Get_UserProfileDetailsApiModel model)
        {
            #region Get/Check User

            // Get user by the claims
            var user = await mUserManager.GetUserAsync(HttpContext.User);
            // If user does not have authorization...
            if (!await AuthorizeUserAsync(user, PolicyNames.PlayerLevel))
                // Return user auth error response
                return GetAuthorizationErrorApiResponse<Result_AllUserProfilesApiModel>(Localization.Resource.AuthError_UserNotFound);

            #endregion

            // The error message to user
            var errorResponse = new ApiResponse<Result_AllUserProfilesApiModel> { ErrorMessage = Localization.Resource.ApiController_ValidationErrorMsg };
            // If we have no model...
            if (model == null)
                // Return failed response
                return errorResponse;

            // Get online status
            Dictionary<string, DateTime> loggedInUsers;
            if (!mMemoryCache.TryGetValue(MemoryCacheIdentifiers.LoggedInUsers, out loggedInUsers))
                loggedInUsers = new Dictionary<string, DateTime>();

            // Get all users
            List<UserDataModel> users = mContext.Users
                .OrderBy(o => o.Nickname)
                .Select(o => UserDataModel.Convert(o))
                .ToList();

            // Set additional user data
            foreach (var u in users)
                await IncludeAdditionalDataAsync(u, user, model.IncludeRoleList, model.IncludeOnlineStatus, loggedInUsers, model.IncludeIsFriendWith, model.IncludeIsChallanged, model.IncludeIsPlaying);

            // Return successful response
            return new ApiResponse<Result_AllUserProfilesApiModel>
            {
                // Pass back the user details
                Response = new Result_AllUserProfilesApiModel
                {
                    UserModels = users
                }
            };
        }

        /// <summary>
        /// Returns the users profile details for all online users
        /// </summary>
        [HttpPost]
        [Route(ApiRoutes.GetOnlineUserProfiles)]
        public async Task<ApiResponse<Result_OnlineUserProfilesApiModel>> GetOnlineUserProfilesAsync([FromBody] Get_UserProfileDetailsApiModel model)
        {
            #region Get/Check User

            // Get user by the claims
            var user = await mUserManager.GetUserAsync(HttpContext.User);
            // If user does not have authorization...
            if (!await AuthorizeUserAsync(user, PolicyNames.PlayerLevel))
                // Return user auth error response
                return GetAuthorizationErrorApiResponse<Result_OnlineUserProfilesApiModel>(Localization.Resource.AuthError_UserNotFound);

            #endregion

            // The error message to user
            var errorResponse = new ApiResponse<Result_OnlineUserProfilesApiModel> { ErrorMessage = Localization.Resource.ApiController_ValidationErrorMsg };
            // If we have no model...
            if (model == null)
                // Return failed response
                return errorResponse;

            // Get online status
            Dictionary<string, DateTime> loggedInUsers;
            if (!mMemoryCache.TryGetValue(MemoryCacheIdentifiers.LoggedInUsers, out loggedInUsers))
                loggedInUsers = new Dictionary<string, DateTime>();

            // Get all users
            List<UserDataModel> users = mContext.Users
                .OrderBy(o => o.Nickname)
                .Select(o => UserDataModel.Convert(o))
                .ToList();

            List<UserDataModel> resultUsers = new List<UserDataModel>();
            // Set additional user data
            foreach (var u in users)
            {
                await IncludeAdditionalDataAsync(u, user, model.IncludeRoleList, model.IncludeOnlineStatus, loggedInUsers, model.IncludeIsFriendWith, model.IncludeIsChallanged, model.IncludeIsPlaying);
                if (loggedInUsers.ContainsKey(u.Username))
                    resultUsers.Add(u);
            }

            // Return successful response
            return new ApiResponse<Result_OnlineUserProfilesApiModel>
            {
                // Pass back the user details
                Response = new Result_OnlineUserProfilesApiModel
                {
                    UserModels = resultUsers
                }
            };
        }

        /// <summary>
        /// Returns the user requests based on the authenticated user
        /// </summary>
        [HttpPost]
        [Route(ApiRoutes.GetUserRequests)]
        public async Task<ApiResponse<Result_UserRequestsApiModel>> GetUserRequestsAsync([FromBody] Get_UserProfileDetailsApiModel model)
        {
            #region Get/Check User

            // Get user by the claims
            var user = await mUserManager.GetUserAsync(HttpContext.User);
            // If user does not have authorization...
            if (!await AuthorizeUserAsync(user, PolicyNames.PlayerLevel))
                // Return user auth error response
                return GetAuthorizationErrorApiResponse<Result_UserRequestsApiModel>(Localization.Resource.AuthError_UserNotFound);

            #endregion

            // The error message to user
            var errorResponse = new ApiResponse<Result_UserRequestsApiModel> { ErrorMessage = Localization.Resource.ApiController_ValidationErrorMsg };
            // If we have no model...
            if (model == null)
                // Return failed response
                return errorResponse;

            // Get online status
            Dictionary<string, DateTime> loggedInUsers;
            if (!mMemoryCache.TryGetValue(MemoryCacheIdentifiers.LoggedInUsers, out loggedInUsers))
                loggedInUsers = new Dictionary<string, DateTime>();

            // Get all requests of the curretnly logged in user
            var requests = mContext.UserRequests
                .Where(o => o.RelatedUserId.Equals(user.Id))
                .Include(o => o.User)
                .OrderByDescending(o => o.CreatedAt)
                .ToList();

            List<UserRequestViewModel> resultRequests = new List<UserRequestViewModel>();
            // Set additional user data
            foreach (var req in requests)
            {
                var u = UserDataModel.Convert(req.User);
                await IncludeAdditionalDataAsync(u, user, model.IncludeRoleList, model.IncludeOnlineStatus, loggedInUsers, model.IncludeIsFriendWith, model.IncludeIsChallanged, model.IncludeIsPlaying);
                resultRequests.Add(new UserRequestViewModel
                { 
                    Id = req.Id,
                    Type = req.Type,
                    User = u
                });
            }

            // Return user details to user
            return new ApiResponse<Result_UserRequestsApiModel>
            {
                // Pass back the user details
                Response = new Result_UserRequestsApiModel
                {
                    RequestModels = resultRequests
                }
            };
        }

        /// <summary>
        /// Adds a new user request
        /// </summary>
        [HttpPost]
        [Route(ApiRoutes.AddUserRequest)]
        public async Task<ApiResponse> AddUserRequestAsync([FromBody] Add_UserRequestApiModel model)
        {
            #region Get/Check User

            // Get user by the claims
            var user = await mUserManager.GetUserAsync(HttpContext.User);
            // If user does not have authorization...
            if (!await AuthorizeUserAsync(user, PolicyNames.PlayerLevel))
                // Return user auth error response
                return GetAuthorizationErrorApiResponse(Localization.Resource.AuthError_UserNotFound);

            #endregion

            // The error message to user
            var errorResponse = new ApiResponse { ErrorMessage = Localization.Resource.ApiController_ValidationErrorMsg };
            // If we have no model...
            if (model == null)
                // Return failed response
                return errorResponse;

            // Check the user exists
            if (mContext.Users.Any(o => o.Id.Equals(model.RelatedUserId)) 
                && !mContext.UserRequests.Any(o => o.Type == model.Type && o.UserId.Equals(user.Id) && o.RelatedUserId.Equals(model.RelatedUserId))
                )
            {
                // Add
                mContext.UserRequests.Add(new UserRequestDataModel
                {
                    UserId = user.Id,
                    RelatedUserId = model.RelatedUserId,
                    Type = model.Type,
                    CreatedAt = DateTime.Now
                });

                // Save changes
                var affectedRows = mContext.SaveChanges();
                if (affectedRows > 0)
                    // Return successful response
                    return new ApiResponse();
            }

            // Failed
            return new ApiResponse { ErrorMessage = Localization.Resource.ApiController_AddUserRequest_FailedErrorMsg };
        }

        /// <summary>
        /// Removes the user request
        /// </summary>
        [HttpPost]
        [Route(ApiRoutes.DeleteUserRequest)]
        public async Task<ApiResponse> DeleteUserRequestAsync([FromBody] Get_UserRequestApiModel model)
        {
            #region Get/Check User

            // Get user by the claims
            var user = await mUserManager.GetUserAsync(HttpContext.User);
            // If user does not have authorization...
            if (!await AuthorizeUserAsync(user, PolicyNames.PlayerLevel))
                // Return user auth error response
                return GetAuthorizationErrorApiResponse(Localization.Resource.AuthError_UserNotFound);

            #endregion

            // The error message to user
            var errorResponse = new ApiResponse { ErrorMessage = Localization.Resource.ApiController_ValidationErrorMsg };
            // If we have no model...
            if (model == null)
                // Return failed response
                return errorResponse;

            var item = mContext.UserRequests.FirstOrDefault(o => o.Id == model.Id && o.RelatedUserId.Equals(user.Id));
            // Check the item exists
            if (item != null)
            {
                // Delete
                mContext.UserRequests.Remove(item);

                // Save changes
                var affectedRows = mContext.SaveChanges();
                if (affectedRows > 0)
                    // Return successful response
                    return new ApiResponse();
            }

            // Failed
            return new ApiResponse { ErrorMessage = Localization.Resource.ApiController_DeleteUserRequest_FailedErrorMsg };
        }

        /// <summary>
        /// Adds a new user friend to the currently authenticated user
        /// </summary>
        [HttpPost]
        [Route(ApiRoutes.AddUserFriendByRequest)]
        public async Task<ApiResponse> AddUserFriendByRequestAsync([FromBody] Add_UserFriendByRequestApiModel model)
        {
            #region Get/Check User

            // Get user by the claims
            var user = await mUserManager.GetUserAsync(HttpContext.User);
            // If user does not have authorization...
            if (!await AuthorizeUserAsync(user, PolicyNames.PlayerLevel))
                // Return user auth error response
                return GetAuthorizationErrorApiResponse(Localization.Resource.AuthError_UserNotFound);

            #endregion

            // The error message to user
            var errorResponse = new ApiResponse { ErrorMessage = Localization.Resource.ApiController_ValidationErrorMsg };
            // If we have no model...
            if (model == null)
                // Return failed response
                return errorResponse;

            var item = mContext.UserRequests.FirstOrDefault(o => o.Id == model.Id && o.RelatedUserId.Equals(user.Id));
            // Check the user exists
            if (item != null
                && !mContext.FriendUserRelations.Any(o => o.UserId.Equals(model.FriendUserId) && o.FriendUserId.Equals(user.Id))
                )
            {
                // Add
                mContext.FriendUserRelations.Add(new FriendUserRelationDataModel
                {
                    UserId = model.FriendUserId,
                    FriendUserId = user.Id
                });
                // Remove the request
                mContext.Remove(item);

                // Save changes
                var affectedRows = mContext.SaveChanges();
                if (affectedRows > 1)
                    // Return successful response
                    return new ApiResponse();
            }

            // Failed
            return new ApiResponse { ErrorMessage = Localization.Resource.ApiController_AddUserFriendByRequest_FailedErrorMsg };
        }

        /// <summary>
        /// Removes a user friend to the currently authenticated user
        /// </summary>
        [HttpPost]
        [Route(ApiRoutes.DeleteUserFriend)]
        public async Task<ApiResponse> DeleteUserFriendAsync([FromBody] Get_UserFriendApiModel model)
        {
            #region Get/Check User

            // Get user by the claims
            var user = await mUserManager.GetUserAsync(HttpContext.User);
            // If user does not have authorization...
            if (!await AuthorizeUserAsync(user, PolicyNames.PlayerLevel))
                // Return user auth error response
                return GetAuthorizationErrorApiResponse(Localization.Resource.AuthError_UserNotFound);

            #endregion

            // The error message to user
            var errorResponse = new ApiResponse { ErrorMessage = Localization.Resource.ApiController_ValidationErrorMsg };
            // If we have no model...
            if (model == null)
                // Return failed response
                return errorResponse;

            var item = mContext.FriendUserRelations.FirstOrDefault(o => o.UserId == user.Id && o.FriendUserId.Equals(model.FriendUserId));
            // Check the item exists
            if (item != null)
            {
                // Delete
                mContext.FriendUserRelations.Remove(item);

                // Save changes
                var affectedRows = mContext.SaveChanges();
                if (affectedRows > 0)
                    // Return successful response
                    return new ApiResponse();
            }

            // Failed
            return new ApiResponse { ErrorMessage = Localization.Resource.ApiController_DeleteUserFriend_FailedErrorMsg };
        }

        #endregion

        #region Game

        /// <summary>
        /// Adds a new game friend to the currently authenticated user
        /// </summary>
        [HttpPost]
        [Route(ApiRoutes.AddGameByRequest)]
        public async Task<ApiResponse> AddGameByRequestAsync([FromBody] Add_GameByRequestApiModel model)
        {
            #region Get/Check User

            // Get user by the claims
            var user = await mUserManager.GetUserAsync(HttpContext.User);
            // If user does not have authorization...
            if (!await AuthorizeUserAsync(user, PolicyNames.PlayerLevel))
                // Return user auth error response
                return GetAuthorizationErrorApiResponse(Localization.Resource.AuthError_UserNotFound);

            #endregion

            // The error message to user
            var errorResponse = new ApiResponse { ErrorMessage = Localization.Resource.ApiController_ValidationErrorMsg };
            // If we have no model...
            if (model == null)
                // Return failed response
                return errorResponse;

            //TODO ---
            var acceptReq = mContext.UserRequests.FirstOrDefault(o => o.Id == model.Id && o.UserId.Equals(model.OpponentUserId) && o.RelatedUserId.Equals(user.Id));
            // Check the request exists
            if (acceptReq != null)
            {
                // Remove the request
                mContext.UserRequests.Remove(acceptReq);

                // Now, check for any desync challange related requests between these two users and delete them, if any
                var opponentAcceptReqs = mContext.UserRequests.Where(o => o.Type == UserRequestType.AcceptChallange && o.UserId.Equals(user.Id) && o.RelatedUserId.Equals(model.OpponentUserId));
                if (opponentAcceptReqs.Count() > 0)
                    mContext.UserRequests.RemoveRange(opponentAcceptReqs);
                var challangeReqs = mContext.UserRequests.Where(o => o.Type == UserRequestType.Challange && ((o.UserId.Equals(user.Id) && o.RelatedUserId.Equals(model.OpponentUserId)) || (o.UserId.Equals(model.OpponentUserId) && o.RelatedUserId.Equals(user.Id))));
                if (challangeReqs.Count() > 0)
                    mContext.UserRequests.RemoveRange(challangeReqs);

                // Save changes
                mContext.SaveChanges();
                
                // Add the game
                bool result = await mGameRepository.AddItemAsync(new GameSession(model.OpponentUserId, user.Id)
                {
                    PlayerOne = null,
                    PlayerTwo = null,
                    InProgress = false
                });
                if (result)
                {
                    // Create challange accept request to let opponent join the game
                    mContext.UserRequests.Add(new UserRequestDataModel
                    { 
                        UserId = user.Id,
                        RelatedUserId = model.OpponentUserId,
                        Type = UserRequestType.AcceptChallange,
                        CreatedAt = DateTime.Now
                    });

                    // Save changes
                    mContext.SaveChanges();

                    // Return successful response
                    return new ApiResponse();
                }
            }

            // Failed
            return new ApiResponse { ErrorMessage = Localization.Resource.ApiController_AddGameByRequest_FailedErrorMsg };
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

        /// <summary>
        /// Include additional data into the user dataw model
        /// </summary>
        /// <returns>The same model with additional data included</returns>
        private async Task IncludeAdditionalDataAsync(UserDataModel user, ApplicationUser appUser, bool roles = false, bool onlineStatus = false, Dictionary<string, DateTime> loggedInUserCache = null, bool isFriendWith = false, bool isChallanged = false, bool isPlaying = false)
        {
            if (roles && appUser != null)
            {
                // Get user roles
                user.RoleList = (await mUserManager.GetRolesAsync(appUser)).ToList();
            }
            if (onlineStatus && loggedInUserCache != null)
            {
                // Set status
                user.IsOnline = loggedInUserCache.ContainsKey(user.Username);
            }
            if (isFriendWith && !user.Id.Equals(appUser.Id))
            {
                // Set friend relation
                user.IsFriendWith = mContext.FriendUserRelations.FirstOrDefault(o => o.UserId.Equals(appUser.Id) && o.FriendUserId.Equals(user.Id)) != null;
                user.IsFriendWith = mContext.UserRequests.FirstOrDefault(o => o.Type == UserRequestType.Friend && o.UserId.Equals(appUser.Id) && o.RelatedUserId.Equals(user.Id)) == null ? user.IsFriendWith : null;
            }
            if (isChallanged && !user.Id.Equals(appUser.Id))
            {
                // Set challange request status
                user.IsChallanged = mContext.UserRequests
                    .Where(o => 
                        (o.Type == UserRequestType.Challange && o.UserId.Equals(user.Id) && o.RelatedUserId.Equals(appUser.Id))
                        || o.Type == UserRequestType.Challange && o.UserId.Equals(appUser.Id) && o.RelatedUserId.Equals(user.Id)
                        )
                    .Count() > 0;
            }
            if (isPlaying && !user.Id.Equals(appUser.Id))
            {
                // Set is playing request status
                user.IsPlaying = mContext.GameParticipants
                    .Any(o => o.Game.Result == GameResult.None && o.UserId.Equals(user.Id));
            }
        }

        #endregion
    }
}
