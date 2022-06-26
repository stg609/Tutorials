using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;

namespace GrpcConsoleClient
{
    public class ClientLoggingInterceptor : Interceptor
    {
        private readonly ILogger _logger;

        public ClientLoggingInterceptor(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ClientLoggingInterceptor>();
        }

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
            TRequest request,
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            _logger.LogInformation($"Starting call. Type: {context.Method.Type}. " +
                $"Method: {context.Method.Name}.{request}");
            var call = continuation(request, context);

            return new AsyncUnaryCall<TResponse>(
               HandleResponse(_logger, call.ResponseAsync),
               call.ResponseHeadersAsync,
               call.GetStatus,
               call.GetTrailers,
               call.Dispose);
        }

        private async Task<TResponse> HandleResponse<TResponse>(ILogger logger, Task<TResponse> inner)
        {
            try
            {
                var rsp = await inner;
                logger.LogInformation("Response:" + rsp);
                return rsp;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Custom error", ex);
            }
        }
    }
}
