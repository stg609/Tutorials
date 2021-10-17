using System;
using System.Threading.Tasks;
using MassTransit;
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
                x.AddConsumer<SubmitOrderConsumer>();

                // 指定使用的 Transport 是 RabbitMq
                x.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.Host("localhost", "requestresjponseDemo1");

                    // 一个 ReceiveEndpoint 对应 一个 Channel
                    cfg.ReceiveEndpoint("submit-order-consumer-queue", e =>
                    {
                        // 配置 消费者 （必须先注册)
                        e.ConfigureConsumer<SubmitOrderConsumer>(ctx);
                    });
                });

                x.AddRequestClient<SubmitOrder>();
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

    public interface SubmitOrder
    {
        Guid OrderId { get; }
    }

    public interface SubmitOrderResponse
    {
        public Guid OrderId { get;}
        DateTime Timestamp { get; }
    }



    public class SubmitOrderConsumer :
        IConsumer<SubmitOrder>
    {
        private readonly ILogger<SubmitOrderConsumer> _logger;

        public SubmitOrderConsumer(ILogger<SubmitOrderConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<SubmitOrder> context)
        {
            _logger.LogInformation("Order Submission Received:{OrderId} {CorrelationId}", context.Message.OrderId, context.CorrelationId);

            await context.RespondAsync<SubmitOrderResponse>(new
            {
                OrderId = context.Message.OrderId,
                Timestamp = DateTime.Now
            });
        }

    }
}
