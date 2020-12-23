using Fiar.Attributes;
using Ixs.DNA;
using System.Collections.Generic;

namespace Fiar
{
    /// <summary>
    /// The data model for representing user (not a database table)
    /// </summary>
    [ValidableModel(typeof(UserDataModel))]
    public class UserDataModel
    {
        #region Limit Constants

        /// <summary>
        /// Limit property for this data model property
        /// </summary>
        public static readonly bool Username_IsRequired = true;

        /// <summary>
        /// Limit property for this data model property
        /// </summary>
        public static readonly short Username_MaxLength = 16;

        /// <summary>
        /// Limit property for this data model property
        /// </summary>
        public static readonly string Username_CanContainRegex = @"^[a-zA-Z0-9_-]*$";



        /// <summary>
        /// Limit property for this data model property
        /// </summary>
        public static readonly bool Email_IsRequired = true;

        /// <summary>
        /// Limit property for this data model property
        /// </summary>
        public static readonly short Email_MaxLength = 100;

        /// <summary>
        /// Limit property for this data model property
        /// </summary>
        public static readonly string Email_CanContainRegex = @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$";



        /// <summary>
        /// Limit property for this data model property
        /// Special case - not part of this data model
        /// </summary>
        public static readonly short Password_MinLength = 6;

        /// <summary>
        /// Limit property for this data model property
        /// Special case - not part of this data model
        /// </summary>
        public static readonly short Password_MaxLength = 32;

        #endregion

        #region Properties

        /// <summary>
        /// The unique Id
        /// </summary>
        [ValidateIgnore]
        public string Id { get; set; }

        /// <summary>
        /// The users username
        /// </summary>
        [ValidateString(nameof(Username), typeof(UserDataModel),
            pIsRequired: nameof(Username_IsRequired),
            pMaxLength: nameof(Username_MaxLength),
            pCanContainRegex: nameof(Username_CanContainRegex))]
        public string Username { get; set; }

        /// <summary>
        /// The users email
        /// </summary>
        [ValidateString(nameof(Email), typeof(UserDataModel),
            pIsRequired: nameof(Email_IsRequired),
            pMaxLength: nameof(Email_MaxLength),
            pCanContainRegex: nameof(Email_CanContainRegex))]
        public string Email { get; set; }

        /// <summary>
        /// Indicates if the user has suppressed alerts
        /// </summary>
        [ValidateIgnore]
        public bool AlertSuppressed { get; set; }

        /// <summary>
        /// THe users role list
        /// </summary>
        [ValidateIgnore]
        public List<string> RoleList { get; set; }

        #endregion

        #region Helper Properties

        /// <summary>
        /// Indicates the users is valid for mail sending
        /// </summary>
        [ValidateIgnore]
        public bool IsValidMailRecipient => !AlertSuppressed && MailAlerts;

        /// <summary>
        /// Indicates the user is valid for SMS sending
        /// </summary>
        [ValidateIgnore]
        public bool IsValidSmsRecipient => !AlertSuppressed && !PhoneNumber.IsNullOrEmpty();

        #endregion
    }
}
