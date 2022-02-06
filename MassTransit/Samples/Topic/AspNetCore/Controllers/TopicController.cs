using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using GreenPipes.Util;
using MassTransit.RabbitMqTransport;

namespace AspNetCore.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TopicController : ControllerBase
    {
        private readonly ILogger<TopicController> _logger;
        private readonly ILoggerFactory _loggerFactory;

        public TopicController(ILogger<TopicController> logger, ILoggerFactory loggerFactory)
        {
            _logger = logger;
            _loggerFactory = loggerFactory;
        }

        //基本思路就是在逻辑编排从数据库读取信息后，根据租户的设置，把这个消息放到topic队列中
        [HttpPost("Subscribe")]
        public async Task<IActionResult> Subscribe([FromServices] IBus bus, [FromBody] QueueInfo msg)
        {
            // 动态添加 receive endpoint (exchange, queue)
            var handle = bus.ConnectReceiveEndpoint("topic-" + msg.Name, x =>
              {
                  x.ConfigureConsumeTopology = false;
                  x.Consumer(typeof(DemoConsumer1), t => new DemoConsumer1(_loggerFactory.CreateLogger<DemoConsumer1>()));

                  // 为了演示多个 subscriber 的 consumer 之间互不影响
                  x.PrefetchCount = 1;

                  var rabbitmqConfigurator = (IRabbitMqReceiveEndpointConfigurator)x;
                  (rabbitmqConfigurator).Bind<IDemo1Msg>(e =>
                  {
                      e.RoutingKey = "topic." + msg.Name;
                      e.ExchangeType = ExchangeType.Topic;
                  });


              });

            await handle.Ready;

            return Ok();
        }

        [HttpPost("publish")]
        public async Task<IActionResult> Publish([FromServices] IBus bus, PublishMsg msg)
        {
            await bus.Publish<IDemo1Msg>(new
            {
                Value = msg.Value
            }, c =>
            {
                c.SetRoutingKey("topic." + msg.TopicName);
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
        public string TopicName { get; set; }
        public string Value { get; set; }
    }
}
