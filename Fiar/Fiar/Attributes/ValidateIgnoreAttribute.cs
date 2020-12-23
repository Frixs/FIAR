using System;

namespace Fiar.Attributes
{
    /// <summary>
    /// Mark property with this placeholder to let programmer know, it should not have any validation
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class ValidateIgnoreAttribute : BaseValidateAttribute
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public ValidateIgnoreAttribute()
        {
        }

        #endregion
    }
}
