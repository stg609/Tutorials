using Demo.Protos.v1;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace GrpcServerDemo.Services
{
    public class EvaluatorService : Evaluator.EvaluatorBase
    {
        private readonly ILogger<EvaluatorService> _logger;

        public EvaluatorService(ILogger<EvaluatorService> logger)
        {
            _logger = logger;
        }
        public override Task<EvaluateResponse> Evaluate(EvaluateRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Saying hello to {Name}", request.CodeBlock);

            var rsp = new EvaluateResponse
            {
                VariablesJson = "{}",
            };

            rsp.Dic.Add("abc", "");


            return Task.FromResult(rsp);
        }
    }
}
