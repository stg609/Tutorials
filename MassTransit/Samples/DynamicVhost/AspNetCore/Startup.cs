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

            // ���� Masstransit
            services.AddMassTransit(x =>
            {
                x.AddConsumer<DemoConsumer1>();

                // ָ�� Masstransit ʹ�õ� Transport �� RabbitMq
                x.UsingRabbitMq((ctx, cfg) =>
                {
                    // �������ӵ� Rabbitmq ��ַ��demo1 Ϊ vhost ��ַ����Ҫ������ rabbitmq ����Ӹ� vhost)
                    cfg.Host("localhost");

                    // һ�� ReceiveEndpoint ��Ӧ һ�� Channel
                    // ���һ��½�һ�� ��Ϊ demo-recieve-queue �� Exchange �� Queue,
                    // ͬʱ���� Exchange �� Queue �İ󶨹�ϵ
                    cfg.ReceiveEndpoint("demo-static-vhost", e =>
                    {
                        // ���� ������ ��������ע��)
                        // ���»Ὠ�� Consumer �� MessageType ��Ӧ�� Exchange (e.g. EventContracts:ValueEntered)
                        // ������ EventContracts:ValueEntered (exchange) -> demo-receive-queue (exchange) �İ󶨹�ϵ
                        e.ConfigureConsumer<DemoConsumer1>(ctx);

                        // ����Ϊ1��������ʾ����һ�� consumer ���к�����ʱ����һ�� consumer ����Ӱ��
                        e.PrefetchCount = 1;
                    });
                });
            });

            // ���� MassTransit
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
