using Dapr.AppCallback.Autogen.Grpc.v1;
using Dapr.Client.Autogen.Grpc.v1;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcGreeter;

namespace Server.Services
{
    public class HelloService : AppCallback.AppCallbackBase
    {
        public override async Task<InvokeResponse> OnInvoke(InvokeRequest request, ServerCallContext context)
        {
            var response = new InvokeResponse();
            switch (request.Method)
            {
                case "sayhi":
                    var input = request.Data.Unpack<HelloRequest>();
                    response.Data = Any.Pack(new HelloReply { Message = "ok" });
                    break;
            }
            return response;
        }
    }
}
