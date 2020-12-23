using Microsoft.AspNetCore.Identity;

namespace Fiar
{
    /// <summary>
    /// The user data and profile for our application
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        #region Public Properties

        /// <summary>
        /// <see cref="UserDataModel.AlertSuppressed"/>
        /// </summary>
        public bool AlertSuppressed { get; set; } = false;

        #endregion
    }
}
