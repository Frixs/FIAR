using System.Collections.Generic;

namespace Fiar
{
    /// <summary>
    /// The result of a login request or get user profile details request via API
    /// </summary>
    public class Result_UserFriendProfilesApiModel
    {
        /// <summary>
        /// The user models
        /// </summary>
        public List<UserDataModel> FriendModels { get; set; }
    }
}
