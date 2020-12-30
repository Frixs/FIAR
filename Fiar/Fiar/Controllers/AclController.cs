using Fiar.Attributes;
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
            var users = mContext.Users.OrderBy(o => o.UserName).ToList();
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
        public async Task<IActionResult> UserEdit(string u)
        {
            var user = await GetUserEdit(u);
            if (user == null)
                return Redirect(nameof(Index));
            return View(user);
        }

        #endregion

        #region Requests

        /// <summary>
        /// An edit user profile page request
        /// </summary>
        /// <param name="user">The user data</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(WebRoutes.UserEditProfileDataAclRequest)]
        public async Task<IActionResult> UserEditProfileDataRequestAsync([Bind(
            nameof(UserProfileViewModel.Id) + "," +
            nameof(UserProfileViewModel.Username) + "," +
            nameof(UserProfileViewModel.Email) + "," +
            nameof(UserProfileViewModel.Nickname))] UserProfileViewModel user, string[] selectedRoles)
        {
            var appRoles = mRoleManager.Roles.ToList();
            ApplicationUser dbUser = null;

            // If model binding is valid...
            if (ModelState.IsValid)
            {
                if (!user.Id.IsNullOrEmpty())
                {
                    // Get the user from database
                    dbUser = mContext.Users.Find(user.Id);
                    if (dbUser != null)
                    {
                        // Validate the model
                        var userToValidate = new UserDataModel
                        {
                            Username = user.Username,
                            Email = user.Email,
                            Nickname = user.Nickname
                        };
                        var validationResult = typeof(UserDataModel).GetAttribute<ValidableModelAttribute>().Validate(userToValidate);
                        if (validationResult.Succeeded)
                        {
                            // Update edited fields
                            dbUser.UserName = userToValidate.Username;
                            dbUser.Email = userToValidate.Email;
                            dbUser.Nickname = userToValidate.Nickname;

                            // Update
                            var updateResult = await mUserManager.UpdateAsync(dbUser);

                            // If the user update was successful...
                            if (updateResult.Succeeded)
                            {
                                // Check the roles
                                // We want to have user at least 1 role
                                if (selectedRoles.Length > 0)
                                {
                                    // Remove all previous roles first
                                    foreach (var role in appRoles)
                                    {
                                        if (await mUserManager.IsInRoleAsync(dbUser, role.Name))
                                        {
                                            if (role.Name.Equals(RoleNames.Administrator) && (await mUserManager.GetUsersInRoleAsync(role.Name)).Count() <= 1)
                                            {
                                                // Do nothing - there should be always at least 1 administrator
                                            }
                                            else
                                            {
                                                await mUserManager.RemoveFromRoleAsync(dbUser, role.Name);
                                            }
                                        }
                                    }
                                    // Add user to the new selected roles
                                    foreach (var role in selectedRoles)
                                        if (await mRoleManager.RoleExistsAsync(role))
                                            await mUserManager.AddToRoleAsync(dbUser, role);

                                    // Log it
                                    mLogger.LogInformation($"User {dbUser.UserName} has been updated!");

                                    // Go back to ACL
                                    return RedirectToAction(nameof(Index));
                                }
                                else
                                {
                                    ViewData["ufeedback_failure"] = "User must be assigned at least to 1 role.";
                                }
                            }
                            else
                            {
                                ViewData["ufeedback_failure"] = updateResult.Errors.AggregateErrors();
                            }
                        }
                        else
                        {
                            ViewData["ufeedback_failure"] = validationResult.Errors.AggregateErrors();
                        }
                    }
                    else
                    {
                        ViewData["ufeedback_failure"] = "The user does not exist.";
                    }
                }
                else
                {
                    ViewData["ufeedback_failure"] = "Something went wrong while parsing user ID.";
                }
            }

            // Go back to the view
            user = await GetUserEdit(user?.Id);
            if (user == null)
                return Redirect(nameof(Index));
            // Bind back selected roles into the model
            user.RoleListView.Clear();
            foreach (var role in appRoles)
                user.RoleListView.Add(role.Name, selectedRoles.Contains(role.Name));

            return View(nameof(UserEdit), user);
        }

        /// <summary>
        /// An edit user password page request
        /// </summary>
        /// <param name="user">The user data</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(WebRoutes.UserEditProfilePasswordAclRequest)]
        public async Task<IActionResult> UserEditProfilePasswordRequestAsync([Bind(
            nameof(UserProfileViewModel.Id) + "," +
            nameof(UserProfileViewModel.CurrentPassword) + "," +
            nameof(UserProfileViewModel.NewPassword))] UserProfileViewModel user)
        {
            var appRoles = mRoleManager.Roles.ToList();
            ApplicationUser dbUser = null;

            // If model binding is valid...
            if (ModelState.IsValid)
            {
                if (!user.Id.IsNullOrEmpty())
                {
                    // Get the user from database
                    dbUser = mContext.Users.Find(user.Id);
                    if (dbUser != null)
                    {
                        // Attempt to commit changes to data store
                        var result = await mUserManager.ChangePasswordAsync(dbUser, user.CurrentPassword, user.NewPassword);
                        if (result.Succeeded)
                        {
                            // Log it
                            mLogger.LogInformation($"User {dbUser.UserName} has successfully changed password!");

                            // Go back to ACL
                            return RedirectToAction(nameof(Index));
                        }
                        else
                        {
                            ViewData["ufeedback_failure"] = result.Errors.AggregateErrors();
                        }
                    }
                    else
                    {
                        ViewData["ufeedback_failure"] = "The user does not exist.";
                    }
                }
                else
                {
                    ViewData["ufeedback_failure"] = "Something went wrong while parsing user ID.";
                }
            }

            // Go back to the view
            user = await GetUserEdit(user?.Id);
            if (user == null)
                return Redirect(nameof(Index));
            return View(nameof(UserEdit), user);
        }

        #endregion

        #region Helpers

        /// <summary>
        ///  Gets the user with all required data for edit forms
        /// </summary>
        /// <param name="u">The user ID</param>
        /// <returns>The user view model</returns>
        private async Task<UserProfileViewModel> GetUserEdit(string u)
        {
            var user = mContext.Users.Find(u);
            if (user == null)
                return null;

            var userProfile = new UserProfileViewModel
            {
                Id = user.Id,
                Username = user.UserName,
                Email = user.Email,
                Nickname = user.Nickname
            };
            // Get user roles
            userProfile.RoleList = (await mUserManager.GetRolesAsync(user)).ToList();
            userProfile.RoleListView = new Dictionary<string, bool>();
            var roles = mRoleManager.Roles.ToList();
            foreach (var role in roles)
                userProfile.RoleListView.Add(role.Name, userProfile.RoleList.Contains(role.Name));

            return userProfile;
        }

        #endregion
    }
}
