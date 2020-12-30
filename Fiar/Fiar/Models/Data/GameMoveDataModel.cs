using Fiar.Attributes;

namespace Fiar
{
    /// <summary>
    /// The data model for representing a game move
    /// </summary>
    [ValidableModel(typeof(GameMoveDataModel))]
    public class GameMoveDataModel
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

        #endregion

        #region Properties

        /// <summary>
        /// Type of the move (P1 or P2 etc.)
        /// </summary>
        [ValidateIgnore]
        public PlayerType Type { get; set; }

        /// <summary>
        /// Marker coordinates - X
        /// </summary>
        [ValidateIgnore]
        public int PosX { get; set; }

        /// <summary>
        /// Marker coordinates - Y
        /// </summary>
        [ValidateIgnore]
        public int PosY { get; set; }

        #endregion
    }
}
