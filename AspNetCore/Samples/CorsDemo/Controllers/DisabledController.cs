using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CorsDemo.Controllers
{
    /// <summary>
    /// 演示不使用任何 EnableCorsAttribute 效果就是未开启 CORS 的效果
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class DisabledController : ControllerBase
    {
        private readonly ILogger<DisabledController> _logger;

        public DisabledController(ILogger<DisabledController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Content("Ok");
        }
    }
}
