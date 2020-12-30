namespace Fiar
{
    /// <summary>
    /// The result of a login request or get user profile details request via API
    /// </summary>
    public class Result_UserProfileDetailsApiModel
    {
        /// <summary>
        /// The authentication token used to stay authenticated through future requests
        /// </summary>
        /// <remarks>The Token is only provided when call is done by the login methods otherwise null</remarks>
        public string Token { get; set; } = null;

        /// <summary>
        /// The user model
        /// </summary>
        public UserDataModel Model { get; set; }
    }
}
