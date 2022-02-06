using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace AspNetCore
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "AspNetCore", Version = "v1" });
            });

            // 配置 Masstransit
            services.AddMassTransit(x =>
            {
                x.AddConsumer<DemoConsumer1>();

                // 指定 Masstransit 使用的 Transport 是 RabbitMq
                x.UsingRabbitMq((ctx, cfg) =>
                {
                    // 配置连接的 Rabbitmq 地址，demo1 为 vhost 地址（需要事先在 rabbitmq 中添加该 vhost)
                    cfg.Host("localhost");

                    // 一个 ReceiveEndpoint 对应 一个 Channel
                    // 并且会新建一个 名为 demo-recieve-queue 的 Exchange 和 Queue,
                    // 同时建立 Exchange 到 Queue 的绑定关系
                    cfg.ReceiveEndpoint("demo-static-vhost", e =>
                    {
                        // 配置 消费者 （必须先注册)
                        // 如下会建立 Consumer 的 MessageType 对应的 Exchange (e.g. EventContracts:ValueEntered)
                        // 并建立 EventContracts:ValueEntered (exchange) -> demo-receive-queue (exchange) 的绑定关系
                        e.ConfigureConsumer<DemoConsumer1>(ctx);

                        // 设置为1个用于演示：当一个 consumer 队列很满的时候，另一个 consumer 互不影响
                        e.PrefetchCount = 1;
                    });
                });
            });

            // 启动 MassTransit
            services.AddMassTransitHostedService();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "AspNetCore v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }

    class DemoConsumer1 :
       IConsumer<IDemo1Msg>
    {
        ILogger<DemoConsumer1> _logger;

        public DemoConsumer1(ILogger<DemoConsumer1> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<IDemo1Msg> context)
        {
            await Task.Delay(3000);
            _logger.LogInformation("VHost: {VHost}, Value: {Value}", context.DestinationAddress.AbsoluteUri, context.Message.Value);
        }
    }

    public interface IDemo1Msg
    {
        string Value { get; }
    }
}
