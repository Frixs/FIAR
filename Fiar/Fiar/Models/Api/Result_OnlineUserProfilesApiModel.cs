using System.Collections.Generic;

namespace Fiar
{
    /// <summary>
    /// The result for getting online users via API
    /// </summary>
    public class Result_OnlineUserProfilesApiModel
    {
        /// <summary>
        /// The user models
        /// </summary>
        public List<UserDataModel> UserModels { get; set; }
    }
}
