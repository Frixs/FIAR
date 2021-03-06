﻿using Fiar.Attributes;
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
        /// Reference to MANY game moves // Fluent API
        /// ---
        /// E.g. You can use this list to put as many moves into this list 
        /// while creation a new game to create new moves associated to this game at the same time during commit
        /// </summary>
        [ValidateIgnore]
        public ICollection<GameMoveDataModel> Moves { get; set; }

        /// <summary>
        /// Reference to MANY game participants // Fluent API
        /// ---
        /// E.g. You can use this list to put as many moves into this list 
        /// while creation a new game to create new moves associated to this game at the same time during commit
        /// </summary>
        [ValidateIgnore]
        public ICollection<GameParticipantDataModel> Participants { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// The result of the game
        /// </summary>
        [ValidateIgnore]
        public GameResult Result { get; set; }

        #endregion
    }
}
