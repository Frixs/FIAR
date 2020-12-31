using Ixs.DNA;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Fiar.ViewModels;

namespace Fiar
{
    /// <summary>
    /// Manages the game
    /// </summary>
    [Authorize(Roles = RoleNames.Player)]
    [Route("game")]
    public class GameController : Controller
    {
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
        /// The manager for handling signing in and out for our users
        /// </summary>
        protected SignInManager<ApplicationUser> mSignInManager;

        /// <summary>
        /// The manager for handling user roles
        /// </summary>
        protected RoleManager<IdentityRole> mRoleManager;

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
        /// <param name="context">The injected context</param>
        /// <param name="userManager">The Identity user manager</param>
        /// <param name="signInManager">The Identity sign in manager</param>
        /// <param name="roleManager">The Identity role manager</param>
        public GameController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager, ILogger logger, IConfigBox configBox)
        {
            mContext = context;
            mUserManager = userManager;
            mSignInManager = signInManager;
            mRoleManager = roleManager;
            mConfigBox = configBox ?? throw new ArgumentNullException(nameof(configBox));
            mLogger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Pages

        /// <summary>
        /// Base page (index)
        /// </summary>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Game replay page
        /// </summary>
        [Route("replay")]
        public IActionResult Replay(long gid)
        {
            ViewData["replayGameId"] = gid;
            return View();
        }

        #endregion

        #region Requests

        /// <summary>
        /// Get replay data based on game ID
        /// </summary>
        /// <param name="gid">The game ID</param>
        [HttpPost]
        [Route(WebRoutes.GetGameReplayDataGameRequest)]
        public async Task<GameReplayDataViewModel> GetGameReplayDataRequestAsync([Bind("gid")] long gid)
        {
            // Get user by the claims
            var user = await mUserManager.GetUserAsync(HttpContext.User);
            if (user == null)
                return null;

            bool isAllowedToReplay = false;
            GameReplayDataViewModel result = new GameReplayDataViewModel();
            Player playerOne = null;
            Player playerTwo = null;
            List<GameBoardCellType[][]> boardHistory = new List<GameBoardCellType[][]>();

            // Get the game
            var game = mContext.Games.Find(gid);
            if (game == null)
                return null;

            // Get the participants
            var participants = mContext.GameParticipants
                .Where(o => o.GameId == gid)
                .Include(o => o.User)
                .ToList();
            foreach (var p in participants)
            {
                if (p.Type == PlayerType.PlayerTwo)
                {
                    playerTwo = Player.Convert(p.User, null, PlayerType.PlayerTwo);
                    result.PlayerTwo = p.User.Nickname;
                }
                else
                {
                    playerOne = Player.Convert(p.User, null, PlayerType.PlayerOne);
                    result.PlayerOne = p.User.Nickname;
                }

                if (p.User.Id.Equals(user.Id))
                    isAllowedToReplay = true;
            }

            // Check if the user is allowed to replay
            if (!isAllowedToReplay)
                return null;

            // Get all game moves
            var moves = mContext.GameMoves
                .Where(o => o.GameId == gid)
                .OrderBy(o => o.RecordedAt)
                .ToList();

            // Create replay game session
            var gameSession = new GameSession(null, null)
            {
                PlayerOne = playerOne ?? new Player(PlayerType.PlayerOne) { Id = "id", ConnectionId = null, Nickname = "---" },
                PlayerTwo = playerTwo ?? new Player(PlayerType.PlayerTwo) { Id = "id", ConnectionId = null, Nickname = "---" },
                InProgress = true
            };

            // Generate gameplay
            foreach (var m in moves)
            {
                // Save the board state
                boardHistory.Add(gameSession.Board);

                // If the current player is not set yet...
                if (gameSession.CurrentPlayer == null)
                    // Set it then
                    gameSession.MoveTurnPointerToNextPlayer();

                // Try to assign player turn into the board
                gameSession.TryAssignPlayerToCell(m.PosY, m.PosX);

                // Check victory conditions (only the current player can win)
                if (gameSession.CheckVictory(m.PosY, m.PosX))
                    break;

                // Check for expanding the board
                gameSession.TryExpandBoard(m.PosY, m.PosX);

                // Move to the next turn
                gameSession.MoveTurnPointerToNextPlayer();
            }
            result.BoardHistory = boardHistory;

            return result;
        }

        #endregion
    }
}
