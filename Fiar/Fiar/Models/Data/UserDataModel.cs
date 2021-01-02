using Fiar.Attributes;
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
        /// </summary>
        public static readonly bool Nickname_IsRequired = true;

        /// <summary>
        /// Limit property for this data model property
        /// </summary>
        public static readonly short Nickname_MaxLength = 16;

        /// <summary>
        /// Limit property for this data model property
        /// </summary>
        public static readonly string Nickname_CanContainRegex = @"^[a-zA-Z0-9_-]*$";



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
        /// The users nickname
        /// </summary>
        [ValidateString(nameof(Nickname), typeof(UserDataModel),
            pIsRequired: nameof(Nickname_IsRequired),
            pMaxLength: nameof(Nickname_MaxLength),
            pCanContainRegex: nameof(Nickname_CanContainRegex))]
        public string Nickname { get; set; }

        /// <summary>
        /// THe users role list
        /// </summary>
        /// <remarks>
        ///     Separate value
        /// </remarks>
        [ValidateIgnore]
        public List<string> RoleList { get; set; }

        /// <summary>
        /// If not null, it indicates if the user is logged in or not
        /// </summary>
        /// <remarks>
        ///     Separate value
        /// </remarks>
        [ValidateIgnore]
        public bool? IsOnline { get; set; }

        /// <summary>
        /// If not null, it indicates if the user is a friend of the currently logged in user
        /// </summary>
        /// <remarks>
        ///     Separate value - helper value
        /// </remarks>
        [ValidateIgnore]
        public bool? IsFriendWith { get; set; }

        /// <summary>
        /// If not null, it indicates if the user is challanged by another user (request)
        /// </summary>
        /// <remarks>
        ///     Separate value - helper value
        /// </remarks>
        [ValidateIgnore]
        public bool? IsChallanged { get; set; }

        /// <summary>
        /// If not null, it indicates if the user is currently playing a game
        /// </summary>
        /// <remarks>
        ///     Separate value - helper value
        /// </remarks>
        [ValidateIgnore]
        public bool? IsPlaying { get; set; }

        #endregion

        #region Helpers

        /// <summary>
        /// Convert the database user to the user data model
        /// </summary>
        /// <param name="user">The user</param>
        /// <returns>User data model</returns>
        public static UserDataModel Convert(ApplicationUser user)
        {
            return new UserDataModel
            {
                Id = user.Id,
                Username = user.UserName,
                Email = user.Email,
                Nickname = user.Nickname
            };
        }

        #endregion
    }
}
