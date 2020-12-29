namespace Fiar
{
    /// <summary>
    /// Player object representation
    /// </summary>
    public class Player
    {
        #region Properties

        /// <summary>
        /// <see cref="ApplicationUser.Id"/>
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// <see cref="ApplicationUser.Nickname"/>
        /// </summary>
        public string Nickname { get; set; }

        /// <summary>
        /// The context connection ID (SignalR)
        /// </summary>
        public string ConnectionId { get; set; }

        /// <summary>
        /// Player color (hex)
        /// </summary>
        public string Color { get; set; }

        #endregion

        #region Helpers

        /// <summary>
        /// Convert the database user to the player object
        /// </summary>
        /// <param name="user">The user</param>
        /// <returns>Player object</returns>
        public static Player Convert(ApplicationUser user, string connectionId, string color)
        {
            return new Player
            {
                Id = user.Id,
                Nickname = user.Nickname,
                ConnectionId = connectionId,
                Color = color
            };
        }

        #endregion
    }
}
