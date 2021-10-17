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
                x.AddConsumer<DemoConsumer1>();

                x.AddConsumer<DemoConsumer2>();

                // SetKebabCaseEndpointNameFormatter sets the kebab-case endpoint name formatter, 
                // which will create a receive endpoint named value-entered-event for the ValueEnteredEventConsumer. 
                // The Consumer suffix is removed by default.
                x.SetKebabCaseEndpointNameFormatter();

                // 指定使用的 Transport 是 RabbitMq
                x.UsingRabbitMq((ctx, cfg) =>
                {
                    // 配置连接的 Rabbitmq 地址，demo1 为 vhost 地址（需要事先在 rabbitmq 中添加该 vhost)
                    cfg.Host("localhost", "demo1");

                    // 使用ConfigureEndpoints 自动根据注册的 Consumer 来配置 ReceiveEndpoint, Echange, Queue
                    cfg.ConfigureEndpoints(ctx);
                });
            });

            // 启动 MassTransit
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
        IConsumer<Demo1Msg>
    {
        ILogger<DemoConsumer1> _logger;

        public DemoConsumer1(ILogger<DemoConsumer1> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<Demo1Msg> context)
        {
            _logger.LogInformation("Value: {Value}", context.Message.Value);
        }
    }

    class DemoConsumer2 :
        IConsumer<Demo1Msg>
    {
        ILogger<DemoConsumer2> _logger;

        public DemoConsumer2(ILogger<DemoConsumer2> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<Demo1Msg> context)
        {
            _logger.LogInformation("Value2: {Value}", context.Message.Value);
        }
    }

}
