using Fiar.ViewModels;
using System.Collections.Generic;

namespace Fiar
{
    /// <summary>
    /// The result of getting user requests via API
    /// </summary>
    public class Result_UserRequestsApiModel
    {
        /// <summary>
        /// The request models
        /// </summary>
        public List<UserRequestViewModel> RequestModels { get; set; }
    }
}
