using Ixs.DNA;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

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

        #endregion

        #region Requests

        #endregion
    }
}
