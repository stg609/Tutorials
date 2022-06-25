using Microsoft.AspNetCore.Mvc;

namespace CrashDemo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CrashController : ControllerBase
    {
        internal static bool _crashed = false;
        private readonly ILogger<CrashController> _logger;
        private static List<byte[]> _items = new List<byte[]>();

        public CrashController(ILogger<CrashController> logger)
        {
            _logger = logger;
        }

        [HttpGet("Unhealthy")]
        public ActionResult Unhealthy()
        {
            _crashed = true;
            return Ok();
        }


        [HttpGet("Healthy")]
        public ActionResult Healthy()
        {
            _crashed = false;
            return Ok();
        }

        [HttpGet("OOM/{max}")]
        public ActionResult OOM(int max = 1000)
        {
            for (int i = 0; i < max; i++)
            {
                byte[] c = new byte[10000];
                _items.Add(c);
            }

            return Ok();
        }
    }
}