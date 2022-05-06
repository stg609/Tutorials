using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

namespace TrivialDemo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoggerMessagesController : ControllerBase
    {
        private readonly ILogger<LoggerMessagesController> _logger;

        public LoggerMessagesController(ILogger<LoggerMessagesController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {
            LoggerHelper.LogInformation(_logger, "abc");
            LoggerHelper.LogWarning(_logger, new ArgumentNullException(), "def");
            return Ok();
        }
    }

    /// <summary>
    /// 以下方法必须要求 .Net 6 及以上
    /// </summary>
    public static partial class LoggerHelper
    {
        [LoggerMessage(0, LogLevel.Information, "Do something {message}")]
        public static partial void LogInformation(ILogger logger, string message);

        [LoggerMessage(0, LogLevel.Warning, "Do something {message}")]
        public static partial void LogWarning(ILogger logger, Exception ex, string message);
    }
}
