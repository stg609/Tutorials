using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace OcelotDemo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SomeController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public SomeController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// 演示 ocelot 调用自己
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task Get()
        {
            HttpRequestMessage req = new HttpRequestMessage
            {
                Method = new HttpMethod(Request.Method),
                RequestUri = new Uri(Request.Scheme + "://localhost:" + Request.Host.Port + "/api/weatherforecast"),
            };
            foreach (var header in Request.Headers)
            {
                if(header.Key.StartsWith(":"))
                {
                    continue;
                }
                if (header.Value.Count == 1)
                {

                    req.Headers.Add(header.Key, header.Value.First());
                }
                else
                {
                    req.Headers.Add(header.Key, header.Value.ToList<string>());

                }
            }
            await _httpClient.SendAsync(req);
        }
    }
}
