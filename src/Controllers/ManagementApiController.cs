namespace WhMgr.Controllers
{
    using System;
    using System.Net.Mime;

    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    using WhMgr.Extensions;

    [ApiController]
    [Route("/api/v1/")]
    public class ManagementApiController : ControllerBase
    {
        private readonly Microsoft.Extensions.Logging.ILogger<ManagementApiController> _logger;
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
            string status;
            try
            {
                _appLifetime.StopApplication();
                Program.Restart();
                status = "OK";
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to restart application: {ex}");
                status = "Error";
            }

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