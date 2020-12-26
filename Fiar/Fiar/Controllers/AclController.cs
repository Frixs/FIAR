using Fiar.ViewModels.Acl;
using Ixs.DNA;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fiar
{
    /// <summary>
    /// Manages the ACL
    /// </summary>
    [Authorize(Roles = RoleNames.Administrator)]
    [Route("acl")]
    public class AclController : Controller
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
        public AclController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager, IConfigBox configBox)
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
        /// Base page (index)
        /// </summary>
        public async Task<IActionResult> Index()
        {
            UserListViewModel result = new UserListViewModel();
            result.Users = new List<UserProfileViewModel>();

            // Get all existing users
            var users = mContext.Users.ToList();
            foreach (var u in users)
            {
                // Create user profile view model
                var userProfile = new UserProfileViewModel
                {
                    Id = u.Id,
                    Username = u.UserName,
                    Email = u.Email,
                    Nickname = u.Nickname
                };
                // Get user roles
                userProfile.RoleList = (await mUserManager.GetRolesAsync(u)).ToList();

                result.Users.Add(userProfile);
            }

            return View(result);
        }

        /// <summary>
        /// Base page (index)
        /// </summary>
        /// <param name="u">User ID to edit</param>
        [Route("user/edit")]
        public IActionResult UserEdit(string u)
        {
            var user = mContext.Users.Find(u);
            if (user == null)
                return Redirect(nameof(Index));

            var userProfile = new UserProfileViewModel
            {
                Id = user.Id,
                Username = user.UserName,
                Email = user.Email,
                Nickname = user.Nickname
            };

            return View(userProfile);
        }

        #endregion

        #region Requests

        #endregion
    }
}
