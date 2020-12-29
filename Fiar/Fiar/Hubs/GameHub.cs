using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Ixs.DNA;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;

namespace Fiar
{
    public interface IGameClient
    {
        Task RenderBoard(string[][] board);
        Task Color(string color);
        Task Turn(string player);
        Task RollCall(Player player1, Player player2);
        Task Concede();
        Task Victory(string player, string[][] board);
    }

    /// <summary>
    /// TODO
    /// </summary>
    public class GameHub : Hub<IGameClient>
    {
        #region Private Members

        /// <summary>
        /// Prefix for SignalR group name
        /// </summary>
        private const string mGroupNamePrefix = "game-";

        #endregion

        #region Protected Members

        /// <summary>
        /// The scoped Application context
        /// </summary>
        protected ApplicationDbContext mContext;

        /// <summary>
        /// The manager for handling user creation, deletion, searching, roles, etc.
        /// </summary>
        protected UserManager<ApplicationUser> mUserManager;

        /// <summary>
        /// Injection - <inheritdoc cref="IRepository<GameSession>"/>
        /// </summary>
        protected readonly IRepository<GameSession> mGameRepository;

        /// <summary>
        /// Injection - <inheritdoc cref="ILogger"/>
        /// </summary>
        protected readonly ILogger mLogger;

        /// <summary>
        /// Injection - <inheritdoc cref="IConfigBox"/>
        /// </summary>
        protected readonly IConfigBox mConfigBox;

        #endregion
        
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public GameHub(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IRepository<GameSession> gameRepository, IConfigBox configBox)
        {
            mContext = context;
            mUserManager = userManager;
            mGameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository));
            mConfigBox = configBox ?? throw new ArgumentNullException(nameof(configBox));
            mLogger = FrameworkDI.Logger ?? throw new ArgumentNullException(nameof(mLogger));
        }

        #endregion

        #region Connection Methods

        /// <inheritdoc/>
        public override async Task OnConnectedAsync()
        {
            // Get user by the claims
            var user = await mUserManager.GetUserAsync(Context.User);
            // If user does not have authorization...
            if (!await AuthorizeUserAsync(user, PolicyNames.PlayerLevel))
            {
                mLogger.LogDebugSource($"Unauthorized user to connect! ({user?.UserName ?? ""})");
                Context.Abort();
                return;
            }

            var game = await mGameRepository.GetItemAsync(o => !o.InProgress && (o.PlayerOneUserId.Equals(user.Id) || o.PlayerTwoUserId.Equals(user.Id)));
            if (game == null)
            {
                mLogger.LogErrorSource($"Failed to find the game to connect to! ({user.UserName})");
                Context.Abort();
                return;
            }

            // Assign players to the game
            if (user.Id.Equals(game.PlayerOneUserId))
            {
                game.PlayerOne = Player.Convert(user, Context.ConnectionId, PlayerDefaultColor.One);
            }
            else
            {
                game.PlayerTwo = Player.Convert(user, Context.ConnectionId, PlayerDefaultColor.Two);
            }

            // Once we have the last user (the only opponent one), flag up for starting the game
            if (game.PlayerOne != null & game.PlayerTwo != null)
                game.InProgress = true;

            await Groups.AddToGroupAsync(Context.ConnectionId, mGroupNamePrefix + game.Id);
            await base.OnConnectedAsync();

            // Log it
            mLogger.LogDebugSource($"User {user.Nickname} connected to the game {game.Id}!");

            if (game.InProgress)
            {
                // Initial game starting process
                // TODO
                //CoinToss(game);
                //await Clients.Group(game.Id.ToString()).RenderBoard(game.Board);
            }
        }

        /// <inheritdoc/>
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            // Get user by the claims
            var user = await mUserManager.GetUserAsync(Context.User);
            // If user does not have authorization...
            if (!await AuthorizeUserAsync(user, PolicyNames.PlayerLevel))
            {
                mLogger.LogDebugSource($"Unauthorized user to disconnect! ({user?.UserName ?? ""})");
                Context.Abort();
                return;
            }

            //If game is complete (or any user disconnects) delete it
            var game = await mGameRepository.GetItemAsync(o => o.PlayerOne.ConnectionId.Equals(Context.ConnectionId) || o.PlayerTwo.ConnectionId.Equals(Context.ConnectionId));
            if (game == null)
            {
                mLogger.LogErrorSource($"Failed to find the game to disconnect from! ({user.UserName})");
                Context.Abort();
                return;
            }

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, mGroupNamePrefix + game.Id);
            await Clients.Group(mGroupNamePrefix + game.Id).Concede();

            // Log it
            mLogger.LogDebugSource($"User {user.Nickname} disconnected from the game {game.Id}!");

            await mGameRepository.RemoveItemAsync(game);

            await base.OnDisconnectedAsync(exception);
        }

        #endregion

        public async Task UpdateUser(string email, string name)
        {
            //var game = mGameRepository.Games.FirstOrDefault(g => g.HasPlayer(Context.ConnectionId));
            //if (game != null)
            //{
            //    var player = game.GetPlayer(Context.ConnectionId);
            //    player.Email = email;
            //    player.Name = name;
            //    await Clients.Group(game.Id).RollCall(game.Player1, game.Player2);
            //}
        }

        public async Task ColumnClick(int column)
        {
            //var game = mGameRepository.Games.FirstOrDefault(g => g.HasPlayer(Context.ConnectionId));
            //if (game is null)
            //{
            //    //Ignore click if no game exists
            //    return;
            //}

            //if (Context.ConnectionId != game.CurrentPlayer.ConnectionId)
            //{
            //    //Ignore player clicking if it's not their turn
            //    return;
            //}

            ////ignore games that havent started
            //if (!game.InProgress) return;

            //var result = game.TryGetNextOpenRow(column);

            //// find first open spot in the column
            //if (!result.exists)
            //{
            //    //ignore clicks on full columns
            //    return;
            //}

            //await Clients.Group(game.Id.ToString()).RenderBoard(game.Board);

            //// Check victory (only current player can win)
            //if (game.CheckVictory(result.row, column))
            //{
            //    if (game.CurrentPlayer == game.Player1)
            //    {
            //        UpdateHighScore(game.Player1, game.Player2);
            //    }
            //    else
            //    {
            //        UpdateHighScore(game.Player2, game.Player1);
            //    }

            //    await Clients.Group(game.Id).Victory(game.CurrentPlayer.Color, game.Board);
            //    mGameRepository.Games.Remove(game);
            //    return;
            //}

            //game.NextPlayer();

            //await Clients.Group(game.Id).Turn(game.CurrentPlayer.Color);
        }

        private void UpdateHighScore(Player winner, Player loser)
        {
            //var winnerScore = mGameRepository.HighScores.FirstOrDefault(s => s.PlayerName == winner.Name);
            //if (winnerScore == null)
            //{
            //    winnerScore = new HighScore { PlayerName = winner.Name };
            //    mGameRepository.HighScores.Add(winnerScore);
            //}

            //winnerScore.Played++;
            //winnerScore.Won++;
            //winnerScore.Percentage = Convert.ToInt32((winnerScore.Won / Convert.ToSingle(winnerScore.Played)) * 100);

            //var loserScore = mGameRepository.HighScores.FirstOrDefault(s => s.PlayerName == loser.Name);
            //if (loserScore == null)
            //{
            //    loserScore = new HighScore { PlayerName = winner.Name };
            //    mGameRepository.HighScores.Add(loserScore);
            //}

            //loserScore.Played++;
            //loserScore.Percentage = Convert.ToInt32((loserScore.Won / Convert.ToSingle(loserScore.Played)) * 100);
        }

        private async void CoinToss(Game game)
        {
            //var result = _random.Next(2);
            //if (result == 1)
            //{
            //    game.Player1.Color = Game.RedCell;
            //    game.Player2.Color = Game.YellowCell;
            //    game.CurrentPlayer = game.Player1;
            //}
            //else
            //{
            //    game.Player1.Color = Game.YellowCell;
            //    game.Player2.Color = Game.RedCell;
            //    game.CurrentPlayer = game.Player2;
            //}

            //await Clients.Client(game.Player1.ConnectionId).Color(game.Player1.Color);
            //await Clients.Client(game.Player2.ConnectionId).Color(game.Player2.Color);
            //await Clients.Group(game.Id).Turn(Game.RedCell);
        }


        #region Private Helpers

        /// <summary>
        /// Authorize user by policy
        /// TODO: merge with the method in the API controller
        /// </summary>
        /// <param name="user">The user</param>
        /// <param name="policyName">The policy name identifier</param>
        /// <returns>TRUE = authorized, FALSE = otherwise</returns>
        private async Task<bool> AuthorizeUserAsync(ApplicationUser user, string policyName)
        {
            // User does not exist...
            if (user == null)
                return false;

            // Get user roles
            var userRoles = await mUserManager.GetRolesAsync(user);
            // Go thourgh user roles to find a match...
            foreach (var role in userRoles)
                // If user has authorization according to policy...
                if (Policies.Dict[policyName].Contains(role))
                    return true;

            return false;
        }

        #endregion
    }
}
