namespace WhMgr.Controllers
{
    using System;
    using System.Net.Mime;

    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    [ApiController]
    [Route("/api/v1/")]
    public class ManagementApiController : ControllerBase
    {
        private readonly ILogger<ManagementApiController> _logger;
        private readonly IHostApplicationLifetime _appLifetime;

        public ManagementApiController(
            ILogger<ManagementApiController> logger,
            IHostApplicationLifetime appLifetime)
        {
            _logger = logger;
            _appLifetime = appLifetime;
        }

        [HttpGet("restart")]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult Restart()
        {
            _appLifetime.StopApplication();
            Program.Restart();

            var status = "OK";
            return new JsonResult(new
            {
                status,
                message = status == "OK"
                    ? "Application successfully restarted."
                    : "Failed to restart application.",
            });
        }
    }
}