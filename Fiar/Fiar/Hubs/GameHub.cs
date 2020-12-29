using Ixs.DNA;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Fiar
{
    /// <summary>
    /// TODO
    /// </summary>
    public interface IGameClient
    {
        Task RenderBoard(GameBoardCellType[][] board);
        Task Color(string color);
        Task Turn(PlayerType playerType);
        Task RollCall(Player player1, Player player2);
        Task Concede();
        Task Victory(PlayerType playerType, GameBoardCellType[][] board);
    }

    /// <summary>
    /// Game hub handles the SignalR connection for the entire gameplay
    /// </summary>
    public class GameHub : Hub<IGameClient>
    {
        #region Private Members

        /// <summary>
        /// Prefix for SignalR group name
        /// </summary>
        private const string mGameGroupNamePrefix = "game-";

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
                game.PlayerOne = Player.Convert(user, Context.ConnectionId, PlayerType.PlayerOne);
            }
            else
            {
                game.PlayerTwo = Player.Convert(user, Context.ConnectionId, PlayerType.PlayerTwo);
            }

            // Once we have the last user (the only opponent one), flag up for starting the game
            if (game.PlayerOne != null && game.PlayerTwo != null)
                game.InProgress = true;

            await Groups.AddToGroupAsync(Context.ConnectionId, mGameGroupNamePrefix + game.Id);
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

            // If game is complete (or any user disconnects) delete it
            var game = await GetGameAsync();
            if (game == null)
            {
                mLogger.LogErrorSource($"Failed to find the game to disconnect from! ({user.UserName})");
                Context.Abort();
                return;
            }

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, mGameGroupNamePrefix + game.Id);
            await Clients.Group(mGameGroupNamePrefix + game.Id).Concede();

            // Log it
            mLogger.LogDebugSource($"User {user.Nickname} disconnected from the game {game.Id}!");

            await mGameRepository.RemoveItemAsync(game);

            await base.OnDisconnectedAsync(exception);
        }

        #endregion

        /// <summary>
        /// TODO
        /// </summary>
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

        /// <summary>
        /// The game turn by clicking the cell
        /// </summary>
        /// <param name="row">The player's turn row</param>
        /// <param name="column">The player's turn column</param>
        public async Task CellClick(int row, int column)
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

            // Get the game to play on
            var game = await GetGameAsync();
            if (game == null)
            {
                mLogger.LogErrorSource($"Failed to find the game to play on! ({user.UserName})");
                return;
            }

            // If the user is not the player on turn
            if (!Context.ConnectionId.Equals(game.CurrentPlayer.ConnectionId))
                // Ignore player clicking if it's not their turn
                return;

            // If the game is not started yet...
            if (!game.InProgress)
                // Ignore
                return;

            // Try to assign player turn into the board
            var result = game.TryAssignPlayerToCell(row, column);

            // If the assign failed...
            if (!result)
                // Ignore clicks on non-empty cells
                return;

            // Call for render the board to the players
            await Clients.Group(mGameGroupNamePrefix + game.Id).RenderBoard(game.Board);

            // Check victory conditions (only the current player can win)
            if (game.CheckVictory(row, column))
            {
                // TODO
                if (game.CurrentPlayer.Id.Equals(game.PlayerOne.Id))
                    ; //UpdateHighScore(game.Player1, game.Player2);
                else
                    ; //UpdateHighScore(game.Player2, game.Player1);

                // Log it
                mLogger.LogInformationSource($"User {game.CurrentPlayer.Nickname} won the game {game.Id}!");

                // Call the victory
                await Clients.Group(mGameGroupNamePrefix + game.Id).Victory(game.CurrentPlayer.Type, game.Board);

                // Log it
                mLogger.LogDebugSource($"Game {game.Id} has ended!");

                await mGameRepository.RemoveItemAsync(game);

                return;
            }

            // Move to the next turn
            game.MoveTurnPointerToNextPlayer();

            // Call the next turn
            await Clients.Group(mGameGroupNamePrefix + game.Id).Turn(game.CurrentPlayer.Type);
        }

        #region Private Helpers

        /// <summary>
        /// Gets the game based on authenticated user context
        /// </summary>
        /// <returns>The game session or null on failure</returns>
        private async Task<GameSession> GetGameAsync()
        {
            return await mGameRepository.GetItemAsync(o => o.HasPlayer(Context.ConnectionId));
        }

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
