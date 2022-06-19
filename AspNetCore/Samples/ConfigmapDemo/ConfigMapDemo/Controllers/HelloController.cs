using Microsoft.AspNetCore.Mvc;

namespace ConfigMapDemo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HelloController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<HelloController> _logger;
        private readonly IConfiguration _config;

        public HelloController(ILogger<HelloController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _config = configuration;
        }

        [HttpGet()]
        public string Get()
        {
            return _config["test"];
        }
    }
}