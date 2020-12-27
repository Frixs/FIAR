using Fiar.Attributes;

namespace Fiar
{
    /// <summary>
    /// The data model for representing a user request
    /// </summary>
    [ValidableModel(typeof(UserRequestDataModel))]
    public class UserRequestDataModel
    {
        #region Limit Constants



        #endregion

        #region Properties (Keys / Relations)

        /// <summary>
        /// The unique Id for this entry
        /// </summary>
        [ValidateIgnore]
        public long Id { get; set; }

        /// <summary>
        /// Foreign Key for <see cref="ApplicationUser"/>
        /// </summary>
        [ValidateIgnore]
        public string UserId { get; set; }

        /// <summary>
        /// Reference to ONE user // Fluent API
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        [ValidateIgnore]
        public ApplicationUser User { get; set; }

        /// <summary>
        /// Foreign Key for <see cref="ApplicationUser"/>
        /// </summary>
        [ValidateIgnore]
        public string RelatedUserId { get; set; }

        /// <summary>
        /// Reference to ONE user // Fluent API
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        [ValidateIgnore]
        public ApplicationUser RelatedUser { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Type of the request
        /// </summary>
        [ValidateIgnore]
        public UserRequestType Type { get; set; }

        #endregion
    }
}
