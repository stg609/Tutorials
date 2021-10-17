using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Automatonymous;
using GreenPipes;
using MassTransit;
using MassTransit.Saga;
using MessageContracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace AspNetCore_Consumer
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
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "AspNetCore_Consumer", Version = "v1" });
            });


            // 配置 Masstransit
            services.AddMassTransit(x =>
            {
                // 注册消费者
                //x.AddConsumer<EventConsumer>();
                x.AddConsumer<EventConsumer3>();

                x.SetKebabCaseEndpointNameFormatter();



                // 指定使用的 Transport 是 RabbitMq
                x.UsingRabbitMq((ctx, cfg) =>
                {
                    //// configureEndpoints 会自动根据注册的 Consumer 来配置 ReceiveEndpoint, Echange, Queue
                    //cfg.ConfigureEndpoints(ctx);

                    // 一个 ReceiveEndpoint 对应 一个 Channel
                    // 并且会新建一个 名为 example-recieve-queue 的 Exchange 和 Queue,
                    // 同时建立 Exchange 到 Queue 的绑定关系


                    //cfg.ReceiveEndpoint("example-receive-queue3", e =>
                    //{
                    //    e.PrefetchCount = 5;

                    //    // 表示这个 consumer 内部同时多少个线程处理
                    //    e.ConcurrentMessageLimit = 2;

                    //    // 配置 消费者 （必须先注册)
                    //    // 建立 Consumer 的 MessageType 对应的 Exchange (e.g. EventContracts:ValueEntered)
                    //    // 并建立 EventContracts:ValueEntered -> example-receive-queue 的绑定关系
                    //    e.ConfigureConsumer<EventConsumer3>(ctx);
                    //});

                   
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
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "AspNetCore_Consumer v1"));
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


    class EventConsumer3 :
        IConsumer<Demo1Msg>
    {
        ILogger<EventConsumer3> _logger;

        public EventConsumer3(ILogger<EventConsumer3> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<Demo1Msg> context)
        {
            _logger.LogInformation($"Thread:{Thread.CurrentThread.ManagedThreadId} started at {DateTime.Now}");
            Thread.Sleep(10000);
            _logger.LogInformation($"Thread:{Thread.CurrentThread.ManagedThreadId} finished at {DateTime.Now}");
        }
    }

    //class EventConsumer5 :
    //IConsumer<DemoSagaMsg1Submitted>
    //{
    //    ILogger<EventConsumer5> _logger;

    //    public EventConsumer5(ILogger<EventConsumer5> logger)
    //    {
    //        _logger = logger;
    //    }

    //    public async Task Consume(ConsumeContext<DemoSagaMsg1Submitted> context)
    //    {
    //        _logger.LogInformation($"DemoSagaMsg1Submitted {context.Message.OrderId}");
    //    }
    //}

}
