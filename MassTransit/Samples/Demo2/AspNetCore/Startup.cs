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

            // ���� Masstransit
            services.AddMassTransit(x =>
            {
                // ע��������
                x.AddConsumer<DemoConsumer4>();
                x.AddConsumer<DemoConsumer5>();
                                

                // ָ��ʹ�õ� Transport �� RabbitMq
                x.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.Host("localhost", "demo2");

                    // һ�� ReceiveEndpoint ��Ӧ һ�� Channel
                    // ���һ��½�һ�� ��Ϊ demo-recieve-queue �� Exchange �� Queue,
                    // ͬʱ���� Exchange �� Queue �İ󶨹�ϵ
                    cfg.ReceiveEndpoint("demo-receive-queue", e =>
                     {
                         // ���� ������ ��������ע��)
                         // ���»Ὠ�� Consumer �� MessageType ��Ӧ�� Exchange (e.g. EventContracts:ValueEntered)
                         // ������ EventContracts:ValueEntered (exchange) -> demo-receive-queue (exchange) �İ󶨹�ϵ
                         e.ConfigureConsumer<DemoConsumer4>(ctx);
                     });

                    // �������� demo-receive-queue2 �ĳ� demo-receive-queue ��Ҳ���Ǻ�ǰһ�� RE ������һ����
                    // ��ô����ʱ��ͻᱨ����Ϊ2�� queue ����һ��
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

            // ��һ�� ConsumerA �н��� Publish, ������ ConversationId ����ȥ, 
            // ��� correaltionId ��ֵ�������� outgoing message �� initiatorid �ͻᱻ��Ϊ correlationId
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
