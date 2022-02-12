using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.RabbitMqTransport;
using MassTransit.RabbitMqTransport.Transport;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;

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
                // 注册消费者
                x.AddConsumer<DemoConsumer1>();

                
                // 指定使用的 Transport 是 RabbitMq
                x.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.Host("localhost", "dynamicDirect");

                    // 设置 发送到 'AspNetCore:IDemo1Msg' 的类型参数为 direct
                    cfg.Publish<IDemo1Msg>(x => x.ExchangeType = ExchangeType.Direct);
                });

            });

            services.AddMassTransitHostedService();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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
            _logger.LogInformation("Queue: {Queue}, Value: {Value}", context.DestinationAddress.AbsoluteUri, context.Message.Value);
        }
    }

    public interface IDemo1Msg
    {
        string Value { get; }
    }
}
