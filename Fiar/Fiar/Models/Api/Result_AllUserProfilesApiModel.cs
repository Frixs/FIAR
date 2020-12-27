using System.Collections.Generic;

namespace Fiar
{
    /// <summary>
    /// The result of getting all users via API
    /// </summary>
    public class Result_AllUserProfilesApiModel
    {
        /// <summary>
        /// The user models
        /// </summary>
        public List<UserDataModel> UserModels { get; set; }
    }
}
