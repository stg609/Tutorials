using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AspNetCore.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DynamicQueueController : ControllerBase
    {
        private readonly ILogger<DynamicQueueController> _logger;
        private readonly ILoggerFactory _loggerFactory;

        public DynamicQueueController(ILogger<DynamicQueueController> logger,ILoggerFactory loggerFactory)
        {
            _logger = logger;
            _loggerFactory = loggerFactory;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromServices]IBus bus,[FromBody]QueueInfo msg)
        {
            // 动态添加 receive endpoint (exchange, queue), 如果 queue 名字相同 就认为是同一个 queue，如果添加的 consumer 不同，则会有多个consumer
            var handle = bus.ConnectReceiveEndpoint(msg.Name, x =>
            {
                x.Consumer(typeof(DemoConsumer1), t => new DemoConsumer1(_loggerFactory.CreateLogger<DemoConsumer1>()));
                x.PrefetchCount = 1;
            });

            await handle.Ready;

            return Ok();
        }

        [HttpPost("send")]
        public async Task<IActionResult> Send([FromServices] ISendEndpointProvider sendEndpointProvider, PublishMsg msg)
        {
            // send 到某个特定的队列
            var ep = await sendEndpointProvider.GetSendEndpoint(new Uri($"rabbitmq://localhost/{msg.QueueName}"));
            await ep.Send<IDemo1Msg>(new
            {
                Value = msg.Value
            });

            return Ok();
        }
    }

    public class QueueInfo
    {
        public string Name { get; set; }
    }

    public class PublishMsg
    {
        public string QueueName { get; set; }
        public string Value { get; set; }
    }
}
