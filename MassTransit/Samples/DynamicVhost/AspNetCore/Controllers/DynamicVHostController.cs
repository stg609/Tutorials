using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AspNetCore.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DynamicVHostController : ControllerBase
    {
        private readonly ILogger<DynamicVHostController> _logger;
        private readonly ILoggerFactory _loggerFactory;

        public DynamicVHostController(ILogger<DynamicVHostController> logger, ILoggerFactory loggerFactory)
        {
            _logger = logger;
            _loggerFactory = loggerFactory;
        }

        /*
         * Demo:
         * 1. 用 Post 新建一个 VHost
         * 2. 使用 publish 往 Vhost 中加入 N 条消息
         * 3. 使用 publihs 往 / 中加入 N 条消息
         * 4. 观察这两个 consumer 互相互不影响
         * */

        [HttpPost]
        public async Task<IActionResult> Post(VHostModel host)
        {
            // 通过 rabbitmq 提供的 rest api 来创建一个新的 VHost
            var credentials = new NetworkCredential() { UserName = "guest", Password = "guest" };
            using (var handler = new HttpClientHandler { Credentials = credentials })
            using (var client = new HttpClient(handler))
            {
                var url = $"http://localhost:15672/api/vhosts/{host.Name}";

                var content = new StringContent("", Encoding.UTF8, "application/json");
                var result = client.PutAsync(url, content).Result;

                if ((int)result.StatusCode >= 300)
                    throw new Exception(result.ToString());
            }

            // 启动这个 bus 绑定消费者到这个 vhost，用于接收消息
            var busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.Host("localhost", host.Name);
                cfg.ReceiveEndpoint("dynamic-receive-ep", x =>
                {
                    x.Consumer(typeof(DemoConsumer1), t => new DemoConsumer1(_loggerFactory.CreateLogger<DemoConsumer1>()));

                    // 设置为1个用于演示：当一个 consumer 队列很满的时候，另一个 consumer 互不影响
                    x.PrefetchCount = 1;
                });
            });

            await busControl.StartAsync();
            return Ok();
        }

        /// <summary>
        /// 往某个 VHost 的队列发送消息
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        [HttpPost("publish")]
        public async Task<IActionResult> Publish(PublishMsg msg)
        {
            var busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.Host("localhost", string.IsNullOrWhiteSpace(msg.VHost) ? "/" : msg.VHost.Trim());
            });
            busControl.Start();

            await busControl.Publish<IDemo1Msg>(new
            {
                Value = msg.Value
            });
            return Ok();
        }
    }

    public class PublishMsg
    {
        /// <summary>
        /// 如果不填，则是默认的vhost
        /// </summary>
        public string VHost { get; set; }
        public string Value { get; set; }
    }
}
