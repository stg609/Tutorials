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

            // 配置 Masstransit
            services.AddMassTransit(x =>
            {
                // 指定使用的 Transport 是 RabbitMq
                x.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.Host("localhost", "sagaDemo1");

                    cfg.UseMessageRetry(r => r.Interval(5, 1000));

                    cfg.ReceiveEndpoint("demosaga", e =>
                    {
                        // 配置 saga，使用内存 repository
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
    /// 定义一个 Saga 实例
    /// </summary>
    class DemoSagaInstanceState
        : SagaStateMachineInstance
    {
        /// <summary>
        /// 用于唯一确定一个 Saga 实例
        /// </summary>
        public Guid CorrelationId { get; set; }

        /// <summary>
        /// 用于存储 saga 实例的当前状态
        /// </summary>
        public string TestCurrentState { get; set; }
    }

    /// <summary>
    /// 定义 Event
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
    /// 定义用于 Saga 的状态机
    /// </summary>
    class DemoSagaStateMachine : MassTransitStateMachine<DemoSagaInstanceState>
    {
        // 定义该 saga 除了默认的 Initial, Final 之外的其他状态
        public State Submitted { get; private set; }
        public State Accepted { get; private set; }

        // 定义用于 Saga 状态变化的 事件
        public Event<DemoSagaSubmittedEvent> SubmittedEvent { get; private set; }
        public Event<DemoSagaAcceptedEvent> AcceptedEvent { get; private set; }

        public DemoSagaStateMachine()
        {
            // 指明哪个属性用于作为 CurrentState
            // 如果 CurrentState 类型是 int，则必须完整的列出所有状态，如 InstanceState(x => x.TestState, Submitted, Accepted);
            InstanceState(x => x.TestCurrentState);

            // 配置事件，让事件和 saga 实例关联起来，如下表示 OrderId 作为 CorrelationId
            Event(() => SubmittedEvent, x => x.CorrelateById(ctx => ctx.Message.OrderId));
            Event(() => AcceptedEvent, x =>
            {
                x.CorrelateById(ctx => ctx.Message.OrderId);
            });

            // 初始状态时，当遇到 SubmittedEvent 时，则控制台输出, 并将状态该为 Submitted
            Initially(
                When(SubmittedEvent)
                    .Then(ctx => Console.WriteLine(ctx.Instance.TestCurrentState)) // 输出 Initial
                    .TransitionTo(Submitted)
                    .Then(ctx => Console.WriteLine(ctx.Instance.TestCurrentState))); // 输出 submitted

            // Submitted 状态时，当遇到 AcceptedEvent，则控制台输出, 并将状态该为 Accepted
            During(Submitted,
                When(AcceptedEvent)
                    .Then(ctx => Console.WriteLine(ctx.Instance.TestCurrentState))
                    .Then(ctx => throw new NotImplementedException()) // 输出 Submitted
                    .TransitionTo(Accepted)
                    .Then(ctx => Console.WriteLine(ctx.Instance.TestCurrentState)) // 输出 Accepted
                    .Finalize());

            //// 设置当前状态是 Accepted 时，表示整个 saga 完成，则 saga instance 将被移除
            //SetCompleted(async instance =>
            //{
            //    var currentState = await this.GetState(instance);
            //    return Accepted.Equals(currentState);
            //});
        }
    }
}
