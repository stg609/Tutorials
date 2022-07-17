using CrashDemo.Controllers;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using IApplicationLifetime = Microsoft.Extensions.Hosting.IApplicationLifetime;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks()
    .AddCheck("liveness", () =>
     {
         if (CrashController._crashed)
         {
             return HealthCheckResult.Unhealthy();
         }
         else
         {
             return HealthCheckResult.Healthy();
         }
     });

var app = builder.Build();

string? serviceName = typeof(Program).Assembly.GetName().Name;
var logger = LoggerFactory.Create(builder =>
    builder
    .AddConsole()
    .AddDebug()
#if RELEASE
    .AddJsonConsole()
#endif
    ).CreateLogger<Program>();

AppDomain currentDomain = AppDomain.CurrentDomain;
currentDomain.UnhandledException += new UnhandledExceptionEventHandler((object sender, UnhandledExceptionEventArgs args) =>
   {
       Exception e = (Exception)args.ExceptionObject;
       Console.WriteLine("MyHandler caught : " + e.Message);
       Console.WriteLine("Runtime terminating: {0}", args.IsTerminating);

       logger.LogCritical("This is unhandled ex by unhandledexception handler:" + e.Message);
   });

try
{
    var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
    lifetime.ApplicationStopping.Register(() =>
    {
        logger.LogCritical("This is unhandled ex by IHostApplicationLifetime");

    });

    // Configure the HTTP request pipeline.

    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseAuthorization();
    app.UseRouting();
    app.UseEndpoints(eps =>
    {
        eps.MapControllers();
        eps.MapHealthChecks("/liveness");
    });

    Console.WriteLine("--- IsServerGC: " + System.Runtime.GCSettings.IsServerGC);
    app.Run();

}
catch (Exception ex)
{
    logger.LogCritical("This is unhandled ex:" + ex.Message);
}