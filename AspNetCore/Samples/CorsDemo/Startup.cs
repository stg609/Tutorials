using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CorsDemo.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace CorsDemo
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "CorsDemo", Version = "v1" });
            });

            // 添加一个空的非默认的 Cors Policy, 用于后续 AddCustomCorsPolicy 时候添加规则
            // 如果是默认的Policy，那 middleware 中添加 useCors 后，没有使用 EnableCorsAttribute 也会有效果，
            // 所以为了避免默认的行为，这里采用命名的 Policy
            services.AddCors(x=>x.AddPolicy("custom", builder=>
            {
            }));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CorsDemo v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.AddCustomCorsPolicy();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
