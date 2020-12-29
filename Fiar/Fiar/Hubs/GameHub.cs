using Ixs.DNA;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Fiar
{
    /// <summary>
    /// Interface for game client interactions
    /// </summary>
    public interface IGameClient
    {
        /// <summary>
        /// Cal for rendering game board
        /// </summary>
        /// <param name="board">The board representation</param>
        Task RenderBoard(GameBoardCellType[][] board);

        /// <summary>
        /// Call to update player data
        /// </summary>
        Task UpdatePlayers(Player playerOne, Player playerTwo);

        /// <summary>
        /// Call to next turn
        /// </summary>
        /// <param name="playerType">The player type on turn</param>
        Task Turn(PlayerType playerType);

        /// <summary>
        /// Call to victory
        /// </summary>
        /// <param name="playerType">The victory player type</param>
        Task Victory(PlayerType playerType);

        /// <summary>
        /// Call to concede
        /// </summary>
        Task Concede();

        /// <summary>
        /// Call to send chat message
        /// </summary>
        /// <param name="nickname">The nickname</param>
        /// <param name="message">The message</param>
        /// <returns></returns>
        Task ReceiveChatMessage(string nickname, string message);
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
                await UpdatePlayersCallAsync(game);
            }
            else
            {
                game.PlayerTwo = Player.Convert(user, Context.ConnectionId, PlayerType.PlayerTwo);
                await UpdatePlayersCallAsync(game);
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
                // Log it
                mLogger.LogDebugSource($"The game {game.Id} is starting now!");
                // Initial game starting process
                await RenderBoardCall(game);
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
            await ConcedeCallAsync(game);

            // Log it
            mLogger.LogDebugSource($"User {user.Nickname} disconnected from the game {game.Id}!");

            await mGameRepository.RemoveItemAsync(game);

            await base.OnDisconnectedAsync(exception);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Send chat message task
        /// </summary>
        /// <param name="message">Message to send</param>
        public async Task SendChatMessage(string message)
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

            // Trim the message
            message = message.Trim();

            // Ignore empty messages
            if (message.IsNullOrEmpty())
                return;

            // Cal the send chat message task
            await ReceiveChatMessageCallAsync(game, user.Nickname, message);
        }

        /// <summary>
        /// The game turn by clicking the cell task
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
            await RenderBoardCall(game);

            // Get DB game object
            var dbGame = mContext.Games.Find(game.Id);
            if (dbGame == null)
            {
                mLogger.LogCriticalSource($"Cannot find the game under the ID: {game.Id}!");
                return;
            }

            // Check victory conditions (only the current player can win)
            if (game.CheckVictory(row, column))
            {
                // Update result in DB
                if (game.CurrentPlayer.Id.Equals(game.PlayerTwo.Id))
                    dbGame.Result = GameResult.PlayerTwoWon;
                else
                    dbGame.Result = GameResult.PlayerOneWon;
                mContext.SaveChanges();

                // Log it
                mLogger.LogInformationSource($"User {game.CurrentPlayer.Nickname} won the game {game.Id}!");

                // Call the victory
                await RenderBoardCall(game);
                await VictoryCallAsync(game);

                // Log it
                mLogger.LogDebugSource($"Game {game.Id} has ended!");

                await mGameRepository.RemoveItemAsync(game);

                return;
            }

            // Add the turn into DB
            mContext.GameMoves.Add(new GameMoveDataModel
            {
                GameId = dbGame.Id,
                PosX = row,
                PosY = column,
                Type = game.CurrentPlayer.Type
            });
            mContext.SaveChanges();

            // Move to the next turn
            game.MoveTurnPointerToNextPlayer();

            // Call the next turn
            await TurnCallAsync(game);
        }

        #endregion

        #region Call Methods

        /// <summary>
        /// Method to call <see cref="IGameClient.RenderBoard(GameBoardCellType[][])"/> to the players
        /// </summary>
        private async Task RenderBoardCall(GameSession game)
        {
            // Call for render the board to the players
            await Clients.Group(mGameGroupNamePrefix + game.Id).RenderBoard(game.Board);
            // Log it
            mLogger.LogDebugSource($"Updating game board for the game {game.Id}.");
        }

        /// <summary>
        /// Method to call <see cref="IGameClient.UpdatePlayers(Player, Player)"/> to the players
        /// </summary>
        private async Task UpdatePlayersCallAsync(GameSession game)
        {
            // Call to update players
            await Clients.Group(mGameGroupNamePrefix + game.Id).UpdatePlayers(game.PlayerOne, game.PlayerTwo);
            // Log it
            mLogger.LogDebugSource($"Updating players for the game {game.Id}: '{game.PlayerOne?.Nickname ?? ""}', '{game.PlayerTwo?.Nickname ?? ""}'.");
        }

        /// <summary>
        /// Method to call <see cref="IGameClient.Turn(PlayerType)"/> to the players
        /// </summary>
        private async Task TurnCallAsync(GameSession game)
        {
            // Call the next turn
            await Clients.Group(mGameGroupNamePrefix + game.Id).Turn(game.CurrentPlayer.Type);
            // Log it
            mLogger.LogDebugSource($"Moving turn in the game {game.Id}. Player on next turn: '{game.CurrentPlayer?.Nickname ?? ""}'.");
        }

        /// <summary>
        /// Method to call <see cref="IGameClient.Victory(PlayerType)"/> to the players
        /// </summary>
        private async Task VictoryCallAsync(GameSession game)
        {
            // Call the victory
            await Clients.Group(mGameGroupNamePrefix + game.Id).Victory(game.CurrentPlayer.Type);
            // Log it
            mLogger.LogDebugSource($"Resulting the game {game.Id} in the victory of '{game.CurrentPlayer?.Nickname ?? ""}'.");
        }

        /// <summary>
        /// Method to call <see cref="IGameClient.Concede()"/> to the players
        /// </summary>
        private async Task ConcedeCallAsync(GameSession game)
        {
            // Call to concede
            await Clients.Group(mGameGroupNamePrefix + game.Id).Concede();
            // Log it
            mLogger.LogDebugSource($"The game {game.Id} ends.");
        }

        /// <summary>
        /// Method to call <see cref="IGameClient.ReceiveChatMessage(string, string)"/> to the players
        /// </summary>
        /// <param name="nickname">The nickname</param>
        /// <param name="message">The message</param>
        private async Task ReceiveChatMessageCallAsync(GameSession game, string nickname, string message)
        {
            // Call to send chat message
            await Clients.Group(mGameGroupNamePrefix + game.Id).ReceiveChatMessage(nickname, message);
            // Log it
            mLogger.LogDebugSource($"New message in the game {game.Id} from '{nickname}'.");
        }

        #endregion

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
