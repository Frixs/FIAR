using Fiar.ViewModels;
using Ixs.DNA;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;

namespace Fiar.Controllers
{
    /// <summary>
    /// Manages the base web server pages
    /// </summary>
    public class HomeController : Controller
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
        private readonly ILogger mLogger;

        /// <summary>
        /// Injection - <inheritdoc cref="IConfigBox"/>
        /// </summary>
        private readonly IConfigBox mConfigBox;

        #endregion
        
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="context">The injected context</param>
        /// <param name="userManager">The Identity user manager</param>
        /// <param name="signInManager">The Identity sign in manager</param>
        /// <param name="roleManager">The Identity role manager</param>
        public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager, IConfigBox configBox)
        {
            mContext = context;
            mUserManager = userManager;
            mSignInManager = signInManager;
            mRoleManager = roleManager;
            mConfigBox = configBox ?? throw new ArgumentNullException(nameof(configBox));
            mLogger = FrameworkDI.Logger ?? throw new ArgumentNullException(nameof(mLogger));
        }

        #endregion

        /// <summary>
        /// Welcome page (index)
        /// </summary>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Login page
        /// </summary>
        [Route(WebRoutes.Login)]
        public IActionResult Login()
        {
            return View();
        }

        /// <summary>
        /// Login page
        /// </summary>
        [Route(WebRoutes.Register)]
        public IActionResult Register()
        {
            return View();
        }

        /// <summary>
        /// Dump config-box
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = RoleNames.Administrator)]
        [Route(WebRoutes.ConfigDump)]
        public string ConfigBoxDump()
        {
            return DI.ConfigBox.Dump();
        }

        /// <summary>
        /// Error handling for dev mode
        /// </summary>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
