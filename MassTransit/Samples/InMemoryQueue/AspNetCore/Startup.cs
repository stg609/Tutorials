using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Testing;
using MassTransit.Testing.Indicators;
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

            services.AddMassTransit(x =>
            {
                x.AddConsumer<DemoConsumer1>();

                x.UsingInMemory((context, cfg) =>
                {
                    cfg.TransportConcurrencyLimit = 100;

                    cfg.ConcurrentMessageLimit = 1;

                    cfg.ReceiveEndpoint("demo-receive-queue", e =>
                    {
                        // 配置 消费者 （必须先注册)
                        // 如下会建立 Consumer 的 MessageType 对应的 Exchange (e.g. EventContracts:ValueEntered)
                        // 并建立 EventContracts:ValueEntered (exchange) -> demo-receive-queue (exchange) 的绑定关系
                        e.ConfigureConsumer<DemoConsumer1>(context);
                    });
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


            var bus = app.ApplicationServices.GetService<IBusControl>();

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
