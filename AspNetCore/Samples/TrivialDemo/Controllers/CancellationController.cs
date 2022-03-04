using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace TrivialDemo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CancellationController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<CancellationController> _logger;

        public CancellationController(ILogger<CancellationController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get(CancellationToken cancellationToken)
        {
            // 因为不是 await，所以这个请求会很快正常结束（cancellationToken 并不会被设置，所以下面的 break 是无效的）
            Task.Run(() =>
            {
                // 注意，这个 Task.Run 是可以在 action 结束后继续执行的，
                // 但是如果内部访问了很多 scoped 的服务，那当请求结束，这些服务会被 Dispose
                // 如果不想 Dispose，那 Task 内部要手动创建一个新的 scope, see  https://stackoverflow.com/a/64892214/6294524
                while (true)
                {
                    if(cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    System.Console.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff"));
                    Thread.Sleep(1000);
                }
            }, cancellationToken);

            // 等待 5 s, 如果这期间结束这个请求（比如 关闭浏览器 tab 页），那么 cancellationToken 就会标志为 cancelled。
            // 如果超过5s,那将永远无法结束上面的循环
            Thread.Sleep(5000);
            return Content("Ok");
        }
    }
}
