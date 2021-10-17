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
        private readonly IPublishEndpoint _publishEndpoint;

        public ValuesController(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }


        [HttpPost]
        public async Task<ActionResult> Post(Guid? value)
        {
            await _publishEndpoint.Publish<SubmitOrder>(new
            {
                OrderId = value ?? Guid.NewGuid()
            });

            return Ok();
        }
    }
}
