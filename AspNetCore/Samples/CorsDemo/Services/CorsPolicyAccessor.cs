using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace CorsDemo.Services
{
    public interface ICorsPolicyAccessor
    {
        CorsPolicy GetPolicy();
    }


    public class CorsMiddleware
    {
        private readonly CorsOptions _options;
        private readonly RequestDelegate _next;

        public CorsMiddleware(RequestDelegate next, IOptions<CorsOptions> options)
        {
            _options = options.Value;
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            var policy = _options.GetPolicy(_options.DefaultPolicyName);
            policy.Origins.Add("http://localhost:3333");

            await _next(context);
        }
    }

    public static class CorsExtensions
    {
        public static IApplicationBuilder AddCustomCorsPolicy(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CorsMiddleware>()
               .UseCors();
        }
    }
}
