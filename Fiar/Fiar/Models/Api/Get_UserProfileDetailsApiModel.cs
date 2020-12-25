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
    }
}
