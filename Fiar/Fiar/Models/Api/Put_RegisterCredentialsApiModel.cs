namespace Fiar
{
    /// <summary>
    /// The credentials for an API client to register the server
    /// </summary>
    public class Put_RegisterCredentialsApiModel
    {
        /// <summary>
        /// The users username
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// The users email
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// The users password
        /// </summary>
        public string Password { get; set; }
    }
}
