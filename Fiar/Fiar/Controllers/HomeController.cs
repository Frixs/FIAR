using Fiar.Attributes;
using Fiar.ViewModels;
using Fiar.ViewModels.Acl;
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
using System.Web;

namespace Fiar
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
        protected readonly ILogger mLogger;

        /// <summary>
        /// Injection - <inheritdoc cref="IConfigBox"/>
        /// </summary>
        protected readonly IConfigBox mConfigBox;

        /// <summary>
        /// Injection - <inheritdoc cref="IEmailSender"/>
        /// </summary>
        protected readonly IEmailSender mEmailSender;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="context">The injected context</param>
        /// <param name="userManager">The Identity user manager</param>
        /// <param name="signInManager">The Identity sign in manager</param>
        /// <param name="roleManager">The Identity role manager</param>
        public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager, ILogger logger, IConfigBox configBox, IEmailSender emailSender)
        {
            mContext = context;
            mUserManager = userManager;
            mSignInManager = signInManager;
            mRoleManager = roleManager;
            mConfigBox = configBox ?? throw new ArgumentNullException(nameof(configBox));
            mLogger = logger ?? throw new ArgumentNullException(nameof(logger));
            mEmailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
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
        /// Register page
        /// </summary>
        [Route(WebRoutes.Register)]
        public IActionResult Register()
        {
            return View();
        }

        /// <summary>
        /// ForgotPassword page
        /// </summary>
        [Route(WebRoutes.ForgotPassword)]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        /// <summary>
        /// ForgotPassword page
        /// </summary>
        [Route(WebRoutes.ResetPassword)]
        public IActionResult ResetPassword(string uid, string ptoken)
        {
            // NOTE: Issue with URL decoding containing /s does not replace them
            //       https://github.com/aspnet/Home/issues/2669
            //
            //       Manual fix here:
            ptoken = ptoken.Replace("%2f", "/").Replace("%2F", "/");

            return View(new ResetPasswordViewModel
            {
                Id = uid,
                Token = ptoken
            });
        }

        /// <summary>
        /// Resend confirmation email page
        /// </summary>
        [Route(WebRoutes.ResendVerificationEmail)]
        public IActionResult ResendVerificationEmail()
        {
            return View();
        }

        /// <summary>
        /// Verify email page
        /// </summary>
        [Route(WebRoutes.VerifyEmail)]
        public async Task<IActionResult> VerifyEmail(string uid, string etoken)
        {
            // NOTE: Issue with URL decoding containing /s does not replace them
            //       https://github.com/aspnet/Home/issues/2669
            //
            //       Manual fix here:
            etoken = etoken.Replace("%2f", "/").Replace("%2F", "/");

            // Verify if the ID matches
            var user = await mUserManager.FindByIdAsync(uid);
            if (user == null)
            {
                ViewData["ufeedback_failure"] = "User not found!";
                return View();
            }

            // If we have the user...

            // Verify the email token
            var result = await mUserManager.ConfirmEmailAsync(user, etoken);

            // If succeeded...
            if (result.Succeeded)
            {
                ViewData["ufeedback_failure"] = "Email verified!";
                return View();
            }

            ViewData["ufeedback_failure"] = "Invalid token!";
            return View();
        }

        /// <summary>
        /// User edit page
        /// </summary>
        [Authorize]
        [Route(WebRoutes.UserEdit)]
        public async Task<IActionResult> UserEdit()
        {
            // Get user by the claims
            var user = await mUserManager.GetUserAsync(HttpContext.User);
            if (user == null)
                return Redirect(WebRoutes.Login);

            var userView = GetUserEdit(user);
            if (userView == null)
                return Redirect(nameof(Index));
            return View(userView);
        }

        /// <summary>
        /// Creates the admin user (only during initialization - no users exist)
        /// </summary>
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
                Nickname = "Administrator69",
            };
            // Create admin with default password
            var result = await mUserManager.CreateAsync(user, "Admin123456789");
            // Add administrator role to the newly created user
            await mUserManager.AddToRoleAsync(user, RoleNames.Administrator);
            await mUserManager.AddToRoleAsync(user, RoleNames.Player);

            return RedirectToAction(nameof(Index));
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

                ViewData["ufeedback_failure"] = "Invalid credentials or your email is not verified.";
            }
            // Do not return password
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
                    var emailVerificationCode = await mUserManager.GenerateEmailConfirmationTokenAsync(newUser);
                    var confirmationUrl = $"{Request.Scheme}://" + Request.Host.Value + WebRoutes.VerifyEmail.Replace("{uid}", HttpUtility.UrlEncode(newUser.Id)).Replace("{etoken}", HttpUtility.UrlEncode(emailVerificationCode));

                    // Email the user the verification code
                    await AppEmailSender.SendUserEmailVerificationAsync(newUser.Email, confirmationUrl);

                    // Log it
                    mLogger.LogInformation($"New user {newUser.UserName} has been registered!");

                    // Give feedback
                    ViewData["ufeedback_success"] = "Successfully registered! Confirmation email has been send to your email address.";

                    // Go to view
                    return View(nameof(Register), null);
                }
                else
                {
                    ViewData["ufeedback_failure"] = result.Errors.AggregateErrors();
                }
            }

            // DO not return password
            user.Password = "";
            // Otherwise, go to the login view and try log in again
            return View(nameof(Register), user);
        }

        /// <summary>
        /// resend confirmation email
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(WebRoutes.ResendVerificationEmailRequest)]
        public async Task<IActionResult> ResendVerificationEmailRequestAsync([Bind(
            nameof(Put_LoginCredentialsApiModel.UsernameOrEmail))] Put_LoginCredentialsApiModel data)
        {
            // If model binding is valid...
            if (ModelState.IsValid)
            {
                if (data?.UsernameOrEmail != null)
                {
                    // Try to find the user
                    var user = mContext.Users.FirstOrDefault(o => o.UserName.Equals(data.UsernameOrEmail) || o.Email.Equals(data.UsernameOrEmail));

                    // If the user exists...
                    if (user != null)
                    {
                        if (!user.EmailConfirmed)
                        {
                            // Generate an email verification code
                            var emailVerificationCode = await mUserManager.GenerateEmailConfirmationTokenAsync(user);
                            var confirmationUrl = $"{Request.Scheme}://" + Request.Host.Value + WebRoutes.VerifyEmail.Replace("{uid}", HttpUtility.UrlEncode(user.Id)).Replace("{etoken}", HttpUtility.UrlEncode(emailVerificationCode));

                            // Email the user the verification code
                            await AppEmailSender.SendUserEmailVerificationAsync(user.Email, confirmationUrl);

                            // Give feedback
                            ViewData["ufeedback_success"] = "Confirmation email has been send to your email address.";

                            // Go to view
                            return View(nameof(ResendVerificationEmail));
                        }
                        else
                        {
                            ViewData["ufeedback_failure"] = "Email is already confirmed.";
                        }
                    }
                    else
                    {
                        ViewData["ufeedback_failure"] = "User does not exist!";
                    }
                }
            }

            // Otherwise, go to the view
            return View(nameof(ResendVerificationEmail), data);
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

        /// <summary>
        /// An edit user profile page request
        /// </summary>
        /// <param name="data">The user data</param>
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [Route(WebRoutes.UserEditProfileDataRequest)]
        public async Task<IActionResult> UserEditProfileDataRequestAsync([Bind(
            nameof(UserProfileViewModel.Nickname))] UserProfileViewModel data, string[] selectedRoles)
        {
            // If model binding is valid...
            if (ModelState.IsValid)
            {
                // Get user by the claims
                var user = await mUserManager.GetUserAsync(HttpContext.User);
                if (user == null)
                    return Redirect(WebRoutes.Login);

                // Validate the model
                var userToValidate = new UserDataModel
                {
                    Username = user.UserName,
                    Email = user.Email,
                    Nickname = data.Nickname
                };
                var validationResult = typeof(UserDataModel).GetAttribute<ValidableModelAttribute>().Validate(userToValidate);
                if (validationResult.Succeeded)
                {
                    // Update edited fields
                    user.UserName = userToValidate.Username;
                    user.Email = userToValidate.Email;
                    user.Nickname = userToValidate.Nickname;

                    // Update
                    var updateResult = await mUserManager.UpdateAsync(user);

                    // If the user update was successful...
                    if (updateResult.Succeeded)
                    {
                        // Log it
                        mLogger.LogInformation($"User {user.UserName} has been updated!");

                        // Go back to Index
                        return RedirectToAction(nameof(Index));
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

                // Go back to the view
                return View(nameof(UserEdit), GetUserEdit(user));
            }

            // Go back to the page
            return RedirectToAction(nameof(UserEdit));
        }

        /// <summary>
        /// An edit user password page request
        /// </summary>
        /// <param name="data">The user data</param>
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [Route(WebRoutes.UserEditProfilePasswordRequest)]
        public async Task<IActionResult> UserEditProfilePasswordRequestAsync([Bind(
            nameof(UserProfileViewModel.Id) + "," +
            nameof(UserProfileViewModel.CurrentPassword) + "," +
            nameof(UserProfileViewModel.NewPassword))] UserProfileViewModel data)
        {
            // If model binding is valid...
            if (ModelState.IsValid)
            {
                // Get user by the claims
                var user = await mUserManager.GetUserAsync(HttpContext.User);
                if (user == null)
                    return Redirect(WebRoutes.Login);

                // Attempt to commit changes to data store
                var result = await mUserManager.ChangePasswordAsync(user, data.CurrentPassword, data.NewPassword);
                if (result.Succeeded)
                {
                    // Log it
                    mLogger.LogInformation($"User {user.UserName} has successfully changed password!");

                    // Go back to page
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ViewData["ufeedback_failure"] = result.Errors.AggregateErrors();
                }

                // Go back to the view
                return View(nameof(UserEdit), GetUserEdit(user));
            }

            // Go back to the page
            return RedirectToAction(nameof(UserEdit));
        }

        /// <summary>
        /// An login page request
        /// </summary>
        /// <param name="returnUrl">The url to return to if successfully logged in</param>
        /// <param name="user">The user credentials</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(WebRoutes.ForgetPassowrdRequest)]
        public async Task<IActionResult> ForgetPassowrdRequestAsync([Bind(
            nameof(Put_LoginCredentialsApiModel.UsernameOrEmail))] Put_LoginCredentialsApiModel data)
        {
            if (data == null || data.UsernameOrEmail.IsNullOrEmpty())
            {
                ViewData["ufeedback_failure"] = "The user data are invalid!";
                // Go back to the view
                return View(nameof(ForgotPassword));
            }

            // Check if the user exists
            var user = mContext.Users.FirstOrDefault(o => o.UserName.Equals(data.UsernameOrEmail) || o.Email.Equals(data.UsernameOrEmail));
            if (user != null)
            {
                // Generate an email verification code
                var passwordVerificationCode = await mUserManager.GeneratePasswordResetTokenAsync(user);
                var confirmationUrl = $"{Request.Scheme}://" + Request.Host.Value + WebRoutes.ResetPassword.Replace("{uid}", HttpUtility.UrlEncode(user.Id)).Replace("{ptoken}", HttpUtility.UrlEncode(passwordVerificationCode));

                // Email the user the verification code
                await AppEmailSender.SendUserPasswordResetVerificationAsync(user.Email, confirmationUrl);

                // Give feedback
                ViewData["ufeedback_success"] = "Confirmation email has been send to your email address.";

                // Go to view
                return View(nameof(ForgotPassword), null);
            }
            else
            {
                ViewData["ufeedback_failure"] = "The user does not exist.";
                // Go back to the view
                return View(nameof(ForgotPassword), data);
            }
        }

        /// <summary>
        /// An edit user password page request
        /// </summary>
        /// <param name="data">The user data</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(WebRoutes.ResetPassowrdRequest)]
        public async Task<IActionResult> ResetPasswordRequestAsync([Bind(
            nameof(ResetPasswordViewModel.Id) + "," +
            nameof(ResetPasswordViewModel.Token) + "," +
            nameof(ResetPasswordViewModel.NewPassword))] ResetPasswordViewModel model)
        {
            // If model binding is valid...
            if (ModelState.IsValid)
            {
                // Verify if the ID matches
                var user = await mUserManager.FindByIdAsync(model.Id);
                if (user == null)
                {
                    // Go back to the page
                    return RedirectToAction(nameof(Index));
                }

                // Attempt to commit changes to data store
                var result = await mUserManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
                if (result.Succeeded)
                {
                    // Log it
                    mLogger.LogInformation($"User {user.UserName} has successfully reset password!");

                    // Go back to page
                    return RedirectToAction(nameof(Login));
                }
                else
                {
                    ViewData["ufeedback_failure"] = result.Errors.AggregateErrors();
                }

                model.NewPassword = string.Empty;
                // Go back to the view
                return View(nameof(ResetPassword), model);
            }

            // Go back to the page
            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region Helpers

        /// <summary>
        ///  Gets the user with all required data for edit forms
        /// </summary>
        /// <param name="user">The user DB</param>
        /// <returns>The user view model</returns>
        private UserProfileViewModel GetUserEdit(ApplicationUser user)
        {
            var userProfile = new UserProfileViewModel
            {
                Id = user.Id,
                Username = user.UserName,
                Email = user.Email,
                Nickname = user.Nickname
            };

            return userProfile;
        }

        #endregion
    }
}
