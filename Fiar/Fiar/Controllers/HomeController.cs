using Fiar.ViewModels;
using Ixs.DNA;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

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

        #region Pages

        /// <summary>
        /// Welcome page (index)
        /// </summary>
        public IActionResult Index()
        {
            // If no user exists...
            if (!mUserManager.Users.Any())
                ViewData["notInitialized"] = true;

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

        #endregion

        #region Requests

        /// <summary>
        /// Creates the admin user (only during initialization - no users exist)
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(WebRoutes.InitializeRequest)]
        public async Task<IActionResult> InitializeRequestAsync()
        {
            // If any user already exists
            if (mUserManager.Users.Any())
                return RedirectToAction(nameof(Index));

            // Create roles
            await mRoleManager.CreateAsync(new IdentityRole(RoleNames.Administrator));
            await mRoleManager.CreateAsync(new IdentityRole(RoleNames.Player));

            // Define new user
            var user = new ApplicationUser
            {
                UserName = "admin",
                Email = "admin@fiar.admin",
            };
            // Create admin with default password
            var result = await mUserManager.CreateAsync(user, "Admin123456789");
            // Add administrator role to the newly created user
            await mUserManager.AddToRoleAsync(user, RoleNames.Administrator);

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// An login page request
        /// </summary>
        /// <param name="returnUrl">The url to return to if successfully logged in</param>
        /// <param name="user">The user credentials</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(WebRoutes.LoginRequest)]
        public async Task<IActionResult> LoginRequestAsync(string returnUrl, [Bind(
            nameof(Put_LoginCredentialsApiModel.UsernameOrEmail) + "," +
            nameof(Put_LoginCredentialsApiModel.Password) + "," +
            nameof(Put_LoginCredentialsApiModel.StayLoggedIn))] Put_LoginCredentialsApiModel user)
        {
            // If model binding is valid...
            if (ModelState.IsValid)
            {
                // Sign out any previous session
                await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

                // Sign user in with the valid credentials
                var result = await mSignInManager.PasswordSignInAsync(user.UsernameOrEmail, user.Password, user.StayLoggedIn, false);

                // If successful...
                if (result.Succeeded)
                {
                    // If we have no return URL...
                    if (string.IsNullOrEmpty(returnUrl))
                        // Go to home
                        return RedirectToAction(nameof(Index));
                    // Otherwise, go to the return url
                    return Redirect(returnUrl);
                }
            }

            // DO not return password
            user.Password = "";
            // Otherwise, go to the login view and try log in again
            return View(nameof(Login), user);
        }

        /// <summary>
        /// An register page request
        /// </summary>
        /// <param name="user">The user credentials</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(WebRoutes.RegisterRequest)]
        public async Task<IActionResult> RegisterRequestAsync([Bind(
            nameof(Put_RegisterCredentialsApiModel.Username) + "," +
            nameof(Put_RegisterCredentialsApiModel.Email) + "," +
            nameof(Put_RegisterCredentialsApiModel.Password))] Put_RegisterCredentialsApiModel user)
        {
            // If model binding is valid...
            if (ModelState.IsValid)
            {
                // Create the desired user from the given details
                var newUser = new ApplicationUser
                {
                    // USER-SPEC-HERE (specific for registration)
                    UserName = user.Username,
                    Email = user.Email,
                    Nickname = user.Username,
                };

                // Try and create a user
                var result = await mUserManager.CreateAsync(newUser, user.Password);

                // If the registration was successful...
                if (result.Succeeded)
                {
                    // Add the user to desired role
                    await mUserManager.AddToRoleAsync(newUser, RoleNames.Player);

                    // Generate an email verification code
                    var emailVerificationCode = mUserManager.GenerateEmailConfirmationTokenAsync(newUser);

                    // TODO: Email the user the verification code (server-side) https://youtu.be/iYFP26_zI98?t=2407

                    // Go to login
                    return RedirectToAction(nameof(Login), new { register = string.Empty });
                }
                else
                {
                    ViewData["ufeedback"] = result.Errors.AggregateErrors();
                }
            }

            // DO not return password
            user.Password = "";
            // Otherwise, go to the login view and try log in again
            return View(nameof(Register), user);
        }

        /// <summary>
        /// Log the user out
        /// </summary>
        [Authorize]
        [Route(WebRoutes.LogoutRequest)]
        public async Task<IActionResult> LogoutRequestAsync()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            return RedirectToAction(nameof(Index));
        }

        #endregion
    }
}
