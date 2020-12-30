using System.Collections.Generic;

namespace Fiar
{
    /// <summary>
    /// The result returning list of user roles via API
    /// </summary>
    public class Result_UserRolesApiModel
    {
        /// <summary>
        /// List of user roles
        /// </summary>
        public List<string> RoleList { get; set; }
    }
}
