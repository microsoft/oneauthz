// -------------------------------------------------------------------------------
// <copyright file="HomeController.cs" company="Microsoft Corporation">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------

namespace UserSyncCore.Controllers
{
    using System;
    using System.Diagnostics;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using UserSyncCore.Models;

    /// <summary>
    /// Controller for main home page. A common header provides navigation
    /// to other pages. User is authenticated by middleware before controller
    /// actions are invoked.
    /// </summary>
    [Authorize]
    public class HomeController : Controller
    {
        /// <summary>
        /// Logger is created at app startup and passed in by DI.
        /// </summary>
        private readonly ILogger<HomeController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController"/> class.
        /// </summary>
        /// <param name="logger">ILogger<HomeController></param>
        public HomeController(ILogger<HomeController> logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Action for GET of home page and render view.
        /// </summary>
        /// <returns>IActionResult</returns>
        public IActionResult Index()
        {
            ViewResult view;

            try
            {
                view = this.View();
            }
            catch (Exception e)
            {
                this.logger.LogError(e.Message);
                return this.Redirect("/Error");
            }

            return this.View();
        }

        /// <summary>
        /// Action for GET of privacy page and render privacy view.
        /// </summary>
        /// <returns>IActionResult</returns>
        public IActionResult Privacy()
        {
            ViewResult view;

            try
            {
                view = this.View();
            }
            catch (Exception e)
            {
                this.logger.LogError(e.Message);
                return this.Redirect("/Error");
            }

            return this.View();
        }

        /// <summary>
        /// Invoke render of error view whenever an error occurs.
        /// </summary>
        /// <returns>IActionResult</returns>
        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return this.View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? this.HttpContext.TraceIdentifier });
        }
    }
}
