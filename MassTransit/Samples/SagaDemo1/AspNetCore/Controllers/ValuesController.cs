using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        readonly IPublishEndpoint _publishEndpoint;

        public ValuesController(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        [HttpPost("saga-submit")]
        public async Task<ActionResult> Post3(Guid? value)
        {
            await _publishEndpoint.Publish<DemoSagaSubmittedEvent>(new
            {
                OrderId = value ?? Guid.NewGuid()
            });

            return Ok();
        }

        [HttpPost("saga-accept")]
        public async Task<ActionResult> Post4(Guid? value)
        {
            await _publishEndpoint.Publish<DemoSagaAcceptedEvent>(new
            {
                OrderId = value ?? Guid.NewGuid()
            });

            return Ok();
        }
    }
}
