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
    }
}
