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
        /// The users first name
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// The users last name
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// <see cref="UserDataModel.AlertSuppressed"/>
        /// </summary>
        public bool AlertSuppressed { get; set; } = false;

        /// <summary>
        /// <see cref="UserDataModel.MailAlerts"/>
        /// </summary>
        public bool MailAlerts { get; set; } = false;

        /// <summary>
        /// <see cref="UserDataModel.IsStrictRecipient"/>
        /// </summary>
        public bool IsStrictRecipient { get; set; } = false;

        /// <summary>
        /// <see cref="UserDataModel.AllOkRecipient"/>
        /// </summary>
        public bool AllOkRecipient { get; set; } = true;

        /// <summary>
        /// <see cref="UserDataModel.DailyReportRecipient"/>
        /// </summary>
        public bool DailyReportRecipient { get; set; } = true;

        #endregion
    }
}
