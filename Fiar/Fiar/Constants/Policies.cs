using System.Collections.Generic;

namespace Fiar
{
    /// <summary>
    /// List of all policies defined
    /// </summary>
    public static class Policies
    {
        /// <summary>
        /// Policy dictionary
        /// </summary>
        public static IDictionary<string, IList<string>> Dict { get; } = new Dictionary<string, IList<string>>()
        { 
            [PolicyNames.PlayerLevel] = new List<string>()
            {
                RoleNames.Administrator,
                RoleNames.Player
            },
            [PolicyNames.AdministratorLevel] = new List<string>()
            {
                RoleNames.Administrator
            }
        };
    }
}
