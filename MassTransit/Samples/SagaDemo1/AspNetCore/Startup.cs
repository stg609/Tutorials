using System;
using Automatonymous;
using GreenPipes;
using MassTransit;
using MassTransit.Saga;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
                // ָ��ʹ�õ� Transport �� RabbitMq
                x.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.Host("localhost", "sagaDemo1");

                    cfg.UseMessageRetry(r => r.Interval(5, 1000));

                    cfg.ReceiveEndpoint("demosaga", e =>
                    {
                        // ���� saga��ʹ���ڴ� repository
                        e.StateMachineSaga(new DemoSagaStateMachine(), new InMemorySagaRepository<DemoSagaInstanceState>());
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

    /// <summary>
    /// ����һ�� Saga ʵ��
    /// </summary>
    class DemoSagaInstanceState
        : SagaStateMachineInstance
    {
        /// <summary>
        /// ����Ψһȷ��һ�� Saga ʵ��
        /// </summary>
        public Guid CorrelationId { get; set; }

        /// <summary>
        /// ���ڴ洢 saga ʵ���ĵ�ǰ״̬
        /// </summary>
        public string TestCurrentState { get; set; }
    }

    /// <summary>
    /// ���� Event
    /// </summary>
    public interface DemoSagaSubmittedEvent
    {
        Guid OrderId { get; }
    }

    public interface DemoSagaAcceptedEvent
    {
        Guid OrderId { get; }
    }

    /// <summary>
    /// �������� Saga ��״̬��
    /// </summary>
    class DemoSagaStateMachine : MassTransitStateMachine<DemoSagaInstanceState>
    {
        // ����� saga ����Ĭ�ϵ� Initial, Final ֮�������״̬
        public State Submitted { get; private set; }
        public State Accepted { get; private set; }

        // �������� Saga ״̬�仯�� �¼�
        public Event<DemoSagaSubmittedEvent> SubmittedEvent { get; private set; }
        public Event<DemoSagaAcceptedEvent> AcceptedEvent { get; private set; }

        public DemoSagaStateMachine()
        {
            // ָ���ĸ�����������Ϊ CurrentState
            // ��� CurrentState ������ int��������������г�����״̬���� InstanceState(x => x.TestState, Submitted, Accepted);
            InstanceState(x => x.TestCurrentState);

            // �����¼������¼��� saga ʵ���������������±�ʾ OrderId ��Ϊ CorrelationId
            Event(() => SubmittedEvent, x => x.CorrelateById(ctx => ctx.Message.OrderId));
            Event(() => AcceptedEvent, x =>
            {
                x.CorrelateById(ctx => ctx.Message.OrderId);
            });

            // ��ʼ״̬ʱ�������� SubmittedEvent ʱ�������̨���, ����״̬��Ϊ Submitted
            Initially(
                When(SubmittedEvent)
                    .Then(ctx => Console.WriteLine(ctx.Instance.TestCurrentState)) // ��� Initial
                    .TransitionTo(Submitted)
                    .Then(ctx => Console.WriteLine(ctx.Instance.TestCurrentState))); // ��� submitted

            // Submitted ״̬ʱ�������� AcceptedEvent�������̨���, ����״̬��Ϊ Accepted
            During(Submitted,
                When(AcceptedEvent)
                    .Then(ctx => Console.WriteLine(ctx.Instance.TestCurrentState))
                    .Then(ctx => throw new NotImplementedException()) // ��� Submitted
                    .TransitionTo(Accepted)
                    .Then(ctx => Console.WriteLine(ctx.Instance.TestCurrentState)) // ��� Accepted
                    .Finalize());

            //// ���õ�ǰ״̬�� Accepted ʱ����ʾ���� saga ��ɣ��� saga instance �����Ƴ�
            //SetCompleted(async instance =>
            //{
            //    var currentState = await this.GetState(instance);
            //    return Accepted.Equals(currentState);
            //});
        }
    }
}
