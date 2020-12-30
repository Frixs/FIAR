namespace Fiar
{
    /// <summary>
    /// The request to get user profile details via API
    /// </summary>
    public class Get_UserProfileDetailsApiModel
    {
        /// <summary>
        /// Indicates including user roles into the result
        /// </summary>
        public bool IncludeRoleList { get; set; }

        /// <summary>
        /// Indicates including online status
        /// </summary>
        public bool IncludeOnlineStatus { get; set; }

        /// <summary>
        /// Indicates including user friend relation to the currently logged one
        /// </summary>
        public bool IncludeIsFriendWith { get; set; }

        /// <summary>
        /// Indicates including user challange request
        /// </summary>
        public bool IncludeIsChallanged { get; set; }

        /// <summary>
        /// Indicates including user is playing request
        /// </summary>
        public bool IncludeIsPlaying { get; set; }
    }
}
