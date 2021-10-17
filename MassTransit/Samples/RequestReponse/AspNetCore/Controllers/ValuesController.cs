using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly IRequestClient<SubmitOrder> _requestClient;

        public ValuesController(IRequestClient<SubmitOrder> requestClient)
        {
            _requestClient = requestClient;
        }


        [HttpPost]
        public async Task<SubmitOrderResponse> Post3(Guid? value)
        {
            var resp = await _requestClient.GetResponse<SubmitOrderResponse>(new
            {
                OrderId = value ?? Guid.NewGuid()
            });

            return resp.Message;
        }
    }
}
