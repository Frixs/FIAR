using Microsoft.AspNetCore.Mvc;

namespace Fiar
{
    /// <summary>
    /// Manages the error web server pages
    /// https://joonasw.net/view/custom-error-pages
    /// </summary>
    public class ErrorController : Controller
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public ErrorController()
        {
        }

        #endregion

        /// <summary>
        /// Application error page - 400
        /// </summary>
        [Route(WebRoutes.Error400)]
        public IActionResult AppError400()
        {
            return View();
        }

        /// <summary>
        /// Application error page - 401
        /// </summary>
        [Route(WebRoutes.Error401)]
        public IActionResult AppError401()
        {
            return View();
        }

        /// <summary>
        /// Application error page - 403
        /// </summary>
        [Route(WebRoutes.Error403)]
        public IActionResult AppError403()
        {
            return View();
        }

        /// <summary>
        /// Application error page - 404
        /// </summary>
        [Route(WebRoutes.Error404)]
        public IActionResult AppError404()
        {
            return View();
        }

        /// <summary>
        /// Application error page - 500
        /// </summary>
        [Route(WebRoutes.Error500)]
        public IActionResult AppError500()
        {
            return View();
        }
    }
}
