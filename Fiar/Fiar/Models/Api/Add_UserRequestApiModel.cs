namespace Fiar
{
    /// <summary>
    /// The request to add user request via API
    /// </summary>
    public class Add_UserRequestApiModel
    {
        /// <summary>
        /// <see cref="UserRequestDataModel.Type"/>
        /// </summary>
        public UserRequestType Type { get; set; }

        /// <summary>
        /// <see cref="UserRequestDataModel.RelatedUserId"/>
        /// </summary>
        public string RelatedUserId { get; set; }
    }
}
