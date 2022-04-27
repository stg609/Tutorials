using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OcelotDemo.Handlers
{
public class DemoHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // 可以根据需要重新构造 HttpRequestMessage, 比如使用 其它的 http method
        // HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, request.RequestUri);
        // req.Content = new StringContent("{\"name\":\"John Doe\",\"age\":33}", Encoding.UTF8, "application/json");
            
        // 继续调用 ocelot
        var rsp = await base.SendAsync(request, cancellationToken);


        // do something after request (.e.g change response)

        return rsp;
    }
}
}
