using System.Threading.Tasks;
using MassTransit;
using MessageContracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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
                // 注册消费者
                x.AddConsumer<DemoConsumer4>();
                x.AddConsumer<DemoConsumer5>();
                                

                // 指定使用的 Transport 是 RabbitMq
                x.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.Host("localhost", "demo2");

                    // 一个 ReceiveEndpoint 对应 一个 Channel
                    // 并且会新建一个 名为 demo-recieve-queue 的 Exchange 和 Queue,
                    // 同时建立 Exchange 到 Queue 的绑定关系
                    cfg.ReceiveEndpoint("demo-receive-queue", e =>
                     {
                         // 配置 消费者 （必须先注册)
                         // 如下会建立 Consumer 的 MessageType 对应的 Exchange (e.g. EventContracts:ValueEntered)
                         // 并建立 EventContracts:ValueEntered (exchange) -> demo-receive-queue (exchange) 的绑定关系
                         e.ConfigureConsumer<DemoConsumer4>(ctx);
                     });

                    // 如果这里把 demo-receive-queue2 改成 demo-receive-queue （也就是和前一个 RE 的名字一样）
                    // 那么启动时候就会报错，因为2个 queue 名字一样
                    cfg.ReceiveEndpoint("demo-receive-queue2", e =>
                    {
                        e.ConfigureConsumer<DemoConsumer5>(ctx);
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
        }
    }

    class DemoConsumer4 :
        IConsumer<Demo2MsgA>
    {
        ILogger<DemoConsumer4> _logger;

        public DemoConsumer4(ILogger<DemoConsumer4> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<Demo2MsgA> context)
        {
            _logger.LogInformation("MsgA: {Value}, CorrelationId: {CorrelationId}, ConversationId:{ConversationId}, MessageId: {MessageId}, InitiatorId: {InitiatorId}", context.Message.Value, context.CorrelationId, context.ConversationId, context.MessageId, context.InitiatorId);

            // 在一个 ConsumerA 中接着 Publish, 这个会把 ConversationId 传下去, 
            // 如果 correaltionId 有值，则所有 outgoing message 的 initiatorid 就会被设为 correlationId
            await context.Publish<Demo2MsgB>(new
            {
                Value = context.Message.Value + " sent by DemoConsumer4"
            });
        }
    }

    class DemoConsumer5 :
       IConsumer<Demo2MsgB>
    {
        ILogger<DemoConsumer5> _logger;

        public DemoConsumer5(ILogger<DemoConsumer5> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<Demo2MsgB> context)
        {
            _logger.LogInformation("MsgB: {Value}, CorrelationId: {CorrelationId}, ConversationId:{ConversationId}, MessageId: {MessageId}, InitiatorId: {InitiatorId}", context.Message.Value, context.CorrelationId, context.ConversationId, context.MessageId, context.InitiatorId);
        }
    }
}
