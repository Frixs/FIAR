using Fiar.Attributes;

namespace Fiar
{
    /// <summary>
    /// The data model for representing a game participant
    /// </summary>
    [ValidableModel(typeof(GameParticipantDataModel))]
    public class GameParticipantDataModel
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
        /// Foreign Key for <see cref="GameDataModel"/>
        /// </summary>
        [ValidateIgnore]
        public long GameId { get; set; }

        /// <summary>
        /// Reference to ONE game // Fluent API
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        [ValidateIgnore]
        public GameDataModel Game { get; set; }

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

        #endregion

        #region Properties

        /// <summary>
        /// Type of the move (P1 or P2 etc.)
        /// </summary>
        [ValidateIgnore]
        public PlayerType Type { get; set; }

        #endregion
    }
}
