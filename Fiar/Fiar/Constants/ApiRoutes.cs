namespace Fiar
{
    /// <summary>
    /// The relative routes to all API calls in the server
    /// </summary>
    public static class ApiRoutes
    {
        /// <summary>
        /// The route to the API method
        /// </summary>
        public const string Register = "/api/register";

        /// <summary>
        /// The route to the API method
        /// </summary>
        public const string Login = "/api/login";

        /// <summary>
        /// The route to the API method
        /// </summary>
        public const string GetUserProfile = "/api/user/profile";

        /// <summary>
        /// The route to the API method
        /// </summary>
        public const string GetUserRoles = "/api/user/roles";

        /// <summary>
        /// The route to the API method
        /// </summary>
        public const string GetUserFriendProfiles = "/api/user/friend/profile/all";

        /// <summary>
        /// The route to the API method
        /// </summary>
        public const string GetAllUserProfiles = "/api/user/profile/all";

        /// <summary>
        /// The route to the API method
        /// </summary>
        public const string GetOnlineUserProfiles = "/api/user/profile/all/online";

        /// <summary>
        /// The route to the API method
        /// </summary>
        public const string GetUserRequests = "/api/user/requests";

        /// <summary>
        /// The route to the API method
        /// </summary>
        public const string AddUserRequest = "/api/user/request/add";

        /// <summary>
        /// The route to the API method
        /// </summary>
        public const string DeleteUserRequest = "/api/user/request/delete";

        /// <summary>
        /// The route to the API method
        /// </summary>
        public const string AddUserFriendByRequest = "/api/user/friend/add/request";

        /// <summary>
        /// The route to the API method
        /// </summary>
        public const string DeleteUserFriend = "/api/user/friend/delete";

        /// <summary>
        /// The route to the API method
        /// </summary>
        public const string AddGameByRequest = "/api/game/add/request";
    }
}
