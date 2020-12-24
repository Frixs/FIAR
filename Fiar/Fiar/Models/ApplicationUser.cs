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
        /// <see cref="UserDataModel.Nickname"/>
        /// </summary>
        public string Nickname { get; set; }

        #endregion
    }
}
