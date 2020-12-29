using Ixs.DNA;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fiar
{
    /// <summary>
    /// Manages currently opened game sessions
    /// </summary>
    public class GameRepository : IRepository<GameSession>
    {
        #region Private Members

        /// <summary>
        /// List of currently running game sessions
        /// </summary>
        private List<GameSession> mItems = new List<GameSession>();

        /// <summary>
        /// Indicates the repository is processing
        /// </summary>
        private bool mIsProcessing;

        #endregion

        #region Protected Members

        protected readonly ILogger mLogger;
        protected readonly IServiceScopeFactory mServiceScopeFactory;
        protected readonly IConfigBox mConfigBox;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public GameRepository(IServiceScopeFactory serviceScopeFactory, IConfigBox configBox)
        {
            mServiceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            mConfigBox = configBox ?? throw new ArgumentNullException(nameof(configBox));
            mLogger = FrameworkDI.Logger ?? throw new ArgumentNullException(nameof(mLogger));
        }

        #endregion

        #region Interface Methods

        /// <inheritdoc/>
        /// <remarks>
        ///     Identification User IDs must be set afterwards!
        ///     Do not care about item's ID, it is fixed in the process
        /// </remarks>
        public async Task<bool> AddItemAsync(GameSession item)
        {
            if (item == null)
            {
                mLogger.LogCriticalSource("No item specified!");
                return false;
            }

            // Lock the task (monitor)
            return await AsyncLock.LockResultAsync(nameof(mIsProcessing), () =>
            {
                bool result = false;

                using (var scope = mServiceScopeFactory.CreateScope())
                {
                    // Get DB context
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    if (!dbContext.GameParticipants.Any(o => o.Game.Result == GameResult.None && (o.UserId.Equals(item.PlayerOneUserId) || o.UserId.Equals(item.PlayerTwoUserId))))
                    {
                        // Create game data model
                        var participants = new List<GameParticipantDataModel>();
                        participants.Add(new GameParticipantDataModel
                        {
                            UserId = item.PlayerOneUserId,
                            Type = PlayerType.PlayerOne
                        });
                        participants.Add(new GameParticipantDataModel
                        {
                            UserId = item.PlayerTwoUserId,
                            Type = PlayerType.PlayerTwo
                        });
                        var game = new GameDataModel
                        {
                            Result = GameResult.None,
                            Participants = participants
                        };

                        // Insert it into DB
                        dbContext.Games.Add(game);
                        if (dbContext.SaveChanges() > 0)
                        {
                            item.Id = game.Id; // Fix the ID upon inserting it into DB
                            result = true;
                        }
                    }
                    else
                    {
                        mLogger.LogWarningSource($"The user is already participating in other game! ({item.PlayerOneUserId} / {item.PlayerTwoUserId})");
                    }
                }

                // Add the item
                if (result && !mItems.Any(o => o.Id.Equals(item.Id)))
                    mItems.Add(item);

                return result;
            });
        }

        /// <inheritdoc/>
        public async Task<GameSession> GetItemAsync(Func<GameSession, bool> predicate)
        {
            // Lock the task (monitor)
            return await AsyncLock.LockResultAsync(nameof(mIsProcessing), () =>
            {
                GameSession result = null;

                // Try to get the session
                var session = mItems.FirstOrDefault(predicate);
                if (session != null)
                    result = session;

                return result;
            });
        }

        /// <inheritdoc/>
        public async Task RemoveItemAsync(GameSession item)
        {
            if (item == null)
            {
                mLogger.LogCriticalSource("No item specified!");
                return;
            }

            // Lock the task (monitor)
            await AsyncLock.LockAsync(nameof(mIsProcessing), () =>
            {
                bool result = false;

                using (var scope = mServiceScopeFactory.CreateScope())
                {
                    // Get DB context
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var game = dbContext.Games.Find(item.Id);
                    if (game != null)
                    {
                        if (game.Result == GameResult.None)
                        {
                            dbContext.Games.Remove(game);
                            if (dbContext.SaveChanges() > 0)
                                result = true;
                        }
                        else
                        {
                            result = true;
                        }

                        // Possibly rmeove requests if any...
                        var dbP1 = dbContext.GameParticipants.FirstOrDefault(o => o.GameId == game.Id && o.Type == PlayerType.PlayerOne);
                        var dbP2 = dbContext.GameParticipants.FirstOrDefault(o => o.GameId == game.Id && o.Type == PlayerType.PlayerTwo);
                        if (dbP1 != null && dbP2 != null)
                        {
                            var requests = dbContext.UserRequests.Where(o => o.Type == UserRequestType.AcceptChallange && (o.UserId.Equals(dbP1.Id) || o.UserId.Equals(dbP2.Id))).ToList();
                            dbContext.RemoveRange(requests);
                            dbContext.SaveChanges();
                        }
                    }
                }

                // Remove the item
                if (result && mItems.RemoveAll(o => o.Id.Equals(item.Id)) == 0)
                    mLogger.LogCriticalSource("Item for deletion does not exist!");
            });
        }

        #endregion
    }
}
