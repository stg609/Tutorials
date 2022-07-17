using DangQu.PT.LowCodeEngine.Scripts.Core;
using DangQu.PT.LowCodeEngine.Scripts.Roslyn;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace CrashDemo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CrashController : ControllerBase
    {
        internal static bool _crashed = false;
        private readonly ILogger<CrashController> _logger;
        private static List<byte[]> _items = new List<byte[]>();
        private static IMemoryCache cache = new MemoryCache(new MemoryCacheOptions());
        private static RoslynScriptExecutorV2<DefaultRoslynGlobalContext> roslynScript = new RoslynScriptExecutorV2<DefaultRoslynGlobalContext>(cache, new DefaultRoslynGlobalContextFactory());

        public CrashController(ILogger<CrashController> logger)
        {
            _logger = logger;
        }

        [HttpGet("Unhealthy")]
        public ActionResult Unhealthy()
        {
            _crashed = true;
            return Ok();
        }


        [HttpGet("Healthy")]
        public ActionResult Healthy()
        {
            _crashed = false;
            return Ok();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="max">循环次数，每次循环都会增加 10000 字节 </param>
        /// <returns></returns>
        [HttpGet("OOM/{max}")]
        public ActionResult OOM(int max = 1000)
        {
            for (int i = 0; i < max; i++)
            {
                byte[] c = new byte[10000];
                _items.Add(c);
            }

            return Ok();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="max">循环次数，每次循环都会使用 Roslyn 执行一次脚本</param>
        /// <returns></returns>
        [HttpGet("OOMByRoslyn/{max}")]
        public async Task<ActionResult> OOMByRoslyn(int max = 1000, CancellationToken cancellationToken = default)
        {
            string code = "";
            for (int i = 0; i < max; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                code = "Console.WriteLine(\" > " + i + "\");";

                await roslynScript.ExecuteAsync(code);
            }

            return Ok();
        }
    }
}