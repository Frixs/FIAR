using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace Fiar
{
    /// <summary>
    /// The user data and profile for our application
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        #region Properties (Keys / Relations)

        /// <summary>
        /// Reference to MANY friends // Fluent API
        /// ---
        /// E.g. You can use this list to put as many moves into this list 
        /// while creation a new game to create new moves associated to this game at the same time during commit
        /// </summary>
        public ICollection<FriendUserRelationDataModel> Friends { get; set; }

        /// <summary>
        /// Reference to MANY friends (reference to all users that have this user in their friend list) // Fluent API
        /// ---
        /// E.g. You can use this list to put as many moves into this list 
        /// while creation a new game to create new moves associated to this game at the same time during commit
        /// </summary>
        public ICollection<FriendUserRelationDataModel> BeingFriendOfs { get; set; }

        #endregion

        #region Public Properties

        /// <summary>
        /// <see cref="UserDataModel.Nickname"/>
        /// </summary>
        public string Nickname { get; set; }

        #endregion
    }
}
