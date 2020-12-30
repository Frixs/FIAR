namespace Fiar
{
    /// <summary>
    /// The request to add a user friend by request via API
    /// </summary>
    public class Add_UserFriendByRequestApiModel
    {
        /// <summary>
        /// <see cref="UserRequestDataModel.Id"/>
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// <see cref="FriendUserRelationDataModel.FriendUserId"/>
        /// </summary>
        public string FriendUserId { get; set; }
    }
}
