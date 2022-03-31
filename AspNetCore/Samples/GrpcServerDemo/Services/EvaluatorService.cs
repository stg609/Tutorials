﻿using Demo.Protos.v1;
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

        public override Task<EvaluateResponse> EvaluateAsync(EvaluateRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Saying hello to {Name}", request.CodeBlock);
            return Task.FromResult(new EvaluateResponse
            {
                VariablesJson = "{}"
            });
        }
    }
}
