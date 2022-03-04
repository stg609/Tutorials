using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Polly.RateLimit;

namespace PollyDemo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RateLimitController : ControllerBase
    {
        static int i = 0;

        /// <summary>
        /// 设置每10秒最多10次请求，最后一个参数如果不提供，则会把以差不多1秒1个的方式限制每个请求，如果提供，表示是否允许某一刻最多10个。
        /// </summary>
        static Polly.RateLimit.RateLimitPolicy policy = RateLimitPolicy.RateLimit(10, TimeSpan.FromSeconds(10), 10);

        private readonly ILogger<RateLimitController> _logger;

        public RateLimitController(ILogger<RateLimitController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                policy.Execute(() =>
                    {
                        _logger.LogInformation("xxx-" + (i++));
                    });
            }
            catch (Exception ex)
            {
                var t = (ex).GetType();
                throw;
            }

            return Ok();
        }
    }
}
