using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CorsDemo.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CorsDemo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [EnableCors("custom")]
    public class EnabledController : ControllerBase
    {
        private readonly ILogger<EnabledController> _logger;

        public EnabledController(ILogger<EnabledController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Content("Ok");
        }

        /// <summary>
        /// 演示 DisableCors 的场景
        /// </summary>
        /// <returns></returns>
        [DisableCors]
        [HttpGet("Disabled")]
        public IActionResult GetDisabled()
        {
            return Content("Ok");
        }
    }
}
