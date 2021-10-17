using System;
using System.Threading.Tasks;
using MassTransit.Courier;
using Microsoft.Extensions.Logging;

namespace AspNetCore
{
    // 烘培 Activity (Activity 就是表示Routing Slip 中的一个执行步骤）
    public class GrillBurgerActivity :
         IActivity<GrillBurgerArguments, GrillBurgerLog>
    {
        private readonly ILogger<GrillBurgerActivity> _logger;

        public GrillBurgerActivity()
        {

        }

        public GrillBurgerActivity(ILogger<GrillBurgerActivity> logger)
        {
            _logger = logger;
        }

        public Task<CompensationResult> Compensate(CompensateContext<GrillBurgerLog> context)
        {
            _logger?.LogInformation(" ===> Grilling Burger Compensate:{OrderId}", context.Log.OrderId);

            return Task.FromResult(context.Compensated());
        }

        public async Task<ExecutionResult> Execute(ExecuteContext<GrillBurgerArguments> context)
        {
            _logger?.LogInformation(" ===> Grilling Burger:{OrderId} {Weight}", context.Arguments.OrderId, context.Arguments.Weight);

            await Task.Delay(5000);

            // 如果要使用 compensate 方式，必须在 startup 注册的时候设置comensate address
            return context.CompletedWithVariables<GrillBurgerLog>(new { OrderId = context.Arguments.OrderId }, new { Ketchup = true });

            // 有些时候我们希望立刻终止 routing slip，不再继续往下执行其他 activity，但是我们又不希望状态为fault，
            // 这个时候可以使用 Terminate, e.g.
            // context.Terminate();

            // 如果 throw 异常或者返回一个 Faulted 结果，那么这个activity 就被认为失败了，
            // routing slip 就会开始执行之前activity 的 compensate 方法（前提，Log 参数必须有值，否则不会触发）

        }
    }

    public interface GrillBurgerArguments
    {
        Guid OrderId { get; }
        decimal Weight { get; }
        bool Cheese { get; }
        decimal Temerature { get; }
    }

    // Log 是用于之后做 补偿 的时候使用
    public interface GrillBurgerLog
    {
        Guid OrderId { get; }
    }

}
