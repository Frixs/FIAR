namespace Fiar
{
    /// <summary>
    /// The credentials for an API client to log into the server and receive a token back
    /// </summary>
    public class Put_LoginCredentialsApiModel
    {
        /// <summary>
        /// The users username or email
        /// </summary>
        public string UsernameOrEmail { get; set; }

        /// <summary>
        /// The users password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// The users information about staying logged in or not
        /// </summary>
        public bool StayLoggedIn { get; set; }
    }
}
