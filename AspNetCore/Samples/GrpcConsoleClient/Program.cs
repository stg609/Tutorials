using Demo.Protos.v1;
using Grpc.Net.Client;
using System;
using System.Threading.Tasks;

namespace GrpcConsoleClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new Evaluator.EvaluatorClient(channel);

            var reply = await client.EvaluateAsyncAsync(new EvaluateRequest
            {
                CodeBlock = "test",

                // https://github.com/grpc/grpc-dotnet/issues/917#issuecomment-631765081
                // 这里 ContextJson 最理想的是object类型，于是想到是否可以用 protocol 中的 Any
                // 不能使用 Any 类型，因为 Any 其实需要 (Un)pack 一个消息，而这个消息任然需要定义的某个消息类型（因为需要实现 IMessage )
                ContextJson = "xxx"
            });

            Console.WriteLine("Greeter 服务返回数据: " + reply.VariablesJson);
            Console.ReadKey();

        }
    }
}
