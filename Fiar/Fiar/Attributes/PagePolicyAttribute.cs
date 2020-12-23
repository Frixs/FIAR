using System;

namespace Fiar.Attributes
{
    /// <summary>
    /// Posibility to add policy to <see cref="ApplicationPage"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class PagePolicyAttribute : Attribute
    {
        #region Public Properties

        /// <summary>
        /// Name of the policy
        /// </summary>
        public string PolicyName { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public PagePolicyAttribute(string policy)
        {
            PolicyName = policy;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Authorize role according to <see cref="PolicyName"/>
        /// </summary>
        /// <param name="role">The role</param>
        /// <returns>TRUE = authorized, FALSE otherwise</returns>
        public bool Authorize(string role) => Policies.Dict[PolicyName].Contains(role);

        #endregion
    }
}
