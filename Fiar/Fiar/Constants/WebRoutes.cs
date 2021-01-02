namespace Fiar
{
    /// <summary>
    /// The relative routes to all normal (non-API) calls in the server
    /// </summary>
    public static class WebRoutes
    {
        /// <summary>
        /// The route to the web page
        /// </summary>
        public const string Login = "/login";

        /// <summary>
        /// The route to the web page
        /// </summary>
        public const string Register = "/register";

        /// <summary>
        /// The route to the web page
        /// </summary>
        public const string ForgotPassword = "/forgotpassword";

        /// <summary>
        /// The route to the web page
        /// </summary>
        public const string ResendVerificationEmail = "/resendconfirmationemail";

        /// <summary>
        /// The route to the web page
        /// </summary>
        public const string VerifyEmail = "/verify/email/{uid}/{etoken}";

        /// <summary>
        /// The route to the web page
        /// </summary>
        public const string UserEdit = "/user/edit";

        /// <summary>
        /// The route to the web page
        /// </summary>
        public const string ConfigDump = "/config/dump";

        /// <summary>
        /// The route to the web page
        /// </summary>
        public const string Lobby = "/lobby";

        /// <summary>
        /// The route to the web page
        /// </summary>
        public const string Game = "/game";

        /// <summary>
        /// The route to the web page
        /// </summary>
        public const string Error400 = "/400";

        /// <summary>
        /// The route to the web page
        /// </summary>
        public const string Error401 = "/401";

        /// <summary>
        /// The route to the web page
        /// </summary>
        public const string Error403 = "/403";

        /// <summary>
        /// The route to the web page
        /// </summary>
        public const string Error404 = "/404";

        /// <summary>
        /// The route to the web page
        /// </summary>
        public const string Error500 = "/500";

        /// <summary>
        /// The route to the web request method
        /// </summary>
        public const string InitializeRequest = "/request/initialize";

        /// <summary>
        /// The route to the web request method
        /// </summary>
        public const string LoginRequest = "/request/login";

        /// <summary>
        /// The route to the web request method
        /// </summary>
        public const string LogoutRequest = "/request/logout";

        /// <summary>
        /// The route to the web request method
        /// </summary>
        public const string RegisterRequest = "/request/register";

        /// <summary>
        /// The route to the web request method
        /// </summary>
        public const string ResendVerificationEmailRequest = "/request/verify/email/resend";

        /// <summary>
        /// The route to the web request method
        /// </summary>
        public const string UserEditProfileDataRequest = "/request/user/profile/edit";

        /// <summary>
        /// The route to the web request method
        /// </summary>
        public const string UserEditProfilePasswordRequest = "/request/user/password/edit";

        /// <summary>
        /// The route to the web request method
        /// </summary>
        public const string ForgetPassowrdRequest = "/request/user/password/reset";

        /// <summary>
        /// The route to the web request method
        /// </summary>
        public const string UserEditProfileDataAclRequest = "/request/acl/user/profile/edit";

        /// <summary>
        /// The route to the web request method
        /// </summary>
        public const string UserEditProfilePasswordAclRequest = "/request/acl/user/password/edit";

        /// <summary>
        /// The route to the web request method
        /// </summary>
        public const string GetGameReplayDataGameRequest = "/request/game/replay/data";
    }
}
