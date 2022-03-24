using Dapr.Client;
using GrpcGreeter;
using Microsoft.AspNetCore.Mvc;

namespace Client.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DaprController : ControllerBase
    {
        [HttpGet("grpc")]
        public async Task<ActionResult> GrpcAsync()
        {
            using var daprClient = new DaprClientBuilder().Build();
            var result = await daprClient.InvokeMethodGrpcAsync<HelloRequest, HelloReply>("backend", "sayhi", new HelloRequest { Name = "aaa" });
            return Ok(result);
        }
    }
}