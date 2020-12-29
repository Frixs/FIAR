namespace Fiar
{
    /// <summary>
    /// Running game session context
    /// </summary>
    public class GameSession
    {
        /// <summary>
        /// <see cref="GameDataModel.Id"/>
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// <see cref="ApplicationUser.Id"/> of player 1
        /// It indicates who is the player 1
        /// </summary>
        public string PlayerOneUserId { get; set; }

        /// <summary>
        /// <see cref="ApplicationUser.Id"/> of player 2
        /// It indicates who is the player 2
        /// </summary>
        public string PlayerTwoUserId { get; set; }

        /// <summary>
        /// Defines player 1 instance joined into the game
        /// </summary>
        public Player PlayerOne { get; set; }

        /// <summary>
        /// Defines player 2 instance joined into the game
        /// </summary>
        public Player PlayerTwo { get; set; }

        /// <summary>
        /// Indicates if the game is in progress (true) or not (false)
        /// </summary>
        /// <remarks>
        ///     It indicates the game already started (true).
        /// </remarks>
        public bool InProgress { get; set; }
    }
}
