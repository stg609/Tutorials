using System;
using System.Threading.Tasks;
using MassTransit.Courier;
using Microsoft.Extensions.Logging;

namespace AspNetCore
{
    // 给汉堡包加料
    public class DressBurgerActivity :
        IActivity<DressBurgerArguments, DressBurgerLog>
    {
        private readonly ILogger<DressBurgerActivity> _logger;

        public DressBurgerActivity()
        {

        }

        public DressBurgerActivity(ILogger<DressBurgerActivity> logger)
        {
            _logger = logger;
        }

        public Task<CompensationResult> Compensate(CompensateContext<DressBurgerLog> context)
        {
            throw new NotImplementedException();
        }

        public async Task<ExecutionResult> Execute(ExecuteContext<DressBurgerArguments> context)
        {
            _logger?.LogInformation(" ===> Dressing Burger:{OrderId} {Ketchup}",context.Arguments.OrderId, context.Arguments.Ketchup);

            await Task.Delay(1000);

            return context.Completed();

            // uncomment if wanna demo compensate
            return context.Faulted();
        }
    }

    public interface DressBurgerArguments
    {
        Guid OrderId { get; }

        /// <summary>
        /// 生菜
        /// </summary>
        bool Lettuce { get; }

        /// <summary>
        /// 咸菜
        /// </summary>
        bool Pickles { get; }

        /// <summary>
        /// 番茄酱
        /// </summary>
        bool Ketchup { get; }
    }

    // Log 是用于之后做 补偿 的时候使用
    public interface DressBurgerLog
    {

    }
}
