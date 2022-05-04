using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace TrivialDemo.Controllers
{
    /*
     * 相比 DataAnnoation 的优点：
     * 1. 把 validation 的逻辑 和 model 分离开，更符合 clean arch。
     * 2. 更利于 model 的重用，比如有些逻辑中需要对 model 的不同字段进行不同的约束，因为 validator 是分离的所以不会影响重用。
     *  
     */

    [ApiController]
    [Route("[controller]")]
    public class FluentValidationController : ControllerBase
    {
        private readonly ILogger<FluentValidationController> _logger;

        public FluentValidationController(ILogger<FluentValidationController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public IActionResult POST(SomeModel model)
        {
            return Ok();
        }
    }

    public class SomeModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public decimal Experience { get; set; }
    }
}
