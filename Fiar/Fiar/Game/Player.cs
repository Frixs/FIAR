using System;

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
        /// Player cell type representing the player
        /// </summary>
        public PlayerType Type { get; private set; }

        /// <summary>
        /// Player color (hex)
        /// </summary>
        public string Color { get; private set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public Player(PlayerType playerType)
        {
            if (playerType == PlayerType.PlayerOne)
            {
                Type = PlayerType.PlayerOne;
                Color = PlayerDefaultColor.One;
            }
            else if (playerType == PlayerType.PlayerTwo)
            {
                Type = PlayerType.PlayerTwo;
                Color = PlayerDefaultColor.Two;
            }
            else
                throw new ArgumentException("Invalid player board cell type!");
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Convert the database user to the player object
        /// </summary>
        /// <param name="user">The user</param>
        /// <returns>Player object</returns>
        public static Player Convert(ApplicationUser user, string connectionId, PlayerType playerType)
        {
            return new Player(playerType)
            {
                Id = user.Id,
                Nickname = user.Nickname,
                ConnectionId = connectionId
            };
        }

        #endregion
    }
}
