using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MyFrontend.Pages
{
    public class IndexModel : PageModel
    {
        private readonly DaprClient _daprClient;

        public IndexModel(DaprClient daprClient)
        {
            _daprClient = daprClient;
        }

        public async Task OnGet()
        {
            // 这里使用了 service invocation building block
            // 调用 MyBackend 服务的 weatherforecast 方法
            var forecasts = await _daprClient.InvokeMethodAsync<IEnumerable<WeatherForecast>>(
                HttpMethod.Get,
                "MyBackend",
                "weatherforecast");

            ViewData["WeatherForecastData"] = forecasts;
        }
    }
}