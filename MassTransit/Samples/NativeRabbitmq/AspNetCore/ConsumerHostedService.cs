using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AspNetCore
{
    public class ConsumerHostedService : BackgroundService
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public ConsumerHostedService()
        {
            var factory = new ConnectionFactory() { HostName = "localhost", UserName = "guest", Password = "guest", VirtualHost = "nativeRabbitmq" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(queue: "hello",
                                    durable: true,
                                    exclusive: false,
                                    autoDelete: false,
                                    arguments: null);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                // 第二个参数：true 批量 ack，也就是把 <= DeliveryTag 的都 ack 了。
                _channel.BasicAck(ea.DeliveryTag, false);

                Console.WriteLine(" [x] Received {0}", message);
            };

            string tag = _channel.BasicConsume(queue: "hello",
                                autoAck: false, // 需要手动 ack，不 ack 则会一直为 nack 的状态，其他 cosnumer 无法消费
                                consumer: consumer);

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }
}
