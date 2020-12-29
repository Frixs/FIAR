namespace Fiar
{
    /// <summary>
    /// The request to add a game via API
    /// </summary>
    public class Add_GameByRequestApiModel
    {
        /// <summary>
        /// <see cref="UserRequestDataModel.Id"/>
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// <see cref="ApplicationUser.Id"/>
        /// </summary>
        public string OpponentUserId { get; set; }
    }
}
