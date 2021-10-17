using System;
using System.Threading.Tasks;
using AspNetCore.Activities;
using MassTransit;
using MassTransit.Courier;
using MassTransit.Courier.Contracts;
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
                x.AddConsumer<SubmitOrderConsumer>();
                x.AddActivitiesFromNamespaceContaining<CourierActivities>();

                // ָ��ʹ�õ� Transport �� RabbitMq
                x.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.Host("localhost", "courierDemo1");

                    Uri compensateAddress = default;
                    cfg.ReceiveEndpoint("grill-compensate-queue", e =>
                    {
                        // ���� comensate host
                        e.CompensateActivityHost<GrillBurgerActivity, GrillBurgerLog>();
                        compensateAddress = e.InputAddress;
                    });

                    cfg.ReceiveEndpoint("grill-executing-queue", e =>
                    {
                        // ���� ִ�� host 
                        // ע�� 1. executing host �� compensate host ���������������� host��������ִ�е�ʱ�� comensate Ҳ�����У��ͻ����
                        // ע�� 2. ���Ҫʹ�� compensate ��ʽ�������Ҫ�ڴ��� comensateAddress
                        e.ExecuteActivityHost<GrillBurgerActivity, GrillBurgerArguments>(compensateAddress);
                    });

                    cfg.ReceiveEndpoint("dress-executing-queue", e =>
                    {
                        // ���� dress host
                        e.ExecuteActivityHost<DressBurgerActivity, DressBurgerArguments>();
                    });

                    cfg.ReceiveEndpoint("submit-order-queue", e =>
                    {
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

    public class OrderSubmissionAccepted
    {
        public Guid OrderId { get; set; }
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

            var routingSlip = CreateRoutingSlip(context.Message);

            // ִ�� routing  Slip
            await context.Execute(routingSlip);

            if (context.ResponseAddress != null)
            {
                await context.RespondAsync<OrderSubmissionAccepted>(new { context.Message.OrderId });
            }
        }

        RoutingSlip CreateRoutingSlip(SubmitOrder submitOrder)
        {
            // ÿ�� routing slip ����һ��Ψһ id
            var builder = new RoutingSlipBuilder(NewId.NextGuid());
            builder.AddActivity("grill-burger", new Uri("queue:grill-executing-queue"), new
            {
                Weight = 0.5m,
                Temperature = 165.0m
            });

            builder.AddActivity("dress-burger", new Uri("queue:dress-executing-queue"), new
            {
                Lettuce = true
            });

            // ������ Activity ��arguments �л�ȡ
            builder.AddVariable(nameof(submitOrder.OrderId), submitOrder.OrderId);

            var routingSlip = builder.Build();
            return routingSlip;
        }


    }
}
