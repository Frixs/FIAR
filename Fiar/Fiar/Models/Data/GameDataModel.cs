using Fiar.Attributes;
using System;
using System.Collections.Generic;

namespace Fiar
{
    /// <summary>
    /// The data model for representing game instance
    /// </summary>
    [ValidableModel(typeof(GameDataModel))]
    public class GameDataModel
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
        public string PlayerOneUserId { get; set; }

        /// <summary>
        /// Reference to ONE user // Fluent API
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        [ValidateIgnore]
        public ApplicationUser PlayerOneUser { get; set; }

        /// <summary>
        /// Foreign Key for <see cref="ApplicationUser"/>
        /// </summary>
        [ValidateIgnore]
        public string PlayerTwoUserId { get; set; }

        /// <summary>
        /// Reference to ONE user // Fluent API
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        [ValidateIgnore]
        public ApplicationUser PlayerTwoUser { get; set; }

        /// <summary>
        /// Reference to MANY game moves // Fluent API
        /// ---
        /// E.g. You can use this list to put as many moves into this list 
        /// while creation a new game to create new moves associated to this game at the same time during commit
        /// </summary>
        [ValidateIgnore]
        public ICollection<GameMoveDataModel> Moves { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// The result of the game
        /// </summary>
        [ValidateIgnore]
        public GameResult Result { get; set; }

        /// <summary>
        /// The datetime of the game start
        /// </summary>
        [ValidateIgnore]
        public DateTime StartedAt { get; set; }

        /// <summary>
        /// The datetime of the game finish
        /// </summary>
        [ValidateIgnore]
        public DateTime FinishedAt { get; set; }

        #endregion
    }
}
