using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace TrivialDemo.Controllers
{
    /// <summary>
    /// 演示如何调试 nuget pkg
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class DebugNugetPkgController : ControllerBase
    {
        private readonly ILogger<DebugNugetPkgController> _logger;

        public DebugNugetPkgController(ILogger<DebugNugetPkgController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get(CancellationToken cancellationToken)
        {
            // F11，可以调试进去
            
            string str = JsonConvert.SerializeObject(DateTime.Now);


            var sc = new DebugNugetPkgDemoPkgForTrivialDemo.SomeClass();
            str = sc.SomeMethod();
            return Content("Ok");
        }
    }
}
