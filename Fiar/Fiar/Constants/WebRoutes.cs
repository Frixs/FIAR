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
        public const string ConfigDump = "/config/dump";

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
    }
}
