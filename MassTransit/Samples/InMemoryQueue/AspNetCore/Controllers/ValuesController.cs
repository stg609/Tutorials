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

        [HttpPost]
        public async Task<ActionResult> Post(string value)
        {
            // 如果没有 Exchange，Publish 会自动创建一个 message type (EventContracts:ValueEntered) 同名的 Exchange (fanout)
            // 但是没有 binding 到其他Exchagne 或 queue
            await _publishEndpoint.Publish<IDemo1Msg>(new
            {
                Value = value
            });

            return Ok();
        }
    }
}
